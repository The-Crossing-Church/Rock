using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Data.Entity;
using System.Web.UI.HtmlControls;
using System.Data;
using Newtonsoft.Json;
using WebGrease.Css.Extensions;
using System.Diagnostics;

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting.CK
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "CK Attendance Report" )]
    [Category( "com_thecrossingchurch > Reports > CK" )]
    [Description( "Custom Attendance Report for Crossing Kids" )]

    [IntegerField( "Notes Page", "The page id of the notes page for the report.", true, 0, "", 0 )]
    [IntegerField( "Note Type Id", "The id of the note type used for the report.", true, 0, "", 1 )]
    [NoteTypeField( "Note Type", "", false, "", "", "", true, "", "", 1 )]
    [LinkedPage( "Notes Page", "The page to add/view notes.", true, "", "", 2 )]
    [CheckinConfigurationTypeField( "Check In Areas", Order = 3 )]
    [AttributeField( "Location Sort", EntityTypeGuid = "0D6410AD-C83C-47AC-AF3D-616D09EDF63B", EntityTypeQualifierColumn = "", EntityTypeQualifierValue = "", Order = 4 )]
    [AttributeField( "Location Historical Threshold", EntityTypeGuid = "0D6410AD-C83C-47AC-AF3D-616D09EDF63B", EntityTypeQualifierColumn = "", EntityTypeQualifierValue = "", Order = 5 )]

    public partial class CKAttendance : Rock.Web.UI.RockBlock //, ICustomGridColumns
    {
        #region Variables
        // Variables that get set with filter 
        private DateTime start;
        private DateTime end;
        private List<int> svcTimes;
        //Configuration Variables
        private NoteType noteType;
        private Rock.Model.Page notesPage;
        private Guid? locationSortGuid;
        //Local Variables
        private List<DateAttendance> data;
        private List<DateThreshold> thresholdData;
        private List<ScheduleLocations> scheduleLocations;
        private List<int> groupTypeIds;

        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
            ScriptManager.RegisterStartupScript( Page, this.GetType(), "AKey", "notes();", true );
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if (!stDate.SelectedDate.HasValue)
            {
                var dt = DateTime.Now.StartOfWeek( DayOfWeek.Sunday );
                stDate.SelectedDate = dt.AddDays( -21 );
            }
            if (!endDate.SelectedDate.HasValue)
            {
                endDate.SelectedDate = DateTime.Now;
            }
            start = stDate.SelectedDate.Value;
            end = endDate.SelectedDate.Value;
            locationSortGuid = GetAttributeValue( "LocationSort" ).AsGuidOrNull();
            Guid? noteTypeGuid = GetAttributeValue( "NoteType" ).AsGuidOrNull();
            Guid? notePageGuid = GetAttributeValue( "NotesPage" ).AsGuidOrNull();
            if (noteTypeGuid.HasValue && notePageGuid.HasValue)
            {
                using (RockContext context = new RockContext())
                {
                    noteType = new NoteTypeService( context ).Get( noteTypeGuid.Value );
                    notesPage = new PageService( context ).Get( notePageGuid.Value );
                }
            }

            BindFilter();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            start = stDate.SelectedDate.Value;
            end = endDate.SelectedDate.Value;
            svcTimes = lbSchedules.SelectedValues.Select( x => Int32.Parse( x ) ).ToList();
            GetAttendance();
        }

        protected void sdrpDates_SelectedDateRangeChanged( object sender, EventArgs e )
        {
            BindFilter();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Binds the schedules.
        /// </summary>
        private void BindFilter()
        {
            var selectedItems = lbSchedules.SelectedValuesAsInt;

            lSchedules.Visible = true;
            lbSchedules.Visible = false;
            lbSchedules.Items.Clear();
            btnFilter.Enabled = false;

            DateTime? dateStart = stDate.SelectedDate;
            DateTime? dateEnd = endDate.SelectedDate;

            lbSchedules.Required = GetAttributeValue( "RequireSchedule" ).AsBoolean( true );
            if (lbSchedules.Required)
            {
                pnlSchedules.AddCssClass( "required" );
            }
            else
            {
                pnlSchedules.RemoveCssClass( "required" );
            }

            if (dateStart.HasValue)
            {
                var areas = GetAttributeValues( "CheckInAreas" ).AsGuidList();

                using (var rockContext = new RockContext())
                {
                    var occQry = new AttendanceOccurrenceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( o =>
                            o.OccurrenceDate >= dateStart &&
                            (!dateEnd.HasValue || o.OccurrenceDate < dateEnd) &&
                            o.Attendees.Any( a => a.DidAttend.HasValue && a.DidAttend.Value ) &&
                            o.Schedule != null
                        );

                    if (areas.Count() > 0)
                    {
                        var gt_svc = new GroupTypeService( rockContext );
                        var groupTypeIds = new List<int>();
                        foreach (var area in areas)
                        {
                            groupTypeIds.AddRange( gt_svc
                                .GetCheckinAreaDescendants( area )
                                .Select( t => t.Id )
                                .ToList() );
                        }

                        occQry = occQry
                            .Where( o =>
                                o.Group != null &&
                                groupTypeIds.Contains( o.Group.GroupTypeId ) );
                    }

                    var serviceTimes = occQry
                        .Select( o => o.Schedule )
                        .Distinct()
                        .ToList()
                        .OrderBy( s => s.StartTimeOfDay )
                        .Select( o => new
                        {
                            o.Id,
                            o.Name
                        } )
                        .ToList();

                    foreach (var serviceTime in serviceTimes)
                    {
                        var item = new ListItem( serviceTime.Name, serviceTime.Id.ToString() );
                        item.Selected = selectedItems.Contains( serviceTime.Id );
                        lbSchedules.Items.Add( item );
                    }

                    if (serviceTimes.Any())
                    {
                        lSchedules.Visible = false;
                        lbSchedules.Visible = true;
                        btnFilter.Enabled = true;
                    }
                    else
                    {
                        btnFilter.Enabled = !lbSchedules.Required;
                        lSchedules.Text = "There are not any Service Times for selected Date(s)";
                    }
                }
            }
        }

        /// <summary>
        /// Generates the report data
        /// </summary>
        private void GetAttendance()
        {
            using (var rockContext = new RockContext())
            {
                var areas = GetAttributeValues( "CheckInAreas" ).AsGuidList();
                groupTypeIds = new List<int>();
                GroupTypeService gt_svc = new GroupTypeService( rockContext );
                foreach (var area in areas)
                {
                    groupTypeIds.AddRange( gt_svc
                        .GetCheckinAreaDescendants( area )
                        .Select( t => t.Id )
                        .ToList() );
                }

                Rock.Model.Attribute locationSortAttr = new AttributeService( rockContext ).Get( locationSortGuid.Value );
                var locationSortOrders = new AttributeValueService( rockContext ).Queryable().Where( av => av.AttributeId == locationSortAttr.Id );
                var scheduleSortOrders = new ScheduleService( rockContext ).Queryable().Where( s => lbSchedules.SelectedValuesAsInt.Contains( s.Id ) ).ToList();

                if (stDate.SelectedDate.HasValue && locationSortGuid.HasValue)
                {
                    //All attendance occurrences for the selected Check-in Area within the date range at the selected schedules 
                    var occurrences = new AttendanceOccurrenceService( rockContext ).Queryable().Where( ao => groupTypeIds.Contains( ao.Group.GroupTypeId ) && ao.OccurrenceDate > stDate.SelectedDate.Value && (endDate.SelectedDate.HasValue ? ao.OccurrenceDate < endDate.SelectedDate.Value : true) && lbSchedules.SelectedValuesAsInt.Contains( ao.ScheduleId.Value ) );
                    var occurrenceIds = occurrences.Select( ao => ao.Id ).ToList();
                    var attendance = new AttendanceService( rockContext ).Queryable().Join( occurrences, a => a.OccurrenceId, ao => ao.Id, ( a, ao ) => a ).Where( a => a.DidAttend.Value );

                    //Find an group attendance data by Date, Schedule, Location, and Group with counts for each group and a total for each location
                    data = attendance.Select( a => new { Date = a.Occurrence.OccurrenceDate, Location = new { a.Occurrence.LocationId, a.Occurrence.Location.Name }, Group = new { a.Occurrence.GroupId, a.Occurrence.Group.Name, a.Occurrence.Group.Order }, Schedule = new { a.Occurrence.ScheduleId, a.Occurrence.Schedule.Name } } )
                        .Join( locationSortOrders,
                            att => att.Location.LocationId,
                            av => av.EntityId,
                            ( att, av ) => new { att.Date, Location = new { att.Location.LocationId, att.Location.Name, Order = av.Value }, att.Group, att.Schedule }
                         )
                        .GroupBy( a => new { a.Date, a.Location, a.Group, a.Schedule } )
                        .Select( a => new { a.Key.Date, a.Key.Location, a.Key.Group, a.Key.Schedule, Count = a.Count() } )
                        .GroupBy( a => new { a.Date, a.Location, a.Schedule } )
                        .Select( a => new { a.Key.Date, a.Key.Location, a.Key.Schedule, Total = a.Select( d => d.Count ).Sum(), Groups = a.Select( d => new GroupAttendance { GroupId = d.Group.GroupId, Group = d.Group.Name, Count = d.Count, Order = d.Group.Order } ).OrderBy( ga => ga.Order ) } )
                        .GroupBy( a => new { a.Date, a.Schedule } )
                        .Select( a => new { a.Key.Date, a.Key.Schedule, Locations = a.Select( d => new LocationAttendance { LocationId = d.Location.LocationId, Location = d.Location.Name, Order = d.Location.Order, Total = d.Total, Attendance = d.Groups } ) } )
                        .GroupBy( a => a.Date )
                        .Select( a => new DateAttendance { Date = a.Key, Attendance = a.Select( d => new ScheduleAttendance { ScheduleId = d.Schedule.ScheduleId, Schedule = d.Schedule.Name, Attendance = d.Locations } ) } )
                        .ToList();

                    for (int i = 0; i < data.Count(); i++)
                    {
                        data[i].Attendance.ForEach( att =>
                        {
                            var schedule = scheduleSortOrders.FirstOrDefault( s => s.Id == att.ScheduleId );
                            att.ScheduleStart = RockDateTime.Now.StartOfDay().Add( schedule.StartTimeOfDay );
                        } );
                        data[i].Attendance = data[i].Attendance.OrderBy( s => s.ScheduleStart );
                    }
                    data = data.OrderBy( d => d.Date ).ToList();

                    BuildControl();
                }

            }
        }

        private void BuildControl()
        {
            if (data.Count() == 0)
            {
                return;
            }
            scheduleLocations = data
                .SelectMany( d => d.Attendance.Select( s => new ScheduleLocations { ScheduleId = s.ScheduleId, Schedule = s.Schedule, ScheduleStart = s.ScheduleStart } ) )
                .GroupBy( d => new { d.ScheduleId, d.Schedule, d.ScheduleStart } ) //Distinct() won't work here, group and select first to get distinct results
                .Select( d => d.First() )
                .OrderBy( d => d.ScheduleStart )
                .ToList();
            scheduleLocations.ForEach( s =>
            {
                s.Locations = data
                .SelectMany( d =>
                    d.Attendance.Where( sa => sa.ScheduleId == s.ScheduleId )
                    .SelectMany( sa => sa.Attendance
                    .Select( la => new AttendanceLocation { LocationId = la.LocationId, Location = la.Location, Order = la.Order.AsIntegerOrNull() } ) ) )
                .GroupBy( d => new { d.LocationId, d.Location, d.Order } ) //Distinct() won't work here, group and select first to get distinct results
                .Select( d => d.First() )
                .OrderBy( l => l.Order )
                .ToList();
            } );
            DateTime lastDate = data.Last().Date.EndOfDay();
            thresholdData = BuildThresholdData( scheduleLocations ).Where( t => t.Date <= lastDate ).ToList();

            var container = new HtmlGenericControl( "div" );
            container.AddCssClass( "att-container" );
            //Create Notes Row
            var notes = new HtmlGenericControl( "div" );
            notes.ID = "notes";
            notes.AddCssClass( "att-schedule-total" );
            var notesTitle = new HtmlGenericControl( "div" );
            notesTitle.AddCssClass( "att-location-title" );
            notesTitle.InnerText = "Notes";
            notes.Controls.Add( notesTitle );
            var dayTotal = new HtmlGenericControl( "div" );
            dayTotal.AddCssClass( "att-schedule-total" );
            var dayTotalTitle = new HtmlGenericControl( "div" );
            dayTotalTitle.AddCssClass( "att-location-title" );
            dayTotalTitle.InnerText = "Day's Total";
            dayTotal.Controls.Add( dayTotalTitle );
            var dayUniqueTotal = new HtmlGenericControl( "div" );
            dayUniqueTotal.AddCssClass( "att-schedule-total" );
            var dayUniqueTotalTitle = new HtmlGenericControl( "div" );
            dayUniqueTotalTitle.AddCssClass( "att-location-title" );
            dayUniqueTotalTitle.InnerText = "Day's Unique Total";
            dayUniqueTotal.Controls.Add( dayUniqueTotalTitle );
            var dailyTotalContainer = new HtmlGenericControl( "div" );
            dailyTotalContainer.AddCssClass( "att-schedule" );
            dailyTotalContainer.Controls.Add( dayTotal );
            dailyTotalContainer.Controls.Add( dayUniqueTotal );
            RockContext context = new RockContext();
            NoteService note_svc = new NoteService( context );
            AttendanceOccurrenceService ao_svc = new AttendanceOccurrenceService( context );

            for (int i = 0; i < scheduleLocations.Count(); i++)
            {
                var schedule = new HtmlGenericControl( "div" );
                schedule.ID = "schedule_" + scheduleLocations[i].ScheduleId;
                schedule.AddCssClass( "att-schedule" );
                var scheduleHeader = new HtmlGenericControl( "div" );
                scheduleHeader.AddCssClass( "att-schedule-header" );
                var scheduleTitle = new HtmlGenericControl( "div" );
                scheduleTitle.AddCssClass( "att-schedule-title" );
                scheduleTitle.InnerText = scheduleLocations[i].Schedule;
                scheduleHeader.Controls.Add( scheduleTitle );
                schedule.Controls.Add( scheduleHeader );

                var total = new HtmlGenericControl( "div" );
                total.ID = "total_schedule_" + scheduleLocations[i].ScheduleId;
                total.AddCssClass( "att-schedule-total" );
                var totalTitle = new HtmlGenericControl( "div" );
                totalTitle.AddCssClass( "att-location-title" );
                totalTitle.InnerText = "Total";
                total.Controls.Add( totalTitle );
                for (int k = 0; k < scheduleLocations[i].Locations.Count(); k++)
                {
                    var location = new HtmlGenericControl( "div" );
                    location.ID = "schedule_" + scheduleLocations[i].ScheduleId + "_location_" + scheduleLocations[i].Locations[k].LocationId;
                    location.AddCssClass( "att-location" );
                    var locTitle = new HtmlGenericControl( "div" );
                    locTitle.AddCssClass( "att-location-title" );
                    locTitle.InnerText = scheduleLocations[i].Locations[k].Location;
                    location.Controls.Add( locTitle );

                    //Add Data
                    for (int h = 0; h < thresholdData.Count(); h++)
                    {
                        var thresholdCol = new HtmlGenericControl( "div" );
                        thresholdCol.AddCssClass( "att-threshold" );
                        var currentThreshold = thresholdData[h].Thresholds.Where( st => st.ScheduleId == scheduleLocations[i].ScheduleId ).SelectMany( st => st.Thresholds ).Where( lt => lt.LocationId == scheduleLocations[i].Locations[k].LocationId ).FirstOrDefault();
                        if (currentThreshold != null)
                        {
                            thresholdCol.InnerText = currentThreshold.Threshold.ToString();
                            location.Controls.Add( thresholdCol );
                            if (k == 0)
                            {
                                var dateCol = new HtmlGenericControl( "div" );
                                dateCol.AddCssClass( "att-date-threshold" );
                                dateCol.InnerText = "Threshold";
                                scheduleHeader.Controls.Add( dateCol );
                                var totalCol = new HtmlGenericControl( "div" );
                                totalCol.AddCssClass( "att-total-threshold" );
                                totalCol.InnerText = "";
                                total.Controls.Add( totalCol );
                                if (i == 0)
                                {
                                    var noteCol = new HtmlGenericControl( "div" );
                                    noteCol.AddCssClass( "att-total-threshold" );
                                    noteCol.InnerText = "";
                                    notes.Controls.Add( noteCol );
                                    var dayTotalCol = new HtmlGenericControl( "div" );
                                    dayTotalCol.AddCssClass( "att-total-threshold" );
                                    dayTotalCol.InnerText = "";
                                    dayTotal.Controls.Add( dayTotalCol );
                                    var dayUniqueTotalCol = new HtmlGenericControl( "div" );
                                    dayUniqueTotalCol.AddCssClass( "att-total-threshold" );
                                    dayUniqueTotalCol.InnerText = "";
                                    dayUniqueTotal.Controls.Add( dayUniqueTotalCol );
                                }
                            }
                        }
                        var dataInThreshold = data.Where( d => d.Date >= thresholdData[h].Date && (h < (thresholdData.Count() - 1) ? d.Date < thresholdData[h + 1].Date : true) ).ToList();
                        for (int j = 0; j < dataInThreshold.Count(); j++)
                        {
                            if (k == 0)
                            {
                                var dateCol = new HtmlGenericControl( "div" );
                                dateCol.AddCssClass( "att-date" );
                                dateCol.InnerText = dataInThreshold[j].Date.ToString( "MM/dd/yy" );
                                scheduleHeader.Controls.Add( dateCol );
                                var totalCol = new HtmlGenericControl( "div" );
                                totalCol.AddCssClass( "att-total-data" );
                                totalCol.InnerText = dataInThreshold[j].Attendance.Where( sa => sa.ScheduleId == scheduleLocations[i].ScheduleId ).Sum( a => a.Attendance.Sum( la => la.Total ) ).ToString();
                                total.Controls.Add( totalCol );
                                if (i == 0)
                                {
                                    DateTime checkDate = dataInThreshold[j].Date;
                                    var occurrencesOnDate = ao_svc.Queryable().Where( ao => ao.OccurrenceDate == checkDate );
                                    //Find any existing notes for date
                                    if (noteType != null)
                                    {
                                        var notesForDate = note_svc.Queryable().Where( n => n.NoteTypeId == noteType.Id ).Join( occurrencesOnDate,
                                            n => n.EntityId,
                                            ao => ao.Id,
                                            ( n, ao ) => n
                                        );
                                        int? entityId = null;
                                        if (notesForDate.Count() > 0)
                                        {
                                            entityId = notesForDate.First().EntityId;
                                        }
                                        else
                                        {
                                            List<int?> scheduleIds = scheduleLocations.Select( sl => sl.ScheduleId ).ToList();
                                            entityId = occurrencesOnDate.FirstOrDefault( ao => scheduleIds.Contains( ao.ScheduleId ) ).Id;
                                        }
                                        if (entityId.HasValue)
                                        {
                                            var noteCol = new HtmlGenericControl( "div" );
                                            noteCol.AddCssClass( "att-total-data" );
                                            noteCol.InnerHtml = $"<a class='add-note' href='/page/{notesPage.Id}?Id={entityId}'><i class='fa fa-sticky-note'></i></a>";
                                            notes.Controls.Add( noteCol );
                                        }
                                    }
                                    //Fill in Day's Total Info
                                    var totalCheckIns = occurrencesOnDate.Where( ao => groupTypeIds.Contains( ao.Group.GroupTypeId ) && ao.OccurrenceDate > stDate.SelectedDate.Value && (endDate.SelectedDate.HasValue ? ao.OccurrenceDate < endDate.SelectedDate.Value : true) && lbSchedules.SelectedValuesAsInt.Contains( ao.ScheduleId.Value ) ).SelectMany( ao => ao.Attendees ).Where( a => a.DidAttend.Value );
                                    var dayTotalCol = new HtmlGenericControl( "div" );
                                    dayTotalCol.AddCssClass( "att-total-data" );
                                    dayTotalCol.InnerText = totalCheckIns.Count().ToString();
                                    dayTotal.Controls.Add( dayTotalCol );
                                    var dayUniqueTotalCol = new HtmlGenericControl( "div" );
                                    dayUniqueTotalCol.AddCssClass( "att-total-data" );
                                    dayUniqueTotalCol.InnerText = totalCheckIns.Select( a => a.PersonAliasId ).Distinct().Count().ToString();
                                    dayUniqueTotal.Controls.Add( dayUniqueTotalCol );
                                }
                            }
                            var attendanceCol = new HtmlGenericControl( "div" );
                            attendanceCol.AddCssClass( "att-data" );
                            var attendanceData = dataInThreshold[j].Attendance.Where( sa => sa.ScheduleId == scheduleLocations[i].ScheduleId ).SelectMany( sa => sa.Attendance ).Where( la => la.LocationId == scheduleLocations[i].Locations[k].LocationId ).FirstOrDefault();
                            if (attendanceData != null)
                            {
                                if (currentThreshold == null)
                                {
                                    var existingThreshold = thresholdData.Where( td => td.Date < thresholdData[h].Date && td.Thresholds.Any( st => st.ScheduleId == scheduleLocations[i].ScheduleId && st.Thresholds.Any( lt => lt.LocationId == scheduleLocations[i].Locations[k].LocationId ) ) ).OrderByDescending( td => td.Date ).FirstOrDefault();
                                    if (existingThreshold != null)
                                    {
                                        currentThreshold = existingThreshold.Thresholds.FirstOrDefault( st => st.ScheduleId == scheduleLocations[i].ScheduleId ).Thresholds.FirstOrDefault( lt => lt.LocationId == scheduleLocations[i].Locations[k].LocationId );
                                    }
                                }
                                if (currentThreshold != null && attendanceData.Total >= currentThreshold.Threshold)
                                {
                                    attendanceCol.AddCssClass( "att-closed" );
                                }
                                //attendanceCol.InnerText = attendanceData.Total.ToString();
                                attendanceCol.InnerHtml = "<div data-content=\"" + String.Join( "", attendanceData.Attendance.Select( a => "<div class='row'><div class='col col-xs-8'>" + a.Group + "</div><div class='col col-xs-4'>" + a.Count + "</div></div>" ) ) + "\" onclick=\"displayGroups(this)\">" + attendanceData.Total + "</div>";
                                location.Controls.Add( attendanceCol );
                            }
                            else
                            {
                                attendanceCol.InnerText = "";
                                location.Controls.Add( attendanceCol );
                            }
                        }
                    }
                    schedule.Controls.Add( location );
                }
                schedule.Controls.Add( total );
                container.Controls.Add( schedule );
            }
            //Add Notes and Daily totals Rows
            container.Controls.Add( dailyTotalContainer );
            container.Controls.Add( notes );

            phContent.Controls.Add( container );
            phContent.Visible = true;
            DataContainer.Visible = true;
            btnExport.Visible = true;
        }

        /// <summary>
        /// Thresholds change over time due to building space, service times, and volunteer availablility
        /// Historical data was stored in a location attribute so we can display whether a room was "over threshold"
        /// at the particular time and not base it on current threshold data
        /// </summary>
        /// <param name="scheduleLocations">List of all locations used for each schedule in the attendance data</param>
        /// <returns></returns>
        private List<DateThreshold> BuildThresholdData( List<ScheduleLocations> scheduleLocations )
        {
            using (RockContext rockContext = new RockContext())
            {
                List<DateThreshold> thresholds = new List<DateThreshold>();
                DateTime startDate = data.First().Date;
                Guid? historicThresholdAttrGuid = GetAttributeValue( "LocationHistoricalThreshold" ).AsGuidOrNull();
                var locationIds = scheduleLocations.SelectMany( sl => sl.Locations.Select( al => al.LocationId.Value ) ).Distinct();
                var locations = new LocationService( rockContext ).Queryable().Join( locationIds,
                        l => l.Id,
                        locId => locId,
                        ( l, locId ) => l
                    ).ToList();
                Rock.Model.Attribute historicThresholdAttr;
                List<DateTime> dates = new List<DateTime>();
                List<HistoricThreshold> historicThresholds = new List<HistoricThreshold>();
                if (historicThresholdAttrGuid.HasValue)
                {
                    //Pull data into a list of <Date, Int> for threshold data by room
                    historicThresholdAttr = new AttributeService( rockContext ).Get( historicThresholdAttrGuid.Value );
                    historicThresholds = new AttributeValueService( rockContext ).Queryable().Where( av => av.AttributeId == historicThresholdAttr.Id )
                        .Join( locationIds,
                            av => av.EntityId,
                            locId => locId,
                            ( av, locId ) => av
                        ).ToList().Select( av =>
                            new HistoricThreshold
                            {
                                EntityId = av.EntityId,
                                Data = av.Value.Split( '|' ).Select( v => new HistoricThresholdData { Date = DateTime.Parse( v.Split( '^' ).First() ), Threshold = Int32.Parse( v.Split( '^' ).Last() ) } ).ToList()
                            }
                        ).ToList();
                    //Build Threshold data object and filter to dates relevant to the data we are looking at
                    dates = historicThresholds.SelectMany( t => t.Data.Select( d => d.Date ) ).Distinct().OrderBy( d => d.Date ).ToList();
                }
                if (dates.Count() > 0)
                {
                    var indexOfDatesAfterStart = dates.Count() - dates.Where( d => d > startDate ).Count() - 1;
                    if (indexOfDatesAfterStart < 0)
                    {
                        indexOfDatesAfterStart = 0;
                    }
                    for (var i = indexOfDatesAfterStart; i < dates.Count(); i++)
                    {
                        var threshold = new DateThreshold() { Date = dates[i] };
                        threshold.Thresholds = scheduleLocations.Select( sl =>
                        {
                            var locThreshold = from l in sl.Locations
                                               join ht in historicThresholds on l.LocationId equals ht.EntityId into lht
                                               from joinData in lht.DefaultIfEmpty()
                                               select new
                                               {
                                                   l,
                                                   ht = joinData
                                               };
                            return new ScheduleThreshold
                            {
                                ScheduleId = sl.ScheduleId,
                                Thresholds = locThreshold.Select( lt =>
                                {
                                    var t = lt.ht != null ? lt.ht.Data.FirstOrDefault( d => d.Date == dates[i] ) : null;
                                    int dateThreshold = 0;
                                    if (t != null)
                                    {
                                        dateThreshold = t.Threshold;
                                    }
                                    else
                                    {
                                        var idxOfThresholdDate = lt.ht != null ? lt.ht.Data.Count() - lt.ht.Data.Where( d => d.Date > dates[i] ).Count() - 1 : -1;
                                        if (idxOfThresholdDate >= 0)
                                        {
                                            dateThreshold = lt.ht.Data[idxOfThresholdDate].Threshold;
                                        }
                                        else
                                        {
                                            //Use Soft Room Threshold if historical data does not go back far enough
                                            var location = locations.FirstOrDefault( loc => loc.Id == lt.l.LocationId );
                                            if (location != null && location.SoftRoomThreshold.HasValue)
                                            {
                                                dateThreshold = location.SoftRoomThreshold.Value;
                                            }
                                        }
                                    }
                                    return new LocationThreshold { LocationId = lt.l.LocationId, Threshold = dateThreshold };
                                } ).Distinct().ToList()
                            };
                        } ).ToList();
                        thresholds.Add( threshold );
                    }
                }
                else
                {
                    //If the historical threshold attribute is not configured, the Soft Room Threshold on the location will be used for reference instead 
                    var threshold = new DateThreshold() { Date = startDate };
                    threshold.Thresholds = scheduleLocations.Select( sl => new ScheduleThreshold
                    {
                        ScheduleId = sl.ScheduleId,
                        Thresholds = sl.Locations.Join( locations, l => l.LocationId, loc => loc.Id, ( l, loc ) => new LocationThreshold
                        {
                            LocationId = l.LocationId,
                            Threshold = loc.SoftRoomThreshold
                        } ).ToList()
                    } ).ToList();
                    thresholds.Add( threshold );
                }
                return thresholds;
            }
        }

        #endregion
        [DebuggerDisplay( "Group: {Group}, Order: {Order}, Count: {Count}" )]
        private class GroupAttendance
        {
            public int? GroupId { get; set; }
            public string Group { get; set; }
            public int Order { get; set; }
            public int Count { get; set; }
        }
        [DebuggerDisplay( "Location: {Location}, Total: {Total}" )]
        private class LocationAttendance
        {
            public int? LocationId { get; set; }
            public string Location { get; set; }
            public string Order { get; set; }
            public int Total { get; set; }
            public IEnumerable<GroupAttendance> Attendance { get; set; }
        }
        [DebuggerDisplay( "Location: {Location}, Order: {Order}" )]
        private class AttendanceLocation
        {
            public int? LocationId { get; set; }
            public string Location { get; set; }
            public int? Order { get; set; }
        }
        [DebuggerDisplay( "Schedule: {Schedule}, ScheduleStart: {ScheduleStart}" )]
        private class ScheduleAttendance
        {
            public int? ScheduleId { get; set; }
            public string Schedule { get; set; }
            public DateTime ScheduleStart { get; set; }
            public IEnumerable<LocationAttendance> Attendance { get; set; }
        }
        [DebuggerDisplay( "Schedule: {Schedule}, ScheduleStart: {ScheduleStart}" )]
        private class ScheduleLocations
        {
            public int? ScheduleId { get; set; }
            public string Schedule { get; set; }
            public DateTime ScheduleStart { get; set; }
            public List<AttendanceLocation> Locations { get; set; }

        }
        [DebuggerDisplay( "Date: {Date}" )]
        private class DateAttendance
        {
            public DateTime Date { get; set; }
            public IEnumerable<ScheduleAttendance> Attendance { get; set; }
        }

        [DebuggerDisplay( "LocationId: {LocationId}, Threshold: {Threshold}" )]
        private class LocationThreshold
        {
            public int? LocationId { get; set; }
            public int? Threshold { get; set; }
        }
        private class ScheduleThreshold
        {
            public int? ScheduleId { get; set; }
            public List<LocationThreshold> Thresholds { get; set; }

        }
        private class DateThreshold
        {
            public DateTime Date { get; set; }
            public List<ScheduleThreshold> Thresholds { get; set; }
        }
        private class HistoricThreshold
        {
            public int? EntityId { get; set; }
            public List<HistoricThresholdData> Data { get; set; }
        }
        private class HistoricThresholdData
        {
            public DateTime Date { get; set; }
            public int Threshold { get; set; }
        }
    }
}