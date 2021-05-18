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

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    [DisplayName( "Content Item Repeater" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Gets all active and approved content for the specified channel and returns it to a list for display" )]
    [ContentChannelField( "Content Channel", required: true )]
    [IntegerField( "Item Limit", "The max number of items to display, leave blank to not limit", required: false )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "", "" )]

    public partial class ContentItemRepeater : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
        private int? _limit { get; set; }
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
            _limit = GetAttributeValue( "ItemLimit" ).AsIntegerOrNull();
            _cciSvc = new ContentChannelItemService( _context );
            _ccSvc = new ContentChannelService( _context );
            if ( ContentChannelGuid.HasValue )
            {
                LoadItems( ContentChannelGuid.Value );
            }
        }

        #endregion

        #region Methods
        private void LoadItems( Guid guid )
        {
            ContentChannel channel = _ccSvc.Get( guid );
            int id = channel.Id;
            List<ContentChannelItem> items = new List<ContentChannelItem>();

            //Filter Content
            if ( channel.RequiresApproval )
            {
                items = _cciSvc.Queryable().Where( i => i.ContentChannelId == id && DateTime.Compare( i.StartDateTime, RockDateTime.Now ) <= 0 && ( !i.ExpireDateTime.HasValue || DateTime.Compare( i.ExpireDateTime.Value, RockDateTime.Now ) >= 0 ) && i.Status == ContentChannelItemStatus.Approved ).ToList();
            }
            else
            {
                items = _cciSvc.Queryable().Where( i => i.ContentChannelId == id && DateTime.Compare( i.StartDateTime, RockDateTime.Now ) <= 0 && ( !i.ExpireDateTime.HasValue || DateTime.Compare( i.ExpireDateTime.Value, RockDateTime.Now ) >= 0 ) ).ToList();
            }

            //Order Content
            if ( channel.ItemsManuallyOrdered )
            {
                items = items.OrderBy( i => i.Order ).ToList();
            }
            else
            {
                items = items.OrderBy( i => i.StartDateTime ).ToList();
            }

            if ( _limit.HasValue )
            {
                items = items.Take( _limit.Value ).ToList();
            }

            items.LoadAttributes();
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Items", items );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }
        #endregion
    }
}