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

namespace RockWeb.Plugins.rocks_pillars.Event
{
    /// <summary>
    /// Renders a particular calendar using Lava.
    /// </summary>
    [DisplayName( "All Church Calendar Lava" )]
    [Category( "Pillars > Event" )]
    [Description( "Renders an all church calendar using Lava." )]

    //[EventCalendarField( "Event Calendar", "The event calendar to be displayed", true, "8A444668-19AF-4417-9C74-09F842572974", order: 0 )]
    [CustomDropdownListField( "Default View Option", "Determines the default view option", "Day,Week,Month", true, "Week", order: 1 )]
    [LinkedPage( "Details Page", "Detail page for events", order: 2 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 2 )]
    [LinkedPage("Event Add Page", "Event Add page", order: 2)]

    [CampusesField(name: "Campuses", description: "Select campuses to display calendar events for. No selection will show all.", required: false, defaultCampusGuids: "", category: "", order: 3, key: "Campuses")]
    [CustomRadioListField( "Campus Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", order: 4 )]

    [CustomRadioListField( "Audience Filter Display Mode", "", "1^Hidden, 2^Plain, 3^Panel Open, 4^Panel Closed", true, "1", key: "CategoryFilterDisplayMode", order: 5 )]
    //[DefinedValueField("cb22f54e-b3e8-4b62-a54b-570e68cb9ee9", "Filter By Ministry", "Determines which ministry should be displayed in the filter.", false, true, key: "FilterMinistry", order: 6 )]
    [BooleanField( "Show Date Range Filter", "Determines whether the date range filters are shown", false, order: 7 )]

    [BooleanField( "Show Small Calendar", "Determines whether the calendar widget is shown", true, order: 8 )]
    [BooleanField( "Show Day View", "Determines whether the day view option is shown", false, order: 9 )]
    [BooleanField( "Show Week View", "Determines whether the week view option is shown", true, order: 10 )]
    [BooleanField( "Show Month View", "Determines whether the month view option is shown", true, order: 11 )]

    [BooleanField( "Enable Campus Context", "If the page has a campus context its value will be used as a filter", order: 12 )]
    [CodeEditorField( "Month Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/Calendar2.lava' %}", "", 13 )]
    [CodeEditorField( "Week Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"", "", 14)]
    [CodeEditorField( "Day Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"", "", 15)]

    [DayOfWeekField( "Start of Week Day", "Determines what day is the start of a week.", true, DayOfWeek.Sunday, order: 14 )]

    //[BooleanField( "Set Page Title", "Determines if the block should set the page title with the calendar name.", false, order: 15 )]

    public partial class AllChurchCalendarLava : Rock.Web.UI.RockBlock
    {
        #region Fields

        private DayOfWeek _firstDayOfWeek = DayOfWeek.Sunday;

        protected bool CampusPanelOpen { get; set; }

        protected bool CampusPanelClosed { get; set; }

        protected bool CategoryPanelOpen { get; set; }

        protected bool CategoryPanelClosed { get; set; }

        #endregion

        #region Properties

        private string ViewMode { get; set; }

        //Different from start date for Monthly View Calendar because we'll see days from the prev month possibly
        //so we'll want those events on the calendar as well
        private DateTime FilterStartDate { get; set; }

        //Different from end date for Monthly View Calendar because we'll see days from the next month possibly
        //so we'll want those events on the calendar as well
        private DateTime FilterEndDate { get; set; }

        private DateTime StartDate { get; set; }

        private DateTime EndDate { get; set; }

        private List<DateTime> CalendarEventDates { get; set; }

        #endregion

        #region Base ControlMethods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _firstDayOfWeek = GetAttributeValue( "StartofWeekDay" ).ConvertToEnum<DayOfWeek>();

            CampusPanelOpen = GetAttributeValue( "CampusFilterDisplayMode" ) == "3";
            CampusPanelClosed = GetAttributeValue( "CampusFilterDisplayMode" ) == "4";
            CategoryPanelOpen = GetAttributeValue( "CategoryFilterDisplayMode" ) == "3";
            CategoryPanelClosed = GetAttributeValue( "CategoryFilterDisplayMode" ) == "4";

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

            nbMessage.Visible = false;

            if ( !Page.IsPostBack )
            {
                if ( SetFilterControls() )
                {
                    pnlDetails.Visible = true;
                    SetViewModeFromQueryString();
                    BindData(GetDateFromQueryString());
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }
        private DateTime GetDateFromQueryString()
        {
            DateTime res = RockDateTime.Today;
            var date = PageParameter("Date");
            if(date != string.Empty)
            {
                res = DateTime.Parse(date);
            }

            return res;
        }

        private void SetViewModeFromQueryString()
        {
            var viewMode = PageParameter("ViewMode");
            if ( viewMode.IsNullOrWhiteSpace() )
            {
                viewMode = GetAttributeValue( "DefaultViewOption" );
            }

            if(viewMode == "Week")
            {
                ViewMode = viewMode;
            }
            else if(viewMode == "Day")
            {
                ViewMode = viewMode;
            }
            else
            {
                ViewMode = "Month";
            }
        }

        private List<string> GetCampusIdsFromQueryString()
        {
            var campuses = PageParameter("CampusIds");

            return campuses.Split(',').Where(r => r != string.Empty).ToList();
        }

        private string SetCampusIdsForQueryString(List<string> campuses)
        {
            if (campuses.Count() > 0)
            {
                return string.Format("&CampusIds={0}", string.Join(",", campuses));
            }

            return string.Empty;
        }

        private List<string> GetMinistryIdsFromQueryString()
        {
            var campuses = PageParameter("MinistryIds");

            return campuses.Split(',').Where(r => r != string.Empty).ToList();
        }

        private string SetMinistryIdsForQueryString(List<string> ministries)
        {
            if (ministries.Count() > 0)
            {
                return string.Format("&MinistryIds={0}", string.Join(",", ministries));
            }

            return string.Empty;
        }

        protected override void OnPreRender( EventArgs e )
        {
            //if ( GetAttributeValue( "SetPageTitle" ).AsBoolean() &&  )
            //{
            //    string pageTitle = "All Church Calendar";
            //    RockPage.PageTitle = pageTitle;
            //    RockPage.BrowserTitle = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
            //    RockPage.Header.Title = string.Format( "{0} | {1}", pageTitle, RockPage.Site.Name );
            //}

            btnDay.CssClass = "btn btn-default" + ( ViewMode == "Day" ? " active" : string.Empty );
            btnWeek.CssClass = "btn btn-default" + ( ViewMode == "Week" ? " active" : string.Empty );
            btnMonth.CssClass = "btn btn-default" + ( ViewMode == "Month" ? " active" : string.Empty );

            base.OnPreRender( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            if ( SetFilterControls() )
            {
                pnlDetails.Visible = true;
                SetFilterControls();
                SetViewModeFromQueryString();
                BindData(GetDateFromQueryString());
            }
            else
            {
                pnlDetails.Visible = false;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectionChanged event of the calEventCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void calEventCalendar_SelectionChanged( object sender, EventArgs e )
        {
            SetViewModeFromQueryString();
            var date = calEventCalendar.SelectedDate;
            ResetCalendarSelection(date);
            RedirectUrl(StartDate.ToShortDateString().Replace('/', '-'));
        }

        /// <summary>
        /// Handles the DayRender event of the calEventCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DayRenderEventArgs"/> instance containing the event data.</param>
        protected void calEventCalendar_DayRender( object sender, DayRenderEventArgs e )
        {
            DateTime day = e.Day.Date;
            if ( CalendarEventDates != null && CalendarEventDates.Any( d => d.Date.Equals( day.Date ) ) )
            {
                e.Cell.AddCssClass( "calendar-hasevent" );
            }
        }

        /// <summary>
        /// Handles the VisibleMonthChanged event of the calEventCalendar control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MonthChangedEventArgs"/> instance containing the event data.</param>
        protected void calEventCalendar_VisibleMonthChanged( object sender, MonthChangedEventArgs e )
        {
            SetViewModeFromQueryString();
            var date = e.NewDate;
            ResetCalendarSelection(date);
            RedirectUrl(StartDate.ToShortDateString().Replace('/', '-'));
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCampus control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCampus_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetViewModeFromQueryString();
            var date = calEventCalendar.SelectedDate;
            ResetCalendarSelection(date);
            var campuses = SetCampusIdsForQueryString(cblCampus.Items.OfType<ListItem>().Where(i => i.Selected).Select(i => i.Value).ToList());
            RedirectUrl(StartDate.ToShortDateString().Replace('/', '-'), campuses);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cblCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cblCategory_SelectedIndexChanged( object sender, EventArgs e )
        {
            SetViewModeFromQueryString();
            var date = calEventCalendar.SelectedDate;
            ResetCalendarSelection(date);
            var ministries = SetMinistryIdsForQueryString(cblCategory.Items.OfType<ListItem>().Where(i => i.Selected).Select(i => i.Value.Split(',').First()).ToList());
            RedirectUrl(StartDate.ToShortDateString().Replace('/', '-'), null, ministries);
        }

        /// <summary>
        /// Handles the Click event of the btnWeek control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnViewMode_Click( object sender, EventArgs e )
        {
            var btnViewMode = sender as BootstrapButton;
            if ( btnViewMode != null )
            {
                ViewMode = btnViewMode.Text;
                var date = calEventCalendar.SelectedDate;
                ResetCalendarSelection(date);
                RedirectUrl(StartDate.ToShortDateString().Replace('/', '-'));
            }
        }

        private void RedirectUrl(string date, string campuses = null, string ministries = null)
        {
            string url;
            string view;

            campuses = campuses ?? SetCampusIdsForQueryString(GetCampusIdsFromQueryString());
            ministries = ministries ?? SetMinistryIdsForQueryString(GetMinistryIdsFromQueryString());

            switch(ViewMode)
            {
                case "Day":
                    view = "Day";
                    break;

                case "Week":
                    view = "Week";
                    break;

                default:
                case "Month":
                    view = "Month";
                    break;
            }

            url = ResolveUrl(string.Format("~/{0}?ViewMode={1}&Date={2}{3}{4}",
                            CurrentPageReference.BuildUrl().Split('?').First(),
                            view,
                            date,
                            campuses,
                            ministries));
                Response.Redirect(url, false);
                Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnPrev_Click(object sender, EventArgs e)
        {
            SetViewModeFromQueryString();
            var date = calEventCalendar.SelectedDate.AddDays(-1);
            ResetCalendarSelection(date);
            RedirectUrl(StartDate.ToShortDateString().Replace('/', '-'));
        }

        protected void btnNext_Click(object sender, EventArgs e)
        {
            SetViewModeFromQueryString();
            var lastDate = calEventCalendar.SelectedDates.Count - 1;
            var date = calEventCalendar.SelectedDates[lastDate].AddDays(1);
            ResetCalendarSelection(date);
            RedirectUrl(StartDate.ToShortDateString().Replace('/', '-'));
        }

        protected void btnAddEvent_Click(object sender, EventArgs e)
        {
            NavigateToLinkedPage("EventAddPage");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads and displays the event item occurrences
        /// </summary>
        private void BindData(DateTime date)
        {
            var rockContext = new RockContext();
            var eventItemOccurrenceService = new EventItemOccurrenceService( rockContext );

            ResetCalendarSelection(date);

            // Grab events
            var qry = eventItemOccurrenceService
                    .Queryable( "EventItem, EventItem.EventItemAudiences,Schedule" )
                    .Where( m =>
                        m.EventItem.IsActive &&
                        m.EventItem.IsApproved );

            // Get the beginning and end dates
            var beginDate = FilterStartDate;
            var endDate = FilterEndDate.AddDays(1).AddMilliseconds(-1);

            // Get the occurrences
            var occurrences = AddCampusFilter(qry).ToList();

            var occurrencesWithDates = occurrences
                .Select( o => new EventOccurrenceDate
                {
                    EventItemOccurrence = o,
                    Dates = o.GetStartTimes( beginDate, endDate ).ToList()
                } )
                .Where( d => d.Dates.Any() )
                .ToList();

            CalendarEventDates = new List<DateTime>();

            //var selectedMinistries = cblCategory.Items.OfType<ListItem>().Where(i => i.Selected).Select(i => i.Value.Split(',').Skip(1).First().AsGuid()).ToList();

            var eventOccurrenceSummaries = new List<EventOccurrenceSummary>();
            foreach ( var occurrenceDates in occurrencesWithDates )
            {
                var eventItemOccurrence = occurrenceDates.EventItemOccurrence;
                foreach ( var datetime in occurrenceDates.Dates )
                {
                    if ( eventItemOccurrence.Schedule.EffectiveEndDate.HasValue && ( eventItemOccurrence.Schedule.EffectiveStartDate != eventItemOccurrence.Schedule.EffectiveEndDate ) )
                    {
                        var multiDate = eventItemOccurrence.Schedule.EffectiveStartDate;
                        while ( multiDate.HasValue && ( multiDate.Value < eventItemOccurrence.Schedule.EffectiveEndDate.Value ) )
                        {
                            CalendarEventDates.Add( multiDate.Value.Date );
                            multiDate = multiDate.Value.AddDays( 1 );
                        }
                    }
                    else
                    {
                        CalendarEventDates.Add( datetime.Date );
                    }

                    eventItemOccurrence.EventItem.LoadAttributes();
                    var ministries = eventItemOccurrence.EventItem.GetAttributeValues("Ministry");

                    if ( datetime >= beginDate && datetime < endDate /*&& MinistryFilter(selectedMinistries, ministries) */)
                    {
                        var endTime = eventItemOccurrence.Schedule.GetOccurrences(datetime).First().Period.EndTime.ToString();
                        eventOccurrenceSummaries.Add(new EventOccurrenceSummary
                        {
                            EventItemOccurrence = eventItemOccurrence,
                            Name = eventItemOccurrence.EventItem.Name,
                            DateTime = datetime,
                            Date = datetime.ToShortDateString(),
                            Time = datetime.ToShortTimeString(),
                            EndTime = endTime,
                            End = Convert.ToDateTime(endTime),
                            Campus = eventItemOccurrence.Campus != null ? eventItemOccurrence.Campus.Name : "All Campuses",
                            Location = eventItemOccurrence.Location,
                            //Ministry = GetMinistries(ministries),
                            Color = GetColorFromCampus(rockContext, eventItemOccurrence.CampusId),
                            Blocked = false
                        });
                    }
                }
            }

            eventOccurrenceSummaries = eventOccurrenceSummaries
                .OrderBy( e => e.DateTime )
                .ThenBy( e => e.Name )
                .ToList();

            var days = GetDays(eventOccurrenceSummaries);

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "TimeFrame", ViewMode );
            mergeFields.Add( "StartDate", StartDate );
            mergeFields.Add( "EndDate", EndDate );
            mergeFields.Add( "DetailsPage", LinkedPageRoute( "DetailsPage" ) );
            mergeFields.Add("EventItemOccurrences", eventOccurrenceSummaries);
            mergeFields.Add( "DayEvents", days );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lOutput.Text = GetLavaTemplateBasedOnView().ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        private bool MinistryFilter(List<Guid> selected, List<string> ministries)
        {
            if(selected.Any())
            {
                return ministries.Any(m => selected.Contains(m.AsGuid()));
            }
            else
            {
                return true;
            }
        }

        private List<string> GetMinistries(List<string> ministryGuids)
        {
            List<string> min = new List<string>();

            if (ministryGuids.Any())
            {
                min = ministryGuids.Select(m => cblCategory.Items.OfType<ListItem>().Where(i => i.Value.Split(',').Skip(1).First() == m).Select(i => i.Text).First()).ToList();
            }

            return min;
        }

        private string GetColorFromCampus(RockContext context, int? cId)
        {
            string color = string.Empty;
            if (cId != null)
            {
                var service = new AttributeValueService(context);

                color = service.AsNoFilter().AsQueryable()
                            .Where(val => val.Attribute.Id == 21594 && val.EntityId == cId)
                            .Select(val => val.Value)
                            .FirstOrDefault();
            }
            else
            {
                color = "rgb(130, 130, 130)";
            }

            return color;
        }

        private string GetLavaTemplateBasedOnView()
        {
            if(ViewMode == "Week")
            {
                return GetAttributeValue("WeekTemplate");
            }
            else if(ViewMode == "Day")
            {
                return GetAttributeValue("DayTemplate");
            }
            else
            {
                return GetAttributeValue("MonthTemplate");
            }
        }

        private IQueryable<EventItemOccurrence> AddCampusFilter(IQueryable<EventItemOccurrence> qry)
        {
            // Filter by campus
            var campusesAllowedByBlockConfig = cblCampus.Items.OfType<ListItem>();

            if (campusesAllowedByBlockConfig.Any(l => l.Selected))
            {
                var selectedCampusIdList = campusesAllowedByBlockConfig.Where(l => l.Selected).Select(l => l.Value.AsInteger()).ToList();

                // No value gets them all, otherwise get the ones selected
                // Block level campus filtering has already been performed on cblCampus, so no need to do it again here
                // If CampusId is null, then the event is an 'All Campuses' event, so include those
                qry = qry.Where(c => !c.CampusId.HasValue || selectedCampusIdList.Contains(c.CampusId.Value));
            }
            else
            {
                var campuses = campusesAllowedByBlockConfig.Select(l => l.Value.AsInteger());
                // If no campus filter is selected then check the block filtering
                // If CampusId is null, then the event is an 'All Campuses' event, so include those
                qry = qry.Where(c => !c.CampusId.HasValue || campuses.Contains(c.CampusId.Value));
            }

            return qry;
        }

        private List<DayEvent> GetDays(List<EventOccurrenceSummary> eventOccurrences)
        {
            var days = new List<DayEvent>();
            
            var rockContext = new RockContext();
            var service = new ScheduleCategoryExclusionService(rockContext);

            var blockedDates = service.AsNoFilter().AsQueryable()
                                    .Where(d => d.StartDate <= FilterEndDate)
                                    .ToList();

            for(DateTime i = FilterStartDate; i <= FilterEndDate; i = i.AddDays(1))
            {
                var blockedHours = blockedDates.Where(b => b.StartDate <= i && b.EndDate > i || b.StartDate.ToShortDateString() == i.ToShortDateString()).ToList();
                var newDay = new DayEvent
                {
                    Day = i,
                    Date = i.ToShortDateString(),
                    EventItemOccurrenceSummaries = GetDayEvents(eventOccurrences, blockedHours, i),
                    FocusMonth = i.Month == StartDate.Month,
                    Hours = SetHours(),
                    DayLink = SetDayUrl(i)
                };

                newDay.Blocked = newDay.EventItemOccurrenceSummaries.All(e => e.Blocked);

                days.Add(newDay);
            }

            return days;
        }

        private List<EventOccurrenceSummary> GetDayEvents(List<EventOccurrenceSummary> eventOccurrences, List<ScheduleCategoryExclusion> blockedHours, DateTime date)
        {
            var events = new List<EventOccurrenceSummary>();

            events = eventOccurrences
                        .Where(e => e.Date == date.ToShortDateString() && !blockedHours.Any(b => b.StartDate <= e.DateTime && b.EndDate >= e.End))
                        .ToList();

            foreach(var blocked in blockedHours)
            {
                var endOfDay = date.AddDays(1).AddMilliseconds(-1);
                var endTime = blocked.EndDate > endOfDay ? endOfDay : blocked.EndDate;

                var startTime = blocked.StartDate < date ? date : blocked.StartDate;

                events.Add(new EventOccurrenceSummary
                {
                    Color = "rgb(255, 55, 55)",
                    Name = blocked.Title,
                    DateTime = startTime,
                    Date = startTime.ToShortDateString(),
                    Time = startTime.ToShortTimeString(),
                    End = endTime,
                    EndTime = endTime.ToString(),
                    Campus = "All Campuses",
                    Blocked = true
                });
            }

            return events.OrderBy(e => e.DateTime).ToList();
        }

        private string SetDayUrl(DateTime day)
        {
            var date = day.ToShortDateString().Replace('/', '-');
            var campuses = SetCampusIdsForQueryString(GetCampusIdsFromQueryString());
            return ResolveUrl(string.Format("~/{0}?ViewMode=Day&Date={1}{2}", CurrentPageReference.BuildUrl().Split('?').First(), date, campuses));
        }

        private List<string> SetHours()
        {
            var res = new List<string>();

            for(DateTime i = DateTime.Today; i < DateTime.Today.AddDays(1).AddMilliseconds(-1); i = i.AddHours(1))
            {
                res.Add(i.ToShortTimeString());
            }

            return res;
        }

        #region Filter Setup

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private bool SetFilterControls()
        {
            // Get and verify the view mode
            ViewMode = GetAttributeValue( "DefaultViewOption" );
            if ( !GetAttributeValue( string.Format( "Show{0}View", ViewMode ) ).AsBoolean() )
            {
                ShowError( "Configuration Error", string.Format( "The Default View Option setting has been set to '{0}', but the Show {0} View setting has not been enabled.", ViewMode ) );
                return false;
            }

            // Show/Hide calendar control
            pnlCalendar.Visible = GetAttributeValue( "ShowSmallCalendar" ).AsBoolean();

            BindCampusFilters();

            BindCategoryFilters();

            // Get the View Modes, and only show them if more than one is visible
            var viewsVisible = new List<bool> {
                GetAttributeValue( "ShowDayView" ).AsBoolean(),
                GetAttributeValue( "ShowWeekView" ).AsBoolean(),
                GetAttributeValue( "ShowMonthView" ).AsBoolean()
            };
            var howManyVisible = viewsVisible.Where( v => v ).Count();
            btnDay.Visible = howManyVisible > 1 && viewsVisible[0];
            btnWeek.Visible = howManyVisible > 1 && viewsVisible[1];
            btnMonth.Visible = howManyVisible > 1 && viewsVisible[2];

            // Set filter visibility
            bool showFilter = pnlCalendar.Visible || rcwCampus.Visible || rcwCategory.Visible;
            pnlFilters.Visible = showFilter;
            pnlList.CssClass = showFilter ? "col-md-9 col-xl-10" : "col-md-12";

            return true;
        }

        private void BindCampusFilters()
        {
            // Setup Campus Filter
            var campusGuidList = GetAttributeValue("Campuses").Split(',').AsGuidList();
            rcwCampus.Visible = GetAttributeValue("CampusFilterDisplayMode").AsInteger() > 1;

            if (campusGuidList.Any())
            {
                cblCampus.DataSource = CampusCache.All(false).Where(c => campusGuidList.Contains(c.Guid));
            }
            else
            {
                cblCampus.DataSource = CampusCache.All(false);
            }

            cblCampus.DataBind();

            var campusQuery = GetCampusIdsFromQueryString();

            foreach(var c in campusQuery)
            {
                var campus = cblCampus.Items.OfType<ListItem>().Where(l => l.Value == c);

                if (campus != null)
                {
                    campus.First().Selected = true;
                }
            }

            if (cblCampus.Items.Count == 1)
            {
                CampusPanelClosed = false;
                CampusPanelOpen = false;
                rcwCampus.Visible = false;
            }
        }

        private void BindCategoryFilters()
        {
            // Setup Category Filter
            var selectedCategoryGuids = GetAttributeValue("FilterMinistry").SplitDelimitedValues(true).AsGuidList();
            rcwCategory.Visible = GetAttributeValue("CategoryFilterDisplayMode").AsInteger() > 1;

            var audiences = DefinedTypeCache.Get("cb22f54e-b3e8-4b62-a54b-570e68cb9ee9".AsGuid());

            if (audiences != null)
            {
                foreach(var ministry in audiences.DefinedValues)
                {
                    if (selectedCategoryGuids.Any())
                    {
                        if(selectedCategoryGuids.Contains(ministry.Guid))
                        {
                            cblCategory.Items.Add(new ListItem { Text = ministry.Value, Value = string.Join(",", ministry.Id.ToString(), ministry.Guid.ToString()) });
                        }

                    }
                    else
                    {
                        cblCategory.Items.Add(new ListItem { Text = ministry.Value, Value = string.Join(",", ministry.Id.ToString(), ministry.Guid.ToString()) });
                    }
                }

                cblCategory.DataBind();

                var ministries = GetMinistryIdsFromQueryString();

                foreach (var m in ministries)
                {
                    var ministry = cblCategory.Items.OfType<ListItem>().Where(l => l.Value.Split(',').First() == m);

                    if (ministry != null)
                    {
                        ministry.First().Selected = true;
                    }
                }
            }

            if (cblCategory.Items.Count == 1)
            {
                CategoryPanelClosed = false;
                CategoryPanelOpen = false;
                rcwCategory.Visible = false;
            }
        }

        #endregion Filter Setup

        /// <summary>
        /// Resets the calendar selection. The control is configured for day selection, but selection will be changed to the week or month if that is the viewmode being used
        /// </summary>
        private void ResetCalendarSelection(DateTime date)
        {

            calEventCalendar.SelectedDate = date;

            var selectedDate = calEventCalendar.SelectedDate;
            var today = RockDateTime.Today;
            StartDate = FilterStartDate = selectedDate;
            EndDate = FilterEndDate = selectedDate;
            if ( ViewMode == "Week" )
            {
                StartDate = FilterStartDate = selectedDate.StartOfWeek( _firstDayOfWeek );
                EndDate = FilterEndDate = selectedDate.EndOfWeek( _firstDayOfWeek );
            }
            else if ( ViewMode == "Month" )
            {
                StartDate = new DateTime(selectedDate.Year, selectedDate.Month, 1);
                FilterStartDate = StartDate.StartOfWeek( _firstDayOfWeek );
                EndDate = new DateTime(selectedDate.Year, selectedDate.Month, 1).AddMonths(1).AddDays(-1);
                FilterEndDate = EndDate.EndOfWeek( _firstDayOfWeek );
            }

            // Reset the selection
            calEventCalendar.SelectedDates.SelectRange( StartDate, EndDate );
            calEventCalendar.VisibleDate = StartDate;
        }

        /// <summary>
        /// Shows the warning.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowWarning( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }

        /// <summary>
        /// Shows the error.
        /// </summary>
        /// <param name="heading">The heading.</param>
        /// <param name="message">The message.</param>
        private void ShowError( string heading, string message )
        {
            nbMessage.Heading = heading;
            nbMessage.Text = string.Format( "<p>{0}</p>", message );
            nbMessage.NotificationBoxType = NotificationBoxType.Danger;
            nbMessage.Visible = true;
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// A class to store event item occurrence data for liquid
        /// </summary>
        [DotLiquid.LiquidType( "EventItemOccurrence", "DateTime", "Name", "Date", "Ministry", "Time", "EndTime", "End", "Campus", "Location", "Color", "Blocked" )]
        public class EventOccurrenceSummary
        {
            public EventItemOccurrence EventItemOccurrence { get; set; }

            public DateTime DateTime { get; set; }

            public string Name { get; set; }

            public string Date { get; set; }

            public List<string> Ministry { get; set; }

            public string Time { get; set; }

            public string EndTime { get; set; }

            public DateTime End { get; set; }

            public string Campus { get; set; }

            public string Location { get; set; }

            public string Color { get; set; }

            public bool Blocked { get; set; }

        }

        [DotLiquid.LiquidType("EventItemOccurrenceSummaries", "Day", "Hours", "Date", "FocusMonth", "DayLink", "Blocked" )]
        public class DayEvent
        {
            public List<EventOccurrenceSummary> EventItemOccurrenceSummaries { get; set; }

            public DateTime Day { get; set; }

            public List<string> Hours { get; set; }

            public string Date { get; set; }

            public bool FocusMonth { get; set; }

            public string DayLink { get; set; }

            public bool Blocked { get; set; }
        }

        /// <summary>
        /// A class to store the event item occurrences dates
        /// </summary>
        public class EventOccurrenceDate
        {
            public EventItemOccurrence EventItemOccurrence { get; set; }

            public List<DateTime> Dates { get; set; }
        }


        #endregion

    }
}
