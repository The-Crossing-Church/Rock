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
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Store;
using System.Text;
using Rock.Security;
using DDay.iCal;
using System.Web.UI.HtmlControls;
using Newtonsoft.Json;
using Rock.Reporting;
using System.Text.RegularExpressions;

namespace RockWeb.Plugins.com_thecrossingchurch.Event
{
    /// <summary>
    /// Renders a particular calendar using Lava.
    /// </summary>
    [DisplayName( "Event Registration CSV Importer" )]
    [Category( "com_thecrossingchurch > Event" )]
    [Description( "Creates registraiton entries based on a csv" )]

    public partial class EventRegistrationCSVImporter : Rock.Web.UI.RockBlock
    {

        #region Properties
        private RegistrationInstance instance { get; set; }
        private BinaryFile file { get; set; }
        private RockContext _context { get; set; }
        private bool registrarIsRegistrant { get; set; }
        #endregion

        #region Base ControlMethods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            _context = new RockContext();
            //txtRegistrantFName.Text = "RegistrantFName";
            //txtRegistrantLName.Text = "RegistrantLName";
            //txtRegistrarFName.Text = "RegistrarFName";
            //txtRegistrarLName.Text = "RegistrarLName";
            //txtRegistrarGrp.Text = "RegistrarEmail";
            //txtRegistrarPhone.Text = "RegistrarPhone";
            //Save to viewstate
            List<CustomColumns> customCols = new List<CustomColumns>();
            if ( ViewState["CustomColumns"] != null )
            {
                customCols = JsonConvert.DeserializeObject<List<CustomColumns>>( ViewState["CustomColumns"].ToString() );
            }
            for ( int i = 0; i < customCols.Count(); i++ )
            {
                GenerateCustomColumn( i );
            }
        }

        #endregion

        #region Methods

        private void GenerateCustomColumn( int idx )
        {
            //Load ViewState Info
            List<CustomColumns> customCols = new List<CustomColumns>();
            if ( ViewState["CustomColumns"] != null )
            {
                customCols = JsonConvert.DeserializeObject<List<CustomColumns>>( ViewState["CustomColumns"].ToString() );
            }

            //Create a new row
            var row = new HtmlGenericControl( "div" );
            row.AddCssClass( "row" );

            //Header Value in CSV
            var keyCol = new HtmlGenericControl( "div" );
            keyCol.AddCssClass( "col col-xs-4" );
            var keyTxt = new RockTextBox();
            keyTxt.Label = "Header Value in CSV";
            keyTxt.ID = "key_" + idx;
            keyTxt.TextChanged += new EventHandler( chgHeaderValue );
            keyTxt.AutoPostBack = true;
            keyCol.Controls.Add( keyTxt );

            //Type of Value
            var typeCol = new HtmlGenericControl( "div" );
            typeCol.AddCssClass( "col col-xs-4" );
            var typeDDL = new RockDropDownList();
            typeDDL.Label = "Data Type";
            typeDDL.ID = "type_" + idx;
            typeDDL.DataSource = new List<string>() { "", "Person Attribute", "Person Property", "Registration Attribute", "Registrant Attribute" };
            typeDDL.DataBind();
            typeDDL.SelectedIndexChanged += new EventHandler( chgTypeValue );
            typeDDL.AutoPostBack = true;
            typeCol.Controls.Add( typeDDL );

            //Attribute/Prperty in Rock
            var rockCol = new HtmlGenericControl( "div" );
            rockCol.AddCssClass( "col col-xs-4" );
            var rockDDL = new RockDropDownList();

            if ( customCols.Count() > idx && !String.IsNullOrEmpty( customCols[idx].ValueType ) )
            {
                rockDDL = GenerateRockValueField( customCols[idx].ValueType, idx );
                rockCol.Controls.Add( rockDDL );
            }

            row.Controls.Add( keyCol );
            row.Controls.Add( typeCol );
            row.Controls.Add( rockCol );

            var wrapper = new HtmlGenericControl( "div" );
            wrapper.AddCssClass( "row" );
            var dataCol = new HtmlGenericControl( "div" );
            dataCol.AddCssClass( "col col-xs-11" );
            var delCol = new HtmlGenericControl( "div" );
            delCol.AddCssClass( "col col-xs-1 del-btn-wrapper" );

            var delBtn = new BootstrapButton();
            delBtn.Text = "<i class='fa fa-times'></i>";
            delBtn.CssClass = "btn btn-xs btn-square btn-danger";
            delBtn.ID = "btnDel_" + idx;
            delBtn.Click += new EventHandler( btnDel_Click );
            delCol.Controls.Add( delBtn );

            dataCol.Controls.Add( row );
            wrapper.Controls.Add( dataCol );
            wrapper.Controls.Add( delCol );

            //Add to panel
            pnlCSVColumns.Controls.Add( wrapper );
        }

        private RockDropDownList GenerateRockValueField( string valueType, int idx )
        {
            var rockDDL = new RockDropDownList();
            rockDDL.ID = "rock_" + idx;
            rockDDL.SelectedIndexChanged += new EventHandler( chgRockValue );
            rockDDL.AutoPostBack = true;
            int entityTypeId = 0;
            RegistrationInstance ri = new RegistrationInstanceService( _context ).Get( pkrRegistrationInstance.RegistrationInstanceId.Value );
            switch ( valueType )
            {
                case "Person Attribute":
                    rockDDL.Label = "Person Attribute";
                    entityTypeId = EntityTypeCache.GetId<Rock.Model.Person>().Value;
                    rockDDL.DataSource = new AttributeService( _context ).Queryable().Where( a => a.EntityTypeId == entityTypeId ).ToList().Select( a => new ListItem( a.Key, a.Key ) ).OrderBy( a => a.Text ).ToList();
                    break;
                case "Person Property":
                    rockDDL.Label = "Person Property";
                    //entityTypeId = EntityTypeCache.GetId<Rock.Model.Person>().Value;
                    //var entityFields = EntityHelper.GetEntityFields( typeof( Person ) ).Where( ef => ef.FieldKind == FieldKind.Property ).Select( ef => new ListItem( ef.Name, ef.UniqueName ) ).OrderBy( ef => ef.Text ).ToList();
                    //entityFields.Add( new ListItem( "Mobile Phone", "MobilePhone" ) );
                    rockDDL.DataSource = new List<ListItem>() { new ListItem( "", "" ), new ListItem( "Id", "Id" ), new ListItem( "Birthdate", "Birthdate" ), new ListItem( "Email", "Email" ), new ListItem( "Gender", "Gender" ), new ListItem( "MobilePhone", "MobilePhone" ) };
                    break;
                case "Registration Attribute":
                    rockDDL.Label = "Registration Attribute";
                    entityTypeId = EntityTypeCache.GetId<Rock.Model.Registration>().Value;
                    var regAttrSource = new List<ListItem>();
                    regAttrSource.Add( new ListItem( "", "" ) );
                    regAttrSource.AddRange( new AttributeService( _context ).Queryable().Where( a => a.EntityTypeId == entityTypeId && a.EntityTypeQualifierValue == ri.RegistrationTemplateId.ToString() ).ToList().Select( a => new ListItem( a.Key, a.Key ) ).OrderBy( a => a.Text ) );
                    rockDDL.DataSource = regAttrSource;
                    break;
                case "Registrant Attribute":
                    rockDDL.Label = "Registrant Attribute";
                    entityTypeId = EntityTypeCache.GetId<Rock.Model.RegistrationRegistrant>().Value;
                    var rrAttrSource = new List<ListItem>();
                    rrAttrSource.Add( new ListItem( "", "" ) );
                    rrAttrSource.AddRange( new AttributeService( _context ).Queryable().Where( a => a.EntityTypeId == entityTypeId && a.EntityTypeQualifierValue == ri.RegistrationTemplateId.ToString() ).ToList().Select( a => new ListItem( a.Key, a.Key ) ).OrderBy( a => a.Text ) );
                    rockDDL.DataSource = rrAttrSource;
                    break;
            }
            rockDDL.DataBind();
            return rockDDL;
        }

        /// <summary>
        /// Create Registration items for the specified instance based on the CSV
        /// </summary>
        /// <param name="regEmail">The header column value of the column that contains the registrar's email</param>
        /// <param name="regPhone">The header column value of the column that contains the registrar's mobile phone</param>
        /// <param name="regRockId">The header column value of the column that contains the registrar's Rock Id</param>
        /// <param name="regFName">The header column value of the column that contains the registrar's first name</param>
        /// <param name="regLName">The header column value of the column that contains the registrar's last name</param>
        /// <param name="rrFName">The header column value of the column that contains the registrant's first name</param>
        /// <param name="rrLName">The header column value of the column that contains the registrant's last name</param>
        private void ProcessCSV( string regEmail, string regPhone, string regRockId, string regFName, string regLName, string rrFName, string rrLName )
        {
            List<CustomColumns> customCols = new List<CustomColumns>();
            if ( ViewState["CustomColumns"] != null )
            {
                customCols = JsonConvert.DeserializeObject<List<CustomColumns>>( ViewState["CustomColumns"].ToString() );
            }
            List<Registration> registrations = new List<Registration>();
            registrarIsRegistrant = false;

            //If no registrar names were given, assume registrant is self-registering 
            if ( String.IsNullOrEmpty( regFName ) )
            {
                regFName = rrFName;
                registrarIsRegistrant = true;
            }
            if ( String.IsNullOrEmpty( regLName ) )
            {
                regLName = rrLName;
            }
            List<string> errors = new List<string>();

            //Get CSV Contents
            byte[] content = file.DatabaseData.Content;
            string value = System.Text.Encoding.UTF8.GetString( content );
            List<string> headers = value.Split( new[] { Environment.NewLine }, StringSplitOptions.None )[0].Split( ',' ).ToList();
            headers = headers.Select( h => Regex.Replace( h, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled ) ).ToList();
            List<string> rawData = value.Split( new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            rawData.RemoveAt( 0 );

            //Verify Necessary Columns Exist
            if ( !headers.Contains( regEmail ) || !headers.Contains( rrFName ) || !headers.Contains( rrLName ) )
            {
                //Cannot process file
                nbMessage.Text = "Unable to process file, header columns do not match data";
                return;
            }

            //Process CSV Contents
            for ( int i = 0; i < rawData.Count(); i++ )
            {
                List<string> data = rawData[i].Split( ',' ).ToList();
                Registration registration = new Registration();
                //Set Registrar
                Person registrar = null;
                IEnumerable<Person> matches = new List<Person>();
                if ( !String.IsNullOrEmpty( regRockId ) )
                {
                    registrar = new PersonService( _context ).Get( data[headers.IndexOf( regRockId )].AsInteger() );
                }
                if ( registrar == null )
                {
                    CustomColumns regphoneCol = customCols.FirstOrDefault( c => c.HeaderValue == regPhone );
                    string regphoneVal = null;
                    if ( regphoneCol != null && !String.IsNullOrEmpty( regPhone ) )
                    {
                        regphoneVal = data[headers.IndexOf( regPhone )];
                    }
                    PersonService.PersonMatchQuery query = new PersonService.PersonMatchQuery( data[headers.IndexOf( regFName )], data[headers.IndexOf( regLName )], data[headers.IndexOf( regEmail )], regphoneVal );
                    matches = new PersonService( _context ).FindPersons( query );
                    if ( matches.Count() == 1 )
                    {
                        registrar = matches.First();
                    }
                    else
                    {
                        //Create a new Person
                        try
                        {
                            registrar = CreateNewPerson( data[headers.IndexOf( regFName )], data[headers.IndexOf( regLName )], data[headers.IndexOf( regEmail )], regphoneVal, null );
                        }
                        catch ( Exception e )
                        {
                            errors.Add( "Unable to Create Person Error: " + e.Message + "<br/>Details: " + data[headers.IndexOf( regFName )] + ", " + data[headers.IndexOf( regLName )] + ", " + data[headers.IndexOf( regEmail )] + ", " + regphoneVal );
                        }
                    }
                }
                if ( registrar != null )
                {
                    var existing = registrations.FirstOrDefault( r => r.PersonAliasId == registrar.PrimaryAliasId );
                    if ( existing != null )
                    {
                        registration = existing;
                        registrations.Remove( existing );
                    }
                    else
                    {
                        //Initialize the Registration
                        registration.RegistrationInstanceId = instance.Id;
                        registration.PersonAliasId = registrar.PrimaryAliasId;
                        registration.ConfirmationEmail = data[headers.IndexOf( regEmail )];
                        registration.PersonAlias = registrar.PrimaryAlias;
                        registration.FirstName = registrar.NickName;
                        registration.LastName = registrar.LastName;
                        registration.CreatedByPersonAliasId = CurrentPersonAliasId;
                        registration.ModifiedByPersonAliasId = CurrentPersonAliasId;
                        registration.CreatedDateTime = RockDateTime.Now;
                        registration.Registrants = new List<RegistrationRegistrant>();
                    }
                    //Save the registration
                    if ( registration.Id <= 0 )
                    {
                        _context.Registrations.Add( registration );
                        _context.SaveChanges();
                    }

                    //Add the registrant
                    Person registrant = null;
                    CustomColumns emailCol = customCols.FirstOrDefault( c => c.RockValue == "Email" );
                    CustomColumns phoneCol = customCols.FirstOrDefault( c => c.RockValue == "MobilePhone" );
                    CustomColumns genderCol = customCols.FirstOrDefault( c => c.RockValue == "Gender" );
                    CustomColumns birthdateCol = customCols.FirstOrDefault( c => c.RockValue == "Birthdate" );
                    CustomColumns rockidCol = customCols.FirstOrDefault( c => c.ValueType == "Person Property" && c.RockValue == "Id" );
                    string emailVal = null;
                    string phoneVal = null;
                    Gender? genderVal = null;
                    DateTime? birthdateVal = null;
                    int? rockidVal = null;
                    if ( emailCol != null )
                    {
                        emailVal = data[headers.IndexOf( emailCol.HeaderValue )];
                    }
                    if ( phoneCol != null )
                    {
                        phoneVal = data[headers.IndexOf( phoneCol.HeaderValue )];
                    }
                    if ( genderCol != null )
                    {
                        var gen = data[headers.IndexOf( genderCol.HeaderValue )];
                        genderVal = gen.ConvertToEnumOrNull<Gender>() ?? Gender.Unknown;
                    }
                    if ( birthdateCol != null )
                    {
                        var bd = data[headers.IndexOf( birthdateCol.HeaderValue )];
                        birthdateVal = DateTime.Parse( bd );
                    }
                    if ( rockidCol != null )
                    {
                        rockidVal = data[headers.IndexOf( rockidCol.HeaderValue )].AsIntegerOrNull();
                    }
                    if ( registrarIsRegistrant )
                    {
                        registrant = registrar;
                    }
                    //Get the person if we were given a Rock Id
                    if ( rockidVal.HasValue )
                    {
                        registrant = new PersonService( _context ).Get( rockidVal.Value );
                    }

                    if ( registrant == null )
                    {
                        PersonService.PersonMatchQuery query = new PersonService.PersonMatchQuery( data[headers.IndexOf( rrFName )], data[headers.IndexOf( rrLName )], emailVal, phoneVal, genderVal, birthdateVal );

                        var rrMatches = new PersonService( _context ).FindPersons( query );
                        if ( rrMatches.Count() == 1 )
                        {
                            registrant = rrMatches.First();
                        }
                        else
                        {
                            //Try to find a person in the family with the same name
                            var family = registrar.GetFamilyMembers().ToList().Where( gm => ( gm.Person.NickName == data[headers.IndexOf( rrFName )] || gm.Person.FirstName == data[headers.IndexOf( rrFName )] ) && gm.Person.LastName == data[headers.IndexOf( rrLName )] ).ToList();
                            if ( family.Count() == 1 )
                            {
                                registrant = family.First().Person;
                            }
                            //Try to find a person in the family with the same email
                            if ( registrant == null && family.Count() > 1 && !String.IsNullOrEmpty( emailVal ) )
                            {
                                var emailMatch = family.Where( gm => gm.Person.Email.ToLower() == emailVal.ToLower() );
                                if ( emailMatch.Count() == 1 )
                                {
                                    registrant = emailMatch.First().Person;
                                }
                            }
                            if ( registrant == null && family.Count() > 1 )
                            {
                                registrant = family.First().Person;
                            }
                        }
                        //Create a new Person
                        if ( registrant == null )
                        {
                            try
                            {
                                registrant = CreateNewPerson( data[headers.IndexOf( rrFName )], data[headers.IndexOf( rrLName )], emailVal, phoneVal, birthdateVal, genderVal, registrar.PrimaryFamilyId );
                            }
                            catch ( Exception e )
                            {
                                errors.Add( "Unable to Create Person Error: " + e.Message + "<br/>Details: " + data[headers.IndexOf( rrFName )] + ", " + data[headers.IndexOf( rrLName )] + ", " + emailVal + ", " + phoneVal + ", " + birthdateVal + ", " + genderVal + ", " + registrar.PrimaryFamilyId );
                            }
                        }
                    }

                    RegistrationRegistrant rr = new RegistrationRegistrant();
                    rr.RegistrationId = registration.Id;
                    rr.PersonAliasId = registrant.PrimaryAliasId;
                    registration.LoadAttributes();
                    rr.LoadAttributes();
                    registrant.LoadAttributes();

                    for ( int k = 0; k < customCols.Count(); k++ )
                    {
                        switch ( customCols[k].ValueType )
                        {
                            case "Person Attribute":
                                try
                                {
                                    registrant.SetAttributeValue( customCols[k].RockValue, data[headers.IndexOf( customCols[k].HeaderValue )] );
                                }
                                catch
                                {
                                    errors.Add( "Unable to save " + customCols[k].RockValue + " for " + registrant.FullName );
                                }
                                break;
                            case "Person Property":
                                errors.Add( "Unable to save " + customCols[k].RockValue + " for " + registrant.FullName );
                                break;
                            case "Registration Attribute":
                                try
                                {
                                    registration.SetAttributeValue( customCols[k].RockValue, data[headers.IndexOf( customCols[k].HeaderValue )] );
                                }
                                catch
                                {
                                    errors.Add( "Unable to save " + customCols[k].RockValue + " for " + registrar.FullName + "'s Registration" );
                                }
                                break;
                            case "Registrant Attribute":
                                try
                                {
                                    rr.SetAttributeValue( customCols[k].RockValue, data[headers.IndexOf( customCols[k].HeaderValue )] );
                                }
                                catch
                                {
                                    errors.Add( "Unable to save " + customCols[k].RockValue + " for " + registrant.FullName );
                                }
                                break;
                        }
                    }
                    registration.Registrants.Add( rr );
                }
                registrations.Add( registration );
                string msg = "Processed " + ( i + 1 ) + " rows out of " + ( rawData.Count() );
                ShowMessage( null, msg );
            }
            //Save the registrations
            _context.SaveChanges();
            //Save the Attribute Values
            for ( int i = 0; i < registrations.Count(); i++ )
            {
                registrations[i].SaveAttributeValues( _context );
                foreach ( RegistrationRegistrant r in registrations[i].Registrants )
                {
                    r.SaveAttributeValues( _context );
                }
            }
            string finalMsg = "Import completed " + rawData.Count() + " out of " + rawData.Count() + " rows.";
            if ( errors.Count() > 0 )
            {
                finalMsg = "Import completed with the following errors: ";
            }
            ShowMessage( errors, finalMsg );
        }

        protected Person CreateNewPerson( string fname, string lname, string email, string phone, DateTime? birthdate, Gender? gender = null, int? familyId = null )
        {
            // Not Currently Adding in People to Rock from CSV
            throw new NotImplementedException( "Not adding new people at this time." );

            //Person person = new Person();
            //person.FirstName = fname.FixCase();
            //person.NickName = fname.FixCase();
            //person.LastName = lname.FixCase();
            //person.Email = email;
            //if ( birthdate.HasValue )
            //{
            //    person.SetBirthDate( birthdate );
            //}
            //if ( gender.HasValue )
            //{
            //    person.Gender = gender.Value;
            //}
            //person.EmailPreference = EmailPreference.EmailAllowed;
            //person.RecordTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON.AsGuid() ).Id;

            //UpdatePhoneNumber( person, phone );

            //var defaultConnectionStatus = DefinedValueCache.Get( GetAttributeValue( Rock.SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_WEB_PROSPECT ).AsGuid() );
            //if ( defaultConnectionStatus != null )
            //{
            //    person.ConnectionStatusValueId = defaultConnectionStatus.Id;
            //}

            //var defaultRecordStatus = DefinedValueCache.Get( GetAttributeValue( Rock.SystemGuid.DefinedValue.PERSON_RECORD_STATUS_PENDING ).AsGuid() );
            //if ( defaultRecordStatus != null )
            //{
            //    person.RecordStatusValueId = defaultRecordStatus.Id;
            //}
            //if ( familyId.HasValue )
            //{
            //    var familyGroupType = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FAMILY );
            //    var adultRoleId = familyGroupType.Roles
            //        .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_ADULT.AsGuid() ) )
            //        .Select( r => r.Id )
            //        .FirstOrDefault();
            //    var childRoleId = familyGroupType.Roles
            //        .Where( r => r.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_FAMILY_MEMBER_CHILD.AsGuid() ) )
            //        .Select( r => r.Id )
            //        .FirstOrDefault();
            //    var age = person.Age;
            //    int familyRoleId = age.HasValue && age < 18 ? childRoleId : adultRoleId;
            //    PersonService.AddPersonToFamily( person, true, familyId.Value, familyRoleId, _context );
            //}
            //else
            //{
            //    var familyGroup = PersonService.SaveNewPerson( person, _context );
            //    if ( familyGroup != null && familyGroup.Members.Any() )
            //    {
            //        person = familyGroup.Members.Select( m => m.Person ).First();
            //    }
            //}
            //return person;
        }

        void UpdatePhoneNumber( Person person, string mobileNumber )
        {
            if ( !string.IsNullOrWhiteSpace( PhoneNumber.CleanNumber( mobileNumber ) ) )
            {
                var phoneNumberType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                if ( phoneNumberType == null )
                {
                    return;
                }

                var phoneNumber = person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == phoneNumberType.Id );
                string oldPhoneNumber = string.Empty;
                if ( phoneNumber == null )
                {
                    phoneNumber = new PhoneNumber { NumberTypeValueId = phoneNumberType.Id };
                    person.PhoneNumbers.Add( phoneNumber );
                }
                else
                {
                    oldPhoneNumber = phoneNumber.NumberFormattedWithCountryCode;
                }

                // TODO handle country code here
                phoneNumber.Number = PhoneNumber.CleanNumber( mobileNumber );
            }
        }

        protected void ShowMessage( List<string> errors, string message )
        {
            if ( errors != null && errors.Count() > 0 )
            {
                string response = message + "<ul>";
                for ( int i = 0; i < errors.Count(); i++ )
                {
                    response += "<li>" + errors[i] + "</li>";
                }
                response += "</ul>";
                nbMessage.Text = response;
                nbMessage.NotificationBoxType = NotificationBoxType.Warning;
            }
            else
            {
                nbMessage.Text = message;
                nbMessage.NotificationBoxType = NotificationBoxType.Info;
            }
            nbMessage.Visible = true;
        }

        #endregion


        #region Events

        protected void btnChangePnl_Click( object sender, EventArgs e )
        {
            BootstrapButton btn = ( BootstrapButton ) sender;
            switch ( btn.CommandArgument )
            {
                case "1":
                    pnlCSV.Visible = false;
                    pnlRI.Visible = true;
                    pnlSettings.Visible = false;
                    break;
                case "2":
                    pnlCSV.Visible = false;
                    pnlRI.Visible = false;
                    pnlSettings.Visible = true;
                    break;
                default:
                    pnlCSV.Visible = true;
                    pnlRI.Visible = false;
                    pnlSettings.Visible = false;
                    break;
            }
        }

        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            //Reset the Message Box
            nbMessage.Visible = false;
            nbMessage.Text = "";

            //Pull fields from input
            int? fileId = inputFile.BinaryFileId;
            //For Testing
            //fileId = 342529;
            if ( fileId.HasValue )
            {
                file = new BinaryFileService( _context ).Get( fileId.Value );
            }
            int? riId = pkrRegistrationInstance.RegistrationInstanceId;
            if ( riId.HasValue )
            {
                instance = new RegistrationInstanceService( _context ).Get( riId.Value );
            }
            //Clean Fields
            string regEmail = Regex.Replace( txtRegistrarGrp.Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled );
            string regPhone = Regex.Replace( txtRegistrarPhone.Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled );
            string regRockId = Regex.Replace( txtRegistrarRockId.Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled );
            string regFName = Regex.Replace( txtRegistrarFName.Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled );
            string regLName = Regex.Replace( txtRegistrarLName.Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled );
            string rrFName = Regex.Replace( txtRegistrantFName.Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled );
            string rrLName = Regex.Replace( txtRegistrantLName.Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled );

            if ( !fileId.HasValue || !riId.HasValue || String.IsNullOrEmpty( regEmail ) || String.IsNullOrEmpty( rrFName ) || String.IsNullOrEmpty( rrLName ) )
            {
                nbMessage.Text = "Please fill out all the required information.";
                nbMessage.Visible = true;
            }
            else
            {
                ProcessCSV( regEmail, regPhone, regRockId, regFName, regLName, rrFName, rrLName );
            }
        }

        protected void btnAddColumn_Click( object sender, EventArgs e )
        {
            var size = pnlCSVColumns.Controls.Count;
            GenerateCustomColumn( size );

            //Save to viewstate
            List<CustomColumns> customCols = new List<CustomColumns>();
            if ( ViewState["CustomColumns"] != null )
            {
                customCols = JsonConvert.DeserializeObject<List<CustomColumns>>( ViewState["CustomColumns"].ToString() );
            }
            customCols.Add( new CustomColumns() );
            ViewState["CustomColumns"] = JsonConvert.SerializeObject( customCols );
        }

        protected void chgTypeValue( object sender, EventArgs e )
        {
            RockDropDownList ddl = ( RockDropDownList ) sender;
            List<CustomColumns> customCols = new List<CustomColumns>();
            if ( ViewState["CustomColumns"] != null )
            {
                customCols = JsonConvert.DeserializeObject<List<CustomColumns>>( ViewState["CustomColumns"].ToString() );
            }
            int idx = ddl.ID.Split( '_' )[1].AsInteger();
            customCols[idx].ValueType = ddl.SelectedValue;
            ViewState["CustomColumns"] = JsonConvert.SerializeObject( customCols );
            pnlCSVColumns.Controls[idx].Controls[0].Controls[0].Controls[2].Controls.Clear();
            pnlCSVColumns.Controls[idx].Controls[0].Controls[0].Controls[2].Controls.Add( GenerateRockValueField( ddl.SelectedValue, idx ) );
        }

        protected void chgHeaderValue( object sender, EventArgs e )
        {
            RockTextBox txt = ( RockTextBox ) sender;
            List<CustomColumns> customCols = new List<CustomColumns>();
            if ( ViewState["CustomColumns"] != null )
            {
                customCols = JsonConvert.DeserializeObject<List<CustomColumns>>( ViewState["CustomColumns"].ToString() );
            }
            int idx = txt.ID.Split( '_' )[1].AsInteger();
            customCols[idx].HeaderValue = Regex.Replace( txt.Text, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled );
            ViewState["CustomColumns"] = JsonConvert.SerializeObject( customCols );
        }

        protected void chgRockValue( object sender, EventArgs e )
        {
            RockDropDownList ddl = ( RockDropDownList ) sender;
            List<CustomColumns> customCols = new List<CustomColumns>();
            if ( ViewState["CustomColumns"] != null )
            {
                customCols = JsonConvert.DeserializeObject<List<CustomColumns>>( ViewState["CustomColumns"].ToString() );
            }
            int idx = ddl.ID.Split( '_' )[1].AsInteger();
            customCols[idx].RockValue = ddl.SelectedValue;
            ViewState["CustomColumns"] = JsonConvert.SerializeObject( customCols );
        }

        protected void btnDel_Click( object sender, EventArgs e )
        {
            BootstrapButton btn = ( BootstrapButton ) sender;
            int idx = btn.ID.Split( '_' )[1].AsInteger();
            pnlCSVColumns.Controls.RemoveAt( idx );
            List<CustomColumns> customCols = new List<CustomColumns>();
            if ( ViewState["CustomColumns"] != null )
            {
                customCols = JsonConvert.DeserializeObject<List<CustomColumns>>( ViewState["CustomColumns"].ToString() );
            }
            customCols.RemoveAt( idx );
            ViewState["CustomColumns"] = JsonConvert.SerializeObject( customCols );
        }

        #endregion

        #region Helper Classes

        private class CustomColumns
        {
            public string HeaderValue { get; set; }
            public string ValueType { get; set; }
            public string RockValue { get; set; }
        }

        #endregion
    }
}
