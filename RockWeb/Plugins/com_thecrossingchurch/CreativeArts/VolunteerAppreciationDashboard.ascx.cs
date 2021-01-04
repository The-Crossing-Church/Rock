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

namespace RockWeb.Plugins.com_thecrossingchurch.CreativeArts
{
    /// <summary>
    /// Dashboard for Volunteer Appreciation surveys
    /// </summary>
    [DisplayName( "Volunteer Appreciation Dashboard" )]
    [Category( "com_thecrossingchurch > Creative Arts" )]
    [Description( "Dashboard for Volunteer Appreciation Survey" )]

    [IntegerField( "Workflow Type Id", "The id of the workflow for the survey.", true, 0, "", 0 )]

    public partial class VolunteerAppreciationDashboard : Rock.Web.UI.RockBlock
    {
        #region Variables
        public RockContext context { get; set; }
        private int WorkflowTypeId { get; set; }
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
            WorkflowTypeId = GetAttributeValue( "WorkflowTypeId" ).AsInteger();
            List<Workflow> surveys = new WorkflowService( context ).Queryable().Where( w => w.WorkflowTypeId == WorkflowTypeId ).ToList();
            surveys.LoadAttributes();
            hfSurveys.Value = JsonConvert.SerializeObject( surveys ); 
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Events

        #endregion

        #region Methods

        #endregion
    }
}