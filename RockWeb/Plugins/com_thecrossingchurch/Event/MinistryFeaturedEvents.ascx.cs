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
    [DisplayName( "Featured Ministry Events" )]
    [Category( "com_thecrossingchurch > Event" )]
    [Description( "Renders featured ministry events slider." )]

    [EventCalendarField( "Event Calendar", "The event calendar to be displayed", true, "8A444668-19AF-4417-9C74-09F842572974", order: 0 )]
    [LinkedPage( "Details Page", "Detail page for events", order: 1 )]
    [DefinedValueField( "Audiences", Description = "The audiences for this ministry", Key = "Audiences", DefinedTypeGuid = Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, AllowMultiple = true, Order = 2 )]
    [DefinedValueField( "Additional Audiences", Description = "Additional audiences to use when going to all events", Key = "AdditionalAudiences", DefinedTypeGuid = Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, AllowMultiple = true, IsRequired = false, Order = 3 )]
    [DefinedValueField( "Ministry", Key = "Ministry", DefinedTypeGuid = "c5696677 -82e5-4329-a8c2-2b006f589636", AllowMultiple = false, IsRequired = false, Order = 4 )]
    [BooleanField( "Upcoming Events Only", "Selecting this option will only show upcoming events, deselecting it will include events currently being featured on the main calendar page", defaultValue: false, order: 5 )]
    [IntegerField( "Limit", "Max number of events to display", required: false, order: 6 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 7 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/MinistryFeaturedEvents.lava' %}", "", 8 )]

    public partial class MinistryFeaturedEvents : Rock.Web.UI.RockBlock
    {

        #region Properties

        private List<EventItem> CalendarEvents { get; set; }
        private DefinedValue Ministry { get; set; }
        private bool UpcomingOnly { get; set; }
        private int? Limit { get; set; }
        private List<Guid> Audiences { get; set; }
        private List<Guid> AdditionalAudiences { get; set; }
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }
        private string Search { get; set; }
        private int CalendarId { get; set; }

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
            DateTime today = RockDateTime.Now;
            StartDate = RockDateTime.Now;
            EndDate = StartDate.Value.AddDays( 21 );
            var eventCalendar = new EventCalendarService( new RockContext() ).Get( GetAttributeValue( "EventCalendar" ).AsGuid() );
            if ( !String.IsNullOrEmpty( GetAttributeValue( "Ministry" ) ) )
            {
                Ministry = new DefinedValueService( new RockContext() ).Get( GetAttributeValue( "Ministry" ).AsGuid() );
            }
            UpcomingOnly = GetAttributeValue( "UpcomingEventsOnly" ).AsBoolean();
            Limit = GetAttributeValue( "Limit" ).AsIntegerOrNull();
            Audiences = GetAttributeValue( "Audiences" ).SplitDelimitedValues( true ).AsGuidList();
            AdditionalAudiences = GetAttributeValue( "AdditionalAudiences" ).SplitDelimitedValues( true ).AsGuidList();
            if ( eventCalendar != null )
            {
                CalendarId = eventCalendar.Id;
            }
            if ( Audiences.Count() > 0 && eventCalendar != null )
            {
                LoadFeaturedEvents();
            }
        }

        #endregion

        #region Methods

        private void LoadFeaturedEvents()
        {
            var _context = new RockContext();
            var eventSvc = new EventItemOccurrenceService( _context );
            var events = eventSvc.Queryable( "EventItem" ).Where( m =>
                                         m.EventItem.EventCalendarItems.Any( i => i.EventCalendarId == CalendarId ) &&
                                         m.EventItem.IsActive &&
                                         m.EventItem.IsApproved &&
                                         m.EventItem.EventItemAudiences.Count() > 0 &&
                                         m.EventItem.EventItemAudiences.Any( a => Audiences.Contains( a.DefinedValue.Guid ) )
                            ).ToList()
                            .Where( m =>
                            {
                                m.LoadAttributes();
                                if ( UpcomingOnly == false )
                                {
                                    string featured = m.AttributeValues["FeaturedDates"].Value;
                                    DateTime featuredStart;
                                    DateTime featuredEnd;
                                    if ( !String.IsNullOrEmpty( featured ) )
                                    {
                                        featuredStart = DateTime.Parse( featured.Split( ',' ).First() );
                                        featuredEnd = DateTime.Parse( featured.Split( ',' ).Last() );
                                        if ( DateTime.Compare( featuredStart, RockDateTime.Now ) <= 0 && DateTime.Compare( featuredEnd, RockDateTime.Now ) >= 0 )
                                        {
                                            m.AttributeValues.Add( "_IsFeatured", new AttributeValueCache() { Value = "true" } );
                                            return true;
                                        }

                                    }
                                }
                                DateTime? nextDate = m.NextStartDateTime;
                                if ( nextDate.HasValue && DateTime.Compare( RockDateTime.Now, nextDate.Value ) <= 0 )
                                {
                                    return true;
                                }
                                return false;
                            }
                            ).ToList();
            if ( Limit.HasValue )
            {
                events = events.Take( Limit.Value ).ToList();
            }
            events = events.OrderBy( e => e.NextStartDateTime ).ToList();
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "StartDate", StartDate.Value );
            mergeFields.Add( "DetailsPage", LinkedPageRoute( "DetailsPage" ) );
            mergeFields.Add( "EventItemOccurrences", events );
            mergeFields.Add( "CurrentPerson", CurrentPerson );
            if ( Ministry != null )
            {
                mergeFields.Add( "Ministry", Ministry.Value );
            }
            List<DefinedValue> audiences = new DefinedValueService( _context ).GetByGuids( Audiences ).ToList();
            List<DefinedValue> addAudiences = new DefinedValueService( _context ).GetByGuids( AdditionalAudiences ).ToList();
            mergeFields.Add( "Audiences", audiences );
            var combined = audiences;
            combined.AddRange( addAudiences );
            mergeFields.Add( "AdditionalAudiences", combined.Distinct() );

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
