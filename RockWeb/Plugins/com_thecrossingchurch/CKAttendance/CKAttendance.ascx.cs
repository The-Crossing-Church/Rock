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

namespace RockWeb.Plugins.com_thecrossingchurch.CKAttendance
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "CK Attendance Report" )]
    [Category( "com_thecrossingchurch > CK Attendance" )]
    [Description( "Custom Attendance Report for Crossing Kids" )]

    public partial class CKAttendance : Rock.Web.UI.RockBlock //, ICustomGridColumns
    {
        #region Variables

        // Variables that get set with filter 
        private DateTime start;
        private DateTime end;
        private List<int> svcTimes;
        public List<AttendanceReportData> results;
        public List<AttendanceReportData> thresholds; 
        public List<ClassesBySchedule> inUseClassrooms; 
        public List<string> classroomSort = new List<string>() {
            "Infants",
            "Crawlers",
            "Toddler Blue",
            "Toddler Blue A",
            "Toddler Blue B",
            "Toddler Green",
            "Toddler Green A",
            "Toddler Green B",
            "Toddler Overflow",
            "Preschool Purple",
            "Preschool Purple A",
            "Preschool Purple B",
            "Preschool Orange",
            "Preschool Orange A",
            "Preschool Orange B",
            "Preschool Red",
            "Preschool Red A",
            "Preschool Red B",
            "Preschool Overflow",
            "Kindergarten",
            "Kindergarten A",
            "Kindergarten B",
            "1st Grade",
            "1st Grade A",
            "1st Grade B",
            "K-1st Overflow",
            "2nd Grade",
            "2nd Grade A",
            "2nd Grade B",
            "3rd Grade",
            "3rd Grade A",
            "3rd Grade B",
            "2nd-3rd Overflow",
            "4th Grade",
            "4th Grade A",
            "4th Grade B",
            "5th Grade",
            "5th Grade A",
            "5th Grade B",
            "4th-5th Overflow",
            "K-2 Multi-Age",
            "3-5 Multi-Age"
        };

        #endregion

        #region Base Control Methods

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
            if (!stDate.SelectedDate.HasValue)
            {
                stDate.SelectedDate = new DateTime(2020, 1, 1); //DateTime.Now.AddMonths(-3);
            }
            if (!endDate.SelectedDate.HasValue)
            {
                endDate.SelectedDate = new DateTime(2020, 2, 15); // DateTime.Now;
            }
            start = stDate.SelectedDate.Value;
            end = endDate.SelectedDate.Value;
            BindFilter();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            start = stDate.SelectedDate.Value;
            end = endDate.SelectedDate.Value;
            svcTimes = lbSchedules.SelectedValues.Select(x => Int32.Parse(x)).ToList();
            GetAttendance();
        }
        protected void sdrpDates_SelectedDateRangeChanged(object sender, EventArgs e)
        {
            BindFilter();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Binds the schedules.
        /// </summary>
        private void BindFilter()
        {
            var selectedItems = lbSchedules.SelectedValuesAsInt;

            lSchedules.Visible = true;
            lbSchedules.Visible = false;
            lbSchedules.Items.Clear();
            btnFilter.Enabled = false;

            DateTime? dateStart = stDate.SelectedDate;
            DateTime? dateEnd = endDate.SelectedDate;

            lbSchedules.Required = GetAttributeValue("RequireSchedule").AsBoolean(true);
            if (lbSchedules.Required)
            {
                pnlSchedules.AddCssClass("required");
            }
            else
            {
                pnlSchedules.RemoveCssClass("required");
            }

            if (dateStart.HasValue)
            {
                var area = GetAttributeValue("CheckInArea").AsIntegerOrNull();

                using (var rockContext = new RockContext())
                {
                    var occQry = new AttendanceOccurrenceService(rockContext)
                        .Queryable().AsNoTracking()
                        .Where(o =>
                           o.OccurrenceDate >= dateStart &&
                           (!dateEnd.HasValue || o.OccurrenceDate < dateEnd) &&
                           o.Attendees.Any(a => a.DidAttend.HasValue && a.DidAttend.Value) &&
                           o.Schedule != null
                        );

                    if (area.HasValue)
                    {
                        var groupTypeIds = new GroupTypeService(rockContext)
                            .GetAllAssociatedDescendents(area.Value)
                            .Select(t => t.Id)
                            .ToList();

                        occQry = occQry
                            .Where(o =>
                               o.Group != null &&
                               groupTypeIds.Contains(o.Group.GroupTypeId));
                    }

                    var serviceTimes = occQry
                        .Select(o => o.Schedule)
                        .Distinct()
                        .ToList()
                        .OrderBy(s => s.StartTimeOfDay)
                        .Select(o => new
                        {
                            o.Id,
                            o.Name
                        })
                        .ToList();

                    foreach (var serviceTime in serviceTimes)
                    {
                        var item = new ListItem(serviceTime.Name, serviceTime.Id.ToString());
                        item.Selected = selectedItems.Contains(serviceTime.Id);
                        lbSchedules.Items.Add(item);
                    }

                    if (serviceTimes.Any())
                    {
                        lSchedules.Visible = false;
                        lbSchedules.Visible = true;
                        btnFilter.Enabled = true;
                    }
                    else
                    {
                        btnFilter.Enabled = !lbSchedules.Required;
                        lSchedules.Text = "There are not any Service Times for selected Date(s)";
                    }
                }
            }
        }

        /// <summary>
        /// Generates the report data
        /// </summary>
        private void GetAttendance() {
            var schedules = new ScheduleService(new RockContext()).Queryable().Where(x => svcTimes.Contains(x.Id)).ToList().OrderBy(x => x.StartTimeOfDay);
            var locations = new LocationService(new RockContext()).Queryable().Where(l => l.IsActive && !String.IsNullOrEmpty(l.Name)).ToList();

            var attendance = new AttendanceService(new RockContext()).Queryable().Where(x =>
                DateTime.Compare(start, x.StartDateTime) <= 0 &&
                DateTime.Compare(end, x.StartDateTime) >= 0 &&
                svcTimes.Contains(x.Occurrence.ScheduleId.Value) &&
                (x.Occurrence.Group.GroupTypeId == 129 || x.Occurrence.Group.GroupTypeId == 128 || x.Occurrence.Group.GroupTypeId == 122) //LO, PS and Elem Areas Only
            );
            inUseClassrooms = new List<ClassesBySchedule>();
            for(var i=0; i<svcTimes.Count(); i++)
            {
                var svcId = svcTimes[i];
                var current = attendance.Where(x => x.Occurrence.ScheduleId == svcId).Select(x => x.Occurrence.Location.Name).ToList().Select(x => TransformClassName(x)).Distinct().OrderBy(x => classroomSort.IndexOf(x)).ToList();
                inUseClassrooms.Add(new ClassesBySchedule { ScheduleId = svcId, Classrooms = current });
            }
            BuildThresholdData(locations, schedules.ToList()); 

            //Early Childhood, Multiage doesn't exist
            var early_childhood = attendance.Where(x => x.Occurrence.Group.GroupTypeId == 128 || x.Occurrence.Group.GroupTypeId == 122).ToList().GroupBy(x => new { x.Occurrence.OccurrenceDate, x.Occurrence.ScheduleId, x.Occurrence.LocationId }).Select(x => { 
                var location = locations.FirstOrDefault(l => l.Id == x.Key.LocationId);
                location.LoadAttributes();
                var threshold = GetThreshold(location.AttributeValues["ThresholdHistoricalData"].Value.ToString(), x.Key.OccurrenceDate);
                var ca =  new ClassAttendance()
                {
                    ClassName = TransformClassName(location.Name),
                    ScheduleId = x.Key.ScheduleId.Value,
                    AttendanceCount = x.Count(),
                    OccurrenceDate = x.Key.OccurrenceDate
                };
                if (threshold.HasValue && ca.AttendanceCount > threshold)
                {
                    ca.OverThreshold = true;
                }
                else if (!threshold.HasValue && ca.AttendanceCount > location.SoftRoomThreshold)
                {
                    ca.OverThreshold = true;
                }
                return ca; 
            }).Distinct().ToList();
            var ecSvcAtt = early_childhood.GroupBy(x => new { x.OccurrenceDate, x.ScheduleId }).Select(x =>
            {
                var svcAtt = new ServiceAttendance()
                {
                    ServiceTime = schedules.FirstOrDefault(s => s.Id == x.Key.ScheduleId).Name,
                    OccurrenceDate = x.Key.OccurrenceDate,
                    ClassAttendances = early_childhood.Where(ec => ec.OccurrenceDate == x.Key.OccurrenceDate && ec.ScheduleId == x.Key.ScheduleId).OrderBy(c => classroomSort.IndexOf(c.ClassName)).ToList()
                };
                svcAtt.Total = svcAtt.ClassAttendances.Select(ca => ca.AttendanceCount).Sum();
                return svcAtt;
            });

            //Elementary, Multiage exists
            var elem = attendance.Where(x => x.Occurrence.Group.GroupTypeId == 129).ToList().Select(e => {
                var scheduleStart = new DateTime(e.StartDateTime.Year, e.StartDateTime.Month, e.StartDateTime.Day, 0, 0, 0).Add(e.Occurrence.Schedule.StartTimeOfDay);
                var ts = scheduleStart - e.StartDateTime;
                if (ts.TotalMinutes <= 45)
                {
                    return e;
                }
                return new Attendance() { Id = -1 };
            }).Where(e => e.Id > 0).GroupBy(x => new { x.Occurrence.OccurrenceDate, x.Occurrence.ScheduleId, x.Occurrence.LocationId }).Select(x => {
                var location = locations.FirstOrDefault(l => l.Id == x.Key.LocationId);
                location.LoadAttributes();
                var threshold = GetThreshold(location.AttributeValues["ThresholdHistoricalData"].Value.ToString(), x.Key.OccurrenceDate);
                var ca = new ClassAttendance()
                {
                    ClassName = TransformClassName(location.Name),
                    ScheduleId = x.Key.ScheduleId.Value,
                    AttendanceCount = x.Count(),
                    OccurrenceDate = x.Key.OccurrenceDate
                };
                if (threshold.HasValue && ca.AttendanceCount > threshold)
                {
                    ca.OverThreshold = true;
                }
                else if (!threshold.HasValue && ca.AttendanceCount > location.SoftRoomThreshold)
                {
                    ca.OverThreshold = true;
                }
                return ca;
            }).Distinct().ToList();
            var elemMultiAge = attendance.Where(x => x.Occurrence.Group.GroupTypeId == 129).ToList().Select(e => {
                var scheduleStart = new DateTime(e.StartDateTime.Year, e.StartDateTime.Month, e.StartDateTime.Day, 0, 0, 0).Add(e.Occurrence.Schedule.StartTimeOfDay);
                var ts = scheduleStart - e.StartDateTime;
                if (ts.TotalMinutes > 45)
                {
                    var isK2 = false;
                    if (e.Occurrence.Group.Name.Contains("Kindergarten") || e.Occurrence.Group.Name.Contains("1st") || e.Occurrence.Group.Name.Contains("2nd"))
                    {
                        isK2 = true;
                    }
                    return new { isValid=true, att=e, isK2= isK2};
                }
                return new { isValid=false, att = new Attendance(){ }, isK2=false };
            }).Where(e => e.isValid == true).GroupBy(x => new { x.isK2, x.att.Occurrence.OccurrenceDate, x.att.Occurrence.ScheduleId }).Select(x => {
                var idx = inUseClassrooms.Select(iuc => iuc.ScheduleId).ToList().IndexOf(x.Key.ScheduleId.Value);
                var className = x.Key.isK2 ? "K-2 Multi-Age" : "3-5 Multi-Age";
                if (inUseClassrooms[idx].Classrooms.IndexOf(className) < 0)
                {
                    inUseClassrooms[idx].Classrooms.Add(className);
                    inUseClassrooms[idx].Classrooms = inUseClassrooms[idx].Classrooms.OrderBy(iuc => classroomSort.IndexOf(iuc)).ToList();
                }
                return new ClassAttendance()
                {
                    ClassName = x.Key.isK2 ? "K-2 Multi-Age" : "3-5 Multi-Age",
                    ScheduleId = x.Key.ScheduleId.Value,
                    AttendanceCount = x.Count(),
                    OccurrenceDate = x.Key.OccurrenceDate
                };
            }).OrderBy(x => classroomSort.IndexOf(x.ClassName)).Distinct().ToList();

            var elemSvcAtt = elem.Union(elemMultiAge).GroupBy(x => new { x.OccurrenceDate, x.ScheduleId }).Select(x =>
            {
                var svcAtt = new ServiceAttendance()
                {
                    ServiceTime = schedules.FirstOrDefault(s => s.Id == x.Key.ScheduleId).Name,
                    OccurrenceDate = x.Key.OccurrenceDate,
                    ClassAttendances = elem.Where(ec => ec.OccurrenceDate == x.Key.OccurrenceDate && ec.ScheduleId == x.Key.ScheduleId).Union(elemMultiAge.Where(em => em.ScheduleId == x.Key.ScheduleId && em.OccurrenceDate == x.Key.OccurrenceDate)).OrderBy(c => classroomSort.IndexOf(c.ClassName)).ToList()
                };
                svcAtt.Total = svcAtt.ClassAttendances.Select(ca => ca.AttendanceCount).Sum();
                svcAtt.MultiAgeTotal = elemMultiAge.Where(ma => ma.OccurrenceDate == x.Key.OccurrenceDate && ma.ScheduleId == x.Key.ScheduleId).Select(ma => ma.AttendanceCount).Sum();
                return svcAtt;
            });

            var list = from ec in ecSvcAtt
                       join el in elemSvcAtt
                       on new { ec.ServiceTime, ec.OccurrenceDate } equals new { el.ServiceTime, el.OccurrenceDate }
                       select new ServiceAttendance() { ServiceTime = ec.ServiceTime, ClassAttendances = ec.ClassAttendances.Union(el.ClassAttendances).ToList(), OccurrenceDate = ec.OccurrenceDate, Total = ec.Total + el.Total, MultiAgeTotal = el.MultiAgeTotal };

            results = list.GroupBy(x => new { x.OccurrenceDate }).Select(x =>
            {
                var data = new AttendanceReportData()
                {
                    Title = x.Key.OccurrenceDate.ToString("MM/dd/yy"),
                    OccurrenceDate = x.Key.OccurrenceDate,
                    ServiceAttendace = list.Where(l => l.OccurrenceDate == x.Key.OccurrenceDate).OrderBy(l => schedules.Select(s => s.Name).ToList().IndexOf(l.ServiceTime)).ToList()
                };
                data.Total = data.ServiceAttendace.Select(s => s.Total).Sum();
                data.UniqueTotal = attendance.Where(a => a.Occurrence.OccurrenceDate == x.Key.OccurrenceDate).Select(a => a.PersonAlias.PersonId).Distinct().Count();
                data.MultiAgeTotal = elemMultiAge.Where(em => em.OccurrenceDate == x.Key.OccurrenceDate).Select(em => em.AttendanceCount).Sum();
                return data;
            }).ToList();

            //Add in threshold data where it needs to go
            results = results.Union(thresholds).OrderByDescending(x => x.Title).OrderBy(x => x.OccurrenceDate).ToList(); 

            BuildControl();
        }

        public void BuildControl()
        {
            var div = new HtmlGenericControl("div");
            div.AddCssClass("custom-row");
            var header = new HtmlGenericControl("div");
            header.InnerText = "Classroom";
            header.AddCssClass("custom-col name-col");
            div.Controls.Add(header);
            phContent.Controls.Add(div);
            for (var i=0; i<results.Count(); i++)
            {
                var h = new HtmlGenericControl("div");
                h.InnerText = results[i].Title;
                h.AddCssClass("custom-col");
                if(i == 0)
                {
                    h.AddCssClass("first-custom-col");
                }
                div.Controls.Add(h);
            }
            for(var i=0; i<results[0].ServiceAttendace.Count(); i++)
            {
                var r = new HtmlGenericControl("div");
                r.AddCssClass("service-group");
                var svcTime = new HtmlGenericControl("div");
                svcTime.AddCssClass("custom-row service-time");
                var svcTimeCol = new HtmlGenericControl("div");
                svcTimeCol.AddCssClass("custom-col name-col");
                svcTimeCol.InnerText = results[0].ServiceAttendace[i].ServiceTime;
                svcTime.Controls.Add(svcTimeCol);
                r.Controls.Add(svcTime);
                var idx = inUseClassrooms.Select(x => x.ScheduleId).ToList().IndexOf(results[0].ServiceAttendace[i].ClassAttendances[0].ScheduleId);
                for (var j=0; j<inUseClassrooms[idx].Classrooms.Count(); j++)
                {
                    var classroom = new HtmlGenericControl("div");
                    classroom.AddCssClass("custom-row");
                    var classCol = new HtmlGenericControl("div");
                    classCol.AddCssClass("custom-col name-col");
                    classCol.InnerText = inUseClassrooms[idx].Classrooms[j];
                    if(j%2 > 0)
                    {
                        classroom.AddCssClass("bg-secondary");
                        classCol.AddCssClass("bg-secondary");
                    }
                    classroom.Controls.Add(classCol);

                    //Add Attendance Numbers
                    for(var k=0; k<results.Count(); k++)
                    {
                        var att = new HtmlGenericControl("div");
                        att.AddCssClass("custom-col");
                        var temp = results[k].ServiceAttendace.Count() > i ? results[k].ServiceAttendace[i].ClassAttendances.FirstOrDefault(c => c.ClassName == inUseClassrooms[idx].Classrooms[j]) : null;
                        att.InnerText = temp != null ? temp.AttendanceCount.ToString() : "";
                        if(temp != null && temp.OverThreshold)
                        {
                            att.AddCssClass("over-threshold");
                        }
                        if(k == 0)
                        {
                            att.AddCssClass("first-custom-col");
                        }
                        classroom.Controls.Add(att); 
                    }
                    r.Controls.Add(classroom);
                }
                var total = new HtmlGenericControl("div");
                total.AddCssClass("custom-row");
                var totalCol = new HtmlGenericControl("div");
                totalCol.AddCssClass("custom-col name-col");
                totalCol.InnerText = "Total";
                total.Controls.Add(totalCol);

                //Add total attendance
                for (var k = 0; k < results.Count(); k++)
                {
                    var att = new HtmlGenericControl("div");
                    att.AddCssClass("custom-col");
                    att.InnerText = results[k].ServiceAttendace.Count() > i ? results[k].ServiceAttendace[i].Total.ToString() : "";
                    if (k == 0)
                    {
                        att.AddCssClass("first-custom-col");
                    }
                    total.Controls.Add(att);
                }
                r.Controls.Add(total);
                if(results[0].ServiceAttendace[i].MultiAgeTotal != null)
                {
                    var mtotal = new HtmlGenericControl("div");
                    mtotal.AddCssClass("custom-row");
                    var mTotalCol = new HtmlGenericControl("div");
                    mTotalCol.AddCssClass("custom-col name-col");
                    mTotalCol.InnerText = "Multi-Age Total";
                    mtotal.Controls.Add(mTotalCol);

                    //Add total attendance
                    for (var k = 0; k < results.Count(); k++)
                    {
                        var att = new HtmlGenericControl("div");
                        att.AddCssClass("custom-col");
                        att.InnerText = results[k].ServiceAttendace.Count() > i ? results[k].ServiceAttendace[i].MultiAgeTotal.ToString() : "";
                        if (k == 0)
                        {
                            att.AddCssClass("first-custom-col");
                        }
                        mtotal.Controls.Add(att);
                    }
                    r.Controls.Add(mtotal);
                }
                phContent.Controls.Add(r);
            }

            var dailyData = new HtmlGenericControl("div");
            dailyData.AddCssClass("custom-seperator");
            var totals = new HtmlGenericControl("div");
            totals.AddCssClass("custom-row");
            var totalsCol = new HtmlGenericControl("div");
            totalsCol.InnerText = "Total";
            totalsCol.AddCssClass("custom-col service-time name-col");
            totals.Controls.Add(totalsCol);
            var utotals = new HtmlGenericControl("div");
            utotals.AddCssClass("custom-row");
            var utotalsCol = new HtmlGenericControl("div");
            utotalsCol.InnerText = "Unique Total";
            utotalsCol.AddCssClass("custom-col service-time name-col");
            utotals.Controls.Add(utotalsCol);
            var matotals = new HtmlGenericControl("div");
            matotals.AddCssClass("custom-row");
            var matotalsCol = new HtmlGenericControl("div");
            matotalsCol.InnerText = "MultiAge Total";
            matotalsCol.AddCssClass("custom-col service-time name-col");
            matotals.Controls.Add(matotalsCol);
            for (var i = 0; i < results.Count(); i++)
            {
                var h = new HtmlGenericControl("div");
                h.InnerText = results[i].Total.ToString();
                h.AddCssClass("custom-col");
                if (i == 0)
                {
                    h.AddCssClass("first-custom-col");
                }
                totals.Controls.Add(h);
                var ut = new HtmlGenericControl("div");
                ut.InnerText = results[i].UniqueTotal.ToString();
                ut.AddCssClass("custom-col");
                if (i == 0)
                {
                    ut.AddCssClass("first-custom-col");
                }
                utotals.Controls.Add(ut);
                var mt = new HtmlGenericControl("div");
                mt.InnerText = results[i].MultiAgeTotal.ToString();
                mt.AddCssClass("custom-col");
                if (i == 0)
                {
                    mt.AddCssClass("first-custom-col");
                }
                matotals.Controls.Add(mt);
            }
            dailyData.Controls.Add(totals);
            dailyData.Controls.Add(utotals);
            dailyData.Controls.Add(matotals);

            phContent.Controls.Add(dailyData);

            phContent.Visible = true;
        }

        public string TransformClassName(string name)
        {
            try
            {
                if (name.Contains("("))
                {
                    if (name.Contains("11:15"))
                    {
                        var idx = name.IndexOf("(");
                        var diff = name.Length - idx + 7; 
                        return name.Substring(6, name.Length - diff);
                    }
                    else
                    {
                        var idx = name.IndexOf("(");
                        var diff = name.Length - idx + 6;
                        return name.Substring(5, name.Length - diff);
                    }
                }
                else
                {
                    if (name.Contains("11:15"))
                    {
                        return name.Substring(6);
                    }
                    else
                    {
                        return name.Substring(5);
                    }
                }
            }
            catch
            {
                return name;
            }

        }

        public void BuildThresholdData(List<Location> locations, List<Schedule> schedules) {
            var data = new List<ThresholdList>(); 
            for(var i=0; i<locations.Count(); i++)
            {
                //If the location is in the list we're using anywhere then we need to load the attributes and get the thresholds 
                if(inUseClassrooms.Any(iuc => iuc.Classrooms.Any(c => c == TransformClassName(locations[i].Name))))
                {
                    locations[i].LoadAttributes();
                    var t = locations[i].AttributeValues["ThresholdHistoricalData"].Value;
                    if (!String.IsNullOrEmpty(t))
                    {
                        var pairs = t.Split(',');
                        var list = new Dictionary<DateTime, int>();
                        DateTime? mostRecentBeforeRange = null;
                        int mostRecentThreshold = 0; 
                        for(var k=0; k<pairs.Count(); k++)
                        {
                            var info = pairs[k].Split('^');
                            //If the date is within our timeframe add it to the data list
                            if(DateTime.Compare(start, DateTime.Parse(info[0])) <= 0 && DateTime.Compare(end, DateTime.Parse(info[0])) >= 0)
                            {
                                data = AddItem(data, locations[i], DateTime.Parse(info[0]), Int32.Parse(info[1]));
                                list.Add(DateTime.Parse(info[0]), Int32.Parse(info[1]));
                            }
                            if (DateTime.Compare(DateTime.Parse(info[0]), start) < 0)
                            {
                                if(mostRecentBeforeRange == null || DateTime.Compare(mostRecentBeforeRange.Value, DateTime.Parse(info[0])) < 0)
                                {
                                    mostRecentBeforeRange = DateTime.Parse(info[0]);
                                    mostRecentThreshold = Int32.Parse(info[1]);
                                }
                            }
                        }
                        if(!list.ContainsKey(start) && mostRecentBeforeRange.HasValue)
                        {
                            list.Add(mostRecentBeforeRange.Value, mostRecentThreshold);
                        }
                        else if (!list.ContainsKey(start))
                        {
                            list.Add(start, locations[i].SoftRoomThreshold.Value);
                        }

                        var startThreshold = list.OrderByDescending(l => l.Key).First(l => DateTime.Compare(l.Key, start) <= 0);
                        var idx = data.Select(d => d.ChangeDate).ToList().IndexOf(start);
                        if (idx < 0)
                        {
                            var item = new ThresholdList()
                            {
                                ChangeDate = start,
                                Thresholds = new Dictionary<string, int>()
                            };
                            item.Thresholds.Add(TransformClassName(locations[i].Name), startThreshold.Value);
                            data.Add(item);
                        }
                        else
                        {
                            if (!data[idx].Thresholds.ContainsKey(TransformClassName(locations[i].Name)))
                            {
                                data[idx].Thresholds.Add(TransformClassName(locations[i].Name), startThreshold.Value);
                            }
                        }
                    }
                    else
                    {
                        var idx = data.Select(d => d.ChangeDate).ToList().IndexOf(start);
                        if (idx < 0)
                        {
                            var item = new ThresholdList()
                            {
                                ChangeDate = start,
                                Thresholds = new Dictionary<string, int>()
                            };
                            item.Thresholds.Add(TransformClassName(locations[i].Name), locations[i].SoftRoomThreshold.Value);
                            data.Add(item);
                        }
                        else
                        {
                            if (!data[idx].Thresholds.ContainsKey(TransformClassName(locations[i].Name)))
                            {
                                data[idx].Thresholds.Add(TransformClassName(locations[i].Name), locations[i].SoftRoomThreshold.Value);
                            }
                        }
                    }
                }
            }
            data = data.OrderBy(x => x.ChangeDate).ToList();
            thresholds = new List<AttendanceReportData>(); 
            for(var i=0; i<data.Count(); i++)
            {
                var item = new AttendanceReportData()
                {
                    Title = "Threshold",
                    OccurrenceDate = data[i].ChangeDate,
                    ServiceAttendace = new List<ServiceAttendance>()
                };
                for(var k=0; k<inUseClassrooms.Count(); k++)
                {
                    var svcAtt = new ServiceAttendance()
                    {
                        OccurrenceDate = data[i].ChangeDate,
                        ServiceTime = schedules.FirstOrDefault(s => s.Id == inUseClassrooms[k].ScheduleId).Name,
                        ClassAttendances = new List<ClassAttendance>(),
                        Total = 0,
                        MultiAgeTotal = 0
                    };
                    for(var j=0; j<inUseClassrooms[k].Classrooms.Count(); j++)
                    {
                        var classAtt = new ClassAttendance()
                        {
                            OccurrenceDate = data[i].ChangeDate,
                            ClassName = inUseClassrooms[k].Classrooms[j],
                            ScheduleId = inUseClassrooms[k].ScheduleId
                        };
                        if (data[i].Thresholds.ContainsKey(inUseClassrooms[k].Classrooms[j]))
                        {
                            classAtt.AttendanceCount = data[i].Thresholds[inUseClassrooms[k].Classrooms[j]];
                        }
                        else
                        {
                            var h = 1;
                            while(i -h <= 0 && !data[i-h].Thresholds.ContainsKey(inUseClassrooms[k].Classrooms[j]))
                            {
                                h++;
                            }
                            classAtt.AttendanceCount = data[i-h].Thresholds[inUseClassrooms[k].Classrooms[j]];
                        }
                        svcAtt.ClassAttendances.Add(classAtt);
                    }
                    item.ServiceAttendace.Add(svcAtt);
                }
                thresholds.Add(item);
            }
        }

        private List<ThresholdList> AddItem(List<ThresholdList> data, Location location, DateTime changeDate, int threshold)
        {
            var idx = data.Select(d => d.ChangeDate).ToList().IndexOf(changeDate);
            if (idx < 0)
            {
                var item = new ThresholdList()
                {
                    ChangeDate = changeDate,
                    Thresholds = new Dictionary<string, int>()
                };
                item.Thresholds.Add(TransformClassName(location.Name), threshold);
                data.Add(item);
            }
            else
            {
                if (!data[idx].Thresholds.ContainsKey(TransformClassName(location.Name)))
                {
                    data[idx].Thresholds.Add(TransformClassName(location.Name), threshold);
                }
            }
            return data;
        }

        public int? GetThreshold(string values, DateTime occurrence)
        {
            var data = values.Split(',');
            var dict = new Dictionary<DateTime, int>();
            int? current = null;
            if(values != "")
            {
                foreach(var val in data)
                {
                    var info = val.Split('^');
                    var thresholdDate = DateTime.Parse(info[0]);
                    var threshold = Int32.Parse(info[1]);
                    dict.Add(thresholdDate, threshold);
                }
                var sorted = dict.OrderBy(d => d.Key);
                foreach(var item in sorted)
                {
                    if (DateTime.Compare(occurrence, item.Key) >= 0)
                    {
                        current = item.Value;
                    }
                }
            }
            return current;
        }

        //public void BuildGrid() {
        //    var div = new HtmlGenericControl("div");
        //    div.AddCssClass("grid");

        //    if (GetAttributeValue("PaneledGrid").AsBoolean())
        //    {
        //        div.AddCssClass("grid-panel");
        //    }

        //    phContent.Controls.Add(div);
        //    var grid = new Grid();
        //    div.Controls.Add(grid);
        //    grid.ID = "dynamic_data_0";
        //    grid.AllowSorting = false;
        //    grid.EmptyDataText = "No Results";
        //    grid.GridRebind += gReport_GridRebind;
        //    grid.custom-rowSelected += gReport_custom-rowSelected;

        //    AddGridColumns(grid);
        //    DataTable dt = new DataTable();
        //    dt = results;
        //    grid.DataSource = results;
        //    grid.DataBind();
        //    phContent.Visible = true;

        //}

        ///// <summary>
        ///// Handles the GridRebind event of the gReport control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //protected void gReport_GridRebind(object sender, EventArgs e)
        //{
        //    foreach (Control div in phContent.Controls)
        //    {
        //        foreach (var grid in div.Controls.OfType<Grid>())
        //        {
        //            grid.DataSource = results;
        //            grid.DataBind();
        //        }
        //    }
        //    upnlContent.Update();
        //}

        ///// <summary>
        ///// Handles the custom-rowSelected event of the gReport control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="custom-rowEventArgs"/> instance containing the event data.</param>
        //protected void gReport_custom-rowSelected(object sender, custom-rowEventArgs e)
        //{
        //    Grid grid = sender as Grid;
        //    string url = GetAttributeValue("UrlMask");
        //    if (grid != null && !string.IsNullOrWhiteSpace(url))
        //    {
        //        foreach (string key in grid.DataKeyNames)
        //        {
        //            url = url.Replace("{" + key + "}", grid.DataKeys[e.custom-rowIndex][key].ToString());
        //        }

        //        Response.Redirect(url, false);
        //    }
        //}
        ///// <summary>
        ///// Adds the grid columns.
        ///// </summary>
        ///// <param name="dataTable">The data table.</param>
        //private void AddGridColumns(Grid grid)
        //{
        //    grid.Columns.Clear();
        //    for(var i=0; i<results.Count(); i++)
        //    {
        //        BoundField bf = new BoundField();
        //        bf.DataField = results[i].Title;
        //        bf.HeaderText = results[i].Title;
        //        grid.Columns.Add(bf);
            //}
        //}

        #endregion

    }

    public class AttendanceReportData
    {
        public string Title { get; set;  }
        public DateTime OccurrenceDate { get; set; }
        public List<ServiceAttendance> ServiceAttendace { get; set; }
        public int Total { get; set; }
        public int UniqueTotal { get; set; }
        public int MultiAgeTotal { get; set; }
    }

    public class ClassAttendance
    {
        public string ClassName { get; set; }
        public int AttendanceCount { get; set; }
        public bool OverThreshold { get; set; }
        public int ScheduleId { get; set; }
        public DateTime OccurrenceDate { get; set; }
    }

    public class ServiceAttendance
    {
        public string ServiceTime { get; set; }
        public List<ClassAttendance> ClassAttendances { get; set; }
        public int Total { get; set; }
        public int? MultiAgeTotal { get; set; }
        public DateTime OccurrenceDate { get; set; }
    }

    public class ClassesBySchedule
    {
        public int ScheduleId { get; set; }
        public List<string> Classrooms { get; set; }
    }

    public class ThresholdList
    {
        public DateTime ChangeDate { get; set; }
        public Dictionary<string, int> Thresholds { get; set; }
    }
}