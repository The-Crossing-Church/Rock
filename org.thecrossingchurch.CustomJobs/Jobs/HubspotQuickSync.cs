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
using HubSpot.NET.Core;
using Newtonsoft.Json;
using System.Net;
using HubSpot.NET.Api.Contact.Dto;
using System.IO;
using System.Reflection;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using System.Threading;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace org.crossingchurch.HubspotQuickSync.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( "Hubspot Property", "Property to update in Hubspot", false, "", "HubSpot Data", 2 )]
    [TextField( "Hubspot Value", "Value to write to the property for all people that match criteria", false, "", "HubSpot Data", 3 )]
    [TextField( "Contact Source", "Value ot use for the contact source in HubSpot if a new person is added ot HubSpot", false, "", "HubSpot Data", 4 )]
    [AttributeField( "Person Attributes",
        Description = "Additional person attributes to save to Hubspot",
        AllowMultiple = true,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        Category = "HubSpot Data",
        IsRequired = false )]
    [DataViewsField( "DataViews Filter", "Filter to people in these dataviews", false, "", "Rock.Model.Person", "Rock Filters", 0 )]
    [GroupField( "Group Filter", "Filter to people in this group", false, "", "Rock Filters", 1 )]
    [RegistrationInstanceField( "Registration Filter", "Filter to registrants in this registration", false, "", "Rock Filters", 2 )]
    [AttributeField( "Person Attribute Filter",
        Description = "Filter people based on this attribute",
        AllowMultiple = false,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        Category = "Rock Filters",
        IsRequired = false,
        Order = 3 )]
    [TextField( "Person Attribute Filter Value", "Filter to people with this value for the selected attribute", false, "", "Rock Filters", 4 )]
    [DisallowConcurrentExecution]
    public class HubspotQuickSync : IJob
    {

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public HubspotQuickSync()
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
            //Variables
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            RockContext _context = new RockContext();

            string key = GlobalAttributesCache.Get().GetValue( "HubspotAPIKeyGlobal" );
            string property = dataMap.GetString( "HubspotProperty" );
            string propertyValue = dataMap.GetString( "HubspotValue" );
            string contactSource = dataMap.GetString( "ContactSource" );
            List<Guid?> syncAttrGuids = dataMap.GetString( "PersonAttributes" ).Split( ',' ).AsGuidOrNullList();
            List<Guid?> filterDataViewGuids = dataMap.GetString( "DataViewsFilter" ).Split( ',' ).AsGuidOrNullList();
            Guid? filterGroupGuid = dataMap.GetString( "GroupFilter" ).AsGuidOrNull();
            Guid? filterRegistrationGuid = dataMap.GetString( "RegistrationFilter" ).AsGuidOrNull();
            Guid? filterAttributeGuid = dataMap.GetString( "PersonAttributeFilter" ).AsGuidOrNull();
            string filterAttributeValue = dataMap.GetString( "PersonAttributeFilterValue" );

            HubSpotApi api = new HubSpotApi( key );

            //Get custom contact properties from Hubspot 
            WebRequest request = WebRequest.Create( $"https://api.hubapi.com/properties/v1/contacts/properties?hapikey={key}" );
            var response = request.GetResponse();
            var props = new List<HubspotProperty>();
            using ( Stream stream = response.GetResponseStream() )
            {
                using ( StreamReader reader = new StreamReader( stream ) )
                {
                    var jsonResponse = reader.ReadToEnd();
                    props = JsonConvert.DeserializeObject<List<HubspotProperty>>( jsonResponse );
                }
            }
            //Only care about the ones that are custom and could be valid Rock fields
            props = props.Where( p => p.createdUserId != null ).ToList();

            //Get List of all contacts from Hubspot
            List<HubSpotContact> contacts = new List<HubSpotContact>();
            long offset = 0;
            var hasmore = true;
            while ( hasmore )
            {
                var list = GetContacts( api, offset, 0 );
                hasmore = list.MoreResultsAvailable;
                offset = list.ContinuationOffset;
                contacts.AddRange( list.Contacts );
            }
            //Contacts with emails only 
            var contacts_with_email = contacts.Where( c => c.Email != null ).ToList();

            //Get All Active Records
            List<Person> syncContacts = new PersonService( _context ).Queryable().Where( p => p.RecordStatusValue.Guid.ToString() == Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE ).ToList();

            //Filter By DataViews
            for ( int i = 0; i < filterDataViewGuids.Count(); i++ )
            {
                if ( filterDataViewGuids[i].HasValue )
                {
                    var dataview = new DataViewService( _context ).Get( filterDataViewGuids[i].Value );
                    List<string> errors = new List<string>();
                    var serviceInstance = Reflection.GetServiceForEntityType( typeof( Person ), _context );
                    ParameterExpression parameterExpression = Expression.Parameter( typeof( Person ), "x" );
                    var exp = dataview.GetExpression( serviceInstance, parameterExpression, out errors );

                    MethodInfo getMethod = serviceInstance.GetType().GetMethod( "Get", new Type[] { typeof( ParameterExpression ), typeof( Expression ), typeof( Rock.Web.UI.Controls.SortProperty ), typeof( int? ) } );

                    var getResult = getMethod.Invoke( serviceInstance, new object[] { parameterExpression, exp, null, null } );
                    var queryResult = getResult as IQueryable<Person>;
                    syncContacts = syncContacts.Intersect( queryResult ).ToList();
                }
            }

            //Filter By Group
            if ( filterGroupGuid.HasValue )
            {
                Group group = new GroupService( _context ).Get( filterGroupGuid.Value );
                syncContacts = syncContacts.Intersect( group.Members.Select( gm => gm.Person ) ).ToList();
            }

            //Filter By Registration
            if ( filterRegistrationGuid.HasValue )
            {
                RegistrationInstance registration = new RegistrationInstanceService( _context ).Get( filterRegistrationGuid.Value );
                syncContacts = syncContacts.Intersect( registration.Registrations.SelectMany( r => r.Registrants.Select( rr => rr.Person ) ) ).ToList();
            }

            syncContacts.LoadAttributes();

            //Filter By Attribute Value
            if ( filterAttributeGuid.HasValue && !String.IsNullOrEmpty( filterAttributeValue ) )
            {
                Rock.Model.Attribute attribute = new AttributeService( _context ).Get( filterAttributeGuid.Value );
                syncContacts = syncContacts.Where( p => p.AttributeValues.ContainsKey( attribute.Key ) && p.AttributeValues[attribute.Key].ValueFormatted == filterAttributeValue ).ToList();
            }

            int count = 0;
            for ( var i = 0; i < syncContacts.Count(); i++ )
            {
                var matches = contacts_with_email.Where( c => c.rock_id == syncContacts[i].Id.ToString() || c.Email.ToLower() == syncContacts[i].Email.ToLower() ).ToList();
                //Set properties
                List<HubspotPropertyUpdate> properties = new List<HubspotPropertyUpdate>();
                if ( !String.IsNullOrEmpty( property ) && !String.IsNullOrEmpty( propertyValue ) )
                {
                    properties.Add( new HubspotPropertyUpdate { property = property, value = propertyValue } );
                }
                foreach ( Guid? g in syncAttrGuids )
                {
                    if ( g.HasValue )
                    {
                        Rock.Model.Attribute attr = new AttributeService( _context ).Get( g.Value );
                        var hsProp = props.FirstOrDefault( hsp => hsp.label == attr.Key );
                        var val = syncContacts[i].AttributeValues[attr.Key];
                        if ( !String.IsNullOrEmpty( val.Value ) )
                        {
                            properties.Add( new HubspotPropertyUpdate { property = hsProp.name, value = val.ValueFormatted } );
                        }
                    }
                }
                //Update Existing HubSpot Contacts
                if ( matches.Count() > 0 )
                {
                    for ( var k = 0; k < matches.Count(); k++ )
                    {
                        //Check Rate Limit
                        if ( count % 100 == 0 )
                        {
                            System.Threading.Thread.Sleep( 10000 );
                        }
                        MakeRequest( "PATCH", $"https://api.hubapi.com/crm/v3/objects/contacts/{matches[k].Id}?hapikey={key}", properties, syncContacts[i].Id, 0 );
                        //Increase count for rate-limit check
                        count++;
                    }
                }
                //Create new HubSpot Contact
                else
                {
                    //Check Rate Limit
                    if ( count % 100 == 0 )
                    {
                        System.Threading.Thread.Sleep( 10000 );
                    }
                    properties.Add( new HubspotPropertyUpdate { property = "email", value = syncContacts[i].Email } );
                    properties.Add( new HubspotPropertyUpdate { property = "firstname", value = syncContacts[i].NickName } );
                    properties.Add( new HubspotPropertyUpdate { property = "lastname", value = syncContacts[i].LastName } );
                    if ( !String.IsNullOrEmpty( contactSource ) )
                    {
                        properties.Add( new HubspotPropertyUpdate { property = "contact_source", value = contactSource } );
                    }
                    MakeRequest( "POST", $"https://api.hubapi.com/crm/v3/objects/contacts?hapikey={key}", properties, syncContacts[i].Id, 0 );
                    //Increase count for rate-limit check
                    count++;
                }
            }

        }

        private ContactListHubSpotModel<HubSpotContact> GetContacts( HubSpotApi api, long offset, int attempt )
        {
            ContactListHubSpotModel<HubSpotContact> list = new ContactListHubSpotModel<HubSpotContact>();
            try
            {
                list = api.Contact.List<HubSpotContact>( new ListRequestOptions
                {
                    PropertiesToInclude = new List<string> { "firstname", "lastname", "email", "phone", "rock_id", "rock_firstname", "rock_lastname", "rock_email", "which_best_describes_your_involvement_with_the_crossing_", "createdate", "lastmodifieddate", "has_potential_rock_match" },
                    Limit = 100,
                    Offset = offset
                } );
            }
            catch ( Exception e )
            {
                HttpContext context2 = HttpContext.Current;
                ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{e}{Environment.NewLine}Exception from Library:{Environment.NewLine}{e.Message}{Environment.NewLine}" ), context2 );
                if ( attempt < 5 )
                {
                    Thread.Sleep( 60000 );
                    GetContacts( api, offset, attempt + 1 );
                }
                else
                {
                    throw new Exception( "Hubspot Sync Error: Failed to get Hubspot Contacts", e );
                }
            }
            return list;
        }

        private void MakeRequest( string method, string url, List<HubspotPropertyUpdate> properties, int id, int attempt )
        {
            //Update the Hubspot Contact
            try
            {
                var webrequest = WebRequest.Create( url );
                webrequest.Method = method;
                webrequest.ContentType = "application/json";
                using ( Stream requestStream = webrequest.GetRequestStream() )
                {
                    var json = $"{{\"properties\": {GenerateJSON( properties )} }}";
                    byte[] bytes = Encoding.ASCII.GetBytes( json );
                    requestStream.Write( bytes, 0, bytes.Length );
                }
                using ( WebResponse webResponse = webrequest.GetResponse() )
                {
                    using ( Stream responseStream = webResponse.GetResponseStream() )
                    {
                        using ( StreamReader reader = new StreamReader( responseStream ) )
                        {
                            var jsonResponse = reader.ReadToEnd();
                            Console.WriteLine( jsonResponse );
                        }
                    }
                }

            }
            catch ( WebException ex )
            {
                using ( Stream responseStream = ex.Response.GetResponseStream() )
                {
                    using ( StreamReader reader = new StreamReader( responseStream ) )
                    {
                        var jsonResponse = reader.ReadToEnd();
                        Console.WriteLine( $"Hubspot: {jsonResponse}" );
                        HttpContext context2 = HttpContext.Current;
                        ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error<br/>{ex}<br/>Current Id: {id}<br/>URL: {url}<br/>Body: {GenerateJSON( properties )}<br/>Response:{jsonResponse} " ), context2 );
                        //Retry the request
                        if ( attempt < 5 )
                        {
                            Thread.Sleep( 10000 );
                            //Task.Delay( TimeSpan.FromSeconds( 60 ) ).ContinueWith( t => MakeRequest( method, url, properties, id, attempt + 1 ) );
                            MakeRequest( method, url, properties, id, attempt + 1 );
                        }
                    }
                }
            }
            catch ( Exception e )
            {
                HttpContext context2 = HttpContext.Current;
                var json = $"{{\"properties\": {JsonConvert.SerializeObject( properties )} }}";
                ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{e}{Environment.NewLine}Current Id: {id}{Environment.NewLine}Exception from Request:{Environment.NewLine}{e.Message}{Environment.NewLine}Request:{Environment.NewLine}{json}{Environment.NewLine}" ), context2 );
            }
        }

        private string GenerateJSON( List<HubspotPropertyUpdate> properties )
        {
            string json = "{";
            for ( var i = 0; i < properties.Count(); i++ )
            {
                json += $"\"{properties[i].property}\":\"{properties[i].value}\",";
            }
            json = json.Substring( 0, json.Length - 1 );
            json += "}";
            return json;
        }
    }

    public class HubspotProperty
    {
        public string name { get; set; }
        public string label { get; set; }
        public string fieldType { get; set; }
        public bool? deleted { get; set; }
        public int? createdUserId { get; set; }
    }

    public class HubspotPropertyUpdate
    {
        public string property { get; set; }
        public string value { get; set; }
    }

    public class HubSpotContact : ContactHubSpotModel
    {
        public string rock_firstname { get; set; }
        public string rock_lastname { get; set; }
        public string rock_email { get; set; }
        public string rock_id { get; set; }
        public string which_best_describes_your_involvement_with_the_crossing_ { get; set; }
        public string lastmodifieddate { get; set; }
        public string createdate { get; set; }
    }
}
