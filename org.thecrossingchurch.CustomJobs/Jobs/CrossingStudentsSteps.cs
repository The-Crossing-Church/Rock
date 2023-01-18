// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System.Text.RegularExpressions;
using System;
using Rock.Communication;
using Rock;
using Rock.Web.Cache;

namespace org.crossingchurch.CrossingStudentsSteps.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( "Group Purpose: Small Group Id", "The Id of the DefinedValue for the Group Type Purpose, Small Group", true, "", "", 0 )]
    [TextField( "Group Purpose: Serving Area Id", "The Id of the DefinedValue for the Group Type Purpose, Serving Area", true, "", "", 0 )]
    [TextField( "Step Type: Attending Small Group Id", "The Id of the Step Type for Attending Small Group", true, "", "", 0 )]
    [TextField( "Step Type: Attending Sunday Id", "The Id of the Step Type for Attending on Sunday", true, "", "", 0 )]
    [TextField( "Step Type: Serving Id", "The Id of the Step Type for Serving", true, "", "", 0 )]
    [TextField( "Step Program Id", "The Id of the Crossing Students Step Program", true, "", "", 0 )]
    [DateField( "Override Date", "Date to use as 'Today' to create data for previous time frames", false )]
    [BooleanField( "Run Serving", "Whether or not to run the serving portion of the job", false )]
    [BooleanField( "Run Small Group Attendance", "Whether or not to run the small group attendance portion of the job", false )]
    [BooleanField( "Run Sunday Attendance", "Whether or not to run the sunday attendance portion of the job", false )]
    [DisallowConcurrentExecution]
    public class CrossingStudentsSteps : IJob
    {
        ///Variables
        private RockContext _context { get; set; }
        private StepService _stepsvc { get; set; }
        private GroupTypeService _grouptypesvc { get; set; }
        private GroupService _groupsvc { get; set; }
        private GroupMemberService _groupmembersvc { get; set; }
        private StepStatusService _stepstatussvc { get; set; }
        private AttributeService _attributesvc { get; set; }
        private AttendanceOccurrenceService _attendanceoccsvc { get; set; }
        private AttendanceService _attendancesvc { get; set; }
        private DateTime? _overrideDate { get; set; }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CrossingStudentsSteps()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            int smallGroup = Int32.Parse( dataMap.GetString( "GroupPurpose:SmallGroupId" ) );
            int servingArea = Int32.Parse( dataMap.GetString( "GroupPurpose:ServingAreaId" ) );
            int smallgroupStep = Int32.Parse( dataMap.GetString( "StepType:AttendingSmallGroupId" ) );
            int sundayStep = Int32.Parse( dataMap.GetString( "StepType:AttendingSundayId" ) );
            int servingStep = Int32.Parse( dataMap.GetString( "StepType:ServingId" ) );
            int stepProgram = Int32.Parse( dataMap.GetString( "StepProgramId" ) );
            string overrideDt = dataMap.GetString( "OverrideDate" );
            bool runServing = dataMap.GetBoolean( "RunServing" );
            bool runSGAtt = dataMap.GetBoolean( "RunSmallGroupAttendance" );
            bool runSunAtt = dataMap.GetBoolean( "RunSundayAttendance" );
            if ( !String.IsNullOrEmpty( overrideDt ) )
            {
                _overrideDate = DateTime.Parse( overrideDt );
            }


            _context = new RockContext();
            PersonService personsvc = new PersonService( _context );
            _stepsvc = new StepService( _context );
            _grouptypesvc = new GroupTypeService( _context );
            _groupsvc = new GroupService( _context );
            _groupmembersvc = new GroupMemberService( _context );
            _stepstatussvc = new StepStatusService( _context );
            _attributesvc = new AttributeService( _context );
            _attendanceoccsvc = new AttendanceOccurrenceService( _context );
            _attendancesvc = new AttendanceService( _context );
            List<int> connStatuses = new DefinedValueService( _context ).Queryable().Where( dv => dv.DefinedTypeId == 4 && (
                    dv.Value == "Student Member" ||
                    dv.Value == "Member" ||
                    dv.Value == "Attendee"
               ) ).Select( dv => dv.Id ).ToList();
            List<Person> students = personsvc.Queryable().ToList().Where( p => p.RecordStatusValueId == 3 && p.GradeOffset >= 0 && p.GradeOffset <= 6 && connStatuses.Contains( p.ConnectionStatusValueId.Value ) ).ToList();

            //Run the services requested
            if ( runServing )
            {
                //Check for Students No Longer Serving
                NoLongerServing( servingStep );
                //Check if Actively in Serving Group
                StartedServing( students, servingStep, servingArea, stepProgram );
            }

            if ( runSGAtt )
            {
                //Check for Small Group Attendance
                SmallGroupAttendance( students, smallgroupStep, smallGroup, stepProgram );
            }
            if ( runSunAtt )
            {
                //Check for Sunday Attendance
                SundayAttendance( students, sundayStep, stepProgram );
            }
        }

        /// <summary>
        /// Function to check for students that are no longer serving due to groups becoming inactive or being removed from membership
        /// </summary>
        /// <param name="servingStep"></param>
        private void NoLongerServing( int servingStep )
        {
            //Get all Steps of the Serving Step Type without an end time, meaning they are active
            var steps = _stepsvc.Queryable().Where( s => s.StepTypeId == servingStep && s.EndDateTime == null ).ToList();
            for ( var i = 0; i < steps.Count(); i++ )
            {
                var step = steps[i];
                step.LoadAttributes();
                var personid = step.PersonAlias.PersonId;
                if ( step.AttributeValues["ServingTeam"] != null )
                {
                    var group = _groupsvc.Get( Guid.Parse( step.AttributeValues["ServingTeam"].Value ) );
                    if ( group == null || group.IsActive == false )
                    {
                        //End their step because they aren't serving in that group anymore
                        step.EndDateTime = RockDateTime.Now;
                        _context.SaveChanges();
                        continue;
                    }
                    var groupmem = _groupmembersvc.Queryable().FirstOrDefault( gm => gm.PersonId == personid && gm.GroupId == group.Id );
                    if ( groupmem == null || groupmem.GroupMemberStatus == GroupMemberStatus.Inactive )
                    {
                        //End their step because they aren't serving in that group anymore
                        step.EndDateTime = RockDateTime.Now;
                        _context.SaveChanges();
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Function to check for students who are active members of serving groups
        /// </summary>
        /// <param name="students"></param>
        /// <param name="servingStep"></param>
        /// <param name="servingArea"></param>
        /// <param name="stepProgram"></param>
        private void StartedServing( List<Person> students, int servingStep, int servingArea, int stepProgram )
        {
            //Get all the active groups that are Serving Groups
            var groupTypes = _grouptypesvc.Queryable().Where( gt => gt.GroupTypePurposeValueId == servingArea ).Select( gt => gt.Id ).ToList();
            var groups = _groupsvc.Queryable().Where( g => groupTypes.Contains( g.GroupTypeId ) && g.IsActive == true );
            for ( var i = 0; i < students.Count(); i++ )
            {
                var student = students[i];
                //Return the list of serving groups this student is an active member of
                var validMemberships = _groupmembersvc.Queryable().Where( gm => gm.PersonId == student.Id && gm.GroupMemberStatus != GroupMemberStatus.Inactive ).Join( groups,
                        gm => gm.GroupId,
                        g => g.Id,
                        ( gm, g ) => g
                  ).OrderBy( g => g.Id ).ToList();
                //Get all the steps of type Serving that exist for this student
                var steps = _stepsvc.Queryable().Where( s => s.StepTypeId == servingStep && s.PersonAlias.PersonId == student.Id ).ToList();
                for ( var j = 0; j < steps.Count(); j++ )
                {
                    var step = steps[j];
                    step.LoadAttributes();
                    var group = _groupsvc.Get( Guid.Parse( step.AttributeValues["ServingTeam"].Value ) );
                    if ( group != null && validMemberships.Select( g => g.Id ).Contains( group.Id ) )
                    {
                        //If the group in valid memberships is already contained in a step remove it from the list
                        var idx = validMemberships.Select( g => g.Id ).ToList().IndexOf( group.Id );
                        validMemberships.RemoveAt( idx );
                    }
                }
                //Enumerate Again
                validMemberships = validMemberships.ToList();
                //Now we are left with only the valid memberships not already contained in a step
                for ( var j = 0; j < validMemberships.Count(); j++ )
                {
                    int id = validMemberships[j].Id;
                    var membership = _groupmembersvc.Queryable().FirstOrDefault( gm => gm.PersonId == student.Id && gm.GroupMemberStatus != GroupMemberStatus.Inactive && gm.GroupId == id );
                    var status = _stepstatussvc.Queryable().FirstOrDefault( ss => ss.StepProgramId == stepProgram && ss.Order == 0 );
                    var step = new Step()
                    {
                        StepTypeId = servingStep,
                        PersonAliasId = student.PrimaryAliasId.Value,
                        StartDateTime = membership.CreatedDateTime,
                        CompletedDateTime = membership.CreatedDateTime,
                        StepStatusId = status.Id
                    };
                    _context.Steps.Add( step );
                    _context.SaveChanges();
                    step.LoadAttributes( _context );
                    step.SetAttributeValue( "ServingTeam", validMemberships[j].Guid );
                    step.SaveAttributeValues( _context );
                }
            }
        }

        /// <summary>
        /// Function to add or remove steps for the most recent small group session based on attendance
        /// </summary>
        /// <param name="students">The List of People classified as CS</param>
        /// <param name="smallgroupStep">The step in the CS Step Program for Attending Small Group</param>
        /// <param name="smallGroup">The Group Purpose Id for Small Groups</param>
        /// <param name="stepProgram">The Id of the Crossing Students Step Program</param>
        private void SmallGroupAttendance( List<Person> students, int smallgroupStep, int smallGroup, int stepProgram )
        {
            //Figure out date range for current/most recent trimester
            var today = DateTime.Now;
            //User override date if we are looking at previous data
            if ( _overrideDate.HasValue )
            {
                today = _overrideDate.Value;
            }
            DateTime start;
            DateTime end;
            if ( today.Month > 1 && today.Month < 6 )
            {
                start = new DateTime( today.Year, 1, 1 );
                end = new DateTime( today.Year, 5, 31 );
            }
            else if ( today.Month >= 6 && today.Month < 9 )
            {
                start = new DateTime( today.Year, 6, 1 );
                end = new DateTime( today.Year, 8, 31 );
            }
            else
            {
                if ( today.Month == 1 )
                {
                    start = new DateTime( today.Year - 1, 9, 1 );
                    end = new DateTime( today.Year - 1, 12, 31 );
                }
                else
                {
                    start = new DateTime( today.Year, 9, 1 );
                    end = new DateTime( today.Year, 12, 31 );
                }
            }

            // Figure out which groups are small groups, which students are in them, and what occurrences are in the most recent trimester
            var smallGroupTypes = _grouptypesvc.Queryable().Where( gt => gt.GroupTypePurposeValueId == smallGroup );
            var smallGroups = _groupsvc.Queryable().Join( smallGroupTypes,
                    g => g.GroupTypeId,
                    gt => gt.Id,
                    ( g, gt ) => g
                );
            var sgMembers = _groupmembersvc.Queryable().Join( smallGroups,
                    gm => gm.GroupId,
                    g => g.Id,
                    ( gm, g ) => gm
                ).Where( gm => gm.GroupRole.Name == "Member" );
            var studentsInSG = students.Join( sgMembers,
                    p => p.Id,
                    gm => gm.PersonId,
                    ( p, gm ) => p
                ).Distinct().ToList();
            var ocurrences = _attendanceoccsvc.Queryable().Where( ao => DateTime.Compare( start, ao.OccurrenceDate ) <= 0 && DateTime.Compare( end, ao.OccurrenceDate ) >= 0 ).Join( smallGroups,
                    ao => ao.GroupId,
                    g => g.Id,
                    ( ao, g ) => ao
                );

            // Figure out if student attended more than 50% of their group's meetings 
            for ( var i = 0; i < studentsInSG.Count(); i++ )
            {
                var student = studentsInSG[i];
                var studentId = student.Id;
                var studentsSgOcc = sgMembers.Where( gm => gm.PersonId == student.Id ).Join( ocurrences,
                        gm => gm.GroupId,
                        ao => ao.GroupId,
                        ( gm, ao ) => ao
                    );
                var attendance = _attendancesvc.Queryable().Where( a => a.PersonAlias.PersonId == studentId ).Join( studentsSgOcc,
                        a => a.OccurrenceId,
                        ao => ao.Id,
                        ( a, ao ) => a
                    ).Where( a => a.DidAttend.Value == true ).ToList();

                //Steps for this student in the correct date range
                var steps = _stepsvc.Queryable().Where( s => s.StepTypeId == smallgroupStep && s.PersonAlias.PersonId == studentId && DateTime.Compare( start, s.StartDateTime.Value ) <= 0 && DateTime.Compare( end, s.StartDateTime.Value ) >= 0 );
                var group = sgMembers.First( gm => gm.PersonId == student.Id ).Group.Guid.ToString();
                if ( attendance.Count() > 0 && attendance.Count() >= ( studentsSgOcc.ToList().Count() / 2 ) )
                {
                    //Student is attending so make sure they have the step
                    var forGroup = steps.WhereAttributeValue( _context, av => av.Attribute.Key == "SmallGroup" && av.Value == group ).ToList();
                    if ( forGroup.Count() > 1 )
                    {
                        //Somehting isn't right here, they should only have one step per session and group
                    }
                    else if ( forGroup.Count() == 1 )
                    {
                        //Don't actually need to do anything here
                    }
                    else
                    {
                        //The student doesn't have a step and we need to add one
                        var status = _stepstatussvc.Queryable().FirstOrDefault( ss => ss.StepProgramId == stepProgram && ss.Order == 0 );
                        DateTime stepDt = attendance.OrderBy( a => a.Occurrence.OccurrenceDate ).First().Occurrence.OccurrenceDate;
                        var step = new Step()
                        {
                            StepTypeId = smallgroupStep,
                            PersonAliasId = student.PrimaryAliasId.Value,
                            StartDateTime = stepDt,
                            CompletedDateTime = stepDt,
                            StepStatusId = status.Id
                        };
                        _context.Steps.Add( step );
                        _context.SaveChanges();
                        step.LoadAttributes( _context );
                        step.SetAttributeValue( "SmallGroup", group );
                        step.SaveAttributeValues( _context );
                    }
                }
                else
                {
                    //Student did not attend 50%, we will remove the step if it exists
                    var forGroup = steps.WhereAttributeValue( _context, av => av.Attribute.Key == "SmallGroup" && av.Value == group ).ToList();
                    if ( forGroup.Count() > 0 )
                    {
                        for ( var k = 0; k < forGroup.Count(); k++ )
                        {
                            _context.Steps.Remove( forGroup[k] );
                        }
                        _context.SaveChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Function to determin if student has attended at least 50% of Sunday morning classes in the last 6 months 
        /// </summary>
        /// <param name="students"></param>
        /// <param name="sundayStep"></param>
        /// <param name="stepProgram"></param>
        private void SundayAttendance( List<Person> students, int sundayStep, int stepProgram )
        {
            DateTime today = DateTime.Now;
            //Use override if it has a value 
            if ( _overrideDate.HasValue )
            {
                today = _overrideDate.Value;
            }

            //Figure out start and end time frame 
            DateTime start;
            DateTime end;
            if ( today.Month > 1 && today.Month < 7 )
            {
                start = new DateTime( today.Year, 1, 1 );
                end = today;
            }
            else if ( today.Month == 1 )
            {
                start = new DateTime( today.Year - 1, 7, 1 );
                end = new DateTime( today.Year - 1, 12, 31 );
            }
            else
            {
                start = new DateTime( today.Year, 7, 1 );
                end = today;
            }

            //Crossing Students Check-in Groups
            var checkinGroups = _groupsvc.Queryable().Where( g => g.GroupTypeId == 79 || g.GroupTypeId == 80 ).Select( g => g.Id ).ToList(); //Middle School and High School 
            //Get Attendance Occurences for Sunday mornings between the start and end ranges
            var occurrence = _attendanceoccsvc.Queryable().Where( ao => checkinGroups.Contains( ao.GroupId.Value ) && DateTime.Compare( start, ao.OccurrenceDate ) <= 0 && DateTime.Compare( end, ao.OccurrenceDate ) >= 0 );
            for ( var i = 0; i < students.Count(); i++ )
            {
                var student = students[i];
                int studentId = student.Id;
                var studentAtt = _attendancesvc.Queryable().Where( a => a.DidAttend != false && a.PersonAlias.PersonId == studentId ).Join( occurrence,
                        a => a.OccurrenceId,
                        ao => ao.Id,
                        ( a, ao ) => a
                    ).ToList();
                //Get occurences that student could check-in to based off their attendance
                if ( studentAtt.Count() > 0 )
                {
                    var groups = studentAtt.Select( a => a.Occurrence.GroupId ).Distinct();
                    var studentOcc = occurrence.Where( ao => groups.Contains( ao.GroupId ) ).Select( ao => ao.OccurrenceDate ).Distinct().ToList().Where( d => d.DayOfWeek == DayOfWeek.Sunday );
                    if ( studentAtt.Count() > ( studentOcc.Count() / 2 ) )
                    {
                        //Student attended at least 50% of Sunday classes this semester
                        //Steps for this student in the correct date range
                        var steps = _stepsvc.Queryable().Where( s => s.StepTypeId == sundayStep && s.PersonAlias.PersonId == studentId && DateTime.Compare( start, s.StartDateTime.Value ) <= 0 && DateTime.Compare( end, s.StartDateTime.Value ) >= 0 ).ToList();
                        if ( steps.Count() > 1 )
                        {
                            //Figure out what to do because there should only be one :(
                        }
                        else if ( steps.Count() == 0 )
                        {
                            //Add a step for this student 
                            var status = _stepstatussvc.Queryable().FirstOrDefault( ss => ss.StepProgramId == stepProgram && ss.Order == 0 );
                            DateTime stepDt = studentAtt.OrderBy( a => a.Occurrence.OccurrenceDate ).First().Occurrence.OccurrenceDate;
                            var step = new Step()
                            {
                                StepTypeId = sundayStep,
                                PersonAliasId = student.PrimaryAliasId.Value,
                                StartDateTime = stepDt,
                                CompletedDateTime = stepDt,
                                StepStatusId = status.Id
                            };
                            _context.Steps.Add( step );
                            //_context.SaveChanges();
                        }
                    }
                    else
                    {
                        //Student did not attend at least 50% of Sunday classes
                        //Steps for this student in the correct date range
                        var steps = _stepsvc.Queryable().Where( s => s.StepTypeId == sundayStep && s.PersonAlias.PersonId == studentId && DateTime.Compare( start, s.StartDateTime.Value ) <= 0 && DateTime.Compare( end, s.StartDateTime.Value ) >= 0 ).ToList();
                        for ( var k = 0; k < steps.Count(); k++ )
                        {
                            _context.Steps.Remove( steps[k] );
                        }
                    }
                }
            }
            _context.SaveChanges();
        }
    }
}
