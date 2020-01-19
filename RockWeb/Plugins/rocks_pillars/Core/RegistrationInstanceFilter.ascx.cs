
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Web.Cache;
using System.Data;

namespace RockWeb.Plugins.rocks_pillars.Core
{
    /// <summary>
    /// Block used to filter attendance by date/service
    /// </summary>
    [DisplayName( "Registration Instance Filter" )]
    [Category( "Pillars > Core" )]
    [Description( "Utility block used to select a registration instance and optional attribute(s) and then refresh page or redirect to new page." )]

    [CodeEditorField("Instance Filter Query", "The query to run to for the available instance values. Should return a Text and Value column. Use '{0}' for the active filter value", CodeEditorMode.Sql, CodeEditorTheme.Rock, 300, true, "", "", 0)]
    [LinkedPage( "Page Redirect", "If set, the filter button will redirect to the selected page.", false, "", "", 1 )]
    [BooleanField("Allow Multiple", "Should user be able to select multiple registration instances?", false, "", 2 )]
    [CustomDropdownListField("Show Attribute Field", "If only selecting one instance, should user be able to select one or more attributes?", "0^No,1^One Attribute,2^Multiple Attributes", false, "0", "", 3)]
    public partial class RegistrationInstanceFilter : Rock.Web.UI.RockBlock
    {

        private bool _allowMultiple = false;
        private int _attributeSelection = 0;

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            _allowMultiple = GetAttributeValue( "AllowMultiple" ).AsBoolean();
            _attributeSelection = GetAttributeValue( "ShowAttributeField" ).AsInteger();

            ddlInstance.Visible = !_allowMultiple;
            lbInstances.Visible = _allowMultiple;
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            nbQueryError.Visible = false;

            if ( !Page.IsPostBack )
            {
                string activeFilter = PageParameter( "Active" );
                string instanceFilter = PageParameter( "Instance" );
                string instancesFilter = PageParameter( "Instances" );
                string attributeFilter = PageParameter( "Attributes" );

                if ( activeFilter.IsNullOrWhiteSpace() )
                {
                    activeFilter = "active";
                }
                ddlActive.SetValue( activeFilter );

                BindInstances();

                if ( _allowMultiple )
                {
                    if ( lbInstances.Items.Count == 1 && instancesFilter == string.Empty )
                    {
                        lbInstances.Items[0].Selected = true;
                    }
                    else
                    {
                        lbInstances.SetValues( instancesFilter.SplitDelimitedValues().AsIntegerList() );
                    }
                }
                else
                {
                    if ( ddlInstance.Items.Count == 1 && instanceFilter == string.Empty )
                    {
                        ddlInstance.Items[0].Selected = true;
                    }
                    else
                    {
                        ddlInstance.SetValue( instanceFilter );
                    }
                }

                BindAttributes();

                if ( attributeFilter.IsNotNullOrWhiteSpace() )
                {
                    if ( _attributeSelection == 1 )
                    {
                        ddlAttribute.SetValue( attributeFilter );
                    }
                    else if ( _attributeSelection == 2 )
                    {
                        lbAttributes.SetValues( attributeFilter.SplitDelimitedValues() );
                    }
                }

            }
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
            BindInstances();
            BindAttributes();
        }

        protected void ddlActive_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindInstances();
            BindAttributes();
        }

        protected void ddlInstance_SelectedIndexChanged( object sender, EventArgs e )
        {
            BindAttributes();
        }

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            BindData();
        }

        #endregion

        #region Methods 

        /// <summary>
        /// Binds the schedules.
        /// </summary>
        /// <param name="serviceDate">The service date.</param>
        private void BindInstances()
        {
            string query = GetAttributeValue( "InstanceFilterQuery" ).Replace( "{0}", ddlActive.SelectedValue );

            string currentValue = _allowMultiple ?
                lbInstances.SelectedValuesAsInt.AsDelimited( "," ) :
                ddlInstance.SelectedValue;

            ddlInstance.Items.Clear();
            lbInstances.Items.Clear();
			
			ddlInstance.Items.Add( new ListItem() );
            lbInstances.Items.Add( new ListItem() );

            if ( query.IsNotNullOrWhiteSpace() )
            {
                try
                {
                    var dataTable = Rock.Data.DbService.GetDataTable( query, CommandType.Text, null );
                    if ( dataTable != null )
                    {
                        if ( dataTable.Columns.Contains( "Value" ) && dataTable.Columns.Contains( "Text" ) )
                        {
                            foreach ( DataRow row in dataTable.Rows )
                            {
                                var li = new ListItem( row["text"].ToString(), row["value"].ToString() );

                                ddlInstance.Items.Add( li );
                                lbInstances.Items.Add( li );
                            }
                        }
                        else
                        {
                            nbQueryError.Text = "<p>Query needs to return a 'Text' and a 'Value' column.</p>";
                            nbQueryError.Visible = true;
                        }
                    }
                }
                catch ( Exception ex )
                {
                    nbQueryError.Text = string.Format( "<p>{0}</p>", ex.Message );
                    nbQueryError.Visible = true;
                }
            }

            if ( _allowMultiple )
            {
                lbInstances.SetValues( currentValue.SplitDelimitedValues().AsIntegerList() );
            }
            else
            {
                ddlInstance.SetValue( currentValue );
            }
        }

        private void BindAttributes()
        { 
            ddlAttribute.Visible = false;
            lbAttributes.Visible = false;

            int? selectedInstanceId = ddlInstance.SelectedValueAsInt();
            if ( !_allowMultiple && _attributeSelection > 0 && selectedInstanceId.HasValue )
            {
                string currentValue = _attributeSelection == 2 ?
                    lbAttributes.SelectedValuesAsInt.AsDelimited( "," ) :
                    ddlAttribute.SelectedValue;

                ddlAttribute.Items.Clear();
                ddlAttribute.Items.Add( new ListItem( "-- Select Attribute --", "" ) );
                lbAttributes.Items.Clear();

                bool anyAttributes = false;

                using ( var rockContext = new RockContext() )
                {
                    var instance = new RegistrationInstanceService( rockContext ).Get( selectedInstanceId.Value );
                    if ( instance != null )
                    {
                        var entityTypeId = EntityTypeCache.Get( "Rock.Model.RegistrationRegistrant" ).Id;
                        foreach( var attribute in new AttributeService( rockContext )
                            .GetByEntityTypeQualifier( entityTypeId, "RegistrationTemplateId", instance.RegistrationTemplateId.ToString(), false )
                            .OrderBy( a => a.Order )
                            .ThenBy( a => a.Name ) )
                        {
                            var li = new ListItem( attribute.Name, attribute.Id.ToString() );

                            ddlAttribute.Items.Add( li );
                            lbAttributes.Items.Add( li );

                            anyAttributes = true;
                        }
                           
                    }
                }

                ddlAttribute.SetValue( currentValue );
                lbAttributes.SetValues( currentValue.SplitDelimitedValues().AsIntegerList() );

                ddlAttribute.Visible = anyAttributes && _attributeSelection == 1;
                lbAttributes.Visible = anyAttributes && _attributeSelection == 2;
            }
        }

        private void BindData()
        {
            var pageParams = new Dictionary<string, string>();
            pageParams.Add( "Active", ddlActive.SelectedValue );
            if ( _allowMultiple )
            {
                pageParams.Add( "Instances", lbInstances.SelectedValuesAsInt.AsDelimited( "," ) );
            }
            else
            {
                pageParams.Add( "Instance", ddlInstance.SelectedValue );
            }

            string attributeSelection = string.Empty;
            if ( _attributeSelection == 1 )
            {
                attributeSelection = ddlAttribute.SelectedValue;
            }
            else if ( _attributeSelection == 2 )
            {
                attributeSelection = lbAttributes.SelectedValues.AsDelimited( "," );
            }
            if ( attributeSelection.IsNotNullOrWhiteSpace())
            {
                pageParams.Add( "Attributes", attributeSelection );
            }

            if ( GetAttributeValue( "PageRedirect" ).IsNotNullOrWhiteSpace() )
            {
                NavigateToLinkedPage( "PageRedirect", pageParams );
            }
            else
            {
                NavigateToCurrentPage( pageParams );
            }
        }

        #endregion

    }
}