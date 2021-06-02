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
using System.Net;
using System.IO;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_thecrossingchurch.Cms
{
    [DisplayName( "External Site Search" )]
    [Category( "com_thecrossingchurch > Cms" )]
    [Description( "Display results of search query" )]
    [ContentChannelField( "Staff Content Channel", required: false, order: 1 )]
    [EventCalendarField( "Calendar", required: false, order: 2 )]
    [TextField( "Page Ids", "Comma seperated list of page ids to search", required: false, order: 3 )]
    [LavaCommandsField( "Enabled Lava Commands", "The Lava commands that should be enabled for this HTML block.", false, order: 4 )]
    [CodeEditorField( "Lava Template", "Lava template to use to display the list of events.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 400, true, @"{% include '~~/Assets/Lava/' %}", "", 5 )]

    public partial class ExternalSiteSearch : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private ContentChannelService _ccSvc { get; set; }
        private string query { get; set; }
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
            Guid? StaffContentChannelGuid = GetAttributeValue( "StaffContentChannel" ).AsGuidOrNull();
            Guid? CalendarGuid = GetAttributeValue( "Calendar" ).AsGuidOrNull();
            string idsRaw = GetAttributeValue( "PageIds" );
            _cciSvc = new ContentChannelItemService( _context );
            _ccSvc = new ContentChannelService( _context );
            query = PageParameter( "q" ).ToLower();

            List<EventItemOccurrence> events = new List<EventItemOccurrence>();
            List<ContentChannelItem> staff = new List<ContentChannelItem>();
            List<PageResult> pages = new List<PageResult>();
            var mergeFields = new Dictionary<string, object>();

            if ( StaffContentChannelGuid.HasValue )
            {
                staff = SearchStaff( StaffContentChannelGuid.Value );
                mergeFields.Add( "Staff", staff );
            }
            if ( CalendarGuid.HasValue )
            {
                events = SearchEvents( CalendarGuid.Value );
                mergeFields.Add( "Events", events );
            }
            if ( !String.IsNullOrEmpty( idsRaw ) )
            {
                List<int> pageIds = idsRaw.Split( ',' ).Select( i => Int32.Parse( i ) ).ToList();
                pages = SearchPages( pageIds );
                mergeFields.Add( "Pages", pages );
            }

            lOutput.Text = GetAttributeValue( "LavaTemplate" ).ResolveMergeFields( mergeFields, GetAttributeValue( "EnabledLavaCommands" ) );
        }

        #endregion

        #region Methods

        private List<EventItemOccurrence> SearchEvents( Guid guid )
        {
            EventCalendar calendar = new EventCalendarService( _context ).Get( guid );
            List<EventItemOccurrence> events = new EventItemOccurrenceService( _context ).Queryable().Where( e => e.EventItem.EventCalendarItems.Select( eci => eci.EventCalendarId ).Contains( calendar.Id ) && ( e.EventItem.Name.ToLower().Contains( query ) || e.EventItem.Description.ToLower().Contains( query ) ) ).ToList().Where( e => e.NextStartDateTime.HasValue && DateTime.Compare( e.NextStartDateTime.Value, RockDateTime.Now ) >= 0 ).ToList();
            return events;
        }

        private List<ContentChannelItem> SearchStaff( Guid guid )
        {
            ContentChannel channel = _ccSvc.Get( guid );
            List<ContentChannelItem> items = _cciSvc.Queryable().Where( i => i.ContentChannelId == channel.Id && ( !channel.RequiresApproval || i.Status == ContentChannelItemStatus.Approved ) && DateTime.Compare( i.StartDateTime, RockDateTime.Now ) <= 0 && ( !i.ExpireDateTime.HasValue || DateTime.Compare( i.ExpireDateTime.Value, RockDateTime.Now ) > 0 ) && ( i.Title.ToLower().Contains( query ) || i.Content.ToLower().Contains( query ) ) ).ToList();
            items.LoadAttributes();
            return items;
        }

        private List<PageResult> SearchPages( List<int> pageIds )
        {
            List<PageResult> results = new List<PageResult>();
            for ( int i = 0; i < pageIds.Count(); i++ )
            {
                Rock.Model.Page p = new PageService( _context ).Get( pageIds[i] );
                bool added = false;
                foreach ( var b in p.Blocks )
                {
                    if ( b.BlockTypeId == 6 )
                    {
                        var html = new HtmlContentService( _context ).Queryable().Where( c => c.BlockId == b.Id && c.IsApproved ).OrderByDescending( c => c.Version ).FirstOrDefault();
                        if ( html != null )
                        {
                            if ( html.Content.ToLower().Contains( query ) )
                            {
                                var idx = html.Content.ToLower().IndexOf( query );
                                if ( idx >= 0 )
                                {
                                    //Html block contains search query
                                    PageResult r = new PageResult() { Id = p.Id, Title = p.PageTitle, Description = p.Description };
                                    p.LoadAttributes();
                                    r.Tags = p.AttributeValues["Tags"].Value.Split( ',' ).ToList();
                                    if ( html.Content.Length > ( 150 + idx ) )
                                    {
                                        r.Matched = html.Content.Substring( idx, 150 );
                                    }
                                    else {
                                        r.Matched = html.Content.Substring( idx );
                                    }
                                    results.Add( r );
                                    added = true;
                                }
                            }
                        }
                    }
                }
                if ( !added )
                {
                    //Check for matching tags
                    PageResult r = new PageResult() { Id = p.Id, Title = p.PageTitle, Description = p.Description };
                    p.LoadAttributes();
                    r.Tags = p.AttributeValues["Tags"].Value.Split( ',' ).ToList();
                    if ( r.Tags.Select( t => t.ToLower() ).Contains( query ) )
                    {
                        results.Add( r );
                        added = true;
                    }
                }
            }
            return results;
        }

        #endregion

        [DotLiquid.LiquidType( "Title", "Id", "Matched", "Tags", "Description" )]
        private class PageResult
        {
            public int Id { get; set; }
            public string Matched { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public List<string> Tags { get; set; }
        }
    }
}