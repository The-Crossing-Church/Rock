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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using System;
using System.Data.Entity;
using Rock.Communication;
using System.Runtime.CompilerServices;
using DotLiquid.Util;

namespace org.crossingchurch.VeritasSupportChanges.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [GroupField("Fundraising Group", "Veritas Fundraising Group", true, "", "", 0)]
    [DisallowConcurrentExecution]
    public class VeritasSupportChanges : IJob
    {
        private RockContext _context { get; set; }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public VeritasSupportChanges() { }

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
            _context = new RockContext();

            string guid = dataMap.GetString("FundraisingGroup");
            int groupId = new GroupService(_context).Get(Guid.Parse(guid)).Id;
            var members = new GroupMemberService(_context).Queryable().Where(gm => gm.GroupId == groupId).ToList();
            for(var i = 0; i < members.Count(); i++)
            {
                //Get transaction data
                LoadTransactionData(members[i]);
            }
        }

        /// <summary>
        /// Get Account and Load Financial Transactions for last 24 hours
        /// </summary>
        private void LoadTransactionData(GroupMember groupMember)
        {
            //Load Attibutes and find Primary Fundraising Account
            groupMember.LoadAttributes();
            string accountGuid = groupMember.GetAttributeValue("FundraisingAccount");
            FinancialAccount account = new FinancialAccountService(_context).Get(Guid.Parse(accountGuid));
            List<FinancialTransaction> transactions = new FinancialTransactionService(_context).Queryable().Where(ft => ft.TransactionDetails.Any(td => td.AccountId == account.Id)).ToList();
            ProcessTransactions(transactions, groupMember, account);
        }

        /// <summary>
        /// Process Transaction Data
        /// </summary>
        private void ProcessTransactions(List<FinancialTransaction> transactions, GroupMember groupMember, FinancialAccount account)
        {
            DateTime dt = DateTime.Now.AddDays(-1);
            dt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0);
            var current = transactions.Where(t => DateTime.Compare(dt, t.TransactionDateTime.Value) <= 0).ToList();
            var message = "";
            for(var i = 0; i < current.Count(); i++)
            {
                bool isFirst = false;
                bool isChanged = false;
                bool isCanceled = false;
                //Check if this is first contribution to this account
                int contributor = current[i].AuthorizedPersonAliasId.Value;
                if(!transactions.Any(t => t.AuthorizedPersonAliasId == contributor && t.Id != current[i].Id))
                {
                    isFirst = true;
                }
                //Check if the scheduled transaction has changed
                if(current[i].ScheduledTransactionId.HasValue)
                {
                    FinancialTransaction lastTransaction = transactions.Where(t => t.ScheduledTransactionId == current[i].ScheduledTransactionId).OrderByDescending(t => t.TransactionDateTime).First();
                    if(lastTransaction.TransactionDetails.FirstOrDefault(td => td.AccountId == account.Id).Amount != current[i].TransactionDetails.FirstOrDefault(td => td.AccountId == account.Id).Amount)
                    {
                        isChanged = true;
                    }
                    //Check if the scheduled transaction has been cancelled
                    var schedule = new FinancialScheduledTransactionService(_context).Get(current[i].ScheduledTransactionId.Value);
                    if(!schedule.IsActive)
                    {
                        isCanceled = true;
                    }
                }

                //Add this transaction to message if different
                if(isChanged || isFirst || isCanceled)
                {
                    Person person = new PersonAliasService(_context).Get(contributor).Person;
                    //Title for Section
                    message += "<p style='font-size: 18px; font-weight: bold;'>";
                    if(isFirst)
                    {
                        message += "New Contibution From " + person.FullName;
                    }
                    else if(isCanceled)
                    {
                        message += person.FullName + " is No Longer Contributing";
                    }
                    else
                    {
                        message += "Recurring Contribution From " + person.FullName + " Has Changed";
                    }
                    message += "</p>\n";
                    //Information about transaction
                    message += "<p style='font-size: 14px;'>";
                    //message += "Gifted By: " + person.FullName + "\n";
                    message += "Amount: " + current[i].TransactionDetails.FirstOrDefault(td => td.AccountId == account.Id).Amount + "\n";
                    message += "Mobile Number: " + person.PhoneNumbers.FirstOrDefault(p => p.NumberTypeValue.Value == "Mobile") + "\n";
                    message += "Address: " + person.GetHomeLocation().FormattedAddress; 
                    message += "</p>\n\n";
                }
            }
            //Send Message
            if(message.IsNotNullOrWhiteSpace())
            {
                message = "<p style='font-size: 20px; font-weight: bold;'>Contibution Changes for " + account.Name + "</p>\n" + message;
                string subject = "Contibution Changes for " + account.Name;
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient(groupMember.Person, new Dictionary<string, object>());
                RockEmailMessage email = new RockEmailMessage();
                email.Subject = subject;
                email.Message = message;
                email.FromEmail = "info@thecrossingchurch.com";
                email.FromName = "The Crossing System";
                email.AddRecipient(recipient);
                var output = email.Send();
                Console.WriteLine(output);
            }
        }
    }
}
