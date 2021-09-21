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
using tech.triumph.WebAgility.Redirector;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace RockWeb.Plugins.tech_triumph.WebAgility
{
    /// <summary>
    /// Block for editing Redirector Rules
    /// </summary>
    [DisplayName( "Redirector Rule Detail" )]
    [Category( "Triumph Tech > Web Agility" )]
    [Description( "Block for editing Redirector Rules." )]
    public partial class RedirectorRuleDetail : RockBlock
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

            // Load default settings on controls
            ddlSourceComparisonType.BindToEnum<SourceComparisonType>();
            ddlCookieMatchLogic.BindToEnum<CookieMatchLogicType>();
            ddlMatchComponents.BindToEnum<MatchComponentOptions>();
            ddlActionType.BindToEnum<ActionType>();
            rdlErrorTypes.BindToEnum<ErrorType>();
            rdlErrorTypes.SetValue( (int) ErrorType.Error404 );
            rdlRedirectHttpCode.BindToEnum<RedirectHttpCode>();
            rdlRedirectHttpCode.SetValue( (int) RedirectHttpCode.MovedPermanently301 );
            ConfigureOptionControls();
            cbIsActive.Checked = true;

            if ( !ruleId.HasValue )
            {
                return;
            }

            hfRuleId.Value = ruleId.Value.ToString(); ;

            var rule = RedirectorUtilities.ReadSettings().Rules.Where( r => r.Id == ruleId.Value).FirstOrDefault();

            if ( rule.IsNull() )
            {
                return;
            }

            tbName.Text = rule.Name;
            cbIsActive.Checked = rule.IsActive;
            
            // Source options
            tbSourceUrl.Text = rule.SourceOptions.SourceUrl;
            ddlSourceComparisonType.SetValue( ( int ) rule.SourceOptions.ComparisonType );
            cbIsCaseSensitive.Checked = rule.SourceOptions.IsCaseSensitive;

            // Advanced match criteria options
            if ( rule.SourceOptions.IpAddressFilters.Count() > 0 || rule.SourceOptions.CookieKey.IsNotNullOrWhiteSpace() )
            {
                hfRedirectorShowAdvancedSettings.Value = "true";
            }
            
            liIpAddressFilters.Value = rule.SourceOptions.IpAddressFilters.ToJson();
            tbCookieKey.Text = rule.SourceOptions.CookieKey;
            ddlCookieMatchLogic.SetValue( ( int ) rule.SourceOptions.CookieMatchLogic );
            tbCookieValue.Text = rule.SourceOptions.CookieValue;

            // Match options
            ddlMatchComponents.SetValue( ( int ) rule.MatchOptions.MatchCriteriaType );
            tbReferrer.Text = rule.MatchOptions.ReferrerPattern;
            tbUserAgent.Text = rule.MatchOptions.UserAgentPattern;
            if ( rule.MatchOptions.IsUserLoggedIn )
            {
                rblMatchIsLoggedIn.SetValue("LoggedIn");
            }
            else
            {
                rblMatchIsLoggedIn.SetValue( "NotLoggedIn" );
            }

            // Action options
            ddlActionType.SetValue( ( int ) rule.ActionOptions.ActionType );
            rdlErrorTypes.SetValue( ( int ) rule.ActionOptions.ErrorType );
            rdlRedirectHttpCode.SetValue( ( int ) rule.ActionOptions.RedirectHttpCode );

            // Target options
            tbTarget.Text = rule.TargetOptions.TargetUrl;

            // Configure which controls should be visible
            ConfigureOptionControls();
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
                return false;

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

        /// <summary>
        /// Configures all the controls for options.
        /// </summary>
        private void ConfigureOptionControls()
        {
            ConfigureActionControls();
            ConfigureMatchControls();
        }

        /// <summary>
        /// Configures the match controls.
        /// </summary>
        private void ConfigureMatchControls()
        {
            var matchCriteria = ddlMatchComponents.SelectedValueAsEnum<MatchComponentOptions>();

            rblMatchIsLoggedIn.Visible = false;
            tbReferrer.Visible = false;
            tbUserAgent.Visible = false;

            switch ( matchCriteria )
            {
                case MatchComponentOptions.UrlAndLoginStatus:
                    {
                        rblMatchIsLoggedIn.Visible = true;
                        break;
                    }
                case MatchComponentOptions.UrlAndReferrer:
                    {
                        tbReferrer.Visible = true;
                        break;
                    }
                case MatchComponentOptions.UrlAndUserAgent:
                    {
                        tbUserAgent.Visible = true;
                        break;
                    }
            }
        }

        /// <summary>
        /// Configures the action controls.
        /// </summary>
        private void ConfigureActionControls()
        {
            rdlErrorTypes.Visible = false;
            rdlRedirectHttpCode.Visible = false;
            lbRegexTester.Visible = false;
            tbTarget.Visible = true;

            var actionType = ddlActionType.SelectedValueAsEnum<ActionType>();
            var sourceComparisonType = ddlSourceComparisonType.SelectedValueAsEnum<SourceComparisonType>();

            if ( sourceComparisonType == SourceComparisonType.Regex )
            {
                lbRegexTester.Visible = true;
            }

            switch ( actionType )
            {
                case ActionType.Error:
                    {
                        rdlErrorTypes.Visible = true;
                        tbTarget.Visible = false;
                        lbRegexTester.Visible = false;
                        break;
                    }
                case ActionType.Redirect:
                    {
                        rdlRedirectHttpCode.Visible = true;
                        break;
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
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the Click event of the lbRegexTester control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbRegexTester_Click( object sender, EventArgs e )
        {
            tbSourceMatch.Text = tbSourceUrl.Text;
            tbTargetMatch.Text = tbTarget.Text;

            if ( tbTestUrl.Text.IsNullOrWhiteSpace() )
            {
                tbTestUrl.Text = "http://example.com/test";
            }

            lTestResults.Text = string.Empty;

            mdRegexTester.Show();
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

            var settings = RedirectorUtilities.ReadSettings();
            RedirectorRule rule = new RedirectorRule();

            var ruleId = hfRuleId.Value.AsInteger();

            if ( ruleId != 0 )
            {
                rule = settings.Rules.Where( r => r.Id == hfRuleId.Value.AsInteger() ).FirstOrDefault();
            }
            else
            {
                settings.Rules.Add( rule );
                rule.Id = RedirectorUtilities.GetNextRuleId();
            }

            rule.Name = tbName.Text;
            rule.IsActive = cbIsActive.Checked;

            // Source options
            rule.SourceOptions.SourceUrl = tbSourceUrl.Text;
            rule.SourceOptions.ComparisonType = ruleType;
            rule.SourceOptions.IsCaseSensitive = cbIsCaseSensitive.Checked;

            // Advanced match criteria options
            rule.SourceOptions.IpAddressFilters = JsonConvert.DeserializeObject<List<IpAddressFilter>>( liIpAddressFilters.Value );

            rule.SourceOptions.CookieKey = tbCookieKey.Text;
            rule.SourceOptions.CookieMatchLogic = ddlCookieMatchLogic.SelectedValueAsEnum<CookieMatchLogicType>();
            rule.SourceOptions.CookieValue = tbCookieValue.Text;

            // Match options
            rule.MatchOptions.MatchCriteriaType = ddlMatchComponents.SelectedValueAsEnum<MatchComponentOptions>();
            rule.MatchOptions.UserAgentPattern = tbUserAgent.Text;
            rule.MatchOptions.ReferrerPattern = tbReferrer.Text;

            if ( rblMatchIsLoggedIn.SelectedValue == "LoggedIn" )
            {
                rule.MatchOptions.IsUserLoggedIn = true;
            }
            else
            {
                rule.MatchOptions.IsUserLoggedIn = false;
            }

            // Action options
            rule.ActionOptions.ActionType = ddlActionType.SelectedValueAsEnum<ActionType>();
            rule.ActionOptions.ErrorType = rdlErrorTypes.SelectedValueAsEnum<ErrorType>();
            rule.ActionOptions.RedirectHttpCode = rdlRedirectHttpCode.SelectedValueAsEnum<RedirectHttpCode>();

            // Target options
            rule.TargetOptions.TargetUrl = tbTarget.Text;
            
            RedirectorUtilities.WriteSettings( settings );

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

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlActionType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlActionType_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Show/Hide the error control
            ConfigureActionControls();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlMatchCriteria control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlMatchCriteria_SelectedIndexChanged( object sender, EventArgs e )
        {
            // Show/Hide the match controls
            ConfigureMatchControls();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlSourceComparisonType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlSourceComparisonType_SelectedIndexChanged( object sender, EventArgs e )
        {
            ConfigureActionControls();
        }

        /// <summary>
        /// Handles the Click event of the lbTestRule control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTestRule_Click( object sender, EventArgs e )
        {
            var rgxUrls = new Regex( tbSourceMatch.Text );
            var result = rgxUrls.Replace( tbTestUrl.Text, tbTargetMatch.Text );

            lTestResults.Text += string.Format( "<p>Before: {0}<br>After: {1}", tbTestUrl.Text, result );
        }

        /// <summary>
        /// Handles the SaveClick event of the mdRegexTester control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdRegexTester_SaveClick( object sender, EventArgs e )
        {
            tbSourceUrl.Text = tbSourceMatch.Text;
            tbTarget.Text = tbTargetMatch.Text;

            mdRegexTester.Hide();
        }

        #endregion
    }
}