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

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName("Attendance Metric Entry")]
    [Category("com_thecrossingchurch > Attendance Metric Entry")]
    [Description("Custom page for entering the attendance metrics")]

    [IntegerField("Service Type DefinedTypeId", "The defined type for the service times.", true, 0, "", 0)]
    [IntegerField("Sunday Service Times CategoryId", "The category for the service times.", true, 0, "", 0)]

    public partial class AttendanceMetricEntry : Rock.Web.UI.RockBlock //, ICustomGridColumns
    {
        #region Variables
        public int ServiceTypeId { get; set; }
        public int ServiceCategoryId { get; set; }
        public List<DefinedValue> ServiceTypes { get; set; }
        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            //scriptManager.RegisterPostBackControl(this.btnSpecial);
            //ScriptManager.RegisterStartupScript(Page, this.GetType(), "AKey", "notes();", true);
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit(e);
        }

        /// <summary>
        /// Raises the <see cref="E:Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad(e);
            this.ErrorMsg.Visible = false;
            ServiceTypeId = GetAttributeValue("ServiceTypeDefinedTypeId").AsInteger();
            ServiceCategoryId = GetAttributeValue("SundayServiceTimesCategoryId").AsInteger();
            LoadServiceTimes();
            LoadServiceTypes();
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
            if ( this.OccurrenceDate.SelectedDate.HasValue && !String.IsNullOrEmpty(this.Time.SelectedValue) && !String.IsNullOrEmpty(this.Attendance.Text) && !String.IsNullOrEmpty(this.Location.SelectedValue) )
            {
                DateTime occurrence = DateTime.Parse(this.OccurrenceDate.SelectedDate.Value.ToString("MM/dd/yyyy") + " " + this.Time.SelectedValue);
                int att = int.Parse(this.Attendance.Text);
                DefinedValue serviceType = ServiceTypes.FirstOrDefault(st => st.Value == "Sunday Morning");
                int? locationId = this.Location.SelectedValueAsInt();
                GenerateMetric(occurrence, att, serviceType.Id, locationId.Value); 
            }
            else if ( this.seDate.SelectedDate.HasValue && !String.IsNullOrEmpty(this.ServiceType.SelectedValue) && this.seTime.SelectedTime.HasValue && !String.IsNullOrEmpty(this.seAttendance.Text) && !String.IsNullOrEmpty(this.seLocation.SelectedValue) )
            {
                DateTime occurrence = DateTime.Parse(this.seDate.SelectedDate.Value.ToString("MM/dd/yyyy") + " " + this.seTime.SelectedTime);
                int att = int.Parse(this.seAttendance.Text);
                int? serviceTypeId = this.ServiceType.SelectedValueAsInt(); 
                int? locationId = this.seLocation.SelectedValueAsInt();
                GenerateMetric(occurrence, att, serviceTypeId.Value, locationId.Value);
            }
            else
            {
                //display error that not all required fields are filled out
                this.ErrorMsg.Visible = true;
            }
            //Clear all values
            this.OccurrenceDate.SelectedDate = null;
            this.Time.SelectedValue = null;
            this.Attendance.Text = "";
            this.Location.SetValue(null);
            this.seDate.SelectedDate = null;
            this.seTime.SelectedTime = null;
            this.ServiceType.SelectedValue = null;
            this.seAttendance.Text = "";
            this.seLocation.SetValue(null);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds items to drop down for service time for Sunday Morning
        /// </summary>
        protected void LoadServiceTimes()
        {
            List<Schedule> schedules = new ScheduleService(new RockContext()).Queryable().Where(s => s.CategoryId == ServiceCategoryId).ToList().OrderBy(s => int.Parse(s.Name.Split(':')[0])).ToList();
            for ( var i = 0; i < schedules.Count(); i++ )
            {
                ListItem item = new ListItem()
                {
                    Text = schedules[i].Name,
                    Value = schedules[i].Id.ToString()
                };
                this.Time.Items.Add(item);
            }
        }

        /// <summary>
        /// Adds items to drop down for service type for special events
        /// </summary>
        protected void LoadServiceTypes()
        {
            List<DefinedValue> values = new DefinedValueService(new RockContext()).Queryable().Where(dv => dv.DefinedTypeId == ServiceTypeId).OrderBy(dv => dv.Value).ToList();
            ServiceTypes = values;
            for ( var i = 0; i < values.Count(); i++ )
            {
                ListItem item = new ListItem()
                {
                    Text = values[i].Value,
                    Value = values[i].Id.ToString()
                };
                if ( item.Text != "Sunday Morning" )
                {
                    this.ServiceType.Items.Add(item);
                }
            }
        }

        protected void GenerateMetric(DateTime occurrence, int attendance, int serviceTypeId, int locationId)
        {
            var x = occurrence;
            var y = attendance;
            var z = serviceTypeId;
            var k = locationId; 
        }

        #endregion

    }

}