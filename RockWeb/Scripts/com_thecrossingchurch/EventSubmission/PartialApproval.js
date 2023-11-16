import utils from '/Scripts/com_thecrossingchurch/EventSubmission/Utilities.js';
import approvalField from '/Scripts/com_thecrossingchurch/EventSubmission/ApprovalField.js';
export default {
  template: `
    <v-card>
      <v-card-title>
        <template v-if="request.Changes != null && request.Name != request.Changes.Name">
          <template v-if="approvalmode">
            <approval-field :request="request" :e="null" :idx="null" field="Name" fieldname="Event Name" v-on:approvechange="approveChange" v-on:denychange="denyChange"></approval-field>
          </template>
          <template v-else>
            <span class='red--text'>{{request.Name}}: </span>
            <span class='primary--text'>{{request.Changes.Name}}</span>
          </template>
        </template>
        <template v-else>
          {{request.Name}}
        </template>
        <v-spacer></v-spacer>
        <div :class="getStatusPillClass(request.RequestStatus)">
          {{request.RequestStatus}}
        </div>
      </v-card-title>
      <v-card-text>
        <v-row>
          <v-col>
            <div class="floating-title">Submitted By</div>
            {{request.CreatedBy}}
          </v-col>
          <v-col class="text-right">
            <div class="floating-title">Submitted On</div>
            {{request.SubmittedOn | formatDateTime}}
          </v-col>
        </v-row>
        <hr />
        <v-row>
          <v-col>
            <div class="floating-title">Ministry</div>
            <template v-if="request.Changes != null && request.Ministry != request.Changes.Ministry">
              <template v-if="approvalmode">
                <approval-field :request="request" :e="null" :idx="null" field="Ministry" fieldname="Ministry" :formatter="formatMinistry" v-on:approvechange="approveChange" v-on:denychange="denyChange"></approval-field>
              </template>
              <template v-else>
                <span class='red--text'>{{formatMinistry(request.Ministry)}}: </span>
                <span class='primary--text'>{{formatMinistry(request.Changes.Ministry)}}</span>
              </template>
            </template>
            <template v-else>
              {{formatMinistry(request.Ministry)}}
            </template>
          </v-col>
          <v-col>
            <div class="floating-title">Contact</div>
            <template v-if="request.Changes != null && request.Contact != request.Changes.Contact">
              <template v-if="approvalmode">
                <approval-field :request="request" :e="null" :idx="null" field="Contact" fieldname="Contact" v-on:approvechange="approveChange" v-on:denychange="denyChange"></approval-field>
              </template>
              <template v-else>
                <span class='red--text'>{{request.Contact}}: </span>
                <span class='primary--text'>{{request.Changes.Contact}}</span>
              </template>
            </template>
            <template v-else>
              {{request.Contact}}
            </template>
          </v-col>
        </v-row>
        <v-row>
          <v-col>
            <div class="floating-title">Requested Resources</div>
            {{requestType(request)}}
          </v-col>
        </v-row>
        <v-expansion-panels v-model="panels" multiple flat>
          <v-expansion-panel v-for="(e, idx) in request.Events" :key="panelKey(idx)">
            <v-expansion-panel-header>
              <template v-if="request.IsSame || request.Events.length == 1">
                <template v-if="request.Changes != null && formatDates(request.EventDates) != formatDates(request.Changes.EventDates)">
                  <template v-if="approvalmode">
                    <approval-field :request="request" :e="null" :idx="null" field="EventDates" fieldname="Event Dates" :formatter="formatDates" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></approval-field>
                  </template>
                  <template v-else>
                    <span class='red--text'>{{formatDates(request.EventDates)}}: </span>
                    <span class='primary--text'>{{formatDates(request.Changes.EventDates)}} ({{formatRooms(e.Rooms)}})</span>
                  </template>
                </template>
                <template v-else>
                  {{formatDates(request.EventDates)}} ({{formatRooms(e.Rooms)}})
                </template>
              </template>
              <template v-else>
                <template v-if="request.Changes != null && formatDates(request.EventDates) != formatDates(request.Changes.EventDates)">
                  <span class='red--text'>{{e.EventDate | formatDate}}: </span>
                  <span class='primary--text'>{{request.Changes.Events[idx].EventDate | formatDate}} ({{formatRooms(e.Rooms)}})</span>
                </template>
                <template v-else>
                  {{e.EventDate | formatDate}} ({{formatRooms(e.Rooms)}})
                </template>
              </template>
            </v-expansion-panel-header>
            <v-expansion-panel-content style="color: rgba(0,0,0,.6);">
              <event-details :e="e" :idx="idx" :selected="request" :approvalmode="true" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></event-details>
            </v-expansion-panel-content>
          </v-expansion-panel>
        </v-expansion-panels>
        <pub-details v-if="request.needsPub || request.Changes.needsPub" :request="request" :approvalmode="true" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:newchoice="newchoice" v-on:newchange="newchange"></pub-details>
      </v-card-text>
      <v-card-actions>
        <v-btn color="accent" @click="complete">Complete</v-btn>
        <v-spacer></v-spacer>
        <v-btn color="secondary" @click="cancel">Cancel</v-btn>
      </v-card-actions>
    </v-card>
`,
  props: ["request"],
  data: function () {
    return {
      rooms: [],
      ministries: [],
      panels: [0],
      approvalmode: true
    }
  },
  created: function () {
    this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
    this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value)
    
  },
  filters: {
    ...utils.filters
  },
  computed: {
    
  },
  methods: {
    ...utils.methods, 
    approveChange(field) {
      this.$emit("approvechange", field)
    },
    denyChange(field) {
      this.$emit("denychange", field)
    },
    complete() {
      this.$emit("complete")
    },
    cancel() {
      this.$emit("cancel")
    },
    newchange() {
      this.$emit("newchange")
    },
    newchoice() {
      this.$emit("newchoice")
    },
    getStatusPillClass(status) {
      if (status == "Approved") {
        return "no-top-pad status-pill approved";
      }
      if (status == "In Progress") {
        return "no-top-pad status-pill inprogress";
      }
      if (status == "Submitted" || status == "Pending Changes" || status == "Changes Accepted by User") {
        return "no-top-pad status-pill submitted";
      }
      if (status == "Cancelled" || status == "Cancelled by User") {
        return "no-top-pad status-pill cancelled";
      }
      if (status == "Denied" || status == "Proposed Changes Denied") {
        return "no-top-pad status-pill denied";
      }
    },
    panelKey(idx) {
      return `panel_${idx}`
    },
  },
  watch: {
    
  },
  components: {
    'approval-field': approvalField,
  }
}