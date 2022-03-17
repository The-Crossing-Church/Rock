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
    [DisplayName( "Most Recent Content Item" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Displays the most recently approved or created item from the specified channel" )]
    [ContentChannelField( "Content Channel", required: true )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "", "" )]

    public partial class MostRecentContentItem : Rock.Web.UI.RockBlock
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
                LoadItem( ContentChannelGuid.Value );
            }
        }

        #endregion

        #region Methods
        private void LoadItem( Guid guid )
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
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Item", item );
            mergeFields.Add( "CurrentPerson", CurrentPerson );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }
        #endregion
    }
}