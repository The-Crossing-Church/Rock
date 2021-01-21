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

namespace RockWeb.Plugins.com_thecrossingchurch.EventSubmission
{
    /// <summary>
    /// Request form for Event Submissions
    /// </summary>
    [DisplayName( "Event Submission Form" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "Request form for Event Submissions" )]

    [IntegerField( "DefinedTypeId", "The id of the defined type for rooms.", true, 0, "", 0 )]
    [IntegerField( "MinistryDefinedTypeId", "The id of the defined type for ministries.", true, 0, "", 0 )]
    [IntegerField( "ContentChannelId", "The id of the content channel for an event request.", true, 0, "", 0 )]
    [IntegerField( "ContentChannelTypeId", "The id of the content channel type for an event request.", true, 0, "", 0 )]
    [TextField( "Page Guid", "The guid of the page for redirect on save.", true, "", "", 0 )]
    [SecurityRoleField( "Room Request Admin", "The role for people handling the room only requests who need to be notified", true )]
    [SecurityRoleField( "Event Request Admin", "The role for people handling all other requests who need to be notified", true )]

    public partial class EventSubmissionForm : Rock.Web.UI.RockBlock
    {
        #region Variables
        public RockContext context { get; set; }
        private int DefinedTypeId { get; set; }
        private int MinistryDefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private int ContentChannelTypeId { get; set; }
        private List<DefinedValue> Rooms { get; set; }
        private List<DefinedValue> Ministries { get; set; }
        private Group RoomOnlySR { get; set; }
        private Group EventSR { get; set; }
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
            var RoomSRGuid = GetAttributeValue( "RoomRequestAdmin" ).AsGuidOrNull();
            var EventSRGuid = GetAttributeValue( "EventRequestAdmin" ).AsGuidOrNull();
            if ( RoomSRGuid.HasValue )
            {
                RoomOnlySR = new GroupService( context ).Get( RoomSRGuid.Value );
            }
            if ( EventSRGuid.HasValue )
            {
                EventSR = new GroupService( context ).Get( EventSRGuid.Value );
            }
            Rooms = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == DefinedTypeId ).ToList();
            hfRooms.Value = JsonConvert.SerializeObject( Rooms.Select( dv => new { Id = dv.Id, Value = dv.Value } ) );
            Ministries = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == MinistryDefinedTypeId ).ToList();
            hfMinistries.Value = JsonConvert.SerializeObject( Ministries.Select( dv => new { Id = dv.Id, Value = dv.Value } ) );
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
            bool isExisting = false;
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                int id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
                item = svc.Get( id );
                isExisting = true;
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
            if ( String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                NotifyReviewers( item, request, isPreApproved );
                ConfirmationEmail( item, request, isPreApproved );
            }
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "ShowSuccess", "true" );
            if( isExisting )
            {
                query.Add( "Id", item.Id.ToString() );
            }
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
            hfRequest.Value = item.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value;
        }

        /// <summary>
        /// Load upcoming requests
        /// </summary>
        protected void LoadUpcoming()
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            List<ContentChannelItem> items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).ToList();
            items.LoadAttributes();
            items = items.Where( i =>
            {
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
        private void NotifyReviewers( ContentChannelItem item, EventRequest request, string isPreApproved )
        {
            string message = "";
            string subject = "";
            List<GroupMember> groupMembers = new List<GroupMember>();
            if ( item.AttributeValues["RequestType"].Value == "Room" )
            {
                //Notify the Room Only Request Group
                subject = "New Room Request from " + CurrentPerson.FullName;
                groupMembers = RoomOnlySR.Members.ToList();
                message = CurrentPerson.FullName + " has submitted a room request for " + Ministries.FirstOrDefault( dv => dv.Id.ToString() == request.Ministry ).Value + ".<br/>";
                message += "<strong>Ministry Contact:</strong> " + request.Contact + "<br/>";
                message += "<strong>Event Dates:</strong> " + String.Join( ", ", request.EventDates.Select( e => DateTime.Parse( e ).ToString( "MM/dd/yyyy" ) ) ) + "<br/>";
                message += "<strong>Start Time:</strong> " + request.StartTime + "<br/>";
                message += "<strong>End Time:</strong> " + request.EndTime + "<br/>";
                message += "<strong>Requested Rooms:</strong> " + String.Join( ", ", Rooms.Where( dv => request.Rooms.Contains( dv.Id.ToString() ) ).Select( dv => dv.Value ) ) + "<br/>";
                message += "<strong>Expected Attendance:</strong> " + request.ExpectedAttendance + "<br/>";
                if ( isPreApproved == "Yes" )
                {
                    message += "Because of the date, time, location, and expected attendance this request has been pre-approved.<br/>";
                }
            }
            else
            {
                //Notify the Event Request Group
                subject = "New Event Request from " + CurrentPerson.FullName;
                groupMembers = EventSR.Members.ToList();
                message = GenerateEmailDetails( item, request );
            }
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
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
        private void ConfirmationEmail( ContentChannelItem item, EventRequest request, string isPreApproved )
        {
            string message = "";
            string subject = "";
            //Notify the Event Request Group
            subject = "Your Request has been submitted";
            if ( isPreApproved == "Yes" )
            {
                message = "Your room request has been submitted and is pre-approved due to the nature of your request. The details of your request are as follows: <br/>";
            }
            else
            {
                message = "Your request has been submitted, someone will review your request and notify you when it has been approved. Here are the details of your request that was submitted: <br/>";
            }
            message += GenerateEmailDetails( item, request );
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
            string message = "";
            message += "<strong>Ministry:</strong> " + Ministries.FirstOrDefault( dv => dv.Id.ToString() == request.Ministry ).Value + "<br/>";
            message += "<strong>Ministry Contact:</strong> " + request.Contact + "<br/>";
            message += "<strong>Requested Resources:</strong> " + item.AttributeValues["RequestType"].Value + "<br/>";
            message += "<strong>Event Dates:</strong> " + String.Join( ", ", request.EventDates.Select( e => DateTime.Parse( e ).ToString( "MM/dd/yyyy" ) ) ) + "<br/>";
            if ( !String.IsNullOrEmpty( request.StartTime ) )
            {
                message += "<strong>Start Time:</strong> " + request.StartTime + "<br/>";
            }
            if ( !String.IsNullOrEmpty( request.EndTime ) )
            {
                message += "<strong>End Time:</strong> " + request.EndTime + "<br/>";
            }
            if ( request.needsSpace )
            {
                message += "<strong>Requested Rooms:</strong> " + String.Join( ", ", Rooms.Where( dv => request.Rooms.Contains( dv.Id.ToString() ) ).Select( dv => dv.Value ) ) + "<br/>";
                message += "<strong>Needs Check-in:</strong> " + ( request.Checkin.Value == true ? "Yes" : "No" ) + "<br/>";
                message += "<strong>Expected Attendance:</strong> " + request.ExpectedAttendance + "<br/>";
            }
            if ( request.needsOnline )
            {
                message += "<strong>Event Link:</strong> " + request.EventURL + "<br/>";
                if ( !String.IsNullOrEmpty( request.ZoomPassword ) )
                {
                    message += "<strong>Zoom Password:</strong> " + request.ZoomPassword + "<br/>";
                }
            }
            if ( request.needsPub )
            {
                for ( var i = 0; i < request.Publicity.Count(); i++ )
                {
                    message += "<strong>Publicity Week " + ( i + 1 ) + ":</strong> " + DateTime.Parse( request.Publicity[i].Date ).ToString( "MM/dd/yyyy" ) + " - " + String.Join( ", ", request.Publicity[i].Needs ) + "<br/>";
                }
                if ( !String.IsNullOrEmpty( request.PublicityBlurb ) )
                {
                    message += "<strong>Publicity Blurb:</strong> " + request.PublicityBlurb + "<br/>";
                }
                message += "<strong>Add to Public Calendar:</strong> " + ( request.ShowOnCalendar == true ? "Yes" : "No" ) + "<br/>";
            }
            if ( request.needsChildCare )
            {
                message += "<strong>Childcare Age Groups:</strong> " + String.Join( ", ", request.ChildCareOptions ) + "<br/>";
                message += "<strong>Expected Number of Children:</strong> " + request.EstimatedKids + "<br/>";
                message += "<strong>Childcare Start Time:</strong> " + request.CCStartTime + "<br/>";
                message += "<strong>Childcare End Time:</strong> " + request.CCEndTime + "<br/>";
            }
            if ( request.needsCatering )
            {
                message += "<strong>Preferred Vendor:</strong> " + request.Vendor + "<br/>";
                message += "<strong>Budget Line:</strong> " + request.BudgetLine + "<br/>";
                message += "<strong>Preferred Menu:</strong> " + request.Menu + "<br/>";
                if ( request.FoodDelivery )
                {
                    message += "<strong>Food Set-up Time:</strong> " + request.FoodTime + "<br/>";
                    message += "<strong>Food Drop off Location:</strong> " + request.FoodDropOff + "<br/>";
                }
                else
                {
                    message += "<strong>Desired Pick-up time from Vendore:</strong> " + request.FoodTime + "<br/>";
                }
                if ( request.needsChildCare )
                {
                    message += "<strong>Preferred Vendor for Childcare:</strong> " + request.CCVendor + "<br/>";
                    message += "<strong>Budget Line for Childcare:</strong> " + request.CCBudgetLine + "<br/>";
                    message += "<strong>Preferred Menu for Childcare:</strong> " + request.CCMenu + "<br/>";
                    message += "<strong>ChildCare Food Set-up Time:</strong> " + request.CCFoodTime + "<br/>";
                }
            }
            if ( request.needsAccom )
            {
                if ( request.Drinks.Count() > 0 )
                {
                    message += "<strong>Drinks:</strong> " + String.Join( ", ", request.Drinks ) + "<br/>";
                }
                if ( request.TechNeeds.Count() > 0 )
                {
                    message += "<strong>Tech Needs:</strong> " + String.Join( ", ", request.TechNeeds ) + "<br/>";
                }
                if ( request.RegistrationDate.HasValue )
                {
                    message += "<strong>Registration Date:</strong> " + request.RegistrationDate.Value.ToString( "MM/dd/yyyy" ) + "<br/>";
                }
                if ( !String.IsNullOrEmpty( request.Fee ) )
                {
                    message += "<strong>Registration Fee:</strong> " + request.Fee + "<br/>";
                }
            }
            if ( !String.IsNullOrEmpty( request.Notes ) )
            {
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
            public bool? Checkin { get; set; }
            public string EventURL { get; set; }
            public string ZoomPassword { get; set; }
            public List<PublicityItem> Publicity { get; set; }
            public string PublicityBlurb { get; set; }
            public bool ShowOnCalendar { get; set; }
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
            public List<string> TechNeeds { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public string Fee { get; set; }
            public string Notes { get; set; }
        }

        private class PublicityItem
        {
            public string Date { get; set; }
            public List<string> Needs { get; set; }
        }
    }
}