﻿using System;
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
    [DisplayName( "Related Resources" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Pulls Watch, Listen, Read Content with similar tags" )]
    [TextField( "HubSpot API Key", required: true, order: 0 )]
    [IntegerField( "Number of Posts", required: true, order: 1, defaultValue: 6 )]
    [ContentChannelField( "Watch Content Channel", required: true, order: 2 )]
    [ContentChannelField( "Listen Content Channel", required: true, order: 3 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 4 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/FeaturedBlogPosts.lava' %}", "", 5 )]

    public partial class RelatedResources : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private string apiKey { get; set; }
        private int numPosts { get; set; }
        private ContentChannel ccWatch { get; set; }
        private ContentChannel ccListen { get; set; }
        private List<string> tags { get; set; }
        private int itemId { get; set; }
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
            apiKey = GetAttributeValue( "HubSpotAPIKey" );
            numPosts = GetAttributeValue( "NumberofPosts" ).AsInteger();
            string watchGuid = GetAttributeValue( "WatchContentChannel" );
            if ( !String.IsNullOrEmpty( watchGuid ) )
            {
                ccWatch = new ContentChannelService( _context ).Get( watchGuid.AsGuid() );
            }
            string listenGuid = GetAttributeValue( "ListenContentChannel" );
            if ( !String.IsNullOrEmpty( listenGuid ) )
            {
                ccListen = new ContentChannelService( _context ).Get( listenGuid.AsGuid() );
            }
            string itemGlobalKey = PageParameter( "Slug" );
            ContentChannelItem item = new ContentChannelItemService( _context ).Queryable().FirstOrDefault( i => i.ItemGlobalKey == itemGlobalKey );
            if ( item != null )
            {
                item.LoadAttributes();
                itemId = item.Id;
                var attrVal = item.AttributeValues.FirstOrDefault( av => av.Key == "Tags" );
                if ( attrVal.Value != null )
                {
                    tags = attrVal.Value.Value.Split( ',' ).ToList();
                    if ( tags.Count() > 0 )
                    {
                        GetContent();
                    }
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
            List<Post> listen = new List<Post>();
            if ( !String.IsNullOrEmpty( apiKey ) )
            {
                read = GetBlogPosts();
            }
            if ( ccWatch != null )
            {
                watch = GetWatch();
            }
            if ( ccListen != null )
            {
                listen = GetListen();
            }

            //Roughly even number of posts from each content type
            var evenDist = numPosts / 3;
            var numRead = numPosts / 3;
            var numListen = numPosts / 3;
            var numWatch = numPosts / 3;

            //Not enough read content
            if ( read.Count() < evenDist )
            {
                var diff = evenDist - read.Count();
                numRead = read.Count();
                numListen += ( diff / 2 );
                numWatch += ( diff / 2 );
            }
            //Not enough listen content
            if ( listen.Count() < evenDist )
            {
                var diff = evenDist - listen.Count();
                numListen = listen.Count();
                numRead += ( diff / 2 );
                numWatch += ( diff / 2 );
            }
            //Not enough watch content
            if ( watch.Count() < evenDist )
            {
                var diff = evenDist - watch.Count();
                numWatch = watch.Count();
                numRead += ( diff / 2 );
                numListen += ( diff / 2 );
            }

            content.AddRange( read.OrderByDescending( e => e.PublishDate ).Take( numRead ) );
            content.AddRange( watch.OrderByDescending( e => e.PublishDate ).Take( numWatch ) );
            content.AddRange( listen.OrderByDescending( e => e.PublishDate ).Take( numListen ) );

            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Posts", content );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        private List<Post> GetWatch()
        {
            var items = new ContentChannelItemService( _context ).Queryable().Where( i => i.ContentChannelId == ccWatch.Id ).ToList();
            items.LoadAttributes();
            items = items.Where( i =>
            {
                if ( i.Id != itemId )
                {
                    var itemTag = i.AttributeValues["Tags"].Value.Split( ',' ).ToList();
                    var intersect = tags.Intersect( itemTag );
                    if ( intersect.Count() > 0 )
                    {
                        return true;
                    }
                }
                return false;
            } ).ToList();
            return items.Select( e =>
            {
                var p = new Post() { Title = e.Title, Author = e.AttributeValues["Author"].ValueFormatted, Image = e.AttributeValues["Image"].Value, Url = e.AttributeValues["Link"].Value, PublishDate = e.StartDateTime, ItemGlobalKey = e.ItemGlobalKey, ContentChannelId = e.ContentChannelId };
                var itemTag = e.AttributeValues["Tags"].Value.Split( ',' ).ToList();
                var intersect = tags.Intersect( itemTag );
                p.MatchingTags = intersect.ToList();
                return p;
            } ).ToList();
        }

        private List<Post> GetListen()
        {
            var items = new ContentChannelItemService( _context ).Queryable().Where( i => i.ContentChannelId == ccListen.Id ).ToList();
            items.LoadAttributes();
            items = items.Where( i =>
            {
                if ( i.Id != itemId )
                {
                    var itemTag = i.AttributeValues["Tags"].Value.Split( ',' ).ToList();
                    var intersect = tags.Intersect( itemTag );
                    if ( intersect.Count() > 0 )
                    {
                        return true;
                    }
                }
                return false;
            } ).ToList();
            return items.Select( e =>
            {
                var p = new Post() { Title = e.Title, Author = e.AttributeValues["Author"].ValueFormatted, Image = e.AttributeValues["Image"].Value, Url = e.AttributeValues["Link"].Value, PublishDate = e.StartDateTime, ItemGlobalKey = e.ItemGlobalKey, ContentChannelId = e.ContentChannelId };
                var itemTag = e.AttributeValues["Tags"].Value.Split( ',' ).ToList();
                var intersect = tags.Intersect( itemTag );
                p.MatchingTags = intersect.ToList();
                return p;
            } ).ToList();
        }

        private List<Post> GetBlogPosts()
        {
            //Get the Hubspot Tags
            List<string> tag_ids = new List<string>();
            Dictionary<string, string> tagDict = new Dictionary<string, string>();
            for ( int i = 0; i < tags.Count(); i++ )
            {
                WebRequest tagrequest = WebRequest.Create( "https://api.hubapi.com/cms/v3/blogs/tags?hapikey=" + apiKey + "&name=" + tags[i] );
                var tagresponse = tagrequest.GetResponse();
                HubspotTagResponse tagResponse = new HubspotTagResponse();
                using ( Stream stream = tagresponse.GetResponseStream() )
                {
                    using ( StreamReader reader = new StreamReader( stream ) )
                    {
                        var jsonResponse = reader.ReadToEnd();
                        tagResponse = JsonConvert.DeserializeObject<HubspotTagResponse>( jsonResponse );
                        if ( tagResponse.results.Count() > 0 )
                        {
                            tag_ids.Add( tagResponse.results[0].id );
                            tagDict.Add( tagResponse.results[0].id, tags[i] );
                        }
                    }
                }
            }

            if ( tag_ids.Count() > 0 )
            {
                //Get blog posts that match
                string url = "https://api.hubapi.com/cms/v3/blogs/posts?hapikey=" + apiKey + "&sort=-publishDate";
                for ( int i = 0; i < tag_ids.Count(); i++ )
                {
                    url += "&topic_id__in=" + tag_ids[i];
                }
                WebRequest request = WebRequest.Create( url );
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
                            var p = new Post() { Title = e.name, Author = e.authorName, Image = e.featuredImage, Url = e.url, PublishDate = e.publishDate.Value };
                            List<string> matchingTags = new List<string>();
                            for ( var k = 0; k < e.tagIds.Count(); k++ )
                            {
                                if ( tagDict.ContainsKey( e.tagIds[k] ) )
                                {
                                    matchingTags.Add( tagDict[e.tagIds[k]] );
                                }
                            }
                            p.MatchingTags = matchingTags;
                            return p;
                        } );
                        return posts.ToList();
                    }
                }
            }
            return new List<Post>();
        }
        #endregion

        [DotLiquid.LiquidType( "Title", "Author", "Url", "PublishDate", "Image", "ItemGlobalKey", "MatchingTags", "ContentChannelId" )]
        private class Post
        {
            public string Title { get; set; }
            public string Author { get; set; }
            public DateTime PublishDate { get; set; }
            public string Image { get; set; }
            public string Url { get; set; }
            public string ItemGlobalKey { get; set; }
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
    }
}