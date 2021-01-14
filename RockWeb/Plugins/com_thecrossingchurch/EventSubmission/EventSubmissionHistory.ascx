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

<asp:HiddenField ID="hfRooms" runat="server" />
<asp:HiddenField ID="hfRequests" runat="server" />

<div id="app">
  <v-app>
    <div>
      <v-row>
        <v-col>
          <v-card>
            <v-card-text>
              <v-list>
                <v-list-item class="list-with-border">
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
                        :items="['Submitted', 'Approved', 'Denied', 'Cancelled']"
                        v-model="filters.status"
                        multiple
                        attach
                        clearable
                      ></v-autocomplete>
                    </v-col>
                    <v-col>
                      <v-autocomplete
                        label="Resources"
                        :items="['Room', 'Online', 'Publicity', 'Childcare', 'Catering', 'Extra Resources']"
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
                </v-list-item>
                <v-list-item class="list-with-border">
                  <v-row>
                    <v-col><strong>Request</strong></v-col>
                    <v-col><strong>Submitted By</strong></v-col>
                    <v-col><strong>Submitted On</strong></v-col>
                    <v-col><strong>Event Dates</strong></v-col>
                    <v-col><strong>Requested Resources</strong></v-col>
                    <v-col><strong>Status</strong></v-col>
                  </v-row>
                </v-list-item>
                <v-list-item
                  v-for="(r, idx) in requests"
                  :key="r.Id"
                  :class="getClass(idx)"
                >
                  <v-row align="center">
                    <v-col @click="selected = r; overlay = true;"
                      ><div class="hover">{{ r.Name }}</div></v-col
                    >
                    <v-col>{{ r.CreatedBy }}</v-col>
                    <v-col>{{ r.CreatedOn | formatDateTime }}</v-col>
                    <v-col>{{ formatDates(r.EventDates) }}</v-col>
                    <v-col>{{ requestType(r) }}</v-col>
                    <v-col :class="getStatusPillClass(r.RequestStatus)"
                      >{{ r.RequestStatus }}</v-col
                    >
                  </v-row>
                </v-list-item>
              </v-list>
            </v-card-text>
            <v-card-actions>
              <v-row align="center">
                <v-col cols="10"></v-col>
                <v-col cols="1">
                  <v-select
                    label="Rows"
                    v-model="rows"
                    :items="[5,10,15,30]"
                    attach
                  ></v-select>
                </v-col>
                <v-col cols="1">
                  <v-btn icon @click='paginate("prev")'>
                    <v-icon>mdi-chevron-left</v-icon>
                  </v-btn>
                  {{(page + 1)}}
                  <v-btn icon @click='paginate("next")'>
                    <v-icon>mdi-chevron-right</v-icon>
                  </v-btn>
                </v-col>
              </v-row>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>
      <v-overlay :value="overlay">
        <v-card
          light
          width="100%"
          style="max-height: 75vh; overflow-y: scroll; margin-top: 100px"
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
                {{selected.Ministry}}
              </v-col>
              <v-col>
                <div class="floating-title">Contact</div>
                {{selected.Contact}}
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <div class="floating-title">Requested Resources</div>
                {{requestType(selected)}}
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <div class="floating-title">Request Dates</div>
                {{formatDates(selected.EventDates)}}
              </v-col>
            </v-row>
            <v-row v-if="selected.StartTime || selected.EndTime">
              <v-col v-if="selected.StartTime">
                <div class="floating-title">Start Time</div>
                {{selected.StartTime}}
              </v-col>
              <v-col v-if="selected.EndTime">
                <div class="floating-title">End Time</div>
                {{selected.EndTime}}
              </v-col>
            </v-row>
            <template v-if="selected.needsSpace">
              <v-row>
                <v-col>
                  <div class="floating-title">Expected Number of Attendees</div>
                  {{selected.ExpectedAttendance}}
                </v-col>
                <v-col>
                  <div class="floating-title">Desired Rooms/Spaces</div>
                  {{formatRooms(selected.Rooms)}}
                </v-col>
              </v-row>
            </template>
            <template v-if="selected.needsOnline">
              <v-row>
                <v-col>
                  <div class="floating-title">Event Link</div>
                  {{selected.EventURL}}
                </v-col>
                <v-col v-if="selected.ZoomPassword != ''">
                  <div class="floating-title">Password</div>
                  {{selected.ZoomPassword}}
                </v-col>
              </v-row>
            </template>
            <template v-if="selected.needsPub">
              <v-row v-for="(p, idx) in selected.Publicity" :key="`pub_${idx}`">
                <v-col>
                  <div class="floating-title">Publicity Date</div>
                  {{p.Date | formatDate}}
                </v-col>
                <v-col>
                  <div class="floating-title">Publicity Need</div>
                  {{p.Needs.join(', ')}}
                </v-col>
              </v-row>
              <v-row v-if="selected.PublicityBlurb">
                <v-col>
                  <div class="floating-title">Publicity Blurb</div>
                  {{selected.PublicityBlurb}}
                </v-col>
              </v-row>
              <v-row>
                <v-col v-if="selected.PubImage">
                  <div class="floating-title">Publicity Image</div>
                  {{selected.PubImage.name}}
                  <v-btn icon color="accent" @click="saveFile"
                    ><v-icon color="accent">mdi-download</v-icon></v-btn
                  >
                </v-col>
                <v-col>
                  <div class="floating-title">Add to public calendar</div>
                  {{boolToYesNo(selected.ShowOnCalendar)}}
                </v-col>
              </v-row>
            </template>
            <template v-if="selected.needsChildCare">
              <v-row>
                <v-col>
                  <div class="floating-title">Childcare Ages Groups</div>
                  {{selected.ChildCareOptions.join(', ')}}
                </v-col>
                <v-col>
                  <div class="floating-title">Expected Number of Children</div>
                  {{selected.EstimatedKids}}
                </v-col>
              </v-row>
            </template>
            <template v-if="selected.needsCatering">
              <v-row>
                <v-col>
                  <div class="floating-title">Preferred Vendor</div>
                  {{selected.Vendor}}
                </v-col>
                <v-col>
                  <div class="floating-title">Budget Line</div>
                  {{selected.BudgetLine}}
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Preferred Menu</div>
                  {{selected.Menu}}
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">{{foodTimeTitle}}</div>
                  {{selected.FoodTime}}
                </v-col>
                <v-col v-if="selected.FoodDelivery">
                  <div class="floating-title">Food Drop off Location</div>
                  {{selected.FoodDropOff}}
                </v-col>
              </v-row>
              <template v-if="selected.needsChildCare">
                <v-row>
                  <v-col>
                    <div class="floating-title">
                      Preferred Vendor for Childcare
                    </div>
                    {{selected.CCVendor}}
                  </v-col>
                  <v-col>
                    <div class="floating-title">Budget Line for Childcare</div>
                    {{selected.CCBudgetLine}}
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <div class="floating-title">
                      Preferred Menu for Childcare
                    </div>
                    {{selected.CCMenu}}
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <div class="floating-title">ChildCare Food Set-up time</div>
                    {{selected.CCFoodTime}}
                  </v-col>
                </v-row>
              </template>
            </template>
            <template v-if="selected.needsAccom">
              <v-row v-if="selected.Drinks">
                <v-col>
                  <div class="floating-title">Desired Drinks</div>
                  {{selected.Drinks.join(', ')}}
                </v-col>
              </v-row>
              <v-row v-if="selected.TechNeeds">
                <v-col>
                  <div class="floating-title">Tech Needs</div>
                  {{selected.TechNeeds.join(', ')}}
                </v-col>
              </v-row>
              <v-row v-if="selected.RegistrationDate">
                <v-col>
                  <div class="floating-title">Registration Date</div>
                  {{selected.RegistrationDate | formatDate}}
                </v-col>
                <v-col v-if="selected.Fee">
                  <div class="floating-title">Registration Fee</div>
                  {{selected.Fee | formatCurrency}}
                </v-col>
              </v-row>
            </template>
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="overlay = false; selected = {}">
              <v-icon>mdi-close</v-icon> Close
            </v-btn>
          </v-card-actions>
        </v-card>
      </v-overlay>
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
                requests: [],
                allrequests: [],
                selected: {},
                overlay: false,
                rooms: [],
                page: 0,
                rows: 15,
                filters: {
                    query: "",
                    submitter: "",
                    status: [],
                },
            },
            created() {
                this.getRequests();
                this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
            },
            filters: {
                formatDateTime(val) {
                    return moment(val).format("MM/DD/yyyy hh:mm A");
                },
                formatDate(val) {
                    return moment(val).format("MM/DD/yyyy");
                },
                formatCurrency(val) {
                    var formatter = new Intl.NumberFormat("en-US", {
                        style: "currency",
                        currency: "USD",
                    });
                    return formatter.format(val);
                },
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
                getRequests() {
                    let raw = JSON.parse($('[id$="hfRequests"]').val());
                    let temp = [];
                    raw.forEach((i) => {
                        let req = JSON.parse(i.Value);
                        req.Id = i.Id;
                        req.CreatedBy = i.CreatedBy;
                        req.CreatedOn = i.CreatedOn;
                        req.RequestStatus = i.RequestStatus;
                        temp.push(req);
                    });
                    this.allrequests = temp;
                    this.requests = temp.slice(0, this.rows);
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
                    this.requests = temp.slice(
                        this.page * this.rows,
                        this.page * this.rows + this.rows
                    );
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
                boolToYesNo(val) {
                    if (val) {
                        return "Yes";
                    }
                    return "No";
                },
                formatDates(val) {
                    if (val) {
                        let dates = [];
                        val.forEach((i) => {
                            dates.push(moment(i).format("MM/DD/yyyy"));
                        });
                        return dates.join(", ");
                    }
                    return "";
                },
                formatRooms(val) {
                    if (val) {
                        let rms = [];
                        val.forEach((i) => {
                            this.rooms.forEach((r) => {
                                if (i == r.Id) {
                                    rms.push(r.Value.split(` (`)[0]);
                                }
                            });
                        });
                        return rms.join(", ");
                    }
                    return "";
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
                    if (status == "Submitted") {
                        return "no-top-pad status-pill submitted";
                    }
                    if (status == "Cancelled") {
                        return "no-top-pad status-pill cancelled";
                    }
                    if (status == "Denied") {
                        return "no-top-pad status-pill denied";
                    }
                },
                requestType(itm) {
                    if (itm) {
                        let resources = [];
                        if (itm.needsSpace) {
                            resources.push("Room");
                        }
                        if (itm.needsOnline) {
                            resources.push("Online");
                        }
                        if (itm.needsPub) {
                            resources.push("Publicity");
                        }
                        if (itm.needsChildCare) {
                            resources.push("Childcare");
                        }
                        if (itm.needsCatering) {
                            resources.push("Catering");
                        }
                        if (itm.needsAccom) {
                            resources.push("Extra Resources");
                        }
                        return resources.join(", ");
                    }
                    return "";
                },
                saveFile() {
                    var a = document.createElement("a");
                    a.style = "display: none";
                    document.body.appendChild(a);
                    a.href = this.selected.PubImage.data;
                    a.download = this.selected.PubImage.name;
                    a.click();
                },
            },
            watch: {
                page(val) {
                    this.filter();
                },
                rows(val) {
                    this.filter();
                },
            },
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
</style>
