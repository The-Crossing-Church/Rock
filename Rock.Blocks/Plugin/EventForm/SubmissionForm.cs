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
    [ContentChannelField( "Event Changes Content Channel", key: AttributeKey.EventChangesContentChannel, category: "General", required: true, order: 2 )]
    [ContentChannelField( "Event Details Changes Content Channel", key: AttributeKey.EventDetailsChangesContentChannel, category: "General", required: true, order: 3 )]
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
            public const string EventChangesContentChannel = "EventChangesContentChannel";
            public const string EventDetailsChangesContentChannel = "EventDetailsChangesContentChannel";
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
            SetProperties();

            SubmissionFormViewModel viewModel = LoadRequest();
            viewModel.isSuperUser = CheckSecurityRole( context, AttributeKey.SuperUserRole );
            viewModel.isEventAdmin = CheckSecurityRole( context, AttributeKey.EventAdminRole );
            viewModel.isRoomAdmin = CheckSecurityRole( context, AttributeKey.RoomAdminRole );
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
                Rock.Model.DefinedType locationDT = new DefinedTypeService( context ).Get( locationGuid );
                var locs = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == locationDT.Id ).ToList();
                viewModel.locations = locs.Select( l => l.ToViewModel( p, true ) ).ToList();
                string templateIdVal;
                int templateid;
                var setUpAttr = locs.First().Attributes["StandardSetUp"];
                setUpAttr.ConfigurationValues.TryGetValue( "attributematrixtemplate", out templateIdVal );
                if ( !String.IsNullOrEmpty( templateIdVal ) )
                {
                    if ( Int32.TryParse( templateIdVal, out templateid ) )
                    {
                        var guids = viewModel.locations.Select( l => l.AttributeValues["StandardSetUp"] ).ToList();
                        AttributeMatrixService am_svc = new AttributeMatrixService( context );
                        var ams = am_svc.Queryable().Where( am => am.AttributeMatrixTemplateId == templateid && guids.Contains( am.Guid.ToString() ) );
                        var amis = ams.SelectMany( am => am.AttributeMatrixItems ).ToList();
                        viewModel.locationSetupMatrix = ams.ToList().Select( am => am.ToViewModel( null, false ) ).ToList();
                        viewModel.locationSetupMatrixItem = amis.ToList().Select( ami => ami.ToViewModel( null, true ) ).ToList();
                    }
                }
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryGuid ) )
            {
                Rock.Model.DefinedType ministryDT = new DefinedTypeService( context ).Get( ministryGuid );
                var min = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == ministryDT.Id );
                min.LoadAttributes();
                viewModel.ministries = min.ToList();
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.BudgetList ), out budgetLineGuid ) )
            {
                Rock.Model.DefinedType budgetDT = new DefinedTypeService( context ).Get( locationGuid );
                var budget = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == budgetDT.Id );
                budget.LoadAttributes();
                viewModel.budgetLines = budget.ToList();
            }

            return viewModel;
        }

        #endregion Obsidian Block Type Overrides

        #region Properties

        private int EventContentChannelId { get; set; }
        private int EventContentChannelTypeId { get; set; }
        private int EventDetailsContentChannelId { get; set; }
        private int EventDetailsContentChannelTypeId { get; set; }
        private int EventChangesContentChannelId { get; set; }
        private int EventDetailsChangesContentChannelId { get; set; }
        private Guid EventDatesAttrGuid { get; set; }
        private Guid RequestStatusAttrGuid { get; set; }
        private Guid IsSameAttrGuid { get; set; }
        private RockContext context { get; set; }

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
                //todo Send Notifications

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
            RockContext context = new RockContext();
            int? id = PageParameter( PageParameterKey.RequestId ).AsIntegerOrNull();
            ContentChannelItem item = new ContentChannelItem();
            var p = GetCurrentPerson();
            SubmissionFormViewModel viewModel = new SubmissionFormViewModel();
            //Linked Page URLs
            string adminVal = GetAttributeValue( AttributeKey.AdminDashboard );
            List<Guid> adminGuids = new List<Guid>();
            if ( adminVal.Contains( "," ) )
            {
                adminGuids = adminVal.Split( ',' ).Select( g => Guid.Parse( g ) ).ToList();
            }
            else
            {
                adminGuids.Add( Guid.Parse( adminVal ) );
            }
            if ( adminGuids.Count() > 1 )
            {
                //Use Page Route
                Rock.Model.PageRoute route = new PageRouteService( context ).Get( adminGuids.Last() );
                viewModel.adminDashboardURL = route.Route;
            }
            else
            {
                //Use Page Id
                Rock.Model.Page page = new PageService( context ).Get( adminGuids.First() );
                viewModel.adminDashboardURL = "page/" + page.Id.ToString();
            }
            string userVal = GetAttributeValue( AttributeKey.UserDashboard );
            List<Guid> userGuids = new List<Guid>();
            if ( userVal.Contains( "," ) )
            {
                userGuids = userVal.Split( ',' ).Select( g => Guid.Parse( g ) ).ToList();
            }
            else
            {
                userGuids.Add( Guid.Parse( userVal ) );
            }
            if ( userGuids.Count() > 1 )
            {
                //Use Page Route
                Rock.Model.PageRoute route = new PageRouteService( context ).Get( userGuids.Last() );
                viewModel.userDashboardURL = route.Route;
            }
            else
            {
                //Use Page Id
                Rock.Model.Page page = new PageService( context ).Get( userGuids.First() );
                viewModel.userDashboardURL = "page/" + page.Id.ToString();
            }

            if ( id.HasValue )
            {
                item = new ContentChannelItemService( context ).Get( id.Value );
                viewModel.request = item.ToViewModel( p, true );
                viewModel.events = item.ChildItems.Where( cd => cd.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci => ci.ChildContentChannelItem.ToViewModel( p, true ) ).ToList();
                var changes = item.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventChangesContentChannelId );
                if ( changes != null )
                {
                    viewModel.originalRequest = viewModel.request;
                    viewModel.request = changes.ChildContentChannelItem.ToViewModel( p, true );
                    //Don't want the tite to show as "Changes"
                    viewModel.request.Title = viewModel.request.Title.Substring( 0, viewModel.request.Title.Length - 8 );
                    viewModel.events = item.ChildItems.Where( cd => cd.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).SelectMany( i => i.ChildContentChannelItem.ChildItems ).Where( i => i.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId ).Select( ci => ci.ChildContentChannelItem.ToViewModel( p, true ) ).ToList();
                }
                string status = "";
                if ( viewModel.request.AttributeValues.TryGetValue( "RequestStatus", out status ) )
                {
                    if ( status == "Approved" )
                    {
                        viewModel.originalRequest = viewModel.request;
                        viewModel.request.ContentChannelId = EventChangesContentChannelId;
                    }
                }
                return viewModel;
            }
            else
            {
                item.ContentChannelId = EventContentChannelId;
                item.ContentChannelTypeId = EventContentChannelTypeId;
                var details = new ContentChannelItem() { ContentChannelId = EventDetailsContentChannelId, ContentChannelTypeId = EventDetailsContentChannelTypeId };
                viewModel.request = item.ToViewModel( p, true );
                viewModel.events = new List<ContentChannelItemViewModel>() { details.ToViewModel( p, true ) };
                return viewModel;
            }
        }

        private List<ContentChannelItem> LoadExisting( int id )
        {
            List<ContentChannelItem> items = new List<ContentChannelItem>();
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
                        i.ChildItems = i.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci =>
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
                              else
                              {
                                  //Need to be able to filter out requests without rooms
                                  ci.ChildContentChannelItem.AttributeValues.Add( roomAttr.Key, new AttributeValueCache() { Value = "", AttributeId = roomAttr.Id } );
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
                    } ).Where( i => !i.ChildItems.Any( ci => ci.ChildContentChannelItem.AttributeValues[roomAttr.Key].Value == "" ) ).ToList();
                }
            }
            return items;
        }

        private int SaveRequest( ContentChannelItemViewModel viewModel, List<ContentChannelItemViewModel> events )
        {
            SetProperties();
            ContentChannelItem item = FromViewModel( viewModel );
            var cciSvc = new ContentChannelItemService( context );
            var assocSvc = new ContentChannelItemAssociationService( context );
            var avSvc = new AttributeValueService( context );
            var p = GetCurrentPerson();
            item.ModifiedByPersonAliasId = p.PrimaryAliasId;
            item.ModifiedDateTime = RockDateTime.Now;
            ContentChannelItem original = null;
            var currentStatus = item.GetAttributeValue( "RequestStatus" );

            if ( viewModel.ContentChannelId == EventChangesContentChannelId && currentStatus == "Approved" )
            {
                ContentChannelItem changes = new ContentChannelItem()
                {
                    ContentChannelTypeId = item.ContentChannelTypeId,
                    ContentChannelId = viewModel.ContentChannelId,
                    Title = item.Title + " Changes"
                };
                changes.LoadAttributes();
                foreach ( var av in item.AttributeValues )
                {
                    changes.SetAttributeValue( av.Key, av.Value.Value );
                }
                changes.SetAttributeValue( "RequestStatus", "Pending Changes" );
                original = cciSvc.Get( item.Id );
                original.SetAttributeValue( "RequestStatus", "Pending Changes" );
                original.SaveAttributeValue( "RequestStatus" );
                item = changes;
            }

            if ( item.Id == 0 )
            {
                item.CreatedByPersonAliasId = p.PrimaryAliasId;
                item.CreatedDateTime = item.ModifiedDateTime;
                cciSvc.Add( item );
                if ( original != null )
                {
                    //Create Association for Pending Changes
                    var order = assocSvc.Queryable().AsNoTracking()
                        .Where( a => a.ContentChannelItemId == original.Id )
                        .Select( a => ( int? ) a.Order )
                        .DefaultIfEmpty()
                        .Max();
                    var assoc = new ContentChannelItemAssociation();
                    assoc.ContentChannelItemId = original.Id;
                    assoc.ChildContentChannelItemId = item.Id;
                    assoc.Order = order.HasValue ? order.Value + 1 : 0;
                    assocSvc.Add( assoc );
                    context.SaveChanges();
                }
            }
            context.SaveChanges();

            //For Pre-Approval Check
            List<PreApprovalData> eventDates = new List<PreApprovalData>();
            AttributeCache roomAttr = null;
            AttributeCache eventDateAttr = null;
            AttributeCache startTimeAttr = null;
            AttributeCache endTimeAttr = null;
            AttributeCache startBufferAttr = null;
            AttributeCache endBufferAttr = null;

            for ( int i = 0; i < events.Count(); i++ )
            {
                var detail = FromViewModel( events[i] );
                var needsAssociation = false;
                ContentChannelItem originalDetail = null;

                //For Pre-Approval Check
                roomAttr = detail.Attributes[GetAttributeValue( AttributeKey.Rooms )];
                eventDateAttr = detail.Attributes[GetAttributeValue( AttributeKey.DetailsEventDate )];
                startTimeAttr = detail.Attributes[GetAttributeValue( AttributeKey.StartDateTime )];
                endTimeAttr = detail.Attributes[GetAttributeValue( AttributeKey.EndDateTime )];
                startBufferAttr = detail.Attributes[GetAttributeValue( AttributeKey.StartBuffer )];
                endBufferAttr = detail.Attributes[GetAttributeValue( AttributeKey.EndBuffer )];

                if ( events.Count() == 1 )
                {
                    var dates = item.GetAttributeValue( "EventDates" ).Split( ',' );
                    for ( int k = 0; k < dates.Length; k++ )
                    {
                        DateTime start = DateTime.Parse( $"{dates[k]} {detail.GetAttributeValue( "StartTime" )}" );
                        DateTime end = DateTime.Parse( $"{dates[k]} {detail.GetAttributeValue( "EndTime" )}" );
                        DateRange r = new DateRange() { Start = start, End = end };
                        PreApprovalData d = new PreApprovalData() { range = r, rooms = detail.GetAttributeValue( "Rooms" ), attendance = detail.GetAttributeValue( "ExpectedAttendance" ) };
                        eventDates.Add( d );
                    }
                }
                else
                {
                    DateTime start = DateTime.Parse( $"{detail.GetAttributeValue( "EventDate" )} {detail.GetAttributeValue( "StartTime" )}" );
                    DateTime end = DateTime.Parse( $"{detail.GetAttributeValue( "EventDate" )} {detail.GetAttributeValue( "EndTime" )}" );
                    DateRange r = new DateRange() { Start = start, End = end };
                    PreApprovalData d = new PreApprovalData() { range = r, rooms = detail.GetAttributeValue( "Rooms" ), attendance = detail.GetAttributeValue( "ExpectedAttendance" ) };
                    eventDates.Add( d );
                }

                if ( original != null )
                {
                    ContentChannelItem changes = new ContentChannelItem()
                    {
                        ContentChannelTypeId = detail.ContentChannelTypeId,
                        ContentChannelId = EventDetailsChangesContentChannelId,
                        Title = detail.Title
                    };
                    changes.LoadAttributes();
                    foreach ( var av in detail.AttributeValues )
                    {
                        changes.SetAttributeValue( av.Key, av.Value.Value );
                    }
                    originalDetail = cciSvc.Get( detail.Id );
                    detail = changes;
                }
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
                    int? order;
                    var assoc = new ContentChannelItemAssociation();
                    if ( original != null )
                    {
                        order = assocSvc.Queryable().AsNoTracking()
                            .Where( a => a.ContentChannelItemId == originalDetail.Id )
                            .Select( a => ( int? ) a.Order )
                            .DefaultIfEmpty()
                            .Max();
                        assoc.ContentChannelItemId = originalDetail.Id;
                    }
                    else
                    {
                        order = assocSvc.Queryable().AsNoTracking()
                            .Where( a => a.ContentChannelItemId == item.Id )
                            .Select( a => ( int? ) a.Order )
                            .DefaultIfEmpty()
                            .Max();
                        assoc.ContentChannelItemId = item.Id;
                    }
                    assoc.ChildContentChannelItemId = detail.Id;
                    assoc.Order = order.HasValue ? order.Value + 1 : 0;
                    assocSvc.Add( assoc );
                    context.SaveChanges();
                }
                detail.SaveAttributeValues( context );
            }

            //Pre-Approval Check
            List<string> notValidForPreApprovalReasons = new List<string>();
            item.SetAttributeValue( "IsPreApproved", "False" );
            //Request is Valid
            if ( item.GetAttributeValue( "RequestIsValid" ) == "True" )
            {
                //Room Request Only
                if ( item.GetAttributeValue( "RequestType" ) == "Room" )
                {
                    //Analyze Date Details
                    DateRange twoWeeks = new DateRange() { Start = DateTime.Now, End = DateTime.Now.AddDays( 14 ) };
                    twoWeeks.Start = twoWeeks.Start.Value.StartOfDay();
                    twoWeeks.End = twoWeeks.End.Value.EndOfDay();
                    bool inRange = true;
                    bool validRooms = true;
                    bool validAttendance = true;
                    bool noConflicts = true;
                    for ( int i = 0; i < eventDates.Count(); i++ )
                    {
                        if ( inRange )
                        {
                            if ( !twoWeeks.Contains( eventDates[i].range.Start.Value ) )
                            {
                                inRange = false;
                                notValidForPreApprovalReasons.Add( "Request is not within the next 14 days." );
                            }
                            else
                            {
                                if ( eventDates[i].range.Start.Value.DayOfWeek == DayOfWeek.Saturday )
                                {
                                    //out of range
                                    inRange = false;
                                    notValidForPreApprovalReasons.Add( "Request is not within normal business hours." );
                                }
                                else
                                {
                                    DateRange businessHours = new DateRange();
                                    businessHours.End = DateTime.Parse( $"{eventDates[i].range.Start.Value.ToString( "yyyy-MM-dd" )} 21:00:00" );
                                    if ( eventDates[i].range.Start.Value.DayOfWeek == DayOfWeek.Sunday )
                                    {
                                        //1pm to 9pm
                                        businessHours.Start = DateTime.Parse( $"{eventDates[i].range.Start.Value.ToString( "yyyy-MM-dd" )} 13:00:00" );
                                    }
                                    else
                                    {
                                        //9am to 9pm
                                        businessHours.Start = DateTime.Parse( $"{eventDates[i].range.Start.Value.ToString( "yyyy-MM-dd" )} 09:00:00" );
                                    }

                                    //Check within business hours
                                    if ( !businessHours.Contains( eventDates[i].range.Start.Value ) || !businessHours.Contains( eventDates[i].range.End.Value ) )
                                    {
                                        inRange = false;
                                        notValidForPreApprovalReasons.Add( "Request is not within normal business hours." );
                                    }
                                }
                            }
                        }
                        //Attendance <= 30
                        if ( validAttendance )
                        {
                            if ( !String.IsNullOrEmpty( eventDates[i].attendance ) )
                            {
                                int attendance = Int32.Parse( eventDates[i].attendance );
                                if ( attendance > 30 )
                                {
                                    validAttendance = false;
                                    notValidForPreApprovalReasons.Add( "Attendance can not be more than 30 people." );
                                }
                            }
                        }
                        //Not Gym or Aud
                        if ( validRooms )
                        {
                            if ( !String.IsNullOrEmpty( eventDates[i].rooms ) )
                            {
                                var rooms = eventDates[i].rooms.Split( ',' );
                                //todo change to guid
                                if ( rooms.Contains( "Auditorium" ) || rooms.Contains( "Gym" ) )
                                {
                                    validRooms = false;
                                    notValidForPreApprovalReasons.Add( "Request is for spaces that require approval." );
                                }
                            }
                        }
                        //Conflicts
                        if ( noConflicts )
                        {
                            if ( roomAttr != null && eventDateAttr != null && startTimeAttr != null && endTimeAttr != null && startBufferAttr != null && endBufferAttr != null )
                            {
                                if ( EventDatesAttrGuid != Guid.Empty && RequestStatusAttrGuid != Guid.Empty && IsSameAttrGuid != Guid.Empty )
                                {
                                    string dateCompareVal = eventDates[i].range.Start.Value.ToString( "yyyy-MM-dd" );
                                    var items = cciSvc.Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId && cci.Id != item.Id );
                                    //Requests that are on the calendar
                                    var statusAttr = new AttributeService( context ).Get( RequestStatusAttrGuid );
                                    var statusValues = avSvc.Queryable().Where( av => av.AttributeId == statusAttr.Id && ( av.Value != "Draft" && av.Value != "Submitted" && av.Value != "Denied" && !av.Value.Contains( "Cancelled" ) ) );
                                    items = items.Join( statusValues,
                                        itm => itm.Id,
                                        av => av.EntityId,
                                        ( itm, av ) => itm
                                    );
                                    var eventDetails = items.Select( itm => itm.ChildItems.FirstOrDefault( ccia => ccia.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).ChildContentChannelItem );
                                    //Filter items to isSame, others will be in the eventDetails list
                                    var isSameAttr = new AttributeService( context ).Get( IsSameAttrGuid );
                                    var isSameValues = avSvc.Queryable().Where( av => av.AttributeId == isSameAttr.Id && av.Value == "True" );
                                    items = items.Join( isSameValues,
                                        itm => itm.Id,
                                        av => av.EntityId,
                                        ( itm, av ) => itm
                                    );
                                    //Events on same date
                                    var eventAttr = new AttributeService( context ).Get( EventDatesAttrGuid );
                                    var eventDateValues = avSvc.Queryable().Where( av => ( av.AttributeId == eventDateAttr.Id && av.Value == dateCompareVal ) || ( av.AttributeId == eventAttr.Id && av.Value.Contains( dateCompareVal ) ) );
                                    eventDetails = eventDetails.Join( eventDateValues,
                                        itm => itm.Id,
                                        av => av.EntityId,
                                        ( itm, av ) => itm
                                    );
                                    items = items.Join( eventDateValues,
                                        itm => itm.Id,
                                        av => av.EntityId,
                                        ( itm, av ) => itm
                                    );
                                    var ccItems = items.Select( itm => itm.ChildItems.FirstOrDefault( ccia => ccia.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).ChildContentChannelItem ).ToList();
                                    ccItems.AddRange( eventDetails );
                                    //Events with overlapping rooms
                                    var roomValues = avSvc.Queryable().Where( av => av.AttributeId == roomAttr.Id ).ToList().Where( av =>
                                     {
                                         if ( av.AttributeId == roomAttr.Id )
                                         {
                                             var intersection = av.Value.Split( ',' ).Intersect( eventDates[i].rooms.Split( ',' ) );
                                             if ( intersection.Count() > 0 )
                                             {
                                                 return true;
                                             }
                                         }
                                         return false;
                                     } );
                                    ccItems = ccItems.Join( roomValues,
                                        itm => itm.Id,
                                        av => av.EntityId,
                                        ( itm, av ) => itm
                                    ).ToList();
                                    //Check Times overlap
                                    ccItems.LoadAttributes();
                                    ccItems = ccItems.Where( itm =>
                                     {
                                         DateRange r = new DateRange();
                                         r.Start = DateTime.Parse( $"{dateCompareVal} {itm.GetAttributeValue( "StartTime" )}" );
                                         r.End = DateTime.Parse( $"{dateCompareVal} {itm.GetAttributeValue( "EndTime" )}" );
                                         var startBuffer = itm.GetAttributeValue( startBufferAttr.Key );
                                         var endBuffer = itm.GetAttributeValue( endBufferAttr.Key );
                                         if ( !String.IsNullOrEmpty( startBuffer ) )
                                         {
                                             int buffer = Int32.Parse( startBuffer );
                                             r.Start.Value.AddMinutes( buffer * -1 );
                                         }
                                         if ( !String.IsNullOrEmpty( endBuffer ) )
                                         {
                                             int buffer = Int32.Parse( endBuffer );
                                             r.End.Value.AddMinutes( buffer );
                                         }
                                         if ( r.Contains( eventDates[i].range.Start.Value ) || r.Contains( eventDates[i].range.End.Value ) )
                                         {
                                             return true;
                                         }
                                         return false;
                                     } ).ToList();
                                    if ( ccItems.Count() > 0 )
                                    {
                                        noConflicts = false;
                                        notValidForPreApprovalReasons.Add( "Request conflicts with another request." );
                                    }
                                }
                            }
                        }
                    }
                    if ( inRange && validAttendance && validRooms && noConflicts )
                    {
                        item.SetAttributeValue( "IsPreApproved", "True" );
                    }
                }
                else
                {
                    notValidForPreApprovalReasons.Add( "More than a physical space was requested." );
                }
            }
            else
            {
                notValidForPreApprovalReasons.Add( "All information was not filled out." );
            }
            notValidForPreApprovalReasons = notValidForPreApprovalReasons.Distinct().ToList();
            item.SaveAttributeValues( context );
            return item.Id;
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
            if ( Guid.TryParse( GetAttributeValue( attrKey ), out securityRoleGuid ) && p != null )
            {
                Rock.Model.Group securityRole = new GroupService( context ).Get( securityRoleGuid );
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
                Rock.Model.PersonAlias createdBy = null;
                if ( item.CreatedByPersonAliasId.HasValue )
                {
                    createdBy = new PersonAliasService( new RockContext() ).Get( item.CreatedByPersonAliasId.Value );
                }
                Rock.Model.PersonAlias modifiedBy = null;
                if ( item.ModifiedByPersonAliasId.HasValue )
                {
                    modifiedBy = new PersonAliasService( new RockContext() ).Get( item.ModifiedByPersonAliasId.Value );
                }
                if ( ( createdBy != null && createdBy.PersonId == p.Id ) || ( modifiedBy != null && modifiedBy.PersonId == p.Id ) )
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
            context = new RockContext();
            Guid eventCCGuid = Guid.Empty;
            Guid eventDetailsCCGuid = Guid.Empty;
            Guid eventChangesCCGuid = Guid.Empty;
            Guid eventDetailsChangesCCGuid = Guid.Empty;
            Guid eventDatesAttrGuid = Guid.Empty;
            Guid requestStatusAttrGuid = Guid.Empty;
            Guid isSameAttrGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventContentChannel ), out eventCCGuid ) )
            {
                ContentChannel cc = new ContentChannelService( context ).Get( eventCCGuid );
                EventContentChannelId = cc.Id;
                EventContentChannelTypeId = cc.ContentChannelTypeId;
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventDetailsContentChannel ), out eventDetailsCCGuid ) )
                {
                    ContentChannel dCC = new ContentChannelService( context ).Get( eventDetailsCCGuid );
                    EventDetailsContentChannelId = dCC.Id;
                    EventDetailsContentChannelTypeId = dCC.ContentChannelTypeId;
                }
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventChangesContentChannel ), out eventChangesCCGuid ) )
            {
                ContentChannel cc = new ContentChannelService( context ).Get( eventChangesCCGuid );
                EventChangesContentChannelId = cc.Id;
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventDetailsChangesContentChannel ), out eventDetailsChangesCCGuid ) )
            {
                ContentChannel cc = new ContentChannelService( context ).Get( eventDetailsChangesCCGuid );
                EventDetailsChangesContentChannelId = cc.Id;
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
        }

        private void SubmittedNotifications( ContentChannelItem item )
        {

        }
        #endregion Helpers

        public class SubmissionFormViewModel
        {
            public ContentChannelItemViewModel request { get; set; }
            public ContentChannelItemViewModel originalRequest { get; set; }
            public List<ContentChannelItemViewModel> events { get; set; }
            public List<ContentChannelItem> existing { get; set; }
            public List<ContentChannelItemAssociation> existingDetails { get; set; }
            public bool isSuperUser { get; set; }
            public bool isEventAdmin { get; set; }
            public bool isRoomAdmin { get; set; }
            public List<string> permissions { get; set; }
            public List<DefinedValueViewModel> locations { get; set; }
            public List<AttributeMatrixViewModel> locationSetupMatrix { get; set; }
            public List<AttributeMatrixItemViewModel> locationSetupMatrixItem { get; set; }
            public List<Rock.Model.DefinedValue> ministries { get; set; }
            public List<Rock.Model.DefinedValue> budgetLines { get; set; }
            public string adminDashboardURL { get; set; }
            public string userDashboardURL { get; set; }
        }

        public class PreApprovalData
        {
            public DateRange range { get; set; }
            public string rooms { get; set; }
            public string attendance { get; set; }
        }
    }
}
