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

namespace org.crossingchurch.HubspotGivingIntegration.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [AccountField( "Fund", "The fund to pull people from to add to HubSpot", true, "", "", 0 )]
    [TextField( "Hubspot Property", "Property to Update in Hubspot", true, "", "", 2 )]
    [AttributeField( "Person Attributes",
        Description = "Person Attributes to save to Hubspot",
        AllowMultiple = true,
        EntityTypeGuid = Rock.SystemGuid.EntityType.PERSON,
        IsRequired = false )]
    [DisallowConcurrentExecution]
    public class HubspotGivingIntegration : IJob
    {

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public HubspotGivingIntegration()
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
            string fundGuid = dataMap.GetString( "Fund" );
            List<string> rockAttrGuids = dataMap.GetString( "PersonAttributes" ).Split( ',' ).ToList();
            FinancialAccount fund = new FinancialAccountService( _context ).Get( Guid.Parse( fundGuid ) );
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
                var list = api.Contact.List<HubSpotContact>( new ListRequestOptions
                {
                    PropertiesToInclude = new List<string> { "firstname", "lastname", "email", "phone", "rock_id", "rock_firstname", "rock_lastname", "rock_email", "which_best_describes_your_involvement_with_the_crossing_", "createdate", "lastmodifieddate" },
                    Limit = 100,
                    Offset = offset
                } );
                hasmore = list.MoreResultsAvailable;
                offset = list.ContinuationOffset;
                contacts.AddRange( list.Contacts );
            }
            //Contacts with emails only 
            var contacts_with_email = contacts.Where( c => c.Email != null ).ToList();

            var startOfDay = RockDateTime.Now;
            startOfDay = new DateTime( startOfDay.Year, startOfDay.Month, startOfDay.Day, 0, 0, 0 );

            //Get all people in Rock who have donated to the Fund
            List<Person> sponsors = new FinancialTransactionService( _context ).Queryable().Where( ft => DateTime.Compare( ft.TransactionDateTime.Value, startOfDay ) >= 0 && ft.TransactionDetails.Any( ftd => ftd.AccountId == fund.Id ) ).Select( ft => ft.AuthorizedPersonAlias.Person ).ToList();

            int count = 0;
            for ( var i = 0; i < sponsors.Count(); i++ )
            {
                sponsors[i].LoadAttributes();
                var matches = contacts_with_email.Where( c => c.rock_id == sponsors[i].Id.ToString() || c.Email.ToLower() == sponsors[i].Email.ToLower() ).ToList();
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
                        List<HubspotPropertyUpdate> properties = new List<HubspotPropertyUpdate>();
                        properties.Add( new HubspotPropertyUpdate { property = property, value = "Yes" } );
                        foreach ( string g in rockAttrGuids )
                        {
                            Rock.Model.Attribute attr = new AttributeService( _context ).Get( Guid.Parse( g ) );
                            var hsProp = props.FirstOrDefault( hsp => hsp.label == attr.Key );
                            var val = sponsors[i].AttributeValues[attr.Key];
                            if ( !String.IsNullOrEmpty( val.Value ) )
                            {
                                properties.Add( new HubspotPropertyUpdate { property = hsProp.name, value = val.ValueFormatted } );
                            }
                        }
                        MakeRequest( "PATCH", $"https://api.hubapi.com/crm/v3/objects/contacts/{matches[k].Id}?hapikey={key}", properties, sponsors[i].Id );
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
                    List<HubspotPropertyUpdate> properties = new List<HubspotPropertyUpdate>();
                    properties.Add( new HubspotPropertyUpdate { property = "email", value = sponsors[i].Email } );
                    properties.Add( new HubspotPropertyUpdate { property = "firstname", value = sponsors[i].NickName } );
                    properties.Add( new HubspotPropertyUpdate { property = "lastname", value = sponsors[i].LastName } );
                    properties.Add( new HubspotPropertyUpdate { property = property, value = "Yes" } );
                    foreach ( string g in rockAttrGuids )
                    {
                        Rock.Model.Attribute attr = new AttributeService( _context ).Get( Guid.Parse( g ) );
                        var hsProp = props.FirstOrDefault( hsp => hsp.label == attr.Key );
                        var val = sponsors[i].AttributeValues[attr.Key];
                        if ( !String.IsNullOrEmpty( val.Value ) )
                        {
                            properties.Add( new HubspotPropertyUpdate { property = hsProp.name, value = val.Value } );
                        }
                    }
                    MakeRequest( "POST", $"https://api.hubapi.com/crm/v3/objects/contacts?hapikey={key}", properties, sponsors[i].Id );
                    //Increase count for rate-limit check
                    count++;
                }
            }

        }

        private void MakeRequest( string method, string url, List<HubspotPropertyUpdate> properties, int id )
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
                        ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{ex}{Environment.NewLine}Current Id: {id}{Environment.NewLine}Response:{jsonResponse} " ), context2 );
                    }
                }
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
