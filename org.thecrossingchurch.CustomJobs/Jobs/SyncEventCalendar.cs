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

namespace org.crossingchurch.PiiAlert.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    [TextField( "MicrosoftTennant", "MS Tennant for Graph API", true )]
    [TextField( "MicrosoftClientID", "MS Client ID for Graph API", true )]
    [TextField( "MicrosoftClientSecret", "MS Client Secret for Graph API", true )]
    [DisallowConcurrentExecution]
    public class SyncEventCalendar : IJob
    {

        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public SyncEventCalendar()
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
            getEvents( dataMap );

        }

        public void getEvents( JobDataMap dataMap )
        {
            string tennant = dataMap.GetString( "MicrosoftTennant" );
            string clientId = dataMap.GetString( "MicrosoftClientID" );
            string clientSecret = dataMap.GetString( "MicrosoftClientSecret" );
            IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            .Create( clientId )
            .WithTenantId( tennant )
            .WithClientSecret( clientSecret )
            .Build();

            ClientCredentialProvider authProvider = new ClientCredentialProvider( confidentialClientApplication );
            GraphServiceClient graphClient = new GraphServiceClient( authProvider );
            //var graphTask = graphClient.Me.Events
            //    .Request()
            //    //.Header( "Prefer", "outlook.timezone=\"Pacific Standard Time\"" )
            //    .Select( "subject,body,bodyPreview,organizer,attendees,start,end,location" )
            //    .GetAsync();
            //graphTask.RunSynchronously();
            //var result = graphTask.Result; 
            var graphTask = Task.Run( async () =>
            {
                await graphClient.Me.Events
                .Request()
                //.Header( "Prefer", "outlook.timezone=\"Pacific Standard Time\"" )
                .Select( "subject,body,bodyPreview,organizer,attendees,start,end,location" )
                .GetAsync();
            } );
            graphTask.Wait();
            Console.WriteLine( "ey" );
        }
    }
}
