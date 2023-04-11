using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Tasks;
using Rock.ViewModel;
using Rock.ViewModel.Controls;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;

namespace Rock.Blocks.Plugin.EventCalendar
{
    /// <summary>
    /// Event Calendar.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Event Calendar" )]
    [Category( "Obsidian > Plugin > Event Form" )]
    [Description( "Obsidian Event Calendar" )]
    [IconCssClass( "fa fa-calendar-check" )]

    #region Block Attributes

    [ContentChannelField( "Event Content Channel", key: AttributeKey.EventContentChannel, category: "General", required: true, order: 0 )]
    [ContentChannelField( "Event Details Content Channel", key: AttributeKey.EventDetailsContentChannel, category: "General", required: true, order: 1 )]
    [KeyValueListField( "Calendar Colors", "The location type and the color of the calendar", true, key: AttributeKey.CalendarColors, category: "General", order: 2 )]
    [DefinedTypeField( "Locations Defined Type", key: AttributeKey.LocationList, category: "Lists", required: true, order: 0 )]
    [DefinedTypeField( "Ministries Defined Type", key: AttributeKey.MinistryList, category: "Lists", required: true, order: 1 )]
    [LinkedPage( "Event Submission Form", key: AttributeKey.SubmissionPage, category: "Pages", required: true, order: 0 )]
    [TextField( "Request Status Attribute Key", key: AttributeKey.RequestStatusAttrKey, category: "Filters", defaultValue: "RequestStatus", required: true, order: 1 )]
    [TextField( "Requested Resources Attribute Key", key: AttributeKey.RequestedResourcesAttrKey, category: "Filters", defaultValue: "RequestType", required: true, order: 2 )]
    [TextField( "Event Dates Attribute Key", key: AttributeKey.EventDatesAttrKey, category: "Filters", defaultValue: "EventDates", required: true, order: 3 )]
    [TextField( "Ministry Attribute Key", key: AttributeKey.MinistryAttrKey, category: "Filters", defaultValue: "Ministry", required: true, order: 4 )]
    [TextField( "Details Event Date", "Attribute Key for Event Date on Details", key: AttributeKey.DetailsEventDate, defaultValue: "EventDate", category: "Attributes", order: 3 )]
    [TextField( "Start Time Key", "Attribute Key for Start Time", key: AttributeKey.StartDateTime, defaultValue: "StartTime", category: "Attributes", order: 4 )]
    [TextField( "End Time Key", "Attribute Key for End Time", key: AttributeKey.EndDateTime, defaultValue: "EndTime", category: "Attributes", order: 5 )]
    [TextField( "Room", "Attribute Key for Room", key: AttributeKey.Rooms, defaultValue: "Rooms", category: "Attributes", order: 6 )]
    [TextField( "Start Buffer", "Attribute Key for Start Buffer", key: AttributeKey.StartBuffer, defaultValue: "StartBuffer", category: "Attributes", order: 7 )]
    [TextField( "End Buffer", "Attribute Key for End Buffer", key: AttributeKey.EndBuffer, defaultValue: "EndBuffer", category: "Attributes", order: 8 )]
    [TextField( "Contact", "Attribute Key for Ministry Contact", key: AttributeKey.ContactAttrKey, defaultValue: "Contact", category: "Attributes", order: 9 )]
    #endregion Block Attributes

    public class Calendar : RockObsidianBlockType
    {
        #region Keys

        /// <summary>
        /// Attribute Key
        /// </summary>
        private static class AttributeKey
        {
            public const string EventContentChannel = "EventContentChannel";
            public const string EventDetailsContentChannel = "EventDetailsContentChannel";
            public const string CalendarColors = "CalendarColors";
            public const string LocationList = "LocationList";
            public const string MinistryList = "MinistryList";
            public const string BudgetList = "BudgetList";
            public const string MinistryBudgetList = "MinistryBudgetList";
            public const string DrinksList = "DrinksList";
            public const string InventoryList = "InventoryList";
            public const string SubmissionPage = "SubmissionPage";
            public const string DefaultStatuses = "DefaultStatuses";
            public const string RequestStatusAttrKey = "RequestStatusAttrKey";
            public const string RequestedResourcesAttrKey = "RequestedResourcesAttrKey";
            public const string EventDatesAttrKey = "EventDatesAttrKey";
            public const string MinistryAttrKey = "MinistryAttrKey";
            public const string DetailsEventDate = "DetailsEventDate";
            public const string StartDateTime = "StartDateTime";
            public const string EndDateTime = "EndDateTime";
            public const string ContactAttrKey = "ContactAttrKey";
            public const string Rooms = "Rooms";
            public const string StartBuffer = "StartBuffer";
            public const string EndBuffer = "EndBuffer";
        }

        #endregion Keys

        #region Obsidian Block Type Overrides

        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            var rockContext = new RockContext();
            Guid eventDatesAttrGuid = Guid.Empty;
            Guid requestStatusAttrGuid = Guid.Empty;
            Guid isSameAttrGuid = Guid.Empty;
            CalendarBlockViewModel viewModel = new CalendarBlockViewModel();

            SetProperties();
            if ( EventContentChannelId > 0 && EventDetailsContentChannelId > 0 )
            {
                //Lists
                var p = GetCurrentPerson();
                if ( Locations != null && Locations.Count() > 0 )
                {
                    var locs = Locations.Select( l => l.ToViewModel( p, true ) );
                    viewModel.locations = locs.ToList();
                }
                if ( Ministries != null && Ministries.Count() > 0 )
                {
                    var mins = Ministries.Select( m => m.ToViewModel( p, true ) );
                    viewModel.ministries = mins.ToList();
                }

                //Attributes
                string requestStatusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
                if ( !String.IsNullOrEmpty( requestStatusAttrKey ) )
                {
                    viewModel.requestStatus = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == requestStatusAttrKey ).ToViewModel();
                }
                string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
                if ( !String.IsNullOrEmpty( resourcesAttrKey ) )
                {
                    viewModel.requestType = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == resourcesAttrKey ).ToViewModel();
                }

            }
            return viewModel;
        }

        #endregion Obsidian Block Type Overrides

        #region Properties

        private int EventContentChannelId { get; set; }
        private int EventContentChannelTypeId { get; set; }
        private int EventDetailsContentChannelId { get; set; }
        private int EventDetailsContentChannelTypeId { get; set; }
        private Rock.Model.Attribute EventDatesAttr { get; set; }
        private Rock.Model.Attribute RequestStatusAttr { get; set; }
        private Rock.Model.Attribute ResourcesAttr { get; set; }
        private Rock.Model.Attribute EventDateAttr { get; set; }
        private Rock.Model.Attribute StartTimeAttr { get; set; }
        private Rock.Model.Attribute EndTimeAttr { get; set; }
        private Rock.Model.Attribute StartBufferAttr { get; set; }
        private Rock.Model.Attribute EndBufferAttr { get; set; }
        private Rock.Model.Attribute LocationAttr { get; set; }
        private Rock.Model.Attribute MinistryAttr { get; set; }
        private Rock.Model.Attribute ContactAttr { get; set; }
        private List<DefinedValue> Locations { get; set; }
        private List<DefinedValue> Ministries { get; set; }

        #endregion

        #region Block Actions


        #endregion Block Actions

        [BlockAction]
        public BlockActionResult GetEvents( DateTime start, DateTime end )
        {
            try
            {
                SetProperties();
                if ( EventDatesAttr != null && EventDateAttr != null && StartTimeAttr != null && StartBufferAttr != null && EndTimeAttr != null && EndBufferAttr != null && LocationAttr != null )
                {
                    RockContext context = new RockContext();
                    AttributeValueService av_svc = new AttributeValueService( context );
                    ContentChannelItemService cci_svc = new ContentChannelItemService( context );
                    var dates = av_svc.Queryable().Where( av => av.AttributeId == EventDatesAttr.Id ).ToList().Where( av =>
                    {
                        bool inRange = false;
                        if ( !String.IsNullOrEmpty( av.Value ) )
                        {
                            List<DateTime> d = av.Value.Split( ',' ).Select( dt => DateTime.Parse( dt ) ).ToList();
                            for ( int i = 0; i < d.Count(); i++ )
                            {
                                if ( start <= d[i] && d[i] <= end )
                                {
                                    inRange = true;
                                }
                            }
                        }
                        return inRange;
                    } ).ToList();
                    var statuses = av_svc.Queryable().Where( av => av.AttributeId == RequestStatusAttr.Id && av.Value != "Draft" && av.Value != "Submitted" && av.Value != "Denied" && !av.Value.Contains( "Cancelled" ) );
                    var eventsInRange = cci_svc.Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId ).Join( statuses,
                        cci => cci.Id,
                        av => av.EntityId,
                        ( cci, av ) => cci
                    );
                    var inRangeEvents = eventsInRange.ToList().Join( dates,
                        cci => cci.Id,
                        av => av.EntityId,
                        ( cci, av ) => cci
                    ).ToList();

                    List<EventFormCalendar> calendars = new List<EventFormCalendar>();
                    for ( int i = 0; i < inRangeEvents.Count(); i++ )
                    {
                        var details = inRangeEvents[i].ChildItems.Select( ci => ci.ChildContentChannelItem ).Where( cci => cci.ContentChannelId == EventDetailsContentChannelId ).ToList();
                        for ( int k = 0; k < details.Count(); k++ )
                        {
                            var currentDetail = details[k];
                            var eventDateAV = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == EventDateAttr.Id && av.EntityId == currentDetail.Id );
                            if ( eventDateAV == null || String.IsNullOrEmpty( eventDateAV.Value ) )
                            {
                                var parent = inRangeEvents[i];
                                eventDateAV = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == EventDatesAttr.Id && av.EntityId == parent.Id );
                                List<string> eventDates = eventDateAV.Value.Split( ',' ).Select( d => d.Trim() ).ToList();
                                for ( int h = 0; h < eventDates.Count(); h++ )
                                {
                                    BuildEvent( calendars, currentDetail, parent, eventDates[h] );
                                }
                            }
                            else
                            {
                                string eventDate = DateTime.Parse( eventDateAV.Value ).ToString( "yyyy-MM-dd" );
                                BuildEvent( calendars, currentDetail, inRangeEvents[i], eventDate );
                            }
                        }
                    }

                    return ActionOk( calendars );
                }
                throw new Exception( "Configuration Error: Cannot find Event Dates Attribute" );
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
                return ActionBadRequest( e.Message );
            }
        }
        #region Helpers

        private void BuildEvent( List<EventFormCalendar> calendars, ContentChannelItem currentDetail, ContentChannelItem parent, string eventDate )
        {
            RockContext context = new RockContext();
            AttributeValueService av_svc = new AttributeValueService( context );
            ContentChannelItemService cci_svc = new ContentChannelItemService( context );
            DateTime? eStart = null;
            DateTime? eEnd = null;
            int startbuffer = 0;
            int endbuffer = 0;
            var startAV = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == StartTimeAttr.Id && av.EntityId == currentDetail.Id );
            if ( startAV != null )
            {
                DateTime dt;
                if ( DateTime.TryParse( $"{eventDate} {startAV.Value}", out dt ) )
                {
                    eStart = dt;
                    var startBufferAV = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == StartBufferAttr.Id && av.EntityId == currentDetail.Id );
                    if ( startBufferAV != null )
                    {
                        Int32.TryParse( startBufferAV.Value, out startbuffer );
                    }
                }
            }
            var endAV = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == EndTimeAttr.Id && av.EntityId == currentDetail.Id );
            if ( endAV != null )
            {
                DateTime dt;
                if ( DateTime.TryParse( $"{eventDate} {endAV.Value}", out dt ) )
                {
                    eEnd = dt;
                    var endBufferAV = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == EndBufferAttr.Id && av.EntityId == currentDetail.Id );
                    if ( endBufferAV != null )
                    {
                        Int32.TryParse( endBufferAV.Value, out endbuffer );
                    }
                }
            }
            var locationAV = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == LocationAttr.Id && av.EntityId == currentDetail.Id );
            var ministryAv = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == MinistryAttr.Id && av.EntityId == parent.Id );
            var contactAv = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == ContactAttr.Id && av.EntityId == parent.Id );
            var resourcesAv = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == ResourcesAttr.Id && av.EntityId == parent.Id );
            if ( locationAV != null && ministryAv != null && resourcesAv != null && eStart.HasValue && eEnd.HasValue )
            {
                var locationGuids = locationAV.Value.Split( ',' ).AsGuidOrNullList();
                var locs = Locations.Where( l => locationGuids.Contains( l.Guid ) ).ToList();
                var ministry = Ministries.FirstOrDefault( dv => dv.Guid == Guid.Parse( ministryAv.Value ) );
                var grouped = locs.Select( l => new { Category = l.GetAttributeValue( "Type" ), Location = l.Value } ).GroupBy( l => l.Category ).ToList();

                for ( int j = 0; j < grouped.Count(); j++ )
                {
                    var calendarIdx = calendars.Select( c => c.name ).ToList().IndexOf( grouped[j].Key );
                    EventFormCalendar calendar;
                    if ( calendarIdx < 0 )
                    {
                        var colors = GetAttributeValue( AttributeKey.CalendarColors ).Split( '|' ).ToList();
                        var color = colors.FirstOrDefault( c => c.Split( '^' )[0] == grouped[j].Key );
                        calendar = new EventFormCalendar()
                        {
                            name = grouped[j].Key,
                            events = new List<EventFormEvent>(),
                            color = color != null ? ( "rgba(" + color.Split( '^' )[1] + ", .7)" ) : "rgba(78, 135, 140, .7)",
                            border = color != null ? ( "rgba(" + color.Split( '^' )[1] + ", 1)" ) : "rgba(78, 135, 140, 1)"
                        };
                        calendars.Add( calendar );
                    }
                    else
                    {
                        calendar = calendars[calendarIdx];
                    }
                    EventFormEvent e = new EventFormEvent()
                    {
                        id = currentDetail.Id,
                        parentId = parent.Id,
                        title = parent.Title,
                        start = eStart.Value,
                        end = eEnd.Value,
                        startBuffer = startbuffer,
                        endBuffer = endbuffer,
                        location = String.Join( ", ", grouped[j].Select( g => g.Location ) ),
                        ministry = ministry.Value,
                        submitterId = parent.CreatedByPersonAliasId.Value,
                        submitter = parent.CreatedByPersonName,
                        contact = contactAv != null ? contactAv.Value : parent.CreatedByPersonName,
                        resources = resourcesAv.Value.Split( ',' ).ToList()
                    };
                    calendar.events.Add( e );
                }
            }
        }

        private ContentChannelItem FromViewModel( ContentChannelItemViewModel viewModel )
        {
            RockContext context = new RockContext();
            Rock.Model.Person p = GetCurrentPerson();
            ContentChannelItem item = new ContentChannelItem()
            {
                ContentChannelId = viewModel.ContentChannelId,
                ContentChannelTypeId = viewModel.ContentChannelTypeId
            };
            if ( viewModel.Id > 0 )
            {
                item = new ContentChannelItemService( context ).Get( viewModel.Id );
            }
            item.LoadAttributes();
            item.Title = viewModel.Title;
            foreach ( KeyValuePair<string, string> av in viewModel.AttributeValues )
            {
                item.SetPublicAttributeValue( av.Key, av.Value, p, false );
            }

            return item;
        }

        private void SetProperties()
        {
            RockContext rockContext = new RockContext();
            Guid eventCCGuid = Guid.Empty;
            Guid eventDetailsCCGuid = Guid.Empty;

            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventContentChannel ), out eventCCGuid ) )
            {
                ContentChannel cc = new ContentChannelService( rockContext ).Get( eventCCGuid );
                EventContentChannelId = cc.Id;
                EventContentChannelTypeId = cc.ContentChannelTypeId;
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventDetailsContentChannel ), out eventDetailsCCGuid ) )
            {
                ContentChannel dCC = new ContentChannelService( rockContext ).Get( eventDetailsCCGuid );
                EventDetailsContentChannelId = dCC.Id;
                EventDetailsContentChannelTypeId = dCC.ContentChannelTypeId;

            }
            string eventDatesAttrKey = GetAttributeValue( AttributeKey.EventDatesAttrKey );
            if ( !String.IsNullOrEmpty( eventDatesAttrKey ) )
            {
                EventDatesAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == eventDatesAttrKey );
            }
            string statusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
            if ( !String.IsNullOrEmpty( statusAttrKey ) )
            {
                RequestStatusAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == statusAttrKey );
            }
            string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
            if ( !String.IsNullOrEmpty( resourcesAttrKey ) )
            {
                ResourcesAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == resourcesAttrKey );
            }
            string eventDateAttrKey = GetAttributeValue( AttributeKey.DetailsEventDate );
            if ( !String.IsNullOrEmpty( eventDateAttrKey ) )
            {
                EventDateAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == eventDateAttrKey );
            }
            string startTimeAttrKey = GetAttributeValue( AttributeKey.StartDateTime );
            if ( !String.IsNullOrEmpty( startTimeAttrKey ) )
            {
                StartTimeAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == startTimeAttrKey );
            }
            string startBufferAttrKey = GetAttributeValue( AttributeKey.StartBuffer );
            if ( !String.IsNullOrEmpty( startBufferAttrKey ) )
            {
                StartBufferAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == startBufferAttrKey );
            }
            string endTimeAttrKey = GetAttributeValue( AttributeKey.EndDateTime );
            if ( !String.IsNullOrEmpty( endTimeAttrKey ) )
            {
                EndTimeAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == endTimeAttrKey );
            }
            string endBufferAttrKey = GetAttributeValue( AttributeKey.EndBuffer );
            if ( !String.IsNullOrEmpty( endBufferAttrKey ) )
            {
                EndBufferAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == endBufferAttrKey );
            }
            string locationAttrKey = GetAttributeValue( AttributeKey.Rooms );
            if ( !String.IsNullOrEmpty( locationAttrKey ) )
            {
                LocationAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == locationAttrKey );
            }
            string ministryAttrKey = GetAttributeValue( AttributeKey.MinistryAttrKey );
            if ( !String.IsNullOrEmpty( ministryAttrKey ) )
            {
                MinistryAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == ministryAttrKey );
            }
            string contactAttrKey = GetAttributeValue( AttributeKey.ContactAttrKey );
            if ( !String.IsNullOrEmpty( contactAttrKey ) )
            {
                ContactAttr = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == contactAttrKey );
            }
            Guid locationGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.LocationList ), out locationGuid ) )
            {
                DefinedType locationDT = new DefinedTypeService( rockContext ).Get( locationGuid );
                var locs = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == locationDT.Id ).ToList();
                locs.LoadAttributes();
                Locations = locs;
            }
            Guid ministryGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryGuid ) )
            {
                DefinedType ministryDT = new DefinedTypeService( rockContext ).Get( ministryGuid );
                var min = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == ministryDT.Id );
                min.LoadAttributes();
                Ministries = min.ToList();
            }
        }

        #endregion Helpers

        public class CalendarBlockViewModel
        {
            public List<ContentChannelItemViewModel> events { get; set; }
            public List<DefinedValueViewModel> locations { get; set; }
            public List<DefinedValueViewModel> ministries { get; set; }
            public AttributeViewModel requestStatus { get; set; }
            public AttributeViewModel requestType { get; set; }
        }

        public class EventFormCalendar
        {
            public string name { get; set; }
            public string color { get; set; }
            public string border { get; set; }
            public List<EventFormEvent> events { get; set; }
        }

        public class EventFormEvent
        {
            public int id { get; set; }
            public int parentId { get; set; }
            public DateTime start { get; set; }
            public DateTime end { get; set; }
            public int startBuffer { get; set; }
            public int endBuffer { get; set; }
            public string title { get; set; }
            public string location { get; set; }
            public string ministry { get; set; }
            public List<string> resources { get; set; }
            public int submitterId { get; set; }
            public string submitter { get; set; }
            public string contact { get; set; }
        }

    }
}
