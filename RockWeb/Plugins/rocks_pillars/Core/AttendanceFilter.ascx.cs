
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;

namespace RockWeb.Plugins.rocks_pillars.Core
{
    /// <summary>
    /// Block used to filter attendance by date/service
    /// </summary>
    [DisplayName( "Attendance Filter" )]
    [Category( "Pillars > Core" )]
    [Description( "Utility block used to select a date/service for attendance and then refresh page or redirect to new page." )]

    [CustomDropdownListField( "Check In Area", "The Check In Area to display.", @"
SELECT 
	T.[Id] AS [Value], 
	T.[Name] AS [Text]
FROM [GroupType] T
INNER JOIN [DefinedValue] P ON P.[Id] = T.[GroupTypePurposeValueId]
WHERE P.[Guid] = '4A406CB0-495B-4795-B788-52BDFDE00B01'
ORDER BY T.[Name]
", false, "", "", 0 )]
    [BooleanField("Show Area Selection", "Should user be able to select the area (vs. configured area)?", false, "", 1 )]
    [BooleanField("Use Date Range", "Should user be able to select a date range (vs. a specific date)", false, "", 1)]
    [SlidingDateRangeField("Default Date Range", "The default date range to use if using a date range", false, "Last|3|Month||", "", 2)]
    [BooleanField( "Require Schedule", "Should the schedule(s) be required", true, "", 3 )]
    [BooleanField( "Show Check-In Time", "Should the Check-In Time filter be displayed", false, "", 4, "ShowTime")]
    [TextField("Early Check In Filter Caption", "The caption to use for the Early Check-In Filter Field", false, "", "", 5 )]
    [TextField("Early Check In Filter Values", "The values to select from for the Early Check-In Filter Field", false, "^Include Normal and Multi-Age Check ins|Exclude^Only Include Normal Check ins|Only^Only Include Multi-Age Check ins", "", 6)]
    [IntegerField( "Early Checkin", "Number of minutes prior to the schedule's start that should be considered an early check in. Only used if Show Early Check In Option is selected.", false, 45, "", 7 )]
    [LinkedPage( "Page Redirect", "If set, the filter button will redirect to the selected page.", false, "", "", 8 )]
    public partial class AttendanceFilter : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                string delimitedDateRange = PageParameter( "DateRange" );
                if ( delimitedDateRange.IsNullOrWhiteSpace() )
                {
                    delimitedDateRange = GetAttributeValue( "DefaultDateRange" );
                }
                sdrpDates.DelimitedValues = delimitedDateRange;

                DateTime? date = PageParameter( "Date" ).AsDateTime();
                dpDate.SelectedDate = date ?? RockDateTime.Today;

                BindFilter();

                string area = PageParameter( "CheckInArea" );
                if ( area.IsNullOrWhiteSpace() )
                {
                    area = GetAttributeValue( "CheckInArea" );
                }
                ddlArea.SetValue( area );

                lbSchedules.SetValues( PageParameter( "ServiceTimes" ).SplitDelimitedValues().AsIntegerList() );

                pnlCheckInTime.Visible = GetAttributeValue( "ShowTime" ).AsBoolean();
                tpCheckInTimeStart.SelectedTime = PageParameter( "StartTime" ).AsTimeSpan();
                tpCheckInTimeEnd.SelectedTime = PageParameter( "EndTime" ).AsTimeSpan();

                ddlEarlyCheckin.Label = GetAttributeValue( "EarlyCheckInFilterCaption" );
                ddlEarlyCheckin.Visible = ddlEarlyCheckin.Label.IsNotNullOrWhiteSpace();
                foreach( var option in GetAttributeValue("EarlyCheckInFilterValues").Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
                {
                    var kv = option.Split( new char[] { '^' } );
                    if ( kv.Length == 2 )
                    {
                        ddlEarlyCheckin.Items.Add( new ListItem( kv[1], kv[0] ) );
                    }
                }
                ddlEarlyCheckin.SetValue( PageParameter( "EarlyCheckin" ) );
            }
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
            BindFilter();
        }

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            var pageParams = new Dictionary<string, string>();

            string area = string.Empty;
            if ( GetAttributeValue( "ShowAreaSelection" ).AsBoolean() )
            {
                area = ddlArea.SelectedValue;
            }
            else
            {
                area = GetAttributeValue( "CheckInArea" );
            }

            if ( area.IsNotNullOrWhiteSpace() )
            {
                pageParams.Add( "CheckInArea", area );
            }

            if ( GetAttributeValue( "UseDateRange" ).AsBoolean() )
            {
                pageParams.Add( "DateRange", sdrpDates.DelimitedValues );
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDates.DelimitedValues );
                if ( dateRange.Start.HasValue )
                {
                    pageParams.Add( "StartDate", dateRange.Start.Value.ToShortDateString() );
                }
                if ( dateRange.End.HasValue )
                {
                    pageParams.Add( "EndDate", dateRange.End.Value.ToShortDateString() );
                }
            }
            else
            {
                pageParams.Add( "Date", dpDate.SelectedDate.HasValue ? dpDate.SelectedDate.Value.ToShortDateString() : "" );
            }

            pageParams.Add( "ServiceTimes", lbSchedules.SelectedValues.AsDelimited( "," ) );

            if ( GetAttributeValue("ShowTime").AsBoolean() )
            {
                pageParams.Add( "StartTime", tpCheckInTimeStart.SelectedTime.ToString() );
                pageParams.Add( "EndTime", tpCheckInTimeEnd.SelectedTime.ToString() );
            }

            string earlyCheckin = GetAttributeValue( "EarlyCheckInFilterCaption" ).IsNotNullOrWhiteSpace() ? ddlEarlyCheckin.SelectedValue : string.Empty;
            pageParams.Add( "EarlyCheckin", earlyCheckin );

            int? minutesPrior = GetAttributeValue( "EarlyCheckin" ).AsIntegerOrNull();
            pageParams.Add( "MinutesPrior", minutesPrior.HasValue ? minutesPrior.Value.ToString() : string.Empty );

            if ( GetAttributeValue( "PageRedirect" ).IsNotNullOrWhiteSpace() )
            {
                NavigateToLinkedPage( "PageRedirect", pageParams );
            }
            else
            {
                NavigateToCurrentPage( pageParams );
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the dpDate control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dpDate_TextChanged( object sender, EventArgs e )
        {
            BindFilter();
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
        /// <param name="serviceDate">The service date.</param>
        private void BindFilter()
        {
            pnlArea.Visible = GetAttributeValue( "ShowAreaSelection" ).AsBoolean();
            if ( pnlArea.Visible )
            {
                ddlArea.Items.Clear();
                ddlArea.Items.Add( new ListItem( "All", "" ) );

                var templateType = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.GROUPTYPE_PURPOSE_CHECKIN_TEMPLATE.AsGuid() );
                foreach ( var groupType in new GroupTypeService( new RockContext() )
                    .Queryable().AsNoTracking()
                    .Where( t =>
                        t.GroupTypePurposeValueId.HasValue &&
                        t.GroupTypePurposeValueId.Value == templateType.Id )
                    .OrderBy( t => t.Name ) )
                {
                    ddlArea.Items.Add( new ListItem( groupType.Name, groupType.Id.ToString() ) );
                }
            }

            var selectedItems = lbSchedules.SelectedValuesAsInt;
            
            lSchedules.Visible = true;
            lbSchedules.Visible = false;
            lbSchedules.Items.Clear();
            btnFilter.Enabled = false;

            DateTime? dateStart = null;
            DateTime? dateEnd = null;

            bool useDateRange = GetAttributeValue( "UseDateRange" ).AsBoolean();
            dpDate.Visible = !useDateRange;
            sdrpDates.Visible = useDateRange;

            if ( useDateRange )
            {
                var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( sdrpDates.DelimitedValues );

                if ( sdrpDates.SlidingDateRangeMode == SlidingDateRangePicker.SlidingDateRangeType.DateRange )
                {
                    lSchedules.Text = "After selecting date, click refresh icon to see Service Times";
                    lbRefreshSchedules.Visible = true;
                }
                else
                {
                    lSchedules.Text = "Select a Date Range to see Service Times";
                    lbRefreshSchedules.Visible = false;
                }

                if ( dateRange.Start.HasValue )
                {
                    dateStart = dateRange.Start.Value;
                }
                if ( dateRange.End.HasValue )
                {
                    dateEnd = dateRange.End.Value;
                }
            }
            else
            {
                lSchedules.Text = "Select a Date to see Service Times";
                dateStart = dpDate.SelectedDate;
                dateEnd = dateStart.HasValue ? dateStart.Value.AddDays( 1 ) : (DateTime?)null;
            }

            lbSchedules.Required = GetAttributeValue( "RequireSchedule" ).AsBoolean( true );
            if ( lbSchedules.Required )
            {
                pnlSchedules.AddCssClass( "required" );
            }
            else
            {
                pnlSchedules.RemoveCssClass( "required" );
            }

            if ( dateStart.HasValue )
            {
                var area = GetAttributeValue( "CheckInArea" ).AsIntegerOrNull();

                using ( var rockContext = new RockContext() )
                {
                    var occQry = new AttendanceOccurrenceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( o =>
                            o.OccurrenceDate >= dateStart &&
                            (!dateEnd.HasValue || o.OccurrenceDate < dateEnd ) &&
                            o.Attendees.Any( a => a.DidAttend.HasValue && a.DidAttend.Value ) &&
                            o.Schedule != null
                        );

                    if ( area.HasValue )
                    {
                        var groupTypeIds = new GroupTypeService( rockContext )
                            .GetAllAssociatedDescendents( area.Value )
                            .Select( t => t.Id )
                            .ToList();

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

                    foreach ( var serviceTime in serviceTimes )
                    {
                        var item = new ListItem( serviceTime.Name, serviceTime.Id.ToString() );
                        item.Selected = selectedItems.Contains( serviceTime.Id );
                        lbSchedules.Items.Add( item );
                    }

                    if ( serviceTimes.Any() )
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

        #endregion
    }
}