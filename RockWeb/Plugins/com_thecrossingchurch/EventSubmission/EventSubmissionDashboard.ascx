<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="EventSubmissionDashboard.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.EventSubmission.EventSubmissionDashboard"
%> <%-- Add Vue and Vuetify CDN --%>
<!-- <script src="https://cdn.jsdelivr.net/npm/vue@2.6.12"></script>  -->
<script src="https://cdn.jsdelivr.net/npm/vue@2.6.12/dist/vue.js"></script>
<script src="https://cdn.jsdelivr.net/npm/vuetify@2.x/dist/vuetify.js"></script>
<script src="https://cdn.jsdelivr.net/npm/chart.js@2.9.4/dist/Chart.min.js"></script>
<link
  href="https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900"
  rel="stylesheet"
/>
<link
  href="https://cdn.jsdelivr.net/npm/@mdi/font@6.x/css/materialdesignicons.min.css"
  rel="stylesheet"
/>
<link
  href="https://cdn.jsdelivr.net/npm/vuetify@2.x/dist/vuetify.min.css"
  rel="stylesheet"
/>
<script
  src="https://cdnjs.cloudflare.com/ajax/libs/moment.js/2.29.1/moment.min.js"
  integrity="sha512-qTXRIMyZIFb8iQcfjXWCO8+M5Tbc38Qi5WzdPOYZHIlZpzBHG3L3by84BBBOiRGiEb7KKtAOAs5qYdUiZiQNNQ=="
  crossorigin="anonymous"
></script>
<script 
  src="https://cdnjs.cloudflare.com/ajax/libs/moment-range/4.0.2/moment-range.js" 
  integrity="sha512-XKgbGNDruQ4Mgxt7026+YZFOqHY6RsLRrnUJ5SVcbWMibG46pPAC97TJBlgs83N/fqPTR0M89SWYOku6fQPgyw==" 
  crossorigin="anonymous"
></script>

<asp:HiddenField ID="hfRooms" runat="server" />
<asp:HiddenField ID="hfDoors" runat="server" />
<asp:HiddenField ID="hfMinistries" runat="server" />
<asp:HiddenField ID="hfBudgetLines" runat="server" />
<asp:HiddenField ID="hfRequests" runat="server" />
<asp:HiddenField ID="hfUpcomingRequests" runat="server" />
<asp:HiddenField ID="hfCurrent" runat="server" />
<asp:HiddenField ID="hfRequestURL" runat="server" />
<asp:HiddenField ID="hfHistoryURL" runat="server" />
<asp:HiddenField ID="hfRequestID" runat="server" />
<asp:HiddenField ID="hfAction" runat="server" />
<asp:HiddenField ID="hfUpdatedItem" runat="server" />
<asp:HiddenField ID="hfApprovedEmail" runat="server" />
<asp:HiddenField ID="hfDeniedEmail" runat="server" />
<asp:HiddenField ID="hfComment" runat="server" />
<asp:HiddenField ID="hfChanges" runat="server" />
<Rock:BootstrapButton
  ID="btnChangeStatus"
  CssClass="btn-hidden"
  runat="server"
  OnClick="ChangeStatus_Click"
/>
<Rock:BootstrapButton
  ID="btnAddBuffer"
  CssClass="btn-hidden"
  runat="server"
  OnClick="AddBuffer_Click"
/>
<Rock:BootstrapButton
  ID="btnAddComment"
  CssClass="btn-hidden"
  runat="server"
  OnClick="AddComment_Click"
/>
<Rock:BootstrapButton
  ID="btnPartialApproval"
  CssClass="btn-hidden"
  runat="server"
  OnClick="PartialApproval_Click"
/>

<div id="app" v-cloak>
  <v-app v-cloak>
    <div>
      <v-row>
        <v-col>
          <v-card>
            <v-card-text>
              <v-row align="center">
                <v-col>
                  <v-text-field
                    label="Search"
                    prepend-inner-icon="mdi-magnify"
                    v-model="filters.query"
                    clearable
                  ></v-text-field>
                </v-col>
                <v-col>
                  <v-text-field
                    label="Submitter"
                    prepend-inner-icon="mdi-account"
                    v-model="filters.submitter"
                    clearable
                  ></v-text-field>
                </v-col>
                <v-col>
                  <v-autocomplete
                    label="Status"
                    :items="['Draft','Submitted','In Progress','Approved','Denied','Cancelled','Pending Changes','Proposed Changes Denied','Changes Accepted by User','Cancelled by User']"
                    v-model="filters.status"
                    multiple
                    attach
                    clearable
                  ></v-autocomplete>
                </v-col>
                <v-col>
                  <v-autocomplete
                    label="Resources"
                    :items="['Room', 'Catering', 'Childcare', 'Extra Resources', 'Online', 'Publicity', 'Registration']"
                    v-model="filters.resources"
                    multiple
                    attach
                    clearable
                  ></v-autocomplete>
                </v-col>
                <v-col cols="2">
                  <v-tooltip bottom>
                    <template v-slot:activator="{ on, attrs }">
                      <v-btn fab small color="primary" class="pull-right ml-2" @click="openHistory" v-bind="attrs" v-on="on">
                        <v-icon>mdi-history</v-icon>
                      </v-btn>
                    </template>
                    <span>View History</span>
                  </v-tooltip>
                  <v-btn color="primary" class="pull-right" @click="filter">
                    Filter
                  </v-btn>
                </v-col>
              </v-row>
              <v-list>
                <v-list-item class="list-with-border">
                  <v-row>
                    <v-col><strong>Request</strong></v-col>
                    <v-col><strong>Submitted By</strong></v-col>
                    <v-col><strong>Submitted On</strong></v-col>
                    <v-col><strong>Event Dates</strong></v-col>
                    <v-col><strong>Requested Resources</strong></v-col>
                    <v-col cols="3"><strong>Status</strong></v-col>
                  </v-row>
                </v-list-item>
                <v-list-item
                  v-for="(r, idx) in requests"
                  :key="r.Id"
                  :class="getClass(idx)"
                >
                  <v-row align="center">
                    <v-col @click="selected = r; overlay = true; conflictingRequests = []; checkHasConflicts();"
                      ><div class="hover">{{ r.Name }}</div></v-col
                    >
                    <v-col>{{ r.CreatedBy }}</v-col>
                    <v-col>{{ r.SubmittedOn | formatDateTime }}</v-col>
                    <v-col>{{ formatDates(r.EventDates) }}</v-col>
                    <v-col>{{ requestType(r) }}</v-col>
                    <v-col cols="3" class='d-flex justify-center'>
                      <event-action :r="r" v-on:calladdbuffer="callAddBuffer" v-on:setapproved="setApproved" v-on:setinprogress="setInProgress" v-on:partialapproval="partialApproval"></event-action>
                    </v-col>
                  </v-row>
                </v-list-item>
              </v-list>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-card>
            <v-card-text>
              <template v-if="sortedCurrent.length > 0">
                <v-row>
                  <v-col v-for="d in sortedCurrent" :key="d.Timeframe">
                    <v-list dense>
                      <v-list-item><strong>{{d.Timeframe}}</strong></v-list-item>
                      <v-list-item
                        v-for="(i, idx) in d.Events"
                        :key="`event_${idx}`"
                        class="event-pill hover"
                        @click="selected = i.Full; overlay = true;"
                      >
                        {{i.Name}} {{i.StartTime}} - {{formatRooms(i.Rooms)}}
                      </v-list-item>
                    </v-list>
                  </v-col>
                </v-row>
              </template>
              <template v-else>
                There are no approved events this week.
              </template>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
      <v-dialog 
        v-if="overlay" 
        v-model="overlay" 
        max-width="85%"
        style="margin-top: 100px !important; max-height: 80vh;"
      >
        <v-card
          light
          width="100%"
        >
          <v-card-title>
            <div>
              <template v-if="selected.Changes != null && selected.Name != selected.Changes.Name">
                <span class='red--text'>{{selected.Name}}: </span>
                <span class='primary--text'>{{selected.Changes.Name}}</span>
              </template>
              <template v-else>
                {{selected.Name}} 
                <v-icon color="accent" v-if="selected.IsValid">mdi-check-circle</v-icon>
                <v-icon color="inprogress" v-else>mdi-alert-circle</v-icon>
              </template>
              <div class='overline' color="inprogress" v-if="invalidSections(selected).length > 0">Invalid Sections: {{invalidSections(selected)}}</div>
            </div>
            <v-spacer></v-spacer>
            <div :class="getStatusPillClass(selected.RequestStatus)">
              {{selected.RequestStatus}}
            </div>
          </v-card-title>
          <v-card-text>
            <v-row>
              <v-col>
                <div class="floating-title">Submitted By</div>
                {{selected.CreatedBy}}
              </v-col>
              <v-col class="text-right">
                <div class="floating-title">Submitted On</div>
                {{selected.SubmittedOn | formatDateTime}}
              </v-col>
            </v-row>
            <hr />
            <v-row>
              <v-col>
                <div class="floating-title">Ministry</div>
                <template v-if="selected.Changes != null && selected.Ministry != selected.Changes.Ministry">
                  <span class='red--text'>{{formatMinistry(selected.Ministry)}}: </span>
                  <span class='primary--text'>{{formatMinistry(selected.Changes.Ministry)}}</span>
                </template>
                <template v-else>
                  {{formatMinistry(selected.Ministry)}}
                </template>
              </v-col>
              <v-col>
                <div class="floating-title">Contact</div>
                <template v-if="selected.Changes != null && selected.Contact != selected.Changes.Contact">
                  <span class='red--text'>{{selected.Contact}}: </span>
                  <span class='primary--text'>{{selected.Changes.Contact}}</span>
                </template>
                <template v-else>
                  {{selected.Contact}}
                </template>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <div class="floating-title">Requested Resources</div>
                {{requestType(selected)}}
              </v-col>
            </v-row>
            <v-expansion-panels v-model="panels" multiple flat>
              <v-expansion-panel v-for="(e, idx) in selected.Events" :key="`panel_${idx}`">
                <v-expansion-panel-header>
                  <template v-if="selected.IsSame || selected.Events.length == 1">
                    <template v-if="selected.Changes != null && formatDates(selected.EventDates) != formatDates(selected.Changes.EventDates)">
                      <span class='red--text'>{{formatDates(selected.EventDates)}}: </span>
                      <span class='primary--text'>{{formatDates(selected.Changes.EventDates)}} ({{formatRooms(e.Rooms)}})</span>
                    </template>
                    <template v-else>
                      {{formatDates(selected.EventDates)}} ({{formatRooms(e.Rooms)}})
                    </template>
                  </template>
                  <template v-else>
                    <template v-if="selected.Changes != null && formatDates(selected.EventDates) != formatDates(selected.Changes.EventDates)">
                      <span class='red--text'>{{e.EventDate | formatDate}}: </span>
                      <span class='primary--text'>{{selected.Changes.Events[idx].EventDate | formatDate}} ({{formatRooms(e.Rooms)}})</span>
                    </template>
                    <template v-else>
                      {{e.EventDate | formatDate}} ({{formatRooms(e.Rooms)}})
                    </template>
                  </template>
                </v-expansion-panel-header>
                <v-expansion-panel-content style="color: rgba(0,0,0,.6);">
                  <event-details :e="e" :idx="idx" :selected="selected" :approvalmode="false"></event-details>
                </v-expansion-panel-content>
              </v-expansion-panel>
            </v-expansion-panels>
            <template v-if="selected.needsPub || (selected.Changes && selected.Changes.needsPub)">
              <pub-details :request="selected" :approvalmode="false"></pub-details>
            </template>
            <v-row v-if="selected.Notes">
              <v-col>
                <div class="floating-title">Notes</div>
                <template v-if="selected.Changes != null && selected.Notes != selected.Changes.Notes">
                  <span class='red--text'>{{(selected.Notes ? selected.Notes : 'Empty')}}: </span>
                  <span class='primary--text'>{{(selected.Changes.Notes ? selected.Changes.Notes : 'Empty')}}</span>
                </template>
                <template v-else>
                  {{selected.Notes}}
                </template>
              </v-col>
            </v-row>
            <v-row v-if="selected.Comments && selected.Comments.length > 0">
              <v-col>
                <div class="floating-title">Comments</div>
                <div class='comment-viewer'>
                  <div v-for="(c,idx) in selected.Comments" :key="idx" class='comment'>
                    <strong>{{c.CreatedBy}}</strong> - {{c.CreatedOn | formatDateTime}}<br/>
                    {{c.Message}}
                  </div>
                </div>
              </v-col>
            </v-row>
            <v-row v-if="selected.HistoricData">
              <v-col>
                <div class="floating-title">Non-Transferrable Data</div>
                <div v-html="nonTransferable(selected.HistoricData)"></div>
              </v-col>
            </v-row>
            <v-row v-if="conflictingRequests.length > 0">
              <v-col>
                <strong>Conflicting Requests</strong>
                <v-list dense>
                  <v-list-item v-for="r in conflictingRequests" :idx="r.Id">
                    <v-list-item-content>
                      <a :href="`${currentPath}?Id=${r.Id}`">{{(JSON.parse(r.Value).Name)}}</a>
                    </v-list-item-content>
                  </v-list-item>
                </v-list>
              </v-col>
            </v-row>
          </v-card-text>
          <v-card-actions>
            <v-btn color="primary" @click="editRequest">
              <v-icon>mdi-pencil</v-icon> Edit
            </v-btn>
            <v-btn
              v-if="selected.RequestStatus != 'Approved'"
              color="accent"
              @click="setApproved(selected)"
            >
              <v-icon>mdi-check</v-icon> Approve
            </v-btn>
            <v-speed-dial
              v-model="fab"
              open-on-hover
              style="margin-left: 8px;"
            >
              <template v-slot:activator>
                <v-btn
                  v-if="selected.RequestStatus != 'Denied'"
                  color="red"
                  v-model="fab"
                >
                  <v-icon>mdi-close</v-icon> Deny
                </v-btn>
              </template>
              <v-btn
                v-if="selected.RequestStatus != 'Denied'"
                color="red"
                @click="changeStatus('Deny', selected.Id)"
              >
                <v-icon>mdi-close</v-icon> Request
              </v-btn>
              <v-btn
                v-if="selected.RequestStatus == 'Pending Changes'"
                color="red"
                @click="changeStatus('DenyUser', selected.Id)"
              >
                <v-icon>mdi-close</v-icon> Changes w/o Comment
              </v-btn>
              <v-btn
                v-if="selected.RequestStatus == 'Pending Changes'"
                color="red"
                @click="changeStatus('DenyUserComments', selected.Id)"
              >
                <v-icon>mdi-close</v-icon> Changes w/ Comment
              </v-btn>
            </v-speed-dial>
            <v-btn
              v-if="selected.RequestStatus != 'Cancelled'"
              color="grey"
              @click="changeStatus('Cancel', selected.Id)"
              style="margin-left: 8px;"
            >
              <v-icon>mdi-cancel</v-icon> Cancel
            </v-btn>
            <v-btn 
              @click="commentDialog = true"
              style="margin-left: 8px;"
              color="accent"
            >
              <v-icon>mdi-comment-edit</v-icon> Add Comment
            </v-btn>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="overlay = false; selected = {}">
              <v-icon>mdi-close</v-icon> Close
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
      <v-dialog
        v-model="dialog"
        v-if="dialog"
        max-width="50%"
      >
        <v-card>
          <v-card-title>
            {{selected.Name}}
          </v-card-title>
          <v-card-text>
            <v-alert v-if="bufferErrMsg != ''">{{bufferErrMsg}}</v-alert>
            <v-row v-for="(e, idx) in selected.Events" :key="`row_${idx}`">
              <v-col cols="12">
                <template v-if="selected.IsSame || selected.Events.length == 1">
                  {{formatDates(selected.EventDates)}} <br/>
                </template>
                <template v-else>
                  {{e.EventDate | formatDate}}
                </template>
                {{e.StartTime}} - {{e.EndTime}}
              </v-col>
              <v-col>
                <v-autocomplete
                  label="Set-up Buffer"
                  v-model="e.MinsStartBuffer"
                  :items="[{text: '15 Mins', value:'15'}, {text: '30 Mins', value:'30'}, {text: '45 Mins', value:'45'}, {text: '1 Hour', value:'60'}]"
                  clearable
                  ></v-autocomplete>
                </v-col>
                <v-col>
                  <v-autocomplete
                  label="Tear-down Buffer"
                  v-model="e.MinsEndBuffer"
                  :items="[{text: '15 Mins', value:'15'}, {text: '30 Mins', value:'30'}, {text: '45 Mins', value:'45'}, {text: '1 Hour', value:'60'}]"
                  clearable
                ></v-autocomplete>
              </v-col>
            </v-row>
          </v-card-text>
          <v-card-actions>
            <v-btn color="primary" @click="addBuffer">Add Buffer</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
      <v-dialog v-if="commentDialog" v-model="commentDialog" max-width="80%">
        <v-card>
          <v-card-title></v-card-title>
          <v-card-text>
            <v-textarea label="New Comment" v-model="comment"></v-textarea>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="saveComment">Add Comment</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
      <v-dialog
        v-if="approvalDialog" 
        v-model="approvalDialog" 
        max-width="85%"
        style="margin-top: 100px !important; max-height: 80vh;"
      >
        <partial-approval ref="partialApproval" :request="selected" v-on:approvechange="approveChange" v-on:denychange="denyChange" v-on:complete="sendPartialApproval" v-on:cancel="ignorePartialApproval" v-on:newchange="increaseChangeCount" v-on:newchoice="increaseSelectionCount" ></partial-approval>
      </v-dialog>
      <v-dialog
        v-if="approvalErrorDialog"
        v-model="approvalErrorDialog"
        max-width="85%"
        style="margin-top: 100px !important; max-height: 80vh;"
      >
        <v-card>
          <v-card-title>Error Approving Request</v-card-title>
          <v-card-text>
            <v-row>
              <v-col>
                <v-list dense>
                  <v-list-item>
                    <v-list-item-content>
                      <strong>Conflicts</strong> 
                    </v-list-item-content>
                  </v-list-item>
                  <v-list-item v-for="(m, idx) in conflictingMessage" :key="idx">
                    <v-list-item-content>
                      {{m}}
                    </v-list-item-content>
                  </v-list-item>
                </v-list>
              </v-col> 
              <v-col>
                <v-list dense>
                  <v-list-item>
                    <v-list-item-content>
                      <strong>Conflicting Requests</strong> 
                    </v-list-item-content>
                  </v-list-item>
                  <v-list-item v-for="(r, idx) in conflictingRequests" :key="idx">
                    <v-list-item-content>
                      <a :href="`${currentPath}?Id=${r.Id}`">{{(JSON.parse(r.Value).Name)}}</a>
                    </v-list-item-content>
                  </v-list-item>
                </v-list>
              </v-col> 
            </v-row> 
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="accent" @click="changeStatus('Approved', selected.Id)">
              Approve Anyways
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </v-app>
</div>
<script type="module">
import eventActions from '/Scripts/com_thecrossingchurch/EventSubmission/EventActions.js';
import eventDetails from '/Scripts/com_thecrossingchurch/EventSubmission/EventDetailsExpansion.js';
import pubDetails from '/Scripts/com_thecrossingchurch/EventSubmission/PublicityDetails.js';
import partialApproval from '/Scripts/com_thecrossingchurch/EventSubmission/PartialApproval.js';
import utils from '/Scripts/com_thecrossingchurch/EventSubmission/Utilities.js';
document.addEventListener("DOMContentLoaded", function () {
  Vue.component("event-action", eventActions);
  Vue.component("event-details", eventDetails);
  Vue.component("pub-details", pubDetails);
  Vue.component("partial-approval", partialApproval);
  new Vue({
    el: "#app",
    vuetify: new Vuetify({
      theme: {
        themes: {
          light: {
            primary: "#347689",
            secondary: "#3D3D3D",
            accent: "#8ED2C9",
            accentDark: "#6DC5B9",
            inprogress: '#ECC30B',
            denied: '#CC3F0C',
            pending: '#61A4A9'
          },
        },
      },
      iconfont: "mdi",
    }),
    config: {
      devtools: true,
    },
    data: {
      requests: [],
      allrequests: [],
      current: [],
      selected: {},
      overlay: false,
      dialog: false,
      approvalDialog: false,
      approvalErrorDialog: false,
      changes: [],
      changeCount: 0,
      selectedCount: 0,
      panels: [0],
      rooms: [],
      doors: [],
      ministries: [],
      budgetLines: [],
      bufferErrMsg: '',
      fab: false,
      commentDialog: false,
      comment: '',
      filters: {
        query: "",
        submitter: "",
        status: [],
        resources: []
      },
      conflictingMessage: '',
      conflictingRequests: []
    },
    created() {
      this.getRecent();
      this.getCurrent();
      this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value)
      this.doors = JSON.parse($('[id$="hfDoors"]')[0].value)
      this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value)
      this.budgetLines = JSON.parse($('[id$="hfBudgetLines"]')[0].value)
      window['moment-range'].extendMoment(moment)
      let query = new URLSearchParams(window.location.search);
      if (query.get('Id')) {
        this.selected = this.requests.filter(i => {
          if (i.Id == query.get('Id')) {
            return i
          }
        })[0]
        this.checkHasConflicts()
        this.overlay = true
      }
    },
    filters: {
      ...utils.filters
    },
    computed: {
      sortedCurrent() {
        let ordered = [
          { Timeframe: "Today", Events: [] },
          { Timeframe: "Tomorrow", Events: [] },
          { Timeframe: moment().add(2, "days").format("dddd"), Events: [] },
          { Timeframe: moment().add(3, "days").format("dddd"), Events: [] },
          { Timeframe: moment().add(4, "days").format("dddd"), Events: [] },
          { Timeframe: moment().add(5, "days").format("dddd"), Events: [] },
          { Timeframe: moment().add(6, "days").format("dddd"), Events: [] },
        ];
        this.current.forEach((i) => {
          let dates = i.EventDates;
          dates.forEach((d) => {
            let timeframe = [];
            if (d == moment().format("yyyy-MM-DD")) {
              timeframe.push("Today");
            }
            if (d == moment().add(1, "days").format("yyyy-MM-DD")) {
              timeframe.push("Tomorrow");
            }
            if (
              d == moment().add(2, "days").format("yyyy-MM-DD") ||
              d == moment().add(3, "days").format("yyyy-MM-DD") ||
              d == moment().add(4, "days").format("yyyy-MM-DD") ||
              d == moment().add(5, "days").format("yyyy-MM-DD") ||
              d == moment().add(6, "days").format("yyyy-MM-DD")
            ) {
              timeframe.push(moment(d).format("dddd"));
            }
            ordered.forEach((o) => {
              if (timeframe.includes(o.Timeframe)) {
                if (i.IsSame || i.Events.length == 1) {
                  o.Events.push({ Name: i.Name, Rooms: i.Events[0].Rooms, Full: i });
                } else {
                  let idx = i.EventDates.indexOf(d)
                  o.Events.push({ Name: i.Name, Rooms: i.Events[idx].Rooms, Full: i })
                }
              }
            })
          })
        })
        return ordered.filter((o) => {
          return o.Events.length > 0;
        })
      },
      currentPath() {
        return window.location.pathname
      }
    },
    methods: {
      ...utils.methods, 
      getRecent() {
        let raw = JSON.parse($('[id$="hfRequests"]').val());
        let temp = [];
        raw.forEach((i) => {
          let req = JSON.parse(i.Value);
          req.Id = i.Id;
          req.CreatedBy = i.CreatedBy;
          req.CreatedOn = i.CreatedOn;
          req.RequestStatus = i.RequestStatus;
          req.HistoricData = i.HistoricData;
          req.Changes = i.Changes != '' ? JSON.parse(i.Changes) : null;
          req.Comments = i.Comments;
          req.SubmittedOn = i.SubmittedOn;
          temp.push(req);
        });
        this.allrequests = temp;
        this.filter();
      },
      getCurrent() {
        let raw = JSON.parse($('[id$="hfCurrent"]').val());
        let temp = [];
        raw.forEach((i) => {
          let req = i.Request;
          req.Id = i.Id;
          req.CreatedBy = i.CreatedBy;
          req.CreatedOn = i.CreatedOn;
          req.RequestStatus = i.RequestStatus;
          req.HistoricData = i.HistoricData;
          req.SubmittedOn = i.SubmittedOn;
          temp.push(req);
        });
        this.current = temp;
      },
      getClass(idx) {
        if (idx < this.requests.length - 1) {
          return "list-with-border";
        }
        return "";
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
      editRequest() {
        let url = $('[id$="hfRequestURL"]').val();
        window.location = url + `?Id=${this.selected.Id}`;
      },
      openHistory() {
        let url = $('[id$="hfHistoryURL"]').val();
        window.location = url
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
      nonTransferable(val) {
        val = val.replace(/(?:\\[rn])+/g, '<br/>')
        return val
      },
      changeStatus(status, id) {
        $('[id$="hfRequestID"]').val(id)
        $('[id$="hfAction"]').val(status)
        $('[id$="btnChangeStatus"]')[0].click()
        $('#updateProgress').show()
      },
      callAddBuffer(r) {
        this.selected = r
        this.bufferErrMsg = ''
        this.dialog = true
      },
      setApproved(r) {
        if(r.Changes) {
          r = r.Changes
        } 
        r.Changes = null
        this.selected = r
        $('[id$="hfUpdatedItem"]').val(JSON.stringify(r))
        if(!this.checkHasConflicts()) {
          this.changeStatus('Approved', r.Id)
        } else {
          this.approvalErrorDialog = true
        }
      },
      setInProgress(r) {
        this.changeStatus('InProgress', r.Id)
      },
      partialApproval(r) {
        this.selected = r
        this.approvalDialog = true
      },
      addBuffer() {
        this.bufferErrMsg = ''
        if (!this.checkHasConflicts()) {
          $('[id$="hfRequestID"]').val(this.selected.Id);
          $('[id$="hfUpdatedItem"]').val(JSON.stringify(this.selected));
          $('[id$="btnAddBuffer"]')[0].click();
          $('#updateProgress').show();
        } else {
          this.bufferErrMsg = 'The buffer you have chosen will conflict with another event'
        }
      },
      saveComment() {
        $('[id$="hfRequestID"]').val(this.selected.Id);
        $('[id$="hfComment"]').val(this.comment);
        $('[id$="btnAddComment"')[0].click();
        $('#updateProgress').show();
      },
      filter() {
        let temp = this.allrequests;
        if (this.filters.submitter != "" && this.filters.submitter != null) {
          temp = temp.filter((i) => {
            return i.CreatedBy.toLowerCase().includes(
              this.filters.submitter.toLowerCase()
            );
          });
        }
        if (this.filters.status.length > 0) {
          temp = temp.filter((i) => {
            return this.filters.status.includes(i.RequestStatus);
          });
        }
        if (this.filters.resources.length > 0) {
          temp = temp.filter((i) => {
            let iRR = this.requestType(i).split(',')
            let intersects = false
            this.filters.resources.forEach(r => {
              if (iRR.includes(r)) {
                intersects = true
              }
            })
            return intersects
          })
        }
        if (this.filters.query != "" && this.filters.query != null) {
          temp = temp.filter((i) => {
            return i.Name.toLowerCase().includes(
              this.filters.query.toLowerCase()
            );
          });
        }
        this.requests = temp
      },
      checkHasConflicts() {
        this.existingRequests = JSON.parse(
          $('[id$="hfUpcomingRequests"]')[0].value
        );
        this.conflictingMessage = []
        this.conflictingRequests = this.existingRequests.filter((r) => {
          if (r.Id == this.selected.Id) {
            return false
          }
          r = JSON.parse(r.Value);
          let compareTarget = [], compareSource = []
          //Build an object for each date to compare with 
          if (r.IsSame || r.Events.length == 1) {
            for (let i = 0; i < r.EventDates.length; i++) {
              compareTarget.push({ Date: r.EventDates[i], StartTime: r.Events[0].StartTime, EndTime: r.Events[0].EndTime, Rooms: r.Events[0].Rooms, MinsStartBuffer: r.Events[0].MinsStartBuffer, MinsEndBuffer: r.Events[0].MinsEndBuffer });
            }
          } else {
            for (let i = 0; i < r.Events.length; i++) {
              compareTarget.push({ Date: r.Events[i].EventDate, StartTime: r.Events[i].StartTime, EndTime: r.Events[i].EndTime, Rooms: r.Events[i].Rooms, MinsStartBuffer: r.Events[i].MinsStartBuffer, MinsEndBuffer: r.Events[i].MinsEndBuffer });
            }
          }
          if (this.selected.Events.length == 1 || this.selected.IsSame) {
            for (let i = 0; i < this.selected.EventDates.length; i++) {
              compareSource.push({ Date: this.selected.EventDates[i], StartTime: this.selected.Events[0].StartTime, EndTime: this.selected.Events[0].EndTime, Rooms: this.selected.Events[0].Rooms, MinsStartBuffer: this.selected.Events[0].MinsStartBuffer, MinsEndBuffer: this.selected.Events[0].MinsEndBuffer })
            }
          } else {
            for (let i = 0; i < this.selected.Events.length; i++) {
              compareSource.push({ Date: this.selected.Events[i].EventDate, StartTime: this.selected.Events[i].StartTime, EndTime: this.selected.Events[i].EndTime, Rooms: this.selected.Events[i].Rooms, MinsStartBuffer: this.selected.Events[i].MinsStartBuffer, MinsEndBuffer: this.selected.Events[i].MinsEndBuffer })
            }
          }
          let conflicts = false
          for (let x = 0; x < compareTarget.length; x++) {
            for (let y = 0; y < compareSource.length; y++) {
              if (compareTarget[x].Date == compareSource[y].Date) {
                //On same date
                //Check for conflicting rooms
                let conflictingRooms = compareSource[y].Rooms.filter(value => compareTarget[x].Rooms.includes(value));
                if (conflictingRooms.length > 0) {
                  //Check they do not overlap with moment-range
                  let cdStart = moment(`${compareTarget[x].Date} ${compareTarget[x].StartTime}`, `yyyy-MM-DD hh:mm A`);
                  if (compareTarget[x].MinsStartBuffer) {
                    cdStart = cdStart.subtract(r.MinsStartBuffer, "minute");
                  }
                  let cdEnd = moment(`${compareTarget[x].Date} ${compareTarget[x].EndTime}`, `yyyy-MM-DD hh:mm A`);
                  if (compareTarget[x].MinsEndBuffer) {
                    cdEnd = cdEnd.add(compareTarget[x].MinsEndBuffer, "minute");
                  }
                  let cRange = moment.range(cdStart, cdEnd);
                  let current = moment.range(
                    moment(`${compareSource[y].Date} ${compareSource[y].StartTime}`, `yyyy-MM-DD hh:mm A`),
                    moment(`${compareSource[y].Date} ${compareSource[y].EndTime}`, `yyyy-MM-DD hh:mm A`)
                  );
                  if (cRange.overlaps(current)) {
                    conflicts = true
                    let roomNames = []
                    conflictingRooms.forEach(r => {
                      let roomName = this.rooms.filter((room) => {
                        return room.Id == r;
                      })
                      if (roomName.length > 0) {
                        roomName = roomName[0].Value;
                      }
                      roomNames.push(roomName)
                    })
                    this.conflictingMessage.push(`${moment(compareSource[y].Date).format('MM/DD/yyyy')} (${roomNames.join(", ")})`)
                  }
                }
              }
            }
          }
          return conflicts
        })
        
        if (this.conflictingRequests.length > 0) {
          return true
        } else {
          return false
        }
      },
      approveChange(field) {
        let exists = this.changes.filter(c => { return c.label == field.label})
        if(exists.length > 0) {
          exists[0].isApproved = true
        } else {
          this.changes.push({label: field.label, field: field.field, isApproved: true, idx: field.idx})
        }
      },
      denyChange(field) {
        let exists = this.changes.filter(c => { return c.label == field.label})
        if(exists.length > 0) {
          exists[0].isApproved = false
        } else {
          this.changes.push({label: field.label, field: field.field, isApproved: false, idx: field.idx})
        }
      },
      increaseChangeCount() {
        this.changeCount++
      },
      increaseSelectionCount() {
        this.selectedCount++
      },
      sendPartialApproval() {
        if(this.changeCount > this.selectedCount){
          //Don't save until he picks something for every change
          window.alert("Not all changes have been approved or denied")
        } else {
          for(let i=0; i<this.changes.length; i++) {
            if(this.changes[i].isApproved) {
              if(this.changes[i].idx != null) {
                this.selected.Events[this.changes[i].idx][this.changes[i].field] = this.selected.Changes.Events[this.changes[i].idx][this.changes[i].field]
              } else {
                this.selected[this.changes[i].field] = this.selected.Changes[this.changes[i].field]
              }
            } 
          }
          console.log(this.selected)
          //Click the button
          $('[id$="hfChanges"]').val(JSON.stringify(this.changes))
          $('[id$="hfRequestID"]').val(this.selected.Id)
          $('[id$="hfUpdatedItem"]').val(JSON.stringify(this.selected))
          $('[id$="btnPartialApproval"]')[0].click()
          $('#updateProgress').show()
        } 
      },
      ignorePartialApproval() {
        this.changes = []
        this.approvalDialog = false
      }
    },
  });
});
</script>
<style>
  .theme--light.v-application {
    background: rgba(0, 0, 0, 0);
  }
  .text--accent {
    color: #8ED2C9;
  }
  .text--error, .text--denied {
    color: #CC3F0C;
  }
  .row {
    margin: 0;
  }
  .col {
    padding: 4px 12px !important;
  }
  .no-top-pad {
    padding: 0px 12px !important;
  }
  input[type="text"]:focus,
  textarea:focus {
    border: none !important;
    box-shadow: none !important;
  }
  .v-input__slot {
    min-height: 42px !important;
  }
  .v-window {
    overflow: visible !important;
  }
  .btn-hidden {
    visibility: hidden;
  }
  .list-with-border {
    border-bottom: 1px solid #c7c7c7;
  }
  .hover {
    cursor: pointer;
  }
  .v-overlay__content {
    width: 60%;
  }
  .v-dialog:not(.v-dialog--fullscreen) {
    max-height: 80vh !important;
  }
  .v-dialog {
    margin-top: 100px !important;
  }
  .floating-title {
    text-transform: uppercase;
    font-size: 0.65rem;
  }
  .event-pill {
    border-radius: 6px;
    border: 2px solid #5f9bad;
  }
  .status-pill {
    border-radius: 6px;
    display: flex;
    min-height: 36px;
    align-items: center;
    justify-content: center;
  }
  .status-pill.submitted {
    border: 2px solid #347689;
  }
  .status-pill.approved {
    border: 2px solid #8ed2c9;
  }
  .status-pill.denied {
    border: 2px solid #f44336;
  }
  .status-pill.cancelled {
    border: 2px solid #9e9e9e;
  }
  .status-pill.inprogress {
    border: 2px solid #ECC30B;
  }
  ::-webkit-scrollbar {
    width: 5px;
    border-radius: 3px;
  }
  ::-webkit-scrollbar-track {
    background: #bfbfbf;
    -webkit-box-shadow: inset 1px 1px 2px rgba(0, 0, 0, 0.1);
  }
  ::-webkit-scrollbar-thumb {
    background: rgb(224, 224, 224);
    -webkit-box-shadow: inset 1px 1px 2px rgba(0, 0, 0, 0.2);
  }
  ::-webkit-scrollbar-thumb:hover {
    background: #aaa;
  }
  ::-webkit-scrollbar-thumb:active {
    background: #888;
    -webkit-box-shadow: inset 1px 1px 2px rgba(0, 0, 0, 0.3);
  }
  .v-expansion-panel--active>.v-expansion-panel-header {
    border-bottom: 1px solid #e2e2e2;
  }
  .comment {
    background-color: lightgrey;
    padding: 8px;
    border-radius: 6px;
    margin: 4px 0px;
  }
  [v-cloak] {
    display: none !important;
  }
</style>