using System;
using System.Collections.Generic;
using Quartz;
using System.Linq;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Jobs;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    [DefinedTypeField( "Podcast Series", "", true )]
    [TextField( "Megaphone API Key", "Global Attribute Key for the Megaphone API Key", true )]
    [TextField( "Megaphone Network Id", "Id in Megaphone of the network", true )]
    [ContentChannelField( "Content Channel", "The content channel to sync items to", true )]
    [AttributeField( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, "ContentChannelId", "55", "Megaphone Id Attribute" )]
    [DisallowConcurrentExecution]
    public class PodcastSubseriesUpdates : RockJob
    {
        public ContentChannel channel { get; set; }
        public Rock.Model.Attribute megaphoneIdAttr { get; set; }

        public override void Execute()
        {
            RockContext rockContext = new RockContext();
            Guid seriesDTGuid;
            Guid.TryParse( GetAttributeValue( "PodcastSeries" ), out seriesDTGuid );
            DefinedType seriesDT;
            List<DefinedValue> series = new List<DefinedValue>();
            if ( seriesDTGuid != null )
            {
                seriesDT = new DefinedTypeService( rockContext ).Get( seriesDTGuid );
                series = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == seriesDT.Id && dv.IsActive ).ToList();
                series.LoadAttributes();
            }
            Guid ccGuid;
            Guid.TryParse( GetAttributeValue( "ContentChannel" ), out ccGuid );
            if ( ccGuid != null )
            {
                channel = new ContentChannelService( rockContext ).Get( ccGuid );
            }
            Guid idAttrGuid;
            Guid.TryParse( GetAttributeValue( "MegaphoneIdAttribute" ), out idAttrGuid );
            if ( idAttrGuid != null )
            {
                megaphoneIdAttr = new AttributeService( rockContext ).Get( idAttrGuid );
            }
            string key = GetAttributeValue( "MegaphoneAPIKey" );
            string megaphoneApiKey = Encryption.DecryptString( GlobalAttributesCache.Get().GetValue( key ) );
            string netId = GetAttributeValue( "MegaphoneNetworkId" );
        }
    }
}
