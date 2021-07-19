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

namespace RockWeb.Plugins.com_thecrossingchurch.Event
{
    /// <summary>
    /// Renders a particular calendar using Lava.
    /// </summary>
    [DisplayName( "Calendar Lava" )]
    [Category( "com_thecrossingchurch > Event" )]
    [Description( "Renders a particular calendar using Lava." )]

    [EventCalendarField( "Event Calendar", "The event calendar to be displayed", true, "8A444668-19AF-4417-9C74-09F842572974", order: 0 )]
    [LinkedPage( "Details Page", "Detail page for events", order: 1 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 2 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/Calendar.lava' %}", "", 3 )]

    public partial class CalendarLava : Rock.Web.UI.RockBlock
    {

        #region Properties
        private List<EventItem> CalendarEvents { get; set; }
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }
        private string Search { get; set; }
        private int CalendarId { get; set; }
        private RockContext rockContext { get; set; }
        private List<Guid> Audiences { get; set; }
        private static class PageParameterKey
        {
            public const string Search = "Search";
            public const string Start = "Start";
            public const string Audience = "Aud";
        }

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
            rockContext = new RockContext();
            DateTime today = RockDateTime.Now;
            StartDate = new DateTime( today.Year, today.Month, 1, 0, 0, 0 );
            EndDate = new DateTime( today.Year, ( today.Month + 1 ), 1, 0, 0, 0 );
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Start ) ) )
            {
                StartDate = DateTime.Parse( PageParameter( PageParameterKey.Start ) );
                StartDate = new DateTime( StartDate.Value.Year, StartDate.Value.Month, 1, 0, 0, 0 );
                EndDate = new DateTime( StartDate.Value.Year, ( StartDate.Value.Month + 1 ), 1, 0, 0, 0 );
            }
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Search ) ) )
            {
                Search = PageParameter( PageParameterKey.Search );
            }
            var eventCalendar = new EventCalendarService( new RockContext() ).Get( GetAttributeValue( "EventCalendar" ).AsGuid() );
            List<string> auds = PageParameter( PageParameterKey.Audience ).Split( ',' ).ToList();
            DefinedType audienceDT = new DefinedTypeService( rockContext ).Get( Guid.Parse( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE ) );
            Audiences = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == audienceDT.Id && auds.Contains( dv.Value ) ).Select( dv => dv.Guid ).ToList();
            if ( eventCalendar != null )
            {
                CalendarId = eventCalendar.Id;
            }
            LoadAllInRange();
        }

        #endregion

        #region Events



        #endregion

        #region Methods

        /// <summary>
        /// Load list of events occuring soon
        /// </summary>
        private void LoadAllInRange()
        {
            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

            // Grab events
            var qry = eventItemOccurrenceService
                    .Queryable( "EventItem, EventItem.EventItemAudiences,Schedule" )
                    .Where( m =>
                        m.EventItem.EventCalendarItems.Any( i => i.EventCalendarId == CalendarId ) &&
                        m.EventItem.IsActive &&
                        m.EventItem.IsApproved &&
                        ( Audiences.Count() == 0 || m.EventItem.EventItemAudiences.Any( a => Audiences.Contains( a.DefinedValue.Guid ) ) ) );

            // Get the occurrences
            var occurrences = qry.ToList();
            var occurrencesWithDates = occurrences
                .Select( o =>
                {
                    var eventOccurrenceDate = new EventOccurrenceDate
                    {
                        EventItemOccurrence = o
                    };

                    if ( o.Schedule != null )
                    {
                        eventOccurrenceDate.ScheduleOccurrences = o.Schedule.GetOccurrences( StartDate.Value, EndDate.Value ).ToList();
                    }
                    else
                    {
                        eventOccurrenceDate.ScheduleOccurrences = new List<Occurrence>();
                    }

                    return eventOccurrenceDate;
                } )
                .Where( d => d.ScheduleOccurrences.Any() )
                .ToList();

            //CalendarEventDates = new List<DateTime>();

            var eventOccurrenceSummaries = new List<EventOccurrenceSummary>();
            foreach ( var occurrenceDates in occurrencesWithDates )
            {
                var eventItemOccurrence = occurrenceDates.EventItemOccurrence;
                foreach ( var scheduleOccurrence in occurrenceDates.ScheduleOccurrences )
                {

                    var datetime = scheduleOccurrence.Period.StartTime.Value;
                    var occurrenceEndTime = scheduleOccurrence.Period.EndTime;

                    if ( datetime >= StartDate.Value && datetime < EndDate.Value )
                    {
                        if ( ( !String.IsNullOrEmpty( Search ) && eventItemOccurrence.EventItem.Name.ToLower().Contains( Search.ToLower() ) ) || String.IsNullOrEmpty( Search ) )
                        {
                            eventOccurrenceSummaries.Add( new EventOccurrenceSummary
                            {
                                EventItemOccurrence = eventItemOccurrence,
                                Name = eventItemOccurrence.EventItem.Name,
                                DateTime = datetime,
                                Date = datetime.ToShortDateString(),
                                Time = datetime.ToShortTimeString(),
                                EndDate = occurrenceEndTime != null ? occurrenceEndTime.Value.ToShortDateString() : null,
                                EndTime = occurrenceEndTime != null ? occurrenceEndTime.Value.ToShortTimeString() : null,
                                Campus = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                                Location = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                                LocationDescription = eventItemOccurrence.Location,
                                Description = eventItemOccurrence.EventItem.Description,
                                Summary = eventItemOccurrence.EventItem.Summary,
                                OccurrenceNote = eventItemOccurrence.Note.SanitizeHtml(),
                                DetailPage = string.IsNullOrWhiteSpace( eventItemOccurrence.EventItem.DetailsUrl ) ? null : eventItemOccurrence.EventItem.DetailsUrl,
                                PhotoPath = eventItemOccurrence.EventItem.Photo != null ? eventItemOccurrence.EventItem.Photo.Path.Substring( 1 ) : ""
                            } );
                        }
                    }
                }
            }

            var eventSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .GroupBy( e => e.Name )
                .Select( e => e.ToList() )
                .ToList();

            eventOccurrenceSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .ThenBy( e => e.Name )
                .ToList();

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "StartDate", StartDate.Value );
            mergeFields.Add( "EndDate", EndDate.Value );
            mergeFields.Add( "DetailsPage", LinkedPageRoute( "DetailsPage" ) );
            mergeFields.Add( "EventItems", eventSummaries );
            mergeFields.Add( "EventItemOccurrences", eventOccurrenceSummaries );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A class to store event item occurrence data for liquid
        /// </summary>
        [DotLiquid.LiquidType( "EventItemOccurrence", "DateTime", "Name", "Date", "Time", "EndDate", "EndTime", "Campus", "Location", "LocationDescription", "Description", "Summary", "OccurrenceNote", "DetailPage", "PhotoPath" )]
        public class EventOccurrenceSummary
        {
            public EventItemOccurrence EventItemOccurrence { get; set; }

            public DateTime DateTime { get; set; }

            public string Name { get; set; }

            public string Date { get; set; }

            public string Time { get; set; }

            public string EndDate { get; set; }

            public string EndTime { get; set; }

            public string PhotoPath { get; set; }
            public string Campus { get; set; }

            public string Location { get; set; }

            public string LocationDescription { get; set; }

            public string Summary { get; set; }

            public string Description { get; set; }

            public string OccurrenceNote { get; set; }

            public string DetailPage { get; set; }
        }

        /// <summary>
        /// A class to store the event item occurrences dates
        /// </summary>
        public class EventOccurrenceDate
        {
            public EventItemOccurrence EventItemOccurrence { get; set; }

            public List<Occurrence> ScheduleOccurrences { get; set; }
        }

        #endregion
    }
}
