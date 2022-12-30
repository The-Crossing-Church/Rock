using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
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

namespace Rock.Blocks.Plugin.EventDashboard
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "User Dashboard" )]
    [Category( "Obsidian > Plugin > Event Form" )]
    [Description( "Obsidian Event User Dashboard" )]
    [IconCssClass( "fa fa-calendar-check" )]

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
    [LinkedPage( "Event Submission Form", key: AttributeKey.SubmissionPage, category: "Pages", required: true, order: 0 )]
    [LinkedPage( "Workflow Entry Page", key: AttributeKey.WorkflowEntryPage, category: "Pages", required: true, order: 1 )]
    [LinkedPage( "Event Admin Dashboard", key: AttributeKey.AdminDashboard, category: "Pages", required: true, order: 2 )]
    [SecurityRoleField( "Event Request Admin", key: AttributeKey.EventAdminRole, category: "Security", required: true, order: 0 )]
    [SecurityRoleField( "Room Request Admin", key: AttributeKey.RoomAdminRole, category: "Security", required: true, order: 1 )]
    [TextField( "Default Statuses", key: AttributeKey.DefaultStatuses, category: "Filters", defaultValue: "Draft,Submitted,In Progress,Pending Changes,Proposed Changes Denied", required: true, order: 0 )]
    [TextField( "Request Status Attribute Key", key: AttributeKey.RequestStatusAttrKey, category: "Filters", defaultValue: "RequestStatus", required: true, order: 1 )]
    [TextField( "Requested Resources Attribute Key", key: AttributeKey.RequestedResourcesAttrKey, category: "Filters", defaultValue: "RequestType", required: true, order: 2 )]
    [TextField( "Event Dates Attribute Key", key: AttributeKey.EventDatesAttrKey, category: "Filters", defaultValue: "EventDates", required: true, order: 3 )]
    [TextField( "Ministry Attribute Key", key: AttributeKey.MinistryAttrKey, category: "Filters", defaultValue: "Ministry", required: true, order: 4 )]
    [WorkflowTypeField( "Request Action Worfklow", "Workflow to update request status", true, key: AttributeKey.RequestActionWorkflow, category: "Workflow" )]
    [GroupTypeField( "Shared Event Group Type", "Group Type of groups that allow for seeing shared requests", false, "", "Sharing", 1, AttributeKey.SharingGroupType )]
    [SecurityRoleField( "Staff Group", "The role of people you can share requests with", false, "", "Sharing", 2, AttributeKey.StaffGroup )]
    [TextField( "Shared With Attribut Key", category: "Sharing", order: 3, key: AttributeKey.SharedWithAttr )]
    #endregion Block Attributes

    public class UserDashboard : RockObsidianBlockType
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
            public const string SubmissionPage = "SubmissionPage";
            public const string WorkflowEntryPage = "WorkflowEntryPage";
            public const string AdminDashboard = "AdminDashboard";
            public const string UserDashboard = "UserDashboard";
            public const string EventAdminRole = "EventAdminRole";
            public const string RoomAdminRole = "RoomAdminRole";
            public const string DefaultStatuses = "DefaultStatuses";
            public const string RequestStatusAttrKey = "RequestStatusAttrKey";
            public const string RequestedResourcesAttrKey = "RequestedResourcesAttrKey";
            public const string EventDatesAttrKey = "EventDatesAttrKey";
            public const string MinistryAttrKey = "MinistryAttrKey";
            public const string RequestActionWorkflow = "RequestActionWorkflow";
            public const string SharingGroupType = "SharingGroupType";
            public const string StaffGroup = "StaffGroup";
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
            using ( var rockContext = new RockContext() )
            {
                Guid eventDatesAttrGuid = Guid.Empty;
                Guid requestStatusAttrGuid = Guid.Empty;
                Guid isSameAttrGuid = Guid.Empty;
                DashboardViewModel viewModel = null;

                SetProperties();
                if ( EventContentChannelId > 0 && EventDetailsContentChannelId > 0 && EventChangesContentChannelId > 0 && EventDetailsChangesContentChannelId > 0 )
                {
                    viewModel = LoadRequests();
                    viewModel.isEventAdmin = CheckSecurityRole( rockContext, AttributeKey.EventAdminRole );
                    viewModel.isRoomAdmin = CheckSecurityRole( rockContext, AttributeKey.RoomAdminRole );
                    viewModel.eventDetailsCCId = EventDetailsContentChannelId;
                    viewModel.commentsCCId = EventCommentsContentChannelId;

                    //Lists
                    Guid locationGuid = Guid.Empty;
                    Guid ministryGuid = Guid.Empty;
                    Guid budgetLineGuid = Guid.Empty;
                    Guid drinksGuid = Guid.Empty;
                    var p = GetCurrentPerson();
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.LocationList ), out locationGuid ) )
                    {
                        DefinedType locationDT = new DefinedTypeService( rockContext ).Get( locationGuid );
                        var locs = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == locationDT.Id ).ToList().Select( l => l.ToViewModel( p, true ) );
                        viewModel.locations = locs.ToList();
                    }
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryGuid ) )
                    {
                        DefinedType ministryDT = new DefinedTypeService( rockContext ).Get( ministryGuid );
                        var min = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == ministryDT.Id );
                        min.LoadAttributes();
                        viewModel.ministries = min.ToList();
                    }
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.BudgetList ), out budgetLineGuid ) )
                    {
                        DefinedType budgetDT = new DefinedTypeService( rockContext ).Get( budgetLineGuid );
                        var budget = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == budgetDT.Id );
                        budget.LoadAttributes();
                        viewModel.budgetLines = budget.ToList();
                    }
                    if ( Guid.TryParse( GetAttributeValue( AttributeKey.DrinksList ), out drinksGuid ) )
                    {
                        DefinedType drinkDT = new DefinedTypeService( rockContext ).Get( drinksGuid );
                        var drinks = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == drinkDT.Id );
                        drinks.LoadAttributes();
                        viewModel.drinks = drinks.ToList();
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

                    List<string> defaultStatuses = GetAttributeValue( AttributeKey.DefaultStatuses ).Split( ',' ).ToList();
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
        public BlockActionResult GetRequestDetails( int id )
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
            response.request = item.ToViewModel( null, true );
            var requestchanges = item.ChildItems.Where( i => i.ChildContentChannelItem.ContentChannelId == EventChangesContentChannelId ).FirstOrDefault();
            if ( requestchanges != null )
            {
                response.requestPendingChanges = requestchanges.ChildContentChannelItem.ToViewModel( null, true );
            }
            var details = item.ChildItems.Where( i => i.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( i => i.ChildContentChannelItem ).ToList();
            response.details = details.Select( i => new Details() { detail = i.ToViewModel( null, true ) } ).ToList();
            for ( int i = 0; i < details.Count(); i++ )
            {
                var detailChanges = details[i].ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId );
                if ( detailChanges != null )
                {
                    response.details[i].detailPendingChanges = detailChanges.ChildContentChannelItem.ToViewModel( null, true );
                }
            }
            response.comments = item.ChildItems.Where( i => i.ChildContentChannelItem.ContentChannelId == EventCommentsContentChannelId ).Select( ci => new Comment { comment = ci.ChildContentChannelItem.ToViewModel( null, true ), createdBy = ci.ChildContentChannelItem.CreatedByPersonName } ).ToList();
            response.createdBy = item.CreatedByPersonAlias.Person.ToViewModel( null, false );
            response.modifiedBy = item.ModifiedByPersonAlias.Person.ToViewModel( null, false );
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
            DashboardViewModel viewModel = LoadRequests( filters );
            return ActionOk( viewModel );
        }

        [BlockAction]
        public BlockActionResult ChangeStatus( int id, string status, bool denyWithComments = false )
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
                StatusChangeNotification( item, status );
                return ActionOk( new { status = item.GetAttributeValue( requestStatusAttrKey ) } );
            }
            catch ( Exception e )
            {
                return ActionBadRequest( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult AddComment( int id, string message )
        {
            try
            {
                RockContext rockContext = new RockContext();
                SetProperties();
                Person p = GetCurrentPerson();
                ContentChannelItemService cci_svc = new ContentChannelItemService( rockContext );
                ContentChannel commentChannel = new ContentChannelService( rockContext ).Get( EventCommentsContentChannelId );
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
                ContentChannelItem request = cci_svc.Get( id );
                request.ModifiedDateTime = RockDateTime.Now;

                //Add association between comment and request
                var assocSvc = new ContentChannelItemAssociationService( rockContext );
                var order = assocSvc.Queryable().AsNoTracking()
                    .Where( a => a.ContentChannelItemId == id )
                    .Select( a => ( int? ) a.Order )
                    .DefaultIfEmpty()
                    .Max();
                var assoc = new ContentChannelItemAssociation();
                assoc.ContentChannelItemId = id;
                assoc.ChildContentChannelItemId = comment.Id;
                assoc.Order = order.HasValue ? order.Value + 1 : 0;
                assocSvc.Add( assoc );

                rockContext.SaveChanges();

                CommentNotification( request, comment );
                return ActionOk( new { createdBy = p.FullName, comment = comment } );
            }
            catch ( Exception e )
            {
                return ActionBadRequest( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult DuplicateEvent( int? id, string eventDates, List<string> removedResources, List<DuplicateDates> copyDates )
        {
            try
            {
                RockContext context = new RockContext();
                SetProperties();
                var cciSvc = new ContentChannelItemService( context );
                if ( id.HasValue )
                {
                    var p = GetCurrentPerson();
                    ContentChannelItem original = cciSvc.Get( id.Value );
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
                        var attrs = originalChildren[0].Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Registration" ) );
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
                        var attrs = item.Attributes.Where( a => a.Value.Categories.Select( cc => cc.Name ).Contains( "Event Publicity" ) );
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
                return ActionBadRequest( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult ProposedChangesAction( int id, string action )
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
                return ActionBadRequest( e.Message );
            }
        }
        #endregion Block Actions

        #region Helpers
        /// <summary>
        /// Loads the requests
        /// </summary>
        /// <returns></returns>
        private DashboardViewModel LoadRequests( Filters filters = null )
        {
            DashboardViewModel viewModel = new DashboardViewModel();
            int? id = PageParameter( PageParameterKey.RequestId ).AsIntegerOrNull();
            ContentChannelItem item = new ContentChannelItem();
            List<ContentChannelItem> itemList = new List<ContentChannelItem>();
            IEnumerable<ContentChannelItem> items = null;
            RockContext context = new RockContext();
            AttributeValueService av_svc = new AttributeValueService( context );
            var p = GetCurrentPerson();

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
            string sharedWithAttrKey = GetAttributeValue( AttributeKey.SharedWithAttr );
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
            List<int?> sharedRequests = new List<int?>();
            if ( sharedWithAttr != null )
            {
                sharedRequests = new AttributeValueService( context ).Queryable().Where( av => av.AttributeId == sharedWithAttr.Id ).ToList().Where( av => av.Value.Split( ',' ).Contains( p.Id.ToString() ) ).Select( av => av.EntityId ).ToList();
            }
            Guid ministryGuid = Guid.Empty;
            List<int?> personalRequests = new List<int?>();
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryGuid ) )
            {
                DefinedType ministryDT = new DefinedTypeService( context ).Get( ministryGuid );
                var min = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == ministryDT.Id );
                var personalRequest = min.FirstOrDefault( dv => dv.Value.ToLower().Contains( "personal" ) );
                if ( personalRequest != null )
                {
                    //filter out personal requests from the shared requests list
                    personalRequests = new AttributeValueService( context ).Queryable().Where( av => av.AttributeId == ministryAttr.Id ).ToList().Where( av => av.Value == personalRequest.Guid.ToString() ).Select( av => av.EntityId ).ToList();
                }
            }
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
                if ( !personalRequests.Contains( i.Id ) )
                {
                    if ( aliasIds.Contains( i.CreatedByPersonAliasId ) )
                    {
                        return true;
                    }
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
            Person submitter = null;
            if ( filters.submitter != null && !String.IsNullOrEmpty( filters.submitter.value ) )
            {
                submitter = new PersonService( context ).Get( Guid.Parse( filters.submitter.value ) );
            }
            //AND Filters
            filtered_items = items.Where( i =>
            {
                bool meetsCriteria = true;
                if ( submitter != null )
                {
                    if ( i.CreatedByPersonAliasId != submitter.PrimaryAlias.Id && i.ModifiedByPersonAliasId != submitter.PrimaryAlias.Id )
                    {
                        meetsCriteria = false;
                    }
                }
                if ( !String.IsNullOrEmpty( filters.title ) )
                {
                    if ( !i.Title.ToLower().Contains( filters.title.ToLower() ) )
                    {
                        meetsCriteria = false;
                    }
                }

                return meetsCriteria;
            } );
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
                    var eventDates = av_svc.Queryable().Where( av => av.AttributeId == eventDatesAttr.Id ).ToList().Where( av =>
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
            if ( filters.resources != null && filters.resources.Count() > 0 )
            {
                var requestedResources = av_svc.Queryable().Where( av => av.AttributeId == requestResourcesAttr.Id ).ToList().Where( av =>
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
                var requestStatuses = av_svc.Queryable().Where( av => av.AttributeId == requestStatusAttr.Id && filters.statuses.Contains( av.Value ) );
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
            viewModel.events = itemList.OrderByDescending( i => i.ModifiedDateTime ).Select( i => i.ToViewModel( p, true ) ).ToList();
            viewModel.eventDetails = itemList.SelectMany( i => i.ChildItems ).ToList();
            return viewModel;
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
            string url;
            string baseUrl = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" );
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.AdminDashboard, queryParams );
            string subject = p.FullName + " Has Changed the Status of " + item.Title;
            string message = "<p>This request has been marked: " + status + ".</p><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
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
            string baseUrl = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" );
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.AdminDashboard, queryParams );
            string subject = p.FullName + " Has Added a Comment to " + item.Title;
            string message = "<p>This comment has been added to " + p.FullName + "'s request:</p>" +
                "<blockquote>" + comment.Content + "</blockquote><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
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
            public List<ContentChannelItemViewModel> events { get; set; }
            public List<ContentChannelItemAssociation> eventDetails { get; set; }
            public bool isEventAdmin { get; set; }
            public bool isRoomAdmin { get; set; }
            public List<DefinedValueViewModel> locations { get; set; }
            public List<Rock.Model.DefinedValue> ministries { get; set; }
            public List<Rock.Model.DefinedValue> budgetLines { get; set; }
            public List<Rock.Model.DefinedValue> drinks { get; set; }
            public AttributeViewModel requestStatus { get; set; }
            public AttributeViewModel requestType { get; set; }
            public string workflowURL { get; set; }
            public List<string> defaultStatuses { get; set; }
            public int eventDetailsCCId { get; set; }
            public int commentsCCId { get; set; }
        }

        public class GetRequestResponse
        {
            public ContentChannelItemViewModel request { get; set; }
            public ContentChannelItemViewModel requestPendingChanges { get; set; }
            public List<Comment> comments { get; set; }
            public List<Details> details { get; set; }
            public PersonViewModel createdBy { get; set; }
            public PersonViewModel modifiedBy { get; set; }
        }

        public class Comment
        {
            public ContentChannelItemViewModel comment { get; set; }
            public string createdBy { get; set; }
        }

        public class Details
        {
            public ContentChannelItemViewModel detail { get; set; }
            public ContentChannelItemViewModel detailPendingChanges { get; set; }
        }

        public class Filters
        {
            public string title { get; set; }
            public string ministry { get; set; }
            public List<string> statuses { get; set; }
            public List<string> resources { get; set; }
            public DateRangeParts eventDates { get; set; }
            public DateRangeParts eventModified { get; set; }
            public Submitter submitter { get; set; }
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
    }
}
