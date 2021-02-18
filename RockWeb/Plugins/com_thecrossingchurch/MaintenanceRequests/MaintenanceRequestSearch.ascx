<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="MaintenanceRequestSearch.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.MaintenanceRequests.MaintenanceRequestSearch"
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

<div id="app">
  <v-app>
    <div>
      <v-card>
        <v-card-text>
          <v-row>
            <v-col>
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
            <v-col>
              <v-autocomplete
                label="Current Status"
                :items="['Submitted', 'Active', 'Complete']"
                v-model="search.status"
                multiple
                clearable
                attach
              ></v-autocomplete>
            </v-col>
            <v-col>
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
            <v-col>
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
          <v-data-table
            :headers="headers"
            :items="filtered"
          >
            <template v-slot:item.Location="{ item }">
              {{ formatLocation(item.Location) }}
            </template>
            <template v-slot:item.CreatedOn="{ item }">
              {{ item.CreatedOn | formatDateTime }}
            </template>
          </v-data-table>
        </v-card-text>
      </v-card>
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
                filtered: [],
                locations: [],
                headers: [
                    { text: 'Location', value: 'location' },
                    { text: 'Submitted By', value: 'CreatedBy.FullName' },
                    { text: 'Submitted On', value: 'createdon' },
                    { text: 'Status', value: 'RequestStatus' },
                ],
                search: {},
                startMenu: false,
                endMenu: false
            },
            created() {
                this.locations = JSON.parse($('[id$="hfLocations"]')[0].value);
                this.requests = JSON.parse($('[id$="hfRequests"]')[0].value);
                this.filtered = this.requests
                window["moment-range"].extendMoment(moment);
            },
            mounted() { },
            computed: {},
            methods: {
                formatLocation(id) {
                    return this.locations.filter((l) => {
                        return l.Id == id;
                    })[0].Value;
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
                    this.filtered = temp
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
  .comment {
    background-color: lightgrey;
    padding: 8px;
    border-radius: 6px;
  }
  .floating-title {
    text-transform: uppercase;
  }
  .v-dialog {
    max-height: 75vh !important;
  }
</style>
