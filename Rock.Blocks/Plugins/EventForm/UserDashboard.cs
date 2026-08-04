using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Configuration;

using DocumentFormat.OpenXml.Math;

using Microsoft.Ajax.Utilities;

using Newtonsoft.Json;

using OpenXmlPowerTools;

using Rock.Attribute;
using Rock.Blocks.Plugins.EventForm;
using Rock.Blocks.Plugins.ViewModels;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

using static Rock.Blocks.Plugins.EventDashboard.UserDashboard;
using static Rock.Model.StepProgram;

namespace Rock.Blocks.Plugins.EventDashboard
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "User Dashboard" )]
    [Category( "Obsidian > Plugin > Event Form" )]
    [Description( "Obsidian Event User Dashboard" )]
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
    [DefinedTypeField( "Drinks Defined Type", key: AttributeKey.DrinksList, category: "Lists", required: true, order: 3 )]
    [DefinedTypeField( "Ops Inventory Defined Type", key: AttributeKey.InventoryList, category: "Lists", required: true, order: 4 )]

    [LinkedPage( "Event Submission Form", key: AttributeKey.SubmissionPage, category: "Pages", required: true, order: 0 )]
    [LinkedPage( "Workflow Entry Page", key: AttributeKey.WorkflowEntryPage, category: "Pages", required: true, order: 1 )]
    [LinkedPage( "Event Admin Dashboard", key: AttributeKey.AdminDashboard, category: "Pages", required: true, order: 2 )]

    [SecurityRoleField( "Event Request Admin", key: AttributeKey.EventAdminRole, category: "Security", required: true, order: 0 )]
    [SecurityRoleField( "Room Request Admin", key: AttributeKey.RoomAdminRole, category: "Security", required: true, order: 1 )]
    [SecurityRoleField( "Ministry Event Admin", key: AttributeKey.MinistryEventAdminRole, category: "Security", required: true, order: 2 )]

    [TextField( "Default Statuses", key: AttributeKey.DefaultStatuses, category: "Filters", required: false, order: 0 )]
    [TextField( "Request Status Attribute Key", key: AttributeKey.RequestStatusAttrKey, category: "Filters", defaultValue: "RequestStatus", required: true, order: 1 )]
    [TextField( "Requested Resources Attribute Key", key: AttributeKey.RequestedResourcesAttrKey, category: "Filters", defaultValue: "RequestType", required: true, order: 2 )]
    [TextField( "Event Dates Attribute Key", key: AttributeKey.EventDatesAttrKey, category: "Filters", defaultValue: "EventDates", required: true, order: 3 )]
    [TextField( "Ministry Attribute Key", key: AttributeKey.MinistryAttrKey, category: "Filters", defaultValue: "Ministry", required: true, order: 4 )]
    [TextField( "Request is Valid Attribute Key", key: AttributeKey.IsValidAttrKey, category: "Filters", defaultValue: "RequestIsValid", required: true, order: 5 )]

    [WorkflowTypeField( "Request Action Worfklow", "Workflow to update request status", true, key: AttributeKey.RequestActionWorkflow, category: "Workflow" )]

    [GroupTypeField( "Shared Event Group Type", "Group Type of groups that allow for seeing shared requests", false, "", "Sharing", 1, AttributeKey.SharingGroupType )]
    [SecurityRoleField( "Staff Group", "The role of people you can share requests with", false, "", "Sharing", 2, AttributeKey.StaffGroup )]
    [TextField( "Shared With Attribut Key", category: "Sharing", order: 3, key: AttributeKey.SharedWithAttrKey )]

    [AttributeField( name: "Grid Attributes", allowMultiple: true, required: true, category: "Attributes", order: 0, key: "GridAttrs", entityTypeGuid: Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, entityTypeQualifierColumn: "ContentChannelTypeId", entityTypeQualifierValue: "16" )]
    #endregion Block Attributes

    public class UserDashboard : RockBlockType
    {
        #region Keys

        /// <summary>
        /// Attribute Key
        /// </summary>
        private static class AttributeKey
        {
            public const string EventContentChannel = "EventContentChannel";
            public const string EventChangesContentChannel = "EventChangesContentChannel";
            public const string EventDetailsContentChannel = "EventDetailsContentChannel";
            public const string EventDetailsChangesContentChannel = "EventDetailsChangesContentChannel";
            public const string EventCommentsContentChannel = "EventCommentsContentChannel";
            public const string LocationList = "LocationList";
            public const string MinistryList = "MinistryList";
            public const string BudgetList = "BudgetList";
            public const string MinistryBudgetList = "MinistryBudgetList";
            public const string DrinksList = "DrinksList";
            public const string InventoryList = "InventoryList";
            public const string SubmissionPage = "SubmissionPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string AdminDashboard = "AdminDashboard";
            public const string UserDashboard = "UserDashboard";
            public const string EventAdminRole = "EventAdminRole";
            public const string RoomAdminRole = "RoomAdminRole";
            public const string MinistryEventAdminRole = "MinistryEventAdminRole";
            public const string DefaultStatuses = "DefaultStatuses";
            public const string RequestStatusAttrKey = "RequestStatusAttrKey";
            public const string RequestedResourcesAttrKey = "RequestedResourcesAttrKey";
            public const string EventDatesAttrKey = "EventDatesAttrKey";
            public const string IsValidAttrKey = "IsValidAttrKey";
            public const string MinistryAttrKey = "MinistryAttrKey";
            public const string RequestActionWorkflow = "RequestActionWorkflow";
            public const string SharingGroupType = "SharingGroupType";
            public const string StaffGroup = "StaffGroup";
            public const string SharedWithAttrKey = "SharedWithAttrKey";
        }

        /// <summary>
        /// Page Parameter
        /// </summary>
        private static class PageParameterKey
        {
            public const string RequestId = "Id";
        }

        #endregion Keys

        private ObsidianPluginsShared EventFormHelper = new ObsidianPluginsShared();
        private EventFormShared EventFormShared = new EventFormShared();

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
                Guid eventDatesAttrGuid = Guid.Empty;
                Guid requestStatusAttrGuid = Guid.Empty;
                Guid isSameAttrGuid = Guid.Empty;
                DashboardViewModel viewModel = null;

                SetProperties();
                if ( EventContentChannelId > 0 && EventDetailsContentChannelId > 0 && EventChangesContentChannelId > 0 && EventDetailsChangesContentChannelId > 0 )
                {
                    viewModel = new DashboardViewModel();
                    //viewModel.events = LoadRequests();
                    viewModel.isEventAdmin = CheckSecurityRole( rockContext, AttributeKey.EventAdminRole );
                    viewModel.isRoomAdmin = CheckSecurityRole( rockContext, AttributeKey.RoomAdminRole );
                    viewModel.isSuperUser = CheckSecurityRole( rockContext, AttributeKey.MinistryEventAdminRole );
                    viewModel.eventDetailsCCId = EventDetailsContentChannelId;
                    viewModel.commentsCCId = EventCommentsContentChannelId;

                    //Lists
                    Guid locationGuid = Guid.Empty;
                    Guid ministryGuid = Guid.Empty;
                    Guid budgetLineGuid = Guid.Empty;
                    Guid drinksGuid = Guid.Empty;
                    Guid inventoryGuid = Guid.Empty;
                    var p = GetCurrentPerson();
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.LocationList ), out locationGuid ) )
                    {
                        DefinedType locationDT = new DefinedTypeService( rockContext ).Get( locationGuid );
                        var locs = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == locationDT.Id ).ToList();
                        locs.LoadAttributes();
                        viewModel.locations = locs;
                    }
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryGuid ) )
                    {
                        DefinedType ministryDT = new DefinedTypeService( rockContext ).Get( ministryGuid );
                        var min = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == ministryDT.Id ).ToList();
                        min.LoadAttributes();
                        viewModel.ministries = min.ToList();
                    }
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.BudgetList ), out budgetLineGuid ) )
                    {
                        DefinedType budgetDT = new DefinedTypeService( rockContext ).Get( budgetLineGuid );
                        var budget = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == budgetDT.Id ).ToList();
                        budget.LoadAttributes();
                        viewModel.budgetLines = budget.ToList();
                    }
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.DrinksList ), out drinksGuid ) )
                    {
                        DefinedType drinkDT = new DefinedTypeService( rockContext ).Get( drinksGuid );
                        var drinks = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == drinkDT.Id ).ToList();
                        drinks.LoadAttributes();
                        viewModel.drinks = drinks.ToList();
                    }
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.InventoryList ), out inventoryGuid ) )
                    {
                        DefinedType invDT = new DefinedTypeService( rockContext ).Get( inventoryGuid );
                        var inventory = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == invDT.Id ).ToList();
                        inventory.LoadAttributes();
                        viewModel.inventory = inventory.ToList();
                    }

                    //Attributes
                    string requestStatusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
                    if ( !String.IsNullOrEmpty( requestStatusAttrKey ) )
                    {
                        viewModel.requestStatus = EventFormHelper.GetCommonAttributeEntityBag( new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == requestStatusAttrKey ) );
                    }
                    string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
                    if ( !String.IsNullOrEmpty( resourcesAttrKey ) )
                    {
                        viewModel.requestType = EventFormHelper.GetCommonAttributeEntityBag( new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == resourcesAttrKey ) );
                    }

                    List<string> defaultStatuses = GetAttributeValue( AttributeKey.DefaultStatuses ).Split( ',' ).Where( s => !String.IsNullOrEmpty( s ) ).ToList();
                    viewModel.defaultStatuses = defaultStatuses;

                    Guid? workflowGuid = GetAttributeValue( AttributeKey.RequestActionWorkflow ).AsGuidOrNull();
                    if ( workflowGuid.HasValue )
                    {
                        WorkflowType wf = new WorkflowTypeService( rockContext ).Get( workflowGuid.Value );
                        viewModel.workflowURL = "/WorkflowEntry/" + wf.Id;
                    }
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
        private int EventChangesContentChannelId { get; set; }
        private int EventDetailsChangesContentChannelId { get; set; }
        private int EventCommentsContentChannelId { get; set; }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetRequestDetails( string id )
        {
            GetRequestResponse response = new GetRequestResponse();
            RockContext context = new RockContext();
            SetProperties();
            var item = new ContentChannelItemService( context ).Get( id );
            if ( item == null )
            {
                return ActionOk( new { isError = true, errorMessage = "Request does not exist" } );
            }
            if ( item.ContentChannelId == EventChangesContentChannelId )
            {
                var parent = item.ParentItems.FirstOrDefault( pi => pi.ContentChannelItem.ContentChannelId == EventContentChannelId );
                if ( parent != null )
                {
                    item = parent.ContentChannelItem;
                }
            }
            response.request = EventFormHelper.GetCommonContentChannelItemEntityBag( item );
            response.request.Id = item.Id;
            item.LoadAttributes();
            response.request.LoadAttributesAndValuesForPublicEdit( item, RequestContext.CurrentPerson, false );
            var requestchanges = item.ChildItems.Where( i => i.ChildContentChannelItem.ContentChannelId == EventChangesContentChannelId ).FirstOrDefault();
            if ( requestchanges != null )
            {
                response.requestPendingChanges = EventFormHelper.GetCommonContentChannelItemEntityBag( requestchanges.ChildContentChannelItem );
                requestchanges.ChildContentChannelItem.LoadAttributes();
                response.requestPendingChanges.LoadAttributesAndValuesForPublicEdit( requestchanges.ChildContentChannelItem, RequestContext.CurrentPerson, false );
            }
            var details = item.ChildItems.Where( i => i.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( i => i.ChildContentChannelItem ).ToList();
            response.details = details.Select( i =>
            {
                var detail = new Details() { detail = EventFormHelper.GetCommonContentChannelItemEntityBag( i ) };
                i.LoadAttributes();
                detail.detail.LoadAttributesAndValuesForPublicEdit( i, RequestContext.CurrentPerson, false );
                return detail;
            } ).ToList();
            for ( int i = 0; i < details.Count(); i++ )
            {
                var detailChanges = details[i].ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId );
                if ( detailChanges != null )
                {
                    response.details[i].detailPendingChanges = EventFormHelper.GetCommonContentChannelItemEntityBag( detailChanges.ChildContentChannelItem );
                    detailChanges.ChildContentChannelItem.LoadAttributes();
                    response.details[i].detailPendingChanges.LoadAttributesAndValuesForPublicEdit( detailChanges.ChildContentChannelItem, RequestContext.CurrentPerson, false );
                }
            }
            response.comments = item.ChildItems.Where( i => i.ChildContentChannelItem.ContentChannelId == EventCommentsContentChannelId ).Select( ci => new Comment { comment = EventFormHelper.GetCommonContentChannelItemEntityBag( ci.ChildContentChannelItem ), createdBy = ci.ChildContentChannelItem.CreatedByPersonName } ).ToList();
            response.createdBy = EventFormHelper.GetCommonPersonEntityBag( item.CreatedByPersonAlias.Person );
            response.modifiedBy = EventFormHelper.GetCommonPersonEntityBag( item.ModifiedByPersonAlias.Person );

            // Get Permissions
            var auth = CheckRequestPermissions( item );
            if ( !auth.CanView )
            {
                return ActionBadRequest( "You do not have permission to view this request." );
            }
            response.request.CanEdit = auth.CanEdit;
            return ActionOk( response );
        }

        [BlockAction]
        public BlockActionResult FilterRequests( Filters filters = null )
        {
            RockContext rockContext = new RockContext();
            Guid eventCCGuid = Guid.Empty;
            Guid eventDetailsCCGuid = Guid.Empty;
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
            DashboardViewModel viewModel = new DashboardViewModel();
            viewModel.events = LoadRequests( filters );
            return ActionOk( viewModel );
        }

        [BlockAction]
        public BlockActionResult ChangeStatus( string id, string status, bool denyWithComments = false )
        {
            try
            {
                RockContext rockContext = new RockContext();
                var p = GetCurrentPerson();
                SetProperties();
                string requestStatusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
                var cci_svc = new ContentChannelItemService( rockContext );
                var ccia_svc = new ContentChannelItemAssociationService( rockContext );
                ContentChannelItem item = cci_svc.Get( id );
                item.LoadAttributes();
                RequestAuthorization auth = CheckRequestPermissions( item );
                if ( !auth.CanEdit )
                {
                    throw new UnauthorizedAccessException( "You do not have permission to make changes to request." );
                }
                string currentStatus = item.GetAttributeValue( requestStatusAttrKey );
                item.ModifiedByPersonAliasId = p.PrimaryAliasId;
                item.ModifiedDateTime = RockDateTime.Now;
                if ( status != "Cancelled by User" )
                {
                    throw new Exception( "You do not have permission to mark a request: " + status );
                }
                rockContext.SaveChanges();
                item.SetAttributeValue( requestStatusAttrKey, status );
                item.SaveAttributeValue( requestStatusAttrKey );

                ConcurrentBag<Task> taskBag = new ConcurrentBag<Task>();
                Task task = new Task( () =>
                {
                    StatusChangeNotification( item, status );
                } );
                taskBag.Add( task );
                task.Start();

                return ActionOk( new { status = item.GetAttributeValue( requestStatusAttrKey ) } );
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
                return ActionBadRequest( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult AddComment( string id, string message )
        {
            try
            {
                RockContext rockContext = new RockContext();
                SetProperties();
                Person p = GetCurrentPerson();
                ContentChannelItemService cci_svc = new ContentChannelItemService( rockContext );
                ContentChannelItem request = cci_svc.Get( id );
                RequestAuthorization auth = CheckRequestPermissions( request );
                if ( !auth.CanEdit )
                {
                    throw new UnauthorizedAccessException( "You do not have permission to comment on this request." );
                }
                ContentChannel commentChannel = new ContentChannelService( rockContext ).Get( EventCommentsContentChannelId );
                ContentChannelItem comment = new ContentChannelItem()
                {
                    ContentChannelId = EventCommentsContentChannelId,
                    ContentChannelTypeId = commentChannel.ContentChannelTypeId,
                    Title = "Comment From " + p.FullName + " for " + request.Title + " on " + RockDateTime.Now.ToString( "M/d/yy h:mm tt" ),
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

                ConcurrentBag<Task> taskBag = new ConcurrentBag<Task>();
                Task task = new Task( () =>
                {
                    CommentNotification( comment, request );
                } );
                taskBag.Add( task );
                task.Start();

                var bag = EventFormHelper.GetCommonContentChannelItemEntityBag( comment );
                return ActionOk( new { createdBy = p.FullName, comment = bag } );
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
                return ActionBadRequest( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult DuplicateEvent( string id, string eventDates, List<string> removedResources, List<DuplicateDates> copyDates )
        {
            try
            {
                RockContext context = new RockContext();
                SetProperties();
                var cciSvc = new ContentChannelItemService( context );
                if ( !String.IsNullOrEmpty( id ) )
                {
                    var p = GetCurrentPerson();
                    ContentChannelItem original = cciSvc.Get( id );
                    original.LoadAttributes();
                    ContentChannelItem item = new ContentChannelItem()
                    {
                        Title = original.Title,
                        ContentChannelId = original.ContentChannelId,
                        ContentChannelTypeId = original.ContentChannelTypeId,
                        ModifiedByPersonAliasId = p.PrimaryAliasId,
                        ModifiedDateTime = RockDateTime.Now,
                        CreatedByPersonAliasId = p.PrimaryAliasId,
                        CreatedDateTime = RockDateTime.Now
                    };
                    cciSvc.Add( item );
                    context.SaveChanges();

                    item.LoadAttributes();
                    item.SetAttributeValue( "IsSame", original.GetAttributeValue( "IsSame" ) );
                    item.SetAttributeValue( "Ministry", original.GetAttributeValue( "Ministry" ) );
                    item.SetAttributeValue( "Contact", original.GetAttributeValue( "Contact" ) );
                    item.SetAttributeValue( "RequestStatus", "Draft" );
                    List<ContentChannelItem> children = new List<ContentChannelItem>();
                    var originalChildren = original.ChildItems.Where( cci => cci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( cci => cci.ChildContentChannelItem ).ToList();
                    originalChildren.LoadAttributes();
                    for ( var i = 0; i < originalChildren.Count(); i++ )
                    {
                        ContentChannelItem c = new ContentChannelItem()
                        {
                            ContentChannelId = originalChildren[0].ContentChannelId,
                            ContentChannelTypeId = originalChildren[0].ContentChannelTypeId,
                            ModifiedByPersonAliasId = p.PrimaryAliasId,
                            ModifiedDateTime = RockDateTime.Now,
                            CreatedByPersonAliasId = p.PrimaryAliasId,
                            CreatedDateTime = RockDateTime.Now
                        };
                        c.LoadAttributes();
                        c.SetAttributeValue( "EventDate", originalChildren[i].GetAttributeValue( "EventDate" ) );
                        children.Add( c );
                    }

                    List<string> requestType = new List<string>();

                    item.SetAttributeValue( "NeedsSpace", removedResources.Contains( "NeedsSpace" ) ? "False" : original.GetAttributeValue( "NeedsSpace" ) );
                    if ( item.GetAttributeValue( "NeedsSpace" ) == "True" )
                    {
                        requestType.Add( "Room" );
                        var attrs = originalChildren[0].Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Space" ) );
                        for ( var i = 0; i < children.Count(); i++ )
                        {
                            foreach ( var attr in attrs )
                            {
                                children[i].SetAttributeValue( attr.Key, originalChildren[i].GetAttributeValue( attr.Key ) );
                            }
                        }
                    }
                    item.SetAttributeValue( "NeedsOnline", removedResources.Contains( "NeedsOnline" ) ? "False" : original.GetAttributeValue( "NeedsOnline" ) );
                    if ( item.GetAttributeValue( "NeedsOnline" ) == "True" )
                    {
                        requestType.Add( "Online Event" );
                        var attrs = originalChildren[0].Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Online" ) );
                        for ( var i = 0; i < children.Count(); i++ )
                        {
                            foreach ( var attr in attrs )
                            {
                                children[i].SetAttributeValue( attr.Key, originalChildren[i].GetAttributeValue( attr.Key ) );
                            }
                        }
                    }
                    item.SetAttributeValue( "NeedsRegistration", removedResources.Contains( "NeedsRegistration" ) ? "False" : original.GetAttributeValue( "NeedsRegistration" ) );
                    if ( item.GetAttributeValue( "NeedsRegistration" ) == "True" )
                    {
                        requestType.Add( "Registration" );
                        var attrs = originalChildren[0].Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Registration" ) && a.Key != "RegistrationStartDate" && a.Key != "RegistrationEndDate" );
                        for ( var i = 0; i < children.Count(); i++ )
                        {
                            foreach ( var attr in attrs )
                            {
                                children[i].SetAttributeValue( attr.Key, originalChildren[i].GetAttributeValue( attr.Key ) );
                            }
                        }
                    }
                    item.SetAttributeValue( "NeedsChildCare", removedResources.Contains( "NeedsChildCare" ) ? "False" : original.GetAttributeValue( "NeedsChildCare" ) );
                    if ( item.GetAttributeValue( "NeedsChildCare" ) == "True" )
                    {
                        requestType.Add( "Childcare" );
                        var attrs = originalChildren[0].Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Childcare" ) );
                        for ( var i = 0; i < children.Count(); i++ )
                        {
                            foreach ( var attr in attrs )
                            {
                                children[i].SetAttributeValue( attr.Key, originalChildren[i].GetAttributeValue( attr.Key ) );
                            }
                        }
                    }
                    item.SetAttributeValue( "NeedsCatering", removedResources.Contains( "NeedsCatering" ) ? "False" : original.GetAttributeValue( "NeedsCatering" ) );
                    if ( item.GetAttributeValue( "NeedsCatering" ) == "True" )
                    {
                        requestType.Add( "Catering" );
                        var attrs = originalChildren[0].Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Catering" ) );
                        for ( var i = 0; i < children.Count(); i++ )
                        {
                            foreach ( var attr in attrs )
                            {
                                children[i].SetAttributeValue( attr.Key, originalChildren[i].GetAttributeValue( attr.Key ) );
                            }
                        }
                    }
                    item.SetAttributeValue( "NeedsChildCareCatering", removedResources.Contains( "NeedsChildCareCatering" ) ? "False" : original.GetAttributeValue( "NeedsChildCareCatering" ) );
                    if ( item.GetAttributeValue( "NeedsChildCareCatering" ) == "True" )
                    {
                        requestType.Add( "Childcare Catering" );
                        var attrs = originalChildren[0].Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Childcare Catering" ) );
                        for ( var i = 0; i < children.Count(); i++ )
                        {
                            foreach ( var attr in attrs )
                            {
                                children[i].SetAttributeValue( attr.Key, originalChildren[i].GetAttributeValue( attr.Key ) );
                            }
                        }
                    }
                    item.SetAttributeValue( "NeedsOpsAccommodations", removedResources.Contains( "NeedsOpsAccommodations" ) ? "False" : original.GetAttributeValue( "NeedsOpsAccommodations" ) );
                    if ( item.GetAttributeValue( "NeedsOpsAccommodations" ) == "True" )
                    {
                        requestType.Add( "Extra Resources" );
                        var attrs = originalChildren[0].Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Ops Requests" ) );
                        for ( var i = 0; i < children.Count(); i++ )
                        {
                            foreach ( var attr in attrs )
                            {
                                children[i].SetAttributeValue( attr.Key, originalChildren[i].GetAttributeValue( attr.Key ) );
                            }
                        }
                    }

                    item.SetAttributeValue( "NeedsPublicity", removedResources.Contains( "NeedsPublicity" ) ? "False" : original.GetAttributeValue( "NeedsPublicity" ) );
                    if ( item.GetAttributeValue( "NeedsPublicity" ) == "True" )
                    {
                        requestType.Add( "Publicity" );
                        var attrs = item.Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Publicity" ) && a.Key != "PublicityStartDate" && a.Key != "PublicityEndDate" );
                        foreach ( var attr in attrs )
                        {
                            item.SetAttributeValue( attr.Key, original.GetAttributeValue( attr.Key ) );
                        }
                    }
                    item.SetAttributeValue( "NeedsWebCalendar", removedResources.Contains( "NeedsWebCalendar" ) ? "False" : original.GetAttributeValue( "NeedsWebCalendar" ) );
                    if ( item.GetAttributeValue( "NeedsWebCalendar" ) == "True" )
                    {
                        requestType.Add( "Web Calendar" );
                        item.SetAttributeValue( "WebCalendarDescription", original.GetAttributeValue( "WebCalendarDescription" ) );
                    }
                    item.SetAttributeValue( "NeedsProductionAccommodations", removedResources.Contains( "NeedsProductionAccommodations" ) ? "False" : original.GetAttributeValue( "NeedsProductionAccommodations" ) );
                    if ( item.GetAttributeValue( "NeedsProductionAccommodations" ) == "True" )
                    {
                        requestType.Add( "Production" );
                        var attrs = item.Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Production" ) );
                        foreach ( var attr in attrs )
                        {
                            item.SetAttributeValue( attr.Key, original.GetAttributeValue( attr.Key ) );
                        }
                    }
                    if ( String.IsNullOrEmpty( eventDates ) )
                    {
                        eventDates = String.Join( ",", copyDates.Select( cd => DateTime.Parse( cd.newDate ) ).OrderBy( cd => cd ).Select( cd => cd.ToString( "yyyy-MM-dd" ) ) );
                    }
                    item.SetAttributeValue( "EventDates", eventDates );
                    item.SetAttributeValue( "RequestType", String.Join( ",", requestType ) );
                    List<ContentChannelItem> updated = new List<ContentChannelItem>();
                    if ( item.GetAttributeValue( "IsSame" ) == "False" )
                    {
                        for ( var i = 0; i < children.Count(); i++ )
                        {
                            var date = children[i].GetAttributeValue( "EventDate" );
                            var idx = copyDates.Select( cd => cd.originalDate ).ToList().IndexOf( date );
                            if ( idx >= 0 )
                            {
                                children[i].SetAttributeValue( "EventDate", copyDates[idx].newDate );
                                children[i].SetAttributeValue( "StartTime", originalChildren[idx].GetAttributeValue( "StartTime" ) );
                                children[i].SetAttributeValue( "EndTime", originalChildren[idx].GetAttributeValue( "EndTime" ) );
                                updated.Add( children[i] );
                            }
                        }
                        children = updated.OrderBy( cci => DateTime.Parse( cci.GetAttributeValue( "EventDate" ) ) ).ToList();
                    }
                    else
                    {
                        children[0].SetAttributeValue( "StartTime", originalChildren[0].GetAttributeValue( "StartTime" ) );
                        children[0].SetAttributeValue( "EndTime", originalChildren[0].GetAttributeValue( "EndTime" ) );
                    }

                    for ( int i = 0; i < children.Count(); i++ )
                    {
                        var detail = children[i];
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
                    return ActionOk( new { id = item.Id } );
                }
                else
                {
                    throw new Exception( "Id is required" );
                }

            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
                return ActionBadRequest( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult ProposedChangesAction( string id, string action )
        {
            try
            {
                RockContext context = new RockContext();
                var cci_svc = new ContentChannelItemService( context );
                var ccia_svc = new ContentChannelItemAssociationService( context );
                SetProperties();
                ContentChannelItem item = cci_svc.Get( id );
                if ( item != null )
                {
                    item.LoadAttributes();
                    RequestAuthorization auth = CheckRequestPermissions( item );
                    if ( !auth.CanEdit )
                    {
                        throw new UnauthorizedAccessException( "You do not have permission to make changes to request." );
                    }
                    string status = item.GetAttributeValue( "RequestStatus" );
                    if ( status != "Proposed Changes Denied" )
                    {
                        throw new Exception( "Unable to complete this action, request is not in an appropriate status." );
                    }
                    if ( action == "ChangesAccepted" )
                    {
                        item.SetAttributeValue( "RequestStatus", "Changes Accepted by User" );
                        item.SaveAttributeValue( "RequestStatus" );
                    }
                    else
                    {
                        if ( action == "Original" )
                        {
                            //Use Originally Approved
                            item.SetAttributeValue( "RequestStatus", "Approved" );
                            item.SaveAttributeValue( "RequestStatus" );
                        }
                        else if ( action == "Cancelled" )
                        {
                            //Set Request to Cancelled by User
                            item.SetAttributeValue( "RequestStatus", "Cancelled by User" );
                            item.SaveAttributeValue( "RequestStatus" );
                        }
                        var changesAssoc = item.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventChangesContentChannelId );
                        if ( changesAssoc != null )
                        {
                            var changes = changesAssoc.ChildContentChannelItem;
                            cci_svc.Delete( changes );
                            ccia_svc.Delete( changesAssoc );
                            var events = item.ChildItems.Where( ci => ci.ChildContentChannelItem != null && ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).ToList();
                            for ( int i = 0; i < events.Count(); i++ )
                            {
                                var eventChanges = events[i].ChildContentChannelItem.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId );
                                if ( eventChanges != null )
                                {
                                    cci_svc.Delete( eventChanges.ChildContentChannelItem );
                                    ccia_svc.Delete( eventChanges );
                                }
                            }
                            context.SaveChanges();
                        }
                    }
                    return ActionOk( new { id = id } );
                }
                else
                {
                    throw new Exception( "Item not found." );
                }
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );
                return ActionBadRequest( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult DeleteDraft( string id )
        {
            try
            {
                RockContext context = new RockContext();
                var cci_svc = new ContentChannelItemService( context );
                var ccia_svc = new ContentChannelItemAssociationService( context );
                SetProperties();
                ContentChannelItem item = cci_svc.Get( id );
                string title = item.Title;
                if ( item != null )
                {
                    item.LoadAttributes();
                    RequestAuthorization auth = CheckRequestPermissions( item );
                    if ( !auth.CanEdit )
                    {
                        throw new UnauthorizedAccessException( "You do not have permission to delete this request." );
                    }
                    string status = item.GetAttributeValue( "RequestStatus" );
                    if ( status != "Draft" )
                    {
                        throw new Exception( "Only drafts can be deleted." );
                    }

                    var children = item.ChildItems.ToList();
                    for ( int i = 0; i < children.Count; i++ )
                    {
                        cci_svc.Delete( children[i].ChildContentChannelItem );
                        ccia_svc.Delete( children[i] );
                    }
                    cci_svc.Delete( item );

                    context.SaveChanges();
                }
                else
                {
                    throw new Exception( "Draft not found." );
                }

                return ActionOk( "Draft for " + title + " has been deleted." );
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
        /// Loads the requests
        /// </summary>
        /// <returns></returns>
        private List<ContentChannelItemBag> LoadRequests( Filters filters = null )
        {
            int? id = PageParameter( PageParameterKey.RequestId ).AsIntegerOrNull();
            ContentChannelItem item = new ContentChannelItem();
            List<ContentChannelItem> itemList = new List<ContentChannelItem>();
            IEnumerable<ContentChannelItem> items = null;
            RockContext context = new RockContext();
            AttributeValueService av_svc = new AttributeValueService( context );
            var p = GetCurrentPerson();

            if ( filters == null )
            {
                List<string> defaultStatuses = GetAttributeValue( AttributeKey.DefaultStatuses ).Split( ',' ).Select( i => i.Trim() ).Where( s => !String.IsNullOrEmpty( s ) ).ToList();
                //Default Filters
                filters = new Filters()
                {
                    statuses = defaultStatuses
                };
            }

            items = new ContentChannelItemService( new RockContext() ).Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId );
            items.First().LoadAttributes();

            IEnumerable<ContentChannelItem> filtered_items = null;
            IEnumerable<ContentChannelItem> items_modified_match = null;
            string requestStatusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
            var requestStatusAttr = items.First().Attributes[requestStatusAttrKey];
            string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
            var requestResourcesAttr = items.First().Attributes[resourcesAttrKey];
            string eventDatesAttrKey = GetAttributeValue( AttributeKey.EventDatesAttrKey );
            var eventDatesAttr = items.First().Attributes[eventDatesAttrKey];
            string sharedWithAttrKey = GetAttributeValue( AttributeKey.SharedWithAttrKey );
            var sharedWithAttr = items.First().Attributes[sharedWithAttrKey];
            string ministryAttrKey = GetAttributeValue( AttributeKey.MinistryAttrKey );
            var ministryAttr = items.First().Attributes[ministryAttrKey];
            Guid? sharedRequestGroupTypeGuid = GetAttributeValue( AttributeKey.SharingGroupType ).AsGuidOrNull();
            List<int?> aliasIds = new List<int?>();

            //Only Requests Created By the Current Person or Shared With the Current Person
            if ( sharedRequestGroupTypeGuid.HasValue )
            {
                //Shared requests are configured, find any for the current user.
                var SharedRequestGT = new GroupTypeService( context ).Get( sharedRequestGroupTypeGuid.Value );
                var groups = new GroupService( context ).Queryable().Where( g => g.GroupTypeId == SharedRequestGT.Id );
                var groupMembers = new GroupMemberService( context ).Queryable().Where( gm => gm.PersonId == p.Id && gm.GroupRole.Name == "Can View" );
                groups = from g in groups
                         join gm in groupMembers on g.Id equals gm.GroupId
                         select g;
                var grpList = groups.ToList();
                for ( int k = 0; k < grpList.Count(); k++ )
                {
                    var ids = grpList[k].Members.Where( gm => gm.GroupRole.Name == "Request Creator" ).Select( gm => gm.Person.PrimaryAliasId );
                    aliasIds.AddRange( ids );
                }
            }

            List<RequestAuthorization> sharedRequestAuth = GetSharedRequests( context, p, sharedWithAttr, ministryAttr );
            List<int> sharedRequests = sharedRequestAuth.Select( ra => ra.RequestId ).ToList();
            items = items.Where( i =>
            {
                if ( i.CreatedByPersonAliasId == p.PrimaryAliasId )
                {
                    return true;
                }
                //We don't want personal requests in the group based sharing, but if we specifically share it with a person that's ok
                if ( sharedRequests.Contains( i.Id ) )
                {
                    return true;
                }
                return false;
            } );

            //OR Filter
            if ( filters.eventModified != null )
            {
                if ( !String.IsNullOrEmpty( filters.eventModified.lowerValue ) && !String.IsNullOrEmpty( filters.eventModified.upperValue ) )
                {
                    items_modified_match = items.Where( i => i.ModifiedDateTime >= DateTime.Parse( filters.eventModified.lowerValue ) && i.ModifiedDateTime <= DateTime.Parse( filters.eventModified.upperValue ).EndOfDay() );
                }
                else
                {
                    if ( !String.IsNullOrEmpty( filters.eventModified.lowerValue ) )
                    {
                        items_modified_match = items.Where( i => i.ModifiedDateTime >= DateTime.Parse( filters.eventModified.lowerValue ) );
                    }
                    if ( !String.IsNullOrEmpty( filters.eventModified.upperValue ) )
                    {
                        items_modified_match = items.Where( i => i.ModifiedDateTime <= DateTime.Parse( filters.eventModified.upperValue ).EndOfDay() );
                    }
                }
            }
            //AND Filters
            filtered_items = items.Where( i =>
            {
                bool meetsCriteria = true;
                if ( !String.IsNullOrEmpty( filters.title ) )
                {
                    if ( !i.Title.ToLower().Contains( filters.title.ToLower() ) )
                    {
                        meetsCriteria = false;
                    }
                }

                return meetsCriteria;
            } );

            IEnumerable<AttributeValue> eventDates = null;
            IEnumerable<AttributeValue> requestedResources = null;
            IQueryable<AttributeValue> requestStatuses = null;

            if ( filters.eventDates != null && !String.IsNullOrEmpty( filters.eventDates.lowerValue ) && !String.IsNullOrEmpty( filters.eventDates.upperValue ) )
            {
                DateTime? lowerValue = null;
                DateTime? upperValue = null;
                if ( !String.IsNullOrEmpty( filters.eventDates.lowerValue ) )
                {
                    lowerValue = DateTime.Parse( filters.eventDates.lowerValue );
                }
                if ( !String.IsNullOrEmpty( filters.eventDates.upperValue ) )
                {
                    upperValue = DateTime.Parse( filters.eventDates.upperValue );
                }
                if ( lowerValue.HasValue || upperValue.HasValue )
                {
                    eventDates = av_svc.Queryable().Where( av => av.AttributeId == eventDatesAttr.Id ).ToList().Where( av =>
                    {
                        bool dateInRange = false;

                        List<DateTime> dates = av.Value != "" ? av.Value.Split( ',' ).Select( d => DateTime.Parse( d.Trim() ) ).ToList() : new List<DateTime>();
                        for ( int i = 0; i < dates.Count(); i++ )
                        {
                            if ( lowerValue.HasValue && upperValue.HasValue )
                            {
                                if ( dates[i] >= lowerValue.Value && dates[i] <= upperValue.Value )
                                {
                                    dateInRange = true;
                                }
                            }
                            else
                            {
                                if ( lowerValue.HasValue )
                                {
                                    if ( dates[i] >= lowerValue.Value )
                                    {
                                        dateInRange = true;
                                    }
                                }
                                if ( upperValue.HasValue )
                                {
                                    if ( dates[i] <= upperValue.Value )
                                    {
                                        dateInRange = true;
                                    }
                                }
                            }

                        }
                        return dateInRange;
                    } );
                    filtered_items = filtered_items.Join( eventDates,
                            i => i.Id,
                            av => av.EntityId,
                            ( i, av ) => i
                        );
                }
            }
            if ( filters.ministry != null && !String.IsNullOrEmpty( filters.ministry ) )
            {
                var ministries = av_svc.Queryable().Where( av => av.AttributeId == ministryAttr.Id && av.Value.ToLower() == filters.ministry.ToLower() );
                filtered_items = filtered_items.Join( ministries,
                        i => i.Id,
                        av => av.EntityId,
                        ( i, av ) => i
                    );
            }
            if ( filters.resources != null && filters.resources.Count() > 0 )
            {
                requestedResources = av_svc.Queryable().Where( av => av.AttributeId == requestResourcesAttr.Id ).ToList().Where( av =>
                {
                    var resources = av.Value.Split( ',' ).Select( v => v.Trim() ).ToList();
                    var intersect = filters.resources.Intersect( resources );
                    if ( intersect.Count() > 0 )
                    {
                        return true;
                    }
                    return false;
                } );
                filtered_items = filtered_items.Join( requestedResources,
                        i => i.Id,
                        av => av.EntityId,
                        ( i, av ) => i
                    );
            }
            if ( filters.statuses.Count() > 0 )
            {
                requestStatuses = av_svc.Queryable().Where( av => av.AttributeId == requestStatusAttr.Id && filters.statuses.Contains( av.Value ) );
                filtered_items = filtered_items.Join( requestStatuses,
                        i => i.Id,
                        av => av.EntityId,
                        ( i, av ) => i
                    );
            }
            filtered_items = filtered_items.OrderBy( i => i.Title );
            if ( items_modified_match != null && filtered_items != null )
            {
                itemList = filtered_items.Union( items_modified_match ).Distinct().ToList();

            }
            else if ( items_modified_match != null )
            {
                itemList = items_modified_match.ToList();
            }
            else
            {
                itemList = filtered_items.ToList();
            }

            //Make sure desired item is in list
            if ( id.HasValue )
            {
                var exists = items.FirstOrDefault( i => i.Id == id.Value );
                if ( exists == null )
                {
                    item = new ContentChannelItemService( context ).Get( id.Value );
                    if ( item != null )
                    {
                        if ( item.ContentChannelId == EventChangesContentChannelId )
                        {
                            var parent = item.ParentItems.FirstOrDefault( pi => pi.ContentChannelItem.ContentChannelId == EventContentChannelId );
                            if ( parent != null )
                            {
                                item = parent.ContentChannelItem;
                                exists = items.FirstOrDefault( i => i.Id == item.Id );
                                if ( exists == null )
                                {
                                    itemList.Add( item );
                                }
                            }
                        }
                        else
                        {
                            itemList.Add( item );
                        }
                    }
                }
            }

            return itemList.OrderByDescending( i => i.ModifiedDateTime ).Select( cci =>
            {
                var bag = EventFormHelper.GetCommonContentChannelItemEntityBag( cci );
                bag.CreatedBy = cci.CreatedByPersonName;
                bag.ModifiedBy = cci.ModifiedByPersonName;
                bag.Id = cci.Id;
                //Update Authorization on Request to use in Dashboard 
                bag.CanEdit = true;
                var auth = sharedRequestAuth.FirstOrDefault( ra => ra.RequestId == cci.Id );
                if ( auth != null )
                {
                    bag.CanEdit = auth.CanEdit;
                }
                cci.LoadAttributes();
                bag.LoadAttributesAndValuesForPublicView( cci, RequestContext.CurrentPerson );
                return bag;
            } ).Where( ccib =>
                //Run Submitted filter criteria now that we have Created By and Modified By names as strings
                String.IsNullOrEmpty( filters.submitter ) ||
                ( !String.IsNullOrEmpty( ccib.CreatedBy ) && ccib.CreatedBy.ToLower().Contains( filters.submitter.ToLower() ) ) ||
                ( !String.IsNullOrEmpty( ccib.ModifiedBy ) && ccib.ModifiedBy.ToLower().Contains( filters.submitter.ToLower() ) )
            ).ToList();
        }


        private List<ContentChannelItemBag> LoadRequestsSQL( Filters filters = null )
        {
            int? id = PageParameter( PageParameterKey.RequestId ).AsIntegerOrNull();
            Person p = GetCurrentPerson();

            if ( filters == null )
            {
                //Default Filters
                filters = new Filters()
                {
                    statuses = GetAttributeValue( AttributeKey.DefaultStatuses ).Split( ',' ).Select( i => i.Trim() ).ToList(),
                };
                filters.eventModified = new DateRangeParts()
                {
                    lowerValue = RockDateTime.Now.AddDays( -14 ).ToString( "yyyy-MM-dd" ),
                    upperValue = RockDateTime.Now.ToString( "yyyy-MM-dd" )
                };
            }

            using ( RockContext context = new RockContext() )
            {
                //Attributes
                AttributeService attr_svc = new AttributeService( context );
                string statusAttrId = "", isValidAttrId = "", datesAttrId = "", resourceAttrId = "", ministryAttrId = "", sharedWithAttrId = "", sharingGroupTypeId = "";
                string requestStatusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
                if ( !String.IsNullOrEmpty( requestStatusAttrKey ) )
                {
                    statusAttrId = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == requestStatusAttrKey ).Id.ToString();
                }
                string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
                if ( !String.IsNullOrEmpty( resourcesAttrKey ) )
                {
                    resourceAttrId = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == resourcesAttrKey ).Id.ToString();
                }
                string isValidAttrKey = GetAttributeValue( AttributeKey.IsValidAttrKey );
                if ( !String.IsNullOrEmpty( resourcesAttrKey ) )
                {
                    isValidAttrId = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == isValidAttrKey ).Id.ToString();
                }
                string datesAttrKey = GetAttributeValue( AttributeKey.EventDatesAttrKey );
                if ( !String.IsNullOrEmpty( resourcesAttrKey ) )
                {
                    datesAttrId = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == datesAttrKey ).Id.ToString();
                }
                string ministryAttrKey = GetAttributeValue( AttributeKey.MinistryAttrKey );
                if ( !String.IsNullOrEmpty( resourcesAttrKey ) )
                {
                    ministryAttrId = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == ministryAttrKey ).Id.ToString();
                }
                string sharedWithAttrKey = GetAttributeValue( AttributeKey.SharedWithAttrKey );
                if ( !String.IsNullOrEmpty( sharedWithAttrKey ) )
                {
                    sharedWithAttrId = attr_svc.Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == EventContentChannelTypeId.ToString() && a.Key == sharedWithAttrKey ).Id.ToString();
                }
                Guid? sharingGroupTypeGuid = GetAttributeValue( AttributeKey.SharingGroupType ).AsGuidOrNull();
                if ( sharingGroupTypeGuid.HasValue )
                {
                    sharingGroupTypeId = new GroupTypeService( context ).Get( sharingGroupTypeGuid.Value ).Id.ToString();
                }

                var sqlParams = new SqlParameter[] {
                    new SqlParameter( "@ContentChannelId", EventContentChannelId ),
                    new SqlParameter( "@CommentChannelId", EventCommentsContentChannelId ),
                    new SqlParameter( "@StatusAttrId", statusAttrId ),
                    new SqlParameter( "@IsValidAttrId", isValidAttrId ),
                    new SqlParameter( "@DatesAttrId", datesAttrId ),
                    new SqlParameter( "@ResourceAttrId", resourceAttrId ),
                    new SqlParameter( "@MinistryAttrId", ministryAttrId ),
                    new SqlParameter( "@SharedWithAttrId", sharedWithAttrId ),
                    new SqlParameter( "@SharingGroupTypeId", sharingGroupTypeId ),
                    new SqlParameter( "@CurrentPersonId", p.Id ),
                    new SqlParameter( "@ModifiedLowerBound", filters.eventModified?.lowerValue ?? "" ),
                    new SqlParameter( "@ModifiedUpperBound", filters.eventModified?.upperValue ?? "" ),
                    new SqlParameter( "@EventDateLowerBound", filters.eventDates?.lowerValue ?? "" ),
                    new SqlParameter( "@EventDateUpperBound", filters.eventDates?.upperValue ?? "" ),
                    new SqlParameter( "@StatusFilter", filters.statuses != null ? String.Join(",", filters.statuses) : "" ),
                    new SqlParameter( "@ResourceFilter", filters.resources != null ? String.Join(",", filters.resources) : "" ),
                    new SqlParameter( "@TitleFilter", filters.title ?? "" ),
                    new SqlParameter( "@SubmitterFilter", filters.submitter ?? "" ),
                    new SqlParameter( "@MinistryFilter", filters.ministry ?? "" ),
                    new SqlParameter( "@AdditionalEventId", id.ToString() ?? "" )
                };
                var rawQuery = context.Database.SqlQuery<RequestGridView>( $@"
    DECLARE @EventDates TABLE
                        (
                            EntityId   INT,
                            EventDates NVARCHAR(MAX),
                            Date       DATE
                        );
    DECLARE @EventResources TABLE
                            (
                                EntityId  INT,
                                Resources NVARCHAR(750),
                                Resource  VARCHAR(100)
                            );
    DECLARE @ExplicitShares TABLE
                            (
                                EntityId           INT,
                                SharedWithPersonId INT
                            );
    DECLARE @GroupShares TABLE
                         (
                             AllowedId INT
                         );
    DECLARE @StatusFilterTable TABLE
                               (
                                   Status VARCHAR(50)
                               );
    DECLARE @ResourceFilterTable TABLE
                                 (
                                     FilterResource VARCHAR(50)
                                 );
    DECLARE @CommentData TABLE
                         (
                             ContentChannelItemId INT,
                             CommentNotifications INT
                         );

    INSERT INTO @EventDates
    SELECT EntityId, AttributeValue.Value AS EventDates, CAST(Dates.value AS DATE) AS Date
    FROM AttributeValue
             CROSS APPLY STRING_SPLIT(AttributeValue.Value, ',') AS Dates
    WHERE AttributeId = @DatesAttrId;

    INSERT INTO @EventResources
    SELECT EntityId, AttributeValue.Value AS Resources, TRIM(Resources.value) AS Resource
    FROM AttributeValue
             CROSS APPLY STRING_SPLIT(AttributeValue.Value, ',') AS Resources
    WHERE AttributeId = @ResourceAttrId;

    INSERT INTO @ExplicitShares
    SELECT *
    FROM (SELECT EntityId, Ids.value AS SharedWithPersonId
          FROM AttributeValue
                   CROSS APPLY STRING_SPLIT(AttributeValue.Value, ',') AS Ids
          WHERE AttributeId = @SharedWithAttrId) AS SharedWithIds
    WHERE SharedWithPersonId = @CurrentPersonId

    INSERT INTO @GroupShares
    SELECT GroupMember.PersonId AS AllowedId
    FROM GroupMember
             INNER JOIN GroupTypeRole ON GroupMember.GroupRoleId = GroupTypeRole.Id
             INNER JOIN (SELECT GroupId, PersonId, GroupRoleId, Name
                         FROM GroupMember
                                  INNER JOIN GroupTypeRole ON GroupMember.GroupRoleId = GroupTypeRole.Id
                         WHERE GroupMember.GroupTypeId = @SharingGroupTypeId
                           AND Name NOT LIKE '%creator%'
                           AND PersonId = @CurrentPersonId) AS UserAllowed ON UserAllowed.GroupId = GroupMember.GroupId
    WHERE GroupMember.GroupTypeId = @SharingGroupTypeId
      AND GroupTypeRole.Name LIKE '%creator%'

    INSERT INTO @StatusFilterTable SELECT TRIM(value) AS Status FROM STRING_SPLIT(@StatusFilter, ',');

    INSERT INTO @ResourceFilterTable SELECT TRIM(value) AS FilterResource FROM STRING_SPLIT(@ResourceFilter, ',');

    INSERT INTO @CommentData
    SELECT ContentChannelItemId,
           CAST(IIF(MyFirstComment IS NULL OR MyFirstComment > FirstUserComment, LastUserComment,
                    0) AS INT) AS CommentNotifications
    FROM (SELECT ContentChannelItemId,
                 MAX(MyFirstComment)   AS MyFirstComment,
                 MAX(MyLastComment)    AS MyLastComment,
                 MAX(FirstUserComment) AS FirstUserComment,
                 MAX(LastUserComment)  AS LastUserComment
          FROM (SELECT ContentChannelItemId,
                       IIF(IsCurrentUser = 1, MinComment, NULL) AS MyFirstComment,
                       IIF(IsCurrentUser = 1, MaxComment, NULL) AS MyLastComment,
                       IIF(IsCurrentUser = 0, MinComment, NULL) AS FirstUserComment,
                       IIF(IsCurrentUser = 0, MaxComment, NULL) AS LastUserComment,
                       IsCurrentUser,
                       MinComment,
                       MaxComment
                FROM (SELECT ContentChannelItemId,
                             IsCurrentUser,
                             MIN(CommentOrderDesc) AS MinComment,
                             MAX(CommentOrderDesc) AS MaxComment
                      FROM (SELECT *,
                                   ROW_NUMBER() OVER (PARTITION BY ContentChannelItemId ORDER BY CreatedDateTime DESC) AS CommentOrderDesc
                            FROM (SELECT ContentChannelItemId,
                                         ChildContentChannelItemId,
                                         ContentChannelItem.CreatedDateTime,
                                         PersonId,
                                         IIF(PersonId = @CurrentPersonId, 1, 0) AS IsCurrentUser
                                  FROM ContentChannelItem
                                           INNER JOIN ContentChannelItemAssociation
                                                      ON ContentChannelItem.Id =
                                                         ContentChannelItemAssociation.ChildContentChannelItemId
                                           INNER JOIN PersonAlias
                                                      ON ContentChannelItem.CreatedByPersonAliasId = PersonAlias.Id
                                  WHERE ContentChannelId = @CommentChannelId) AS CommentData) AS CommentData
                      GROUP BY ContentChannelItemId, IsCurrentUser) AS CommentData) AS CommentData
          GROUP BY ContentChannelItemId) AS CommentData;

    SELECT Id,
           Title,
           CreatedByPersonAliasId,
           ModifiedByPersonAliasId,
           StartDateTime,
           ModifiedDateTime,
           IsValid,
           Ministry,
           Status,
           CreatedBy,
           ModifiedBy,
           EventDates,
           Resources,
           ContentChannelItemId,
           CommentNotifications
    FROM (SELECT Id,
                 Title,
                 CreatedByPersonAliasId,
                 ModifiedByPersonAliasId,
                 StartDateTime,
                 ModifiedDateTime,
                 IsValid,
                 Ministry,
                 Events.Status,
                 CreatedBy,
                 ModifiedBy,
                 EventDates,
                 Resources
          FROM (SELECT Id,
                       Title,
                       StartDateTime,
                       ModifiedDateTime,
                       CreatedByPersonAliasId,
                       MAX(CreatedByPersonId)  AS CreatedByPersonId,
                       ModifiedByPersonAliasId,
                       MAX(ModifiedByPersonId) AS ModifiedByPersonId,
                       MAX(IsValid)            AS IsValid,
                       MAX(Status)             AS Status,
                       MAX(Ministry)           AS Ministry,
                       MAX(MinistryValue)      AS MinistryValue,
                       MAX(CreatedBy)          AS CreatedBy,
                       MAX(ModifiedBy)         AS ModifiedBy
                FROM (SELECT ContentChannelItem.Id,
                             Title,
                             StartDateTime,
                             ContentChannelItem.ModifiedDateTime,
                             ContentChannelItem.CreatedByPersonAliasId,
                             IIF(PA.Id = ContentChannelItem.CreatedByPersonAliasId, PA.PersonId,
                                 NULL)                                                    AS CreatedByPersonId,
                             IIF(PA.Id = ContentChannelItem.ModifiedByPersonAliasId, PA.PersonId,
                                 NULL)                                                    AS ModifiedByPersonId,
                             ContentChannelItem.ModifiedByPersonAliasId,
                             IIF(AttributeId = @IsValidAttrId, Value, NULL)               AS IsValid,
                             IIF(AttributeId = @StatusAttrId, Value, NULL)                AS Status,
                             IIF(AttributeId = @MinistryAttrId, PersistedTextValue, NULL) AS Ministry,
                             IIF(AttributeId = @MinistryAttrId, Value, NULL)              AS MinistryValue,
                             IIF(PA.Id = ContentChannelItem.CreatedByPersonAliasId, CONCAT(NickName, ' ', LastName),
                                 NULL)                                                    AS CreatedBy,
                             IIF(PA.Id = ContentChannelItem.ModifiedByPersonAliasId, CONCAT(NickName, ' ', LastName),
                                 NULL)                                                    AS ModifiedBy
                      FROM ContentChannelItem
                               INNER JOIN AttributeValue ON EntityId = ContentChannelItem.Id
                               INNER JOIN PersonAlias PA ON ContentChannelItem.CreatedByPersonAliasId = PA.Id OR
                                                            ContentChannelItem.ModifiedByPersonAliasId = PA.Id
                               INNER JOIN Person ON PA.PersonId = Person.Id
                      WHERE ContentChannelId = @ContentChannelId
                        AND (@TitleFilter IS NULL OR Title LIKE '%' + @TitleFilter + '%' OR
                             ContentChannelItem.Id = @AdditionalEventId)
                        AND AttributeId IN (@IsValidAttrId, @StatusAttrId, @MinistryAttrId)) AS Events
                GROUP BY Id, Title, StartDateTime, ModifiedDateTime, CreatedByPersonAliasId,
                         ModifiedByPersonAliasId) AS Events
                   INNER JOIN @EventDates EventDates ON EventDates.EntityId = Events.Id
                   INNER JOIN @EventResources EventResources ON EventResources.EntityId = Events.Id
                   LEFT OUTER JOIN @StatusFilterTable StatusFilter ON StatusFilter.Status = Events.Status
                   LEFT OUTER JOIN @ResourceFilterTable ResourceFilter
                                   ON ResourceFilter.FilterResource = EventResources.Resource
          WHERE (@ModifiedLowerBound = '' OR (ModifiedDateTime >= @ModifiedLowerBound) OR
                 Events.Id = @AdditionalEventId)
            AND (@ModifiedUpperBound = '' OR ModifiedDateTime <= @ModifiedUpperBound OR Events.Id = @AdditionalEventId)
            AND (@EventDateLowerBound = '' OR Date >= @EventDateLowerBound OR Events.Id = @AdditionalEventId)
            AND (@EventDateUpperBound = '' OR Date <= @EventDateUpperBound OR Events.Id = @AdditionalEventId)
            AND (CreatedByPersonId = @CurrentPersonId OR ModifiedByPersonId = @CurrentPersonId OR
                 (CreatedByPersonId IN (SELECT * FROM @GroupShares) AND Ministry NOT LIKE '%personal%') OR
                 (ModifiedByPersonId IN (SELECT * FROM @GroupShares) AND Ministry NOT LIKE '%personal%') OR
                 Id IN (SELECT EntityId FROM @ExplicitShares) OR Events.Id = @AdditionalEventId)
            AND (@MinistryFilter = '' OR @MinistryFilter = MinistryValue OR Events.Id = @AdditionalEventId)
            AND (@StatusFilter = '' OR StatusFilter.Status IS NOT NULL OR Events.Id = @AdditionalEventId)
            AND (@ResourceFilter = '' OR ResourceFilter.FilterResource IS NOT NULL OR Events.Id = @AdditionalEventId)
          GROUP BY Id, Title, CreatedByPersonAliasId, ModifiedByPersonAliasId, StartDateTime, ModifiedDateTime, IsValid,
                   Ministry, Events.Status, CreatedBy, ModifiedBy, EventDates, Resources) AS Events
             LEFT OUTER JOIN @CommentData AS Comments
                             ON ContentChannelItemId = Id;
                ", sqlParams );
                var query = rawQuery.ToList();

                ContentChannelItemService cci_svc = new ContentChannelItemService( context );

                var items = cci_svc.Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId ).ToList().Join( query,
                    cci => cci.Id,
                    qry => qry.Id,
                    ( cci, qry ) => cci
                ).OrderByDescending( i => i.ModifiedDateTime ).ToList().Select( cci =>
                {
                    var row = query.FirstOrDefault( qry => qry.Id == cci.Id );
                    var bag = EventFormHelper.GetCommonContentChannelItemEntityBag( cci );
                    bag.AttributeValues = new Dictionary<string, string>();
                    if ( row != null )
                    {
                        bag.CreatedBy = row.CreatedBy;
                        bag.ModifiedBy = row.ModifiedBy;
                        bag.AttributeValues.Add( "EventDates", row.EventDates );
                        bag.AttributeValues.Add( "RequestIsValid", row.IsValid );
                        bag.AttributeValues.Add( "Ministry", row.Ministry );
                        bag.AttributeValues.Add( "RequestType", row.Resources );
                        bag.AttributeValues.Add( "RequestStatus", row.Status );
                        bag.AttributeValues.Add( "CommentNotifications", row.UnreadComments.ToString() );
                    }
                    return bag;
                } ).ToList();

                return items;
            }

        }

        private List<ContentChannelItemBag> LoadRequestsNew( Filters filters = null )
        {
            using ( RockContext context = new RockContext() )
            {
                ContentChannelItemService cci_svc = new ContentChannelItemService( context );
                AttributeValueService av_svc = new AttributeValueService( context );

                int? id = PageParameter( PageParameterKey.RequestId ).AsIntegerOrNull();
                var p = GetCurrentPerson();
                List<int> aliasIds = p.Aliases.Select( pa => pa.Id ).ToList();

                if ( filters == null )
                {
                    List<string> defaultStatuses = GetAttributeValue( AttributeKey.DefaultStatuses ).Split( ',' ).Select( i => i.Trim() ).Where( s => !String.IsNullOrEmpty( s ) ).ToList();
                    //Default Filters
                    filters = new Filters()
                    {
                        statuses = defaultStatuses
                    };
                }

                //Attributes
                List<Guid> attrGuids = GetAttributeValues( "GridAttrs" ).AsGuidList();
                List<int> attrIds = new List<int>();
                List<AttributeCache> attrs = new List<AttributeCache>();

                foreach ( Guid attrGuid in attrGuids )
                {
                    var attr = AttributeCache.Get( attrGuid );
                    attrIds.Add( attr.Id );
                    attrs.Add( attr );
                }

                var itemQry = cci_svc.Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId );
                var avQry = av_svc.Queryable().Where( av => attrIds.Contains( av.AttributeId ) );

                Guid? sharingGroupTypeGuid = GetAttributeValue( AttributeKey.SharingGroupType ).AsGuidOrNull();
                if ( sharingGroupTypeGuid.HasValue )
                {
                    GroupMemberService gm_svc = new GroupMemberService( context );
                    GroupType sharingGT = new GroupTypeService( context ).Get( sharingGroupTypeGuid.Value );
                    var memberships = gm_svc.Queryable().Where( gm => gm.GroupTypeId == sharingGT.Id && gm.PersonId == p.Id && gm.GroupRoleId == 234 ).Select( gm => gm.GroupId ).ToList();
                    if ( memberships.Count() > 0 )
                    {
                        var otherAliasIds = gm_svc.Queryable().Where( gm => gm.GroupTypeId == sharingGT.Id && memberships.Contains( gm.GroupId ) && gm.GroupRoleId == 235 ).SelectMany( gm => gm.Person.Aliases.Select( pa => pa.Id ) ).ToList();
                        aliasIds.AddRange( otherAliasIds );
                    }
                }

                string sharedWithAttrKey = GetAttributeValue( AttributeKey.SharedWithAttrKey );
                var sharedItemsQry = itemQry;
                List<int> explicitSharedItemIds = new List<int>();
                if ( !String.IsNullOrEmpty( sharedWithAttrKey ) )
                {
                    var sharedWithAttr = attrs.FirstOrDefault( attr => attr.Key == sharedWithAttrKey );
                    var sharingAvQry = avQry.Where( av => av.AttributeId == sharedWithAttr.Id && av.Value != "" ).ToList().Where( av => av.Value.Split( ',' ).ToList().Contains( p.Id.ToString() ) );
                    explicitSharedItemIds = sharingAvQry.Select( av => av.EntityId.Value ).ToList();
                }

                itemQry = itemQry.Where( cci => ( cci.CreatedByPersonAliasId.HasValue && aliasIds.Contains( cci.CreatedByPersonAliasId.Value ) ) || ( cci.ModifiedByPersonAliasId.HasValue && aliasIds.Contains( cci.ModifiedByPersonAliasId.Value ) ) || explicitSharedItemIds.Contains( cci.Id ) ).OrderByDescending( cci => cci.ModifiedDateTime );

                var items = itemQry.ToList();

                var comments = cci_svc.Queryable().Where( cci => cci.ContentChannelId == EventCommentsContentChannelId );
                var comment_assoc = new ContentChannelItemAssociationService( context ).Queryable().Join( itemQry,
                    assoc => assoc.ContentChannelItemId,
                    cci => cci.Id,
                    ( assoc, cci ) => assoc
                ).Join( comments,
                    assoc => assoc.ChildContentChannelItemId,
                    cci => cci.Id,
                    ( assoc, cci ) => assoc
                ).ToList();

                return items.Select( cci =>
                {
                    var avs = avQry.Where( av => av.EntityId == cci.Id );
                    var bag = EventFormHelper.GetCommonContentChannelItemEntityBag( cci );
                    bag.CreatedBy = cci.CreatedByPersonName;
                    bag.ModifiedBy = cci.ModifiedByPersonName;
                    bag.AttributeValues = new Dictionary<string, string>();
                    foreach ( var attr in attrs )
                    {
                        var av = avs.FirstOrDefault( av => av.AttributeId == attr.Id );
                        if ( av != null )
                        {
                            bag.AttributeValues.Add( attr.Key, av.ValueFormatted );
                        }
                    }
                    var firstUser = comment_assoc.Where( cci_assoc => cci_assoc.ContentChannelItemId == cci.Id && cci_assoc.CreatedByPersonId == p.Id ).OrderByDescending( cci_assoc => cci_assoc.CreatedDateTime ).FirstOrDefault();
                    if ( firstUser != null )
                    {
                        var nonUser = comment_assoc.Where( cci_assoc => cci_assoc.ContentChannelItemId == cci.Id && cci_assoc.CreatedByPersonId != p.Id && cci_assoc.CreatedDateTime < firstUser.CreatedDateTime ).Count();
                        if ( nonUser > 0 )
                        {
                            bag.AttributeValues.Add( "CommentNotifications", nonUser.ToString() );
                        }
                    }
                    return bag;
                } ).ToList();
            }
        }

        /// <summary>
        /// Return true/false is the current person a member of the given Security Role
        /// </summary>
        /// <returns></returns>
        private bool CheckSecurityRole( RockContext rockContext, string attrKey )
        {
            bool hasRole = false;
            Person p = GetCurrentPerson();
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

        private List<RequestAuthorization> GetSharedRequests( RockContext context, Person p, AttributeCache sharedWithAttr, AttributeCache ministryAttr )
        {
            List<RequestAuthorization> sharedRequests = new List<RequestAuthorization>();
            Guid? sharedRequestGroupTypeGuid = GetAttributeValue( AttributeKey.SharingGroupType ).AsGuidOrNull();

            if ( sharedWithAttr != null )
            {
                sharedRequests = new AttributeValueService( context ).Queryable().Where( av => av.AttributeId == sharedWithAttr.Id ).ToList().Where( av => !String.IsNullOrEmpty( av.Value ) && av.Value.Split( ',' ).Contains( p.Id.ToString() ) ).Select( av => new RequestAuthorization { RequestId = av.EntityId.Value, CanEdit = true } ).ToList();
            }

            if ( sharedRequestGroupTypeGuid.HasValue )
            {
                Guid ministryListGuid = Guid.Empty;
                List<DefinedValue> ministries = new List<DefinedValue>();
                List<int?> personalRequests = new List<int?>();
                DefinedValue personalRequest = null;

                if ( Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryListGuid ) )
                {
                    DefinedType ministryDT = new DefinedTypeService( context ).Get( ministryListGuid );
                    ministries = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == ministryDT.Id ).ToList();
                    personalRequest = ministries.FirstOrDefault( dv => dv.Value.ToLower().Contains( "personal" ) );
                }

                //Only Requests Created By the Current Person or Shared With the Current Person
                //Shared requests are configured, find any for the current user.
                var sharedRequestGT = new GroupTypeService( context ).Get( sharedRequestGroupTypeGuid.Value );
                //Anything but Request Creator roles
                var sharingMembership = new GroupMemberService( context ).Queryable().Where( gm => gm.GroupTypeId == sharedRequestGT.Id && gm.PersonId == p.Id && !gm.GroupRole.IsLeader ).ToList();
                sharingMembership.LoadAttributes();

                for ( int k = 0; k < sharingMembership.Count(); k++ )
                {
                    //List<DefinedValue> limitedToMinistry = null;
                    List<string> limitedToMinistryGuid = sharingMembership[k].GetAttributeValue( "Ministry" ).Split( ',' ).ToList();
                    bool membershipHasEdit = false;
                    if ( sharingMembership[k].GroupRole.Name == "Can Edit" )
                    {
                        membershipHasEdit = true;
                    }

                    GroupMember creator = sharingMembership[k].Group.Members.FirstOrDefault( gm => gm.GroupRole.IsLeader );
                    if ( creator != null )
                    {
                        var aliasIds = creator.Person.Aliases.Select( pa => pa.Id ).ToList();
                        var creatorsRequests = new ContentChannelItemService( context ).Queryable().Where( cci => aliasIds.Contains( cci.CreatedByPersonAliasId.Value ) );
                        var requestMinistries = new AttributeValueService( context ).Queryable().Where( av => av.AttributeId == ministryAttr.Id && av.Value != personalRequest.Guid.ToString() );
                        if ( limitedToMinistryGuid.Any() )
                        {
                            requestMinistries = requestMinistries.Where( av => limitedToMinistryGuid.Contains( av.Value ) );
                        }
                        creatorsRequests = creatorsRequests.Join( requestMinistries,
                            cci => cci.Id,
                            av => av.EntityId,
                            ( cci, av ) => cci
                        );
                        List<RequestAuthorization> creatorRequestIds = creatorsRequests.Select( cci => new RequestAuthorization { RequestId = cci.Id, CanEdit = membershipHasEdit } ).ToList();

                        sharedRequests.AddRange( creatorRequestIds );
                    }
                }
            }
            return sharedRequests;
        }

        /// <summary>
        /// Method to verify the permissions the current person has for this request
        /// </summary>
        /// <param name="request">The Event Request Content Channel Item</param>
        /// <returns>Auth model with view and edit permissions for the current person</returns>
        private RequestAuthorization CheckRequestPermissions( ContentChannelItem request )
        {
            using ( RockContext context = new RockContext() )
            {
                var p = GetCurrentPerson();
                bool isEventAdmin = CheckSecurityRole( context, AttributeKey.EventAdminRole );
                bool isRoomAdmin = CheckSecurityRole( context, AttributeKey.RoomAdminRole );
                Guid? sharedRequestGroupTypeGuid = GetAttributeValue( AttributeKey.SharingGroupType ).AsGuidOrNull();
                return EventFormShared.CheckRequestPermissions( request, p, isEventAdmin, isRoomAdmin, sharedRequestGroupTypeGuid );
            }
        }

        private ContentChannelItem FromViewModel( ContentChannelItem viewModel )
        {
            RockContext context = new RockContext();
            Rock.Model.Person p = GetCurrentPerson();
            ContentChannelItem item = new ContentChannelItem()
            {
                ContentChannelId = viewModel.ContentChannelId,
                ContentChannelTypeId = viewModel.ContentChannelTypeId
            };
            if ( !String.IsNullOrEmpty( viewModel.IdKey ) )
            {
                item = new ContentChannelItemService( context ).Get( viewModel.IdKey );
            }
            item.LoadAttributes();
            item.Title = viewModel.Title;
            foreach ( KeyValuePair<string, AttributeValueCache> av in viewModel.AttributeValues )
            {
                item.SetPublicAttributeValue( av.Key, av.Value.Value, p, false );
            }

            return item;
        }

        private void SetProperties()
        {
            RockContext rockContext = new RockContext();
            Guid eventCCGuid = Guid.Empty;
            Guid eventDetailsCCGuid = Guid.Empty;
            Guid eventChangesCCGuid = Guid.Empty;
            Guid eventDetailsChangesCCGuid = Guid.Empty;
            Guid eventCommentsCCGuid = Guid.Empty;

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
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventChangesContentChannel ), out eventChangesCCGuid ) )
            {
                ContentChannel cc = new ContentChannelService( rockContext ).Get( eventChangesCCGuid );
                EventChangesContentChannelId = cc.Id;
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventDetailsChangesContentChannel ), out eventDetailsChangesCCGuid ) )
            {
                ContentChannel dCC = new ContentChannelService( rockContext ).Get( eventDetailsChangesCCGuid );
                EventDetailsChangesContentChannelId = dCC.Id;
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.EventCommentsContentChannel ), out eventCommentsCCGuid ) )
            {
                ContentChannel cCC = new ContentChannelService( rockContext ).Get( eventCommentsCCGuid );
                EventCommentsContentChannelId = cCC.Id;
            }
        }

        private void StatusChangeNotification( ContentChannelItem item, string status )
        {
            RockContext context = new RockContext();
            Person p = GetCurrentPerson();
            GlobalAttributesCache attributesCache = GlobalAttributesCache.Get();
            string url;
            string baseUrl = attributesCache.GetValue( "InternalApplicationRoot" );
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.AdminDashboard, queryParams );
            string subject = p.FullName + " Has Changed the Status of " + item.Title;
            string message = "<p>This request has been marked: " + status + ".</p><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
            var header = attributesCache.GetValue( "EmailHeader" );
            var footer = attributesCache.GetValue( "EmailFooter" );
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

        private void CommentNotification( ContentChannelItem comment, ContentChannelItem item )
        {
            RockContext context = new RockContext();
            Person p = GetCurrentPerson();
            string url;
            var attributeCache = GlobalAttributesCache.Get();
            string baseUrl = attributeCache.GetValue( "InternalApplicationRoot" );
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.AdminDashboard, queryParams );
            string subject = p.FullName + " Has Added a Comment to " + item.Title;
            string message = "<p>This comment has been added to " + p.FullName + "'s request:</p>" +
                "<blockquote>" + comment.Content + "</blockquote><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
            var header = attributeCache.GetValue( "EmailHeader" ); //Email Header
            var footer = attributeCache.GetValue( "EmailFooter" ); //Email Footer 
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

        private List<Person> GetAdminUsers()
        {
            List<Person> users = new List<Person>();
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

        #endregion Helpers

        public class DashboardViewModel
        {
            public List<ContentChannelItemBag> events { get; set; }
            public List<ContentChannelItemAssociation> eventDetails { get; set; }
            public bool isEventAdmin { get; set; }
            public bool isRoomAdmin { get; set; }
            public bool isSuperUser { get; set; }
            public List<DefinedValue> locations { get; set; }
            public List<DefinedValue> ministries { get; set; }
            public List<DefinedValue> budgetLines { get; set; }
            public List<DefinedValue> drinks { get; set; }
            public List<DefinedValue> inventory { get; set; }
            public AttributeBag requestStatus { get; set; }
            public AttributeBag requestType { get; set; }
            public string workflowURL { get; set; }
            public List<string> defaultStatuses { get; set; }
            public int eventDetailsCCId { get; set; }
            public int commentsCCId { get; set; }
        }

        public class GetRequestResponse
        {
            public ContentChannelItemBag request { get; set; }
            public ContentChannelItemBag requestPendingChanges { get; set; }
            public List<Comment> comments { get; set; }
            public List<Details> details { get; set; }
            public PersonBag createdBy { get; set; }
            public PersonBag modifiedBy { get; set; }
        }

        public class Comment
        {
            public ContentChannelItemBag comment { get; set; }
            public string createdBy { get; set; }
        }

        public class Details
        {
            public ContentChannelItemBag detail { get; set; }
            public ContentChannelItemBag detailPendingChanges { get; set; }
        }

        public class Filters
        {
            public string title { get; set; }
            public string ministry { get; set; }
            public List<string> statuses { get; set; }
            public List<string> resources { get; set; }
            public DateRangeParts eventDates { get; set; }
            public DateRangeParts eventModified { get; set; }
            public string submitter { get; set; }
        }
        public class Submitter
        {
            public string value { get; set; }
            public string text { get; set; }
        }
        public class DateRangeParts
        {
            public string lowerValue { get; set; }
            public string upperValue { get; set; }
        }
        public class DuplicateDates
        {
            public string originalDate { get; set; }
            public string newDate { get; set; }
        }
        public class RequestGridView
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public DateTime SubmittedOn { get; set; }
            public DateTime ModifiedDateTime { get; set; }
            public int CreatedByPersonAliasId { get; set; }
            public string CreatedBy { get; set; }
            public int ModifiedByPersonAliasId { get; set; }
            public string ModifiedBy { get; set; }
            public string Ministry { get; set; }
            public string Status { get; set; }
            public string Resources { get; set; }
            public string EventDates { get; set; }
            public string IsValid { get; set; }
            public int? UnreadComments { get; set; }
        }
        //private class RequestAuthorization
        //{
        //    public int RequestId { get; set; }
        //    public bool CanEdit { get; set; }
        //    public bool CanView { get; set; }
        //}
    }
}
