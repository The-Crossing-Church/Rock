
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_thecrossingchurch.CheckIn.Manager
{
    /// <summary>
    /// Block used to display person and details about recent check-ins
    /// </summary>
    [DisplayName( "Reprint Labels" )]
    [Category( "com_thecrossingchurch > Check-in Manager" )]
    [Description( "Displays buttons for reprinting lables." )]

    [TextField("Desk A IP", null, true, "10.5.60.102", Order = 6 )]
    [TextField("Desk B IP", null, true, "10.5.60.119", Order = 7 )]
    [TextField("Foyer IP", null, true, "10.5.60.112", Order = 8 )]

    public partial class ReprintLables : Rock.Web.UI.RockBlock
    {

        #region Page Parameter Constants

        private const string PERSON_GUID_PAGE_QUERY_KEY = "Person";

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                if ( IsUserAuthorized( Authorization.VIEW ) )
                {
                    Guid? personGuid = PageParameter( PERSON_GUID_PAGE_QUERY_KEY ).AsGuidOrNull();
                    if ( personGuid.HasValue )
                    {
                        ShowDetail( personGuid.Value );
                    }
                }
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( PageParameter( PERSON_GUID_PAGE_QUERY_KEY ).AsGuid() );
        }

        /// <summary>
        /// Handles sending the selected labels off to the selected printer from the custom buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void mdReprintLabelsCustom_Click( object sender, EventArgs e )
        {
            // Get the person Id from the Guid in the page parameter
            var rockContext = new RockContext();
            Guid? personGuid = PageParameter(PERSON_GUID_PAGE_QUERY_KEY).AsGuidOrNull();
            var personId = new PersonService(rockContext).Queryable().Where(p => p.Guid == personGuid.Value).Select(p => p.Id).FirstOrDefault();
            hfPersonId.Value = personId.ToString();
            if ( personId == 0 )
            {
                return;
            }

            //Get the printer based on the button they clicked
            BootstrapButton btn = (BootstrapButton)sender;
            string printer = "";
            if ( btn.ID == "btnReprintDeskA" )
            {
                printer = GetAttributeValue( "DeskAIP" );
            }
            else if ( btn.ID == "btnReprintDeskB" )
            {
                printer = GetAttributeValue( "DeskBIP" );
            }
            else if ( btn.ID == "btnReprintFoyer3" )
            {
                printer = GetAttributeValue( "FoyerIP" );
            }

            // Get the person Id from the Guid
            var selectedAttendanceIds = hfCurrentAttendanceIds.Value.SplitDelimitedValues().AsIntegerList();

            // Print all available label types
            var possibleLabels = ZebraPrint.GetLabelTypesForPerson(personId, selectedAttendanceIds);

            //var fileGuids = new List<Guid>() { new Guid("779123b2-a54c-4c0e-8b26-d45a4a9a5097"), new Guid("c9a9e544-073c-4133-bafd-a360d6068434") };
            var fileGuids = possibleLabels.Select(pl => pl.FileGuid).ToList();

            // Now, finally, re-print the labels.
            List<string> messages = ReprintLabels( fileGuids, personId, selectedAttendanceIds, printer );
            nbReprintMessage.Visible = true;
            nbReprintMessage.Text = messages.JoinStrings("<br>");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Show the details for the given person.
        /// </summary>
        /// <param name="personGuid"></param>
        private void ShowDetail( Guid personGuid )
        {
            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                var person = personService.Queryable( true, true ).Include(a => a.PhoneNumbers).Include(a => a.RecordStatusValue)
                    .FirstOrDefault( a => a.Guid == personGuid );

                if ( person == null )
                {
                    return;
                }

                var schedules = new ScheduleService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( s => s.CheckInStartOffsetMinutes.HasValue )
                    .ToList();

                var scheduleIds = schedules.Select( s => s.Id ).ToList();

                int? personAliasId = person.PrimaryAliasId;

                PersonAliasService personAliasService = new PersonAliasService( rockContext );
                if ( !personAliasId.HasValue )
                {
                    personAliasId = personAliasService.GetPrimaryAliasId( person.Id );
                }

                var date = DateTime.Today;

                var attendanceIds = new AttendanceService(rockContext)
                    .Queryable("Occurrence.Schedule,Occurrence.Group,Occurrence.Location,AttendanceCode")
                    .Where(a =>
                       a.PersonAliasId.HasValue &&
                       a.PersonAliasId == personAliasId &&
                       a.Occurrence.ScheduleId.HasValue &&
                       a.Occurrence.GroupId.HasValue &&
                       a.Occurrence.LocationId.HasValue &&
                       a.DidAttend.HasValue &&
                       a.DidAttend.Value &&
                       scheduleIds.Contains(a.Occurrence.ScheduleId.Value) &&
                       DateTime.Compare( a.Occurrence.OccurrenceDate, date ) == 0 )
                    .ToList()                                                             // Run query to get recent most 20 checkins
                    .OrderByDescending(a => a.Occurrence.Schedule.StartTimeOfDay)
                    .Select(a => a.Id)
                    .ToList();

                hfCurrentAttendanceIds.Value = attendanceIds.AsDelimited(",");
            }
        }

        public static List<string> ReprintLabels( List<Guid> fileGuids, int personId, List<int> selectedAttendanceIds, string printerAddress )
        {
            // Fetch the actual labels and print them
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );

            // Get the selected attendance records (but only the ones that have label data)
            var labelDataList = attendanceService
                .GetByIds( selectedAttendanceIds )
                .Where( a => a.AttendanceData.LabelData != null )
                .Select( a => a.AttendanceData.LabelData );

            var printFromServer = new List<CheckInLabel>();

            // Now grab only the selected label types (matching fileGuids) from those record's AttendanceData
            // for the selected  person
            foreach ( var labelData in labelDataList )
            {
                var json = labelData.Trim();

                // skip if the return type is not an array
                if ( json.Substring( 0, 1 ) != "[" )
                {
                    continue;
                }

                // De-serialize the JSON into a list of objects
                var checkinLabels = JsonConvert.DeserializeObject<List<CheckInLabel>>( json );

                // skip if no labels were found
                if ( checkinLabels == null )
                {
                    continue;
                }

                // Take only the labels that match the selected person (or if they are Family type labels) and file guids).
                checkinLabels = checkinLabels.Where( l => ( l.PersonId == personId || l.LabelType == KioskLabelType.Family ) && fileGuids.Contains( l.FileGuid ) ).ToList();

                // Override the printer by printing to the given printerAddress?
                checkinLabels.ToList().ForEach( l => l.PrinterAddress = printerAddress );
                printFromServer.AddRange( checkinLabels );
            }

            // Remove Duplicates
            var labelsToPrint = new List<CheckInLabel>();
            var labelContents = new List<string>();

            foreach( var label in printFromServer.ToList() )
            {
                var labelCache = KioskLabel.Get( label.FileGuid );
                if ( labelCache != null )
                {
                    var labelContent = MergeLabelFields( labelCache.FileContent, label.MergeFields );
                    if ( !labelContents.Contains( labelContent ) )
                    {
                        labelContents.Add( labelContent );
                        labelsToPrint.Add( label );
                    }
                }
            }

            // Print server labels
            var messages = ZebraPrint.PrintLabels( labelsToPrint );

            // No messages is "good news".
            if ( messages.Count == 0 )
            {
                messages.Add( "The labels have been printed." );
            }

            return messages;
        }

        private static string MergeLabelFields( string label, Dictionary<string, string> mergeFields )
        {
            foreach ( var mergeField in mergeFields )
            {
                if ( !string.IsNullOrWhiteSpace( mergeField.Value ) )
                {
                    label = Regex.Replace( label, string.Format( @"(?<=\^FD){0}(?=\^FS)", mergeField.Key ), mergeField.Value );
                }
                else
                {
                    // Remove the box preceding merge field
                    label = Regex.Replace( label, string.Format( @"\^FO.*\^FS\s*(?=\^FT.*\^FD{0}\^FS)", mergeField.Key ), string.Empty );

                    // Remove the merge field
                    label = Regex.Replace( label, string.Format( @"\^FD{0}\^FS", mergeField.Key ), "^FD^FS" );
                }
            }

            return label;
        }
        #endregion

    }
}