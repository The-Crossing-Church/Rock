<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="EventSubmissionHistory.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.EventSubmission.EventSubmissionHistory"
%> <%-- Add Vue and Vuetify CDN --%>
<!-- <script src="https://cdn.jsdelivr.net/npm/vue@2.6.12"></script> -->
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
<asp:HiddenField ID="hfDashboardURL" runat="server" />
<asp:HiddenField ID="hfIsSuperUser" runat="server" />

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
                  <v-btn color="primary" class="pull-right" @click="filter"
                    >Filter</v-btn
                  >
                </v-col>
              </v-row>
              <v-data-table
                :headers="headers"
                :items="requests"
                :items-per-page="15"
                :footer-props="{
                  'items-per-page-options': [5, 15, 30, 50, -1]
                }"
              >
                <template v-slot:item="{ item, index }">
                  <tr
                    @click="selected = item; overlay = true;"
                    :data-id="item.Id"
                    style="width: 100%;"
                  >
                    <td>
                      {{item.Name}}
                    </td>
                    <td>
                      {{item.CreatedBy}}
                    </td>
                    <td>
                      {{item.CreatedOn | formatDateTime}}
                    </td>
                    <td style="max-width: 300px;">
                      {{formatDates(item.EventDates)}}
                    </td>
                    <td>
                      {{requestType(item)}}
                    </td>
                    <td>
                      <event-action :r="item"></event-action>
                    </td>
                  </tr>
                </template>
              </v-data-table>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
      <v-dialog 
        v-model="overlay" 
        v-if="overlay"
        max-width="85%"
        style="margin-top: 100px !important; max-height: 80vh;"
      >
        <v-card
          light
          width="100%"
        >
          <v-card-title>
            {{selected.Name}}
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
                {{selected.CreatedOn | formatDateTime}}
              </v-col>
            </v-row>
            <hr />
            <v-row>
              <v-col>
                <div class="floating-title">Ministry</div>
                {{formatMinistry(selected.Ministry)}}
              </v-col>
              <v-col>
                <div class="floating-title">Contact</div>
                {{selected.Contact}}
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <div class="floating-title">Requested Resources</div>
                {{requestType(this.selected)}}
              </v-col>
            </v-row>
            <v-expansion-panels v-model="panels" multiple flat>
              <v-expansion-panel v-for="(e, idx) in selected.Events" :key="`panel_${idx}`">
                <v-expansion-panel-header>
                  <template v-if="selected.IsSame || selected.Events.length == 1">
                    {{formatDates(selected.EventDates)}} ({{formatRooms(e.Rooms)}})
                  </template>
                  <template v-else>
                    {{e.EventDate | formatDate}} ({{formatRooms(e.Rooms)}})
                  </template>
                </v-expansion-panel-header>
                <v-expansion-panel-content>
                  <event-details :e="e" :idx="idx" :selected="selected"></event-details>
                </v-expansion-panel-content>
              </v-expansion-panel>
            </v-expansion-panels>
            <template v-if="selected.needsPub">
              <h6 class='text--accent text-uppercase'>Publicity Information</h6>
              <v-row>
                <v-col>
                  <div class="floating-title">Describe Why Someone Should Attend Your Event (450)</div>
                  {{selected.WhyAttendSixtyFive}}
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Target Audience</div>
                  {{selected.TargetAudience}}
                </v-col>
                <v-col>
                  <div class="floating-title">Event is Sticky</div>
                  {{boolToYesNo(selected.EventIsSticky)}}
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Publicity Start Date</div>
                  {{selected.PublicityStartDate | formatDate}}
                </v-col>
                <v-col>
                  <div class="floating-title">Publicity End Date</div>
                  {{selected.PublicityEndDate | formatDate}}
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Publicity Strategies</div>
                  {{selected.PublicityStrategies.join(', ')}}
                </v-col>
              </v-row>
              <template v-if="selected.PublicityStrategies.includes('Social Media/Google Ads')">
                <v-row>
                  <v-col>
                    <div class="floating-title">Describe Why Someone Should Attend Your Event (90)</div>
                    {{selected.WhyAttendNinety}}
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <div class="floating-title">Google Keys</div>
                    <ul>
                      <li v-for="k in selected.GoogleKeys" :key="k">
                        {{k}}
                      </li>
                    </ul>
                  </v-col>
                </v-row>
              </template>
              <template v-if="selected.PublicityStrategies.includes('Mobile Worship Folder')">
                <v-row>
                  <v-col>
                    <div class="floating-title">Describe Why Someone Should Attend Your Event (65)</div>
                    {{selected.WhyAttendTen}}
                  </v-col>
                  <v-col v-if="selected.VisualIdeas != ''">
                    <div class="floating-title">Visual Ideas for Graphic</div>
                    {{selected.VisualIdeas}}
                  </v-col>
                </v-row>
              </template>
              <template v-if="selected.PublicityStrategies.includes('Announcement')">
                <v-row v-for="(s, sidx) in selected.Stories" :key="`Story_${sidx}`">
                  <v-col>
                    <div class="floating-title">Story {{sidx+1}}</div>
                    {{s.Name}}, {{s.Email}} <br/>
                    {{s.Description}}
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <div class="floating-title">Describe Why Someone Should Attend Your Event (175)</div>
                    {{selected.WhyAttendTwenty}}
                  </v-col>
                </v-row>
              </template>
            </template>
            <v-row v-if="selected.Notes">
              <v-col>
                <div class="floating-title">Notes</div>
                {{selected.Notes}}
              </v-col>
            </v-row>
            <v-row v-if="selected.HistoricData">
              <v-col>
                <div class="floating-title">Non-Transferrable Data</div>
                <div v-html="selected.HistoricData"></div>
              </v-col>
            </v-row>
          </v-card-text>
          <v-card-actions>
            <v-btn color="accent" @click="openInDash">
              Open in Dashboard
            </v-btn>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="overlay = false; selected = {}">
              <v-icon>mdi-close</v-icon> Close
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </v-app>
</div>
<script type="module">
import eventActions from '/Scripts/com_thecrossingchurch/EventSubmission/EventActions.js?v=1.0.4';
import eventDetails from '/Scripts/com_thecrossingchurch/EventSubmission/EventDetailsExpansion.js?v=1.0.4';
import utils from '/Scripts/com_thecrossingchurch/EventSubmission/Utilities.js?v=1.0.4';
document.addEventListener("DOMContentLoaded", function () {
  Vue.component("event-action", eventActions);
  Vue.component("event-details", eventDetails);
  new Vue({
    el: "#app",
    vuetify: new Vuetify({
      theme: {
        themes: {
          light: {
            primary: "#347689",
            secondary: "#3D3D3D",
            accent: "#8ED2C9",
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
        selected: {},
        overlay: false,
        panels: [0],
        rooms: [],
        doors: [],
        ministries: [],
        budgetLines: [],
        page: 0,
        rows: 15,
        filters: {
          query: "",
          submitter: "",
          status: [],
          resources: []
        },
        headers: [
          { text: "Request", value: "Name" },
          { text: "Submitted By", value: "CreatedBy" },
          { text: "Submitted On", value: "CreatedOn" },
          { text: "Event Dates", value: "EventDates" },
          { text: "Requested Resources", value: "Id" },
          { text: "Status", value: "RequestStatus" },
        ]
    },
    created() {
      this.getRequests()
      this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value)
      this.doors = JSON.parse($('[id$="hfDoors"]')[0].value)
      this.budgetLines = JSON.parse($('[id$="hfBudgetLines"]')[0].value)
      this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value)
      window['moment-range'].extendMoment(moment)
    },
    filters: {
      ...utils.filters
    },
    computed: {
      foodTimeTitle() {
        if (this.selected) {
          if (this.selected.FoodDelivery) {
            return "Food Set-up time";
          } else {
            return "Desired Pick-up time from Vendor";
          }
        }
        return "";
      },
    },
    methods: {
      ...utils.methods,
      getRequests() {
          let raw = JSON.parse($('[id$="hfRequests"]').val());
          let temp = [];
          raw.forEach((i) => {
              let req = JSON.parse(i.Value);
              req.Id = i.Id;
              req.CreatedBy = i.CreatedBy;
              req.CreatedOn = i.CreatedOn;
              req.RequestStatus = i.RequestStatus;
              req.HistoricData = i.HistoricData;
              temp.push(req);
          });
          this.allrequests = temp;
          this.requests = temp;
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
      paginate(val) {
        if (val == "next") {
          let total = this.requests.length;
          if (total / this.rows > this.page) {
            this.page++;
          }
        } else {
          if (this.page > 0) {
            this.page--;
          }
        }
      },
      saveFile(idx, type) {
        var a = document.createElement("a")
        a.style = "display: none"
        document.body.appendChild(a)
        if (type == 'existing') {
            a.href = this.selected.Events[idx].SetUpImage.data
            a.download = this.selected.Events[idx].SetUpImage.name
        } else if (type == 'new') {
          a.href = this.selected.Changes.Events[idx].SetUpImage.data
          a.download = this.selected.Changes.Events[idx].SetUpImage.name
        }
        a.click()
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
      openInDash() {
        let url = $('[id$="hfDashboardURL"]')[0].value
        url += `?Id=${this.selected.Id}`
        window.location = url
      }
    },
    watch: {
      page(val) {
        this.filter()
      },
      rows(val) {
        this.filter()
      },
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
  [v-cloak] {
    display: none !important;
  }
  .layout {
    display: inline-block;
    width: 100%;
  }
  /* Get rid of the table scroll bar */
  .v-data-table__wrapper {
    overflow-x: hidden;
  }
</style>
