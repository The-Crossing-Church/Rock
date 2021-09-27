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
using System.Web.UI.WebControls;
using tech.triumph.WebAgility.Redirector;
using tech.triumph.WebAgility.ExtensionMethods;

namespace RockWeb.Plugins.tech_triumph.WebAgility
{
    /// <summary>
    /// Block for listing Redirector Rules
    /// </summary>
    [DisplayName( "Redirector Configuration" )]
    [Category( "Triumph Tech > Web Agility" )]
    [Description( "Block for configuring the Redirector plugin." )]
    public partial class RedirectorConfiguration : RockBlock
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
                LoadViewConfiguration();
            }
        }

        #endregion

        #region Methods 

        /// <summary>
        /// Loads the edit configuration.
        /// </summary>
        private void LoadEditConfiguration()
        {
            var settings = RedirectorUtilities.ReadSettings();

            vlExclusionExtensions.Value = settings.ExtensionExceptions.AsDelimited( "|" );
            cbDynamicRobotFile.Checked = settings.EnableDynamicRobotFile;
        }

        /// <summary>
        /// Loads the view configuration.
        /// </summary>
        private void LoadViewConfiguration()
        {
            var settings = RedirectorUtilities.ReadSettings();

            lExclusionList.Text = settings.ExtensionExceptions.AsDelimited( ", " );

            if ( lExclusionList.Text.IsNullOrWhiteSpace() )
            {
                lExclusionList.Text = "None";
            }

            lDynamicRobotFile.Text = settings.EnableDynamicRobotFile.ToString();
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
            LoadViewConfiguration();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            var settings = RedirectorUtilities.ReadSettings();

            settings.ExtensionExceptions = vlExclusionExtensions.Value.Split( new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries ).ToList();
            settings.EnableDynamicRobotFile = cbDynamicRobotFile.Checked;

            RedirectorUtilities.WriteSettings( settings );

            HidePanels();
            pnlView.Visible = true;

            LoadViewConfiguration();
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            LoadEditConfiguration();

            HidePanels();
            pnlEdit.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTest_Click( object sender, EventArgs e )
        {
            HidePanels();
            pnlTest.Visible = true;
            lTestResults.Visible = false;

            // Populate the login status radio button
            if ( rblLoginStatus.Items.Count == 0 )
            {
                rblLoginStatus.Items.Add( new ListItem( "Logged In", "1" ) );
                rblLoginStatus.Items.Add( new ListItem( "Not Logged In", "0" ) );
                rblLoginStatus.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbTestLink control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTestLink_Click( object sender, EventArgs e )
        {
            lTestResults.Visible = true;

            // Get settings
            var settings = RedirectorUtilities.ReadSettings();

            // Check for excluded extension
            var exludedExtension = CheckExclusions( settings, urlTestLink.Text );
            if ( exludedExtension != null )
            {
                lTestResults.Text = string.Format("This URL was not processed as it contains the excluded extension {0}", exludedExtension );
                return;
            }

            // Find first matching rule
            MatchCollection regexSourceMatches = null;
            var isLoggedin = rblLoginStatus.SelectedValue.AsBoolean();
            var rule = GetFirstMatchingRule( settings, urlTestLink.Text, isLoggedin, txtUserAgent.Text, urlReferrer.Text,  out regexSourceMatches );

            if ( rule == null )
            {
                lTestResults.Text = "No rule was matched.";
            }
            else
            {
                lTestResults.Text = string.Format( "The rule <span class='label label-info'>{0}</span> was matched with the source of <span class='label label-info'>{1}</span>.", rule.Name, rule.SourceOptions.SourceUrl );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbCancelTest control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelTest_Click( object sender, EventArgs e )
        {
            HidePanels();
            pnlView.Visible = true;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Hides the panels.
        /// </summary>
        private void HidePanels()
        {
            pnlEdit.Visible = false;
            pnlView.Visible = false;
            pnlTest.Visible = false;
        }

        /// <summary>
        /// Checks the exclusions.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        private string CheckExclusions( RedirectorSettings settings, string url )
        {
            // Check exclusion extension list (to skip things like gif, jpg, etc)
            foreach ( var extension in settings.ExtensionExceptions )
            {
                if ( url.EndsWith( extension ) )
                {
                    return extension;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the first matching rule.
        /// </summary>
        /// <param name="settings">The settings.</param>
        /// <param name="url">The URL.</param>
        /// <param name="isLoggedIn">if set to <c>true</c> [is logged in].</param>
        /// <param name="userAgent">The user agent.</param>
        /// <param name="referrer">The referrer.</param>
        /// <param name="regexSourceMatches">The regex source matches.</param>
        /// <returns></returns>
        private RedirectorRule GetFirstMatchingRule( RedirectorSettings settings, string url, bool isLoggedIn, string userAgent, string referrer, out MatchCollection regexSourceMatches )
        {
            regexSourceMatches = null;

            foreach ( var rule in settings.Rules )
            {
                // First determine the case insensitivity
                var sourceUrl = url;

                var stringComparisonType = StringComparison.CurrentCulture;
                var regexOptions = RegexOptions.IgnoreCase;

                if ( !rule.SourceOptions.IsCaseSensitive )
                {
                    stringComparisonType = StringComparison.CurrentCultureIgnoreCase;
                    regexOptions = RegexOptions.None;
                }

                RedirectorRule matchedRule = null;

                // Next, do the check
                switch ( rule.SourceOptions.ComparisonType )
                {
                    case SourceComparisonType.StartsWith:
                        {
                            if ( sourceUrl.StartsWith( rule.SourceOptions.SourceUrl, stringComparisonType ) )
                            {
                                matchedRule = rule;
                            }
                            break;
                        }
                    case SourceComparisonType.Contains:
                        {
                            if ( sourceUrl.WebAgilityCaseInsensitiveContains( rule.SourceOptions.SourceUrl, stringComparisonType ) )
                            {
                                matchedRule = rule;
                            }
                            break;
                        }
                    case SourceComparisonType.EndsWith:
                        {
                            if ( sourceUrl.EndsWith( rule.SourceOptions.SourceUrl, stringComparisonType ) )
                            {
                                matchedRule = rule;
                            }
                            break;
                        }
                    case SourceComparisonType.Regex:
                        {
                            try
                            {
                                regexSourceMatches = Regex.Matches( sourceUrl, rule.SourceOptions.SourceUrl, regexOptions );
                                if ( regexSourceMatches.Count > 0 )
                                {
                                    matchedRule = rule;
                                }
                            }
                            catch ( Exception ) { }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                // No rule found, so no need to check other requirements
                if ( matchedRule == null )
                {
                    continue;
                }

                // Check is any other criteria exists
                if ( rule.MatchOptions.MatchCriteriaType == MatchComponentOptions.UrlOnly )
                {
                    return rule;
                }

                // Check if the rule cares about user login
                if ( rule.MatchOptions.MatchCriteriaType == MatchComponentOptions.UrlAndLoginStatus )
                {
                    if ( rule.MatchOptions.IsUserLoggedIn && isLoggedIn )
                    {
                        return rule;
                    }
                    else if ( !rule.MatchOptions.IsUserLoggedIn && !isLoggedIn )
                    {
                        return rule;

                    }

                    continue;
                }

                // Check if user agent matches
                if ( rule.MatchOptions.MatchCriteriaType == MatchComponentOptions.UrlAndUserAgent )
                {
                    if ( userAgent.Contains( rule.MatchOptions.UserAgentPattern ) )
                    {
                        return rule;
                    }

                    continue;
                }

                // Check if referrer matches
                if ( rule.MatchOptions.MatchCriteriaType == MatchComponentOptions.UrlAndReferrer )
                {
                    if ( referrer.Contains( rule.MatchOptions.ReferrerPattern ) )
                    {
                        return rule;
                    }

                    continue;
                }
            }

            return null;
        }

        #endregion
    }
}