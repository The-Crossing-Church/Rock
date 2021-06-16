<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="EventCalendar.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.EventSubmission.EventCalendar"
%> <%-- Add Vue and Vuetify CDN --%>
<script src="https://cdn.jsdelivr.net/npm/vue@2.6.12"></script>
<!-- <script src="https://cdn.jsdelivr.net/npm/vue@2.6.12/dist/vue.js"></script> -->
<script src="https://cdn.jsdelivr.net/npm/vuetify@2.4.2/dist/vuetify.js"></script>
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
<asp:HiddenField ID="hfMinistries" runat="server" />
<asp:HiddenField ID="hfRequests" runat="server" />
<asp:HiddenField ID="hfFocusDate" runat="server" />
<Rock:BootstrapButton
  ID="btnSwitchFocus"
  CssClass="btn-hidden"
  runat="server"
  OnClick="btnSwitchFocus_Click"
/>

<div id="app">
  <v-app>
    <v-main>
      <v-card>
        <v-card-text>
          <v-expansion-panels v-model="panels" flat>
            <v-expansion-panel>
              <v-expansion-panel-header><h5><i class='fa fa-filter'></i> Filter Events</h5></v-expansion-panel-header>
              <v-expansion-panel-content>
                <v-row>
                  <v-col cols="12" lg="3">
                    <v-autocomplete
                      label="Ministry"
                      :items="ministries"
                      item-text="Value"
                      item-value="Id"
                      v-model="filters.Ministry"
                      multiple
                      clearable
                      attach
                    ></v-autocomplete>
                  </v-col>
                  <v-col cols="12" lg="3">
                    <v-autocomplete
                      label="General Location"
                      :items="roomTypes"
                      v-model="filters.RoomType"
                      multiple
                      clearable
                      attach
                    ></v-autocomplete>
                  </v-col>
                  <v-col cols="12" lg="3">
                    <v-autocomplete
                      label="Room"
                      :items="rooms"
                      item-text="Value"
                      item-value="Id"
                      v-model="filters.Room"
                      multiple
                      clearable
                      attach
                    ></v-autocomplete>
                  </v-col>
                  <v-col cols="12" lg="3" style="align-items: center; display: flex; justify-content: flex-end;">
                    <v-btn right color="primary" @click="filter">Filter</v-btn>
                  </v-col>
                </v-row>
              </v-expansion-panel-content>
            </v-expansion-panel>
          </v-expansion-panels>
          <br/>
          <v-row>
            <v-col style="display:flex; align-items:center;">
              <v-btn icon @click="changeDate(-1)">
                <v-icon>mdi-chevron-left</v-icon>
              </v-btn>
            </v-col>
            <v-col cols="6"><h3 style='text-align: center'>{{currentMonth}}</h3></v-col>
            <v-col style="display:flex; align-items:center; justify-content: flex-end;">
              <v-btn icon @click="changeDate(1)">
                <v-icon>mdi-chevron-right</v-icon>
              </v-btn>
            </v-col>
          </v-row>
          <v-sheet height="500">
            <v-calendar
              ref="calendar"
              v-model="value"
              :events="requests"
              @click:event="showEvent"
              @click:more="showDayEvents"
            ></v-calendar>
          </v-sheet>
        </v-card-text>
      </v-card>
      <br/>
      <div class="d-block d-lg-none">
        <template v-if="focusedEvents.length > 0">
          <v-carousel cycle :show-arrows="false" hide-delimiter-background light height="fit-content">
            <v-carousel-item v-for="(e, idx) in focusedEvents" :key="idx">
              <v-toolbar height="auto" color="accent">
                <h4 style="font-weight: bold;">{{e.name}} {{e.date}}</h4>
              </v-toolbar>
              <v-row>
                <v-col>
                  <strong>Ministry:</strong> {{formatMinistry(e.ministry)}} <br/>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <strong>Contact:</strong> {{e.contact}} <br/>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <strong>Start:</strong> {{e.starttime}} <br/>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <strong>End:</strong> {{e.endtime}} <br/>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <strong>Rooms:</strong> {{formatRooms(e.rooms)}} <br/>
                </v-col>
              </v-row>
              <br/><br/>
            </v-carousel-item>
          </v-carousel>
        </template>
        <template v-else>
          There are no events matching your filters for this day.
        </template>
      </div>
      <v-dialog
        v-if="dialog"
        v-model="dialog"
        max-width="850px"
      >
        <v-card>
          <template v-if="selected">
            <v-toolbar color="accent">
              <h4 style="font-weight: bold;">{{selected.name}} {{selected.date}}</h4>
            </v-toolbar>
            <v-card-text>
              <br/>
              <v-row>
                <v-col>
                  <strong>Ministry:</strong> {{formatMinistry(selected.ministry)}} <br/>
                </v-col>
                <v-col>
                  <strong>Contact:</strong> {{selected.contact}} <br/>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <strong>Start:</strong> {{selected.starttime}} <br/>
                </v-col>
                <v-col>
                  <strong>End:</strong> {{selected.endtime}} <br/>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <strong>Rooms:</strong> {{formatRooms(selected.rooms)}} <br/>
                </v-col>
              </v-row>
            </v-card-text>
          </template>
          <template v-else>
            <v-card-text>
              <br/>
              <v-expansion-panels>
                <v-expansion-panel v-for="(e, idx) in focusedEvents" :key="idx">
                  <v-expansion-panel-header color="accent">
                    <h4 style="font-weight: bold;">{{e.name}} {{e.date}}</h4>
                  </v-expansion-panel-header>
                  <v-expansion-panel-content>
                    <v-row>
                      <v-col>
                        <strong>Ministry:</strong> {{formatMinistry(e.ministry)}} <br/>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col>
                        <strong>Contact:</strong> {{e.contact}} <br/>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col>
                        <strong>Start:</strong> {{e.starttime}} <br/>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col>
                        <strong>End:</strong> {{e.endtime}} <br/>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col>
                        <strong>Rooms:</strong> {{formatRooms(e.rooms)}} <br/>
                      </v-col>
                    </v-row>
                  </v-expansion-panel-content>
                </v-expansion-panel>
              </v-expansion-panels>
            </v-card-text>
          </template>
        </v-card>
      </v-dialog>
    </v-main>
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
                dialog: false,
                rooms: [],
                ministries: [],
                panels: [],
                filters: {
                    RoomType: [],
                    Room: [],
                    Ministry: []
                },
                value: '',
                colors: [
                    '#818F95',
                    '#347689',
                    '#8ED2C9',
                    '#99B6C4',
                    '#A499BE',
                    '#D28E8F',
                ],
            },
            created() {
                this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
                this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value);
                this.getRequests();
                this.value = $('[id$="hfFocusDate"]')[0].value;
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
                roomTypes() {
                    let temp = this.rooms.map(r => r.Type)
                    return ["None", ...new Set(temp)]
                },
                focusedEvents() {
                    if (this.value) {
                        return this.requests.filter(e => {
                            return e.date == moment(this.value).format("MM/DD/YYYY")
                        })
                    }
                    return []
                },
                currentMonth() {
                    if (this.value) {
                        return moment(this.value).format("MMMM")
                    } else {
                        return moment().format("MMMM")
                    }
                }
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
                    this.filter();
                },
                formatRooms(val) {
                    if (val) {
                        let rms = [];
                        val.forEach((i) => {
                            this.rooms.forEach((r) => {
                                if (i == r.Id) {
                                    rms.push(r.Value);
                                }
                            });
                        });
                        return rms.join(", ");
                    }
                    return "";
                },
                formatMinistry(val) {
                    if (val) {
                        let formattedVal = this.ministries.filter((m) => {
                            return m.Id == val;
                        });
                        return formattedVal[0].Value;
                    }
                    return "";
                },
                filter() {
                    let temp = JSON.parse(JSON.stringify(this.allrequests))
                    let indv = []
                    if (this.filters.Ministry && this.filters.Ministry.length > 0) {
                        temp = temp.filter(e => {
                            return this.filters.Ministry.includes(e.ministry)
                        })
                    }
                    temp.forEach(e => {
                        if (e.IsSame || e.Events.length == 1) {
                            e.EventDates.forEach(d => {
                                let color = ''
                                let arr = this.rooms.filter(r => { return r.Id == e.Events[0].Rooms[0] })
                                let type = "None"
                                if (arr.length > 0) {
                                    type = arr[0].Type
                                }
                                color = this.colors[this.roomTypes.indexOf(type)]
                                let st = moment(`${d} ${e.Events[0].StartTime}`, 'YYYY-MM-DD hh:mm a')
                                if (e.Events[0].MinsStartBuffer) {
                                    st = st.subtract(e.Events[0].MinsStartBuffer, 'minute')
                                }
                                let et = moment(`${d} ${e.Events[0].EndTime}`, 'YYYY-MM-DD hh:mm a')
                                if (e.Events[0].MinsEndBuffer) {
                                    et = et.subtract(e.Events[0].MinsEndBuffer, 'minute')
                                }
                                let obj = {
                                    name: this.formatRooms(e.Events[0].Rooms) + ' - ' + e.Name,
                                    start: moment(st).format('YYYY-MM-DDTHH:mm'),
                                    end: moment(et).format('YYYY-MM-DDTHH:mm'),
                                    ministry: e.Ministry,
                                    contact: e.Contact,
                                    rooms: e.Events[0].Rooms,
                                    starttime: moment(st).format('hh:mm A'),
                                    endtime: moment(et).format('hh:mm A'),
                                    date: moment(d).format('MM/DD/YYYY'),
                                    color: color
                                }
                                indv.push(obj)
                            })
                        } else {
                            e.Events.forEach(d => {
                                let color = ''
                                let arr = this.rooms.filter(r => { return r.Id == d.Rooms[0] })
                                let type = "None"
                                if (arr.length > 0) {
                                    type = arr[0].Type
                                }
                                color = this.colors[this.roomTypes.indexOf(type)]
                                let st = moment(`${d.EventDate} ${d.StartTime}`, 'YYYY-MM-DD hh:mm a')
                                if (d.MinsStartBuffer) {
                                    st = st.subtract(d.MinsStartBuffer, 'minute')
                                }
                                let et = moment(`${d.EventDate} ${d.EndTime}`, 'YYYY-MM-DD hh:mm a')
                                if (d.MinsEndBuffer) {
                                    et = et.subtract(d.MinsEndBuffer, 'minute')
                                }
                                let obj = {
                                    name: this.formatRooms(d.Rooms) + ' - ' + e.Name,
                                    start: moment(st).format('YYYY-MM-DDTHH:mm'),
                                    end: moment(et).format('YYYY-MM-DDTHH:mm'),
                                    ministry: e.Ministry,
                                    contact: e.Contact,
                                    rooms: d.Rooms,
                                    starttime: moment(st).format('hh:mm A'),
                                    endtime: moment(et).format('hh:mm A'),
                                    date: moment(d.EventDate).format('MM/DD/YYYY'),
                                    color: color
                                }
                                indv.push(obj)
                            })
                        }
                    })
                    if (this.filters.Room && this.filters.Room.length > 0) {
                        indv = indv.filter(e => {
                            let intersection = e.rooms.filter(value => this.filters.Room.includes(value))
                            return intersection.length > 0
                        })
                    } else {
                        if (this.filters.RoomType && this.filters.RoomType.length > 0) {
                            indv = indv.filter(e => {
                                let types = ["None"]
                                if (e.rooms.length > 0) {
                                    types = this.rooms.filter(value => e.rooms.includes(value.Id)).map(value => value.Type)
                                }
                                let intersection = types.filter(value => this.filters.RoomType.includes(value))
                                return intersection.length > 0
                            })
                        }
                    }
                    this.requests = indv
                },
                showEvent({ nativeEvent, event }) {
                    this.selected = event
                    // this.selectedElement = nativeEvent.target
                    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
                        // true for mobile device
                        // don't show the dialog
                    } else {
                        // false for not mobile device
                        // show the dialog
                        this.dialog = true
                    }
                    this.value = moment(event.date).format('YYYY-MM-DD')
                },
                showDayEvents(event) {
                    this.selected = null
                    this.value = event.date
                    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
                        // true for mobile device
                        // don't show the dialog
                    } else {
                        // false for not mobile device
                        // show the dialog
                        this.dialog = true
                    }
                },
                changeDate(changeVal) {
                    let callChange = false
                    if (!this.value) {
                        this.value = new Date()
                        callChange = true
                    }
                    if (changeVal > 0) {
                        this.value = moment(this.value).add(1, 'month')
                    } else {
                        this.value = moment(this.value).subtract(1, 'month')
                    }
                    if (callChange) {
                        $('[id$="hfFocusDate"]').val(moment(this.value).format('YYYY-MM-01'));
                        $('[id$="btnSwitchFocus"')[0].click();
                        $('#updateProgress').show();
                    }
                }
            },
            watch: {
                value(newValue, oldValue) {
                    if (oldValue) {
                        if (moment(newValue).format('MM') != moment(oldValue).format('MM')) {
                            $('[id$="hfFocusDate"]').val(moment(newValue).format('YYYY-MM-01'));
                            $('[id$="btnSwitchFocus"')[0].click();
                            $('#updateProgress').show();
                        }
                    }
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
    color: #8ed2c9;
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
  .v-expansion-panels {
    z-index: 2;
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
  .v-expansion-panel--active > .v-expansion-panel-header {
    border-bottom: 1px solid #e2e2e2;
  }
</style>
