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

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    [DisplayName( "Resource Search" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Display results of search query" )]
    [ContentChannelField( "Watch Content Channel", required: false )]
    [ContentChannelField( "Listen Content Channel", required: false )]
    [TextField( "Hubspot API Key", required: false )]
    [IntegerField( "Limit", "The max number of posts to display", required: false )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 4 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/FeaturedBlogPosts.lava' %}", "", 5 )]

    public partial class ResourceSearch : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
        private string title { get; set; }
        private List<string> tags { get; set; }
        private string series { get; set; }
        private string author { get; set; }
        private string global { get; set; }
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
            string HubspotAPIKey = GetAttributeValue( "HubspotAPIKey" );
            int? limit = GetAttributeValue( "Limit" ).AsIntegerOrNull();
            _cciSvc = new ContentChannelItemService( _context );
            _ccSvc = new ContentChannelService( _context );
            title = PageParameter( "title" ).ToLower();
            tags = !String.IsNullOrEmpty( PageParameter( "tags" ) ) ? PageParameter( "tags" ).ToLower().Split( ',' ).ToList() : new List<string>();
            series = PageParameter( "series" ).ToLower();
            author = PageParameter( "author" ).ToLower();
            global = PageParameter( "q" ).ToLower();
            List<Post> results = new List<Post>();

            if ( WatchContentChannelGuid.HasValue )
            {
                results.AddRange( SearchContent( WatchContentChannelGuid.Value ) );
            }
            if ( ListenContentChannelGuid.HasValue )
            {
                results.AddRange( SearchContent( ListenContentChannelGuid.Value ) );
            }
            if ( !String.IsNullOrEmpty( HubspotAPIKey ) )
            {
                results.AddRange( SearchRead( HubspotAPIKey ) );
            }

            if ( limit.HasValue )
            {
                results = results.Take( limit.Value ).ToList();
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Posts", results.OrderByDescending( p => p.PublishDate ).ToList() );

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
                //var itemTag = i.AttributeValues["Tags"].Value.ToLower().Split( ',' ).ToList();
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
                if ( !String.IsNullOrEmpty( series ) )
                {
                    if ( !itemSeries.Contains( series ) )
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
                //var itemTag = e.AttributeValues["Tags"].Value.Split( ',' ).ToList();
                var itemTag = _tiSvc.Get( 0, "", "", null, e.Guid ).Select( ti => ti.Tag.Name.ToLower() ).ToList();
                var intersect = tags.Intersect( itemTag );
                p.MatchingTags = intersect.ToList();
                return p;
            } ).ToList();
        }

        private List<Post> SearchRead( string apiKey )
        {
            //Get blog posts that match
            WebRequest request = WebRequest.Create( "https://api.hubapi.com/contentsearch/v2/search?portalId=6480645&term=" + global + "&type=BLOG_POST&state=PUBLISHED" );
            var response = request.GetResponse();
            HubspotBlogResponse blogResponse = new HubspotBlogResponse();
            using ( Stream stream = response.GetResponseStream() )
            {
                using ( StreamReader reader = new StreamReader( stream ) )
                {
                    var jsonResponse = reader.ReadToEnd();
                    blogResponse = JsonConvert.DeserializeObject<HubspotBlogResponse>( jsonResponse );
                    var posts = blogResponse.results.Select( e =>
                    {
                        var p = new Post() { Id = 0, Title = e.name, Author = e.authorName, Image = e.featuredImage, Url = e.url, Type = "Read" };
                        if ( e.publishDate.HasValue )
                        {
                            p.PublishDate = e.publishDate.Value;
                        }
                        List<string> matchingTags = new List<string>();
                        var intersect = tags.Intersect( e.tags );
                        p.MatchingTags = intersect.ToList();
                        return p;
                    } );
                    return posts.ToList();
                }
            }
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
            public string name { get; set; }
            public string authorName { get; set; }
            public string url { get; set; }
            public string featuredImage { get; set; }
            public DateTime? publishDate { get; set; }
            public List<string> tags { get; set; }
        }
    }
}