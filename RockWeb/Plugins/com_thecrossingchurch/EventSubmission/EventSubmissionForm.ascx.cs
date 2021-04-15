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
using Rock.Communication;
using Microsoft.Identity.Client;
using Microsoft.Graph.Auth;
using Microsoft.Graph;
using System.Threading.Tasks;

namespace RockWeb.Plugins.com_thecrossingchurch.EventSubmission
{
    /// <summary>
    /// Request form for Event Submissions
    /// </summary>
    [DisplayName( "Event Submission Form" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "Request form for Event Submissions" )]

    [IntegerField( "DefinedTypeId", "The id of the defined type for rooms.", true, 0, "", 0 )]
    [IntegerField( "MinistryDefinedTypeId", "The id of the defined type for ministries.", true, 0, "", 1 )]
    [IntegerField( "ContentChannelId", "The id of the content channel for an event request.", true, 0, "", 2 )]
    [IntegerField( "ContentChannelTypeId", "The id of the content channel type for an event request.", true, 0, "", 3 )]
    [TextField( "Page Guid", "The guid of the page for redirect on save.", true, "", "", 4 )]
    [TextField( "Rock Base URL", "Base URL for Rock", true, "https://rock.thecrossingchurch.com", "", 5 )]
    [TextField( "Request Page Id", "Page Id of the Request Form", true, "", "", 6 )]
    [TextField( "Dashboard Page Id", "Page Id of the Request Dashboard", true, "", "", 7 )]
    [SecurityRoleField( "Room Request Admin", "The role for people handling the room only requests who need to be notified", true )]
    [SecurityRoleField( "Event Request Admin", "The role for people handling all other requests who need to be notified", true )]

    public partial class EventSubmissionForm : Rock.Web.UI.RockBlock
    {
        #region Variables
        public RockContext context { get; set; }
        public string BaseURL { get; set; }
        public string RequestPageId { get; set; }
        public string DashboardPageId { get; set; }
        private int DefinedTypeId { get; set; }
        private int MinistryDefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private int ContentChannelTypeId { get; set; }
        private List<DefinedValue> Rooms { get; set; }
        private List<DefinedValue> Ministries { get; set; }
        private Rock.Model.Group RoomOnlySR { get; set; }
        private Rock.Model.Group EventSR { get; set; }
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
            MinistryDefinedTypeId = GetAttributeValue( "MinistryDefinedTypeId" ).AsInteger();
            ContentChannelId = GetAttributeValue( "ContentChannelId" ).AsInteger();
            ContentChannelTypeId = GetAttributeValue( "ContentChannelTypeId" ).AsInteger();
            BaseURL = GetAttributeValue( "RockBaseURL" );
            RequestPageId = GetAttributeValue( "RequestPageId" );
            DashboardPageId = GetAttributeValue( "DashboardPageId" );
            var RoomSRGuid = GetAttributeValue( "RoomRequestAdmin" ).AsGuidOrNull();
            var EventSRGuid = GetAttributeValue( "EventRequestAdmin" ).AsGuidOrNull();
            hfIsAdmin.Value = "False";
            if ( RoomSRGuid.HasValue )
            {
                RoomOnlySR = new GroupService( context ).Get( RoomSRGuid.Value );
            }
            if ( EventSRGuid.HasValue )
            {
                EventSR = new GroupService( context ).Get( EventSRGuid.Value );
                if ( CurrentPersonId.HasValue && EventSR.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Select( m => m.PersonId ).ToList().Contains( CurrentPersonId.Value ) )
                {
                    hfIsAdmin.Value = "True";
                }
            }
            Rooms = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == DefinedTypeId ).ToList();
            Rooms.LoadAttributes();
            hfRooms.Value = JsonConvert.SerializeObject( Rooms.Select( dv => new { Id = dv.Id, Value = dv.Value, Type = dv.AttributeValues.FirstOrDefault( av => av.Key == "Type" ).Value.Value, Capacity = dv.AttributeValues.FirstOrDefault( av => av.Key == "Capacity" ).Value.Value.AsInteger() } ) );
            Ministries = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == MinistryDefinedTypeId ).ToList();
            hfMinistries.Value = JsonConvert.SerializeObject( Ministries.Select( dv => new { Id = dv.Id, Value = dv.Value } ) );
            ThisWeekRequests();
            if ( !Page.IsPostBack )
            {
                if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
                {
                    int id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
                    LoadRequest( id );
                }
                LoadUpcoming();
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
                    int? expAtt = request.Events.Select( ev => ev.ExpectedAttendance ).Min();
                    if ( expAtt.HasValue && expAtt.Value <= 12 ) //No more than 12 people attending
                    {
                        var allMeetTimeRequirements = true;
                        for ( var k = 0; k < request.Events.Count(); k++ )
                        {
                            if ( request.Events[k].StartTime.Contains( "AM" ) )
                            {
                                var info = request.Events[k].StartTime.Split( ':' );
                                if ( Int32.Parse( info[0] ) < 9 )
                                {
                                    allMeetTimeRequirements = false;
                                }
                            }
                            if ( request.Events[k].EndTime.Contains( "PM" ) )
                            {
                                var info = request.Events[k].EndTime.Split( ':' );
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
                                    if ( dt.DayOfWeek == System.DayOfWeek.Sunday )
                                    {
                                        if ( request.Events[k].StartTime.Contains( "AM" ) )
                                        {
                                            allMeetTimeRequirements = false;
                                        }
                                        else
                                        {
                                            var info = request.Events[k].StartTime.Split( ':' );
                                            if ( Int32.Parse( info[0] ) >= 9 )
                                            {
                                                allMeetTimeRequirements = false;
                                            }
                                        }
                                    }
                                    else if ( dt.DayOfWeek == System.DayOfWeek.Saturday )
                                    {
                                        if ( request.Events[k].StartTime.Contains( "PM" ) )
                                        {
                                            allMeetTimeRequirements = false;
                                        }
                                        if ( request.Events[k].EndTime.Contains( "PM" ) )
                                        {
                                            var info = request.Events[k].StartTime.Split( ':' );
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
                                for ( var i = 0; i < request.Events[k].Rooms.Count(); i++ )
                                {
                                    if ( roomsNeedingApproval.Contains( request.Events[k].Rooms[i] ) )
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
            }

            ContentChannelItemService svc = new ContentChannelItemService( context );
            ContentChannelItem item = new ContentChannelItem();
            item.ContentChannelTypeId = ContentChannelTypeId;
            item.ContentChannelId = ContentChannelId;
            bool isExisting = false;
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                int id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
                item = svc.Get( id );
                isExisting = true;
            }
            item.LoadAttributes();
            if ( isExisting )
            {
                item.ModifiedByPersonAliasId = CurrentPersonAliasId;
            }
            else
            {
                item.CreatedByPersonAliasId = CurrentPersonAliasId;
            }
            item.CreatedDateTime = RockDateTime.Now;
            item.Title = request.Name;
            if ( isPreApproved == "No" )
            {
                string currentStatus = item.GetAttributeValue( "RequestStatus" );
                if ( currentStatus == "Approved" || currentStatus == "Pending Changes" )
                {
                    status = "Pending Changes";
                }
            }
            item.SetAttributeValue( "RequestStatus", status );
            //Changes are proposed if the event isn't pre-approved, is existing, and the requestor isn't in the Event Admin Role
            if ( item.Id > 0 && status != "Approved" && status != "Submitted" && !EventSR.Members.Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active ).Select( m => m.PersonId ).Contains( CurrentPersonId.Value ) )
            {
                item.SetAttributeValue( "ProposedChangesJSON", raw );
            }
            else
            {
                item.SetAttributeValue( "RequestJSON", raw );
                item.SetAttributeValue( "ProposedChangesJSON", "" );
            }
            item.SetAttributeValue( "EventDates", String.Join( ", ", request.EventDates ) );
            item.SetAttributeValue( "RequestType", requestType );
            item.SetAttributeValue( "IsPreApproved", isPreApproved );

            //Save everything
            context.ContentChannelItems.AddOrUpdate( item );
            context.SaveChanges();
            item.SaveAttributeValues( context );
            if ( !isExisting )
            {
                NotifyReviewers( item, request, isPreApproved, false );
                ConfirmationEmail( item, request, isPreApproved, false );
            }
            else
            {
                if ( CurrentPersonId == item.CreatedByPersonId && status != "Submitted" )
                {
                    //User is modifying their request, send notification
                    NotifyReviewers( item, request, isPreApproved, true );
                    ConfirmationEmail( item, request, isPreApproved, true );
                }
            }
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "ShowSuccess", "true" );
            if ( isExisting )
            {
                query.Add( "Id", item.Id.ToString() );
            }
            NavigateToPage( Guid.Parse( GetAttributeValue( "PageGuid" ) ), query );
        }



        /// <summary>
        /// Submit Date Change Request
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnChangeRequest_Click( object sender, EventArgs e )
        {
            int id = 0;
            ContentChannelItem item = new ContentChannelItem();
            ContentChannelItemService svc = new ContentChannelItemService( context );
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
                item = svc.Get( id );
            }
            //Email Events Director
            string subject = CurrentPerson.FullName + " is Requesting a Date Change to " + item.Title;
            string message = CurrentPerson.FullName + " is requesting the following change to their event: <br/>";
            message += "<blockquote>" + hfChangeRequest.Value + "</blockquote>";
            List<GroupMember> groupMembers = EventSR.Members.Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer
            message += "<br/>" +
                "<table style='width: 100%;'>" +
                    "<tr>" +
                        "<td></td>" +
                        "<td style='text-align:center;'>" +
                            "<a href='" + BaseURL + DashboardPageId + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Open Request</a>" +
                        "</td>" +
                        "<td></td>" +
                    "</tr>" +
                "</table>";
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            for ( var i = 0; i < groupMembers.Count(); i++ )
            {
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( groupMembers[i].Person, new Dictionary<string, object>() );
                email.AddRecipient( recipient );
            }
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            var output = email.Send();

            //Redirect
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "ShowSuccess", "true" );
            query.Add( "Id", id.ToString() );
            NavigateToPage( Guid.Parse( GetAttributeValue( "PageGuid" ) ), query );
        }
        #endregion

        #region Methods

        /// <summary>
        /// Load the existing request
        /// </summary>
        /// <param name="id"></param>
        protected void LoadRequest( int id )
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            ContentChannelItem item = svc.Get( id );
            item.LoadAttributes();
            bool canEdit = false;
            if ( item.CreatedByPersonId == CurrentPersonId )
            {
                string status = item.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value;
                if ( status == "Submitted" || status == "Approved" )
                {
                    canEdit = true;
                }
            }
            else
            {
                if ( RoomOnlySR.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Select( m => m.PersonId ).Contains( CurrentPersonId.Value ) )
                {
                    canEdit = true;
                }
                if ( EventSR.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Select( m => m.PersonId ).Contains( CurrentPersonId.Value ) )
                {
                    canEdit = true;
                }
            }
            hfRequest.Value = JsonConvert.SerializeObject( new { Id = item.Id, Value = item.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, CreatedBy = item.CreatedByPersonId, CreatedOn = item.CreatedDateTime, RequestStatus = item.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value, CanEdit = canEdit } );
        }

        /// <summary>
        /// Load requests happening in the next seven days to display on the quick view event selection
        /// </summary>
        protected void ThisWeekRequests()
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            List<ContentChannelItem> items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).ToList();
            items.LoadAttributes();
            DateTime oneWeek = DateTime.Now.AddDays( 7 );
            oneWeek = new DateTime( oneWeek.Year, oneWeek.Month, oneWeek.Day, 23, 59, 59 );
            items = items.Where( i =>
            {
                //Don't show non-approved requests
                string status = i.AttributeValues["RequestStatus"].Value;
                if ( status == "Submitted" || status == "Denied" || status == "Cancelled" || status == "Cancelled by User" )
                {
                    return false;
                }
                var dateStr = i.AttributeValues["EventDates"];
                var dates = dateStr.Value.Split( ',' );
                foreach ( var d in dates )
                {
                    DateTime dt = DateTime.Parse( d );
                    if ( DateTime.Compare( dt, DateTime.Now ) >= 0 && DateTime.Compare( dt, oneWeek ) <= 0 )
                    {
                        return true;
                    }
                }
                return false;
            } ).ToList();
            hfThisWeeksRequests.Value = JsonConvert.SerializeObject( items.Select( i => i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value ).ToList() );
        }

        /// <summary>
        /// Load upcoming requests
        /// </summary>
        protected void LoadUpcoming()
        {
            int id = 0;
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
            }
            ContentChannelItemService svc = new ContentChannelItemService( context );
            List<ContentChannelItem> items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).ToList();
            items.LoadAttributes();
            items = items.Where( i =>
            {
                if ( i.Id == id )
                {
                    return false;
                }
                //Don't show non-approved requests
                string status = i.AttributeValues["RequestStatus"].Value;
                if ( status == "Submitted" || status == "Denied" || status == "Cancelled" || status == "Cancelled by User" )
                {
                    return false;
                }
                var dateStr = i.AttributeValues["EventDates"];
                var dates = dateStr.Value.Split( ',' );
                foreach ( var d in dates )
                {
                    DateTime dt = DateTime.Parse( d );
                    if ( DateTime.Compare( dt, DateTime.Now ) >= 1 )
                    {
                        return true;
                    }
                }
                return false;
            } ).ToList();
            hfUpcomingRequests.Value = JsonConvert.SerializeObject( items.Select( i => i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value ).ToList() );
        }

        /// <summary>
        /// Notify Correct Users of a New Submission
        /// </summary>
        private void NotifyReviewers( ContentChannelItem item, EventRequest request, string isPreApproved, bool isRequestingChanges )
        {
            string message = "";
            string subject = "";
            List<GroupMember> groupMembers = new List<GroupMember>();
            if ( item.AttributeValues["RequestType"].Value == "Room" )
            {
                //Notify the Room Only Request Group
                if ( isRequestingChanges )
                {
                    subject = CurrentPerson.FullName + " is Requesting Changes to a Reservation";
                }
                else
                {
                    subject = "New Room Request from " + CurrentPerson.FullName;
                }
                groupMembers = RoomOnlySR.Members.Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
                if ( isRequestingChanges )
                {
                    message = CurrentPerson.FullName + " is requesting changes to the reservation for " + Ministries.FirstOrDefault( dv => dv.Id.ToString() == request.Ministry ).Value + ".<br/>";
                }
                else
                {
                    message = CurrentPerson.FullName + " has submitted a room request for " + Ministries.FirstOrDefault( dv => dv.Id.ToString() == request.Ministry ).Value + ".<br/>";
                }
                message += "<strong>Ministry Contact:</strong> " + request.Contact + "<br/>";
                for ( int i = 0; i < request.Events.Count(); i++ )
                {
                    if ( request.Events.Count() == 1 || request.IsSame )
                    {
                        message += "<strong>Event Dates:</strong> " + String.Join( ", ", request.EventDates.Select( e => DateTime.Parse( e ).ToString( "MM/dd/yyyy" ) ) ) + "<br/>";
                    }
                    else
                    {
                        message += "<strong>Date:</strong> " + DateTime.Parse( request.Events[i].EventDate ).ToString( "MM/dd/yyyy" ) + "<br/>";
                    }
                    message += "<strong>Start Time:</strong> " + request.Events[i].StartTime + "<br/>";
                    message += "<strong>End Time:</strong> " + request.Events[i].EndTime + "<br/>";
                    message += "<strong>Requested Rooms:</strong> " + String.Join( ", ", Rooms.Where( dv => request.Events[i].Rooms.Contains( dv.Id.ToString() ) ).Select( dv => dv.Value ) ) + "<br/>";
                    message += "<strong>Expected Attendance:</strong> " + request.Events[i].ExpectedAttendance + "<br/>";
                }
                if ( isPreApproved == "Yes" )
                {
                    message += "Because of the date, time, location, and expected attendance this request has been pre-approved.<br/>";
                }
            }
            else
            {
                //Notify the Event Request Group
                if ( isRequestingChanges )
                {
                    subject = CurrentPerson.FullName + " is Requesting Changes to a Reservation";
                }
                else
                {
                    subject = "New Event Request from " + CurrentPerson.FullName;
                }
                groupMembers = EventSR.Members.Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
                message = GenerateEmailDetails( item, request );
            }
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer
            message += "<br/>" +
                "<table style='width: 100%;'>" +
                    "<tr>" +
                        "<td></td>" +
                        "<td style='text-align:center;'>" +
                            "<a href='" + BaseURL + DashboardPageId + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Open Request</a>" +
                        "</td>" +
                        "<td></td>" +
                    "</tr>" +
                "</table>";
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            for ( var i = 0; i < groupMembers.Count(); i++ )
            {
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( groupMembers[i].Person, new Dictionary<string, object>() );
                email.AddRecipient( recipient );
            }
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            var output = email.Send();
        }

        /// <summary>
        /// Confirm Request Submission
        /// </summary>
        private void ConfirmationEmail( ContentChannelItem item, EventRequest request, string isPreApproved, bool isRequestingChanges )
        {
            string message = "";
            string subject = "";
            //Notify the Event Request Group
            if ( isRequestingChanges )
            {
                subject = "Your changes have been submitted";
            }
            else
            {
                subject = "Your Request has been submitted";
            }
            if ( isPreApproved == "Yes" )
            {
                message = "Your room request has been submitted and is pre-approved due to the nature of your request. The details of your request are as follows: <br/>";
            }
            else
            {
                if ( isRequestingChanges )
                {
                    message = "The changes to your request have been submitted, someone will review your changes and notify you if they are approved. Here are the details of your request that was submitted: <br/>";
                }
                else
                {
                    message = "Your request has been submitted, someone will review your request and notify you when it has been approved. Here are the details of your request that was submitted: <br/>";
                }
            }
            message += GenerateEmailDetails( item, request );
            message += "<br/>" +
                "<table style='width: 100%;'>" +
                    "<tr>" +
                        "<td></td>" +
                        "<td style='text-align:center;'>" +
                            "<strong>See a mistake? You can modify your request using the link below. If your request was already approved the changes you make will have to be approved as well.</strong><br/><br/><br/>" +
                            "<a href='" + BaseURL + RequestPageId + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Modify Request</a>" +
                        "</td>" +
                        "<td></td>" +
                    "</tr>" +
                "</table>";
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( CurrentPerson, new Dictionary<string, object>() );
            email.AddRecipient( recipient );
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            var output = email.Send();
        }

        private string GenerateEmailDetails( ContentChannelItem item, EventRequest request )
        {
            string message = "<br/>";
            message += "<strong style='font-size: 16px;'>Ministry:</strong> <span style='font-size: 16px;'>" + Ministries.FirstOrDefault( dv => dv.Id.ToString() == request.Ministry ).Value + "</span><br/>";
            message += "<strong style='font-size: 16px;'>Event Name:</strong> <span style='font-size: 16px;'>" + request.Name + "</span><br/>";
            message += "<strong style='font-size: 16px;'>Ministry Contact:</strong> <span style='font-size: 16px;'>" + request.Contact + "</span><br/><br/>";

            message += "<strong>Requested Resources:</strong> " + item.AttributeValues["RequestType"].Value + "<br/><br/>";

            for ( int i = 0; i < request.Events.Count(); i++ )
            {
                message += "<strong style='color: #6485b3;'>Date Information</strong><br/>";
                if ( request.Events.Count() == 1 || request.IsSame )
                {
                    message += "<strong style='font-size: 14px;'>Event Dates:</strong> <span style='font-size: 14px;'>" + String.Join( ", ", request.EventDates.Select( e => DateTime.Parse( e ).ToString( "MM/dd/yyyy" ) ) ) + "</span><br/>";
                }
                else
                {
                    message += "<strong style='font-size: 14px;'>Date:</strong> <span style='font-size: 14px;'>" + DateTime.Parse( request.Events[i].EventDate ).ToString( "MM/dd/yyyy" ) + "</span><br/>";
                }
                if ( !String.IsNullOrEmpty( request.Events[i].StartTime ) )
                {
                    message += "<strong>Start Time:</strong> " + request.Events[i].StartTime + "<br/>";
                }
                if ( !String.IsNullOrEmpty( request.Events[i].EndTime ) )
                {
                    message += "<strong>End Time:</strong> " + request.Events[i].EndTime + "<br/>";
                }

                if ( request.needsSpace )
                {
                    message += "<br/><strong style='color: #6485b3;'>Room Information</strong><br/>";
                    message += "<strong>Requested Rooms:</strong> " + String.Join( ", ", Rooms.Where( dv => request.Events[i].Rooms.Contains( dv.Id.ToString() ) ).Select( dv => dv.Value ) ) + "<br/>";
                    message += "<strong>Needs In-Person Check-in:</strong> " + ( request.Events[i].Checkin.Value == true ? "Yes" : "No" ) + "<br/>";
                    message += "<strong>Expected Attendance:</strong> " + request.Events[i].ExpectedAttendance + "<br/>";
                    if ( request.Events[i].Checkin.Value == true && request.Events[i].ExpectedAttendance >= 100 )
                    {
                        message += "<strong>Requested Database Team Support:</strong> " + ( request.Events[i].SupportTeam.Value == true ? "Yes" : "No" ) + "<br/>";
                    }
                }

                if ( request.needsCatering )
                {
                    message += "<br/><strong style='color: #6485b3;'>Food/Drink Information</strong><br/>";
                    message += "<strong>Preferred Vendor:</strong> " + request.Events[i].Vendor + "<br/>";
                    message += "<strong>Preferred Menu:</strong> " + request.Events[i].Menu + "<br/>";
                    message += "<strong>Budget Line:</strong> " + request.Events[i].BudgetLine + "<br/>";
                    if ( request.Events[i].FoodDelivery )
                    {
                        message += "<strong>Food Set-Up Time:</strong> " + request.Events[i].FoodTime + "<br/>";
                        message += "<strong>Food Set-Up Location:</strong> " + request.Events[i].FoodDropOff + "<br/>";
                    }
                    else
                    {
                        message += "<strong>Desired Pick-up time from Vendor:</strong> " + request.Events[i].FoodTime + "<br/>";
                    }
                    if ( request.Events[i].Drinks != null && request.Events[i].Drinks.Count() > 0 )
                    {
                        message += "<strong>Drinks:</strong> " + String.Join( ", ", request.Events[i].Drinks ) + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].DrinkTime ) )
                    {
                        message += "<strong>Drink Set-Up Time:</strong> " + request.Events[i].DrinkTime + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].DrinkDropOff ) )
                    {
                        message += "<strong>Drink Set-Up Location:</strong> " + request.Events[i].DrinkDropOff + "<br/>";
                    }
                }

                if ( request.needsOnline )
                {
                    message += "<br/><strong style='color: #6485b3;'>Online Information</strong><br/>";
                    message += "<strong>Event Link:</strong> " + request.Events[i].EventURL + "<br/>";
                    if ( !String.IsNullOrEmpty( request.Events[i].ZoomPassword ) )
                    {
                        message += "<strong>Zoom Password:</strong> " + request.Events[i].ZoomPassword + "<br/>";
                    }
                }

                if ( request.needsChildCare )
                {
                    message += "<br/><strong style='color: #6485b3;'>Childcare Information</strong><br/>";
                    message += "<strong>Childcare Age Groups:</strong> " + String.Join( ", ", request.Events[i].ChildCareOptions ) + "<br/>";
                    message += "<strong>Expected Number of Children:</strong> " + request.Events[i].EstimatedKids + "<br/>";
                    message += "<strong>Childcare Start Time:</strong> " + request.Events[i].CCStartTime + "<br/>";
                    message += "<strong>Childcare End Time:</strong> " + request.Events[i].CCEndTime + "<br/>";
                    if ( request.needsCatering )
                    {
                        message += "<strong>Preferred Vendor for Childcare:</strong> " + request.Events[i].CCVendor + "<br/>";
                        message += "<strong>Budget Line for Childcare:</strong> " + request.Events[i].CCBudgetLine + "<br/>";
                        message += "<strong>Preferred Menu for Childcare:</strong> " + request.Events[i].CCMenu + "<br/>";
                        message += "<strong>ChildCare Food Set-Up Time:</strong> " + request.Events[i].CCFoodTime + "<br/>";
                    }
                }

                if ( request.needsReg )
                {
                    message += "<br/><strong style='color: #6485b3;'>Registration Information</strong><br/>";
                    if ( request.Events[i].RegistrationDate.HasValue )
                    {
                        message += "<strong>Registration Date:</strong> " + request.Events[i].RegistrationDate.Value.ToString( "MM/dd/yyyy" ) + "<br/>";
                    }
                    if ( request.Events[i].RegistrationEndDate.HasValue )
                    {
                        message += "<strong>Registration Close Date:</strong> " + request.Events[i].RegistrationEndDate.Value.ToString( "MM/dd/yyyy" ) + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].RegistrationEndTime ) )
                    {
                        message += "<strong>Registration Close Time:</strong> " + request.Events[i].RegistrationEndTime + "<br/>";
                    }
                    if ( request.Events[i].FeeType.Count() > 0 )
                    {
                        message += "<strong>Registration Fee Types:</strong> " + String.Join( ", ", request.Events[i].FeeType ) + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].Fee ) )
                    {
                        message += "<strong>Registration Fee Per Individual:</strong> " + request.Events[i].Fee + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].CoupleFee ) )
                    {
                        message += "<strong>Registration Fee Per Couple:</strong> " + request.Events[i].CoupleFee + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].OnlineFee ) )
                    {
                        message += "<strong>Registration Online Fee:</strong> " + request.Events[i].OnlineFee + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].ThankYou ) )
                    {
                        message += "<strong>Confirmation Email Thank You:</strong> " + request.Events[i].ThankYou + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].TimeLocation ) )
                    {
                        message += "<strong>Confirmation Email Time and Location:</strong> " + request.Events[i].TimeLocation + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].AdditionalDetails ) )
                    {
                        message += "<strong>Confirmation Email Additional Details:</strong> " + request.Events[i].AdditionalDetails + "<br/>";
                    }
                }

                if ( request.needsAccom )
                {
                    message += "<br/><strong style='color: #6485b3;'>Tech Information</strong><br/>";
                    if ( request.Events[i].TechNeeds != null && request.Events[i].TechNeeds.Count() > 0 )
                    {
                        message += "<strong>Tech Needs:</strong> " + String.Join( ", ", request.Events[i].TechNeeds ) + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].TechDescription ) )
                    {
                        message += "<strong>Tech Description:</strong> " + request.Events[i].TechDescription + "<br/>";
                    }

                    if ( !String.IsNullOrEmpty( request.Events[i].SetUp ) )
                    {
                        message += "<br/><strong style='color: #6485b3;'>Set-Up Information</strong><br/>";
                        message += "<strong>Room Set-Up:</strong> " + request.Events[i].SetUp + "<br/>";
                    }

                    if ( !request.needsCatering )
                    {
                        message += "<br/><strong style='color: #6485b3;'>Drink Information</strong><br/>";
                        if ( request.Events[i].Drinks != null && request.Events[i].Drinks.Count() > 0 )
                        {
                            message += "<strong>Drinks:</strong> " + String.Join( ", ", request.Events[i].Drinks ) + "<br/>";
                        }
                        if ( !String.IsNullOrEmpty( request.Events[i].DrinkTime ) )
                        {
                            message += "<strong>Drink Set-up Time:</strong> " + request.Events[i].DrinkTime + "<br/>";
                        }
                        if ( !String.IsNullOrEmpty( request.Events[i].DrinkDropOff ) )
                        {
                            message += "<strong>Drink Drop off Location:</strong> " + request.Events[i].DrinkDropOff + "<br/>";
                        }
                    }

                    message += "<br/><strong style='color: #6485b3;'>Web Calendar Information</strong><br/>";
                    message += "<strong>Add to Public Calendar:</strong> " + ( request.Events[i].ShowOnCalendar == true ? "Yes" : "No" ) + "<br/>";
                    if ( request.Events[i].ShowOnCalendar && !String.IsNullOrEmpty( request.Events[i].PublicityBlurb ) )
                    {
                        message += "<strong>Publicity Blurb:</strong> " + request.Events[i].PublicityBlurb + "<br/>";
                    }
                }
                message += "<hr/>";
            }

            if ( request.needsPub )
            {
                message += "<br/><strong style='color: #6485b3;'>Publicity Information</strong><br/>";
                if ( !String.IsNullOrEmpty( request.WhyAttendSixtyFive ) )
                {
                    message += "<strong>Describe Why Someone Should Attend Your Event (450):</strong> " + request.WhyAttendSixtyFive + "<br/>";
                }
                if ( !String.IsNullOrEmpty( request.TargetAudience ) )
                {
                    message += "<strong>Target Audience:</strong> " + request.TargetAudience + "<br/>";
                }
                message += "<strong>Event is Sticky:</strong> " + ( request.EventIsSticky == true ? "Yes" : "No" ) + "<br/>";
                if ( request.PublicityStartDate.HasValue )
                {
                    message += "<strong>Publicity Start Date:</strong> " + request.PublicityStartDate.Value.ToString( "MM/dd/yyyy" ) + "<br/>";
                }
                if ( request.PublicityEndDate.HasValue )
                {
                    message += "<strong>Publicity End Date:</strong> " + request.PublicityEndDate.Value.ToString( "MM/dd/yyyy" ) + "<br/>";
                }
                if ( request.PublicityStrategies != null && request.PublicityStrategies.Count() > 0 )
                {
                    message += "<strong>Publicity Strategies:</strong> " + String.Join( ", ", request.PublicityStrategies ) + "<br/>";

                    if ( request.PublicityStrategies.Contains( "Social Media/Google Ads" ) )
                    {
                        message += "<br/><strong style='color: #6485b3;'>Social Media/Google Information</strong><br/>";
                        if ( !String.IsNullOrEmpty( request.WhyAttendNinety ) )
                        {
                            message += "<strong>Describe Why Someone Should Attend Your Event (90):</strong> " + request.WhyAttendNinety + "<br/>";
                        }
                        if ( request.GoogleKeys != null && request.GoogleKeys.Count() > 0 )
                        {
                            message += "<strong>Google Keys:</strong> <ul>";
                            for ( int i = 0; i < request.GoogleKeys.Count(); i++ )
                            {
                                message += "<li>" + request.GoogleKeys[i] + "</li>";
                            }
                            message += "</ul>";
                        }
                    }

                    if ( request.PublicityStrategies.Contains( "Mobile Worship Folder" ) )
                    {
                        message += "<br/><strong style='color: #6485b3;'>Mobile Worship Folder Information</strong><br/>";
                        if ( !String.IsNullOrEmpty( request.WhyAttendTen ) )
                        {
                            message += "<strong>Describe Why Someone Should Attend Your Event (65):</strong> " + request.WhyAttendTen + "<br/>";
                        }
                        if ( !String.IsNullOrEmpty( request.VisualIdeas ) )
                        {
                            message += "<strong>Visual Ideas for Graphic:</strong> " + request.VisualIdeas + "<br/>";
                        }
                    }

                    if ( request.PublicityStrategies.Contains( "Announcement" ) )
                    {
                        message += "<br/><strong style='color: #6485b3;'>Announcement Information</strong><br/>";
                        if ( request.Stories != null && request.Stories.Count() > 0 )
                        {
                            for ( int i = 0; i < request.Stories.Count(); i++ )
                            {
                                if ( !String.IsNullOrEmpty( request.Stories[i].Name ) && !String.IsNullOrEmpty( request.Stories[i].Email ) && !String.IsNullOrEmpty( request.Stories[i].Description ) )
                                {
                                    message += "<strong>Story " + i + ":</strong> " + request.Stories[i].Name + ", " + request.Stories[i].Email + "<br/>";
                                    message += request.Stories[i].Description + "<br/>";
                                }
                            }
                        }
                        if ( !String.IsNullOrEmpty( request.WhyAttendTwenty ) )
                        {
                            message += "<strong>Describe Why Someone Should Attend Your Event (175):</strong> " + request.WhyAttendTwenty + "<br/>";
                        }
                    }
                }
            }

            if ( !String.IsNullOrEmpty( request.Notes ) )
            {
                message += "<br/><strong style='color: #6485b3;'>Additional Notes</strong><br/>";
                message += "<strong>Notes:</strong> " + request.Notes + "<br/>";
            }

            return message;
        }

        #endregion

        private class EventRequest
        {
            public bool needsSpace { get; set; }
            public bool needsOnline { get; set; }
            public bool needsPub { get; set; }
            public bool needsReg { get; set; }
            public bool needsCatering { get; set; }
            public bool needsChildCare { get; set; }
            public bool needsAccom { get; set; }
            public bool IsSame { get; set; }
            public string Name { get; set; }
            public string Ministry { get; set; }
            public string Contact { get; set; }
            public List<string> EventDates { get; set; }
            public List<EventDetails> Events { get; set; }
            public string WhyAttendSixtyFive { get; set; }
            public string TargetAudience { get; set; }
            public bool EventIsSticky { get; set; }
            public DateTime? PublicityStartDate { get; set; }
            public DateTime? PublicityEndDate { get; set; }
            public List<string> PublicityStrategies { get; set; }
            public string WhyAttendNinety { get; set; }
            public List<string> GoogleKeys { get; set; }
            public string WhyAttendTen { get; set; }
            public string VisualIdeas { get; set; }
            public List<StoryItem> Stories { get; set; }
            public string WhyAttendTwenty { get; set; }
            public string Notes { get; set; }
        }
        private class StoryItem
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Description { get; set; }
        }
        private class EventDetails
        {
            public string EventDate { get; set; }
            public string StartTime { get; set; }
            public string EndTime { get; set; }
            public int? MinsStartBuffer { get; set; }
            public int? MinsEndBuffer { get; set; }
            public int? ExpectedAttendance { get; set; }
            public List<string> Rooms { get; set; }
            public bool? Checkin { get; set; }
            public bool? SupportTeam { get; set; }
            public string EventURL { get; set; }
            public string ZoomPassword { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public DateTime? RegistrationEndDate { get; set; }
            public string RegistrationEndTime { get; set; }
            public List<string> FeeType { get; set; }
            public string Fee { get; set; }
            public string CoupleFee { get; set; }
            public string OnlineFee { get; set; }
            public string Sender { get; set; }
            public string SenderEmail { get; set; }
            public string ThankYou { get; set; }
            public string TimeLocation { get; set; }
            public string AdditionalDetails { get; set; }
            public string Vendor { get; set; }
            public string Menu { get; set; }
            public bool FoodDelivery { get; set; }
            public string FoodTime { get; set; }
            public string BudgetLine { get; set; }
            public string FoodDropOff { get; set; }
            public string CCVendor { get; set; }
            public string CCMenu { get; set; }
            public string CCFoodTime { get; set; }
            public string CCBudgetLine { get; set; }
            public List<string> ChildCareOptions { get; set; }
            public int? EstimatedKids { get; set; }
            public string CCStartTime { get; set; }
            public string CCEndTime { get; set; }
            public List<string> Drinks { get; set; }
            public string DrinkDropOff { get; set; }
            public string DrinkTime { get; set; }
            public List<string> TechNeeds { get; set; }
            public bool ShowOnCalendar { get; set; }
            public string PublicityBlurb { get; set; }
            public string TechDescription { get; set; }
            public string SetUp { get; set; }
        }
        private Dictionary<string, string> LocationCalendarLink
        {
            get
            {
                return new Dictionary<string, string>()
                {
                    { "Main Building", "AAMkADg4MDQ5ZWI2LWNiZDctNDhjNS1iN2E3LTdiZGY0NjlhM2Y3YQBGAAAAAACTvbFMzsQkTJoAHKXKm6KiBwAkIvEoPoD3TKdG3RiCVJj-AAAAAAEGAAAkIvEoPoD3TKdG3RiCVJj-AAARNgxcAAA=" },
                    { "Auditorium", "AAMkADg4MDQ5ZWI2LWNiZDctNDhjNS1iN2E3LTdiZGY0NjlhM2Y3YQBGAAAAAACTvbFMzsQkTJoAHKXKm6KiBwAkIvEoPoD3TKdG3RiCVJj-AAAAAAEGAAAkIvEoPoD3TKdG3RiCVJj-AAA8aNryAAA=" },
                    { "Student Center", "AAMkADg4MDQ5ZWI2LWNiZDctNDhjNS1iN2E3LTdiZGY0NjlhM2Y3YQBGAAAAAACTvbFMzsQkTJoAHKXKm6KiBwAkIvEoPoD3TKdG3RiCVJj-AAAAAAEGAAAkIvEoPoD3TKdG3RiCVJj-AAARNgxfAAA=" },
                    { "Gym", "AAMkADg4MDQ5ZWI2LWNiZDctNDhjNS1iN2E3LTdiZGY0NjlhM2Y3YQBGAAAAAACTvbFMzsQkTJoAHKXKm6KiBwAkIvEoPoD3TKdG3RiCVJj-AAAAAAEGAAAkIvEoPoD3TKdG3RiCVJj-AAARNgxbAAA=" }
                };
            }
        }
        private class CalendarRoomLink
        {
            public string Calendar { get; set; }
            public List<string> Rooms { get; set; }
            public Dictionary<string, string> Events { get; set; }
        }
    }
}