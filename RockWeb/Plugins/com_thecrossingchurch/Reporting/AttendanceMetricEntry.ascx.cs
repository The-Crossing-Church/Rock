﻿using System;
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
using System.Text.RegularExpressions;
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

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Attendance Metric Entry" )]
    [Category( "com_thecrossingchurch > Attendance Metric Entry" )]
    [Description( "Custom page for entering the attendance metrics" )]

    [IntegerField( "Service Type DefinedTypeId", "The defined type for the service times.", true, 0, "", 0 )]
    [IntegerField( "Sunday Service Times CategoryId", "The category for the service times.", true, 0, "", 0 )]
    [IntegerField( "MetricId", "The Id of the Metric", true, 0, "", 0 )]
    [IntegerField( "Detailed View Page", "The Id of the Detailed View Page", true, 0, "", 0 )]
    [IntegerField( "Location Parent Group Id", "The Id of the location category to be expanded", true, 0, "", 0 )]

    public partial class AttendanceMetricEntry : Rock.Web.UI.RockBlock //, ICustomGridColumns
    {
        #region Variables
        public int ServiceTypeId { get; set; }
        public int ServiceCategoryId { get; set; }
        public int LocationCategoryId { get; set; }
        public int MetricId { get; set; }
        public List<DefinedValue> ServiceTypes { get; set; }
        public List<Schedule> ServiceTimes { get; set; }
        public int? Id { get; set; }
        public MetricValue Metric { get; set; }
        public MetricValueService service { get; set; }
        public RockContext context { get; set; }
        private static class PageParameterKey
        {
            public const string Id = "Id";
        }
        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
            //scriptManager.RegisterPostBackControl(this.btnSpecial);
            //ScriptManager.RegisterStartupScript(Page, this.GetType(), "AKey", "notes();", true);
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
            ServiceTypeId = GetAttributeValue( "ServiceTypeDefinedTypeId" ).AsInteger();
            ServiceCategoryId = GetAttributeValue( "SundayServiceTimesCategoryId" ).AsInteger();
            LocationCategoryId = GetAttributeValue( "LocationParentGroupId" ).AsInteger();
            MetricId = GetAttributeValue( "MetricId" ).AsInteger();
            if ( !String.IsNullOrEmpty( PageParameter( PageParameterKey.Id ) ) )
            {
                Id = Int32.Parse( PageParameter( PageParameterKey.Id ) );
            }
            context = new RockContext();
            service = new MetricValueService( context );
            ServiceTimes = new ScheduleService( new RockContext() ).Queryable().Where( s => s.CategoryId == ServiceCategoryId ).ToList().OrderBy( s => int.Parse( s.Name.Split( ':' )[0] ) ).ToList();
            ServiceTypes = new DefinedValueService( new RockContext() ).Queryable().Where( dv => dv.DefinedTypeId == ServiceTypeId ).OrderBy( dv => dv.Value ).ToList();
            this.Location.InitialItemParentIds = LocationCategoryId.ToString();
            if ( !Page.IsPostBack )
            {
                LoadServiceTimes();
                LoadServiceTypes();
                int diff = ( 7 + ( DateTime.Now.DayOfWeek - DayOfWeek.Sunday ) ) % 7;
                var stow = DateTime.Now.AddDays( -1 * diff ).Date;
                this.OccurrenceDate.SelectedDate = stow;
                if ( Id.HasValue )
                {
                    Metric = service.Get( Id.Value );
                    var sunday = ServiceTypes.FirstOrDefault( dv => dv.Value == "Sunday Morning" );
                    var svcType = Metric.MetricValuePartitions.FirstOrDefault( mvp => mvp.MetricPartition.Label == "Service Type" );
                    var location = Metric.MetricValuePartitions.FirstOrDefault( mvp => mvp.MetricPartition.Label == "Location" );
                    var loc = new LocationService( new RockContext() ).Get( location.EntityId.Value );
                    if ( svcType.EntityId == sunday.Id )
                    {
                        OpenPanel( new BootstrapButton() { ID = "btnSunday" }, new EventArgs() { } );
                        if ( loc.Name == "Online" )
                        {
                            this.Time.SelectedValue = ServiceTimes.First().Id.ToString();
                        }
                        else
                        {
                            this.Time.SelectedValue = ServiceTimes.FirstOrDefault( st => st.Name == Metric.MetricValueDateTime.Value.ToString( "h:mm" ) ).Id.ToString();
                        }
                    }
                    else
                    {
                        OpenPanel( new BootstrapButton() { ID = "btnSpecial" }, new EventArgs() { } );
                        this.seTime.SelectedTime = TimeSpan.Parse( Metric.MetricValueDateTime.Value.ToString( "HH:mm:ss" ) );
                        this.ServiceType.SelectedValue = svcType.Id.ToString();
                    }
                    this.Location.SetValue( loc );
                    this.Location.DataBind();
                    this.OccurrenceDate.SelectedDate = Metric.MetricValueDateTime;
                    this.Attendance.Text = Metric.YValue.ToString().Split( '.' )[0];
                    this.Attendance.DataBind();
                    this.Notes.Text = Metric.Note;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Adds attendance entry to metrics.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnAddAttendance_Click( object sender, EventArgs e )
        {
            string time = "";
            if ( this.seTime.SelectedTime.HasValue )
            {
                time = this.seTime.SelectedTime.ToString();
            }
            else
            {
                var schedule = ServiceTimes.FirstOrDefault( s => s.Id == int.Parse( this.Time.SelectedValue ) );
                time = schedule.Name;
            }
            int? locationId = this.Location.SelectedValueAsInt();
            Location location = new LocationService( context ).Get( locationId.Value );
            if ( location.Name == "Online" )
            {
                time = "00:00";
            }
            DateTime occurrence = DateTime.Parse( this.OccurrenceDate.SelectedDate.Value.ToString( "MM/dd/yyyy" ) + " " + time );
            int att = int.Parse( this.Attendance.Text.Replace( ",", "" ) );
            int serviceTypeId = ServiceTypes.FirstOrDefault( dv => dv.Value == "Sunday Morning" ).Id;
            if ( !String.IsNullOrEmpty( this.ServiceType.SelectedItem.Value ) && this.seTime.SelectedTime.HasValue )
            {
                serviceTypeId = int.Parse( this.ServiceType.SelectedItem.Value );
            }
            //Check if metric for this location, date, and time already exists
            BootstrapButton btn = new BootstrapButton();
            try
            {
                btn = (BootstrapButton)sender;
            }
            catch
            {
                btn.ID = "ModalButton";
            }
            if ( AlreadyExists( occurrence, locationId.Value ) && btn.ID == "btnAddAttendance" )
            {
                this.alertConfirmAdd.Show(); 
            }
            else
            {
                this.alertConfirmAdd.Hide();
                GenerateMetric( occurrence, att, serviceTypeId, locationId.Value, this.Notes.Text );
                //Clear all values
                //this.OccurrenceDate.SelectedDate = null;
                //this.Time.SelectedValue = null;
                //this.seTime.SelectedTime = null;
                //this.ServiceType.SelectedValue = null;
                this.Attendance.Text = "";
                this.Location.SetValue( null );
                this.Notes.Text = "";
            }
        }

        private bool AlreadyExists( DateTime occurrence, int location )
        {
            var exits = false;
            var metrics = service.Queryable().FirstOrDefault( mv => mv.MetricValueDateTime == occurrence && mv.MetricValuePartitions.FirstOrDefault( mvp => mvp.MetricPartition.Label == "Location" ) != null && mv.MetricValuePartitions.FirstOrDefault( mvp => mvp.MetricPartition.Label == "Location" ).EntityId == location );
            if ( metrics != null )
            {
                exits = true;
            }
            return exits;
        }

        protected void OpenPanel( object sender, EventArgs e )
        {
            //Clear all values
            //this.OccurrenceDate.SelectedDate = null;
            this.Time.SelectedValue = null;
            this.seTime.SelectedTime = null;
            this.ServiceType.SelectedValue = null;
            this.Attendance.Text = "";
            this.Location.SetValue( null );
            this.Notes.Text = "";
            BootstrapButton btn = (BootstrapButton)sender;
            if ( btn.ID == "btnSunday" )
            {
                this.Time.Visible = true;
                this.seTime.Visible = false;
                this.ServiceType.Visible = false;
                this.headingPlaceholder.Controls.Clear();
                var h4 = new HtmlGenericControl( "h4" );
                h4.InnerHtml = "Sunday Morning Attendance";
                this.headingPlaceholder.Controls.Add( h4 );
                this.SpecialPnl.RemoveCssClass( "pressed" );
                this.SundayPnl.AddCssClass( "pressed" );
            }
            else
            {
                this.Time.Visible = false;
                this.seTime.Visible = true;
                this.ServiceType.Visible = true;
                this.headingPlaceholder.Controls.Clear();
                var h4 = new HtmlGenericControl( "h4" );
                h4.InnerHtml = "Special Event Attendance";
                this.headingPlaceholder.Controls.Add( h4 );
                this.SundayPnl.RemoveCssClass( "pressed" );
                this.SpecialPnl.AddCssClass( "pressed" );
            }
            this.EntryForm.Visible = true;
        }

        /// <summary>
        /// Deletes attendance entry metric.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnRemoveAttendance_Click( object sender, EventArgs e )
        {
            if ( !Id.HasValue )
            {
                return;
            }
            var rockContext = new RockContext();
            var metricValueService = new MetricValueService( rockContext );
            var metricValuePartitionService = new MetricValuePartitionService( rockContext );
            var metricValue = metricValueService.Get( Id.Value );

            rockContext.WrapTransaction( () =>
             {
                 metricValuePartitionService.DeleteRange( metricValue.MetricValuePartitions );
                 metricValueService.Delete( metricValue );
                 rockContext.SaveChanges();
             } );
            Response.Redirect( Request.Url.ToString().Split( '?' )[0] );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds items to drop down for service time for Sunday Morning
        /// </summary>
        protected void LoadServiceTimes()
        {
            List<Schedule> schedules = new ScheduleService( new RockContext() ).Queryable().Where( s => s.CategoryId == ServiceCategoryId ).ToList().OrderBy( s => int.Parse( s.Name.Split( ':' )[0] ) ).ToList();
            List<ListItem> items = new List<ListItem>();
            for ( var i = 0; i < schedules.Count(); i++ )
            {
                ListItem item = new ListItem()
                {
                    Text = schedules[i].Name,
                    Value = schedules[i].Id.ToString()
                };
                items.Add( item );
            }
            this.Time.DataTextField = "Text";
            this.Time.DataValueField = "Value";
            this.Time.DataSource = items;
            this.Time.DataBind();
        }

        /// <summary>
        /// Adds items to drop down for service type for special events
        /// </summary>
        protected void LoadServiceTypes()
        {
            List<DefinedValue> values = new DefinedValueService( new RockContext() ).Queryable().Where( dv => dv.DefinedTypeId == ServiceTypeId ).OrderBy( dv => dv.Value ).ToList();
            List<ListItem> items = new List<ListItem>();
            for ( var i = 0; i < values.Count(); i++ )
            {
                ListItem item = new ListItem()
                {
                    Text = values[i].Value,
                    Value = values[i].Id.ToString()
                };
                if ( item.Text != "Sunday Morning" )
                {
                    items.Add( item );
                }
            }
            this.ServiceType.DataTextField = "Text";
            this.ServiceType.DataValueField = "Value";
            this.ServiceType.DataSource = items;
            this.ServiceType.DataBind();
        }

        protected void GenerateMetric( DateTime occurrence, int? attendance, int serviceTypeId, int locationId, string notes )
        {
            if ( Id.HasValue )
            {
                Metric = service.Get( Id.Value );
                Metric.MetricValueDateTime = occurrence;
                //Metric.YValue = attendance == 0 ? null : attendance; //Not sure how we want it to behave, 0s or nulls so the graph breaks 
                Metric.YValue = attendance;
                Metric.Note = notes;
                var loc = Metric.MetricValuePartitions.FirstOrDefault( mvp => mvp.MetricPartition.Label == "Location" );
                loc.EntityId = locationId;
                var svc = Metric.MetricValuePartitions.FirstOrDefault( mvp => mvp.MetricPartition.Label == "Service Type" );
                svc.EntityId = serviceTypeId;
                Metric.MetricValuePartitions = new List<MetricValuePartition>() { loc, svc };
                Metric.ModifiedDateTime = RockDateTime.Now;
                Metric.ModifiedByPersonAliasId = CurrentPersonAliasId;
                context.SaveChanges();
                int pageId = GetAttributeValue( "DetailedViewPage" ).AsInteger();
                var svctype = ServiceTypes.FirstOrDefault( st => st.Id == serviceTypeId );
                string url = "/page/" + pageId + "?Date=" + occurrence.ToString( "M/d/yyyy" ) + "&ServiceType=" + svctype.Value;
                Response.Redirect( url );
            }
            else
            {
                var _context = new RockContext();
                List<MetricPartition> metricPartitions = new MetricPartitionService( _context ).Queryable().Where( mp => mp.MetricId == MetricId ).ToList();
                Collection<MetricValuePartition> partitions = new Collection<MetricValuePartition>();
                for ( var i = 0; i < metricPartitions.Count(); i++ )
                {
                    var par = new MetricValuePartition();
                    par.MetricPartitionId = metricPartitions[i].Id;
                    par.CreatedDateTime = RockDateTime.Now;
                    par.ModifiedDateTime = RockDateTime.Now;
                    par.CreatedByPersonAliasId = CurrentPersonAliasId;
                    par.ModifiedByPersonAliasId = CurrentPersonAliasId;
                    if ( metricPartitions[i].Label == "Location" )
                    {
                        par.EntityId = locationId;
                    }
                    else
                    {
                        par.EntityId = serviceTypeId;
                    }
                    partitions.Add( par );
                }
                MetricValue value = new MetricValue()
                {
                    MetricValueType = 0,
                    YValue = attendance,
                    MetricId = MetricId,
                    Note = notes,
                    MetricValueDateTime = occurrence,
                    CreatedDateTime = RockDateTime.Now,
                    ModifiedDateTime = RockDateTime.Now,
                    CreatedByPersonAliasId = CurrentPersonAliasId,
                    ModifiedByPersonAliasId = CurrentPersonAliasId,
                    MetricValuePartitions = partitions
                };
                new MetricValueService( _context ).Add( value );
                _context.SaveChanges();
            }
        }

        #endregion

    }

}