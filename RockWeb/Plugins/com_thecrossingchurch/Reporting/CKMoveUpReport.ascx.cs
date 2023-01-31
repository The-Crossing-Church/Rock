using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Z.EntityFramework.Plus;
using com._9embers.FieldTypes;
using Newtonsoft.Json;

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting
{
    [DisplayName( "CK Move-Up Report" )]
    [Category( "com_thecrossingchurch > Reporting" )]
    [Description( "Report to let you see the breakdown of attendance for people by schedule" )]
    [SchedulesField( "Default Schedules", key: AttributeKey.DefaultSchedules )]
    public partial class CKMoveUpReport : Rock.Web.UI.RockBlock
    {
        #region Keys
        private static class AttributeKey
        {
            public const string DefaultSchedules = "DefaultSchedules";
        }
        #endregion

        #region Variables
        private RockContext _context { get; set; }
        private PersonAliasService _paSvc { get; set; }
        private AttendanceService _attSvc { get; set; }
        private ScheduleService _schSvc { get; set; }
        private List<int> scheduleIds { get; set; }
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
            _context = new RockContext();
            _paSvc = new PersonAliasService( _context );
            _attSvc = new AttendanceService( _context );
            _schSvc = new ScheduleService( _context );

            if ( !Page.IsPostBack )
            {
                var defaultSchedules = GetAttributeValue( AttributeKey.DefaultSchedules );
                if ( !String.IsNullOrEmpty( defaultSchedules ) )
                {
                    var guids = defaultSchedules.Split( ',' ).ToList().Select( v => Guid.Parse( v ) ).ToList();
                    List<int> ids = new List<int>();
                    for ( int i = 0; i < guids.Count(); i++ )
                    {
                        Schedule s = _schSvc.Get( guids[i] );
                        if ( s != null )
                        {
                            ids.Add( s.Id );
                        }
                    }
                    pkrSchedule.SetValues( ids );
                }
                pkrBirthdate.DateRange = new DateRange( DateTime.Parse( "2019-10-01" ), DateTime.Parse( "2019-10-31" ) );
                pkrAttendance.DateRange = new DateRange( DateTime.Parse( "2022-01-01" ), DateTime.Parse( "2022-10-31" ) );
            }
        }

        #endregion

        #region Methods
        private void LoadData()
        {
            DateRange birthdateRange = pkrBirthdate.DateRange;
            DateTime bdayStart = birthdateRange.Start.Value.StartOfDay();
            DateTime bdayEnd = birthdateRange.End.Value.EndOfDay();
            DateRange attendanceRange = pkrAttendance.DateRange;
            DateTime attStart = attendanceRange.Start.Value.StartOfDay();
            DateTime attEnd = attendanceRange.End.Value.EndOfDay();

            var people = _paSvc.Queryable().Where( pa => pa.Person.BirthDate.Value >= bdayStart && pa.Person.BirthDate.Value <= bdayEnd );
            var attendace = _attSvc.Queryable().Where( a => a.StartDateTime >= attStart && a.StartDateTime <= attEnd );
            attendace = attendace.Join( scheduleIds,
                a => a.Occurrence.ScheduleId,
                s => s,
                ( a, s ) => a
            );
            attendace = attendace.Join( people,
                a => a.PersonAliasId,
                p => p.Id,
                ( a, p ) => a
            );
            var groupedAttendance = attendace.GroupBy( a => a.PersonAlias.PersonId ).OrderByDescending( a => a.Count() ).ToList().Select( a =>
              {
                  var attendedSchedules = a.Select( d => d.Occurrence.Schedule ).GroupBy( s => s.Id ).Select( s => new { Id = s.Key, Schedule = s.First(), Count = s.Count() } ).OrderBy( s => s.Schedule.NextStartDateTime ).ToList();
                  int totalAttendance = a.Select( d => d.Occurrence.OccurrenceDate ).Distinct().Count();
                  string data = $"{{\"Id\": \"{a.Key}\", \"Name\": \"{a.First().PersonAlias.Person.FullName }\", \"DaysAttended\": \"{totalAttendance}\", \"ServicesAttended\": \"{a.Count()}\", ";
                  for ( int i = 0; i < attendedSchedules.Count(); i++ )
                  {
                      var percentage = Math.Round( 100 * ( ( double ) attendedSchedules[i].Count / ( double ) totalAttendance ) );
                      data += $"\"Schedule_{attendedSchedules[i].Id}\": \"{percentage}%\", ";
                  }
                  var unattended = scheduleIds.Where( i => !attendedSchedules.Select( s => s.Id ).Contains( i ) ).ToList();
                  for ( int i = 0; i < unattended.Count(); i++ )
                  {
                      data += $"\"Schedule_{unattended[i]}\": \"0%\", ";
                  }
                  data += "}";
                  return JsonConvert.DeserializeObject<Object>( data );
              } ).ToList();

            //Build Chart
            var totalDays = attendace.Count();
            var serviceAttendance = attendace.GroupBy( a => a.Occurrence.ScheduleId ).ToList().OrderBy( a => a.FirstOrDefault().Occurrence.Schedule.NextStartDateTime ).ToList().Select( a =>
            {
                var percentage = Math.Round( 100 * ( ( double ) a.Count() / ( double ) totalDays ) );
                return new { Schedule = a.FirstOrDefault().Occurrence.Schedule.Name, Data = percentage };
            } );

            ScriptManager.RegisterStartupScript(
                Page,
                GetType(),
                "BuildChart",
                $"buildChart({JsonConvert.SerializeObject( serviceAttendance )});",
                true );

            grdKids.DataSource = groupedAttendance;
            grdKids.DataBind();
        }
        #endregion

        #region Actions
        protected void RunReport_Click( object sender, EventArgs e )
        {
            scheduleIds = pkrSchedule.SelectedValuesAsInt().ToList();
            foreach ( DataControlField col in grdKids.Columns )
            {
                if ( col.HeaderText.StartsWith( "Schedule_" ) )
                {
                    grdKids.Columns.Remove( col );
                }
            }
            List<Schedule> schedules = new List<Schedule>();
            for ( int i = 0; i < scheduleIds.Count(); i++ )
            {
                Schedule s = _schSvc.Get( scheduleIds[i] );
                schedules.Add( s );
            }
            schedules = schedules.OrderBy( s => s.NextStartDateTime ).ToList();
            for ( int i = 0; i < schedules.Count(); i++ )
            {
                BoundField col = new BoundField()
                {
                    HeaderText = schedules[i].Name,
                    DataField = "Schedule_" + schedules[i].Id.ToString()
                };
                grdKids.Columns.Add( col );
            }
            LoadData();
        }

        #endregion

    }
}