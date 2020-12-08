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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using Rock;
using Rock.Attribute;
using Rock.CheckIn;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_thecrossingchurch.CheckIn
{
    /// <summary>
    /// Block used to display person and details about recent check-ins
    /// </summary>
    [DisplayName( "Reprint" )]
    [Category( "com_thecrossingchurch > Check-in Manager" )]
    [Description( "Reprint Check-in labels." )]

    public partial class Reprint : Rock.Web.UI.RockBlock
    {
        #region Page Parameter Constants

        private const string PERSON_ID_PAGE_QUERY_KEY = "Person";
        private const string DEVICE_ID_PAGE_QUERY_KEY = "DeviceId";
        private const string DATE_PAGE_QUERY_KEY = "Date";

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            RockPage.AddCSSLink( "~/Styles/fluidbox.css" );
            RockPage.AddScriptLink( "~/Scripts/imagesloaded.min.js" );
            RockPage.AddScriptLink( "~/Scripts/jquery.fluidbox.min.js" );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            ReprintLabels();
        }

        #endregion

        #region Methods

        // handlers called by the controls on your block

        /// <summary>
        /// Handles sending the selected labels off to the selected printer from the custom buttons.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ReprintLabels()
        {
            // Get the person Id from the Guid in the page parameter
            var rockContext = new RockContext();
            int personId = PageParameter( PERSON_ID_PAGE_QUERY_KEY ).AsInteger();
            int deviceId = PageParameter( DEVICE_ID_PAGE_QUERY_KEY ).AsInteger();
            var person = new PersonService( rockContext ).Get( personId );
            var device = new DeviceService( rockContext ).Get( deviceId );
            if ( person == null || deviceId == 0 )
            {
                return;
            }
            var date = DateTime.Today;
            if ( !String.IsNullOrEmpty( PageParameter( DATE_PAGE_QUERY_KEY ) ) )
            {
                date = DateTime.Parse( PageParameter( DATE_PAGE_QUERY_KEY ) );
                date = new DateTime( date.Year, date.Month, date.Day, 0, 0, 0 );
            }
            var selectedAttendance = new AttendanceService( rockContext ).Queryable().Where( a => a.PersonAliasId == person.PrimaryAliasId && DateTime.Compare( a.Occurrence.OccurrenceDate, date ) == 0 );
            var selectedAttendanceIds = selectedAttendance.Select( a => a.Id ).ToList();

            // Print all available label types
            var possibleLabels = ZebraPrint.GetLabelTypesForPerson( person.Id, selectedAttendanceIds );
            var fileGuids = possibleLabels.Select( pl => pl.FileGuid ).ToList();

            // Now, finally, re-print the labels.
            List<string> messages = ZebraPrint.ReprintZebraLabels( fileGuids, person.Id, selectedAttendanceIds, nbReprintMessage, this.Request, device.IPAddress );
            nbReprintMessage.Visible = true;
            nbReprintMessage.Text = messages.JoinStrings( "<br>" );
        }

        #endregion

        #region Helper Classes

        public class AttendanceInfo
        {
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public int GroupId { get; set; }
            public string Group { get; set; }
            public int LocationId { get; set; }
            public string Location { get; set; }
            public string Schedule { get; set; }
            public bool IsActive { get; set; }
            public string Code { get; set; }
            public string CheckInByPersonName { get; set; }
            public Guid? CheckInByPersonGuid { get; set; }
        }

        #endregion
    }
}