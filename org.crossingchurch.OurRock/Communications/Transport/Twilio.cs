using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;

using Rock;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace org.crossingchurch.OurRock.Communications.Transport
{
    [Description( "Sends a communication through Twilio API, but will not send to any recipients who belong to the from number's STOP group (requires that the From Numbers defined type has a 'StopGroup' attribute)" )]
    [Export( typeof( TransportComponent ) )]
    [ExportMetadata( "ComponentName", "Twilio (With STOP support)" )]
    public class Twilio : Rock.Communication.Transport.Twilio
    {
        public override void Send( Rock.Model.Communication communication, int mediumEntityTypeId, Dictionary<string, string> mediumAttributes )
        {
            using ( var rockContext = new RockContext() )
            {
                communication = new CommunicationService( rockContext ).Get( communication.Id );
                if ( communication != null && communication.SMSFromDefinedValueId.HasValue )
                {
                    var fromNumber = DefinedValueCache.Get( communication.SMSFromDefinedValueId.Value );
                    if ( fromNumber != null )
                    {
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
            }

            base.Send( communication, mediumEntityTypeId, mediumAttributes );
        }
    }
}
