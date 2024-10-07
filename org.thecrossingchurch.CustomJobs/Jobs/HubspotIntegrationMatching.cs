using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Quartz;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Newtonsoft.Json;
using OfficeOpenXml;
using System.Drawing;
using OfficeOpenXml.Style;
using RestSharp;
using Rock.Security;
using RestSharp.Extensions;
using System.ComponentModel;
using Rock.Jobs;
using System.Net;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

namespace org.crossingchurch.HubspotIntegration.Jobs
{
    /// <summary>
    /// Job to supply hubspot contacts with a rock_id to the pull related information.
    /// </summary>
    [DisplayName( "Hubspot Integration: Match Records" )]
    [Description( "This job only supplies Hubspot contacts with a Rock ID and adds potential matches to an excel for further investigation." )]
    [DisallowConcurrentExecution]

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
    [TextField( "HubSpot Properties",
        Description = "If properties outside of Id, Name, Email, Phone, Created Date, and Last Modified Date are required add them as a comma seperated list here.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKey.AdditionalHubSpotProps,
        Category = "General Settings",
        Order = 3
    )]
    [TextField( "HubSpot Potential Rock Match Property Name",
        Description = "The name of the property in your HubSpot evnvironment that will be updated by this job if there is a potential but not confirmed Rock match, if left blank no update to HubSpot will occurr.",
        IsRequired = false,
        DefaultValue = "has_potential_rock_match",
        Key = AttributeKey.PotentialMatchProp,
        Category = "General Settings",
        Order = 4
    )]
    [TextField( "Potential Matches File Name",
        Description = "Name of the file for this job to list potential matches for cleaning",
        IsRequired = true,
        DefaultValue = "Potential_Matches",
        Key = AttributeKey.PotentialMatchFile,
        Category = "General Settings",
        Order = 5
    )]
    [TextField( "HubSpot Account Id",
        Description = "The id from your HubSpot url when you access your dashboard. Used to create links to HubSpot records for potential matches.",
        IsRequired = true,
        Key = AttributeKey.HubSpotAccountId,
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
    [IntegerField( "HubSpot Record ID",
        Description = "If you wish to test by syncing data to a specific HubSpot record, enter the ID here. URL for POST/PATCH will be updated to use this ID.",
        IsRequired = false,
        DefaultValue = "",
        Key = AttributeKey.HubSpotRecordId,
        Category = "Testing Environment Configuration",
        Order = 2
    )]
    [IntegerField( "HubSpot Contact Limit",
        Description = "If you want to provide a hard limit for the number of contacts that should be pulled from HubSpot for processing provide it here.",
        IsRequired = false,
        Key = AttributeKey.ContactLimit,
        Category = "Testing Environment Configuration",
        Order = 3
    )]
    #endregion
    public class HubspotIntegrationMatching : RockJob
    {
        private string key { get; set; }
        private List<HSContactResult> contacts { get; set; }
        private List<string> additionalProperties { get; set; }
        private int request_count { get; set; }
        private string businessUnit { get; set; }
        private string rockUrl { get; set; }
        private string hubspotUrl { get; set; }
        private ConcurrentBag<PotentialMatch> potentialMatches { get; set; }

        private bool updatesAreDisabled { get; set; }
        private string syncType { get; set; }
        private string hubSpotTestContactId { get; set; }
        private int? contactLimit { get; set; }

        #region Attribute Keys
        private class AttributeKey
        {
            public const string APIKeyAttribute = "AttributeKey";
            public const string BusinessUnit = "BusinessUnit";
            public const string AdditionalHubSpotProps = "AdditionalHubSpotProps";
            public const string HubSpotAccountId = "HubSpotAccountId";
            public const string PotentialMatchProp = "PotentialMatchProp";
            public const string PotentialMatchFile = "PotentialMatchFile";
            public const string ThreadCount = "ThreadCount";

            public const string DisableUpdates = "DisableUpdates";
            public const string SyncType = "SyncType";
            public const string ContactLimit = "ContactLimit";
            public const string HubSpotRecordId = "HubSpotRecordId";
        }
        #endregion Attribute Keys

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public HubspotIntegrationMatching()
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

            //Pull HubSpot Authorization from Global Attribute so we don't have to update it everywhere when we rotate the key
            string attrKey = GetAttributeValue( AttributeKey.APIKeyAttribute );
            key = Encryption.DecryptString( GlobalAttributesCache.Get().GetValue( attrKey ) ); //global variable should be an encrypted field

            //Business Unit is not requried, but if provided will filter to contacts in that business unit
            businessUnit = GetAttributeValue( AttributeKey.BusinessUnit );

            //Urls for Excel file to have links to the records for simpler validation
            hubspotUrl = $"https://app.hubspot.com/contacts/{GetAttributeValue( AttributeKey.HubSpotAccountId )}/contact/";
            rockUrl = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" );
            rockUrl = rockUrl.EndsWith( "/" ) ? rockUrl + "Person/" : rockUrl + "/Person/";

            string potentialRockMatchProp = GetAttributeValue( AttributeKey.PotentialMatchProp );

            //Build the distinct list of properties we want included with our Contact records from HubSpot
            List<string> requestedProperties = new List<string>() { "rock_id", "firstname", "lastname", "email", "phone", "createdate", "lastmodifieddate" };
            additionalProperties = GetAttributeValue( AttributeKey.AdditionalHubSpotProps ).Split( ',' ).ToList();
            requestedProperties.AddRange( additionalProperties );
            if ( !String.IsNullOrEmpty( potentialRockMatchProp ) )
            {
                requestedProperties.Add( potentialRockMatchProp );
            }
            if ( !String.IsNullOrEmpty( businessUnit ) )
            {
                requestedProperties.Add( "hs_all_assigned_business_unit_ids" );
            }
            requestedProperties = requestedProperties.Select( p => p.ToLower() ).Where( p => !String.IsNullOrEmpty( p ) ).Distinct().ToList();

            //Get Hubspot Properties, we don't care what group they are in we are just looking for any that have been requested in the job config
            var propClient = new RestClient( "https://api.hubapi.com/crm/v3/properties/contacts?properties=name,label,createdUserId,groupName,options,fieldType" );
            propClient.Timeout = -1;
            var propRequest = new RestRequest( Method.GET );
            propRequest.AddHeader( "Authorization", $"Bearer {key}" );
            IRestResponse propResponse = propClient.Execute( propRequest );
            List<HubspotProperty> props = new List<HubspotProperty>();
            var propsQry = JsonConvert.DeserializeObject<HSPropertyQueryResult>( propResponse.Content );
            props = propsQry.results;

            //Set up Static Report of Potential Matches
            ExcelPackage excel = new ExcelPackage();
            excel.Workbook.Properties.Title = "Potential Matches";
            excel.Workbook.Properties.Author = "Rock";
            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( "Potential Matches" );
            worksheet.PrinterSettings.LeftMargin = .5m;
            worksheet.PrinterSettings.RightMargin = .5m;
            worksheet.PrinterSettings.TopMargin = .5m;
            worksheet.PrinterSettings.BottomMargin = .5m;
            var headers = new List<string> { "HubSpot FirstName", "Rock FirstName", "HubSpot LastName", "Rock LastName", "HubSpot Email", "Rock Email", "HubSpot Phone", "Rock Phone", "Rock Connection Status", "HubSpot Link", "Rock Link", "HubSpot CreatedDate", "Rock Created Date", "HubSpot Modified Date", "Rock Modified Date", "Rock ID" };
            //If someone has requested additional values we will assume they want them in the excel sheet
            for ( int i = 0; i < additionalProperties.Count; i++ )
            {
                var currentProp = props.FirstOrDefault( p => p.name == additionalProperties[i] );
                if ( currentProp != null )
                {
                    headers.Add( currentProp.label );
                }
            }

            //Write headers to excel sheet
            var col = 1;
            var row = 2;
            foreach ( var header in headers )
            {
                worksheet.Cells[1, col].Value = header;
                col++;
            }

            //Get List of all contacts from Hubspot
            contacts = new List<HSContactResult>();
            request_count = 0;
            string contactsUrl = "https://api.hubapi.com/crm/v3/objects/contacts?limit=100&properties=" + String.Join( ",", requestedProperties ) + "&archived=false";
            GetContacts( contactsUrl );

            int numThreads = GetAttributeValue( AttributeKey.ThreadCount ).AsInteger();
            if ( numThreads <= 0 )
            {
                numThreads = 50; //Default number of threads if not configured
            }

            //Create the thread-safe collecion of potential matches we will eventually write to the excel report
            potentialMatches = new ConcurrentBag<PotentialMatch>();

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

                var hubspotProps = props;
                Task task = new Task( () =>
                {
                    ProcessList( subset, potentialRockMatchProp, hubspotProps );
                } );
                taskBag.Add( task );
                task.Start();
            }
            Task.WaitAll( taskBag.ToArray() );

            //After all threads have executed, write data to our file
            foreach ( var match in potentialMatches )
            {
                SaveData( worksheet, row, match.rockPerson, match.hubspotContact, props );
                row++;
            }

            try
            {
                byte[] sheetbytes = excel.GetAsByteArray();
                string path = AppDomain.CurrentDomain.BaseDirectory + "\\Content\\" + GetAttributeValue( AttributeKey.PotentialMatchFile ) + ".xlsx";
                System.IO.File.WriteAllBytes( path, sheetbytes );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( new Exception( "HubSpot Matching Error: Failed to write potential matches to file.", ex ) );
            }
        }

        private void ProcessList( List<HSContactResult> contacts, string potentialRockMatchProp, List<HubspotProperty> props )
        {
            using ( RockContext _context = new RockContext() )
            {
                PersonService person_svc = new PersonService( new RockContext() );
                HistoryService history_svc = new HistoryService( _context );
                string current_id = "";

                #region DB Queries
                #endregion

                //Foreach contact with an email, look for a 1:1 match in Rock by email and schedule it's update 
                for ( var i = 0; i < contacts.Count(); i++ )
                {
                    current_id = contacts[i].id;
                    try
                    {
                        //First Check if they have a rock Id in their hubspot data to use
                        Person person = null;
                        bool hasMultiEmail = false;
                        List<int> matchingIds = FindPersonIds( contacts[i] );
                        if ( matchingIds.Count > 1 )
                        {
                            hasMultiEmail = true;
                        }
                        if ( matchingIds.Count == 1 )
                        {
                            person = person_svc.Get( matchingIds.First() );
                        }

                        //Atempt to match 1:1 based on email history making sure we exclude emails with multiple matches in the person table
                        if ( person == null && !hasMultiEmail )
                        {
                            string email = contacts[i].email;
                            var matches = history_svc.Queryable().Where( hist => hist.EntityTypeId == 15 && hist.ValueName == "Email" && hist.NewValue.ToLower() == email ).Select( hist => hist.EntityId ).Distinct();
                            if ( matches.Count() == 1 )
                            {
                                //If 1:1 Email match and Hubspot has no other info, make it a match
                                if ( String.IsNullOrEmpty( contacts[i].firstname ) && String.IsNullOrEmpty( contacts[i].lastname ) )
                                {
                                    person = person_svc.Get( matches.First() );
                                }
                            }
                        }

                        bool inBucket = false;
                        //Try to mark people that are potential matches, only people who can at least match email or phone and one other thing
                        if ( person == null )
                        {
                            var contact = contacts[i];
                            //Matches phone number and one other piece of info
                            if ( !String.IsNullOrEmpty( contact.phone ) )
                            {
                                var phone_matches = person_svc.Queryable().Where( p => p.PhoneNumbers.Any( ph => contact.phone.Contains( ph.Number ) || ph.Number.Contains( contact.phone ) ) ).ToList();
                                if ( phone_matches.Count() > 0 )
                                {
                                    phone_matches = phone_matches.Where( p => CustomEquals( p.FirstName, contact.firstname ) || CustomEquals( p.NickName, contact.firstname ) || CustomEquals( p.Email, contact.email ) || CustomEquals( p.LastName, contact.lastname ) ).ToList();
                                    for ( var j = 0; j < phone_matches.Count(); j++ )
                                    {
                                        //Add to thread safe collection of items that will be written to excel sheet....
                                        potentialMatches.Add( new PotentialMatch() { hubspotContact = contact, rockPerson = phone_matches[j] } );
                                        inBucket = true;
                                    }
                                }
                            }
                            //Matches email and one other piece of info
                            var email_matches = person_svc.Queryable().ToList().Where( p =>
                            {
                                return CustomEquals( p.Email, contact.email );
                            } ).ToList();
                            if ( email_matches.Count() > 0 )
                            {
                                email_matches = email_matches.Where( p => CustomEquals( p.FirstName, contact.firstname ) || CustomEquals( p.NickName, contact.firstname ) || ( !String.IsNullOrEmpty( contact.phone ) && p.PhoneNumbers.Select( pn => pn.Number ).Contains( contact.phone ) ) || CustomEquals( p.LastName, contact.lastname ) ).ToList();
                                for ( var j = 0; j < email_matches.Count(); j++ )
                                {
                                    //Add to thread safe collection of items that will be written to excel sheet....
                                    potentialMatches.Add( new PotentialMatch() { hubspotContact = contact, rockPerson = email_matches[j] } );
                                    inBucket = true;
                                }
                            }
                        }

                        //Schedule HubSpot update if 1:1 match
                        if ( person != null )
                        {
                            var properties = new List<HubspotPropertyUpdate>() { new HubspotPropertyUpdate() { property = "rock_id", value = person.Id.ToString() } };

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
                            string url = $"https://api.hubapi.com/crm/v3/objects/contacts/{contacts[i].id}";
                            MakeRequest( current_id, url, properties, 0 );
                        }
                        else
                        {
                            if ( inBucket )
                            {
                                string alreadyKnown = String.Empty;
                                contacts[i].properties.TryGetValue( potentialRockMatchProp, out alreadyKnown );
                                if ( alreadyKnown != "True" )
                                {
                                    //We don't have an exact match but we have guesses, so update Hubspot to reflect that.
                                    var bucket_prop = props.FirstOrDefault( p => p.name == potentialRockMatchProp );
                                    List<HubspotPropertyUpdate> properties = new List<HubspotPropertyUpdate>() { new HubspotPropertyUpdate() { property = bucket_prop.name, value = "True" } };
                                    string url = $"https://api.hubapi.com/crm/v3/objects/contacts/{contacts[i].id}";
                                    MakeRequest( current_id, url, properties, 0 );
                                }
                                //If it is already known, do nothing
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( new Exception( $"HubSpot Matching Error: Unable to process matches for {current_id}", ex ) );
                    }
                }
            }
        }
        private void MakeRequest( string current_id, string url, List<HubspotPropertyUpdate> properties, int attempt )
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
                    //Handle API Rate Limit
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
                ExceptionLogService.LogException( new Exception( $"Hubspot Matching Error: API Exception{Environment.NewLine}Current Id: {current_id}{Environment.NewLine}Request:{Environment.NewLine}{json}", e ) );
            }
        }

        private void WriteToLog( string message )
        {
            string logFile = System.Web.Hosting.HostingEnvironment.MapPath( $"~/App_Data/Logs/HubSpotMatchLog_{Task.CurrentId}.txt" );
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
            contactRequest.AddHeader( "accept", "application/json" );
            contactRequest.AddHeader( "content-type", "application/json" );
            IRestResponse contactResponse = contactClient.Execute( contactRequest );
            var contactResults = JsonConvert.DeserializeObject<HSContactQueryResult>( contactResponse.Content );
            //Contacts with emails that do not already have Rock IDs in the desired business unit (if applicaable) 
            contacts.AddRange( contactResults.results.Where( c =>
                ( c.properties["rock_id"] == null || c.properties["rock_id"] == "" || c.properties["rock_id"] == "0" ) &&
                ( ( c.email != null && c.email != "" ) || ( c.phone != null && c.phone != "" ) ) &&
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

            //Eventually we will have to change this check, but for right now we add the Request Count < 5000 so we don't end up in an infinite loop scenario
            if ( contactResults.paging != null && contactResults.paging.next != null && !String.IsNullOrEmpty( contactResults.paging.next.link ) && request_count < 5000 )
            {
                GetContacts( contactResults.paging.next.link );
            }
        }

        private List<int> FindPersonIds( HSContactResult contact )
        {
            using ( RockContext context = new RockContext() )
            {
                SqlParameter[] sqlParams = new SqlParameter[] {
                    new SqlParameter( "@rock_id", contact.rock_id > 0 ? contact.rock_id.ToString() : "" ),
                    new SqlParameter( "@first_name", contact.firstname.HasValue() ? contact.firstname : "" ),
                    new SqlParameter( "@last_name", contact.lastname.HasValue() ? contact.lastname : "" ),
                    new SqlParameter( "@email", contact.email.HasValue() ? contact.email : "" ),
                    new SqlParameter( "@mobile_number", contact.phone.HasValue() ? contact.phone : "" ),
                };
                var query = context.Database.SqlQuery<int>( $@"
DECLARE @matches int = (SELECT COUNT(*) FROM Person WHERE Email = @email);

SELECT DISTINCT Person.Id
FROM Person
         LEFT OUTER JOIN PhoneNumber ON Person.Id = PhoneNumber.PersonId
WHERE ((@email IS NOT NULL AND @email != '') AND
       (Email = @email AND
        (((@first_name IS NULL OR @first_name = '') AND (@last_name IS NULL OR @last_name = '') AND @matches = 1) OR
         ((@first_name IS NOT NULL AND @first_name != '' AND
           (FirstName = @first_name OR NickName = @first_name)) OR
          (@last_name IS NOT NULL AND @last_name != '' AND LastName = @last_name) OR
          (@mobile_number IS NOT NULL AND @mobile_number != '' AND
           (Number = @mobile_number OR @mobile_number LIKE '%' + Number ))))))
", sqlParams ).ToList();
                return query;
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

        private ExcelWorksheet SaveData( ExcelWorksheet worksheet, int row, Person person, HSContactResult contact, List<HubspotProperty> props )
        {
            //Add FirstNames
            worksheet.Cells[row, 1].Value = contact.firstname;
            worksheet.Cells[row, 2].Value = person.NickName;
            if ( person.NickName != person.FirstName )
            {
                worksheet.Cells[row, 2].Value += " (" + person.FirstName + ")";
            }
            //Color cells if they match
            if ( CustomEquals( contact.firstname, person.FirstName ) || CustomEquals( contact.firstname, person.NickName ) )
            {
                worksheet = ColorCell( worksheet, row, 1 );
                worksheet = ColorCell( worksheet, row, 2 );
            }

            //Add LastNames
            worksheet.Cells[row, 3].Value = contact.lastname;
            worksheet.Cells[row, 4].Value = person.LastName;
            //Color cells if they match 
            if ( CustomEquals( contact.lastname, person.LastName ) )
            {
                worksheet = ColorCell( worksheet, row, 3 );
                worksheet = ColorCell( worksheet, row, 4 );
            }

            //Add Emails
            worksheet.Cells[row, 5].Value = contact.email;
            worksheet.Cells[row, 6].Value = person.Email;
            //Color cells if they match
            if ( CustomEquals( contact.email, person.Email ) )
            {
                worksheet = ColorCell( worksheet, row, 5 );
                worksheet = ColorCell( worksheet, row, 6 );
            }

            //Add Phone Numbers
            var num = person.PhoneNumbers.FirstOrDefault( pn => pn.Number == contact.phone );
            worksheet.Cells[row, 7].Value = contact.phone;
            worksheet.Cells[row, 8].Value = num != null ? num.Number : "No Matching Number";
            //Color cells if they match
            if ( num != null && CustomEquals( contact.phone, num.Number ) )
            {
                worksheet = ColorCell( worksheet, row, 7 );
                worksheet = ColorCell( worksheet, row, 8 );
            }

            //Add Connection Status
            worksheet.Cells[row, 9].Value = person.ConnectionStatusValue;

            //Add links
            worksheet.Cells[row, 10].Value = hubspotUrl + contact.id;
            worksheet.Cells[row, 11].Value = rockUrl + person.Id;

            //Add Created Dates
            if ( !String.IsNullOrEmpty( contact.properties["createdate"] ) )
            {
                DateTime hubspotVal;
                if ( DateTime.TryParse( contact.properties["createdate"], out hubspotVal ) )
                {
                    worksheet.Cells[row, 12].Value = hubspotVal.ToString( "MM/dd/yyyy" );
                }
            }
            worksheet.Cells[row, 13].Value = person.CreatedDateTime.Value.ToString( "MM/dd/yyyy" );

            //Add Modified Dates
            if ( !String.IsNullOrEmpty( contact.properties["lastmodifieddate"] ) )
            {
                DateTime hubspotVal;
                if ( DateTime.TryParse( contact.properties["lastmodifieddate"], out hubspotVal ) )
                {
                    worksheet.Cells[row, 14].Value = hubspotVal.ToString( "MM/dd/yyyy" );
                }
            }
            worksheet.Cells[row, 15].Value = person.ModifiedDateTime.Value.ToString( "MM/dd/yyyy" );

            //Add Rock Id
            worksheet.Cells[row, 16].Value = person.Id;

            //Add Custom Requested HubSpot Properties
            for ( int i = 0; i < additionalProperties.Count; i++ )
            {
                var currentProp = props.FirstOrDefault( p => p.name == additionalProperties[i] );
                if ( currentProp != null )
                {
                    worksheet.Cells[row, 17 + i].Value = contact.properties[additionalProperties[i]];
                }
            }

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
}
