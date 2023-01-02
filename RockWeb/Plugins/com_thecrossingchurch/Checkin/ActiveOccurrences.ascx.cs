using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;


using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using Rock.CheckIn;
using System.Data.Entity;

namespace RockWeb.Plugins.com_thecrossingchurch.Checkin
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Active Occurrences" )]
    [Category( "com_thecrossingchurch > Checkin" )]
    [Description( "Block that can be used to view occurrences that are active at a given time" )]

    [LinkedPage("Location Page")]
    public partial class ActiveOccurrences : Rock.Web.UI.RockBlock
    {
        #region Setting Keys

        // Constant like string-key-settings that are tied to user saved filter preferences.
        const string ACTIVE_DATE = "Active Date";
        const string ATTENDANCE_AREA = "Attendance Area";
        const string ONLY_WITH_ATTENDEES = "Only Occurrences with Attendees";

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gOccurrences.DataKeyNames = new string[] { "Id" };
            gOccurrences.Actions.ShowAdd = false;
            gOccurrences.GridRebind += GOccurrences_GridRebind;

            RockPage.AddScriptLink( "~/Scripts/idle-timer.min.js" );

        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            string script = string.Format( @"
            $(function () {{
                Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function () {{
                    $.idleTimer('destroy');
                }});

                $.idleTimer(15000);
                $(document).bind('idle.idleTimer', function() {{
                    setTimeout(function() {{ {0} }}, 1);
                }});
            }});
            ", this.Page.ClientScript.GetPostBackEventReference( lbRefresh, "" ) );
            ScriptManager.RegisterStartupScript( Page, this.GetType(), "refresh", script, true );

        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the GOccurrences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void GOccurrences_GridRebind( object sender, GridRebindEventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfOccurrenceFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfOccurrenceFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfOccurrenceFilter.SaveUserPreference( ATTENDANCE_AREA, ddlAttendanceArea.SelectedValue );
            gfOccurrenceFilter.SaveUserPreference( ACTIVE_DATE, dtpActiveDate.SelectedDateTime.HasValue ? dtpActiveDate.SelectedDateTime.Value.ToString() : string.Empty );
            gfOccurrenceFilter.SaveUserPreference( ONLY_WITH_ATTENDEES, cbOnlyWithAttendees.Checked.ToString() );

            BindGrid();
        }

        /// <summary>
        /// Gfs the occurrence filter display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void gfOccurrenceFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case ATTENDANCE_AREA:
                    int? groupTypeId = e.Value.AsIntegerOrNull();
                    if ( groupTypeId.HasValue )
                    {
                        var groupType = GroupTypeCache.Get( groupTypeId.Value );
                        e.Value = groupType != null ? groupType.Name : string.Empty;
                    }
                    break;
                case ACTIVE_DATE:
                    break;
                case ONLY_WITH_ATTENDEES:
                    bool? onlyWithAttendees = e.Value.AsBooleanOrNull();
                    e.Value = onlyWithAttendees.HasValue && onlyWithAttendees.Value ? "Yes" : string.Empty;
                    break;
                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfOccurrenceFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfOccurrenceFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfOccurrenceFilter.DeleteUserPreferences();

            BindFilter();
        }

        /// <summary>
        /// Handles the Click event of the lbRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            // Bind attendance area dropdown
            ddlAttendanceArea.Items.Clear();
            ddlAttendanceArea.Items.Add( new ListItem() );
            var checkInTemplatePurposeGuid = Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid();
            using ( var rockContext = new RockContext() )
            {
                foreach( var groupType in new GroupTypeService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( t =>
                        t.GroupTypePurposeValue != null &&
                        t.GroupTypePurposeValue.Guid == checkInTemplatePurposeGuid )
                    .OrderBy( t => t.Order )
                    .Select( t => new
                    {
                        t.Name,
                        t.Id
                    } )
                    .ToList() )
                {
                    ddlAttendanceArea.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
                }
            }

            // Set filter values from user preferences
            ddlAttendanceArea.SetValue( gfOccurrenceFilter.GetUserPreference( ATTENDANCE_AREA ).AsIntegerOrNull() );
            dtpActiveDate.SelectedDateTime = gfOccurrenceFilter.GetUserPreference( ACTIVE_DATE ).AsDateTime();
            cbOnlyWithAttendees.Checked = gfOccurrenceFilter.GetUserPreference( ONLY_WITH_ATTENDEES ).AsBoolean();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var qryParams = new Dictionary<string, string>
            {
                { "GroupType", "_0_" },
                { "Group", "_1_" },
                { "Location", "_2_" }
            };
            var hyperLinkCol = gOccurrences.ColumnsOfType<HyperLinkField>().First();
            hyperLinkCol.DataNavigateUrlFormatString = LinkedPageUrl( "LocationPage", qryParams )
                .Replace( "_0_", "{0}" )
                .Replace( "_1_", "{1}" )
                .Replace( "_2_", "{2}" );

            // Get the filter values
            DateTime when = gfOccurrenceFilter.GetUserPreference( ACTIVE_DATE ).AsDateTime() ?? RockDateTime.Now;
            int? attendanceArea = gfOccurrenceFilter.GetUserPreference( ATTENDANCE_AREA ).AsIntegerOrNull();
            bool activeOnly = gfOccurrenceFilter.GetUserPreference( ONLY_WITH_ATTENDEES ).AsBoolean();

            var rockContext = new RockContext();

            // If an attendance area was selected, find all the group types (areas) in that attendance area
            var groupTypeIds = new List<int>();
            if ( attendanceArea.HasValue )
            {
                groupTypeIds = new GroupTypeService( rockContext )
                    .GetAllAssociatedDescendents( attendanceArea.Value )
                    .Select( t => t.Id )
                    .ToList();
            }

            // Start a list to store all the matching occurrences
            var occurrences = new List<OccurrenceItem>();

            // Loop through each schedule that is active and allows check-in
            foreach ( var schedule in new ScheduleService( rockContext )
                .Queryable().AsNoTracking()
                .Where( s => s.CheckInStartOffsetMinutes.HasValue && s.IsActive )
                .ToList() )
            {
                if ( schedule.WasCheckInActive( when ) || ( when.TimeOfDay.Ticks == 0 && schedule.GetICalOccurrences( when.Date ).Any() ) )
                {
                    // Get the start/end times for the schedule
                    var start = when;
                    var end = when;
                    var calEvent = schedule.GetICalEvent();
                    if ( calEvent != null && calEvent.DtStart != null )
                    {
                        start = when.Date.Add( calEvent.DtStart.Value.TimeOfDay );
                        end = when.Date.Add( calEvent.DtEnd.Value.TimeOfDay );
                    }

                    // Start a query for the group/locations linked to that schedule
                    var groupLocationQry = new GroupLocationService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( gl =>
                            gl.Schedules.Any( s => s.Id == schedule.Id ) &&
                            gl.Location != null &&
                            gl.Location.Name != null &&
                            gl.Location.Name != "" &&
                            gl.Group != null &&
                            gl.Group.GroupType != null &&
                            gl.Group.GroupType.TakesAttendance &&
                            gl.Group.IsActive );

                    // If filtering by attendance area, limit the list to groups in that area
                    if ( attendanceArea.HasValue )
                    {
                        groupLocationQry = groupLocationQry
                            .Where( gl => groupTypeIds.Contains( gl.Group.GroupTypeId ) );
                    }

                    // Loop through each group location
                    foreach ( var groupLocation in groupLocationQry.ToList() )
                    {
                        using ( var occContext = new RockContext() )
                        {
                            var occurrenceService = new AttendanceOccurrenceService( occContext );

                            // Check to see if there's already an occurrence record for this group/location/schedule/date
                            var occurrence = occurrenceService.Get( when.Date,
                                groupLocation.Group.Id, groupLocation.Location.Id, schedule.Id );

                            // If not (and were including occurrences without any attendees))
                            if ( occurrence == null && !activeOnly )
                            {
                                // create one and save it
                                occurrence = new AttendanceOccurrence
                                {
                                    OccurrenceDate = when.Date,
                                    GroupId = groupLocation.Group.Id,
                                    LocationId = groupLocation.Location.Id,
                                    ScheduleId = schedule.Id
                                };
                                occurrenceService.Add( occurrence );
                                occContext.SaveChanges();

                                // and then query for it again
                                occurrence = occurrenceService.Get( when.Date,
                                    groupLocation.Group.Id, groupLocation.Location.Id, schedule.Id );
                            }

                            // If we have an occurrence 
                            if ( occurrence != null )
                            {
                                // and we're either including all, or that occurrence has attendees
                                if ( !cbOnlyWithAttendees.Checked || occurrence.Attendees.Any() )
                                {
                                    // Add the occurrence to the list
                                    occurrences.Add( new OccurrenceItem
                                    {
                                        Id = occurrence.Id,
                                        GroupName = groupLocation.Group.Name,
                                        Attendees = occurrence.Attendees.Count(),
                                        ScheduleName = schedule.Name,
                                        CheckInStart = start.AddMinutes( 0 - ( schedule.CheckInStartOffsetMinutes ?? 0 ) ),
                                        Start = start,
                                        End = end,
                                        SoftThreshold = groupLocation.Location.SoftRoomThreshold,
                                        HardThreshold = groupLocation.Location.FirmRoomThreshold,
                                        Location = groupLocation.Location.Name,
                                        GroupTypeId = groupLocation.Group.GroupTypeId,
                                        GroupId = groupLocation.GroupId,
                                        LocationId = groupLocation.LocationId
                                    } );
                                }
                            }
                        }
                    }
                }
            }

            // Sort the list
            var sortProperty = gOccurrences.SortProperty;
            if ( sortProperty != null )
            {
                occurrences = occurrences.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                occurrences = occurrences.OrderBy( o => o.Start ).ThenBy( o => o.GroupName ).ThenBy( o => o.Location ).ToList();
            }

            lOccurrenceCount.Text = occurrences.Count.ToString( "N0" );

            // Bind the grid to our list
            gOccurrences.DataSource = occurrences;
            gOccurrences.DataBind();


        }

        #endregion

        /// <summary>
        /// Helper class used to bind to the grid
        /// </summary>
        public class OccurrenceItem
        {
            public int Id { get; set; }
            public string GroupName { get; set; }
            public int Attendees { get; set; }
            public string ScheduleName { get; set; }
            public DateTime CheckInStart { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public int? SoftThreshold { get; set; }
            public int? HardThreshold { get; set; }
            public string Location { get; set; }
            public int? GroupTypeId { get; set; }
            public int? GroupId { get; set; }
            public int? LocationId { get; set; }

            public string Threshold
            {
                get
                {
                    return string.Format( "{0} / {1}",
                        SoftThreshold.HasValue ? SoftThreshold.Value.ToString() : "-",
                        HardThreshold.HasValue ? HardThreshold.Value.ToString() : "-"
                    );
                }
            }
        }

    }
}