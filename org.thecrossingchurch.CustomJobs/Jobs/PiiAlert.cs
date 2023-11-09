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

namespace org.crossingchurch.PiiAlert.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [TextField("Email List", "Comma Separated List of Emails to Alert", true, "", "", 0)]
    [DisallowConcurrentExecution]
    public class PiiAlert : IJob
    {

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public PiiAlert()
        {
        }

        /// <summary>
        /// Job that will run quick SQL queries on a schedule.
        /// 
        /// Called by the <see cref="IScheduler" /> when a
        /// <see cref="ITrigger" /> fires that is associated with
        /// the <see cref="IJob" />.
        /// </summary>
        public virtual void Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string emails = dataMap.GetString("EmailList");
            Dictionary<string, List<string>> alerts = new Dictionary<string, List<string>>() {
                { "cc", new List<string>()},
                { "ssn", new List<string>()},
                { "pass", new List<string>()},
                { "drivers", new List<string>()},
            };
            string ssn = @"\d{3}-\d{2}-\d{4}";
            string cc = @"((4\d{3})|(5[1-5]\d{2})|(6011))-?\d{4}-?\d{4}-?\d{4}|3[4,7]\d{13}";
            //string pass = @"(?!^0+$)[a-zA-Z0-9]{3,20}";
            string pass = @"^[A-Z0-9<]{9}[0-9]{1}[A-Z]{3}[0-9]{7}[A-Z]{1}[0-9]{7}[A-Z0-9<]{14}[0-9]{2}$";
            List<string> mo_drivers = new List<string>() {
                @"[a-zA-Z]\d{5,9}$",
                @"[a-zA-Z]\d{6}R",
                @"\d{8}[a-zA-Z][a-zA-Z]",
                @"\d{9}[a-zA-Z]",
                @"\d{9}"
            };
            try
            {
                var prayer_requests = new PrayerRequestService(new RockContext()).Queryable().AsNoTracking().ToList();
                for (var i = 0; i < prayer_requests.Count(); i++)
                {
                    if (!String.IsNullOrEmpty(prayer_requests[i].Text))
                    {
                        if (Regex.Match(prayer_requests[i].Text, ssn).Success)
                        {
                            alerts["ssn"].Add($"Possible SSN match in Prayer Request\n\tFirst Name: {prayer_requests[i].FirstName}\n\tLast Name: {prayer_requests[i].LastName}\n\tMatched Text: {prayer_requests[i].Text}");
                        }
                        if (Regex.Match(prayer_requests[i].Text, cc).Success)
                        {
                            alerts["cc"].Add($"Possible Credit Card match in Prayer Request\n\tFirst Name: {prayer_requests[i].FirstName}\n\tLast Name: {prayer_requests[i].LastName}\n\tMatched Text: {prayer_requests[i].Text}");
                        }
                        if (Regex.Match(prayer_requests[i].Text, pass).Success)
                        {
                            alerts["pass"].Add($"Possible Passport Number match in Prayer Request\n\tFirst Name: {prayer_requests[i].FirstName}\n\tLast Name: {prayer_requests[i].LastName}\n\tMatched Text: {prayer_requests[i].Text}");
                        }
                        for (var j = 0; j < mo_drivers.Count(); j++)
                        {
                            if (Regex.Match(prayer_requests[i].Text, mo_drivers[j]).Success)
                            {
                                alerts["drivers"].Add($"Possible Driver's License Number match in Prayer Request\n\tFirst Name: {prayer_requests[i].FirstName}\n\tLast Name: {prayer_requests[i].LastName}\n\tMatched Text: {prayer_requests[i].Text}");
                            }
                        }
                    }
                }

                var notes = new NoteService(new RockContext()).Queryable().AsNoTracking().ToList();
                for (var i = 0; i < notes.Count(); i++)
                {
                    if (!String.IsNullOrEmpty(notes[i].Text))
                    {
                        if (Regex.Match(notes[i].Text, ssn).Success)
                        {
                            alerts["ssn"].Add($"Possible SSN match in Note\n\tFirst Name: {(notes[i].CreatedByPersonAlias != null ? notes[i].CreatedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(notes[i].CreatedByPersonAlias != null ? notes[i].CreatedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {notes[i].Text}");
                        }
                        if (Regex.Match(notes[i].Text, cc).Success)
                        {
                            alerts["cc"].Add($"Possible Credit Card match in Note\n\tFirst Name: {(notes[i].CreatedByPersonAlias != null ? notes[i].CreatedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(notes[i].CreatedByPersonAlias != null ? notes[i].CreatedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {notes[i].Text}");
                        }
                        if (Regex.Match(notes[i].Text, pass).Success)
                        {
                            alerts["pass"].Add($"Possible Passport Number match in Note\n\tFirst Name: {(notes[i].CreatedByPersonAlias != null ? notes[i].CreatedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(notes[i].CreatedByPersonAlias != null ? notes[i].CreatedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {notes[i].Text}");
                        }
                        for (var j = 0; j < mo_drivers.Count(); j++)
                        {
                            if (Regex.Match(notes[i].Text, mo_drivers[j]).Success)
                            {
                                alerts["drivers"].Add($"Possible Driver's License Number match in Note\n\tFirst Name: {(notes[i].CreatedByPersonAlias != null ? notes[i].CreatedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(notes[i].CreatedByPersonAlias != null ? notes[i].CreatedByPersonAlias.Person.LastName : "")}nMatched Text: {notes[i].Text}");
                            }
                        }
                    }
                }

                var metricvalue = new MetricValueService(new RockContext()).Queryable().AsNoTracking().ToList();
                for (var i = 0; i < metricvalue.Count(); i++)
                {
                    if (!String.IsNullOrEmpty(metricvalue[i].Note))
                    {
                        if (Regex.Match(metricvalue[i].Note, ssn).Success)
                        {
                            alerts["ssn"].Add($"Possible SSN match in Metric Value\n\tFirst Name: {(metricvalue[i].ModifiedByPersonAlias != null ? metricvalue[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(metricvalue[i].ModifiedByPersonAlias != null ? metricvalue[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {metricvalue[i].Note}");
                        }
                        if (Regex.Match(metricvalue[i].Note, cc).Success)
                        {
                            alerts["cc"].Add($"Possible Credit Card match in Metric Value\n\tFirst Name: {(metricvalue[i].ModifiedByPersonAlias != null ? metricvalue[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(metricvalue[i].ModifiedByPersonAlias != null ? metricvalue[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {metricvalue[i].Note}");
                        }
                        if (Regex.Match(metricvalue[i].Note, pass).Success)
                        {
                            alerts["pass"].Add($"Possible Passport Number match in Metric Value\n\tFirst Name: {(metricvalue[i].ModifiedByPersonAlias != null ? metricvalue[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(metricvalue[i].ModifiedByPersonAlias != null ? metricvalue[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {metricvalue[i].Note}");
                        }
                        for (var j = 0; j < mo_drivers.Count(); j++)
                        {
                            if (Regex.Match(metricvalue[i].Note, mo_drivers[j]).Success)
                            {
                                alerts["drivers"].Add($"Possible Driver's License Number match in Metric Value\n\tFirst Name: {(metricvalue[i].ModifiedByPersonAlias != null ? metricvalue[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(metricvalue[i].ModifiedByPersonAlias != null ? metricvalue[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {metricvalue[i].Note}");
                            }
                        }
                    }
                }

                var connectionrequestactivity = new ConnectionRequestActivityService(new RockContext()).Queryable().AsNoTracking().ToList();
                for (var i = 0; i < connectionrequestactivity.Count(); i++)
                {
                    if (!String.IsNullOrEmpty(connectionrequestactivity[i].Note))
                    {
                        if (Regex.Match(connectionrequestactivity[i].Note, ssn).Success)
                        {
                            alerts["ssn"].Add($"Possible SSN match in Connection Request Activity Note\n\tFirst Name: {(connectionrequestactivity[i].ModifiedByPersonAlias != null ? connectionrequestactivity[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(connectionrequestactivity[i].ModifiedByPersonAlias != null ? connectionrequestactivity[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {connectionrequestactivity[i].Note}");
                        }
                        if (Regex.Match(connectionrequestactivity[i].Note, cc).Success)
                        {
                            alerts["cc"].Add($"Possible Credit Card match in Connection Request Activity Note\n\tFirst Name: {(connectionrequestactivity[i].ModifiedByPersonAlias != null ? connectionrequestactivity[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(connectionrequestactivity[i].ModifiedByPersonAlias != null ? connectionrequestactivity[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {connectionrequestactivity[i].Note}");
                        }
                        if (Regex.Match(connectionrequestactivity[i].Note, pass).Success)
                        {
                            alerts["pass"].Add($"Possible Passport Number match in Connection Request Activity Note\n\tFirst Name: {(connectionrequestactivity[i].ModifiedByPersonAlias != null ? connectionrequestactivity[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(connectionrequestactivity[i].ModifiedByPersonAlias != null ? connectionrequestactivity[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {connectionrequestactivity[i].Note}");
                        }
                        for (var j = 0; j < mo_drivers.Count(); j++)
                        {
                            if (Regex.Match(connectionrequestactivity[i].Note, mo_drivers[j]).Success)
                            {
                                alerts["drivers"].Add($"Possible Driver's License Number match in Connection Request Activity Note\n\tFirst Name: {(connectionrequestactivity[i].ModifiedByPersonAlias != null ? connectionrequestactivity[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(connectionrequestactivity[i].ModifiedByPersonAlias != null ? connectionrequestactivity[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {connectionrequestactivity[i].Note}");
                            }
                        }
                    }
                }

                var connectionrequestcomments = new ConnectionRequestService(new RockContext()).Queryable().AsNoTracking().ToList();
                for (var i = 0; i < connectionrequestcomments.Count(); i++)
                {
                    if (!String.IsNullOrEmpty(connectionrequestcomments[i].Comments))
                    {
                        if (Regex.Match(connectionrequestcomments[i].Comments, ssn).Success)
                        {
                            alerts["ssn"].Add($"Possible SSN match in Connection Request Comments\n\tFirst Name: {(connectionrequestcomments[i].ModifiedByPersonAlias != null ? connectionrequestcomments[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(connectionrequestcomments[i].ModifiedByPersonAlias != null ? connectionrequestcomments[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {connectionrequestcomments[i].Comments}");
                        }
                        if (Regex.Match(connectionrequestcomments[i].Comments, cc).Success)
                        {
                            alerts["cc"].Add($"Possible Credit Card match in Connection Request Comments\n\tFirst Name: {(connectionrequestcomments[i].ModifiedByPersonAlias != null ? connectionrequestcomments[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(connectionrequestcomments[i].ModifiedByPersonAlias != null ? connectionrequestcomments[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {connectionrequestcomments[i].Comments}");
                        }
                        if (Regex.Match(connectionrequestcomments[i].Comments, pass).Success)
                        {
                            alerts["pass"].Add($"Possible Passport Number match in Connection Request Comments\n\tFirst Name: {(connectionrequestcomments[i].ModifiedByPersonAlias != null ? connectionrequestcomments[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(connectionrequestcomments[i].ModifiedByPersonAlias != null ? connectionrequestcomments[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {connectionrequestcomments[i].Comments}");
                        }
                        for (var j = 0; j < mo_drivers.Count(); j++)
                        {
                            if (Regex.Match(connectionrequestcomments[i].Comments, mo_drivers[j]).Success)
                            {
                                alerts["drivers"].Add($"Possible Driver's License Number match in Connection Request Comments\n\tFirst Name: {(connectionrequestcomments[i].ModifiedByPersonAlias != null ? connectionrequestcomments[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(connectionrequestcomments[i].ModifiedByPersonAlias != null ? connectionrequestcomments[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {connectionrequestcomments[i].Comments}");
                            }
                        }
                    }
                }

                var counter = 0;
                var total = new CommunicationService(new RockContext()).Queryable().Count();
                var processed = 0;
                var communication = new List<Communication>();
                while (processed < total)
                {
                    communication = new CommunicationService(new RockContext()).Queryable().OrderBy(x => x.Id).Skip(counter * 500).Take(500).AsNoTracking().ToList();
                    processed += communication.Count();
                    counter++;
                    for (var i = 0; i < communication.Count(); i++)
                    {
                        if (!String.IsNullOrEmpty(communication[i].Message))
                        {
                            if (Regex.Match(communication[i].Message, ssn).Success)
                            {
                                alerts["ssn"].Add($"Possible SSN match in Communication Message\n\tFirst Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {communication[i].Message}");
                            }
                            if (Regex.Match(communication[i].Message, cc).Success)
                            {
                                alerts["cc"].Add($"Possible Credit Card match in Communication Message\n\tFirst Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {communication[i].Message}");
                            }
                            if (Regex.Match(communication[i].Message, pass).Success)
                            {
                                alerts["pass"].Add($"Possible Passport Number match in Communication Message\n\tFirst Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {communication[i].Message}");
                            }
                            for (var j = 0; j < mo_drivers.Count(); j++)
                            {
                                if (Regex.Match(communication[i].Message, mo_drivers[j]).Success)
                                {
                                    alerts["drivers"].Add($"Possible Driver's License Number match in Communication Message\n\tFirst Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {communication[i].Message}");
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(communication[i].Subject))
                        {
                            if (Regex.Match(communication[i].Subject, ssn).Success)
                            {
                                alerts["ssn"].Add($"Possible SSN match in Communication Subject\n\tFirst Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {communication[i].Subject}");
                            }
                            if (Regex.Match(communication[i].Subject, cc).Success)
                            {
                                alerts["cc"].Add($"Possible Credit Card match in Communication Subject\n\tFirst Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {communication[i].Subject}");
                            }
                            if (Regex.Match(communication[i].Subject, pass).Success)
                            {
                                alerts["pass"].Add($"Possible Passport Number match in Communication Subject\n\tFirst Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {communication[i].Subject}");
                            }
                            for (var j = 0; j < mo_drivers.Count(); j++)
                            {
                                if (Regex.Match(communication[i].Subject, mo_drivers[j]).Success)
                                {
                                    alerts["drivers"].Add($"Possible Driver's License Number match in Communication Subject\n\tFirst Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(communication[i].ModifiedByPersonAlias != null ? communication[i].ModifiedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {communication[i].Subject}");
                                }
                            }
                        }
                    }
                }

                counter = 0;
                total = new AuditDetailService(new RockContext()).Queryable().Count();
                processed = 0;
                var auditdetail = new List<AuditDetail>();
                while (processed < total)
                {
                    auditdetail = new AuditDetailService(new RockContext()).Queryable().AsNoTracking().OrderBy(x => x.Id).Skip(counter * 500).Take(500).ToList();
                    processed += auditdetail.Count();
                    counter++;
                    for (var i = 0; i < auditdetail.Count(); i++)
                    {
                        if (!String.IsNullOrEmpty(auditdetail[i].OriginalValue))
                        {
                            if (Regex.Match(auditdetail[i].OriginalValue, ssn).Success)
                            {
                                alerts["ssn"].Add($"Possible SSN match in Audit Detail Original Value\n\tFirst Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.FirstName : "")}\n\tLast Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.LastName : "")}\n\tMatched Text: {auditdetail[i].OriginalValue}");
                            }
                            if (Regex.Match(auditdetail[i].OriginalValue, cc).Success)
                            {
                                alerts["cc"].Add($"Possible Credit Card match in Audit Detail Original Value\n\tFirst Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.FirstName : "")}\n\tLast Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.LastName : "")}\n\tMatched Text: {auditdetail[i].OriginalValue}");
                            }
                            if (Regex.Match(auditdetail[i].OriginalValue, pass).Success)
                            {
                                alerts["pass"].Add($"Possible Passport Number match in Audit Detail Original Value\n\tFirst Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.FirstName : "")}\n\tLast Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.LastName : "")}\n\tMatched Text: {auditdetail[i].OriginalValue}");
                            }
                            for (var j = 0; j < mo_drivers.Count(); j++)
                            {
                                if (Regex.Match(auditdetail[i].OriginalValue, mo_drivers[j]).Success)
                                {
                                    alerts["drivers"].Add($"Possible Driver's License Number match in Audit Detail Original Value\n\tFirst Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.FirstName : "")}\n\tLast Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.LastName : "")}\n\tMatched Text: {auditdetail[i].OriginalValue}");
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(auditdetail[i].CurrentValue))
                        {
                            if (Regex.Match(auditdetail[i].CurrentValue, ssn).Success)
                            {
                                alerts["ssn"].Add($"Possible SSN match in Audit Detail Current Value\n\tFirst Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.FirstName : "")}\n\tLast Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.LastName : "")}\n\tMatched Text: {auditdetail[i].CurrentValue}");
                            }
                            if (Regex.Match(auditdetail[i].CurrentValue, cc).Success)
                            {
                                alerts["cc"].Add($"Possible Credit Card match in Audit Detail Current Value\n\tFirst Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.FirstName : "")}\n\tLast Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.LastName : "")}\n\tMatched Text: {auditdetail[i].CurrentValue}");
                            }
                            if (Regex.Match(auditdetail[i].CurrentValue, pass).Success)
                            {
                                alerts["pass"].Add($"Possible Passport Number match in Audit Detail Current Value\n\tFirst Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.FirstName : "")}\n\tLast Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.LastName : "")}\n\tMatched Text: {auditdetail[i].CurrentValue}");
                            }
                            for (var j = 0; j < mo_drivers.Count(); j++)
                            {
                                if (Regex.Match(auditdetail[i].CurrentValue, mo_drivers[j]).Success)
                                {
                                    alerts["drivers"].Add($"Possible Driver's License Number match in Audit Detail Current Value\n\tFirst Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.FirstName : "")}\n\tLast Name: {(auditdetail[i].Audit.PersonAlias != null ? auditdetail[i].Audit.PersonAlias.Person.LastName : "")}\n\tMatched Text: {auditdetail[i].CurrentValue}");
                                }
                            }
                        }
                    }
                }

                counter = 0;
                total = new AttributeValueService(new RockContext()).Queryable().Count();
                processed = 0;
                var attributevalue = new List<AttributeValue>();
                while (processed < total)
                {
                    attributevalue = new AttributeValueService(new RockContext()).Queryable().AsNoTracking().OrderBy(x => x.Id).Skip(counter * 500).Take(500).ToList();
                    processed += attributevalue.Count();
                    counter++;
                    for (var i = 0; i < attributevalue.Count(); i++)
                    {
                        if (!String.IsNullOrEmpty(attributevalue[i].Value))
                        {
                            if (Regex.Match(attributevalue[i].Value, ssn).Success)
                            {
                                alerts["ssn"].Add($"Possible SSN match in Attribute Value\nAttributte: {attributevalue[i].Attribute.Name}\n\tFirst Name: {(attributevalue[i].CreatedByPersonAlias != null ? attributevalue[i].CreatedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(attributevalue[i].CreatedByPersonAlias != null ? attributevalue[i].CreatedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {attributevalue[i].Value}");
                            }
                            if (Regex.Match(attributevalue[i].Value, cc).Success)
                            {
                                alerts["cc"].Add($"Possible Credit Card match in Attribute Value\nAttributte: {attributevalue[i].Attribute.Name}\n\tFirst Name: {(attributevalue[i].CreatedByPersonAlias != null ? attributevalue[i].CreatedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(attributevalue[i].CreatedByPersonAlias != null ? attributevalue[i].CreatedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {attributevalue[i].Value}");
                            }
                            if (Regex.Match(attributevalue[i].Value, pass).Success)
                            {
                                alerts["pass"].Add($"Possible Passport Number match in Attribute Value\nAttributte: {attributevalue[i].Attribute.Name}\n\tFirst Name: {(attributevalue[i].CreatedByPersonAlias != null ? attributevalue[i].CreatedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(attributevalue[i].CreatedByPersonAlias != null ? attributevalue[i].CreatedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {attributevalue[i].Value}");
                            }
                            for (var j = 0; j < mo_drivers.Count(); j++)
                            {
                                if (Regex.Match(attributevalue[i].Value, mo_drivers[j]).Success)
                                {
                                    alerts["drivers"].Add($"Possible Driver's License Number match in Attribute Value\nAttributte: {attributevalue[i].Attribute.Name}\n\tFirst Name: {(attributevalue[i].CreatedByPersonAlias != null ? attributevalue[i].CreatedByPersonAlias.Person.FirstName : "")}\n\tLast Name: {(attributevalue[i].CreatedByPersonAlias != null ? attributevalue[i].CreatedByPersonAlias.Person.LastName : "")}\n\tMatched Text: {attributevalue[i].Value}");
                                }
                            }
                        }
                    }
                }

            }
            catch (Exception e)
            {
                System.IO.File.AppendAllText(@"C:\Users\CourtneyCooksey\Dropbox (The Crossing)\Code\Crossing\org.crossingchurch.PiiAlertpii_error.txt", $"{e.Message}\n{e.StackTrace}");
            }
            SendAlert(alerts, emails);
        }

        private void SendAlert(Dictionary<string, List<string>> alerts, string emails)
        {
            //Build Message Body
            var message = "<strong>PII Alert</strong>\n";
            foreach (var a in alerts)
            {
                foreach (var m in a.Value)
                {
                    message += $"{m}\n";
                }
            }
            var mail = new RockEmailMessage();
            mail.Message = message;
            mail.Subject = "PII Alert";
            //mail.SetRecipients(emails.Split(',').ToList());
            //try
            //{
            //    var output = new List<string>();
            //    mail.Send(out output);
            //    Console.WriteLine(output);
            //} catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
        }
    }
}
