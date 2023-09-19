// <copyright>
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
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.Text.RegularExpressions;
using System;
using RestSharp;
using Rock;
using Newtonsoft.Json;
using Rock.Security;
using Rock.Web.Cache;

namespace org.crossingchurch.PodcastSync.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [DefinedTypeField( "Podcast Series", "", true )]
    [TextField( "Megaphone API Key", "Global Attribute Key for the Megaphone API Key", true )]
    [TextField( "Megaphone Network Id", "Id in Megaphone of the network", true )]
    [TextField( "Megaphone Embed Link", "", true, "https://playlist.megaphone.fm/?e=" )]
    [ContentChannelField( "Content Channel", "The content channel to sync items to", true )]
    [AttributeField( Rock.SystemGuid.EntityType.CONTENT_CHANNEL_ITEM, "ContentChannelId", "55", "Megaphone Id Attribute" )]
    [BooleanField( "Run for all time", "Check this box if the job should run on all podcasts ever created, leave unchecked if it should only get podcasts that have been modified since the last run", ControlType = Rock.Field.Types.BooleanFieldType.BooleanControlType.Checkbox )]
    [DisallowConcurrentExecution]
    public class PodcastSync : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public PodcastSync()
        {
        }

        public ContentChannel channel { get; set; }
        public string embedLink { get; set; }
        public Rock.Model.Attribute megaphoneIdAttr { get; set; }

        /// <summary>
        /// Job that will pull podcast data from Megaphone
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            RockContext rockContext = new RockContext();
            int jobId = 0;
            ServiceJob job = null;
            Int32.TryParse( context.JobDetail.Description, out jobId );
            if (jobId > 0)
            {
                job = new ServiceJobService( rockContext ).Get( jobId );
            }
            Guid seriesDTGuid;
            Guid.TryParse( dataMap.GetString( "PodcastSeries" ), out seriesDTGuid );
            DefinedType seriesDT;
            List<DefinedValue> series = new List<DefinedValue>();
            if (seriesDTGuid != null)
            {
                seriesDT = new DefinedTypeService( rockContext ).Get( seriesDTGuid );
                series = new DefinedValueService( rockContext ).Queryable().Where( dv => dv.DefinedTypeId == seriesDT.Id && dv.IsActive ).ToList();
                series.LoadAttributes();
            }
            Guid ccGuid;
            Guid.TryParse( dataMap.GetString( "ContentChannel" ), out ccGuid );
            if (ccGuid != null)
            {
                channel = new ContentChannelService( rockContext ).Get( ccGuid );
            }
            embedLink = dataMap.GetString( "MegaphoneEmbedLink" );
            Guid idAttrGuid;
            Guid.TryParse( dataMap.GetString( "MegaphoneIdAttribute" ), out idAttrGuid );
            if (idAttrGuid != null)
            {
                megaphoneIdAttr = new AttributeService( rockContext ).Get( idAttrGuid );
            }
            string key = dataMap.GetString( "MegaphoneAPIKey" );
            string megaphoneApiKey = Encryption.DecryptString( GlobalAttributesCache.Get().GetValue( key ) );
            string netId = dataMap.GetString( "MegaphoneNetworkId" );
            bool allTime = dataMap.GetBooleanFromString( "Runforalltime" );

            for (int i = 0; i < series.Count(); i++)
            {
                var podcasts = GetPodcasts( netId, series[i].GetAttributeValue( "MegaphonePodcastId" ), megaphoneApiKey, allTime, job.LastRunDateTime );
                ProcessPodcasts( podcasts, series[i].GetAttributeValue( "PodcastSeries" ), series[i].GetAttributeValue( "SeriesImage" ) );
            }
        }

        private List<Podcast> GetPodcasts( string networkId, string podcastId, string token, bool getAll, DateTime? lastRun )
        {
            string url = $"https://cms.megaphone.fm/api/networks/{networkId}/podcasts/{podcastId}";
            string podcastInfo = MakeRequest( url, token );
            PodcastInfo info = JsonConvert.DeserializeObject<PodcastInfo>( podcastInfo );
            double x = (double) info.episodesCount / 250;
            int totalPages = (int) Math.Ceiling( x );
            url += "/episodes";
            if (!getAll)
            {
                url += "?updated_since=" + lastRun.Value.StartOfDay().ToString( "yyyy-MM-ddTHH:mm:ss" ) + "&";
            }
            List<Podcast> podcasts = new List<Podcast>();
            for (int i = 1; i <= totalPages; i++)
            {
                string pagination = "";
                if (getAll)
                {
                    pagination = "?";
                }
                pagination += "page=" + i + "&per_page=250";
                var results = MakeRequest( url + pagination, token );
                var episodes = JsonConvert.DeserializeObject<List<Podcast>>( results );
                podcasts.AddRange( episodes );
            }
            return podcasts;
        }

        public string MakeRequest( string url, string token )
        {
            var client = new RestClient( url );
            client.Timeout = -1;
            var request = new RestRequest( Method.GET );
            string header = "Token token=\"" + token + "\"";
            request.AddHeader( "Authorization", header );
            IRestResponse response = client.Execute( request );
            return response.Content;
        }

        private void ProcessPodcasts( List<Podcast> podcasts, string series, string image )
        {
            RockContext context = new RockContext();
            for (int i = 0; i < podcasts.Count(); i++)
            {
                var podcast = podcasts[i];
                ContentChannelItem item = null;
                AttributeValue megaphoneId = new AttributeValueService( context ).Queryable().FirstOrDefault( av => av.AttributeId == megaphoneIdAttr.Id && av.Value == podcast.id.ToString() );
                ContentChannelItemService cci_svc = new ContentChannelItemService( context );
                if (megaphoneId != null)
                {
                    item = cci_svc.Get( megaphoneId.EntityId.Value );
                }
                if (megaphoneId == null || item == null)
                {
                    item = cci_svc.Queryable().Where( cci => cci.ContentChannelId == channel.Id ).ToList().FirstOrDefault( cci => cci.Title == podcast.title || podcast.title.Contains( cci.Title ) || podcast.title.StartsWith( cci.Title ) );
                }
                if (item == null)
                {
                    item = new ContentChannelItem()
                    {
                        ContentChannelId = channel.Id,
                        ContentChannelTypeId = channel.ContentChannelTypeId,
                        Title = podcast.title,
                        CreatedDateTime = RockDateTime.Now,
                        ModifiedDateTime = RockDateTime.Now,
                        StartDateTime = podcast.pubdate
                    };
                }
                item.LoadAttributes();

                item.SetAttributeValue( "MegaphoneId", podcast.id.ToString() );
                item.SetAttributeValue( "ContentAltText", podcast.title );
                if (podcast.customFields != null)
                {
                    if (podcast.customFields.Author != null && !String.IsNullOrEmpty( podcast.customFields.Author.ToString() ))
                    {
                        int pid;
                        if (Int32.TryParse( podcast.customFields.Author.ToString(), out pid ))
                        {
                            Person p = new PersonService( context ).Get( pid );
                            item.SetAttributeValue( "Author", p.PrimaryAlias.Guid );
                        }
                    }
                    if (podcast.customFields.Author2 != null && !String.IsNullOrEmpty( podcast.customFields.Author2.ToString() ))
                    {
                        int pid;
                        if (Int32.TryParse( podcast.customFields.Author2.ToString(), out pid ))
                        {
                            Person p = new PersonService( context ).Get( pid );
                            item.SetAttributeValue( "Author2", p.PrimaryAlias.Guid );
                        }
                    }
                    if (!String.IsNullOrEmpty( podcast.customFields.Subseries ))
                    {
                        item.SetAttributeValue( "Subseries", podcast.customFields.Subseries );
                    }
                    if (!String.IsNullOrEmpty( podcast.customFields.Guest ))
                    {
                        item.SetAttributeValue( "Guest", podcast.customFields.Guest );
                    }
                    if (!String.IsNullOrEmpty( podcast.customFields.MetaDescription ))
                    {
                        item.SetAttributeValue( "MetaDescription", podcast.customFields.MetaDescription );
                    }
                }
                item.SetAttributeValue( "Image", image );
                item.Content = podcast.summary;
                item.SetAttributeValue( "Link", embedLink + podcast.uid );
                item.SetAttributeValue( "Series", series );
                if (item.Title != podcast.title)
                {
                    item.Title = podcast.title;
                    new ContentChannelItemSlugService( context ).DeleteRange( item.ContentChannelItemSlugs );
                }
                item.StartDateTime = podcast.pubdate;
                item.ModifiedDateTime = RockDateTime.Now;
                if (item.Id == 0)
                {
                    cci_svc.Add( item );
                }
                context.SaveChanges();
                item.SaveAttributeValues();
            }
        }

        private class Podcast
        {
            public Guid id { get; set; }
            public string title { get; set; }
            public DateTime createdAt { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime pubdate { get; set; }
            public string summary { get; set; }
            public string audioFile { get; set; }
            public string imageFile { get; set; }
            public string uid { get; set; }
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

        private class PodcastInfo
        {
            public int episodesCount { get; set; }
            public string imageFile { get; set; }
        }
    }
}
