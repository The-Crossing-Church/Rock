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
using System.Data.Entity;
using System.Linq;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Communication
{
    /// <summary>
    /// Rock SMS Message
    /// </summary>
    /// <seealso cref="Rock.Communication.RockMessage" />
    public class RockSMSMessage : RockMessage
    {
        /// <summary>
        /// Gets the medium entity type identifier.
        /// </summary>
        /// <value>
        /// The medium entity type identifier.
        /// </value>
        public override int MediumEntityTypeId
        {
            get
            {
                return EntityTypeCache.Get( SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() ).Id;
            }
        }

        /// <summary>
        /// Gets or sets from number.
        /// </summary>
        /// <value>
        /// From number.
        /// </value>
        public DefinedValueCache FromNumber
        {
            get
            {
                if ( fromNumberValueId.HasValue  )
                {
                    return DefinedValueCache.Get( fromNumberValueId.Value );
                }

                return null;
            }

            set
            {
                fromNumberValueId = value?.Id;
            }
        }

        private int? fromNumberValueId = null;

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the name of the communication.
        /// When CreateCommunicationRecord = true this value will insert Communication.Name
        /// </summary>
        /// <value>
        /// The name of the communication.
        /// </value>
        [Obsolete( "Use CommunicationName instead" )]
        [RockObsolete("1.12")]
        public string communicationName
        {
            get => CommunicationName;
            set => CommunicationName = value;
        }

        /// <summary>
        /// Gets or sets the name of the communication.
        /// When CreateCommunicationRecord = true this value will insert Communication.Name
        /// </summary>
        /// <value>
        /// The name of the communication.
        /// </value>
        public string CommunicationName { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSMSMessage"/> class.
        /// </summary>
        public RockSMSMessage() : base() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RockSMSMessage"/> class.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        public RockSMSMessage( SystemCommunication systemCommunication ) : this()
        {
            if ( systemCommunication != null )
            {
                InitializeSmsMessage( systemCommunication );
            }
        }

        /// <summary>
        /// Initializes the SMS message.
        /// </summary>
        /// <param name="systemCommunication">The system communication.</param>
        private void InitializeSmsMessage( SystemCommunication systemCommunication )
        {
            if ( systemCommunication == null )
            {
                return;
            }

            this.FromNumber = DefinedValueCache.Get( systemCommunication.SMSFromDefinedValue );
            this.Message = systemCommunication.SMSMessage;
            this.SystemCommunicationId = systemCommunication.Id;
        }

        /// <summary>
        /// Returns the Person sending the SMS communication.
        /// Will use the Response Recipient if one exists otherwise the Current Person.
        /// </summary>
        /// <returns></returns>
        public Rock.Model.Person GetSMSFromPerson()
        {
            // Try to get a from person
            Rock.Model.Person person = CurrentPerson;

            // If the response recipient exists use it
            var fromPersonAliasGuid = FromNumber.GetAttributeValue( "ResponseRecipient" ).AsGuidOrNull();
            if ( fromPersonAliasGuid.HasValue )
            {
                person = new Rock.Model.PersonAliasService( new Data.RockContext() )
                    .Queryable()
                    .AsNoTracking()
                    .Where( p => p.Guid.Equals( fromPersonAliasGuid.Value ) )
                    .Select( p => p.Person )
                    .FirstOrDefault();
            }

            return person;
        }

        /// <summary>
        /// Sets the recipients.
        /// </summary>
        /// <param name="recipients">The recipients.</param>
        public void SetRecipients( List<RockSMSMessageRecipient> recipients )
        {
            this.Recipients = new List<RockMessageRecipient>();
            this.Recipients.AddRange( recipients );
        }
    }
}
