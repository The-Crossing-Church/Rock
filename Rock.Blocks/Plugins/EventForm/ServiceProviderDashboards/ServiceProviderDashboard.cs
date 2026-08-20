using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;

using Rock.Attribute;
using Rock.Blocks.Plugins.ViewModels;
using Rock.Blocks.Plugins.EventForm;
using Rock.Data;
using Rock.Model;

namespace Rock.Blocks.Plugins.EventDashboard.ServiceProviderDashboards
{
    #region Block Attributes
    [ContentChannelField( "Event Content Channel", key: AttributeKey.EventContentChannel, category: "General", required: true, order: 0 )]
    [ContentChannelField( "Event Details Content Channel", key: AttributeKey.EventDetailsContentChannel, category: "General", required: true, order: 1 )]
    [ContentChannelField( "Event Changes Content Channel", key: AttributeKey.EventChangesContentChannel, category: "General", required: true, order: 2 )]
    [ContentChannelField( "Event Details Changes Content Channel", key: AttributeKey.EventDetailsChangesContentChannel, category: "General", required: true, order: 3 )]
    [ContentChannelField( "Event Comments Content Channel", key: AttributeKey.EventCommentsContentChannel, category: "General", required: true, order: 4 )]
    #endregion
    public abstract class ServiceProviderDashboard : RockBlockType
    {
        #region Keys
        /// <summary>
        /// Attribute Key
        /// </summary>
        protected static class AttributeKey
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
        }

        /// <summary>
        /// Page Parameter
        /// </summary>
        private static class PageParameterKey
        {
            public const string RequestId = "Id";
        }
        #endregion

        #region Properties
        protected ObsidianPluginsShared PluginHelper = new ObsidianPluginsShared();
        protected EventFormShared EventFormHelper = new EventFormShared();
        protected int EventContentChannelId { get; set; }
        protected int EventContentChannelTypeId { get; set; }
        protected int EventDetailsContentChannelId { get; set; }
        protected int EventDetailsContentChannelTypeId { get; set; }
        protected int EventChangesContentChannelId { get; set; }
        protected int EventDetailsChangesContentChannelId { get; set; }
        protected int EventCommentsContentChannelId { get; set; }
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
            SetProperties();

            viewModel.CCId = EventContentChannelId;
            if ( EventContentChannelId > 0 && EventDetailsContentChannelId > 0 && EventChangesContentChannelId > 0 && EventDetailsChangesContentChannelId > 0 )
            {
                ////Lists
                //Guid locationGuid = Guid.Empty;
                //Guid ministryGuid = Guid.Empty;
                //Guid budgetLineGuid = Guid.Empty;
                //Guid drinksGuid = Guid.Empty;
                //Guid inventoryGuid = Guid.Empty;
                //var p = GetCurrentPerson();
                //if ( Guid.TryParse( GetAttributeValue( AttributeKey.LocationList ), out locationGuid ) )
                //{
                //    DefinedType locationDT = new DefinedTypeService( rockContext ).Get( locationGuid );
                //    var locs = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == locationDT.Id ).ToList();
                //    locs.LoadAttributes();
                //    viewModel.locations = locs;
                //}
                //if ( Guid.TryParse( GetAttributeValue( AttributeKey.MinistryList ), out ministryGuid ) )
                //{
                //    DefinedType ministryDT = new DefinedTypeService( rockContext ).Get( ministryGuid );
                //    var min = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == ministryDT.Id ).ToList();
                //    min.LoadAttributes();
                //    viewModel.ministries = min.ToList();
                //}
                //if ( Guid.TryParse( GetAttributeValue( AttributeKey.BudgetList ), out budgetLineGuid ) )
                //{
                //    DefinedType budgetDT = new DefinedTypeService( rockContext ).Get( budgetLineGuid );
                //    var budget = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == budgetDT.Id ).ToList();
                //    budget.LoadAttributes();
                //    viewModel.budgetLines = budget.ToList();
                //}
                //if ( Guid.TryParse( GetAttributeValue( AttributeKey.DrinksList ), out drinksGuid ) )
                //{
                //    DefinedType drinkDT = new DefinedTypeService( rockContext ).Get( drinksGuid );
                //    var drinks = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == drinkDT.Id ).ToList();
                //    drinks.LoadAttributes();
                //    viewModel.drinks = drinks.ToList();
                //}
                //if ( Guid.TryParse( GetAttributeValue( AttributeKey.InventoryList ), out inventoryGuid ) )
                //{
                //    DefinedType invDT = new DefinedTypeService( rockContext ).Get( inventoryGuid );
                //    var inventory = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == invDT.Id ).ToList();
                //    inventory.LoadAttributes();
                //    viewModel.inventory = inventory.ToList();
                //}
            }
            return viewModel;
        }

        #endregion Obsidian Block Type Overrides

        #region Block Actions
        /// <summary>
        /// Load the details of the specific request
        /// </summary>
        /// <param name="id">The request to load</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult GetRequestDetails( string id )
        {
            try
            {
                return ActionOk();
            }
            catch ( Exception ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Edit the details of the specific request
        /// </summary>
        /// <param name="id">The request to modify</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult EditRequest( string id )
        {
            try
            {
                return ActionOk();
            }
            catch ( Exception ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }

        /// <summary>
        /// Approve or Deny a service on a request
        /// </summary>
        /// <param name="id">The request</param>
        /// <param name="status">The status of their service request</param>
        /// <param name="section">The service</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult ChangeSectionStatus( string id, string status, string section )
        {
            try
            {
                return ActionOk();
            }
            catch ( Exception ex )
            {
                return ActionBadRequest( ex.Message );
            }
        }
        #endregion

        #region Helpers
        private void LoadRequests()
        {

        }

        protected void SetProperties()
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
        #endregion

        #region Private Classes
        public class ProviderViewModel
        {
            public int CCId { get; set; }
            public List<ContentChannelItemBag> events { get; set; }
            public List<DefinedValue> locations { get; set; }
            public List<DefinedValue> ministries { get; set; }
            public List<DefinedValue> budgetLines { get; set; }
            public List<DefinedValue> drinks { get; set; }
            public List<DefinedValue> inventory { get; set; }
            public AttributeBag requestStatus { get; set; }
            public AttributeBag requestType { get; set; }
        }
        #endregion
    }
}
