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
using Rock.Address;

namespace org.crossingchurch.VeritasSupportChanges.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [GroupField( "Fundraising Group", "Veritas Fundraising Group", true, "", "", 0 )]
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
        public virtual void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            _context = new RockContext();

            string guid = dataMap.GetString( "FundraisingGroup" );
            int groupId = new GroupService( _context ).Get( Guid.Parse( guid ) ).Id;
            var members = new GroupMemberService( _context ).Queryable().Where( gm => gm.GroupId == groupId ).ToList();
            for ( var i = 0; i < members.Count(); i++ )
            {
                //Get transaction data
                LoadTransactionData( members[i] );
            }
        }

        /// <summary>
        /// Get Account and Load Financial Transactions for last 24 hours
        /// </summary>
        private void LoadTransactionData( GroupMember groupMember )
        {
            //Load Attibutes and find Primary Fundraising Account
            groupMember.LoadAttributes();
            string accountGuid = groupMember.GetAttributeValue( "FundraisingAccount" );
            FinancialAccount account = new FinancialAccountService( _context ).Get( Guid.Parse( accountGuid ) );
            List<FinancialTransaction> transactions = new FinancialTransactionService( _context ).Queryable().Where( ft => ft.TransactionDetails.Any( td => td.AccountId == account.Id ) ).ToList();
            ProcessTransactions( transactions, groupMember, account );
        }

        /// <summary>
        /// Process Transaction Data
        /// </summary>
        private void ProcessTransactions( List<FinancialTransaction> transactions, GroupMember groupMember, FinancialAccount account )
        {
            DateTime dt = DateTime.Now.AddDays( -1 );
            dt = new DateTime( dt.Year, dt.Month, dt.Day, 0, 0, 0 );
            var current = transactions.Where( t => DateTime.Compare( dt, t.TransactionDateTime.Value ) <= 0 ).ToList();
            var message = "";
            for ( var i = 0; i < current.Count(); i++ )
            {
                bool isFirst = false;
                bool isChanged = false;

                //Check if this is first contribution to this account
                int contributor = current[i].AuthorizedPersonAliasId.Value;
                if ( !transactions.Any( t => t.AuthorizedPersonAliasId == contributor && t.Id != current[i].Id ) )
                {
                    isFirst = true;
                }

                //Check if the scheduled transaction has changed
                if ( current[i].ScheduledTransactionId.HasValue )
                {
                    FinancialTransaction lastTransaction = transactions.Where( t => t.ScheduledTransactionId == current[i].ScheduledTransactionId && t.Id != current[i].Id ).OrderByDescending( t => t.TransactionDateTime ).FirstOrDefault();
                    if ( lastTransaction != null && lastTransaction.TransactionDetails.FirstOrDefault( td => td.AccountId == account.Id ).Amount != current[i].TransactionDetails.FirstOrDefault( td => td.AccountId == account.Id ).Amount )
                    {
                        isChanged = true;
                    }
                }

                //Add this transaction to message if different
                if ( isChanged || isFirst )
                {
                    Person person = new PersonAliasService( _context ).Get( contributor ).Person;
                    //Title for Section
                    message += "<p style='font-size: 18px; font-weight: bold;'>";
                    if ( isFirst )
                    {
                        message += "New Contibution From " + ( current[i].ShowAsAnonymous == true ? "an Anonymous Donor" : person.FullName );
                    }
                    else
                    {
                        message += "Recurring Contribution From " + ( current[i].ShowAsAnonymous == true ? "an Anonymous Donor" : person.FullName ) + " Has Changed";
                    }
                    message += "</p>\n<br/>";
                    //Information about transaction
                    message += "<p style='font-size: 14px;'>";
                    message += "Amount: $" + current[i].TransactionDetails.FirstOrDefault( td => td.AccountId == account.Id ).Amount + "\n<br/>";
                    if ( isChanged )
                    {
                        FinancialTransaction lastTransaction = transactions.Where( t => t.ScheduledTransactionId == current[i].ScheduledTransactionId && t.Id != current[i].Id ).OrderByDescending( t => t.TransactionDateTime ).First();
                        message += "Previous Amount: $" + lastTransaction.TransactionDetails.FirstOrDefault( td => td.AccountId == account.Id ).Amount + "\n<br/>";
                    }
                    message += ( current[i].ShowAsAnonymous == true ? "" : GetMobileNumber( person ) );
                    message += ( current[i].ShowAsAnonymous == true ? "" : GetAddress( person ) );
                    message += "</p>\n<br/>\n<br/>";
                }
            }

            //Check for potentially failed payments
            DateTime today = DateTime.Now;
            today = new DateTime( today.Year, today.Month, today.Day, 0, 0, 0 );
            var scheduled = new FinancialScheduledTransactionService( _context ).Queryable().Where( fst => fst.IsActive && fst.ScheduledTransactionDetails.Any( fstd => fstd.AccountId == account.Id ) && DateTime.Compare( dt, fst.NextPaymentDate.Value ) <= 0 && DateTime.Compare( today, fst.NextPaymentDate.Value ) > 0 ).ToList();
            var failed = scheduled.Where( s => !current.Select( c => c.ScheduledTransactionId ).Contains( s.Id ) ).ToList();
            for ( var i = 0; i < failed.Count(); i++ )
            {
                Person person = new PersonAliasService( _context ).Get( failed[i].AuthorizedPersonAliasId ).Person;
                //Title for Section
                message += "<p style='font-size: 18px; font-weight: bold;'>";
                message += "Potentially Failed Payment from " + ( current[i].ShowAsAnonymous == true ? "an Anonymous Donor" : person.FullName );
                message += "</p>\n<br/>";
                //Information about transaction
                message += "<p style='font-size: 14px;'>";
                message += "Amount: $" + failed[i].ScheduledTransactionDetails.FirstOrDefault( td => td.AccountId == account.Id ).Amount + "\n<br/>";
                message += ( current[i].ShowAsAnonymous == true ? "" : GetMobileNumber( person ) );
                message += ( current[i].ShowAsAnonymous == true ? "" : GetAddress( person ) );
                message += "</p>\n<br/>\n<br/>";
            }

            //Send Message
            if ( !String.IsNullOrWhiteSpace( message ) )
            {
                var header = new AttributeValueService( _context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
                var footer = new AttributeValueService( _context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer 
                message = header + "<p style='font-size: 20px; font-weight: bold;'>Contibution Changes for " + account.Name + "</p>\n<br/>" + message + footer;
                string subject = "Contibution Changes for " + account.Name;
                RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( groupMember.Person, new Dictionary<string, object>() );
                RockEmailMessage email = new RockEmailMessage();
                email.Subject = subject;
                email.Message = message;
                email.FromEmail = "info@thecrossingchurch.com";
                email.FromName = "The Crossing System";
                email.AddRecipient( recipient );
                var output = email.Send();
                Console.WriteLine( output );
            }
        }

        private string GetMobileNumber( Person person )
        {
            var phone = person.PhoneNumbers.FirstOrDefault( p => p.NumberTypeValue.Value == "Mobile" );
            if ( phone == null )
            {
                phone = person.PhoneNumbers.FirstOrDefault();
            }
            if ( phone != null )
            {
                return phone.NumberTypeValue.Value + " Number: " + phone.NumberFormatted + "\n<br/>";
            }
            return "Phone Number not available.";
        }

        private string GetAddress( Person person )
        {
            var addr = person.GetHomeLocation();
            if ( addr == null )
            {
                addr = person.GetMailingLocation();
            }
            if ( addr != null )
            {
                return "Address: " + addr.FormattedAddress;
            }
            return "Address not available.";
        }
    }
}