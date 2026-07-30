using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Newtonsoft.Json;

using OpenXmlPowerTools;

using Rock.Attribute;
using Rock.Blocks.Plugins.ViewModels;
using Rock.Communication;
using Rock.Data;
using Rock.Field;
using Rock.Model;
using Rock.SystemGuid;
using Rock.Web.Cache;

using static Rock.Model.StepProgram;

namespace Rock.Blocks.Plugins.EventForm
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Submission Form" )]
    [Category( "Obsidian > Plugin > Event Form" )]
    [Description( "Obsidian Event Submission Form" )]
    [IconCssClass( "fa fa-calendar-check" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes

    [ContentChannelField( "Event Content Channel", key: AttributeKey.EventContentChannel, category: "General", required: true, order: 0 )]
    [ContentChannelField( "Event Details Content Channel", key: AttributeKey.EventDetailsContentChannel, category: "General", required: true, order: 1 )]
    [ContentChannelField( "Event Changes Content Channel", key: AttributeKey.EventChangesContentChannel, category: "General", required: true, order: 2 )]
    [ContentChannelField( "Event Details Changes Content Channel", key: AttributeKey.EventDetailsChangesContentChannel, category: "General", required: true, order: 3 )]
    [ContentChannelField( "Event Comments Content Channel", key: AttributeKey.EventCommentsContentChannel, category: "General", required: true, order: 4 )]
    [DefinedTypeField( "Locations Defined Type", key: AttributeKey.LocationList, category: "Lists", required: true, order: 0 )]
    [DefinedTypeField( "Ministries Defined Type", key: AttributeKey.MinistryList, category: "Lists", required: true, order: 1 )]
    [DefinedTypeField( "Budgets Defined Type", key: AttributeKey.BudgetList, category: "Lists", required: true, order: 2 )]
    [DefinedTypeField( "Ops Inventory Defined Type", key: AttributeKey.InventoryList, category: "Lists", required: true, order: 3 )]
    [DefinedTypeField( "Preferred Vendors Defined Type", key: AttributeKey.VendorList, category: "Lists", required: true, order: 4 )]
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
    [TextField( "Ops Inventory", "Attribute Key for Ops Inventory", key: AttributeKey.OpsInventory, defaultValue: "OpsInventory", category: "Attributes", order: 9 )]
    [TextField( "Room Set-up", "Attribute Key for Room SetUp", key: AttributeKey.RoomSetUp, defaultValue: "RoomSetUp", category: "Attributes", order: 10 )]
    [TextField( "Discount Code", "Attribute Key for Discount Code", key: AttributeKey.DiscountCode, defaultValue: "DiscountCodes", category: "Attributes", order: 11 )]
    [TextField( "Discount Code Matrix Template Id", "The Id of the Attribute Matrix Template for Discount Codes", key: AttributeKey.DiscountCodeMatrix, category: "Attributes", order: 12 )]
    [BinaryFileTypeField( "Event Image File Type", "The file type all images and files associated with an event request should be assigned to", key: AttributeKey.EventImageFileType, category: "Attributes", order: 13 )]

    [GroupTypeField( "Shared Event Group Type", "Group Type of groups that allow for seeing shared requests", false, "", "Sharing", 1, AttributeKey.SharingGroupType )]
    [TextField( "Shared With Attribut Key", category: "Sharing", order: 2, key: AttributeKey.SharedWithAttr )]
    #endregion Block Attributes

    public class SubmissionForm : RockBlockType
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
            public const string EventCommentsContentChannel = "EventCommentsContentChannel";
            public const string LocationList = "LocationList";
            public const string MinistryList = "MinistryList";
            public const string BudgetList = "BudgetList";
            public const string InventoryList = "InventoryList";
            public const string VendorList = "VendorList";
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
            public const string OpsInventory = "OpsInventory";
            public const string RoomSetUp = "RoomSetUp";
            public const string DiscountCode = "DiscountCode";
            public const string DiscountCodeMatrix = "DiscountCodeMatrix";
            public const string EventImageFileType = "EventImageFileType";
            public const string SharingGroupType = "SharingGroupType";
            public const string SharedWithAttr = "SharedWithAttr";
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
            SubmissionFormViewModel viewModel = new SubmissionFormViewModel();
            try
            {
                SetProperties();

                viewModel = LoadRequest();
                viewModel.isSuperUser = CheckSecurityRole( context, AttributeKey.SuperUserRole );
                viewModel.existing = LoadExisting( viewModel.request.IdKey );
                viewModel.existingDetails = viewModel.existing.SelectMany( cci => cci.ChildItems ).ToList();

                //Lists
                Guid locationGuid = Guid.Empty;
                Guid ministryGuid = Guid.Empty;
                Guid budgetLineGuid = Guid.Empty;
                Guid inventoryGuid = Guid.Empty;
                Guid vendorGuid = Guid.Empty;
                var p = GetCurrentPerson();
                DefinedTypeService dt_svc = new DefinedTypeService( context );
                DefinedValueService dv_svc = new DefinedValueService( context );
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.LocationList ), out locationGuid ) )
                {
                    DefinedTypeCache locationDT = DefinedTypeCache.Get( locationGuid );
                    viewModel.locations = locationDT.DefinedValues;
                    string templateIdVal;
                    int templateid;
                    var setUpAttr = viewModel.locations.First().Attributes["StandardSetUp"];
                    setUpAttr.ConfigurationValues.TryGetValue( "attributematrixtemplate", out templateIdVal );
                    if ( !String.IsNullOrEmpty( templateIdVal ) )
                    {
                        if ( Int32.TryParse( templateIdVal, out templateid ) )
                        {
                            var guids = viewModel.locations.Select( l => l.AttributeValues["StandardSetUp"].Value ).ToList();
                            AttributeMatrixService am_svc = new AttributeMatrixService( context );
                            var ams = am_svc.Queryable().Where( am => am.AttributeMatrixTemplateId == templateid && guids.Contains( am.Guid.ToString() ) );
                            var amis = ams.SelectMany( am => am.AttributeMatrixItems ).ToList();
                            amis.LoadAttributes();
                            viewModel.locationSetupMatrix = ams.ToList();
                            viewModel.locationSetupMatrixItem = amis.ToList();
                        }
                    }
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryGuid ) )
                {
                    DefinedTypeCache ministryDT = DefinedTypeCache.Get( ministryGuid );
                    viewModel.ministries = ministryDT.DefinedValues;
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.BudgetList ), out budgetLineGuid ) )
                {
                    DefinedTypeCache budgetDT = DefinedTypeCache.Get( budgetLineGuid );
                    viewModel.budgetLines = budgetDT.DefinedValues;
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.InventoryList ), out inventoryGuid ) )
                {
                    DefinedTypeCache inventoryDT = DefinedTypeCache.Get( inventoryGuid );
                    viewModel.inventoryList = inventoryDT.DefinedValues;
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKey.VendorList ), out vendorGuid ) )
                {
                    DefinedTypeCache vendorDT = DefinedTypeCache.Get( vendorGuid );
                    viewModel.vendorList = vendorDT.DefinedValues;
                }
                string matrixId = GetAttributeValue( AttributeKey.DiscountCodeMatrix );
                if ( !String.IsNullOrEmpty( matrixId ) )
                {
                    viewModel.discountCodeAttrs = AttributeCache.GetByEntityTypeQualifier( 483, "AttributeMatrixTemplateId", matrixId, true ).Select( a => EventFormHelper.GetCommonAttributeEntityBag( a ) ).ToList();
                }
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                viewModel.errors = ex.Message;
            }
            return viewModel;
        }

        #endregion Obsidian Block Type Overrides

        #region Properties

        private ObsidianPluginsShared EventFormHelper = new ObsidianPluginsShared();
        private EventFormShared helper = new EventFormShared();
        private int EventContentChannelId { get; set; }
        private int EventContentChannelTypeId { get; set; }
        private int EventDetailsContentChannelId { get; set; }
        private int EventDetailsContentChannelTypeId { get; set; }
        private int EventChangesContentChannelId { get; set; }
        private int EventDetailsChangesContentChannelId { get; set; }
        private int EventCommentsContentChannelId { get; set; }
        private Guid EventDatesAttrGuid { get; set; }
        private Guid RequestStatusAttrGuid { get; set; }
        private Guid IsSameAttrGuid { get; set; }
        private RockContext context { get; set; }
        private string RoomSetUpKey { get; set; }
        private string DiscountCodeKey { get; set; }
        private string OpsInventoryKey { get; set; }
        private string ExtensiveSetUpKey { get; set; }

        #endregion

        #region Block Actions
        [BlockAction]
        public BlockActionResult ReloadRequest( string id )
        {
            try
            {
                SetProperties();
                SubmissionFormViewModel viewModel = LoadRequest( id );
                if ( !String.IsNullOrEmpty( viewModel.errors ) )
                {
                    throw new Exception( viewModel.errors );
                }
                if ( viewModel.request.ContentChannelId == EventChangesContentChannelId )
                {
                    //Need to get the events differently
                    var item = new ContentChannelItemService( context ).Get( id );
                    var parent = item.ParentItems.FirstOrDefault( pi => pi.ContentChannelItem.ContentChannelId == EventContentChannelId );
                    if ( parent != null )
                    {
                        var events = parent.ContentChannelItem.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci => ci.ChildContentChannelItem );
                        if ( events != null && events.Count() > 0 )
                        {
                            viewModel.events = events.SelectMany( ci => ci.ChildItems.Where( ccia => ccia.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId ).Select( ccci =>
                            {
                                var bag = EventFormHelper.GetCommonContentChannelItemEntityBag( ccci.ChildContentChannelItem );
                                ccci.ChildContentChannelItem.LoadAttributes();
                                bag.LoadAttributesAndValuesForPublicEdit( ccci.ChildContentChannelItem, RequestContext.CurrentPerson, false );
                                return bag;
                            } ) ).ToList();
                        }
                    }
                }
                return ActionOk( viewModel );
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
                return ActionInternalServerError( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult Save( ContentChannelItemBag viewModel, List<ContentChannelItemBag> events )
        {
            try
            {
                FormResponse r = SaveRequest( viewModel, events, false );
                r.message = "Your draft has been saved.";
                return ActionOk( r );
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
                return ActionInternalServerError( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult Submit( ContentChannelItemBag viewModel, List<ContentChannelItemBag> events )
        {
            try
            {
                string originalStatus = viewModel.AttributeValues["RequestStatus"];
                FormResponse r = SaveRequest( viewModel, events, true );
                ContentChannelItem item = new ContentChannelItemService( context ).Get( r.id );
                item.LoadAttributes();
                string currentStatus = item.GetAttributeValue( "RequestStatus" );
                if ( originalStatus == "Approved" || currentStatus == "Approved" )
                {
                    SubmittedNotifications( item );
                    SubmittedConfirmation( item );
                    r.message = "Your changes have been submitted.";
                }
                else if ( originalStatus == "Draft" )
                {
                    item.SetAttributeValue( "RequestStatus", "Submitted" );
                    item.SaveAttributeValue( "RequestStatus" );
                    SubmittedNotifications( item );
                    SubmittedConfirmation( item );
                    r.message = "Your request has been submitted.";
                }
                else if ( originalStatus == "Submitted" || originalStatus == "In Progress" )
                {
                    r.message = "Your request has been updated.";
                }
                else
                {
                    r.message = "Your changes have been submitted.";
                }

                return ActionOk( r );
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
                return ActionInternalServerError( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult AddComment( string id, string message )
        {
            try
            {
                RockContext rockContext = new RockContext();
                SetProperties();
                Rock.Model.Person p = GetCurrentPerson();
                ContentChannelItemService cci_svc = new ContentChannelItemService( rockContext );
                ContentChannel commentChannel = new ContentChannelService( rockContext ).Get( EventCommentsContentChannelId );
                ContentChannelItem request = cci_svc.Get( id );
                var authCheck = CheckRequestPermissions( new SubmissionFormViewModel(), request );
                if ( !authCheck.permissions.Contains( "Edit" ) )
                {
                    throw new UnauthorizedAccessException( "You are not authorized to comment on this request." );
                }

                ContentChannelItem comment = new ContentChannelItem()
                {
                    ContentChannelId = EventCommentsContentChannelId,
                    ContentChannelTypeId = commentChannel.ContentChannelTypeId,
                    Title = "Comment From " + p.FullName,
                    Content = message,
                    CreatedByPersonAliasId = p.PrimaryAliasId,
                    ModifiedByPersonAliasId = p.PrimaryAliasId,
                    CreatedDateTime = RockDateTime.Now,
                    ModifiedDateTime = RockDateTime.Now
                };
                cci_svc.Add( comment );
                rockContext.SaveChanges();

                //We want the request to move to the top of the stack when a note is added
                request.ModifiedDateTime = RockDateTime.Now;

                //Add association between comment and request
                var assocSvc = new ContentChannelItemAssociationService( rockContext );
                var order = assocSvc.Queryable().AsNoTracking()
                    .Where( a => a.ContentChannelItemId == request.Id )
                    .Select( a => ( int? ) a.Order )
                    .DefaultIfEmpty()
                    .Max();
                var assoc = new ContentChannelItemAssociation();
                assoc.ContentChannelItemId = request.Id;
                assoc.ChildContentChannelItemId = comment.Id;
                assoc.Order = order.HasValue ? order.Value + 1 : 0;
                assocSvc.Add( assoc );

                rockContext.SaveChanges();

                CommentNotification( comment, request );
                return ActionOk( new { createdBy = p.FullName, comment = comment } );
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
                return ActionBadRequest( e.Message );
            }
        }

        #endregion Block Actions

        #region Helpers
        /// <summary>
        /// Loads the request or returns a new CCI
        /// </summary>
        /// <returns></returns>
        private SubmissionFormViewModel LoadRequest( string altId = "" )
        {
            RockContext context = new RockContext();
            string id = PageParameter( PageParameterKey.RequestId );
            if ( !String.IsNullOrEmpty( altId ) )
            {
                id = altId;
            }
            ContentChannelItem item = new ContentChannelItem();
            var p = GetCurrentPerson();
            SubmissionFormViewModel viewModel = new SubmissionFormViewModel();
            //Linked Page URLs
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            viewModel.adminDashboardURL = this.GetLinkedPageUrl( AttributeKey.AdminDashboard, queryParams );
            viewModel.userDashboardURL = this.GetLinkedPageUrl( AttributeKey.UserDashboard, queryParams );

            if ( !String.IsNullOrEmpty( id ) )
            {
                item = new ContentChannelItemService( context ).Get( id );
                viewModel.request = EventFormHelper.GetCommonContentChannelItemEntityBag( item );
                item.LoadAttributes();
                // Verify User Permissions
                viewModel = CheckRequestPermissions( viewModel, item );
                if ( !viewModel.permissions.Any() )
                {
                    viewModel = GenerateNewRequest( new ContentChannelItem(), viewModel, p );
                    viewModel.errors = "You do not have permission to view this request";
                    return viewModel;
                }
                viewModel.request.LoadAttributesAndValuesForPublicEdit( item, p, false );
                viewModel.events = item.ChildItems.Where( cd => cd.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci =>
                {
                    var bag = EventFormHelper.GetCommonContentChannelItemEntityBag( ci.ChildContentChannelItem );
                    ci.ChildContentChannelItem.LoadAttributes();
                    bag.LoadAttributesAndValuesForPublicEdit( ci.ChildContentChannelItem, p, false );
                    return bag;
                } ).ToList();
                var changes = item.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventChangesContentChannelId );
                if ( changes != null )
                {
                    viewModel.originalRequest = viewModel.request;
                    viewModel.request = EventFormHelper.GetCommonContentChannelItemEntityBag( changes.ChildContentChannelItem );
                    changes.ChildContentChannelItem.LoadAttributes();
                    viewModel.request.LoadAttributesAndValuesForPublicEdit( changes.ChildContentChannelItem, p, false );
                    viewModel.events = item.ChildItems.Where( cd => cd.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId )
                        .SelectMany( i => i.ChildContentChannelItem.ChildItems )
                        .Where( i => i.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId )
                        .Select( ci =>
                        {
                            var bag = EventFormHelper.GetCommonContentChannelItemEntityBag( ci.ChildContentChannelItem );
                            ci.ChildContentChannelItem.LoadAttributes();
                            bag.LoadAttributesAndValuesForPublicEdit( ci.ChildContentChannelItem, p, false );
                            return bag;
                        } ).ToList();
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
                viewModel = GenerateNewRequest( item, viewModel, p );
                return viewModel;
            }
        }

        private SubmissionFormViewModel GenerateNewRequest( ContentChannelItem item, SubmissionFormViewModel viewModel, Rock.Model.Person p )
        {
            item.ContentChannelId = EventContentChannelId;
            item.ContentChannelTypeId = EventContentChannelTypeId;
            var details = new ContentChannelItem() { ContentChannelId = EventDetailsContentChannelId, ContentChannelTypeId = EventDetailsContentChannelTypeId };
            viewModel.request = EventFormHelper.GetCommonContentChannelItemEntityBag( item );
            item.LoadAttributes();
            viewModel.request.LoadAttributesAndValuesForPublicEdit( item, p, false );
            var detailBag = EventFormHelper.GetCommonContentChannelItemEntityBag( details );
            details.LoadAttributes();
            detailBag.LoadAttributesAndValuesForPublicEdit( details, p, false );
            viewModel.events = new List<ContentChannelItemBag>() { detailBag };
            viewModel.permissions = new List<string>() { "Edit" };
            return viewModel;
        }

        private List<ContentChannelItem> LoadExisting( string id )
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
                if ( !String.IsNullOrEmpty( id ) )
                {
                    int entityId = 0;
                    Int32.TryParse( id, out entityId );
                    //Filter to Requests that intersect dates or are in future
                    DateTime today = new DateTime( RockDateTime.Now.Year, RockDateTime.Now.Month, RockDateTime.Now.Day, 0, 0, 0 );
                    var itemEventDates = av_svc.Queryable().FirstOrDefault( av => av.AttributeId == eventAttr.Id && av.EntityId == entityId );
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
                string opsInvKey = GetAttributeValue( AttributeKey.OpsInventory );
                var startTimeAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == startKey );
                var endTimeAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == endKey );
                var roomAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == roomKey );
                var eventDateAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == dateKey );
                var sBufferAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == sBufferKey );
                var eBufferAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == eBufferKey );
                var opsInvAttr = attr_svc.Queryable().FirstOrDefault( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventDetailsContentChannelTypeId.ToString() && a.Key == opsInvKey );
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
                    var inventory = av_svc.Queryable().Where( av => av.AttributeId == opsInvAttr.Id );
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
                              var invItems = inventory.FirstOrDefault( q => q.EntityId == ci.ChildContentChannelItem.Id );
                              if ( invItems != null )
                              {
                                  ci.ChildContentChannelItem.AttributeValues.Add( opsInvAttr.Key, new AttributeValueCache() { Value = invItems.Value, AttributeId = opsInvAttr.Id } );
                              }

                              return ci;
                          } ).ToList();
                        return i;
                    } ).Where( i => !i.ChildItems.Any( ci => ci.ChildContentChannelItem.AttributeValues[roomAttr.Key].Value == "" ) ).ToList();
                }
            }
            return items;
        }

        private FormResponse SaveRequest( ContentChannelItemBag viewModel, List<ContentChannelItemBag> events, bool isSubmission )
        {
            SetProperties();
            ContentChannelItem item = FromViewModel( viewModel );
            var authCheck = CheckRequestPermissions( new SubmissionFormViewModel(), item );
            if ( !authCheck.permissions.Contains( "Edit" ) )
            {
                throw new UnauthorizedAccessException( "You are not authorized to edit this request." );
            }
            var cciSvc = new ContentChannelItemService( context );
            var assocSvc = new ContentChannelItemAssociationService( context );
            var avSvc = new AttributeValueService( context );
            var p = GetCurrentPerson();
            item.ModifiedByPersonAliasId = p.PrimaryAliasId;
            item.ModifiedDateTime = RockDateTime.Now;
            ContentChannelItem original = null;
            var currentStatus = item.GetAttributeValue( "RequestStatus" );

            //Pre-Approval Check
            var notValidForPreApprovalReasons = PreApprovalCheck( item, events, isSubmission );
            var isPreApproved = item.GetAttributeValue( "IsPreApproved" );

            if ( viewModel.ContentChannelId == EventChangesContentChannelId && currentStatus == "Pending Changes" && isPreApproved == "True" )
            {
                item = FromViewModel( viewModel, true );
                //The changes being requested qualify for pre-approval, go ahead and change the event
                var changesAssoc = item.ParentItems.FirstOrDefault( ci => ci.ContentChannelItem.ContentChannelId == EventContentChannelId );
                if ( changesAssoc != null )
                {
                    original = changesAssoc.ContentChannelItem;
                    original.LoadAttributes();
                    original.Title = item.Title;
                    foreach ( var av in item.AttributeValues )
                    {
                        original.SetAttributeValue( av.Key, item.AttributeValues[av.Key].Value );
                    }
                    original.SetAttributeValue( "RequestStatus", "Approved" );
                    original.SaveAttributeValues();
                }
                cciSvc.Delete( item );
                assocSvc.Delete( changesAssoc );
                item = original;

                for ( int i = 0; i < events.Count(); i++ )
                {
                    var detail = FromViewModel( events[i], true );
                    var originalEventAssoc = detail.ParentItems.FirstOrDefault();
                    if ( originalEventAssoc != null )
                    {
                        var originalEvent = originalEventAssoc.ContentChannelItem;
                        originalEvent.LoadAttributes();
                        foreach ( var av in detail.AttributeValues )
                        {
                            originalEvent.SetAttributeValue( av.Key, detail.AttributeValues[av.Key].Value );
                        }
                        originalEvent.SaveAttributeValues();
                        cciSvc.Delete( detail );
                        assocSvc.Delete( originalEventAssoc );
                    }
                }
                context.SaveChanges();
            }
            else
            {
                if ( viewModel.ContentChannelId == EventChangesContentChannelId && currentStatus == "Approved" )
                {
                    if ( isPreApproved != "True" )
                    {
                        ContentChannelItem changes = new ContentChannelItem()
                        {
                            ContentChannelTypeId = item.ContentChannelTypeId,
                            ContentChannelId = viewModel.ContentChannelId,
                            Title = item.Title
                        };
                        changes.LoadAttributes();
                        foreach ( var av in item.AttributeValues )
                        {
                            changes.SetAttributeValue( av.Key, av.Value.Value );
                        }
                        changes.SetAttributeValue( "RequestStatus", "Pending Changes" );
                        original = cciSvc.Get( item.Id );
                        original.LoadAttributes();
                        original.SetAttributeValue( "RequestStatus", "Pending Changes" );
                        original.SaveAttributeValue( "RequestStatus" );
                        item = changes;
                    }
                    else
                    {
                        //Previously approved item and changes that are pre-approved, need to update context so title changes apply on save
                        item = FromViewModel( viewModel, true );
                    }
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
                    }
                    context.SaveChanges();
                }
                item.SaveAttributeValues();

                //Check for event items that were deleted
                var existingAssociations = assocSvc.Queryable().Where( a => a.ContentChannelItemId == item.Id && a.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).ToList();
                List<ContentChannelItem> currentDetails = new List<ContentChannelItem>();


                for ( int i = 0; i < events.Count(); i++ )
                {
                    var detail = FromViewModel( events[i] );
                    var needsAssociation = false;
                    ContentChannelItem originalDetail = null;

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
                    if ( !String.IsNullOrEmpty( detail.GetAttributeValue( "EventDate" ) ) )
                    {
                        detail.Title = item.Title + ": " + detail.GetAttributeValue( "EventDate" );
                    }
                    else
                    {
                        detail.Title = item.Title;
                    }
                    if ( detail.Id == 0 )
                    {
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
                    currentDetails.Add( detail );
                }

                if ( original == null )
                {
                    var deletedAssociations = existingAssociations.Where( a => !currentDetails.Select( e => e.Id ).Contains( a.ChildContentChannelItemId ) ).ToList();
                    for ( int i = 0; i < deletedAssociations.Count; i++ )
                    {
                        cciSvc.Delete( deletedAssociations[i].ChildContentChannelItem );
                        assocSvc.Delete( deletedAssociations[i] );
                    }
                    context.SaveChanges();
                }
            }

            FormResponse res = new FormResponse() { id = item.Id, idKey = item.IdKey, notValidForPreApprovalReasons = notValidForPreApprovalReasons, isPreApproved = notValidForPreApprovalReasons.Count() == 0 };
            return res;
        }

        private List<string> PreApprovalCheck( ContentChannelItem item, List<ContentChannelItemBag> events, bool isSubmission )
        {
            var cciSvc = new ContentChannelItemService( context );
            var avSvc = new AttributeValueService( context );
            List<PreApprovalData> eventDates = new List<PreApprovalData>();
            AttributeCache roomAttr = null;
            AttributeCache detailsEventDateAttr = null;
            AttributeCache startTimeAttr = null;
            AttributeCache endTimeAttr = null;
            AttributeCache startBufferAttr = null;
            AttributeCache endBufferAttr = null;
            List<string> notValidForPreApprovalReasons = new List<string>();

            for ( int i = 0; i < events.Count(); i++ )
            {
                var detail = FromViewModel( events[i] );
                roomAttr = detail.Attributes[GetAttributeValue( AttributeKey.Rooms )];
                detailsEventDateAttr = detail.Attributes[GetAttributeValue( AttributeKey.DetailsEventDate )];
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
                        DateTime end = DateTime.Parse( $"{dates[k]} {detail.GetAttributeValue( "EndTime" )}" ).AddMinutes( -1 );
                        DateRange r = new DateRange() { Start = start, End = end };
                        var startBuffer = detail.GetAttributeValue( startBufferAttr.Key );
                        var endBuffer = detail.GetAttributeValue( endBufferAttr.Key );
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
                        PreApprovalData d = new PreApprovalData() { range = r, rooms = detail.GetAttributeValue( "Rooms" ), attendance = detail.GetAttributeValue( "ExpectedAttendance" ) };
                        eventDates.Add( d );
                    }
                }
                else
                {
                    DateTime? eventDate = detail.GetAttributeValue( "EventDate" ).AsDateTime();
                    string dateString = eventDate.Value.ToString( "yyyy-MM-dd" );
                    DateTime start = DateTime.Parse( $"{dateString} {detail.GetAttributeValue( "StartTime" )}" );
                    DateTime end = DateTime.Parse( $"{dateString} {detail.GetAttributeValue( "EndTime" )}" ).AddMinutes( -1 );
                    DateRange r = new DateRange() { Start = start, End = end };
                    var startBuffer = detail.GetAttributeValue( startBufferAttr.Key );
                    var endBuffer = detail.GetAttributeValue( endBufferAttr.Key );
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
                    PreApprovalData d = new PreApprovalData() { range = r, rooms = detail.GetAttributeValue( "Rooms" ), attendance = detail.GetAttributeValue( "ExpectedAttendance" ) };
                    eventDates.Add( d );
                }
            }

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
                    bool noNotes = true;
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
                                var rooms = eventDates[i].rooms.Split( ',' ).Select( r => r.ToLower().Trim() ).ToList();
                                Guid locationGuid = Guid.Empty;
                                if ( Guid.TryParse( GetAttributeValue( AttributeKey.LocationList ), out locationGuid ) )
                                {
                                    Rock.Model.DefinedType locationDT = new DefinedTypeService( context ).Get( locationGuid );
                                    var locs = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == locationDT.Id ).ToList();
                                    locs.LoadAttributes();
                                    var locGuids = locs.Where( dv =>
                                    {
                                        var ineligible = dv.GetAttributeValue( "IneligibleForPreApproval" ).AsBoolean();
                                        return ineligible;
                                    } ).Select( dv => dv.Guid.ToString().ToLower() ).ToList();
                                    var intersection = rooms.Intersect( locGuids );
                                    if ( intersection != null && intersection.Count() > 0 )
                                    {
                                        validRooms = false;
                                        notValidForPreApprovalReasons.Add( "Request is for spaces that require approval." );
                                    }
                                }
                            }
                        }
                        //Conflicts
                        if ( noConflicts )
                        {
                            if ( roomAttr != null && detailsEventDateAttr != null && startTimeAttr != null && endTimeAttr != null && startBufferAttr != null && endBufferAttr != null )
                            {
                                if ( EventDatesAttrGuid != Guid.Empty && RequestStatusAttrGuid != Guid.Empty && IsSameAttrGuid != Guid.Empty )
                                {
                                    string dateCompareVal = eventDates[i].range.Start.Value.ToString( "yyyy-MM-dd" );
                                    int originalId = 0;
                                    if ( item.ContentChannelId == EventChangesContentChannelId )
                                    {
                                        var parent = item.ParentItems.FirstOrDefault();
                                        if ( parent != null )
                                        {
                                            originalId = parent.ContentChannelItemId;
                                        }
                                    }
                                    var items = cciSvc.Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId && cci.Id != item.Id && cci.Id != originalId );
                                    //Requests that are on the calendar
                                    var statusAttr = new AttributeService( context ).Get( RequestStatusAttrGuid );
                                    var statusValues = avSvc.Queryable().Where( av => av.AttributeId == statusAttr.Id && ( av.Value != "Draft" && av.Value != "Submitted" && av.Value != "Denied" && !av.Value.Contains( "Cancelled" ) ) );
                                    items = items.Join( statusValues,
                                        itm => itm.Id,
                                        av => av.EntityId,
                                        ( itm, av ) => itm
                                    );
                                    var eventDetails = items.SelectMany( itm => itm.ChildItems.Where( ccia => ccia.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ccia => ccia.ChildContentChannelItem ) );
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
                                    //When events are edited through Content Channel UI they can get the Event Date format changed which no longer matches yyyy-MM-dd
                                    var eventDateValues = avSvc.Queryable().Where( av => av.AttributeId == detailsEventDateAttr.Id && av.Value.Substring( 0, 10 ) == dateCompareVal );
                                    var eventDatesValues = avSvc.Queryable().Where( av => av.AttributeId == eventAttr.Id && av.Value.Contains( dateCompareVal ) );
                                    eventDetails = eventDetails.Join( eventDateValues,
                                        itm => itm.Id,
                                        av => av.EntityId,
                                        ( itm, av ) => itm
                                    );
                                    items = items.Join( eventDatesValues,
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
                                    ccItems = ccItems.Distinct().ToList();
                                    ccItems.LoadAttributes();
                                    ccItems = ccItems.Where( itm =>
                                    {
                                        DateRange r = new DateRange();
                                        r.Start = DateTime.Parse( $"{dateCompareVal} {itm.GetAttributeValue( "StartTime" )}" );
                                        r.End = DateTime.Parse( $"{dateCompareVal} {itm.GetAttributeValue( "EndTime" )}" ).AddMinutes( -1 );
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
                                    } ).Distinct().ToList();
                                    if ( ccItems.Count() > 0 )
                                    {
                                        noConflicts = false;
                                        notValidForPreApprovalReasons.Add( "Request conflicts with another request." );
                                    }
                                }
                            }
                        }
                    }
                    //Request has extra notes
                    if ( !String.IsNullOrEmpty( item.GetAttributeValue( "Notes" ) ) )
                    {
                        notValidForPreApprovalReasons.Add( "Request has additional notes." );
                        noNotes = false;
                    }
                    if ( inRange && validAttendance && validRooms && noConflicts && noNotes )
                    {
                        item.SetAttributeValue( "IsPreApproved", "True" );
                        if ( isSubmission ) //We were pre-approving when people tried to save drafts, need to not do that. 
                        {
                            item.SetAttributeValue( "RequestStatus", "Approved" );
                            item.SaveAttributeValue( "RequestStatus" );
                        }
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
            item.SaveAttributeValue( "IsPreApproved" );
            return notValidForPreApprovalReasons;
        }

        private void CommentNotification( ContentChannelItem comment, ContentChannelItem item )
        {
            RockContext context = new RockContext();
            Rock.Model.Person p = GetCurrentPerson();
            GlobalAttributesCache attributeCache = GlobalAttributesCache.Get();
            string url;
            string baseUrl = attributeCache.GetValue( "InternalApplicationRoot" );
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.AdminDashboard, queryParams );
            string subject = p.FullName + " Has Added a Comment to " + item.Title;
            string message = "<p>This comment has been added to " + p.FullName + "'s request:</p>" +
                "<blockquote>" + comment.Content + "</blockquote><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
            var header = attributeCache.GetValue( "EmailHeader" );
            var footer = attributeCache.GetValue( "EmailFooter" );
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            var users = GetAdminUsers();
            users.Remove( p );
            for ( int i = 0; i < users.Count(); i++ )
            {
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( users[i], new Dictionary<string, object>() );
                email.AddRecipient( recipient );
            }
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            email.CreateCommunicationRecord = true;
            var output = email.Send();
        }
        private List<Rock.Model.Person> GetAdminUsers()
        {
            List<Rock.Model.Person> users = new List<Rock.Model.Person>();
            RockContext context = new RockContext();
            Guid securityRoleGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventAdminRole ), out securityRoleGuid ) )
            {
                Rock.Model.Group securityRole = new GroupService( context ).Get( securityRoleGuid );
                users.AddRange( securityRole.Members.Select( gm => gm.Person ) );
            }
            users = users.Distinct().ToList();
            return users;
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

        /// <summary>
        /// Method to verify the permissions the current person has for this request
        /// </summary>
        /// <param name="request">The Event Request Content Channel Item</param>
        /// <returns>Auth model with view and edit permissions for the current person</returns>
        private SubmissionFormViewModel CheckRequestPermissions( SubmissionFormViewModel viewModel, ContentChannelItem request )
        {
            using ( RockContext context = new RockContext() )
            {
                var p = GetCurrentPerson();
                bool isEventAdmin = CheckSecurityRole( context, AttributeKey.EventAdminRole );
                bool isRoomAdmin = CheckSecurityRole( context, AttributeKey.RoomAdminRole );
                Guid? sharedRequestGroupTypeGuid = GetAttributeValue( AttributeKey.SharingGroupType ).AsGuidOrNull();
                RequestAuthorization auth = helper.CheckRequestPermissions( request, p, isEventAdmin, isRoomAdmin, sharedRequestGroupTypeGuid );
                List<string> permissions = new List<string>();
                if ( auth.CanEdit )
                {
                    permissions.Add( "Edit" );
                    permissions.Add( "View" );
                }
                else if ( auth.CanView )
                {
                    permissions.Add( "View" );
                }
                viewModel.isEventAdmin = isEventAdmin;
                viewModel.isRoomAdmin = isRoomAdmin;
                viewModel.permissions = permissions;
                return viewModel;
            }
        }

        private List<string> SetPermissions( ContentChannelItemBag item, bool isAdmin )
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
            if ( !String.IsNullOrEmpty( item.IdKey ) )
            {
                List<int> ids = new List<int>() { p.Id };
                PersonAlias createdBy = null;
                if ( item.CreatedByPersonAliasId.HasValue )
                {
                    createdBy = new PersonAliasService( new RockContext() ).Get( item.CreatedByPersonAliasId.Value );
                }
                PersonAlias modifiedBy = null;
                if ( item.ModifiedByPersonAliasId.HasValue )
                {
                    modifiedBy = new PersonAliasService( new RockContext() ).Get( item.ModifiedByPersonAliasId.Value );
                }
                //Shared Request
                string sharedWithAttrKey = GetAttributeValue( AttributeKey.SharedWithAttr );
                if ( !String.IsNullOrEmpty( sharedWithAttrKey ) )
                {
                    var sharedWithAttrValue = item.AttributeValues[sharedWithAttrKey];
                    if ( !String.IsNullOrEmpty( sharedWithAttrValue ) )
                    {
                        List<int> sharedWithIds = sharedWithAttrValue.Split( ',' ).Select( i =>
                        {
                            int id;
                            if ( int.TryParse( i, out id ) )
                            {
                                return id;
                            }
                            return 0;
                        } ).Where( i => i > 0 ).ToList();
                        ids.AddRange( sharedWithIds );
                    }
                }

                Guid? sharedRequestGroupTypeGuid = GetAttributeValue( AttributeKey.SharingGroupType ).AsGuidOrNull();
                if ( sharedRequestGroupTypeGuid.HasValue )
                {
                    //Shared requests are configured, find any for the current user.
                    var SharedRequestGT = new GroupTypeService( context ).Get( sharedRequestGroupTypeGuid.Value );
                    var groups = new GroupService( context ).Queryable().Where( g => g.GroupTypeId == SharedRequestGT.Id ).Where( g => g.Members.Any( gm => gm.PersonId == p.Id && gm.GroupRole.Name == "Can View" ) ).ToList();
                    for ( int k = 0; k < groups.Count(); k++ )
                    {
                        var pids = groups[k].Members.Where( gm => gm.GroupRole.Name == "Request Creator" ).Select( gm => gm.Person.Id );
                        ids.AddRange( pids );
                    }
                }

                if ( ( createdBy != null && ids.Contains( createdBy.PersonId ) ) || ( modifiedBy != null && ids.Contains( modifiedBy.PersonId ) ) )
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

        private ContentChannelItem FromViewModel( ContentChannelItemBag viewModel, bool forceContext = false )
        {
            Rock.Model.Person p = GetCurrentPerson();
            ContentChannelItem item = new ContentChannelItem()
            {
                ContentChannelId = viewModel.ContentChannelId,
                ContentChannelTypeId = viewModel.ContentChannelTypeId
            };
            if ( !String.IsNullOrEmpty( viewModel.IdKey ) )
            {
                if ( ( viewModel.AttributeValues.ContainsKey( "RequestStatus" ) && ( viewModel.AttributeValues["RequestStatus"] == "Submitted" || viewModel.AttributeValues["RequestStatus"] == "In Progress" || viewModel.AttributeValues["RequestStatus"] == "Draft" ) ) || forceContext )
                {
                    item = new ContentChannelItemService( context ).Get( viewModel.IdKey );
                }
                else
                {
                    item = new ContentChannelItemService( new RockContext() ).Get( viewModel.IdKey );
                }
            }
            item.LoadAttributes();
            item.Title = viewModel.Title;
            string dateKey = GetAttributeValue( AttributeKey.DetailsEventDate );
            foreach ( KeyValuePair<string, string> av in viewModel.AttributeValues )
            {
                if ( av.Key == dateKey )
                {
                    item.SetAttributeValue( av.Key, av.Value );
                }
                else
                {
                    item.SetPublicAttributeValue( av.Key, av.Value, null, false );
                }
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
            Guid eventCommentsCCGuid = Guid.Empty;
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
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventCommentsContentChannel ), out eventCommentsCCGuid ) )
            {
                ContentChannel cc = new ContentChannelService( context ).Get( eventCommentsCCGuid );
                EventCommentsContentChannelId = cc.Id;
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
            RoomSetUpKey = GetAttributeValue( AttributeKey.RoomSetUp );
            OpsInventoryKey = GetAttributeValue( AttributeKey.OpsInventory );
            DiscountCodeKey = GetAttributeValue( AttributeKey.DiscountCode );
            ExtensiveSetUpKey = "Setup";
        }

        private void SubmittedNotifications( ContentChannelItem item )
        {
            item.LoadAttributes();
            var events = item.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci => ci.ChildContentChannelItem ).ToList();
            if ( item.ContentChannelId == EventChangesContentChannelId )
            {
                var parentAssoc = item.ParentItems.FirstOrDefault();
                if ( parentAssoc != null )
                {
                    var parent = parentAssoc.ContentChannelItem;
                    events = parent.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci => ci.ChildContentChannelItem.ChildItems.FirstOrDefault().ChildContentChannelItem ).ToList();
                }
            }
            Rock.Model.Person p = GetCurrentPerson();
            events.LoadAttributes();
            string message = "";
            string subject = "";

            helper.InitializeEventFormHelper( EventContentChannelId, EventDetailsContentChannelId, EventChangesContentChannelId, EventDetailsChangesContentChannelId, RoomSetUpKey, DiscountCodeKey, OpsInventoryKey, ExtensiveSetUpKey );

            List<GroupMember> groupMembers = new List<GroupMember>();
            if ( item.GetAttributeValue( "IsPreApproved" ) == "True" )
            {
                subject = "New Room Request " + item.Title + " from " + p.FullName;
                message = p.FullName + " has submitted a room request. This request meets criteria for pre-approval. Details of the request are as follows:<br/><br/>";

                Guid? securityRoleGuid = GetAttributeValue( AttributeKey.RoomAdminRole ).AsGuidOrNull();
                if ( securityRoleGuid.HasValue )
                {
                    groupMembers = new GroupService( context ).Get( securityRoleGuid.Value ).Members.Where( gm => gm.IsArchived == false && gm.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
                }
            }
            else
            {
                subject = "New Event Request " + item.Title + " from " + p.FullName;
                message = "Details of the request are as follows: <br/><br/>";
                Guid? securityRoleGuid = GetAttributeValue( AttributeKey.EventAdminRole ).AsGuidOrNull();
                if ( securityRoleGuid.HasValue )
                {
                    groupMembers = new GroupService( context ).Get( securityRoleGuid.Value ).Members.Where( gm => gm.IsArchived == false && gm.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
                }
            }
            if ( item.GetAttributeValue( "RequestStatus" ) == "Pending Changes" )
            {
                subject = p.FullName + " is Requesting Changes to " + item.Title;
                message = "Details of the changes are as follows: <br/><br/>";
            }

            //message += GetRequestDetails( item, events );
            message += helper.GetRequestDetails( item, events );

            GlobalAttributesCache attributesCache = GlobalAttributesCache.Get();
            var header = attributesCache.GetValue( "EmailHeader" );
            var footer = attributesCache.GetValue( "EmailFooter" );

            string url = "";
            string baseUrl = attributesCache.GetValue( "InternalApplicationRoot" );
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.AdminDashboard, queryParams );

            message += "<br/>" +
                "<table style='width: 100%;'>" +
                    "<tr>" +
                        "<td></td>" +
                        "<td style='text-align:center;'>" +
                            "<a href='" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Open Request</a>" +
                        "</td>" +
                        "<td></td>" +
                    "</tr>" +
                "</table>";

            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            for ( var i = 0; i < groupMembers.Count(); i++ )
            {
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( groupMembers[i].Person, new Dictionary<string, object>() );
                email.AddRecipient( recipient );
            }
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            email.CreateCommunicationRecord = true;
            var output = email.Send();
        }
        private void SubmittedConfirmation( ContentChannelItem item )
        {
            Rock.Model.Person p = GetCurrentPerson();
            item.LoadAttributes();
            var events = item.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci => ci.ChildContentChannelItem ).ToList();
            events.LoadAttributes();
            string message = "";
            string subject = "";

            helper.InitializeEventFormHelper( EventContentChannelId, EventDetailsContentChannelId, EventChangesContentChannelId, EventDetailsChangesContentChannelId, RoomSetUpKey, DiscountCodeKey, OpsInventoryKey, ExtensiveSetUpKey );

            if ( item.GetAttributeValue( "IsPreApproved" ) == "True" )
            {
                subject = "Your Request " + item.Title + " has been approved";
                message = "Your room/space request has been approved. The details of your request are as follows: <br/><br/>";
            }
            else
            {
                subject = "Your Request " + item.Title + " has been submitted";
                message = "Your Event Request has been submitted and is pending approval. You can expect a response from the Events Director within 48 hours and/or 2 business days, if not sooner. Thank you! <br/>The details of your request are as follows: <br/><br/>";
            }
            //message += GetRequestDetails( item, events );
            message += helper.GetRequestDetails( item, events );

            //Deadline Reminders
            DateTime firstDate = item.AttributeValues["EventDates"].Value.Split( ',' ).Select( e => DateTime.Parse( e.Trim() ) ).OrderBy( e => e.Date ).FirstOrDefault();
            DateTime twoWeekDate = firstDate.AddDays( -14 );
            DateTime thirtyDayDate = firstDate.AddDays( -30 );
            DateTime sixWeekDate = firstDate.AddDays( -42 );
            DateTime pubGoLive = firstDate.AddDays( -21 );
            if ( !String.IsNullOrEmpty( item.AttributeValues["PublicityStartDate"].Value ) )
            {
                sixWeekDate = DateTime.Parse( item.AttributeValues["PublicityStartDate"].Value ).AddDays( -21 );
                pubGoLive = DateTime.Parse( item.AttributeValues["PublicityStartDate"].Value );
            }
            DateTime today = RockDateTime.Now;
            today = new DateTime( today.Year, today.Month, today.Day, 0, 0, 0 );
            List<String> unavailableResources = new List<String>();
            if ( twoWeekDate >= today )
            {
                message += "<br/><div><strong>Important Dates for Your Request</strong></div>";
                message += "Last date to request and provide all information for the following resources is two weeks before your first event date <strong>(" + twoWeekDate.ToShortDateString() + ")</strong>:";
                message += "<ul>" +
                        "<li>Catering</li>" +
                        "<li>Ops Accommodations</li>" +
                        "<li>Production Accommodations</li>" +
                        "<li>Zoom</li>";
                if ( String.IsNullOrEmpty( item.AttributeValues["WebCalendarGoLive"].Value ) )
                {
                    message += "<li>Web Calendar</li>";
                }
                message += "</ul> <br/>";
                message += "Last date to request and provide all information for Registration is two weeks before your registration goes live:" +
                    "<ul>";
                if ( item.ContentChannelId == EventChangesContentChannelId )
                {
                    events = item.ParentItems.FirstOrDefault( pi => pi.ContentChannelItem.ContentChannelId == EventContentChannelId ).ContentChannelItem.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci => ci.ChildContentChannelItem.ChildItems.FirstOrDefault().ChildContentChannelItem ).ToList();
                    events.LoadAttributes();
                }
                for ( int i = 0; i < events.Count(); i++ )
                {
                    DateTime twoWeekRegistrationDate = twoWeekDate;
                    DateTime goLiveDate = firstDate;
                    if ( !String.IsNullOrEmpty( events[i].AttributeValues["RegistrationStartDate"].Value ) )
                    {
                        twoWeekRegistrationDate = DateTime.Parse( events[i].AttributeValues["RegistrationStartDate"].Value ).AddDays( -14 );
                        goLiveDate = DateTime.Parse( events[i].AttributeValues["RegistrationStartDate"].Value );
                    }
                    message += "<li><strong>" + twoWeekRegistrationDate.ToShortDateString() + "</strong> for the go live date " + goLiveDate.ToShortDateString() + "</li>";
                }
                message += "</ul> <br/>";
                if ( !String.IsNullOrEmpty( item.AttributeValues["WebCalendarGoLive"].Value ) )
                {
                    message += "Last date to provide all information for the Web Calendar is two weeks before your calendar event goes live:";
                    DateTime webCalGoLive = DateTime.Parse( item.AttributeValues["WebCalendarGoLive"].Value ).AddDays( -14 );
                    message += "<ul><li><strong>" + webCalGoLive.ToShortDateString() + "</strong></li></ul>";
                }
                if ( thirtyDayDate >= today )
                {
                    message += "Last date to request and provide all information for the following resources is <strong>" + thirtyDayDate.ToShortDateString() + "</strong>:";
                    message += "<ul><li>Childcare</li></ul>";
                    if ( sixWeekDate >= today )
                    {
                        message += "Last date to request and provide all information for Publicity is three weeks before your publicity goes live:";
                        message += "<ul><li><strong>" + sixWeekDate.ToShortDateString() + "</strong> for the go live date " + pubGoLive.ToShortDateString() + "</li></ul>";
                    }
                    else
                    {
                        unavailableResources.Add( "Publicity" );
                    }
                }
                else
                {
                    unavailableResources.Add( "Childcare" );
                    unavailableResources.Add( "Publicity" );
                }
            }
            else
            {
                unavailableResources.Add( "Catering" );
                unavailableResources.Add( "Ops Accommodations" );
                unavailableResources.Add( "Registration" );
                unavailableResources.Add( "Childcare" );
                unavailableResources.Add( "Web Calendar" );
                unavailableResources.Add( "Publicity" );
                unavailableResources.Add( "Production Accommodations" );
                unavailableResources.Add( "Zoom" );
            }
            if ( unavailableResources.Count() > 0 )
            {
                message += "<div>There is not enough time between now and your first event date to allow for the following resources:</div>";
                message += "<ul>";
                for ( int i = 0; i < unavailableResources.Count(); i++ )
                {
                    message += "<li>" + unavailableResources[i] + "</li>";
                }
                message += "</ul>";
            }

            GlobalAttributesCache attributesCache = GlobalAttributesCache.Get();
            var header = attributesCache.GetValue( "EmailHeader" );
            var footer = attributesCache.GetValue( "EmailFooter" );

            string url;
            string baseUrl = attributesCache.GetValue( "InternalApplicationRoot" );
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.UserDashboard, queryParams );
            message += "<br/>" +
                "<table style='width: 100%;'>" +
                    "<tr>" +
                        "<td></td>" +
                        "<td style='text-align:center;'>" +
                            "<strong>See a mistake? You can modify your request using the link below. If your request was already approved the changes you make will have to be approved as well.</strong><br/><br/><br/>" +
                            "<a href='" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Modify Request</a>" +
                        "</td>" +
                        "<td></td>" +
                    "</tr>" +
                "</table>";
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( p, new Dictionary<string, object>() );
            email.AddRecipient( recipient );
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            email.CreateCommunicationRecord = true;
            var output = email.Send();
        }

        #endregion Helpers

        public class SubmissionFormViewModel
        {
            public ContentChannelItemBag request { get; set; }
            public ContentChannelItemBag originalRequest { get; set; }
            public List<ContentChannelItemBag> events { get; set; }
            public List<ContentChannelItem> existing { get; set; }
            public List<ContentChannelItemAssociation> existingDetails { get; set; }
            public bool isSuperUser { get; set; }
            public bool isEventAdmin { get; set; }
            public bool isRoomAdmin { get; set; }
            public List<string> permissions { get; set; }
            public List<DefinedValueCache> locations { get; set; }
            public List<AttributeMatrix> locationSetupMatrix { get; set; }
            public List<AttributeMatrixItem> locationSetupMatrixItem { get; set; }
            public List<DefinedValueCache> ministries { get; set; }
            public List<DefinedValueCache> budgetLines { get; set; }
            public List<DefinedValueCache> inventoryList { get; set; }
            public List<DefinedValueCache> vendorList { get; set; }
            public string adminDashboardURL { get; set; }
            public string userDashboardURL { get; set; }
            public List<AttributeBag> discountCodeAttrs { get; set; }
            public string errors { get; set; }
        }

        public class PreApprovalData
        {
            public DateRange range { get; set; }
            public string rooms { get; set; }
            public string attendance { get; set; }
        }

        public class FormResponse
        {
            public int id { get; set; }
            public string idKey { get; set; }
            public List<string> notValidForPreApprovalReasons { get; set; }
            public bool isPreApproved { get; set; }
            public string message { get; set; }
        }
    }
}
