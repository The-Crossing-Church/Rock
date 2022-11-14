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

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Featured Blog Posts" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Pulls most recently published blog posts from HubSpot" )]
    [TextField( "Hubspot Key Attribute", "", true, "HubspotPrivateAppKey", order: 1 )]
    [IntegerField( "Number of Posts", required: true, order: 2, defaultValue: 6 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 3 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/FeaturedBlogPosts.lava' %}", "", 4 )]

    public partial class FeaturedBlogPosts : Rock.Web.UI.RockBlock
    {
        #region Variables
        private string key { get; set; }
        private int numPosts { get; set; }
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
            string attrKey = GetAttributeValue( "HubspotKeyAttribute" );
            key = GlobalAttributesCache.Get().GetValue( attrKey );
            numPosts = GetAttributeValue( "NumberofPosts" ).AsInteger();
            if ( !String.IsNullOrEmpty( key ) )
            {
                GetPosts();
            }

            if ( !Page.IsPostBack )
            {
            }
        }

        #endregion

        #region Methods
        private void GetPosts()
        {
            //Get custom contact properties from Hubspot 
            var postClient = new RestClient( "https://api.hubapi.com/content/api/v2/blog-posts?sort=-publishDate&state=PUBLISHED&content_group_id=14822403917&limit=" + numPosts );
            postClient.Timeout = -1;
            var postRequest = new RestRequest( Method.GET );
            postRequest.AddHeader( "Authorization", $"Bearer {key}" );
            IRestResponse jsonResponse = postClient.Execute( postRequest );
            BlogResponse blogResponse = JsonConvert.DeserializeObject<BlogResponse>( jsonResponse.Content );
            DateTime start = new DateTime( 1970, 1, 1, 0, 0, 0, 0 );

            var posts = blogResponse.objects.Select( b => new BlogPost() { name = b.name, authorName = b.blog_post_author.display_name, url = b.url, featured_image = b.featured_image, publishDate = start.AddMilliseconds( b.publish_date_local_time.Value ) } );

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Posts", posts );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }
        #endregion
        private class BlogResponse
        {
            public int total { get; set; }
            public List<BlogPost> objects { get; set; }
        }

        [DotLiquid.LiquidType( "name", "authorName", "url", "publishDate", "featured_image" )]
        private class BlogPost
        {
            public string name { get; set; }
            public BlogAuthor blog_post_author { get; set; }
            public string authorName { get; set; }
            public string url { get; set; }
            public string featured_image { get; set; }
            public long? publish_date_local_time { get; set; }
            public DateTime? publishDate { get; set; }
        }

        private class BlogAuthor
        {
            public string display_name { get; set; }
        }
    }
}