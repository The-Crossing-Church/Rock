using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;


using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using System.Text.RegularExpressions;
using System.Net.Sockets;
using System.Net;
using Rock.CheckIn;
using System.Data.Entity;

namespace RockWeb.Plugins.com_thecrossingchurch.Checkin
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Reprint Labels Local" )]
    [Category( "com_thecrossingchurch > Checkin" )]
    [Description( "Block that can be used to reprint labels to a particular networked printer." )]

    [IntegerField("Hours Back", "How many hours back should we check for attendance in addition to 'Current' attendance.", true, 2, "", 0)]
    [LinkedPage("Person Select Page", "Where to go when printing is done.", false, "", "", 5)]
    public partial class ReprintLabelsLocal : Rock.Web.UI.RockBlock
    {
        #region Fields

        private List<int> _personIds;
        private List<int> _labelIds;
        private Boolean printFromClient;
        private Guid? _personGuid;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            /* for client printing */
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/cordova-2.4.0.js", false );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/ZebraPrint.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            var rockContext = new RockContext();
            var personService = new PersonService(rockContext);

            _personGuid = PageParameter("Person").AsGuidOrNull();
            _personIds = Array.ConvertAll(PageParameter( "PersonIds" ).SplitDelimitedValues(), int.Parse).ToList();
            if (_personGuid.HasValue && !_personIds.Any())
            {
                _personIds = personService.Queryable().Where(p => p.Guid == _personGuid.Value).Select(p => p.Id).ToList();
            }
            _labelIds = Array.ConvertAll(PageParameter("LabelIds").SplitDelimitedValues(), int.Parse).ToList();


            var personGuids = personService.Queryable().Where(p => _personIds.Contains(p.Id)).Select(p => p.Guid).ToList();
            var binaryFileService = new BinaryFileService(rockContext);
            var labelGuids = binaryFileService.Queryable().Where(f => _labelIds.Contains(f.Id)).Select(f => f.Guid).ToList();


            if (_personIds.Any())
            {
                PrintLabels(personGuids, labelGuids);
            }
            else
            {
                lWarning.Text = "<div class='alert alert-warning'><strong>Warning</strong> A Person is required to reprint labels.</div>";
                return;
            }
        }

        #endregion

        #region Events
        /// <summary>
        /// Handles the Click event of the gCompileTheme control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void bOk_Click(object sender, EventArgs e)
        {
            var personService = new PersonService(new RockContext());
            var person = personService.Queryable().Where(p => p.Id == _personIds.FirstOrDefault()).FirstOrDefault();

            var queryParams = new Dictionary<string, string>() { { "Person", person.Guid.ToString() } };
            NavigateToLinkedPage("PersonSelectPage", queryParams);
        }

        #endregion

        #region Methods

        #endregion

        /// <summary>
        /// This will re-print out the person's check-in labels to the specified printer.  The
        /// person must be currently checked in.
        /// </summary>
        /// <param name="printerDeviceId">The printer device id.</param>
        /// <param name="personGuid">The person guid.</param>
        private void PrintLabels(List<Guid> selectedPeople, List<Guid> selectedLabels)
        {
            var labelsToPrint = new List<CheckInLabel>();

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService(rockContext);
            var personService = new PersonService(rockContext);
            var deviceService = new DeviceService(rockContext);
            var groupTypeService = new GroupTypeService(rockContext);
            var attendanceOccurrenceService = new AttendanceOccurrenceService(rockContext);

            // get schedules
            var schedules = new ScheduleService(rockContext)
                        .Queryable().AsNoTracking()
                        .Where(s => s.CheckInStartOffsetMinutes.HasValue)
                        .ToList();

            var scheduleIds = schedules.Select(s => s.Id).ToList();

            // get active schedules because we only want attendance that is currently active.
            var activeScheduleIds = new List<int>();
            foreach (var schedule in schedules)
            {
                if (schedule.WasScheduleOrCheckInActive(RockDateTime.Now))
                {
                    activeScheduleIds.Add(schedule.Id);
                }

            }

            // get the person's current check-in attendance.  We only want current attendance since this
            //  is all we need for re-printing labels and will also reduce the complexity of this feature.
            int hoursBack = GetAttributeValue("HoursBack").AsInteger();

            var attendanceQry = attendanceService.Queryable()
                   .Where(a => selectedPeople.Contains(a.PersonAlias.Person.Guid));

            if (hoursBack > 0)
            {
                DateTime hoursBackDate = DateTime.Now.AddHours(-hoursBack);

                attendanceQry = attendanceQry.Where(a => (a.StartDateTime > DateTime.Today && activeScheduleIds.Contains(a.Occurrence.ScheduleId.Value)) ||
                             (a.StartDateTime >= hoursBackDate));

            }
            else
            {
                attendanceQry = attendanceQry.Where(a => a.StartDateTime > DateTime.Today && activeScheduleIds.Contains(a.Occurrence.ScheduleId.Value));
            }

            var attendance = attendanceQry.ToList();

            if (!attendance.Any())
            {
                lWarning.Text = "<div class='alert alert-danger'><strong>Error</strong> The person is not currently checked in.</div>";
                return;
            }

            var attendancePeople = personService.Queryable().Where(p => selectedPeople.Contains(p.Guid));

            // re-create the checkin state from recent check-in attendance so that we can re-create the label merge objects.
            var CurrentCheckInState = new CheckInState(attendance.FirstOrDefault().DeviceId.Value, null, new List<int>());
            var checkinPeople = new List<CheckInPerson>();
            var checkinGroupTypes = new List<CheckInGroupType>();
            // re-create people
            foreach (var attendancePerson in attendancePeople)
            {
                var checkinPerson = new CheckInPerson();
                checkinPerson.Person = attendancePerson;
                checkinPerson.SecurityCode = attendance.Find(a => a.PersonAliasId == attendancePerson.PrimaryAliasId)
                    .AttendanceCode.Code;
                var personLabels = GetLabels(checkinPerson.Person, selectedLabels, new List<KioskLabel>());

                // re-create people
                checkinPeople.Add(checkinPerson);

                // re-create group types
                foreach (var groupType in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                    .DistinctBy(a => a.Occurrence.Group.GroupType).Select(g => g.Occurrence.Group.GroupType).ToList())

                {
                    // we need to iterate through the heirarchy twice.  This first iteration will setup the check-in state with all of the needed groups/locations/schedules
                    CheckInGroupType checkinGroupType = new CheckInGroupType();
                    foreach (var groupType2 in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                        .DistinctBy(a => a.Occurrence.Group.GroupType).Select(g => g.Occurrence.Group.GroupType).ToList())

                    {
                        checkinGroupType = new CheckInGroupType();
                        checkinGroupTypes.Add(checkinGroupType);
                        checkinGroupType.GroupType = GroupTypeCache.Get(groupType2.Guid);
                        checkinGroupType.Labels = new List<CheckInLabel>();
                        checkinGroupType.Selected = true;
                        checkinPerson.GroupTypes.Add(checkinGroupType);


                        CheckInGroup checkinGroup;
                        foreach (var group2 in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                            .Where(a => a.Occurrence.Group.GroupType.Id == groupType2.Id)
                            .DistinctBy(a => a.Occurrence.Group).Select(g => g.Occurrence.Group).ToList())
                        {
                            checkinGroup = new CheckInGroup();
                            checkinGroup.Group = group2;
                            checkinGroup.Selected = true;
                            checkinGroupType.Groups.Add(checkinGroup);

                            CheckInLocation checkinLocation;
                            foreach (var location2 in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                                .Where(a => a.Occurrence.GroupId == group2.Id)
                                .DistinctBy(a => a.Occurrence.Location).Select(l => l.Occurrence.Location).ToList())
                            {
                                checkinLocation = new CheckInLocation();
                                checkinLocation.Location = location2.Clone(false);
                                checkinGroup.Locations.Add(checkinLocation);
                                checkinGroup.Selected = true;

                                foreach (var schedule2 in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                                    .Where(a => a.Occurrence.GroupId == group2.Id && a.Occurrence.LocationId == location2.Id)
                                    .DistinctBy(a => a.Occurrence.Schedule).Select(s => s.Occurrence.Schedule).ToList())
                                {
                                    CheckInSchedule checkInSchedule = new CheckInSchedule();
                                    checkInSchedule.Schedule = schedule2.Clone(false);
                                    checkInSchedule.StartTime = RockDateTime.Today.Add(schedule2.StartTimeOfDay);
                                    checkinLocation.Schedules.Add(checkInSchedule);
                                    checkinLocation.Selected = true;
                                    checkinLocation.SelectedForSchedule.Add(schedule2.Id);
                                }
                            }
                        }
                    }

                    checkinGroupType.GroupType = GroupTypeCache.Get(groupType.Guid);
                    checkinGroupType.Labels = new List<CheckInLabel>();
                    checkinGroupType.Selected = true;

                    // re-create groups
                    // this second iteration will ensure that we get all of the labels added and merge fields resolved.
                    foreach (var group in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                        .Where(a => a.Occurrence.Group.GroupType.Id == groupType.Id)
                        .DistinctBy(a => a.Occurrence.Group).Select(g => g.Occurrence.Group).ToList())
                    {
                        CheckInGroup checkinGroup = new CheckInGroup();
                        checkinGroup.Group = group;
                        checkinGroup.Selected = true;

                        // re-create locations
                        foreach (var location in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                            .Where(a => a.Occurrence.GroupId == group.Id)
                            .DistinctBy(a => a.Occurrence.Location).Select(l => l.Occurrence.Location).ToList())
                        {
                            CheckInLocation checkinLocation = new CheckInLocation();
                            checkinLocation.Location = location.Clone(false);
                            checkinGroup.Selected = true;

                            // re-create schedules
                            foreach (var schedule in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                                .Where(a => a.Occurrence.GroupId == group.Id && a.Occurrence.LocationId == location.Id)
                                .DistinctBy(a => a.Occurrence.Schedule).Select(s => s.Occurrence.Schedule).ToList())
                            {
                                CheckInSchedule checkInSchedule = new CheckInSchedule();
                                checkInSchedule.Schedule = schedule.Clone(false);
                                checkInSchedule.StartTime = RockDateTime.Today.Add(schedule.StartTimeOfDay);
                                checkinLocation.Selected = true;
                                checkinLocation.SelectedForSchedule.Add(schedule.Id);
                            }

                        }
                    }
                }
            }

            // re-create labels
            var familyLabelsAdded = new List<Guid>();

            foreach (var checkinPerson in checkinPeople)
            {
                var personLabelsAdded = new List<Guid>();

                var personLabels = GetLabels(checkinPerson.Person, selectedLabels, new List<KioskLabel>());

                // re-create group types
                foreach (var groupType in checkinPerson.GroupTypes)
                {


                    var groupTypeLabels = GetLabels(groupType.GroupType, selectedLabels, personLabels);

                    // re-create groups
                    // this second iteration will ensure that we get all of the labels added and merge fields resolved.
                    foreach (var group in groupType.Groups)
                    {
                        var groupLabels = GetLabels(group.Group, selectedLabels, groupTypeLabels);

                        // re-create locations
                        foreach (var location in group.Locations)
                        {
                            var locationLabels = GetLabels(location.Location, selectedLabels, groupLabels);

                            // re-create labels
                            foreach (var labelCache in locationLabels.OrderBy(l => l.LabelType).ThenBy(l => l.Order))
                            {
                                checkinPerson.SetOptions(labelCache);

                                if (labelCache.LabelType == KioskLabelType.Family)
                                {
                                    if (familyLabelsAdded.Contains(labelCache.Guid) ||
                                        personLabelsAdded.Contains(labelCache.Guid))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        familyLabelsAdded.Add(labelCache.Guid);
                                    }
                                }
                                else if (labelCache.LabelType == KioskLabelType.Person)
                                {
                                    if (personLabelsAdded.Contains(labelCache.Guid))
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        personLabelsAdded.Add(labelCache.Guid);
                                    }
                                }

                                var commonMergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields(null);

                                var mergeObjects = new Dictionary<string, object>();
                                foreach (var keyValue in commonMergeFields)
                                {
                                    mergeObjects.Add(keyValue.Key, keyValue.Value);
                                }

                                mergeObjects.Add("Location", location);
                                mergeObjects.Add("Group", group);
                                mergeObjects.Add("Person", checkinPerson);
                                mergeObjects.Add("People", checkinPeople);
                                mergeObjects.Add("GroupType", groupType);

                                CheckInLabel checkinLabel = new CheckInLabel(labelCache, mergeObjects, checkinPerson.Person.Id);
                                checkinLabel.PrinterAddress = null;
                                checkinLabel.PrinterDeviceId = null;
                                checkinLabel.FileGuid = labelCache.Guid;

                                groupType.Labels.Add(checkinLabel);
                            }
                        }
                    }
                }
            }

            if (checkinGroupTypes.Any())
            {
                foreach (var checkinGroupType in checkinGroupTypes)
                {
                    if (checkinGroupType.Labels != null && checkinGroupType.Labels.Any())
                    {
                        labelsToPrint.AddRange(checkinGroupType.Labels);
                    }
                }
            }
            if (!labelsToPrint.Any())
            {
                lWarning.Text = "<div class='alert alert-danger'><strong>Alert</strong> No labels found to print.  Does the check-in area have labels configured?</div>";
                return;
            }

            // Send labels to printer
            // Note: This code was copied from the Success.ascx block since this code is not re-useable.
            //       In the future, it might nice to refactor this to use the refactored printing code in v8.
            if (labelsToPrint.Any())
            {
                string currentIp = string.Empty;

                    var urlRoot = string.Format("{0}://{1}", Request.Url.Scheme, Request.Url.Authority);

                    labelsToPrint
                        .OrderBy(l => l.PersonId)
                        .ThenBy(l => l.Order)
                        .ToList()
                        .ForEach(l => l.LabelFile = urlRoot + l.LabelFile);

                    AddLabelScript(labelsToPrint.ToJson());


            }
        }

        private string ZebraFormatString( string input, bool isJson = false )
        {
            if (isJson)
            {
                return input.Replace( "é", @"\\82" );  // fix acute e
            }
            else
            {
                return input.Replace( "é", @"\82" );  // fix acute e
            }
        }

        /// <summary>
        /// Gets the labels for an item (person, grouptype, group, location). 
        /// Note: Code orginally came from the CreateLabels.cs workflow action.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="existingLabels">The existing labels.</param>
        /// <returns></returns>
        public virtual List<KioskLabel> GetLabels(IHasAttributes item, List<Guid> selectedLabels, List<KioskLabel> existingLabels)
        {
            var labels = new List<KioskLabel>(existingLabels);

            if (item.Attributes == null)
            {
                item.LoadAttributes();
            }

            foreach (var attribute in item.Attributes.OrderBy(a => a.Value.Order))
            {
                if (attribute.Value.FieldType.Class == typeof(Rock.Field.Types.LabelFieldType).FullName)
                {
                    Guid? binaryFileGuid = item.GetAttributeValue(attribute.Key).AsGuidOrNull();
                    if (binaryFileGuid != null)
                    {
                        // is label in selected or is selectedLabels empty which means print all labels
                        if ( !labels.Any(l => l.Guid == binaryFileGuid.Value) 
                            && (selectedLabels.Contains(binaryFileGuid.Value) || !selectedLabels.Any()) )
                        {
                            var labelCache = KioskLabel.Get(binaryFileGuid.Value);
                            labelCache.Order = attribute.Value.Order;
                            if (labelCache != null && (
                                labelCache.LabelType == KioskLabelType.Family ||
                                labelCache.LabelType == KioskLabelType.Person ||
                                labelCache.LabelType == KioskLabelType.Location))
                            {
                                labels.Add(labelCache);
                            }
                        }
                    }
                }
            }

            return labels;
        }

        /// <summary>
        /// Adds the label script.
        /// </summary>
        /// <param name="jsonObject">The json object.</param>
        private void AddLabelScript(string jsonObject)
        {
            string script = string.Format(@"

        // setup deviceready event to wait for cordova
	    if (navigator.userAgent.match(/(iPhone|iPod|iPad)/)) {{
            document.addEventListener('deviceready', onDeviceReady, false);
        }} else {{
            $( document ).ready(function() {{
                onDeviceReady();
            }});
        }}

	    // label data
        var labelData = {0};

		function onDeviceReady() {{
            try {{		
                window.setTimeout('redirect()', 2000);
                printLabels();
            }} 
            catch (err) {{
                console.log('An error occurred printing labels: ' + err);
            }}
		}}

        function redirect() {{
            location.href = document.referrer;
        }}
		
		function alertDismissed() {{
		    // do something
		}}
		
		function printLabels() {{
		    ZebraPrintPlugin.printTags(
            	JSON.stringify(labelData), 
            	function(result) {{ 
			        console.log('Tag printed');
			    }},
			    function(error) {{   
				    // error is an array where:
				    // error[0] is the error message
				    // error[1] determines if a re-print is possible (in the case where the JSON is good, but the printer was not connected)
			        console.log('An error occurred: ' + error[0]);
                    navigator.notification.alert(
                        'An error occurred while printing the labels.' + error[0],  // message
                        alertDismissed,         // callback
                        'Error',            // title
                        'Ok'                  // buttonName
                    );
			    }}
            );
	    }}
", jsonObject);
            ScriptManager.RegisterStartupScript(this, this.GetType(), "addLabelScript", script, true);
        }

    }
}
