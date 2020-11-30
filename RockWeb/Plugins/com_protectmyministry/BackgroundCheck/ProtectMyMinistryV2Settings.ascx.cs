//
// Copyright (C) Protect My Ministry - All Rights Reserved.
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Rock.Checkr.Constants;
using System.Web.UI.WebControls;

namespace RockWeb.Plugins.com_protectmyministry.BackgroundCheck
{
    [DisplayName( "Protect My Ministry 2.0 Settings" )]
    [Category( "Protect My Ministry > Background Check " )]
    [Description( "Block for updating the settings used by the Protect My Ministry 2.0 integration." )]
    public partial class ProtectMyMinistryV2Settings : Rock.Web.UI.RockBlock
    {

        #region Constants

        private const string CLIENT_URL = "https://protectmyministry.com/rockrms2-0/";
        private const string PROMOTION_IMAGE_URL = "~/Plugins/com_protectmyministry/BackgroundCheck/Assets/Images/pmm_transparent1.png";

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the active tab.
        /// </summary>
        /// <value>
        /// The active tab.
        /// </value>
        protected string ActiveTab { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            ActiveTab = ( ViewState["ActiveTab"] as string ) ?? string.Empty;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gTypes.DataKeyNames = new string[] { "Id" };
            gTypes.GridRebind += gTypes_GridRebind;
            gTypes.GridReorder += gTypes_GridReorder;
            gTypes.Actions.ShowAdd = false;
            gTypes.IsDeleteEnabled = false;

            gUsers.DataKeyNames = new string[] { "Id" };
            gUsers.Actions.AddClick += gUsers_Add;
            gUsers.GridRebind += gUsers_GridRebind;
            gUsers.Actions.ShowAdd = true;
            gUsers.IsDeleteEnabled = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbPackageListError.Visible = false;
            nbNotification.Visible = false;

            if ( !Page.IsPostBack )
            {
                int? tab = PageParameter( "Tab" ).AsIntegerOrNull();
                if ( tab.HasValue )
                {
                    switch ( tab.Value )
                    {
                        case 1:
                            ActiveTab = "lbPackages";
                            break;

                        case 2:
                            ActiveTab = "lbUsers";
                            break;
                    }
                }

                ShowDetail();
            }
            else
            {
                ShowDialog();
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            ViewState["ActiveTab"] = ActiveTab;
            return base.SaveViewState();
        }

        #endregion

        #region Events

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the lbSaveNew control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveNew_Click( object sender, EventArgs e )
        {
            if ( !string.IsNullOrWhiteSpace( tbUserNameNew.Text ) && !string.IsNullOrWhiteSpace( tbPasswordNew.Text ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    var settings = GetSettings( rockContext );
                    SetSettingValue( rockContext, settings, "AdminUsername", tbUserNameNew.Text );
                    SetSettingValue( rockContext, settings, "AdminPassword", tbPasswordNew.Text, true );

                    string defaultReturnUrl = string.Format( "{0}Webhooks/ProtectMyMinistryV2.ashx",
                        GlobalAttributesCache.Value( "PublicApplicationRoot" ).EnsureTrailingForwardslash() );
                    SetSettingValue( rockContext, settings, "ReturnURL", defaultReturnUrl );

                    rockContext.SaveChanges();

                    BackgroundCheckContainer.Instance.Refresh();

                    var definedType = DefinedTypeCache.Get( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_USERS.AsGuid() );
                    if ( definedType != null )
                    {
                        var service = new DefinedValueService( rockContext );

                        var definedValue = new DefinedValue();
                        definedValue.DefinedTypeId = definedType.Id;
                        definedValue.Value = tbUserNameNew.Text;
                        definedValue.Description = "";
                        definedValue.IsActive = true;

                        service.Add( definedValue );

                        rockContext.SaveChanges();

                        definedValue.LoadAttributes( rockContext );

                        definedValue.SetAttributeValue( "Username", tbUserNameNew.Text );
                        definedValue.SetAttributeValue( "Password", Encryption.EncryptString( tbPasswordNew.Text ) );
                        definedValue.SaveAttributeValues( rockContext );
                    }

                    RefreshPackages();

                    ShowEdit( settings );
                }
            }
            else
            {
                nbNotification.Text = "<p>Admin Username and Admin Password are both required.</p>";
                nbNotification.Visible = true;
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ShowEdit( GetSettings( rockContext ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var settings = GetSettings( rockContext );
                SetSettingValue( rockContext, settings, "ReturnURL", urlWebHook.Text );
                SetSettingValue( rockContext, settings, "Active", cbActive.Checked.ToString() );
                rockContext.SaveChanges();

                BackgroundCheckContainer.Instance.Refresh();

                ShowView( settings );
            }

        }

        /// <summary>
        /// Handles the Click event of the lbCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancel_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                ShowView( GetSettings( rockContext ) );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnDefault control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDefault_Click( object sender, EventArgs e )
        {
            var bioBlock = BlockCache.Get( Rock.SystemGuid.Block.BIO.AsGuid() );

            // Record an exception if the stock Bio block has been deleted but continue processing
            // the remaining settings.
            if ( bioBlock == null )
            {
                var errorMessage = string.Format( "Stock Bio block ({0}) is missing.", Rock.SystemGuid.Block.BIO );
                ExceptionLogService.LogException( new Exception( errorMessage ) );
            }
            else
            {
                List<Guid> workflowActionGuidList = bioBlock.GetAttributeValues( "WorkflowActions" ).AsGuidList();
                if ( workflowActionGuidList == null || workflowActionGuidList.Count == 0 )
                {
                    // Add to Bio Workflow Actions
                    bioBlock.SetAttributeValue( "WorkflowActions", com.protectmyministry.SystemGuid.WorkflowType.PROTECT_MY_MINISTRY_V2 );
                }
                else
                {
                    //var workflowActionValues = workflowActionValue.Split( new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    Guid guid = com.protectmyministry.SystemGuid.WorkflowType.PROTECT_MY_MINISTRY_V2.AsGuid();
                    if ( !workflowActionGuidList.Any( w => w == guid ) )
                    {
                        // Add to Bio Workflow Actions
                        workflowActionGuidList.Add( guid );
                    }

                    // Remove PMM from Bio Workflow Actions
                    guid = Rock.SystemGuid.WorkflowType.PROTECTMYMINISTRY.AsGuid();
                    workflowActionGuidList.RemoveAll( w => w == guid );

                    // Remove Checkr from Bio Workflow Actions
                    guid = CheckrSystemGuid.CHECKR_WORKFLOW_TYPE.AsGuid();
                    workflowActionGuidList.RemoveAll( w => w == guid );

                    bioBlock.SetAttributeValue( "WorkflowActions", workflowActionGuidList.AsDelimited( "," ) );
                }

                bioBlock.SaveAttributeValue( "WorkflowActions" );
            }

            // Save the admin username/password
            string pmm2TypeName = ( typeof( com.protectmyministry.BackgroundCheck.ProtectMyMinistryV2 ) ).FullName;
            var pmm2Component = BackgroundCheckContainer.Instance.Components.Values.FirstOrDefault( c => c.Value.TypeName == pmm2TypeName );
            pmm2Component.Value.SetAttributeValue( "Active", "True" );
            pmm2Component.Value.SaveAttributeValue( "Active" );

            // Set as the default provider in the system setting
            Rock.Web.SystemSettings.SetValue( Rock.SystemKey.SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER, pmm2TypeName );

            ShowDetail();
        }


        #endregion

        #region Tab Events

        /// <summary>
        /// Handles the Click event of the lbTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbTab_Click( object sender, EventArgs e )
        {
            LinkButton lb = sender as LinkButton;
            if ( lb != null )
            {
                ActiveTab = lb.ID;
                ShowTab();
            }
        }

        #endregion

        #region Package Grid Events

        /// <summary>
        /// Handles the GridRebind event of the gTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gTypes_GridRebind( object sender, EventArgs e )
        {
            BindPackageGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gTypes_RowSelected( object sender, RowEventArgs e )
        {
            ShowPackageEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridReorder event of the gTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gTypes_GridReorder( object sender, GridReorderEventArgs e )
        {
            var definedType = DefinedTypeCache.Get( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_PACKAGES.AsGuid() );
            if ( definedType != null )
            {
                var changedIds = new List<int>();

                using ( var rockContext = new RockContext() )
                {
                    var definedValueService = new DefinedValueService( rockContext );
                    var definedValues = definedValueService.Queryable().Where( a => a.DefinedTypeId == definedType.Id ).OrderBy( a => a.Order ).ThenBy( a => a.Value );
                    changedIds = definedValueService.Reorder( definedValues.ToList(), e.OldIndex, e.NewIndex );
                    rockContext.SaveChanges();
                }
            }

            BindPackageGrid();
        }

        protected void dlgPackage_SaveClick( object sender, EventArgs e )
        {
            int? definedValueId = hlPackageDefinedValueId.Value.AsIntegerOrNull();
            if ( definedValueId.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = new DefinedValueService( rockContext );

                    var definedValue = service.Get( definedValueId.Value );
                    if ( definedValue != null )
                    {
                        definedValue.IsActive = cbPackageIsActive.Checked;

                        definedValue.LoadAttributes();
                        definedValue.SetAttributeValue( "DateAttribute", ddlDateAttribute.SelectedValue );
                        definedValue.SetAttributeValue( "ResultAttribute", ddlResultAttribute.SelectedValue );
                        definedValue.SetAttributeValue( "DocumentAttribute", ddlDocumentAttribute.SelectedValue );
                        definedValue.SetAttributeValue( "CheckedAttribute", ddlCheckedAttribute.SelectedValue );

                        rockContext.SaveChanges();
                        definedValue.SaveAttributeValues( rockContext );
                    }
                }
            }

            BindPackageGrid();
            HideDialog();
        }

        #endregion

        #region User Grid Events

        protected void gUsers_GridRebind( object sender, EventArgs e )
        {
            BindUsersGrid();
        }

        protected void gUsers_RowSelected( object sender, RowEventArgs e )
        {
            ShowUserEdit( e.RowKeyId );
        }

        protected void gUsers_Add( object sender, EventArgs e )
        {
            ShowUserEdit( 0 );
        }

        /// <summary>
        /// Handles the Delete event of the gUsers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gUsers_Delete( object sender, RowEventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                var definedValueService = new DefinedValueService( rockContext );
                var value = definedValueService.Get( e.RowKeyId );
                if ( value != null )
                {
                    string errorMessage;
                    if ( !definedValueService.CanDelete( value, out errorMessage ) )
                    {
                        mdGridUsersWarningValues.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    definedValueService.Delete( value );
                    rockContext.SaveChanges();
                }

                BindUsersGrid();
            }
        }

        protected void dlgUser_SaveClick( object sender, EventArgs e )
        {
            int definedValueId = hlUserDefinedValueId.Value.AsInteger();

            var definedType = DefinedTypeCache.Get( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_USERS.AsGuid() );
            if ( definedType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var service = new DefinedValueService( rockContext );

                    DefinedValue definedValue = null;
                    if ( !definedValueId.Equals( 0 ) )
                    {
                        definedValue = service.Get( definedValueId );
                    }

                    if ( definedValue == null )
                    {
                        definedValue = new DefinedValue();
                        definedValue.DefinedTypeId = definedType.Id;
                        service.Add( definedValue );
                    }

                    definedValue.Value = tbUserTitle.Text;
                    definedValue.Description = tbUserDescription.Text;
                    definedValue.IsActive = cbUserIsActive.Checked;
                    rockContext.SaveChanges();

                    definedValue.LoadAttributes( rockContext );

                    definedValue.SetAttributeValue( "Username", tbUserUsername.Text );
                    definedValue.SetAttributeValue( "Password", Encryption.EncryptString( tbUserPassword.Text ) );
                    definedValue.SaveAttributeValues( rockContext );
                }
            }

            BindUsersGrid();
            HideDialog();
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the tab.
        /// </summary>
        private void ShowTab()
        {
            liPackages.RemoveCssClass( "active" );
            pnlPackages.Visible = false;

            liUsers.RemoveCssClass( "active" );
            pnlUsers.Visible = false;

            switch ( ActiveTab ?? string.Empty )
            {
                case "lbPackages":
                    liPackages.AddCssClass( "active" );
                    pnlPackages.Visible = true;
                    BindPackageGrid();
                    break;

                case "lbUsers":
                    liUsers.AddCssClass( "active" );
                    pnlUsers.Visible = true;
                    BindUsersGrid();
                    break;
            }
        }

        /// <summary>
        /// Determines whether PMM is the default provider.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if PMM is the default provider; otherwise, <c>false</c>.
        /// </returns>
        private bool IsDefaultProvider()
        {
            string providerTypeName = ( typeof( com.protectmyministry.BackgroundCheck.ProtectMyMinistryV2 ) ).FullName;
            string defaultProvider = Rock.Web.SystemSettings.GetValue( Rock.SystemKey.SystemSetting.DEFAULT_BACKGROUND_CHECK_PROVIDER ) ?? string.Empty;
            return providerTypeName == defaultProvider;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="restUserId">The rest user identifier.</param>
        public void ShowDetail()
        {
            using ( var rockContext = new RockContext() )
            {
                var settings = GetSettings( rockContext );
                if ( settings != null )
                {
                    if ( IsDefaultProvider() )
                    {
                        btnDefault.Visible = false;
                    }
                    else
                    {
                        btnDefault.Visible = true;
                    }

                    var definedType = DefinedTypeCache.Get( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_USERS.AsGuid() );
                    if ( definedType != null && definedType.DefinedValues.Any() )
                    { 
                        ShowView( settings );
                    }
                    else
                    {
                        ShowNew();
                    }
                }
                else
                {
                    ShowNew();
                }
            }
        }

        /// <summary>
        /// Shows the new.
        /// </summary>
        public void ShowNew()
        {
            imgPromotion.ImageUrl = RockPage.ResolveRockUrl( PROMOTION_IMAGE_URL );
            hlClient.NavigateUrl = CLIENT_URL;

            tbUserNameNew.Text = string.Empty;
            tbPasswordNew.Text = string.Empty;

            pnlNew.Visible = true;
            pnlViewDetails.Visible = false;
            pnlEditDetails.Visible = false;
            pnlTabs.Visible = false;

            HideSecondaryBlocks( true );
        }

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void ShowView( List<AttributeValue> settings )
        {
            using ( var rockContext = new RockContext() )
            {
                var packages = new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_PACKAGES.AsGuid() )
                    .Where( v => v.IsActive )
                    .Select( v => v.Value )
                    .ToList();

                lPackages.Text = packages.AsDelimited( "<br/>" );
            }

            nbSSLWarning.Visible = !GetSettingValue( settings, "ReturnURL" ).StartsWith( "https://" );
            nbSSLWarning.NotificationBoxType = NotificationBoxType.Warning;

            BindPackageGrid();
            BindUsersGrid();

            pnlNew.Visible = false;
            pnlViewDetails.Visible = true;
            pnlEditDetails.Visible = false;
            pnlTabs.Visible = false;

            HideSecondaryBlocks( false );
        }

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public void ShowEdit( List<AttributeValue> settings )
        {
            urlWebHook.Text = GetSettingValue( settings, "ReturnURL" );
            cbActive.Checked = GetSettingValue( settings, "Active" ).AsBoolean();

            RefreshPackages();

            pnlNew.Visible = false;
            pnlViewDetails.Visible = false;
            pnlEditDetails.Visible = true;
            pnlTabs.Visible = true;

            HideSecondaryBlocks( true );
        }

        private void RefreshPackages()
        {
            var errorMessages = new List<string>();

            DefinedValueCache userAccountDv = null;
            var userAccountsDt = DefinedTypeCache.Get( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_USERS.AsGuid() );
            if ( userAccountsDt != null )
            {
                userAccountDv = userAccountsDt.DefinedValues.FirstOrDefault( v => v.IsActive );
            }

            if ( userAccountDv != null )
            {
                string pmm2TypeName = ( typeof( com.protectmyministry.BackgroundCheck.ProtectMyMinistryV2 ) ).FullName;
                var bgComponent = BackgroundCheckContainer.Instance.Components.Values.FirstOrDefault( c => c.Value.TypeName == pmm2TypeName );
                if ( bgComponent != null )
                {
                    var pmm2Component = bgComponent.Value as com.protectmyministry.BackgroundCheck.ProtectMyMinistryV2;
                    if ( pmm2Component != null )
                    {
                        string username = userAccountDv.GetAttributeValue( "Username" );
                        string password = Encryption.DecryptString( userAccountDv.GetAttributeValue( "Password" ) );

                        var invitations = pmm2Component.GetPackageTypes( username, password, out errorMessages );
                        if ( invitations != null && !errorMessages.Any() )
                        {
                            var definedType = DefinedTypeCache.Get( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_PACKAGES.AsGuid() );
                            if ( definedType != null )
                            {
                                using ( var rockContext = new RockContext() )
                                {
                                    var dvService = new DefinedValueService( rockContext );
                                    var definedValues = dvService
                                        .GetByDefinedTypeGuid( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_PACKAGES.AsGuid() )
                                        .ToList();

                                    // Update existing
                                    var existingInvitations = new List<string>();
                                    foreach ( var dv in definedValues )
                                    {
                                        dv.LoadAttributes( rockContext );
                                        string pkgName = dv.GetAttributeValue( "PMMPackageName" );

                                        var invitation = invitations.FirstOrDefault( i => i.Id == pkgName );
                                        if ( invitation == null )
                                        {
                                            dv.IsActive = false;
                                        }
                                        else
                                        {
                                            dv.Value = invitation.Name;
                                            dv.Description = invitation.IncludedPackage.AsDelimited( ", " );

                                            rockContext.SaveChanges();

                                            dv.SetAttributeValue( "PMMPackageName", invitation.Id );
                                            dv.SaveAttributeValues( rockContext );

                                            existingInvitations.Add( invitation.Id );
                                        }
                                    }

                                    // New 
                                    foreach ( var invitation in invitations.Where( i => !existingInvitations.Contains( i.Id ) ) )
                                    {
                                        var dv = new DefinedValue
                                        {
                                            DefinedTypeId = definedType.Id,
                                            Value = invitation.Name,
                                            Description = invitation.IncludedPackage.AsDelimited( ", " ),
                                            IsActive = true,
                                        };
                                        dvService.Add( dv );
                                        rockContext.SaveChanges();

                                        dv.LoadAttributes( rockContext );

                                        dv.SetAttributeValue( "PMMPackageName", invitation.Id );
                                        dv.SaveAttributeValues( rockContext );
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        errorMessages.Add( "Could not load the Protect My Ministry 2.0 Component needed to query for packages." );
                    }
                }
                else
                {
                    errorMessages.Add( "Could not load the Protect My Ministry 2.0 Component needed to query for packages." );
                }
            }
            else
            {
                errorMessages.Add( "Could not find an active User Account needed to query for packages." );
            }
        
            nbPackageListError.Visible = errorMessages.Any();
            nbPackageListError.Text = "There was an error trying to retrieve the list of available packages for submitting requests to Protect My Ministry.<ul><li>" + errorMessages.AsDelimited( "</li><li>" ) + "</li></ul>";
        }


        /// <summary>
        /// Binds the package grid.
        /// </summary>
        public void BindPackageGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var definedType = DefinedTypeCache.Get( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_PACKAGES.AsGuid() );

                var definedValues = new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_PACKAGES.AsGuid() )
                    .ToList();

                foreach ( var definedValue in definedValues )
                {
                    definedValue.LoadAttributes( rockContext );
                }

                gTypes.DataSource = definedValues.Select( v => new
                {
                    v.Id,
                    PersonAttributes = GetPersonAttributeNames( v ),
                    v.Value,
                    v.Description,
                    v.IsActive
                } )
                .ToList();
                gTypes.DataBind();
            }
        }

        private string GetPersonAttributeNames( DefinedValue dv )
        {
            var names = new List<string>();
            AddAttributeName( names, dv.GetAttributeValue( "CheckedAttribute" ).AsGuidOrNull() );
            AddAttributeName( names, dv.GetAttributeValue( "DateAttribute" ).AsGuidOrNull() );
            AddAttributeName( names, dv.GetAttributeValue( "DocumentAttribute" ).AsGuidOrNull() );
            AddAttributeName( names, dv.GetAttributeValue( "ResultAttribute" ).AsGuidOrNull() );

            return names.AsDelimited( ", " );
        }

        private void AddAttributeName( List<string> names, Guid? attributeGuid )
        {
            if ( attributeGuid.HasValue )
            {
                var attr = AttributeCache.Get( attributeGuid.Value );
                if ( attr != null )
                {
                    names.Add( attr.Name );
                }
            }
        }

        /// <summary>
        /// Binds the user grid.
        /// </summary>
        public void BindUsersGrid()
        {
            using ( var rockContext = new RockContext() )
            {
                var definedValues = new DefinedValueService( rockContext )
                    .GetByDefinedTypeGuid( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_USERS.AsGuid() )
                    .ToList();

                foreach ( var definedValue in definedValues )
                {
                    definedValue.LoadAttributes( rockContext );
                }

                gUsers.DataSource = definedValues.Select( v => new
                {
                    v.Id,
                    v.Value,
                    v.Description,
                    Username = v.GetAttributeValue( "Username" ),
                    v.IsActive,
                } )
                .ToList();
                gUsers.DataBind();
            }
        }

        /// <summary>
        /// Shows the package edit.
        /// </summary>
        /// <param name="definedValueId">The defined value identifier.</param>
        public void ShowPackageEdit( int definedValueId )
        {
            var definedType = DefinedTypeCache.Get( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_PACKAGES.AsGuid() );
            if ( definedType != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    DefinedValue definedValue = new DefinedValueService( rockContext ).Get( definedValueId );
                    if ( definedValue != null )
                    {
                        hlPackageDefinedValueId.Value = definedValue.Id.ToString();
                        dlgPackage.Title = definedValue.Value;
                        lPackageDescription.Text = definedValue.Description;
                        cbPackageIsActive.Checked = definedValue.IsActive;

                        BindAttributeControls( rockContext );

                        definedValue.LoadAttributes();
                        ddlDateAttribute.SetValue( definedValue.GetAttributeValue( "DateAttribute" ).AsGuidOrNull() );
                        ddlResultAttribute.SetValue( definedValue.GetAttributeValue( "ResultAttribute" ).AsGuidOrNull() );
                        ddlDocumentAttribute.SetValue( definedValue.GetAttributeValue( "DocumentAttribute" ).AsGuidOrNull() );
                        ddlCheckedAttribute.SetValue( definedValue.GetAttributeValue( "CheckedAttribute" ).AsGuidOrNull() );

                        ShowDialog( "Package" );
                    }
                }
            }
        }

        public void BindAttributeControls( RockContext rockContext )
        {
            var personEntityTypeGuid =Rock.SystemGuid.EntityType.PERSON.AsGuid();
            var dateFieldTypeGuid = Rock.SystemGuid.FieldType.DATE.AsGuid();
            var singleSelectFieldTypeGuid = Rock.SystemGuid.FieldType.SINGLE_SELECT.AsGuid();
            var booleanFieldTypeGuid = Rock.SystemGuid.FieldType.BOOLEAN.AsGuid();
            var backgroundCheckFieldTypeGuid = Rock.SystemGuid.FieldType.BACKGROUNDCHECK.AsGuid();
            var FileFieldTypeGuid = Rock.SystemGuid.FieldType.FILE.AsGuid();

            var attributes = new AttributeService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a => a.EntityType.Guid == personEntityTypeGuid )
                .Select( a => new
                {
                    Value = a.Guid.ToString(),
                    Text = a.Name,
                    FieldTypeGuid = a.FieldType.Guid
                } )
                .OrderBy( a => a.Text )
                .ToList();

            ddlDateAttribute.Items.Clear();
            ddlDateAttribute.DataSource = attributes.Where( a => a.FieldTypeGuid == dateFieldTypeGuid );
            ddlDateAttribute.DataBind();
            ddlDateAttribute.Items.Insert( 0, new ListItem( "--Select Attribute--", "" ) );

            ddlResultAttribute.Items.Clear();
            ddlResultAttribute.DataSource = attributes.Where( a => a.FieldTypeGuid == singleSelectFieldTypeGuid );
            ddlResultAttribute.DataBind();
            ddlResultAttribute.Items.Insert( 0, new ListItem( "--Select Attribute--", "" ) );

            ddlDocumentAttribute.Items.Clear();
            ddlDocumentAttribute.DataSource = attributes.Where( a => a.FieldTypeGuid == backgroundCheckFieldTypeGuid || a.FieldTypeGuid == FileFieldTypeGuid );
            ddlDocumentAttribute.DataBind();
            ddlDocumentAttribute.Items.Insert( 0, new ListItem( "--Select Attribute--", "" ) );

            ddlCheckedAttribute.Items.Clear();
            ddlCheckedAttribute.DataSource = attributes.Where( a => a.FieldTypeGuid == booleanFieldTypeGuid );
            ddlCheckedAttribute.DataBind();
            ddlCheckedAttribute.Items.Insert( 0, new ListItem( "--Select Attribute--", "" ) );
        }

        public void ShowUserEdit( int definedValueId )
        {
            var definedType = DefinedTypeCache.Get( com.protectmyministry.SystemGuid.DefinedType.PROTECT_MY_MINISTRY_V2_USERS.AsGuid() );
            if ( definedType != null )
            {
                DefinedValue definedValue = null;
                if ( !definedValueId.Equals( 0 ) )
                {
                    definedValue = new DefinedValueService( new RockContext() ).Get( definedValueId );
                }

                if ( definedValue != null )
                {
                    hlUserDefinedValueId.Value = definedValue.Id.ToString();
                    dlgUser.Title = definedValue.Value;
                }
                else
                {
                    definedValue = new DefinedValue();
                    definedValue.DefinedTypeId = definedType.Id;
                    hlUserDefinedValueId.Value = string.Empty;
                    dlgUser.Title = "New User";
                }

                tbUserTitle.Text = definedValue.Value;
                tbUserDescription.Text = definedValue.Description;
                cbUserIsActive.Checked = definedValue.IsActive;

                definedValue.LoadAttributes();

                tbUserUsername.Text = definedValue.GetAttributeValue( "Username" );
                tbUserPassword.Text = Encryption.DecryptString( definedValue.GetAttributeValue( "Password" ) );

                ShowDialog( "User" );
            }
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        private void ShowDialog( string dialog )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog();
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        private void ShowDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "PACKAGE":
                    dlgPackage.Show();
                    break;
                case "USER":
                    dlgUser.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "PACKAGE":
                    dlgPackage.Hide();
                    break;
                case "USER":
                    dlgUser.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        /// <summary>
        /// Gets the settings.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private List<AttributeValue> GetSettings( RockContext rockContext )
        {
            var pmm2EntityType = EntityTypeCache.Get( typeof( com.protectmyministry.BackgroundCheck.ProtectMyMinistryV2 ) );
            if ( pmm2EntityType != null )
            {
                var service = new AttributeValueService( rockContext );
                return service.Queryable( "Attribute" )
                    .Where( v => v.Attribute.EntityTypeId == pmm2EntityType.Id )
                    .ToList();
            }

            return null;
        }

        /// <summary>
        /// Gets the setting value.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        private string GetSettingValue( List<AttributeValue> values, string key, bool encryptedValue = false )
        {
            string value = values
                .Where( v => v.AttributeKey == key )
                .Select( v => v.Value )
                .FirstOrDefault();
            if ( encryptedValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try { value = Encryption.DecryptString( value ); }
                catch { }
            }

            return value;
        }

        /// <summary>
        /// Sets the setting value.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="values">The values.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        private void SetSettingValue( RockContext rockContext, List<AttributeValue> values, string key, string value, bool encryptValue = false )
        {
            if ( encryptValue && !string.IsNullOrWhiteSpace( value ) )
            {
                try { value = Encryption.EncryptString( value ); }
                catch { }
            }

            var attributeValue = values
                .Where( v => v.AttributeKey == key )
                .FirstOrDefault();
            if ( attributeValue != null )
            {
                attributeValue.Value = value;
            }
            else
            {
                var pmm2EntityType = EntityTypeCache.Get( typeof( com.protectmyministry.BackgroundCheck.ProtectMyMinistryV2 ) );
                if ( pmm2EntityType != null )
                {
                    var attribute = new AttributeService( rockContext )
                        .Queryable()
                        .Where( a =>
                            a.EntityTypeId == pmm2EntityType.Id &&
                            a.Key == key
                        )
                        .FirstOrDefault();

                    if ( attribute != null )
                    {
                        attributeValue = new AttributeValue();
                        new AttributeValueService( rockContext ).Add( attributeValue );
                        attributeValue.AttributeId = attribute.Id;
                        attributeValue.Value = value;
                        attributeValue.EntityId = 0;
                    }
                }
            }

        }

        #endregion
    }
}
