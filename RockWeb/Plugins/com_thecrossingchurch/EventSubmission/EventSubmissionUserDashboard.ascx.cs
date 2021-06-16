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
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using System.Collections.ObjectModel;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Newtonsoft.Json;
using CSScriptLibrary;
using System.Data.Entity.Migrations;
using Microsoft.Identity.Client;
using System.Threading.Tasks;
using Rock.Communication;

namespace RockWeb.Plugins.com_thecrossingchurch.EventSubmission
{
    /// <summary>
    /// Request form for Event Submissions
    /// </summary>
    [DisplayName( "Event Submission User Dashboard" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "Dashboard for Current User's Event Submissions" )]

    [IntegerField( "DefinedTypeId", "The id of the defined type for rooms.", true, 0, "", 0 )]
    [IntegerField( "MinistryDefinedTypeId", "The id of the defined type for ministries.", true, 0, "", 1)]
    [IntegerField( "ContentChannelId", "The id of the content channel for an event request.", true, 0, "", 2 )]
    [TextField( "Rock Base URL", "Base URL for Rock", true, "https://rock.thecrossingchurch.com/page/", "", 3 )]
    [IntegerField( "Page Id", "The id of the page for editing requests.", true, 0, "", 4 )]
    [IntegerField( "Admin Dashboard Page Id", "The id of the page for the admin dashboard.", true, 0, "", 5 )]
    [IntegerField( "Workflow Entry Page Id", "The id of the page for workflow entries.", true, 0, "", 6 )]
    [WorkflowTypeField( "User Action Workflow", "The workflow that allows users to accept proposed changes, use original, or cancel request", order: 7 )]
    [SecurityRoleField( "Room Request Admin", "The role for people handling the room only requests who need to be notified", true, order: 8 )]
    [SecurityRoleField( "Event Request Admin", "The role for people handling all other requests who need to be notified", true, order: 9 )]

    public partial class EventSubmissionUserDashboard : Rock.Web.UI.RockBlock
    {
        #region Variables
        public RockContext context { get; set; }
        private int DefinedTypeId { get; set; }
        private int MinistryDefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        public string BaseURL { get; set; }
        private int PageId { get; set; }
        private int AdminPageId { get; set; }
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
            BaseURL = GetAttributeValue( "RockBaseURL" );
            PageId = GetAttributeValue( "PageId" ).AsInteger();
            AdminPageId = GetAttributeValue( "AdminDashboardPageId" ).AsInteger();
            hfRequestURL.Value = "/page/" + PageId;
            int workflowEntryPageId = GetAttributeValue( "WorkflowEntryPageId" ).AsInteger();
            int workflowTypeId = new WorkflowTypeService( context ).Get( Guid.Parse( GetAttributeValue( "UserActionWorkflow" ) ) ).Id;
            hfWorkflowURL.Value = "/page/" + workflowEntryPageId + "?WorkflowTypeId=" + workflowTypeId;
            Rooms = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == DefinedTypeId ).ToList();
            Rooms.LoadAttributes();
            hfRooms.Value = JsonConvert.SerializeObject( Rooms.Select( dv => new { Id = dv.Id, Value = dv.Value, Type = dv.AttributeValues.FirstOrDefault( av => av.Key == "Type" ).Value.Value, Capacity = dv.AttributeValues.FirstOrDefault( av => av.Key == "Capacity" ).Value.Value.AsInteger(), IsActive = dv.IsActive } ) );
            Ministries = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == MinistryDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
            Ministries.LoadAttributes();
            hfMinistries.Value = JsonConvert.SerializeObject( Ministries.Select( dv => new { Id = dv.Id, Value = dv.Value, IsPersonal = dv.AttributeValues.FirstOrDefault( av => av.Key == "IsPersonalRequest" ).Value.Value.AsBoolean(), IsActive = dv.IsActive } ) );
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
            LoadMyRequests();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the recently submitted events
        /// </summary>
        protected void LoadMyRequests()
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            var items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId && i.CreatedByPersonAliasId == CurrentPersonAliasId ).ToList();
            items.LoadAttributes();
            var requests = items.Select( i => new { Id = i.Id, Value = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, HistoricData = i.AttributeValues.FirstOrDefault( av => av.Key == "NonTransferrableData" ).Value.Value, CreatedBy = i.CreatedByPersonName, Changes = i.AttributeValues.FirstOrDefault( av => av.Key == "ProposedChangesJSON" ).Value.Value, CreatedOn = i.CreatedDateTime, RequestStatus = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value, Comments = JsonConvert.DeserializeObject<List<Comment>>( i.AttributeValues.FirstOrDefault( av => av.Key == "Comments" ).Value.Value ) } );
            hfRequests.Value = JsonConvert.SerializeObject( requests );
        }
        protected void AddComment_Click( object sender, EventArgs e )
        {
            int? id = hfRequestID.Value.AsIntegerOrNull();
            if ( id.HasValue )
            {
                ContentChannelItem item = new ContentChannelItemService( context ).Get( id.Value );
                item.LoadAttributes();
                List<Comment> Comments = JsonConvert.DeserializeObject<List<Comment>>( item.AttributeValues["Comments"].Value );
                Comment newComment = new Comment();
                newComment.Message = hfComment.Value;
                newComment.CreatedBy = CurrentPerson.FullName;
                newComment.CreatedOn = RockDateTime.Now;
                Comments.Add( newComment );
                item.SetAttributeValue( "Comments", JsonConvert.SerializeObject( Comments ) );

                //Notify Admins
                SendCommentEmail( item, newComment );

                //Save CCI
                item.SaveAttributeValues( context );
                hfRequestID.Value = null;
                Page.Response.Redirect( Page.Request.Url.ToString(), true );
            }
        }

        private void SendCommentEmail( ContentChannelItem item, Comment comment )
        {
            string subject = CurrentPerson.FullName + " Has Added a Comment to " + item.Title;
            string message = "<p>" + CurrentPerson.FullName + " has added this comment to their request:</p>" +
                "<blockquote>" + comment.Message + "</blockquote><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + BaseURL + AdminPageId + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
            List<GroupMember> groupMembers = new List<GroupMember>();
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            if ( item.AttributeValues["IsPreApproved"].Value == "Yes" )
            {
                groupMembers = RoomOnlySR.Members.Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
            }
            else
            {
                groupMembers = EventSR.Members.Where( gm => gm.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
            }
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
            public List<string> TableType { get; set; }
            public int? NumTablesRound { get; set; }
            public int? NumTablesRect { get; set; }
            public int? NumChairsRound { get; set; }
            public int? NumChairsRect { get; set; }
            public bool? Checkin { get; set; }
            public string EventURL { get; set; }
            public string ZoomPassword { get; set; }
            public DateTime? RegistrationDate { get; set; }
            public DateTime? RegistrationEndDate { get; set; }
            public string RegistrationEndTime { get; set; }
            public List<string> FeeType { get; set; }
            public string FeeBudgetLine { get; set; }
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
        private class Comment
        {
            public string CreatedBy { get; set; }
            public DateTime? CreatedOn { get; set; }
            public string Message { get; set; }
        }
    }
}