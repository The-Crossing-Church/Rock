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
using System.ComponentModel.Composition;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Reporting;
using Rock.Reporting.DataFilter;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace org.crossingchurch.OurRock.Reporting.DataFilter
{
    /// <summary>
    /// Filter entities on any of its related entity properties or attribute values
    /// </summary>
    [Description( "Filter entities on any of its related entity properties or attribute values" )]
    [Export( typeof( DataFilterComponent ) )]
    [ExportMetadata( "ComponentName", "Related Entity Filter" )]
    public class RelatedEntityFilter : DataFilterComponent
    {
        #region Properties

        /// <summary>
        /// Gets the entity type that filter applies to.
        /// </summary>
        /// <value>
        /// The entity that filter applies to.
        /// </value>
        public override string AppliesToEntityType
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the section.
        /// </summary>
        /// <value>
        /// The section.
        /// </value>
        public override string Section
        {
            get { return string.Empty; }
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public override int Order
        {
            get
            {
                return int.MinValue;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the title.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public override string GetTitle( Type entityType )
        {
            return EntityTypeCache.Get( entityType ).FriendlyName + " Related Entities";
        }

        /// <summary>
        /// Formats the selection on the client-side.  When the filter is collapsed by the user, the Filterfield control
        /// will set the description of the filter to whatever is returned by this property.  If including script, the
        /// controls parent container can be referenced through a '$content' variable that is set by the control before 
        /// referencing this property.
        /// </summary>
        /// <value>
        /// The client format script.
        /// </value>
        public override string GetClientFormatSelection( Type entityType )
        {
            return "Join " + entityType.Name + " With Related Entities";
        }

        /// <summary>
        /// Formats the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="selection">The selection.</param>
        /// <returns></returns>
        public override string FormatSelection( Type entityType, string selection )
        {
            Configuration config = JsonConvert.DeserializeObject<Configuration>( selection );
            string friendlyName = "Join " + entityType.Name + " with ";
            if ( !String.IsNullOrEmpty( config.primary_entity ) )
            {
                friendlyName += config.primary_entity;
                if ( !String.IsNullOrEmpty( config.primary_entity_column ) )
                {
                    friendlyName += " on " + config.primary_entity_column;
                }
                if ( config.primary_entity_option.StartsWith( "Join" ) )
                {
                    friendlyName += ".";
                    if ( !String.IsNullOrEmpty( config.secondary_entity ) )
                    {
                        friendlyName += " Join " + config.primary_entity + " with " + config.secondary_entity;
                        if ( !String.IsNullOrEmpty( config.secondary_entity_column ) )
                        {
                            friendlyName += " on " + config.secondary_entity_column;
                        }
                    }
                }
                friendlyName += ".";
                if ( !String.IsNullOrEmpty( config.entity_filter_property ) )
                {
                    friendlyName += " Filter on ";
                    if ( config.primary_entity_option.StartsWith( "Join" ) )
                    {
                        friendlyName += config.secondary_entity;
                    }
                    else
                    {
                        friendlyName += config.primary_entity;
                    }
                    friendlyName += " > " + config.entity_filter_property;
                    if ( !String.IsNullOrEmpty( config.entity_filter_check ) )
                    {
                        friendlyName += " " + config.entity_filter_check;
                    }
                    if ( !String.IsNullOrEmpty( config.entity_filter_value ) )
                    {
                        friendlyName += " " + config.entity_filter_value;
                    }
                }
                friendlyName += ".";
            }
            else
            {
                friendlyName += "Related Entity";
                return friendlyName;
            }
            return friendlyName;
        }

        public override Expression GetExpression( Type entityType, IService serviceInstance, ParameterExpression parameterExpression, string selection )
        {
            if ( !String.IsNullOrEmpty( selection ) )
            {
                Configuration config = JsonConvert.DeserializeObject<Configuration>( selection );
                //Verify we have enough data to create the SQL
                if ( !String.IsNullOrEmpty( config.primary_join_entity_column ) && //column on entity
                     !String.IsNullOrEmpty( config.primary_entity ) && //join entity
                     !String.IsNullOrEmpty( config.primary_entity_column ) && //column on join entity
                     !String.IsNullOrEmpty( config.entity_filter_property ) && //filter property
                     !String.IsNullOrEmpty( config.entity_filter_check ) && //filter operator 
                     !String.IsNullOrEmpty( config.entity_filter_value ) && //filter value
                    ( config.primary_entity_option.StartsWith( "Filter" ) || //filter on primary entity or 
                    ( !String.IsNullOrEmpty( config.secondary_join_primary_column ) && //column on primary entity
                      !String.IsNullOrEmpty( config.secondary_entity ) && //secondary join entity
                      !String.IsNullOrEmpty( config.secondary_entity_column ) ) //column on secondary join entity
                    ) )
                {
                    string query = $@"
                        SELECT [{entityType.Name}].Id FROM [{entityType.Name}]
                            INNER JOIN [{config.primary_entity}] ON [{config.primary_entity}].[{config.primary_entity_column}] = [{entityType.Name}].[{config.primary_join_entity_column}]";
                    if ( config.primary_entity_option.StartsWith( "Join" ) )
                    {
                        query += $@"
                            INNER JOIN [{config.secondary_entity}] ON [{config.secondary_entity}].[{config.secondary_entity_column}] = [{config.primary_entity}].[{config.secondary_join_primary_column}]";
                        query += $@"
                            WHERE [{config.secondary_entity}].[{config.entity_filter_property}]";
                    }
                    else
                    {
                        query += $@"
                            WHERE [{config.primary_entity}].[{config.entity_filter_property}]";
                    }
                    if ( config.entity_filter_check == "Not Equal To" )
                    {
                        query += " != ";
                    }
                    else if ( config.entity_filter_check == "Contains" || config.entity_filter_check == "Starts With" || config.entity_filter_check == "Ends With" )
                    {
                        query += " LIKE ";
                    }
                    else if ( config.entity_filter_check == "Does Not Contain" )
                    {
                        query += " NOT LIKE ";
                    }
                    else if ( config.entity_filter_check == "Is Blank" )
                    {
                        query += " IS NULL";
                    }
                    else if ( config.entity_filter_check == "Is Not Blank" )
                    {
                        query += " IS NOT NULL";
                    }
                    else if ( config.entity_filter_check.Contains( "Greater Than" ) )
                    {
                        query += " >";
                        if ( config.entity_filter_check.Contains( "Or Equal To" ) )
                        {
                            query += "=";
                        }
                        query += " ";
                    }
                    else if ( config.entity_filter_check.Contains( "Less Than" ) )
                    {
                        query += " <";
                        if ( config.entity_filter_check.Contains( "Or Equal To" ) )
                        {
                            query += "=";
                        }
                        query += " ";
                    }
                    else
                    {
                        query += " = ";
                    }
                    if ( !query.EndsWith( "NULL" ) )
                    {
                        string value = ParseUserInput( config.entity_filter_property_data_type, config.entity_filter_value );

                        //Add search term to query, sanitize string input
                        if ( config.entity_filter_property_data_type.Contains( "date" ) || config.entity_filter_property_data_type.Contains( "time" ) || config.entity_filter_property_data_type.Contains( "char" ) || config.entity_filter_property_data_type.Contains( "text" ) || config.entity_filter_property_data_type.Contains( "unique" ) )
                        {
                            if ( config.entity_filter_check.Contains( "Contains" ) || config.entity_filter_check.Contains( "Ends With" ) )
                            {
                                query += "'%' + ";
                            }
                            query += $"'{value.Replace( "'", "''" )}'";
                            if ( config.entity_filter_check.Contains( "Contains" ) || config.entity_filter_check.Contains( "Starts With" ) )
                            {
                                query += " + '%' ";
                            }
                        }
                        else
                        {
                            query += value;
                        }

                        query += ";";

                        if ( !String.IsNullOrEmpty( value ) )
                        {
                            var entityIds = serviceInstance.Context.Database.SqlQuery<int>( query );
                            MethodInfo queryableMethodInfo = serviceInstance.GetType().GetMethod( "Queryable", new Type[] { } );
                            IQueryable<IEntity> entityQuery = queryableMethodInfo.Invoke( serviceInstance, null ) as IQueryable<IEntity>;
                            var qry = entityQuery.Where( p => entityIds.Contains( p.Id ) );
                            return FilterExpressionExtractor.Extract<IEntity>( qry, parameterExpression, "p" );
                        }
                    }
                }
            }
            return Expression.Empty();
        }

        /// <summary>
        /// Creates the child controls.
        /// </summary>
        /// <returns></returns>
        public override Control[] CreateChildControls( Type entityType, FilterField filterControl, FilterMode filterMode )
        {
            //Primary Entity Selection
            var primaryEntity = BuildEntitySection( filterControl.ID, "Primary" );
            var ddlPrimaryEntity = primaryEntity.Controls[0].Controls[0] as RockDropDownList;
            PopulateEntityData( entityType.Name, ddlPrimaryEntity );
            filterControl.Controls.Add( primaryEntity );

            //Entity Option Join to Another Table or Filter on Column in Table
            var entityOption = new RockRadioButtonList();
            entityOption.ID = string.Format( "{0}_radio{1}EntityOption", filterControl.ID, "Primary" );
            entityOption.DataSource = new List<ListItem>() { new ListItem() { Text = "Filter on", Value = "Filter" }, new ListItem() { Text = "Join", Value = "Join" } };
            entityOption.Visible = false;
            entityOption.AutoPostBack = true;
            entityOption.SelectedIndexChanged += radioPrimaryEntityOption_SelectedIndexChange;
            filterControl.Controls.Add( entityOption );

            //Secondary Entity Selection
            var secondaryEntity = BuildEntitySection( filterControl.ID, "Secondary" );
            secondaryEntity.Visible = false;
            filterControl.Controls.Add( secondaryEntity );

            //Entity Filter
            var entityFilter = BuildEntityFilter( filterControl.ID );
            filterControl.Controls.Add( entityFilter );

            return new Control[] { primaryEntity, entityOption, secondaryEntity, entityFilter };
        }


        #region Build Helpers
        protected Control BuildEntitySection( string filterControlId, string sectionPrefix )
        {
            //Create Row for Related Entity
            var rowEntitySelection = new HtmlGenericControl( "div" );
            rowEntitySelection.ID = string.Format( "{0}_row{1}Entity", filterControlId, sectionPrefix );
            rowEntitySelection.AddCssClass( "row" );
            //Entity Column
            var entityCol = new HtmlGenericControl( "div" );
            entityCol.ID = string.Format( "{0}_col{1}Entity", filterControlId, sectionPrefix );
            entityCol.AddCssClass( "col-xs-4" );
            //Entity Dropdown
            var ddlEntityField = new RockDropDownList();
            ddlEntityField.ID = string.Format( "{0}_ddl{1}Entity", filterControlId, sectionPrefix );
            ddlEntityField.Required = true;
            ddlEntityField.AutoPostBack = true;
            if ( sectionPrefix == "Primary" )
            {
                ddlEntityField.SelectedIndexChanged += ddlPrimaryEntity_SelectedIndexChange;
            }
            else
            {
                ddlEntityField.SelectedIndexChanged += ddlSecondaryEntity_SelectedIndexChange;
            }
            entityCol.Controls.Add( ddlEntityField );
            rowEntitySelection.Controls.Add( entityCol );

            //Source FK Column
            var fkCol = new HtmlGenericControl( "div" );
            fkCol.ID = string.Format( "{0}_col{1}SourceFK", filterControlId, sectionPrefix );
            fkCol.AddCssClass( "col-xs-4" );
            fkCol.Visible = false; //Only should be visible when entity is selected
            //Source FK Dropdown
            var ddlFKField = new RockDropDownList();
            ddlFKField.ID = string.Format( "{0}_ddl{1}SourceEntityColumn", filterControlId, sectionPrefix );
            ddlFKField.Required = true;
            ddlFKField.Label = "On Column:";
            ddlFKField.AutoPostBack = true;
            if ( sectionPrefix == "Primary" )
            {
                ddlFKField.SelectedIndexChanged += ddlPrimarySourceEntityColumn_SelectedIndexChange;
            }
            else
            {
                ddlFKField.SelectedIndexChanged += ddlSecondarySourceEntityColumn_SelectedIndexChange;
            }
            fkCol.Controls.Add( ddlFKField );
            rowEntitySelection.Controls.Add( fkCol );

            //Target FK Column
            var fkTargetCol = new HtmlGenericControl( "div" );
            fkTargetCol.ID = string.Format( "{0}_col{1}TargetFK", filterControlId, sectionPrefix );
            fkTargetCol.AddCssClass( "col-xs-4" );
            fkTargetCol.Visible = false; //Only should be visible when entity is selected
            //Target FK Dropdown
            var ddlTargetFKField = new RockDropDownList();
            ddlTargetFKField.ID = string.Format( "{0}_ddl{1}TargetEntityColumn", filterControlId, sectionPrefix );
            ddlTargetFKField.Required = true;
            ddlTargetFKField.Label = "Equals Column:";
            fkTargetCol.Controls.Add( ddlTargetFKField );
            rowEntitySelection.Controls.Add( fkTargetCol );

            return rowEntitySelection;
        }

        private void PopulateEntityData( string entityName, RockDropDownList ddlEntityField )
        {
            List<ForeignKeyRelationship> foreignKeys = GetForeignKeys( entityName );
            ddlEntityField.Label = "Join " + entityName + " With: ";
            ddlEntityField.Attributes["EntityType"] = entityName; //EntityType that is a relation for all the list items
            //var foreignKeyData = foreignKeys.GroupBy( fk => fk.foreign_table ).Select( g => new ForeignTableColumns() { table_name = g.Key, column_names = g.Select( i => i.foreign_column ).ToList() } ).ToList();
            ddlEntityField.Items.Clear();
            ddlEntityField.Attributes["Data"] = JsonConvert.SerializeObject( foreignKeys ); //All the possible foreign keys
            ddlEntityField.Items.Add( new ListItem() );
            ddlEntityField.Items.AddRange( foreignKeys.Select( fk => fk.foreign_table ).Distinct().Select( fk => new ListItem( fk ) ).ToArray() );
        }

        protected Control BuildEntityFilter( string filterControlId )
        {
            //Primary Entity Filter Row
            var entityFilterRow = new HtmlGenericControl( "div" );
            entityFilterRow.ID = string.Format( "{0}_rowEntityFilterRow", filterControlId );
            entityFilterRow.AddCssClass( "row" );
            entityFilterRow.Visible = false;
            //Primary Entity Filter Property Selection 
            var entityFilterFieldCol = new HtmlGenericControl( "div" );
            entityFilterFieldCol.ID = string.Format( "{0}_colEntityFilterField", filterControlId );
            entityFilterFieldCol.AddCssClass( "col-xs-4" );
            var ddlEntityFilterField = new RockDropDownList();
            ddlEntityFilterField.ID = string.Format( "{0}_ddlEntityFilterField", filterControlId );
            ddlEntityFilterField.Required = true;
            ddlEntityFilterField.Label = "Where:";
            ddlEntityFilterField.SelectedIndexChanged += ddlEntityFilter_SelectedIndexchange;
            ddlEntityFilterField.AutoPostBack = true;

            entityFilterFieldCol.Controls.Add( ddlEntityFilterField );
            entityFilterRow.Controls.Add( entityFilterFieldCol );

            //Primary Entity Filter Option Selection
            var entityFilterOptionCol = new HtmlGenericControl( "div" );
            entityFilterOptionCol.ID = string.Format( "{0}_colEntityFilterOption", filterControlId );
            entityFilterOptionCol.AddCssClass( "col-xs-4" );
            entityFilterOptionCol.Visible = false;
            var ddlEntityFilterOption = new RockDropDownList();
            ddlEntityFilterOption.ID = string.Format( "{0}_ddlEntityFilterOption", filterControlId );
            ddlEntityFilterOption.Required = true;
            ddlEntityFilterOption.Label = "Is...";

            entityFilterOptionCol.Controls.Add( ddlEntityFilterOption );
            entityFilterRow.Controls.Add( entityFilterOptionCol );

            //Primary Entity Filter Value Input
            var entityFilterValueCol = new HtmlGenericControl( "div" );
            entityFilterValueCol.Visible = false;
            entityFilterValueCol.ID = string.Format( "{0}_colEntityFilterValue", filterControlId );
            entityFilterValueCol.AddCssClass( "col-xs-4" );
            var entityFilterValue = new RockTextBox();
            entityFilterValue.ID = string.Format( "{0}_txtEntityFilterValue", filterControlId );
            entityFilterValue.Required = true;
            entityFilterValue.Label = "Value:";
            var filterValidator = new CustomValidator();
            filterValidator.ControlToValidate = entityFilterValue.ID;
            filterValidator.ID = string.Format( "{0}_validatorFilterValue", filterControlId );
            filterValidator.AddCssClass( "validation-error" );
            filterValidator.ServerValidate += FilterValidator_ServerValidate;

            entityFilterValueCol.Controls.Add( entityFilterValue );
            entityFilterValueCol.Controls.Add( filterValidator );
            entityFilterRow.Controls.Add( entityFilterValueCol );

            return entityFilterRow;
        }

        private void PopulateEntityFilter( string entityName, RockDropDownList ddlEntityFilterField )
        {
            List<EntityColumns> entityFields = GetEntityColumns( entityName );
            ddlEntityFilterField.Items.Clear();
            ddlEntityFilterField.Items.Add( new ListItem() );
            ddlEntityFilterField.Items.AddRange( entityFields.Select( ef => new ListItem( ef.column_name ) ).ToArray() );
            ddlEntityFilterField.Attributes["Data"] = JsonConvert.SerializeObject( entityFields ); //Store Data Type
            FilterField filterControl = ddlEntityFilterField.FirstParentControlOfType<FilterField>();
            var colEntityFilterOption = filterControl.FindControl( filterControl.ID + "_colEntityFilterOption" ) as HtmlGenericControl;
            colEntityFilterOption.Visible = false;
            var colEntityFilterValue = filterControl.FindControl( filterControl.ID + "_colEntityFilterValue" ) as HtmlGenericControl;
            colEntityFilterValue.Visible = false;
        }

        #endregion

        #region Actions

        protected void ddlPrimaryEntity_SelectedIndexChange( object sender, EventArgs e )
        {
            RockDropDownList ddl = sender as RockDropDownList;
            FilterField filterControl = ddl.FirstParentControlOfType<FilterField>();
            var ddlPrimarySourceFK = filterControl.FindControl( filterControl.ID + "_ddlPrimarySourceEntityColumn" ) as RockDropDownList;
            var ddlPrimarySourceFKCol = filterControl.FindControl( filterControl.ID + "_colPrimarySourceFK" ) as HtmlGenericControl;
            var ddlPrimaryTargetFKCol = filterControl.FindControl( filterControl.ID + "_colPrimaryTargetFK" ) as HtmlGenericControl;
            var radioOption = filterControl.FindControl( filterControl.ID + "_radioPrimaryEntityOption" ) as RockRadioButtonList;
            var rowSecondaryEntity = filterControl.FindControl( filterControl.ID + "_rowSecondaryEntity" ) as HtmlGenericControl;
            var rowEntityFilter = filterControl.FindControl( filterControl.ID + "_rowEntityFilterRow" ) as HtmlGenericControl;

            if ( !String.IsNullOrEmpty( ddl.SelectedValue ) )
            {
                var primaryFKData = JsonConvert.DeserializeObject<List<ForeignKeyRelationship>>( ddl.Attributes["Data"] );
                ddlPrimarySourceFK.Items.Clear();
                var data = primaryFKData.Where( fk => fk.foreign_table == ddl.SelectedValue );
                ddlPrimarySourceFK.Attributes["Data"] = JsonConvert.SerializeObject( data );
                ddlPrimarySourceFK.Items.Add( new ListItem() );
                ddlPrimarySourceFK.Items.AddRange( data.Select( c => new ListItem( c.primary_column ) ).Distinct().ToArray() );
                ddlPrimarySourceFK.Label = "On " + ddl.Attributes["EntityType"] + " Column:";
                ddlPrimarySourceFKCol.Visible = true;

                //Change Label
                radioOption.DataSource = new List<ListItem>() { new ListItem() { Text = $"Filter on {ddl.SelectedValue} Property", Value = "Filter" }, new ListItem() { Text = $"Join {ddl.SelectedValue} with Related Entity", Value = "Join" } };
                radioOption.SelectedValue = null;
                radioOption.DataBind();
                radioOption.Visible = true;

                //Hide rows until option is selected and repopulates them
                rowSecondaryEntity.Visible = false;
                rowEntityFilter.Visible = false;
                ddlPrimaryTargetFKCol.Visible = false;
            }
            else
            {
                radioOption.Visible = false;
                ddlPrimarySourceFKCol.Visible = false;
                ddlPrimaryTargetFKCol.Visible = false;
            }
        }

        protected void ddlPrimarySourceEntityColumn_SelectedIndexChange( object sender, EventArgs e )
        {
            RockDropDownList ddl = sender as RockDropDownList;
            FilterField filterControl = ddl.FirstParentControlOfType<FilterField>();
            var ddlPrimaryEntity = filterControl.FindControl( filterControl.ID + "_ddlPrimaryEntity" ) as RockDropDownList;
            var ddlPrimaryTargetFK = filterControl.FindControl( filterControl.ID + "_ddlPrimaryTargetEntityColumn" ) as RockDropDownList;
            var ddlPrimaryTargetFKCol = filterControl.FindControl( filterControl.ID + "_colPrimaryTargetFK" ) as HtmlGenericControl;

            if ( !String.IsNullOrEmpty( ddl.SelectedValue ) )
            {
                var primaryFKData = JsonConvert.DeserializeObject<List<ForeignKeyRelationship>>( ddl.Attributes["Data"] );
                ddlPrimaryTargetFK.Items.Clear();
                var data = primaryFKData.Where( fk => fk.foreign_table == ddlPrimaryEntity.SelectedValue && fk.primary_column == ddl.SelectedValue );
                ddlPrimaryTargetFK.Attributes["Data"] = JsonConvert.SerializeObject( data );
                ddlPrimaryTargetFK.Items.AddRange( data.Select( c => new ListItem( c.foreign_column ) ).Distinct().ToArray() );
                ddlPrimaryTargetFK.Label = "Equals " + ddlPrimaryEntity.SelectedValue + " Column:";
                ddlPrimaryTargetFKCol.Visible = true;
            }
            else
            {
                ddlPrimaryTargetFKCol.Visible = false;
            }
        }

        protected void ddlSecondarySourceEntityColumn_SelectedIndexChange( object sender, EventArgs e )
        {
            RockDropDownList ddl = sender as RockDropDownList;
            FilterField filterControl = ddl.FirstParentControlOfType<FilterField>();
            var ddlSecondaryEntity = filterControl.FindControl( filterControl.ID + "_ddlSecondaryEntity" ) as RockDropDownList;
            var ddlSecondaryTargetFK = filterControl.FindControl( filterControl.ID + "_ddlSecondaryTargetEntityColumn" ) as RockDropDownList;
            var ddlSecondaryTargetFKCol = filterControl.FindControl( filterControl.ID + "_colSecondaryTargetFK" ) as HtmlGenericControl;

            if ( !String.IsNullOrEmpty( ddl.SelectedValue ) )
            {
                var primaryFKData = JsonConvert.DeserializeObject<List<ForeignKeyRelationship>>( ddl.Attributes["Data"] );
                ddlSecondaryTargetFK.Items.Clear();
                var data = primaryFKData.Where( fk => fk.foreign_table == ddlSecondaryEntity.SelectedValue && fk.primary_column == ddl.SelectedValue );
                ddlSecondaryTargetFK.Attributes["Data"] = JsonConvert.SerializeObject( data );
                ddlSecondaryTargetFK.Items.AddRange( data.Select( c => new ListItem( c.foreign_column ) ).Distinct().ToArray() );
                ddlSecondaryTargetFK.Label = "Equals " + ddlSecondaryEntity.SelectedValue + " Column:";
                ddlSecondaryTargetFKCol.Visible = true;
            }
            else
            {
                ddlSecondaryTargetFKCol.Visible = false;
            }

        }

        protected void ddlSecondaryEntity_SelectedIndexChange( object sender, EventArgs e )
        {
            RockDropDownList ddl = sender as RockDropDownList;
            FilterField filterControl = ddl.FirstParentControlOfType<FilterField>();
            var ddlPrimaryEntity = filterControl.FindControl( filterControl.ID + "_ddlPrimaryEntity" ) as RockDropDownList;
            var ddlSecondaryFK = filterControl.FindControl( filterControl.ID + "_ddlSecondarySourceEntityColumn" ) as RockDropDownList;
            var ddlSecondaryFKCol = filterControl.FindControl( filterControl.ID + "_colSecondarySourceFK" ) as HtmlGenericControl;
            var ddlSecondaryTargetFKCol = filterControl.FindControl( filterControl.ID + "_colSecondaryTargetFK" ) as HtmlGenericControl;
            var rowEntityFilter = filterControl.FindControl( filterControl.ID + "_rowEntityFilterRow" ) as HtmlGenericControl;

            ddlSecondaryTargetFKCol.Visible = false;

            if ( !String.IsNullOrEmpty( ddl.SelectedValue ) )
            {
                var secondaryFKData = JsonConvert.DeserializeObject<List<ForeignKeyRelationship>>( ddl.Attributes["Data"] );
                ddlSecondaryFK.Items.Clear();
                var data = secondaryFKData.Where( fk => fk.foreign_table == ddl.SelectedValue );
                ddlSecondaryFK.Items.Add( new ListItem() );
                ddlSecondaryFK.Items.AddRange( data.Select( c => new ListItem( c.primary_column ) ).Distinct().ToArray() );
                ddlSecondaryFK.Attributes["Data"] = JsonConvert.SerializeObject( data );
                ddlSecondaryFK.Label = "On " + ddlPrimaryEntity.SelectedValue + " Column:";
                ddlSecondaryFKCol.Visible = true;

                //Show Entity Filter
                var ddlEntityFilter = filterControl.FindControl( filterControl.ID + "_ddlEntityFilterField" ) as RockDropDownList;
                PopulateEntityFilter( ddl.SelectedValue, ddlEntityFilter );
                rowEntityFilter.Visible = true;
            }
            else
            {
                ddlSecondaryFKCol.Visible = false;
                rowEntityFilter.Visible = false;
            }
        }

        protected void radioPrimaryEntityOption_SelectedIndexChange( object sender, EventArgs e )
        {
            RockRadioButtonList radio = sender as RockRadioButtonList;
            FilterField filterControl = radio.FirstParentControlOfType<FilterField>();
            var ddlPrimaryEntity = filterControl.FindControl( filterControl.ID + "_ddlPrimaryEntity" ) as RockDropDownList;
            var ddlSecondaryEntity = filterControl.FindControl( filterControl.ID + "_ddlSecondaryEntity" ) as RockDropDownList;
            var ddlSecondaryFK = filterControl.FindControl( filterControl.ID + "_ddlSecondarySourceEntityColumn" ) as RockDropDownList;
            var ddlSecondaryFKCol = filterControl.FindControl( filterControl.ID + "_colSecondarySourceFK" ) as HtmlGenericControl;
            var ddlSecondaryTargetFK = filterControl.FindControl( filterControl.ID + "_ddlSecondaryTargetEntityColumn" ) as RockDropDownList;
            var ddlSecondaryTargetFKCol = filterControl.FindControl( filterControl.ID + "_colSecondaryTargetFK" ) as HtmlGenericControl;
            var ddlEntityFilter = filterControl.FindControl( filterControl.ID + "_ddlEntityFilterField" ) as RockDropDownList;
            var rowSecondaryEntity = filterControl.FindControl( filterControl.ID + "_rowSecondaryEntity" ) as HtmlGenericControl;
            var rowEntityFilter = filterControl.FindControl( filterControl.ID + "_rowEntityFilterRow" ) as HtmlGenericControl;

            rowSecondaryEntity.Visible = false;
            ddlSecondaryFKCol.Visible = false;
            ddlSecondaryTargetFKCol.Visible = false;
            ddlSecondaryEntity.Items.Clear();
            ddlSecondaryFK.Items.Clear();
            ddlSecondaryTargetFK.Items.Clear();
            rowEntityFilter.Visible = false;
            ddlEntityFilter.Items.Clear();

            if ( radio.SelectedIndex == 0 )
            {
                rowSecondaryEntity.Visible = false;
                PopulateEntityFilter( ddlPrimaryEntity.SelectedValue, ddlEntityFilter );
                rowEntityFilter.Visible = true;
                ddlSecondaryEntity.Required = false;
                ddlSecondaryFK.Required = false;
                ddlSecondaryTargetFK.Required = false;
            }
            else if ( radio.SelectedIndex == 1 )
            {
                rowEntityFilter.Visible = false;
                PopulateEntityData( ddlPrimaryEntity.SelectedValue, ddlSecondaryEntity );
                rowSecondaryEntity.Visible = true;
            }
        }

        protected void ddlEntityFilter_SelectedIndexchange( object sender, EventArgs e )
        {
            var ddlEntityFilterField = sender as RockDropDownList;
            if ( !String.IsNullOrEmpty( ddlEntityFilterField.SelectedValue ) )
            {
                FilterField filterControl = ddlEntityFilterField.FirstParentControlOfType<FilterField>();
                var ddlEntityFilterOption = filterControl.FindControl( filterControl.ID + "_ddlEntityFilterOption" ) as RockDropDownList;
                var colEntityFilterOption = filterControl.FindControl( filterControl.ID + "_colEntityFilterOption" );
                var colEntityFilterValue = filterControl.FindControl( filterControl.ID + "_colEntityFilterValue" );
                List<EntityColumns> data = JsonConvert.DeserializeObject<List<EntityColumns>>( ddlEntityFilterField.Attributes["Data"] );
                var selectedDataType = data.FirstOrDefault( col => col.column_name == ddlEntityFilterField.SelectedValue );
                if ( selectedDataType != null )
                {
                    ddlEntityFilterOption.Items.Clear();
                    //Add Items based on FieldType
                    ddlEntityFilterOption.Items.Add( new ListItem( "Equal to" ) );
                    ddlEntityFilterOption.Items.Add( new ListItem( "Not Equal To" ) );
                    if ( selectedDataType.data_type.Contains( "varchar" ) )
                    {
                        ddlEntityFilterOption.Items.Add( new ListItem( "Contains" ) );
                        ddlEntityFilterOption.Items.Add( new ListItem( "Does Not Contain" ) );
                    }
                    ddlEntityFilterOption.Items.Add( new ListItem( "Is Blank" ) );
                    ddlEntityFilterOption.Items.Add( new ListItem( "Is Not Blank" ) );
                    if ( selectedDataType.data_type.Contains( "varchar" ) )
                    {
                        ddlEntityFilterOption.Items.Add( new ListItem( "Starts With" ) );
                        ddlEntityFilterOption.Items.Add( new ListItem( "Ends With" ) );
                    }
                    if ( selectedDataType.data_type.Contains( "int" ) || selectedDataType.data_type.Contains( "date" ) || selectedDataType.data_type.Contains( "float" ) || selectedDataType.data_type.Contains( "decimal" ) || selectedDataType.data_type.Contains( "time" ) )
                    {
                        ddlEntityFilterOption.Items.Add( new ListItem( "Greater Than" ) );
                        ddlEntityFilterOption.Items.Add( new ListItem( "Greater Than Or Equal To" ) );
                        ddlEntityFilterOption.Items.Add( new ListItem( "Less Than" ) );
                        ddlEntityFilterOption.Items.Add( new ListItem( "Less Than Or Equal To" ) );
                    }
                }
                colEntityFilterOption.Visible = true;
                colEntityFilterValue.Visible = true;
            }
        }

        private void FilterValidator_ServerValidate( object source, ServerValidateEventArgs args )
        {
            CustomValidator validator = source as CustomValidator;
            FilterField filterControl = validator.FirstParentControlOfType<FilterField>();
            var ddlEntityFilterField = filterControl.FindControl( filterControl.ID + "_ddlEntityFilterField" ) as RockDropDownList;
            var txtEntityFilterValue = filterControl.FindControl( filterControl.ID + "_txtEntityFilterValue" ) as RockTextBox;
            List<EntityColumns> data = JsonConvert.DeserializeObject<List<EntityColumns>>( ddlEntityFilterField.Attributes["Data"] );
            var selectedDataType = data.FirstOrDefault( col => col.column_name == ddlEntityFilterField.SelectedValue );
            if ( selectedDataType != null )
            {
                string value = ParseUserInput( selectedDataType.data_type, txtEntityFilterValue.Text );
                if ( !String.IsNullOrEmpty( value ) )
                {
                    args.IsValid = true;
                }
                else
                {
                    args.IsValid = false;
                    validator.ErrorMessage = "Filter value for " + selectedDataType.column_name + " must be a" + ( ( selectedDataType.data_type.StartsWith( "a" ) || selectedDataType.data_type.StartsWith( "e" ) || selectedDataType.data_type.StartsWith( "i" ) || selectedDataType.data_type.StartsWith( "o" ) || selectedDataType.data_type.StartsWith( "u" ) ) ? "n " : " " ) + selectedDataType.data_type + ".";
                }
            }
            var x = 7;
        }

        #endregion

        /// <summary>
        /// Renders the controls.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="filterControl">The filter control.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="controls">The controls.</param>
        public override void RenderControls( Type entityType, FilterField filterControl, HtmlTextWriter writer, Control[] controls )
        {
            base.RenderControls( entityType, filterControl, writer, controls );
        }

        /// <summary>
        /// Gets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="filterMode"></param>
        /// <returns></returns>
        public override string GetSelection( Type entityType, Control[] controls, FilterMode filterMode )
        {
            Configuration config = new Configuration();

            if ( controls.Length > 0 )
            {
                var rowPrimaryEntity = controls.FirstOrDefault( c => c.ID.EndsWith( "_rowPrimaryEntity" ) ) as HtmlGenericControl;
                if ( rowPrimaryEntity != null )
                {
                    var ddlPrimaryEntity = rowPrimaryEntity.Controls[0].Controls[0] as RockDropDownList;
                    if ( ddlPrimaryEntity != null )
                    {
                        config.primary_entity = ddlPrimaryEntity.SelectedValue;
                    }
                    var ddlPrimaryFK = rowPrimaryEntity.Controls[1].Controls[0] as RockDropDownList;
                    if ( ddlPrimaryFK != null )
                    {
                        config.primary_join_entity_column = ddlPrimaryFK.SelectedValue;
                        var ddlPrimaryTargetFK = rowPrimaryEntity.Controls[2].Controls[0] as RockDropDownList;
                        if ( ddlPrimaryTargetFK != null )
                        {
                            config.primary_entity_column = ddlPrimaryTargetFK.SelectedValue;
                        }
                    }
                }

                var radioOption = controls.FirstOrDefault( c => c.ID.EndsWith( "_radioPrimaryEntityOption" ) ) as RockRadioButtonList;
                if ( radioOption != null && radioOption.Visible == true )
                {
                    config.primary_entity_option = radioOption.SelectedValue;
                }

                var rowSecondaryEntity = controls.FirstOrDefault( c => c.ID.EndsWith( "_rowSecondaryEntity" ) ) as HtmlGenericControl;
                if ( rowSecondaryEntity != null && rowSecondaryEntity.Visible == true )
                {
                    var ddlSecondaryEntity = rowSecondaryEntity.Controls[0].Controls[0] as RockDropDownList;
                    if ( ddlSecondaryEntity != null )
                    {
                        config.secondary_entity = ddlSecondaryEntity.SelectedValue;
                    }
                    var ddlSecondaryFK = rowSecondaryEntity.Controls[1].Controls[0] as RockDropDownList;
                    if ( ddlSecondaryFK != null )
                    {
                        config.secondary_join_primary_column = ddlSecondaryFK.SelectedValue;
                        var ddlSecondaryTargetFK = rowSecondaryEntity.Controls[2].Controls[0] as RockDropDownList;
                        if ( ddlSecondaryTargetFK != null )
                        {
                            config.secondary_entity_column = ddlSecondaryTargetFK.SelectedValue;
                        }
                    }
                }

                var rowEntityFilterField = controls.FirstOrDefault( c => c.ID.EndsWith( "_rowEntityFilterRow" ) ) as HtmlGenericControl;
                if ( rowEntityFilterField != null && rowEntityFilterField.Visible == true )
                {
                    var ddlEntityFilterField = rowEntityFilterField.Controls[0].Controls[0] as RockDropDownList;
                    if ( ddlEntityFilterField != null )
                    {
                        config.entity_filter_property = ddlEntityFilterField.SelectedValue;
                        if ( !String.IsNullOrEmpty( ddlEntityFilterField.Attributes["Data"] ) )
                        {
                            List<EntityColumns> data = JsonConvert.DeserializeObject<List<EntityColumns>>( ddlEntityFilterField.Attributes["Data"] );
                            var selectedDataType = data.FirstOrDefault( col => col.column_name == ddlEntityFilterField.SelectedValue );
                            config.entity_filter_property_data_type = selectedDataType != null ? selectedDataType.data_type : "";
                        }
                    }
                    var ddlEntityFilterOption = rowEntityFilterField.Controls[1].Controls[0] as RockDropDownList;
                    if ( ddlEntityFilterOption != null )
                    {
                        config.entity_filter_check = ddlEntityFilterOption.SelectedValue;
                    }
                    var txtEntityFilterValue = rowEntityFilterField.Controls[2].Controls[0] as RockTextBox;
                    if ( txtEntityFilterValue != null )
                    {
                        config.entity_filter_value = txtEntityFilterValue.Text;
                    }
                }
            }

            return JsonConvert.SerializeObject( config );
        }
        /// <summary>
        /// Sets the selection.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <param name="controls">The controls.</param>
        /// <param name="selection">The selection.</param>
        public override void SetSelection( Type entityType, Control[] controls, string selection )
        {
            if ( controls.Count() < 1 )
            {
                return;
            }
            if ( !String.IsNullOrEmpty( selection ) )
            {
                Configuration config = JsonConvert.DeserializeObject<Configuration>( selection );
                var rowPrimaryEntity = controls.FirstOrDefault( c => c.ID.EndsWith( "_rowPrimaryEntity" ) ) as HtmlGenericControl;
                if ( rowPrimaryEntity != null )
                {
                    var ddlPrimaryEntity = rowPrimaryEntity.Controls[0].Controls[0] as RockDropDownList;
                    if ( ddlPrimaryEntity != null )
                    {
                        ddlPrimaryEntity.SelectedValue = config.primary_entity;
                        ddlPrimaryEntity_SelectedIndexChange( ddlPrimaryEntity, new EventArgs() );
                    }
                    var ddlPrimaryFK = rowPrimaryEntity.Controls[1].Controls[0] as RockDropDownList;
                    if ( ddlPrimaryFK != null )
                    {
                        ddlPrimaryFK.SelectedValue = config.primary_join_entity_column;
                        ddlPrimarySourceEntityColumn_SelectedIndexChange( ddlPrimaryFK, new EventArgs() );
                    }
                    var ddlPrimaryTargetFK = rowPrimaryEntity.Controls[2].Controls[0] as RockDropDownList;
                    if ( ddlPrimaryTargetFK != null )
                    {
                        ddlPrimaryTargetFK.SelectedValue = config.primary_entity_column;
                    }
                }

                var radioOption = controls.FirstOrDefault( c => c.ID.EndsWith( "_radioPrimaryEntityOption" ) ) as RockRadioButtonList;
                if ( radioOption != null )
                {
                    radioOption.SelectedValue = config.primary_entity_option;
                    radioPrimaryEntityOption_SelectedIndexChange( radioOption, new EventArgs() );
                }

                var rowSecondaryEntity = controls.FirstOrDefault( c => c.ID.EndsWith( "_rowSecondaryEntity" ) ) as HtmlGenericControl;
                if ( rowSecondaryEntity != null )
                {
                    var ddlSecondaryEntity = rowSecondaryEntity.Controls[0].Controls[0] as RockDropDownList;
                    if ( ddlSecondaryEntity != null )
                    {
                        ddlSecondaryEntity.SelectedValue = config.secondary_entity;
                        ddlSecondaryEntity_SelectedIndexChange( ddlSecondaryEntity, new EventArgs() );
                    }
                    var ddlSecondaryFK = rowSecondaryEntity.Controls[1].Controls[0] as RockDropDownList;
                    if ( ddlSecondaryFK != null )
                    {
                        ddlSecondaryFK.SelectedValue = config.secondary_join_primary_column;
                        ddlSecondarySourceEntityColumn_SelectedIndexChange( ddlSecondaryFK, new EventArgs() );
                    }
                    var ddlSecondaryTargetFK = rowSecondaryEntity.Controls[2].Controls[0] as RockDropDownList;
                    if ( ddlSecondaryTargetFK != null )
                    {
                        ddlSecondaryTargetFK.SelectedValue = config.secondary_entity_column;
                    }
                }

                var rowEntityFilterField = controls.FirstOrDefault( c => c.ID.EndsWith( "_rowEntityFilterRow" ) ) as HtmlGenericControl;
                if ( rowEntityFilterField != null )
                {
                    var ddlEntityFilterField = rowEntityFilterField.Controls[0].Controls[0] as RockDropDownList;
                    if ( ddlEntityFilterField != null )
                    {
                        ddlEntityFilterField.SelectedValue = config.entity_filter_property;
                        ddlEntityFilter_SelectedIndexchange( ddlEntityFilterField, new EventArgs() );
                    }
                    var ddlEntityFilterOption = rowEntityFilterField.Controls[1].Controls[0] as RockDropDownList;
                    if ( ddlEntityFilterOption != null )
                    {
                        ddlEntityFilterOption.SelectedValue = config.entity_filter_check;
                    }
                    var txtEntityFilterValue = rowEntityFilterField.Controls[2].Controls[0] as RockTextBox;
                    if ( txtEntityFilterValue != null )
                    {
                        txtEntityFilterValue.Text = config.entity_filter_value;
                    }
                }
            }
        }

        private List<ForeignKeyRelationship> GetForeignKeys( string tableName )
        {
            RockContext context = new RockContext();
            return context.Database.SqlQuery<ForeignKeyRelationship>( $@"
                SELECT primary_table.name AS primary_table,
                       primary_col.name   AS primary_column,
                       foreign_table.name AS foreign_table,
                       foreign_col.name   AS foreign_column
                FROM sys.foreign_key_columns fkc
                         INNER JOIN sys.tables foreign_table
                                    ON foreign_table.object_id = fkc.parent_object_id
                         INNER JOIN sys.columns foreign_col
                                    ON foreign_col.column_id = parent_column_id AND foreign_col.object_id = foreign_table.object_id
                         INNER JOIN sys.tables primary_table
                                    ON primary_table.object_id = fkc.referenced_object_id
                         INNER JOIN sys.columns primary_col
                                    ON primary_col.column_id = referenced_column_id AND primary_col.object_id = primary_table.object_id
                WHERE primary_table.name = @tableName
                UNION
                SELECT primary_table.name AS primary_table,
                       primary_col.name   AS primary_column,
                       foreign_table.name AS foreign_table,
                       foreign_col.name   AS foreign_column
                FROM sys.foreign_key_columns fkc
                         INNER JOIN sys.tables primary_table
                                    ON primary_table.object_id = fkc.parent_object_id
                         INNER JOIN sys.columns primary_col
                                    ON primary_col.column_id = parent_column_id AND primary_col.object_id = primary_table.object_id
                         INNER JOIN sys.tables foreign_table
                                    ON foreign_table.object_id = fkc.referenced_object_id
                         INNER JOIN sys.columns foreign_col
                                    ON foreign_col.column_id = referenced_column_id AND foreign_col.object_id = foreign_table.object_id
                WHERE primary_table.name = @tableName
                ORDER BY foreign_table, primary_column, foreign_column
            ", new SqlParameter( "@tableName", tableName ) ).ToList();
        }

        private List<EntityColumns> GetEntityColumns( string tableName )
        {
            RockContext context = new RockContext();
            return context.Database.SqlQuery<EntityColumns>( $@"
                SELECT COLUMN_NAME AS column_name, DATA_TYPE AS data_type
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME = @tableName
                ORDER BY column_name
            ", new SqlParameter( "@tableName", tableName ) ).ToList();
        }

        #endregion

        #region Helper Methods
        private string ParseUserInput( string data_type, string input_value )
        {
            string value = "";

            if ( data_type == "smallint" )
            {
                Int16 parsed;
                if ( Int16.TryParse( input_value, out parsed ) )
                {
                    value = parsed.ToString();
                }
            }
            else if ( data_type == "int" )
            {
                Int32 parsed;
                if ( Int32.TryParse( input_value, out parsed ) )
                {
                    value = parsed.ToString();
                }
            }
            else if ( data_type == "bigint" )
            {
                Int64 parsed;
                if ( Int64.TryParse( input_value, out parsed ) )
                {
                    value = parsed.ToString();
                }
            }
            else if ( data_type == "decimal" )
            {
                Decimal parsed;
                if ( Decimal.TryParse( input_value, out parsed ) )
                {
                    value = parsed.ToString();
                }
            }
            else if ( data_type == "float" || data_type == "money" || data_type == "numeric" || data_type == "smallmoney" )
            {
                Double parsed;
                if ( Double.TryParse( input_value, out parsed ) )
                {
                    value = parsed.ToString();
                }
            }
            else if ( data_type == "tinyint" )
            {
                Byte parsed;
                if ( Byte.TryParse( input_value, out parsed ) )
                {
                    value = parsed.ToString();
                }
            }
            else if ( data_type == "bit" )
            {
                Boolean parsed;
                if ( Boolean.TryParse( input_value, out parsed ) )
                {
                    if ( parsed == true )
                    {
                        value = "1";
                    }
                    else
                    {
                        value = "0";
                    }
                }
                else
                {
                    int bitParsed;
                    if ( Int32.TryParse( input_value, out bitParsed ) )
                    {
                        value = bitParsed.ToString();
                    }
                }
            }
            else if ( data_type == "date" || data_type == "datetime" || data_type == "datetime2" || data_type == "smalldatetime" )
            {
                DateTime parsed;
                if ( DateTime.TryParse( input_value, out parsed ) )
                {
                    value = parsed.ToString();
                }
            }
            else if ( data_type == "datetimeoffset" )
            {
                DateTimeOffset parsed;
                if ( DateTimeOffset.TryParse( input_value, out parsed ) )
                {
                    value = parsed.ToString();
                }
            }
            else if ( data_type == "time" )
            {
                TimeSpan parsed;
                if ( TimeSpan.TryParse( input_value, out parsed ) )
                {
                    value = parsed.ToString();
                }
            }
            else if ( data_type == "char" || data_type == "nchar" || data_type == "ntext" || data_type == "nvarchar" || data_type == "text" || data_type == "varchar" || data_type == "uniqueidentifier" )
            {
                value = input_value;
            }
            return value;
        }
        #endregion

        #region helper classes
        private class ForeignKeyRelationship
        {
            public string primary_table { get; set; }
            public string primary_column { get; set; }
            public string foreign_table { get; set; }
            public string foreign_column { get; set; }
        }

        private class EntityColumns
        {
            public string column_name { get; set; }
            public string data_type { get; set; }
        }

        private class Configuration
        {
            public string primary_entity { get; set; }
            public string primary_entity_column { get; set; }
            public string primary_join_entity_column { get; set; }
            public string primary_entity_option { get; set; }
            public string secondary_entity { get; set; }
            public string secondary_join_primary_column { get; set; }
            public string secondary_entity_column { get; set; }
            public string entity_filter_property { get; set; }
            public string entity_filter_property_data_type { get; set; }
            public string entity_filter_check { get; set; }
            public string entity_filter_value { get; set; }
        }
        #endregion
    }
}