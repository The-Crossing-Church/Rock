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

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( "Hubspot API Key", "API Key for Hubspot", true, "", "", 0 )]
    [DisallowConcurrentExecution]
    public class HubspotIntegration : IJob
    {

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

            string key = dataMap.GetString( "HubspotAPIKey" );

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


            //New instance of api 
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

            //Foreach contact with an email, look for a 1:1 match in Rock by email and schedule it's update 
            for ( var i = 0; i < contacts_with_email.Count(); i++ )
            {
                //First Check if they have a rock Id in their hubspot data to use
                Person person = null;
                if ( !String.IsNullOrEmpty( contacts_with_email[i].rock_id ) )
                {
                    person = personService.Get( Int32.Parse( contacts_with_email[i].rock_id ) );
                }

                //If there is not a value for Rock Id, proceed to run the query with HubSpot data
                if ( person == null )
                {
                    var query = new PersonService.PersonMatchQuery( contacts_with_email[i].FirstName, contacts_with_email[i].LastName, contacts_with_email[i].Email, contacts_with_email[i].Phone );
                    person = personService.FindPerson( query, false );
                }

                //If person is still null, attempt to use the Rock FirstName, Rock LastName, Rock Email for the query
                if ( person == null && !String.IsNullOrEmpty( contacts_with_email[i].rock_firstname ) && !String.IsNullOrEmpty( contacts_with_email[i].rock_lastname ) && ( !String.IsNullOrEmpty( contacts_with_email[i].rock_email ) || !String.IsNullOrEmpty( contacts_with_email[i].Email ) ) )
                {
                    PersonService.PersonMatchQuery query;
                    if ( !String.IsNullOrEmpty( contacts_with_email[i].rock_email ) )
                    {
                        query = new PersonService.PersonMatchQuery( contacts_with_email[i].rock_firstname, contacts_with_email[i].rock_lastname, contacts_with_email[i].rock_email, "" );
                    }
                    else
                    {
                        query = new PersonService.PersonMatchQuery( contacts_with_email[i].rock_firstname, contacts_with_email[i].rock_lastname, contacts_with_email[i].Email, "" );
                    }
                    person = personService.FindPerson( query, false );
                }

                //Attempt to match on email and one other piece of info if we are still null
                if ( person == null )
                {
                    string email = contacts_with_email[i].Email.ToLower();
                    var matches = personService.Queryable().Where( p => p.Email.ToLower() == email ).ToList();
                    if ( matches.Count() == 1 )
                    {
                        //1:1 Email match so we want to check other information, start by checking for a name match 
                        if ( CustomEquals( matches.First().NickName, contacts_with_email[i].FirstName ) ||
                            CustomEquals( matches.First().FirstName, contacts_with_email[i].FirstName ) ||
                            CustomEquals( matches.First().LastName, contacts_with_email[i].LastName ) )
                        {
                            person = matches.First();
                        }
                        else if ( !String.IsNullOrEmpty( contacts_with_email[i].Phone ) )
                        {
                            //Try looking for a phone number
                            if ( matches.First().PhoneNumbers.Select( p => p.Number ).Contains( contacts_with_email[i].Phone ) )
                            {
                                person = matches.First();
                            }
                        }
                    }
                }

                //Try to mark people that are potential matches
                if ( person == null )
                {
                    var contact = contacts_with_email[i];
                    //Try based on firstname
                    var fname_matches = personService.Queryable().ToList().Where( p =>
                    {
                        return CustomEquals( p.FirstName, contact.FirstName ) || CustomEquals( p.NickName, contact.FirstName );
                    } ).ToList();
                    if ( fname_matches.Count() > 0 )
                    {
                        fname_matches = fname_matches.Where( p => CustomEquals( p.LastName, contact.LastName ) || CustomEquals( p.Email, contact.Email ) || ( !String.IsNullOrEmpty( contact.Phone ) && p.PhoneNumbers.Select( pn => pn.Number ).Contains( contact.Phone ) ) ).ToList();
                        for ( var j = 0; j < fname_matches.Count(); j++ )
                        {
                            //Save this information in the excel sheet....
                            SaveData( worksheet, row, fname_matches[j], contact );
                            row++;
                        }
                    }
                    else
                    {
                        //Try based on last name
                        var lname_matches = personService.Queryable().ToList().Where( p =>
                        {
                            return CustomEquals( p.LastName, contact.LastName );
                        } ).ToList();
                        if ( lname_matches.Count() > 0 )
                        {
                            lname_matches = lname_matches.Where( p => CustomEquals( p.FirstName, contact.FirstName ) || CustomEquals( p.NickName, contact.FirstName ) || CustomEquals( p.Email, contact.Email ) || ( !String.IsNullOrEmpty( contact.Phone ) && p.PhoneNumbers.Select( pn => pn.Number ).Contains( contact.Phone ) ) ).ToList();
                            for ( var j = 0; j < lname_matches.Count(); j++ )
                            {
                                //Save this information in the excel sheet....
                                SaveData( worksheet, row, lname_matches[j], contact );
                                row++;
                            }
                        }
                        else
                        {
                            //Try based on phone number
                            if ( !String.IsNullOrEmpty( contact.Phone ) )
                            {
                                var phone_matches = personService.Queryable().Where( p => p.PhoneNumbers.Select( pn => pn.Number ).Contains( contact.Phone ) ).ToList();
                                if ( phone_matches.Count() > 0 )
                                {
                                    phone_matches = phone_matches.Where( p => CustomEquals( p.FirstName, contact.FirstName ) || CustomEquals( p.NickName, contact.FirstName ) || CustomEquals( p.Email, contact.Email ) || CustomEquals( p.LastName, contact.LastName ) ).ToList();
                                    for ( var j = 0; j < phone_matches.Count(); j++ )
                                    {
                                        //Save this information in the excel sheet....
                                        SaveData( worksheet, row, phone_matches[j], contact );
                                        row++;
                                    }
                                }
                            }
                        }
                    }
                }

                //For Testing
                //if ( contacts_with_email[i].Email != "jimbeatyjr@gmail.com" )
                //{
                //    person = null;
                //}

                //Schedule HubSpot update if 1:1 match
                if ( person != null )
                {
                    try
                    {
                        current_id = person.Id;
                        //Get the Attributes for that Person 
                        person.LoadAttributes();
                        //Limit to the non Field props we want
                        var attrs = props.Where( p => !p.label.Contains( "Rock " ) ).ToList();
                        //Build the POST request and schedule in the db 10 at a time 
                        var url = $"https://api.hubapi.com/contacts/v1/contact/vid/{contacts_with_email[i].Id}/profile?hapikey={key}";
                        var properties = new List<HubspotPropertyUpdate>();
                        //Add each Rock prop to the list with the Hubspot name
                        for ( var j = 0; j < attrs.Count(); j++ )
                        {
                            AttributeCache current_prop = null;
                            try
                            {
                                current_prop = person.Attributes[attrs[j].label];
                            }
                            catch
                            {
                                current_prop = null; 
                            }
                            //If the attribute is in our list of props from Hubspot
                            if ( current_prop != null )
                            {
                                if ( current_prop.FieldType.Name == "Date" || current_prop.FieldType.Name == "Date Time" )
                                {
                                    //Get Epoc miliseconds 
                                    DateTime tryDate;
                                    if ( DateTime.TryParse( person.GetAttributeValue( attrs[j].label ), out tryDate ) )
                                    {
                                        var d = tryDate.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds * 1000;
                                        properties.Add( new HubspotPropertyUpdate() { property = attrs[j].name, value = d.ToString() } );
                                    }
                                }
                                else
                                {
                                    properties.Add( new HubspotPropertyUpdate() { property = attrs[j].name, value = person.AttributeValues[attrs[j].label].ValueFormatted } );
                                }
                            }
                        }
                        //All properties begining with "Rock " are properties on the Person entity itself 
                        var person_props = props.Where( p => p.label.Contains( "Rock " ) ).ToList();
                        foreach ( PropertyInfo propInfo in person.GetType().GetProperties() )
                        {
                            var current_prop = props.FirstOrDefault( p => p.label == "Rock " + propInfo.Name );
                            if ( current_prop != null )
                            {
                                if ( propInfo.PropertyType.FullName == "Rock.Model.DefinedValue" )
                                {
                                    DefinedValue dv = JsonConvert.DeserializeObject<DefinedValue>( JsonConvert.SerializeObject( propInfo.GetValue( person ) ) );
                                    properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = dv.Value } );
                                }
                                else if ( propInfo.PropertyType.FullName.Contains( "Date" ) )
                                {
                                    //Get Epoc miliseconds 
                                    DateTime tryDate;
                                    if ( DateTime.TryParse( propInfo.GetValue( person ).ToString(), out tryDate ) )
                                    {
                                        var d = tryDate.Subtract( new DateTime( 1970, 1, 1 ) ).TotalSeconds * 1000;
                                        properties.Add( new HubspotPropertyUpdate() { property = current_prop.name, value = d.ToString() } );
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
                            var child_ages_prop = props.FirstOrDefault( p => p.label == "Children's Age Groups" );
                            var children = person.PrimaryFamily.Members.Where( m => m.GroupRole.Name == "Child" ).ToList();
                            var agegroups = "";
                            //Known Relationships
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
                            var memberships = person.Members.Where( m => m.Group != null && ( m.Group.Name.Contains( today.ToString( "yyyy" ) ) ||
                                 m.Group.Name.Contains( $"{today.AddYears( -1 ).ToString( "yyyy" )}-{today.ToString( "yy" )}" ) ) ).ToList();
                            //Where the group name has Fall/Winter/Summer
                            var current_serving = memberships.Where( m => m.Group.Name.ToLower().Contains( term ) ).ToList();
                            //All current groups with the words Small Group
                            var current_sg = memberships.Where( m => m.Group.Name.ToLower().Contains( "small group" ) ).ToList();

                            var serving_prop = props.FirstOrDefault( p => p.label == "Currently Serving" );
                            var sg_props = props.Where( p => p.label.Contains( "Small Group" ) ).ToList();
                            //set the serving prop
                            if ( current_serving.Count() > 0 )
                            {
                                properties.Add( new HubspotPropertyUpdate() { property = serving_prop.name, value = "true" } );
                            }
                            else
                            {
                                properties.Add( new HubspotPropertyUpdate() { property = serving_prop.name, value = "false" } );
                            }
                            //figure out if they attend small group
                            if ( current_sg.Count() > 0 )
                            {
                                if ( current_sg.Count() > 1 && current_sg.Where( sg => !sg.GroupRole.IsLeader ).Count() > 0 )
                                { //See if we can get this to one small group hopefully
                                    current_sg = current_sg.Where( sg => !sg.GroupRole.IsLeader ).ToList();
                                }
                                foreach ( var sg in current_sg )
                                {
                                    var small_group = sg_props.FirstOrDefault( p => p.label == "Currently in Adult Small Group" );
                                    if ( sg.Group.ParentGroup.Name.ToLower().Contains( "veritas" ) )
                                    {
                                        small_group = sg_props.FirstOrDefault( p => p.label == "Currently in Veritas Small Group" );
                                    }
                                    else if ( sg.Group.ParentGroup.Name.ToLower().Contains( "twenties" ) )
                                    {
                                        small_group = sg_props.FirstOrDefault( p => p.label == "Currently in Twenties Small Group" );
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
                            //List of all groups person is a member of (fingers crossed it's not too long) 
                            var group_prop = props.FirstOrDefault( p => p.label == "Group Membership" );
                            var grps = "";
                            for ( var idx = 0; idx < person.Members.Count(); idx++ )
                            {
                                grps += person.Members.ToList()[idx].Group != null ? $"{person.Members.ToList()[idx].Group.Name}, " : "";
                            }
                            grps = grps.Substring( 0, grps.Length - 2 );
                            properties.Add( new HubspotPropertyUpdate() { property = group_prop.name, value = grps } );
                            //Figure out if the person is currently involved in connections 
                            //Prod ID for Parent Group: 136298
                            var conn_prop = props.FirstOrDefault( p => p.label == "Currently in Connections" );
                            var groups = new GroupService( new RockContext() ).Queryable().AsNoTracking().Where( g => g.ParentGroupId == 103379 ).ToList(); //test with sg minitry group 
                            var inConnections = FindGroup( groups, person, false );
                            if ( inConnections )
                            {
                                properties.Add( new HubspotPropertyUpdate() { property = conn_prop.name, value = "true" } );
                            }
                            else
                            {
                                properties.Add( new HubspotPropertyUpdate() { property = conn_prop.name, value = "false" } );
                            }
                        }

                        //Update the Hubspot Contact
                        try
                        {
                            var webrequest = WebRequest.Create( url );
                            webrequest.Method = "POST";
                            webrequest.ContentType = "application/json";
                            using ( Stream requestStream = webrequest.GetRequestStream() )
                            {
                                var json = $"{{\"properties\": {JsonConvert.SerializeObject( properties )} }}";
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
                                    ExceptionLogService.LogException( new Exception( $"Hubspot Sync Error{Environment.NewLine}{ex}{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Response:{jsonResponse} " ), context2 );
                                }
                            }
                        }
                        catch ( Exception e )
                        {
                            Console.WriteLine( $"Other: {e.Message}" );
                            HttpContext context2 = HttpContext.Current;
                            ExceptionLogService.LogException( e, context2 );
                        }

                    }
                    catch ( Exception err )
                    {
                        HttpContext context2 = HttpContext.Current;
                        ExceptionLogService.LogException( err, context2 );
                    }
                }
            }
            byte[] sheetbytes = excel.GetAsByteArray();
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Content\\Potential_Matches.xlsx";
            System.IO.File.WriteAllBytes( path, sheetbytes );
        }

        private bool FindGroup( List<Group> groups, Person per, bool subOfYear )
        {
            var today = DateTime.UtcNow;
            var inConnections = false;
            for ( var i = 0; i < groups.Count(); i++ )
            {
                if ( groups[i].Name.Contains( today.ToString( "yyyy" ) ) || groups[i].Name.Contains( $"{today.AddYears( -1 ).ToString( "yyyy" )}-{today.ToString( "yy" )}" ) || subOfYear )
                {
                    if ( groups[i].Members.Count() > 0 )
                    {
                        var exists = groups[i].Members.FirstOrDefault( p => p.PersonId == per.Id );
                        if ( exists != null )
                        {
                            return true;
                        }
                    }
                    else
                    {
                        inConnections = FindGroup( groups[i].Groups.ToList(), per, true );
                    }
                }
                else
                {
                    inConnections = FindGroup( groups[i].Groups.ToList(), per, false );
                }
            }
            return inConnections;
        }

        private void WriteToLog( string message )
        {
            string logFile = System.Web.Hosting.HostingEnvironment.MapPath( "~/App_Data/Logs/HubSpotLog.txt" );

            // Write to the log, but if an ioexception occurs wait a couple seconds and then try again (up to 3 times).
            var maxRetry = 3;
            for ( int retry = 0; retry < maxRetry; retry++ )
            {
                try
                {
                    using ( System.IO.FileStream fs = new System.IO.FileStream( logFile, System.IO.FileMode.Append, System.IO.FileAccess.Write ) )
                    {
                        using ( System.IO.StreamWriter sw = new System.IO.StreamWriter( fs ) )
                        {
                            sw.WriteLine( string.Format( "{0} - {1}", RockDateTime.Now.ToString(), message ) );
                            return;
                        }
                    }
                }
                catch ( System.IO.IOException )
                {
                    if ( retry < maxRetry - 1 )
                    {
                        System.Threading.Tasks.Task.Delay( 2000 ).Wait();
                    }
                }
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

        private ExcelWorksheet SaveData( ExcelWorksheet worksheet, int row, Person person, HubSpotContact contact )
        {
            //Add FirstNames
            worksheet.Cells[row, 1].Value = contact.FirstName;
            worksheet.Cells[row, 2].Value = person.NickName;
            if ( person.NickName != person.FirstName )
            {
                worksheet.Cells[row, 2].Value += " (" + person.FirstName + ")";
            }
            //Color cells if they match
            if ( CustomEquals( contact.FirstName, person.FirstName ) || CustomEquals( contact.FirstName, person.NickName ) )
            {
                worksheet = ColorCell( worksheet, row, 1 );
                worksheet = ColorCell( worksheet, row, 2 );
            }

            //Add LastNames
            worksheet.Cells[row, 3].Value = contact.LastName;
            worksheet.Cells[row, 4].Value = person.LastName;
            //Color cells if they match 
            if ( CustomEquals( contact.LastName, person.LastName ) )
            {
                worksheet = ColorCell( worksheet, row, 3 );
                worksheet = ColorCell( worksheet, row, 4 );
            }

            //Add Emails
            worksheet.Cells[row, 5].Value = contact.Email;
            worksheet.Cells[row, 6].Value = person.Email;
            //Color cells if they match
            if ( CustomEquals( contact.Email, person.Email ) )
            {
                worksheet = ColorCell( worksheet, row, 5 );
                worksheet = ColorCell( worksheet, row, 6 );
            }

            //Add Phone Numbers
            var num = person.PhoneNumbers.FirstOrDefault( pn => pn.Number == contact.Phone );
            worksheet.Cells[row, 7].Value = contact.Phone;
            worksheet.Cells[row, 8].Value = num != null ? num.Number : "No Matching Number";
            //Color cells if they match
            if ( num != null && CustomEquals( contact.Phone, num.Number ) )
            {
                worksheet = ColorCell( worksheet, row, 7 );
                worksheet = ColorCell( worksheet, row, 8 );
            }

            //Add Connection Statuses
            worksheet.Cells[row, 9].Value = contact.which_best_describes_your_involvement_with_the_crossing_;
            worksheet.Cells[row, 10].Value = person.ConnectionStatusValue;

            //Add links
            worksheet.Cells[row, 11].Value = "https://app.hubspot.com/contacts/6480645/contact/" + contact.Id;
            worksheet.Cells[row, 12].Value = "https://rock.thecrossingchurch.com/Perosn/" + person.Id;

            //Add Created Dates
            DateTime epoch = new DateTime( 1970, 1, 1, 0, 0, 0, DateTimeKind.Utc );
            worksheet.Cells[row, 13].Value = epoch.AddMilliseconds( Double.Parse( contact.createdate ) ).ToString( "MM/dd/yyyy" );
            worksheet.Cells[row, 14].Value = person.CreatedDateTime.Value.ToString( "MM/dd/yyyy" );

            //Add Modified Dates
            worksheet.Cells[row, 15].Value = epoch.AddMilliseconds( Double.Parse( contact.lastmodifieddate ) ).ToString( "MM/dd/yyyy" );
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
