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
    [DisplayName( "Watch" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Watch Resource Viewer" )]
    [ContentChannelField( "Content Channel", required: true )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/Watch.lava' %}", "" )]

    public partial class Watch : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
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
            if ( ContentChannelGuid.HasValue )
            {
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
            var items = _cciSvc.Queryable().Where( i => i.ContentChannelId == id && DateTime.Compare( i.StartDateTime, RockDateTime.Now ) <= 0 ).ToList();
            items.LoadAttributes();
            var groupedItems = items.GroupBy( i => i.AttributeValues.First(av => av.Key == "Series").Value.Value ).Select( g => new WatchGroup { Series = g.Key, Items = g.ToList() } );
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Items", groupedItems );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }
        #endregion

        #region Events

        #endregion
    }

    [DotLiquid.LiquidType( "Series", "Items")]
    public class WatchGroup
    {
        public string Series { get; set; }
        public List<ContentChannelItem> Items { get; set; }
    }
}