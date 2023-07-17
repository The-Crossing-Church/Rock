using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;


using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using Rock.CheckIn;
using System.Data.Entity;
using System.Web.UI.HtmlControls;
using System.Data;
using System.Text;
using System.Web;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Microsoft.Ajax.Utilities;
using System.Collections.ObjectModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using CSScriptLibrary;
using System.Data.Entity.Migrations;
using Z.EntityFramework.Plus;
using Newtonsoft.Json;
using RestSharp;
using System.Data.SqlClient;

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Related Resources" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Pulls Watch, Listen, Read Content with similar tags" )]
    [TextField( "Hubspot Key Attribute", "", false, "HubspotPrivateAppKey", order: 1 )]
    [IntegerField( "Number of Posts", required: true, order: 2, defaultValue: 7 )]
    [ContentChannelField( "Watch Content Channel", required: true, order: 3 )]
    [TextField( "Watch URL", required: true, defaultValue: "/Resources/Watch/Sermon Archives/", order: 4 )]
    [ContentChannelField( "Listen Content Channel", required: true, order: 5 )]
    [TextField( "Listen URL", required: true, defaultValue: "/Resources/Listen/", order: 6 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 7 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/FeaturedBlogPosts.lava' %}", "", 8 )]

    public partial class RelatedResources : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private string key { get; set; }
        private int numPosts { get; set; }
        private ContentChannel ccWatch { get; set; }
        private string watchURL { get; set; }
        private ContentChannel ccListen { get; set; }
        private string listenURL { get; set; }
        private List<int> previouslyViewed { get; set; }
        private TaggedItemService _tiSvc { get; set; }
        private List<string> tags { get; set; }
        private int itemId { get; set; }
        private int itemChannelId { get; set; }
        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            _context = new RockContext();
            string attrKey = GetAttributeValue( "HubspotKeyAttribute" );
            key = GlobalAttributesCache.Get().GetValue( attrKey );
            numPosts = GetAttributeValue( "NumberofPosts" ).AsInteger();
            watchURL = GetAttributeValue( "WatchURL" );
            listenURL = GetAttributeValue( "ListenURL" );
            string watchGuid = GetAttributeValue( "WatchContentChannel" );
            if (!String.IsNullOrEmpty( watchGuid ))
            {
                ccWatch = new ContentChannelService( _context ).Get( watchGuid.AsGuid() );
            }
            string listenGuid = GetAttributeValue( "ListenContentChannel" );
            if (!String.IsNullOrEmpty( listenGuid ))
            {
                ccListen = new ContentChannelService( _context ).Get( listenGuid.AsGuid() );
            }
            previouslyViewed = GetPreviouslyViewedContent();
            string itemGlobalKey = PageParameter( "Slug" );
            ContentChannelItem item = new ContentChannelItemService( _context ).Queryable().FirstOrDefault( i => i.ItemGlobalKey == itemGlobalKey );
            if (!Page.IsPostBack)
            {
                if (item != null)
                {
                    item.LoadAttributes();
                    itemId = item.Id;
                    itemChannelId = item.ContentChannelId;
                    _tiSvc = new TaggedItemService( _context );
                    tags = _tiSvc.Queryable().Where( ti => ti.EntityGuid == item.Guid && !ti.Tag.OwnerPersonAliasId.HasValue ).Select( ti => ti.Tag.Name.ToLower() ).Distinct().ToList();
                    GetContent();
                }
            }
        }

        #endregion

        #region Methods
        private void GetContent()
        {
            List<Post> content = new List<Post>();
            List<Post> read = new List<Post>();
            List<Post> watch = new List<Post>();
            List<Post> popWatch = new List<Post>();
            List<Post> listen = new List<Post>();
            List<Post> popListen = new List<Post>();
            if (!String.IsNullOrEmpty( key ))
            {
                read = GetBlogPosts();
            }
            if (ccWatch != null)
            {
                watch = GetContent( ccWatch.Id ).Select( e => ConvertWatchToPost( e ) ).ToList();
                popWatch = GetTrending( watch.Select( p => p.Id ).ToList(), ccWatch.Id ).Select( e => ConvertWatchToPost( e ) ).ToList();
            }
            if (ccListen != null)
            {
                listen = GetContent( ccListen.Id ).Select( e => ConvertListenToPost( e ) ).ToList();
                popListen = GetTrending( listen.Select( p => p.Id ).ToList(), ccListen.Id ).Select( e => ConvertListenToPost( e ) ).ToList();
            }

            int numWatch = itemChannelId == ccWatch.Id ? 2 : 1;
            int numPopWatch = 1;
            int numListen = itemChannelId == ccListen.Id ? 2 : 1;
            int numPopListen = 1;
            int numRead = 2;

            if (read.Count() < numRead)
            {
                if (watch.Count() > numWatch)
                {
                    numWatch++;
                }
                else
                {
                    numPopWatch++;
                }
                if (listen.Count() > numListen)
                {
                    numListen++;
                }
                else
                {
                    numPopListen++;
                }
            }
            if (watch.Count() < numWatch)
            {
                if (popWatch.Count() > numPopWatch)
                {
                    numPopWatch++;
                }
                else if (listen.Count() > numListen)
                {
                    numListen++;
                }
                else if (popListen.Count() > numPopListen)
                {
                    numPopListen++;
                }
                else if (read.Count() > numRead)
                {
                    numRead++;
                }
            }
            if (popWatch.Count() < numPopWatch)
            {
                if (watch.Count() > numWatch)
                {
                    numWatch++;
                }
                else
                {
                    if (listen.Count() > numListen)
                    {
                        numListen++;
                    }
                    else if (popListen.Count() > numPopListen)
                    {
                        numPopListen++;
                    }
                    else if (read.Count() > numRead)
                    {
                        numRead++;
                    }
                }
            }
            if (listen.Count() < numListen)
            {
                if (popListen.Count() > numPopListen)
                {
                    numPopListen++;
                }
                else if (watch.Count() > numWatch)
                {
                    numWatch++;
                }
                else if (popWatch.Count() > numPopWatch)
                {
                    numPopWatch++;
                }
                else if (read.Count() > numRead)
                {
                    numRead++;
                }
            }
            if (popListen.Count() < numPopListen)
            {
                if (listen.Count() > numListen)
                {
                    numListen++;
                }
                else
                {
                    if (watch.Count() > numWatch)
                    {
                        numWatch++;
                    }
                    else if (popWatch.Count() > numPopWatch)
                    {
                        numPopWatch++;
                    }
                    else if (read.Count() > numRead)
                    {
                        numRead++;
                    }
                }
            }

            content.AddRange( watch.OrderByDescending( e => e.PublishDate ).Take( numWatch ) );
            content.AddRange( listen.OrderByDescending( e => e.PublishDate ).Take( numListen ) );
            content.AddRange( read.OrderByDescending( e => e.PublishDate ).Take( numRead ) );
            content.AddRange( popWatch.OrderByDescending( e => e.PublishDate ).Take( numPopWatch ) );
            content.AddRange( popListen.OrderByDescending( e => e.PublishDate ).Take( numPopListen ) );

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Posts", content );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        private List<ContentChannelItem> GetContent( int channelId )
        {
            var items = new ContentChannelItemService( _context ).Queryable().Where( i => i.ContentChannelId == channelId && i.Id != itemId && !previouslyViewed.Contains( i.Id ) ).ToList();
            items = items.Select( i =>
            {
                var itemTag = _tiSvc.Queryable().Where( ti => ti.EntityGuid == i.Guid && !ti.Tag.OwnerPersonAliasId.HasValue ).Select( ti => ti.Tag.Name.ToLower() ).ToList();
                var intersect = tags.Intersect( itemTag );
                return new { Item = i, MatchingTags = intersect };
            } ).Where( i => i.MatchingTags.Count() > 2 ).OrderByDescending( i => i.MatchingTags.Count() ).ThenByDescending( i => i.Item.StartDateTime ).Select( i => i.Item ).Take( 7 ).ToList();
            items.LoadAttributes();
            return items.ToList();
        }

        private List<ContentChannelItem> GetTrending( List<int> alreadyIncluded, int channelId )
        {
            DateTime checkDate = RockDateTime.Now.StartOfDay().AddDays( -30 );
            InteractionChannelService ic_svc = new InteractionChannelService( _context );
            InteractionChannel channel = ic_svc.Queryable().FirstOrDefault( ic => ic.ChannelEntityId == channelId );
            if (channel == null)
            {
                return new List<ContentChannelItem>();
            }
            var popular = new InteractionService( _context ).Queryable().Where( i => i.InteractionComponent.InteractionChannelId == channel.Id && i.InteractionDateTime > checkDate && !i.InteractionSession.DeviceType.Application.ToLower().Contains( "bot" ) && !i.InteractionSession.DeviceType.Application.ToLower().Contains( "spider" ) && !i.InteractionSession.DeviceType.Application.ToLower().Contains( "crawler" ) ).Select( i => i.InteractionComponent.EntityId ).GroupBy( i => i ).Select( i => new PopularityResult() { EntityId = i.Key.Value, NumViews = i.Count() } );
            var items = new ContentChannelItemService( _context ).Queryable().Where( i => i.ContentChannelId == channelId && i.Id != itemId && !previouslyViewed.Contains( i.Id ) && !alreadyIncluded.Contains( i.Id ) );
            var results = popular.Join( items,
                    pop => pop.EntityId,
                    cci => cci.Id,
                    ( pop, cci ) => new { CCI = cci, Pop = pop }
                ).Select( i => i.CCI ).Take( 7 ).ToList();
            results.LoadAttributes();
            return results;
        }

        private List<Post> GetBlogPosts()
        {
            //Get the Hubspot Tags
            List<string> tag_ids = new List<string>();
            Dictionary<string, string> tagDict = new Dictionary<string, string>();
            for (int i = 0; i < tags.Count(); i++)
            {
                var tagClient = new RestClient( "https://api.hubapi.com/cms/v3/blogs/tags?name__like=" + tags[i] );
                tagClient.Timeout = -1;
                var tagRequest = new RestRequest( Method.GET );
                tagRequest.AddHeader( "Authorization", $"Bearer {key}" );
                IRestResponse jsonResponse = tagClient.Execute( tagRequest );
                HubspotTagResponse tagResponse = JsonConvert.DeserializeObject<HubspotTagResponse>( jsonResponse.Content );
                if (tagResponse.results.Count() > 0)
                {
                    tag_ids.Add( tagResponse.results[0].id );
                    tagDict.Add( tagResponse.results[0].id, tags[i] );
                }
            }

            if (tag_ids.Count() > 0)
            {
                //Get blog posts that match
                string url = "https://api.hubapi.com/cms/v3/blogs/posts?sort=-publishDate&state=PUBLISHED&content_group_id=14822403917";
                if (tag_ids.Count() > 0)
                {
                    url += "&tagId__in=" + String.Join( ",", tag_ids );
                }
                var postClient = new RestClient( url );
                postClient.Timeout = -1;
                var postRequest = new RestRequest( Method.GET );
                postRequest.AddHeader( "Authorization", $"Bearer {key}" );
                IRestResponse jsonResponse = postClient.Execute( postRequest );
                HubspotBlogResponse blogResponse = JsonConvert.DeserializeObject<HubspotBlogResponse>( jsonResponse.Content );
                var posts = blogResponse.results.Select( e =>
                {
                    var p = new Post() { Title = e.name, Author = e.authorName, Image = e.featuredImage, Url = e.url, PublishDate = e.publishDate.Value };
                    List<string> matchingTags = new List<string>();
                    for (var k = 0; k < e.tagIds.Count(); k++)
                    {
                        if (tagDict.ContainsKey( e.tagIds[k] ))
                        {
                            matchingTags.Add( tagDict[e.tagIds[k]] );
                        }
                    }
                    p.MatchingTags = matchingTags;
                    return p;
                } );
                return posts.OrderByDescending( p => p.MatchingTags.Count() ).ToList();
            }
            return new List<Post>();
        }

        private Post ConvertWatchToPost( ContentChannelItem e )
        {
            Post p = new Post() { Id = e.Id, Title = e.Title, Author = e.AttributeValues["Author"].ValueFormatted, Image = e.AttributeValues["Image"].Value, PublishDate = e.StartDateTime, Slug = e.PrimarySlug, ContentChannelId = e.ContentChannelId };
            if (e.Attributes.ContainsKey( "Series" ))
            {
                p.Url = watchURL + e.AttributeValues["Series"] + "/" + e.PrimarySlug;
            }
            else
            {
                p.Url = watchURL + e.PrimarySlug;
            }
            var itemTag = _tiSvc.Queryable().Where( ti => ti.EntityGuid == e.Guid && !ti.Tag.OwnerPersonAliasId.HasValue ).Select( ti => ti.Tag.Name.ToLower() ).ToList();
            var intersect = tags.Intersect( itemTag );
            p.MatchingTags = intersect.ToList();
            return p;
        }
        private Post ConvertListenToPost( ContentChannelItem e )
        {
            var p = new Post() { Id = e.Id, Title = e.Title, Author = e.AttributeValues["Author"].ValueFormatted, Image = e.AttributeValues["Image"].Value, PublishDate = e.StartDateTime, Slug = e.PrimarySlug, ContentChannelId = e.ContentChannelId };
            if (e.Attributes.ContainsKey( "Series" ) && e.Attributes.ContainsKey( "Subseries" ))
            {
                p.Url = listenURL + e.AttributeValues["Series"] + "/" + e.AttributeValues["Subseries"] + "/" + e.PrimarySlug;
            }
            else
            {
                p.Url = listenURL + e.PrimarySlug;
            }
            var itemTag = _tiSvc.Queryable().Where( ti => ti.EntityGuid == e.Guid && !ti.Tag.OwnerPersonAliasId.HasValue ).Select( ti => ti.Tag.Name.ToLower() ).ToList();
            var intersect = tags.Intersect( itemTag );
            p.MatchingTags = intersect.ToList();
            return p;
        }

        private List<int> GetPreviouslyViewedContent()
        {
            if (CurrentPerson != null)
            {
                List<int> aliasIds = CurrentPerson.Aliases.Select( pa => pa.Id ).ToList();
                InteractionChannelService ic_svc = new InteractionChannelService( _context );
                List<int> channels = ic_svc.Queryable().Where( ic => ic.ComponentEntityTypeId == 209 && (ic.ChannelEntityId == ccWatch.Id || ic.ChannelEntityId == ccListen.Id) ).Select( ic => ic.Id ).ToList();
                InteractionService int_svc = new InteractionService( _context );
                List<int> viewedContent = int_svc.Queryable().Where( i => i.PersonAliasId.HasValue && i.InteractionComponent.EntityId.HasValue && aliasIds.Contains( i.PersonAliasId.Value ) && channels.Contains( i.InteractionComponent.InteractionChannelId ) ).Select( i => i.InteractionComponent.EntityId.Value ).Distinct().ToList();
                return viewedContent;
            }
            return new List<int>();
        }
        #endregion

        [DotLiquid.LiquidType( "Title", "Author", "Url", "PublishDate", "Image", "ItemGlobalKey", "Slug", "MatchingTags", "ContentChannelId" )]
        private class Post
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Author { get; set; }
            public DateTime PublishDate { get; set; }
            public string Image { get; set; }
            public string Url { get; set; }
            public string ItemGlobalKey { get; set; }
            public string Slug { get; set; }
            public List<string> MatchingTags { get; set; }
            public int ContentChannelId { get; set; }
        }

        private class HubspotBlogResponse
        {
            public int total { get; set; }
            public List<BlogPost> results { get; set; }
        }

        private class HubspotTagResponse
        {
            public int total { get; set; }
            public List<BlogTag> results { get; set; }
        }

        private class BlogPost
        {
            public string name { get; set; }
            public string authorName { get; set; }
            public string url { get; set; }
            public string featuredImage { get; set; }
            public DateTime? publishDate { get; set; }
            public List<string> tagIds { get; set; }
        }

        private class BlogTag
        {
            public string id { get; set; }
            public string name { get; set; }
        }

        private class PopularityResult
        {
            public int EntityId { get; set; }
            public int NumViews { get; set; }
        }
    }
}