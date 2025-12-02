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
using System.ComponentModel;
using Newtonsoft.Json;
using RestSharp;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace org.thecrossingchurch.CustomJobs.Jobs
{
    [DisplayName( "Update Podcast Series in Megaphone" )]
    [Description( "Job for swapping the podcast urls that fixes data in MEgaphone to match current standards." )]
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
        private List<string> unprocessedPodcasts { get; set; }

        public override void Execute()
        {
            RockContext rockContext = new RockContext();
            unprocessedPodcasts = new List<string>();
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


            for ( int i = 0; i < series.Count(); i++ )
            {
                var podcasts = GetPodcasts( netId, series[i].GetAttributeValue( "MegaphonePodcastId" ), megaphoneApiKey );
                ProcessPodcasts( netId, series[i].GetAttributeValue( "MegaphonePodcastId" ), megaphoneApiKey, podcasts );
            }

            if ( unprocessedPodcasts.Count > 0 )
            {
                string jobStatus = unprocessedPodcasts.Count + " Unprocessed Podcasts\n" + String.Join( "\n", unprocessedPodcasts );
                this.UpdateLastStatusMessage( jobStatus );
                throw new RockJobWarningException( jobStatus );
            }
        }

        private string MakeRequest( string url, string token )
        {
            var client = new RestClient( url );
            client.Timeout = -1;
            var request = new RestRequest( Method.GET );
            string header = "Token token=\"" + token + "\"";
            request.AddHeader( "Authorization", header );
            IRestResponse response = client.Execute( request );
            return response.Content;
        }

        private void UpdatePodcast( string url, string token, string body )
        {
            var client = new RestClient( url );
            var request = new RestRequest( Method.PUT );
            request.AddHeader( "Content-Type", "application/json" );
            string header = "Token token=\"" + token + "\"";
            request.AddHeader( "Authorization", header );
            request.AddParameter( "application/json", body, ParameterType.RequestBody );
            IRestResponse response = client.Execute( request );
        }

        private List<Podcast> GetPodcasts( string networkId, string podcastId, string token )
        {
            string url = $"https://cms.megaphone.fm/api/networks/{networkId}/podcasts/{podcastId}";
            string podcastInfo = MakeRequest( url, token );
            PodcastInfo info = JsonConvert.DeserializeObject<PodcastInfo>( podcastInfo );
            double x = ( double ) info.episodesCount / 250;
            int totalPages = ( int ) Math.Ceiling( x );
            url += "/episodes";
            List<Podcast> podcasts = new List<Podcast>();
            for ( int i = 1; i <= totalPages; i++ )
            {
                string pagination = "?";
                pagination += "page=" + i + "&per_page=250";
                var results = MakeRequest( url + pagination, token );
                var episodes = JsonConvert.DeserializeObject<List<Podcast>>( results );
                podcasts.AddRange( episodes );
            }
            return podcasts;
        }

        private void ProcessPodcasts( string networkId, string podcastId, string token, List<Podcast> podcasts )
        {
            RockContext context = new RockContext();
            string url = "https://cms.megaphone.fm/api/networks/" + networkId + "/podcasts/" + podcastId + "/episodes/";
            for ( int i = 0; i < podcasts.Count(); i++ )
            {
                var podcast = podcasts[i];
                ContentChannelItem item = null;
                AttributeValue megaphoneId = new AttributeValueService( context ).Queryable().FirstOrDefault( av => av.AttributeId == megaphoneIdAttr.Id && av.Value == podcast.id.ToString() );
                ContentChannelItemService cci_svc = new ContentChannelItemService( context );
                if ( megaphoneId != null )
                {
                    item = cci_svc.Get( megaphoneId.EntityId.Value );
                }
                if ( megaphoneId == null || item == null )
                {
                    item = cci_svc.Queryable().Where( cci => cci.ContentChannelId == channel.Id ).ToList().FirstOrDefault( cci => cci.Title == podcast.title || podcast.title.Contains( cci.Title ) || podcast.title.StartsWith( cci.Title ) );
                }
                if ( item != null )
                {
                    item.LoadAttributes();
                    MegaphoneRequestBody body = new MegaphoneRequestBody()
                    {
                        customFields = podcast.customFields
                    };
                    body.customFields.Subseries = item.GetAttributeValue( "Subseries" );
                    UpdatePodcast( url + item.GetAttributeValue( "MegaphoneId" ), token, JsonConvert.SerializeObject( body ) );
                }
                else
                {
                    unprocessedPodcasts.Add( podcast.id + ": " + podcast.title );
                }
            }
        }

        private class Podcast
        {
            public Guid id { get; set; }
            public string title { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime pubdate { get; set; }
            public string pubdateTimezone { get; set; }
            public string summary { get; set; }
            public string audioFile { get; set; }
            public string imageFile { get; set; }
            public string uid { get; set; }
            public string status { get; set; }
            public bool draft { get; set; }
            public PodcastCustomFields customFields { get; set; }
        }

        private class PodcastCustomFields
        {
            public string Guest { get; set; }
            public Object Author { get; set; }
            public Object Author2 { get; set; }
            public string Subseries { get; set; }
            public string MetaDescription { get; set; }
        }

        private class MegaphoneRequestBody
        {
            public PodcastCustomFields customFields { get; set; }
        }

        private class PodcastInfo
        {
            public int episodesCount { get; set; }
            public string imageFile { get; set; }
        }
    }
}
