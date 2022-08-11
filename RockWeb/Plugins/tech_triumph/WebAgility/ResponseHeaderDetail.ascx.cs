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
using Rock.Model;
using Rock.Web.UI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using tech.triumph.WebAgility.ResponseHeader;

namespace RockWeb.Plugins.tech_triumph.WebAgility
{
    /// <summary>
    /// Block for editing Response Header Rules
    /// </summary>
    [DisplayName( "Response Header Rule Detail" )]
    [Category( "Triumph Tech > Web Agility" )]
    [Description( "Block for editing Response Header Rules." )]
    public partial class ResponseHeaderDetail : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
                        
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
                ShowDetails();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Shows the details.
        /// </summary>
        private void ShowDetails()
        {
            // Read role from the query string
            var ruleId = PageParameter( "RuleId" ).AsIntegerOrNull();

            // Set default settings on controls
            ddlSourceComparisonType.BindToEnum<SourceComparisonType>();
            cbIsActive.Checked = true;

            if ( !ruleId.HasValue )
            {
                return;
            }

            hfRuleId.Value = ruleId.Value.ToString(); ;

            var rule = ResponseHeaderUtilities.ReadSettings().Rules.Where( r => r.Id == ruleId.Value).FirstOrDefault();

            if ( rule.IsNull() )
            {
                return;
            }

            tbName.Text = rule.Name;
            cbIsActive.Checked = rule.IsActive;

            // Match Criteria
            tbSourceUrl.Text = rule.SourceOptions.SourceUrl;
            ddlSourceComparisonType.SetValue( ( int ) rule.SourceOptions.ComparisonType );
            cbIsCaseSensitive.Checked = rule.SourceOptions.IsCaseSensitive;

            // Header Configuration
            tbHeaderName.Text = rule.HeaderConfiguration.HeaderName;
            tbHeaderValue.Text = rule.HeaderConfiguration.HeaderValue;
            cbOverwriteExistingValue.Checked = rule.HeaderConfiguration.OverwriteExistingValue;
        }

        /// <summary>
        /// Determines whether [is valid regex] [the specified pattern].
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>
        ///   <c>true</c> if [is valid regex] [the specified pattern]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsValidRegex( string pattern )
        {
            if ( string.IsNullOrEmpty( pattern ) )
            {
                return false;
            }

            try
            {
                Regex.Match( "", pattern );
            }
            catch ( ArgumentException )
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            // Add validation to regex rules
            var ruleType = ddlSourceComparisonType.SelectedValueAsEnum<SourceComparisonType>();

            if ( ruleType == SourceComparisonType.Regex )
            {
                // Parse template to ensure it is correct
                if ( !IsValidRegex( tbSourceUrl.Text  ) )
                {
                    nbWarnings.Text = "The Regex template provided is not valid.";
                    return;
                }
            }

            var settings = ResponseHeaderUtilities.ReadSettings();
            ResponseHeaderRule rule = new ResponseHeaderRule();

            var ruleId = hfRuleId.Value.AsInteger();

            if ( ruleId != 0 )
            {
                rule = settings.Rules.Where( r => r.Id == hfRuleId.Value.AsInteger() ).FirstOrDefault();
            }
            else
            {
                settings.Rules.Add( rule );
                rule.Id = ResponseHeaderUtilities.GetNextRuleId();
            }

            rule.Name = tbName.Text;
            rule.IsActive = cbIsActive.Checked;

            // Match Criteria
            rule.SourceOptions.SourceUrl = tbSourceUrl.Text;
            rule.SourceOptions.ComparisonType = ruleType;
            rule.SourceOptions.IsCaseSensitive = cbIsCaseSensitive.Checked;

            // Header Configuration
            rule.HeaderConfiguration.HeaderName = tbHeaderName.Text;
            rule.HeaderConfiguration.HeaderValue = tbHeaderValue.Text;
            rule.HeaderConfiguration.OverwriteExistingValue = cbOverwriteExistingValue.Checked;

            ResponseHeaderUtilities.WriteSettings( settings );

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion
    }
}