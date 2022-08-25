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
using PartialApprovalChange = RockWeb.TheCrossing.EventSubmissionHelper.PartialApprovalChange;
using Comment = RockWeb.TheCrossing.EventSubmissionHelper.Comment;

namespace RockWeb.Plugins.com_thecrossingchurch.EventSubmission
{
    /// <summary>
    /// Request form for Event Submissions
    /// </summary>
    [DisplayName( "Event Submission Dashboard" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "Dashboard for Event Submissions" )]

    [DefinedTypeField( "Room List", "The defined type for the list of available rooms", true, "", "", 0 )]
    [DefinedTypeField( "Ministry List", "The defined type for the list of ministries", true, "", "", 1 )]
    [DefinedTypeField( "Budget Lines", "The defined type for the list of budget lines", true, "", "", 2 )]
    [ContentChannelField( "Content Channel", "The conent channel for event requests", true, "", "", 3 )]
    [LinkedPage( "Request Page", "The Request Form Page", true, "", "", 4 )]
    [LinkedPage( "User Dashboard Page", "The Request User Dashboard Page", true, "", "", 5 )]
    [LinkedPage( "History Page", "The Request History Page", true, "", "", 6 )]
    [WorkflowTypeField( "Request Workflow", "Workflow to launch when request is approved or denied to send email", order: 7 )]
    [WorkflowTypeField( "User Action Workflow", "Workflow to launch when change request is denied", order: 8 )]
    [LinkedPage( "Workflow Entry Page", order: 9 )]
    [DateField( "Filter Date", "Don't show requests created before this date", required: false, order: 10 )]
    [TextField( "Denied ChangesURL", "URL for the User Action Workflow Entry", order: 10 )]

    public partial class EventSubmissionDashboard : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext context { get; set; }
        private EventSubmissionHelper eventSubmissionHelper { get; set; }
        private int ContentChannelId { get; set; }
        private int RequestWorkflowId { get; set; }
        private int UserActionWorkflowId { get; set; }
        public string BaseURL { get; set; }
        private string RequestPageId { get; set; }
        private string UserDashboardPageId { get; set; }
        private string HistoryPageId { get; set; }
        private DateTime? FilterDate { get; set; }
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

            eventSubmissionHelper = new EventSubmissionHelper( RoomDefinedTypeGuid, MinistryDefinedTypeGuid, BudgetDefinedTypeGuid, ContentChannelGuid, null );
            hfRooms.Value = eventSubmissionHelper.RoomsJSON;
            hfDoors.Value = eventSubmissionHelper.DoorsJSON;
            hfMinistries.Value = eventSubmissionHelper.MinistriesJSON;
            hfBudgetLines.Value = eventSubmissionHelper.BudgetLinesJSON;
            ContentChannelId = eventSubmissionHelper.EventContentChannelId;
            BaseURL = eventSubmissionHelper.BaseURL;

            Guid? RequestPageGuid = GetAttributeValue( "RequestPage" ).AsGuidOrNull();
            Guid? DashboardPageGuid = GetAttributeValue( "UserDashboardPage" ).AsGuidOrNull();
            Guid? HistoryPageGuid = GetAttributeValue( "HistoryPage" ).AsGuidOrNull();
            if ( RequestPageGuid.HasValue && DashboardPageGuid.HasValue && HistoryPageGuid.HasValue )
            {
                RequestPageId = new PageService( context ).Get( RequestPageGuid.Value ).Id.ToString();
                UserDashboardPageId = new PageService( context ).Get( DashboardPageGuid.Value ).Id.ToString();
                HistoryPageId = new PageService( context ).Get( HistoryPageGuid.Value ).Id.ToString();
                hfRequestURL.Value = "/page/" + RequestPageId;
                hfHistoryURL.Value = "/page/" + HistoryPageId;
            }

            Guid? requestWF = GetAttributeValue( "RequestWorkflow" ).AsGuidOrNull();
            if ( requestWF.HasValue )
            {
                RequestWorkflowId = new WorkflowTypeService( context ).Get( requestWF.Value ).Id;
            }
            Guid? userActionWF = GetAttributeValue( "UserActionWorkflow" ).AsGuidOrNull();
            if ( userActionWF.HasValue )
            {
                UserActionWorkflowId = new WorkflowTypeService( context ).Get( userActionWF.Value ).Id;
            }

            if ( !String.IsNullOrEmpty( GetAttributeValue( "FilterDate" ) ) )
            {
                FilterDate = GetAttributeValue( "FilterDate" ).AsDateTime();
            }

            //Admins are always super users 
            hfIsSuperUser.Value = "True";

            //Throw an error if not all values are present
            if ( !RoomDefinedTypeGuid.HasValue || !MinistryDefinedTypeGuid.HasValue || !BudgetDefinedTypeGuid.HasValue || !ContentChannelGuid.HasValue || String.IsNullOrEmpty( BaseURL ) || !RequestPageGuid.HasValue || !DashboardPageGuid.HasValue || !HistoryPageGuid.HasValue || !requestWF.HasValue || !userActionWF.HasValue )
            {
                return;
            }

            GetRecentRequests();
            GetThisWeeksEvents();
            LoadUpcoming();
        }

        #endregion

        #region Events

        /// <summary>
        /// Partial Approval
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PartialApproval_Click( object sender, EventArgs e )
        {
            int? id = hfRequestID.Value.AsIntegerOrNull();
            if ( id.HasValue )
            {
                string raw = hfUpdatedItem.Value;
                EventRequest request = JsonConvert.DeserializeObject<EventRequest>( raw );
                ContentChannelItem item = new ContentChannelItemService( context ).Get( id.Value );
                item.LoadAttributes();
                item.SetAttributeValue( "RequestStatus", "Approved" );
                item.SetAttributeValue( "RequestJSON", raw );
                item.SetAttributeValue( "ProposedChangesJSON", "" );

                //Save CCI
                item.SaveAttributeValues( context );
                hfRequestID.Value = null;
                GetRecentRequests();
                GetThisWeeksEvents();

                //Send Changes Email
                string rawChanges = hfChanges.Value;
                List<PartialApprovalChange> changes = JsonConvert.DeserializeObject<List<PartialApprovalChange>>( hfChanges.Value ).OrderBy( c => c.isApproved ).ToList();
                string message = "Hello " + item.CreatedByPersonAlias.Person.NickName + ",<br/>";
                message += "Please see below which modifications have been approved or denied:<br/>";
                message += "<strong>Approved Modifications</strong><br/>";
                message += "<ul>";
                List<PartialApprovalChange> approved = changes.Where( c => c.isApproved ).ToList();
                for ( int i = 0; i < approved.Count(); i++ )
                {
                    message += "<li>" + approved[i].label + "</li>";
                }
                message += "</ul>";
                message += "<strong>Denied Modifications</strong><br/>";
                List<PartialApprovalChange> denied = changes.Where( c => !c.isApproved ).ToList();
                message += "<ul>";
                for ( int i = 0; i < denied.Count(); i++ )
                {
                    message += "<li>" + denied[i].label + "</li>";
                }
                message += "</ul>";
                message +=
                    "<table style='width: 100%;'>" +
                        "<tr>" +
                            "<td></td>" +
                            "<td style='text-align:center;'>" +
                                "<a href='" + BaseURL + "page/" + UserDashboardPageId + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>View Updated Event</a>" +
                            "</td>" +
                            "<td style='text-align:center;'>" +
                                "<a href='" + BaseURL + "page/" + RequestPageId + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Continue Modifying</a>" +
                            "</td>" +
                            "<td></td>" +
                        "</tr>" +
                    "</table>";

                var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
                var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
                message = header + message + footer;
                RockEmailMessage email = new RockEmailMessage();
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( item.CreatedByPersonAlias.Person, new Dictionary<string, object>() );
                email.AddRecipient( recipient );
                email.Subject = "Some of Your Changes Have Been Approved";
                email.Message = message;
                email.FromEmail = "system@thecrossingchurch.com";
                email.FromName = "The Crossing System";
                email.CreateCommunicationRecord = true;
                var output = email.Send();
            }
        }

        /// <summary>
        /// Change Status
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
                bool emailDenyOptions = false;
                switch ( action )
                {
                    case "Deny":
                        item.SetAttributeValue( "RequestStatus", "Denied" );
                        break;
                    case "Cancel":
                        item.SetAttributeValue( "RequestStatus", "Cancelled" );
                        break;
                    case "DenyUserComments":
                        //Need to email user alternative
                        emailDenyOptions = true;
                        item.SetAttributeValue( "RequestStatus", "Proposed Changes Denied" );
                        break;
                    case "DenyUser":
                        //This option will send a generic denied changes email allowing them to revert or cancel 
                        item.SetAttributeValue( "RequestStatus", "Proposed Changes Denied" );
                        break;
                    case "InProgress":
                        //New option meaning Andrew has looked at it but they need to make changes
                        item.SetAttributeValue( "RequestStatus", "In Progress" );
                        break;
                    default:
                        item.SetAttributeValue( "RequestStatus", "Approved" );
                        string raw = hfUpdatedItem.Value;
                        item.SetAttributeValue( "RequestJSON", raw );
                        item.SetAttributeValue( "ProposedChangesJSON", "" );
                        break;
                }
                item.SaveAttributeValues( context );
                hfRequestID.Value = null;
                if ( action == "Approved" || action == "Deny" || emailDenyOptions )
                {
                    Dictionary<string, string> query = new Dictionary<string, string>();
                    query.Add( "WorkflowTypeId", RequestWorkflowId.ToString() );
                    query.Add( "ItemId", item.Id.ToString() );
                    NavigateToLinkedPage( "WorkflowEntryPage", query );
                }
                if ( action == "DenyUser" )
                {
                    //Send Generic Email
                    SendDeniedChangesEmail( item );
                }
                GetRecentRequests();
                GetThisWeeksEvents();
            }
        }

        /// <summary>
        /// Add Buffer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void AddBuffer_Click( object sender, EventArgs e )
        {
            //getEvents();
            int? id = hfRequestID.Value.AsIntegerOrNull();
            if ( id.HasValue )
            {
                ContentChannelItem item = new ContentChannelItemService( context ).Get( id.Value );
                item.LoadAttributes();
                //Update Buffer
                item.SetAttributeValue( "RequestJSON", hfUpdatedItem.Value );

                //Save CCI
                item.SaveAttributeValues( context );
                hfRequestID.Value = null;
                GetRecentRequests();
                GetThisWeeksEvents();
            }
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

                //Notify User
                SendCommentEmail( item, newComment );

                //Save CCI
                item.SaveAttributeValues( context );
                hfRequestID.Value = null;
                string url = Page.Request.Url.ToString().Split( '?' )[0] + "?Id=" + item.Id.ToString();
                Page.Response.Redirect( url, true );
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
            var items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId && ( !FilterDate.HasValue || DateTime.Compare( i.CreatedDateTime.Value, FilterDate.Value ) >= 0 ) ).ToList();
            var itm = items[0];
            itm.LoadAttributes();
            var attrId = itm.Attributes["RequestStatus"].Id;
            var statuses = new AttributeValueService( context ).Queryable().Where( av => av.AttributeId == attrId && av.Value != "Draft" && av.Value != "Approved" && !av.Value.Contains( "Cancelled" ) && av.Value != "Denied" );
            var statusItems = items.Join( statuses,
                    i => i.Id,
                    s => s.EntityId,
                    ( i, s ) => i
                ).ToList();
            items = items.Where( i => DateTime.Compare( i.CreatedDateTime.Value, oneweekago ) >= 0 ).ToList();
            items.AddRange( statusItems );
            items = items.Distinct().ToList();
            items.LoadAttributes();
            items = items.Where( i => i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value != "Draft" ).ToList();
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                var item = svc.Get( Int32.Parse( PageParameter( PageParameterKey.Id ) ) );
                if ( items.IndexOf( item ) < 0 )
                {
                    item.LoadAttributes();
                    items.Add( item );
                }
            }
            var requests = items.OrderByDescending( i => i.ModifiedDateTime ).Select( i => new { Id = i.Id, Value = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, HistoricData = i.AttributeValues.FirstOrDefault( av => av.Key == "NonTransferrableData" ).Value.Value, CreatedBy = i.CreatedByPersonName, Changes = i.AttributeValues.FirstOrDefault( av => av.Key == "ProposedChangesJSON" ).Value.Value, CreatedOn = i.CreatedDateTime, SubmittedOn = i.StartDateTime, RequestStatus = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value, Comments = JsonConvert.DeserializeObject<List<Comment>>( i.AttributeValues.FirstOrDefault( av => av.Key == "Comments" ).Value.Value ) } );
            hfRequests.Value = JsonConvert.SerializeObject( requests );
        }

        /// <summary>
        /// Load upcoming requests
        /// </summary>
        protected void LoadUpcoming()
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            List<ContentChannelItem> items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).ToList();
            var itm = items[0];
            itm.LoadAttributes();
            var attrId = itm.Attributes["EventDates"].Id;
            var eventDates = new AttributeValueService( context ).Queryable().Where( av => av.AttributeId == attrId ).ToList();
            var upcomingDates = eventDates.Where( r =>
            {
                var dates = r.Value.Split( ',' );
                foreach ( var d in dates )
                {
                    DateTime dt = DateTime.Parse( d );
                    if ( DateTime.Compare( dt, DateTime.Now ) >= 1 )
                    {
                        return true;
                    }
                }
                return false;
            } );
            items = items.Join( upcomingDates,
                    i => i.Id,
                    e => e.EntityId,
                    ( i, e ) => i
                ).ToList();

            items.LoadAttributes();
            items = items.Where( i => !( i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value == "Draft" || i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value == "Submitted" || i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value.Contains( "Cancelled" ) || i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value == "Denied" ) ).ToList();
            hfUpcomingRequests.Value = JsonConvert.SerializeObject( items.Select( i => new { Id = i.Id, Value = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, HistoricData = i.AttributeValues.FirstOrDefault( av => av.Key == "NonTransferrableData" ).Value.Value, CreatedBy = i.CreatedByPersonName, CreatedOn = i.CreatedDateTime, SubmittedOn = i.StartDateTime, RequestStatus = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value } ).ToList() );
        }

        /// <summary>
        /// Get events for the next 7 days
        /// </summary>
        protected void GetThisWeeksEvents()
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            var items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).ToList();
            var itm = items[0];
            itm.LoadAttributes();
            var attrId = itm.Attributes["EventDates"].Id;
            var eventDates = new AttributeValueService( context ).Queryable().Where( av => av.AttributeId == attrId ).ToList();
            var currentEvents = eventDates.Where( r =>
            {
                var occursInWeek = false;
                var endOfWeek = DateTime.Now.AddDays( 6 );
                endOfWeek = new DateTime( endOfWeek.Year, endOfWeek.Month, endOfWeek.Day, 23, 59, 59 );
                var today = DateTime.Now;
                today = new DateTime( today.Year, today.Month, today.Day, 0, 0, 0 );
                var dates = r.Value.Split( ',' ).ToList();
                for ( var i = 0; i < dates.Count(); i++ )
                {
                    if ( DateTime.Compare( DateTime.Parse( dates[i] ), endOfWeek ) <= 0 && DateTime.Compare( DateTime.Parse( dates[i] ), today ) >= 0 )
                    {
                        occursInWeek = true;
                    }
                }
                return occursInWeek;
            } );
            items = items.Join( currentEvents,
                    i => i.Id,
                    e => e.EntityId,
                    ( i, e ) => i
                ).ToList();

            items.LoadAttributes();
            var reqs = items.Select( i => new FullRequest() { Id = i.Id, Value = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, Request = JsonConvert.DeserializeObject<EventRequest>( i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value ), HistoricData = i.AttributeValues.FirstOrDefault( av => av.Key == "NonTransferrableData" ).Value.Value, CreatedBy = i.CreatedByPersonName, CreatedOn = i.CreatedDateTime, RequestStatus = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value } );
            var current = reqs.Where( r => r.RequestStatus == "Approved" && r.Request != null );
            hfCurrent.Value = JsonConvert.SerializeObject( current );
        }

        protected void SendDeniedChangesEmail( ContentChannelItem item )
        {
            string url = BaseURL + "WorkflowEntry/" + UserActionWorkflowId + "?ItemId=";
            string subject = "Proposed Changes for " + item.Title + " have been Denied";
            string message = "Hello " + item.CreatedByPersonName + ",<br/>" +
                "<p>We regret to inform you the changes you have requested to your event request have been denied.</p> <br/>" +
                "<p>Please select one of the following options for your request. You can...</p>" +
                "<ul>" +
                    "<li> Continue with the originally approved request </li>" +
                    "<li> Cancel your request entirely </li>" +
                "</ul>" +
                "<table>" +
                    "<tr>" +
                        "<td style='tect-align: center;'>" +
                            "<a href='" + url + item.Id + "&Action=Original' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Use Original</a>" +
                        "</td>" +
                        "<td style='tect-align: center;'>" +
                            "<a href='" + url + item.Id + "&Action=Cancelled' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Cancel Request </a>" +
                        "</td>" +
                    "</tr>" +
                "</table>";
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( item.CreatedByPersonAlias.Person, new Dictionary<string, object>() );
            email.AddRecipient( recipient );
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            email.CreateCommunicationRecord = true;
            var output = email.Send();
        }

        private void SendCommentEmail( ContentChannelItem item, Comment comment )
        {
            string subject = CurrentPerson.FullName + " Has Added a Comment to " + item.Title;
            string message = "Hello " + item.CreatedByPersonName + ",<br/>" +
                "<p>This comment has been added to your request:</p>" +
                "<blockquote>" + comment.Message + "</blockquote><br/>" +
                "<p style='width: 100%; text-align: center;'><a href = '" + BaseURL + "page/" + UserDashboardPageId + "?Id=" + item.Id + "' style = 'background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;' > Open Request </a></p>";
            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( item.CreatedByPersonAlias.Person, new Dictionary<string, object>() );
            email.AddRecipient( recipient );
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
            public string HistoricData { get; set; }
            public EventRequest Request { get; set; }
            public string CreatedBy { get; set; }
            public DateTime? CreatedOn { get; set; }
            public string RequestStatus { get; set; }
        }
    }
}