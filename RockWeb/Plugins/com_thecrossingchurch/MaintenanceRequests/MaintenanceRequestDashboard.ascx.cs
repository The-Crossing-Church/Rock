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

namespace RockWeb.Plugins.com_thecrossingchurch.MaintenanceRequests
{
    /// <summary>
    /// Maintenance Request Forms
    /// </summary>
    [DisplayName( "Maintenance Request Dashboard" )]
    [Category( "com_thecrossingchurch > Maintenance Request" )]
    [Description( "Maintenance Request Dashboard" )]

    [IntegerField( "DefinedTypeId", "The id of the defined type for rooms.", true, 0, "", 0 )]
    [IntegerField( "ContentChannelId", "The id of the content channel for an event request.", true, 0, "", 2 )]
    [IntegerField( "ContentChannelTypeId", "The id of the content channel type for an event request.", true, 0, "", 3 )]
    [TextField( "Page Guid", "The guid of the page for redirect on save.", true, "", "", 4 )]
    [TextField( "Rock Base URL", "Base URL for Rock", true, "https://rock.thecrossingchurch.com", "", 5 )]
    [TextField( "Dashboard Page Id", "The id of the dashboard page.", true, "", "", 6 )]
    [SecurityRoleField( "Maintenance Admin", "The role for people handling the maintenance requests", true )]

    public partial class MaintenanceRequestDashboard : Rock.Web.UI.RockBlock
    {
        #region Variables
        public RockContext context { get; set; }
        public ContentChannelItemService svc { get; set; }
        public string BaseURL { get; set; }
        public string DashboardPageId { get; set; }
        private int DefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private int ContentChannelTypeId { get; set; }
        private List<DefinedValue> Locations { get; set; }
        private Group MaintenanceAdmin { get; set; }
        private bool isAdmin { get; set; }
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
            BaseURL = GetAttributeValue( "RockBaseURL" );
            DashboardPageId = GetAttributeValue( "DashboardPageId" );
            svc = new ContentChannelItemService( context );
            var MaintenanceRoleGuid = GetAttributeValue( "MaintenanceAdmin" ).AsGuidOrNull();
            if ( MaintenanceRoleGuid.HasValue )
            {
                MaintenanceAdmin = new GroupService( context ).Get( MaintenanceRoleGuid.Value );
                var inGroup = MaintenanceAdmin.Members.Where( gm => gm.PersonId == CurrentPersonId );
                if ( inGroup.Count() > 0 )
                {
                    isAdmin = true;
                }
                else
                {
                    isAdmin = false;
                }
                hfIsAdmin.Value = isAdmin.ToString();
                LoadRequests();
            }
            Locations = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == DefinedTypeId ).ToList();
            hfLocations.Value = JsonConvert.SerializeObject( Locations.Select( dv => new { Id = dv.Id, Value = dv.Value } ) );
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Events

        protected void btnAddComment_Click( object sender, EventArgs e )
        {
            int id = hfRequestId.ValueAsInt();
            if ( id > 0 )
            {
                ContentChannelItem item = svc.Get( id );
                item.LoadAttributes();
                Comment comm = new Comment();
                comm.Message = hfNewComment.Value;
                comm.CreatedBy = CurrentPerson.FullName;
                comm.CreatedOn = RockDateTime.Now;
                List<Comment> comments = new List<Comment>();
                if ( !String.IsNullOrEmpty( item.AttributeValues["Comments"].Value ) )
                {
                    comments = JsonConvert.DeserializeObject<List<Comment>>( item.AttributeValues["Comments"].Value );
                }
                comments.Add( comm );
                item.SetAttributeValue( "Comments", JsonConvert.SerializeObject( comments ) );
                item.SaveAttributeValues( context );
                string sendNotifications = item.AttributeValues["SendNotifications"].Value;
                if ( !isAdmin || sendNotifications == "True" )
                {
                    SendCommentNotification( comm, item.Title, item.CreatedByPersonAlias );
                }
                Dictionary<string, string> query = new Dictionary<string, string>();
                query.Add( "CommentAdded", "true" );
                NavigateToPage( Guid.Parse( GetAttributeValue( "PageGuid" ) ), query );
            }
        }

        protected void btnChangeStatus_Click( object sender, EventArgs e )
        {
            int id = hfRequestId.ValueAsInt();
            if ( id > 0 )
            {
                string status = hfNewStatus.Value;
                if ( !String.IsNullOrEmpty( status ) )
                {
                    ContentChannelItem item = svc.Get( id );
                    item.LoadAttributes();
                    item.SetAttributeValue( "RequestStatus", status );
                    item.SaveAttributeValues( context );
                    string sendNotifications = item.AttributeValues["SendNotifications"].Value;
                    if ( status == "Complete" && sendNotifications == "True" )
                    {
                        SendCompleteNotification( item.CreatedByPersonAlias, item.Title );
                    }
                    Dictionary<string, string> query = new Dictionary<string, string>();
                    query.Add( "StatusChanged", "true" );
                    NavigateToPage( Guid.Parse( GetAttributeValue( "PageGuid" ) ), query );
                }
            }

        }

        #endregion

        #region Methods

        protected void LoadRequests()
        {
            var items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).ToList();
            //If someone is not an admin, only return their requests
            if ( !isAdmin )
            {
                items = items.Where( i => i.CreatedByPersonAliasId == CurrentPersonAliasId ).ToList();
            }
            items.LoadAttributes();
            var formattedItems = items.Select( i => new { Id = i.Id, Title = i.Title, CreatedBy = i.CreatedByPersonAlias.Person, CreatedOn = i.CreatedDateTime, Description = i.AttributeValues["Description"].Value, RequestedCompletionDate = i.AttributeValues["RequestedCompletionDate"].Value, Location = i.AttributeValues["Location"].Value, SafetyIssue = i.AttributeValues["SafetyIssue"].Value, RequestStatus = i.AttributeValues["RequestStatus"].Value, Image = i.AttributeValues["Image"].Value, Comments = i.AttributeValues["Comments"].Value } ).Where( i => i.RequestStatus != "Complete" ).ToList();
            hfRequests.Value = JsonConvert.SerializeObject( formattedItems );
        }

        protected void SendCompleteNotification( PersonAlias person, string title )
        {
            string message = CurrentPerson.FullName + " has completed " + title + " with this note.<br/>";
            string subject = title + " is Complete";
            message += "<blockquote>" + hfNewComment.Value + "</blockquote><br/><br/>";

            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( person.Person, new Dictionary<string, object>() );
            email.AddRecipient( recipient );
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            var output = email.Send();
        }

        protected void SendCommentNotification( Comment comm, string title, PersonAlias person )
        {
            string message = comm.CreatedBy + " has added a Comment to " + title + ".<br/>";
            string subject = "New Comment on " + title;
            message += "<blockquote>" + comm.Message + "</blockquote>";
            message += "<br/>" +
                        "<p style='text-align:center; width: 100%;'>" +
                            "<a href='" + BaseURL + DashboardPageId + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>View Request</a>" +
                        "</p>";

            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            RockEmailMessage email = new RockEmailMessage();
            //If person who created the request adds a comment
            if ( CurrentPersonAliasId == person.Id )
            {
                List<GroupMember> admins = MaintenanceAdmin.Members.ToList();
                for ( var i = 0; i < admins.Count(); i++ )
                {
                    RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( admins[i].Person, new Dictionary<string, object>() );
                    email.AddRecipient( recipient );
                }
            }
            else
            {
                //Admin added a comment
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( person.Person, new Dictionary<string, object>() );
                email.AddRecipient( recipient );
            }
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            var output = email.Send();
        }

        #endregion

        protected class Request
        {
            public string Description { get; set; }
            public DateTime? RequestedCompletionDate { get; set; }
            public int Location { get; set; }
            public bool SafetyIssue { get; set; }
            public string Image { get; set; }
            public List<Comment> Comments { get; set; }
        }

        protected class Comment
        {
            public string CreatedBy { get; set; }
            public DateTime? CreatedOn { get; set; }
            public string Message { get; set; }
        }
    }
}