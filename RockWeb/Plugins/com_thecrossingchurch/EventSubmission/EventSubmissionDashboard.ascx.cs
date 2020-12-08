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
using Newtonsoft.Json;
using CSScriptLibrary;
using System.Data.Entity.Migrations;

namespace RockWeb.Plugins.com_thecrossingchurch.EventSubmission
{
    /// <summary>
    /// Request form for Event Submissions
    /// </summary>
    [DisplayName( "Event Submission Dashboard" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "Dashboard for Event Submissions" )]

    [IntegerField( "DefinedTypeId", "The id of the defined type for rooms.", true, 0, "", 0 )]
    [IntegerField( "ContentChannelId", "The id of the content channel for an event request.", true, 0, "", 0 )]
    [IntegerField( "PageId", "The id of the page for editing requests.", true, 0, "", 0 )]
    [IntegerField( "HistoryPageId", "The id of the page for viewing all requests.", true, 0, "", 0 )]

    public partial class EventSubmissionDashboard : Rock.Web.UI.RockBlock
    {
        #region Variables
        public RockContext context { get; set; }
        private int DefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private int PageId { get; set; }
        private List<DefinedValue> Rooms { get; set; }
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
            context = new RockContext();
            DefinedTypeId = GetAttributeValue( "DefinedTypeId" ).AsInteger();
            ContentChannelId = GetAttributeValue( "ContentChannelId" ).AsInteger();
            PageId = GetAttributeValue( "PageId" ).AsInteger();
            hfRequestURL.Value = "/page/" + PageId;
            var HistoryPageId = GetAttributeValue( "HistoryPageId" ).AsInteger();
            hfHistoryURL.Value = "/page/" + HistoryPageId;
            Rooms = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == DefinedTypeId ).ToList();
            hfRooms.Value = JsonConvert.SerializeObject( Rooms.Select( dv => new { Id = dv.Id, Value = dv.Value } ) );
            GetRecentRequests();
            GetThisWeeksEvents();
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Submit Request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ChangeStatus_Click( object sender, EventArgs e )
        {
            int? id = hfRequestID.Value.AsIntegerOrNull();
            if ( id.HasValue )
            {
                ContentChannelItem item = new ContentChannelItemService( context ).Get( id.Value );
                item.LoadAttributes();
                string action = hfAction.Value;
                switch ( action )
                {
                    case "Deny":
                        item.SetAttributeValue( "RequestStatus", "Denied" );
                        break;
                    case "Cancel":
                        item.SetAttributeValue( "RequestStatus", "Cancelled" );
                        break;
                    default:
                        item.SetAttributeValue( "RequestStatus", "Approved" );
                        break;
                }
                item.SaveAttributeValues( context );
                hfRequestID.Value = null;
                GetRecentRequests();
                GetThisWeeksEvents();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the recently submitted events
        /// </summary>
        protected void GetRecentRequests()
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            DateTime oneweekago = DateTime.Now.AddDays( -7 );
            var items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId && DateTime.Compare( i.CreatedDateTime.Value, oneweekago ) >= 0 ).ToList();
            items.LoadAttributes();
            hfRequests.Value = JsonConvert.SerializeObject( items.Select( i => new { Id = i.Id, Value = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, CreatedBy = i.CreatedByPersonName, CreatedOn = i.CreatedDateTime, RequestStatus = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value } ) );
        }

        /// <summary>
        /// Get events for the next 7 days
        /// </summary>
        protected void GetThisWeeksEvents()
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            var items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).ToList();
            items.LoadAttributes();
            var reqs = items.Select( i => new FullRequest() { Id = i.Id, Value = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, Request = JsonConvert.DeserializeObject<EventRequest>( i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value ), CreatedBy = i.CreatedByPersonName, CreatedOn = i.CreatedDateTime, RequestStatus = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value } );
            var current = reqs.Where( r =>
            {
                if(r.RequestStatus != "Approved" )
                {
                    return false;
                }
                var dates = r.Request.EventDates;
                var occursInWeek = false;
                var endOfWeek = DateTime.Now.AddDays( 6 );
                endOfWeek = new DateTime( endOfWeek.Year, endOfWeek.Month, endOfWeek.Day, 23, 59, 59 );
                var today = DateTime.Now;
                today = new DateTime( today.Year, today.Month, today.Day, 0, 0, 0 );
                for ( var i = 0; i < dates.Count(); i++ )
                {
                    if ( DateTime.Compare( DateTime.Parse( dates[i] ), endOfWeek ) <= 0 && DateTime.Compare( DateTime.Parse( dates[i] ), today ) >= 0 )
                    {
                        occursInWeek = true;
                    }
                }
                return occursInWeek;
            } );
            hfCurrent.Value = JsonConvert.SerializeObject( current );
        }

        #endregion
        private class FullRequest
        {
            public int Id { get; set; }
            public string Value { get; set; }
            public EventRequest Request { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? CreatedOn { get; set; }
            public string RequestStatus { get; set; }
        }
        private class EventRequest
        {
            public bool needsSpace { get; set; }
            public bool needsOnline { get; set; }
            public bool needsPub { get; set; }
            public bool needsCatering { get; set; }
            public bool needsChildCare { get; set; }
            public bool needsAccom { get; set; }
            public string Name { get; set; }
            public string Ministry { get; set; }
            public string Contact { get; set; }
            public List<string> EventDates { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public int? ExpectedAttendance { get; set; }
            public List<string> Rooms { get; set; }
            public List<string> Drinks { get; set; }
            public List<string> TechNeeds { get; set; }
            public DateTime? RegistrationDate { get; set; }
        }
    }
}