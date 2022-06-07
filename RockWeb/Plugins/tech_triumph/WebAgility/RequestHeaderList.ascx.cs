// <copyright>
// Copyright by Triumph Tech
//
// NOTICE: All information contained herein is, and remains
// the property of Triumph Tech LLC. The intellectual and technical concepts contained
// herein are proprietary to Triumph Tech LLC  and may be covered by U.S. and Foreign Patents,
// patents in process, and are protected by trade secret or copyright law.
//
// Dissemination of this information or reproduction of this material
// is strictly forbidden unless prior written permission is obtained
// from Triumph Tech LLC.
// </copyright>
//
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using tech.triumph.WebAgility.RequestHeader;

namespace RockWeb.Plugins.tech_triumph.WebAgility
{
    /// <summary>
    /// Block for listing Request Header Rules
    /// </summary>
    [DisplayName( "Request Header Rule List" )]
    [Category( "Triumph Tech > Web Agility" )]
    [Description( "Block for listing Request Header Rules." )]

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Description = "Page to add/edit redirector rules",
        IsRequired = true,
        Order = 0 )]

    public partial class RequestHeaderList : RockBlock, ICustomGridColumns
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        /// <summary>
        /// Page Parameter Keys
        /// </summary>
        private class PageParameterKey
        {
            public const string RuleId = "RuleId";
        }

        #endregion Keys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gRequestHeaderRules.GridRebind += gTemplateList_GridRebind;
            gRequestHeaderRules.DataKeyNames = new string[] { "Id" };
            gRequestHeaderRules.Actions.ShowAdd = true;
            gRequestHeaderRules.Actions.AddClick += gRequestHeaderRules_AddClick;
            gRequestHeaderRules.GridReorder += gRequestHeaderRules_GridReorder;
            gRequestHeaderRules.AllowSorting = false;

            bool canAddEditDelete = IsUserAuthorized( Authorization.EDIT );
            gRequestHeaderRules.IsDeleteEnabled = canAddEditDelete;

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            
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

            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPledges control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gTemplateList_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Grid Events

        /// <summary>
        /// Handles the GridReorder event of the gRequestHeaderRules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        /// <exception cref="NotImplementedException"></exception>
        private void gRequestHeaderRules_GridReorder( object sender, GridReorderEventArgs e )
        {
            var settings = RequestHeaderUtilities.ReadSettings();

            var sourceRule = settings.Rules[e.OldIndex];
            settings.Rules.RemoveAt( e.OldIndex );
            settings.Rules.Insert( e.NewIndex, sourceRule );

            RequestHeaderUtilities.WriteSettings( settings );

            BindGrid();
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var settings = RequestHeaderUtilities.ReadSettings();
            var rules = settings.Rules;

            var sourceContainsFilter = rFilter.GetUserPreference( "SourceContains" );
            var filterActive = rFilter.GetUserPreference( "ShowActive" ).AsBooleanOrNull();

            // Filter based on source
            if ( sourceContainsFilter.IsNotNullOrWhiteSpace() )
            {
                rules = rules.Where( r => r.SourceOptions.SourceUrl.Contains( sourceContainsFilter ) ).ToList();
            }

            // Filter on is active
            if ( filterActive.HasValue && filterActive.Value == true )
            {
                rules = rules.Where( r => r.IsActive ).ToList();
            }

            gRequestHeaderRules.DataSource = rules;
            gRequestHeaderRules.DataBind();
        }

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            cbFilterActive.Checked = rFilter.GetUserPreference( "ShowActive" ).AsBoolean();
            tbFilterSource.Text = rFilter.GetUserPreference( "SourceContains" );
        }

        /// <summary>
        /// Handles the AddClick event of the gRequestHeaderRules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gRequestHeaderRules_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.RuleId, 0 );
        }

        /// <summary>
        /// Handles the RowSelected event of the gRequestHeaderRules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequestHeaderRules_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.RuleId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.DeleteUserPreferences();

            if ( cbFilterActive.Checked )
            {
                rFilter.SaveUserPreference( "ShowActive", cbFilterActive.Checked.ToString() );
            }

            rFilter.SaveUserPreference( "SourceContains", tbFilterSource.Text );
            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gRequestHeaderRules control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gRequestHeaderRules_Delete( object sender, RowEventArgs e )
        {
            var settings = RequestHeaderUtilities.ReadSettings();

            var newRuleList = new List<RequestHeaderRule>();

            foreach( var rule in settings.Rules )
            {
                if ( rule.Id != e.RowKeyId )
                {
                    newRuleList.Add( rule );
                }
            }

            settings.Rules = newRuleList;

            RequestHeaderUtilities.WriteSettings( settings );

            BindGrid();
        }

        #endregion
    }
}