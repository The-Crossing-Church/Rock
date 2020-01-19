
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_thecrossingchurch.Checkin
{
    /// <summary>
    /// Block used to view current check-in counts and locations
    /// </summary>
    [DisplayName( "Closed Locations" )]
    [Category( "com_thecrossingchurch > Checkin" )]
    [Description( "Block used to view locations that have closed or are about to close." )]
    [GroupTypeField( "Check-in Type", "The Check-in Area.", false, "", "", 1, "GroupTypeTemplate", Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE )]
    [LinkedPage( "Location Detail Page")]
    public partial class ClosedLocations : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
                RefreshView();
            }

            string script = string.Format( @"
            $(function () {{
                Sys.WebForms.PageRequestManager.getInstance().add_pageLoading(function () {{
                    $.idleTimer('destroy');
                }});

                $.idleTimer(30000);
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
        /// Handles the Click event of the lbRefresh control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRefresh_Click( object sender, EventArgs e )
        {
            RefreshView();
        }

        #endregion

        #region Methods

        private void RefreshView()
        {
            var now = RockDateTime.Now;

            var locations = new List<LocationItem>();

            using ( var rockContext = new RockContext() )
            {
                // Get the group types
                //var groupTypeIds = new List<int>();
                //var groupTypeTemplateGuid = this.GetAttributeValue( "GroupTypeTemplate" ).AsGuidOrNull();
                //if ( groupTypeTemplateGuid.HasValue )
                //{
                //    var parentGroupType = GroupTypeCache.Get( groupTypeTemplateGuid.Value );
                //    if ( parentGroupType != null )
                //    {
                //        AddGroupType( parentGroupType, groupTypeIds );
                //    }
                //}

                var occurrenceService = new AttendanceOccurrenceService( rockContext );

                var schedules = new List<Schedule>();
                var activeSchedules = new List<Schedule>();

                // Loop through each schedule that is active and allows check-in
                foreach ( var schedule in new ScheduleService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( s => s.CheckInStartOffsetMinutes.HasValue && s.IsActive )
                    .ToList() )
                {
                    bool scheduleActive = schedule.WasScheduleActive( now );

                    // If the schedule is currently active or available for check-in
                    if ( scheduleActive || schedule.WasCheckInActive( now ) )
                    {
                        schedules.Add( schedule );
                        if ( scheduleActive )
                        {
                            activeSchedules.Add( schedule );
                        }

                        // Loop through the group/locations linked to that schedule
                        foreach ( var groupLocation in new GroupLocationService( rockContext )
                            .Queryable().AsNoTracking()
                            .Where( gl =>
                                gl.Schedules.Any( s => s.Id == schedule.Id ) &&
                                gl.Location != null &&
                                gl.Location.Name != null &&
                                gl.Location.Name != "" &&
                                gl.Group != null &&
                                gl.Group.GroupType != null &&
                                gl.Group.GroupType.TakesAttendance &&
                                gl.Group.IsActive 
                                // && groupTypeIds.Contains( gl.Group.GroupTypeId )
                                )
                            .ToList() )
                        {
                            var attendees = new List<int>();

                            // Check to see if there's an occurrence
                            var occurrence = occurrenceService.Get( now.Date, groupLocation.Group.Id, groupLocation.Location.Id, schedule.Id );
                            if ( occurrence != null )
                            {
                                // if so, get the attendance count
                                attendees = occurrence.Attendees
                                    .Where( a =>
                                        a.DidAttend.HasValue &&
                                        a.DidAttend.Value &&
                                        !a.EndDateTime.HasValue )
                                    .Select( a => a.PersonAlias.PersonId )
                                    .ToList();
                            }

                            // Add or update the location item
                            var locationItem = locations.FirstOrDefault( l => l.LocationId == groupLocation.LocationId );
                            if ( locationItem == null )
                            {
                                locationItem = new LocationItem
                                {
                                    LocationId = groupLocation.LocationId,
                                    LocationName = groupLocation.Location.Name,
                                    Threshold = groupLocation.Location.SoftRoomThreshold ?? int.MaxValue,
                                    IsClosed = !groupLocation.Location.IsActive,
                                    GroupTypeOrder = groupLocation.Group.GroupType.Order,
                                    GroupOrder = groupLocation.Group.Order,
                                    Attendees = new List<int>()
                                };
                                locations.Add( locationItem );
                            }

                            locationItem.IsCurrentlyActive = locationItem.IsCurrentlyActive || schedule.WasScheduleActive( now );
                            locationItem.Attendees.AddRange( attendees );
                        }
                    }
                }

                // Set Title
                var titleParts = new List<string>();
                var currentSchedule = schedules.OrderBy( s => s.StartTimeOfDay ).FirstOrDefault();
                if ( activeSchedules.Any() )
                {
                    currentSchedule = activeSchedules.OrderBy( s => s.StartTimeOfDay ).FirstOrDefault();
                }
                if ( currentSchedule != null )
                {
                    titleParts.Add( currentSchedule.Name );
                }
                titleParts.Add( string.Format( "{0}{1}", now.ToString( "MMM d" ),
                    ( now.Day % 10 == 1 && now.Day != 11 ) ? "st"
                    : ( now.Day % 10 == 2 && now.Day != 12 ) ? "nd"
                    : ( now.Day % 10 == 3 && now.Day != 13 ) ? "rd"
                    : "th" ) );
                lTitle.Text = titleParts.AsDelimited( " - " );
            }

            // Check to see if capacity has been reached
            foreach ( var location in locations )
            {
                location.Attendees = location.Attendees.Distinct().ToList();
                location.IsClosed = location.IsClosed || ( location.Attendees.Count >= location.Threshold );
            }

            // Show those locations that are closed, or near closing
            var fullLocations = locations
                .Where( l =>
                    l.IsClosed ||
                    ( l.Threshold - l.Attendance ) <= 3 )
                .OrderByDescending( l => l.IsCurrentlyActive )
                .ThenByDescending( l => l.IsClosed )
                .ThenBy( l => l.Remaining)
                .ThenBy( l => l.GroupTypeOrder )
                .ThenBy( l => l.GroupOrder )
                .ThenBy( l => l.LocationName )
                .ToList();

            if ( fullLocations.Any() )
            {
                nbAllOpen.Visible = false;
                rLocations.Visible = true;
                rLocations.DataSource = fullLocations;
                rLocations.DataBind();
            }
            else
            {
                nbAllOpen.Visible = true;
                rLocations.Visible = false;
            }
        }

        private void AddGroupType( GroupTypeCache groupType, List<int> groupTypeIds )
        {
            if ( groupType != null )
            {
                groupTypeIds.Add( groupType.Id );
                foreach( var childGroupType in groupType.ChildGroupTypes )
                {
                    AddGroupType( childGroupType, groupTypeIds );
                }
            }
        }

        protected string GetLocationLink( object obj )
        {
            int? locationId = obj as int?;
            if ( locationId.HasValue )
            {
                var qryParams = new Dictionary<string, string>();
                qryParams.Add( "Area", this.GetAttributeValue( "GroupTypeTemplate" ) );
                qryParams.Add( "Location", locationId.Value.ToString() );
                return LinkedPageUrl( "LocationDetailPage", qryParams );
            }

            return string.Empty;
        }

        #endregion

        protected class LocationItem
        {
            public int LocationId { get; set; }
            public bool IsCurrentlyActive { get; set; }
            public bool IsClosed { get; set; }
            public string LocationName { get; set; }
            public int Threshold { get; set; }
            public List<int> Attendees { get; set; }
            public int GroupTypeOrder { get; set; }
            public int GroupOrder { get; set; }

            public int Attendance
            {
                get
                {
                    return Attendees.Count();
                }
            }

            public int Remaining
            {
                get
                {
                    return Threshold - Attendance;
                }
            }

            public string Capacity
            {
                get
                {
                    if ( Threshold == int.MaxValue )
                    {
                        return Attendance.ToString( "N0" );
                    }
                    return string.Format( "{0:N0}/{1:N0}", Attendance, Threshold );
                }
            }
        }
    }
}