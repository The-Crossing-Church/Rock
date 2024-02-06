using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Entities;
using Rock.Web.Cache;

namespace Rock.Blocks.Plugins.EventCalendar
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
    [LinkedPage( "User Dashboard", key: AttributeKey.UserDashboard, category: "Pages", required: true, order: 1 )]
    [LinkedPage( "Admin Dashboard", key: AttributeKey.AdminDashboard, category: "Pages", required: true, order: 2 )]
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
    [TextField( "IsSame", "Attribute Key for IsSame", key: AttributeKey.IsSameAttrKey, defaultValue: "IsSame", category: "Attributes", order: 10 )]
    [TextField( "NeedsSpace", "Attribute Key for NeedsSpace", key: AttributeKey.NeedsSpaceAttrKey, defaultValue: "NeedsSpace", category: "Attributes", order: 11 )]
    [TextField( "NeedsOnline", "Attribute Key for NeedsOnline", key: AttributeKey.NeedsOnlineAttrKey, defaultValue: "NeedsOnline", category: "Attributes", order: 12 )]
    [TextField( "NeedsPublicity", "Attribute Key for NeedsPublicity", key: AttributeKey.NeedsPublicityAttrKey, defaultValue: "NeedsPublicity", category: "Attributes", order: 13 )]
    [TextField( "NeedsRegistration", "Attribute Key for NeedsRegistration", key: AttributeKey.NeedsRegistrationAttrKey, defaultValue: "NeedsRegistration", category: "Attributes", order: 14 )]
    [TextField( "NeedsCalendar", "Attribute Key for NeedsCalendar", key: AttributeKey.NeedsCalendarAttrKey, defaultValue: "NeedsWebCalendar", category: "Attributes", order: 15 )]
    [TextField( "NeedsCatering", "Attribute Key for NeedsCatering", key: AttributeKey.NeedsCateringAttrKey, defaultValue: "NeedsCatering", category: "Attributes", order: 16 )]
    [TextField( "NeedsChildcare", "Attribute Key for NeedsChildcare", key: AttributeKey.NeedsChildcareAttrKey, defaultValue: "NeedsChildCare", category: "Attributes", order: 17 )]
    [TextField( "NeedsChildcareCatering", "Attribute Key for NeedsChildcareCatering", key: AttributeKey.NeedsChildcareCateringAttrKey, defaultValue: "NeedsChildCareCatering", category: "Attributes", order: 18 )]
    [TextField( "NeedsOps", "Attribute Key for NeedsOps", key: AttributeKey.NeedsOpsAttrKey, defaultValue: "NeedsOpsAccommodations", category: "Attributes", order: 19 )]
    [TextField( "NeedsProduction", "Attribute Key for NeedsProduction", key: AttributeKey.NeedsProductionAttrKey, defaultValue: "NeedsProductionAccommodations", category: "Attributes", order: 20 )]
    [TextField( "Infrastructure Space", "Attribute Key for Infrastructure Space", key: AttributeKey.InfrstructureSpaceAttrKey, defaultValue: "InfrastructureSpace", category: "Attributes", order: 21 )]
    [SecurityRoleField( "Event Request Admin", key: AttributeKey.EventAdminRole, category: "Security", required: true, order: 0 )]

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
            public const string UserDashboard = "UserDashboard";
            public const string AdminDashboard = "AdminDashboard";
            public const string DefaultStatuses = "DefaultStatuses";
            public const string RequestStatusAttrKey = "RequestStatusAttrKey";
            public const string RequestedResourcesAttrKey = "RequestedResourcesAttrKey";
            public const string EventDatesAttrKey = "EventDatesAttrKey";
            public const string MinistryAttrKey = "MinistryAttrKey";
            public const string DetailsEventDate = "DetailsEventDate";
            public const string StartDateTime = "StartDateTime";
            public const string EndDateTime = "EndDateTime";
            public const string ContactAttrKey = "ContactAttrKey";
            public const string IsSameAttrKey = "IsSameAttrKey";
            public const string NeedsSpaceAttrKey = "NeedsSpaceAttrKey";
            public const string NeedsOnlineAttrKey = "NeedsOnlineAttrKey";
            public const string NeedsPublicityAttrKey = "NeedsPublicityAttrKey";
            public const string NeedsRegistrationAttrKey = "NeedsRegistrationAttrKey";
            public const string NeedsCalendarAttrKey = "NeedsCalendarAttrKey";
            public const string NeedsCateringAttrKey = "NeedsCateringAttrKey";
            public const string NeedsChildcareAttrKey = "NeedsChildcareAttrKey";
            public const string NeedsChildcareCateringAttrKey = "NeedsChildcareCateringAttrKey";
            public const string NeedsOpsAttrKey = "NeedsOpsAttrKey";
            public const string NeedsProductionAttrKey = "NeedsProductionAttrKey";
            public const string InfrstructureSpaceAttrKey = "InfrstructureSpaceAttrKey";
            public const string Rooms = "Rooms";
            public const string StartBuffer = "StartBuffer";
            public const string EndBuffer = "EndBuffer";
            public const string EventAdminRole = "EventAdminRole";
        }

        #endregion Keys

        #region Properties

        private int EventContentChannelId { get; set; }
        private int EventContentChannelTypeId { get; set; }
        private int EventDetailsContentChannelId { get; set; }
        private int EventDetailsContentChannelTypeId { get; set; }
        private Rock.Model.Attribute EventDatesAttr { get; set; }
        private Rock.Model.Attribute RequestStatusAttr { get; set; }
        private Rock.Model.Attribute InfrastructureSpaceAttr { get; set; }
        private Rock.Model.Attribute EventDateAttr { get; set; }
        private Rock.Model.Attribute StartTimeAttr { get; set; }
        private Rock.Model.Attribute EndTimeAttr { get; set; }
        private Rock.Model.Attribute StartBufferAttr { get; set; }
        private Rock.Model.Attribute EndBufferAttr { get; set; }
        private Rock.Model.Attribute LocationAttr { get; set; }
        private Rock.Model.Attribute MinistryAttr { get; set; }
        private Rock.Model.Attribute ContactAttr { get; set; }
        private Rock.Model.Attribute IsSameAttr { get; set; }
        private Rock.Model.Attribute NeedsSpaceAttr { get; set; }
        private Rock.Model.Attribute NeedsOnlineAttr { get; set; }
        private Rock.Model.Attribute NeedsPublicityAttr { get; set; }
        private Rock.Model.Attribute NeedsRegistrationAttr { get; set; }
        private Rock.Model.Attribute NeedsCalendarAttr { get; set; }
        private Rock.Model.Attribute NeedsCateringAttr { get; set; }
        private Rock.Model.Attribute NeedsChildcareAttr { get; set; }
        private Rock.Model.Attribute NeedsChildcareCateringAttr { get; set; }
        private Rock.Model.Attribute NeedsOpsAttr { get; set; }
        private Rock.Model.Attribute NeedsProductionAttr { get; set; }
        private DefinedType LocationDT { get; set; }
        private DefinedType MinistryDT { get; set; }
        private List<DefinedValue> Locations { get; set; }
        private List<DefinedValue> Ministries { get; set; }

        #endregion

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

            if (EventContentChannelId > 0 && EventDetailsContentChannelId > 0)
            {
                //Lists
                var p = GetCurrentPerson();
                if (Locations != null && Locations.Count() > 0)
                {
                    viewModel.locations = Locations;
                }
                if (Ministries != null && Ministries.Count() > 0)
                {
                    viewModel.ministries = Ministries;
                }

                //Attributes
                string requestStatusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
                if (!String.IsNullOrEmpty( requestStatusAttrKey ))
                {
                    viewModel.requestStatus = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == requestStatusAttrKey ).ToViewModel();
                }
                string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
                if (!String.IsNullOrEmpty( resourcesAttrKey ))
                {
                    viewModel.requestType = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == resourcesAttrKey ).ToViewModel();
                }
                viewModel.isEventAdmin = CheckSecurityRole( rockContext, AttributeKey.EventAdminRole );
                viewModel.formUrl = this.GetLinkedPageUrl( AttributeKey.SubmissionPage );
                if (viewModel.isEventAdmin)
                {
                    viewModel.dashboardUrl = this.GetLinkedPageUrl( AttributeKey.AdminDashboard );
                }
                else
                {
                    viewModel.dashboardUrl = this.GetLinkedPageUrl( AttributeKey.UserDashboard );
                }
            }
            return viewModel;
        }

        #endregion Obsidian Block Type Overrides

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetEvents( DateTime start, DateTime end )
        {
            try
            {
                SetProperties();
                if (EventDatesAttr != null && EventDateAttr != null && StartTimeAttr != null && StartBufferAttr != null && EndTimeAttr != null && EndBufferAttr != null && LocationAttr != null)
                {
                    var calendars = GetEventDataSQL( start, end );

                    return ActionOk( calendars );
                }
                throw new Exception( "Configuration Error: Cannot find Event Dates Attribute" );
            }
            catch (Exception e)
            {
                ExceptionLogService.LogException( e );
                return ActionBadRequest( e.Message );
            }
        }

        #endregion Block Actions

        #region Helpers

        private ContentChannelItem FromViewModel( ContentChannelItemBag viewModel )
        {
            RockContext context = new RockContext();
            Rock.Model.Person p = GetCurrentPerson();
            ContentChannelItem item = new ContentChannelItem()
            {
                ContentChannelId = viewModel.ContentChannelId,
                ContentChannelTypeId = viewModel.ContentChannelTypeId
            };
            if (!String.IsNullOrEmpty( viewModel.IdKey ))
            {
                item = new ContentChannelItemService( context ).Get( viewModel.IdKey );
            }
            item.LoadAttributes();
            item.Title = viewModel.Title;
            foreach (KeyValuePair<string, string> av in viewModel.AttributeValues)
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

            if (Guid.TryParse( GetAttributeValue( AttributeKey.EventContentChannel ), out eventCCGuid ))
            {
                ContentChannel cc = new ContentChannelService( rockContext ).Get( eventCCGuid );
                EventContentChannelId = cc.Id;
                EventContentChannelTypeId = cc.ContentChannelTypeId;
            }
            if (Guid.TryParse( GetAttributeValue( AttributeKey.EventDetailsContentChannel ), out eventDetailsCCGuid ))
            {
                ContentChannel dCC = new ContentChannelService( rockContext ).Get( eventDetailsCCGuid );
                EventDetailsContentChannelId = dCC.Id;
                EventDetailsContentChannelTypeId = dCC.ContentChannelTypeId;

            }
            var attr_svc = new AttributeService( rockContext );
            string eventDatesAttrKey = GetAttributeValue( AttributeKey.EventDatesAttrKey );
            if (!String.IsNullOrEmpty( eventDatesAttrKey ))
            {
                EventDatesAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == eventDatesAttrKey );
            }
            string statusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
            if (!String.IsNullOrEmpty( statusAttrKey ))
            {
                RequestStatusAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == statusAttrKey );
            }
            string infrstructureSpaceAttrKey = GetAttributeValue( AttributeKey.InfrstructureSpaceAttrKey );
            if (!String.IsNullOrEmpty( infrstructureSpaceAttrKey ))
            {
                InfrastructureSpaceAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == infrstructureSpaceAttrKey );
            }
            string eventDateAttrKey = GetAttributeValue( AttributeKey.DetailsEventDate );
            if (!String.IsNullOrEmpty( eventDateAttrKey ))
            {
                EventDateAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == eventDateAttrKey );
            }
            string startTimeAttrKey = GetAttributeValue( AttributeKey.StartDateTime );
            if (!String.IsNullOrEmpty( startTimeAttrKey ))
            {
                StartTimeAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == startTimeAttrKey );
            }
            string startBufferAttrKey = GetAttributeValue( AttributeKey.StartBuffer );
            if (!String.IsNullOrEmpty( startBufferAttrKey ))
            {
                StartBufferAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == startBufferAttrKey );
            }
            string endTimeAttrKey = GetAttributeValue( AttributeKey.EndDateTime );
            if (!String.IsNullOrEmpty( endTimeAttrKey ))
            {
                EndTimeAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == endTimeAttrKey );
            }
            string endBufferAttrKey = GetAttributeValue( AttributeKey.EndBuffer );
            if (!String.IsNullOrEmpty( endBufferAttrKey ))
            {
                EndBufferAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == endBufferAttrKey );
            }
            string locationAttrKey = GetAttributeValue( AttributeKey.Rooms );
            if (!String.IsNullOrEmpty( locationAttrKey ))
            {
                LocationAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == locationAttrKey );
            }
            string ministryAttrKey = GetAttributeValue( AttributeKey.MinistryAttrKey );
            if (!String.IsNullOrEmpty( ministryAttrKey ))
            {
                MinistryAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == ministryAttrKey );
            }
            string contactAttrKey = GetAttributeValue( AttributeKey.ContactAttrKey );
            if (!String.IsNullOrEmpty( contactAttrKey ))
            {
                ContactAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == contactAttrKey );
            }
            string isSameAttrKey = GetAttributeValue( AttributeKey.IsSameAttrKey );
            if (!String.IsNullOrEmpty( isSameAttrKey ))
            {
                IsSameAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == isSameAttrKey );
            }
            string needsSpaceAttrKey = GetAttributeValue( AttributeKey.NeedsSpaceAttrKey );
            if (!String.IsNullOrEmpty( needsSpaceAttrKey ))
            {
                NeedsSpaceAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsSpaceAttrKey );
            }
            string needsOnlineAttrKey = GetAttributeValue( AttributeKey.NeedsOnlineAttrKey );
            if (!String.IsNullOrEmpty( needsOnlineAttrKey ))
            {
                NeedsOnlineAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsOnlineAttrKey );
            }
            string needsPublicityAttrKey = GetAttributeValue( AttributeKey.NeedsPublicityAttrKey );
            if (!String.IsNullOrEmpty( needsPublicityAttrKey ))
            {
                NeedsPublicityAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsPublicityAttrKey );
            }
            string needsRegistrationAttrKey = GetAttributeValue( AttributeKey.NeedsRegistrationAttrKey );
            if (!String.IsNullOrEmpty( needsRegistrationAttrKey ))
            {
                NeedsRegistrationAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsRegistrationAttrKey );
            }
            string needsCalendarAttrKey = GetAttributeValue( AttributeKey.NeedsCalendarAttrKey );
            if (!String.IsNullOrEmpty( needsCalendarAttrKey ))
            {
                NeedsCalendarAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsCalendarAttrKey );
            }
            string needsCateringAttrKey = GetAttributeValue( AttributeKey.NeedsCateringAttrKey );
            if (!String.IsNullOrEmpty( needsCateringAttrKey ))
            {
                NeedsCateringAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsCateringAttrKey );
            }
            string needsChildcareAttrKey = GetAttributeValue( AttributeKey.NeedsChildcareAttrKey );
            if (!String.IsNullOrEmpty( needsChildcareAttrKey ))
            {
                NeedsChildcareAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsChildcareAttrKey );
            }
            string needsChildcareCateringAttrKey = GetAttributeValue( AttributeKey.NeedsChildcareCateringAttrKey );
            if (!String.IsNullOrEmpty( needsChildcareCateringAttrKey ))
            {
                NeedsChildcareCateringAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsChildcareCateringAttrKey );
            }
            string needsOpsAttrKey = GetAttributeValue( AttributeKey.NeedsOpsAttrKey );
            if (!String.IsNullOrEmpty( needsOpsAttrKey ))
            {
                NeedsOpsAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsOpsAttrKey );
            }
            string needsProductionAttrKey = GetAttributeValue( AttributeKey.NeedsProductionAttrKey );
            if (!String.IsNullOrEmpty( needsProductionAttrKey ))
            {
                NeedsProductionAttr = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == needsProductionAttrKey );
            }

            Guid locationGuid = Guid.Empty;
            if (Guid.TryParse( GetAttributeValue( AttributeKey.LocationList ), out locationGuid ))
            {
                LocationDT = new DefinedTypeService( rockContext ).Get( locationGuid );
                var locs = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == LocationDT.Id ).ToList();
                locs.LoadAttributes();
                Locations = locs;
            }
            Guid ministryGuid = Guid.Empty;
            if (Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryGuid ))
            {
                MinistryDT = new DefinedTypeService( rockContext ).Get( ministryGuid );
                var min = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == MinistryDT.Id );
                min.LoadAttributes();
                Ministries = min.ToList();
            }
        }

        /// <summary>
        /// Return true/false is the current person a member of the given Security Role
        /// </summary>
        /// <returns></returns>
        private bool CheckSecurityRole( RockContext context, string attrKey )
        {
            bool hasRole = false;
            Rock.Model.Person p = GetCurrentPerson();
            Guid securityRoleGuid = Guid.Empty;
            //A role was configured and the current person is not null
            if (Guid.TryParse( GetAttributeValue( attrKey ), out securityRoleGuid ) && p != null)
            {
                Rock.Model.Group securityRole = new GroupService( context ).Get( securityRoleGuid );
                if (securityRole.Members.Select( gm => gm.PersonId ).Contains( p.Id ))
                {
                    hasRole = true;
                }
            }
            return hasRole;
        }

        private List<EventFormCalendar> GetEventDataSQL( DateTime start, DateTime end )
        {
            using (RockContext context = new RockContext())
            {
                var sqlParams = new SqlParameter[] {
                    new SqlParameter( "@eventDetailsContentChannelId", EventDetailsContentChannelId ),
                    new SqlParameter( "@start", start.ToString( "yyyy-MM-dd" ) ),
                    new SqlParameter( "@end", end.ToString( "yyyy-MM-dd" ) ),
                    new SqlParameter( "@eventDatesAttrId", EventDatesAttr.Id ),
                    new SqlParameter( "@eventDateAttrId", EventDateAttr.Id ),
                    new SqlParameter( "@requestStatusAttrId", RequestStatusAttr.Id ),
                    new SqlParameter( "@ministryAttrId", MinistryAttr.Id ),
                    new SqlParameter( "@ministryContactAttrId", ContactAttr.Id ),
                    new SqlParameter( "@isSameAttrId", IsSameAttr.Id ),
                    new SqlParameter( "@needsSpaceAttrId", NeedsSpaceAttr.Id ),
                    new SqlParameter( "@needsOnlineAttrId", NeedsOnlineAttr.Id ),
                    new SqlParameter( "@needsPublicityAttrId", NeedsPublicityAttr.Id ),
                    new SqlParameter( "@needsRegistrationAttrId", NeedsRegistrationAttr.Id ),
                    new SqlParameter( "@needsCalendarAttrId", NeedsCalendarAttr.Id ),
                    new SqlParameter( "@needsCateringAttrId", NeedsCateringAttr.Id ),
                    new SqlParameter( "@needsChildcareAttrId", NeedsChildcareAttr.Id ),
                    new SqlParameter( "@needsChildcareCateringAttrId", NeedsChildcareCateringAttr.Id ),
                    new SqlParameter( "@needsOpsAttrId", NeedsOpsAttr.Id ),
                    new SqlParameter( "@needsProductionAttrId", NeedsProductionAttr.Id ),
                    new SqlParameter( "@startTimeAttrId", StartTimeAttr.Id ),
                    new SqlParameter( "@startBufferAttrId", StartBufferAttr.Id ),
                    new SqlParameter( "@endTimeAttrId", EndTimeAttr.Id ),
                    new SqlParameter( "@endBufferAttrId", EndBufferAttr.Id ),
                    new SqlParameter( "@roomAttrId", LocationAttr.Id ),
                    new SqlParameter( "@infrastructureSpaceAttrId", InfrastructureSpaceAttr.Id ),
                    new SqlParameter( "@ministryDTId", MinistryDT.Id ),
                    new SqlParameter( "@locationDTId", LocationDT.Id ) };

                var query = context.Database.SqlQuery<EventData>( $@"
SELECT ParentId AS 'parentId',
       ChildId AS 'id',
       Title AS 'title',
       RequestStatus AS 'requestStatus',
       IsSame AS 'isSame',
       EventDate AS 'eventDate',
       CAST(CONCAT(LEFT(EventDate, 10), ' ', LEFT(StartTime, 8)) AS DateTime) AS 'start',
       CAST(CONCAT(LEFT(EventDate, 10), ' ', LEFT(AdjustedStartTime, 8)) AS DateTime) AS 'adjustedStart',
       CAST(AdjustedStartTime AS varchar(8)) AS 'adjustedStartTime', 
       StartTime AS 'startTime',
       (CASE WHEN StartBuffer IS NOT NULL THEN CAST(StartBuffer AS int) ELSE 0 END) AS 'startBuffer',
       CAST(CONCAT(LEFT(EventDate, 10), ' ', LEFT(EndTime, 8)) AS DateTime) AS 'end',
       CAST(CONCAT(LEFT(EventDate, 10), ' ', LEFT(AdjustedEndTime, 8)) AS DateTime) AS 'adjustedEnd',
       CAST(AdjustedEndTime AS varchar(8)) AS 'adjustedEndTime',
       EndTime AS 'endTime',
       (CASE WHEN EndBuffer IS NOT NULL THEN CAST(EndBuffer AS int) ELSE 0 END) AS 'endBuffer',
       Calendar AS 'calendar',
       STRING_AGG(Room, ', ') AS 'location',
       CreatedDateTime AS 'createdDateTime',
       CreatedByPersonId AS 'createdByPersonId',
       CreatedByPersonName AS 'createdByPersonName',
       ModifiedDateTime AS 'modifiedDateTime',
       ModifiedByPersonId AS 'modifiedByPersonId',
       ModifiedByPersonName AS 'modifiedByPersonName',
       NeedsSpace AS 'needsSpace',
       NeedsOnline AS 'needsOnline',
       NeedsPublicity AS 'needsPublicity',
       NeedsRegistration AS 'needsRegistration',
       NeedsCalendar AS 'needsCalendar',
       NeedsCatering AS 'needsCatering',
       NeedsChildcare AS 'needsChildcare',
       NeedsChildcareCatering AS 'needsChildcareCatering',
       NeedsOps AS 'needsOps',
       NeedsProduction AS 'needsProduction',
       MinistryContact AS 'contact',
       Ministry AS 'ministry',
       InfrastructureSpaces AS 'infrastructureSpaces'
FROM (
         SELECT ParentId,
                ChildId,
                Title,
                RequestStatus,
                IsSame,
                EventDate,
                AdjustedStartTime,
                StartTime,
                StartBuffer,
                (CASE WHEN AdjustedEndTime = '00:00:00' THEN '23:59:00' ELSE AdjustedEndTime END) AS 'AdjustedEndTime',
                (CASE WHEN EndTime = '00:00:00' THEN '23:59:00' ELSE EndTime END) AS 'EndTime',
                EndBuffer,
                DefinedValue.Value   AS 'Room',
                AttributeValue.Value AS 'Calendar',
                rooms.CreatedDateTime,
                CreatedByPersonId,
                CreatedByPersonName,
                rooms.ModifiedDateTime,
                ModifiedByPersonId,
                ModifiedByPersonName,
                NeedsSpace,
                NeedsOnline,
                NeedsPublicity,
                NeedsRegistration,
                NeedsCalendar,
                NeedsCatering,
                NeedsChildcare,
                NeedsChildcareCatering,
                NeedsOps,
                NeedsProduction,
                MinistryContact,
                Ministry,
                InfrastructureSpaces
         FROM (
                  SELECT ParentId,
                         ChildId,
                         Title,
                         RequestStatus,
                         IsSame,
                         EventDates,
                         EventDate,
                         AdjustedStartTime,
                         StartTime,
                         StartBuffer,
                         AdjustedEndTime,
                         EndTime,
                         EndBuffer,
                         TRIM(value) AS RoomGuid,
                         CreatedDateTime,
                         CreatedByPersonId,
                         CreatedByPersonName,
                         ModifiedDateTime,
                         ModifiedByPersonId,
                         ModifiedByPersonName,
                         NeedsSpace,
                         NeedsOnline,
                         NeedsPublicity,
                         NeedsRegistration,
                         NeedsCalendar,
                         NeedsCatering,
                         NeedsChildcare,
                         NeedsChildcareCatering,
                         NeedsOps,
                         NeedsProduction,
                         MinistryContact,
                         Ministry,
                         InfrastructureSpaces
                  FROM (
                           SELECT ParentId,
                                  ChildId,
                                  Title,
                                  RequestStatus,
                                  IsSame,
                                  EventDates,
                                  TRIM(value) AS 'EventDate',
                                  AdjustedStartTime,
                                  StartTime,
                                  StartBuffer,
                                  AdjustedEndTime,
                                  EndTime,
                                  EndBuffer,
                                  CreatedDateTime,
                                  CreatedByPersonId,
                                  CreatedByPersonName,
                                  ModifiedDateTime,
                                  ModifiedByPersonId,
                                  ModifiedByPersonName,
                                  NeedsSpace,
                                  NeedsOnline,
                                  NeedsPublicity,
                                  NeedsRegistration,
                                  NeedsCalendar,
                                  NeedsCatering,
                                  NeedsChildcare,
                                  NeedsChildcareCatering,
                                  NeedsOps,
                                  NeedsProduction,
                                  MinistryContact,
                                  Ministry,
                                  RoomGuids,
                                  InfrastructureSpaces
                           FROM (
                                    SELECT ParentId,
                                           ChildId,
                                           Title,
                                           RequestStatus,
                                           IsSame,
                                           (CASE WHEN IsSame = 1 THEN EventDates ELSE EventDate END) AS 'EventDates',
                                           CreatedDateTime,
                                           CreatedByPersonId,
                                           CreatedByPersonName,
                                           ModifiedDateTime,
                                           ModifiedByPersonId,
                                           ModifiedByPersonName,
                                           NeedsSpace,
                                           NeedsOnline,
                                           NeedsPublicity,
                                           NeedsRegistration,
                                           NeedsCalendar,
                                           NeedsCatering,
                                           NeedsChildcare,
                                           NeedsChildcareCatering,
                                           NeedsOps,
                                           NeedsProduction,
                                           MinistryContact,
                                           Ministry,
                                           StartTime,
                                           StartBuffer,
                                           (CASE
                                                WHEN StartBuffer IS NOT NULL AND StartBuffer > 0
                                                    THEN CAST(DATEADD(N, -1 * StartBuffer, CAST(StartTime AS DATETIME)) AS Time)
                                                ELSE StartTime END)                                  AS 'AdjustedStartTime',
                                           EndTime,
                                           EndBuffer,
                                           (CASE
                                                WHEN EndBuffer IS NOT NULL AND EndBuffer > 0
                                                    THEN CAST(DATEADD(N, EndBuffer, CAST(EndTime AS DATETIME)) AS Time)
                                                ELSE EndTime END)                                    AS 'AdjustedEndTime',
                                           RoomGuids,
                                           InfrastructureSpaces
                                    FROM (
                                             SELECT ParentId,
                                                    ChildContentChannelItemId AS 'ChildId',
                                                    parentWithAVs.Title,
                                                    RequestStatus,
                                                    EventDates,
                                                    parentWithAVs.CreatedDateTime,
                                                    CreatedByPersonId,
                                                    CreatedByPersonName,
                                                    parentWithAVs.ModifiedDateTime,
                                                    ModifiedByPersonId,
                                                    ModifiedByPersonName,
                                                    NeedsSpace,
                                                    NeedsOnline,
                                                    NeedsPublicity,
                                                    NeedsRegistration,
                                                    NeedsCalendar,
                                                    NeedsCatering,
                                                    NeedsChildcare,
                                                    NeedsChildcareCatering,
                                                    NeedsOps,
                                                    NeedsProduction,
                                                    IsSame,
                                                    MinistryContact,
                                                    Ministry
                                             FROM (
                                                      SELECT *
                                                      FROM (
                                                               SELECT DISTINCT Id AS 'ParentId',
                                                                               Title,
                                                                               RequestStatus,
                                                                               CreatedDateTime,
                                                                               CreatedByPersonAliasId,
                                                                               ModifiedDateTime,
                                                                               ModifiedByPersonAliasId
                                                               FROM ContentChannelItem
                                                                        INNER JOIN (
                                                                   SELECT EntityId, TRIM(value) AS 'Event Date'
                                                                   FROM (
                                                                            SELECT EntityId, Value AS 'Dates'
                                                                            FROM AttributeValue
                                                                            WHERE AttributeId = @eventDatesAttrId 
                                                                        ) AS dates
                                                                            CROSS APPLY STRING_SPLIT(Dates, ',')
                                                                   WHERE TRIM(value) BETWEEN @start AND @end
                                                               ) AS filterDates
                                                                                   ON EntityId = Id
                                                                        CROSS APPLY (SELECT Value AS 'RequestStatus'
                                                                                     FROM AttributeValue
                                                                                     WHERE AttributeId = @requestStatusAttrId 
                                                                                       AND ContentChannelItem.Id = AttributeValue.EntityId) AS status
                                                               WHERE [RequestStatus] NOT LIKE 'Cancelled%'
                                                                 AND [RequestStatus] != 'Denied'
                                                                 AND [RequestStatus] != 'Draft'
                                                                 AND [RequestStatus] != 'Submitted'
                                                           ) AS parentItems
                                                               CROSS APPLY (
                                                          SELECT Value AS 'EventDates'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @eventDatesAttrId 
                                                            AND EntityId = ParentId
                                                      ) AS eventDates
                                                               CROSS APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsSpace'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsSpaceAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsSpace
                                                               OUTER APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsOnline'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsOnlineAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsOnline
                                                               OUTER APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsPublicity'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsPublicityAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsPublicity
                                                               OUTER APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsRegistration'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsRegistrationAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsRegistration
                                                               OUTER APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsCalendar'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsCalendarAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsCalendar
                                                               OUTER APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsCatering'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsCateringAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsCatering
                                                               OUTER APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsChildcare'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsChildcareAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsChildcare
                                                               OUTER APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsChildcareCatering'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsChildcareCateringAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsChildcareCatering
                                                               OUTER APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsOps'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsOpsAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsOps
                                                               OUTER APPLY (
                                                          SELECT ValueAsBoolean AS 'NeedsProduction'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @needsProductionAttrId
                                                            AND EntityId = ParentId
                                                      ) AS needsProduction
                                                               CROSS APPLY (
                                                          SELECT ValueAsBoolean AS 'IsSame'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @isSameAttrId
                                                            AND EntityId = ParentId
                                                      ) AS isSame
                                                               OUTER APPLY (
                                                          SELECT Value AS 'MinistryGuid'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @ministryAttrId
                                                            AND EntityId = ParentId
                                                      ) AS ministryGuid
                                                               OUTER APPLY (
                                                          SELECT Value AS 'MinistryContact'
                                                          FROM AttributeValue
                                                          WHERE AttributeId = @ministryContactAttrId
                                                            AND EntityId = ParentId
                                                      ) AS contact
                                                      WHERE NeedsSpace = 1
                                                  ) AS parentWithAVs
                                                      LEFT OUTER JOIN (
                                                 SELECT Value AS 'Ministry', Guid
                                                 FROM DefinedValue
                                                 WHERE DefinedTypeId = @ministryDTId
                                             ) AS dv ON MinistryGuid = Guid
                                                      LEFT OUTER JOIN (
                                                 SELECT Id, PersonId AS 'CreatedByPersonId'
                                                 FROM PersonAlias
                                             ) AS cbpa
                                                                      ON cbpa.Id = CreatedByPersonAliasId
                                                      LEFT OUTER JOIN (
                                                 SELECT Id, PersonId AS 'ModifiedByPersonId'
                                                 FROM PersonAlias
                                             ) AS mbpa
                                                                      ON mbpa.Id = ModifiedByPersonAliasId
                                                      LEFT OUTER JOIN (
                                                 SELECT Id, CONCAT(NickName, ' ', LastName) AS 'CreatedByPersonName'
                                                 FROM Person
                                             ) AS cbp ON CreatedByPersonId = cbp.Id
                                                      LEFT OUTER JOIN (
                                                 SELECT Id, CONCAT(NickName, ' ', LastName) AS 'ModifiedByPersonName'
                                                 FROM Person
                                             ) AS mbp ON ModifiedByPersonId = mbp.Id
                                                      INNER JOIN ContentChannelItemAssociation ON ContentChannelItemId = ParentId
                                                      INNER JOIN ContentChannelItem
                                                                 ON ChildContentChannelItemId =
                                                                    ContentChannelItem.Id AND
                                                                    ContentChannelId =
                                                                    @eventDetailsContentChannelId
                                         ) AS ccia
                                             OUTER APPLY (
                                        SELECT CONVERT(char(10), Value, 126) AS 'EventDate'
                                        FROM AttributeValue
                                        WHERE AttributeId = @eventDateAttrId
                                          AND EntityId = ChildId
                                    ) AS EventDate
                                             OUTER APPLY (
                                        SELECT Value AS 'StartTime'
                                        FROM AttributeValue
                                        WHERE AttributeId = @startTimeAttrId
                                          AND EntityId = ChildId
                                    ) AS StartTime
                                             OUTER APPLY (
                                        SELECT Value AS 'EndTime'
                                        FROM AttributeValue
                                        WHERE AttributeId = @endTimeAttrId
                                          AND EntityId = ChildId
                                    ) AS EndTime
                                             OUTER APPLY (
                                        SELECT ValueAsNumeric AS 'StartBuffer'
                                        FROM AttributeValue
                                        WHERE AttributeId = @startBufferAttrId
                                          AND EntityId = ChildId
                                    ) AS StartBuffer
                                             OUTER APPLY (
                                        SELECT ValueAsNumeric AS 'EndBuffer'
                                        FROM AttributeValue
                                        WHERE AttributeId = @endBufferAttrId
                                          AND EntityId = ChildId
                                    ) AS EndBuffer
                                             CROSS APPLY (
                                        SELECT Value AS 'RoomGuids'
                                        FROM AttributeValue
                                        WHERE AttributeId = @roomAttrId
                                          AND EntityId = ChildId
                                    ) AS RoomGuids
                                             OUTER APPLY (
                                        SELECT Value AS 'InfrastructureSpaces'
                                        FROM AttributeValue
                                        WHERE AttributeId = @infrastructureSpaceAttrId
                                          AND EntityId = ChildId
                                    ) AS InfrastructureSpaces
                                ) AS childWithAVs
                                    CROSS APPLY STRING_SPLIT(EventDates, ',')
                       ) AS eventDates
                           CROSS APPLY STRING_SPLIT(RoomGuids, ',')
              ) AS rooms
                  INNER JOIN DefinedValue
                             ON RoomGuid = CAST(Guid AS varchar(100)) AND DefinedTypeId = @locationDTId
                  LEFT OUTER JOIN AttributeValue ON AttributeId = 18373 AND EntityId = DefinedValue.Id
     ) AS events
GROUP BY ParentId, ChildId, Title, RequestStatus, IsSame, EventDate,
         AdjustedStartTime, StartTime, StartBuffer,
         AdjustedEndTime, EndTime, EndBuffer, CreatedDateTime, CreatedByPersonId,
         CreatedByPersonName, ModifiedDateTime,
         ModifiedByPersonId, ModifiedByPersonName, NeedsSpace, NeedsOnline,
         NeedsPublicity, NeedsRegistration,
         NeedsCalendar, NeedsCatering, NeedsChildcare, NeedsChildcareCatering,
         NeedsOps, NeedsProduction, Ministry,
         MinistryContact, InfrastructureSpaces, Calendar
", sqlParams ).ToList();

                var colors = GetAttributeValue( AttributeKey.CalendarColors ).Split( '|' ).ToList();
                return query.GroupBy( e => e.calendar ).Select( c =>
                {
                    var color = colors.FirstOrDefault( col => col.Split( '^' )[0] == c.Key );
                    return new EventFormCalendar
                    {
                        name = c.Key,
                        events = c.OrderBy( e => e.start ).ToList(),
                        color = color != null ? ("rgba(" + color.Split( '^' )[1] + ", .7)") : "rgba(78, 135, 140, .7)",
                        border = color != null ? ("rgba(" + color.Split( '^' )[1] + ", 1)") : "rgba(78, 135, 140, 1)"
                    };
                } ).ToList();
            }
        }

        #endregion Helpers

        private class CalendarBlockViewModel
        {
            public List<ContentChannelItemBag> events { get; set; }
            public List<DefinedValue> locations { get; set; }
            public List<DefinedValue> ministries { get; set; }
            public AttributeBag requestStatus { get; set; }
            public AttributeBag requestType { get; set; }
            public string formUrl { get; set; }
            public string dashboardUrl { get; set; }
            public bool isEventAdmin { get; set; }
        }

        private class EventFormCalendar
        {
            public string name { get; set; }
            public string color { get; set; }
            public string border { get; set; }
            public List<EventData> events { get; set; }
        }

        private class EventData
        {
            public int parentId { get; set; }
            public int id { get; set; }
            public string title { get; set; }
            public string requestStatus { get; set; }
            public bool isSame { get; set; }
            public string eventDate { get; set; }
            public DateTime start { get; set; }
            public DateTime adjustedStart { get; set; }
            public string adjustedStartTime { get; set; }
            public string startTime { get; set; }
            public int startBuffer { get; set; }
            public DateTime end { get; set; }
            public DateTime adjustedEnd { get; set; }
            public string adjustedEndTime { get; set; }
            public string endTime { get; set; }
            public int endBuffer { get; set; }
            public string calendar { get; set; }
            public string location { get; set; }
            public DateTime createdDateTime { get; set; }
            public int createdByPersonId { get; set; }
            public string createdByPersonName { get; set; }
            public DateTime modifiedDateTime { get; set; }
            public int modifiedByPersonId { get; set; }
            public string modifiedByPersonName { get; set; }
            public bool needsSpace { get; set; }
            public bool needsOnline { get; set; }
            public bool needsPublicity { get; set; }
            public bool needsRegistration { get; set; }
            public bool needsCalendar { get; set; }
            public bool needsCatering { get; set; }
            public bool needsChildcare { get; set; }
            public bool needsChildcareCatering { get; set; }
            public bool needsOps { get; set; }
            public bool needsProduction { get; set; }
            public String contact { get; set; }
            public String ministry { get; set; }
            public String infrastructureSpaces { get; set; }
        }

    }
}
