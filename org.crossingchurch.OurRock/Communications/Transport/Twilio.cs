using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Model;

namespace org.crossingchurch.OurRock.Communications.Transport
{
    [Description( "Sends a communication through Twilio API, but will not send to any recipients who belong to the from number's STOP group (requires that From Numbers have a 'StopGroup' attribute)" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Twilio (With STOP support)" )]
    public class TwilioWithStop : Rock.Communication.Transport.Twilio, IAsyncTransport
    {
        public async new Task SendAsync( Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            using ( var rockContext = new RockContext() )
            {
                communication = new CommunicationService( rockContext ).Get( communication.Id );
                if ( communication?.SmsFromSystemPhoneNumberId != null )
                {
                    var fromNumber = communication.SmsFromSystemPhoneNumber;
                    fromNumber.LoadAttributes( rockContext );
                    var stopGroup = fromNumber.GetAttributeValue( "StopGroup" ).AsGuidOrNull();
                    if ( stopGroup.HasValue )
                    {
                        var stopPersonIds = new GroupMemberService( rockContext )
                            .GetByGroupGuid( stopGroup.Value )
                            .Select( m => m.PersonId )
                            .ToList();

                        foreach ( var recipient in communication.Recipients
                            .Where( r => stopPersonIds.Contains( r.PersonAlias.PersonId ) ) )
                        {
                            recipient.Status = CommunicationRecipientStatus.Failed;
                            recipient.StatusNote = "Recipient has previously replied STOP to the sending phone number.";
                        }

                        rockContext.SaveChanges();
                    }
                }
            }
            await base.SendAsync(communication, mediumEntityTypeId, mediumAttributes );
        }

        public async new Task<SendMessageResult> SendAsync( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            var smsMessage = rockMessage as RockSMSMessage;
            if ( smsMessage != null && smsMessage.FromSystemPhoneNumber != null ) 
            {
                var fromNumber = smsMessage.FromSystemPhoneNumber;
                var stopGroup = fromNumber.GetAttributeValue( "StopGroup" ).AsGuidOrNull();
                if ( stopGroup.HasValue )
                {
                    var stopPersonIds = new GroupMemberService( new RockContext() )
                        .GetByGroupGuid( stopGroup.Value )
                        .Select( m => m.PersonId )
                    .ToList();

                    smsMessage.SetRecipients( smsMessage.GetRecipients()
                        .Where( r =>
                            r.PersonId.HasValue &&
                            !stopPersonIds.Contains( r.PersonId.Value ) )
                        .ToList() );
                }
            }

            return await base.SendAsync( rockMessage, mediumEntityTypeId, mediumAttributes );

        }

        public override bool Send( RockMessage rockMessage, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes, out List<string> errorMessages )
        {
            errorMessages = new List<string>();

            var sendMessageResult = AsyncHelper.RunSync( () => SendAsync( rockMessage, mediumEntityTypeId, mediumAttributes ) );

            errorMessages.AddRange( sendMessageResult.Errors );

            return !errorMessages.Any();
        }

        public override void Send( Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            using ( var rockContext = new RockContext() )
            {
                communication = new CommunicationService( rockContext ).Get( communication.Id );
                if ( communication?.SmsFromSystemPhoneNumberId != null )
                {
                    var fromNumber = communication.SmsFromSystemPhoneNumber;
                    fromNumber.LoadAttributes( rockContext );
                    var stopGroup = fromNumber.GetAttributeValue( "StopGroup" ).AsGuidOrNull();
                    if ( stopGroup.HasValue )
                    {
                        var stopPersonIds = new GroupMemberService( rockContext )
                            .GetByGroupGuid( stopGroup.Value )
                            .Select( m => m.PersonId )
                            .ToList();

                        foreach ( var recipient in communication.Recipients
                            .Where( r => stopPersonIds.Contains( r.PersonAlias.PersonId ) ) )
                        {
                            recipient.Status = CommunicationRecipientStatus.Failed;
                            recipient.StatusNote = "Recipient has previously replied STOP to the sending phone number.";
                        }

                        rockContext.SaveChanges();
                    }
                }
            }

            base.Send( communication, mediumEntityTypeId, mediumAttributes );
        }

        private static class AsyncHelper
        {
            private static readonly TaskFactory _myTaskFactory = new TaskFactory(
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default );

            /// <summary>
            /// Runs the synchronize.
            /// </summary>
            /// <typeparam name="TResult">The type of the result.</typeparam>
            /// <param name="func">The function.</param>
            /// <returns></returns>
            public static TResult RunSync<TResult>( Func<Task<TResult>> func )
            {
                var cultureUi = CultureInfo.CurrentUICulture;
                var culture = CultureInfo.CurrentCulture;
                return _myTaskFactory.StartNew( () =>
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = cultureUi;
                    return func();
                } ).Unwrap().GetAwaiter().GetResult();
            }

            /// <summary>
            /// Runs the synchronize.
            /// </summary>
            /// <param name="func">The function.</param>
            public static void RunSync( Func<Task> func )
            {
                var cultureUi = CultureInfo.CurrentUICulture;
                var culture = CultureInfo.CurrentCulture;
                _myTaskFactory.StartNew( () =>
                {
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = cultureUi;
                    return func();
                } ).Unwrap().GetAwaiter().GetResult();
            }
        }
    }
}
