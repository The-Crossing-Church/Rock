using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Z.EntityFramework.Plus;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Rock.Web.Cache;
using RestSharp;

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    [DisplayName( "Resource Search" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [TextField( "Hubspot Key Attribute", "", true, "HubspotPrivateAppKey", order: 1 )]
    [Description( "Display results of search query" )]
    [ContentChannelField( "Watch Content Channel", required: false, order: 2 )]
    [ContentChannelField( "Listen Content Channel", required: false, order: 3 )]
    [IntegerField( "Limit", "The max number of posts to display", required: false, order: 4 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 5 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/FeaturedBlogPosts.lava' %}", "", 6 )]

    public partial class ResourceSearch : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
        private string title { get; set; }
        private List<string> contentType { get; set; }
        private List<string> tags { get; set; }
        private List<string> series { get; set; }
        private string author { get; set; }
        private string global { get; set; }
        private string key { get; set; }
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
            Guid? WatchContentChannelGuid = GetAttributeValue( "WatchContentChannel" ).AsGuidOrNull();
            Guid? ListenContentChannelGuid = GetAttributeValue( "ListenContentChannel" ).AsGuidOrNull();
            string attrKey = GetAttributeValue( "HubspotKeyAttribute" );
            key = GlobalAttributesCache.Get().GetValue( attrKey );
            int? limit = GetAttributeValue( "Limit" ).AsIntegerOrNull();
            _cciSvc = new ContentChannelItemService( _context );
            _ccSvc = new ContentChannelService( _context );
            title = PageParameter( "title" ).ToLower();
            tags = !String.IsNullOrEmpty( PageParameter( "tags" ) ) ? PageParameter( "tags" ).ToLower().Split( ',' ).ToList() : new List<string>();
            series = !String.IsNullOrEmpty( PageParameter( "series" ) ) ? PageParameter( "series" ).ToLower().Split( ',' ).ToList() : new List<string>();
            contentType = !String.IsNullOrEmpty( PageParameter( "contenttype" ) ) ? PageParameter( "contenttype" ).ToLower().Split( ',' ).ToList() : new List<string>();
            author = PageParameter( "author" ).ToLower();
            global = PageParameter( "q" ).ToLower();
            List<Post> results = new List<Post>();

            if ( WatchContentChannelGuid.HasValue && ( contentType.Count() == 0 || contentType.Contains( "watch" ) ) )
            {
                results.AddRange( SearchContent( WatchContentChannelGuid.Value ) );
            }
            if ( ListenContentChannelGuid.HasValue && ( contentType.Count() == 0 || contentType.Contains( "listen" ) ) )
            {
                results.AddRange( SearchContent( ListenContentChannelGuid.Value ) );
            }
            if ( !String.IsNullOrEmpty( key ) && ( contentType.Count() == 0 || contentType.Contains( "read" ) ) )
            {
                results.AddRange( SearchRead() );
            }

            results = results.OrderByDescending( r => r.PublishDate ).ToList();

            if ( limit.HasValue )
            {
                results = results.Take( limit.Value ).ToList();
            }

            //Limit items if there was no search filter
            if ( String.IsNullOrEmpty( title ) && tags.Count() == 0 && series.Count() == 0 && contentType.Count() == 0 && String.IsNullOrEmpty( author ) && String.IsNullOrEmpty( global ) )
            {
                results = results.Take( 50 ).ToList();
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Posts", results );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        #endregion

        #region Methods
        private List<Post> SearchContent( Guid guid )
        {
            ContentChannel channel = _ccSvc.Get( guid );
            TaggedItemService _tiSvc = new TaggedItemService( _context );
            channel.LoadAttributes();
            //Filter by Content Channel and Title (if present in query) 
            var items = _cciSvc.Queryable().Where( i => i.ContentChannelId == channel.Id && ( String.IsNullOrEmpty( title ) || i.Title.ToLower().Contains( title ) ) ).ToList();
            items.LoadAttributes();
            items = items.Where( i =>
            {
                bool meetsRec = true;
                var itemTag = _tiSvc.Get( 0, "", "", null, i.Guid ).Select( ti => ti.Tag.Name.ToLower() ).ToList();
                var itemSeries = i.AttributeValues["Series"].Value.ToLower();
                var itemAuthor = i.AttributeValues["Author"].ValueFormatted.ToLower();
                var itemDesc = i.Content != null ? i.Content.ToLower() : "";

                if ( channel.RequiresApproval && i.Status != ContentChannelItemStatus.Approved )
                {
                    meetsRec = false;
                }
                if ( DateTime.Compare( i.StartDateTime, RockDateTime.Now ) > 0 || ( i.ExpireDateTime.HasValue && DateTime.Compare( i.ExpireDateTime.Value, RockDateTime.Now ) <= 0 ) )
                {
                    meetsRec = false;
                }

                if ( tags.Count() > 0 )
                {

                    var intersect = tags.Intersect( itemTag );
                    if ( intersect.Count() == 0 )
                    {
                        meetsRec = false;
                    }
                }
                if ( series.Count() > 0 )
                {
                    if ( !series.Contains( itemSeries ) )
                    {
                        meetsRec = false;
                    }
                }
                if ( !String.IsNullOrEmpty( author ) )
                {
                    if ( !itemAuthor.Contains( author ) )
                    {
                        meetsRec = false;
                    }
                }

                if ( !string.IsNullOrEmpty( global ) && meetsRec )
                {
                    List<string> queryParts = new List<string>();
                    if ( global.Contains( "," ) )
                    {
                        queryParts = global.Split( ',' ).ToList();
                    }
                    else
                    {
                        queryParts.Add( global );
                    }
                    bool hasGlobalPart = false;
                    for ( int k = 0; k < queryParts.Count(); k++ )
                    {
                        if ( i.Title.ToLower().Contains( queryParts[k] ) || itemTag.Contains( queryParts[k] ) || itemSeries.Contains( queryParts[k] ) || itemAuthor.Contains( queryParts[k] ) || itemDesc.Contains( queryParts[k] ) )
                        {
                            hasGlobalPart = true;
                        }
                    }
                    meetsRec = hasGlobalPart;
                }
                return meetsRec;
            } ).ToList();

            return items.Select( e =>
            {
                var p = new Post() { Id = e.Id, Title = e.Title, Author = e.AttributeValues["Author"].ValueFormatted, Image = e.AttributeValues["Image"].Value, Url = e.AttributeValues["Link"].Value, PublishDate = e.StartDateTime, ItemGlobalKey = e.ItemGlobalKey, Slug = e.PrimarySlug, ContentChannelId = e.ContentChannelId, Type = channel.AttributeValues["ContentType"].Value };
                var itemTag = _tiSvc.Get( 0, "", "", null, e.Guid ).Select( ti => ti.Tag.Name.ToLower() ).ToList();
                var intersect = tags.Intersect( itemTag );
                p.MatchingTags = intersect.ToList();
                return p;
            } ).ToList();
        }

        private List<Post> SearchRead()
        {
            //Get blog posts that match
            var postClient = new RestClient( "https://api.hubapi.com/contentsearch/v2/search?portalId=6480645&term=" + global + "&type=BLOG_POST&state=PUBLISHED&domain=info.thecrossingchurch.com" );
            postClient.Timeout = -1;
            var postRequest = new RestRequest( Method.GET );
            postRequest.AddHeader( "Authorization", $"Bearer {key}" );
            IRestResponse jsonResponse = postClient.Execute( postRequest );
            HubspotBlogResponse blogResponse = JsonConvert.DeserializeObject<HubspotBlogResponse>( jsonResponse.Content );
            var posts = blogResponse.results.Select( e =>
            {
                var p = new Post() { Id = 0, Title = e.title.Replace( " - The Crossing Blog", "" ), Author = e.authorFullName, Image = e.featuredImageUrl, Url = e.url, Type = "Read" };
                if ( e.publishedDate.HasValue )
                {
                    //Convert Epoch Time
                    DateTime start = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
                    start = start.AddMilliseconds( e.publishedDate.Value );
                    //Convert Time Zone
                    start = start.ToLocalTime();
                    p.PublishDate = start;
                }
                List<string> matchingTags = new List<string>();
                var intersect = tags.Intersect( e.tags );
                p.MatchingTags = intersect.ToList();
                return p;
            } );
            return posts.ToList();
        }

        #endregion

        [DotLiquid.LiquidType( "Id", "Title", "Author", "Url", "PublishDate", "Image", "ItemGlobalKey", "Slug", "MatchingTags", "ContentChannelId", "Type" )]
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
            public string Type { get; set; }
        }

        private class HubspotBlogResponse
        {
            public int total { get; set; }
            public List<BlogPost> results { get; set; }
        }

        private class BlogPost
        {
            public string title { get; set; }
            public string authorFullName { get; set; }
            public string url { get; set; }
            public string featuredImageUrl { get; set; }
            public double? publishedDate { get; set; }
            public List<string> tags { get; set; }
        }
    }
}