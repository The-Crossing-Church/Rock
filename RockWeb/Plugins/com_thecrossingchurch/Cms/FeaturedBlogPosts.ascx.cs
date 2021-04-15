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

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Featured Blog Posts" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Pulls most recently published blog posts from HubSpot" )]
    [TextField( "HubSpot API Key", required: true, order: 0 )]
    [IntegerField( "Number of Posts", required: true, order: 1, defaultValue: 6 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 2 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/FeaturedBlogPosts.lava' %}", "", 3 )]

    public partial class FeaturedBlogPosts : Rock.Web.UI.RockBlock
    {
        #region Variables
        private string apiKey { get; set; }
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
            apiKey = GetAttributeValue( "HubSpotAPIKey" );
            numPosts = GetAttributeValue( "NumberofPosts" ).AsInteger();
            if ( !String.IsNullOrEmpty( apiKey ) )
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
            WebRequest request = WebRequest.Create( "https://api.hubapi.com/cms/v3/blogs/posts?hapikey=" + apiKey + "&sort=-publishDate&limit=" + numPosts );
            var response = request.GetResponse();
            BlogResponse blogResponse = new BlogResponse();
            using ( Stream stream = response.GetResponseStream() )
            {
                using ( StreamReader reader = new StreamReader( stream ) )
                {
                    var jsonResponse = reader.ReadToEnd();
                    blogResponse = JsonConvert.DeserializeObject<BlogResponse>( jsonResponse );
                }
            }

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Posts", blogResponse.results );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }
        #endregion
        public class BlogResponse
        {
            public int total { get; set; }
            public List<BlogPost> results { get; set; }
        }

        [DotLiquid.LiquidType( "name", "authorName", "url", "publishDate" )]
        public class BlogPost
        {
            public string name { get; set; }
            public string authorName { get; set; }
            public string url { get; set; }
            public DateTime? publishDate { get; set; }
        }
    }
}