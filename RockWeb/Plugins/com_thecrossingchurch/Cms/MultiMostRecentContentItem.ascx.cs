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
using System.Text.RegularExpressions;

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    [DisplayName( "Multi-Most Recent Content Item" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Displays the most recently approved or created item from each of the specified channels" )]
    [ContentChannelsField( "Content Channels", required: true, order: 1 )]
    [BooleanField( "Validate Child Items", "If true, will only include the most recent child item", order: 2 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 3 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "", "", order: 4 )]

    public partial class MultiMostRecentContentItem : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
        private bool childValid { get; set; }
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
            List<Guid?> ContentChannelGuids = GetAttributeValues( "ContentChannels" ).AsGuidOrNullList();
            _cciSvc = new ContentChannelItemService( _context );
            _ccSvc = new ContentChannelService( _context );
            List<ContentChannelItem> items = new List<ContentChannelItem>();
            childValid = GetAttributeValue( "ValidateChildItems" ).AsBoolean();
            var mergeFields = new Dictionary<string, object>();
            for ( int i = 0; i < ContentChannelGuids.Count(); i++ )
            {
                if ( ContentChannelGuids[i].HasValue )
                {
                    ContentChannelItem item = LoadItem( ContentChannelGuids[i].Value );
                    string itemName = Regex.Replace( item.ContentChannel.Name, "[^a-zA-Z]", String.Empty ).ToLower() + "Item";
                    mergeFields.Add( itemName, item );
                    items.Add( item );
                }
            }
            if ( items.Count() > 0 )
            {
                mergeFields.Add( "Items", items );
                lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
            }
        }

        #endregion

        #region Methods
        private ContentChannelItem LoadItem( Guid guid )
        {
            ContentChannel channel = _ccSvc.Get( guid );
            int id = channel.Id;
            ContentChannelItem item;
            if ( channel.RequiresApproval )
            {
                item = _cciSvc.Queryable().Where( i => i.ContentChannelId == id && DateTime.Compare( i.StartDateTime, RockDateTime.Now ) <= 0 && ( !i.ExpireDateTime.HasValue || DateTime.Compare( i.ExpireDateTime.Value, RockDateTime.Now ) >= 0 ) && i.Status == ContentChannelItemStatus.Approved ).OrderByDescending( i => i.StartDateTime ).FirstOrDefault();
            }
            else
            {
                item = _cciSvc.Queryable().Where( i => i.ContentChannelId == id && DateTime.Compare( i.StartDateTime, RockDateTime.Now ) <= 0 && ( !i.ExpireDateTime.HasValue || DateTime.Compare( i.ExpireDateTime.Value, RockDateTime.Now ) >= 0 ) ).OrderByDescending( i => i.StartDateTime ).FirstOrDefault();
            }
            item.LoadAttributes();
            if ( childValid && item.ChildItems.Count() > 1 )
            {
                item.ChildItems = item.ChildItems.ToList().Where( cia => DateTime.Compare( cia.ChildContentChannelItem.StartDateTime, RockDateTime.Now ) <= 0 && ( !cia.ChildContentChannelItem.ExpireDateTime.HasValue || DateTime.Compare( cia.ChildContentChannelItem.ExpireDateTime.Value, RockDateTime.Now ) >= 0 ) && ( !cia.ChildContentChannelItem.ContentChannel.RequiresApproval || cia.ChildContentChannelItem.Status == ContentChannelItemStatus.Approved ) ).OrderByDescending( cia => cia.ChildContentChannelItem.StartDateTime ).Take( 1 ).ToList();
            }
            return item;
        }
        #endregion
    }
}