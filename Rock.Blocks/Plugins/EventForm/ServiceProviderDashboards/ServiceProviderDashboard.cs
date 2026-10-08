using System;
using System.Collections.Generic;
using System.Linq;

using Rock.Attribute;
using Rock.Blocks.Plugins.EventForm;
using Rock.Blocks.Plugins.ViewModels;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Blocks.Plugins.EventDashboard.ServiceProviderDashboards
{
    #region Block Attributes
    [ContentChannelField( "Event Content Channel", key: AttributeKeyBase.EventContentChannel, category: "General", required: true, order: 0 )]
    [ContentChannelField( "Event Details Content Channel", key: AttributeKeyBase.EventDetailsContentChannel, category: "General", required: true, order: 1 )]
    [ContentChannelField( "Event Changes Content Channel", key: AttributeKeyBase.EventChangesContentChannel, category: "General", required: true, order: 2 )]
    [ContentChannelField( "Event Details Changes Content Channel", key: AttributeKeyBase.EventDetailsChangesContentChannel, category: "General", required: true, order: 3 )]
    [ContentChannelField( "Event Comments Content Channel", key: AttributeKeyBase.EventCommentsContentChannel, category: "General", required: true, order: 4 )]

    [SecurityRoleField( "Service Provider Role", key: AttributeKeyBase.ProviderRole, category: "Security", required: true, order: 0 )]
    [SecurityRoleField( "Event Request Admin", key: AttributeKeyBase.EventAdminRole, category: "Security", required: true, order: 1 )]
    [SecurityRoleField( "Room Request Admin", key: AttributeKeyBase.RoomAdminRole, category: "Security", required: true, order: 2 )]
    [CategoryField( "Editable Cateogires", key: AttributeKeyBase.EditCategories, allowMultiple: true, entityTypeName: "Rock.Model.Attribute", entityTypeQualifierColumn: "EntityTypeId", entityTypeQualifierValue: "208", category: "Security", required: true, order: 3 )]
    [CategoryField( "Read Only Cateogires", key: AttributeKeyBase.ViewCategories, allowMultiple: true, entityTypeName: "Rock.Model.Attribute", entityTypeQualifierColumn: "EntityTypeId", entityTypeQualifierValue: "208", category: "Security", required: true, order: 4 )]

    [GroupTypeField( "Shared Event Group Type", "Group Type of groups that allow for seeing shared requests", false, "", "Sharing", 0, AttributeKeyBase.SharingGroupType )]
    [TextField( "Shared With Attribut Key", category: "Sharing", order: 1, key: AttributeKeyBase.SharedWithAttrKey )]
    #endregion
    public abstract class ServiceProviderDashboard : RockBlockType
    {
        #region Keys
        /// <summary>
        /// Attribute Key
        /// </summary>
        protected partial class AttributeKeyBase
        {
            public const string EventContentChannel = "EventContentChannel";
            public const string EventChangesContentChannel = "EventChangesContentChannel";
            public const string EventDetailsContentChannel = "EventDetailsContentChannel";
            public const string EventDetailsChangesContentChannel = "EventDetailsChangesContentChannel";
            public const string EventCommentsContentChannel = "EventCommentsContentChannel";

            public const string LocationList = "LocationList";
            public const string MinistryList = "MinistryList";

            public const string ProviderRole = "ProviderRole";
            public const string EventAdminRole = "EventAdminRole";
            public const string RoomAdminRole = "RoomAdminRole";
            public const string EditCategories = "EditCategories";
            public const string ViewCategories = "ViewCategories";

            public const string SharingGroupType = "SharingGroupType";
            public const string SharedWithAttrKey = "SharedWithAttrKey";
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
        protected AttributeCache ministryAttr { get; set; }
        protected List<Category> EditCategories { get; set; }
        protected List<Category> ViewCategories { get; set; }
        protected List<AttributeCache> VisibleAttributes { get; set; }
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

            if ( EventContentChannelId > 0 && EventDetailsContentChannelId > 0 && EventChangesContentChannelId > 0 && EventDetailsChangesContentChannelId > 0 )
            {
                //Lists
                Guid locationGuid = Guid.Empty;
                Guid ministryGuid = Guid.Empty;
                var p = GetCurrentPerson();
                if ( Guid.TryParse( GetAttributeValue( AttributeKeyBase.LocationList ), out locationGuid ) )
                {
                    DefinedType locationDT = new DefinedTypeService( rockContext ).Get( locationGuid );
                    var locs = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == locationDT.Id ).ToList();
                    locs.LoadAttributes();
                    viewModel.locations = locs;
                }
                if ( Guid.TryParse( GetAttributeValue( AttributeKeyBase.MinistryList ), out ministryGuid ) )
                {
                    DefinedType ministryDT = new DefinedTypeService( rockContext ).Get( ministryGuid );
                    var min = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == ministryDT.Id ).ToList();
                    min.LoadAttributes();
                    viewModel.ministries = min.ToList();
                }
                viewModel.editCategories = EditCategories;
                viewModel.viewCategories = ViewCategories;
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

                response.request = PluginHelper.GetCommonContentChannelItemEntityBag( item );
                response.request.CreatedBy = item.CreatedByPersonName;
                response.request.ModifiedBy = item.ModifiedByPersonName;
                response.request.Id = item.Id;

                //Only Load Attributes Visible to the Provider
                Rock.Attribute.Helper.LoadAttributes( item, context, VisibleAttributes );
                response.request.LoadAttributesAndValuesForPublicView( item, RequestContext.CurrentPerson );

                // Get Permissions
                var auth = CheckRequestPermissions( item );
                if ( !auth.CanView )
                {
                    return ActionBadRequest( "You do not have permission to view this request." );
                }
                response.request.CanEdit = auth.CanEdit;

                var requestChanges = item.ChildItems.Where( i => i.ChildContentChannelItem.ContentChannelId == EventChangesContentChannelId ).FirstOrDefault();
                if ( requestChanges != null )
                {
                    response.requestChanges = PluginHelper.GetCommonContentChannelItemEntityBag( requestChanges.ChildContentChannelItem );
                    Rock.Attribute.Helper.LoadAttributes( requestChanges.ChildContentChannelItem, context, VisibleAttributes );
                    response.requestChanges.LoadAttributesAndValuesForPublicEdit( requestChanges.ChildContentChannelItem, RequestContext.CurrentPerson, false );
                }

                var details = item.ChildItems.Where( i => i.ChildContentChannelItem.ContentChannelId == EventDetailsContentChannelId ).Select( i => i.ChildContentChannelItem ).ToList();
                response.details = details.Select( i =>
                {
                    var detail = new Details() { detail = PluginHelper.GetCommonContentChannelItemEntityBag( i ) };
                    Rock.Attribute.Helper.LoadAttributes( i, context, VisibleAttributes );
                    detail.detail.LoadAttributesAndValuesForPublicEdit( i, RequestContext.CurrentPerson, false );
                    return detail;
                } ).ToList();
                for ( int i = 0; i < details.Count(); i++ )
                {
                    var detailChanges = details[i].ChildItems.FirstOrDefault( ci => ci.ChildContentChannelItem.ContentChannelId == EventDetailsChangesContentChannelId );
                    if ( detailChanges != null )
                    {
                        response.details[i].detailChanges = PluginHelper.GetCommonContentChannelItemEntityBag( detailChanges.ChildContentChannelItem );
                        Rock.Attribute.Helper.LoadAttributes( detailChanges, context, VisibleAttributes );
                        response.details[i].detailChanges.LoadAttributesAndValuesForPublicEdit( detailChanges.ChildContentChannelItem, RequestContext.CurrentPerson, false );
                    }
                }

                response.comments = item.ChildItems.Where( i => i.ChildContentChannelItem.ContentChannelId == EventCommentsContentChannelId ).Select( ci => new Comment { comment = PluginHelper.GetCommonContentChannelItemEntityBag( ci.ChildContentChannelItem ), createdBy = ci.ChildContentChannelItem.CreatedByPersonName } ).ToList();
                response.createdBy = PluginHelper.GetCommonPersonEntityBag( item.CreatedByPersonAlias.Person );
                response.modifiedBy = PluginHelper.GetCommonPersonEntityBag( item.ModifiedByPersonAlias.Person );

                return ActionOk( response );
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
        /// <summary>
        /// Load Events in the desired filters
        /// </summary>
        /// <param name="categories">Used to limit requests just to ones with a specific service being requested</param>
        /// <param name="statuses">Used to limit requests based on their overall status (i.e. no Drafts/Submitted)</param>
        /// <param name="lowerBound">The lower date bound for an event happening within this range</param>
        /// <param name="upperBound">The u[[er date bound for an event happening within this tange</param>
        /// <returns></returns>
        protected List<ContentChannelItemBag> LoadRequests( List<string> categories, List<string> statuses = null, SSPDashboardOption option = SSPDashboardOption.All, DateRangeParts eventDateRange = null, DateRangeParts eventModifiedRange = null, List<string> resources = null, string submitter = null, string ministry = null, string title = null )
        {
            SetProperties();

            // Limit requests only to ones that are requesting the service of relevance to the provider
            if ( categories == null || categories.Count == 0 )
            {
                categories = EditCategories.Select( c => c.Name ).ToList();
            }
            else
            {
                //Make sure they aren't requesting a category not relevant to them
                categories = categories.Where( c => EditCategories.Select( cat => cat.Name ).Contains( c ) ).ToList();
            }
            var map = SectionMappings.Where( sm => categories.Contains( sm.category ) && sm.attr != null ).ToList();
            var serviceAttrKeys = map.Select( sm => sm.attr ).ToList();
            var serviceStatusAttrKeys = map.Select( sm => sm.status ).ToList();

            if ( statuses == null || statuses.Count == 0 )
            {
                statuses = new List<string>() { "Approved", "In Progress", "Pending Changes", "Proposed Changes Denied", "Changes Accepted by User", "Cancelled by User" };
            }

            RockContext context = new RockContext();
            AttributeValueService av_svc = new AttributeValueService( context );
            var p = GetCurrentPerson();

            IEnumerable<ContentChannelItem> items = null;
            items = new ContentChannelItemService( context ).Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId );
            ContentChannelItem item = items.First();
            item.LoadAttributes();
            AttributeCache eventDatesAttr = item.Attributes.FirstOrDefault( attr => attr.Key == "EventDates" ).Value;
            AttributeCache requestStatusAttr = item.Attributes.FirstOrDefault( attr => attr.Key == "RequestStatus" ).Value;
            //AttributeCache ministryAttr = item.Attributes.FirstOrDefault( attr => attr.Key == "Ministry" ).Value;
            AttributeCache requestTypeAttr = item.Attributes.FirstOrDefault( attr => attr.Key == "RequestType" ).Value;
            List<AttributeCache> serviceAttrs = item.Attributes.Where( attr => serviceAttrKeys.Contains( attr.Key ) ).Select( kvp => kvp.Value ).ToList();
            List<AttributeCache> serviceStatusAttrs = item.Attributes.Where( attr => serviceStatusAttrKeys.Contains( attr.Key ) ).Select( kvp => kvp.Value ).ToList();
            List<int> serviceAttrIds = serviceAttrs.Select( ac => ac.Id ).ToList();
            List<int> serviceStatusAttrIds = serviceStatusAttrs.Select( ac => ac.Id ).ToList();

            //If there is a modified on filter for events, filter on that first
            if ( eventModifiedRange != null && ( !String.IsNullOrEmpty( eventModifiedRange.lowerValue ) || !String.IsNullOrEmpty( eventModifiedRange.upperValue ) ) )
            {
                DateTime? lowerValue = null;
                DateTime? upperValue = null;
                if ( !String.IsNullOrEmpty( eventModifiedRange.lowerValue ) )
                {
                    lowerValue = DateTime.Parse( eventModifiedRange.lowerValue ).StartOfDay();
                }
                if ( !String.IsNullOrEmpty( eventModifiedRange.upperValue ) )
                {
                    upperValue = DateTime.Parse( eventModifiedRange.upperValue ).EndOfDay();
                }
                if ( lowerValue.HasValue && upperValue.HasValue )
                {
                    items = items.Where( i => i.ModifiedDateTime >= lowerValue.Value && i.ModifiedDateTime <= upperValue.Value );
                }
                else
                {
                    if ( lowerValue.HasValue )
                    {
                        items = items.Where( i => i.ModifiedDateTime >= lowerValue.Value );
                    }
                    if ( upperValue.HasValue )
                    {
                        items = items.Where( i => i.ModifiedDateTime <= upperValue.Value );
                    }
                }
            }

            //Only include requests that have been vetted by Event Request Admin
            items = FilterByStringMatch( context, items, requestStatusAttr, statuses );

            //Only include requests that are asking for a service of this SSP
            var requestedServices = av_svc.Queryable().Where( av => serviceAttrIds.Contains( av.AttributeId ) && av.ValueAsBoolean == true ).Select( av => av.EntityId ).Distinct();
            items = items.Join( requestedServices,
                    i => i.Id,
                    av => av,
                    ( i, av ) => i
                );

            if ( !String.IsNullOrEmpty( ministry ) )
            {
                //Only include requests for the ministry set in the filter
                items = FilterByStringMatch( context, items, ministryAttr, new List<string>() { ministry } );
            }

            if ( !String.IsNullOrEmpty( title ) )
            {
                //Only include requests matching the title set in the filter
                title = title.ToLower();
                items = items.Where( i => i.Title.ToLower().Contains( title ) );
            }

            //Only include items that match the desired status of the service
            if ( option == SSPDashboardOption.PendingConfirmation )
            {
                var notPendingService = av_svc.Queryable().Where( av => serviceStatusAttrIds.Contains( av.AttributeId ) && av.Value != "0" ).Select( av => av.EntityId ).Distinct();
                items = items.Where( i => !notPendingService.Contains( i.Id ) );
            }
            else if ( option != SSPDashboardOption.All )
            {
                string sspStatusValue = option.ToString();
                var matchSSPStatus = av_svc.Queryable().Where( av => serviceStatusAttrIds.Contains( av.AttributeId ) && av.Value == sspStatusValue ).Select( av => av.EntityId ).Distinct();
                items = items.Where( i => matchSSPStatus.Contains( i.Id ) );
            }

            //Filter by resoureces requested for the event
            if ( resources != null && resources.Any() )
            {
                items = FilterByListIntersection( context, items, requestTypeAttr, resources );
            }

            if ( !String.IsNullOrEmpty( submitter ) )
            {
                //Only include requests matching the submitter set in the filter
                submitter = submitter.ToLower();
                items = items.Where( i => i.CreatedByPersonName.ToLower().Contains( submitter ) || i.ModifiedByPersonName.ToLower().Contains( submitter ) );
            }

            //Only include requests that match the event has date in range filter
            if ( eventDateRange != null && !String.IsNullOrEmpty( eventDateRange.lowerValue ) || !String.IsNullOrEmpty( eventDateRange.upperValue ) )
            {
                DateTime? lowerValue = null;
                DateTime? upperValue = null;
                if ( !String.IsNullOrEmpty( eventDateRange.lowerValue ) )
                {
                    lowerValue = DateTime.Parse( eventDateRange.lowerValue );
                }
                if ( !String.IsNullOrEmpty( eventDateRange.upperValue ) )
                {
                    upperValue = DateTime.Parse( eventDateRange.upperValue );
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
                    items = items.Join( eventDates,
                        i => i.Id,
                        av => av.EntityId,
                        ( i, av ) => i
                    );
                }
            }

            var events = items.Distinct().OrderByDescending( i => i.ModifiedDateTime ).Select( cci =>
            {
                var bag = PluginHelper.GetCommonContentChannelItemEntityBag( cci );
                bag.CreatedBy = cci.CreatedByPersonName;
                bag.ModifiedBy = cci.ModifiedByPersonName;
                bag.Id = cci.Id;

                //Only Load Attributes Visible to the Provider
                Rock.Attribute.Helper.LoadAttributes( cci, context, VisibleAttributes );
                bag.LoadAttributesAndValuesForPublicView( cci, RequestContext.CurrentPerson );
                return bag;
            } ).ToList();

            return events;
        }

        /// <summary>
        /// Load Event Requests that match the desired status of the Support Service
        /// </summary>
        /// <returns></returns>
        protected List<ContentChannelItemBag> LoadByStatus( List<string> statuses, SSPDashboardOption option )
        {
            SetProperties();
            RockContext context = new RockContext();
            IEnumerable<ContentChannelItem> items = null;
            AttributeValueService av_svc = new AttributeValueService( context );

            // Limit requests only to ones that are requesting the service of relevance to the provider
            List<string> categories = EditCategories.Select( c => c.Name ).ToList();
            var map = SectionMappings.Where( sm => categories.Contains( sm.category ) && sm.status != null && sm.attr != null ).ToList();
            var serviceAttrKeys = map.Select( sm => sm.attr ).ToList();
            var serviceStatusAttrKeys = map.Select( sm => sm.status ).ToList();

            //Load all attributes to find specific attributes based on keys
            items = new ContentChannelItemService( context ).Queryable().Where( cci => cci.ContentChannelId == EventContentChannelId );
            ContentChannelItem item = items.First();
            item.LoadAttributes();

            AttributeCache requestStatusAttr = item.Attributes.FirstOrDefault( attr => attr.Key == "RequestStatus" ).Value;
            List<AttributeCache> serviceAttrs = item.Attributes.Where( attr => serviceAttrKeys.Contains( attr.Key ) ).Select( kvp => kvp.Value ).ToList();
            List<AttributeCache> serviceStatusAttrs = item.Attributes.Where( attr => serviceStatusAttrKeys.Contains( attr.Key ) ).Select( kvp => kvp.Value ).ToList();
            List<int> serviceAttrIds = serviceAttrs.Select( ac => ac.Id ).ToList();
            List<int> serviceStatusAttrIds = serviceStatusAttrs.Select( ac => ac.Id ).ToList();


            //Only include requests that have been vetted by Event Request Admin
            items = FilterByStringMatch( context, items, requestStatusAttr, statuses );

            //Include items with any of the matching services requested
            var requestedServices = av_svc.Queryable().Where( av => serviceAttrIds.Contains( av.AttributeId ) && av.ValueAsBoolean == true ).Select( av => av.EntityId ).Distinct();
            items = items.Join( requestedServices,
                    i => i.Id,
                    av => av,
                    ( i, av ) => i
                );

            //Only include items that match the desired statusof the service
            if ( option == SSPDashboardOption.PendingConfirmation )
            {
                var notPendingService = av_svc.Queryable().Where( av => serviceStatusAttrIds.Contains( av.AttributeId ) && av.Value != "0" ).Select( av => av.EntityId ).Distinct();
                items = items.Where( i => !notPendingService.Contains( i.Id ) );
            }
            else
            {
                string sspStatusValue = option.ToString();
                var matchSSPStatus = av_svc.Queryable().Where( av => serviceStatusAttrIds.Contains( av.AttributeId ) && av.Value == sspStatusValue ).Select( av => av.EntityId ).Distinct();
                items = items.Where( i => matchSSPStatus.Contains( i.Id ) );
            }

            var events = items.Distinct().AsEnumerable().OrderByDescending( i => i.ModifiedDateTime ).Select( cci =>
            {
                var bag = PluginHelper.GetCommonContentChannelItemEntityBag( cci );
                bag.CreatedBy = cci.CreatedByPersonName;
                bag.ModifiedBy = cci.ModifiedByPersonName;
                bag.Id = cci.Id;

                //Only Load Attributes Visible to the Provider
                Rock.Attribute.Helper.LoadAttributes( cci, context, VisibleAttributes );
                bag.LoadAttributesAndValuesForPublicView( cci, RequestContext.CurrentPerson );
                return bag;
            } ).ToList();

            return events;
        }

        private IEnumerable<ContentChannelItem> FilterByStringMatch( RockContext context, IEnumerable<ContentChannelItem> items, AttributeCache attr, List<string> values )
        {
            AttributeValueService av_svc = new AttributeValueService( context );
            var matches = av_svc.Queryable().Where( av => av.AttributeId == attr.Id && values.Contains( av.Value ) );
            items = items.Join( matches,
                    i => i.Id,
                    av => av.EntityId,
                    ( i, av ) => i
                );

            return items;
        }

        private IEnumerable<ContentChannelItem> FilterByListIntersection( RockContext context, IEnumerable<ContentChannelItem> items, AttributeCache attr, List<string> values )
        {
            AttributeValueService av_svc = new AttributeValueService( context );
            var matches = av_svc.Queryable().Where( av => av.AttributeId == attr.Id ).ToList().Where( av =>
            {
                var list = av.Value.Split( ',' ).Select( v => v.Trim() ).ToList();
                var intersect = values.Intersect( list );
                if ( intersect.Count() > 0 )
                {
                    return true;
                }
                return false;
            } );
            items = items.Join( matches,
                    i => i.Id,
                    av => av.EntityId,
                    ( i, av ) => i
                );

            return items;
        }

        private IQueryable<ContentChannelItem> FilterByBoolean( RockContext context, IQueryable<ContentChannelItem> items, AttributeCache attr, bool value )
        {
            AttributeValueService av_svc = new AttributeValueService( context );
            var matches = av_svc.Queryable().Where( av => av.AttributeId == attr.Id && av.ValueAsBoolean == value );
            items = items.Join( matches,
                    i => i.Id,
                    av => av.EntityId,
                    ( i, av ) => i
                );

            return items;
        }

        /// <summary>
        /// Method to get the configuration values and set class level variables
        /// </summary>
        protected void SetProperties()
        {
            RockContext rockContext = new RockContext();
            ContentChannelService cc_svc = new ContentChannelService( rockContext );
            ContentChannelItemService cci_svc = new ContentChannelItemService( rockContext );
            Guid eventCCGuid = Guid.Empty;
            Guid eventDetailsCCGuid = Guid.Empty;
            Guid eventChangesCCGuid = Guid.Empty;
            Guid eventDetailsChangesCCGuid = Guid.Empty;
            Guid eventCommentsCCGuid = Guid.Empty;

            if ( Guid.TryParse( GetAttributeValue( AttributeKeyBase.EventContentChannel ), out eventCCGuid ) )
            {
                ContentChannel cc = cc_svc.Get( eventCCGuid );
                EventContentChannelId = cc.Id;
                EventContentChannelTypeId = cc.ContentChannelTypeId;
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKeyBase.EventDetailsContentChannel ), out eventDetailsCCGuid ) )
            {
                ContentChannel dCC = cc_svc.Get( eventDetailsCCGuid );
                EventDetailsContentChannelId = dCC.Id;
                EventDetailsContentChannelTypeId = dCC.ContentChannelTypeId;

            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKeyBase.EventChangesContentChannel ), out eventChangesCCGuid ) )
            {
                ContentChannel cc = cc_svc.Get( eventChangesCCGuid );
                EventChangesContentChannelId = cc.Id;
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKeyBase.EventDetailsChangesContentChannel ), out eventDetailsChangesCCGuid ) )
            {
                ContentChannel dCC = cc_svc.Get( eventDetailsChangesCCGuid );
                EventDetailsChangesContentChannelId = dCC.Id;
            }
            if ( Guid.TryParse( GetAttributeValue( AttributeKeyBase.EventCommentsContentChannel ), out eventCommentsCCGuid ) )
            {
                ContentChannel cCC = cc_svc.Get( eventCommentsCCGuid );
                EventCommentsContentChannelId = cCC.Id;
            }

            List<Guid?> editCats = GetAttributeValues( AttributeKeyBase.EditCategories ).AsGuidOrNullList();
            if ( editCats.Any() )
            {
                EditCategories = new CategoryService( rockContext ).Queryable().Where( c => editCats.Contains( c.Guid ) ).ToList();
            }
            else
            {
                EditCategories = new List<Category>();
            }

            List<Guid?> viewCats = GetAttributeValues( AttributeKeyBase.ViewCategories ).AsGuidOrNullList();
            if ( viewCats.Any() )
            {
                ViewCategories = new CategoryService( rockContext ).Queryable().Where( c => viewCats.Contains( c.Guid ) ).ToList();
            }
            else
            {
                ViewCategories = new List<Category>();
            }

            VisibleAttributes = new List<AttributeCache>();
            ContentChannelItem request = new ContentChannelItem() { ContentChannelId = EventContentChannelId, ContentChannelTypeId = EventContentChannelTypeId };
            request.LoadAttributes();
            ContentChannelItem detail = new ContentChannelItem() { ContentChannelId = EventDetailsContentChannelId, ContentChannelTypeId = EventDetailsContentChannelTypeId };
            detail.LoadAttributes();

            foreach ( Category c in EditCategories )
            {
                var attrsInCategory = request.Attributes.Select( kvp => kvp.Value ).Where( ac => ac.CategoryIds.Contains( c.Id ) );
                VisibleAttributes.AddRange( attrsInCategory );
                attrsInCategory = detail.Attributes.Select( kvp => kvp.Value ).Where( ac => ac.CategoryIds.Contains( c.Id ) );
                VisibleAttributes.AddRange( attrsInCategory );
            }
            foreach ( Category c in ViewCategories )
            {
                var attrsInCategory = request.Attributes.Select( kvp => kvp.Value ).Where( ac => ac.CategoryIds.Contains( c.Id ) );
                VisibleAttributes.AddRange( attrsInCategory );
                attrsInCategory = detail.Attributes.Select( kvp => kvp.Value ).Where( ac => ac.CategoryIds.Contains( c.Id ) );
                VisibleAttributes.AddRange( attrsInCategory );
            }

            ministryAttr = request.Attributes.FirstOrDefault( attr => attr.Key == "Ministry" ).Value;
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
                bool isEventAdmin = CheckSecurityRole( context, AttributeKeyBase.EventAdminRole );
                bool isRoomAdmin = CheckSecurityRole( context, AttributeKeyBase.RoomAdminRole );
                Guid? sharedRequestGroupTypeGuid = GetAttributeValue( AttributeKeyBase.SharingGroupType ).AsGuidOrNull();
                return EventFormHelper.CheckRequestPermissions( request, p, isEventAdmin, isRoomAdmin, sharedRequestGroupTypeGuid, GetAttributeValue( AttributeKeyBase.SharedWithAttrKey ) );
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
        #endregion

        #region Classes
        public partial class ProviderViewModel
        {
            public List<ContentChannelItemBag> events { get; set; }
            public List<ContentChannelItemBag> pending { get; set; }
            public List<ContentChannelItemBag> cancelled { get; set; }
            public AttributeCache ministryAttr { get; set; }
            public List<DefinedValue> locations { get; set; }
            public List<DefinedValue> ministries { get; set; }
            public List<DefinedValue> budgetLines { get; set; }
            public List<Category> editCategories { get; set; }
            public List<Category> viewCategories { get; set; }
        }

        protected enum SSPDashboardOption
        {
            PendingConfirmation = 0,
            Confirmed = 1,
            Denied = 2,
            Cancelled = 3,
            All = 4
        }

        private class SectionMap
        {
            /// <summary>
            /// The Event Request Attribute that indicates the service is being requested
            /// </summary>
            public string attr { get; set; }
            /// <summary>
            /// The Attribute Category assigned to information for that service
            /// </summary>
            public string category { get; set; }
            /// <summary>
            /// The Event Request Attribute that indicates the current status (Needs Confirmation/Approval, Confirmed/Approved, or Denied) of that service
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// The Event Request Attribute Values that indicates what resources were requested (Room, Online, Catering, etc) 
            /// </summary>
            public string type { get; set; }
        }

        /// <summary>
        /// Map of the data contained in each event request and how it relates to each service provided by another team
        /// </summary>
        private List<SectionMap> SectionMappings
        {
            get
            {
                var map = new List<SectionMap>();
                map.Add( new SectionMap() { category = "Event" } );
                map.Add( new SectionMap() { attr = "NeedsSpace", category = "Event Space", type = "Room" } );
                map.Add( new SectionMap() { attr = "NeedsOnline", category = "Event Online", type = "Online Event" } );
                map.Add( new SectionMap() { attr = "NeedsCatering", category = "Event Catering", type = "Catering" } );
                map.Add( new SectionMap() { attr = "NeedsChildCare", category = "Event Childcare", status = "ChildcareStatus", type = "Childcare" } );
                map.Add( new SectionMap() { attr = "NeedsChildCareCatering", category = "Event Childcare Catering", status = "ChildcareCateringStatus", type = "Childcare Catering" } );
                map.Add( new SectionMap() { attr = "NeedsChildCare", category = "Event Childcare Registration", status = "ChildcareRegistrationStatus" } );
                map.Add( new SectionMap() { attr = "NeedsOpsAccommodations", category = "Event Ops Requests", type = "Extra Resources" } );
                map.Add( new SectionMap() { attr = "NeedsRegistration", category = "Event Registration", status = "RegistrationStatus", type = "Registration" } );
                map.Add( new SectionMap() { attr = "NeedsPublicity", category = "Event Publicity", type = "Publicity" } );
                map.Add( new SectionMap() { attr = "NeedsProductionAccommodations", category = "Event Production", type = "Production" } );
                map.Add( new SectionMap() { attr = "NeedsWorship", category = "Event Worship", type = "Worship" } );
                map.Add( new SectionMap() { attr = "NeedsWebCalendar", category = "Event Calendar", type = "Web Calendar" } );
                return map;
            }
        }

        public class GetRequestResponse
        {
            public ContentChannelItemBag request { get; set; }
            public ContentChannelItemBag requestChanges { get; set; }
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
            public ContentChannelItemBag detailChanges { get; set; }
        }
        #endregion
    }
}
