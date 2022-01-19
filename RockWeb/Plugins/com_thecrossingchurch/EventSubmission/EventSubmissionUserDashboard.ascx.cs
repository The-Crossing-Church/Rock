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
using RockWeb.TheCrossing;
using EventRequest = RockWeb.TheCrossing.EventSubmissionHelper.EventRequest;
using Comment = RockWeb.TheCrossing.EventSubmissionHelper.Comment;

namespace RockWeb.Plugins.com_thecrossingchurch.EventSubmission
{
    /// <summary>
    /// Request form for Event Submissions
    /// </summary>
    [DisplayName( "Event Submission User Dashboard" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "Dashboard for Current User's Event Submissions" )]

    [DefinedTypeField( "Room List", "The defined type for the list of available rooms", true, "", "", 0 )]
    [DefinedTypeField( "Ministry List", "The defined type for the list of ministries", true, "", "", 1 )]
    [DefinedTypeField( "Budget Lines", "The defined type for the list of budget lines", true, "", "", 2 )]
    [ContentChannelField( "Content Channel", "The conent channel for event requests", true, "", "", 3 )]
    [LinkedPage( "Request Page", "The Request Form Page", true, "", "", 4 )]
    [LinkedPage( "Admin Dashboard Page", "The Request Admin Dashboard Page", true, "", "", 5 )]
    [LinkedPage( "Workflow Entry Page", order: 6 )]
    [WorkflowTypeField( "User Action Workflow", "The workflow that allows users to accept proposed changes, use original, or cancel request", order: 7 )]
    [SecurityRoleField( "Room Request Admin", "The role for people handling the room only requests who need to be notified", true, order: 8 )]
    [SecurityRoleField( "Event Request Admin", "The role for people handling all other requests who need to be notified", true, order: 9 )]
    [SecurityRoleField( "Super User Role", "People who can make full requests", true, "", "", 10 )]

    public partial class EventSubmissionUserDashboard : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext context { get; set; }
        private EventSubmissionHelper eventSubmissionHelper { get; set; }
        private int RoomDefinedTypeId { get; set; }
        private int MinistryDefinedTypeId { get; set; }
        private int BudgetDefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private int ContentChannelTypeId { get; set; }
        public string BaseURL { get; set; }
        private string RequestPageId { get; set; }
        private Guid? RequestPageGuid { get; set; }
        private string AdminDashboardPageId { get; set; }
        private int UserActionWorkflowId { get; set; }
        private List<DefinedValue> Rooms { get; set; }
        private List<DefinedValue> Doors { get; set; }
        private List<DefinedValue> Ministries { get; set; }
        private List<DefinedValue> BudgetLines { get; set; }
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

            Guid? RoomDefinedTypeGuid = GetAttributeValue( "RoomList" ).AsGuidOrNull();
            Guid? MinistryDefinedTypeGuid = GetAttributeValue( "MinistryList" ).AsGuidOrNull();
            Guid? BudgetDefinedTypeGuid = GetAttributeValue( "BudgetLines" ).AsGuidOrNull();
            Guid? ContentChannelGuid = GetAttributeValue( "ContentChannel" ).AsGuidOrNull();

            eventSubmissionHelper = new EventSubmissionHelper( RoomDefinedTypeGuid, MinistryDefinedTypeGuid, BudgetDefinedTypeGuid, ContentChannelGuid );
            hfRooms.Value = eventSubmissionHelper.RoomsJSON;
            hfDoors.Value = eventSubmissionHelper.DoorsJSON;
            hfMinistries.Value = eventSubmissionHelper.MinistriesJSON;
            hfBudgetLines.Value = eventSubmissionHelper.BudgetLinesJSON;
            ContentChannelId = eventSubmissionHelper.ContentChannelId;
            ContentChannelTypeId = eventSubmissionHelper.ContentChannelTypeId;
            BaseURL = eventSubmissionHelper.BaseURL;

            RequestPageGuid = GetAttributeValue( "RequestPage" ).AsGuidOrNull();
            Guid? DashboardPageGuid = GetAttributeValue( "AdminDashboardPage" ).AsGuidOrNull();
            if ( RequestPageGuid.HasValue && DashboardPageGuid.HasValue )
            {
                RequestPageId = new PageService( context ).Get( RequestPageGuid.Value ).Id.ToString();
                AdminDashboardPageId = new PageService( context ).Get( DashboardPageGuid.Value ).Id.ToString();
                hfRequestURL.Value = "/page/" + RequestPageId;
            }

            Guid? userActionWF = GetAttributeValue( "UserActionWorkflow" ).AsGuidOrNull();
            Guid? WorkflowEntryPageGuid = GetAttributeValue( "WorkflowEntryPage" ).AsGuidOrNull();
            if ( userActionWF.HasValue && WorkflowEntryPageGuid.HasValue )
            {
                UserActionWorkflowId = new WorkflowTypeService( context ).Get( userActionWF.Value ).Id;
                if ( WorkflowEntryPageGuid.HasValue )
                {
                    int workflowEntryPageId = new PageService( context ).Get( WorkflowEntryPageGuid.Value ).Id;
                    int workflowTypeId = new WorkflowTypeService( context ).Get( Guid.Parse( GetAttributeValue( "UserActionWorkflow" ) ) ).Id;
                    hfWorkflowURL.Value = "/page/" + workflowEntryPageId + "?WorkflowTypeId=" + workflowTypeId;
                }
            }


            Guid? superUserGuid = GetAttributeValue( "SuperUserRole" ).AsGuidOrNull();
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
            if ( superUserGuid.HasValue )
            {
                Rock.Model.Group superUsers = new GroupService( context ).Get( superUserGuid.Value );
                if ( CurrentPersonId.HasValue && superUsers.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).Select( m => m.PersonId ).ToList().Contains( CurrentPersonId.Value ) )
                {
                    hfIsSuperUser.Value = "True";
                }
            }

            //Throw an error if not all values are present
            if ( !RoomDefinedTypeGuid.HasValue || !MinistryDefinedTypeGuid.HasValue || !BudgetDefinedTypeGuid.HasValue || !ContentChannelGuid.HasValue || String.IsNullOrEmpty( BaseURL ) || !RequestPageGuid.HasValue || !DashboardPageGuid.HasValue || !userActionWF.HasValue || !WorkflowEntryPageGuid.HasValue )
            {
                return;
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
        protected void ResubmitRequest_Click( object sender, EventArgs e )
        {
            EventRequest request = JsonConvert.DeserializeObject<EventRequest>( hfRequest.Value );
            ContentChannelItem item = new ContentChannelItem();
            item.ContentChannelTypeId = ContentChannelTypeId;
            item.ContentChannelId = ContentChannelId;
            item.LoadAttributes();
            item.CreatedByPersonAliasId = CurrentPersonAliasId;
            item.CreatedDateTime = RockDateTime.Now;
            item.StartDateTime = RockDateTime.Now;
            item.Title = request.Name;
            item.SetAttributeValue( "RequestStatus", "Draft" );
            item.SetAttributeValue( "EventDates", String.Join( ", ", request.EventDates ) );
            item.SetAttributeValue( "RequestJSON", hfRequest.Value );
            item.SetAttributeValue( "RequestType", eventSubmissionHelper.GetRequestResources( request ) );
            item.SetAttributeValue( "IsPreApproved", "No" );

            context.ContentChannelItems.AddOrUpdate( item );
            context.SaveChanges();
            item.SaveAttributeValues( context );
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "Id", item.Id.ToString() );
            NavigateToPage( RequestPageGuid.Value, query );
        }

        private void SendCommentEmail( ContentChannelItem item, Comment comment )
        {
            string subject = CurrentPerson.FullName + " Has Added a Comment to " + item.Title;
            string message = "<p>" + CurrentPerson.FullName + " has added this comment to their request:</p>" +
                "<blockquote>" + comment.Message + "</blockquote><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + BaseURL + "page/" + AdminDashboardPageId + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
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
        private class Comment
        {
            public string CreatedBy { get; set; }
            public DateTime? CreatedOn { get; set; }
            public string Message { get; set; }
        }
    }
}