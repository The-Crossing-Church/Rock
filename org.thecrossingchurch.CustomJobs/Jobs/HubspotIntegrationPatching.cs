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
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Reflection;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using RestSharp;
using Rock.Security;
using System.Diagnostics;
using static Quartz.Plugin.Xml.XMLSchedulingDataProcessorPlugin;
using RestSharp.Extensions;
using Rock.Workflow.Action;
using System.ComponentModel;
using System.Threading;

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    /// <summary>
    /// Job to supply hubspot contacts that already have rock_ids with other info.
    /// </summary>
    [DisplayName( "Hubspot Integration: Update Records" )]
    [Description( "This job only updates Hubspot contacts with a valid Rock ID with additional info from Rock." )]
    [DisallowConcurrentExecution]

    [TextField( "AttributeKey", "", true, "HubspotAPIKeyGlobal" )]
    [TextField( "Business Unit", "Hubspot Business Unit value", true, "0" )]
    [DefinedValueField( "Contribution Transaction Type",
        AllowMultiple = false,
        AllowAddingNewValues = false,
        DefaultValue = Rock.SystemGuid.DefinedValue.TRANSACTION_TYPE_CONTRIBUTION,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.FINANCIAL_TRANSACTION_TYPE
    )]
    [BooleanField( "Include TMBT", defaultValue: false )]
    [AccountField( "Financial Account", "If syncing a total amount given which fund should we sync from" )]
    public class HubspotIntegrationPatching : IJob
    {
        private string key { get; set; }
        private List<HSContactResult> contacts { get; set; }
        private int request_count { get; set; }
        private string businessUnit { get; set; }

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
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            //Bearer Token, but I didn't change the Attribute Key
            string attrKey = dataMap.GetString( "AttributeKey" );
            key = Encryption.DecryptString( GlobalAttributesCache.Get().GetValue( attrKey ) );
            businessUnit = dataMap.GetString( "BusinessUnit" );

            var current_id = 0;

            PersonService personService = new PersonService( new RockContext() );

            //Get Hubspot Properties in Rock Information Group
            //This will allow us to add properties temporarily to the sync and then not continue to have them forever
            var propClient = new RestClient( "https://api.hubapi.com/crm/v3/properties/contacts?properties=name,label,createdUserId,groupName,options,fieldType" );
            propClient.Timeout = -1;
            var propRequest = new RestRequest( Method.GET );
            propRequest.AddHeader( "Authorization", $"Bearer {key}" );
            IRestResponse propResponse = propClient.Execute( propRequest );
            var props = new List<HubspotProperty>();
            var tmbtProps = new List<HubspotProperty>();
            var propsQry = JsonConvert.DeserializeObject<HSPropertyQueryResult>( propResponse.Content );
            props = propsQry.results;

            //Filter to props in Rock Information Group
            props = props.Where( p => p.groupName == "rock_information" ).ToList();
            tmbtProps = propsQry.results.Where( p => p.groupName == "rock_tmbt_information" ).ToList();
            //Business Unit hs_all_assigned_business_unit_ids
            //Save a list of the ones that are Rock attributes
            var attrs = props.Where( p => p.label.Contains( "Rock Attribute " ) ).ToList();
            RockContext _context = new RockContext();
            List<string> attrKeys = attrs.Select( hs => hs.label.Replace( "Rock Attribute ", "" ) ).ToList();
            var rockAttributes = new AttributeService( _context ).Queryable().Where( a => a.EntityTypeId == 15 && attrKeys.Contains( a.Key ) );

            Guid transactionTypeGuid = dataMap.GetString( "ContributionTransactionType" ).AsGuid();
            var transactionTypeDefinedValue = new DefinedValueService( _context ).Get( transactionTypeGuid );
            int transactionTypeValueId = transactionTypeDefinedValue.Id;

            //Get List of all contacts from Hubspot
            contacts = new List<HSContactResult>();
            request_count = 0;
            GetContacts( "https://api.hubapi.com/crm/v3/objects/contacts?limit=100&properties=email,firstname,lastname,phone,hs_all_assigned_business_unit_ids,rock_id,which_best_describes_your_involvement_with_the_crossing_,has_potential_rock_match,createdate,lastmodifieddate" );

            PersonAliasService pa_svc = new PersonAliasService( _context );
            FinancialTransactionService ft_svc = new FinancialTransactionService( _context );
            AttributeValueService av_svc = new AttributeValueService( _context );

            //WriteToLog( string.Format( "Total Contacts: {0}", contacts.Count() ) );
            for (var i = 0; i < contacts.Count(); i++)
            {
                //Stopwatch watch = new Stopwatch();
                //watch.Start();
                Person person = personService.Get( contacts[i].properties.rock_id );

                //For Testing
                //WriteToLog( string.Format( "{1}i: {0}{1}", i, Environment.NewLine ) );
                //WriteToLog( string.Format( "    After SQL: {0}{1}", watch.ElapsedMilliseconds, Environment.NewLine ) );

                //For Testing
                //if (contacts[i].properties.email != "coolrobot@hubspot.com")
                //{
                //    person = null;
                //}
                //else
                //{
                //    person = personService.Get( 1 );
                //}

                //Schedule HubSpot update if 1:1 match
                if (person != null)
                {
                    try
                    {
                        current_id = person.Id;
                        //Build the POST request and schedule in the db 10 at a time 
                        var url = $"https://api.hubapi.com/crm/v3/objects/contacts/{contacts[i].id}";
                        var properties = new List<HubspotPropertyUpdate>();
                        var personAttributes = av_svc.Queryable().Where( av => av.EntityId == person.Id ).Join( rockAttributes,
                                av => av.AttributeId,
                                attr => attr.Id,
                                ( av, attr ) => av
                            ).ToList();
                        foreach (var av in personAttributes)
                        {
                            av.Attribute = rockAttributes.FirstOrDefault( a => a.Id == av.AttributeId );
                        }

                        //Add each Rock prop to the list with the Hubspot name
                        for (var j = 0; j < attrs.Count(); j++)
                        {
                            AttributeValue current_prop = null;
                            try
                            {
                                current_prop = personAttributes.FirstOrDefault( av => "Rock Attribute " + av.Attribute.Key == attrs[j].label );
                                if (current_prop == null)
                                {
                                    //Try to get default value for this attr
                                    var rockAttr = rockAttributes.ToList().FirstOrDefault( a => "Rock Attribute " + a.Key == attrs[j].label );
                                    if (rockAttr != null)
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
                            catch (Exception e)
                            {
                                ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{e}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Property Name{Environment.NewLine}{attrs[j].label}{Environment.NewLine}Exception from Job:{Environment.NewLine}{e.Message}{Environment.NewLine}" ) );
                                current_prop = null;
                            }
                            //If the attribute is in our list of props from Hubspot
                            if (current_prop != null)
                            {
                                if (current_prop.Attribute.FieldType.Name == "Date" || current_prop.Attribute.FieldType.Name == "Date Time")
                                {
                                    //Get Epoc miliseconds 
                                    DateTime tryDate;
                                    if (DateTime.TryParse( current_prop.Value, out tryDate ))
                                    {
                                        string propDate = ConvertDate( tryDate );
                                        if (!String.IsNullOrEmpty( propDate ))
                                        {
                                            properties.Add( new HubspotPropertyUpdate() { property = attrs[j].name, value = propDate } );
                                        }
                                    }
                                }
                                else if (current_prop.Attribute.FieldType.Name == "Lava")
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
                        //WriteToLog( string.Format( "    After Attributes: {0}", watch.ElapsedMilliseconds ) );

                        //All properties begining with "Rock " are properties on the Person entity itself 
                        var person_props = props.Where( p => p.label.Contains( "Rock Property " ) ).ToList();
                        foreach (PropertyInfo propInfo in person.GetType().GetProperties())
                        {
                            var current_prop = props.FirstOrDefault( p => p.label == "Rock Property " + propInfo.Name );
                            if (current_prop != null && propInfo.GetValue( person ) != null)
                            {
                                if (propInfo.PropertyType.FullName == "Rock.Model.DefinedValue")
                                {
                                    DefinedValue dv = JsonConvert.DeserializeObject<DefinedValue>( JsonConvert.SerializeObject( propInfo.GetValue( person ) ) );
                                    properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = dv.Value } );
                                }
                                else if (propInfo.PropertyType.FullName.Contains( "Date" ))
                                {
                                    //Get Epoc miliseconds
                                    //Possibly not used anymore, switched to regular date format
                                    DateTime tryDate;
                                    if (DateTime.TryParse( propInfo.GetValue( person ).ToString(), out tryDate ))
                                    {
                                        string propDate = ConvertDate( tryDate );
                                        if (!String.IsNullOrEmpty( propDate ))
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
                        //WriteToLog( string.Format( "    After Properties: {0}", watch.ElapsedMilliseconds ) );

                        //Special Property for Parents
                        if (person.PrimaryFamily.Members.FirstOrDefault( gm => gm.PersonId == person.Id ).GroupRole.Name == "Adult")
                        {
                            //Direct Family Members
                            var child_ages_prop = props.FirstOrDefault( p => p.label == "Rock Custom Children's Age Groups" );
                            var children = person.PrimaryFamily.Members.Where( gm => gm.Person != null && gm.Person.AgeClassification == AgeClassification.Child ).ToList();
                            var agegroups = "";
                            //Known Relationships
                            person.PrimaryFamily.Members.Where( gm => gm.Person.AgeClassification == AgeClassification.Child );
                            int pid = person.Id;
                            var krGroups = new GroupMemberService( new RockContext() ).Queryable().Where( gm => gm.PersonId == pid && gm.GroupRoleId == 5 ).Select( gm => gm.GroupId ).ToList();
                            var childRelationships = new List<int> { 4, 15, 17 };
                            var krMembers = new GroupMemberService( new RockContext() ).Queryable().Where( gm => krGroups.Contains( gm.GroupId ) && childRelationships.Contains( gm.GroupRoleId ) ).ToList();
                            children.AddRange( krMembers );
                            for (var j = 0; j < children.Count(); j++)
                            {
                                if (children[j].Person.GradeOffset > 6)
                                {
                                    //Child is in K-5
                                    if (!agegroups.Contains( "Elementary" ))
                                    {
                                        agegroups += "Elementary,";
                                    }
                                }
                                else if (children[j].Person.GradeOffset > 3)
                                {
                                    //Child is in 6-8
                                    if (!agegroups.Contains( "Middle" ))
                                    {
                                        agegroups += "Middle,";
                                    }
                                }
                                else if (children[j].Person.GradeOffset <= 3)
                                {
                                    //Child is in 9-12
                                    if (!agegroups.Contains( "SeniorHigh" ))
                                    {
                                        agegroups += "SeniorHigh,";
                                    }
                                }
                                else
                                {
                                    //Check if child is infant-toddler or adult
                                    var bornCheck = DateTime.Now;
                                    if (children[j].Person.BirthYear >= (bornCheck.Year - 5))
                                    {
                                        if (!agegroups.Contains( "EarlyChildhood" ))
                                        {
                                            agegroups += "EarlyChildhood,";
                                        }
                                    }
                                    else
                                    {
                                        if (!agegroups.Contains( "Adult" ))
                                        {
                                            agegroups += "Adult,";
                                        }
                                    }
                                }
                            }
                            if (agegroups.Length > 0)
                            {
                                properties.Add( new HubspotPropertyUpdate() { property = child_ages_prop.name, value = agegroups.Substring( 0, agegroups.Length - 1 ) } );
                            }
                        }

                        if (person.Members != null && person.Members.Count() > 0)
                        {
                            //Special properties for a person's group membership 
                            //Currently in adult small group, currently in a 20s small group, currently in veritas small group, currently serving, currently in connections, membership list
                            var today = DateTime.UtcNow;
                            var term = "fall";
                            if (DateTime.Compare( today, new DateTime( today.Year, 5, 15 ) ) <= 0)
                            {
                                term = "winter";
                            }
                            else if (DateTime.Compare( today, new DateTime( today.Year, 8, 15 ) ) <= 0)
                            {
                                term = "summer";
                            }
                            //All current memberships for this year
                            var memberships = person.Members.Where( m => m.Group != null && m.GroupMemberStatus == GroupMemberStatus.Active && m.Group.IsActive && (m.Group.Name.Contains( today.ToString( "yyyy" ) ) ||
                                 m.Group.Name.Contains( $"{today.AddYears( -1 ).ToString( "yyyy" )}-{today.ToString( "yy" )}" )) ).ToList();
                            //Where the group name has Fall/Winter/Summer or Purpose == Serving Area
                            var current_serving = memberships.Where( m => m.Group.Name.ToLower().Contains( term ) || (m.Group.GroupType.GroupTypePurposeValue != null && m.Group.GroupType.GroupTypePurposeValue.Value == "Serving Area") ).ToList();
                            //All current groups with the words Small Group, SG or Purpose == Small Group
                            var current_sg = memberships.Where( m => m.Group.Name.ToLower().Contains( "small group" ) || m.Group.Name.ToLower().Contains( "sg" ) || (m.Group.GroupType.GroupTypePurposeValue != null && m.Group.GroupType.GroupTypePurposeValue.Value == "Small Group") ).ToList();

                            var serving_prop = props.FirstOrDefault( p => p.label == "Rock Custom Currently Serving" );
                            var sg_props = props.Where( p => p.label.Contains( "Small Group" ) ).ToList();

                            //set the serving prop
                            if (serving_prop != null)
                            {
                                if (current_serving.Count() > 0)
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = serving_prop.name, value = "true" } );
                                }
                                else
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = serving_prop.name, value = "false" } );
                                }
                            }
                            //figure out if they attend small group
                            if (current_sg.Count() > 0 && person.AgeClassification != AgeClassification.Child)
                            {
                                if (current_sg.Count() > 1 && current_sg.Where( sg => !sg.GroupRole.IsLeader ).Count() > 0)
                                { //See if we can get this to one small group hopefully
                                    current_sg = current_sg.Where( sg => !sg.GroupRole.IsLeader ).ToList();
                                }
                                foreach (var sg in current_sg)
                                {
                                    var small_group = sg_props.FirstOrDefault( p => p.label == "Rock Custom Currently in Adult Small Group" );
                                    if (sg.Group.ParentGroup.Name.ToLower().Contains( "veritas" ))
                                    {
                                        small_group = sg_props.FirstOrDefault( p => p.label == "Rock Custom Currently in Veritas Small Group" );
                                    }
                                    else if (sg.Group.ParentGroup.Name.ToLower().Contains( "twenties" ))
                                    {
                                        small_group = sg_props.FirstOrDefault( p => p.label == "Rock Custom Currently in Twenties Small Group" );
                                    }

                                    var exists = properties.FirstOrDefault( p => p.property == small_group.name );
                                    if (exists == null)
                                    {
                                        properties.Add( new HubspotPropertyUpdate() { property = small_group.name, value = "true" } );
                                    }
                                }
                            }
                            //Make the other values false so we keep the list up to date
                            foreach (var sg_prop in sg_props)
                            {
                                var exists = properties.FirstOrDefault( p => p.property == sg_prop.name );
                                if (exists == null)
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = sg_prop.name, value = "false" } );
                                }
                            }
                        }

                        //Custom Giving Props
                        var givingPersonIds = person.GivingGroup != null ? person.GivingGroup.Members.Select( gm => gm.PersonId ) : null;
                        if (givingPersonIds.Count() > 0)
                        {
                            var givingAliasIds = pa_svc.Queryable().Where( pa => givingPersonIds.Contains( pa.PersonId ) ).Select( pa => pa.Id );

                            var validTransactions = ft_svc.Queryable().Where( ft => ft.TransactionTypeValueId == transactionTypeValueId ).Join( givingAliasIds,
                                    ft => ft.AuthorizedPersonAliasId,
                                    id => id,
                                    ( ft, id ) => ft
                            ).OrderBy( ft => ft.TransactionDateTime );

                            if (validTransactions.Count() > 0)
                            {
                                //Hubspot Giving Properties
                                var first_contribution_date_prop = props.FirstOrDefault( p => p.label == "Rock Custom FirstContributionDate" );
                                var last_contribution_date_prop = props.FirstOrDefault( p => p.label == "Rock Custom LastContributionDate" );
                                string firstDate = ConvertDate( validTransactions.First().TransactionDateTime );
                                if (!String.IsNullOrEmpty( firstDate ))
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = first_contribution_date_prop.name, value = firstDate } );
                                }
                                var first_contribution_fund_prop = props.FirstOrDefault( p => p.label == "Rock Custom FirstContributionFund" );
                                properties.Add( new HubspotPropertyUpdate() { property = first_contribution_fund_prop.name, value = validTransactions.First().TransactionDetails.First().Account.Name } );

                                //Get Last Transaction
                                validTransactions = validTransactions.OrderByDescending( ft => ft.TransactionDateTime );
                                string lastDate = ConvertDate( validTransactions.First().TransactionDateTime );
                                if (!String.IsNullOrEmpty( lastDate ))
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = last_contribution_date_prop.name, value = lastDate } );
                                }
                                var last_contribution_fund_prop = props.FirstOrDefault( p => p.label == "Rock Custom LastContributionFund" );
                                properties.Add( new HubspotPropertyUpdate() { property = last_contribution_fund_prop.name, value = validTransactions.First().TransactionDetails.First().Account.Name } );
                            }

                            //ZipCode
                            var homeAddress = person.GetHomeLocation();
                            if(homeAddress != null )
                            {
                                var zipcode_prop = props.FirstOrDefault( p => p.label == "Rock Custom ZipCode" );
                                if(zipcode_prop != null )
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = zipcode_prop.name, value = homeAddress.PostalCode } );
                                }
                            }

                            var includeTMBT = dataMap.GetString( "IncludeTMBT" ).AsBoolean();
                            if (includeTMBT)
                            {
                                Guid? accountGuid = dataMap.GetString( "FinancialAccount" ).AsGuidOrNull();
                                if (accountGuid.HasValue)
                                {
                                    var account = new FinancialAccountService( _context ).Get( accountGuid.Value );
                                    var tmbtTransactions = ft_svc.Queryable().Where( ft => ft.TransactionDetails.Any( ftd => ftd.AccountId == account.Id ) ).Join( givingAliasIds,
                                        ft => ft.AuthorizedPersonAliasId,
                                        id => id,
                                        ( ft, id ) => ft
                                    ).OrderBy( ft => ft.TransactionDateTime );
                                    if (tmbtTransactions.Count() > 0)
                                    {
                                        //Total Amount
                                        var total = tmbtTransactions.Sum( ft => ft.TransactionDetails.Sum( ftd => ftd.Amount ) );
                                        var total_contribution_amount_prop = tmbtProps.FirstOrDefault( p => p.label == "Rock Custom Total TMBT Contribution Amount" );
                                        properties.Add( new HubspotPropertyUpdate() { property = total_contribution_amount_prop.name, value = total.ToString() } );
                                        var first_contribution_amt_prop = tmbtProps.FirstOrDefault( p => p.label == "Rock Custom First TMBT Contribution Amount" );
                                        properties.Add( new HubspotPropertyUpdate() { property = first_contribution_amt_prop.name, value = tmbtTransactions.First().TotalAmount.ToString() } );
                                        var first_contribution_date_prop = tmbtProps.FirstOrDefault( p => p.label == "Rock Custom First TMBT Contribution Date" );
                                        string firstDate = ConvertDate( tmbtTransactions.First().TransactionDateTime );
                                        if (!String.IsNullOrEmpty( firstDate ))
                                        {
                                            properties.Add( new HubspotPropertyUpdate() { property = first_contribution_date_prop.name, value = firstDate } );
                                        }

                                        string frquency = String.Join( ", ", tmbtTransactions.Select( ft => ft.ScheduledTransactionId.HasValue ? ft.ScheduledTransaction.TransactionFrequencyValue.Description : "One Time" ).Distinct() );
                                        var frequency_prop = tmbtProps.FirstOrDefault( p => p.label == "Rock Custom TMBT Contribution Frequency" );
                                        properties.Add( new HubspotPropertyUpdate() { property = frequency_prop.name, value = frquency } );

                                        tmbtTransactions = tmbtTransactions.OrderByDescending( ft => ft.TransactionDateTime );

                                        var last_contribution_amt_prop = tmbtProps.FirstOrDefault( p => p.label == "Rock Custom Last TMBT Contribution Amount" );
                                        properties.Add( new HubspotPropertyUpdate() { property = last_contribution_amt_prop.name, value = tmbtTransactions.First().TotalAmount.ToString() } );
                                        var last_contribution_date_prop = tmbtProps.FirstOrDefault( p => p.label == "Rock Custom Last TMBT Contribution Date" );
                                        string lastDate = ConvertDate( tmbtTransactions.First().TransactionDateTime );
                                        if (!String.IsNullOrEmpty( lastDate ))
                                        {
                                            properties.Add( new HubspotPropertyUpdate() { property = last_contribution_date_prop.name, value = lastDate } );
                                        }
                                    }
                                }
                            }
                        }
                        //WriteToLog( string.Format( "    After Custom: {0}", watch.ElapsedMilliseconds ) );

                        //If the Hubspot Contact does not have FirstName, LastName, or Phone Number we want to update those...
                        if (String.IsNullOrEmpty( contacts[i].properties.firstname ))
                        {
                            properties.Add( new HubspotPropertyUpdate() { property = "firstname", value = person.NickName } );
                        }
                        if (String.IsNullOrEmpty( contacts[i].properties.lastname ))
                        {
                            properties.Add( new HubspotPropertyUpdate() { property = "lastname", value = person.LastName } );
                        }
                        if (String.IsNullOrEmpty( contacts[i].properties.phone ))
                        {
                            PhoneNumber mobile = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == 12 );
                            if (mobile != null && !mobile.IsUnlisted)
                            {
                                properties.Add( new HubspotPropertyUpdate() { property = "phone", value = mobile.NumberFormatted } );
                            }
                        }

                        //Update the Contact in Hubspot
                        MakeRequest( current_id, url, properties, 0 );
                        //WriteToLog( string.Format( "    After Request: {0}", watch.ElapsedMilliseconds ) );

                    }
                    catch (Exception err)
                    {
                        ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{err}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Exception from Job:{Environment.NewLine}{err.Message}{Environment.NewLine}" ) );
                    }
                }
                //WriteToLog( string.Format( "    End of iteration: {0}", watch.ElapsedMilliseconds ) );
                //watch.Stop();
            }
        }

        private void MakeRequest( int current_id, string url, List<HubspotPropertyUpdate> properties, int attempt )
        {
            //Update the Hubspot Contact
            try
            {
                //For Testing Write to Log File
                //WriteToLog( string.Format( "{0}     ID: {1}{2}PROPS:{2}{3}", RockDateTime.Now.ToString( "HH:mm:ss.ffffff" ), current_id, Environment.NewLine, JsonConvert.SerializeObject( properties ) ) );

                var client = new RestClient( url );
                client.Timeout = -1;
                var request = new RestRequest( Method.PATCH );
                request.AddHeader( "accept", "application/json" );
                request.AddHeader( "content-type", "application/json" );
                request.AddHeader( "Authorization", $"Bearer {key}" );
                request.AddParameter( "application/json", $"{{\"properties\": {{ {String.Join( ",", properties.Select( p => $"\"{p.property}\": \"{p.value}\"" ) )} }} }}", ParameterType.RequestBody );
                IRestResponse response = client.Execute( request );
                if ((int) response.StatusCode == 429)
                {
                    if (attempt < 3)
                    {
                        Thread.Sleep( 9000 );
                        MakeRequest( current_id, url, properties, attempt + 1 );
                    }
                }
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception( response.Content );
                }
            }
            catch (Exception e)
            {
                var json = $"{{\"properties\": {JsonConvert.SerializeObject( properties )} }}";
                ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{e}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Exception from Request:{Environment.NewLine}{e.Message}{Environment.NewLine}Request:{Environment.NewLine}{json}{Environment.NewLine}" ) );
            }
        }

        private void WriteToLog( string message )
        {
            string logFile = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/Logs/HubSpotPatchLog.txt" );
            using (System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ))
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ))
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
            contacts.AddRange( contactResults.results.Where( c => c.properties.rock_id != null && c.properties.rock_id != "" && c.properties.email != null && c.properties.email != "" && c.properties.hs_all_assigned_business_unit_ids != null && c.properties.hs_all_assigned_business_unit_ids != "" && c.properties.hs_all_assigned_business_unit_ids.Split( ';' ).Contains( businessUnit ) ).ToList() );
            if (contactResults.paging != null && contactResults.paging.next != null && !String.IsNullOrEmpty( contactResults.paging.next.link ) && request_count < 500)
            {
                GetContacts( contactResults.paging.next.link );
            }
        }

        private string ConvertDate( DateTime? date )
        {
            if (date.HasValue)
            {
                DateTime today = RockDateTime.Now;
                if (today.Year - date.Value.Year < 1000 && today.Year - date.Value.Year > -1000)
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
