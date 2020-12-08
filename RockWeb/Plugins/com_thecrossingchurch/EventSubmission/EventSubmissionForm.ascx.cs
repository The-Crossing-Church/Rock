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
    [DisplayName( "Event Submission Form" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "Request form for Event Submissions" )]

    [IntegerField( "DefinedTypeId", "The id of the defined type for rooms.", true, 0, "", 0 )]
    [IntegerField( "ContentChannelId", "The id of the content channel for an event request.", true, 0, "", 0 )]
    [IntegerField( "ContentChannelTypeId", "The id of the content channel type for an event request.", true, 0, "", 0 )]
    [TextField( "Page Guid", "The guid of the page for redirect on save.", true, "", "", 0 )]

    public partial class EventSubmissionForm : Rock.Web.UI.RockBlock
    {
        #region Variables
        public RockContext context { get; set; }
        private int DefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private int ContentChannelTypeId { get; set; }
        private List<DefinedValue> Rooms { get; set; }
        private static class PageParameterKey
        {
            public const string Id = "Id";
        }
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
            ContentChannelTypeId = GetAttributeValue( "ContentChannelTypeId" ).AsInteger();
            Rooms = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == DefinedTypeId ).ToList();
            hfRooms.Value = JsonConvert.SerializeObject( Rooms.Select( dv => new { Id = dv.Id, Value = dv.Value } ) );
            if ( !Page.IsPostBack )
            {
                if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
                {
                    int id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
                    LoadRequest( id );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Submit Request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Submit_Click( object sender, EventArgs e )
        {
            string raw = hfRequest.Value;
            EventRequest request = JsonConvert.DeserializeObject<EventRequest>( raw );
            string requestType = "";
            if ( request.needsSpace )
            {
                requestType = "Room";
            }
            if ( request.needsOnline )
            {
                requestType += ",Online Event";
            }
            if ( request.needsPub )
            {
                requestType += ",Publicity";
            }
            if ( request.needsCatering )
            {
                requestType += ",Catering";
            }
            if ( request.needsChildCare )
            {
                requestType += ",Childcare";
            }
            if ( request.needsAccom )
            {
                requestType += ",Extra Resources";
            }
            string status = "Submitted";
            string isPreApproved = "No";

            //Pre-Approval Check
            //Requests for only a space, between 9am and 9pm (Mon-Fri) 1pm and 9pm (Sun) or 9am and 12pm (Sat), within the next 7 days, not in Gym or Auditorium, and no more than 12 people attending can be pre-approved
            if ( requestType == "Room" ) //Room only
            {
                var allDatesInNextWeek = true;
                for ( var i = 0; i < request.EventDates.Count(); i++ )
                {
                    var totalDays = ( DateTime.Parse( request.EventDates[i] ) - DateTime.Now ).TotalDays;
                    if ( totalDays >= 7 )
                    {
                        allDatesInNextWeek = false;
                    }
                }
                if ( allDatesInNextWeek ) //Request is within the next 7 days
                {
                    if ( request.ExpectedAttendance.HasValue && request.ExpectedAttendance.Value <= 12 ) //No more than 12 people attending
                    {
                        var allMeetTimeRequirements = true;
                        if ( request.StartTime.Contains( "AM" ) )
                        {
                            var info = request.StartTime.Split( ':' );
                            if ( Int32.Parse( info[0] ) < 9 )
                            {
                                allMeetTimeRequirements = false;
                            }
                        }
                        if ( request.EndTime.Contains( "PM" ) )
                        {
                            var info = request.EndTime.Split( ':' );
                            if ( Int32.Parse( info[0] ) >= 9 )
                            {
                                allMeetTimeRequirements = false;
                            }
                        }
                        if ( allMeetTimeRequirements ) //Meets general time requirements, check for other restrictions Sat/Sun
                        {
                            for ( var i = 0; i < request.EventDates.Count(); i++ )
                            {
                                DateTime dt = DateTime.Parse( request.EventDates[i] );
                                if ( dt.DayOfWeek == DayOfWeek.Sunday )
                                {
                                    if ( request.StartTime.Contains( "AM" ) )
                                    {
                                        allMeetTimeRequirements = false;
                                    }
                                    else
                                    {
                                        var info = request.StartTime.Split( ':' );
                                        if ( Int32.Parse( info[0] ) >= 9 )
                                        {
                                            allMeetTimeRequirements = false;
                                        }
                                    }
                                }
                                else if ( dt.DayOfWeek == DayOfWeek.Saturday )
                                {
                                    if ( request.StartTime.Contains( "PM" ) )
                                    {
                                        allMeetTimeRequirements = false;
                                    }
                                    if ( request.EndTime.Contains( "PM" ) )
                                    {
                                        var info = request.StartTime.Split( ':' );
                                        var info2 = info[0].Split( ' ' );
                                        if ( Int32.Parse( info[0] ) != 12 || info2[0] != "00" )
                                        {
                                            allMeetTimeRequirements = false;
                                        }

                                    }
                                }
                            }
                        }
                        if ( allMeetTimeRequirements ) //Start and End Time are within limits
                        {
                            var needsRoomApproval = false;
                            var roomsNeedingApproval = Rooms.Where( dv => dv.Value.Contains( "Auditorium" ) || dv.Value.Contains( "Gym" ) ).Select( dv => dv.Id.ToString() ).ToList();
                            for ( var i = 0; i < request.Rooms.Count(); i++ )
                            {
                                if ( roomsNeedingApproval.Contains( request.Rooms[i] ) )
                                {
                                    needsRoomApproval = true;
                                }
                            }
                            if ( !needsRoomApproval ) //Not in Gym or Auditorium
                            {
                                status = "Approved";
                                isPreApproved = "Yes";
                            }
                        }
                    }
                }
            }

            ContentChannelItemService svc = new ContentChannelItemService( context );
            ContentChannelItem item = new ContentChannelItem();
            item.ContentChannelTypeId = ContentChannelTypeId;
            item.ContentChannelId = ContentChannelId;
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                int id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
                item = svc.Get( id );
            }
            item.LoadAttributes();
            item.CreatedByPersonAliasId = CurrentPersonAliasId;
            item.CreatedDateTime = RockDateTime.Now;
            item.Title = request.Name;
            item.SetAttributeValue( "RequestStatus", status );
            item.SetAttributeValue( "RequestJSON", raw );
            item.SetAttributeValue( "EventDates", String.Join( ", ", request.EventDates ) );
            item.SetAttributeValue( "RequestType", requestType );
            item.SetAttributeValue( "IsPreApproved", isPreApproved );

            //Save everything
            context.ContentChannelItems.AddOrUpdate( item );
            context.SaveChanges();
            item.SaveAttributeValues( context );
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "ShowSuccess", "true" );
            NavigateToPage( Guid.Parse( GetAttributeValue( "PageGuid" ) ), query );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load the existing request
        /// </summary>
        /// <param name="id"></param>
        protected void LoadRequest(int id)
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            ContentChannelItem item = svc.Get( id );
            item.LoadAttributes();
            hfRequest.Value = item.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value;
        }

        #endregion

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