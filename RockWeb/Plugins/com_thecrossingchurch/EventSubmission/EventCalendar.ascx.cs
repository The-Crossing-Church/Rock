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

namespace RockWeb.Plugins.com_thecrossingchurch.EventSubmission
{
    /// <summary>
    /// Request form for Event Submissions
    /// </summary>
    [DisplayName( "Event Calendar" )]
    [Category( "com_thecrossingchurch > Event Submission" )]
    [Description( "Calendar of Events" )]

    [IntegerField( "DefinedTypeId", "The id of the defined type for rooms.", true, 0, "", 0 )]
    [IntegerField( "MinistryDefinedTypeId", "The id of the defined type for ministries.", true, 0, "", 0 )]
    [IntegerField( "ContentChannelId", "The id of the content channel for an event request.", true, 0, "", 0 )]

    public partial class EventCalendar : Rock.Web.UI.RockBlock
    {
        #region Variables
        public RockContext context { get; set; }
        private int DefinedTypeId { get; set; }
        private int MinistryDefinedTypeId { get; set; }
        private int ContentChannelId { get; set; }
        private List<DefinedValue> Rooms { get; set; }
        private List<DefinedValue> Ministries { get; set; }
        private DateTime SelectedMonthStart { get; set; }
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
            Rooms = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == DefinedTypeId ).ToList();
            Rooms.LoadAttributes();
            hfRooms.Value = JsonConvert.SerializeObject( Rooms.Select( dv => new { Id = dv.Id, Value = dv.Value, Type = dv.AttributeValues.FirstOrDefault( av => av.Key == "Type" ).Value.Value, Capacity = dv.AttributeValues.FirstOrDefault( av => av.Key == "Capacity" ).Value.Value.AsInteger(), IsActive = dv.IsActive } ) );
            Ministries = new DefinedValueService( context ).Queryable().Where( dv => dv.DefinedTypeId == MinistryDefinedTypeId ).OrderBy( dv => dv.Order ).ToList();
            Ministries.LoadAttributes();
            hfMinistries.Value = JsonConvert.SerializeObject( Ministries.Select( dv => new { Id = dv.Id, Value = dv.Value, IsPersonal = dv.AttributeValues.FirstOrDefault( av => av.Key == "IsPersonalRequest" ).Value.Value.AsBoolean(), IsActive = dv.IsActive } ) );
            if ( !String.IsNullOrEmpty( hfFocusDate.Value ) )
            {
                SelectedMonthStart = DateTime.Parse( hfFocusDate.Value );
            }
            else
            {
                SelectedMonthStart = DateTime.Now;
                SelectedMonthStart = new DateTime( SelectedMonthStart.Year, SelectedMonthStart.Month, 1 );
            }
            GetAllRequests();
            if ( !Page.IsPostBack )
            {

            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Update the Request List based on focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnSwitchFocus_Click( object sender, EventArgs e )
        {
            if ( !String.IsNullOrEmpty( hfFocusDate.Value ) )
            {
                SelectedMonthStart = DateTime.Parse( hfFocusDate.Value );
            }
            GetAllRequests();
        }

        /// <summary>
        /// Get all requests
        /// </summary>
        protected void GetAllRequests()
        {
            DateTime NextMonthStart = SelectedMonthStart.AddMonths( 1 );
            ContentChannelItemService svc = new ContentChannelItemService( context );
            var items = svc.Queryable().Where( i => i.ContentChannelId == ContentChannelId ).ToList();
            items.LoadAttributes();
            items = items.Where( i => {
                var status = i.AttributeValues["RequestStatus"].Value;
                var resources = i.AttributeValues["RequestType"].Value;
                if(status != "Submitted" && status != "Cancelled" && status != "Denied" && resources.Contains("Room") )
                {
                    var dateStr = i.AttributeValues["EventDates"];
                    var dates = dateStr.Value.Split( ',' );
                    foreach ( var d in dates )
                    {
                        DateTime dt = DateTime.Parse( d );
                        if ( DateTime.Compare( dt, SelectedMonthStart ) >= 1 && DateTime.Compare( dt, NextMonthStart ) < 1 )
                        {
                            return true;
                        }
                    }
                }
                return false;
            } ).ToList();
            hfRequests.Value = JsonConvert.SerializeObject( items.Select( i => new { Id = i.Id, Value = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestJSON" ).Value.Value, CreatedBy = i.CreatedByPersonName, CreatedOn = i.CreatedDateTime, RequestStatus = i.AttributeValues.FirstOrDefault( av => av.Key == "RequestStatus" ).Value.Value } ) );
        }

        #endregion

    }
}