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
using Rock.SystemGuid;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Web.Services;
using Rock.Web.Cache;
using RestSharp;

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    [DisplayName( "External Site Search" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Display results of search query" )]
    [ContentChannelsField( "Enabled Channels", "Channels that should be searched", true, category: "Searchable Entities", order: 0 )]
    [TextField( "Hubspot Key Attribute", "", false, "", category: "Searchable Entities", order: 1 )]
    [TextField( "Page Ids", "Comma seperated list of page ids to search", required: false, category: "Searchable Entities", order: 2 )]
    [EventCalendarField( "Calendar", "", false, "8A444668-19AF-4417-9C74-09F842572974", order: 2 )]
    [LinkedPage( "Event Details Page", "Detail page for events", order: 3 )]
    [DefinedValueField( "Audiences", Description = "The audiences to include in search", Key = "Audiences", DefinedTypeGuid = Rock.SystemGuid.DefinedType.MARKETING_CAMPAIGN_AUDIENCE_TYPE, AllowMultiple = true, IsRequired = false, Order = 4 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 6 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/' %}", "", 7 )]

    public partial class ExternalSiteSearch : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
        private TaggedItemService _tiSvc { get; set; }
        private TagService _tSvc { get; set; }
        private string title { get; set; }
        private List<string> contentType { get; set; }
        private List<string> tags { get; set; }
        private List<string> series { get; set; }
        private string author { get; set; }
        private string global { get; set; }
        private List<string> globalTerms { get; set; }
        private Dictionary<string, object> mergeFields { get; set; }
        #endregion

        public List<ResultSet> SearchResults
        {
            get
            {
                return ( List<ResultSet> ) ViewState["SearchResults"] ?? new List<ResultSet>();
            }
            set
            {
                ViewState["SearchResults"] = value;
            }
        }

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
            if ( !Page.IsPostBack )
            {
                mergeFields = new Dictionary<string, object>();
                _context = new RockContext();
                _cciSvc = new ContentChannelItemService( _context );
                _ccSvc = new ContentChannelService( _context );
                _tiSvc = new TaggedItemService( _context );
                _tSvc = new TagService( _context );
                title = PageParameter( "title" ).ToLower();
                tags = !String.IsNullOrEmpty( PageParameter( "tags" ) ) ? PageParameter( "tags" ).Split( ',' ).ToList() : new List<string>();
                series = !String.IsNullOrEmpty( PageParameter( "series" ) ) ? PageParameter( "series" ).Split( ',' ).ToList() : new List<string>();
                author = PageParameter( "author" ).ToLower();
                global = PageParameter( "q" ).ToLower();
                globalTerms = global.Split( ' ' ).Where( t => !String.IsNullOrEmpty( t ) ).ToList();
                SearchChannels();
                SearchEvents();
                SearchPages();
                SearchBlog();
                lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
            }
        }

        protected void SearchChannels()
        {
            List<Guid> EnabledChannels = GetAttributeValues( "EnabledChannels" ).AsGuidList();

            List<ResultSet> queryResults = SearchResults;

            if ( !String.IsNullOrEmpty( title ) || !String.IsNullOrEmpty( author ) || series.Count() > 0 || tags.Count() > 0 || !String.IsNullOrEmpty( global ) )
            {
                for ( int i = 0; i < EnabledChannels.Count(); i++ )
                {
                    ContentChannel channel = _ccSvc.Get( EnabledChannels[i] );
                    int skip = 0;
                    int take = 12;
                    var results = SearchChannel( channel, skip, take );
                    mergeFields.Add( channel.Name.Replace( " ", "" ), results );
                }
            }
            //SearchResults = queryResults;
        }

        #endregion

        #region Methods

        private void SearchEvents()
        {
            EventItemService ei_svc = new EventItemService( _context );
            EventCalendarService cal_svc = new EventCalendarService( _context );
            Guid? calGuid = GetAttributeValue( "Calendar" ).AsGuidOrNull();
            var audiences = GetAttributeValues( "Audiences" ).AsGuidOrNullList();
            if ( calGuid.HasValue && audiences.Count() > 0 )
            {
                EventCalendar c = cal_svc.Get( calGuid.Value );
                var events = ei_svc.GetActiveItemsByCalendarId( c.Id ).Where( ei => ei.EventItemAudiences.Select( eia => eia.DefinedValue.Guid ).Any( a => audiences.Contains( a ) ) && globalTerms.Any( t => ei.Name.ToLower().Contains( t ) ) );
                mergeFields.Add( "Events", events.ToList() );
            }
        }

        private void SearchPages()
        {
            if ( globalTerms.Count() > 0 )
            {
                List<int> pageIds = GetAttributeValue( "PageIds" ).Split( ',' ).Select( i => Int32.Parse( i.Trim() ) ).ToList();
                PageService pg_svc = new PageService( _context );
                var pages = pg_svc.Queryable().Where( p => pageIds.Contains( p.Id ) && ( globalTerms.Any( t => p.PageTitle.ToLower().Contains( t ) ) || globalTerms.Any( t => p.Description.ToLower().Contains( t ) ) ) );
                mergeFields.Add( "Pages", pages.ToList() );
            }
        }

        private void SearchBlog()
        {
            string attrKey = GetAttributeValue( "HubspotKeyAttribute" );
            if ( !String.IsNullOrEmpty( attrKey ) )
            {
                string key = GlobalAttributesCache.Get().GetValue( attrKey );

                //Get blog posts that match
                var postClient = new RestClient( "https://api.hubapi.com/contentsearch/v2/search?portalId=6480645&term=" + global + "," + String.Join( ",", tags ) + "&type=BLOG_POST&state=PUBLISHED&domain=info.thecrossingchurch.com" );
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
                mergeFields.Add( "Posts", posts.ToList() );
            }
        }

        private List<ContentChannelItem> SearchChannel( ContentChannel channel, int skip, int take )
        {
            List<ContentChannelItem> items = new List<ContentChannelItem>();
            int total = 0;
            if ( channel != null )
            {
                IQueryable<ContentChannelItem> query = _cciSvc.Queryable().Where( cci => cci.ContentChannelId == channel.Id && cci.StartDateTime <= RockDateTime.Now && ( !cci.ExpireDateTime.HasValue || cci.ExpireDateTime.Value > RockDateTime.Now ) );
                List<QueryResult> matchedResults = new List<QueryResult>();

                var titleTerms = new List<string>( globalTerms );
                if ( !String.IsNullOrEmpty( title ) )
                {
                    titleTerms.Add( title );
                }
                foreach ( string term in titleTerms )
                {
                    matchedResults.AddRange( GetTitleEntityIds( channel.Guid.ToString(), term ) );
                }

                var aTerms = new List<string>( globalTerms );
                if ( !String.IsNullOrEmpty( author ) )
                {
                    aTerms.Add( author );
                }
                foreach ( string t in aTerms )
                {
                    matchedResults.AddRange( GetAuthorEntityIds( channel.Guid.ToString(), t ) );
                }

                foreach ( string s in series )
                {
                    matchedResults.AddRange( GetSeriesMatchEntityIds( channel.Guid.ToString(), s ) );
                }
                foreach ( string t in globalTerms )
                {
                    matchedResults.AddRange( GetSeriesEntityIds( channel.Guid.ToString(), t ) );
                }

                var tTerms = new List<string>( globalTerms );
                if ( tags.Count() > 0 )
                {
                    tTerms.AddRange( tags );
                }
                foreach ( string t in tTerms )
                {
                    matchedResults.AddRange( GetTagEntityIds( t ) );
                }

                var joined = matchedResults.Join( query,
                    qr => qr.Id,
                    cci => cci.Id,
                    ( qr, cci ) => new { Item = cci, Match = qr }
                );

                int minWeight = CalculateMinWeight();
                var relevantResults = joined.GroupBy( r => r.Item ).Select( r => new { Item = r.Key, NumMatches = r.Count(), Highest = r.Select( m => m.Match.Weight ).Max(), TotalWeight = r.Select( m => m.Match.Weight ).Sum() } ).Where( r => r.TotalWeight >= minWeight );
                items = relevantResults.OrderByDescending( e => e.Item.StartDateTime ).OrderByDescending( e => e.TotalWeight ).Select( e => e.Item ).ToList();
                total = relevantResults.Count();
            }
            return items;
        }

        private int CalculateMinWeight()
        {
            int minWeight = 0;
            int searchQueries = 0;

            searchQueries += globalTerms.Count() > 0 ? 1 : 0;
            searchQueries += series.Count() > 0 ? 1 : 0;
            searchQueries += tags.Count() > 0 ? 1 : 0;
            searchQueries += !String.IsNullOrEmpty( author ) ? 1 : 0;
            searchQueries += !String.IsNullOrEmpty( title ) ? 1 : 0;

            minWeight += !String.IsNullOrEmpty( author ) ? 1 : 0;
            minWeight += !String.IsNullOrEmpty( title ) ? 1 : 0;
            minWeight += series.Count() > 0 ? 3 : 0;
            minWeight += tags.Count() > 0 ? 3 : 0;
            if ( tags.Count() > 3 )
            {
                //If more than 3 tags are searched we want to match at least 2 of them
                minWeight += 3;
            }
            minWeight += globalTerms.Count() > 0 ? 3 : 0;

            return minWeight;
        }

        private List<QueryResult> GetAuthorEntityIds( string channelGuid, string query )
        {
            using ( var context = new RockContext() )
            {
                var results = context.Database.SqlQuery<QueryResult>( $@"
SELECT EntityId AS Id,
       (CASE
            WHEN Distance = 2 THEN 1
            WHEN Distance = 0 THEN 3
            WHEN Value LIKE '%' + @q + '%' OR Distance = 1 THEN 2
           END) AS Weight
FROM (
         SELECT EntityId,
                Value,
                [dbo].[PartialDamerauLevenschtein](Value, @q) AS 'Distance'
         FROM (
                  SELECT avAuthor.EntityId,
                         (CASE
                              WHEN ValueAsPersonId IS NULL THEN Value
                              ELSE CONCAT(NickName, ' ', LastName)
                             END) AS Value,
                         ValueAsPersonId
                  FROM Person
                           RIGHT OUTER JOIN (
                      SELECT AttributeValue.Id,
                             AttributeValue.EntityId,
                             Value,
                             ValueAsPersonId
                      FROM AttributeValue
                               INNER JOIN (
                          SELECT Attribute.Id
                          FROM Attribute
                                   INNER JOIN (
                              SELECT Id, ContentChannelTypeId
                              FROM ContentChannel
                              WHERE Guid = @channelGuid
                          ) cc ON (EntityTypeQualifierColumn = 'ContentChannelId' AND
                                   EntityTypeQualifierValue = cc.Id) OR
                                  (EntityTypeQualifierColumn = 'ContentChannelTypeId' AND
                                   EntityTypeQualifierValue = cc.ContentChannelTypeId)
                          WHERE EntityTypeId = 208
                            AND ([Key] LIKE '%Author%' OR [Key] LIKE '%Speaker%')
                      ) AS attrAuthor ON AttributeId = attrAuthor.Id
                      WHERE Value IS NOT NULL
                  ) AS avAuthor ON ValueAsPersonId = Person.Id
              ) AS entityAuthor
     ) AS entityMatchAuthor
WHERE Value LIKE '%' + @q + '%'
   OR Distance < 3
", new SqlParameter( "@q", query ), new SqlParameter( "@channelGuid", channelGuid ) ).ToList();
                return results;
            }
        }

        private List<QueryResult> GetSeriesEntityIds( string channelGuid, string query )
        {
            using ( var context = new RockContext() )
            {
                var results = context.Database.SqlQuery<QueryResult>( $@"
SELECT EntityId AS Id,
       (CASE
            WHEN Distance = 1 THEN 1
            WHEN Distance = 0 THEN 3
            WHEN Value LIKE '%' + @q + '%' THEN 2
           END) AS Weight
FROM (
         SELECT EntityId,
                Value,
                [dbo].[PartialDamerauLevenschtein](Value, @q) AS 'Distance'
         FROM AttributeValue
                  INNER JOIN (
             SELECT Attribute.Id
             FROM Attribute
                      INNER JOIN (
                 SELECT Id, ContentChannelTypeId
                 FROM ContentChannel
                 WHERE Guid = @channelGuid
             ) cc ON (EntityTypeQualifierColumn = 'ContentChannelId' AND
                      EntityTypeQualifierValue = cc.Id) OR
                     (EntityTypeQualifierColumn = 'ContentChannelTypeId' AND
                      EntityTypeQualifierValue = cc.ContentChannelTypeId)
             WHERE EntityTypeId = 208
               AND ([Key] LIKE '%Series%')
         ) AS attrAuthor ON AttributeId = attrAuthor.Id
         WHERE Value IS NOT NULL
     ) AS entityMatchAuthor
WHERE Value LIKE '%' + @q + '%'
   OR Distance < 2
", new SqlParameter( "@q", query ), new SqlParameter( "@channelGuid", channelGuid ) ).ToList();
                return results;
            }
        }

        private List<QueryResult> GetSeriesMatchEntityIds( string channelGuid, string query )
        {
            using ( var context = new RockContext() )
            {
                var results = context.Database.SqlQuery<QueryResult>( $@"
SELECT EntityId AS Id,
       3 AS Weight
FROM AttributeValue
         INNER JOIN (
    SELECT Attribute.Id
    FROM Attribute
             INNER JOIN (
        SELECT Id, ContentChannelTypeId
        FROM ContentChannel
        WHERE Guid = @channelGuid
    ) cc ON (EntityTypeQualifierColumn = 'ContentChannelId' AND
             EntityTypeQualifierValue = cc.Id) OR
            (EntityTypeQualifierColumn = 'ContentChannelTypeId' AND
             EntityTypeQualifierValue = cc.ContentChannelTypeId)
    WHERE EntityTypeId = 208
      AND ([Key] LIKE '%Series%')
) AS attrAuthor ON AttributeId = attrAuthor.Id
WHERE Value LIKE @q
", new SqlParameter( "@q", query ), new SqlParameter( "@channelGuid", channelGuid ) ).ToList();
                return results;
            }
        }

        private List<QueryResult> GetTitleEntityIds( string channelGuid, string query )
        {
            using ( var context = new RockContext() )
            {
                var results = context.Database.SqlQuery<QueryResult>( $@"
SELECT Id,
       (CASE
            WHEN Distance = 1 THEN 1
            WHEN Distance = 0 THEN 3
            WHEN Title LIKE '%' + @q + '%' THEN 2
           END) AS Weight
FROM (
         SELECT ContentChannelItem.Id,
                ContentChannelItem.Title,
                [dbo].[PartialDamerauLevenschtein](Title, @q) AS 'Distance'
         FROM ContentChannelItem
                  INNER JOIN (
             SELECT Id
             FROM ContentChannel
             WHERE Guid = @channelGuid
         ) AS cc ON cc.Id = ContentChannelItem.ContentChannelId
     ) AS cci
WHERE Title LIKE '%' + @q + '%'
   OR Distance < 2
", new SqlParameter( "@q", query ), new SqlParameter( "@channelGuid", channelGuid ) ).ToList();
                return results;
            }
        }

        private List<QueryResult> GetTagEntityIds( string query )
        {
            using ( var context = new RockContext() )
            {
                var results = context.Database.SqlQuery<QueryResult>( $@"
SELECT DISTINCT Id, (CASE WHEN LOWER(TagName) = @q THEN 4 ELSE 3 END) AS 'Weight' 
FROM ContentChannelItem
        INNER JOIN (
    SELECT EntityGuid, TagName
    FROM TaggedItem
            INNER JOIN (
        SELECT Id, Name AS 'TagName'
        FROM Tag
        WHERE Name LIKE '%' + @q + '%'
        AND CategoryId = 725
        AND (EntityTypeId = 208 OR EntityTypeId IS NULL)
        AND OwnerPersonAliasId IS NULL 
    ) AS tag ON tag.Id = TagId
) AS taggedItem ON Guid = EntityGuid
", new SqlParameter( "@q", query ) ).ToList();
                return results;
            }
        }

        #endregion

        [DotLiquid.LiquidType( "Title", "Id", "Matched", "Tags", "Description" )]
        private class PageResult
        {
            public int Id { get; set; }
            public string Matched { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public List<string> Tags { get; set; }
        }

        [Serializable]
        public class ResultSet
        {
            public Guid channel { get; set; }
            public string mergefield { get; set; }
            public int skip { get; set; }
            public int take { get; set; }
            public int total { get; set; }
        }

        private class QueryResult
        {
            public int Id { get; set; }
            public int Weight { get; set; }
        }

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

        private class HubspotTagResponse
        {
            public int total { get; set; }
            public List<BlogTag> results { get; set; }
        }

        private class BlogPost
        {
            public string title { get; set; }
            public string authorFullName { get; set; }
            public string url { get; set; }
            public string featuredImageUrl { get; set; }
            public double? publishedDate { get; set; }
            public List<string> tags { get; set; }
            //For Alternate API Versions
            public string name { get; set; }
            public string authorName { get; set; }
            public string featuredImage { get; set; }
            public DateTime? publishDate { get; set; }
            public List<string> tagIds { get; set; }
        }

        private class BlogTag
        {
            public string id { get; set; }
            public string name { get; set; }
        }
    }
}