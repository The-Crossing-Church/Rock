using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Data;
using Rock.Financial;
using Rock.Model;
using Rock.Tasks;
using Rock.ViewModel;
using Rock.ViewModel.Controls;
using Rock.ViewModel.NonEntities;
using Rock.Web.Cache;
using Rock.SystemGuid;

namespace Rock.Blocks.Plugin.EventForm
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Submission Form" )]
    [Category( "Obsidian > Plugin > Event Form" )]
    [Description( "Obsidian Event Submission Form" )]
    [IconCssClass( "fa fa-calendar-check" )]

    #region Block Attributes

    [ContentChannelField( "Event Content Channel", key: AttributeKey.EventContentChannel, category: "General", required: true, order: 0 )]
    [ContentChannelField( "Event Details Content Channel", key: AttributeKey.EventDetailsContentChannel, category: "General", required: true, order: 1 )]
    [DefinedTypeField( "Locations Defined Type", key: AttributeKey.LocationList, category: "Lists", required: true, order: 0 )]
    [DefinedTypeField( "Ministries Defined Type", key: AttributeKey.MinistryList, category: "Lists", required: true, order: 1 )]
    [DefinedTypeField( "Budgets Defined Type", key: AttributeKey.BudgetList, category: "Lists", required: true, order: 2 )]
    [LinkedPage( "Event Submission Form", key: AttributeKey.SubmissionPage, category: "Pages", required: true, order: 0 )]
    [LinkedPage( "Admin Dashboard", key: AttributeKey.AdminDashboard, category: "Pages", required: true, order: 1 )]
    [LinkedPage( "User Dashboard", key: AttributeKey.UserDashboard, category: "Pages", required: true, order: 2 )]
    [SecurityRoleField( "Event Request Admin", key: AttributeKey.EventAdminRole, category: "Security", required: true, order: 0 )]
    [SecurityRoleField( "Room Request Admin", key: AttributeKey.RoomAdminRole, category: "Security", required: true, order: 1 )]
    [SecurityRoleField( "Super User", key: AttributeKey.SuperUserRole, category: "Security", required: true, order: 2 )]
    [AttributeField( name: "Request Status", key: AttributeKey.RequestStatusAttr, allowMultiple: false, category: "Attributes", entityTypeGuid: SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, entityTypeQualifierColumn: "ContentChannelTypeId", entityTypeQualifierValue: "16", order: 0 )]
    [AttributeField( name: "Event Dates", key: AttributeKey.EventDatesAttr, allowMultiple: false, category: "Attributes", entityTypeGuid: SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, entityTypeQualifierColumn: "ContentChannelTypeId", entityTypeQualifierValue: "16", order: 1 )]
    [AttributeField( name: "Details are Same", key: AttributeKey.IsSameAttr, allowMultiple: false, category: "Attributes", entityTypeGuid: SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, entityTypeQualifierColumn: "ContentChannelTypeId", entityTypeQualifierValue: "16", order: 2 )]
    [TextField( "Details Event Date", "Attribute Key for Event Date on Details", key: AttributeKey.DetailsEventDate, defaultValue: "EventDate", category: "Attributes", order: 3 )]
    [TextField( "Start Time Key", "Attribute Key for Start Time", key: AttributeKey.StartDateTime, defaultValue: "StartTime", category: "Attributes", order: 4 )]
    [TextField( "End Time Key", "Attribute Key for End Time", key: AttributeKey.EndDateTime, defaultValue: "EndTime", category: "Attributes", order: 5 )]
    [TextField( "Room", "Attribute Key for Room", key: AttributeKey.Rooms, defaultValue: "Rooms", category: "Attribute", order: 6 )]
    [TextField( "Start Buffer", "Attribute Key for Start Buffer", key: AttributeKey.StartBuffer, defaultValue: "StartBuffer", category: "Attributes", order: 7 )]
    [TextField( "End Buffer", "Attribute Key for End Buffer", key: AttributeKey.EndBuffer, defaultValue: "EndBuffer", category: "Attributes", order: 8 )]
    #endregion Block Attributes

    public class SubmissionForm : RockObsidianBlockType
    {
        #region Keys

        /// <summary>
        /// Attribute Key
        /// </summary>
        private static class AttributeKey
        {
            public const string EventContentChannel = "EventContentChannel";
            public const string EventDetailsContentChannel = "EventDetailsContentChannel";
            public const string LocationList = "LocationList";
            public const string MinistryList = "MinistryList";
            public const string BudgetList = "BudgetList";
            public const string MinistryBudgetList = "MinistryBudgetList";
            public const string SubmissionPage = "SubmissionPage";
            public const string AdminDashboard = "AdminDashboard";
            public const string UserDashboard = "UserDashboard";
            public const string EventAdminRole = "EventAdminRole";
            public const string RoomAdminRole = "RoomAdminRole";
            public const string SuperUserRole = "SuperUserRole";
            public const string EventDatesAttr = "EventDatesAttr";
            public const string IsSameAttr = "IsSameAttr";
            public const string RequestStatusAttr = "RequestStatusAttr";
            public const string DetailsEventDate = "DetailsEventDate";
            public const string StartDateTime = "StartDateTime";
            public const string EndDateTime = "EndDateTime";
            public const string Rooms = "Rooms";
            public const string StartBuffer = "StartBuffer";
            public const string EndBuffer = "EndBuffer";
        }

        /// <summary>
        /// Page Parameter
        /// </summary>
        private static class PageParameterKey
        {
            public const string RequestId = "Id";
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
            using ( var rockContext = new RockContext() )
            {
                Guid eventCCGuid = Guid.Empty;
                Guid eventDetailsCCGuid = Guid.Empty;
                Guid eventDatesAttrGuid = Guid.Empty;
                Guid requestStatusAttrGuid = Guid.Empty;
                Guid isSameAttrGuid = Guid.Empty;
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventContentChannel ), out eventCCGuid ) )
                {
                    ContentChannel cc = new ContentChannelService( rockContext ).Get( eventCCGuid );
                    EventContentChannelId = cc.Id;
                    EventContentChannelTypeId = cc.ContentChannelTypeId;
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventDetailsContentChannel ), out eventDetailsCCGuid ) )
                    {
                        ContentChannel dCC = new ContentChannelService( rockContext ).Get( eventDetailsCCGuid );
                        EventDetailsContentChannelId = dCC.Id;
                        EventDetailsContentChannelTypeId = dCC.ContentChannelTypeId;
                    }
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventDatesAttr ), out eventDatesAttrGuid ) )
                {
                    EventDatesAttrGuid = eventDatesAttrGuid;
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.RequestStatusAttr ), out requestStatusAttrGuid ) )
                {
                    RequestStatusAttrGuid = requestStatusAttrGuid;
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.IsSameAttr ), out isSameAttrGuid ) )
                {
                    IsSameAttrGuid = isSameAttrGuid;
                }

                SubmissionFormViewModel viewModel = LoadRequest();
                viewModel.isSuperUser = CheckSecurityRole( rockContext, AttributeKey.SuperUserRole );
                viewModel.isEventAdmin = CheckSecurityRole( rockContext, AttributeKey.EventAdminRole );
                viewModel.isRoomAdmin = CheckSecurityRole( rockContext, AttributeKey.RoomAdminRole );
                viewModel.permissions = SetPermissions( viewModel.request, viewModel.isEventAdmin );
                viewModel.existing = LoadExisting( viewModel.request.Id );
                viewModel.existingDetails = viewModel.existing.SelectMany( cci => cci.ChildItems ).ToList();

                //Lists
                Guid locationGuid = Guid.Empty;
                Guid ministryGuid = Guid.Empty;
                Guid budgetLineGuid = Guid.Empty;
                var p = GetCurrentPerson();
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.LocationList ), out locationGuid ) )
                {
                    Rock.Model.DefinedType locationDT = new DefinedTypeService( rockContext ).Get( locationGuid );
                    var locs = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == locationDT.Id ).ToList().Select( l => l.ToViewModel( p, true ) );
                    viewModel.locations = locs.ToList();
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryGuid ) )
                {
                    Rock.Model.DefinedType ministryDT = new DefinedTypeService( rockContext ).Get( ministryGuid );
                    var min = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == ministryDT.Id );
                    min.LoadAttributes();
                    viewModel.ministries = min.ToList();
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.BudgetList ), out budgetLineGuid ) )
                {
                    Rock.Model.DefinedType budgetDT = new DefinedTypeService( rockContext ).Get( locationGuid );
                    var budget = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == budgetDT.Id );
                    budget.LoadAttributes();
                    viewModel.budgetLines = budget.ToList();
                }

                return viewModel;
            }
        }

        #endregion Obsidian Block Type Overrides

        #region Properties

        private int EventContentChannelId { get; set; }
        private int EventContentChannelTypeId { get; set; }
        private int EventDetailsContentChannelId { get; set; }
        private int EventDetailsContentChannelTypeId { get; set; }
        private Guid EventDatesAttrGuid { get; set; }
        private Guid RequestStatusAttrGuid { get; set; }
        private Guid IsSameAttrGuid { get; set; }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult Save( ContentChannelItemViewModel viewModel, List<ContentChannelItemViewModel> events )
        {
            try
            {
                int id = SaveRequest( viewModel, events );
                return ActionOk( new { success = true, id = id } );
            }
            catch ( Exception e )
            {
                return ActionInternalServerError( e.Message );
            }
        }
        [BlockAction]
        public BlockActionResult Submit( ContentChannelItemViewModel viewModel, List<ContentChannelItemViewModel> events )
        {
            try
            {
                int id = SaveRequest( viewModel, events );
                //Send Notifications
                return ActionOk( new { success = true, id = id } );
            }
            catch ( Exception e )
            {
                return ActionInternalServerError( e.Message );
            }
        }

        #endregion Block Actions

        #region Helpers
        /// <summary>
        /// Loads the request or returns a new CCI
        /// </summary>
        /// <returns></returns>
        private SubmissionFormViewModel LoadRequest()
        {
            int? id = PageParameter( PageParameterKey.RequestId ).AsIntegerOrNull();
            ContentChannelItem item = new ContentChannelItem();
            var p = GetCurrentPerson();
            if ( id.HasValue )
            {
                item = new ContentChannelItemService( new RockContext() ).Get( id.Value );
                //item.ChildItems.Select( ci => ci.ChildContentChannelItem ).LoadAttributes();
                //item.LoadAttributes();
                return new SubmissionFormViewModel
                {
                    request = item.ToViewModel( p, true ),
                    events = item.ChildItems.Where( cd => cd.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci => ci.ChildContentChannelItem.ToViewModel( p, true ) ).ToList()
                };
            }
            else
            {
                item.ContentChannelId = EventContentChannelId;
                item.ContentChannelTypeId = EventContentChannelTypeId;
                var details = new ContentChannelItem() { ContentChannelId = EventDetailsContentChannelId, ContentChannelTypeId = EventDetailsContentChannelTypeId };
                return new SubmissionFormViewModel
                {
                    request = item.ToViewModel( p, true ),
                    events = new List<ContentChannelItemViewModel>() { details.ToViewModel( p, true ) }
                };
            }
        }

        private List<ContentChannelItem> LoadExisting( int id )
        {
            List<ContentChannelItem> items = new List<ContentChannelItem>();
            RockContext context = new RockContext();
            ContentChannelItemService svc = new ContentChannelItemService( context );
            AttributeValueService av_svc = new AttributeValueService( context );
            AttributeService attr_svc = new AttributeService( context );
            if ( EventDatesAttrGuid != Guid.Empty )
            {
                var eventAttr = attr_svc.Get( EventDatesAttrGuid );
                var requestAttr = attr_svc.Get( RequestStatusAttrGuid );
                var qryItems = svc.Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId );
                var qryStatusAttrs = av_svc.Queryable().Where( av => av.AttributeId == requestAttr.Id && ( av.Value == "In Progress" || av.Value == "Approved" || av.Value == "Pending Changes" || av.Value == "Proposed Changes Denied" || av.Value == "Changes Accepted by User" ) );
                //Filter to Active Requests
                qryItems = qryItems.Join( qryStatusAttrs,
                    i => i.Id,
                    av => av.EntityId,
                    ( i, av ) => i
                );
                var qryEventAttrs = new List<AttributeValue>();
                if ( id > 0 )
                {
                    //Filter to Requests that intersect dates or are in future
                    DateTime today = new DateTime( RockDateTime.Now.Year, RockDateTime.Now.Month, RockDateTime.Now.Day, 0, 0, 0 );
                    var itemEventDates = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == eventAttr.Id && av.EntityId == id );
                    qryEventAttrs = av_svc.Queryable().Where( av => av.AttributeId == eventAttr.Id ).ToList().Where( av =>
                    {
                        bool inFuture = false;
                        bool intersects = false;
                        List<DateTime> dates = av.Value.Split( ',' ).Select( d => DateTime.Parse( d ) ).ToList();
                        for ( int i = 0; i < dates.Count(); i++ )
                        {
                            if ( dates[i] >= today )
                            {
                                inFuture = true;
                            }
                        }
                        if ( itemEventDates != null )
                        {
                            IEnumerable<string> both = av.Value.Split( ',' ).Intersect( itemEventDates.Value.Split( ',' ) );
                            if ( both.Count() > 0 )
                            {
                                intersects = true;
                            }
                        }
                        return inFuture || intersects;
                    } ).ToList();
                    qryItems = qryItems.ToList().Join( qryEventAttrs,
                        i => i.Id,
                        av => av.EntityId,
                        ( i, av ) => i
                    ).AsQueryable();
                }
                else
                {
                    //Filter to Requests in Future
                    DateTime today = new DateTime( RockDateTime.Now.Year, RockDateTime.Now.Month, RockDateTime.Now.Day, 0, 0, 0 );
                    qryEventAttrs = av_svc.Queryable().Where( av => av.AttributeId == eventAttr.Id ).ToList().Where( av =>
                    {
                        bool inFuture = false;
                        List<DateTime> dates = av.Value.Split( ',' ).Select( d => DateTime.Parse( d ) ).ToList();
                        for ( int i = 0; i < dates.Count(); i++ )
                        {
                            if ( dates[i] >= today )
                            {
                                inFuture = true;
                            }
                        }
                        return inFuture;
                    } ).ToList();
                    qryItems = qryItems.ToList().Join( qryEventAttrs,
                        i => i.Id,
                        av => av.EntityId,
                        ( i, av ) => i
                    ).AsQueryable();
                }
                string startKey = GetAttributeValue( AttributeKey.StartDateTime );
                string endKey = GetAttributeValue( AttributeKey.EndDateTime );
                string roomKey = GetAttributeValue( AttributeKey.Rooms );
                string dateKey = GetAttributeValue( AttributeKey.DetailsEventDate );
                string sBufferKey = GetAttributeValue( AttributeKey.StartBuffer );
                string eBufferKey = GetAttributeValue( AttributeKey.EndBuffer );
                var startTimeAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == startKey );
                var endTimeAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == endKey );
                var roomAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == roomKey );
                var eventDateAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == dateKey );
                var sBufferAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == sBufferKey );
                var eBufferAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == eBufferKey );
                var isSameAttr = attr_svc.Get( IsSameAttrGuid );
                if ( startTimeAttr != null && endTimeAttr != null && roomAttr != null && eventDateAttr != null && isSameAttr != null )
                {
                    var startTimes = av_svc.Queryable().Where( av => av.AttributeId == startTimeAttr.Id );
                    var endTimes = av_svc.Queryable().Where( av => av.AttributeId == endTimeAttr.Id );
                    var rooms = av_svc.Queryable().Where( av => av.AttributeId == roomAttr.Id );
                    var dates = av_svc.Queryable().Where( av => av.AttributeId == eventDateAttr.Id );
                    var isSameVals = av_svc.Queryable().Where( av => av.AttributeId == isSameAttr.Id );
                    var sBuffers = av_svc.Queryable().Where( av => av.AttributeId == sBufferAttr.Id );
                    var eBuffers = av_svc.Queryable().Where( av => av.AttributeId == eBufferAttr.Id );
                    items = qryItems.ToList().Select( i =>
                    {
                        if ( i.AttributeValues == null )
                        {
                            i.AttributeValues = new Dictionary<string, AttributeValueCache>();
                        }
                        var eventDate = qryEventAttrs.FirstOrDefault( q => q.EntityId == i.Id );
                        if ( eventDate != null )
                        {
                            i.AttributeValues.Add( eventAttr.Key, new AttributeValueCache() { Value = eventDate.Value, AttributeId = eventAttr.Id } );
                        }
                        var isSame = isSameVals.FirstOrDefault( q => q.EntityId == i.Id );
                        if ( isSame != null )
                        {
                            i.AttributeValues.Add( isSameAttr.Key, new AttributeValueCache() { Value = isSame.Value, AttributeId = isSameAttr.Id } );
                        }
                        i.ChildItems = i.ChildItems.Select( ci =>
                        {
                            if ( ci.ChildContentChannelItem.AttributeValues == null )
                            {
                                ci.ChildContentChannelItem.AttributeValues = new Dictionary<string, AttributeValueCache>();
                            }
                            var date = dates.FirstOrDefault( q => q.EntityId == ci.ChildContentChannelItem.Id );
                            if ( date != null )
                            {
                                ci.ChildContentChannelItem.AttributeValues.Add( eventDateAttr.Key, new AttributeValueCache() { Value = date.Value, AttributeId = eventDateAttr.Id } );
                            }
                            var startTime = startTimes.FirstOrDefault( q => q.EntityId == ci.ChildContentChannelItem.Id );
                            if ( startTime != null )
                            {
                                ci.ChildContentChannelItem.AttributeValues.Add( startTimeAttr.Key, new AttributeValueCache() { Value = startTime.Value, AttributeId = startTimeAttr.Id } );
                            }
                            var endTime = endTimes.FirstOrDefault( q => q.EntityId == ci.ChildContentChannelItem.Id );
                            if ( endTime != null )
                            {
                                ci.ChildContentChannelItem.AttributeValues.Add( endTimeAttr.Key, new AttributeValueCache() { Value = endTime.Value, AttributeId = endTimeAttr.Id } );
                            }
                            var room = rooms.FirstOrDefault( q => q.EntityId == ci.ChildContentChannelItem.Id );
                            if ( room != null )
                            {
                                ci.ChildContentChannelItem.AttributeValues.Add( roomAttr.Key, new AttributeValueCache() { Value = room.Value, AttributeId = roomAttr.Id } );
                            }
                            var sBuffer = sBuffers.FirstOrDefault( q => q.EntityId == ci.ChildContentChannelItem.Id );
                            if ( sBuffer != null )
                            {
                                ci.ChildContentChannelItem.AttributeValues.Add( sBufferAttr.Key, new AttributeValueCache() { Value = sBuffer.Value, AttributeId = sBufferAttr.Id } );
                            }
                            var eBuffer = eBuffers.FirstOrDefault( q => q.EntityId == ci.ChildContentChannelItem.Id );
                            if ( eBuffer != null )
                            {
                                ci.ChildContentChannelItem.AttributeValues.Add( eBufferAttr.Key, new AttributeValueCache() { Value = eBuffer.Value, AttributeId = eBufferAttr.Id } );
                            }

                            return ci;
                        } ).ToList();

                        return i;
                    } ).ToList();
                }
            }
            return items;
        }

        private int SaveRequest( ContentChannelItemViewModel viewModel, List<ContentChannelItemViewModel> events )
        {
            RockContext context = new RockContext();
            ContentChannelItem item = FromViewModel( viewModel );
            var cciSvc = new ContentChannelItemService( context );
            if ( item.Id == 0 )
            {
                cciSvc.Add( item );
                context.SaveChanges();
            }
            for ( int i = 0; i < events.Count(); i++ )
            {
                var detail = FromViewModel( events[i] );
                var needsAssociation = false;
                if ( detail.Id == 0 )
                {
                    if ( !String.IsNullOrEmpty( detail.GetAttributeValue( "EventDate" ) ) )
                    {
                        detail.Title = item.Title + ": " + detail.GetAttributeValue( "EventDate" );
                    }
                    else
                    {
                        detail.Title = item.Title;
                    }
                    cciSvc.Add( detail );
                    needsAssociation = true;
                }
                context.SaveChanges();
                if ( needsAssociation )
                {
                    var assocSvc = new ContentChannelItemAssociationService( context );
                    var order = assocSvc.Queryable().AsNoTracking()
                        .Where( a => a.ContentChannelItemId == item.Id )
                        .Select( a => ( int? ) a.Order )
                        .DefaultIfEmpty()
                        .Max();
                    var assoc = new ContentChannelItemAssociation();
                    assoc.ContentChannelItemId = item.Id;
                    assoc.ChildContentChannelItemId = detail.Id;
                    assoc.Order = order.HasValue ? order.Value + 1 : 0;
                    assocSvc.Add( assoc );
                    context.SaveChanges();
                }
                detail.SaveAttributeValues( context );
            }
            item.SaveAttributeValues( context );
            return item.Id;
        }

        /// <summary>
        /// Return true/false is the current person a member of the given Security Role
        /// </summary>
        /// <returns></returns>
        private bool CheckSecurityRole( RockContext rockContext, string attrKey )
        {
            bool hasRole = false;
            Rock.Model.Person p = GetCurrentPerson();
            Guid securityRoleGuid = Guid.Empty;
            //A role was configured and the current person is not null
            if ( Guid.TryParse( GetAttributeValue( attrKey ), out securityRoleGuid ) && p != null )
            {
                Rock.Model.Group securityRole = new GroupService( rockContext ).Get( securityRoleGuid );
                if ( securityRole.Members.Select( gm => gm.PersonId ).Contains( p.Id ) )
                {
                    hasRole = true;
                }
            }
            return hasRole;
        }

        private List<string> SetPermissions( ContentChannelItemViewModel item, bool isAdmin )
        {
            List<string> permissions = new List<string>();
            Rock.Model.Person p = GetCurrentPerson();

            //Admin Permissions
            if ( isAdmin )
            {
                permissions.Add( "Edit" );
                return permissions;
            }

            //User Permissions
            //TODO Cooksey: Finish Permissions for View vs Edit access
            if ( item.Id > 0 )
            {
                Rock.Model.Person createdBy = new PersonAliasService( new RockContext() ).Get( item.CreatedByPersonAliasId.Value ).Person;
                Rock.Model.Person modifiedBy = new PersonAliasService( new RockContext() ).Get( item.ModifiedByPersonAliasId.Value ).Person;
                if ( createdBy.Id == p.Id || modifiedBy.Id == p.Id )
                {
                    permissions.Add( "Edit" );
                }
                else
                {
                    permissions.Add( "View" );
                }
            }
            else
            {
                permissions.Add( "Edit" );
            }
            return permissions;
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

        #endregion Helpers

        public class SubmissionFormViewModel
        {
            public ContentChannelItemViewModel request { get; set; }
            public List<ContentChannelItemViewModel> events { get; set; }
            public List<ContentChannelItem> existing { get; set; }
            public List<ContentChannelItemAssociation> existingDetails { get; set; }
            public bool isSuperUser { get; set; }
            public bool isEventAdmin { get; set; }
            public bool isRoomAdmin { get; set; }
            public List<string> permissions { get; set; }
            public List<DefinedValueViewModel> locations { get; set; }
            public List<Rock.Model.DefinedValue> ministries { get; set; }
            public List<Rock.Model.DefinedValue> budgetLines { get; set; }
        }
    }
}
