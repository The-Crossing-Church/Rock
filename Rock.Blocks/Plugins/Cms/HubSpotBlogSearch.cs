using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.Cache;
using RestSharp;
using Newtonsoft.Json;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Plugins.Cms
{
    #region BlockAttributes
    [TextField( "Global Attribute Key", "The attribute key of the Global attribute that stores the HubSpot Private App Secret", true, "", key: AttributeKey.HubSpotAttributeKey )]
    [LavaCommandsField( "Enabled Lava Commands", "", false, key: AttributeKey.LavaCommands )]
    [CodeEditorField( "Item Template", "", mode: CodeEditorMode.Lava, key: AttributeKey.LavaTemplate, defaultValue: "" )]
    #endregion
    internal class HubSpotBlogSearch : RockObsidianBlockType
    {
        #region Keys
        private static class AttributeKey
        {
            public const string HubSpotAttributeKey = "HubSpotAttributeKey";
            public const string LavaCommands = "LavaCommands";
            public const string LavaTemplate = "LavaTemplate";
        }
        #endregion

        #region Properties
        private string hubSpotKey { get; set; }
        private string lavaTemplate { get; set; }
        #endregion

        #region Obsidian Block Type Overrides
        /// <summary>
        /// Gets the property values that will be sent to the browser.
        /// </summary>
        /// <returns>
        /// A collection of string/object pairs.
        /// </returns>
        public override object GetObsidianBlockInitialization()
        {
            HubSpotSearchViewModel viewModel = new HubSpotSearchViewModel();
            return viewModel;
        }
        #endregion

        #region Block Actions

        [BlockAction]
        public BlockActionResult GetSearchResults( string q, List<string> tags )
        {
            try
            {
                SetProperties();
                HubSpotSearchViewModel viewModel = new HubSpotSearchViewModel();
                if ( !String.IsNullOrEmpty( q ) )
                {
                    var posts = SearchBlog( q, tags );
                    Dictionary<string, object> mergeFields = new Dictionary<string, object>();
                    mergeFields.Add( "Items", posts );
                    mergeFields.Add( "Query", q );
                    viewModel.items = posts;
                    viewModel.result = lavaTemplate.ResolveMergeFields( mergeFields, GetAttributeValue( AttributeKey.LavaCommands ) );
                }
                return ActionOk( viewModel );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex );
                return ActionBadRequest( ex.Message );
            }
        }

        #endregion

        #region Helpers
        private void SetProperties()
        {
            hubSpotKey = Rock.Security.Encryption.DecryptString( GlobalAttributesCache.Get().GetValue( GetAttributeValue( AttributeKey.HubSpotAttributeKey ) ) );
            lavaTemplate = GetAttributeValue( AttributeKey.LavaTemplate );
        }
        private List<BlogPost> SearchBlog( string q, List<string> tags )
        {

            //Get blog posts that match
            var postClient = new RestClient( "https://api.hubapi.com/contentsearch/v2/search?portalId=6480645&term=" + q + "," + String.Join( ",", tags ) + "&type=BLOG_POST&state=PUBLISHED&domain=info.thecrossingchurch.com" );
            postClient.Timeout = -1;
            var postRequest = new RestRequest( Method.GET );
            postRequest.AddHeader( "Authorization", $"Bearer {hubSpotKey}" );
            IRestResponse jsonResponse = postClient.Execute( postRequest );
            HubspotBlogResponse blogResponse = JsonConvert.DeserializeObject<HubspotBlogResponse>( jsonResponse.Content );
            return blogResponse.results.Select( e =>
            {
                if ( e.publishedDate.HasValue )
                {
                    //Convert Epoch Time
                    DateTime start = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );
                    start = start.AddMilliseconds( e.publishedDate.Value );
                    //Convert Time Zone
                    start = start.ToLocalTime();
                    e.publishDate = start;
                }
                e.title = e.title.Replace( " - The Crossing Blog", "" );
                return e;
            } ).ToList();
        }
        private class HubspotBlogResponse
        {
            public int total { get; set; }
            public List<BlogPost> results { get; set; }
        }

        [DotLiquid.LiquidType( "title", "authorFullName", "url", "featuredImageUrl", "publishedDate", "tags", "publishDate" )]
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
        private class HubSpotSearchViewModel
        {
            public string result { get; set; }
            public List<BlogPost> items { get; set; }
        }
        #endregion
    }
}
