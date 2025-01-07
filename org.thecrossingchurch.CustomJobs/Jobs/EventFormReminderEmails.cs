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
using Microsoft.Graph;
using Microsoft.Graph.Auth;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Rock;

namespace org.crossingchurch.PiiAlert.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    /// 
    [ContentChannelField( "Event Request", "", true, "" )]
    [LinkedPage( "Request Form", "", true )]
    [SecurityRoleField( "Event Request Admin", "", true )]
    [SecurityRoleField( "Room Request Admin", "", true )]
    [DisallowConcurrentExecution]
    public class EventFormReminderEmails : IJob
    {
        private RockContext _context { get; set; }
        private ContentChannelItemService _cciSvc { get; set; }
        private List<GroupMember> _eventAdmins { get; set; }
        private List<int?> _eventAdminAliasIds { get; set; }
        private List<GroupMember> _roomAdmins { get; set; }
        private List<int?> _roomAdminAliasIds { get; set; }
        private string _requestPage { get; set; }

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public EventFormReminderEmails()
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
            _context = new RockContext();
            _cciSvc = new ContentChannelItemService( _context );
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? ccGuid = Guid.Parse( dataMap.GetString( "EventRequest" ) );
            Guid? eventAdminGuid = Guid.Parse( dataMap.GetString( "EventRequestAdmin" ) );
            Guid? roomAdminGuid = Guid.Parse( dataMap.GetString( "RoomRequestAdmin" ) );
            Guid? requestPageGuid = Guid.Parse( dataMap.GetString( "RequestForm" ) );
            if ( eventAdminGuid.HasValue )
            {
                var eventAdminGroup = new GroupService( _context ).Get( eventAdminGuid.Value );
                _eventAdmins = eventAdminGroup.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
                _eventAdminAliasIds = _eventAdmins.Select( gm => gm.Person.PrimaryAliasId ).ToList();
            }
            if ( roomAdminGuid.HasValue )
            {
                var roomAdminGroup = new GroupService( _context ).Get( roomAdminGuid.Value );
                _roomAdmins = roomAdminGroup.Members.Where( m => m.GroupMemberStatus == GroupMemberStatus.Active ).ToList();
                _roomAdminAliasIds = _roomAdmins.Select( gm => gm.Person.PrimaryAliasId ).ToList();
            }
            if ( requestPageGuid.HasValue )
            {
                _requestPage = new PageService( _context ).Get( requestPageGuid.Value ).Id.ToString();
            }
            if ( ccGuid.HasValue )
            {
                ContentChannel cc = new ContentChannelService( _context ).Get( ccGuid.Value );
                getEventRequests( cc );
            }

        }

        public void getEventRequests( ContentChannel cc )
        {
            List<ContentChannelItem> allRequests = _cciSvc.Queryable().Where( i => i.ContentChannelId == cc.Id ).ToList();
            allRequests.LoadAttributes();
            //Filter to upcoming invalid requests
            allRequests = allRequests.Where( i =>
            {
                bool isValid = i.AttributeValues["RequestIsValid"].Value.AsBoolean();
                if ( isValid )
                {
                    return false;
                }
                List<DateTime> dates = i.AttributeValues["EventDates"].Value.Split( ',' ).Select( d => DateTime.Parse( d ) ).OrderBy( d => d ).ToList();
                if ( DateTime.Compare( RockDateTime.Now, dates[0] ) > 0 )
                {
                    return false;
                }
                return true;
            } ).ToList();
            var invalidPub = allRequests.Where( i => i.AttributeValues["RequestType"].Value.Contains( "Publicity" ) && !i.AttributeValues["ValidSections"].Value.Contains( "Publicity" ) ).ToList();
            checkDates( invalidPub, 49, new List<string>() { "Publicity" } );
            checkDates( invalidPub, 44, new List<string>() { "Publicity" } );
            var invalidCC = allRequests.Where( i => i.AttributeValues["RequestType"].Value.Contains( "Childcare" ) && !i.AttributeValues["ValidSections"].Value.Contains( "Childcare" ) || ( i.AttributeValues["RequestType"].Value.Contains( "Registration" ) && !i.AttributeValues["ValidSections"].Value.Contains( "Registration" ) ) ).ToList();
            checkDates( invalidCC, 37, new List<string>() { "Childcare", "Registration" } );
            checkDates( invalidCC, 32, new List<string>() { "Childcare", "Registration" } );
            var invalidOther = allRequests.Where( i => ( i.AttributeValues["RequestType"].Value.Contains( "Room" ) && !i.AttributeValues["ValidSections"].Value.Contains( "Room" ) ) || ( i.AttributeValues["RequestType"].Value.Contains( "Online Event" ) && !i.AttributeValues["ValidSections"].Value.Contains( "Online Event" ) ) || ( i.AttributeValues["RequestType"].Value.Contains( "Catering" ) && !i.AttributeValues["ValidSections"].Value.Contains( "Catering" ) ) || ( i.AttributeValues["RequestType"].Value.Contains( "Extra Resources" ) && !i.AttributeValues["ValidSections"].Value.Contains( "Extra Resources" ) ) ).ToList();
            checkDates( invalidOther, 21, new List<string>() );
            checkDates( invalidOther, 16, new List<string>() );
        }

        private void checkDates( List<ContentChannelItem> list, double numDays, List<string> resources )
        {
            //Target number of days from now
            DateTime reminderTarget = RockDateTime.Now.AddDays( numDays );
            reminderTarget = new DateTime( reminderTarget.Year, reminderTarget.Month, reminderTarget.Day, 0, 0, 0 );
            //Events where first event date is target number of days fom now. 
            list = list.Where( i =>
            {
                List<DateTime> dates = i.AttributeValues["EventDates"].Value.Split( ',' ).Select( d => DateTime.Parse( d ) ).OrderBy( d => d ).ToList();
                if ( DateTime.Compare( reminderTarget, dates[0] ) == 0 )
                {
                    return true;
                }
                return false;
            } ).ToList();
            //Send Communications
            for ( int i = 0; i < list.Count(); i++ )
            {
                ContentChannelItem item = list[i];
                Rock.Model.Person submitter = item.CreatedByPersonAlias.Person;
                if ( item.CreatedByPersonAliasId != item.ModifiedByPersonAliasId )
                {
                    //Person who created the event is not the last one to update, check if last was an event/room admin
                    if ( !_roomAdminAliasIds.Contains( item.ModifiedByPersonAliasId ) && !_eventAdminAliasIds.Contains( item.ModifiedByPersonAliasId ) )
                    {
                        //Since last person to modify was not an event or room admin, they must be in charge of the event now, email them instead
                        submitter = item.ModifiedByPersonAlias.Person;
                    }
                }
                //Check resources has value
                if ( resources.Count() == 0 )
                {
                    resources = item.AttributeValues["RequestType"].Value.Split( ',' ).Where( e => !item.AttributeValues["ValidSections"].Value.Split( ',' ).Contains( e ) ).ToList();
                    if ( resources.IndexOf( "Publicity" ) >= 0 )
                    {
                        resources.Remove( "Publicity" );
                    }
                    if ( resources.IndexOf( "Childcare" ) >= 0 )
                    {
                        resources.Remove( "Childcare" );
                    }
                    if ( resources.IndexOf( "Registration" ) >= 0 )
                    {
                        resources.Remove( "Registration" );
                    }
                }
                else
                {
                    //Check for Childcare/Registration that we don't accidentally notify about one when it is only the other that is invalid
                    resources = resources.Where( e => item.AttributeValues["RequestType"].Value.Split( ',' ).Contains( e ) && !item.AttributeValues["ValidSections"].Value.Split( ',' ).Contains( e ) ).ToList();
                }
                sendComm( item, submitter, resources, RockDateTime.Now.AddDays( 7 ) );
            }
        }

        private void sendComm( ContentChannelItem item, Rock.Model.Person submitter, List<string> resoures, DateTime deadline )
        {
            RockEmailMessage email = new RockEmailMessage();
            RockEmailMessageRecipient recipient = new RockEmailMessageRecipient( submitter, new Dictionary<string, object>() );
            email.AddRecipient( recipient );
            email.Subject = "Deadline Approaching for " + item.Title;
            string Source = String.Join( ", ", resoures );
            int Place = Source.LastIndexOf( "," );
            string result = Source;
            if ( Place > 0 )
            {
                result = Source.Remove( Place, 1 ).Insert( Place, " and" );
            }
            var header = new AttributeValueService( _context ).Queryable().FirstOrDefault( a => a.AttributeId == 140 ).Value; //Email Header
            var footer = new AttributeValueService( _context ).Queryable().FirstOrDefault( a => a.AttributeId == 141 ).Value; //Email Footer
            string BaseURL = "";
            Rock.Model.Attribute attr = new AttributeService( _context ).Queryable().FirstOrDefault( a => a.Key == "InternalApplicationRoot" );
            if ( attr != null )
            {
                BaseURL = new AttributeValueService( _context ).Queryable().FirstOrDefault( av => av.AttributeId == attr.Id ).Value;
                if ( !BaseURL.EndsWith( "/" ) )
                {
                    BaseURL += "/";
                }
            }
            email.Message = header + "The last day to complete all information for " + result + " is " + deadline.ToString( "dddd, MMMM dd, yyyy" ) + ". If information for " + ( resoures.Count() > 1 ? "these resources" : "this resource" ) + " is not completed by that date, the resource" + ( resoures.Count() > 1 ? "s" : "" ) + " will be removed from your request.<br/><br/>" +
                "<table style='width: 100%;'>" +
                    "<tr>" +
                        "<td></td>" +
                        "<td style='text-align: center;'>" +
                            "<a href='" + BaseURL + "page/" + _requestPage + "?Id=" + item.Id + "' style='background-color: rgb(5,69,87); color: #fff; font-weight: bold; font-size: 16px; padding: 15px;'>Modify Request</a>" +
                        "</td>" +
                        "<td></td>" +
                    "</tr>" +
                "</table>" +
                footer;
            email.FromEmail = "system@thecrossingchurch.com";
            email.FromName = "The Crossing System";
            email.CreateCommunicationRecord = true;
            var output = email.Send();
        }
    }
}
