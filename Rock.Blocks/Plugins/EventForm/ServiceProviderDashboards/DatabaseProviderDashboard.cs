using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Attribute;
using Rock.Blocks.Plugins.ViewModels;
using Rock.Data;
using Rock.Model;
using Rock.Blocks.Plugins.EventForm;

namespace Rock.Blocks.Plugins.EventDashboard.ServiceProviderDashboards
{

    [DisplayName( "Database Provider Dashboard" )]
    [Category( "Obsidian > Plugin > Event Form" )]
    [Description( "Registration and Check-in Provider Dashboard" )]
    [IconCssClass( "fa fa-calendar-check" )]
    [SupportedSiteTypes( SiteType.Web )]

    #region Block Attributes
    [DefinedTypeField( "Locations Defined Type", key: AttributeKey.LocationList, category: "Lists", required: true, order: 0 )]
    [DefinedTypeField( "Ministries Defined Type", key: AttributeKey.MinistryList, category: "Lists", required: true, order: 1 )]
    [DefinedTypeField( "Budgets Defined Type", key: AttributeKey.BudgetList, category: "Lists", required: true, order: 2 )]
    [DefinedTypeField( "Drinks Defined Type", key: AttributeKey.DrinksList, category: "Lists", required: true, order: 3 )]
    [DefinedTypeField( "Ops Inventory Defined Type", key: AttributeKey.InventoryList, category: "Lists", required: true, order: 4 )]
    #endregion

    public class DatabaseProviderDashboard : ServiceProviderDashboard
    {
        #region Keys
        /// <summary>
        /// Attribute Key
        /// </summary>
        protected partial class AttributeKey : AttributeKeyBase
        {
            public const string BudgetList = "BudgetList";
            public const string DrinksList = "DrinksList";
            public const string InventoryList = "InventoryList";
        }
        #endregion

        #region Properties
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
            RockContext rockContext = new RockContext();

            ProviderViewModel baseViewModel = ( ProviderViewModel ) base.GetObsidianBlockInitialization();
            DatabaseProviderViewModel viewModel = new DatabaseProviderViewModel( baseViewModel );

            if ( EventContentChannelId > 0 && EventDetailsContentChannelId > 0 && EventChangesContentChannelId > 0 && EventDetailsChangesContentChannelId > 0 )
            {
                //Lists
                Guid budgetLineGuid = Guid.Empty;
                Guid drinksGuid = Guid.Empty;
                Guid inventoryGuid = Guid.Empty;
                var p = GetCurrentPerson();
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
                //viewModel.pending = LoadRequestsByStatus( SSPDashboardOption.PendingConfirmation );
                //viewModel.cancelled = LoadRequestsByStatus( SSPDashboardOption.Cancelled );
                viewModel.ministryAttr = ministryAttr;
            }
            return viewModel;
        }

        #endregion Obsidian Block Type Overrides

        #region Block Actions
        [BlockAction]
        public BlockActionResult FilterRequests( string opt, Filters filters )
        {
            try
            {
                if ( opt == "Pending" )
                {
                    return ActionOk( LoadRequestsByStatus( SSPDashboardOption.PendingConfirmation ) );
                }
                else if ( opt == "Cancelled" )
                {
                    return ActionOk( LoadRequestsByStatus( SSPDashboardOption.Cancelled ) );
                }
                else
                {
                    return ActionOk( LoadRequests( filters ) );
                }
            }
            catch ( Exception ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }
        #endregion

        #region Helpers
        private List<ContentChannelItemBag> LoadRequests( Filters filters )
        {
            List<string> statuses = null;
            List<string> resources = null;
            if ( filters.statuses.Any() )
            {
                statuses = filters.statuses;
            }
            if ( filters.resources.Any() )
            {
                resources = filters.resources;
            }
            return base.LoadRequests( null, statuses, SSPDashboardOption.All, filters.eventDates, filters.eventModified, resources, filters.submitter, filters.ministry, filters.title );
        }

        private List<ContentChannelItemBag> LoadRequestsByStatus( SSPDashboardOption option )
        {
            List<string> statuses = null;
            if ( option == SSPDashboardOption.Cancelled )
            {
                statuses = new List<string>() { "Denied", "Cancelled", "Cancelled by User" };
                option = SSPDashboardOption.Confirmed;
            }
            else
            {
                statuses = new List<string>() { "Approved", "In Progress", "Pending Changes", "Proposed Changes Denied", "Changes Accepted by User" };
            }
            DateRangeParts eventDateRange = new DateRangeParts() { lowerValue = RockDateTime.Now.ToISO8601DateString() };
            return base.LoadRequests( null, statuses, option, eventDateRange, null, null, null, null, null );
        }
        #endregion

        #region Classes
        public partial class DatabaseProviderViewModel : ProviderViewModel
        {
            public DatabaseProviderViewModel() { }
            public DatabaseProviderViewModel( ProviderViewModel baseViewModal )
            {
                this.events = baseViewModal.events;
                this.pending = baseViewModal.pending;
                this.ministryAttr = baseViewModal.ministryAttr;
                this.locations = baseViewModal.locations;
                this.ministries = baseViewModal.ministries;
                this.editCategories = baseViewModal.editCategories;
                this.viewCategories = baseViewModal.viewCategories;
            }

            public List<DefinedValue> drinks { get; set; }
            public List<DefinedValue> inventory { get; set; }
        }
        #endregion
    }
}