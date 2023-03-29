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

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [DisallowConcurrentExecution]
    [TextField( "AttributeKey", "", true, "HubspotAPIKeyGlobal" )]
    public class HubspotIntegration : IJob
    {
        private string key { get; set; }
        private List<HSContactResult> contacts { get; set; }
        private int request_count { get; set; }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public HubspotIntegration()
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
            key = GlobalAttributesCache.Get().GetValue( attrKey );

            var current_id = 0;

            PersonService personService = new PersonService( new RockContext() );

            //Set up Static Report of Potential Matches
            ExcelPackage excel = new ExcelPackage();
            excel.Workbook.Properties.Title = "Potential Matches";
            excel.Workbook.Properties.Author = "Rock";
            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( "Potential Matches" );
            worksheet.PrinterSettings.LeftMargin = .5m;
            worksheet.PrinterSettings.RightMargin = .5m;
            worksheet.PrinterSettings.TopMargin = .5m;
            worksheet.PrinterSettings.BottomMargin = .5m;
            var headers = new List<string> { "HubSpot FirstName", "Rock FirstName", "HubSpot LastName", "Rock LastName", "HubSpot Email", "Rock Email", "HubSpot Phone", "Rock Phone", "HubSpot Connection Status", "Rock Connection Status", "HubSpot Link", "Rock Link", "HubSpot CreatedDate", "Rock Created Date", "HubSpot Modified Date", "Rock Modified Date", "Rock ID" };
            var h = 1;
            var row = 2;
            foreach ( var header in headers )
            {
                worksheet.Cells[1, h].Value = header;
                h++;
            }

            //Get Hubspot Properties in Rock Information Group
            //This will allow us to add properties temporarily to the sync and then not continue to have them forever
            var propClient = new RestClient( "https://api.hubapi.com/crm/v3/properties/contacts?properties=name,label,createdUserId,groupName,options,fieldType" );
            propClient.Timeout = -1;
            var propRequest = new RestRequest( Method.GET );
            propRequest.AddHeader( "Authorization", $"Bearer {key}" );
            IRestResponse propResponse = propClient.Execute( propRequest );
            var props = new List<HubspotProperty>();
            var propsQry = JsonConvert.DeserializeObject<HSPropertyQueryResult>( propResponse.Content );
            props = propsQry.results;

            //Filter to props in Rock Information Group
            props = props.Where( p => p.groupName == "rock_information" ).ToList();
            //Business Unit hs_all_assigned_business_unit_ids
            //Save a list of the ones that are Rock attributes
            var attrs = props.Where( p => p.label.Contains( "Rock Attribute " ) ).ToList();
            RockContext _context = new RockContext();
            List<string> attrKeys = attrs.Select( hs => hs.label.Replace( "Rock Attribute ", "" ) ).ToList();
            var rockAttributes = new AttributeService( _context ).Queryable().Where( a => a.EntityTypeId == 15 && attrKeys.Contains( a.Key ) );
            var rockAttributeValues = new AttributeValueService( _context ).Queryable().Join( rockAttributes,
                    av => av.AttributeId,
                    attr => attr.Id,
                    ( av, attr ) => av
                );
            foreach ( var av in rockAttributeValues )
            {
                av.Attribute = rockAttributes.FirstOrDefault( a => a.Id == av.AttributeId );
            }

            //Get List of all contacts from Hubspot
            contacts = new List<HSContactResult>();
            request_count = 0;
            GetContacts( "https://api.hubapi.com/crm/v3/objects/contacts?limit=100&properties=email,firstname,lastname,phone,hs_all_assigned_business_unit_ids,rock_id,which_best_describes_your_involvement_with_the_crossing_,has_potential_rock_match,createdate,lastmodifieddate" );

            //Contacts with emails only in The Crossing Business Unit
            var contacts_with_email = contacts.Where( c => c.properties.email != null && c.properties.hs_all_assigned_business_unit_ids != null && c.properties.hs_all_assigned_business_unit_ids.Split( ',' ).Contains( "0" ) ).ToList();

            //Foreach contact with an email, look for a 1:1 match in Rock by email and schedule it's update 
            for ( var i = 0; i < contacts_with_email.Count(); i++ )
            {
                //First Check if they have a rock Id in their hubspot data to use
                Person person = null;
                if ( !String.IsNullOrEmpty( contacts_with_email[i].properties.rock_id ) )
                {
                    int id;
                    if ( Int32.TryParse( contacts_with_email[i].properties.rock_id, out id ) )
                    {
                        person = personService.Get( id );
                    }
                }

                //If there is not a value for Rock Id, proceed to run the query with HubSpot data
                if ( person == null )
                {
                    var query = new PersonService.PersonMatchQuery( contacts_with_email[i].properties.firstname, contacts_with_email[i].properties.lastname, contacts_with_email[i].properties.email, contacts_with_email[i].properties.phone );
                    person = personService.FindPerson( query, false );
                }

                bool hasMultiEmail = false;
                //Attempt to match on email and one other piece of info if we are still null
                if ( person == null )
                {
                    string email = contacts_with_email[i].properties.email.ToLower();
                    var matches = personService.Queryable().Where( p => p.Email.ToLower() == email ).ToList();
                    if ( matches.Count() == 1 )
                    {
                        //1:1 Email match so we want to check other information, start by checking for a name match 
                        if ( CustomEquals( matches.First().NickName, contacts_with_email[i].properties.firstname ) ||
                            CustomEquals( matches.First().FirstName, contacts_with_email[i].properties.firstname ) ||
                            CustomEquals( matches.First().LastName, contacts_with_email[i].properties.lastname ) )
                        {
                            person = matches.First();
                        }
                        else if ( !String.IsNullOrEmpty( contacts_with_email[i].properties.phone ) )
                        {
                            //Try looking for a phone number
                            if ( matches.First().PhoneNumbers.Select( p => p.Number ).Contains( contacts_with_email[i].properties.phone ) )
                            {
                                person = matches.First();
                            }
                        }
                        //If 1:1 Email match and Hubspot has no other info, make it a match
                        if ( person == null && String.IsNullOrEmpty( contacts_with_email[i].properties.firstname ) && String.IsNullOrEmpty( contacts_with_email[i].properties.lastname ) )
                        {
                            person = matches.First();
                        }
                    }
                    else
                    {
                        hasMultiEmail = true;
                    }
                }

                //Atempt to match 1:1 based on email history making sure we exclude emails with multiple matches in the person table
                if ( person == null && !hasMultiEmail )
                {
                    string email = contacts_with_email[i].properties.email.ToLower();
                    var history = new HistoryService( new RockContext() ).Queryable().Where( hist => hist.EntityTypeId == 15 && hist.ValueName == "Email" );
                    var matches = history.Where( hist => hist.NewValue.ToLower() == email ).ToList();
                    if ( matches.Count() == 1 )
                    {
                        //If 1:1 Email match and Hubspot has no other info, make it a match
                        if ( String.IsNullOrEmpty( contacts_with_email[i].properties.firstname ) && String.IsNullOrEmpty( contacts_with_email[i].properties.lastname ) )
                        {
                            person = personService.Get( matches.First().EntityId );
                        }
                    }
                }

                bool inBucket = false;
                //Try to mark people that are potential matches, only people who can at least match email or phone and one other thing
                if ( person == null )
                {
                    var contact = contacts_with_email[i];
                    //Matches phone number and one other piece of info
                    if ( !String.IsNullOrEmpty( contact.properties.phone ) )
                    {
                        var phone_matches = personService.Queryable().Where( p => p.PhoneNumbers.Select( pn => pn.Number ).Contains( contact.properties.phone ) ).ToList();
                        if ( phone_matches.Count() > 0 )
                        {
                            phone_matches = phone_matches.Where( p => CustomEquals( p.FirstName, contact.properties.firstname ) || CustomEquals( p.NickName, contact.properties.firstname ) || CustomEquals( p.Email, contact.properties.email ) || CustomEquals( p.LastName, contact.properties.lastname ) ).ToList();
                            for ( var j = 0; j < phone_matches.Count(); j++ )
                            {
                                //Save this information in the excel sheet....
                                SaveData( worksheet, row, phone_matches[j], contact );
                                inBucket = true;
                                row++;
                            }
                        }
                    }
                    //Matches email and one other piece of info
                    var email_matches = personService.Queryable().ToList().Where( p =>
                    {
                        return CustomEquals( p.Email, contact.properties.email );
                    } ).ToList();
                    if ( email_matches.Count() > 0 )
                    {
                        email_matches = email_matches.Where( p => CustomEquals( p.FirstName, contact.properties.firstname ) || CustomEquals( p.NickName, contact.properties.firstname ) || ( !String.IsNullOrEmpty( contact.properties.phone ) && p.PhoneNumbers.Select( pn => pn.Number ).Contains( contact.properties.phone ) ) || CustomEquals( p.LastName, contact.properties.lastname ) ).ToList();
                        for ( var j = 0; j < email_matches.Count(); j++ )
                        {
                            //Save this information in the excel sheet....
                            SaveData( worksheet, row, email_matches[j], contact );
                            inBucket = true;
                            row++;
                        }
                    }
                }

                //For Testing
                //if ( contacts_with_email[i].properties.email != "coolrobot@hubspot.com" )
                //{
                //    person = null;
                //}
                //else
                //{
                //    person = personService.Get(  );
                //}

                //Schedule HubSpot update if 1:1 match
                if ( person != null )
                {
                    try
                    {
                        current_id = person.Id;
                        //Build the POST request and schedule in the db 10 at a time 
                        var url = $"https://api.hubapi.com/crm/v3/objects/contacts/{contacts_with_email[i].id}";
                        var properties = new List<HubspotPropertyUpdate>();
                        var personAttributes = rockAttributeValues.Where( av => av.EntityId == person.Id ).ToList();
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
                                    var rockAttr = rockAttributes.ToList().FirstOrDefault( a => "Rock Attribute " + a.Key == attrs[j].label );
                                    if ( rockAttr != null )
                                    {
                                        current_prop = new AttributeValue() { Value = rockAttr.DefaultValue, AttributeId = rockAttr.Id, Attribute = rockAttr };
                                    }
                                }
                            }
                            catch ( Exception e )
                            {
                                ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{e}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Property Name{Environment.NewLine}{attrs[j].label}{Environment.NewLine}Exception from Job:{Environment.NewLine}{e.Message}{Environment.NewLine}" ) );
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
                                        //Set date time to Midnight because HubSpot sucks 
                                        tryDate = new DateTime( tryDate.Year, tryDate.Month, tryDate.Day, 0, 0, 0 );
                                        var d = tryDate.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds * 1000;
                                        properties.Add( new HubspotPropertyUpdate() { property = attrs[j].name, value = d.ToString() } );
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
                                    DefinedValue dv = JsonConvert.DeserializeObject<DefinedValue>( JsonConvert.SerializeObject( propInfo.GetValue( person ) ) );
                                    properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = dv.Value } );
                                }
                                else if ( propInfo.PropertyType.FullName.Contains( "Date" ) )
                                {
                                    //Get Epoc miliseconds
                                    //Possibly not used anymore, switched to regular date format
                                    DateTime tryDate;
                                    if ( DateTime.TryParse( propInfo.GetValue( person ).ToString(), out tryDate ) )
                                    {
                                        //Set date time to Midnight because HubSpot sucks also verify the year is within 1000 years from today
                                        DateTime today = RockDateTime.Now;
                                        if ( today.Year - tryDate.Year < 1000 && today.Year - tryDate.Year > -1000 )
                                        {
                                            tryDate = new DateTime( tryDate.Year, tryDate.Month, tryDate.Day, 0, 0, 0 );
                                            var d = tryDate.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds * 1000;
                                            properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = d.ToString() } );
                                        }
                                        //properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = tryDate.ToString() } );
                                    }
                                }
                                else
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = propInfo.GetValue( person ).ToString() } );
                                }
                            }
                        }

                        //Special Property for Parents
                        if ( person.PrimaryFamily.Members.FirstOrDefault( gm => gm.PersonId == person.Id ).GroupRole.Name == "Adult" )
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
                            for ( var j = 0; j < children.Count(); j++ )
                            {
                                if ( children[j].Person.GradeOffset > 6 )
                                {
                                    //Child is in K-5
                                    if ( !agegroups.Contains( "Elementary" ) )
                                    {
                                        agegroups += "Elementary,";
                                    }
                                }
                                else if ( children[j].Person.GradeOffset > 3 )
                                {
                                    //Child is in 6-8
                                    if ( !agegroups.Contains( "Middle" ) )
                                    {
                                        agegroups += "Middle,";
                                    }
                                }
                                else if ( children[j].Person.GradeOffset <= 3 )
                                {
                                    //Child is in 9-12
                                    if ( !agegroups.Contains( "SeniorHigh" ) )
                                    {
                                        agegroups += "SeniorHigh,";
                                    }
                                }
                                else
                                {
                                    //Check if child is infant-toddler or adult
                                    var bornCheck = DateTime.Now;
                                    if ( children[j].Person.BirthYear >= ( bornCheck.Year - 5 ) )
                                    {
                                        if ( !agegroups.Contains( "EarlyChildhood" ) )
                                        {
                                            agegroups += "EarlyChildhood,";
                                        }
                                    }
                                    else
                                    {
                                        if ( !agegroups.Contains( "Adult" ) )
                                        {
                                            agegroups += "Adult,";
                                        }
                                    }
                                }
                            }
                            if ( agegroups.Length > 0 )
                            {
                                properties.Add( new HubspotPropertyUpdate() { property = child_ages_prop.name, value = agegroups.Substring( 0, agegroups.Length - 1 ) } );
                            }
                        }

                        if ( person.Members != null && person.Members.Count() > 0 )
                        {
                            //Special properties for a person's group membership 
                            //Currently in adult small group, currently in a 20s small group, currently in veritas small group, currently serving, currently in connections, membership list
                            var today = DateTime.UtcNow;
                            var term = "fall";
                            if ( DateTime.Compare( today, new DateTime( today.Year, 5, 15 ) ) <= 0 )
                            {
                                term = "winter";
                            }
                            else if ( DateTime.Compare( today, new DateTime( today.Year, 8, 15 ) ) <= 0 )
                            {
                                term = "summer";
                            }
                            //All current memberships for this year
                            var memberships = person.Members.Where( m => m.Group != null && m.GroupMemberStatus == GroupMemberStatus.Active && m.Group.IsActive && ( m.Group.Name.Contains( today.ToString( "yyyy" ) ) ||
                                 m.Group.Name.Contains( $"{today.AddYears( -1 ).ToString( "yyyy" )}-{today.ToString( "yy" )}" ) ) ).ToList();
                            //Where the group name has Fall/Winter/Summer or Purpose == Serving Area
                            var current_serving = memberships.Where( m => m.Group.Name.ToLower().Contains( term ) || ( m.Group.GroupType.GroupTypePurposeValue != null && m.Group.GroupType.GroupTypePurposeValue.Value == "Serving Area" ) ).ToList();
                            //All current groups with the words Small Group, SG or Purpose == Small Group
                            var current_sg = memberships.Where( m => m.Group.Name.ToLower().Contains( "small group" ) || m.Group.Name.ToLower().Contains( "sg" ) || ( m.Group.GroupType.GroupTypePurposeValue != null && m.Group.GroupType.GroupTypePurposeValue.Value == "Small Group" ) ).ToList();

                            var serving_prop = props.FirstOrDefault( p => p.label == "Rock Custom Currently Serving" );
                            var sg_props = props.Where( p => p.label.Contains( "Small Group" ) ).ToList();

                            //set the serving prop
                            if ( serving_prop != null )
                            {
                                if ( current_serving.Count() > 0 )
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = serving_prop.name, value = "true" } );
                                }
                                else
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = serving_prop.name, value = "false" } );
                                }
                            }
                            //figure out if they attend small group
                            if ( current_sg.Count() > 0 && person.AgeClassification != AgeClassification.Child )
                            {
                                if ( current_sg.Count() > 1 && current_sg.Where( sg => !sg.GroupRole.IsLeader ).Count() > 0 )
                                { //See if we can get this to one small group hopefully
                                    current_sg = current_sg.Where( sg => !sg.GroupRole.IsLeader ).ToList();
                                }
                                foreach ( var sg in current_sg )
                                {
                                    var small_group = sg_props.FirstOrDefault( p => p.label == "Rock Custom Currently in Adult Small Group" );
                                    if ( sg.Group.ParentGroup.Name.ToLower().Contains( "veritas" ) )
                                    {
                                        small_group = sg_props.FirstOrDefault( p => p.label == "Rock Custom Currently in Veritas Small Group" );
                                    }
                                    else if ( sg.Group.ParentGroup.Name.ToLower().Contains( "twenties" ) )
                                    {
                                        small_group = sg_props.FirstOrDefault( p => p.label == "Rock Custom Currently in Twenties Small Group" );
                                    }

                                    var exists = properties.FirstOrDefault( p => p.property == small_group.name );
                                    if ( exists == null )
                                    {
                                        properties.Add( new HubspotPropertyUpdate() { property = small_group.name, value = "true" } );
                                    }
                                }
                            }
                            //Make the other values false so we keep the list up to date
                            foreach ( var sg_prop in sg_props )
                            {
                                var exists = properties.FirstOrDefault( p => p.property == sg_prop.name );
                                if ( exists == null )
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = sg_prop.name, value = "false" } );
                                }
                            }
                        }

                        //If the Hubspot Contact does not have FirstName, LastName, or Phone Number we want to update those...
                        if ( String.IsNullOrEmpty( contacts_with_email[i].properties.firstname ) )
                        {
                            properties.Add( new HubspotPropertyUpdate() { property = "firstname", value = person.NickName } );
                        }
                        if ( String.IsNullOrEmpty( contacts_with_email[i].properties.lastname ) )
                        {
                            properties.Add( new HubspotPropertyUpdate() { property = "lastname", value = person.LastName } );
                        }
                        if ( String.IsNullOrEmpty( contacts_with_email[i].properties.phone ) )
                        {
                            PhoneNumber mobile = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == 12 );
                            if ( mobile != null && !mobile.IsUnlisted )
                            {
                                properties.Add( new HubspotPropertyUpdate() { property = "phone", value = mobile.NumberFormatted } );
                            }
                        }

                        //Update the Contact in Hubspot
                        var alreadyKnown = contacts_with_email[i].properties.has_potential_rock_match;
                        if ( alreadyKnown == "True" )
                        {
                            //Update the bucket prop to false since they are no longer in the potential matches, but actually matched.
                            var bucket_prop = props.FirstOrDefault( p => p.label == "Rock Custom Has Potential Rock Match" );
                            properties.Add( new HubspotPropertyUpdate() { property = bucket_prop.name, value = "False" } );
                        }
                        MakeRequest( current_id, url, properties, 0 );

                    }
                    catch ( Exception err )
                    {
                        ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{err}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Exception from Job:{Environment.NewLine}{err.Message}{Environment.NewLine}" ) );
                    }
                }
                else
                {
                    if ( inBucket )
                    {
                        var alreadyKnown = contacts_with_email[i].properties.has_potential_rock_match;
                        if ( alreadyKnown != "True" )
                        {
                            //We don't have an exact match but we have guesses, so update Hubspot to reflect that.
                            var bucket_prop = props.FirstOrDefault( p => p.label == "Rock Custom Has Potential Rock Match" );
                            var properties = new List<HubspotPropertyUpdate>() { new HubspotPropertyUpdate() { property = bucket_prop.name, value = "True" } };
                            var url = $"https://api.hubapi.com/crm/v3/objects/contacts/{contacts_with_email[i].id}";
                            MakeRequest( current_id, url, properties, 0 );
                        }
                        //If it is already known, do nothing
                    }
                }
            }
            byte[] sheetbytes = excel.GetAsByteArray();
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Content\\Potential_Matches.xlsx";
            System.IO.File.WriteAllBytes( path, sheetbytes );
        }

        private void MakeRequest( int current_id, string url, List<HubspotPropertyUpdate> properties, int attempt )
        {
            //Update the Hubspot Contact
            try
            {
                var client = new RestClient( url );
                client.Timeout = -1;
                var request = new RestRequest( Method.PATCH );
                request.AddHeader( "accept", "application/json" );
                request.AddHeader( "content-type", "application/json" );
                request.AddHeader( "Authorization", $"Bearer {key}" );
                request.AddParameter( "application/json", $"{{\"properties\": {{ {String.Join( ",", properties.Select( p => $"\"{p.property}\": \"{p.value}\"" ) )} }} }}", ParameterType.RequestBody );
                IRestResponse response = client.Execute( request );
                if ( response.StatusCode != HttpStatusCode.OK )
                {
                    throw new Exception( response.Content );
                }
            }
            catch ( Exception e )
            {
                var json = $"{{\"properties\": {JsonConvert.SerializeObject( properties )} }}";
                ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{e}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Exception from Request:{Environment.NewLine}{e.Message}{Environment.NewLine}Request:{Environment.NewLine}{json}{Environment.NewLine}" ) );
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
            contacts.AddRange( contactResults.results );
            if ( contactResults.paging != null && contactResults.paging.next != null && !String.IsNullOrEmpty( contactResults.paging.next.link ) && request_count < 500 )
            {
                GetContacts( contactResults.paging.next.link );
            }
        }

        private ExcelWorksheet ColorCell( ExcelWorksheet worksheet, int row, int col )
        {
            //Color the Matching Data Green 
            Color c = System.Drawing.ColorTranslator.FromHtml( "#9CD8BC" );
            worksheet.Cells[row, col].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells[row, col].Style.Fill.BackgroundColor.SetColor( c );
            return worksheet;
        }

        private ExcelWorksheet SaveData( ExcelWorksheet worksheet, int row, Person person, HSContactResult contact )
        {
            //Add FirstNames
            worksheet.Cells[row, 1].Value = contact.properties.firstname;
            worksheet.Cells[row, 2].Value = person.NickName;
            if ( person.NickName != person.FirstName )
            {
                worksheet.Cells[row, 2].Value += " (" + person.FirstName + ")";
            }
            //Color cells if they match
            if ( CustomEquals( contact.properties.firstname, person.FirstName ) || CustomEquals( contact.properties.firstname, person.NickName ) )
            {
                worksheet = ColorCell( worksheet, row, 1 );
                worksheet = ColorCell( worksheet, row, 2 );
            }

            //Add LastNames
            worksheet.Cells[row, 3].Value = contact.properties.lastname;
            worksheet.Cells[row, 4].Value = person.LastName;
            //Color cells if they match 
            if ( CustomEquals( contact.properties.lastname, person.LastName ) )
            {
                worksheet = ColorCell( worksheet, row, 3 );
                worksheet = ColorCell( worksheet, row, 4 );
            }

            //Add Emails
            worksheet.Cells[row, 5].Value = contact.properties.email;
            worksheet.Cells[row, 6].Value = person.Email;
            //Color cells if they match
            if ( CustomEquals( contact.properties.email, person.Email ) )
            {
                worksheet = ColorCell( worksheet, row, 5 );
                worksheet = ColorCell( worksheet, row, 6 );
            }

            //Add Phone Numbers
            var num = person.PhoneNumbers.FirstOrDefault( pn => pn.Number == contact.properties.phone );
            worksheet.Cells[row, 7].Value = contact.properties.phone;
            worksheet.Cells[row, 8].Value = num != null ? num.Number : "No Matching Number";
            //Color cells if they match
            if ( num != null && CustomEquals( contact.properties.phone, num.Number ) )
            {
                worksheet = ColorCell( worksheet, row, 7 );
                worksheet = ColorCell( worksheet, row, 8 );
            }

            //Add Connection Statuses
            worksheet.Cells[row, 9].Value = contact.properties.which_best_describes_your_involvement_with_the_crossing_;
            worksheet.Cells[row, 10].Value = person.ConnectionStatusValue;

            //Add links
            worksheet.Cells[row, 11].Value = "https://app.hubspot.com/contacts/6480645/contact/" + contact.id;
            worksheet.Cells[row, 12].Value = "https://rock.thecrossingchurch.com/Perosn/" + person.Id;

            //Add Created Dates
            if ( !String.IsNullOrEmpty( contact.properties.createdate ) )
            {
                DateTime hubspotVal;
                if ( DateTime.TryParse( contact.properties.createdate, out hubspotVal ) )
                {
                    worksheet.Cells[row, 13].Value = hubspotVal.ToString( "MM/dd/yyyy" );
                }
            }
            worksheet.Cells[row, 14].Value = person.CreatedDateTime.Value.ToString( "MM/dd/yyyy" );

            //Add Modified Dates
            if ( !String.IsNullOrEmpty( contact.properties.lastmodifieddate ) )
            {
                DateTime hubspotVal;
                if ( DateTime.TryParse( contact.properties.lastmodifieddate, out hubspotVal ) )
                {
                    worksheet.Cells[row, 15].Value = hubspotVal.ToString( "MM/dd/yyyy" );
                }
            }
            worksheet.Cells[row, 16].Value = person.ModifiedDateTime.Value.ToString( "MM/dd/yyyy" );

            //Add Rock Id
            worksheet.Cells[row, 17].Value = person.Id;


            return worksheet;
        }

        private bool CustomEquals( string a, string b )
        {
            if ( !String.IsNullOrEmpty( a ) && !String.IsNullOrEmpty( b ) )
            {
                return a.ToLower() == b.ToLower();
            }
            return false;
        }

    }

    public class HubspotProperty
    {
        public string name { get; set; }
        public string label { get; set; }
        public string fieldType { get; set; }
        public string groupName { get; set; }
    }

    public class HubspotPropertyUpdate
    {
        public string property { get; set; }
        public string value { get; set; }
    }

    public class HSContactProperties
    {
        public string createdate { get; set; }
        public string email { get; set; }
        public string firstname { get; set; }
        public string has_potential_rock_match { get; set; }
        public string hs_all_assigned_business_unit_ids { get; set; }
        public string lastname { get; set; }
        public string lastmodifieddate { get; set; }
        private string _phone { get; set; }
        public string phone
        {
            get
            {
                return !String.IsNullOrEmpty( _phone ) ? _phone.Replace( " ", "" ).Replace( "(", "" ).Replace( ")", "" ).Replace( "-", "" ) : "";
            }
            set
            {
                _phone = value;
            }
        }
        public string rock_id { get; set; }
        public string which_best_describes_your_involvement_with_the_crossing_ { get; set; }
    }
    public class HSContactResult
    {
        public string id { get; set; }
        public HSContactProperties properties { get; set; }
        public string archived { get; set; }
    }
    public class HSResultPaging
    {
        public HSResultPagingNext next { get; set; }
    }
    public class HSResultPagingNext
    {
        public string link { get; set; }
    }
    public class HSContactQueryResult
    {
        public List<HSContactResult> results { get; set; }
        public HSResultPaging paging { get; set; }
    }
    public class HSPropertyQueryResult
    {
        public List<HubspotProperty> results { get; set; }
    }
}
