// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System.Reflection;

namespace RockWeb.com_bemaservices.Core
{
    /// <summary>
    /// Block for displaying the history of changes to a particular entity.
    /// </summary>
    [DisplayName( "Review History Log" )]
    [Category( "BEMA Services > History" )]
    [Description( "Block for displaying the history of changes to a particular entity." )]
    [EntityTypeField( "Entity Type", "The Entity Type to displayin the History Log.  Default is Person. Note that picking a Category that is not for the Entity Type set results in an empty grid.", true, DefaultValue = Rock.SystemGuid.EntityType.PERSON)]
    [GroupField( "Group To Exclude", "Security Group to exclude from the grid.  Used to not show edits by the team that reviews the list.", false, Rock.SystemGuid.Group.GROUP_ADMINISTRATORS)]
    [IntegerField( "Query Timeout", "Overrides the defult 30 seconds.  Needs to be higher for higher Max Record counts.", true, 60, "", 0)]
    [TextField( "Heading", "The Lava template to use for the heading. <span class='tip tip-lava'></span>", false, "{{ Entity.EntityStringValue }} (ID:{{ Entity.Id }})", "", 0 )]
    public partial class ReviewHistoryLog : RockBlock, ISecondaryBlock
    {

        #region Fields

        private EntityType _entityType = null;

        private Guid dtGuid = new Guid ( "341C6A20-ECC6-478E-89BE-FD5C9C07975D" );
        private string dtKey = "HistoryLogStatus";
        private string dtName = "History Log Status";
        private Guid dvPendingGuid = new Guid ( "45B25F7D-6AC2-475A-8F5B-4A4618A40B56" );
        private string dvPendingKey = "Pending";
        private string dvPendingName = "Pending";
        private Guid dvReviewedGuid = new Guid ( "941D2773-C3A9-48CD-B2A2-FBA71386DA15" );
        private string dvReviewedKey = "Reviewed";
        private string dvReviewedName = "Reviewed";
        private Guid dvCorrectedGuid = new Guid ( "F970EEA9-63F4-48A8-AE01-B0F64E4ED82D" );
        private string dvCorrectedKey = "Corrected";
        private string dvCorrectedName = "Corrected";
        private Guid logAttributeGuid = new Guid ( "5890810A-490B-4F01-9E73-373F6DE62741" );
        private string logAttributeKey = "ReviewStatus";
        private string logAttributeName = "Review Status";
        private int historyEntityId = 179;
        private int definedValueFieldId = 16;

        /// <summary>
        /// Gets or sets the available attributes.
        /// </summary>
        /// <value>
        /// The available attributes.
        /// </value>
        public List<AttributeCache> AvailableAttributes { get; set; }

        #endregion

        #region Base Control Methods


        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;
            gfSettings.DisplayFilterValue += gfSettings_DisplayFilterValue;

            gHistory.GridRebind += gHistory_GridRebind;
            gHistory.DataKeyNames = new string[] { "FirstHistoryId" };

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Check if defined type, values, attribute already created
            var rockContext = new RockContext ();
            var defType = DefinedTypeCache.Get ( dtGuid );
            if (defType == null)
            {
                var defTypeService = new DefinedTypeService ( rockContext );
                DefinedType newType = new DefinedType ();
                newType.Guid = dtGuid;
                newType.Name = dtName;
                newType.Description = "Used by BEMA Review History Log Block.  The status if the entry has been reviewed or corrected.";
                newType.HelpText = "Used by BEMA Review History Log Block.  The status if the entry has been reviewed or corrected.";
                newType.FieldTypeId = 1;
                newType.CategoryId = 155; // Tools
                defTypeService.Add ( newType );
                rockContext.SaveChanges ();

                defType = DefinedTypeCache.Get ( dtGuid );

                var defValue = new DefinedValueService ( rockContext );
                DefinedValue dvPending = new DefinedValue();
                dvPending.Guid = dvPendingGuid;
                dvPending.Value = dvPendingName;
                dvPending.DefinedTypeId = defType.Id;
                defValue.Add ( dvPending );

                DefinedValue dvReviewed = new DefinedValue ();
                dvReviewed.Guid = dvReviewedGuid;
                dvReviewed.Value = dvReviewedName;
                dvReviewed.DefinedTypeId = defType.Id;
                defValue.Add ( dvReviewed );

                DefinedValue dvCorrected = new DefinedValue ();
                dvCorrected.Guid = dvCorrectedGuid;
                dvCorrected.Value = dvCorrectedName;
                dvCorrected.DefinedTypeId = defType.Id;
                defValue.Add ( dvCorrected );

                rockContext.SaveChanges ();
            }

            var attr = AttributeCache.Get ( logAttributeGuid );
            if (attr == null)
            {
                // create History Log Attribute
                var newAttribute = new Rock.Model.Attribute ();
                newAttribute.Name = logAttributeName;
                newAttribute.Key = logAttributeKey;
                newAttribute.Guid = logAttributeGuid;
                newAttribute.EntityTypeId = historyEntityId;
                newAttribute.FieldTypeId = definedValueFieldId;

                var attributeQualifier = new AttributeQualifier ();
                attributeQualifier.Key = "definedtype";
                attributeQualifier.Value = defType.Id.ToString();
                newAttribute.AttributeQualifiers.Add (attributeQualifier);

                var pendingDefault = DefinedValueCache.Get ( dvPendingGuid );
                newAttribute.DefaultValue = pendingDefault.Guid.ToString();
                newAttribute.IsGridColumn = true;
                var attributeService = new AttributeService ( rockContext );
                attributeService.Add ( newAttribute );
                rockContext.SaveChanges ();
                AttributeCache.RemoveEntityAttributes ();
            }

            var entityTypeGuid = GetAttributeValue ( "EntityType" ).AsGuidOrNull ();
            if (entityTypeGuid.HasValue)
            {
                _entityType = new EntityTypeService ( new RockContext () ).Get ( entityTypeGuid.Value );
            }

            if ( _entityType != null )
            {
                if ( !Page.IsPostBack )
                {
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                    mergeFields.Add( "EntityType", _entityType );
                    lHeading.Text = GetAttributeValue( "Heading" ).ResolveMergeFields( mergeFields );

                    BindFilter();

                    BindGrid ();


                    IModel model = _entityType as IModel;
                    if ( model != null && model.CreatedDateTime.HasValue )
                    {
                        hlDateAdded.Text = String.Format( "Date Created: {0}", model.CreatedDateTime.Value.ToShortDateString() );
                    }
                    else
                    {
                        hlDateAdded.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState ( savedState );
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            //this.ViewState["State"] = "ThisIsNeededToMakeButtonsWork!IfNothingSaved,LoadViewStateIsNotCalledOnPartialPostBack.";
            return base.SaveViewState ();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "EntityType", _entityType );
            lHeading.Text = GetAttributeValue( "Heading" ).ResolveMergeFields( mergeFields );

            BindFilter ();

            BindGrid ();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            int? categoryId = cpCategory.SelectedValueAsInt();
            gfSettings.SaveUserPreference( "Category", categoryId.HasValue ? categoryId.Value.ToString() : "" );

            gfSettings.SaveUserPreference( "Summary Contains", tbSummary.Text );

            int? personId = ppWhoFilter.PersonId;
            gfSettings.SaveUserPreference ( "Edited By", personId.HasValue ? personId.ToString () : string.Empty );

            int? personEditedId = ppPersonEdited.PersonId;
            gfSettings.SaveUserPreference ( "Person Edited", personEditedId.HasValue ? personEditedId.ToString () : string.Empty );

            gfSettings.SaveUserPreference( "Date Range", drpDates.DelimitedValues );

            var temp = string.Join("'", dvfReviewStatus.SelectedDefinedValuesId);

            gfSettings.SaveUserPreference ( "Review Status", string.Join ( ",", dvfReviewStatus.SelectedDefinedValuesId ) );

            gfSettings.SaveUserPreference ( "Exclude Group", gpExcludeGroup.SelectedValueAsInt().HasValue ? gpExcludeGroup.SelectedValueAsInt().ToString () : "" );


            BindFilter ();
            BindGrid ();

        }

        /// <summary>
        /// Handles the DisplayFilterValue event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        void gfSettings_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Category":
                    {
                        int? categoryId = e.Value.AsIntegerOrNull();
                        if ( categoryId.HasValue )
                        {
                            var category = CategoryCache.Get( categoryId.Value );
                            if ( category != null )
                            {
                                e.Value = category.Name;
                            }
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }

                        break;
                    }
                case "Summary Contains":
                    {
                        break;
                    }
                case "Edited By":
                    {
                        int personId = int.MinValue;
                        if ( int.TryParse( e.Value, out personId ) )
                        {
                            var person = new PersonService( new RockContext() ).GetNoTracking( personId );
                            if ( person != null )
                            {
                                e.Value = person.FullName;
                            }
                        }
                        break;
                    }
                case "Person Edited":
                    {
                        int personId = int.MinValue;
                        if ( int.TryParse ( e.Value, out personId ) )
                        {
                            var person = new PersonService ( new RockContext () ).GetNoTracking ( personId );
                            if ( person != null )
                            {
                                e.Value = person.FullName;
                            }
                        }
                        break;
                    }
                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                case "Review Status":
                    {
                        var list = dvfReviewStatus.SelectedDefinedValuesId.ToList ();
                        e.Value = "";
                        foreach (var id in list)
                        {
                            if ( e.Value != "" )
                            {
                                e.Value += ", ";
                            }
                            e.Value = e.Value + DefinedValueCache.Get ( id ).Value;
                        }
                        break;
                    }
                case "Exclude Group":
                    {
                        var excludeGroup = gpExcludeGroup.SelectedValueAsInt ();
                        if (excludeGroup.HasValue)
                        {
                            e.Value = new GroupService ( new RockContext() ).Get ( excludeGroup.Value ).Name;
                        }
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gHistory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void gHistory_GridRebind( object sender, EventArgs e )
        {
            BindFilter ();
            BindGrid ();
        }

        /// <summary>
        /// Handles the Click event of the btnReviewed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void btnReviewed_Click( object sender, RowEventArgs e )
        {
            handleClick ( e, dvReviewedGuid );
        }

        /// <summary>
        /// Handles the Click event of the btnPending control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void btnPending_Click( object sender, RowEventArgs e )
        {
            handleClick ( e, dvPendingGuid );
        }

        /// <summary>
        /// Handles the Click event of the btnCorrected control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void btnCorrected_Click( object sender, RowEventArgs e )
        {
            handleClick(e, dvCorrectedGuid );
        }
        /// <summary>
        /// Handles the Click event of the btnPending control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void handleClick( RowEventArgs e, Guid actionGuid )
        {
            var rockContext = new RockContext ();

            var historyItem = new HistoryService ( rockContext ).Get ( e.RowKeyId );
            if ( historyItem != null )
            {
                var historyService = new HistoryService ( rockContext );
                IQueryable<History> qry;

                qry = historyService.Queryable ()
                .Where ( h =>
                     ( h.EntityTypeId == _entityType.Id && h.CreatedByPersonAliasId == historyItem.CreatedByPersonAliasId
                     ) );
                var text = gHistory.Rows[e.RowIndex].Cells[4].Text.Split('|');
                var Ids = text[1].Split(',').ToList();
                qry = qry.Where ( h => Ids.Contains(h.Id.ToString()) );

                var list = qry.Select ( h => h.Id ).ToList ();

                // load all associated log items
                foreach ( var item in list )
                {
                    historyItem = historyService.Get ( item );
                    historyItem.LoadAttributes ();
                    historyItem.SetAttributeValue ( logAttributeKey, actionGuid );
                    historyItem.SaveAttributeValues ( rockContext );
                }
                rockContext.SaveChanges ();
            }

            BindGrid ();
        }

        /// <summary>
        /// Handles the Click event of the btnPending control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAllPending_Click( object sender, EventArgs e )
        {
            updateAll ( dvPendingGuid );
        }

        /// <summary>
        /// Handles the Click event of the btnPending control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAllCorrected_Click( object sender, EventArgs e )
        {
            updateAll ( dvCorrectedGuid );
        }
        /// <summary>
        /// Handles the Click event of the btnPending control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnAllReviewed_Click( object sender, EventArgs e )
        {
            updateAll ( dvReviewedGuid );
        }
        
        /// <summary>
         /// Handles the Click event of the btnPending control.
         /// </summary>
         /// <param name="sender">The source of the event.</param>
         /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void updateAll ( Guid newStatusGuid )
        {
            var rockContext = new RockContext ();
            var itemsSelected = new List<int> ();

            gHistory.SelectedKeys.ToList ().ForEach ( b => itemsSelected.Add ( b.ToString ().AsInteger () ) );

            if ( !itemsSelected.Any () )
            {
                maWarning.Show ( "There were not any items selected.", Rock.Web.UI.Controls.ModalAlertType.None );
                return;
            }

            foreach ( GridViewRow row in gHistory.Rows )
            {
                if ( ((CheckBox)row.Cells[0].Controls[0]).Checked == true )
                {
                    var historyService = new HistoryService ( rockContext );
                    IQueryable<History> qry;

                    qry = historyService.Queryable ()
                    .Where ( h =>
                            ( h.EntityTypeId == _entityType.Id
                            ) );

                    var text = row.Cells[4].Text.Split ( '|' );
                    var Ids = text[1].Split ( ',' ).ToList ();

                    qry = qry.Where ( h => Ids.Contains ( h.Id.ToString () ) );

                    var list = qry.Select ( h => h.Id ).ToList ();

                    // load all associated log items
                    foreach ( var item in list )
                    {
                        var historyItem = historyService.Get ( item );
                        historyItem.LoadAttributes ();
                        historyItem.SetAttributeValue ( logAttributeKey, newStatusGuid );
                        historyItem.SaveAttributeValues ( rockContext );
                    }

                }

            }
            rockContext.SaveChanges ();

            BindGrid ();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            int? categoryId = gfSettings.GetUserPreference( "Category" ).AsIntegerOrNull();
            if ( !categoryId.HasValue )
            {
                cpCategory.SetValue ( 133 ); // Only Demographic Changes
            }
            else
            {
                cpCategory.SetValue ( categoryId );
            }

            tbSummary.Text = gfSettings.GetUserPreference( "Summary Contains" );
            int personId = int.MinValue;
            if ( int.TryParse( gfSettings.GetUserPreference( "Edited By" ), out personId ) )
            {
                var person = new PersonService( new RockContext() ).Get( personId );
                if ( person != null )
                {
                    ppWhoFilter.SetValue( person );
                }
                else
                {
                    gfSettings.SaveUserPreference( "Edited By", string.Empty );
                }
            }

            if ( int.TryParse ( gfSettings.GetUserPreference ( "Person Edited" ), out personId ) )
            {
                var person = new PersonService ( new RockContext () ).Get ( personId );
                if ( person != null )
                {
                    ppPersonEdited.SetValue ( person );
                }
                else
                {
                    gfSettings.SaveUserPreference ( "Person Edited", string.Empty );
                }
            }

            drpDates.DelimitedValues = gfSettings.GetUserPreference( "Date Range" );

            dvfReviewStatus.DefinedTypeId = DefinedTypeCache.Get ( dtGuid ).Id;
            var setting = gfSettings.GetUserPreference ( "Review Status" );
            if (setting != null && setting != "")
            {
                dvfReviewStatus.SelectedDefinedValuesId  = setting.Split ( ',' ).Select ( int.Parse ).ToList ().ToArray();
            }

            var idList = new List<int> ();
            idList.Add ( 1 ); // Security Group Type
            gpExcludeGroup.IncludedGroupTypeIds = idList;

            gpExcludeGroup.SetValue ( new GroupService(new RockContext()).Get ( gfSettings.GetUserPreference ( "Exclude Group" ).AsIntegerOrNull().HasValue ? gfSettings.GetUserPreference ( "Exclude Group" ).AsIntegerOrNull ().Value : 0));

        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( _entityType != null )
            {
                var rockContext = new RockContext();
                var historyService = new HistoryService( rockContext );
                IQueryable<History> qry;

                if ( _entityType.Id == EntityTypeCache.GetId<Rock.Model.Person>() )
                {
                    // If this is History for a Person, also include any History for any of their Families
                    //int? groupEntityTypeId = EntityTypeCache.GetId<Rock.Model.Group>();
                    //List<int> familyIds = ( _entity as Person ).GetFamilies().Select( a => a.Id ).ToList();

                    qry = historyService.Queryable().Include( a => a.CreatedByPersonAlias.Person )
                    .Where( h =>
                        ( h.EntityTypeId == _entityType.Id 
                        )
                        );

                    // as per issue #1594, if relatedEntityType is an Attribute then check View Authorization
                    var attributeEntity = EntityTypeCache.Get( Rock.SystemGuid.EntityType.ATTRIBUTE.AsGuid() );
                    var personAttributes = new AttributeService( rockContext ).GetByEntityTypeId( _entityType.Id ).ToList().Select( a => AttributeCache.Get( a ) );
                    var allowedAttributeIds = personAttributes.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) ).Select( a => a.Id ).ToList();
                    qry = qry.Where( a => ( a.RelatedEntityTypeId == attributeEntity.Id ) ? allowedAttributeIds.Contains( a.RelatedEntityId.Value ) : true );                            
                }
                else
                {
                    qry = historyService.Queryable().Include( a => a.CreatedByPersonAlias.Person )
                    .Where( h =>
                        ( h.EntityTypeId == _entityType.Id 
                        ) );
                }

                var historyCategories = new CategoryService( rockContext ).GetByEntityTypeId( EntityTypeCache.GetId<Rock.Model.History>() ).ToList().Select( a => CategoryCache.Get( a ) );
                var allowedCategoryIds = historyCategories.Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, CurrentPerson ) ).Select( a => a.Id ).ToList();

                qry = qry.Where( a => allowedCategoryIds.Contains( a.CategoryId ) );

                int? categoryId = gfSettings.GetUserPreference ( "Category" ).AsIntegerOrNull ();
                if ( categoryId.HasValue )
                {
                    qry = qry.Where( a => a.CategoryId == categoryId.Value );
                }
                else
                {
                    // if nonw specified, Demographic Changes only
                    qry = qry.Where ( a => a.CategoryId == 133 );
                }

                int? personId = gfSettings.GetUserPreference ( "Edited By" ).AsIntegerOrNull ();
                if ( personId.HasValue )
                {
                    qry = qry.Where ( h => h.CreatedByPersonAlias.PersonId == personId.Value );
                }

                int? personEditedId = gfSettings.GetUserPreference ( "Person Edited" ).AsIntegerOrNull ();
                if ( personEditedId.HasValue && _entityType.Id == EntityTypeCache.GetId<Rock.Model.Person> () )
                {
                    qry = qry.Where ( h => h.EntityId == personEditedId.Value 
                        && h.EntityType.Id == _entityType.Id);
                }

                var drp = new DateRangePicker();
                drp.DelimitedValues = gfSettings.GetUserPreference( "Date Range" );

                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where ( h => h.CreatedDateTime >= drp.LowerValue.Value );
                }
                else
                {
                    DateTime lowerDate = DateTime.Now.AddDays ( -7 );
                    qry = qry.Where ( h => h.CreatedDateTime >= lowerDate );
                }
                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays ( 1 );
                    qry = qry.Where ( h => h.CreatedDateTime < upperDate );
                }
                else
                {
                    DateTime upperDate = DateTime.Now.AddDays ( 1 );
                    qry = qry.Where ( h => h.CreatedDateTime < upperDate );
                }

                var statusFilter = gfSettings.GetUserPreference ( "Review Status" );
                if (statusFilter != "")
                {
                    var statusIds = statusFilter.Split ( ',' ).Select ( int.Parse ).ToList ();
                    if ( statusIds.Any())
                    {
                        var attrId = AttributeCache.Get ( logAttributeGuid ).Id;
                        var statusGuids = new DefinedValueService ( rockContext ).Queryable ()
                            .Where ( d => statusIds.Contains ( d.Id ) )
                            .Select ( d => d.Guid.ToString() ).ToList ();

                        // Most Pending do not have actual attribute so...
                        // If Pending in list, invert list, query not
                        if (statusGuids.Contains( dvPendingGuid.ToString() ))
                        {
                            var invertList = new List<string> ();
                            if ( ! statusGuids.Contains ( dvReviewedGuid.ToString () ) )
                            {
                                invertList.Add ( dvReviewedGuid.ToString () );
                            }
                            if ( !statusGuids.Contains ( dvCorrectedGuid.ToString () ) )
                            {
                                invertList.Add ( dvCorrectedGuid.ToString () );
                            }
                            if (invertList.Any())
                            {
                                var entityIds = new AttributeValueService ( rockContext ).Queryable ().AsNoTracking ()
                                    .Where ( a => invertList.Contains ( a.Value ) && a.AttributeId == attrId )
                                    .Select ( a => a.EntityId )
                                    .ToList ();
                                qry = qry.Where ( h => ! entityIds.Contains ( h.Id ) );
                            }

                        }
                        else
                        {
                            var entityIds = new AttributeValueService ( rockContext ).Queryable ().AsNoTracking ()
                                .Where ( a => statusGuids.Contains ( a.Value ) && a.AttributeId == attrId )
                                .Select ( a => a.EntityId )
                                .ToList ();
                            qry = qry.Where ( h => entityIds.Contains ( h.Id ) );
                        }
                    }
                }

                // Set Timeout
                var timeout = GetAttributeValue ( "QueryTimeout" ).AsInteger ();

                rockContext.Database.CommandTimeout = timeout;

                qry = qry.Where ( h => h.ValueName.Contains("Name")
                    || h.ValueName.Contains ( "Email" )
                    || h.ValueName.Contains ( "Phone" )
                    || h.ValueName.Contains ( "Gender" )
                    || h.ValueName.Contains ( "Birth" )
                    || h.ValueName.Contains ( "Location" )
                );
                qry = qry.Where ( h => h.OldValue != null );

                // Combine history records that were saved at the same time
                var historySummaryList = historyService.GetHistorySummary( qry );

                string summary = gfSettings.GetUserPreference( "Summary Contains" );
                if ( !string.IsNullOrWhiteSpace( summary ) )
                {
                    historySummaryList = historySummaryList.Where( h => h.HistoryList.Any( x => x.SummaryHtml.ScrubHtmlForGridDisplay().IndexOf( summary, StringComparison.OrdinalIgnoreCase ) >= 0 ) ).ToList();
                }

                SortProperty sortProperty = gHistory.SortProperty;
                if ( sortProperty != null )
                {
                    historySummaryList = historySummaryList.AsQueryable().Sort( sortProperty ).ToList();
                }
                else
                {
                    historySummaryList = historySummaryList.OrderByDescending( t => t.CreatedDateTime ).ToList();
                }

                // remove security group
                
                var excludeGroupId = gfSettings.GetUserPreference ( "Exclude Group" ).AsIntegerOrNull ();
                if (excludeGroupId.HasValue )
                {
                    var rockAdminGrp = new GroupMemberService ( rockContext ).GetByGroupId ( excludeGroupId.Value )
                        .Select ( m => m.PersonId ).ToList ();

                    var historySummaryList2 = 
                        historySummaryList.Where ( h => h.HistoryList.Any ( x => rockAdminGrp.Contains( (x.CreatedByPersonAlias == null ? 0 : x.CreatedByPersonAlias.PersonId )))).ToList ();
                    historySummaryList = historySummaryList.Except ( historySummaryList2 ).ToList ();
                }

                var attrService = new AttributeValueService ( rockContext );
                var logAttrId = AttributeCache.Get ( logAttributeGuid ).Id;
                String reviewStatus;
                var listIds = historySummaryList.Select ( l => l.HistoryList[0].Id ).ToList ();

                var reviewStatusList = attrService.Queryable ()
                    .Where ( a => a.AttributeId == logAttrId && listIds.Contains ( a.EntityId.Value ) )
                    .ToList ();
                 

                foreach ( var item in historySummaryList )
                {
                    var reviewStatusGuid = reviewStatusList.AsQueryable().Where( a => a.EntityId == item.HistoryList[0].Id )
                        .Select ( a => a.Value).AsGuidOrNullList().FirstOrDefault();
                    if (reviewStatusGuid != null)
                    {
                        reviewStatus = DefinedValueCache.Get ( reviewStatusGuid.Value ).Value;
                    }
                    else
                    {
                        reviewStatus = "Pending";
                    }

                    String Ids = string.Join ( ",", item.HistoryList.Select(h => h.Id.ToString()));

                    // Add Color to Status
                    if ( reviewStatus == "Pending" )
                    {
                        item.HistoryList[0].Caption = "<div style=\"color: red;\">" + reviewStatus + "</div><div class='hidden'>|" + Ids + "|</div>";
                    }
                    else if ( reviewStatus == "Corrected" )
                    {
                        item.HistoryList[0].Caption = "<div style=\"color: green;\">" + reviewStatus + "</div><div class='hidden'>|" + Ids + "|</div>";
                    }
                    else
                    {
                        item.HistoryList[0].Caption = "<div style=\"color: darkblue;\">" + reviewStatus + "</div><div class='hidden'>|" + Ids + "|</div>";
                    }

                }

                gHistory.DataSource = historySummaryList;
                gHistory.EntityTypeId = EntityTypeCache.Get<History>().Id;
                gHistory.DataBind();

            }
        }


        private void RemoveButtonColumns()
        {
            // Remove added button columns
            DataControlField buttonColumn  = gHistory.Columns.OfType<LinkButtonField> ().FirstOrDefault ( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gHistory.Columns.Remove ( buttonColumn );
            }

            buttonColumn = gHistory.Columns.OfType<LinkButtonField> ().FirstOrDefault ( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gHistory.Columns.Remove ( buttonColumn );
            }

            buttonColumn = gHistory.Columns.OfType<LinkButtonField> ().FirstOrDefault ( c => c.ItemStyle.CssClass == "grid-columncommand" );
            if ( buttonColumn != null )
            {
                gHistory.Columns.Remove ( buttonColumn );
            }
        }


        /// <summary>
        /// Hook so that other blocks can set the visibility of all ISecondaryBlocks on its page
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlList.Visible = visible;
        }

        #endregion
    }
}