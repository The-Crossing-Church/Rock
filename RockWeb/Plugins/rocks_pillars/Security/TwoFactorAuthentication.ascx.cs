//
// Copyright (C) Pillars Inc. - All Rights Reserved
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Hosting;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

using rocks.pillars.TwoFactorAuthentication.Security.Authentication;

namespace RockWeb.Plugins.rocks_pillars.Security
{
    /// <summary>
    /// Block used to download any scheduled payment transactions that were processed by payment gateway during a specified date range.
    /// </summary>
    [DisplayName( "Two Factor Authentication" )]
    [Category( "Pillars > Security" )]
    [Description( "Block used to authenticate user based on code that is sent through email or SMS." )]

    #region "Block Attributes"

    [CodeEditorField(
        "Select Message",
        Key = AttributeKeys.SelectMessage,
        Description = "Message to display when asking user to select how they would like to receive the code.",
        IsRequired = false,
        DefaultValue = "We need you to verify your identity. Please select how you would like to receive a one-time security code that you will need to enter on the next screen.",
        Order = 0 )]

    [CodeEditorField(
        "Code Message",
        Key = AttributeKeys.CodeMessage,
        Description = "Message to display when asking user to enter the code.",
        IsRequired = false,
        DefaultValue = "You should have received a one-time security code sent to your {{ MediumType }}. Please enter that code in the field below.",
        Order = 1 )]

    [CodeEditorField(
        "Email Subject",
        Key = AttributeKeys.EmailSubject,
        Description = "Contents of the email subject that contains the code.",
        IsRequired = false,
        DefaultValue = "Security Code",
        Order = 2 )]

    [CodeEditorField(
        "Email Content",
        Key = AttributeKeys.EmailContent,
        Description = "Contents of the email message that contains the code.",
        IsRequired = false,
        DefaultValue = "Here is your {{ 'Global' | Attribute:'OrganizationName' }} security code: {{ Code }}",
        Order = 3 )]

    [CodeEditorField(
        "Text Message Content",
        Key = AttributeKeys.SMSContent,
        Description = "Contents of the text message that contains the code.",
        IsRequired = false,
        DefaultValue = "Here is your {{ 'Global' | Attribute:'OrganizationName' }} security code: {{ Code }}",
        Order = 4 )]

    #endregion

    public partial class TwoFactorAuthentication : Rock.Web.UI.RockBlock
    {
        protected static class AttributeKeys
        {
            // Block
            public const string SelectMessage = "SelectMessage";
            public const string CodeMessage = "CodeMessage";
            public const string EmailSubject = "EmailSubject";
            public const string EmailContent = "EmailContent";
            public const string SMSContent = "SMSContent";

            // Component
            public const string SMSFromNumber = "SMSFromNumber";
            public const string CodeValidFor = "CodeValidFor";
            public const string AuthorizationValidFor = "AuthorizationValidFor";
            public const string MediumTypes = "MediumTypes";
        }

        private static string _2FAAttributeKey = "Pillars_2FACode";

        private RockContext rockContext = null;
        private UserLogin login = null;

        private Guid? fromGuid = null;
        private int codeValidFor = 300;
        private int authorizationValidFor = 30;
        private string mediumTypes = "Email,Phone";

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

            // Get Component Settings
            string databaseWith2faTypeName = ( typeof( rocks.pillars.TwoFactorAuthentication.Security.Authentication.DatabaseWith2FA ) ).FullName;
            var authComponent = AuthenticationContainer.Instance.Components.Values.FirstOrDefault( c => c.Value.TypeName == databaseWith2faTypeName );
            if ( authComponent != null )
            {
                fromGuid = authComponent.Value.GetAttributeValue( AttributeKeys.SMSFromNumber ).AsGuidOrNull();
                codeValidFor = authComponent.Value.GetAttributeValue( AttributeKeys.CodeValidFor ).AsInteger();
                authorizationValidFor = authComponent.Value.GetAttributeValue( AttributeKeys.AuthorizationValidFor ).AsInteger();
                mediumTypes = authComponent.Value.GetAttributeValue( AttributeKeys.MediumTypes );
            }

        }

        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbMessage.Visible = false;

            rockContext = new RockContext();
            login = GetLogin();

            if ( !Page.IsPostBack )
            {
                string encryptedCode = ValidateExistingToken();
                if ( encryptedCode.IsNotNullOrWhiteSpace() )
                {
                    RedirectToLogin( encryptedCode );
                }
                else
                {
                    ShowSelect();
                }
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
            ShowSelect();
        }

        protected void btnSelect_Click( object sender, EventArgs e )
        {
            ShowEntry();
        }

        protected void btnCancelSelect_Click( object sender, EventArgs e )
        {
            RedirectToLogin();
        }

        protected void btnLogin_Click( object sender, EventArgs e )
        {
            if ( login != null )
            {
                var tokenList = new List<_2FAToken>();
                tokenList.FromEncryptedString( login.Person.GetAttributeValue( _2FAAttributeKey ) );

                var token = tokenList.FirstOrDefault( t => t.Code == tbCode.Text.AsInteger() );

                if ( token != null && token.CreatedDateTime.HasValue && token.CreatedDateTime.Value.AddDays( codeValidFor ) >= RockDateTime.Now )
                {
                    tokenList.Remove( token );

                    var newToken = ( new _2FAToken( GenerateCode(), RockDateTime.Now ) );
                    tokenList.Add( newToken );

                    string encryptedTokens = tokenList.EncryptedString();
                    string encryptedToken = newToken.EncryptedString();

                    login.Person.SetAttributeValue( _2FAAttributeKey, encryptedTokens );
                    login.Person.SaveAttributeValue( _2FAAttributeKey, rockContext );

                    var tokenCookie = new HttpCookie( _2FAAttributeKey )
                    {
                        Value = encryptedToken,
                        Expires = RockDateTime.Now.AddDays( authorizationValidFor )
                    };

                    Response.Cookies.Remove( _2FAAttributeKey );
                    Response.Cookies.Add( tokenCookie );

                    RedirectToLogin( encryptedToken );
                }
                else
                {
                    ShowError( "The code you entered is not correct or has expired. Please click 'Cancel' to return to previous screen to re-send a new code." );
                }
            }
        }

        protected void btnCancelLogin_Click( object sender, EventArgs e )
        {
            if ( rblMediums.Items.Count == 1 )
            {
                RedirectToLogin();
            }
            else
            {
                ShowSelect();
            }
        }

        #endregion

        #region Methods

        private string ValidateExistingToken()
        {
            if ( Request.Cookies[_2FAAttributeKey] != null && login != null && login.Person != null )
            {
                var tokenList = new List<_2FAToken>();
                tokenList.FromEncryptedString( login.Person.GetAttributeValue( _2FAAttributeKey ) );

                var savedTokenCount = tokenList.Count;

                foreach ( var token in tokenList.ToList() )
                {
                    if ( token.CreatedDateTime.HasValue && token.CreatedDateTime.Value.AddDays( authorizationValidFor ) >= RockDateTime.Now )
                    {
                        string encryptedCookieToken = Request.Cookies[_2FAAttributeKey].Value;
                        var cookieToken = new _2FAToken( Encryption.DecryptString( encryptedCookieToken ) );

                        if ( token.Code == cookieToken.Code )
                        {
                            return encryptedCookieToken;
                        }
                    }
                    else
                    {
                        tokenList.Remove( token );
                    }
                }

                if ( tokenList.Count != savedTokenCount )
                {
                    login.Person.SetAttributeValue( _2FAAttributeKey, tokenList.EncryptedString() );
                    login.Person.SaveAttributeValue( _2FAAttributeKey, rockContext );
                }
            }

            return string.Empty;
        }

        private void ShowSelect()
        {
            rblMediums.Items.Clear();

            if ( login != null )
            {
                if ( mediumTypes.Contains( "Email" ) && login.Person.Email.IsNotNullOrWhiteSpace() )
                {
                    rblMediums.Items.Add( new ListItem( MaskEmail( login.Person.Email ), "email" ) );
                }

                if ( mediumTypes.Contains( "Phone" ) && fromGuid.HasValue )
                {
                    var pn = login.Person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                    if ( pn != null && pn.IsMessagingEnabled )
                    {
                        rblMediums.Items.Add( new ListItem( MaskPhone( pn.NumberFormatted ), "phone" ) );
                    }
                }
            }

            pnlSelect.Visible = true;
            pnlEnterCode.Visible = false;

            if ( rblMediums.Items.Count == 0 )
            {
                ShowError( "Could not find a valid email address or phone number associated with your account." );
                rblMediums.Visible = false;
                btnSelect.Visible = false;
            }
            else if ( rblMediums.Items.Count == 1 )
            {
                rblMediums.Items[0].Selected = true;
                ShowEntry();
            }
            else
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                mergeFields.Add( "MediumType", rblMediums.SelectedValue );
                ShowInfo( GetAttributeValue( AttributeKeys.SelectMessage ).ResolveMergeFields( mergeFields ) );
                rblMediums.Visible = true;
                btnSelect.Visible = true;
            }

        }

        private void ShowEntry()
        {
            HostingEnvironment.QueueBackgroundWorkItem( ct => SendCode() );

            var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
            mergeFields.Add( "MediumType", rblMediums.SelectedValue );
            ShowInfo( GetAttributeValue( AttributeKeys.CodeMessage ).ResolveMergeFields( mergeFields ) );

            pnlSelect.Visible = false;
            pnlEnterCode.Visible = true;

            tbCode.Text = string.Empty;
            tbCode.Attributes["autocomplete"] = "one-time-code";
            tbCode.Focus();
        }

        private void SendCode()
        {
            try
            {
                if ( login != null )
                {
                    var tokenList = new List<_2FAToken>();
                    tokenList.FromEncryptedString( login.Person.GetAttributeValue( _2FAAttributeKey ) );

                    var newToken = ( new _2FAToken( GenerateCode(), RockDateTime.Now ) );
                    tokenList.Add( newToken );

                    login.Person.SetAttributeValue( _2FAAttributeKey, tokenList.EncryptedString() );
                    login.Person.SaveAttributeValue( _2FAAttributeKey, rockContext );

                    // Send Code to Person
                    var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                    mergeFields.Add( "Person", login.Person );
                    mergeFields.Add( "Code", newToken.Code );

                    RockMessage rockMessage = null;
                    RockMessageRecipient recipient = null;
                    if ( rblMediums.SelectedValue == "email" )
                    {
                        var emailMessage = new RockEmailMessage
                        {
                            Subject = GetAttributeValue( AttributeKeys.EmailSubject ),
                            Message = GetAttributeValue( AttributeKeys.EmailContent )
                        };
                        rockMessage = emailMessage;
                        recipient = new RockEmailMessageRecipient( login.Person, mergeFields );
                    }
                    else
                    {
                        if ( fromGuid.HasValue )
                        {
                            var smsMessage = new RockSMSMessage
                            {
                                FromSystemPhoneNumber = SystemPhoneNumberCache.Get( fromGuid.Value, rockContext ),
                                Message = GetAttributeValue( AttributeKeys.SMSContent )
                            };
                            rockMessage = smsMessage;

                            var pn = login.Person.GetPhoneNumber( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() );
                            if ( pn != null && pn.IsMessagingEnabled )
                            {
                                string smsNumber = string.Format( "+{0}{1}", pn.CountryCode, pn.Number );
                                recipient = new RockSMSMessageRecipient( login.Person, smsNumber, mergeFields );
                            }
                        }
                    }

                    rockMessage.AddRecipient( recipient );
                    rockMessage.AppRoot = Rock.Web.Cache.GlobalAttributesCache.Get().GetValue( "InternalApplicationRoot" ) ?? string.Empty;
                    rockMessage.CreateCommunicationRecord = true;
                    rockMessage.Send();
                }
            }

            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, null );

                nbMessage.NotificationBoxType = NotificationBoxType.Danger;
                nbMessage.Title = "The following error occurred when attempting to send code:";
                nbMessage.Text = ex.Message;
                nbMessage.Visible = true;
            }
        }

        private UserLogin GetLogin()
        {
            var userGuid = Encryption.DecryptString( System.Web.HttpUtility.UrlDecode( PageParameter( "user" ).Replace( "!", "%" ) ) ).AsGuidOrNull();
            if ( userGuid.HasValue )
            {
                var userLogin = new UserLoginService( rockContext ).Get( userGuid.Value );
                if ( userLogin != null && userLogin.Person != null )
                {
                    userLogin.Person.LoadAttributes( rockContext );
                    return userLogin;
                }
            }

            return null;
        }

        private int GenerateCode()
        {
            int _min = 100000;
            int _max = 999999;
            Random _rdm = new Random();
            return _rdm.Next( _min, _max );
        }

        private void RedirectToLogin( string encryptedCode = "" )
        {
            string loginUrl = PageParameter( "redirectUri" );

            if ( encryptedCode.IsNotNullOrWhiteSpace() )
            {
                if ( !loginUrl.Contains( "code" ) )
                {
                    string encodedCode = HttpUtility.UrlEncode( encryptedCode ).Replace( "%", "!" );
                    string delimiter = loginUrl.Contains( '?' ) ? "&" : "?";
                    loginUrl += delimiter + "code=" + encodedCode;
                }

                string user = Request.QueryString["user"];
                if ( user.IsNotNullOrWhiteSpace() && !loginUrl.Contains( "user" ) )
                {
                    string delimiter = loginUrl.Contains( '?' ) ? "&" : "?";
                    loginUrl += delimiter + "user=" + user;
                }
            }

            string returnUrl = Request.QueryString["returnurl"];
            if ( returnUrl.IsNotNullOrWhiteSpace() && !loginUrl.Contains( "returnurl" ) )
            {
                string delimiter = loginUrl.Contains( '?' ) ? "&" : "?";
                loginUrl += delimiter + "returnurl=" + returnUrl;
            }

            Response.Redirect( loginUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        private void ShowInfo( string msg )
        {
            ShowMessage( msg, NotificationBoxType.Info );
        }

        private void ShowError( string msg )
        {
            ShowMessage( msg, NotificationBoxType.Danger );
        }

        private void ShowMessage( string msg, NotificationBoxType msgType )
        {
            if ( msg.IsNotNullOrWhiteSpace() )
            {
                nbMessage.NotificationBoxType = msgType;
                nbMessage.Text = msg;
                nbMessage.Visible = true;
            }
            else
            {
                nbMessage.Visible = false;
            }
        }

        private string MaskEmail( string s )
        {
            string pattern = @"(?<=[\w]{1})[\w-\._\+%\\]*(?=[\w]{1}@)|(?<=@[\w]{1})[\w-_\+%]*(?=\.)";

            if ( !s.Contains( "@" ) )
            {
                return new String( '*', s.Length );
            }

            if ( s.Split( '@' )[0].Length < 4 )
            {
                return @"*@*.*";
            }

            return Regex.Replace( s, pattern, m => new string( '*', m.Length ) );
        }

        private string MaskPhone( string s )
        {
            string pattern = @"\d(?!\d{0,3}$)";
            return Regex.Replace( s, pattern, m => new string( '*', m.Length ) );
        }

        #endregion



    }
}