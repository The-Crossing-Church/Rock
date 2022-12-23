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

namespace Rock.Blocks.Plugin.EventDashboard
{
    /// <summary>
    /// Registration Entry.
    /// </summary>
    /// <seealso cref="Rock.Blocks.RockObsidianBlockType" />

    [DisplayName( "Admin Dashboard" )]
    [Category( "Obsidian > Plugin > Event Form" )]
    [Description( "Obsidian Event Admin Dashboard" )]
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
    [LinkedPage( "User Dashboard", key: AttributeKey.UserDashboard, category: "Pages", required: true, order: 2 )]
    [SecurityRoleField( "Event Request Admin", key: AttributeKey.EventAdminRole, category: "Security", required: true, order: 0 )]
    [SecurityRoleField( "Room Request Admin", key: AttributeKey.RoomAdminRole, category: "Security", required: true, order: 1 )]
    [TextField( "Default Statuses", key: AttributeKey.DefaultStatuses, category: "Filters", defaultValue: "Submitted,In Progress,Pending Changes,Proposed Changes Denied,Changes Accepted by User", required: true, order: 0 )]
    [TextField( "Request Status Attribute Key", key: AttributeKey.RequestStatusAttrKey, category: "Filters", defaultValue: "RequestStatus", required: true, order: 1 )]
    [TextField( "Requested Resources Attribute Key", key: AttributeKey.RequestedResourcesAttrKey, category: "Filters", defaultValue: "RequestType", required: true, order: 2 )]
    [TextField( "Event Dates Attribute Key", key: AttributeKey.EventDatesAttrKey, category: "Filters", defaultValue: "EventDates", required: true, order: 3 )]
    [TextField( "Ministry Attribute Key", key: AttributeKey.MinistryAttrKey, category: "Filters", defaultValue: "Ministry", required: true, order: 4 )]
    [TextField( "Shared With Attribute Key", key: AttributeKey.SharedWithAttrKey, category: "Sharing", defaultValue: "SharedWith", required: true, order: 0 )]
    [GroupTypeField( "Shared Event Group Type", "Group Type of groups that allow for seeing shared requests", false, "", "Sharing", 1, AttributeKey.SharingGroupType )]
    [WorkflowTypeField( "Request Action Worfklow", "Workflow to update request status", true, key: AttributeKey.RequestActionWorkflow, category: "Workflow" )]
    [WorkflowTypeField( "User Action Worfklow", "Workflow to update request status", true, key: AttributeKey.UserActionWorkflow, category: "Workflow" )]
    [TextField( "Details are Same Key", "Attribute Key for Is Same", key: AttributeKey.IsSameAttrKey, defaultValue: "IsSame", category: "Attributes", order: 1 )]
    [TextField( "Details Event Date", "Attribute Key for Event Date on Details", key: AttributeKey.DetailsEventDate, defaultValue: "EventDate", category: "Attributes", order: 3 )]
    [TextField( "Start Time Key", "Attribute Key for Start Time", key: AttributeKey.StartDateTime, defaultValue: "StartTime", category: "Attributes", order: 4 )]
    [TextField( "End Time Key", "Attribute Key for End Time", key: AttributeKey.EndDateTime, defaultValue: "EndTime", category: "Attributes", order: 5 )]
    [TextField( "Room", "Attribute Key for Room", key: AttributeKey.Rooms, defaultValue: "Rooms", category: "Attributes", order: 6 )]
    [TextField( "Start Buffer", "Attribute Key for Start Buffer", key: AttributeKey.StartBuffer, defaultValue: "StartBuffer", category: "Attributes", order: 7 )]
    [TextField( "End Buffer", "Attribute Key for End Buffer", key: AttributeKey.EndBuffer, defaultValue: "EndBuffer", category: "Attributes", order: 8 )]
    #endregion Block Attributes

    public class AdminDashboard : RockObsidianBlockType
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
            public const string SharedWithAttrKey = "SharedWithAttrKey";
            public const string RequestActionWorkflow = "RequestActionWorkflow";
            public const string UserActionWorkflow = "UserActionWorkflow";
            public const string SharingGroupType = "SharingGroupType";
            public const string IsSameAttrKey = "IsSameAttrKey";
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
            var rockContext = new RockContext();
            Guid eventDatesAttrGuid = Guid.Empty;
            Guid requestStatusAttrGuid = Guid.Empty;
            Guid isSameAttrGuid = Guid.Empty;
            DashboardViewModel viewModel = null;

            SetProperties();
            if ( EventContentChannelId > 0 && EventDetailsContentChannelId > 0 && EventChangesContentChannelId > 0 && EventDetailsChangesContentChannelId > 0 )
            {
                viewModel = LoadRequests();
                viewModel.eventDetailsCCId = EventDetailsContentChannelId;
                viewModel.commentsCCId = EventCommentsContentChannelId;
                viewModel.submittedEvents = LoadByStatus( new Filters() { statuses = new List<string>() { "Submitted" } } );
                viewModel.changedEvents = LoadByStatus( new Filters() { statuses = new List<string>() { "Pending Changes", "Changes Accepted by User", "Cancelled by User" } } );
                viewModel.inprogressEvents = LoadByStatus( new Filters() { statuses = new List<string>() { "In Progress" } } );
                viewModel.isEventAdmin = CheckSecurityRole( rockContext, AttributeKey.EventAdminRole );
                viewModel.isRoomAdmin = CheckSecurityRole( rockContext, AttributeKey.RoomAdminRole );

                var ids = viewModel.submittedEvents.Select( e => e.CreatedByPersonAliasId ).ToList();
                ids.AddRange( viewModel.changedEvents.Select( e => e.CreatedByPersonAliasId ) );
                ids.AddRange( viewModel.inprogressEvents.Select( e => e.CreatedByPersonAliasId ) );
                ids.AddRange( viewModel.events.Select( e => e.CreatedByPersonAliasId ) );
                ids = ids.Distinct().ToList();
                viewModel.users = new PersonAliasService( rockContext ).Queryable().Where( pa => ids.Contains( pa.Id ) ).Select( pa => pa.Person ).ToList(); ;

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
                    Dictionary<string, string> queryParams = new Dictionary<string, string>();
                    queryParams.Add( "WorkflowTypeId", wf.Id.ToString() );
                    viewModel.workflowURL = this.GetLinkedPageUrl( AttributeKey.WorkflowEntryPage, queryParams );
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
        private int EventChangesContentChannelId { get; set; }
        private int EventDetailsChangesContentChannelId { get; set; }
        private int EventCommentsContentChannelId { get; set; }

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult PartialApproval( int id, List<string> approved, List<string> denied, List<EventPartialApproval> events )
        {
            try
            {
                SetProperties();
                var p = GetCurrentPerson();
                RockContext rockContext = new RockContext();
                ContentChannelItemService cci_svc = new ContentChannelItemService( rockContext );
                ContentChannelItemAssociationService ccia_svc = new ContentChannelItemAssociationService( rockContext );
                ContentChannelItem item = cci_svc.Get( id );
                ContentChannelItemAssociation changesAssoc = item.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventChangesContentChannelId );
                ContentChannelItem changes = item.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventChangesContentChannelId ).ChildContentChannelItem;
                List<ContentChannelItem> details = item.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( ci => ci.ChildContentChannelItem ).ToList();

                item.LoadAttributes();
                changes.LoadAttributes();

                //Update that our admin has made a change to this request
                item.ModifiedByPersonAliasId = p.PrimaryAliasId;
                item.ModifiedDateTime = RockDateTime.Now;

                //Request Approved Changes
                for ( var i = 0; i < approved.Count(); i++ )
                {
                    if ( approved[i] == "Title" )
                    {
                        item.Title = changes.Title.Substring( 0, changes.Title.Length - 8 );
                    }
                    else
                    {
                        item.SetAttributeValue( approved[i], changes.GetAttributeValue( approved[i] ) );
                    }
                }
                item.SetAttributeValue( "RequestStatus", "Approved" );
                List<String> resources = new List<string>();
                if ( item.GetAttributeValue( "NeedsSpace" ) == "True" )
                {
                    resources.Add( "Room" );
                }
                if ( item.GetAttributeValue( "NeedsCatering" ) == "True" )
                {
                    resources.Add( "Catering" );
                }
                if ( item.GetAttributeValue( "NeedsOpsAccommodations" ) == "True" )
                {
                    resources.Add( "Extra Resources" );
                }
                if ( item.GetAttributeValue( "NeedsChildCare" ) == "True" )
                {
                    resources.Add( "Childcare" );
                }
                if ( item.GetAttributeValue( "NeedsChildCareCatering" ) == "True" )
                {
                    resources.Add( "Childcare Catering" );
                }
                if ( item.GetAttributeValue( "NeedsRegistration" ) == "True" )
                {
                    resources.Add( "Registration" );
                }
                if ( item.GetAttributeValue( "NeedsWebCalendar" ) == "True" )
                {
                    resources.Add( "Web Calendar" );
                }
                if ( item.GetAttributeValue( "NeedsPublicity" ) == "True" )
                {
                    resources.Add( "Publicity" );
                }
                if ( item.GetAttributeValue( "NeedsProductionAccommodations" ) == "True" )
                {
                    resources.Add( "Production" );
                }
                if ( item.GetAttributeValue( "NeedsOnline" ) == "True" )
                {
                    resources.Add( "Online Event" );
                }
                item.SetAttributeValue( "RequestType", String.Join( ",", resources ) );
                item.SaveAttributeValues();
                cci_svc.Delete( changes );
                ccia_svc.Delete( changesAssoc );

                //Event Details Approved Changes
                for ( var i = 0; i < details.Count(); i++ )
                {
                    var eventChanges = events.FirstOrDefault( e => e.eventid == details[i].Id );
                    var detailChangesAssoc = details[i].ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId );
                    var detailChanges = detailChangesAssoc.ChildContentChannelItem;
                    if ( eventChanges != null )
                    {
                        details.LoadAttributes();
                        detailChanges.LoadAttributes();
                        for ( var k = 0; k < eventChanges.approvedAttrs.Count(); k++ )
                        {
                            details[i].SetAttributeValue( eventChanges.approvedAttrs[k], detailChanges.GetAttributeValue( eventChanges.approvedAttrs[k] ) );
                        }
                        details[i].SaveAttributeValues();
                    }
                    cci_svc.Delete( detailChanges );
                    ccia_svc.Delete( detailChangesAssoc );
                }
                rockContext.SaveChanges();

                //Notifications
                PartialApprovalNotifications( item, approved, denied, events );


                return ActionOk( new { id = id } );
            }
            catch ( Exception e )
            {
                return ActionBadRequest( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult GetRequestDetails( int id )
        {
            GetRequestResponse response = new GetRequestResponse();
            RockContext context = new RockContext();
            SetProperties();
            var item = new ContentChannelItemService( context ).Get( id );
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

            //Get Conflicts
            response.conflicts = GetConflicts( item, details );
            if ( requestchanges != null )
            {
                response.changesConflicts = GetConflicts( requestchanges.ChildContentChannelItem, details.Select( cci => cci.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId ).ChildContentChannelItem ).ToList(), item.Id );
            }

            return ActionOk( response );
        }

        [BlockAction]
        public BlockActionResult FilterRequests( string opt, Filters filters = null )
        {
            SetProperties();
            if ( filters == null )
            {
                filters = new Filters();
            }
            DashboardViewModel viewModel = new DashboardViewModel();
            if ( opt == "Submitted" )
            {
                filters.statuses = new List<string>() { "Submitted" };
                viewModel.submittedEvents = LoadByStatus( filters );
            }
            else if ( opt == "PendingChanges" )
            {
                filters.statuses = new List<string>() { "Pending Changes", "Changes Accepted by User", "Cancelled by User" };
                viewModel.changedEvents = LoadByStatus( filters );
            }
            else if ( opt == "InProgress" )
            {
                filters.statuses = new List<string>() { "In Progress" };
                viewModel.inprogressEvents = LoadByStatus( filters );
            }
            else
            {
                viewModel = LoadRequests( filters );
            }
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
                string url = "";
                var cci_svc = new ContentChannelItemService( rockContext );
                var ccia_svc = new ContentChannelItemAssociationService( rockContext );
                ContentChannelItem item = cci_svc.Get( id );
                item.LoadAttributes();
                string currentStatus = item.GetAttributeValue( requestStatusAttrKey );
                item.ModifiedByPersonAliasId = p.PrimaryAliasId;
                item.ModifiedDateTime = RockDateTime.Now;
                if ( currentStatus == "Pending Changes" )
                {
                    if ( status == "Approved" || status == "In Progress" )
                    {
                        //From Pending to Approved, update all attribute values and delete the pending changes items 
                        var changesAssoc = item.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventChangesContentChannelId );
                        if ( changesAssoc != null )
                        {
                            var changes = changesAssoc.ChildContentChannelItem;
                            changes.LoadAttributes();
                            item.Title = changes.Title.Substring( 0, changes.Title.Length - 8 );
                            foreach ( var av in item.AttributeValues )
                            {
                                item.SetAttributeValue( av.Key, changes.AttributeValues[av.Key].Value );
                            }
                            item.SaveAttributeValues();
                            cci_svc.Delete( changes );
                            ccia_svc.Delete( changesAssoc );
                            var events = item.ChildItems.Where( ci => ci.ChildContentChannelItem != null && ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).ToList();
                            for ( int i = 0; i < events.Count(); i++ )
                            {
                                events[i].ChildContentChannelItem.LoadAttributes();
                                var eventChanges = events[i].ChildContentChannelItem.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId );
                                eventChanges.ChildContentChannelItem.LoadAttributes();
                                foreach ( var av in events[i].ChildContentChannelItem.AttributeValues )
                                {
                                    events[i].ChildContentChannelItem.SetAttributeValue( av.Key, eventChanges.ChildContentChannelItem.AttributeValues[av.Key].Value );
                                }
                                events[i].ChildContentChannelItem.SaveAttributeValues();
                                cci_svc.Delete( eventChanges.ChildContentChannelItem );
                                ccia_svc.Delete( eventChanges );
                            }
                        }
                    }
                }
                if ( status == "Proposed Changes Denied" )
                {
                    //Remove Pending Changes items because they were not approved
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
                            cci_svc.Delete( eventChanges.ChildContentChannelItem );
                            ccia_svc.Delete( eventChanges );
                        }
                    }
                }
                if ( status == "Denied" || ( status == "Proposed Changes Denied" && denyWithComments ) || status == "Approved" )
                {
                    url = LaunchWorkflow( item.Id, status );
                }
                else
                {
                    StatusChangeNotification( item, status );
                }
                rockContext.SaveChanges();
                item.SetAttributeValue( requestStatusAttrKey, status );
                item.SaveAttributeValue( requestStatusAttrKey );
                return ActionOk( new { status = item.GetAttributeValue( requestStatusAttrKey ), url = url } );
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

                CommentNotification( comment, request );
                return ActionOk( new { createdBy = p.FullName, comment = comment } );
            }
            catch ( Exception e )
            {
                return ActionBadRequest( e.Message );
            }
        }

        [BlockAction]
        public BlockActionResult AddBuffer( List<BufferData> data )
        {
            try
            {
                ContentChannelItemService cci_svc = new ContentChannelItemService( new RockContext() );
                for ( int i = 0; i < data.Count(); i++ )
                {
                    ContentChannelItem item = cci_svc.Get( data[i].id );
                    item.LoadAttributes();
                    item.SetAttributeValue( "StartBuffer", data[i].start );
                    item.SetAttributeValue( "EndBuffer", data[i].end );
                    item.SaveAttributeValues();
                }
                return ActionOk( new { success = true } );
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

            items = new ContentChannelItemService( new RockContext() ).Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId );//.ToList();
            items.First().LoadAttributes();

            IEnumerable<ContentChannelItem> filtered_items = null;
            string requestStatusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
            var requestStatusAttr = items.First().Attributes[requestStatusAttrKey];
            string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
            var requestResourcesAttr = items.First().Attributes[resourcesAttrKey];
            string eventDatesAttrKey = GetAttributeValue( AttributeKey.EventDatesAttrKey );
            var eventDatesAttr = items.First().Attributes[eventDatesAttrKey];
            string ministryAttrKey = GetAttributeValue( AttributeKey.MinistryAttrKey );
            var ministryAttr = items.First().Attributes[ministryAttrKey];

            if ( filters.eventModified != null && ( !String.IsNullOrEmpty( filters.eventModified.lowerValue ) || !String.IsNullOrEmpty( filters.eventModified.upperValue ) ) )
            {
                if ( !String.IsNullOrEmpty( filters.eventModified.lowerValue ) && !String.IsNullOrEmpty( filters.eventModified.upperValue ) )
                {
                    filtered_items = items.Where( i => i.ModifiedDateTime >= DateTime.Parse( filters.eventModified.lowerValue ) && i.ModifiedDateTime <= DateTime.Parse( filters.eventModified.upperValue ).EndOfDay() );
                }
                else
                {
                    if ( !String.IsNullOrEmpty( filters.eventModified.lowerValue ) )
                    {
                        filtered_items = items.Where( i => i.ModifiedDateTime >= DateTime.Parse( filters.eventModified.lowerValue ) );
                    }
                    if ( !String.IsNullOrEmpty( filters.eventModified.upperValue ) )
                    {
                        filtered_items = items.Where( i => i.ModifiedDateTime <= DateTime.Parse( filters.eventModified.upperValue ).EndOfDay() );
                    }
                }
                //Don't include drafts in the recently modified
                var requestStatuses = av_svc.Queryable().Where( av => av.AttributeId == requestStatusAttr.Id && av.Value != "Draft" );
                filtered_items = filtered_items.Join( requestStatuses,
                        i => i.Id,
                        av => av.EntityId,
                        ( i, av ) => i
                    );
            }
            if ( filtered_items == null )
            {
                filtered_items = items;
            }
            Person submitter = null;
            if ( filters.submitter != null && !String.IsNullOrEmpty( filters.submitter.value ) )
            {
                submitter = new PersonService( context ).Get( Guid.Parse( filters.submitter.value ) );
            }
            filtered_items = filtered_items.Where( i =>
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
                filters.statuses = filters.statuses.Select( s => s.Trim() ).ToList();
                var requestStatuses = av_svc.Queryable().Where( av => av.AttributeId == requestStatusAttr.Id && filters.statuses.Contains( av.Value ) );
                filtered_items = filtered_items.Join( requestStatuses,
                        i => i.Id,
                        av => av.EntityId,
                        ( i, av ) => i
                    );
            }
            if ( !String.IsNullOrEmpty( filters.ministry ) )
            {
                var ministries = av_svc.Queryable().Where( av => av.AttributeId == ministryAttr.Id && av.Value.ToLower() == filters.ministry.ToLower() );
                filtered_items = filtered_items.Join( ministries,
                        i => i.Id,
                        av => av.EntityId,
                        ( i, av ) => i
                    );
            }

            itemList = filtered_items.ToList();

            //Make sure desired item is in list
            if ( id.HasValue )
            {
                var exists = items.FirstOrDefault( i => i.Id == id.Value );
                if ( exists == null )
                {
                    item = new ContentChannelItemService( context ).Get( id.Value );
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
            viewModel.events = itemList.OrderByDescending( i => i.ModifiedDateTime ).Select( i => i.ToViewModel( p, true ) ).ToList();
            viewModel.events = viewModel.events.Select( e =>
            {
                var i = itemList.FirstOrDefault( il => il.Id == e.Id );
                var comments = i.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventCommentsContentChannelId ).OrderByDescending( ci => ci.ChildContentChannelItem.CreatedDateTime ).ToList();
                int ct = 0;
                for ( var k = 0; k < comments.Count(); k++ )
                {
                    if ( comments[k].ChildContentChannelItem.CreatedByPersonAliasId != p.PrimaryAlias.Id )
                    {
                        ct++;
                    }
                    else
                    {
                        break;
                    }
                }
                e.AttributeValues.Add( "CommentNotifications", ct.ToString() );
                return e;
            } ).ToList();
            return viewModel;
        }

        private List<ContentChannelItemViewModel> LoadByStatus( Filters filters )
        {
            int? id = PageParameter( PageParameterKey.RequestId ).AsIntegerOrNull();
            ContentChannelItem item = new ContentChannelItem();
            List<ContentChannelItem> itemList = new List<ContentChannelItem>();
            IEnumerable<ContentChannelItem> items = null;
            RockContext context = new RockContext();
            AttributeValueService av_svc = new AttributeValueService( context );
            var p = GetCurrentPerson();

            items = new ContentChannelItemService( new RockContext() ).Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId );//.ToList();
            items.First().LoadAttributes();

            IEnumerable<ContentChannelItem> filtered_items = null;
            string requestStatusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
            var requestStatusAttr = items.First().Attributes[requestStatusAttrKey];
            string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
            var requestResourcesAttr = items.First().Attributes[resourcesAttrKey];
            string eventDatesAttrKey = GetAttributeValue( AttributeKey.EventDatesAttrKey );
            var eventDatesAttr = items.First().Attributes[eventDatesAttrKey];
            string ministryAttrKey = GetAttributeValue( AttributeKey.MinistryAttrKey );
            var ministryAttr = items.First().Attributes[ministryAttrKey];

            var requestStatuses = av_svc.Queryable().Where( av => av.AttributeId == requestStatusAttr.Id && filters.statuses.Contains( av.Value ) );
            filtered_items = items.Join( requestStatuses,
                    i => i.Id,
                    av => av.EntityId,
                    ( i, av ) => i
                );

            Person submitter = null;
            if ( filters.submitter != null && !String.IsNullOrEmpty( filters.submitter.value ) )
            {
                submitter = new PersonService( context ).Get( Guid.Parse( filters.submitter.value ) );
            }
            filtered_items = filtered_items.Where( i =>
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

            if ( !String.IsNullOrEmpty( filters.ministry ) )
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

            var results = filtered_items.OrderByDescending( i => i.ModifiedDateTime ).Select( i => i.ToViewModel( p, true ) ).ToList();
            results = results.Select( e =>
            {
                var i = filtered_items.FirstOrDefault( il => il.Id == e.Id );
                var comments = i.ChildItems.Where( ci => ci.ChildContentChannelItem.ContentChannelId == EventCommentsContentChannelId ).OrderByDescending( ci => ci.ChildContentChannelItem.CreatedDateTime ).ToList();
                int ct = 0;
                for ( var k = 0; k < comments.Count(); k++ )
                {
                    if ( comments[k].ChildContentChannelItem.CreatedByPersonAliasId != p.PrimaryAlias.Id )
                    {
                        ct++;
                    }
                    else
                    {
                        break;
                    }
                }
                e.AttributeValues.Add( "CommentNotifications", ct.ToString() );
                return e;
            } ).ToList();
            return results;
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

        private string LaunchWorkflow( int id, string action )
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            Guid workflowGuid = Guid.Empty;
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.RequestActionWorkflow ), out workflowGuid ) )
            {
                WorkflowType wt = new WorkflowTypeService( new RockContext() ).Get( workflowGuid );
                queryParams.Add( "WorkflowTypeId", wt.Id.ToString() );
                queryParams.Add( "Id", id.ToString() );
                queryParams.Add( "Action", action );
                return this.GetLinkedPageUrl( AttributeKey.WorkflowEntryPage, queryParams );
            }
            else
            {
                throw new Exception( "Unable to locate workflow type" );
            }
        }

        private List<Person> GetRequestUsers( ContentChannelItem item )
        {
            List<Person> users = new List<Person>();
            RockContext context = new RockContext();
            PersonService p_svc = new PersonService( context );
            string sharedWithAttrKey = GetAttributeValue( AttributeKey.SharedWithAttrKey );
            var sharedWithAttr = item.GetAttributeValue( sharedWithAttrKey );
            Guid? sharedRequestGroupTypeGuid = GetAttributeValue( AttributeKey.SharingGroupType ).AsGuidOrNull();
            if ( sharedRequestGroupTypeGuid.HasValue )
            {
                //Shared requests are configured, find any for the current user.
                var SharedRequestGT = new GroupTypeService( context ).Get( sharedRequestGroupTypeGuid.Value );
                var groups = new GroupService( context ).Queryable().Where( g => g.GroupTypeId == SharedRequestGT.Id );
                var groupMembers = new GroupMemberService( context ).Queryable().Where( gm => gm.PersonId == item.CreatedByPersonId && gm.GroupRole.Name == "Request Creator" );
                groups = from g in groups
                         join gm in groupMembers on g.Id equals gm.GroupId
                         select g;
                var grpList = groups.ToList();
                for ( int k = 0; k < grpList.Count(); k++ )
                {
                    users.AddRange( grpList[k].Members.Where( gm => gm.GroupRole.Name == "Can View" ).Select( gm => gm.Person ) );
                }
            }
            List<int?> sharedRequests = new List<int?>();
            if ( !String.IsNullOrEmpty( sharedWithAttr ) )
            {
                List<int> ids = sharedWithAttr.Split( ',' ).Select( i => Int32.Parse( i ) ).ToList();
                for ( int k = 0; k < ids.Count(); k++ )
                {
                    users.Add( p_svc.Get( ids[k] ) );
                }
            }
            users.Add( item.CreatedByPersonAlias.Person );
            users = users.Distinct().ToList();
            return users;
        }

        private List<ContentChannelItemViewModel> GetConflicts( ContentChannelItem item, List<ContentChannelItem> events, int originalId = 0 )
        {
            item.LoadAttributes();
            events.LoadAttributes();
            List<ContentChannelItem> conflicts = new List<ContentChannelItem>();
            RockContext context = new RockContext();
            AttributeValueService avSvc = new AttributeValueService( context );
            ContentChannelItemService cciSvc = new ContentChannelItemService( context );
            List<ConflictData> eventDates = new List<ConflictData>();
            string isSameKey = GetAttributeValue( AttributeKey.IsSameAttrKey );
            string statusKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
            string eventDatesKey = GetAttributeValue( AttributeKey.EventDatesAttrKey );
            string eventDateKey = GetAttributeValue( AttributeKey.DetailsEventDate );
            string startKey = GetAttributeValue( AttributeKey.StartDateTime );
            string endKey = GetAttributeValue( AttributeKey.EndDateTime );
            string startBufferKey = GetAttributeValue( AttributeKey.StartBuffer );
            string endBufferKey = GetAttributeValue( AttributeKey.EndBuffer );
            string roomKey = GetAttributeValue( AttributeKey.Rooms );
            for ( int k = 0; k < events.Count(); k++ )
            {
                List<string> dates = item.GetAttributeValue( eventDatesKey ).Split( ',' ).ToList();
                if ( !String.IsNullOrEmpty( events[k].GetAttributeValue( eventDateKey ) ) )
                {
                    dates = new List<string>() { events[k].GetAttributeValue( eventDateKey ) };
                }
                for ( int i = 0; i < dates.Count(); i++ )
                {
                    ConflictData d = new ConflictData() { range = new DateRange() };
                    d.range.Start = DateTime.Parse( $"{dates[i]} {events[k].GetAttributeValue( startKey )}" );
                    d.range.End = DateTime.Parse( $"{dates[i]} {events[k].GetAttributeValue( endKey )}" );
                    var startBuffer = events[0].GetAttributeValue( startBufferKey );
                    if ( !String.IsNullOrEmpty( startBuffer ) )
                    {
                        int buffer = Int32.Parse( startBuffer );
                        d.range.Start.Value.AddMinutes( buffer * -1 );
                    }
                    var endBuffer = events[0].GetAttributeValue( endBufferKey );
                    if ( !String.IsNullOrEmpty( endBuffer ) )
                    {
                        int buffer = Int32.Parse( endBuffer );
                        d.range.Start.Value.AddMinutes( buffer );
                    }
                    d.rooms = events[k].GetAttributeValue( roomKey );
                    //Only care about events we have rooms, dates, and times for
                    if ( !String.IsNullOrEmpty( events[k].GetAttributeValue( startKey ) ) && !String.IsNullOrEmpty( events[k].GetAttributeValue( endKey ) ) && !String.IsNullOrEmpty( events[k].GetAttributeValue( roomKey ) ) )
                    {
                        eventDates.Add( d );
                    }
                }
            }
            for ( int i = 0; i < eventDates.Count(); i++ )
            {
                string dateCompareVal = eventDates[i].range.Start.Value.ToString( "yyyy-MM-dd" );
                var items = cciSvc.Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId && cci.Id != item.Id && cci.Id != originalId ).OrderBy( cci => cci.Title );
                //Requests that are on the calendar
                var statusAttr = item.Attributes[statusKey];
                var statusValues = avSvc.Queryable().Where( av => av.AttributeId == statusAttr.Id && ( av.Value != "Draft" && av.Value != "Submitted" && av.Value != "Denied" && !av.Value.Contains( "Cancelled" ) ) );
                items = items.Join( statusValues,
                    itm => itm.Id,
                    av => av.EntityId,
                    ( itm, av ) => itm
                ).OrderBy( cci => cci.Title );
                var eventDetails = items.Select( itm => itm.ChildItems.FirstOrDefault( ccia => ccia.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).ChildContentChannelItem );
                //Filter items to isSame, others will be in the eventDetails list
                var isSameAttr = item.Attributes[isSameKey];
                var isSameValues = avSvc.Queryable().Where( av => av.AttributeId == isSameAttr.Id && av.Value == "True" );
                items = items.Join( isSameValues,
                    itm => itm.Id,
                    av => av.EntityId,
                    ( itm, av ) => itm
                ).OrderBy( cci => cci.Title );
                //Events on same date
                var eventAttr = events[0].Attributes[eventDateKey];
                var eventDateAttr = item.Attributes[eventDatesKey];
                var eventDateValues = avSvc.Queryable().Where( av => ( av.AttributeId == eventAttr.Id && av.Value == dateCompareVal ) || ( av.AttributeId == eventDateAttr.Id && av.Value.Contains( dateCompareVal ) ) );
                eventDetails = eventDetails.Join( eventDateValues,
                    itm => itm.Id,
                    av => av.EntityId,
                    ( itm, av ) => itm
                );
                items = items.Join( eventDateValues,
                    itm => itm.Id,
                    av => av.EntityId,
                    ( itm, av ) => itm
                ).OrderBy( cci => cci.Title );
                var ccItems = items.Select( itm => itm.ChildItems.FirstOrDefault( ccia => ccia.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).ChildContentChannelItem ).ToList();
                ccItems.AddRange( eventDetails );
                var roomAttr = events[0].Attributes[roomKey];
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
                    var startBuffer = itm.GetAttributeValue( startBufferKey );
                    var endBuffer = itm.GetAttributeValue( endBufferKey );
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
                conflicts.AddRange( ccItems );
            }
            return conflicts.Distinct().Select( c =>
            {
                var x = c.ToViewModel( null, true );
                if ( String.IsNullOrEmpty( c.GetAttributeValue( eventDateKey ) ) )
                {
                    var parent = c.ParentItems.FirstOrDefault();
                    if ( parent != null )
                    {
                        parent.ContentChannelItem.LoadAttributes();
                        x.AttributeValues.Add( eventDatesKey, parent.ContentChannelItem.GetAttributeValue( eventDatesKey ) );
                        x.AttributeValues.Add( "ParentId", parent.ContentChannelItem.Id.ToString() );
                    }
                }
                return x;
            } ).ToList();
        }

        private void CommentNotification( ContentChannelItem comment, ContentChannelItem item )
        {
            RockContext context = new RockContext();
            Person p = GetCurrentPerson();
            string url;
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.UserDashboard, queryParams );
            string baseUrl = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" );
            string subject = p.FullName + " Has Added a Comment to " + item.Title;
            string message = "<p>This comment has been added to your request:</p>" +
                "<blockquote>" + comment.Content + "</blockquote><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            var users = GetRequestUsers( item );
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

        private void StatusChangeNotification( ContentChannelItem item, string status )
        {
            RockContext context = new RockContext();
            Person p = GetCurrentPerson();
            string url;
            string baseUrl = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" );
            Dictionary<string, string> udqueryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.UserDashboard, udqueryParams );
            string subject = p.FullName + " Has Changed the Status of " + item.Title;
            string message = "<p>Your request has been marked: " + status + ".</p><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
            if ( status == "Proposed Changes Denied" )
            {
                Guid? workflowGuid = GetAttributeValue( AttributeKey.UserActionWorkflow ).AsGuidOrNull();
                if ( workflowGuid.HasValue )
                {
                    WorkflowType wf = new WorkflowTypeService( context ).Get( workflowGuid.Value );
                    Dictionary<string, string> queryParams = new Dictionary<string, string>();
                    queryParams.Add( "WorkflowTypeId", wf.Id.ToString() );
                    url = this.GetLinkedPageUrl( AttributeKey.WorkflowEntryPage, queryParams );
                }
                subject = "Proposed Changes for " + item.Title + " have been Denied";
                message = "<p>We regret to inform you the changes you have requested to your event request have been denied.</p> <br/>" +
                "<p>Please select one of the following options for your request. You can...</p>" +
                "<ul>" +
                    "<li> Continue with the originally approved request </li>" +
                    "<li> Cancel your request entirely </li>" +
                "</ul>" +
                "<table>" +
                    "<tr>" +
                        "<td style='tect-align: center;'>" +
                            "<a href='" + baseUrl + url + item.Id + "&Action=Original' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Use Original</a>" +
                        "</td>" +
                        "<td style='tect-align: center;'>" +
                            "<a href='" + baseUrl + url + item.Id + "&Action=Cancelled' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Cancel Request </a>" +
                        "</td>" +
                    "</tr>" +
                "</table>";
            }
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            var users = GetRequestUsers( item );
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

        private void PartialApprovalNotifications( ContentChannelItem item, List<string> approved, List<string> denied, List<EventPartialApproval> events )
        {
            RockContext context = new RockContext();
            Person p = GetCurrentPerson();
            string url;
            string baseUrl = GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" );
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            url = this.GetLinkedPageUrl( AttributeKey.UserDashboard, queryParams );
            item.LoadAttributes();
            string subject = "Some of Your Changes Have Been Approved";
            string message = "Please see below which modifications have been approved or denied:<br/>";
            message += "<strong>Approved Modifications</strong><br/>";
            message += "<ul>";
            for ( int i = 0; i < approved.Count(); i++ )
            {
                if ( approved[i] != "Title" )
                {
                    message += "<li>" + item.Attributes[approved[i]].Name + "</li>";
                }
            }
            if ( events.Count() == 1 )
            {
                var e = item.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId );
                if ( e != null )
                {
                    for ( int i = 0; i < events[0].approvedAttrs.Count(); i++ )
                    {
                        message += "<li>" + e.ChildContentChannelItem.Attributes[events[0].approvedAttrs[i]].Name + "</li>";
                    }
                }
            }
            message += "</ul>";
            message += "<strong>Denied Modifications</strong><br/>";
            message += "<ul>";
            for ( int i = 0; i < denied.Count(); i++ )
            {
                if ( denied[i] != "Title" )
                {
                    message += "<li>" + item.Attributes[denied[i]].Name + "</li>";
                }
            }
            if ( events.Count() == 1 )
            {
                var e = item.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId );
                if ( e != null )
                {
                    for ( int i = 0; i < events[0].deniedAttrs.Count(); i++ )
                    {
                        message += "<li>" + e.ChildContentChannelItem.Attributes[events[0].deniedAttrs[i]].Name + "</li>";
                    }
                }
            }
            message += "</ul>";
            if ( events.Count() > 1 )
            {
                for ( int i = 0; i < events.Count(); i++ )
                {
                    var e = item.ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItemId == events[i].eventid );
                    if ( e != null )
                    {
                        e.ChildContentChannelItem.LoadAttributes();
                        message += "<strong>Approved Modifications for " + DateTime.Parse( e.GetAttributeValue( "EventDate" ) ).ToString( "MM/dd/yyyy" ) + "</strong><br/>";
                        message += "<ul>";
                        for ( int k = 0; i < events[i].approvedAttrs.Count(); k++ )
                        {
                            message += "<li>" + e.ChildContentChannelItem.Attributes[events[i].approvedAttrs[k]].Name + "</li>";
                        }
                        message += "</ul>";
                        message += "<strong>Denied Modifications for " + DateTime.Parse( e.GetAttributeValue( "EventDate" ) ).ToString( "MM/dd/yyyy" ) + "</strong><br/>";
                        message += "<ul>";
                        for ( int k = 0; i < events[i].deniedAttrs.Count(); k++ )
                        {
                            message += "<li>" + e.ChildContentChannelItem.Attributes[events[i].deniedAttrs[k]].Name + "</li>";
                        }
                        message += "</ul>";
                    }
                }
            }
            message +=
                "<table style='width: 100%;'>" +
                    "<tr>" +
                        "<td></td>" +
                        "<td style='text-align:center;'>" +
                            "<a href='" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>View Updated Event</a>" +
                        "</td>" +
                        "<td style='text-align:center;'>" +
                            "<a href='" + baseUrl + url.Substring( 1 ) + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Continue Modifying</a>" +
                        "</td>" +
                        "<td></td>" +
                    "</tr>" +
                "</table>";
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            var users = GetRequestUsers( item );
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

        #endregion Helpers

        public class DashboardViewModel
        {
            public List<ContentChannelItemViewModel> events { get; set; }
            public List<ContentChannelItemViewModel> submittedEvents { get; set; }
            public List<ContentChannelItemViewModel> changedEvents { get; set; }
            public List<ContentChannelItemViewModel> inprogressEvents { get; set; }
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
            public List<Person> users { get; set; }
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
            public List<ContentChannelItemViewModel> conflicts { get; set; }
            public List<ContentChannelItemViewModel> changesConflicts { get; set; }
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

        public class EventPartialApproval
        {
            public int eventid { get; set; }
            public List<string> approvedAttrs { get; set; }
            public List<string> deniedAttrs { get; set; }
        }

        public class ConflictData
        {
            public DateRange range { get; set; }
            public string rooms { get; set; }
        }

        public class BufferData
        {
            public int id { get; set; }
            public string start { get; set; }
            public string end { get; set; }
        }
    }
}
