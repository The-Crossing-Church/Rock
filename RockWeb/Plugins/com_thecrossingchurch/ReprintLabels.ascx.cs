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
using System.Web;

namespace RockWeb.Plugins.com_thecrossingchurch.Checkin
{
    /// <summary>
    /// Displays the details of a Referral Agency.
    /// </summary>
    [DisplayName( "Reprint Labels" )]
    [Category( "com_thecrossingchurch > Checkin" )]
    [Description( "Block that can be used to reprint labels to a particular networked printer." )]

    [IntegerField( "Hours Back", "How many hours back should we check for attendance in addition to 'Current' attendance.", true, 2, "", 0 )]
    [BooleanField("Display Others Checked In", "Should a list of all people included in that check-in be displayed", true, order: 3)]
    [BooleanField("Display Labels to Print", "Select which labels will be printed", true, order: 4)]
    [LinkedPage("Reprint Labels Local Page", "Page with the Reprint Labels Local block for local printing (ip=0.0.0.0).", false, "", "", 5)]
    public partial class ReprintLabels : Rock.Web.UI.RockBlock
    {
        #region Fields

        private Guid? _personGuid;
        private Boolean _showPeople;
        private Boolean _showLabels;
        private List<int> _labelIds;
        private Boolean? _selectAllPeople;
        private List<Person> _personList;
        private int? _deviceId;
        private string _prevPage = String.Empty;

        #endregion

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gPrinters.ShowActionRow = false;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            _personGuid = PageParameter( "Person" ).AsGuidOrNull();
            _deviceId = PageParameter("DeviceId").AsIntegerOrNull();
            _selectAllPeople = PageParameter("SelectAll").AsBooleanOrNull();
            _showPeople = GetAttributeValue("DisplayOthersCheckedIn").AsBoolean();
            _showLabels = GetAttributeValue("DisplayLabelstoPrint").AsBoolean();
            _labelIds = Array.ConvertAll(PageParameter("LabelIds").SplitDelimitedValues(), int.Parse).ToList();

            if (!_personGuid.HasValue)
            {
                lWarning.Text = "<div class='alert alert-warning'><strong>Warning</strong> A Person is required to reprint labels.</div>";
                return;
            }

            if (!Page.IsPostBack)
            {
                _prevPage = Request.UrlReferrer.ToString();

                if ( !_showPeople )
                {
                    pnlPeople.Visible = false;
                }
                if ( !_showLabels )
                {
                    pnlLabels.Visible = false; ;
                }
                if (_deviceId.HasValue)
                {
                    /// If local printer selected (Name = "Local")
                    var rockContext = new RockContext();
                    var deviceService = new DeviceService(rockContext);
                    var printerDevice = deviceService.Queryable().Where(d => d.Id == _deviceId).FirstOrDefault();

                    var selectedPeopleGuids = new List<Guid>();
                    if (_selectAllPeople.HasValue && _selectAllPeople.Value)
                    {
                        _personList = GetPeopleList(_personGuid.Value);
                        if (_personList != null)
                        {
                            foreach (var person in _personList)
                            {
                                selectedPeopleGuids.Add(person.Guid);
                            }
                        }
                        else
                        {
                            selectedPeopleGuids.Add(_personGuid.Value);
                        }
                    }
                    else
                    {
                        selectedPeopleGuids.Add(_personGuid.Value);
                    }

                    var labelList = GetLabelList(_personGuid.Value, new List<BinaryFile>());
                    if (labelList != null)
                    {
                        if (_labelIds.Any())
                        {
                            labelList = labelList.AsQueryable().Where(l => _labelIds.Contains(l.Id)).ToList();
                        }
                        var selectedLabels = labelList.AsQueryable().Select(x => x.Guid.ToString()).ToList();
                        var selectedLabelGuids = selectedLabels.ConvertAll(Guid.Parse);

                        if (printerDevice.IsValid && printerDevice.Name == "Local")
                        {
                            if (GetAttributeValue("ReprintLabelsLocalPage").IsNull())
                            {
                                lWarning.Text = "<div class='alert alert-warning'><strong>Warning</strong> Local print page not set on block.</div>";
                                return;
                            }
                            var personService = new PersonService(rockContext);
                            var personIds = personService.Queryable().Where(p => selectedPeopleGuids.Contains(p.Guid)).Select(p => p.Id).ToList();
                            var binaryFileService = new BinaryFileService(rockContext);
                            var labelIds = binaryFileService.Queryable().Where(f => selectedLabelGuids.Contains(f.Guid)).Select(f => f.Id).ToList();
                            var queryParams = new Dictionary<string, string>() { { "PersonIds", String.Join(",", personIds) },
                            { "LabelIds", String.Join(",", labelIds) }};

                            NavigateToLinkedPage("ReprintLabelsLocalPage", queryParams);
                        }
                        else
                        {
                            PrintLabels(_deviceId.Value, selectedPeopleGuids, selectedLabelGuids);
                        }
                    }
                    else
                    {
                        lWarning.Text = "<div class='alert alert-warning'><strong>Warning</strong> - Person not currently checked in.</div>";
                    }

                    if (lWarning.Text != "")
                    {
                        _prevPage = _prevPage + "&ErrorMsg=" + Regex.Replace(lWarning.Text, "<.*?>", String.Empty);
                    }
                    Response.Redirect(_prevPage);
                }
                else
                {
                    BindGrid();
                    BindPeopleGrid();
                    BindLabelGrid();
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
            BindPeopleGrid();
            BindLabelGrid();
            if ( !_showPeople )
            {
                pnlPeople.Visible = false;
            }
            if ( !_showLabels )
            {
                pnlLabels.Visible = false;
            }
        }

        /// <summary>
        /// Handles the Click event of the gCompileTheme control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPrint_Click( object sender, RowEventArgs e )
        {
            var deviceId = e.RowKeyValue.ToString().AsInteger();

            var selectedPeople = gCheckInGroup.SelectedKeys.AsQueryable().Select(x => x.ToString()).ToList();
            var selectedPeopleGuids = selectedPeople.ConvertAll(Guid.Parse);

            var selectedLabels = gLabels.SelectedKeys.AsQueryable().Select(x => x.ToString()).ToList();
            var selectedLabelGuids = selectedLabels.ConvertAll(Guid.Parse);

            if (deviceId > 0 && selectedPeople.Any())
            {
                /// If local printer selected (Name = "Local")
                var rockContext = new RockContext();
                var deviceService = new DeviceService(rockContext);
                var printerDevice = deviceService.Queryable().Where( d => d.Id == deviceId ).FirstOrDefault();

                if (printerDevice.IsValid && printerDevice.Name == "Local")
                {
                    if ( GetAttributeValue("ReprintLabelsLocalPage").IsNull() )
                    {
                        lWarning.Text = "<div class='alert alert-warning'><strong>Warning</strong> Local print page not set on block.</div>";
                        return;
                    }
                    var personService = new PersonService(rockContext);
                    var personIds = personService.Queryable().Where(p => selectedPeopleGuids.Contains(p.Guid)).Select(p => p.Id).ToList();
                    var binaryFileService = new BinaryFileService(rockContext);
                    var labelIds = binaryFileService.Queryable().Where(f => selectedLabelGuids.Contains(f.Guid)).Select(f => f.Id).ToList();
                    var queryParams = new Dictionary<string, string>() { { "PersonIds", String.Join(",", personIds) },
                        { "LabelIds", String.Join(",", labelIds) }};

                    NavigateToLinkedPage("ReprintLabelsLocalPage", queryParams);
                }
                else
                {
                    PrintLabels(deviceId, selectedPeopleGuids, selectedLabelGuids);
                }

            }
        }

        #endregion

        #region Methods

        private void BindGrid()
        {
            var printerValueId = DefinedValueCache.Get( Rock.SystemGuid.DefinedValue.DEVICE_TYPE_PRINTER ).Id; //9-11: V8 Update 

            var rockContext = new RockContext();
            var deviceService = new DeviceService( rockContext );

            var printers = deviceService.Queryable()
                .Where( d => d.DeviceTypeValueId == printerValueId &&
                             d.IPAddress != "" )
                .OrderBy( d => d.Name )
                .ToList();

            gPrinters.DataSource = printers;
            gPrinters.DataBind();

            if (!printers.Any())
            {
                lWarning.Text = "<div class='alert alert-warning'><strong>Warning</strong> No available printers.  Please have at least one printer created that has an IP address.</div>";
            }
        }
        private void BindPeopleGrid()
        {
            _personList = GetPeopleList(_personGuid.Value);

            if (_personList != null && _personList.Any())
            {
                gCheckInGroup.DataSource = _personList;
                gCheckInGroup.DataKeyNames = new string[] { "Guid" };

                gCheckInGroup.DataBind();

                // set Url person only to selected
                foreach ( var col in gCheckInGroup.ColumnsOfType<SelectField>())
                {
                    var row = 0;
                    foreach( var item in _personList)
                    {
                        if (item.Guid == _personGuid.Value)
                        {
                            ((CheckBox)gCheckInGroup.Rows[row].Cells[col.ColumnIndex].Controls[0]).Checked = true;
                        }
                        row++;
                    }
                }
            }
        }
        private void BindLabelGrid()
        {
            var labelList = GetLabelList(_personGuid.Value, new List<BinaryFile>());

            if (labelList != null)
            {

                if (_personList != null)
                {
                    foreach (Person item in _personList)
                    {
                        var personGuid = item.Guid;
                        labelList = GetLabelList(personGuid, labelList);
                    }
                }
                gLabels.DataSource = labelList;
                gLabels.DataKeyNames = new string[] { "Guid" };
                gLabels.DataBind();

                // Set all to selected
                foreach (var col in gLabels.ColumnsOfType<SelectField>())
                {
                    for (var row=0; row <  gLabels.Rows.Count; row++)
                    {
                        ((CheckBox)gLabels.Rows[row].Cells[col.ColumnIndex].Controls[0]).Checked = true;
                    }
                }


                if (!labelList.Any())
                {
                    lWarning.Text = "<div class='alert alert-warning'><strong>Warning</strong> No labels to be printed.</div>";
                }
            }
        }

        #endregion

        /// <summary>
        /// This will re-print out the person's check-in labels to the specified printer.  The
        /// person must be currently checked in.
        /// </summary>
        /// <param name="printerDeviceId">The printer device id.</param>
        /// <param name="personGuid">The person guid.</param>
        private void PrintLabels( int printerDeviceId, List<Guid>selectedPeople, List<Guid>selectedLabels )
        {
            var labelsToPrint = new List<CheckInLabel>();

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService( rockContext );
            var personService = new PersonService( rockContext );
            var deviceService = new DeviceService( rockContext );
            var groupTypeService = new GroupTypeService( rockContext );
            var attendanceOccurrenceService = new AttendanceOccurrenceService( rockContext );

            var printerDevice = deviceService.Queryable().Where( d => d.Id == printerDeviceId ).FirstOrDefault();

            // get schedules
            var schedules = new ScheduleService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( s => s.CheckInStartOffsetMinutes.HasValue )
                        .ToList();

            var scheduleIds = schedules.Select( s => s.Id ).ToList();

            // get active schedules because we only want attendance that is currently active.
            var activeScheduleIds = new List<int>();
            foreach (var schedule in schedules)
            {
                if (schedule.WasScheduleOrCheckInActive( RockDateTime.Now ))
                {
                    activeScheduleIds.Add( schedule.Id );
                }

            }

            // get the person's current check-in attendance.  We only want current attendance since this
            //  is all we need for re-printing labels and will also reduce the complexity of this feature.
            int hoursBack = GetAttributeValue( "HoursBack" ).AsInteger();

         var attendanceQry = attendanceService.Queryable()
                .Where( a => selectedPeople.Contains(a.PersonAlias.Person.Guid) );

            if (hoursBack > 0)
            {
                DateTime hoursBackDate = DateTime.Now.AddHours( -hoursBack );

                attendanceQry = attendanceQry.Where( a => (a.StartDateTime > DateTime.Today && activeScheduleIds.Contains( a.Occurrence.ScheduleId.Value )) ||
                              (a.StartDateTime >= hoursBackDate) ); 

            }
            else
            {
                attendanceQry = attendanceQry.Where( a => a.StartDateTime > DateTime.Today && activeScheduleIds.Contains( a.Occurrence.ScheduleId.Value ) );
            }

            var attendance = attendanceQry.ToList();

            if (!attendance.Any())
            {
                lWarning.Text = "<div class='alert alert-danger'><strong>Error</strong> - The person is not currently checked in.</div>";
                return;
            }

            var attendancePeople = personService.Queryable().Where( p => selectedPeople.Contains(p.Guid) );

            // re-create the checkin state from recent check-in attendance so that we can re-create the label merge objects.
            var CurrentCheckInState = new CheckInState( attendance.FirstOrDefault().DeviceId.Value, null, new List<int>() );
            var checkinPeople = new List<CheckInPerson>();
            var checkinGroupTypes = new List<CheckInGroupType>();

            // re-create people
            foreach (var attendancePerson in attendancePeople)
            {
                var checkinPerson = new CheckInPerson();
                checkinPerson.Person = attendancePerson;
                checkinPerson.SecurityCode = attendance.Find(a => a.PersonAliasId == attendancePerson.PrimaryAliasId)
                    .AttendanceCode.Code;
                var personLabels = GetLabels( checkinPerson.Person, selectedLabels, new List<KioskLabel>() );

                // re-create people
                checkinPeople.Add( checkinPerson );

                // re-create group types
                foreach (var groupType in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                    .DistinctBy( a => a.Occurrence.Group.GroupType ).Select( g => g.Occurrence.Group.GroupType ).ToList())

                {
                    // we need to iterate through the heirarchy twice.  This first iteration will setup the check-in state with all of the needed groups/locations/schedules
                    CheckInGroupType checkinGroupType = new CheckInGroupType();
                    foreach (var groupType2 in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                        .DistinctBy( a => a.Occurrence.Group.GroupType ).Select( g => g.Occurrence.Group.GroupType ).ToList())

                    {
                        checkinGroupType = new CheckInGroupType();
                        checkinGroupTypes.Add(checkinGroupType);
                        checkinGroupType.GroupType = GroupTypeCache.Get( groupType2.Guid );
                        checkinGroupType.Labels = new List<CheckInLabel>();
                        checkinGroupType.Selected = true;
                        checkinPerson.GroupTypes.Add(checkinGroupType);


                        CheckInGroup checkinGroup;
                        foreach (var group2 in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                            .Where( a => a.Occurrence.Group.GroupType.Id == groupType2.Id )
                            .DistinctBy( a => a.Occurrence.Group ).Select( g => g.Occurrence.Group ).ToList())
                        {
                            checkinGroup = new CheckInGroup();
                            checkinGroup.Group = group2;
                            checkinGroup.Selected = true;
                            checkinGroupType.Groups.Add( checkinGroup );

                            CheckInLocation checkinLocation;
                            foreach (var location2 in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                                .Where( a => a.Occurrence.GroupId == group2.Id )
                                .DistinctBy( a => a.Occurrence.Location ).Select( l => l.Occurrence.Location ).ToList())
                            {
                                checkinLocation = new CheckInLocation();
                                checkinLocation.Location = location2.Clone( false );
                                checkinGroup.Locations.Add( checkinLocation );
                                checkinGroup.Selected = true;

                                foreach (var schedule2 in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                                    .Where( a => a.Occurrence.GroupId == group2.Id && a.Occurrence.LocationId == location2.Id )
                                    .DistinctBy( a => a.Occurrence.Schedule ).Select( s => s.Occurrence.Schedule ).ToList())
                                {
                                    CheckInSchedule checkInSchedule = new CheckInSchedule();
                                    checkInSchedule.Schedule = schedule2.Clone( false );
                                    checkInSchedule.StartTime = RockDateTime.Today.Add( schedule2.StartTimeOfDay );
                                    checkinLocation.Schedules.Add( checkInSchedule );
                                    checkinLocation.Selected = true;
                                    checkinLocation.SelectedForSchedule.Add( schedule2.Id );
                                }
                            }
                        }
                    }

                    checkinGroupType.GroupType = GroupTypeCache.Get( groupType.Guid );
                    checkinGroupType.Labels = new List<CheckInLabel>();
                    checkinGroupType.Selected = true;

                    // re-create groups
                    // this second iteration will ensure that we get all of the labels added and merge fields resolved.
                    foreach (var group in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                        .Where( a => a.Occurrence.Group.GroupType.Id == groupType.Id )
                        .DistinctBy( a => a.Occurrence.Group ).Select( g => g.Occurrence.Group ).ToList())
                    {
                        CheckInGroup checkinGroup = new CheckInGroup();
                        checkinGroup.Group = group;
                        checkinGroup.Selected = true;

                        // re-create locations
                        foreach (var location in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                            .Where( a => a.Occurrence.GroupId == group.Id )
                            .DistinctBy( a => a.Occurrence.Location ).Select( l => l.Occurrence.Location ).ToList())
                        {
                            CheckInLocation checkinLocation = new CheckInLocation();
                            checkinLocation.Location = location.Clone( false );
                            checkinGroup.Selected = true;

                            // re-create schedules
                            foreach (var schedule in attendance.Where(a => a.PersonAlias.Person.Guid == attendancePerson.Guid)
                                .Where( a => a.Occurrence.GroupId == group.Id && a.Occurrence.LocationId == location.Id )
                                .DistinctBy( a => a.Occurrence.Schedule ).Select( s => s.Occurrence.Schedule ).ToList())
                            {
                                CheckInSchedule checkInSchedule = new CheckInSchedule();
                                checkInSchedule.Schedule = schedule.Clone( false );
                                checkInSchedule.StartTime = RockDateTime.Today.Add( schedule.StartTimeOfDay );
                                checkinLocation.Selected = true;
                                checkinLocation.SelectedForSchedule.Add( schedule.Id );
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
                                checkinLabel.PrinterAddress = printerDevice.IPAddress;
                                checkinLabel.PrinterDeviceId = printerDevice.Id;
                                checkinLabel.FileGuid = labelCache.Guid;

                                groupType.Labels.Add(checkinLabel);
                            }
                        }
                    }
                }
            }

            if ( checkinGroupTypes.Any() )
            {
                foreach ( var checkinGroupType in checkinGroupTypes )
                {
                    if (checkinGroupType.Labels != null && checkinGroupType.Labels.Any())
                    {
                        labelsToPrint.AddRange( checkinGroupType.Labels );
                    }
                }
            }
            if ( !labelsToPrint.Any() )
            {
                lWarning.Text = "<div class='alert alert-danger'><strong>Alert</strong> No labels found to print.  Does the check-in area have labels configured?</div>";
                return;
            }


            // Send labels to printer
            // Note: This code was copied from the Success.ascx block since this code is not re-useable.
            //       In the future, it might nice to refactor this to use the refactored printing code in v8.
            if (labelsToPrint.Any())
            {
                Socket socket = null;
                string currentIp = string.Empty;

                foreach (var label in labelsToPrint
                    .OrderBy( l => l.PersonId )
                    .ThenBy( l => l.Order ))
                {
                    var labelCache = KioskLabel.Get( label.FileGuid );
                    if (labelCache != null)
                    {
                        if (!string.IsNullOrWhiteSpace( label.PrinterAddress ))
                        {
                            if (label.PrinterAddress != currentIp)
                            {
                                if (socket != null && socket.Connected)
                                {
                                    socket.Shutdown( SocketShutdown.Both );
                                    socket.Close();
                                }

                                currentIp = label.PrinterAddress;
                                int printerPort = 9100;
                                var printerIp = currentIp;

                                // If the user specified in 0.0.0.0:1234 syntax then pull our the IP and port numbers.
                                if (printerIp.Contains( ":" ))
                                {
                                    var segments = printerIp.Split( ':' );

                                    printerIp = segments[0];
                                    printerPort = segments[1].AsInteger();
                                }

                                var printerEndpoint = new IPEndPoint( IPAddress.Parse( currentIp ), printerPort );

                                socket = new Socket( AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp );
                                IAsyncResult result = socket.BeginConnect( printerEndpoint, null, null );
                                bool success = result.AsyncWaitHandle.WaitOne( 5000, true );
                            }

                            string printContent = labelCache.FileContent;

                            foreach (var mergeField in label.MergeFields)
                            {
                                if (!string.IsNullOrWhiteSpace( mergeField.Value ))
                                {
                                    printContent = Regex.Replace( printContent, string.Format( @"(?<=\^FD){0}(?=\^FS)", mergeField.Key ), ZebraFormatString( mergeField.Value ) );
                                }
                                else
                                {
                                    // Remove the box preceding merge field
                                    printContent = Regex.Replace( printContent, string.Format( @"\^FO.*\^FS\s*(?=\^FT.*\^FD{0}\^FS)", mergeField.Key ), string.Empty );
                                    // Remove the merge field
                                    printContent = Regex.Replace( printContent, string.Format( @"\^FD{0}\^FS", mergeField.Key ), "^FD^FS" );
                                }
                            }

                            if (socket.Connected)
                            {
                                var ns = new NetworkStream( socket );
                                byte[] toSend = System.Text.Encoding.ASCII.GetBytes( printContent );
                                ns.Write( toSend, 0, toSend.Length );
                            }
                            else
                            {
                                lWarning.Text = "<div class='alert alert-danger'><strong>Error</strong> - Could not connect to printer.</div>";
                            }
                        }
                    }
                }

                if (socket != null && socket.Connected)
                {
                    socket.Shutdown( SocketShutdown.Both );
                    socket.Close();
                }
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
        /// This will re-print out the person's check-in labels to the specified printer.  The
        /// person must be currently checked in.
        /// </summary>
        /// <param name="printerDeviceId">The printer device id.</param>
        /// <param name="personGuid">The person guid.</param>
        protected virtual List<BinaryFile> GetLabelList(Guid personGuid, List<BinaryFile> existingLabels)
        {
            var labels = new List<BinaryFile>(existingLabels);

            var rockContext = new RockContext();
            var attendanceService = new AttendanceService(rockContext);
            var personService = new PersonService(rockContext);
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
                .Where(a => a.PersonAlias.Person.Guid == personGuid);

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
                lWarning.Text = "<div class='alert alert-danger'><strong>Error</strong> - The person is not currently checked in.</div>";
                return null;
            }

            var attendancePerson = personService.Queryable().Where(p => p.Guid == personGuid).FirstOrDefault();

            // re-create the checkin state from recent check-in attendance so that we can re-create the label merge objects.
            var CurrentCheckInState = new CheckInState(attendance.FirstOrDefault().DeviceId.Value, null, new List<int>());

            // re-create person
            var checkinPerson = new CheckInPerson();
            checkinPerson.Person = attendancePerson;
            checkinPerson.SecurityCode = attendance.FirstOrDefault().AttendanceCode.Code;
            labels = GetLabelNames(checkinPerson.Person, labels);

            // re-create people - even though we will be adding the one person
            var checkinPeople = new List<CheckInPerson>();
            checkinPeople.Add(checkinPerson);

            // re-create group types
            var checkinGroupTypes = new List<CheckInGroupType>();
            foreach (var groupType in attendance.DistinctBy(a => a.Occurrence.Group.GroupType).Select(g => g.Occurrence.Group.GroupType).ToList())

            {
                // we need to iterate through the heirarchy twice.  This first iteration will setup the check-in state with all of the needed groups/locations/schedules
                CheckInGroupType checkinGroupType = new CheckInGroupType();
                foreach (var groupType2 in attendance.DistinctBy(a => a.Occurrence.Group.GroupType).Select(g => g.Occurrence.Group.GroupType).ToList())

                {
                    checkinGroupType = new CheckInGroupType();
                    checkinGroupType.GroupType = GroupTypeCache.Get(groupType2.Guid);
                    checkinGroupType.Labels = new List<CheckInLabel>();
                    checkinGroupType.Selected = true;
                    checkinPerson.GroupTypes.Add(checkinGroupType);

                    CheckInGroup checkinGroup;
                    foreach (var group2 in attendance.Where(a => a.Occurrence.Group.GroupType.Id == groupType2.Id).DistinctBy(a => a.Occurrence.Group).Select(g => g.Occurrence.Group).ToList())
                    {
                        checkinGroup = new CheckInGroup();
                        checkinGroup.Group = group2;
                        checkinGroup.Selected = true;
                        checkinGroupType.Groups.Add(checkinGroup);

                        CheckInLocation checkinLocation;
                        foreach (var location2 in attendance.Where(a => a.Occurrence.GroupId == group2.Id).DistinctBy(a => a.Occurrence.Location).Select(l => l.Occurrence.Location).ToList())
                        {
                            checkinLocation = new CheckInLocation();
                            checkinLocation.Location = location2.Clone(false);
                            checkinGroup.Locations.Add(checkinLocation);
                            checkinGroup.Selected = true;

                            foreach (var schedule2 in attendance.Where(a => a.Occurrence.GroupId == group2.Id && a.Occurrence.LocationId == location2.Id).DistinctBy(a => a.Occurrence.Schedule).Select(s => s.Occurrence.Schedule).ToList())
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


                var familyLabelsAdded = new List<Guid>();
                var personLabelsAdded = new List<Guid>();

                labels = GetLabelNames(checkinGroupType.GroupType, labels);

                // re-create groups
                // this second iteration will ensure that we get all of the labels added and merge fields resolved.
                foreach (var group in attendance.Where(a => a.Occurrence.Group.GroupType.Id == groupType.Id).DistinctBy(a => a.Occurrence.Group).Select(g => g.Occurrence.Group).ToList())
                {
                    CheckInGroup checkinGroup = new CheckInGroup();
                    checkinGroup.Group = group;
                    checkinGroup.Selected = true;
                    checkinPerson.GroupTypes.Add(checkinGroupType);

                    labels = GetLabelNames(checkinGroup.Group, labels);

                    // re-create locations
                    foreach (var location in attendance.Where(a => a.Occurrence.GroupId == group.Id).DistinctBy(a => a.Occurrence.Location).Select(l => l.Occurrence.Location).ToList())
                    {
                        CheckInLocation checkinLocation = new CheckInLocation();
                        checkinLocation.Location = location.Clone(false);
                        checkinGroup.Selected = true;

                        labels = GetLabelNames(checkinLocation.Location, labels);

                    }
                }

            }

            if (labels.Any())
            {
                return labels;
            }
            else
            {
                lWarning.Text = "<div class='alert alert-danger'><strong>Alert</strong> No labels found to print.  Does the check-in area have labels configured?</div>";
                return null;
            }

        }

        /// <summary>
        /// This will re-print out the person's check-in labels to the specified printer.  The
        /// person must be currently checked in.
        /// </summary>
        /// <param name="printerDeviceId">The printer device id.</param>
        /// <param name="personGuid">The person guid.</param>
        protected virtual List<Person> GetPeopleList(Guid personGuid)
        {
            var rockContext = new RockContext();
            var attendanceService = new AttendanceService(rockContext);
            var personService = new PersonService(rockContext);
            var attendanceOccurrenceService = new AttendanceOccurrenceService(rockContext);
            var personAliasService = new PersonAliasService(rockContext);


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
                .Where(a => a.PersonAlias.Person.Guid == personGuid);

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
                lWarning.Text = "<div class='alert alert-danger'><strong>Error</strong> - The person is not currently checked in.</div>";
                return null;
            }

            // Find all attendance with the same search group Id
            var listOfPeople = attendanceService.Queryable()
                .Where(a => a.SearchResultGroupId == attendanceQry.FirstOrDefault().SearchResultGroupId)
                .Where(a => a.CreatedDateTime == attendanceQry.FirstOrDefault().CreatedDateTime)
                .Select(a => a.PersonAlias.Person.Id)
                .ToList();

            //var listOfPeople = personAliasService.Queryable()
            //    .Where(pa => listOfAliasPeople.Contains(pa.Id))
            //    .Select(pa => pa.PersonId)
            //    .ToList();

            var people = personService.Queryable().Where(p =>listOfPeople.Contains(p.Id));

            if (people.Any())
            {
                return people.ToList();
            }
            else
            {
                lWarning.Text = "<div class='alert alert-danger'><strong>Alert</strong> No labels found to print.  Does the check-in area have labels configured?</div>";
                return null;
            }

        }

        /// <summary>
        /// Gets the labels for an item (person, grouptype, group, location). 
        /// Note: Code orginally came from the CreateLabels.cs workflow action.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="existingLabels">The existing labels.</param>
        /// <returns></returns>
        public virtual List<KioskLabel> GetLabels( IHasAttributes item, List<Guid> selectedLabels, List<KioskLabel> existingLabels )
        {
            var labels = new List<KioskLabel>( existingLabels );

            if (item.Attributes == null)
            {
                item.LoadAttributes();
            }

            foreach (var attribute in item.Attributes.OrderBy( a => a.Value.Order ))
            {
                if (attribute.Value.FieldType.Class == typeof( Rock.Field.Types.LabelFieldType ).FullName)
                {
                    Guid? binaryFileGuid = item.GetAttributeValue( attribute.Key ).AsGuidOrNull();
                    if (binaryFileGuid != null)
                    {
                        if (!labels.Any( l => l.Guid == binaryFileGuid.Value ) && selectedLabels.Contains(binaryFileGuid.Value))
                        {
                            var labelCache = KioskLabel.Get( binaryFileGuid.Value );
                            labelCache.Order = attribute.Value.Order;
                            if (labelCache != null && (
                                labelCache.LabelType == KioskLabelType.Family ||
                                labelCache.LabelType == KioskLabelType.Person ||
                                labelCache.LabelType == KioskLabelType.Location))
                            {
                                labels.Add( labelCache );
                            }
                        }
                    }
                }
            }

            return labels;
        }

        /// <summary>
        /// Gets the labels for an item (person, grouptype, group, location). 
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="existingLabels">The existing labels.</param>
        /// <returns></returns>
        protected virtual List<BinaryFile> GetLabelNames(IHasAttributes item, List<BinaryFile> existingLabels)
        {
            var binaryFileService = new BinaryFileService(new RockContext());
            var labels = new List<BinaryFile>(existingLabels);

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
                        if (!labels.Any(l => l.Guid == binaryFileGuid.Value))
                        {
                            var labelCache = KioskLabel.Get(binaryFileGuid.Value);
                            labelCache.Order = attribute.Value.Order;
                            if (labelCache != null && (
                                labelCache.LabelType == KioskLabelType.Family ||
                                labelCache.LabelType == KioskLabelType.Person ||
                                labelCache.LabelType == KioskLabelType.Location))
                            {
                                var labelName = binaryFileService.Get(binaryFileGuid.Value);
                                labels.Add(labelName);
                            }
                        }
                    }
                }
            }

            return labels;
        }
    }
}
