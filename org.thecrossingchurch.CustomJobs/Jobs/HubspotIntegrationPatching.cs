// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Jobs;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Newtonsoft.Json;
using System.Reflection;
using RestSharp;
using Rock.Security;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Net;
using System.Threading;

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    /// <summary>
    /// Job to supply hubspot contacts that already have rock_ids with other info.
    /// </summary>
    [DisplayName( "Hubspot Integration: Update Records" )]
    [Description( "This job only updates Hubspot contacts with a valid Rock ID with additional info from Rock." )]

    #region General Settings Job Attributes 
    [TextField( "Global Attribute Key for HubSpot API Bearer Token",
        Description = "For ease of rotating, your HubSpot API Key could be stored in a global attribute encrypted type attribute.",
        IsRequired = true,
        DefaultValue = "HubspotAPIKeyGlobal",
        Key = AttributeKey.APIKeyAttribute,
        Category = "General Settings",
        Order = 0
    )]
    [TextField( "Business Unit",
        Description = "Hubspot Business Unit value, add hs_all_assigned_business_unit_ids to the additional properties if you are using business units",
        IsRequired = false,
        DefaultValue = "0",
        Key = AttributeKey.BusinessUnit,
        Category = "General Settings",
        Order = 2
    )]
    [ValueListField( "HubSpot Property Group Names",
        Description = "The internal names of the property groups that contain values that should sync to Rock Properties, Attributes, and Custom Calculations",
        IsRequired = true,
        DefaultValue = "",
        ValuePrompt = "group_name",
        Key = AttributeKey.PropertyGroups,
        Category = "General Settings",
        Order = 3
    )]
    [TextField( "Additional HubSpot Properties",
        Description = "If properties outside of Id, Name, Email, Phone, Created Date, and Last Modified Date are required add them as a comma seperated list here.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKey.AdditionalHubSpotProps,
        Category = "General Settings",
        Order = 4 )]
    [DefinedValueField( "Valid Transaction Types",
        Description = "If you need financial data, specify the transaction types that should be available for processing. If both this field and financial account field are empty, no finanical data will be available for processing.",
        AllowMultiple = true,
        AllowAddingNewValues = false,
        IsRequired = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE,
        Key = AttributeKey.TransactionTypeValue,
        Category = "General Settings",
        Order = 5
    )]
    [AccountsField( "Financial Accounts",
        Description = "If you need financial data, specify which accounts should be included in processing. If both this field and financial transaction type field are empty, no finanical data will be available for processing.",
        IsRequired = false,
        Key = AttributeKey.FinancialAccount,
        Category = "General Settings",
        Order = 6
    )]
    [IntegerField( "Number of Threads",
        Description = "How many threads we should split this job into",
        IsRequired = true,
        DefaultValue = "50",
        Key = AttributeKey.ThreadCount,
        Category = "General Settings",
        Order = 7
    )]
    [KeyValueListField( "Custom Configuration Values",
        Description = "This will be passed to any instance of the IAdditionalProperties interface as a dictionary",
        IsRequired = false,
        Key = AttributeKey.CustomConfig,
        Category = "General Settings",
        Order = 8
    )]
    #endregion

    #region Testing Environment Configuration Job Attributes 
    [BooleanField( "Disable POST/PATCH Requests",
        Description = "Selecting this will stop this Rock Job from writing any data to HubSpot",
        IsRequired = true,
        DefaultBooleanValue = true,
        Category = "Testing Environment Configuration",
        Key = AttributeKey.DisableUpdates,
        Order = 0
    )]
    [CustomDropdownListField( "Sync Type",
        Description = "Determines what the sync should do when updating HubSpot data, whether that should be logging the data instead of making a request, syncing to a specific HubSpot record for testing, or the full sync for production use.",
        ListSource = "Single^Sync to Single HubSpot Record,Full^Regular Sync for Production Use",
        DefaultValue = "Single",
        Category = "Testing Environment Configuration",
        Key = AttributeKey.SyncType,
        Order = 1
    )]
    [IntegerField( "HubSpot Contact Limit",
        Description = "If you want to provide a hard limit for the number of contacts that should be pulled from HubSpot for processing provide it here.",
        IsRequired = false,
        Key = AttributeKey.ContactLimit,
        Category = "Testing Environment Configuration",
        Order = 3
    )]
    [IntegerField( "HubSpot Record ID",
        Description = "If you wish to test by syncing data to a specific HubSpot record, enter the ID here. URL for POST/PATCH will be updated to use this ID.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKey.HubSpotRecordId,
        Category = "Testing Environment Configuration",
        Order = 4
    )]
    [IntegerField( "Rock Record ID",
        Description = "If you wish to test by syncing data of a specific Rock record, enter the ID here. Only the record with this ID will be processed.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKey.RockRecordId,
        Category = "Testing Environment Configuration",
        Order = 5
    )]
    #endregion
    public class HubspotIntegrationPatching : RockJob
    {
        private string key { get; set; }
        private List<HSContactResult> contacts { get; set; }
        private int request_count { get; set; }
        private string businessUnit { get; set; }

        private bool updatesAreDisabled { get; set; }
        private string syncType { get; set; }
        private int? contactLimit { get; set; }
        private string hubSpotTestContactId { get; set; }
        private int? rockTestContactId { get; set; }
        private Person rockTestPerson { get; set; }

        #region Attribute Keys
        private class AttributeKey
        {
            public const string APIKeyAttribute = "AttributeKey";
            public const string BusinessUnit = "BusinessUnit";
            public const string PropertyGroups = "PropertyGroups";
            public const string AdditionalHubSpotProps = "AdditionalHubSpotProps";
            public const string ThreadCount = "ThreadCount";
            public const string TransactionTypeValue = "TransactionTypeValue";
            public const string FinancialAccount = "FinancialAccount";
            public const string CustomConfig = "CustomConfig";

            public const string DisableUpdates = "DisableUpdates";
            public const string SyncType = "SyncType";
            public const string ContactLimit = "ContactLimit";
            public const string HubSpotRecordId = "HubSpotRecordId";
            public const string RockRecordId = "RockRecordId";
        }
        #endregion Attribute Keys

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public HubspotIntegrationPatching()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public override void Execute()
        {
            //Testing Environment Attributes
            updatesAreDisabled = GetAttributeValue( AttributeKey.DisableUpdates ).AsBoolean();
            syncType = GetAttributeValue( AttributeKey.SyncType );
            contactLimit = GetAttributeValue( AttributeKey.ContactLimit ).AsIntegerOrNull();
            hubSpotTestContactId = GetAttributeValue( AttributeKey.HubSpotRecordId );
            rockTestContactId = GetAttributeValue( AttributeKey.RockRecordId ).AsIntegerOrNull();
            if ( rockTestContactId.HasValue )
            {
                rockTestPerson = new PersonService( new RockContext() ).Get( rockTestContactId.Value );
            }

            //Pull HubSpot Authorization from Global Attribute so we don't have to update it everywhere when we rotate the key
            string attrKey = GetAttributeValue( AttributeKey.APIKeyAttribute );
            key = Encryption.DecryptString( GlobalAttributesCache.Get().GetValue( attrKey ) ); //global variable should be an encrypted field

            //Business Unit is not requried, but if provided will filter to contacts in that business unit
            businessUnit = GetAttributeValue( AttributeKey.BusinessUnit );

            //Get Hubspot Properties
            //This will allow us to dynamically add or remove properties/attributes/custom values we want synced
            var propClient = new RestClient( "https://api.hubapi.com/crm/v3/properties/contacts?properties=name,label,createdUserId,groupName,options,fieldType" );
            propClient.Timeout = -1;
            var propRequest = new RestRequest( Method.GET );
            propRequest.AddHeader( "Authorization", $"Bearer {key}" );
            IRestResponse propResponse = propClient.Execute( propRequest );
            var props = new List<HubspotProperty>();
            var propsQry = JsonConvert.DeserializeObject<HSPropertyQueryResult>( propResponse.Content );
            props = propsQry.results;

            //Filter to props in the desired property groups for the sync so we don't try to process unnecessary fields
            string rawPropertyGroupNames = GetAttributeValue( AttributeKey.PropertyGroups );
            List<string> propertyGroupNames = rawPropertyGroupNames.Split( '|' ).Where( g => !String.IsNullOrEmpty( g ) ).ToList();
            props = props.Where( p => propertyGroupNames.Contains( p.groupName ) ).ToList();

            //Save a list of the properties that are Rock attributes as denoted by their label's naming convention
            var attrs = props.Where( p => p.label.Contains( "Rock Attribute " ) ).ToList();
            RockContext _context = new RockContext();
            List<string> attrKeys = attrs.Select( hs => hs.label.Replace( "Rock Attribute ", "" ) ).ToList();

            //Financial Data Sync Props
            //Get the transaction types we care about 
            List<Guid?> transactionTypeGuids = GetAttributeValue( AttributeKey.TransactionTypeValue ).Split( ',' ).AsGuidOrNullList();
            var transactionTypeDefinedValues = new DefinedValueService( _context ).Queryable().Where( dv => transactionTypeGuids.Contains( dv.Guid ) );
            List<int> transactionTypeValueIds = transactionTypeDefinedValues.Select( dv => dv.Id ).ToList();

            //Get the specific financial accounts we care about
            List<Guid?> accountGuids = GetAttributeValue( AttributeKey.FinancialAccount ).Split( ',' ).AsGuidOrNullList().Where( g => g.HasValue ).ToList();
            List<FinancialAccount> accounts = new List<FinancialAccount>();
            if ( accountGuids.Count > 0 )
            {
                accounts = new FinancialAccountService( _context ).Queryable().Where( fa => accountGuids.Contains( fa.Guid ) ).ToList();
            }

            //Custom Configuration values
            //Easily add or update additional data without needing to modify the core job
            Dictionary<string, string> customConfigurationValues = new Dictionary<string, string>();
            string rawCustomConfigValues = GetAttributeValue( AttributeKey.CustomConfig );
            rawCustomConfigValues.ToKeyValuePairList().ForEach( kvp =>
            {
                if ( !String.IsNullOrEmpty( kvp.Key ) && !String.IsNullOrEmpty( kvp.Value.ToString() ) )
                {
                    customConfigurationValues.Add( kvp.Key, kvp.Value.ToString() );
                }
            } );

            //Build the distinct list of properties we want included with our Contact records from HubSpot
            List<string> requestedProperties = new List<string>() { "rock_id", "firstname", "lastname", "email", "phone", "createdate", "lastmodifieddate" };
            List<string> additionalProperties = GetAttributeValue( AttributeKey.AdditionalHubSpotProps ).Split( ',' ).ToList();
            requestedProperties.AddRange( additionalProperties );
            if ( !String.IsNullOrEmpty( businessUnit ) )
            {
                requestedProperties.Add( "hs_all_assigned_business_unit_ids" );
            }
            requestedProperties = requestedProperties.Select( p => p.ToLower() ).Where( p => !String.IsNullOrEmpty( p ) ).Distinct().ToList();

            //Get List of all contacts from Hubspot with the requested properties
            contacts = new List<HSContactResult>();
            request_count = 0;
            string contactsUrl = "https://api.hubapi.com/crm/v3/objects/contacts?limit=100&properties=" + String.Join( ",", requestedProperties ) + "&archived=false";
            GetContacts( contactsUrl );

            //Get Attributes for Person Entity who's keys are in the list obtained from HubSpot 
            var rockAttributes = new AttributeService( _context ).Queryable( "FieldType" ).Where( a => a.EntityTypeId == 15 && attrKeys.Contains( a.Key ) );
            List<Rock.Model.Attribute> rockPersonAttributes = new List<Rock.Model.Attribute>();
            rockPersonAttributes = rockAttributes.ToList();

            //Configurable thread count to easily account for different environments 
            int numThreads = GetAttributeValue( AttributeKey.ThreadCount ).AsInteger();
            if ( numThreads <= 0 )
            {
                numThreads = 50; //Default number of threads if not configured
            }

            ConcurrentBag<Task> taskBag = new ConcurrentBag<Task>();
            int listSize = contacts.Count() / numThreads;
            for ( int i = 0; i < numThreads; i++ )
            {
                var subset = new List<HSContactResult>();
                if ( i == ( numThreads - 1 ) )
                {
                    //If last iteration, take the remaining contacts
                    subset = contacts.Skip( i * listSize ).ToList();
                }
                else
                {
                    subset = contacts.Skip( i * listSize ).Take( listSize ).ToList();
                }

                var hubspotAttrs = attrs;
                var hubspotProps = props;
                var hubspotKeys = attrKeys;
                var rockAccounts = accounts;
                var configValues = customConfigurationValues;
                Task task = new Task( () =>
                {
                    ProcessList( subset, hubspotKeys, hubspotAttrs, hubspotProps, rockPersonAttributes, transactionTypeValueIds, rockAccounts, configValues );
                } );
                taskBag.Add( task );
                task.Start();
            }
            Task.WaitAll( taskBag.ToArray() );
        }

        private void ProcessList( List<HSContactResult> contacts, List<string> attrKeys, List<HubspotProperty> attrs, List<HubspotProperty> props, List<Rock.Model.Attribute> rockPersonAttributes, List<int> transactionTypeValueIds, List<FinancialAccount> accounts, Dictionary<string, string> customConfigurationValues )
        {

            RockContext _context = new RockContext();
            PersonAliasService pa_svc = new PersonAliasService( _context );
            FinancialTransactionService ft_svc = new FinancialTransactionService( _context );
            AttributeValueService av_svc = new AttributeValueService( _context );
            GroupMemberService gm_svc = new GroupMemberService( _context );

            #region DB Queries
            //Get valid integer Rock Ids
            List<int> rockIds = contacts.Select( c => c.rock_id ).Where( id => id > 0 ).ToList();

            //Get Person Aliases for list of Rock Ids
            IQueryable<PersonAlias> aliasQuery = pa_svc.Queryable( "Person.ConnectionStatusValue,Person.MaritalStatusValue,Person.PhoneNumbers,Person.PrimaryFamily.GroupLocations" ).Where( pa => pa.AliasPersonId.HasValue && rockIds.Contains( pa.AliasPersonId.Value ) );
            List<PersonAlias> aliases = new List<PersonAlias>();
            aliases = aliasQuery.ToList();


            //Get Attribute Values for Current List
            List<AttributeValue> personAttributeValues = new List<AttributeValue>();
            List<int> personAttributeIds = rockPersonAttributes.Select( a => a.Id ).ToList();
            personAttributeValues = av_svc.Queryable( "Attribute.FieldType" )
                                            .Where( av => personAttributeIds.Contains( av.AttributeId ) )
                                            .Join( aliasQuery,
                                                av => av.EntityId,
                                                pa => pa.PersonId,
                                                ( av, pa ) => av
                                            ).ToList();


            //Group Membership for Giving, Family (Children), Known Relationships (Adult Children)
            var baseGroupMemberQuery = gm_svc.Queryable( "Group.ParentGroup,GroupRole,Group.GroupType.GroupTypePurposeValue,Person" );
            var allFamilyGroupIds = aliasQuery.Select( pa => pa.Person.PrimaryFamilyId ).Distinct().ToList();
            //Giving Group Ids or save a list of records where the person gives independently 
            var allGivingGroupIds = aliasQuery.Select( pa => pa.Person.GivingGroupId ).Distinct().ToList();
            var allPersonalGivingPersonIds = aliasQuery.Where( pa => !pa.Person.GivingGroupId.HasValue ).Select( pa => pa.PersonId ).Distinct().ToList();

            List<GroupMember> familyGroupMemberships = baseGroupMemberQuery.Where( gm => allFamilyGroupIds.Contains( gm.GroupId ) ).ToList();
            List<GroupMember> personGroupMemberships = baseGroupMemberQuery.Where( gm => gm.GroupTypeId != 10 && gm.GroupTypeId != 11 )
                                                                           .Join( aliasQuery,
                                                                              gm => gm.PersonId,
                                                                              pa => pa.PersonId,
                                                                              ( gm, pa ) => gm
                                                                           ).ToList();

            //Find Children and Adult Children
            IQueryable<GroupMember> personsKnownRelationshipMembershipsQuery = gm_svc.Queryable( "Person,Group" )
                                                                                     .Where( gm => gm.GroupTypeId == 11 && gm.GroupRoleId == 5 )
                                                                                     .Join( aliasQuery,
                                                                                         gm => gm.PersonId,
                                                                                         pa => pa.PersonId,
                                                                                         ( gm, pa ) => gm
                                                                                     );
            IQueryable<GroupMember> childKnownRelationshipsQry = gm_svc.Queryable( "Person" ).Where( gm => gm.GroupTypeId == 11 && ( gm.GroupRoleId == 15 || gm.GroupRoleId == 17 ) );
            var childKnownRelationships = personsKnownRelationshipMembershipsQuery.Join( childKnownRelationshipsQry,
                    kr => kr.GroupId,
                    cr => cr.GroupId,
                    ( kr, cr ) => new { Parent = kr, Child = cr }
                ).GroupBy( obj => obj.Parent.Person )
                 .Select( grp => new ChildrenMap { Parent = grp.Key, Children = grp.Select( g => g.Child.Person ).ToList() } )
                 .ToList();

            //Find the possible aliases a gift could be given under based on the giving group or a person's own aliases
            IQueryable<GroupMember> allGivingGroupMemberships = gm_svc.Queryable().Where( gm => allGivingGroupIds.Contains( gm.GroupId ) );
            var givingAliasesByGroup = pa_svc.Queryable().Join( allGivingGroupMemberships,
                    pa => pa.PersonId,
                    gm => gm.PersonId,
                    ( pa, gm ) => new { gm.GroupId, pa }
                ).GroupBy( d => d.GroupId )
                .Select( grp => new { GroupId = grp.Key, AliasIds = grp.Select( g => g.pa.Id ).ToList() } );
            var givingAliasesByPerson = aliasQuery.Join( givingAliasesByGroup,
                    pa => pa.Person.GivingGroupId,
                    g => g.GroupId,
                    ( pa, g ) => new { pa.Person, GivingAliases = g.AliasIds }
                );
            var personalGivingAliasesByPerson = aliasQuery.Where( pa => allPersonalGivingPersonIds.Contains( pa.PersonId ) )
                .GroupBy( g => g.Person )
                .Select( grp => new { Person = grp.Key, GivingAliases = grp.Select( pa => pa.Id ).ToList() } );

            List<int> accountIds = accounts.Select( fa => fa.Id ).ToList();
            IQueryable<FinancialTransaction> allValidTransactions = ft_svc.Queryable( "TransactionDetails.Account" ).Where( ft => ft.AuthorizedPersonAliasId.HasValue &&
                ( transactionTypeValueIds.Count > 0 || accountIds.Count > 0 ) && //If no accounts are selected and no tranasction types are selected, no transactions will be processed
                ( transactionTypeValueIds.Contains( ft.TransactionTypeValueId ) || ft.TransactionDetails.Any( ftd => accountIds.Contains( ftd.AccountId ) ) )
            );

            List<TransactionMap> givingTransactions = givingAliasesByPerson.ToList().Select( ga => new TransactionMap { Person = ga.Person, ValidTransactions = allValidTransactions.Where( ft => ga.GivingAliases.Contains( ft.AuthorizedPersonAliasId.Value ) ).ToList() } ).ToList();
            givingTransactions.AddRange( personalGivingAliasesByPerson.ToList().Select( ga => new TransactionMap { Person = ga.Person, ValidTransactions = allValidTransactions.Where( ft => ga.GivingAliases.Contains( ft.AuthorizedPersonAliasId.Value ) ).ToList() } ).ToList() );

            //Filter out transaction details that didn't meet requirements
            if ( accountIds.Count > 0 )
            {
                givingTransactions = givingTransactions.Select( tm =>
                {
                    tm.ValidTransactions = tm.ValidTransactions.Select( ft =>
                    {
                        ft.TransactionDetails = ft.TransactionDetails.Where( ftd => accountIds.Contains( ftd.AccountId ) ).ToList();
                        return ft;
                    } ).ToList();
                    return tm;
                } ).ToList();
            }

            //Get Extensions of DB Queries we need to run
            Dictionary<string, object> additionalQueries = new Dictionary<string, object>();
            var instances = from t in Assembly.GetExecutingAssembly().GetTypes()
                            where t.GetInterfaces().Contains( typeof( IAdditionalProperties ) )
                                     && t.GetConstructor( Type.EmptyTypes ) != null
                            select Activator.CreateInstance( t ) as IAdditionalProperties;
            foreach ( var instance in instances )
            {
                try
                {
                    Dictionary<string, object> results = instance.GetAdditionalProperties( customConfigurationValues, rockIds, aliasQuery );
                    foreach ( var result in results )
                    {
                        additionalQueries.Add( result.Key, result.Value );
                    }
                }
                catch ( Exception ex )
                {
                    ExceptionLogService.LogException( new Exception( $"HubSpot Sync Error: Unable to add custom queries from {instance.GetType().FullName}.", ex ) );
                }
            }
            #endregion


            for ( var i = 0; i < contacts.Count(); i++ )
            {
                int current_pid = contacts[i].rock_id;
                int current_id = 0;
                Person person = null;
                if ( current_pid > 0 )
                {
                    person = aliases.Where( pa => pa.AliasPersonId == current_pid ).Select( pa => pa.Person ).FirstOrDefault();
                }

                //For Testing
                if ( rockTestContactId.HasValue && rockTestPerson != null )
                {
                    person = rockTestPerson;
                }

                //In general we don't want to process records for deceased people, the exception is if HubSpot is unaware they are deceased
                bool updateDeceased = false;
                var rockIsDeceasedProp = props.FirstOrDefault( p => p.label == "Rock Property IsDeceased" );
                if ( rockIsDeceasedProp != null )
                {
                    string hubspotIsDeceased;
                    contacts[i].properties.TryGetValue( rockIsDeceasedProp.name, out hubspotIsDeceased );
                    if ( hubspotIsDeceased != "true" )
                    {
                        updateDeceased = true;
                    }
                }

                if ( person != null && ( !person.IsDeceased || updateDeceased ) )
                {
                    try
                    {
                        current_id = person.Id;
                        //Url for the hubspot record to update
                        var url = $"https://api.hubapi.com/crm/v3/objects/contacts/{contacts[i].id}";

                        var properties = new List<HubspotPropertyUpdate>();
                        var personAttributes = personAttributeValues.Where( av => av.EntityId == person.Id ).ToList();

                        //Add each Rock prop to the list with the Hubspot name
                        for ( var j = 0; j < attrs.Count(); j++ )
                        {
                            AttributeValue current_prop = null;
                            try
                            {
                                current_prop = personAttributes.FirstOrDefault( av => "Rock Attribute " + av.Attribute.Key == attrs[j].label );
                                if ( current_prop == null )
                                {
                                    //Try to get default value for this attr
                                    var rockAttr = rockPersonAttributes.FirstOrDefault( a => "Rock Attribute " + a.Key == attrs[j].label );
                                    if ( rockAttr != null )
                                    {
                                        current_prop = new AttributeValue()
                                        {
                                            Value = rockAttr.DefaultValue,
                                            AttributeId = rockAttr.Id,
                                            Attribute = rockAttr
                                        };
                                    }
                                }
                            }
                            catch ( Exception e )
                            {
                                ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error Updating Attribute:{Environment.NewLine}Current Rock Id: {current_id}, Property Name: {attrs[j].label}", e ) );
                                current_prop = null;
                            }
                            //If the attribute is in our list of props from Hubspot
                            if ( current_prop != null )
                            {
                                if ( current_prop.Attribute.FieldType.Name == "Date" || current_prop.Attribute.FieldType.Name == "Date Time" )
                                {
                                    //Get Epoc miliseconds 
                                    DateTime tryDate;
                                    if ( DateTime.TryParse( current_prop.Value, out tryDate ) )
                                    {
                                        string propDate = ConvertDate( tryDate );
                                        if ( !String.IsNullOrEmpty( propDate ) )
                                        {
                                            properties.Add( new HubspotPropertyUpdate() { property = attrs[j].name, value = propDate } );
                                        }
                                    }
                                }
                                else if ( current_prop.Attribute.FieldType.Name == "Lava" )
                                {
                                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                                    mergeFields.Add( "Entity", person );
                                    var renderedLavaValue = current_prop.Value.ResolveMergeFields( mergeFields ).Trim();
                                    properties.Add( new HubspotPropertyUpdate() { property = attrs[j].name, value = renderedLavaValue } );
                                }
                                else
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = attrs[j].name, value = current_prop.ValueFormatted } );
                                }
                            }
                        }

                        //All properties begining with "Rock " are properties on the Person entity itself 
                        var person_props = props.Where( p => p.label.Contains( "Rock Property " ) ).ToList();
                        foreach ( PropertyInfo propInfo in person.GetType().GetProperties() )
                        {
                            var current_prop = props.FirstOrDefault( p => p.label == "Rock Property " + propInfo.Name );
                            if ( current_prop != null && propInfo.GetValue( person ) != null )
                            {
                                if ( propInfo.PropertyType.FullName == "Rock.Model.DefinedValue" )
                                {
                                    DefinedValue dv = ( DefinedValue ) propInfo.GetValue( person );
                                    properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = dv.Value } );
                                }
                                else if ( propInfo.PropertyType.FullName.Contains( "Boolean" ) )
                                {
                                    bool value = ( bool ) propInfo.GetValue( person );
                                    properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = value.ToString().ToLower() } );
                                }
                                else if ( propInfo.PropertyType.FullName.Contains( "Date" ) )
                                {
                                    //Get Epoc miliseconds
                                    //Possibly not used anymore, switched to regular date format
                                    DateTime tryDate;
                                    if ( DateTime.TryParse( propInfo.GetValue( person ).ToString(), out tryDate ) )
                                    {
                                        string propDate = ConvertDate( tryDate );
                                        if ( !String.IsNullOrEmpty( propDate ) )
                                        {
                                            properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = propDate } );
                                        }
                                    }
                                }
                                else
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = propInfo.GetValue( person ).ToString() } );
                                }
                            }
                        }

                        TransactionMap transactionMap = givingTransactions.FirstOrDefault( ft => ft.Person.Id == person.Id );

                        //Run custom processing for additional properties
                        foreach ( var instance in instances )
                        {
                            try
                            {
                                var additionalProps = instance.ProcessAdditionalProperties( customConfigurationValues, additionalQueries, attrs, props, aliases, personAttributeValues, familyGroupMemberships, personGroupMemberships, childKnownRelationships, transactionMap, contacts[i], person );
                                properties.AddRange( additionalProps );
                            }
                            catch ( Exception ex )
                            {
                                ExceptionLogService.LogException( new Exception( $"HubSpot Sync Error: Unable to add custom properties from {instance.GetType().Name} for contact. HubSpot ID: {contacts[i].id}, Rock ID: {person.Id}", ex ) );
                            }
                        }


                        //If the Hubspot Contact does not have FirstName, LastName, or Phone Number we want to update those...
                        if ( String.IsNullOrEmpty( contacts[i].firstname ) )
                        {
                            properties.Add( new HubspotPropertyUpdate() { property = "firstname", value = person.NickName } );
                        }
                        if ( String.IsNullOrEmpty( contacts[i].lastname ) )
                        {
                            properties.Add( new HubspotPropertyUpdate() { property = "lastname", value = person.LastName } );
                        }
                        if ( String.IsNullOrEmpty( contacts[i].phone ) )
                        {
                            PhoneNumber mobile = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == 12 );
                            if ( mobile != null && !mobile.IsUnlisted )
                            {
                                properties.Add( new HubspotPropertyUpdate() { property = "phone", value = mobile.NumberFormatted } );
                            }
                        }

                        //Update the Contact in Hubspot
                        MakeRequest( current_id, url, properties, 0 );
                    }
                    catch ( Exception err )
                    {
                        ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error: Unable to process contact.{Environment.NewLine}HubSpot Id: {contacts[i].id}, Rock Id: {current_id}, Name: {contacts[i].firstname} {contacts[i].lastname}, Email: {contacts[i].email}", err ) );
                    }
                }

            }
        }

        private void MakeRequest( int current_id, string url, List<HubspotPropertyUpdate> properties, int attempt )
        {
            //Update the Hubspot Contact
            try
            {
                //Only run if we have not disabled POST/PATCH Requests
                if ( !updatesAreDisabled )
                {
                    if ( syncType == "Single" && !String.IsNullOrEmpty( hubSpotTestContactId ) )
                    {
                        //If syncing to a single record for testing was selected overwrite the URL
                        url = "https://api.hubapi.com/crm/v3/objects/contacts/" + hubSpotTestContactId;
                    }
                    var x = 7;
                    var client = new RestClient( url );
                    client.Timeout = -1;
                    var request = new RestRequest( Method.PATCH );
                    request.AddHeader( "accept", "application/json" );
                    request.AddHeader( "content-type", "application/json" );
                    request.AddHeader( "Authorization", $"Bearer {key}" );
                    request.AddParameter( "application/json", $"{{\"properties\": {{ {String.Join( ",", properties.Select( p => $"\"{p.property}\": \"{p.value}\"" ) )} }} }}", ParameterType.RequestBody );
                    IRestResponse response = client.Execute( request );
                    if ( ( int ) response.StatusCode == 429 )
                    {
                        if ( attempt < 3 )
                        {
                            Thread.Sleep( 9000 );
                            MakeRequest( current_id, url, properties, attempt + 1 );
                        }
                        else
                        {
                            throw new Exception( "Rate limit and retry limit reached", new Exception( response.Content ) );
                        }
                    }
                    else if ( response.StatusCode != HttpStatusCode.OK )
                    {
                        throw new Exception( response.Content );
                    }
                }
                else
                {
                    //For Testing Write to Log File instead of making request
                    WriteToLog( string.Format( "{0}     ID: {1}{2}PROPS:{2}{3}", RockDateTime.Now.ToString( "HH:mm:ss.ffffff" ), current_id, Environment.NewLine, JsonConvert.SerializeObject( properties ) ) );
                }
            }
            catch ( Exception e )
            {
                var json = $"{{\"properties\": {JsonConvert.SerializeObject( properties )} }}";
                ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error: API Exception{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Request:{Environment.NewLine}{json}", e ) );
            }
        }

        private void WriteToLog( string message )
        {
            string logFile = System.Web.Hosting.HostingEnvironment.MapPath( $"~/App_Data/Logs/HubSpotPatchLog_{Task.CurrentId}.txt" );
            using ( System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ) )
            {
                using ( System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ) )
                {
                    sw.WriteLine( message );
                }
            }
        }

        private void GetContacts( string url )
        {
            request_count++;
            var contactClient = new RestClient( url );
            contactClient.Timeout = -1;
            var contactRequest = new RestRequest( Method.GET );
            contactRequest.AddHeader( "Authorization", $"Bearer {key}" );
            IRestResponse contactResponse = contactClient.Execute( contactRequest );
            var contactResults = JsonConvert.DeserializeObject<HSContactQueryResult>( contactResponse.Content );
            contacts.AddRange( contactResults.results.Where( c =>
                c.properties["rock_id"] != null && c.properties["rock_id"] != "" &&
                c.properties["email"] != null && c.properties["email"] != "" &&
                ( String.IsNullOrEmpty( businessUnit ) ||
                    ( c.properties["hs_all_assigned_business_unit_ids"] != null &&
                      c.properties["hs_all_assigned_business_unit_ids"] != "" &&
                      c.properties["hs_all_assigned_business_unit_ids"].Split( ';' ).Contains( businessUnit )
                    )
                )
            ).ToList() );

            //For Testing
            if ( contactLimit.HasValue && contacts.Count >= contactLimit )
            {
                return;
            }

            if ( contactResults.paging != null && contactResults.paging.next != null && !String.IsNullOrEmpty( contactResults.paging.next.link ) && request_count < 500 )
            {
                GetContacts( contactResults.paging.next.link );
            }
        }

        private string ConvertDate( DateTime? date )
        {
            if ( date.HasValue )
            {
                DateTime today = RockDateTime.Now;
                if ( today.Year - date.Value.Year < 1000 && today.Year - date.Value.Year > -1000 )
                {
                    date = new DateTime( date.Value.Year, date.Value.Month, date.Value.Day, 0, 0, 0 );
                    var d = date.Value.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds * 1000;
                    return d.ToString();
                }
            }
            return "";
        }
    }
}
