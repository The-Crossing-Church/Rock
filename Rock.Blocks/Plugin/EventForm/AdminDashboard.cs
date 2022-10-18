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
    [DefinedTypeField( "Locations Defined Type", key: AttributeKey.LocationList, category: "Lists", required: true, order: 0 )]
    [DefinedTypeField( "Ministries Defined Type", key: AttributeKey.MinistryList, category: "Lists", required: true, order: 1 )]
    [DefinedTypeField( "Budgets Defined Type", key: AttributeKey.BudgetList, category: "Lists", required: true, order: 2 )]
    [DefinedTypeField( "Drinks Defined Type", key: AttributeKey.DrinksList, category: "Lists", required: true, order: 3 )]
    [LinkedPage( "Event Submission Form", key: AttributeKey.SubmissionPage, category: "Pages", required: true, order: 0 )]
    [SecurityRoleField( "Event Request Admin", key: AttributeKey.EventAdminRole, category: "Security", required: true, order: 0 )]
    [SecurityRoleField( "Room Request Admin", key: AttributeKey.RoomAdminRole, category: "Security", required: true, order: 1 )]
    [TextField( "Default Statuses", key: AttributeKey.DefaultStatuses, category: "Filters", defaultValue: "Submitted,Pending Changes,Proposed Changes Denied,Changes Accepted by User,Cancelled by User", required: true, order: 0 )]
    [TextField( "Request Status Attribute Key", key: AttributeKey.RequestStatusAttrKey, category: "Filters", defaultValue: "RequestStatus", required: true, order: 1 )]
    [TextField( "Requested Resources Attribute Key", key: AttributeKey.RequestedResourcesAttrKey, category: "Filters", defaultValue: "RequestType", required: true, order: 2 )]
    [TextField( "Event Dates Attribute Key", key: AttributeKey.EventDatesAttrKey, category: "Filters", defaultValue: "EventDates", required: true, order: 3 )]
    [WorkflowTypeField( "Request Action Worfklow", "Workflow to update request status", true, key: AttributeKey.RequestActionWorkflow, category: "Workflow" )]
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
            public const string EventDetailsContentChannel = "EventDetailsContentChannel";
            public const string LocationList = "LocationList";
            public const string MinistryList = "MinistryList";
            public const string BudgetList = "BudgetList";
            public const string MinistryBudgetList = "MinistryBudgetList";
            public const string DrinksList = "DrinksList";
            public const string SubmissionPage = "SubmissionPage";
            public const string AdminDashboard = "AdminDashboard";
            public const string UserDashboard = "UserDashboard";
            public const string EventAdminRole = "EventAdminRole";
            public const string RoomAdminRole = "RoomAdminRole";
            public const string DefaultStatuses = "DefaultStatuses";
            public const string RequestStatusAttrKey = "RequestStatusAttrKey";
            public const string RequestedResourcesAttrKey = "RequestedResourcesAttrKey";
            public const string EventDatesAttrKey = "EventDatesAttrKey";
            public const string RequestActionWorkflow = "RequestActionWorkflow";
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
                DashboardViewModel viewModel = null;
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

                        viewModel = LoadRequests();
                        viewModel.isEventAdmin = CheckSecurityRole( rockContext, AttributeKey.EventAdminRole );
                        viewModel.isRoomAdmin = CheckSecurityRole( rockContext, AttributeKey.RoomAdminRole );

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
                            viewModel.requestStatus = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == cc.ContentChannelTypeId.ToString() && a.Key == requestStatusAttrKey ).ToViewModel();
                        }
                        string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
                        if ( !String.IsNullOrEmpty( resourcesAttrKey ) )
                        {
                            viewModel.requestType = new AttributeService( rockContext ).Queryable().First( a => a.EntityTypeId == 208 && a.EntityTypeQualifierColumn == "ContentChannelTypeId" && a.EntityTypeQualifierValue == cc.ContentChannelTypeId.ToString() && a.Key == resourcesAttrKey ).ToViewModel();
                        }
                    }
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

        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult Save()
        {
            try
            {
                return ActionOk( new { success = true } );
            }
            catch ( Exception e )
            {
                return ActionInternalServerError( e.Message );
            }
        }

        #endregion Block Actions

        [BlockAction]
        public BlockActionResult GetRequestDetails( int id )
        {
            GetRequestResponse response = new GetRequestResponse();
            RockContext context = new RockContext();
            var item = new ContentChannelItemService( context ).Get( id );
            response.request = item.ToViewModel( null, true );
            response.details = item.ChildItems.Select( i => i.ChildContentChannelItem.ToViewModel( null, true ) ).ToList();
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
        public BlockActionResult ChangeStatus( int id, string status )
        {
            try
            {
                ContentChannelItem item = new ContentChannelItemService( new RockContext() ).Get( id );
                item.LoadAttributes();
                item.SetAttributeValue( "RequestStatus", status );
                item.SaveAttributeValue( "RequestStatus" );
                return ActionOk( new { status = item.GetAttributeValue( "RequestStatus" ) } );
            }
            catch ( Exception e )
            {
                return ActionBadRequest( e.Message );
            }
        }

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
            IEnumerable<ContentChannelItem> items_modified_match = null;
            string requestStatusAttrKey = GetAttributeValue( AttributeKey.RequestStatusAttrKey );
            var requestStatusAttr = items.First().Attributes[requestStatusAttrKey];
            string resourcesAttrKey = GetAttributeValue( AttributeKey.RequestedResourcesAttrKey );
            var requestResourcesAttr = items.First().Attributes[resourcesAttrKey];
            string eventDatesAttrKey = GetAttributeValue( AttributeKey.EventDatesAttrKey );
            var eventDatesAttr = items.First().Attributes[eventDatesAttrKey];

            //OR Filter
            if ( filters.eventModified != null )
            {
                if ( !String.IsNullOrEmpty( filters.eventModified.lowerValue ) && !String.IsNullOrEmpty( filters.eventModified.upperValue ) )
                {
                    items_modified_match = items.Where( i => i.ModifiedDateTime >= DateTime.Parse( filters.eventModified.lowerValue ) && i.ModifiedDateTime <= DateTime.Parse( filters.eventModified.upperValue ) );
                }
                else
                {
                    if ( !String.IsNullOrEmpty( filters.eventModified.lowerValue ) )
                    {
                        items_modified_match = items.Where( i => i.ModifiedDateTime >= DateTime.Parse( filters.eventModified.lowerValue ) );
                    }
                    if ( !String.IsNullOrEmpty( filters.eventModified.upperValue ) )
                    {
                        items_modified_match = items.Where( i => i.ModifiedDateTime <= DateTime.Parse( filters.eventModified.upperValue ) );
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
            if ( filters.eventDates != null )
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
                    itemList.Add( item );
                }
            }
            viewModel.events = itemList.Select( i => i.ToViewModel( p, true ) ).ToList();
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
        }

        public class GetRequestResponse
        {
            public ContentChannelItemViewModel request { get; set; }
            public List<ContentChannelItemViewModel> details { get; set; }
            public PersonViewModel createdBy { get; set; }
            public PersonViewModel modifiedBy { get; set; }
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
    }
}
