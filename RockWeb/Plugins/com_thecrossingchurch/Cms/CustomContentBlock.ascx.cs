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

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Custom Content Block" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Block to pull specific content based on authenticated user" )]

    [ContentChannelField( "Content Channel", "The content channel for this section", true, "" )]
    [LavaCommandsField( "Enabled Lava Commands", description: "The Lava commands that should be enabled for this content channel item block.", required: false )]
    [TextField( "Display Type", "Enter one of the following: Single, Cards, Slider", true, "Single" )]

    public partial class CustomContentBlock : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private string displayType { get; set; }
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
            _cciSvc = new ContentChannelItemService( _context );

            if ( !Page.IsPostBack )
            {
                GetContent();
            }
        }

        #endregion

        #region Methods

        private void GetContent()
        {
            //Get all valid content for this block
            var items = _cciSvc.Queryable().Where( cci => DateTime.Compare( cci.StartDateTime, DateTime.Now ) <= 1 && DateTime.Compare( cci.ExpireDateTime.Value, DateTime.Now ) >= 1 ).ToList();
            //Get specific content if we know who is authenticated 
            if ( CurrentPerson != null )
            {
                List<ContentChannelItem> renderItems = new List<ContentChannelItem>();
                for ( var i = 0; i < items.Count(); i++ )
                {
                    var item = items[i];
                    item.LoadAttributes();
                    var dataviews = item.AttributeValues.FirstOrDefault( av => av.Key == "DataViews" ).Value;
                    if ( !String.IsNullOrEmpty( dataviews.Value ) )
                    {
                        for ( var k = 0; k < dataviews.Value.Split( ',' ).Length; k++ )
                        {
                            var dataview = new DataViewService( _context ).Get( Guid.Parse( dataviews.Value.Split( ',' )[k] ) );
                            List<string> errs = new List<string>();
                            var data = dataview.GetQuery( null, null, out errs );
                            var inDataView = data.FirstOrDefault( d => d.Id == CurrentPersonId );
                            if ( inDataView != null )
                            {
                                //We're in business, yeah boi
                                renderItems.Add( item );
                            }
                        }
                    }
                }
                RenderContent( renderItems );
            }
            //Get generic content
            else
            {
                items.LoadAttributes();
                var generic = items.Where( itm => itm.AttributeValues.Any( av => av.Key == "DataViews" && String.IsNullOrEmpty( av.Value.Value ) ) ).ToList();
                RenderContent( generic );
            }
        }

        private void RenderContent( List<ContentChannelItem> items )
        {
            //Resolve Merge fields
            var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new Rock.Lava.CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );

            var itemMergeFields = new Dictionary<string, object>( commonMergeFields );

            var enabledCommands = GetAttributeValue( "EnabledLavaCommands" );

            displayType = GetAttributeValue( "DisplayType" );
            ContentChannelItem item;
            string html = "";
            switch ( displayType )
            {
                case "Cards":
                    html = "<div class='row equal'>";
                    for ( var i = 0; i < items.Count(); i++ )
                    {
                        html += "<div class='col col-xs-12 col-sm-6 col-md-4 col-xl-3'>";
                        item = items[i];
                        itemMergeFields.AddOrReplace( "Item", item );
                        item.Content = item.Content.ResolveMergeFields( itemMergeFields, enabledCommands );
                        html += item.Content; 
                        html += "</div>";
                    }
                    html += "</div>";
                    content.InnerHtml = html; 
                    break;
                case "Slider":
                    html = "<div id='customContentCarousel' class='carousel slide' data-ride='carousel'>";
                    html += "<div class='carousel-inner'>";
                    for ( var i = 0; i < items.Count(); i++ )
                    {
                        html += "<div class='item";
                        if(i == 0 )
                        {
                            html += " active"; 
                        }
                        html += "'>";
                        item = items[i];
                        itemMergeFields.AddOrReplace( "Item", item );
                        item.Content = item.Content.ResolveMergeFields( itemMergeFields, enabledCommands );
                        html += item.Content;
                        html += "</div>";
                    }
                    html += "</div>"; //End carousel-inner
                    html += "<a class='carousel-control left' href='#customContentCarousel' role='button' data-slide='prev'>";
                    html += "<span class='icon-prev' aria-hidden='true' ></span>";
                    html += "<span class='sr-only'>Previous</span>";
                    html += "</a>";
                    html += "<a class='carousel-control right' href='#customContentCarousel' role='button' data-slide='next'>";
                    html += "<span class='icon-next' aria-hidden='true' ></span>";
                    html += "<span class='sr-only'>Next</span>";
                    html += "</a>";
                    html += "</div>";
                    content.InnerHtml = html;
                    break;
                default:
                    //Single featured item exactly as is, take newest item by deafult 
                    item = items.OrderByDescending( i => i.CreatedDateTime ).First();
                    itemMergeFields.AddOrReplace( "Item", item );
                    item.Content = item.Content.ResolveMergeFields( itemMergeFields, enabledCommands );
                    content.InnerHtml = item.Content;
                    break;
            }

        }

        #endregion

    }

}