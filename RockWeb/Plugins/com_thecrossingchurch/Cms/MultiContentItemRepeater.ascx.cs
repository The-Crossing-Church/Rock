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
    [DisplayName( "Multi-Content Item Repeater" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Gets all active and approved content for the specified channel and returns it to a list for display" )]
    [ContentChannelsField( "Content Channels", required: true, order: 0 )]
    [TextField( "Attribute Key", required: true, defaultValue: "Series", order: 1 )]
    [TextField( "Filter Attribute Key", required: false, order: 2 )]
    [TextField( "Filter Page Parameter", required: false, order: 3 )]
    [IntegerField( "Item Limit", "The max number of items to display, leave blank to not limit", required: false, order: 4 )]
    [BooleanField( "Order By Date", "Check this box to order by date, otherwise the Order property will be used.", defaultValue: true, order: 5 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 6 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, "", "", order: 7 )]

    public partial class MultiContentItemRepeater : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
        private string AttributeKey { get; set; }
        private string FilterAttributeKey { get; set; }
        private string FilterPageParameterKey { get; set; }
        private string FilterPageParameterValue { get; set; }
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
            List<Guid?> ContentChannelGuids = GetAttributeValues( "ContentChannels" ).AsGuidOrNullList();
            _limit = GetAttributeValue( "ItemLimit" ).AsIntegerOrNull();
            AttributeKey = GetAttributeValue( "AttributeKey" );
            FilterAttributeKey = GetAttributeValue( "FilterAttributeKey" );
            FilterPageParameterKey = GetAttributeValue( "FilterPageParameter" );
            if ( !String.IsNullOrEmpty( FilterPageParameterKey ) )
            {
                FilterPageParameterValue = PageParameter( FilterPageParameterKey );
            }
            _cciSvc = new ContentChannelItemService( _context );
            _ccSvc = new ContentChannelService( _context );
            if ( ContentChannelGuids.Count() > 0 )
            {
                List<Guid> guids = ContentChannelGuids.Where( g => g.HasValue ).Select( g => g.Value ).ToList();
                LoadItems( guids );
            }
        }

        #endregion

        #region Methods
        private void LoadItems( List<Guid> guids )
        {
            List<ContentChannel> channels = _ccSvc.GetByGuids( guids ).ToList();
            List<ContentChannelItem> ccItems = new List<ContentChannelItem>();

            for ( int c = 0; c < channels.Count(); c++ )
            {
                ContentChannel channel = channels[c];
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

                items.LoadAttributes();
                if ( !String.IsNullOrEmpty( FilterAttributeKey ) && !String.IsNullOrEmpty( FilterPageParameterValue ) )
                {
                    items = items.Where( i => i.AttributeValues.ContainsKey( FilterAttributeKey ) && i.AttributeValues[FilterAttributeKey].Value == FilterPageParameterValue ).ToList();
                }
                ccItems.AddRange( items );
            }

            //Order Content
            if ( !GetAttributeValue( "OrderByDate" ).AsBoolean() )
            {
                ccItems = ccItems.OrderBy( i => i.Order ).ToList();
            }
            else
            {
                ccItems = ccItems.OrderByDescending( i => i.StartDateTime ).ToList();
            }

            if ( _limit.HasValue )
            {
                ccItems = ccItems.Take( _limit.Value ).ToList();
            }
            var mergeFields = new Dictionary<string, object>();
            mergeFields.Add( "Items", ccItems );

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }
        #endregion
    }
}