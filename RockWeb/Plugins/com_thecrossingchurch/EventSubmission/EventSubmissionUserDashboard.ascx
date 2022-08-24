<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="EventSubmissionUserDashboard.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.EventSubmission.EventSubmissionUserDashboard"
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
<asp:HiddenField ID="hfRequest" runat="server" />
<asp:HiddenField ID="hfRequests" runat="server" />
<asp:HiddenField ID="hfRequestURL" runat="server" />
<asp:HiddenField ID="hfWorkflowURL" runat="server" />
<asp:HiddenField ID="hfRequestID" runat="server" />
<asp:HiddenField ID="hfComment" runat="server" />
<asp:HiddenField ID="hfIsSuperUser" runat="server" />
<asp:HiddenField ID="hfStaffList" runat="server" />
<asp:HiddenField ID="hfSharedWith" runat="server" />
<Rock:BootstrapButton
  ID="btnAddComment"
  CssClass="btn-hidden"
  runat="server"
  OnClick="AddComment_Click"
/>
<Rock:BootstrapButton
  ID="btnResubmitRequest"
  CssClass="btn-hidden"
  runat="server"
  OnClick="ResubmitRequest_Click"
/>
<Rock:BootstrapButton
  ID="btnShareRequest"
  CssClass="btn-hidden"
  runat="server"
  OnClick="ShareRequest_Click"
/>

<div id="app" v-cloak>
  <v-app v-cloak>
    <div>
      <v-row>
        <v-col>
          <v-card>
            <v-card-text>
              <v-expansion-panels v-model="expansion" flat>
                <v-expansion-panel>
                  <v-expansion-panel-header>
                    <h5 style="width:100%;"><i class='fa fa-filter'></i> Filter Requests</h5>
                  </v-expansion-panel-header>
                  <v-expansion-panel-content>
                    <v-row>
                      <v-col cols="12" md="6">
                        <v-text-field
                          label="Request Title"
                          v-model="filters.title"
                          clearable
                        ></v-text-field>
                      </v-col>
                      <v-col cols="12" md="6">
                        <v-autocomplete
                          label="Request Status"
                          v-model="filters.status"
                          :items="['Draft','Submitted','In Progress','Approved','Denied','Cancelled','Pending Changes','Proposed Changes Denied','Changes Accepted by User','Cancelled by User']"
                          multiple
                          attach
                          clearable
                        ></v-autocomplete>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col cols="12" md="3">
                        <v-menu
                          v-model="eventStartMenu"
                          :close-on-content-click="false"
                          :nudge-right="40"
                          transition="scale-transition"
                          offset-y
                          min-width="290px"
                          attach
                        >
                          <template v-slot:activator="{ on, attrs }">
                            <v-text-field
                              v-model="filters.eventStart"
                              label="Requests with an event date after..."
                              prepend-inner-icon="mdi-calendar"
                              readonly
                              v-bind="attrs"
                              v-on="on"
                              clearable
                            ></v-text-field>
                          </template>
                          <v-date-picker
                            v-model="filters.eventStart"
                            @input="eventStartMenu = false"
                          ></v-date-picker>
                        </v-menu>
                      </v-col>
                      <v-col cols="12" md="3">
                        <v-menu
                          v-model="eventEndMenu"
                          :close-on-content-click="false"
                          :nudge-right="40"
                          transition="scale-transition"
                          offset-y
                          min-width="290px"
                          attach
                        >
                          <template v-slot:activator="{ on, attrs }">
                            <v-text-field
                              v-model="filters.eventEnd"
                              label="Requests with an event date before..."
                              prepend-inner-icon="mdi-calendar"
                              readonly
                              v-bind="attrs"
                              v-on="on"
                              clearable
                            ></v-text-field>
                          </template>
                          <v-date-picker
                            v-model="filters.eventEnd"
                            @input="eventEndMenu = false"
                          ></v-date-picker>
                        </v-menu>
                      </v-col>
                      <v-col cols="12" md="3">
                        <v-menu
                          v-model="createStartMenu"
                          :close-on-content-click="false"
                          :nudge-right="40"
                          transition="scale-transition"
                          offset-y
                          min-width="290px"
                          attach
                        >
                          <template v-slot:activator="{ on, attrs }">
                            <v-text-field
                              v-model="filters.createStart"
                              label="Requests created after..."
                              prepend-inner-icon="mdi-calendar"
                              readonly
                              v-bind="attrs"
                              v-on="on"
                              clearable
                            ></v-text-field>
                          </template>
                          <v-date-picker
                            v-model="filters.createStart"
                            @input="createStartMenu = false"
                          ></v-date-picker>
                        </v-menu>
                      </v-col>
                      <v-col cols="12" md="3">
                        <v-menu
                          v-model="createEndMenu"
                          :close-on-content-click="false"
                          :nudge-right="40"
                          transition="scale-transition"
                          offset-y
                          min-width="290px"
                          attach
                        >
                          <template v-slot:activator="{ on, attrs }">
                            <v-text-field
                              v-model="filters.createEnd"
                              label="Requests created before..."
                              prepend-inner-icon="mdi-calendar"
                              readonly
                              v-bind="attrs"
                              v-on="on"
                              clearable
                            ></v-text-field>
                          </template>
                          <v-date-picker
                            v-model="filters.createEnd"
                            @input="createEndMenu = false"
                          ></v-date-picker>
                        </v-menu>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col align="right">
                        <v-btn color="primary" @click="filter">Filter</v-btn>
                      </v-col>
                    </v-row>
                  </v-expansion-panel-content>
                </v-expansion-panel>
              </v-expansion-panels>
              <v-list>
                <v-list-item class="list-with-border">
                  <v-row>
                    <v-col><strong>Request</strong></v-col>
                    <v-col><strong>Submitted On</strong></v-col>
                    <v-col><strong>Event Dates</strong></v-col>
                    <v-col><strong>Requested Resources</strong></v-col>
                    <v-col cols="3"><strong>Status</strong></v-col>
                  </v-row>
                </v-list-item>
                <v-list-item
                  v-for="(r, idx) in filteredRequests"
                  :key="r.Id"
                  :class="getClass(idx)"
                >
                  <v-row align="center">
                    <v-col @click="selected = r; overlay = true;">
                      <div class="hover">{{ r.Name }}</div>
                    </v-col>
                    <v-col>{{ r.CreatedOn | formatDateTime }}</v-col>
                    <v-col>{{ formatDates(r.EventDates) }}</v-col>
                    <v-col>{{ requestType(r) }}</v-col>
                    <v-col cols="3" class='d-flex justify-left'>
                      <event-actions :r="r" v-on:editrequest="editRequest" v-on:cancelrequest="cancelRequest" v-on:resubmitrequest="resubmitRequest" v-on:commentrequest="addComment"></event-actions>
                    </v-col>
                  </v-row>
                </v-list-item>
              </v-list>
            </v-card-text>
          </v-card>
        </v-col>
      </v-row>
      <v-row>
        <v-col cols="12" md="6">
          <v-card>
            <v-card-text>
              <h5>Your Upcoming Events</h5>
              <hr/>
              <template v-if="sortedCurrent.length > 0">
                <v-row v-for="d in sortedCurrent" :key="d.Timeframe">
                  <v-col>
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
                You have no approved events this week.
              </template>
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
            <template v-if="selected.Changes != null && selected.Name != selected.Changes.Name">
              <span class='red--text'>{{selected.Name}}: </span>
              <span class='primary--text'>{{selected.Changes.Name}}</span>
            </template>
            <template v-else>
              {{selected.Name}}
            </template>
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
                <div v-html="selected.HistoricData"></div>
              </v-col>
            </v-row>
          </v-card-text>
          <v-card-actions>
            <v-btn color="primary" @click="editRequest"
              v-if="selected.RequestStatus == 'Draft' || selected.RequestStatus == 'Submitted' || selected.RequestStatus == 'In Progress' || selected.RequestStatus == 'Approved'"
            >
              <v-icon>mdi-pencil</v-icon> Edit
            </v-btn>
            <v-btn
              v-if="selected.RequestStatus == 'Proposed Changes Denied'"
              color="primary"
              @click="changeStatus('Original', selected.Id)"
            >
              <v-icon>mdi-check</v-icon> Use Originally Approved Request
            </v-btn>
            <v-btn
              v-if="selected.RequestStatus != 'Cancelled' && selected.RequestStatus != 'Draft'"
              color="grey"
              @click="changeStatus('Cancel', selected.Id)"
              style="margin-left: 8px;"
            >
              <v-icon>mdi-cancel</v-icon> Cancel Request
            </v-btn>
            <v-btn 
              v-if="selected.RequestStatus != 'Draft'"
              @click="commentDialog = true"
              style="margin-left: 8px;"
              color="accent"
            >
              <v-icon>mdi-comment-edit</v-icon> Add Comment
            </v-btn>
            <v-btn
              @click="resubmitRequest"
              style="margin-left: 8px;"
              color="pending"
            >
              <v-icon>mdi-calendar-refresh</v-icon> Resubmit
            </v-btn>
            <v-btn
              v-if="isSuperUser"
              @click="shareWithInput = selected.SharedWith; shareDialog = true;"
              style="margin-left: 8px;"
              color="draft"
            >
              <v-icon>mdi-share-variant</v-icon> Share
            </v-btn>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="overlay = false; selected = {}">
              <v-icon>mdi-close</v-icon> Close
            </v-btn>
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
      <v-dialog v-if="resubmitDialog" v-model="resubmitDialog" max-width="80%">
        <v-card>
          <v-card-title>Resubmit {{selected.Name}}</v-card-title>
          <v-card-text>
            <template v-if="selected.Events.length == 1">
              To resubmit a request, please select the new date(s) for your event.
            </template>
            <template v-else>
              To resubmit a request, you must select a replacement date for each date that was in the original request or remove the date. Information about each original date will then be pre-filled for the new dates you select. The new request will be saved as a draft you must edit before submitting. 
            </template>
            <div class='overline'>Resources requested for {{selected.Name}}</div>
            <div>Remove any you won't need for your re-submission of this event</div>
            <v-chip v-if="copy.needsSpace" close close-icon="mdi-close" @click:close="copy.needsSpace = false" class="mt-s-1">Space</v-chip>
            <v-chip v-if="copy.needsOnline" close close-icon="mdi-close" @click:close="copy.needsOnline = false" class="mt-s-1">Zoom</v-chip>
            <v-chip v-if="copy.needsCatering" close close-icon="mdi-close" @click:close="copy.needsCatering = false" class="mt-s-1">Catering</v-chip>
            <v-chip v-if="copy.needsChildCare" close close-icon="mdi-close" @click:close="copy.needsChildCare = false" class="mt-s-1">ChildCare</v-chip>
            <v-chip v-if="copy.needsReg" close close-icon="mdi-close" @click:close="copy.needsReg = false" class="mt-s-1">Registration</v-chip>
            <v-chip v-if="copy.needsAccom" close close-icon="mdi-close" @click:close="copy.needsAccom = false" class="mt-s-1">Special Accomodations</v-chip>
            <v-chip v-if="copy.needsPub" close close-icon="mdi-close" @click:close="copy.needsPub = false" class="mt-s-1">Publicity</v-chip>
            <template v-if="selected.Events.length == 1">
              <v-row>
                <v-col cols="6">
                  <strong>Select your new dates</strong><br/>
                  <v-date-picker
                    multiple
                    v-model="copy.EventDates"
                    :min="earliestDateForResubmission"
                    elevation="1"
                    landscape
                  ></v-date-picker>
                </v-col>
                <v-col cols="3" xs="6">
                  <v-list dense>
                    <v-list-item v-for="(d, idx) in copy.EventDates" :key="d">
                      <v-list-item-content>{{d | formatDate}}</v-list-item-content>
                      <v-list-item-action>
                        <v-btn icon color="denied" @click="copy.EventDates.splice(idx, 1)">
                          <v-icon>mdi-close</v-icon>
                        </v-btn>
                      </v-list-item-action>
                    </v-list-item>
                  </v-list>
                </v-col>
              </v-row>
            </template>
            <template v-else>
              <v-row v-for="(e, idx) in selected.EventDates" :key="e">
                <v-col>
                  <v-text-field
                    v-model="formatDate(e)"
                    readonly
                  ></v-text-field>
                </v-col>
                <template v-if="!resubmissionData[idx].wasRemoved">
                  <v-col>
                    <date-picker label="New Date" v-model="resubmissionData[idx].date" :min="earliestDateForResubmission"></date-picker>
                  </v-col>
                  <v-col cols="1" align-self="center">
                    <v-btn fab small color="denied" @click="resubmissionData[idx].wasRemoved = true; resubmissionData[idx].date = '';">
                      <v-icon>mdi-close</v-icon>
                    </v-btn>
                  </v-col>
                </template>
                <template v-else>
                  <v-col>
                    <v-text-field
                      value="Date Removed"
                      disabled
                    ></v-text-field>
                  </v-col>
                  <v-col cols="1" align-self="center">
                    <v-btn fab small color="accent" @click="resubmissionData[idx].wasRemoved = false;">
                      <v-icon>mdi-plus</v-icon>
                    </v-btn>
                  </v-col>
                </template>
              </v-row>
            </template>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="resubmit" :disabled="!canResubmit">Resubmit Request</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
      <v-dialog
        v-if="shareDialog"
        v-model="shareDialog"
        max-width="75%"
      >
        <v-card>
          <v-card-title>Share Request</v-card-title>
          <v-card-text>
            The request sharing feature is only to be used when you are no longer the owner of an event and need to pass ownership to someone else. 
            An example would be covering for someone on leave and giving them ownership of events you created for their ministry while they were on leave.
            <v-autocomplete
              :items="staff"
              item-value="Id"
              item-text="Name"
              v-model="shareWithInput"
              :search-input.sync="searchInput"
              @change="searchInput=''"
              multiple
              chips
              deletable-chips
            >
              <template v-slot:item="data">
                <v-list-item-content>
                  <v-list-item-title>{{data.item.Name}}</v-list-item-title>
                  <v-list-item-subtitle>{{data.item.Email}}</v-list-item-subtitle>
                </v-list-item-content>
              </template>
            </v-autocomplete>
          </v-card-text> 
          <v-card-actions>
            <v-btn color="primary" @click="shareRequest" :disabled="shareWithInput == ''">Share</v-btn>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="shareDialog = false; shareWithInput = ''">
              <v-icon>mdi-close</v-icon> Close
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </v-app>
</div>
<script type="module">
import eventActions from '/Scripts/com_thecrossingchurch/EventSubmission/UserEventActions.js?v=1.0.5';
import eventDetails from '/Scripts/com_thecrossingchurch/EventSubmission/EventDetailsExpansion.js?v=1.0.5';
import datePicker from '/Scripts/com_thecrossingchurch/EventSubmission/DatePicker.js?v=1.0.5';
import pubDetails from '/Scripts/com_thecrossingchurch/EventSubmission/PublicityDetails.js?v=1.0.5';
import utils from '/Scripts/com_thecrossingchurch/EventSubmission/Utilities.js?v=1.0.5';
document.addEventListener("DOMContentLoaded", function () {
  Vue.component("event-actions", eventActions);
  Vue.component("event-details", eventDetails);
  Vue.component("date-picker", datePicker);
  Vue.component("pub-details", pubDetails);
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
            pending: '#61A4A9',
            draft: '#A18276'
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
      filteredRequests: [],
      selected: {},
      copy: {},
      resubmissionData: [],
      overlay: false,
      panels: [0],
      rooms: [],
      ministries: [],
      staff: [],
      filters: {
        status: [],
        title: '',
        eventStart: '',
        eventEnd: '',
        createStart: moment().subtract(3, 'weeks').format('YYYY-MM-DD'),
        createEnd: '',
      },
      expansion: [],
      eventStartMenu: false,
      eventEndMenu: false,
      createStartMenu: false,
      createEndMenu: false,
      commentDialog: false,
      resubmitDialog: false,
      shareDialog: false,
      shareWithInput: '',
      searchInput: '',
      comment: '',
      isSuperUser: false
    },
    created() {
      this.getRecent();
      this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
      this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value)
      this.staff = JSON.parse($('[id$="hfStaffList"]')[0].value)
      let isSU = $('[id$="hfIsSuperUser"]')[0].value
      if(isSU == 'True') {
        this.isSuperUser = true
      }
      window['moment-range'].extendMoment(moment)
      let query = new URLSearchParams(window.location.search);
      if (query.get('Id')) {
        this.selected = this.requests.filter(i => {
          if (i.Id == query.get('Id')) {
            return i
          }
        })[0]
        this.overlay = true
      }
      this.filter()
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
        this.requests.forEach((i) => {
          if(i.Status == "Approved" || i.Status == "Pending Changes" || i.Status == "Proposed Changes Denied" || i.Status == "Changes Accepted by User") {
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
                    o.Events.push({ Name: i.Name, Rooms: i.Events[0].Rooms, Full: i })
                  } else {
                    let idx = i.EventDates.indexOf(d)
                    o.Events.push({ Name: i.Name, Rooms: i.Events[idx].Rooms, Full: i })
                  }
                }
              })
            })
          }
        })
        return ordered.filter((o) => {
          return o.Events.length > 0
        })
      },
      earliestDateForResubmission() {
        if(this.copy) {
          let target = moment()
          if(this.copy.needsPub) {
            target = moment(target).add(6, 'weeks')
          } else if (this.copy.needsChildCare) {
            target = moment(target).add(30, 'days')
          } else if(this.copy.needsOnline || this.copy.needsCatering || this.copy.needsReg) {
            target = moment(target).add(14, 'days')
          } else {
            target = null
          }
          return target ? target.format('yyyy-MM-DD') : target
        }
      },
      canResubmit() {
        if(this.selected.Events.length == 1) {
          return this.copy.EventDates && this.copy.EventDates.length > 0
        } else {
          let numActive = 0
          let numValid = 0
          this.resubmissionData.forEach((d, idx) => {
            if(!d.wasRemoved) {
              numActive++
              if(d.date) {
                numValid++
              }
            }
          })
          return (numActive == numValid && numValid > 0)
        }
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
          req.SharedWith = i.SharedWith != '' ? JSON.parse(i.SharedWith) : "";
          temp.push(req);
        });
        this.requests = temp;
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
      editRequest(r) {
        if(r && r.Id > 0) {
          this.selected = r
        }
        let url = $('[id$="hfRequestURL"]').val();
        window.location = url + `?Id=${this.selected.Id}`;
      },
      addComment(r) {
        this.selected = r
        this.commentDialog = true
      },
      saveComment() {
        $('[id$="hfRequestID"]').val(this.selected.Id);
        $('[id$="hfComment"]').val(this.comment);
        $('[id$="btnAddComment"')[0].click();
        $('#updateProgress').show();
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
      cancelRequest(r){
        this.changeStatus('Cancelled', r.Id)
      },
      changeStatus(status, id) {
        let url = $('[id$="hfWorkflowURL"]').val();
        url += '&ItemId=' + id
        url += '&Action=' + status
        window.location = url
      },
      filter() {
        let temp = JSON.parse(JSON.stringify(this.requests))
        if (this.filters.title) {
          temp = temp.filter(r => {
            return r.Name.toLowerCase().includes(this.filters.title.toLowerCase())
          })
        }
        if (this.filters.status && this.filters.status.length > 0) {
          temp = temp.filter(r => {
            return this.filters.status.includes(r.Status)
          })
        }
        if (this.filters.createStart) {
          temp = temp.filter(r => {
            let eod = moment(`${this.filters.createStart} 23:59`, 'yyyy-MM-DD hh:mm')
            return moment(r.CreatedOn).isAfter(eod)
          })
        }
        if (this.filters.createEnd) {
          temp = temp.filter(r => {
            return moment(this.filters.createEnd).isAfter(moment(r.CreatedOn))
          })
        }
        if (this.filters.eventStart) {
          temp = temp.filter(r => {
            let hasMatch = false
            r.EventDates.forEach(d => {
              let eod = moment(`${this.filters.eventStart} 23:59`, 'yyyy-MM-DD hh:mm')
              if (moment(d).isAfter(eod)) {
                hasMatch = true
              }
            })
            return hasMatch
          })
        }
        if (this.filters.eventEnd) {
          temp = temp.filter(r => {
            let hasMatch = false
            r.EventDates.forEach(d => {
              if (moment(this.filters.eventEnd).isAfter(d)) {
                hasMatch = true
              }
            })
            return hasMatch
          })
        }
        this.filteredRequests = temp
      },
      resubmitRequest(r) {
        if(r && r.Id > 0) {
          this.selected = r
        }
        this.copy = JSON.parse(JSON.stringify(this.selected))
        this.resubmissionData = []
        this.copy.EventDates.forEach((d, idx) => {
          this.resubmissionData.push({idx: idx, wasRemoved: false, date: ''})
        })
        this.copy.EventDates = []
        this.resubmitDialog = true
      },
      resubmit() {
        this.resubmissionDialog = false
        $('#updateProgress').show();
        if(this.copy.Events.length > 1) {
          this.copy.EventDates = []
          //Update dates
          for(let i=0; i < this.resubmissionData.length; i++) {
            this.copy.Events[i].EventDate = moment(this.resubmissionData[i].date).format('yyyy-MM-DD')
            if(!this.resubmissionData[i].wasRemoved) {
              this.copy.EventDates.push(moment(this.resubmissionData[i].date).format('yyyy-MM-DD'))
            } else {
              this.copy.Events[i] = null
            }
          }
          //Remove skipped dates
          this.copy.Events = this.copy.Events.filter(e => { return e != null })
        }
        //null dates
        this.copy.PublicityStartDate = null
        this.copy.PublicityEndDate = null
        for(let i = 0; i< this.copy.Events.length; i++) {
          this.copy.Events[i].RegistrationDate = null
          this.copy.Events[i].RegistrationEndDate = null
        }
        $('[id$="hfRequest"]').val(JSON.stringify(this.copy));
        $('[id$="btnResubmitRequest"')[0].click();
      },
      shareRequest() {
        this.selected.SharedWith = this.shareWithInput
        $('[id$="hfRequestID"]').val(this.selected.Id);
        $('[id$="SharedWith"]').val(JSON.stringify(this.selected.SharedWith));
        $('[id$="btnShareRequest"')[0].click();
        $('#updateProgress').show();
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