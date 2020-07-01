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
using AngleSharp.Dom.Html;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

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
        public bool IsSunday { get; set; }
        public int ServiceTypeId { get; set; }
        public int ServiceCategoryId { get; set; }
        public List<DefinedValue> ServiceTypes { get; set; }
        #endregion

        #region Base Control Methods

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            scriptManager.RegisterPostBackControl(this.btnSpecial);
            scriptManager.RegisterPostBackControl(this.btnSunday);
            this.Entryform.Visible = false;
            IsSunday = true; 
            //ScriptManager.RegisterStartupScript(Page, this.GetType(), "AKey", "notes();", true);
        }

        /// <summary>
        /// Raises the <see cref="E:Init" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit(e);
            GenerateControls();
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
        }

        #endregion

        #region Events

        /// <summary>
        /// Btn is clicked to enter data
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void OpenPanel( object sender, EventArgs e )
        {
            var btn = (Rock.Web.UI.Controls.BootstrapButton)sender;
            if(btn.ID == "btnSunday")
            {
                IsSunday = true;
            }
            else
            {
                IsSunday = false;
            }
            GenerateControls();
        }

        /// <summary>
        /// Save emtric data
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            DatePicker dtPicker = (DatePicker)this.Entryform.FindControl("OccurrenceDate");
            var value = dtPicker.SelectedDate.Value; 
        }


        #endregion

        #region Methods

        /// <summary>
        /// Adds items to drop down for service time for Sunday Morning
        /// </summary>
        protected RockDropDownList LoadServiceTimes(RockDropDownList list)
        {
            List<Schedule> schedules = new ScheduleService(new RockContext()).Queryable().Where(s => s.CategoryId == ServiceCategoryId).ToList().OrderBy(s => int.Parse(s.Name.Split(':')[0])).ToList();
            for ( var i = 0; i < schedules.Count(); i++ )
            {
                ListItem item = new ListItem()
                {
                    Text = schedules[i].Name,
                    Value = schedules[i].Id.ToString()
                };
                list.Items.Add(item);
            }
            return list; 
        }

        /// <summary>
        /// Adds items to drop down for service type for special events
        /// </summary>
        protected RockDropDownList LoadServiceTypes( RockDropDownList list )
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
                list.Items.Add(item);
            }
            return list;
        }

        protected void GenerateControls()
        {
            this.Entryform.Controls.Clear();

            var content = new HtmlGenericControl("div");

            var dtRow = new HtmlGenericControl("div");
            dtRow.AddCssClass("row");
            var dtCol = new HtmlGenericControl("div");
            dtCol.AddCssClass("col col-xs-6");
            var dtPicker = new DatePicker();
            dtPicker.Label = "Date";
            dtPicker.ID = "OccurrenceDate";
            dtPicker.Required = true;
            dtCol.Controls.Add(dtPicker);
            var timeCol = new HtmlGenericControl("div");
            timeCol.AddCssClass("col col-xs-6");
            if ( IsSunday )
            {
                var serviceDropDown = new RockDropDownList();
                serviceDropDown.Label = "Service";
                serviceDropDown.ID = "Time";
                serviceDropDown = LoadServiceTimes(serviceDropDown);
                serviceDropDown.Required = true;
                timeCol.Controls.Add(serviceDropDown);
            }
            else
            {
                var timePicker = new TimePicker();
                timePicker.Label = "Time";
                timePicker.ID = "Time";
                timePicker.Required = true;
                timeCol.Controls.Add(timePicker);
            }
            dtRow.Controls.Add(dtCol);
            dtRow.Controls.Add(timeCol);
            content.Controls.Add(dtRow);

            var svcTypeRow = new HtmlGenericControl("div");
            svcTypeRow.AddCssClass("row");
            var svcTypeCol = new HtmlGenericControl("div");
            svcTypeCol.AddCssClass("col col-xs-12");
            var svcTypeDropDown = new RockDropDownList();
            svcTypeDropDown.Label = "Service Type";
            svcTypeDropDown.ID = "ServiceType";
            svcTypeDropDown = LoadServiceTypes(svcTypeDropDown);
            svcTypeDropDown.Required = true;
            if ( IsSunday )
            {
                var sunday = ServiceTypes.FirstOrDefault(dv => dv.Value == "Sunday Morning");
                svcTypeDropDown.SetValue(sunday);
            }
            svcTypeCol.Controls.Add(svcTypeDropDown);
            svcTypeRow.Controls.Add(svcTypeCol);
            svcTypeRow.Visible = IsSunday ? false : true;
            content.Controls.Add(svcTypeRow);

            var dataRow = new HtmlGenericControl("div");
            dataRow.AddCssClass("row");
            var attCol = new HtmlGenericControl("div");
            attCol.AddCssClass("col col-xs-6");
            var attTextBox = new RockTextBox();
            attTextBox.Label = "Attendance";
            attTextBox.ID = "Attendance";
            attTextBox.Required = true;
            attCol.Controls.Add(attTextBox);
            var locCol = new HtmlGenericControl("div");
            locCol.AddCssClass("col col-xs-6");
            var locPicker = new LocationItemPicker();
            locPicker.Label = "Location";
            locPicker.ID = "Location";
            locPicker.Required = true;
            locCol.Controls.Add(locPicker);
            dataRow.Controls.Add(attCol);
            dataRow.Controls.Add(locCol);
            content.Controls.Add(dataRow);

            var notesRow = new HtmlGenericControl("div");
            notesRow.AddCssClass("row");
            var notesCol = new HtmlGenericControl("div");
            notesCol.AddCssClass("col col-xs-12");
            var noteTextBox = new RockTextBox();
            noteTextBox.Label = "Notes";
            noteTextBox.ID = "Notes";
            noteTextBox.TextMode = TextBoxMode.MultiLine;
            notesCol.Controls.Add(noteTextBox);
            notesRow.Controls.Add(notesCol);
            content.Controls.Add(notesRow);

            this.Entryform.Controls.Add(content);
            this.Entryform.Visible = true;
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