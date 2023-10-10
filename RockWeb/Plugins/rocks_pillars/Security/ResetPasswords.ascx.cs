//
// Copyright (C) Pillars Inc. - All Rights Reserved
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Plugins.rocks_pillars.Security
{
    /// <summary>
    /// Block used to download any scheduled payment transactions that were processed by payment gateway during a specified date range.
    /// </summary>
    [DisplayName( "Reset Passwords" )]
    [Category( "Pillars > Security" )]
    [Description( "Block used to reset all database passwords for a group of people in a selected dataview." )]

    [IntegerField( "Database Timeout", "The number of seconds to wait before reporting a database timeout.", false, 180 )]
    public partial class ResetPasswords : Rock.Web.UI.RockBlock
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

            // Set page timeout 
            int timeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;
            Server.ScriptTimeout = timeout;
            ScriptManager.GetCurrent( Page ).AsyncPostBackTimeout = timeout;

            nbSuccess.Visible = false;
            nbError.Visible = false;

            if ( !Page.IsPostBack )
            {
                dvPeople.EntityTypeId = EntityTypeCache.Get( "Rock.Model.Person" ).Id;
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
        }

        protected void btnReset_Click( object sender, EventArgs e )
        {
            var dataViewId = dvPeople.SelectedValueAsInt();
            var password = tbPassword.Text.Trim();
            int timeout = GetAttributeValue( "DatabaseTimeout" ).AsIntegerOrNull() ?? 180;

            int personCount = 0;
            int userCount = 0;

            if ( dataViewId.HasValue && password.IsNotNullOrWhiteSpace() )
            {
                var rockContext = new RockContext();
                var dataView = new DataViewService( rockContext ).Get( dataViewId.Value );

                List<IEntity> resultSet = null;
                var errorMessages = new List<string>();
                try
                {
                    var qry = dataView.GetQuery( null, rockContext, timeout, out errorMessages );
                    if ( qry != null )
                    {
                        resultSet = qry.AsNoTracking().ToList();
                    }
                }
                catch ( Exception exception )
                {
                    LogException( exception );
                    ShowError( exception.Message );
                }

                var dbEntityType = EntityTypeCache.Get( "Rock.Security.Authentication.Database" );
                var dbComponent = Rock.Security.AuthenticationContainer.GetComponent( dbEntityType.Name );

                if ( resultSet.Any() )
                {
                    foreach ( Person person in resultSet )
                    {
                        using ( var userDbContext = new RockContext() )
                        {
                            var users = new UserLoginService( userDbContext ).Queryable()
                                .Where( u =>
                                    u.PersonId == person.Id &&
                                    u.EntityTypeId == dbEntityType.Id )
                                .ToList();

                            if ( users.Any() )
                            {
                                personCount++;
                                foreach ( var user in users )
                                {
                                    userCount++;

                                    dbComponent.SetPassword( user, password );
                                    user.IsPasswordChangeRequired = true;
                                    userDbContext.SaveChanges();
                                }
                            }
                        }
                    }
                }

                nbSuccess.Text = string.Format( "{0:N0} passwords were reset for {1:N0} people.", userCount, personCount );
                nbSuccess.Visible = true;
            }
            else
            {
                ShowError( "Please select a valid Date View and Password!" );
            }

        }

        #endregion

        #region Methods

        private void ShowError(string message)
        {
            nbError.Text = message;
            nbError.Visible = true;
        }

        #endregion

    }
}