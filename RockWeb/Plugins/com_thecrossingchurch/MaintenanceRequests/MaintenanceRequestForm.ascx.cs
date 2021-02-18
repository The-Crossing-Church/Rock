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
    [DisplayName( "Maintenance Request Form" )]
    [Category( "com_thecrossingchurch > Maintenance Request" )]
    [Description( "Maintenance Request Form" )]

    [IntegerField( "DefinedTypeId", "The id of the defined type for rooms.", true, 0, "", 0 )]
    [IntegerField( "ContentChannelId", "The id of the content channel for an event request.", true, 0, "", 2 )]
    [IntegerField( "ContentChannelTypeId", "The id of the content channel type for an event request.", true, 0, "", 3 )]
    [TextField( "Page Guid", "The guid of the page for redirect on save.", true, "", "", 4 )]
    [TextField( "Rock Base URL", "Base URL for Rock", true, "https://rock.thecrossingchurch.com", "", 5 )]
    [TextField( "Dashboard Page Id", "The id of the dashboard page.", true, "", "", 6 )]
    [SecurityRoleField( "Maintenance Admin", "The role for people handling the maintenance requests", true )]

    public partial class MaintenanceRequestForm : Rock.Web.UI.RockBlock
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
            }
            Locations = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == DefinedTypeId ).ToList();
            Locations.LoadAttributes();
            hfLocations.Value = JsonConvert.SerializeObject( Locations.Select( dv => new { Id = dv.Id, Value = dv.Value, Type = dv.AttributeValues["Type"].Value } ) );
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Events

        protected void btnSubmit_Click( object sender, EventArgs e )
        {
            Request request = JsonConvert.DeserializeObject<Request>( hfRequest.Value );
            ContentChannelItem item = new ContentChannelItem();
            item.ContentChannelTypeId = ContentChannelTypeId;
            item.ContentChannelId = ContentChannelId;
            item.Title = Locations.FirstOrDefault( l => l.Id == request.Location ).AttributeValues["Type"] + " - " + Locations.FirstOrDefault( l => l.Id == request.Location ).Value;
            item.LoadAttributes();
            item.SetAttributeValue( "Description", request.Description );
            item.SetAttributeValue( "RequestedCompletionDate", request.RequestedCompletionDate );
            item.SetAttributeValue( "Location", request.Location );
            item.SetAttributeValue( "SafetyIssue", request.SafetyIssue.ToString() );
            item.SetAttributeValue( "RequestStatus", "Submitted" );
            item.SetAttributeValue( "Image", request.Image );
            item.SetAttributeValue( "SendNotifications", request.SendNotifications.ToString() );
            item.CreatedByPersonAliasId = CurrentPersonAliasId;
            item.CreatedDateTime = RockDateTime.Now;
            context.ContentChannelItems.AddOrUpdate( item );
            context.SaveChanges();
            item.SaveAttributeValues( context );
            NotifyGroup( request );
            if ( request.SendNotifications )
            {
                ConfirmationMessage( request );
            }
            Dictionary<string, string> query = new Dictionary<string, string>();
            query.Add( "ShowSuccess", "true" );
            NavigateToPage( Guid.Parse( GetAttributeValue( "PageGuid" ) ), query );
        }


        #endregion

        #region Methods

        protected void NotifyGroup( Request request )
        {
            List<GroupMember> admins = MaintenanceAdmin.Members.ToList();
            string message = GenerateMessageBody( request, false );
            string subject = "New Maintenance Request Submitted";
            RockEmailMessage email = new RockEmailMessage();
            for ( var i = 0; i < admins.Count(); i++ )
            {
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( admins[i].Person, new Dictionary<string, object>() );
                email.AddRecipient( recipient );
            }
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            var output = email.Send();
        }

        private void ConfirmationMessage( Request request )
        {
            string message = GenerateMessageBody( request, true );
            string subject = "Your Maintenance Request Has Been Submitted";
            RockEmailMessage email = new RockEmailMessage();
            RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( CurrentPerson, new Dictionary<string, object>() );
            email.AddRecipient( recipient );
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            var output = email.Send();
        }

        private string GenerateMessageBody( Request request, bool isConfirmation )
        {
            string message = "";
            if ( isConfirmation )
            {
                message = CurrentPerson.NickName + ", your Maintenance Request has been submitted. Here are the details of your submitted request.<br/>";
            }
            else
            {
                message = CurrentPerson.FullName + " has submitted a new Maintenance Request.<br/>";
            }
            message += "<strong>Description:</strong> " + request.Description + "<br/>";
            if ( request.RequestedCompletionDate.HasValue )
            {
                message += "<strong>Requested Completion Date:</strong> " + request.RequestedCompletionDate.Value.ToString( "MM/dd/yyyy" ) + "<br/>";
            }
            message += "<strong>Location:</strong> " + Locations.FirstOrDefault( l => l.Id == request.Location ).Value + "<br/>";
            message += "<strong>Is Safety Issue:</strong> " + ( request.SafetyIssue ? "Yes" : "No" ) + "<br/>";

            message += "<br/>" +
                        "<p style='text-align:center; width: 100%;'>" +
                            "<a href='" + BaseURL + DashboardPageId + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>View Request</a>" +
                        "</p>";

            var header = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
            message = header + message + footer;
            return message;
        }

        #endregion

        protected class Request
        {
            public string Description { get; set; }
            public DateTime? RequestedCompletionDate { get; set; }
            public int Location { get; set; }
            public bool SafetyIssue { get; set; }
            public string Image { get; set; }
            public bool SendNotifications { get; set; }
            public List<Comment> Comments { get; set; }
        }

        protected class Comment
        {
            public string CreatedBy { get; set; }
            public string CreatedOn { get; set; }
            public string Message { get; set; }
        }
    }
}