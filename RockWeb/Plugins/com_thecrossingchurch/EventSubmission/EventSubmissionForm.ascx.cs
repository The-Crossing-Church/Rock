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
using System.Threading.Tasks;
using RockWeb.TheCrossing;
using EventRequest = RockWeb.TheCrossing.EventSubmissionHelper.EventRequest;
using Comment = RockWeb.TheCrossing.EventSubmissionHelper.Comment;
using System.Web.Optimization;

namespace RockWeb.Plugins.com_thecrossingchurch.EventSubmission
{
    /// <summary>
    /// Request form for Event Submissions
    /// </summary>
    [DisplayName( "Event Submission Form" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "Request form for Event Submissions" )]

    [DefinedTypeField( "Room List", "The defined type for the list of available rooms", true, "", "", 0 )]
    [DefinedTypeField( "Ministry List", "The defined type for the list of ministries", true, "", "", 1 )]
    [DefinedTypeField( "Budget Lines", "The defined type for the list of budget lines", true, "", "", 2 )]
    [ContentChannelField( "Content Channel", "The conent channel for event requests", true, "", "", 3 )]
    [LinkedPage( "Request Page", "The Request Form Page", true, "", "", 4 )]
    [LinkedPage( "Dashboard Page", "The Request Dashboard Page", true, "", "", 5 )]
    [LinkedPage( "User Dashboard Page", "The Request Dashboard Page", true, "", "", 6 )]
    [SecurityRoleField( "Super User Role", "People who can make full requests", true, "", "", 7 )]
    [SecurityRoleField( "Room Request Admin", "The role for people handling the room only requests who need to be notified", true, "", "", 8 )]
    [SecurityRoleField( "Event Request Admin", "The role for people handling all other requests who need to be notified", true, "", "", 9 )]

    public partial class EventSubmissionForm : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext context { get; set; }
        private EventSubmissionHelper eventSubmissionHelper { get; set; }
        private string BaseURL { get; set; }
        private string RequestPageId { get; set; }
        private Guid? RequestPageGuid { get; set; }
        private Guid? UserDashboardPageGuid { get; set; }
        private string DashboardPageId { get; set; }
        private int RoomDefinedTypeId { get; set; }
        private int MinistryDefinedTypeId { get; set; }
        private int BudgetDefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private int ContentChannelTypeId { get; set; }
        private List<DefinedValue> Rooms { get; set; }
        private List<DefinedValue> Doors { get; set; }
        private List<DefinedValue> Ministries { get; set; }
        private List<DefinedValue> BudgetLines { get; set; }
        private Rock.Model.Group RoomOnlySR { get; set; }
        private Rock.Model.Group EventSR { get; set; }
        private bool CurrentPersonIsRoomAdmin { get; set; }
        private bool CurrentPersonIsEventAdmin { get; set; }
        private bool CurrentPersonIsSuperUser { get; set; }
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

            Guid? RoomDefinedTypeGuid = GetAttributeValue( "RoomList" ).AsGuidOrNull();
            Guid? MinistryDefinedTypeGuid = GetAttributeValue( "MinistryList" ).AsGuidOrNull();
            Guid? BudgetDefinedTypeGuid = GetAttributeValue( "BudgetLines" ).AsGuidOrNull();
            Guid? ContentChannelGuid = GetAttributeValue( "ContentChannel" ).AsGuidOrNull();

            eventSubmissionHelper = new EventSubmissionHelper( RoomDefinedTypeGuid, MinistryDefinedTypeGuid, BudgetDefinedTypeGuid, ContentChannelGuid );
            hfRooms.Value = eventSubmissionHelper.RoomsJSON;
            Rooms = eventSubmissionHelper.Rooms;
            hfDoors.Value = eventSubmissionHelper.DoorsJSON;
            Doors = eventSubmissionHelper.Doors;
            hfMinistries.Value = eventSubmissionHelper.MinistriesJSON;
            Ministries = eventSubmissionHelper.Ministries;
            hfBudgetLines.Value = eventSubmissionHelper.BudgetLinesJSON;
            BudgetLines = eventSubmissionHelper.BudgetLines;
            ContentChannelId = eventSubmissionHelper.ContentChannelId;
            ContentChannelTypeId = eventSubmissionHelper.ContentChannelTypeId;
            BaseURL = eventSubmissionHelper.BaseURL;

            RequestPageGuid = GetAttributeValue( "RequestPage" ).AsGuidOrNull();
            Guid? DashboardPageGuid = GetAttributeValue( "DashboardPage" ).AsGuidOrNull();
            UserDashboardPageGuid = GetAttributeValue( "UserDashboardPage" ).AsGuidOrNull();
            if ( RequestPageGuid.HasValue && DashboardPageGuid.HasValue )
            {
                RequestPageId = new PageService( context ).Get( RequestPageGuid.Value ).Id.ToString();
                DashboardPageId = new PageService( context ).Get( DashboardPageGuid.Value ).Id.ToString();
            }

            Guid? superUserGuid = GetAttributeValue( "SuperUserRole" ).AsGuidOrNull();
            Guid? RoomSRGuid = GetAttributeValue( "RoomRequestAdmin" ).AsGuidOrNull();
            Guid? EventSRGuid = GetAttributeValue( "EventRequestAdmin" ).AsGuidOrNull();

            //Throw an error if not all values are present
            if ( !RoomDefinedTypeGuid.HasValue || !MinistryDefinedTypeGuid.HasValue || !BudgetDefinedTypeGuid.HasValue || !ContentChannelGuid.HasValue || String.IsNullOrEmpty( BaseURL ) || !RequestPageGuid.HasValue || !DashboardPageGuid.HasValue || !UserDashboardPageGuid.HasValue || !superUserGuid.HasValue || !RoomSRGuid.HasValue || !EventSRGuid.HasValue )
            {
                return;
            }

            hfIsAdmin.Value = "False";
            hfIsSuperUser.Value = "False";
            CurrentPersonIsEventAdmin = false;
            CurrentPersonIsRoomAdmin = false;
            CurrentPersonIsSuperUser = false;
            if ( RoomSRGuid.HasValue )
            {
                RoomOnlySR = new GroupService( context ).Get( RoomSRGuid.Value );
                if ( CurrentPersonId.HasValue && RoomOnlySR.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Select( m => m.PersonId ).ToList().Contains( CurrentPersonId.Value ) )
                {
                    CurrentPersonIsRoomAdmin = true;
                }
            }
            if ( EventSRGuid.HasValue )
            {
                EventSR = new GroupService( context ).Get( EventSRGuid.Value );
                if ( CurrentPersonId.HasValue && EventSR.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Select( m => m.PersonId ).ToList().Contains( CurrentPersonId.Value ) )
                {
                    hfIsAdmin.Value = "True";
                    CurrentPersonIsEventAdmin = true;
                }
            }
            if ( superUserGuid.HasValue )
            {
                Rock.Model.Group superUsers = new GroupService( context ).Get( superUserGuid.Value );
                if ( CurrentPersonId.HasValue && superUsers.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Select( m => m.PersonId ).ToList().Contains( CurrentPersonId.Value ) )
                {
                    hfIsSuperUser.Value = "True";
                    CurrentPersonIsSuperUser = true;
                }
            }
            hfPersonName.Value = CurrentPerson.FullName;
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
            string requestType = eventSubmissionHelper.GetRequestResources( request );
            string status = "Submitted";
            string isPreApproved = "No";
            List<string> notPreApprovedreason = new List<string>();

            //Pre-Approval Check
            //Requests for only a space, between 9am and 9pm (Mon-Fri) 1pm and 9pm (Sun) or 9am and 12pm (Sat), within the next 14 days, not in Gym or Auditorium, and no more than 30 people attending can be pre-approved
            if ( requestType == "Room" && !request.HasConflicts ) //Room only, no conflicts found
            {
                var allDatesInNextWeek = true;
                for ( var i = 0; i < request.EventDates.Count(); i++ )
                {
                    var totalDays = ( DateTime.Parse( request.EventDates[i] ) - DateTime.Now ).TotalDays;
                    if ( totalDays >= 14 )
                    {
                        allDatesInNextWeek = false;
                    }
                }
                if ( allDatesInNextWeek ) //Request is within the next 7 days
                {
                    int? expAtt = request.Events.Select( ev => ev.ExpectedAttendance ).Min();
                    if ( expAtt.HasValue && expAtt.Value <= 30 ) //No more than 30 people attending
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
                                if ( Int32.Parse( info[0] ) >= 9 && Int32.Parse( info[0] ) < 12 )
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
                                    }
                                    else if ( dt.DayOfWeek == System.DayOfWeek.Saturday )
                                    {
                                        allMeetTimeRequirements = false;
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
                                else
                                {
                                    notPreApprovedreason.Add( "Request is in a location that requires approval" );
                                }
                            }
                            else
                            {
                                notPreApprovedreason.Add( "Request is outside of business hours" );
                            }
                        }
                    }
                    else
                    {
                        notPreApprovedreason.Add( "Expected Attendance is not less than or equal to 30" );
                    }
                }
                else
                {
                    notPreApprovedreason.Add( "Request is not within the next 14 days" );
                }
            }
            else
            {
                notPreApprovedreason.Add( "Request conflicts with another event" );
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
            //Save data about existing request and the new value of the request 
            string newValue = JsonConvert.SerializeObject( request );
            string existingValue = JsonConvert.SerializeObject( JsonConvert.DeserializeObject<EventRequest>( item.GetAttributeValue( "RequestJSON" ) ) );
            string currentStatus = item.GetAttributeValue( "RequestStatus" );

            //Update the created or modified person and time
            if ( isExisting )
            {
                item.ModifiedByPersonAliasId = CurrentPersonAliasId;
                item.ModifiedDateTime = RockDateTime.Now;
                if ( currentStatus == "Draft" )
                {
                    //If the form has not yet been submitted, only saved and now is being submitted for the first time. Update Submission Date (StartDateTime)
                    item.StartDateTime = RockDateTime.Now;
                }
            }
            else
            {
                item.CreatedByPersonAliasId = CurrentPersonAliasId;
                item.CreatedDateTime = RockDateTime.Now;
                item.StartDateTime = RockDateTime.Now;
            }

            item.Title = request.Name;

            if ( isPreApproved == "No" )
            {
                if ( currentStatus == "Approved" || currentStatus == "Pending Changes" )
                {
                    if ( CurrentPersonIsEventAdmin )
                    {
                        status = "Approved";
                    }
                    else
                    {
                        status = "Pending Changes";
                    }
                }
            }
            if ( currentStatus == "In Progress" )
            {
                status = currentStatus;
            }
            item.SetAttributeValue( "RequestStatus", status );

            //Changes are proposed if the event isn't pre-approved, is existing, and the requestor isn't in the Event Admin Role
            if ( item.Id > 0 && isPreApproved == "No" && status != "Submitted" && !CurrentPersonIsEventAdmin )
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
            item.SetAttributeValue( "ValidSections", String.Join( ",", request.ValidSections ) );
            item.SetAttributeValue( "RequestIsValid", request.IsValid.ToString() );
            item.SetAttributeValue( "IsPreApproved", isPreApproved );

            Dictionary<string, string> query = new Dictionary<string, string>();
            //If the request is new, was previously a draft, or has changed we should save and notify people
            if ( !isExisting || newValue != existingValue || currentStatus == "Draft" )
            {
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
                    if ( CurrentPersonId == item.CreatedByPersonId && ( status != "Submitted" || currentStatus == "Draft" ) )
                    {
                        //User is modifying their request, send notification
                        NotifyReviewers( item, request, isPreApproved, currentStatus == "Draft" ? false : true );
                        ConfirmationEmail( item, request, isPreApproved, currentStatus == "Draft" ? false : true );
                    }
                }
            }
            else
            {
                query.Add( "NoChange", "true" );
            }
            query.Add( "ShowSuccess", "true" );
            if ( isPreApproved == "Yes" )
            {
                query.Add( "PreApproved", "true" );
            }
            else
            {
                if ( requestType == "Room" )
                {
                    query.Add( "Reason", String.Join( ";", notPreApprovedreason ) );
                }
            }
            if ( isExisting )
            {
                query.Add( "Id", item.Id.ToString() );
            }
            NavigateToPage( RequestPageGuid.Value, query );
        }

        /// <summary>
        /// Save a request as a draft without submitting it
        /// </summary>
        protected void Save_Click( object sender, EventArgs e )
        {
            string raw = hfRequest.Value;
            EventRequest request = JsonConvert.DeserializeObject<EventRequest>( raw );
            string requestType = eventSubmissionHelper.GetRequestResources( request );
            string status = "Draft";
            string isPreApproved = "No";
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
                item.ModifiedDateTime = RockDateTime.Now;
            }
            else
            {
                item.CreatedByPersonAliasId = CurrentPersonAliasId;
                item.CreatedDateTime = RockDateTime.Now;
            }
            item.Title = request.Name;
            item.SetAttributeValue( "RequestJSON", raw );
            item.SetAttributeValue( "RequestStatus", status );
            item.SetAttributeValue( "EventDates", String.Join( ", ", request.EventDates ) );
            item.SetAttributeValue( "RequestType", requestType );
            item.SetAttributeValue( "ValidSecions", String.Join( ", ", request.ValidSections ) );
            item.SetAttributeValue( "RequestIsValid", request.IsValid.ToString() );
            item.SetAttributeValue( "IsPreApproved", isPreApproved );
            //Save everything
            context.ContentChannelItems.AddOrUpdate( item );
            context.SaveChanges();
            item.SaveAttributeValues( context );
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "Id", item.Id.ToString() );
            NavigateToPage( UserDashboardPageGuid.Value, query );
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
                            "<a href='" + BaseURL + "page/" + DashboardPageId + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Open Request</a>" +
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
            email.CreateCommunicationRecord = true;
            var output = email.Send();

            //Redirect
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "ShowSuccess", "true" );
            query.Add( "Id", id.ToString() );
            NavigateToPage( RequestPageGuid.Value, query );
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
            List<string> sharedWithIds = item.GetAttributeValue( "SharedWith" ).Split( ',' ).ToList();
            bool canEdit = false;
            if ( item.CreatedByPersonId == CurrentPersonId || item.ModifiedByPersonId == CurrentPersonId || sharedWithIds.Contains( CurrentPersonId.ToString() ) )
            {
                string status = item.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value;
                if ( status == "Draft" || status == "Submitted" || status == "In Progress" || status == "Approved" )
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
            hfRequest.Value = JsonConvert.SerializeObject( new { Id = item.Id, Value = item.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, CreatedBy = item.CreatedByPersonId, CreatedOn = item.CreatedDateTime, Active = item.StartDateTime, RequestStatus = item.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value, CanEdit = canEdit } );
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
            var itm = items[0];
            itm.LoadAttributes();
            var statusAttrId = itm.Attributes["RequestStatus"].Id;
            var statuses = new AttributeValueService( context ).Queryable().Where( av => av.AttributeId == statusAttrId && av.Value != "Draft" && av.Value != "Submitted" && !av.Value.Contains( "Cancelled" ) && av.Value != "Denied" );
            items = items.Join( statuses,
                    i => i.Id,
                    s => s.EntityId,
                    ( i, s ) => i
                ).ToList();
            var dateAttrId = itm.Attributes["EventDates"].Id;
            var eventDates = new AttributeValueService( context ).Queryable().Where( av => av.AttributeId == dateAttrId ).ToList();
            var startOfToday = new DateTime( RockDateTime.Now.Year, RockDateTime.Now.Month, RockDateTime.Now.Day, 0, 0, 0 );
            eventDates = eventDates.Where( e =>
            {
                var dates = e.Value.Split( ',' );
                foreach ( var d in dates )
                {
                    DateTime dt = DateTime.Parse( d );
                    if ( DateTime.Compare( dt, startOfToday ) >= 0 )
                    {
                        return true;
                    }
                }
                return false;
            } ).ToList();
            items = items.Join( eventDates,
                    i => i.Id,
                    e => e.EntityId,
                    ( i, e ) => i
                ).ToList();

            items.LoadAttributes();
            hfUpcomingRequests.Value = JsonConvert.SerializeObject( items.Select( i => new { Id = i.Id, data = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value } ).ToList() );
        }

        /// <summary>
        /// Notify Correct Users of a New Submission
        /// </summary>
        private void NotifyReviewers( ContentChannelItem item, EventRequest request, string isPreApproved, bool isRequestingChanges )
        {
            string message = "";
            string subject = "";
            List<GroupMember> groupMembers = new List<GroupMember>();
            if ( item.AttributeValues["IsPreApproved"].Value == "Yes" )
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
                    if ( request.Events[i].TableType.Count() > 0 )
                    {
                        message += "<strong>Requested Tables:</strong> " + String.Join( ", ", request.Events[i].TableType ) + "<br/>";
                    }
                    if ( request.Events[i].TableType.Contains( "Round" ) )
                    {
                        message += "<strong>Number of Round Tables:</strong> " + request.Events[i].NumTablesRound + "<br/>";
                        message += "<strong>Number of Chairs Per Round Table:</strong> " + request.Events[i].NumChairsRound + "<br/>";
                    }
                    if ( request.Events[i].TableType.Contains( "Rectangular" ) )
                    {
                        message += "<strong>Number of Rectangular Tables:</strong> " + request.Events[i].NumTablesRect + "<br/>";
                        message += "<strong>Number of Chairs Per Rectangular Table:</strong> " + request.Events[i].NumChairsRect + "<br/>";
                    }
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
                            "<a href='" + BaseURL + "page/" + DashboardPageId + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Open Request</a>" +
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
            email.CreateCommunicationRecord = true;
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
                subject = "Your Request has been approved";
                message = "Your room/space request has been approved. The details of your request are as follows: <br/>";
            }
            else
            {
                if ( isRequestingChanges )
                {
                    message = "Your changes have been submitted, someone will review your changes and notify you if they are approved. You can expect a response from the Events Director within 48 hours and/or 2 business days, if not sooner. Thank you!<br/>The details of your request are as follows: <br/>";
                }
                else
                {
                    message = "Your Event Request has been submitted and is pending approval. You can expect a response from the Events Director within 48 hours and/or 2 business days, if not sooner. Thank you!<br/>The details of your request are as follows: <br/>";
                }
            }
            message += GenerateEmailDetails( item, request );
            if ( CurrentPersonIsSuperUser )
            {
                DateTime firstDate = request.EventDates.Select( e => DateTime.Parse( e ) ).OrderBy( e => e.Date ).FirstOrDefault();
                DateTime twoWeekDate = firstDate.AddDays( -14 );
                DateTime thirtyDayDate = firstDate.AddDays( -30 );
                DateTime sixWeekDate = firstDate.AddDays( -43 );
                DateTime today = RockDateTime.Now;
                today = new DateTime( today.Year, today.Month, today.Day, 0, 0, 0 );
                List<String> unavailableResources = new List<String>();
                if ( twoWeekDate >= today )
                {
                    message += "<br/><div><strong>Important Dates for Your Request</strong></div>";
                    message += "Last date to request and provide all information for the following resources is <strong>" + twoWeekDate.ToShortDateString() + "</strong>:";
                    message += "<ul>" +
                            "<li>Zoom</li>" +
                            "<li>Catering</li>" +
                            "<li>Registration</li>" +
                            "<li>Extra Accommodations</li>" +
                        "</ul> <br/>";
                    if ( thirtyDayDate >= today )
                    {
                        message += "Last date to request and provide all information for the following resources is <strong>" + thirtyDayDate.ToShortDateString() + "</strong>:";
                        message += "<ul><li>Childcare</li></ul>";
                        if ( sixWeekDate >= today )
                        {
                            message += "Last date to request and provide all information for the following resources is <strong>" + sixWeekDate.ToShortDateString() + "</strong>:";
                            message += "<ul><li>Publicity</li></ul>";
                        }
                        else
                        {
                            unavailableResources.Add( "Publicity" );
                            //message += "There is not enough time between now and your first event date to allow for Publicity.";
                        }
                    }
                    else
                    {
                        unavailableResources.Add( "Childcare" );
                        unavailableResources.Add( "Publicity" );
                        //message += "There is not enough time between now and your first event date to allow for Childcare.";
                    }
                }
                else
                {
                    unavailableResources.Add( "Zoom" );
                    unavailableResources.Add( "Catering" );
                    unavailableResources.Add( "Extra Accommodations" );
                    unavailableResources.Add( "Registration" );
                    unavailableResources.Add( "Childcare" );
                    unavailableResources.Add( "Publicity" );
                }
                if ( unavailableResources.Count() > 0 )
                {
                    message += "<div>There is not enough time between now and your first event date to allow for the following resources:</div>";
                    message += "<ul>";
                    for ( int i = 0; i < unavailableResources.Count(); i++ )
                    {
                        message += "<li>" + unavailableResources[i] + "</li>";
                    }
                    message += "</ul>";
                }
            }
            message += "<br/>" +
                "<table style='width: 100%;'>" +
                    "<tr>" +
                        "<td></td>" +
                        "<td style='text-align:center;'>" +
                            "<strong>See a mistake? You can modify your request using the link below. If your request was already approved the changes you make will have to be approved as well.</strong><br/><br/><br/>" +
                            "<a href='" + BaseURL + "page/" + RequestPageId + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Modify Request</a>" +
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
            email.CreateCommunicationRecord = true;
            var output = email.Send();
        }

        private string GenerateEmailDetails( ContentChannelItem item, EventRequest request )
        {
            string message = "<br/>";
            message += "<strong style='font-size: 16px;'>Ministry:</strong> <span style='font-size: 16px;'>" + ( !String.IsNullOrEmpty( request.Ministry ) ? Ministries.FirstOrDefault( dv => dv.Id.ToString() == request.Ministry ).Value : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "</span><br/>";
            if ( item.AttributeValues["RequestType"].Value == "Room" )
            {
                message += "<strong style='font-size: 16px;'>Meeting Listing on the Calendar:</strong> <span style='font-size: 16px;'>" + request.Name + "</span><br/>";
            }
            else
            {
                message += "<strong style='font-size: 16px;'>Event Name:</strong> <span style='font-size: 16px;'>" + request.Name + "</span><br/>";
            }
            message += "<strong style='font-size: 16px;'>Ministry Contact:</strong> <span style='font-size: 16px;'>" + request.Contact + "</span><br/><br/>";

            if ( item.AttributeValues["RequestType"].Value != "Room" )
            {
                message += "<strong>Requested Resources:</strong> " + String.Join( ", ", item.AttributeValues["RequestType"].Value.Split( ',' ) ) + "<br/><br/>";
            }

            for ( int i = 0; i < request.Events.Count(); i++ )
            {
                if ( i > 0 )
                {
                    message += "<br/><hr/><br/>";
                }
                message += "<div style='font-size: 18px;'><strong style='color: #6485b3;'>Date Information</strong><br/>";
                if ( request.Events.Count() == 1 || request.IsSame )
                {
                    message += "<strong>Event Dates:</strong> " + String.Join( ", ", request.EventDates.Select( e => DateTime.Parse( e ).ToString( "MM/dd/yyyy" ) ) ) + "<br/>";
                }
                else
                {
                    message += "<strong>Date:</strong> " + DateTime.Parse( request.Events[i].EventDate ).ToString( "MM/dd/yyyy" ) + "<br/>";
                }
                if ( !String.IsNullOrEmpty( request.Events[i].StartTime ) )
                {
                    message += "<strong>Start Time:</strong> " + request.Events[i].StartTime + "<br/>";
                }
                if ( !String.IsNullOrEmpty( request.Events[i].EndTime ) )
                {
                    message += "<strong>End Time:</strong> " + request.Events[i].EndTime + "<br/>";
                }
                message += "</div>";

                if ( request.needsSpace )
                {
                    message += "<br/><strong style='color: #6485b3;'>Room Information</strong><br/>";
                    message += "<strong>Requested Rooms:</strong> " + ( request.Events[i].Rooms.Count() > 0 ? String.Join( ", ", Rooms.Where( dv => request.Events[i].Rooms.Contains( dv.Id.ToString() ) ).Select( dv => dv.Value ) ) : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                    if ( !String.IsNullOrEmpty( request.Events[i].InfrastructureSpace ) )
                    {
                        message += "<strong>Other Spaces:</strong> " + request.Events[i].InfrastructureSpace + "<br/>";
                    }
                    if ( request.Events[i].TableType.Count() > 0 )
                    {
                        message += "<strong>Requested Tables:</strong> " + String.Join( ", ", request.Events[i].TableType ) + "<br/>";
                    }
                    if ( request.Events[i].TableType.Contains( "Round" ) )
                    {
                        message += "<strong>Number of Round Tables:</strong> " + request.Events[i].NumTablesRound + "<br/>";
                        message += "<strong>Number of Chairs Per Round Table:</strong> " + request.Events[i].NumChairsRound + "<br/>";
                    }
                    if ( request.Events[i].TableType.Contains( "Rectangular" ) )
                    {
                        message += "<strong>Number of Rectangular Tables:</strong> " + request.Events[i].NumTablesRect + "<br/>";
                        message += "<strong>Number of Chairs Per Rectangular Table:</strong> " + request.Events[i].NumChairsRect + "<br/>";
                    }
                    if ( request.Events[i].TableType.Count() > 0 && request.Events[i].NeedsTableCloths.HasValue )
                    {
                        message += "<strong>Needs Tablecloths:</strong> " + ( request.Events[i].NeedsTableCloths.Value ? "Yes" : "No" ) + "<br/>";
                    }
                    if ( item.AttributeValues["RequestType"].Value != "Room" )
                    {
                        message += "<strong>Needs In-Person Check-in:</strong> " + ( request.Events[i].Checkin.Value == true ? "Yes" : "No" ) + "<br/>";
                    }
                    if ( request.Events[i].ExpectedAttendance.HasValue )
                    {
                        message += "<strong>Expected Attendance:</strong> " + request.Events[i].ExpectedAttendance.Value + "<br/>";
                    }
                    else
                    {
                        message += "<strong>Expected Attendance:</strong> <span style='font-weight: bold; color: #CC3F0C;'>n/a</span><br/>";
                    }
                    if ( request.Events[i].Checkin.Value == true && request.Events[i].ExpectedAttendance >= 100 )
                    {
                        message += "<strong>Requested Database Team Support:</strong> " + ( request.Events[i].SupportTeam.Value == true ? "Yes" : "No" ) + "<br/>";
                    }
                }

                if ( request.needsCatering )
                {
                    message += "<br/><strong style='color: #6485b3;'>Food/Drink Information</strong><br/>";
                    message += "<strong>Preferred Vendor:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].Vendor ) ? request.Events[i].Vendor : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                    message += "<strong>Preferred Menu:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].Menu ) ? request.Events[i].Menu : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                    message += "<strong>Budget Line:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].BudgetLine ) ? BudgetLines.Where( dv => request.Events[i].BudgetLine == dv.Id.ToString() ).Select( dv => dv.Value ).FirstOrDefault() : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                    if ( request.Events[i].FoodDelivery )
                    {
                        message += "<strong>Food Set-Up Time:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].FoodTime ) ? request.Events[i].FoodTime : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                        message += "<strong>Food Set-Up Location:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].FoodDropOff ) ? request.Events[i].FoodDropOff : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                        if ( request.Events[i].TableType.Count() == 0 && request.Events[i].NeedsTableCloths.HasValue )
                        {
                            message += "<strong>Needs Tablecloths:</strong> " + ( request.Events[i].NeedsTableCloths.Value ? "Yes" : "No" ) + "<br/>";
                        }
                    }
                    else
                    {
                        message += "<strong>Desired Pick-up time from Vendor:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].FoodTime ) ? request.Events[i].FoodTime : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
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
                    message += "<strong>Event Link:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].EventURL ) ? request.Events[i].EventURL : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                    if ( !String.IsNullOrEmpty( request.Events[i].ZoomPassword ) )
                    {
                        message += "<strong>Zoom Password:</strong> " + request.Events[i].ZoomPassword + "<br/>";
                    }
                }

                if ( request.needsChildCare )
                {
                    message += "<br/><strong style='color: #6485b3;'>Childcare Information</strong><br/>";
                    message += "<strong>Childcare Age Groups:</strong> " + ( ( request.Events[i].ChildCareOptions != null && request.Events[i].ChildCareOptions.Count() > 0 ) ? String.Join( ", ", request.Events[i].ChildCareOptions ) : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                    if ( request.Events[i].EstimatedKids.HasValue )
                    {
                        message += "<strong>Expected Number of Children:</strong> " + request.Events[i].EstimatedKids + "<br/>";
                    }
                    else
                    {
                        message += "<strong>Expected Number of Children:</strong> <span style='font-weight: bold; color: #CC3F0C;'>n/a</span><br/>";
                    }
                    message += "<strong>Childcare Start Time:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].CCStartTime ) ? request.Events[i].CCStartTime : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                    message += "<strong>Childcare End Time:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].CCEndTime ) ? request.Events[i].CCEndTime : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                    if ( request.needsCatering )
                    {
                        message += "<strong>Preferred Vendor for Childcare:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].CCVendor ) ? request.Events[i].CCVendor : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                        message += "<strong>Preferred Menu for Childcare:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].CCMenu ) ? request.Events[i].CCMenu : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
                        message += "<strong>Budget Line for Childcare:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].CCBudgetLine ) ? BudgetLines.Where( dv => request.Events[i].CCBudgetLine == dv.Id.ToString() ).Select( dv => dv.Value ).FirstOrDefault() : "Not Entered" ) + "<br/>";
                        message += "<strong>Childcare Food Set-Up Time:</strong> " + ( !String.IsNullOrEmpty( request.Events[i].CCFoodTime ) ? request.Events[i].CCFoodTime : "<span style='font-weight: bold; color: #CC3F0C;'>n/a</span>" ) + "<br/>";
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
                    if ( !String.IsNullOrEmpty( request.Events[i].FeeBudgetLine ) )
                    {
                        message += "<strong>Registration Fee Budget Line:</strong> " + BudgetLines.Where( dv => request.Events[i].FeeBudgetLine == dv.Id.ToString() ).Select( dv => dv.Value ).FirstOrDefault() + "<br/>";
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
                    if ( !String.IsNullOrEmpty( request.Events[i].Sender ) )
                    {
                        message += "<strong>Confirmation Email Sender:</strong> " + request.Events[i].Sender + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].SenderEmail ) )
                    {
                        message += "<strong>Confirmation Email From Address:</strong> " + request.Events[i].SenderEmail + "<br/>";
                    }
                    if ( !String.IsNullOrEmpty( request.Events[i].AdditionalDetails ) )
                    {
                        message += "<strong>Confirmation Email Additional Details:</strong> " + request.Events[i].AdditionalDetails + "<br/>";
                    }
                    if ( request.Events[i].NeedsReminderEmail )
                    {
                        if ( !String.IsNullOrEmpty( request.Events[i].ReminderSender ) )
                        {
                            message += "<strong>Reminder Email Sender:</strong> " + request.Events[i].ReminderSender + "<br/>";
                        }
                        if ( !String.IsNullOrEmpty( request.Events[i].ReminderSenderEmail ) )
                        {
                            message += "<strong>Reminder Email From Address:</strong> " + request.Events[i].ReminderSenderEmail + "<br/>";
                        }
                        if ( !String.IsNullOrEmpty( request.Events[i].ReminderAdditionalDetails ) )
                        {
                            message += "<strong>Reminder Email Additional Details:</strong> " + request.Events[i].ReminderAdditionalDetails + "<br/>";
                        }

                    }
                }

                if ( request.needsAccom )
                {
                    if ( CurrentPersonIsSuperUser || CurrentPersonIsEventAdmin || CurrentPersonIsRoomAdmin )
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

                    if ( CurrentPersonIsSuperUser || CurrentPersonIsEventAdmin || CurrentPersonIsRoomAdmin )
                    {
                        message += "<br/><strong style='color: #6485b3;'>Door Information</strong><br/>";
                        message += "<strong>Needs Doors Unlocked:</strong> " + ( request.Events[i].NeedsDoorsUnlocked == true ? "Yes" : "No" ) + "<br/>";
                        if ( request.Events[i].NeedsDoorsUnlocked == true )
                        {
                            message += "<strong>Doors Needed:</strong> " + String.Join( ", ", Doors.Where( dv => request.Events[i].Doors.Contains( dv.Id.ToString() ) ).Select( dv => dv.Value ) ) + "<br/>";
                        }

                        message += "<br/><strong style='color: #6485b3;'>Web Calendar Information</strong><br/>";
                        message += "<strong>Add to Public Calendar:</strong> " + ( request.Events[i].ShowOnCalendar == true ? "Yes" : "No" ) + "<br/>";
                        if ( request.Events[i].ShowOnCalendar && !String.IsNullOrEmpty( request.Events[i].PublicityBlurb ) )
                        {
                            message += "<strong>Publicity Blurb:</strong> " + request.Events[i].PublicityBlurb + "<br/>";
                        }

                        message += "<br/><strong style='color: #6485b3;'>Personnel Information</strong><br/>";
                        message += "<strong>Needs Medical Team:</strong> " + ( request.Events[i].NeedsMedical == true ? "Yes" : "No" ) + "<br/>";
                        message += "<strong>Needs Security Team:</strong> " + ( request.Events[i].NeedsSecurity == true ? "Yes" : "No" ) + "<br/>";
                    }
                }
                if ( !CurrentPersonIsSuperUser && request.Events[i].Drinks != null && request.Events[i].Drinks.Count() > 0 )
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
            }

            if ( request.needsPub )
            {
                message += "<br/><hr/>";
                message += "<br/><strong style='color: #6485b3;'>Publicity Information</strong><br/>";
                if ( !String.IsNullOrEmpty( request.WhyAttendSixtyFive ) )
                {
                    message += "<strong>Describe Why Someone Should Attend Your Event:</strong> " + request.WhyAttendSixtyFive + "<br/>";
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

    }
}