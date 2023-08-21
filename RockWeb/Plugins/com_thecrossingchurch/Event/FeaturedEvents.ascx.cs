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
    [DisplayName( "Featured Events" )]
    [Category( "com_thecrossingchurch > Event" )]
    [Description( "Renders featured events slider." )]

    [EventCalendarField( "Event Calendar", "The event calendar to be displayed", true, "8A444668-19AF-4417-9C74-09F842572974", order: 0 )]
    [LinkedPage( "Details Page", "Detail page for events", order: 1 )]
    [AttributeField( name: "Priority Attribute", allowMultiple: false, entityTypeGuid: "71632E1A-1E7F-42B9-A630-EC99F375303A", order: 2 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 3 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/FeaturedEvents.lava' %}", "", 4 )]

    public partial class FeaturedEvents : Rock.Web.UI.RockBlock
    {

        #region Properties

        private List<EventItem> CalendarEvents { get; set; }
        private DateTime? StartDate { get; set; }
        private DateTime? EndDate { get; set; }
        private string Search { get; set; }
        private int CalendarId { get; set; }
        private List<Guid> Audiences { get; set; }
        private static class PageParameterKey
        {
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
            RockContext rockContext = new RockContext();
            DateTime today = RockDateTime.Now;
            StartDate = new DateTime( today.Year, today.Month, 1, 0, 0, 0 ).AddDays( -7 );
            var eventCalendar = new EventCalendarService( new RockContext() ).Get( GetAttributeValue( "EventCalendar" ).AsGuid() );
            if (!String.IsNullOrEmpty( PageParameter( PageParameterKey.Audience ) ))
            {
                List<string> auds = PageParameter( PageParameterKey.Audience ).Split( ',' ).ToList();
                DefinedType audienceDT = new DefinedTypeService( rockContext ).Get( Guid.Parse( Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE ) );
                Audiences = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == audienceDT.Id && auds.Contains( dv.Value ) ).Select( dv => dv.Guid ).ToList();
            }
            else
            {
                Audiences = new List<Guid>();
            }
            if (eventCalendar != null)
            {
                CalendarId = eventCalendar.Id;
            }
            LoadFeaturedEvents();
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
                                         (Audiences.Count() == 0 || m.EventItem.EventItemAudiences.Any( a => Audiences.Contains( a.DefinedValue.Guid ) ))
                            ).ToList()
                            .Where( m =>
                            {
                                m.LoadAttributes();
                                string featured = m.AttributeValues["FeaturedDates"].Value;
                                DateTime featuredStart;
                                DateTime featuredEnd;
                                if (!String.IsNullOrEmpty( featured ))
                                {
                                    featuredStart = DateTime.Parse( featured.Split( ',' ).First() );
                                    featuredEnd = DateTime.Parse( featured.Split( ',' ).Last() );
                                    if (DateTime.Compare( featuredStart, RockDateTime.Now ) <= 0 && DateTime.Compare( featuredEnd, RockDateTime.Now ) >= 0)
                                    {
                                        return true;
                                    }

                                }
                                return false;
                            }
                            ).ToList();
            if (events.Count() < 8)
            {
                //Add additional upcoming events to this list to fil out the carousel a bit more
                var ids = events.Select( e => e.Id ).ToList();
                var addEvents = eventSvc.Queryable( "EventItem" ).Where( m =>
                                         m.EventItem.EventCalendarItems.Any( i => i.EventCalendarId == CalendarId ) &&
                                         m.EventItem.IsActive &&
                                         m.EventItem.IsApproved &&
                                         !ids.Contains( m.Id ) &&
                                         (Audiences.Count() == 0 || m.EventItem.EventItemAudiences.Any( a => Audiences.Contains( a.DefinedValue.Guid ) ))
                            ).ToList().Where( m =>
                                        m.NextStartDateTime.HasValue &&
                                        DateTime.Compare( m.NextStartDateTime.Value, RockDateTime.Now ) >= 0
                            ).OrderBy( m => m.NextStartDateTime ).Take( 8 - events.Count() ).ToList();
                events.AddRange( addEvents );
            }
            //Sort Events by Priority
            Guid? priorityAttrGuid = GetAttributeValue( "PriorityAttribute" ).AsGuidOrNull();
            if (priorityAttrGuid.HasValue)
            {
                var priorityAttr = new AttributeService( _context ).Get( priorityAttrGuid.Value );
                events = events.OrderBy( e => e.AttributeValues.Where( av => av.Key == priorityAttr.Key ).FirstOrDefault().Value.SortValue ).ToList();
            }

            //If we are filtering by an audience, show only one featured event! 
            if (Audiences.Count() == 1)
            {
                events = events.Take( 1 ).ToList();
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "StartDate", StartDate.Value );
            mergeFields.Add( "DetailsPage", LinkedPageRoute( "DetailsPage" ) );
            mergeFields.Add( "EventItemOccurrences", events );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        #endregion
    }
}
