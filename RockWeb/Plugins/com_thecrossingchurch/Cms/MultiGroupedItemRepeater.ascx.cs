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

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    [DisplayName( "Multi Grouped Item Repeater" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Similar to Content Item Repeater but Groups Items" )]
    [ContentChannelsField( "Content Channels", required: true, order: 0 )]
    [TextField( "Attribute Key", required: true, defaultValue: "Series", order: 1 )]
    [TextField( "Filter Attribute Key", required: false, order: 2 )]
    [TextField( "Filter Page Parameter", required: false, order: 3 )]
    [IntegerField( "Limit", required: false, description: "Limit the number of items in each group to return", order: 4 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 5 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/WatchSeries.lava' %}", "", order: 6 )]

    public partial class MultiGroupedItemRepeater : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
        private ContentChannel channel { get; set; }
        private string AttributeKey { get; set; }
        private string FilterAttributeKey { get; set; }
        private string FilterPageParameterKey { get; set; }
        private string FilterPageParameterValue { get; set; }
        private int? Limit { get; set; }
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
            Guid? ContentChannelGuid = GetAttributeValue( "ContentChannel" ).AsGuidOrNull();
            _cciSvc = new ContentChannelItemService( _context );
            _ccSvc = new ContentChannelService( _context );
            AttributeKey = GetAttributeValue( "AttributeKey" );
            FilterAttributeKey = GetAttributeValue( "FilterAttributeKey" );
            FilterPageParameterKey = GetAttributeValue( "FilterPageParameter" );
            if ( !String.IsNullOrEmpty( FilterPageParameterKey ) )
            {
                FilterPageParameterValue = PageParameter( FilterPageParameterKey );
            }
            Limit = GetAttributeValue( "Limit" ).AsIntegerOrNull();
            if ( ContentChannelGuid.HasValue )
            {
                channel = _ccSvc.Get( ContentChannelGuid.Value );
                LoadItems( ContentChannelGuid.Value );
            }
            if ( !Page.IsPostBack )
            {
            }
        }

        #endregion

        #region Methods
        private void LoadItems( Guid guid )
        {
            int id = _ccSvc.Get( guid ).Id;
            var items = _cciSvc.Queryable().Where( i => i.ContentChannelId == id && DateTime.Compare( i.StartDateTime, RockDateTime.Now ) <= 0 && ( !i.ExpireDateTime.HasValue || DateTime.Compare( i.ExpireDateTime.Value, RockDateTime.Now ) >= 0 ) ).ToList();
            items.LoadAttributes();
            if ( !String.IsNullOrEmpty( FilterAttributeKey ) && !String.IsNullOrEmpty( FilterPageParameterValue ) )
            {
                items = items.Where( i => i.AttributeValues.ContainsKey( FilterAttributeKey ) && i.AttributeValues[FilterAttributeKey].Value == FilterPageParameterValue ).ToList();
            }
            List<ItemGroup> groupedItems = new List<ItemGroup>();
            for ( int i = 0; i < items.Count(); i++ )
            {
                var series = items[i].AttributeValues[AttributeKey].Value.Split( ',' );
                for ( int k = 0; k < series.Length; k++ )
                {
                    var idx = groupedItems.Select( gi => gi.Series ).ToList().IndexOf( series[k] );
                    if ( idx >= 0 )
                    {
                        groupedItems[idx].Items.Add( items[i] );
                    }
                    else
                    {
                        groupedItems.Add( new ItemGroup { Series = series[k], Items = new List<ContentChannelItem>() { items[i] } } );
                    }
                }
            }
            //Sort items
            for ( int i = 0; i < groupedItems.Count(); i++ )
            {
                if ( channel.ItemsManuallyOrdered )
                {
                    groupedItems[i].Items = groupedItems[i].Items.OrderBy( itm => itm.Order ).ToList();
                }
                else
                {
                    groupedItems[i].Items = groupedItems[i].Items.OrderByDescending( itm => itm.StartDateTime ).ToList();
                }
                if ( Limit.HasValue && Limit.Value > 0 )
                {
                    groupedItems[i].Items = groupedItems[i].Items.Take( Limit.Value ).ToList();
                }
            }
            if ( channel.ItemsManuallyOrdered )
            {
                groupedItems = groupedItems.OrderBy( gi => gi.Items.First().Order ).ToList();
            }
            else
            {
                groupedItems = groupedItems.OrderByDescending( gi => gi.Items.First().StartDateTime ).ToList();
            }
            //var groupedItems = items.GroupBy( i => i.AttributeValues.First( av => av.Key == AttributeKey ).Value.Value ).Select( g => new WatchGroup { Series = g.Key, Items = g.ToList() } );
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Items", groupedItems );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }
        #endregion
    }

    [DotLiquid.LiquidType( "Series", "Items" )]
    public class ItemGroup
    {
        public string Series { get; set; }
        public List<ContentChannelItem> Items { get; set; }
    }
}