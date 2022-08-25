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
using RockWeb.TheCrossing;
using EventRequest = RockWeb.TheCrossing.EventSubmissionHelper.EventRequest;
using Comment = RockWeb.TheCrossing.EventSubmissionHelper.Comment;

namespace RockWeb.Plugins.com_thecrossingchurch.EventSubmission
{
    /// <summary>
    /// Request form for Event Submissions
    /// </summary>
    [DisplayName( "Event Submission History" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "All Event Submissions" )]

    [DefinedTypeField( "Room List", "The defined type for the list of available rooms", true, "", "", 0 )]
    [DefinedTypeField( "Ministry List", "The defined type for the list of ministries", true, "", "", 1 )]
    [DefinedTypeField( "Budget Lines", "The defined type for the list of budget lines", true, "", "", 2 )]
    [ContentChannelField( "Content Channel", "The conent channel for event requests", true, "", "", 3 )]
    [LinkedPage( "Dashboard Page", "The Request Dashboard Page", true, "", "", 4 )]

    public partial class EventSubmissionHistory : Rock.Web.UI.RockBlock
    {
        #region Variables
        private RockContext context { get; set; }
        private EventSubmissionHelper eventSubmissionHelper { get; set; }
        private int DefinedTypeId { get; set; }
        private int MinistryDefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private int PageId { get; set; }
        private List<DefinedValue> Rooms { get; set; }
        private List<DefinedValue> Ministries { get; set; }
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
            Guid? DashboardPageGuid = GetAttributeValue( "DashboardPage" ).AsGuidOrNull();

            eventSubmissionHelper = new EventSubmissionHelper( RoomDefinedTypeGuid, MinistryDefinedTypeGuid, BudgetDefinedTypeGuid, ContentChannelGuid, null );
            hfRooms.Value = eventSubmissionHelper.RoomsJSON;
            hfDoors.Value = eventSubmissionHelper.DoorsJSON;
            hfMinistries.Value = eventSubmissionHelper.MinistriesJSON;
            hfBudgetLines.Value = eventSubmissionHelper.BudgetLinesJSON;
            ContentChannelId = eventSubmissionHelper.EventContentChannelId;
            if ( DashboardPageGuid.HasValue )
            {
                string pageId = new PageService( context ).Get( DashboardPageGuid.Value ).Id.ToString();
                hfDashboardURL.Value = "/page/" + pageId;
            }
            hfIsSuperUser.Value = "True";

            GetAllRequests();
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all requests
        /// </summary>
        protected void GetAllRequests()
        {
            ContentChannelItemService svc = new ContentChannelItemService( context );
            var items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).OrderByDescending( i => i.CreatedDateTime ).ToList();
            items.LoadAttributes();
            items = items.Where( i => i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value != "Draft" ).ToList();
            hfRequests.Value = JsonConvert.SerializeObject( items.Select( i => new { Id = i.Id, Value = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, HistoricData = i.AttributeValues.FirstOrDefault( av => av.Key == "NonTransferrableData" ).Value.Value, CreatedBy = i.CreatedByPersonName, CreatedOn = i.CreatedDateTime, RequestStatus = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value } ) );
        }

        #endregion
    }
}