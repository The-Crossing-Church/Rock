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
using System.Linq;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System;
using Rock.Communication;

namespace org.crossingchurch.SimpleGroupScheduler.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [GroupField("Group", "Group to Auto-Schedule", true, "", "", 0)]
    [BooleanField("Include Child Groups", "If checked, the job will schedule all child groups of the selected parent", false, "", 0)]
    [DisallowConcurrentExecution]
    public class SimpleGroupScheduler : IJob
    {
        private GroupService GroupSvc { get; set; }
        private AttendanceOccurrenceService AttendanceOccurrenceSvc { get; set; }
        private AttendanceService AttendanceSvc { get; set; }
        private CommunicationService CommunicationSvc { get; set; }
        private PersonService PersonSvc { get; set; }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SimpleGroupScheduler() { }

        /// <summary>
        /// Job that will create attendance for all group members and then email them for RSVP.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            var _context = new RockContext();
            List<Group> groups = new List<Group>();
            List<Person> scheduledPeople = new List<Person>();
            GroupSvc = new GroupService(_context);
            AttendanceOccurrenceSvc = new AttendanceOccurrenceService(_context);
            AttendanceSvc = new AttendanceService(_context);
            CommunicationSvc = new CommunicationService(_context);
            PersonSvc = new PersonService(_context);

            string guid = dataMap.GetString("Groups");
            bool includeChildren = dataMap.GetBoolean("IncludeChildGroups");

            Group parent = GroupSvc.Get(Guid.Parse(guid));
            if(includeChildren)
            {
                groups = FindAllSubGroups(groups, parent);
            }
            else
            {
                groups.Add(parent);
            }
            //Schedule groups
            for(var i = 0; i < groups.Count(); i++)
            {
                var members = groups[i].Members.ToList();
                for(var j = 0; j < members.Count(); j++)
                {
                    ScheduleMember(groups[i], members[j].Person);
                    if(!scheduledPeople.Select(p => p.Id).Contains(members[j].Person.Id))
                    {
                        scheduledPeople.Add(members[j].Person);
                    }
                }
            }
            //Send Notifications to Scheduled People
            for(var i = 0; i < scheduledPeople.Count(); i++)
            {
                SendMessage(scheduledPeople[i]);
            }
        }

        private List<Group> FindAllSubGroups(List<Group> list, Group current)
        {
            //If the current group has a schedule and members, add it to our list
            if(current.Schedule != null && current.Members.Count() > 0)
            {
                list.Add(current);
            }
            var childGroups = GroupSvc.Queryable().Where(g => g.ParentGroupId == current.Id).ToList();
            for(var i = 0; i < childGroups.Count(); i++)
            {
                list = FindAllSubGroups(list, childGroups[i]);
            }
            return list; 
        }

        private void ScheduleMember(Group group, Person person)
        {
            var locations = group.GroupLocations.ToList();
            for(var i = 0; i < locations.Count(); i++)
            {
                var schedules = locations[i].Schedules.ToList();
                for(var j = 0; j < schedules.Count(); j++)
                {
                    var nextOcc = schedules[j].GetNextStartDateTime(DateTime.Now);
                    var occurrence = AttendanceOccurrenceSvc.GetOrAdd(nextOcc.Value, schedules[j].Id, locations[i].Id, group.Id);
                    var attendance = new Attendance()
                    {
                        OccurrenceId = occurrence.Id,
                        PersonAliasId = person.PrimaryAliasId,
                        RSVP = RSVP.Unknown
                    };
                    AttendanceSvc.Add(attendance);
                }
            }
        }

        private void SendMessage(Person person)
        {
            var token = person.GetImpersonationToken();
            var url = "http://localhost:6030/page/20?rckipid=" + token;
            var msg = "Your serving team is scheduled this week, please use this <a href='" + url + "'>link</a> to let us know if you are able to serve.";
            if(person.CommunicationPreference == CommunicationType.SMS)
            {
                //Send SMS
                //CommunicationSvc.CreateSMSCommunication(person, person.PrimaryAliasId, msg, , "", "Serving Scheduled");
            }
            else
            {
                //Send Email
            }
        }
    }
}
