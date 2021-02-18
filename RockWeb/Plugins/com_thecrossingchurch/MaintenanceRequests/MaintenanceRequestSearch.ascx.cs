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
    [DisplayName( "Maintenance Request Search" )]
    [Category( "com_thecrossingchurch > Maintenance Request" )]
    [Description( "Maintenance Request Advanced Search" )]

    [IntegerField( "DefinedTypeId", "The id of the defined type for rooms.", true, 0, "", 0 )]
    [IntegerField( "ContentChannelId", "The id of the content channel for an event request.", true, 0, "", 2 )]
    [IntegerField( "ContentChannelTypeId", "The id of the content channel type for an event request.", true, 0, "", 3 )]

    public partial class MaintenanceRequestSearch : Rock.Web.UI.RockBlock
    {
        #region Variables
        public RockContext context { get; set; }
        public ContentChannelItemService svc { get; set; }
        private int DefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private int ContentChannelTypeId { get; set; }
        private List<DefinedValue> Locations { get; set; }
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
            svc = new ContentChannelItemService( context );
            LoadRequests();
            Locations = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == DefinedTypeId ).ToList();
            hfLocations.Value = JsonConvert.SerializeObject( Locations.Select( dv => new { Id = dv.Id, Value = dv.Value } ) );
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Methods

        protected void LoadRequests()
        {
            var items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).ToList();
            items.LoadAttributes();
            var formattedItems = items.Select( i => new { Id = i.Id, Title = i.Title, CreatedBy = i.CreatedByPersonAlias.Person, CreatedOn = i.CreatedDateTime, Description = i.AttributeValues["Description"].Value, RequestedCompletionDate = i.AttributeValues["RequestedCompletionDate"].Value, Location = i.AttributeValues["Location"].Value, SafetyIssue = i.AttributeValues["SafetyIssue"].Value, RequestStatus = i.AttributeValues["RequestStatus"].Value, Image = i.AttributeValues["Image"].Value, Comments = i.AttributeValues["Comments"].Value } ).ToList();
            hfRequests.Value = JsonConvert.SerializeObject( formattedItems );
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