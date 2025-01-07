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
using Rock.Web.Cache;
using System.Runtime.InteropServices;
using Rock.Workflow.Action;
using System.Web.UI;
using System.Linq.Expressions;
using DotLiquid.Util;

namespace org.crossingchurch.SimpleGroupScheduler.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [GroupField("Group", "Group to Auto-Schedule", true, "", "", 0)]
    [BooleanField("Include Child Groups", "If checked, the job will schedule all child groups of the selected parent", false, "", 0)]
    [PersonField("Sender", "The person the message about serving should come from", true)]
    [TextField("RSVP Url", "The URL of the Serving RSVP Page", true)]
    [DisallowConcurrentExecution]
    public class SimpleGroupScheduler : IJob
    {
        private GroupService GroupSvc { get; set; }
        private AttendanceOccurrenceService AttendanceOccurrenceSvc { get; set; }
        private AttendanceService AttendanceSvc { get; set; }
        private CommunicationService CommunicationSvc { get; set; }
        private PersonService PersonSvc { get; set; }
        private Person Sender { get; set; }
        private RockContext _context { get; set; }
        private string baseurl { get; set; }

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
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            _context = new RockContext();
            List<Group> groups = new List<Group>();
            List<Person> scheduledPeople = new List<Person>();
            GroupSvc = new GroupService(_context);
            AttendanceOccurrenceSvc = new AttendanceOccurrenceService(_context);
            AttendanceSvc = new AttendanceService(_context);
            CommunicationSvc = new CommunicationService(_context);
            PersonSvc = new PersonService(_context);

            string guid = dataMap.GetString("Group");
            bool includeChildren = dataMap.GetBoolean("IncludeChildGroups");
            string sender_guid = dataMap.GetString("Sender");
            baseurl = dataMap.GetString("RSVPUrl");
            Sender = new PersonAliasService(_context).Get(Guid.Parse(sender_guid)).Person;

            Group parent = GroupSvc.Get(Guid.Parse(guid));
            if ( includeChildren )
            {
                groups = FindAllSubGroups(groups, parent);
            }
            else
            {
                groups.Add(parent);
            }
            //Schedule groups
            for ( var i = 0; i < groups.Count(); i++ )
            {
                var members = groups[i].Members.ToList();
                for ( var j = 0; j < members.Count(); j++ )
                {
                    ScheduleMember(groups[i], members[j].Person);
                    if ( !scheduledPeople.Select(p => p.Id).Contains(members[j].Person.Id) )
                    {
                        scheduledPeople.Add(members[j].Person);
                    }
                }
            }
            //Send Notifications to Scheduled People
            for ( var i = 0; i < scheduledPeople.Count(); i++ )
            {
                SendMessage(scheduledPeople[i]);
            }
        }

        private List<Group> FindAllSubGroups( List<Group> list, Group current )
        {
            //If the current group has a schedule and members, add it to our list
            if ( current.Schedule != null && current.Members.Count() > 0 )
            {
                list.Add(current);
            }
            var childGroups = GroupSvc.Queryable().Where(g => g.ParentGroupId == current.Id).ToList();
            for ( var i = 0; i < childGroups.Count(); i++ )
            {
                list = FindAllSubGroups(list, childGroups[i]);
            }
            return list;
        }

        private void ScheduleMember( Group group, Person person )
        {
            var locations = group.GroupLocations.ToList();
            for ( var i = 0; i < locations.Count(); i++ )
            {
                var schedules = locations[i].Schedules.ToList();
                if ( schedules.Count() == 0 )
                {
                    //Pull Group Schedule if Location Schedule is empty
                    schedules.Add(group.Schedule);
                }
                for ( var j = 0; j < schedules.Count(); j++ )
                {
                    var nextOcc = schedules[j].GetNextStartDateTime(DateTime.Now);
                    if ( nextOcc == null )
                    {
                        //Add new occurrence if none exist
                        nextOcc = GetNextOccurrence(schedules[j]);
                    }
                    var occurrence = AttendanceOccurrenceSvc.GetOrAdd(nextOcc.Value, group.Id, locations[i].LocationId, schedules[j].Id);
                    var attendance = AttendanceSvc.ScheduledPersonAddPending(person.Id, occurrence.Id, Sender.PrimaryAlias);
                    _context.SaveChanges();
                }
            }
        }

        private void SendMessage( Person person )
        {
            var token = person.GetImpersonationToken();
            var url = baseurl + "?rckipid=" + token;
            var msg = "Your serving team is scheduled this week, please use this <a href='" + url + "'>link</a> to let us know if you are able to serve.";
            //Send Email
            var header = new AttributeValueService(_context).Queryable().FirstOrDefault(a => a.AttributeId == 140).Value; //Email Header
            var footer = new AttributeValueService(_context).Queryable().FirstOrDefault(a => a.AttributeId == 141).Value; //Email Footer 
            var message = header + msg + footer;
            string subject = "Your Serving Team is Scheduled This Week";
            RockEmailMessageRecipient recipient = new RockEmailMessageRecipient(person, new Dictionary<string, object>());
            RockEmailMessage email = new RockEmailMessage();
            email.Subject = subject;
            email.Message = message;
            email.FromEmail = "info@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            email.AddRecipient(recipient);
            var output = email.Send();
            #region sms 
            //if ( person.CommunicationPreference == CommunicationType.SMS )
            //{
            //    //Get 65201 Phone
            //    DefinedValue phoneDV = new DefinedValueService(_context).Get(1702);
            //    var cache = new DefinedValueCache();
            //    cache.SetFromEntity(phoneDV);
            //    //Send SMS
            //    PageShortLinkService ShortLinkSvc = new PageShortLinkService(_context);
            //    //Create Short Link
            //    var tkn = ShortLinkSvc.GetUniqueToken(3, 7);
            //    var link = new PageShortLink();
            //    link.SiteId = 3; //The Crossing External Site
            //    link.Token = tkn;
            //    link.Url = url;
            //    ShortLinkSvc.Add(link);
            //    _context.SaveChanges();
            //    link = ShortLinkSvc.GetInclude(link.Id, l => l.Site);
            //    //Set Message
            //    msg = "Your serving team is scheduled this week, please use this link to let us know if you are able to serve. " + link.Site.SiteDomains.First() + "/" + link.Token;
            //    RockSMSMessageRecipient recipient = new RockSMSMessageRecipient(person, person.PhoneNumbers.FirstOrDefault(p => p.NumberTypeValue.Value == "Mobile").Number, new Dictionary<string, object>());
            //    RockSMSMessage sms = new RockSMSMessage();
            //    sms.Message = msg;
            //    sms.FromNumber = cache;
            //    sms.AddRecipient(recipient);
            //    var output = sms.Send();
            //}
            //else
            //{
            //}
            #endregion
        }

        private DateTime GetNextOccurrence( Schedule schedule )
        {
            //Weekly Schedule
            if ( schedule.ScheduleType == ScheduleType.Weekly )
            {
                DateTime today = DateTime.Today;
                int daysUntil = ( (int)schedule.WeeklyDayOfWeek - (int)today.DayOfWeek + 7 ) % 7;
                DateTime nextDate = today.AddDays(daysUntil);
                nextDate = new DateTime(nextDate.Year, nextDate.Month, nextDate.Day, schedule.WeeklyTimeOfDay.Value.Hours, schedule.WeeklyTimeOfDay.Value.Minutes, 0);
                return nextDate;
            }
            //else if ( schedule.ScheduleType == ScheduleType.Named )
            //{
                //TODO
            //}
            return DateTime.Today;
        }
    }
}
