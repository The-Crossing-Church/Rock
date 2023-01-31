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

namespace RockWeb.Plugins.com_thecrossingchurch.Reporting
{
    [DisplayName( "Agape Checkin Report" )]
    [Category( "com_thecrossingchurch > Reporting" )]
    [Description( "Live report of Agape kids and buddies currently checked in" )]
    [GroupsField( "Agape Kids Groups", "", false, key: AttributeKey.AgapeKidsGroup, order: 1 )]
    [GroupsField( "Agape Buddies Groups", "Checkin Groups for Buddies", false, key: AttributeKey.AgapeBuddiesGroup, order: 2 )]
    public partial class AgapeCheckinReport : Rock.Web.UI.RockBlock
    {
        #region Keys
        private static class AttributeKey
        {
            public const string AgapeKidsGroup = "AgapeKidsGroup";
            public const string AgapeBuddiesGroup = "AgapeBuddiesGroup";
        }
        #endregion

        #region Variables
        private RockContext _context { get; set; }
        private GroupService _grpSvc { get; set; }
        private AttendanceService _attSvc { get; set; }
        private PersonAliasService _paSvc { get; set; }
        private List<Guid> agapeKidsGroupGuids { get; set; }
        private List<Guid> agapeBuddiesGroupGuids { get; set; }
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
            var kidsGuids = GetAttributeValue( AttributeKey.AgapeKidsGroup );
            if ( !String.IsNullOrEmpty( kidsGuids ) )
            {
                agapeKidsGroupGuids = kidsGuids.Split( ',' ).Select( v => Guid.Parse( v ) ).ToList();
            }
            var buddyGuids = GetAttributeValue( AttributeKey.AgapeBuddiesGroup );
            if ( !String.IsNullOrEmpty( buddyGuids ) )
            {
                agapeBuddiesGroupGuids = buddyGuids.Split( ',' ).Select( v => Guid.Parse( v ) ).ToList();
            }
            _grpSvc = new GroupService( _context );
            _attSvc = new AttendanceService( _context );
            _paSvc = new PersonAliasService( _context );

            if ( !Page.IsPostBack )
            {
                DateTime today = RockDateTime.Now;
                reportDate.SelectedDate = today.AddDays( -1 * ( int ) today.DayOfWeek );
                if ( !String.IsNullOrEmpty( PageParameter( "Date" ) ) )
                {
                    reportDate.SelectedDate = DateTime.Parse( PageParameter( "Date" ) );
                }

                if ( agapeKidsGroupGuids != null && agapeKidsGroupGuids.Count() > 0 )
                {
                    LoadKids();
                }
                if ( agapeBuddiesGroupGuids != null && agapeBuddiesGroupGuids.Count() > 0 )
                {
                    LoadBuddies();
                }
            }
        }

        #endregion

        #region Methods
        private void LoadKids()
        {
            DateTime start = reportDate.SelectedDate.Value.StartOfDay();
            DateTime end = reportDate.SelectedDate.Value.EndOfDay();
            IEnumerable<Person> members = null;
            for ( int i = 0; i < agapeKidsGroupGuids.Count(); i++ )
            {
                Group agapeKidsGroup = null;
                agapeKidsGroup = _grpSvc.Get( agapeKidsGroupGuids[i] );
                if ( agapeKidsGroup != null )
                {
                    if ( members == null )
                    {
                        members = agapeKidsGroup.Members.Select( gm => gm.Person );
                    }
                    else
                    {
                        members = members.Union( agapeKidsGroup.Members.Select( gm => gm.Person ) );
                    }
                }
            }
            List<int> personIds = members.Select( m => m.Id ).Distinct().ToList();
            var aliases = _paSvc.Queryable().Join( personIds,
                a => a.PersonId,
                p => p,
                ( a, p ) => a
            );
            var attendance = _attSvc.Queryable().Where( a => a.StartDateTime >= start && a.StartDateTime <= end );
            attendance = attendance.Join( aliases,
                a => a.PersonAliasId,
                m => m.Id,
                ( a, m ) => a
            );
            var source = attendance.ToList().Select( a => new { Id = a.PersonAlias.PersonId, FirstName = a.PersonAlias.Person.FirstName, LastName = a.PersonAlias.Person.LastName, CheckInTime = a.StartDateTime, Location = a.Occurrence.Location != null ? a.Occurrence.Location.Name : ( a.Occurrence.Group != null ? a.Occurrence.Group.Name : "" ), Schedule = a.Occurrence.Schedule != null ? a.Occurrence.Schedule.Name : "", Time = a.Occurrence.Schedule.NextStartDateTime } ).OrderBy( s => s.Time ).ThenBy( s => s.LastName ).ToList();
            var groupedSource = source.GroupBy( s => s.Id ).Select( s => new
            {
                Id = s.Key,
                Name = String.Concat( s.First().LastName, ", ", s.First().FirstName ),
                Checkin = String.Join( ", ", s.Select( d =>
                {
                    if ( !String.IsNullOrEmpty( d.Schedule ) && !String.IsNullOrEmpty( d.Location ) )
                    {
                        return String.Concat( d.Schedule, ": ", d.Location );
                    }
                    else if ( !String.IsNullOrEmpty( d.Schedule ) )
                    {
                        return d.Schedule;
                    }
                    else
                    {
                        return d.Location;
                    }
                } ) )
            } ).ToList();
            grdAgapeKids.DataSource = groupedSource;
            grdAgapeKids.DataBind();
        }
        private void LoadBuddies()
        {
            DateTime start = reportDate.SelectedDate.Value.StartOfDay();
            DateTime end = reportDate.SelectedDate.Value.EndOfDay();
            List<int> groupIds = new List<int>();
            for ( int i = 0; i < agapeBuddiesGroupGuids.Count(); i++ )
            {
                Group agapeBuddyGroup = null;
                agapeBuddyGroup = _grpSvc.Get( agapeBuddiesGroupGuids[i] );
                if ( agapeBuddyGroup != null )
                {
                    groupIds.Add( agapeBuddyGroup.Id );
                }
            }
            var attendance = _attSvc.Queryable().Where( a => a.StartDateTime >= start && a.StartDateTime <= end );
            attendance = attendance.Join( groupIds,
                a => a.Occurrence.GroupId,
                g => g,
                ( a, g ) => a
            );
            var source = attendance.ToList().Select( a => new { Id = a.PersonAlias.PersonId, FirstName = a.PersonAlias.Person.FirstName, LastName = a.PersonAlias.Person.LastName, CheckInTime = a.StartDateTime, Location = a.Occurrence.Location.Name, Schedule = a.Occurrence.Schedule.Name, Time = a.Occurrence.Schedule.NextStartDateTime } ).OrderBy( s => s.Time ).ThenBy( s => s.LastName ).ToList();
            var groupedSource = source.GroupBy( s => s.Id ).Select( s => new { Id = s.Key, Name = String.Concat( s.First().LastName, ", ", s.First().FirstName ), Checkin = String.Join( ", ", s.Select( d => d.Schedule ) ) } ).ToList();
            grdBuddies.DataSource = groupedSource;
            grdBuddies.DataBind();
        }
        #endregion

        #region Actions
        protected void reportDate_ValueChanged( object sender, EventArgs e )
        {
            var p = new Dictionary<string, string>();
            p.Add( "Date", reportDate.SelectedDate.Value.ToString( "yyyy-MM-dd" ) );
            NavigateToCurrentPage( p );
        }
        #endregion
    }
}