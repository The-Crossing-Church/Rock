
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text;
using Rock.Communication;
using System.Threading.Tasks;
using System.Data.Entity;

namespace RockWeb.Plugins.rocks_pillars.Communication
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Simple SMS Entry" )]
    [Category( "Pillars > Communication" )]
    [Description( "Sends an SMS messages to one or more people." )]

    [DefinedValueField(Rock.SystemGuid.DefinedType.COMMUNICATION_SMS_FROM, "From Number", "The SMS Number to send message from.", true, false, "", "", 0  )]
    [BooleanField("Update From Number", "Should the From Number's Response Recipient be updated to current person any time a message is sent?", false, "", 1)]
    [TextField("Default Message","The initial default message to display.", false, "", "", 2)]
    public partial class SimpleSMSEntry : Rock.Web.UI.RockBlock
    {
        #region Fields

        private int _recipientCount;
        private int _numbersSelectedCount;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the recipients.
        /// </summary>
        /// <value>
        /// The recipient ids.
        /// </value>
        protected List<Recipient> Recipients
        {
            get
            {
                var recipients = ViewState["Recipients"] as List<Recipient>;
                if ( recipients == null )
                {
                    recipients = new List<Recipient>();
                    ViewState["Recipients"] = recipients;
                }
                return recipients;
            }

            set { ViewState["Recipients"] = value; }
        }

        #endregion

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

            nbInvalidPerson.Visible = false;

            if ( !Page.IsPostBack )
            {
                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
                tbMessage.Text = GetAttributeValue( "DefaultMessage" ).ResolveMergeFields(commonMergeFields);
                BindRecipients();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            BindRecipients();
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
            BindRecipients();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppAddPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void ppAddPerson_SelectPerson( object sender, EventArgs e )
        {
            if ( ppAddPerson.PersonId.HasValue )
            {
                if ( !Recipients.Any( r => r.PersonId == ppAddPerson.PersonId.Value ) )
                {
                    var context = new RockContext();

                    var Person = new PersonService( context ).Get( ppAddPerson.PersonId.Value );
                    if ( Person != null )
                    {
                        int mobileNumberTypeValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.PERSON_PHONE_TYPE_MOBILE.AsGuid() ).Id;
                        var pn = Person.PhoneNumbers.FirstOrDefault( n => n.NumberTypeValueId == mobileNumberTypeValueId && n.IsMessagingEnabled );

                        if ( pn != null )
                        {
                            Recipients.Add( new Recipient
                            {
                                PersonId = Person.Id,
                                PersonName = Person.FullName,
                                PhoneNumber = pn.Number
                            } );
                        }
                        else
                        {
                            nbInvalidPerson.Text = string.Format( "{0} does not have a mobile phone numer with SMS enabled.", Person.NickName );
                            nbInvalidPerson.Visible = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptRecipients control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs" /> instance containing the event data.</param>
        protected void rptRecipients_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int personId = int.MinValue;
            if ( int.TryParse( e.CommandArgument.ToString(), out personId ) )
            {
                Recipients = Recipients.Where( r => r.PersonId != personId ).ToList();
            }
        }

        /// <summary>
        /// Handles the ServerValidate event of the valRecipients control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="ServerValidateEventArgs" /> instance containing the event data.</param>
        protected void valRecipients_ServerValidate( object source, ServerValidateEventArgs args )
        {
            args.IsValid = Recipients.Any();
        }

        /// <summary>
        /// Handles the NextClick event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_NextClick( object sender, EventArgs e )
        {
            ProcessRecipientCountAndDuration();
            pnlConfirm.Visible = true;
            pnlCompose.Visible = false;
            nbMessageSent.Visible = false;
        }

        /// <summary>
        /// Handles the BackClick event of the btnBack control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnBack_BackClick( object sender, EventArgs e )
        {
            pnlConfirm.Visible = false;
            pnlCompose.Visible = true;
            nbMessageSent.Visible = false;
        }

        /// <summary>
        /// Handles the SendClick event of the btnSend control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSend_SendClick( object sender, EventArgs e )
        {
            DefinedValue dv = null;
            using ( var rockContext = new RockContext() )
            {
                dv = new DefinedValueService( rockContext ).Get( GetAttributeValue( "FromNumber" ).AsGuid() );

                if ( GetAttributeValue( "UpdateFromNumber" ).AsBoolean() && dv != null && CurrentPerson != null )
                {
                    dv.LoadAttributes( rockContext );
                    dv.SetAttributeValue( "ResponseRecipient", CurrentPerson.PrimaryAlias.Guid );
                    dv.SaveAttributeValues( rockContext );
                }

                if ( dv != null )
                {
                    var communicationService = new CommunicationService( rockContext );
                    var personService = new PersonService( rockContext );

                    var comm = new Rock.Model.Communication();
                    comm.Status = CommunicationStatus.Approved;
                    comm.ReviewedDateTime = RockDateTime.Now;
                    comm.ReviewerPersonAliasId = CurrentPersonAliasId.Value;
                    comm.SenderPersonAliasId = CurrentPersonAliasId;
                    communicationService.Add( comm );

                    comm.IsBulkCommunication = false;
                    comm.CommunicationType = CommunicationType.SMS;
                    comm.FutureSendDateTime = null;

                    comm.SMSFromDefinedValue = dv;
                    comm.SMSMessage = tbMessage.Text;

                    var smsMediumTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id;

                    foreach ( var personId in Recipients.Select( r => r.PersonId ).ToList() )
                    {
                        var person = personService.Get( personId );
                        if ( person != null )
                        {
                            var commRecipient = new CommunicationRecipient();
                            commRecipient.PersonAlias = person.PrimaryAlias;
                            commRecipient.MediumEntityTypeId = smsMediumTypeId;
                            comm.Recipients.Add( commRecipient );
                        }
                    }

                    rockContext.SaveChanges();

                    var transaction = new Rock.Transactions.SendCommunicationTransaction
                    {
                        CommunicationId = comm.Id,
                        PersonAlias = CurrentPersonAlias
                    };
                    Rock.Transactions.RockQueue.TransactionQueue.Enqueue( transaction );

                    pnlCompose.Visible = false;
                    pnlConfirm.Visible = false;
                    nbMessageSent.Visible = true;

                    nbMessageSent.Text = "Message sent.";
                    nbMessageSent.NotificationBoxType = NotificationBoxType.Success;
                }
                else
                {
                    nbMessageSent.Text = "Could not send message. Invalid From Number.";
                    nbMessageSent.NotificationBoxType = NotificationBoxType.Danger;

                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Builds the controls.
        /// </summary>
        private void BindRecipients()
        {
            ppAddPerson.PersonId = Rock.Constants.None.Id;
            ppAddPerson.PersonName = "Add Person";

            rptRecipients.DataSource = Recipients.ToList();
            rptRecipients.DataBind();
        }

        /// <summary>
        /// Processes the duration and count of the recipients.
        /// </summary>
        private void ProcessRecipientCountAndDuration()
        {
            var mediumComponent = MediumContainer.GetComponentByEntityTypeId( EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id );
            if ( mediumComponent == null || ( mediumComponent != null && mediumComponent.IsActive == false ) )
            {
                nbConfirm.NotificationBoxType = NotificationBoxType.Warning;
                nbConfirm.Text = "No SMS components were found.  Please make sure SMS is enabled.";

                btnSend.Visible = false;
            }
            else 
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine( "<b>" + Recipients.Count().ToString() + "</b> Recipients will be sent this message." );
                sb.AppendLine( "<br/><br/>" );
                sb.AppendLine( "<b>Message</b><br/>" );
                sb.AppendLine( tbMessage.Text );
                nbConfirm.NotificationBoxType = NotificationBoxType.Info;
                nbConfirm.Text = sb.ToString();

                btnSend.Visible = true;
            }
        }

        #endregion

        /// <summary>
        /// The Recipient class.
        /// </summary>
        [Serializable]
        public class Recipient
        {
            /// <summary>
            /// Gets or sets the batch identifier.
            /// </summary>
            /// <value>
            /// The batch identifier.
            /// </value>
            public int PersonId { get; set; }

            /// <summary>
            /// Gets or sets the name of the person.
            /// </summary>
            /// <value>
            /// The name of the person.
            /// </value>
            public string PersonName { get; set; }

            /// <summary>
            /// Gets or sets the person identifier.
            /// </summary>
            /// <value>
            /// The person identifier.
            /// </value>
            public string PhoneNumber { get; set; }
        }
    }


}