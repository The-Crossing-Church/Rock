using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;

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
            ProviderViewModel viewModel = new ProviderViewModel();
            RockContext rockContext = new RockContext();

            viewModel = ( ProviderViewModel ) base.GetObsidianBlockInitialization();

            if ( EventContentChannelId > 0 && EventDetailsContentChannelId > 0 && EventChangesContentChannelId > 0 && EventDetailsChangesContentChannelId > 0 )
            {
                var x = 7;
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
            }
            return viewModel;
        }

        #endregion Obsidian Block Type Overrides

        #region Block Actions
        #endregion

        #region Helpers
        private void LoadRequests()
        {

        }
        #endregion
    }
}