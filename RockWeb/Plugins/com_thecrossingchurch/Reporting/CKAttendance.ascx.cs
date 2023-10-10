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
    [DisplayName( "CK Attendance Report" )]
    [Category( "com_thecrossingchurch > CK Attendance" )]
    [Description( "Custom Attendance Report for Crossing Kids" )]

    [IntegerField( "Notes Page", "The page id of the notes page for the report.", true, 0, "", 0 )]
    [IntegerField( "Note Type Id", "The id of the note type used for the report.", true, 0, "", 1 )]

    public partial class CKAttendance : Rock.Web.UI.RockBlock //, ICustomGridColumns
    {
        #region Variables

        // Variables that get set with filter 
        private DateTime start;
        private DateTime end;
        private List<int> svcTimes;
        //Configuration Variables
        private int pageNum;
        private int noteTypeId;
        //Local Variables
        public string csv
        {
            get
            {
                if ( ViewState["csv"] != null )
                {
                    return ViewState["csv"].ToString();
                }
                else
                {
                    return "";
                }
            }
            set
            {
                ViewState["csv"] = value;
            }
        }
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

        protected void Page_Load( object sender, EventArgs e )
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent( this.Page );
            scriptManager.RegisterPostBackControl( this.btnExport );
            ScriptManager.RegisterStartupScript( Page, this.GetType(), "AKey", "notes();", true );
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
            if ( !stDate.SelectedDate.HasValue )
            {
                var dt = DateTime.Now.StartOfWeek( DayOfWeek.Sunday );
                stDate.SelectedDate = dt.AddDays( -21 );
            }
            if ( !endDate.SelectedDate.HasValue )
            {
                endDate.SelectedDate = DateTime.Now;
            }
            start = stDate.SelectedDate.Value;
            end = endDate.SelectedDate.Value;
            pageNum = GetAttributeValue( "NotesPage" ).AsInteger();
            noteTypeId = GetAttributeValue( "NoteTypeId" ).AsInteger();
            BindFilter();
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnFilter_Click( object sender, EventArgs e )
        {
            start = stDate.SelectedDate.Value;
            end = endDate.SelectedDate.Value;
            svcTimes = lbSchedules.SelectedValues.Select( x => Int32.Parse( x ) ).ToList();
            GetAttendance();
        }

        /// <summary>
        /// Handles the Click event of the btnExport control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnExport_Click( object sender, EventArgs e )
        {
            var excel = GenerateExcel();
            byte[] byteArray;
            using ( MemoryStream ms = new MemoryStream() )
            {
                excel.SaveAs( ms );
                byteArray = ms.ToArray();
            }
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.AddHeader( "content-disposition", "attachment;filename=AttendanceExport.xlsx" );
            Response.Cache.SetCacheability( HttpCacheability.Public );
            Response.Charset = "";
            //Response.Output.Write(csv);
            Response.BinaryWrite( byteArray );
            Response.Flush();
            Response.End();
        }

        protected void sdrpDates_SelectedDateRangeChanged( object sender, EventArgs e )
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

            lbSchedules.Required = GetAttributeValue( "RequireSchedule" ).AsBoolean( true );
            if ( lbSchedules.Required )
            {
                pnlSchedules.AddCssClass( "required" );
            }
            else
            {
                pnlSchedules.RemoveCssClass( "required" );
            }

            if ( dateStart.HasValue )
            {
                var area = GetAttributeValue( "CheckInArea" ).AsIntegerOrNull();

                using ( var rockContext = new RockContext() )
                {
                    var occQry = new AttendanceOccurrenceService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( o =>
                            o.OccurrenceDate >= dateStart &&
                            ( !dateEnd.HasValue || o.OccurrenceDate < dateEnd ) &&
                            o.Attendees.Any( a => a.DidAttend.HasValue && a.DidAttend.Value ) &&
                            o.Schedule != null
                        );

                    if ( area.HasValue )
                    {
                        var groupTypeIds = new GroupTypeService( rockContext )
                            .GetAllAssociatedDescendents( area.Value )
                            .Select( t => t.Id )
                            .ToList();

                        occQry = occQry
                            .Where( o =>
                                o.Group != null &&
                                groupTypeIds.Contains( o.Group.GroupTypeId ) );
                    }

                    var serviceTimes = occQry
                        .Select( o => o.Schedule )
                        .Distinct()
                        .ToList()
                        .OrderBy( s => s.StartTimeOfDay )
                        .Select( o => new
                        {
                            o.Id,
                            o.Name
                        } )
                        .ToList();

                    foreach ( var serviceTime in serviceTimes )
                    {
                        var item = new ListItem( serviceTime.Name, serviceTime.Id.ToString() );
                        item.Selected = selectedItems.Contains( serviceTime.Id );
                        lbSchedules.Items.Add( item );
                    }

                    if ( serviceTimes.Any() )
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
        private void GetAttendance()
        {
            var schedules = new ScheduleService( new RockContext() ).Queryable().Where( x => svcTimes.Contains( x.Id ) ).ToList().OrderBy( x => x.StartTimeOfDay );
            var locations = new LocationService( new RockContext() ).Queryable().Where( l => !String.IsNullOrEmpty( l.Name ) ).ToList(); //l.IsActive &&

            var attendance = new AttendanceService( new RockContext() ).Queryable().Where( x =>
                   DateTime.Compare( start, x.StartDateTime ) <= 0 &&
                   DateTime.Compare( end, x.StartDateTime ) >= 0 &&
                   svcTimes.Contains( x.Occurrence.ScheduleId.Value ) &&
                   ( x.Occurrence.Group.GroupTypeId == 129 || x.Occurrence.Group.GroupTypeId == 128 || x.Occurrence.Group.GroupTypeId == 122 ) && //LO, PS and Elem Areas Only
                   x.DidAttend != false
            );
            inUseClassrooms = new List<ClassesBySchedule>();
            for ( var i = 0; i < svcTimes.Count(); i++ )
            {
                var svcId = svcTimes[i];
                var current = attendance.Where( x => x.Occurrence.ScheduleId == svcId ).Select( x => x.Occurrence.Location.Name ).ToList().Select( x => TransformClassName( x ) ).Distinct().OrderBy( x =>
                {
                    var name = x.Contains( "11:15" ) ? x.Substring( 6 ) : x.Substring( 5 );
                    return classroomSort.IndexOf( name );
                } ).ToList();
                inUseClassrooms.Add( new ClassesBySchedule { ScheduleId = svcId, Classrooms = current } );
            }
            BuildThresholdData( locations, schedules.ToList() );

            //Early Childhood, Multiage doesn't exist
            var early_childhood = attendance.Where( x => x.Occurrence.Group.GroupTypeId == 128 || x.Occurrence.Group.GroupTypeId == 122 ).ToList().GroupBy( x => new { x.Occurrence.OccurrenceDate, x.Occurrence.ScheduleId, x.Occurrence.LocationId } ).Select( x =>
            {
                var location = locations.FirstOrDefault( l => l.Id == x.Key.LocationId );
                location.LoadAttributes();
                var threshold = GetThreshold( location.AttributeValues["ThresholdHistoricalData"].Value.ToString(), x.Key.OccurrenceDate );
                var ca = new ClassAttendance()
                {
                    ClassName = TransformClassName( location.Name ),
                    ScheduleId = x.Key.ScheduleId.Value,
                    AttendanceCount = x.Count(),
                    OccurrenceDate = x.Key.OccurrenceDate
                };
                if ( threshold.HasValue && ca.AttendanceCount >= threshold )
                {
                    ca.OverThreshold = true;
                }
                else if ( !threshold.HasValue && ca.AttendanceCount >= location.SoftRoomThreshold )
                {
                    ca.OverThreshold = true;
                }
                return ca;
            } ).Distinct().ToList();
            var ecSvcAtt = early_childhood.GroupBy( x => new { x.OccurrenceDate, x.ScheduleId } ).Select( x =>
               {
                   var svcAtt = new ServiceAttendance()
                   {
                       ServiceTime = schedules.FirstOrDefault( s => s.Id == x.Key.ScheduleId ).Name,
                       OccurrenceDate = x.Key.OccurrenceDate,
                       ClassAttendances = early_childhood.Where( ec => ec.OccurrenceDate == x.Key.OccurrenceDate && ec.ScheduleId == x.Key.ScheduleId ).OrderBy( c =>
                       {
                           var name = c.ClassName.Contains( "11:15" ) ? c.ClassName.Substring( 6 ) : c.ClassName.Substring( 5 );
                           return classroomSort.IndexOf( name );
                       } ).ToList()
                   };
                   svcAtt.Total = svcAtt.ClassAttendances.Select( ca => ca.AttendanceCount ).Sum();
                   return svcAtt;
               } );

            //Elementary, Multiage exists
            var elem = attendance.Where( x => x.Occurrence.Group.GroupTypeId == 129 ).ToList().Select( e =>
            {
                //Check if is first for person on day
                var otherAttendancesOnDay = attendance.Where(a => a.PersonAliasId == e.PersonAliasId && a.Occurrence.OccurrenceDate == e.Occurrence.OccurrenceDate ).ToList().OrderBy(a => a.Occurrence.Schedule.StartTimeOfDay);
                List<int> idsInOrder = otherAttendancesOnDay.Select( a => a.Id ).ToList();
                int currentAttIdx = idsInOrder.IndexOf( e.Id );
                if(currentAttIdx == 0 )
                {
                    return e;
                }
                //var scheduleStart = new DateTime( e.StartDateTime.Year, e.StartDateTime.Month, e.StartDateTime.Day, 0, 0, 0 ).Add( e.Occurrence.Schedule.StartTimeOfDay );
                //var ts = scheduleStart - e.StartDateTime;
                //if ( ts.TotalMinutes <= 45 )
                //{
                //    return e;
                //}
                return new Attendance() { Id = -1 };
            } ).Where( e => e.Id > 0 ).GroupBy( x => new { x.Occurrence.OccurrenceDate, x.Occurrence.ScheduleId, x.Occurrence.LocationId } ).Select( x =>
            {
                var location = locations.FirstOrDefault( l => l.Id == x.Key.LocationId );
                location.LoadAttributes();
                var threshold = GetThreshold( location.AttributeValues["ThresholdHistoricalData"].Value.ToString(), x.Key.OccurrenceDate );
                var ca = new ClassAttendance()
                {
                    ClassName = TransformClassName( location.Name ),
                    ScheduleId = x.Key.ScheduleId.Value,
                    AttendanceCount = x.Count(),
                    OccurrenceDate = x.Key.OccurrenceDate
                };
                if ( threshold.HasValue && ca.AttendanceCount >= threshold )
                {
                    ca.OverThreshold = true;
                }
                else if ( !threshold.HasValue && ca.AttendanceCount >= location.SoftRoomThreshold )
                {
                    ca.OverThreshold = true;
                }
                return ca;
            } ).Distinct().ToList();
            var elemMultiAge = attendance.Where( x => x.Occurrence.Group.GroupTypeId == 129 ).ToList().Select( e =>
            {
                //Check if is first for person on day
                var otherAttendancesOnDay = attendance.Where( a => a.PersonAliasId == e.PersonAliasId && a.Occurrence.OccurrenceDate == e.Occurrence.OccurrenceDate ).ToList().OrderBy( a => a.Occurrence.Schedule.StartTimeOfDay );
                List<int> idsInOrder = otherAttendancesOnDay.Select( a => a.Id ).ToList();
                int currentAttIdx = idsInOrder.IndexOf( e.Id );
                if( currentAttIdx > 0 )
                {
                    var isK2 = false;
                    if (e.Occurrence.Group.Name.Contains( "Kindergarten" ) || e.Occurrence.Group.Name.Contains( "1st" ) || e.Occurrence.Group.Name.Contains( "2nd" ))
                    {
                        isK2 = true;
                    }
                    return new { isValid = true, att = e, isK2 = isK2 };
                }
                //var scheduleStart = new DateTime( e.StartDateTime.Year, e.StartDateTime.Month, e.StartDateTime.Day, 0, 0, 0 ).Add( e.Occurrence.Schedule.StartTimeOfDay );
                //var ts = scheduleStart - e.StartDateTime;
                //if ( ts.TotalMinutes > 45 )
                //{
                //    var isK2 = false;
                //    if ( e.Occurrence.Group.Name.Contains( "Kindergarten" ) || e.Occurrence.Group.Name.Contains( "1st" ) || e.Occurrence.Group.Name.Contains( "2nd" ) )
                //    {
                //        isK2 = true;
                //    }
                //    return new { isValid = true, att = e, isK2 = isK2 };
                //}
                return new { isValid = false, att = new Attendance() { }, isK2 = false };
            } ).Where( e => e.isValid == true ).GroupBy( x => new { x.isK2, x.att.Occurrence.OccurrenceDate, x.att.Occurrence.ScheduleId } ).Select( x =>
            {
                var idx = inUseClassrooms.Select( iuc => iuc.ScheduleId ).ToList().IndexOf( x.Key.ScheduleId.Value );
                var className = x.Key.isK2 ? "K-2 Multi-Age" : "3-5 Multi-Age";
                if ( inUseClassrooms[idx].Classrooms.IndexOf( className ) < 0 )
                {
                    inUseClassrooms[idx].Classrooms.Add( className );
                    inUseClassrooms[idx].Classrooms = inUseClassrooms[idx].Classrooms.OrderBy( iuc =>
                    {
                        var name = iuc.Contains( ":" ) ? ( iuc.Contains( "11:15" ) ? iuc.Substring( 6 ) : iuc.Substring( 5 ) ) : iuc;
                        return classroomSort.IndexOf( name );
                    } ).ToList();
                }
                return new ClassAttendance()
                {
                    ClassName = x.Key.isK2 ? "K-2 Multi-Age" : "3-5 Multi-Age",
                    ScheduleId = x.Key.ScheduleId.Value,
                    AttendanceCount = x.Count(),
                    OccurrenceDate = x.Key.OccurrenceDate
                };
            } ).OrderBy( x =>
            {
                var name = x.ClassName.Contains( "11:15" ) ? x.ClassName.Substring( 6 ) : x.ClassName.Substring( 5 );
                return classroomSort.IndexOf( name );
            } ).Distinct().ToList();

            var elemSvcAtt = elem.Union( elemMultiAge ).GroupBy( x => new { x.OccurrenceDate, x.ScheduleId } ).Select( x =>
                 {
                     var svcAtt = new ServiceAttendance()
                     {
                         ServiceTime = schedules.FirstOrDefault( s => s.Id == x.Key.ScheduleId ).Name,
                         OccurrenceDate = x.Key.OccurrenceDate,
                         ClassAttendances = elem.Where( ec => ec.OccurrenceDate == x.Key.OccurrenceDate && ec.ScheduleId == x.Key.ScheduleId ).Union( elemMultiAge.Where( em => em.ScheduleId == x.Key.ScheduleId && em.OccurrenceDate == x.Key.OccurrenceDate ) ).OrderBy( c =>
                         {
                             var name = c.ClassName.Contains( "11:15" ) ? c.ClassName.Substring( 6 ) : c.ClassName.Substring( 5 );
                             return classroomSort.IndexOf( name );
                         } ).ToList()
                     };
                     svcAtt.Total = svcAtt.ClassAttendances.Select( ca => ca.AttendanceCount ).Sum();
                     svcAtt.MultiAgeTotal = elemMultiAge.Where( ma => ma.OccurrenceDate == x.Key.OccurrenceDate && ma.ScheduleId == x.Key.ScheduleId ).Select( ma => ma.AttendanceCount ).Sum();
                     return svcAtt;
                 } );

            var list = from ec in ecSvcAtt
                       join el in elemSvcAtt
                       on new { ec.ServiceTime, ec.OccurrenceDate } equals new { el.ServiceTime, el.OccurrenceDate }
                       select new ServiceAttendance() { ServiceTime = ec.ServiceTime, ClassAttendances = ec.ClassAttendances.Union( el.ClassAttendances ).ToList(), OccurrenceDate = ec.OccurrenceDate, Total = ec.Total + el.Total, MultiAgeTotal = el.MultiAgeTotal };

            results = list.GroupBy( x => new { x.OccurrenceDate } ).Select( x =>
               {
                   var data = new AttendanceReportData()
                   {
                       Title = x.Key.OccurrenceDate.ToString( "MM/dd/yy" ),
                       OccurrenceDate = x.Key.OccurrenceDate,
                       ServiceAttendace = list.Where( l => l.OccurrenceDate == x.Key.OccurrenceDate ).OrderBy( l => schedules.Select( s => s.Name ).ToList().IndexOf( l.ServiceTime ) ).ToList()
                   };
                   data.Total = data.ServiceAttendace.Select( s => s.Total ).Sum();
                   data.UniqueTotal = attendance.Where( a => a.Occurrence.OccurrenceDate == x.Key.OccurrenceDate ).Select( a => a.PersonAlias.PersonId ).Distinct().Count();
                   data.MultiAgeTotal = elemMultiAge.Where( em => em.OccurrenceDate == x.Key.OccurrenceDate ).Select( em => em.AttendanceCount ).Sum();
                   return data;
               } ).ToList();

            //Add in threshold data where it needs to go
            results = results.Union( thresholds ).OrderByDescending( x => x.Title ).OrderBy( x => x.OccurrenceDate ).ToList();

            BuildControl();
        }

        public void BuildControl()
        {
            var div = new HtmlGenericControl( "div" );
            div.AddCssClass( "custom-row" );
            var header = new HtmlGenericControl( "div" );
            header.InnerText = "Classroom";
            header.AddCssClass( "custom-col name-col" );
            div.Controls.Add( header );
            phContent.Controls.Add( div );
            for ( var i = 0; i < results.Count(); i++ )
            {
                var h = new HtmlGenericControl( "div" );
                h.InnerText = results[i].Title;
                h.AddCssClass( "custom-col" );
                if ( i == 0 )
                {
                    h.AddCssClass( "first-custom-col" );
                }
                div.Controls.Add( h );
            }
            for ( var i = 0; i < results[0].ServiceAttendace.Count(); i++ )
            {
                var r = new HtmlGenericControl( "div" );
                r.AddCssClass( "service-group" );
                var svcTime = new HtmlGenericControl( "div" );
                svcTime.AddCssClass( "custom-row service-time" );
                var svcTimeCol = new HtmlGenericControl( "div" );
                svcTimeCol.AddCssClass( "custom-col name-col" );
                svcTimeCol.InnerText = results[0].ServiceAttendace[i].ServiceTime;
                svcTime.Controls.Add( svcTimeCol );
                r.Controls.Add( svcTime );
                var idx = inUseClassrooms.Select( x => x.ScheduleId ).ToList().IndexOf( results[0].ServiceAttendace[i].ClassAttendances[0].ScheduleId );
                for ( var j = 0; j < inUseClassrooms[idx].Classrooms.Count(); j++ )
                {
                    var classroom = new HtmlGenericControl( "div" );
                    classroom.AddCssClass( "custom-row" );
                    var classCol = new HtmlGenericControl( "div" );
                    classCol.AddCssClass( "custom-col name-col" );
                    classCol.InnerText = inUseClassrooms[idx].Classrooms[j].Contains( ":" ) ? ( inUseClassrooms[idx].Classrooms[j].Contains( "11:15" ) ? inUseClassrooms[idx].Classrooms[j].Substring( 6 ) : inUseClassrooms[idx].Classrooms[j].Substring( 5 ) ) : inUseClassrooms[idx].Classrooms[j];
                    if ( j % 2 > 0 )
                    {
                        classroom.AddCssClass( "bg-secondary" );
                        classCol.AddCssClass( "bg-secondary" );
                    }
                    classroom.Controls.Add( classCol );

                    //Add Attendance Numbers
                    for ( var k = 0; k < results.Count(); k++ )
                    {
                        var att = new HtmlGenericControl( "div" );
                        att.AddCssClass( "custom-col" );
                        var temp = results[k].ServiceAttendace.Count() > i ? results[k].ServiceAttendace[i].ClassAttendances.FirstOrDefault( c => c.ClassName == inUseClassrooms[idx].Classrooms[j] ) : null;
                        att.InnerText = temp != null ? temp.AttendanceCount.ToString() : "";
                        if ( temp != null && temp.OverThreshold )
                        {
                            att.AddCssClass( "over-threshold" );
                        }
                        if ( k == 0 )
                        {
                            att.AddCssClass( "first-custom-col" );
                        }
                        classroom.Controls.Add( att );
                    }
                    r.Controls.Add( classroom );
                }
                var total = new HtmlGenericControl( "div" );
                total.AddCssClass( "custom-row" );
                var totalCol = new HtmlGenericControl( "div" );
                totalCol.AddCssClass( "custom-col name-col" );
                totalCol.InnerText = "Total";
                total.Controls.Add( totalCol );

                //Add total attendance
                for ( var k = 0; k < results.Count(); k++ )
                {
                    var att = new HtmlGenericControl( "div" );
                    att.AddCssClass( "custom-col" );
                    att.InnerText = results[k].ServiceAttendace.Count() > i ? results[k].ServiceAttendace[i].Total.ToString() : "";
                    if ( k == 0 )
                    {
                        att.AddCssClass( "first-custom-col" );
                    }
                    total.Controls.Add( att );
                }
                r.Controls.Add( total );
                if ( results[0].ServiceAttendace[i].MultiAgeTotal != null )
                {
                    var mtotal = new HtmlGenericControl( "div" );
                    mtotal.AddCssClass( "custom-row" );
                    var mTotalCol = new HtmlGenericControl( "div" );
                    mTotalCol.AddCssClass( "custom-col name-col" );
                    mTotalCol.InnerText = "Multi-Age Total";
                    mtotal.Controls.Add( mTotalCol );

                    //Add total attendance
                    for ( var k = 0; k < results.Count(); k++ )
                    {
                        var att = new HtmlGenericControl( "div" );
                        att.AddCssClass( "custom-col" );
                        att.InnerText = results[k].ServiceAttendace.Count() > i ? results[k].ServiceAttendace[i].MultiAgeTotal.ToString() : "";
                        if ( k == 0 )
                        {
                            att.AddCssClass( "first-custom-col" );
                        }
                        mtotal.Controls.Add( att );
                    }
                    r.Controls.Add( mtotal );
                }
                phContent.Controls.Add( r );
            }

            //Entire day data
            var dailyData = new HtmlGenericControl( "div" );
            dailyData.AddCssClass( "custom-seperator" );
            var totals = new HtmlGenericControl( "div" );
            totals.AddCssClass( "custom-row" );
            var totalsCol = new HtmlGenericControl( "div" );
            totalsCol.InnerText = "Total";
            totalsCol.AddCssClass( "custom-col service-time name-col" );
            totals.Controls.Add( totalsCol );
            var utotals = new HtmlGenericControl( "div" );
            utotals.AddCssClass( "custom-row" );
            var utotalsCol = new HtmlGenericControl( "div" );
            utotalsCol.InnerText = "Unique Total";
            utotalsCol.AddCssClass( "custom-col service-time name-col" );
            utotals.Controls.Add( utotalsCol );
            var matotals = new HtmlGenericControl( "div" );
            matotals.AddCssClass( "custom-row" );
            var matotalsCol = new HtmlGenericControl( "div" );
            matotalsCol.InnerText = "MultiAge Total";
            matotalsCol.AddCssClass( "custom-col service-time name-col" );
            matotals.Controls.Add( matotalsCol );
            var notes = new HtmlGenericControl( "div" );
            notes.AddCssClass( "custom-row" );
            var notesCol = new HtmlGenericControl( "div" );
            notesCol.InnerText = "Notes";
            notesCol.AddCssClass( "cusotm-col service-time name-col" );
            notes.Controls.Add( notesCol );
            var noteDisplay = new HtmlGenericControl( "div" );
            noteDisplay.AddCssClass( "custom-row" );
            var noteDisplayCol = new HtmlGenericControl( "div" );
            noteDisplayCol.InnerText = "Notes";
            noteDisplayCol.AddCssClass( "custom-col no-display" );
            noteDisplay.Controls.Add( noteDisplayCol );
            for ( var i = 0; i < results.Count(); i++ )
            {
                //Total
                var h = new HtmlGenericControl( "div" );
                h.InnerText = results[i].Total.ToString();
                h.AddCssClass( "custom-col" );
                if ( i == 0 )
                {
                    h.AddCssClass( "first-custom-col" );
                }
                totals.Controls.Add( h );
                //Unique Total
                var ut = new HtmlGenericControl( "div" );
                ut.InnerText = results[i].UniqueTotal.ToString();
                ut.AddCssClass( "custom-col" );
                if ( i == 0 )
                {
                    ut.AddCssClass( "first-custom-col" );
                }
                utotals.Controls.Add( ut );
                //Multiage total
                var mt = new HtmlGenericControl( "div" );
                mt.InnerText = results[i].MultiAgeTotal.ToString();
                mt.AddCssClass( "custom-col" );
                if ( i == 0 )
                {
                    mt.AddCssClass( "first-custom-col" );
                }
                matotals.Controls.Add( mt );
                //Notes
                var nt = new HtmlGenericControl( "div" );
                var attNote = new HtmlGenericControl( "div" );
                attNote.AddCssClass( "custom-col no-display" );
                if ( i > 0 )
                {
                    var schedule_id = results[i].ServiceAttendace[0].ClassAttendances[0].ScheduleId;
                    var occ_date = results[i].OccurrenceDate;
                    var occurence = new AttendanceOccurrenceService( new RockContext() ).Queryable().Where( ao => ao.OccurrenceDate == occ_date && ao.ScheduleId == schedule_id );
                    if ( occurence.Count() > 0 )
                    {
                        nt.InnerHtml = "<a class='add-note' href='/page/" + pageNum + "?Id=" + occurence.First().Id + "' ><i class='fa fa-sticky-note'></i></a>";
                        int occId = occurence.First().Id;
                        var attendanceNotes = new NoteService( new RockContext() ).Queryable().Where( n => n.NoteTypeId == noteTypeId && n.EntityId == occId );
                        var str = "";
                        foreach ( var an in attendanceNotes )
                        {
                            str += an.Text + " -" + an.CreatedByPersonName + "<br/>";
                        }
                        attNote.InnerHtml = str;
                    }
                }
                nt.AddCssClass( "custom-col" );
                if ( i == 0 )
                {
                    nt.AddCssClass( "first-custom-col" );
                }
                notes.Controls.Add( nt );
                noteDisplay.Controls.Add( attNote );
            }
            dailyData.Controls.Add( totals );
            dailyData.Controls.Add( utotals );
            dailyData.Controls.Add( matotals );
            dailyData.Controls.Add( notes );
            dailyData.Controls.Add( noteDisplay );

            phContent.Controls.Add( dailyData );

            var html = "";
            foreach ( HtmlGenericControl c in phContent.Controls )
            {
                System.IO.TextWriter tw = new System.IO.StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter( tw );
                c.RenderControl( htw );
                html += tw.ToString();
            }
            html = html.Replace( "<div class=\"custom-row\">", "\r\n" );
            html = html.Replace( "</div>", "," );
            html = html.Replace( "<div class=\"custom-col\">", "" );
            html = html.Replace( "<div class=\"custom-col name-col\">", "" );
            html = html.Replace( "<div class=\"custom-col first-custom-col\">", "" );
            html = html.Replace( "<div class=\"custom-row bg-secondary\">", "\r\n" );
            html = html.Replace( "<div class=\"service-group\">", "\r\n" );
            html = html.Replace( "<div class=\"custom-row service-time\">", "" );
            html = html.Replace( "<div class=\"custom-col name-col bg-secondary\">", "" );
            html = html.Replace( "<div class=\"custom-seperator\">", "" );
            html = html.Replace( "<div class=\"custom-col service-time name-col\">", "" );
            html = html.Replace( "<div class=\"custom-col over-threshold\">", "**" );
            html = html.Replace( "<div class=\"custom-col no-display\">", "" );
            html = html.Replace( "<a class='add-note' href='/page/945?Id=", "" );
            html = html.Replace( "' ><i class='fa fa-sticky-note'></i></a>", "" );
            html = html.Replace( "<div class=\"cusotm-col service-time name-col\">", "" );
            this.csv = html;

            phContent.Visible = true;
        }

        public string TransformClassName( string name )
        {
            try
            {
                if ( !name.Contains( "(" ) && !name.Contains( ":" ) )
                {
                    return name;
                }
                if ( name.Contains( "(" ) )
                {
                    if ( name.Contains( "11:15" ) )
                    {
                        var idx = name.IndexOf( "(" );
                        var diff = name.Length - idx + 7;
                        return name.Substring( 0, name.Length - diff );
                        //return name.Substring(6, name.Length - diff);
                    }
                    else
                    {
                        var idx = name.IndexOf( "(" );
                        var diff = name.Length - idx + 6;
                        return name.Substring( 0, name.Length - diff );
                        //return name.Substring(5, name.Length - diff);
                    }
                }
                else
                {
                    return name;
                    //if (name.Contains("11:15"))
                    //{
                    //    return name.Substring(6);
                    //}
                    //else
                    //{
                    //    return name.Substring(5);
                    //}
                }
            }
            catch
            {
                return name;
            }

        }

        public void BuildThresholdData( List<Location> locations, List<Schedule> schedules )
        {
            var data = new List<ThresholdList>();
            for ( var i = 0; i < locations.Count(); i++ )
            {
                //If the location is in the list we're using anywhere then we need to load the attributes and get the thresholds 
                if ( inUseClassrooms.Any( iuc => iuc.Classrooms.Any( c => c == TransformClassName( locations[i].Name ) ) ) )
                {
                    locations[i].LoadAttributes();
                    var t = locations[i].AttributeValues["ThresholdHistoricalData"].Value;
                    if ( !String.IsNullOrEmpty( t ) )
                    {
                        var pairs = t.Split( '|' );
                        var list = new Dictionary<DateTime, int>();
                        DateTime? mostRecentBeforeRange = null;
                        int mostRecentThreshold = 0;
                        for ( var k = 0; k < pairs.Count(); k++ )
                        {
                            var info = pairs[k].Split( '^' );
                            //If the date is within our timeframe add it to the data list
                            if ( DateTime.Compare( start, DateTime.Parse( info[0] ) ) <= 0 && DateTime.Compare( end, DateTime.Parse( info[0] ) ) >= 0 )
                            {
                                data = AddItem( data, locations[i], DateTime.Parse( info[0] ), Int32.Parse( info[1] ) );
                                list.Add( DateTime.Parse( info[0] ), Int32.Parse( info[1] ) );
                            }
                            if ( DateTime.Compare( DateTime.Parse( info[0] ), start ) < 0 )
                            {
                                if ( mostRecentBeforeRange == null || DateTime.Compare( mostRecentBeforeRange.Value, DateTime.Parse( info[0] ) ) < 0 )
                                {
                                    mostRecentBeforeRange = DateTime.Parse( info[0] );
                                    mostRecentThreshold = Int32.Parse( info[1] );
                                }
                            }
                        }
                        if ( !list.ContainsKey( start ) && mostRecentBeforeRange.HasValue )
                        {
                            list.Add( mostRecentBeforeRange.Value, mostRecentThreshold );
                        }
                        else if ( !list.ContainsKey( start ) )
                        {
                            list.Add( start, locations[i].SoftRoomThreshold.Value );
                        }

                        var startThreshold = list.OrderByDescending( l => l.Key ).First( l => DateTime.Compare( l.Key, start ) <= 0 );
                        var idx = data.Select( d => d.ChangeDate ).ToList().IndexOf( start );
                        if ( idx < 0 )
                        {
                            var item = new ThresholdList()
                            {
                                ChangeDate = start,
                                Thresholds = new Dictionary<string, int>()
                            };
                            item.Thresholds.Add( TransformClassName( locations[i].Name ), startThreshold.Value );
                            data.Add( item );
                        }
                        else
                        {
                            if ( !data[idx].Thresholds.ContainsKey( TransformClassName( locations[i].Name ) ) )
                            {
                                data[idx].Thresholds.Add( TransformClassName( locations[i].Name ), startThreshold.Value );
                            }
                        }
                    }
                    else
                    {
                        var idx = data.Select( d => d.ChangeDate ).ToList().IndexOf( start );
                        if ( idx < 0 )
                        {
                            var item = new ThresholdList()
                            {
                                ChangeDate = start,
                                Thresholds = new Dictionary<string, int>()
                            };
                            item.Thresholds.Add( TransformClassName( locations[i].Name ), locations[i].SoftRoomThreshold.Value );
                            data.Add( item );
                        }
                        else
                        {
                            if ( !data[idx].Thresholds.ContainsKey( TransformClassName( locations[i].Name ) ) )
                            {
                                data[idx].Thresholds.Add( TransformClassName( locations[i].Name ), locations[i].SoftRoomThreshold.Value );
                            }
                        }
                    }
                }
            }
            data = data.OrderBy( x => x.ChangeDate ).ToList();
            thresholds = new List<AttendanceReportData>();
            for ( var i = 0; i < data.Count(); i++ )
            {
                var item = new AttendanceReportData()
                {
                    Title = "Threshold",
                    OccurrenceDate = data[i].ChangeDate,
                    ServiceAttendace = new List<ServiceAttendance>()
                };
                for ( var k = 0; k < inUseClassrooms.Count(); k++ )
                {
                    var svcAtt = new ServiceAttendance()
                    {
                        OccurrenceDate = data[i].ChangeDate,
                        ServiceTime = schedules.FirstOrDefault( s => s.Id == inUseClassrooms[k].ScheduleId ).Name,
                        ClassAttendances = new List<ClassAttendance>(),
                        Total = 0,
                        MultiAgeTotal = 0
                    };
                    for ( var j = 0; j < inUseClassrooms[k].Classrooms.Count(); j++ )
                    {
                        var classAtt = new ClassAttendance()
                        {
                            OccurrenceDate = data[i].ChangeDate,
                            ClassName = inUseClassrooms[k].Classrooms[j],
                            ScheduleId = inUseClassrooms[k].ScheduleId
                        };
                        if ( data[i].Thresholds.ContainsKey( inUseClassrooms[k].Classrooms[j] ) )
                        {
                            classAtt.AttendanceCount = data[i].Thresholds[inUseClassrooms[k].Classrooms[j]];
                        }
                        else
                        {
                            // Check if other dataset has threshold we seek 
                            var h = 0;
                            while ( h < data.Count() && !data[h].Thresholds.ContainsKey( inUseClassrooms[k].Classrooms[j] ) )
                            {
                                h++;
                            }
                            if ( data[h].Thresholds.ContainsKey( inUseClassrooms[k].Classrooms[j] ) )
                            {
                                classAtt.AttendanceCount = data[h].Thresholds[inUseClassrooms[k].Classrooms[j]];
                            }
                            else
                            {
                                //Default to 22 since a threshold could not be found
                                classAtt.AttendanceCount = 22;
                            }
                        }
                        svcAtt.ClassAttendances.Add( classAtt );
                    }
                    item.ServiceAttendace.Add( svcAtt );
                }
                thresholds.Add( item );
            }
        }

        private List<ThresholdList> AddItem( List<ThresholdList> data, Location location, DateTime changeDate, int threshold )
        {
            var idx = data.Select( d => d.ChangeDate ).ToList().IndexOf( changeDate );
            if ( idx < 0 )
            {
                var item = new ThresholdList()
                {
                    ChangeDate = changeDate,
                    Thresholds = new Dictionary<string, int>()
                };
                item.Thresholds.Add( TransformClassName( location.Name ), threshold );
                data.Add( item );
            }
            else
            {
                if ( !data[idx].Thresholds.ContainsKey( TransformClassName( location.Name ) ) )
                {
                    data[idx].Thresholds.Add( TransformClassName( location.Name ), threshold );
                }
            }
            return data;
        }

        public int? GetThreshold( string values, DateTime occurrence )
        {
            var data = values.Split( '|' );
            var dict = new Dictionary<DateTime, int>();
            int? current = null;
            if ( values != "" )
            {
                foreach ( var val in data )
                {
                    var info = val.Split( '^' );
                    var thresholdDate = DateTime.Parse( info[0] );
                    var threshold = Int32.Parse( info[1] );
                    dict.Add( thresholdDate, threshold );
                }
                var sorted = dict.OrderBy( d => d.Key );
                foreach ( var item in sorted )
                {
                    if ( DateTime.Compare( occurrence, item.Key ) >= 0 )
                    {
                        current = item.Value;
                    }
                }
            }
            return current;
        }

        public void GenerateCSV()
        {
            var html = "";
            foreach ( HtmlGenericControl c in phContent.Controls )
            {
                System.IO.TextWriter tw = new System.IO.StringWriter();
                HtmlTextWriter htw = new HtmlTextWriter( tw );
                c.RenderControl( htw );
                html += tw.ToString();
            }
            html = html.Replace( "<div class=\"custom-row\">", "\r\n" );
            html = html.Replace( "</div>", "," );
            html = html.Replace( "<div class=\"custom-col\">", "" );
            html = html.Replace( "<div class=\"custom-col name-col\">", "" );
            html = html.Replace( "<div class=\"custom-col first-custom-col\">", "" );
            html = html.Replace( "<div class=\"custom-row bg-secondary\">", "\r\n" );
            html = html.Replace( "<div class=\"service-group\">", "\r\n" );
            html = html.Replace( "<div class=\"custom-row service-time\">", "" );
            html = html.Replace( "<div class=\"custom-col name-col bg-secondary\">", "" );
            html = html.Replace( "<div class=\"custom-seperator\">", "" );
            html = html.Replace( "<div class=\"custom-col service-time name-col\">", "" );
            html = html.Replace( "<div class=\"custom-col over-threshold\">", "" );

            this.csv = html;
        }

        public ExcelPackage GenerateExcel()
        {
            ExcelPackage excel = new ExcelPackage();
            excel.Workbook.Properties.Title = "CK Attendance";
            // add author info
            Rock.Model.UserLogin userLogin = Rock.Model.UserLoginService.GetCurrentUser();
            if ( userLogin != null )
            {
                excel.Workbook.Properties.Author = userLogin.Person.FullName;
            }
            else
            {
                excel.Workbook.Properties.Author = "Rock";
            }
            ExcelWorksheet worksheet = excel.Workbook.Worksheets.Add( "Attendance" );
            worksheet.PrinterSettings.LeftMargin = .5m;
            worksheet.PrinterSettings.RightMargin = .5m;
            worksheet.PrinterSettings.TopMargin = .5m;
            worksheet.PrinterSettings.BottomMargin = .5m;

            var raw_data = csv.Replace( "\r\n", "\n" );
            var row_data = raw_data.Split( '\n' );

            for ( var i = 1; i <= row_data.Length; i++ )
            {
                if ( row_data[i - 1].Contains( ',' ) && i != row_data.Length - 1 )
                {
                    var col_data = row_data[i - 1].Split( ',' );
                    for ( var j = 1; j <= col_data.Length; j++ )
                    {
                        if ( col_data[j - 1].Length > 2 && col_data[j - 1].Substring( 0, 2 ) == "**" )
                        {
                            col_data[j - 1] = col_data[j - 1].Substring( 2 );
                            Color c = System.Drawing.ColorTranslator.FromHtml( "#9A0000" );
                            worksheet.Cells[i, j].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[i, j].Style.Fill.BackgroundColor.SetColor( c );
                        }
                        if ( col_data[j - 1].Length > 2 && col_data[j - 1].Contains( "<br/>" ) )
                        {
                            col_data[j - 1] = col_data[j - 1].Replace( "<br/>", "\r\n" );
                            worksheet.Cells[i, j].Style.WrapText = true;
                        }
                        worksheet.Cells[i, j].Value = col_data[j - 1];

                    }
                }
            }
            return excel;
        }

        #endregion

    }

    public class AttendanceReportData
    {
        public string Title { get; set; }
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