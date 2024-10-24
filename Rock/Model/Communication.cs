﻿// <copyright>
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
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

using Rock.Communication;
using Rock.Data;
using Rock.Utility;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents a communication in Rock (i.e. email, SMS message, etc.).
    /// </summary>
    [RockDomain( "Communication" )]
    [Table( "Communication" )]
    [DataContract]
    public partial class Communication : Model<Communication>, ICommunicationDetails
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the communication type value identifier.
        /// </summary>
        /// <value>
        /// The communication type value identifier.
        /// </value>
        [Required]
        [DataMember]
        public CommunicationType CommunicationType { get; set; }

        /// <summary>
        /// Gets or sets the URL from where this communication was created (grid)
        /// </summary>
        /// <value>
        /// The URL referrer.
        /// </value>
        [DataMember]
        [MaxLength( 200 )]
        public string UrlReferrer { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.Group">list</see> that email is being sent to.
        /// </summary>
        /// <value>
        /// The list group identifier.
        /// </value>
        [DataMember]
        public int? ListGroupId { get; set; }

        /// <summary>
        /// Gets or sets the segments that list is being filtered to (comma-delimited list of dataview guids).
        /// </summary>
        /// <value>
        /// The segments.
        /// </value>
        [DataMember]
        public string Segments { get; set; }

        /// <summary>
        /// Gets or sets if communication is targeted to people in all selected segments or any selected segments.
        /// </summary>
        /// <value>
        /// The segment criteria.
        /// </value>
        [DataMember]
        public SegmentCriteria SegmentCriteria { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.CommunicationTemplate"/> that was used to compose this communication
        /// </summary>
        /// <value>
        /// The communication template identifier.
        /// </value>
        /// <remarks>
        /// [IgnoreCanDelete] since there is a ON DELETE SET NULL cascade on this
        /// </remarks>
        [IgnoreCanDelete]
        public int? CommunicationTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the sender <see cref="Rock.Model.PersonAlias"/> identifier.
        /// </summary>
        /// <value>
        /// The sender person alias identifier.
        /// </value>
        [DataMember]
        public int? SenderPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the is bulk communication.
        /// </summary>
        /// <value>
        /// The is bulk communication.
        /// </value>
        [DataMember]
        public bool IsBulkCommunication { get; set; }

        /// <summary>
        /// Gets or sets the datetime that communication was sent. This also indicates that communication shouldn't attempt to send again.
        /// </summary>
        /// <value>
        /// The send date time.
        /// </value>
        [DataMember]
        public DateTime? SendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the future send date for the communication. This allows a user to schedule when a communication is sent 
        /// and the communication will not be sent until that date and time.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> value that represents the FutureSendDate for the communication.  If no future send date is provided, this value will be null.
        /// </value>
        [DataMember]
        public DateTime? FutureSendDateTime { get; set; }

        /// <summary>
        /// Gets or sets the status of the Communication.
        /// </summary>
        /// <value>
        /// A <see cref="Rock.Model.CommunicationStatus"/> enum value that represents the status of the Communication.
        /// </value>
        [DataMember]
        public CommunicationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the reviewer person alias identifier.
        /// </summary>
        /// <value>
        /// The reviewer person alias identifier.
        /// </value>
        [DataMember]
        public int? ReviewerPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the date and time stamp of when the Communication was reviewed.
        /// </summary>
        /// <value>
        /// A <see cref="System.DateTime"/> representing the date and time that the Communication was reviewed.
        /// </value>
        [DataMember]
        public DateTime? ReviewedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the note that was entered by the reviewer.
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> representing a note that was entered by the reviewer.
        /// </value>
        [DataMember]
        public string ReviewerNote { get; set; }

        /// <summary>
        /// Gets or sets a JSON formatted string containing the Medium specific data.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any Medium specific data.
        /// </value>
        [DataMember]
        [RockObsolete( "1.7" )]
        [Obsolete( "MediumDataJson is no longer used.", true )]
        public string MediumDataJson { get; set; }

        /// <summary>
        /// Gets or sets a JSON string containing any additional merge fields for the Communication.
        /// </summary>
        /// <value>
        /// A Json formatted <see cref="System.String"/> that contains any additional merge fields for the Communication.
        /// </value>
        [DataMember]
        public string AdditionalMergeFieldsJson
        {
            get
            {
                return AdditionalMergeFields.ToJson();
            }

            set
            {
                AdditionalMergeFields = value.FromJsonOrNull<List<string>>() ?? new List<string>();
            }
        }

        /// <summary>
        /// Gets or sets a comma-delimited list of enabled LavaCommands
        /// </summary>
        /// <value>
        /// The enabled lava commands.
        /// </value>
        [DataMember]
        public string EnabledLavaCommands { get; set; }

        /// <summary>
        /// Gets the send date key.
        /// </summary>
        /// <value>
        /// The send date key.
        /// </value>
        [DataMember]
        [FieldType( Rock.SystemGuid.FieldType.DATE )]
        public int? SendDateKey
        {
            get => ( SendDateTime == null || SendDateTime.Value == default ) ?
                        ( int? ) null :
                        SendDateTime.Value.ToString( "yyyyMMdd" ).AsInteger();

            private set
            {
            }
        }

        #region Email Fields

        /// <summary>
        /// Gets or sets the name of the Communication
        /// </summary>
        /// <value>
        /// A <see cref="System.String"/> that represents the name of the communication.
        /// </value>
        [DataMember]
        [MaxLength( 1000 )]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets from name.
        /// </summary>
        /// <value>
        /// From name.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FromName { get; set; }

        /// <summary>
        /// Gets or sets from email address.
        /// </summary>
        /// <value>
        /// From email address.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string FromEmail { get; set; }

        /// <summary>
        /// Gets or sets the reply to email address.
        /// </summary>
        /// <value>
        /// The reply to email address.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string ReplyToEmail { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list of CC'ed email addresses.
        /// </summary>
        /// <value>
        /// A comma separated list of CC'ed email addresses.
        /// </value>
        [DataMember]
        public string CCEmails { get; set; }

        /// <summary>
        /// Gets or sets a comma separated list of BCC'ed email addresses.
        /// </summary>
        /// <value>
        /// A comma separated list of BCC'ed email addresses.
        /// </value>
        [DataMember]
        public string BCCEmails { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the message meta data.
        /// </summary>
        /// <value>
        /// The message meta data.
        /// </value>
        [DataMember]
        public string MessageMetaData { get; set; }

        #endregion

        #region SMS Properties

        /// <summary>
        /// Gets or sets the SMS from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        [DataMember]
        public int? SMSFromDefinedValueId { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string SMSMessage { get; set; }

        #endregion

        #region Push Notification Properties

        /// <summary>
        /// Gets or sets the push notification title.
        /// </summary>
        /// <value>
        /// Push notification title.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushTitle { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        [DataMember]
        public string PushMessage { get; set; }

        /// <summary>
        /// Gets or sets push sound.
        /// </summary>
        /// <value>
        /// Push sound.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string PushSound { get; set; }

        /// <summary>
        /// Gets or sets the push <see cref="Rock.Model.BinaryFile">image file</see> identifier.
        /// </summary>
        /// <value>
        /// The push image file identifier.
        /// </value>
        [DataMember]
        public int? PushImageBinaryFileId { get; set; }

        /// <summary>
        /// Gets or sets the push open action.
        /// </summary>
        /// <value>
        /// The push open action.
        /// </value>
        [DataMember]
        public PushOpenAction? PushOpenAction { get; set; }

        /// <summary>
        /// Gets or sets the push open message.
        /// </summary>
        /// <value>
        /// The push open message.
        /// </value>
        [DataMember]
        public string PushOpenMessage { get; set; }

        /// <summary>
        /// Gets or sets the push data.
        /// </summary>
        /// <value>
        /// The push data.
        /// </value>
        [DataMember]
        public string PushData { get; set; }
        #endregion

        /// <summary>
        /// Option to prevent communications from being sent to people with the same email/SMS addresses.
        /// This will mean two people who share an address will not receive a personalized communication, only one of them will.
        /// </summary>
        /// <value>
        ///   <c>true</c> if [exclude duplicate recipient address]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool ExcludeDuplicateRecipientAddress { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SystemCommunication"/> that this communication is associated with.
        /// </summary>
        /// <value>
        /// The system communication.
        /// </value>
        [DataMember]
        [IgnoreCanDelete]
        public int? SystemCommunicationId { get; set; }

        #endregion

        #region Virtual Properties

        /// <summary>
        /// Gets or sets the list <see cref="Rock.Model.Group" />.
        /// </summary>
        /// <value>
        /// The list group.
        /// </value>
        [DataMember]
        public virtual Group ListGroup { get; set; }

        /// <summary>
        /// Gets or sets the sender <see cref="Rock.Model.PersonAlias" />.
        /// </summary>
        /// <value>
        /// The sender person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias SenderPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets the reviewer <see cref="Rock.Model.PersonAlias" />.
        /// </summary>
        /// <value>
        /// The reviewer person alias.
        /// </value>
        [DataMember]
        public virtual PersonAlias ReviewerPersonAlias { get; set; }

        /// <summary>
        /// Gets or sets a collection containing the <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> for the Communication.
        /// </summary>
        /// <value>
        /// The <see cref="Rock.Model.CommunicationRecipient">CommunicationRecipients</see> of the Communication.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationRecipient> Recipients
        {
            get
            {
                return _recipients ?? ( _recipients = new Collection<CommunicationRecipient>() );
            }

            set
            {
                _recipients = value;
            }
        }

        private ICollection<CommunicationRecipient> _recipients;

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.CommunicationAttachment">attachments</see>.
        /// NOTE: In most cases, you should use GetAttachments( CommunicationType ) instead.
        /// </summary>
        /// <value>
        /// The attachments.
        /// </value>
        [DataMember]
        public virtual ICollection<CommunicationAttachment> Attachments
        {
            get
            {
                return _attachments ?? ( _attachments = new Collection<CommunicationAttachment>() );
            }

            set
            {
                _attachments = value;
            }
        }

        private ICollection<CommunicationAttachment> _attachments;

        /// <summary>
        /// Gets or sets the additional merge field list. When a communication is created
        /// from a grid, the grid may add additional merge fields that will be available
        /// for the communication.
        /// </summary>
        /// <value>
        /// A <see cref="System.Collections.Generic.List{String}"/> of values containing the additional merge field list.
        /// </value>
        [DataMember]
        public virtual List<string> AdditionalMergeFields
        {
            get
            {
                return _additionalMergeFields;
            }

            set
            {
                _additionalMergeFields = value;
            }
        }

        private List<string> _additionalMergeFields = new List<string>();

        /// <summary>
        /// Gets or sets the SMS from defined value.
        /// </summary>
        /// <value>
        /// The SMS from defined value.
        /// </value>
        [DataMember]
        public virtual DefinedValue SMSFromDefinedValue { get; set; }

        /// <summary>
        /// Gets or sets a list of email binary file ids
        /// </summary>
        /// <value>
        /// The attachment binary file ids
        /// </value>
        [NotMapped]
        public virtual IEnumerable<int> EmailAttachmentBinaryFileIds
        {
            get
            {
                return this.Attachments.Where( a => a.CommunicationType == CommunicationType.Email ).Select( a => a.BinaryFileId ).ToList();
            }
        }

        /// <summary>
        /// Gets or sets a list of sms binary file ids
        /// </summary>
        /// <value>
        /// The attachment binary file ids
        /// </value>
        [NotMapped]
        public virtual IEnumerable<int> SMSAttachmentBinaryFileIds
        {
            get
            {
                return this.Attachments.Where( a => a.CommunicationType == CommunicationType.SMS ).Select( a => a.BinaryFileId ).ToList();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.CommunicationTemplate"/> that was used to compose this communication
        /// </summary>
        /// <value>
        /// The communication template.
        /// </value>
        [DataMember]
        public virtual CommunicationTemplate CommunicationTemplate { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Rock.Model.AnalyticsSourceDate">send source date</see>.
        /// </summary>
        /// <value>
        /// The send source date.
        /// </value>
        [DataMember]
        public virtual AnalyticsSourceDate SendSourceDate { get; set; }

        /// <inheritdoc cref="SystemCommunicationId"/>
        [DataMember]
        public virtual SystemCommunication SystemCommunication { get; set;  }

        #endregion

        #region ISecured

        /// <summary>
        /// A parent authority.  If a user is not specifically allowed or denied access to
        /// this object, Rock will check the default authorization on the current type, and
        /// then the authorization on the Rock.Security.GlobalDefault entity
        /// </summary>
        public override Security.ISecured ParentAuthority
        {
            get
            {
                if ( this.CommunicationTemplate != null )
                {
                    return this.CommunicationTemplate;
                }

                if ( this.SystemCommunication != null )
                {
                    return this.SystemCommunication;
                }

                return base.ParentAuthority;
            }
        }

        #endregion 

        #region Public Methods

        /// <summary>
        /// Gets the <see cref="Rock.Communication.MediumComponent" /> for the communication medium that is being used.
        /// </summary>
        /// <returns></returns>
        /// <value>
        /// The <see cref="Rock.Communication.MediumComponent" /> for the communication medium that is being used.
        /// </value>
        public virtual List<MediumComponent> GetMediums()
        {
            var mediums = new List<MediumComponent>();

            foreach ( var serviceEntry in MediumContainer.Instance.Components )
            {
                var component = serviceEntry.Value.Value;
                if ( component.IsActive &&
                    ( this.CommunicationType == component.CommunicationType ||
                        this.CommunicationType == CommunicationType.RecipientPreference ) )
                {
                    mediums.Add( component );
                }
            }

            return mediums;
        }

        /// <summary>
        /// Adds the attachment.
        /// </summary>
        /// <param name="communicationAttachment">The communication attachment.</param>
        /// <param name="communicationType">Type of the communication.</param>
        public void AddAttachment( CommunicationAttachment communicationAttachment, CommunicationType communicationType )
        {
            communicationAttachment.CommunicationType = communicationType;
            this.Attachments.Add( communicationAttachment );
        }

        /// <summary>
        /// Gets the attachments.
        /// Specify CommunicationType.Email to get the attachments for Email and CommunicationType.SMS to get the Attachment(s) for SMS
        /// </summary>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns></returns>
        public IEnumerable<CommunicationAttachment> GetAttachments( CommunicationType communicationType )
        {
            return this.Attachments.Where( a => a.CommunicationType == communicationType );
        }

        /// <summary>
        /// Gets the attachment <see cref="Rock.Model.BinaryFile" /> ids.
        /// Specify CommunicationType.Email to get the attachments for Email and CommunicationType.SMS to get the Attachment(s) for SMS
        /// </summary>
        /// <param name="communicationType">Type of the communication.</param>
        /// <returns></returns>
        public List<int> GetAttachmentBinaryFileIds( CommunicationType communicationType )
        {
            return this.GetAttachments( communicationType ).Select( a => a.BinaryFileId ).ToList();
        }

        /// <summary>
        /// Returns true if this communication has any pending recipients
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public bool HasPendingRecipients( RockContext rockContext )
        {
            return new CommunicationRecipientService( rockContext ).Queryable().Where( a => a.CommunicationId == this.Id && a.Status == Model.CommunicationRecipientStatus.Pending ).Any();
        }

        /// <summary>
        /// Returns a queryable of the Recipients for this communication. Note that this will return the recipients that have been saved to the database. Any pending changes in the Recipients property are not included.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IQueryable<CommunicationRecipient> GetRecipientsQry( RockContext rockContext )
        {
            return new CommunicationRecipientService( rockContext ).Queryable().Where( a => a.CommunicationId == this.Id );
        }

        /// <summary>
        /// Gets the communication list members.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="listGroupId">The list group identifier.</param>
        /// <param name="segmentCriteria">The segment criteria.</param>
        /// <param name="segmentDataViewIds">The segment data view ids.</param>
        /// <returns></returns>
        public static IQueryable<GroupMember> GetCommunicationListMembers( RockContext rockContext, int? listGroupId, SegmentCriteria segmentCriteria, List<int> segmentDataViewIds )
        {
            IQueryable<GroupMember> groupMemberQuery = null;
            if ( listGroupId.HasValue )
            {
                var groupMemberService = new GroupMemberService( rockContext );
                var personService = new PersonService( rockContext );
                var dataViewService = new DataViewService( rockContext );

                groupMemberQuery = groupMemberService.Queryable().Where( a => a.GroupId == listGroupId.Value && a.GroupMemberStatus == GroupMemberStatus.Active );

                Expression segmentExpression = null;
                ParameterExpression paramExpression = personService.ParameterExpression;
                var segmentDataViewList = dataViewService.GetByIds( segmentDataViewIds ).AsNoTracking().ToList();
                foreach ( var segmentDataView in segmentDataViewList )
                {
                    var exp = segmentDataView.GetExpression( personService, paramExpression );
                    if ( exp != null )
                    {
                        if ( segmentExpression == null )
                        {
                            segmentExpression = exp;
                        }
                        else
                        {
                            if ( segmentCriteria == SegmentCriteria.All )
                            {
                                segmentExpression = Expression.AndAlso( segmentExpression, exp );
                            }
                            else
                            {
                                segmentExpression = Expression.OrElse( segmentExpression, exp );
                            }
                        }
                    }
                }

                if ( segmentExpression != null )
                {
                    var personQry = personService.Get( paramExpression, segmentExpression );
                    groupMemberQuery = groupMemberQuery.Where( a => personQry.Any( p => p.Id == a.PersonId ) );
                }
            }

            return groupMemberQuery;
        }

        /// <summary>
        /// if <see cref="ExcludeDuplicateRecipientAddress" /> is set to true, removes <see cref="CommunicationRecipient"></see>s that have the same SMS/Email address as another recipient
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        public void RemoveRecipientsWithDuplicateAddress( RockContext rockContext )
        {
            if ( !ExcludeDuplicateRecipientAddress )
            {
                return;
            }

            var communicationRecipientService = new CommunicationRecipientService( rockContext );

            var recipientsQry = GetRecipientsQry( rockContext );

            int? smsMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
            if ( smsMediumEntityTypeId.HasValue )
            {
                IQueryable<CommunicationRecipient> duplicateSMSRecipientsQuery = recipientsQry.Where( a => a.MediumEntityTypeId == smsMediumEntityTypeId.Value )
                    .Where( a => a.PersonAlias.Person.PhoneNumbers.Where( pn => pn.IsMessagingEnabled ).Any() )
                    .GroupBy( a => a.PersonAlias.Person.PhoneNumbers.Where( pn => pn.IsMessagingEnabled ).FirstOrDefault().Number )
                    .Where( a => a.Count() > 1 )
                    .Select( a => a.OrderBy( x => x.Id ).Skip( 1 ).ToList() )
                    .SelectMany( a => a );

                var duplicateSMSRecipients = duplicateSMSRecipientsQuery.ToList();
                communicationRecipientService.DeleteRange( duplicateSMSRecipients );
            }

            int? emailMediumEntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
            if ( emailMediumEntityTypeId.HasValue )
            {
                IQueryable<CommunicationRecipient> duplicateEmailRecipientsQry = recipientsQry.Where( a => a.MediumEntityTypeId == emailMediumEntityTypeId.Value )
                    .GroupBy( a => a.PersonAlias.Person.Email )
                    .Where( a => a.Count() > 1 )
                    .Select( a => a.OrderBy( x => x.Id ).Skip( 1 ).ToList() )
                    .SelectMany( a => a );

                var duplicateEmailRecipients = duplicateEmailRecipientsQry.ToList();
                communicationRecipientService.DeleteRange( duplicateEmailRecipients );
            }

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Refresh the recipients list.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public void RefreshCommunicationRecipientList( RockContext rockContext )
        {
            if ( !ListGroupId.HasValue )
            {
                return;
            }

            var segmentDataViewGuids = this.Segments.SplitDelimitedValues().AsGuidList();
            var segmentDataViewIds = new DataViewService( rockContext ).GetByGuids( segmentDataViewGuids ).Select( a => a.Id ).ToList();

            var qryCommunicationListMembers = GetCommunicationListMembers( rockContext, ListGroupId, this.SegmentCriteria, segmentDataViewIds );

            // NOTE: If this is scheduled communication, don't include Members that were added after the scheduled FutureSendDateTime.
            // However, don't exclude if the date added can't be determined or they will never be sent a scheduled communication.
            if ( this.FutureSendDateTime.HasValue )
            {
                var memberAddedCutoffDate = this.FutureSendDateTime;

                qryCommunicationListMembers = qryCommunicationListMembers.Where( a => ( a.DateTimeAdded.HasValue && a.DateTimeAdded.Value < memberAddedCutoffDate )
                                                                                        || ( a.CreatedDateTime.HasValue && a.CreatedDateTime.Value < memberAddedCutoffDate )
                                                                                        || ( !a.DateTimeAdded.HasValue && !a.CreatedDateTime.HasValue ) );
            }

            var communicationRecipientService = new CommunicationRecipientService( rockContext );

            var recipientsQry = GetRecipientsQry( rockContext );

            // Get all the List member which is not part of communication recipients yet
            var newMemberInList = qryCommunicationListMembers
                .Include( c => c.Person )
                .Where( a => !recipientsQry.Any( r => r.PersonAlias.PersonId == a.PersonId ) )
                .AsNoTracking()
                .ToList();

            var emailMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() );
            var smsMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() );
            var pushMediumEntityType = EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() );

            var recipientsToAdd = newMemberInList.Select( m => new CommunicationRecipient
            {
                PersonAliasId = m.Person.PrimaryAliasId.Value,
                Status = CommunicationRecipientStatus.Pending,
                CommunicationId = Id,
                MediumEntityTypeId = DetermineMediumEntityTypeId(
                    emailMediumEntityType.Id,
                    smsMediumEntityType.Id,
                    pushMediumEntityType.Id,
                    CommunicationType,
                    m.CommunicationPreference,
                    m.Person.CommunicationPreference )
            } );
            rockContext.BulkInsert<CommunicationRecipient>( recipientsToAdd );

            // Get all pending communication recipients that are no longer part of the group list member, then delete them from the Recipients
            var missingMemberInList = recipientsQry.Where( a => a.Status == CommunicationRecipientStatus.Pending )
                .Where( a => !qryCommunicationListMembers.Any( r => r.PersonId == a.PersonAlias.PersonId ) );

            rockContext.BulkDelete<CommunicationRecipient>( missingMemberInList );

            rockContext.SaveChanges();
        }

        /// <summary>
        /// Determines the medium entity type identifier.
        /// Given the email, SMS medium, and Push entity type ids, along with the available communication preferences,
        /// this method will determine which medium entity type id should be used and return that id.
        /// </summary>
        /// <remarks>
        ///  NOTE: For the given communicationTypePreferences parameters array, in the event that CommunicationType.RecipientPreference is given,
        ///  the logic below will use the *next* given CommunicationType to determine which medium/type is selected/returned. If none is available,
        ///  it will return the email medium entity type id.  Typically is expected that the ordered params list eventually has either
        ///  CommunicationType.Email, CommunicationType.SMS or CommunicationType.PushNotification.
        /// </remarks>
        /// <param name="emailMediumEntityTypeId">The email medium entity type identifier.</param>
        /// <param name="smsMediumEntityTypeId">The SMS medium entity type identifier.</param>
        /// <param name="pushMediumEntityTypeId">The push medium entity type identifier.</param>
        /// <param name="communicationTypePreference">An array of ordered communication type preferences.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Unexpected CommunicationType: {currentCommunicationPreference.ConvertToString()} - communicationTypePreference</exception>
        /// <exception cref="Exception">Unexpected CommunicationType: " + currentCommunicationPreference.ConvertToString()</exception>
        public static int DetermineMediumEntityTypeId( int emailMediumEntityTypeId, int smsMediumEntityTypeId, int pushMediumEntityTypeId, params CommunicationType[] communicationTypePreference )
        {
            for ( var i = 0; i < communicationTypePreference.Length; i++ )
            {
                var currentCommunicationPreference = communicationTypePreference[i];
                var hasNextCommunicationPreference = ( i + 1 ) < communicationTypePreference.Length;

                switch ( currentCommunicationPreference )
                {
                    case CommunicationType.Email:
                        return emailMediumEntityTypeId;
                    case CommunicationType.SMS:
                        return smsMediumEntityTypeId;
                    case CommunicationType.PushNotification:
                        return pushMediumEntityTypeId;
                    case CommunicationType.RecipientPreference:
                        if ( hasNextCommunicationPreference )
                        {
                            break;
                        }

                        return emailMediumEntityTypeId;
                    default:
                        throw new ArgumentException( $"Unexpected CommunicationType: {currentCommunicationPreference.ConvertToString()}", "communicationTypePreference" );
                }
            }

            return emailMediumEntityTypeId;
        }

        /// <summary>
        /// Determines the medium entity type identifier.
        /// Given the email and sms medium entity type ids and the available communication preferences
        /// this method will determine which medium entity type id should be used and return that id.
        /// If a preference could not be determined the email medium entity type id will be returned.
        /// </summary>
        /// <param name="emailMediumEntityTypeId">The email medium entity type identifier.</param>
        /// <param name="smsMediumEntityTypeId">The SMS medium entity type identifier.</param>
        /// <param name="recipientPreference">The recipient preference.</param>
        /// <returns></returns>
        [Obsolete( "Use the override that includes 'pushMediumEntityTypeId' instead." )]
        [RockObsolete( "1.11" )]
        public static int DetermineMediumEntityTypeId( int emailMediumEntityTypeId, int smsMediumEntityTypeId, params CommunicationType[] recipientPreference )
        {
            return DetermineMediumEntityTypeId( emailMediumEntityTypeId, smsMediumEntityTypeId, 0, recipientPreference );
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name ?? this.Subject ?? base.ToString();
        }

        /// <summary>
        /// Method that will be called on an entity immediately after the item is saved by context
        /// </summary>
        /// <param name="dbContext">The database context.</param>
        public override void PostSaveChanges( Rock.Data.DbContext dbContext )
        {
            // ensure any attachments have the binaryFile.IsTemporary set to False
            var attachmentBinaryFilesIds = this.Attachments.Select( a => a.BinaryFileId ).ToList();
            if ( attachmentBinaryFilesIds.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    var temporaryBinaryFiles = new BinaryFileService( rockContext ).GetByIds( attachmentBinaryFilesIds ).Where( a => a.IsTemporary == true ).ToList();
                    {
                        foreach ( var binaryFile in temporaryBinaryFiles )
                        {
                            binaryFile.IsTemporary = false;
                        }
                    }

                    rockContext.SaveChanges();
                }
            }

            base.PostSaveChanges( dbContext );
        }

        #endregion

        #region Private Methods

        #endregion

        #region Static Methods

        private static object _obj = new object();

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public static void Send( Rock.Model.Communication communication )
        {
            if ( communication == null || communication.Status != CommunicationStatus.Approved )
            {
                return;
            }

            // only alter the Recipient list if it the communication hasn't sent a message to any recipients yet
            if ( communication.SendDateTime.HasValue == false )
            {
                using ( var rockContext = new RockContext() )
                {
                    if ( communication.ListGroupId.HasValue )
                    {
                        communication.RefreshCommunicationRecipientList( rockContext );
                    }

                    if ( communication.ExcludeDuplicateRecipientAddress )
                    {
                        communication.RemoveRecipientsWithDuplicateAddress( rockContext );
                    }
                }
            }

            foreach ( var medium in communication.GetMediums() )
            {
                medium.Send( communication );
            }

            using ( var rockContext = new RockContext() )
            {
                var dbCommunication = new CommunicationService( rockContext ).Get( communication.Id );

                // Set the SendDateTime of the Communication
                dbCommunication.SendDateTime = RockDateTime.Now;
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Sends the specified communication.
        /// </summary>
        /// <param name="communication">The communication.</param>
        public async static Task SendAsync( Rock.Model.Communication communication )
        {
            if ( communication == null || communication.Status != CommunicationStatus.Approved )
            {
                return;
            }

            // only alter the Recipient list if it the communication hasn't sent a message to any recipients yet
            if ( communication.SendDateTime.HasValue == false )
            {
                using ( var rockContext = new RockContext() )
                {
                    if ( communication.ListGroupId.HasValue )
                    {
                        communication.RefreshCommunicationRecipientList( rockContext );
                    }

                    if ( communication.ExcludeDuplicateRecipientAddress )
                    {
                        communication.RemoveRecipientsWithDuplicateAddress( rockContext );
                    }
                }
            }

            var sendTasks = new List<Task>();
            foreach ( var medium in communication.GetMediums() )
            {
                var asyncMedium = medium as IAsyncMediumComponent;

                if ( asyncMedium == null )
                {
                    sendTasks.Add( Task.Run( () => medium.Send( communication ) ) );
                }
                else
                {
                    sendTasks.Add( asyncMedium.SendAsync( communication ) );
                }
            }

            var aggregateExceptions = new List<Exception>();
            while ( sendTasks.Count > 0 )
            {
                var completedTask = await Task.WhenAny( sendTasks ).ConfigureAwait( false );
                if ( completedTask.Exception != null )
                {
                    aggregateExceptions.AddRange( completedTask.Exception.InnerExceptions );
                }

                sendTasks.Remove( completedTask );
            }

            if ( aggregateExceptions.Count > 0 )
            {
                throw new AggregateException( aggregateExceptions );
            }

            using ( var rockContext = new RockContext() )
            {
                var dbCommunication = new CommunicationService( rockContext ).Get( communication.Id );

                // Set the SendDateTime of the Communication
                dbCommunication.SendDateTime = RockDateTime.Now;
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the next pending.
        /// </summary>
        /// <param name="communicationId">The communication identifier.</param>
        /// <param name="mediumEntityId">The medium entity identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public static Rock.Model.CommunicationRecipient GetNextPending( int communicationId, int mediumEntityId, Rock.Data.RockContext rockContext )
        {
            CommunicationRecipient recipient = null;

            var delayTime = RockDateTime.Now.AddMinutes( -10 );

            lock ( _obj )
            {
                recipient = new CommunicationRecipientService( rockContext ).Queryable().Include( r => r.Communication ).Include( r => r.PersonAlias.Person )
                    .Where( r =>
                        r.CommunicationId == communicationId &&
                        ( r.Status == CommunicationRecipientStatus.Pending ||
                            ( r.Status == CommunicationRecipientStatus.Sending && r.ModifiedDateTime < delayTime )
                        ) &&
                        r.MediumEntityTypeId.HasValue &&
                        r.MediumEntityTypeId.Value == mediumEntityId )
                    .FirstOrDefault();

                if ( recipient != null )
                {
                    recipient.Status = CommunicationRecipientStatus.Sending;
                    rockContext.SaveChanges();
                }
            }

            return recipient;
        }
        #endregion
    }

    #region Entity Configuration

    /// <summary>
    /// Communication Configuration class.
    /// </summary>
    public partial class CommunicationConfiguration : EntityTypeConfiguration<Communication>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationConfiguration"/> class.
        /// </summary>
        public CommunicationConfiguration()
        {
            this.HasOptional( c => c.SenderPersonAlias ).WithMany().HasForeignKey( c => c.SenderPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.ReviewerPersonAlias ).WithMany().HasForeignKey( c => c.ReviewerPersonAliasId ).WillCascadeOnDelete( false );
            this.HasOptional( c => c.SMSFromDefinedValue ).WithMany().HasForeignKey( c => c.SMSFromDefinedValueId ).WillCascadeOnDelete( false );

            // the Migration will manually add a ON DELETE SET NULL for ListGroupId
            this.HasOptional( c => c.ListGroup ).WithMany().HasForeignKey( c => c.ListGroupId ).WillCascadeOnDelete( false );

            // the Migration will manually add a ON DELETE SET NULL for CommunicationTemplateId
            this.HasOptional( c => c.CommunicationTemplate ).WithMany().HasForeignKey( c => c.CommunicationTemplateId ).WillCascadeOnDelete( false );

            // NOTE: When creating a migration for this, don't create the actual FK's in the database for this just in case there are outlier OccurrenceDates that aren't in the AnalyticsSourceDate table
            // and so that the AnalyticsSourceDate can be rebuilt from scratch as needed
            this.HasOptional( r => r.SendSourceDate ).WithMany().HasForeignKey( r => r.SendDateKey ).WillCascadeOnDelete( false );

            // the Migration will manually add a ON DELETE SET NULL for SystemCommunicationId
            this.HasOptional( r => r.SystemCommunication ).WithMany().HasForeignKey( r => r.SystemCommunicationId ).WillCascadeOnDelete( false );
        }
    }

    #endregion

    #region Enumerations

    /// <summary>
    /// The status of a communication
    /// </summary>
    public enum CommunicationStatus
    {
        /// <summary>
        /// Communication was created, but not yet edited by a user. (i.e. from data grid or report)
        /// Transient communications more than a few hours old may be deleted by clean-up job.
        /// </summary>
        Transient = 0,

        /// <summary>
        /// Communication is currently being drafted
        /// </summary>
        Draft = 1,

        /// <summary>
        /// Communication has been submitted but not yet approved or denied
        /// </summary>
        PendingApproval = 2,

        /// <summary>
        /// Communication has been approved for sending
        /// </summary>
        Approved = 3,

        /// <summary>
        /// Communication has been denied
        /// </summary>
        Denied = 4,
    }

    /// <summary>
    /// Type of communication
    /// </summary>
    public enum CommunicationType
    {
        /// <summary>
        /// RecipientPreference
        /// </summary>
        [Display( Name = "No Preference" )]
        RecipientPreference = 0,

        /// <summary>
        /// Email
        /// </summary>
        Email = 1,

        /// <summary>
        /// SMS
        /// </summary>
        SMS = 2,

        /// <summary>
        /// Push notification
        /// </summary>
        PushNotification = 3
    }

    /// <summary>
    /// Flag indicating if communication is for all selected segments or any segments
    /// </summary>
    public enum SegmentCriteria
    {
        /// <summary>
        /// All
        /// </summary>
        All = 0,

        /// <summary>
        /// Any
        /// </summary>
        Any = 1,
    }

    #endregion
}
