import approvalField from '/Scripts/com_thecrossingchurch/EventSubmission/ApprovalField.js';
import utils from '/Scripts/com_thecrossingchurch/EventSubmission/Utilities.js';
export default {
  template: `
<div>
  <v-row v-if="e.StartTime || e.EndTime || ( selected.Changes && (selected.Changes.Events[idx].StartTime || selected.Changes.Events[idx].EndTime) )">
    <v-col v-if="e.StartTime || (selected.Changes && selected.Changes.Events[idx].StartTime)">
      <div class="floating-title">Start Time</div>
      <template v-if="selected.Changes != null && e.StartTime != selected.Changes.Events[idx].StartTime">
        <template v-if="approvalmode">
          <approval-field :request="selected" :e="e" :idx="idx" field="StartTime" :fieldname="formatFieldName('Start Time')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
        </template>
        <template v-else>
          <span class='red--text'>{{(e.StartTime ? e.StartTime : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].StartTime ? selected.Changes.Events[idx].StartTime : 'Empty')}}</span>
        </template>
      </template>
      <template v-else>
        {{e.StartTime}}
      </template>
    </v-col>
    <v-col v-if="e.EndTime || (selected.Changes && selected.Changes.Events[idx].EndTime)">
      <div class="floating-title">End Time</div>
      <template v-if="selected.Changes != null && e.EndTime != selected.Changes.Events[idx].EndTime">
        <template v-if="approvalmode">
          <approval-field :request="selected" :e="e" :idx="idx" field="EndTime" :fieldname="formatFieldName('End Time')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
        </template>
        <template v-else>
          <span class='red--text'>{{(e.EndTime ? e.EndTime : 'Empty')}}: </span>
          <span class='primary--text'>{{(selected.Changes.Events[idx].EndTime ? selected.Changes.Events[idx].EndTime : 'Empty')}}</span>
        </template>
      </template>
      <template v-else>
        {{e.EndTime}}
      </template>
    </v-col>
  </v-row>
  <v-row v-if="e.MinsStartBuffer || e.MinsEndBuffer">
    <v-col v-if="e.MinsStartBuffer">
      <div class="floating-title">Set-up Buffer</div>
      {{e.MinsStartBuffer}} minutes
    </v-col>
    <v-col v-if="e.MinsEndBuffer">
      <div class="floating-title">Tear-down Buffer</div>
      {{e.MinsEndBuffer}} minutes
    </v-col>
  </v-row>
  <template v-if="selected.needsSpace || (selected.Changes && selected.Changes.needsSpace)">
    <h6 :class='sectionHeaderClassName("Room")'>Space Information</h6>
    <v-row>
      <v-col>
        <div class="floating-title">Expected Number of Attendees</div>
        <template v-if="selected.Changes != null && e.ExpectedAttendance != selected.Changes.Events[idx].ExpectedAttendance">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="ExpectedAttendance" :fieldname="formatFieldName('Expected Attendance')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.ExpectedAttendance ? e.ExpectedAttendance : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].ExpectedAttendance ? selected.Changes.Events[idx].ExpectedAttendance : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.ExpectedAttendance}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Desired Rooms/Spaces</div>
        <template v-if="selected.Changes != null && formatRooms(e.Rooms) != formatRooms(selected.Changes.Events[idx].Rooms)">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="Rooms" :formatter="formatRooms" :fieldname="formatFieldName('Rooms')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.Rooms ? formatRooms(e.Rooms) : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].Rooms ? formatRooms(selected.Changes.Events[idx].Rooms) : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{formatRooms(e.Rooms)}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <div class="floating-title">Other Spaces</div>
        <template v-if="selected.Changes != null && e.InfrastructureSpace != selected.Changes.Events[idx].InfrastructureSpace">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="InfrastructureSpace" :fieldname="formatFieldName('InfrastructureSpace')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.InfrastructureSpace ? e.InfrastructureSpace : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].InfrastructureSpace ? selected.Changes.Events[idx].InfrastructureSpace : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.InfrastructureSpace}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.TableType && e.TableType.length > 0 || (selected.Changes && selected.Changes.Events[idx].TableType && selected.Changes.Events[idx].TableType.length > 0)">
      <v-col cols="6">
        <div class="floating-title">Requested Tables</div>
        <template v-if="selected.Changes != null && e.TableType.toString() != selected.Changes.Events[idx].TableType.toString()">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="TableType" :formatter="formatList" :fieldname="formatFieldName('Requested Tables')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.TableType ? e.TableType.join(', ')  : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].TableType  ? selected.Changes.Events[idx].TableType.join(', ') : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.TableType.join(', ')}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.TableType && e.TableType.includes('Round') || (selected.Changes && selected.Changes.Events[idx].TableType && selected.Changes.Events[idx].TableType.includes('Round'))">
      <v-col>
        <div class="floating-title">Number of Round Tables</div>
        <template v-if="selected.Changes != null && e.NumTablesRound != selected.Changes.Events[idx].NumTablesRound">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="NumTablesRound" :fieldname="formatFieldName('Number of Round Tables')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.NumTablesRound ? e.NumTablesRound  : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].NumTablesRound ? selected.Changes.Events[idx].NumTablesRound : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.NumTablesRound}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Number of Chairs per Round Table</div>
        <template v-if="selected.Changes != null && e.NumChairsRound != selected.Changes.Events[idx].NumChairsRound">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="NumChairsRound" :fieldname="formatFieldName('Number of Chairs per Round Table')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.NumChairsRound ? e.NumChairsRound  : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].NumChairsRound ? selected.Changes.Events[idx].NumChairsRound : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.NumChairsRound}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.TableType && e.TableType.includes('Rectangular') || (selected.Changes && selected.Changes.Events[idx].TableType && selected.Changes.Events[idx].TableType.includes('Rectangular'))">
      <v-col>
        <div class="floating-title">Number of Rectangular Tables</div>
        <template v-if="selected.Changes != null && e.NumTablesRect != selected.Changes.Events[idx].NumTablesRect">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="NumTablesRect" :fieldname="formatFieldName('Number of Rectangular Tables')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.NumTablesRect ? e.NumTablesRect  : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].NumTablesRect ? selected.Changes.Events[idx].NumTablesRect : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.NumTablesRect}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Number of Chairs per Rectangular Table</div>
        <template v-if="selected.Changes != null && e.NumChairsRect != selected.Changes.Events[idx].NumChairsRect">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="NumChairsRect" :fieldname="formatFieldName('Number of Chairs per Rectangular Table')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.NumChairsRect ? e.NumChairsRect  : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].NumChairsRect ? selected.Changes.Events[idx].NumChairsRect : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.NumChairsRect}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="(e.TableType && e.TableType.length > 0) || (selected.Changes && selected.Changes.Events[idx].TableType && selected.Changes.Events[idx].TableType.length > 0)">
      <v-col cols="12" md="6">
        <div class="floating-title">Needs Tablecloths</div>
        <template v-if="selected.Changes != null && e.NeedsTableCloths != selected.Changes.Events[idx].NeedsTableCloths">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="NeedsTableCloths" :fieldname="formatFieldName('Needs Tablecloths')" :formatter="boolToYesNo" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.NeedsTableCloths ? boolToYesNo(e.NeedsTableCloths)  : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].NeedsTableCloths ? boolToYesNo(selected.Changes.Events[idx].NeedsTableCloths) : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{boolToYesNo(e.NeedsTableCloths)}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="selected.needsReg">
      <v-col>
        <div class="floating-title">Check-in Requested</div>
        <template v-if="selected.Changes != null && e.Checkin != selected.Changes.Events[idx].Checkin">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="Checkin" :formatter="boolToYesNo" :fieldname="formatFieldName('Check-in')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.Checkin != null ? boolToYesNo(e.Checkin) : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].Checkin != null ? boolToYesNo(selected.Changes.Events[idx].Checkin) : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{boolToYesNo(e.Checkin)}}
        </template>
      </v-col>
      <v-col v-if="(e.Checkin || (selected.Changes && selected.Changes.Events[idx].Checkin)) && (e.ExpectedAttendance >= 100 || (selected.Changes && selected.Changes.Events[idx].ExpectedAttendance >= 100))">
        <div class="floating-title">Database Team Support Requested</div>
        <template v-if="selected.Changes != null && e.SupportTeam != selected.Changes.Events[idx].SupportTeam">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="SupportTeam" :formatter="boolToYesNo" :fieldname="formatFieldName('Database Support Team')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.SupportTeam != null ? boolToYesNo(e.SupportTeam) : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].SupportTeam != null ? boolToYesNo(selected.Changes.Events[idx].SupportTeam) : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{boolToYesNo(e.SupportTeam)}}
        </template>
      </v-col>
    </v-row>
  </template>
  <template v-if="selected.needsOnline || (selected.Changes && selected.Changes.needsOnline)">
    <h6 :class='sectionHeaderClassName("Online Event")'>Online Information</h6>
    <v-row>
      <v-col>
        <div class="floating-title">Event Link</div>
        <template v-if="selected.Changes != null && e.EventURL != selected.Changes.Events[idx].EventURL">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="EventURL" :fieldname="formatFieldName('Event Link')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.EventURL ? e.EventURL : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].EventURL ? selected.Changes.Events[idx].EventURL : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.EventURL}}
        </template>
      </v-col>
      <v-col v-if="e.ZoomPassword || (selected.Changes && selected.Changes.Events[idx].ZoomPassword)">
        <div class="floating-title">Password</div>
        <template v-if="selected.Changes != null && e.ZoomPassword != selected.Changes.Events[idx].ZoomPassword">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="ZoomPassword" :fieldname="formatFieldName('Zoom Password')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.ZoomPassword ? e.ZoomPassword : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].ZoomPassword ? selected.Changes.Events[idx].ZoomPassword : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.ZoomPassword}}
        </template>
      </v-col>
    </v-row>
  </template>
  <template v-if="selected.needsChildCare || (selected.Changes && selected.Changes.needsChildCare)">
    <h6 :class='sectionHeaderClassName("Childcare")'>Childcare Information</h6>
    <v-row>
      <v-col>
        <div class="floating-title">Childcare Age Groups</div>
        <template v-if="selected.Changes != null && JSON.stringify(e.ChildCareOptions) != JSON.stringify(selected.Changes.Events[idx].ChildCareOptions)">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="ChildCareOptions" :fieldname="formatFieldName('Childcare Age Groups')" :formatter="formatList" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{((e.ChildCareOptions && e.ChildCareOptions.length > 0) ? e.ChildCareOptions.join(', ') : 'Empty')}}: </span>
            <span class='primary--text'>{{((selected.Changes.Events[idx].ChildCareOptions && selected.Changes.Events[idx].ChildCareOptions.length > 0) ? selected.Changes.Events[idx].ChildCareOptions.join(', ') : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{((e.ChildCareOptions && e.ChildCareOptions.length > 0) ? e.ChildCareOptions.join(', ') : 'Empty')}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Expected Number of Children</div>
        <template v-if="selected.Changes != null && e.EstimatedKids != selected.Changes.Events[idx].EstimatedKids">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="EstimatedKids" :fieldname="formatFieldName('Estimated Number of Children')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.EstimatedKids ? e.EstimatedKids : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].EstimatedKids ? selected.Changes.Events[idx].EstimatedKids : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.EstimatedKids}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <div class="floating-title">Childcare Start Time</div>
        <template v-if="selected.Changes != null && e.CCStartTime != selected.Changes.Events[idx].CCStartTime">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="CCStartTime" :fieldname="formatFieldName('Childcare Start Time')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.CCStartTime ? e.CCStartTime : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].CCStartTime ? selected.Changes.Events[idx].CCStartTime : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.CCStartTime}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Childcare End Time</div>
        <template v-if="selected.Changes != null && e.CCEndTime != selected.Changes.Events[idx].CCEndTime">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="CCEndTime" :fieldname="formatFieldName('Childcare End Time')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.CCEndTime ? e.CCEndTime : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].CCEndTime ? selected.Changes.Events[idx].CCEndTime : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.CCEndTime}}
        </template>
      </v-col>
    </v-row>
  </template>
  <template v-if="selected.needsCatering || (selected.Changes && selected.Changes.needsCatering)">
    <h6 :class='sectionHeaderClassName("Catering")'>Catering Information</h6>
    <v-row>
      <v-col>
        <div class="floating-title">Preferred Vendor</div>
        <template v-if="selected.Changes != null && e.Vendor != selected.Changes.Events[idx].Vendor">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="Vendor" :fieldname="formatFieldName('Preferred Vendor')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.Vendor ? e.Vendor : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].Vendor ? selected.Changes.Events[idx].Vendor : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.Vendor}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Budget Line</div>
        <template v-if="selected.Changes != null && e.BudgetLine != selected.Changes.Events[idx].BudgetLine">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="BudgetLine" :fieldname="formatFieldName('Budget Line')" :formatter="formatBudgetLine" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.BudgetLine ? formatBudgetLine(e.BudgetLine) : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].BudgetLine ? formatBudgetLine(selected.Changes.Events[idx].BudgetLine) : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{formatBudgetLine(e.BudgetLine)}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <div class="floating-title">Preferred Menu</div>
        <template v-if="selected.Changes != null && e.Menu != selected.Changes.Events[idx].Menu">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="Menu" :fieldname="formatFieldName('Preferred Menu')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.Menu ? e.Menu : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].Menu ? selected.Changes.Events[idx].Menu : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.Menu}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <div class="floating-title">Food should be delivered</div>
        <template v-if="selected.Changes != null && e.FoodDelivery != selected.Changes.Events[idx].FoodDelivery">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="FoodDelivery" :fieldname="formatFieldName('Food should be delivered')" :formatter="boolToYesNo" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.FoodDelivery ? e.FoodDelivery : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].FoodDelivery ? selected.Changes.Events[idx].FoodDelivery : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{boolToYesNo(e.FoodDelivery)}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col>
        <div class="floating-title">{{foodTimeTitle(e)}}</div>
        <template v-if="selected.Changes != null && e.FoodTime != selected.Changes.Events[idx].FoodTime">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="FoodTime" :fieldname="formatFieldName(foodTimeTitle(e))" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.FoodTime ? e.FoodTime : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].FoodTime ? selected.Changes.Events[idx].FoodTime : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.FoodTime}}
        </template>
      </v-col>
      <v-col v-if="e.FoodDelivery || (selected.Changes && selected.Changes.Events[idx].FoodDelivery)">
        <div class="floating-title">Food Drop off Location</div>
        <template v-if="selected.Changes != null && e.FoodDropOff != selected.Changes.Events[idx].FoodDropOff">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="FoodDropOff" :fieldname="formatFieldName('Food Drop off Location')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.FoodDropOff ? e.FoodDropOff : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].FoodDropOff ? selected.Changes.Events[idx].FoodDropOff : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.FoodDropOff}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="(e.TableType && e.TableType.length == 0) || (selected.Changes && selected.Changes.Events[idx].TableType && selected.Changes.Events[idx].TableType.length == 0)">
      <v-col cols="12" md="6">
        <div class="floating-title">Needs Tablecloths</div>
        <template v-if="selected.Changes != null && e.NeedsTableCloths != selected.Changes.Events[idx].NeedsTableCloths">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="NeedsTableCloths" :fieldname="formatFieldName('Needs Tablecloths')" :formatter="boolToYesNo" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.NeedsTableCloths ? boolToYesNo(e.NeedsTableCloths)  : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].NeedsTableCloths ? boolToYesNo(selected.Changes.Events[idx].NeedsTableCloths) : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{boolToYesNo(e.NeedsTableCloths)}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="(e.Drinks && e.Drinks.length > 0) || (selected.Changes && selected.Changes.Events[idx].Drinks && selected.Changes.Events[idx].Drinks.length > 0)">
      <v-col>
        <div class="floating-title">Desired Drinks</div>
        <template v-if="selected.Changes != null && JSON.stringify(e.Drinks) != JSON.stringify(selected.Changes.Events[idx].Drinks)">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="Drinks" :fieldname="formatFieldName('Desired Drinks')" :formatter="formatList" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{((e.Drinks && e.Drinks.length > 0) ? e.Drinks.join(', ') : 'Empty')}}: </span>
            <span class='primary--text'>{{((selected.Changes.Events[idx].Drinks && selected.Changes.Events[idx].Drinks.length > 0) ? selected.Changes.Events[idx].Drinks.join(', ') : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{((e.Drinks && e.Drinks.length > 0) ? e.Drinks.join(', ') : 'Empty')}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.DrinkTime || (selected.Changes && selected.Changes.Events[idx].DrinkTime)">
      <v-col>
        <div class="floating-title">Drink Set-up Time</div>
        <template v-if="selected.Changes != null && e.DrinkTime != selected.Changes.Events[idx].DrinkTime">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="DrinkTime" :fieldname="formatFieldName('Drink Set-up Time')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.DrinkTime ? e.DrinkTime : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkTime ? selected.Changes.Events[idx].DrinkTime : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.DrinkTime}}
        </template>
      </v-col>
      <v-col>
        <div class="floating-title">Drink Drop off Location</div>
        <template v-if="selected.Changes != null && e.DrinkDropOff != selected.Changes.Events[idx].DrinkDropOff">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="DrinkDropOff" :fieldname="formatFieldName('Drink Drop off Location')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.DrinkDropOff ? e.DrinkDropOff : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkDropOff ? selected.Changes.Events[idx].DrinkDropOff : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.DrinkDropOff}}
        </template>
      </v-col>
    </v-row>
    <template v-if="selected.needsChildCare || (selected.Changes && selected.Changes.needsChildCare)">
      <h6 :class='sectionHeaderClassName("Catering")'>Childcare Catering Information</h6>
      <v-row>
        <v-col>
          <div class="floating-title">
            Preferred Vendor for Childcare
          </div>
          <template v-if="selected.Changes != null && e.CCVendor != selected.Changes.Events[idx].CCVendor">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="CCVendor" :fieldname="formatFieldName('Preferred Vendor for Childcare')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.CCVendor ? e.CCVendor : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].CCVendor ? selected.Changes.Events[idx].CCVendor : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.CCVendor}}
          </template>
        </v-col>
        <v-col>
          <div class="floating-title">Budget Line for Childcare</div>
          <template v-if="selected.Changes != null && e.CCBudgetLine != selected.Changes.Events[idx].CCBudgetLine">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="CCBudgetLine" :fieldname="formatFieldName('Budget Line for Childcare')" :formatter="formatBudgetLine" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.CCBudgetLine ? formatBudgetLine(e.CCBudgetLine) : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].CCBudgetLine ? formatBudgetLine(selected.Changes.Events[idx].CCBudgetLine) : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{formatBudgetLine(e.CCBudgetLine)}}
          </template>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <div class="floating-title">
            Preferred Menu for Childcare
          </div>
          <template v-if="selected.Changes != null && e.CCMenu != selected.Changes.Events[idx].CCMenu">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="CCMenu" :fieldname="formatFieldName('Preferred Menu for Childcare')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.CCMenu ? e.CCMenu : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].CCMenu ? selected.Changes.Events[idx].CCMenu : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.CCMenu}}
          </template>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <div class="floating-title">ChildCare Food Set-up time</div>
          <template v-if="selected.Changes != null && e.CCFoodTime != selected.Changes.Events[idx].CCFoodTime">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="CCFoodTime" :fieldname="formatFieldName('ChildCare Food Set-up time')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.CCFoodTime ? e.CCFoodTime : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].CCFoodTime ? selected.Changes.Events[idx].CCFoodTime : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.CCFoodTime}}
          </template>
        </v-col>
      </v-row>
    </template>
  </template>
  <template v-if="selected.needsReg || (selected.Changes && selected.Changes.needsReg)">
    <h6 :class='sectionHeaderClassName("Registration")'>Registration Information</h6>
    <v-row v-if="e.RegistrationDate || (selected.Changes && selected.Changes.Events[idx].RegistrationDate)">
      <v-col>
        <div class="floating-title">Registration Date</div>
        <template v-if="selected.Changes != null && e.RegistrationDate != selected.Changes.Events[idx].RegistrationDate">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="RegistrationDate" :fieldname="formatFieldName('Registration Date')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text' v-if="e.RegistrationDate">{{e.RegistrationDate | formatDate}}: </span>
            <span class='red--text' v-else>Empty: </span>
            <span class='primary--text' v-if="selected.Changes.Events[idx].RegistrationDate">{{selected.Changes.Events[idx].RegistrationDate | formatDate}}</span>
            <span class='primary--text' v-else>Empty</span>
          </template>
        </template>
        <template v-else>
          {{e.RegistrationDate | formatDate}}
        </template>
      </v-col>
      <v-col v-if="e.FeeType || (selected.Changes && selected.Changes.Events[idx].FeeType)">
        <div class="floating-title">Registration Fee Types</div>
        <template v-if="selected.Changes != null && e.FeeType.toString() != selected.Changes.Events[idx].FeeType.toString()">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="FeeType" :fieldname="formatFieldName('Registration Fee Types')" :formatter="formatList" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text' v-if="e.FeeType">{{e.FeeType.join(', ')}}: </span>
            <span class='red--text' v-else>Empty: </span>
            <span class='primary--text' v-if="selected.Changes.Events[idx].FeeType">{{selected.Changes.Events[idx].FeeType.join(', ')}}</span>
            <span class='primary--text' v-else>Empty</span>
          </template>
        </template>
        <template v-else>
          {{e.FeeType.join(', ')}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col cols="12" md="6" v-if="e.FeeBudgetLine || (selected.Changes && selected.Changes.Events[idx].FeeBudgetLine)">
        <div class="floating-title">Registration Fee Budget Line</div>
        <template v-if="selected.Changes != null && e.FeeBudgetLine != selected.Changes.Events[idx].FeeBudgetLine">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="FeeBudgetLine" :fieldname="formatFieldName('Registration Fee Budget Line')" :formatter="formatBudgetLine" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text' v-if="e.FeeBudgetLine">{{formatBudgetLine(e.FeeBudgetLine)}}: </span>
            <span class='red--text' v-else>Empty: </span>
            <span class='primary--text' v-if="selected.Changes.Events[idx].FeeBudgetLine">{{formatBudgetLine(selected.Changes.Events[idx].FeeBudgetLine)}}</span>
            <span class='primary--text' v-else>Empty</span>
          </template>
        </template>
        <template v-else>
          {{formatBudgetLine(e.FeeBudgetLine)}}
        </template>
      </v-col>
      <v-col cols="12" md="6" v-if="e.Fee || (selected.Changes && selected.Changes.Events[idx].Fee)">
        <div class="floating-title">Individual Registration Fee</div>
        <template v-if="selected.Changes != null && e.Fee != selected.Changes.Events[idx].Fee">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="Fee" :fieldname="formatFieldName('Individual Registration Fee')" :formatter="formatCurrency" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text' v-if="e.Fee">{{e.Fee | formatCurrency}}: </span>
            <span class='red--text' v-else>Empty: </span>
            <span class='primary--text' v-if="selected.Changes.Events[idx].Fee">{{selected.Changes.Events[idx].Fee | formatCurrency}}</span>
            <span class='primary--text' v-else>Empty</span>
          </template>
        </template>
        <template v-else>
          {{e.Fee | formatCurrency}}
        </template>
      </v-col>
      <v-col cols="12" md="6" v-if="e.CoupleFee || (selected.Changes && selected.Changes.Events[idx].CoupleFee)">
        <div class="floating-title">Couple Registration Fee</div>
        <template v-if="selected.Changes != null && e.CoupleFee != selected.Changes.Events[idx].CoupleFee">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="CoupleFee" :fieldname="formatFieldName('Couple Registration Fee')" :formatter="formatCurrency" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text' v-if="e.CoupleFee">{{e.CoupleFee | formatCurrency}}: </span>
            <span class='red--text' v-else>Empty: </span>
            <span class='primary--text' v-if="selected.Changes.Events[idx].CoupleFee">{{selected.Changes.Events[idx].CoupleFee | formatCurrency}}</span>
            <span class='primary--text' v-else>Empty</span>
          </template>
        </template>
        <template v-else>
          {{e.CoupleFee | formatCurrency}}
        </template>
      </v-col>
      <v-col cols="12" md="6" v-if="e.OnlineFee || (selected.Changes && selected.Changes.Events[idx].OnlineFee)">
        <div class="floating-title">Online Registration Fee</div>
        <template v-if="selected.Changes != null && e.OnlineFee != selected.Changes.Events[idx].OnlineFee">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="OnlineFee" :fieldname="formatFieldName('Online Registration Fee')" :formatter="formatCurrency" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text' v-if="e.OnlineFee">{{e.OnlineFee | formatCurrency}}: </span>
            <span class='red--text' v-else>Empty: </span>
            <span class='primary--text' v-if="selected.Changes.Events[idx].OnlineFee">{{selected.Changes.Events[idx].OnlineFee | formatCurrency}}</span>
            <span class='primary--text' v-else>Empty</span>
          </template>
        </template>
        <template v-else>
          {{e.OnlineFee | formatCurrency}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.RegistrationEndDate || (selected.Changes && selected.Changes.Events[idx].RegistrationEndDate)">
      <v-col>
        <div class="floating-title">Registration Close Date</div>
        <template v-if="selected.Changes != null && e.RegistrationEndDate != selected.Changes.Events[idx].RegistrationEndDate">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="RegistrationEndDate" :fieldname="formatFieldName('Registration Close Date')" :formatter="formatDate" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text' v-if="e.RegistrationEndDate">{{e.RegistrationEndDate | formatDate}}: </span>
            <span class='red--text' v-else>Empty: </span>
            <span class='primary--text' v-if="selected.Changes.Events[idx].RegistrationEndDate">{{selected.Changes.Events[idx].RegistrationEndDate | formatDate}}</span>
            <span class='primary--text' v-else>Empty</span>
          </template>
        </template>
        <template v-else>
          {{e.RegistrationEndDate | formatDate}}
        </template>
      </v-col>
      <v-col v-if="e.RegistrationEndTime || (selected.Changes && selected.Changes.Events[idx].RegistrationEndTime)">
        <div class="floating-title">Registration Close Time</div>
        <template v-if="selected.Changes != null && e.RegistrationEndTime != selected.Changes.Events[idx].RegistrationEndTime">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="RegistrationEndTime" :fieldname="formatFieldName('Registration Close Time')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.RegistrationEndTime ? e.RegistrationEndTime : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].RegistrationEndTime ? selected.Changes.Events[idx].RegistrationEndTime : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.RegistrationEndTime}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col v-if="e.Sender || (selected.Changes && selected.Changes.Events[idx].Sender)">
        <div class="floating-title">Registration Confirmation Email Sender</div>
        <template v-if="selected.Changes != null && e.Sender != selected.Changes.Events[idx].Sender">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="Sender" :fieldname="formatFieldName('Registration Confirmation Email Sender')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.Sender ? e.Sender : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].Sender ? selected.Changes.Events[idx].Sender : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.Sender}}
        </template>
      </v-col>
      <v-col v-if="e.SenderEmail || (selected.Changes && selected.Changes.Events[idx].SenderEmail)">
        <div class="floating-title">Registration Confirmation Email From Address</div>
        <template v-if="selected.Changes != null && e.SenderEmail != selected.Changes.Events[idx].SenderEmail">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="SenderEmail" :fieldname="formatFieldName('Registration Confirmation Sender Email')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.SenderEmail ? e.SenderEmail : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].SenderEmail ? selected.Changes.Events[idx].SenderEmail : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.SenderEmail}}
        </template>
      </v-col>
    </v-row>
    <v-row>
      <v-col v-if="e.ThankYou || (selected.Changes && selected.Changes.Events[idx].ThankYou)">
        <div class="floating-title">Confirmation Email Thank You</div>
        <template v-if="selected.Changes != null && e.ThankYou != selected.Changes.Events[idx].ThankYou">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="ThankYou" :fieldname="formatFieldName('Confirmation Email Thank You')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.ThankYou ? e.ThankYou : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].ThankYou ? selected.Changes.Events[idx].ThankYou : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.ThankYou}}
        </template>
      </v-col>
      <v-col v-if="e.TimeLocation || (selected.Changes && selected.Changes.Events[idx].TimeLocation)">
        <div class="floating-title">Confirmation Email Date, Time, and Location</div>
        <template v-if="selected.Changes != null && e.TimeLocation != selected.Changes.Events[idx].TimeLocation">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="TimeLocation" :fieldname="formatFieldName('Confirmation Email Date, Time, and Location')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.TimeLocation ? e.TimeLocation : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].TimeLocation ? selected.Changes.Events[idx].TimeLocation : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.TimeLocation}}
        </template>
      </v-col>
    </v-row>
    <v-row v-if="e.AdditionalDetails || (selected.Changes && selected.Changes.Events[idx].AdditionalDetails)">
      <v-col>
        <div class="floating-title">Confirmation Email Additional Details</div>
        <template v-if="selected.Changes != null && e.AdditionalDetails != selected.Changes.Events[idx].AdditionalDetails">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="AdditionalDetails" :fieldname="formatFieldName('Confirmation Email Additional Details')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.AdditionalDetails ? e.AdditionalDetails : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].AdditionalDetails ? selected.Changes.Events[idx].AdditionalDetails : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{e.AdditionalDetails}}
        </template>
      </v-col>
    </v-row>
    <template v-if="e.NeedsReminderEmail || (selected.Changes && selected.Changes.Events[idx].NeedsReminderEmail)">
      <v-row>
        <v-col v-if="e.ReminderSender || (selected.Changes && selected.Changes.Events[idx].ReminderSender)">
          <div class="floating-title">Registration Reminder Email Sender</div>
          <template v-if="selected.Changes != null && e.ReminderSender != selected.Changes.Events[idx].ReminderSender">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="Sender" :fieldname="formatFieldName('Registration Confirmation Email Sender')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.ReminderSender ? e.ReminderSender : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].ReminderSender ? selected.Changes.Events[idx].ReminderSender : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.ReminderSender}}
          </template>
        </v-col>
        <v-col v-if="e.ReminderSenderEmail || (selected.Changes && selected.Changes.Events[idx].ReminderSenderEmail)">
          <div class="floating-title">Registration Reminder Email From Address</div>
          <template v-if="selected.Changes != null && e.ReminderSenderEmail != selected.Changes.Events[idx].ReminderSenderEmail">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="SenderEmail" :fieldname="formatFieldName('Registration Confirmation Sender Email')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.ReminderSenderEmail ? e.ReminderSenderEmail : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].ReminderSenderEmail ? selected.Changes.Events[idx].ReminderSenderEmail : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.ReminderSenderEmail}}
          </template>
        </v-col>
      </v-row>
      <v-row>
        <v-col v-if="e.ReminderTimeLocation || (selected.Changes && selected.Changes.Events[idx].ReminderTimeLocation)">
          <div class="floating-title">Reminder Email Date, Time, and Location</div>
          <template v-if="selected.Changes != null && e.ReminderTimeLocation != selected.Changes.Events[idx].ReminderTimeLocation">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="TimeLocation" :fieldname="formatFieldName('Confirmation Email Date, Time, and Location')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.ReminderTimeLocation ? e.ReminderTimeLocation : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].ReminderTimeLocation ? selected.Changes.Events[idx].ReminderTimeLocation : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.ReminderTimeLocation}}
          </template>
        </v-col>
        <v-col v-if="e.ReminderAdditionalDetails || (selected.Changes && selected.Changes.Events[idx].ReminderAdditionalDetails)">
          <div class="floating-title">Reminder Email Additional Details</div>
          <template v-if="selected.Changes != null && e.ReminderAdditionalDetails != selected.Changes.Events[idx].ReminderAdditionalDetails">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="ReminderAdditionalDetails" :fieldname="formatFieldName('Confirmation Email Additional Details')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.ReminderAdditionalDetails ? e.ReminderAdditionalDetails : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].ReminderAdditionalDetails ? selected.Changes.Events[idx].ReminderAdditionalDetails : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.ReminderAdditionalDetails}}
          </template>
        </v-col>
      </v-row>
    </template>
    <v-row v-if="(selected.EventDates && selected.EventDates.length > 1 && selected.IsSame) || ((selected.Changes && selected.Changes.EventDates && selected.Changes.EventDates.length > 1 && selected.Changes.IsSame))">
      <v-col>
        <div class="floating-title">Events Require Separate Links</div>
        <template v-if="selected.Changes != null && selected.EventsNeedSeparateLinks != selected.Changes.EventsNeedSeparateLinks">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="EventsNeedSeparateLinks" :formatter="boolToYesNo" :fieldname="formatFieldName('Events Need Separate Links')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(selected.EventsNeedSeparateLinks ? boolToYesNo(selected.EventsNeedSeparateLinks) : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.EventsNeedSeparateLinks ? boolToYesNo(selected.Changes.EventsNeedSeparateLinks) : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{boolToYesNo(selected.EventsNeedSeparateLinks)}}
        </template>
      </v-col>
    </v-row>
  </template>
  <template v-if="selected.needsAccom || (selected.Changes && selected.Changes.needsAccom)">
    <h6 :class='sectionHeaderClassName("Extra Resources")'>Additional Information</h6>
    <v-row v-if="e.TechNeeds || e.TechDescription || (selected.Changes && (selected.Changes.Events[idx].TechNeeds || selected.Changes.Events[idx].TechDescription))">
      <v-col v-if="e.TechNeeds && e.TechNeeds.length > 0">
        <div class="floating-title">Tech Needs</div>
        <template v-if="selected.Changes != null && JSON.stringify(e.TechNeeds) != JSON.stringify(selected.Changes.Events[idx].TechNeeds)">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="TechNeeds" :fieldname="formatFieldName('Tech Needs')" :formatter="formatList" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.TechNeeds ? e.TechNeeds.join(', ') : 'Empty')}}: </span>
            <span class='primary--text'>{{(selected.Changes.Events[idx].TechNeeds ? selected.Changes.Events[idx].TechNeeds.join(', ') : 'Empty')}}</span>
          </template>
        </template>
        <template v-else>
          {{(e.TechNeeds ? e.TechNeeds.join(', ') : 'Empty')}}
        </template>
      </v-col>
      <v-col v-if="e.TechDescription || (selected.Changes && selected.Changes.Events[idx].TechDescription)">
        <div class="floating-title">Tech Description</div>
        <template v-if="selected.Changes != null && e.TechDescription != selected.Changes.Events[idx].TechDescription">
          <template v-if="approvalmode">
            <approval-field :request="selected" :e="e" :idx="idx" field="TechDescription" :fieldname="formatFieldName('Tech Description')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{(e.TechDescription ? e.TechDescription : 'Empty')}}: </span>
            <span class='primary--text'>{{selected.Changes.Events[idx].TechDescription}}</span>
          </template>
        </template>
        <template v-else>
          {{e.TechDescription}}
        </template>
      </v-col>
    </v-row>
    <template v-if="!selected.needsCatering">
      <v-row v-if="(e.Drinks && e.Drinks.length > 0) || (selected.Changes && selected.Changes.Events[idx].Drinks && selected.Changes.Events[idx].Drinks.length > 0)">
        <v-col>
          <div class="floating-title">Desired Drinks</div>
          <template v-if="selected.Changes != null && JSON.stringify(e.Drinks) != JSON.stringify(selected.Changes.Events[idx].Drinks)">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="Drinks" :fieldname="formatFieldName('Desired Drinks')" :formatter="formatList" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{((e.Drinks && e.Drinks.length > 0) ? e.Drinks.join(', ') : 'Empty')}}: </span>
              <span class='primary--text'>{{((selected.Changes.Events[idx].Drinks && selected.Changes.Events[idx].Drinks.length > 0) ? selected.Changes.Events[idx].Drinks.join(', ') : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{((e.Drinks && e.Drinks.length > 0) ? e.Drinks.join(', ') : 'Empty')}}
          </template>
        </v-col>
      </v-row>
      <v-row v-if="e.DrinkTime || (selected.Changes && selected.Changes.Events[idx].DrinkTime)">
        <v-col>
          <div class="floating-title">Drink Set-up Time</div>
          <template v-if="selected.Changes != null && e.DrinkTime != selected.Changes.Events[idx].DrinkTime">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="DrinkTime" :fieldname="formatFieldName('Drink Set-up Time')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.DrinkTime ? e.DrinkTime : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkTime ? selected.Changes.Events[idx].DrinkTime : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.DrinkTime}}
          </template>
        </v-col>
        <v-col>
          <div class="floating-title">Drink Drop off Location</div>
          <template v-if="selected.Changes != null && e.DrinkDropOff != selected.Changes.Events[idx].DrinkDropOff">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="DrinkDropOff" :fieldname="formatFieldName('Drink Drop off Location')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.DrinkDropOff ? e.DrinkDropOff : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkDropOff ? selected.Changes.Events[idx].DrinkDropOff : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.DrinkDropOff}}
          </template>
        </v-col>
      </v-row>
    </template>
    <template v-if="isSuperUser">
      <v-row>
        <v-col>
          <div class="floating-title">Needs doors unlocked</div>
          <template v-if="selected.Changes != null && e.NeedsDoorsUnlocked != selected.Changes.Events[idx].NeedsDoorsUnlocked">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="NeedsDoorsUnlocked" :fieldname="formatFieldName('Needs doors unlocked')" :formatter="boolToYesNo" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.NeedsDoorsUnlocked != null ? boolToYesNo(e.NeedsDoorsUnlocked) : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].NeedsDoorsUnlocked != null ? boolToYesNo(selected.Changes.Events[idx].NeedsDoorsUnlocked) : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{boolToYesNo(e.NeedsDoorsUnlocked)}}
          </template>
        </v-col>
        <v-col v-if="e.NeedsDoorsUnlocked || (selected.Changes != null && selected.Changes.Events[idx].NeedsDoorsUnlocked)">
          <div class="floating-title">Doors Needed</div>
          <template v-if="selected.Changes != null && formatDoors(e.Doors) != formatDoors(selected.Changes.Events[idx].Doors)">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="Doors" :fieldname="formatFieldName('Doors Needed')" :formatter="formatDoors" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{((e.Doors != null && e.Doors.length > 0) ? formatDoors(e.Doors) : 'Empty')}}: </span>
              <span class='primary--text'>{{((selected.Changes.Events[idx].Doors != null && selected.Changes.Events[idx].Doors.length > 0) ? formatDoors(selected.Changes.Events[idx].Doors) : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{((e.Doors != null && e.Doors.length > 0) ? formatDoors(e.Doors) : 'Empty')}}
          </template>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <div class="floating-title">Add to public calendar</div>
          <template v-if="selected.Changes != null && e.ShowOnCalendar != selected.Changes.Events[idx].ShowOnCalendar">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="ShowOnCalendar" :fieldname="formatFieldName('Add to public calendar')" :formatter="boolToYesNo" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.ShowOnCalendar != null ? boolToYesNo(e.ShowOnCalendar) : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].ShowOnCalendar != null ? boolToYesNo(selected.Changes.Events[idx].ShowOnCalendar) : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{boolToYesNo(e.ShowOnCalendar)}}
          </template>
        </v-col>
      </v-row>
      <v-row v-if="(e.ShowOnCalendar || (selected.Changes && selected.Changes.Events[idx].ShowOnCalendar)) && (e.PublicityBlurb || (selected.Changes && selected.Changes.Events[idx].PublicityBlurb))">
        <v-col>
          <div class="floating-title">Web Calendar Blurb</div>
          <template v-if="selected.Changes != null && e.PublicityBlurb != selected.Changes.Events[idx].PublicityBlurb">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="PublicityBlurb" :fieldname="formatFieldName('Web Calendar Blurb')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.PublicityBlurb ? e.PublicityBlurb : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].PublicityBlurb ? selected.Changes.Events[idx].PublicityBlurb : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.PublicityBlurb}}
          </template>
        </v-col>
      </v-row>
      <v-row v-if="e.SetUp || (selected.Changes && selected.Changes.Events[idx].SetUp)">
        <v-col>
          <div class="floating-title">Requested Set-up</div>
          <template v-if="selected.Changes != null && e.SetUp != selected.Changes.Events[idx].SetUp">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="SetUp" :fieldname="formatFieldName('Requested Set-up')" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.SetUp ? e.SetUp : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].SetUp ? selected.Changes.Events[idx].SetUp : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{e.SetUp}}
          </template>
        </v-col>
      </v-row>
      <v-row v-if="e.SetUpImage || (selected.Changes && selected.Changes.Events[idx].SetUpImage)">
        <template v-if="selected.Changes != null && e.SetUpImage != selected.Changes.Events[idx].SetUpImage">
          <v-col>
            <div class="floating-title">Set-up Image</div>
            <template v-if="e.SetUpImage">
              <span class='red--text'>{{e.SetUpImage.name}}</span>
              <v-btn icon color="accent" @click="saveFile(idx, 'existing')">
                <v-icon color="accent">mdi-download</v-icon>
              </v-btn>
            </template>
            <template v-else>
              <span class='red--text'>Empty</span>
            </template>
          </v-col>
          <v-col>
            <div class="floating-title">Set-up Image</div>
            <template v-if="selected.Changes.Events[idx].SetUpImage">
              <span class='primary--text'>{{selected.Changes.Events[idx].SetUpImage.name}}</span>
              <v-btn icon color="accent" @click="saveFile(idx, 'new')">
                <v-icon color="accent">mdi-download</v-icon>
              </v-btn>
            </template>
            <template v-else>
              <span class='primary--text'>Empty</span>
            </template>
          </v-col>
          <v-col v-if="approvalmode">
            <v-btn fab small color="accent" @click="setUpImageChoiceMade = true; setUpImageIsApproved = true; approveChange({field: 'SetUpImage', label: formatFieldName('Set-up Image'), idx: idx})" :disabled="setUpImageChoiceMade && setUpImageIsApproved">
              <v-icon>mdi-check-circle</v-icon>
            </v-btn>
            <v-btn fab small color="red" @click="setUpImageChoiceMade = true; setUpImageIsApproved = false; denyChange({field: 'SetUpImage', label: formatFieldName('Set-up Image'), idx: idx})" :disabled="setUpImageChoiceMade && !setUpImageIsApproved">
              <v-icon>mdi-cancel</v-icon>
            </v-btn>
          </v-col>
        </template>
        <template v-else>
          <v-col>
            <div class="floating-title">Set-up Image</div>
            <template v-if="e.SetUpImage">
              <span>{{e.SetUpImage.name}}</span>
              <v-btn icon color="accent" @click="saveFile(idx, 'existing')">
                <v-icon color="accent">mdi-download</v-icon>
              </v-btn>
            </template>
            <template v-else>
              <span>Empty</span>
            </template>
          </v-col>
        </template>
      </v-row>
      <v-row>
        <v-col>
          <div class="floating-title">Needs Medical Team</div>
          <template v-if="selected.Changes != null && e.NeedsMedical != selected.Changes.Events[idx].NeedsMedical">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="NeedsMedical" :fieldname="formatFieldName('Needs Medical Team')" :formatter="boolToYesNo" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.NeedsMedical ? boolToYesNo(e.NeedsMedical) : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].NeedsMedical ? boolToYesNo(selected.Changes.Events[idx].NeedsMedical) : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{boolToYesNo(e.NeedsMedical)}}
          </template>
        </v-col>
        <v-col>
          <div class="floating-title">Needs Security Team</div>
          <template v-if="selected.Changes != null && e.NeedsSecurity != selected.Changes.Events[idx].NeedsSecurity">
            <template v-if="approvalmode">
              <approval-field :request="selected" :e="e" :idx="idx" field="NeedsSecurity" :fieldname="formatFieldName('Needs Security Team')" :formatter="boolToYesNo" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
            </template>
            <template v-else>
              <span class='red--text'>{{(e.NeedsSecurity ? boolToYesNo(e.NeedsSecurity) : 'Empty')}}: </span>
              <span class='primary--text'>{{(selected.Changes.Events[idx].NeedsSecurity ? boolToYesNo(selected.Changes.Events[idx].NeedsSecurity) : 'Empty')}}</span>
            </template>
          </template>
          <template v-else>
            {{boolToYesNo(e.NeedsSecurity)}}
          </template>
        </v-col>
      </v-row>
    </template>
  </template>
</div>
`,
  props: ["e", "idx", "selected", "approvalmode"],
  data: function () {
    return {
      rooms: [],
      doors: [],
      ministries: [],
      budgetLines: [],
      setUpImageChoiceMade: false,
      setUpImageIsApproved: false,
      isSuperUser: false,
    }
  },
  created: function () {
    this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
    this.doors = JSON.parse($('[id$="hfDoors"]')[0].value);
    this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value)
    this.budgetLines = JSON.parse($('[id$="hfBudgetLines"]')[0].value)
    window['moment-range'].extendMoment(moment)
    if(this.selected.Changes != null && this.e.SetUpImage != this.selected.Changes.Events[this.idx].SetUpImage) {
      this.$emit('newchange')
    }
    let isSU = $('[id$="hfIsSuperUser"]')[0].value;
    if(isSU == 'True') {
      this.isSuperUser = true
    }
  },
  filters: {
    ...utils.filters
  },
  computed: {
    
  },
  watch: {
    
  },
  methods: {
    ...utils.methods, 
    foodTimeTitle(e) {
      if (e.FoodDelivery) {
        return "Food Set-up time";
      } else {
        return "Desired Pick-up time from Vendor";
      }
    },
    formatFieldName(val) {
      if(this.e.EventDate) {
        return val + " for " + moment(this.e.EventDate).format("MM/DD/yyyy")
      }
      return val
    },
    approveChange(field) {
      this.$emit("approvechange", field)
    },
    denyChange(field) {
      this.$emit("denychange", field)
    },
    newchange() {
      this.$emit("newchange")
    },
    newchoice() {
      this.$emit("newchoice")
    },
    sectionHeaderClassName(section) {
      if(this.invalidSections(this.selected).includes(section)) {
        return 'text--error text-uppercase'
      } else {
        return 'text--accent text-uppercase'
      }
    },
    saveFile(idx, type) {
      var a = document.createElement("a");
      a.style = "display: none";
      document.body.appendChild(a);
      if (type == 'existing') {
        a.href = this.selected.Events[idx].SetUpImage.data;
        a.download = this.selected.Events[idx].SetUpImage.name;
      } else if (type == 'new') {
        a.href = this.selected.Changes.Events[idx].SetUpImage.data;
        a.download = this.selected.Changes.Events[idx].SetUpImage.name;
      }
      a.click();
    },
  },
  components: {
    'approval-field': approvalField,
  },
  watch: {
    setUpImageChoiceMade(val) {
      this.$emit("newchoice")
    }
  }
}