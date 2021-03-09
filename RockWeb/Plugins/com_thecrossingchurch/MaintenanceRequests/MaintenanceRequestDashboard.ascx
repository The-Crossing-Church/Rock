<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="MaintenanceRequestDashboard.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.MaintenanceRequests.MaintenanceRequestDashboard"
%> <%-- Add Vue and Vuetify CDN --%>
<!-- <script src="https://cdn.jsdelivr.net/npm/vue@2.6.12"></script> -->
<script src="https://cdn.jsdelivr.net/npm/vue@2.6.12/dist/vue.js"></script>
<script src="https://cdn.jsdelivr.net/npm/vuetify@2.x/dist/vuetify.js"></script>
<link
  href="https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900"
  rel="stylesheet"
/>
<link
  href="https://cdn.jsdelivr.net/npm/@mdi/font@4.x/css/materialdesignicons.min.css"
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

<asp:HiddenField ID="hfLocations" runat="server" />
<asp:HiddenField ID="hfRequests" runat="server" />
<asp:HiddenField ID="hfRequestId" runat="server" />
<asp:HiddenField ID="hfNewComment" runat="server" />
<asp:HiddenField ID="hfNewStatus" runat="server" />
<asp:HiddenField ID="hfIsAdmin" runat="server" />
<Rock:BootstrapButton
  runat="server"
  ID="btnAddComment"
  CssClass="btn-hidden"
  OnClick="btnAddComment_Click"
/>
<Rock:BootstrapButton
  runat="server"
  ID="btnChangeStatus"
  CssClass="btn-hidden"
  OnClick="btnChangeStatus_Click"
/>

<div id="app">
  <v-app>
    <div>
      <v-expansion-panels v-model="expansion">
        <v-expansion-panel>
          <v-expansion-panel-header>
            <div class='justify-left'>
              <v-icon>mdi-filter</v-icon> Filters
            </div>
          </v-expansion-panel-header>
          <v-expansion-panel-content>
            <v-row>
              <v-col cols="12" md="4">
                <v-autocomplete
                  label="Location"
                  :items="locations"
                  item-text="Value"
                  item-value="Id"
                  multiple
                  clearable
                  v-model="search.location"
                  attach
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" md="4">
                <v-autocomplete
                  label="Current Status"
                  :items="['Submitted', 'Active', 'Complete']"
                  v-model="search.status"
                  multiple
                  clearable
                  attach
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" md="4">
                <v-checkbox
                  label="Is Safety Issue?"
                  v-model="search.isSafety"
                ></v-checkbox>
              </v-col>
              <v-col cols="12" md="4">
                <v-menu
                  ref="startMenu"
                  v-model="startMenu"
                  :close-on-content-click="false"
                  transition="scale-transition"
                  offset-y
                  min-width="auto"
                  attach
                >
                  <template v-slot:activator="{ on, attrs }">
                    <v-text-field
                      v-model="search.start"
                      label="Requests Created After"
                      prepend-inner-icon="mdi-calendar"
                      readonly
                      v-bind="attrs"
                      v-on="on"
                      clearable
                    ></v-text-field>
                  </template>
                  <v-date-picker
                    v-model="search.start"
                    @input="startMenu = false"
                  >
                  </v-date-picker>
                </v-menu>
              </v-col>
              <v-col cols="12" md="4">
                <v-menu
                  ref="endMenu"
                  v-model="endMenu"
                  :close-on-content-click="false"
                  transition="scale-transition"
                  offset-y
                  min-width="auto"
                  attach
                >
                  <template v-slot:activator="{ on, attrs }">
                    <v-text-field
                      v-model="search.end"
                      label="Requests Created Before"
                      prepend-inner-icon="mdi-calendar"
                      readonly
                      v-bind="attrs"
                      v-on="on"
                      clearable
                    ></v-text-field>
                  </template>
                  <v-date-picker
                    v-model="search.end"
                    @input="endMenu = false"
                  >
                  </v-date-picker>
                </v-menu>
              </v-col>
              <v-col cols="12" md="4">
                <v-autocomplete
                  label="Sort By"
                  v-model="search.sort"
                  :items="sortOps"
                  attach
                ></v-autocomplete>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-textarea
                  label="Description or Comments Contains"
                  v-model="search.text"
                ></v-textarea>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-btn style='float:right;' color="primary" @click="filter">Filter</v-btn>
                <v-btn style='float:right; margin-right:8px;' color="secondary" @click="search = {}; filter();">Clear Filters</v-btn>
              </v-col>
            </v-row>
          </v-expansion-panel-content>
        </v-expansion-panel>
      </v-expansion-panels>
      <br/>
      <v-card class="d-none d-md-block">
        <v-card-text>
          <v-list>
            <v-list-item>
              <v-row>
                <v-col cols="3"><strong>Title</strong></v-col>
                <v-col cols="2"><strong>Submitted By</strong></v-col>
                <v-col cols="2"><strong>Submitted On</strong></v-col>
                <v-col cols="1"><strong>Status</strong></v-col>
                <v-col cols="4"><strong>Actions</strong></v-col>
              </v-row>
            </v-list-item>
            <v-list-item
              v-for="i in filtered"
              :key="i.Id"
            >
              <v-row align="center">
                <v-col cols="3" @click="openRequest(i)" class='hover'>{{i.Title}}</v-col>
                <v-col cols="2">{{i.CreatedBy.FullName}}</v-col>
                <v-col cols="2">{{i.CreatedOn | formatDateTime}}</v-col>
                <v-col cols="1">{{i.RequestStatus}}</v-col>
                <v-col cols="4" class="d-none d-lg-flex">
                  <v-btn color="primary" v-if="i.RequestStatus == 'Submitted' && isAdmin" @click="changeStatus(`${nextStatus(i.RequestStatus)}`, i.Id)">
                    Mark {{nextStatus(i.RequestStatus)}}
                  </v-btn>
                  <v-btn color="primary" v-if="i.RequestStatus == 'Active' && isAdmin" @click="status = 'Complete'; selectStatus(i, true)">Mark Complete</v-btn>
                  <v-spacer></v-spacer>
                  <v-btn color="secondary" @click="addComment(i)">Add Comment</v-btn>
                </v-col>
                <v-col cols="4" class="d-flex d-lg-none">
                  <v-btn icon @click="addComment(i)">
                    <v-icon color="#0C4557">mdi-comment-edit</v-icon>
                  </v-btn>
                  <v-btn icon v-if="isAdmin" @click="selectStatus(i, false)">
                    <v-icon :class="getStatusPillClass(i.RequestStatus)">mdi-progress-wrench</v-icon>
                  </v-btn>
                </v-col>
              </v-row>
            </v-list-item>
          </v-list>
        </v-card-text>
      </v-card>
      <v-list class="d-block d-md-none" style="background-color: rgba(225,225,225,0);">
        <v-list-item
          v-for="i in filtered"
          :key="i.Id"
          style="padding: 4px 0px;"
        >
          <v-card style="width: 100%;">
            <v-card-text>
              <strong @click="openRequest(i)">{{i.Title}}</strong><br />
              {{i.CreatedBy.FullName}} - {{i.CreatedOn | formatDateTime}}<br />
              <span :class="getStatusPillClass(i.RequestStatus)">{{i.RequestStatus}}</span>
            </v-card-text>
            <v-card-actions>
              <v-btn icon @click="openRequest(i)">
                <v-icon color="#0C4557">mdi-format-list-text</v-icon>
              </v-btn>
              <v-btn icon @click="addComment(i)">
                <v-icon color="#0C4557">mdi-comment-edit</v-icon>
              </v-btn>
              <v-btn icon v-if="isAdmin" @click="selectStatus(i, false)">
                <v-icon :class="getStatusPillClass(i.RequestStatus)">mdi-progress-wrench</v-icon>
              </v-btn>
            </v-card-actions>
          </v-card>
        </v-list-item>
      </v-list>
      <v-dialog v-if="dialog" v-model="dialog" max-width="80%">
        <v-card>
          <v-card-title> 
            <v-btn icon absolute top right @click="dialog = false; selected = {};">
              <v-icon>mdi-close</v-icon>
            </v-btn>
            <br class='d-block d-md-none' />
            {{selected.Title}} 
          </v-card-title>
          <v-card-text>
            <v-row>
              <v-col>
                <div class="floating-title">Description</div>
                {{selected.Description}}
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="6">
                <div class="floating-title">Location</div>
                {{formatLocation(selected.Location)}}
              </v-col>
              <v-col cols="12" md="6" v-if="selected.RequestedCompletionDate">
                <div class="floating-title">Requested Completion Date</div>
                {{selected.RequestedCompletionDate | formatDate}}
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="6">
                <div class="floating-title">Is Saftey Issue</div>
                {{selected.SafetyIssue}}
              </v-col>
              <v-col cols="12" md="6">
                <div class="floating-title">Status</div>
                {{selected.RequestStatus}}
              </v-col>
            </v-row>
            <v-row v-if="selected.Image">
              <v-col>
                <v-img :src="selected.Image.data"></v-img>
              </v-col>
            </v-row>
            <v-row v-if="selected.Comments">
              <v-col>
                <h4>Comments</h4>
              </v-col>
            </v-row>
            <v-row v-if="selected.Comments">
              <v-col>
                <div class='comment-viewer'>
                  <div v-for="(c,idx) in selected.Comments" :key="idx" class='comment'>
                    <strong>{{c.CreatedBy}}</strong> - {{c.CreatedOn | formatDateTime}}<br/>
                    {{c.Message}}
                  </div>
                </div>
              </v-col>
            </v-row>
          </v-card-text>
        </v-card>
      </v-dialog>
      <v-dialog v-if="alert" v-model="alert" max-width="80%">
        <v-card>
          <v-card-title></v-card-title>
          <v-card-text>
            <v-alert type="success" v-if="isSuccess">
              Your request has been submitted. You can view the status of your
              request here.
            </v-alert>
            <v-alert type="success" v-if="isCommentAdded">
              Your comment has been added.
            </v-alert>
            <v-alert type="success" v-if="isStatusUpdated">
              This request has been updated.
            </v-alert>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="closeAlert">Close</v-btn>
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
      <v-dialog v-if="statusDialog" v-model="statusDialog" max-width="80%">
        <v-card>
          <v-card-title></v-card-title>
          <v-card-text>
            <v-select
              :items="['Submitted', 'Active', 'Complete']"
              v-model="status"
            ></v-select>
            <v-textarea
              label="Closing Comment"
              v-model="comment"
              v-if="status == 'Complete'"
            ></v-textarea>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="changeStatus(status, selected.Id)">Update</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </v-app>
</div>
<script>
    document.addEventListener("DOMContentLoaded", function () {
        new Vue({
            el: "#app",
            vuetify: new Vuetify({
                theme: {
                    themes: {
                        light: {
                            primary: "#347689",
                            secondary: "#3D3D3D",
                            accent: "#8ED2C9",
                        },
                    },
                },
                iconfont: "mdi",
            }),
            config: {
                devtools: true,
            },
            data: {
                isAdmin: false,
                requests: [],
                filtered: [],
                locations: [],
                selected: {},
                dialog: false,
                alert: false,
                commentDialog: false,
                comment: "",
                isSuccess: false,
                isCommentAdded: false,
                isStatusUpdated: false,
                statusDialog: false,
                status: '',
                expansion: [],
                search: {
                    status: ['Submitted', 'Active'],
                    sort: 'Created On'
                },
                sortOps: ['Created By', 'Created On', 'Location', 'Requested Completion Date', 'Request Status', 'Title'],
                startMenu: false,
                endMenu: false
            },
            created() {
                this.locations = JSON.parse($('[id$="hfLocations"]')[0].value);
                this.requests = JSON.parse($('[id$="hfRequests"]')[0].value);
                this.filter()
                let isadmin = $('[id$="hfIsAdmin"]')[0].value
                if (isadmin == "True") {
                    this.isAdmin = true
                }
                window["moment-range"].extendMoment(moment);
            },
            mounted() {
                let query = new URLSearchParams(window.location.search);
                let success = query.get("ShowSuccess");
                let comment = query.get("CommentAdded");
                let status = query.get("StatusChanged");
                if (success == "true") {
                    this.isSuccess = true;
                    this.alert = true;
                }
                if (comment == "true") {
                    this.isCommentAdded = true;
                    this.alert = true;
                }
                if (status == 'true') {
                    this.isStatusUpdated = true;
                    this.alert = true
                }
            },
            computed: {},
            methods: {
                formatLocation(id) {
                    return this.locations.filter((l) => {
                        return l.Id == id;
                    })[0].Value;
                },
                getClass(idx) {
                    if (idx < this.requests.length - 1) {
                        return "list-with-border";
                    }
                    return "";
                },
                getStatusPillClass(status) {
                    if (status == "Active") {
                        return "status-active";
                    }
                    if (status == "Submitted") {
                        return "status-submitted";
                    }
                    if (status == "Complete") {
                        return "status-complete";
                    }
                },
                nextStatus(status) {
                    if (status == "Submitted") {
                        return "Active";
                    } else if (status == "Active") {
                        return "Complete";
                    }
                },
                openRequest(req) {
                    this.selected = JSON.parse(JSON.stringify(req));
                    if (req.Image) {
                        this.selected.Image = JSON.parse(req.Image);
                    }
                    if (req.Comments) {
                        this.selected.Comments = JSON.parse(req.Comments)
                    }
                    this.dialog = true;
                },
                closeAlert() {
                    this.alert = false;
                    this.isSuccess = false;
                },
                addComment(req) {
                    this.comment = "";
                    this.selected = JSON.parse(JSON.stringify(req));
                    this.commentDialog = true;
                },
                saveComment() {
                    $('[id$="hfRequestId"]').val(this.selected.Id.toString());
                    $('[id$="hfNewComment"]').val(this.comment);
                    $('[id$="btnAddComment"')[0].click();
                },
                selectStatus(req, isComplete) {
                    this.selected = JSON.parse(JSON.stringify(req));
                    if (!isComplete) {
                        this.status = req.RequestStatus
                    }
                    this.statusDialog = true;
                },
                changeStatus(status, id) {
                    $('[id$="hfRequestId"]').val(id);
                    $('[id$="hfNewStatus"]').val(status);
                    $('[id$="hfNewComment"]').val(this.comment);
                    $('[id$="btnChangeStatus"')[0].click();
                },
                filter() {
                    let temp = JSON.parse(JSON.stringify(this.requests))
                    if (this.search.location && this.search.location.length > 0) {
                        temp = temp.filter(i => {
                            return this.search.location.includes(parseInt(i.Location))
                        })
                    }
                    if (this.search.status && this.search.status.length > 0) {
                        temp = temp.filter(i => {
                            return this.search.status.includes(i.RequestStatus)
                        })
                    }
                    if (this.search.start) {
                        temp = temp.filter(i => {
                            return moment(i.CreatedOn).isAfter(moment(this.search.start), 'day')
                        })
                    }
                    if (this.search.end) {
                        temp = temp.filter(i => {
                            return moment(i.CreatedOn).isBefore(moment(this.search.end), 'day')
                        })
                    }
                    if (this.search.text) {
                        temp = temp.filter(i => {
                            return i.Description.toLowerCase().includes(this.search.text.toLowerCase()) || i.Comments.toLowerCase().includes(this.search.text.toLowerCase())
                        })
                    }
                    if (this.search.isSafety) {
                        temp = temp.filter(i => {
                            return i.SafetyIssue == "True"
                        })
                    }
                    if (this.search.sort) {
                        switch (this.search.sort) {
                            case 'Created By':
                                temp = temp.sort((a, b) => {
                                    if (a.CreatedBy.FullName < b.CreatedBy.FullName) {
                                        return -1
                                    } else if (a.CreatedBy.FullName > b.CreatedBy.FullName) {
                                        return 1
                                    } else {
                                        return 0
                                    }
                                })
                                break;
                            case 'Title':
                                temp = temp.sort((a, b) => {
                                    if (a.Title < b.Title) {
                                        return -1
                                    } else if (a.Title > b.Title) {
                                        return 1
                                    } else {
                                        return 0
                                    }
                                })
                                break;
                            case 'Location':
                                let self = this
                                temp = temp.sort((a, b) => {
                                    let locA = self.locations.filter(i => { return i.Id.toString() == a.Location })[0].Value
                                    let locB = self.locations.filter(i => { return i.Id.toString() == b.Location })[0].Value
                                    if (locA < locB) {
                                        return -1
                                    } else if (locA > locB) {
                                        return 1
                                    } else {
                                        return 0
                                    }
                                })
                                break;
                            case 'Request Status':
                                let statuses = ['Submitted', 'Active', 'Complete']
                                temp = temp.sort((a, b) => {
                                    if (statuses.indexOf(a.RequestStatus) < statuses.indexOf(b.RequestStatus)) {
                                        return -1
                                    } else if (statuses.indexOf(a.RequestStatus) > statuses.indexOf(b.RequestStatus)) {
                                        return 1
                                    } else {
                                        return 0
                                    }
                                })
                                break;
                            case 'Requested Completion Date':
                                temp = temp.sort((a, b) => {
                                    if (moment(a.RequestedCompletionDate).isAfter(moment(b.RequestedCompletionDate), 'minute')) {
                                        return -1
                                    } else if (moment(a.RequestedCompletionDate).isBefore(moment(b.RequestedCompletionDate), 'minute')) {
                                        return 1
                                    } else {
                                        return 0
                                    }
                                })
                                break;
                            default:
                                temp = temp.sort((a, b) => {
                                    if (moment(a.CreatedOn).isAfter(moment(b.CreatedOn), 'minute')) {
                                        return -1
                                    } else if (moment(a.CreatedOn).isBefore(moment(b.CreatedOn), 'minute')) {
                                        return 1
                                    } else {
                                        return 0
                                    }
                                })
                                break;
                        }
                    }
                    this.filtered = temp
                    this.expansion = []
                    window.scrollTo(0, 0);
                }
            },
            filters: {
                formatDateTime(val) {
                    return moment(val).format("MM/DD/yyyy hh:mm A");
                },
                formatDate(val) {
                    return moment(val).format("MM/DD/yyyy");
                },
            },
            watch: {},
        });
    });
</script>
<style>
  .theme--light.v-application {
    background: rgba(0, 0, 0, 0);
  }
  .col {
    padding: 4px 12px !important;
  }
  .hover {
    cursor: pointer;
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
  .tooltip {
    opacity: 0;
    color: #ffffff;
    background-color: #545454;
    border-radius: 6px;
    transition: 0.16s ease-in;
    padding: 8px;
    width: 125px;
    text-align: center;
    font-size: 14px;
    position: absolute;
    top: -250%;
  }
  .btn-pub-add:hover .tooltip,
  .btn-pub-del:hover .tooltip {
    opacity: 1;
  }
  .accent-text {
    color: #8ed2c9;
  }
  .status-active {
    color: #347689 !important;
  }
  .status-submitted {
    color: #8ed2c9 !important;
  }
  .status-compelte {
    color: #9e9e9e !important;
  }
  .comment {
    background-color: lightgrey;
    padding: 8px;
    border-radius: 6px;
    margin: 4px 0px;
  }
  .floating-title {
    text-transform: uppercase;
  }
  .v-dialog {
    max-height: 75vh !important;
  }
</style>
