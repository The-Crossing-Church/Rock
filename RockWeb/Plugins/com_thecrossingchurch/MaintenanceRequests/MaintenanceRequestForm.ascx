<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="MaintenanceRequestForm.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.MaintenanceRequests.MaintenanceRequestForm"
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
<asp:HiddenField ID="hfRequest" runat="server" />
<asp:HiddenField ID="hfActiveRequests" runat="server" />
<Rock:BootstrapButton
  runat="server"
  ID="btnSubmit"
  CssClass="btn-hidden"
  OnClick="btnSubmit_Click"
/>

<div id="app">
  <v-app>
    <div>
      <v-row> 
        <v-col cols="12">
          <v-card>
            <v-card-title>
              Submit a Maintenance Request
            </v-card-title> 
            <v-card-text>
              <v-form ref="form" v-model="isValid">
                <v-row>
                  <v-col cols="12">
                    <v-textarea
                      label="Description of Problem"
                      v-model="request.Description"
                      :rules="[rules.required(request.Description, 'Description')]"
                    ></v-textarea>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12" md="6">
                    <v-menu
                      ref="menu"
                      v-model="menu"
                      :close-on-content-click="false"
                      transition="scale-transition"
                      offset-y
                      min-width="auto"
                      attach
                    >
                      <template v-slot:activator="{ on, attrs }">
                        <v-text-field
                          v-model="request.RequestedCompletionDate"
                          label="Requested Completion Date"
                          prepend-inner-icon="mdi-calendar"
                          readonly
                          v-bind="attrs"
                          v-on="on"
                        ></v-text-field>
                      </template>
                      <v-date-picker
                        v-model="request.RequestedCompletionDate"
                        @input="menu = false"
                      >
                      </v-date-picker>
                    </v-menu>
                  </v-col>
                  <v-col cols="12" md="6">
                    <v-checkbox
                      :label="`Is this is saftey issue? (${boolToYesNo(request.SafetyIssue)})`"
                      v-model="request.SafetyIssue"
                    ></v-checkbox>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12" md="6">
                    <v-autocomplete
                      :items="groupedLocations"
                      item-text="Value"
                      item-value="Id"
                      label="Location"
                      v-model="request.Location"
                      attach
                      :rules="[rules.required(request.Location, 'Location')]"
                    >
                      <template v-slot:item="data">
                        <template v-if="typeof data.item !== 'object'">
                          <v-list-item-content v-text="data.item"></v-list-item-content>
                        </template>
                        <template v-else>
                          <v-list-item-content>
                            <v-list-item-title v-html="data.item.Value"></v-list-item-title>
                          </v-list-item-content>
                        </template>
                      </template>
                    </v-autocomplete>
                  </v-col>
                  <v-col cols="12" md="6">
                    <v-file-input 
                      accept="image/*" 
                      capture="camera"
                      prepend-inner-icon="mdi-camera"
                      prepend-icon=""
                      label="Take Photo"
                      @change="handleFile"
                    ></v-file-input>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12" md="6">
                    <v-checkbox
                      label="Check here if you want to receive email notifications about this request"
                      v-model="request.SendNotifications"
                    ></v-checkbox>
                  </v-col>
                </v-row>
              </v-form>
            </v-card-text>
            <v-card-actions>
              <v-spacer></v-spacer>
              <v-btn color="primary" @click="submit">Submit</v-btn>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>
      <v-dialog 
        v-if="dialog"
        v-model="dialog"
        max-width="80%"
      >
        <v-card>
          <v-card-title>
            Is This a Duplicate?
          </v-card-title>
          <v-card-text>
            <v-row>
              <v-col>
                Other requests for this location were submitted recently, please review the descriptions and confirm your request is not a duplicate.
              </v-col>
            </v-row>
            <v-row v-for="r in possibleDuplicates" :key="r.Id">
              <v-col>
                {{r.Description}}
              </v-col>
            </v-row>
          </v-card-text>
          <v-card-actions>
            <v-btn color="secondary" @click="request = {}; dialog = false;">Duplicate, Don't Submit</v-btn>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="save">Unique, Submit Request</v-btn>
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
                request: {
                    Id: 0,
                    Description: '',
                    RequestedCompletionDate: '',
                    Location: '',
                    SafetyIssue: false,
                    RequestStatus: '',
                    Image: null,
                    Comments: [],
                    SendNotifications: false
                },
                locations: [],
                active: [],
                menu: false,
                isValid: false,
                dialog: false,
                rules: {
                    required(val, field) {
                        return !!val || `${field} is required`;
                    },
                }
            },
            created() {
                this.locations = JSON.parse($('[id$="hfLocations"]')[0].value);
                this.active = JSON.parse($('[id$="hfActiveRequests"]')[0].value);
                window["moment-range"].extendMoment(moment);
            },
            mounted() {
                let query = new URLSearchParams(window.location.search);
                let locid = query.get("LocationId");
                if (locid) {
                    this.request.Location = parseInt(locid)
                }
            },
            computed: {
                groupedLocations() {
                    let loc = []
                    this.locations.forEach(l => {
                        let idx = -1
                        loc.forEach((i, x) => {
                            if (i.Type == l.Type) {
                                idx = x
                            }
                        })
                        if (idx > -1) {
                            loc[idx].locations.push(l)
                        } else {
                            loc.push({ Type: l.Type, locations: [l] })
                        }
                    })
                    loc.forEach(l => {
                        l.locations = l.locations.sort((a, b) => {
                            if (a.Value < b.Value) {
                                return -1
                            } else if (a.Value > b.Value) {
                                return 1
                            } else {
                                return 0
                            }
                        })
                    })
                    loc = loc.sort((a, b) => {
                        if (a.Type < b.Type) {
                            return -1
                        } else if (a.Type > b.Type) {
                            return 1
                        } else {
                            return 0
                        }
                    })
                    let arr = []
                    loc.forEach(l => {
                        arr.push({ header: l.Type })
                        l.locations.forEach(i => {
                            arr.push((i))
                        })
                        arr.push({ divider: true })
                    })
                    arr.splice(arr.length - 1, 1)
                    return arr
                },
                possibleDuplicates() {
                    return this.active.filter(a => {
                        return parseInt(a.Location) == this.request.Location
                    })
                }
            },
            methods: {
                boolToYesNo(val) {
                    if (val) {
                        return "Yes";
                    }
                    return "No";
                },
                handleFile(e) {
                    let file = { name: e.name, type: e.type };
                    var reader = new FileReader();
                    const self = this;
                    reader.onload = function (e) {
                        console.log(e.target.result);
                        file.data = e.target.result;
                        self.request.Image = file;
                    };
                    reader.readAsDataURL(e);
                },
                submit() {
                    this.$refs.form.validate()
                    if (this.isValid) {
                        if (this.possibleDuplicates.length == 0) {
                            this.save()
                        } else {
                            this.dialog = true
                        }
                    }
                },
                save() {
                    this.request.Image = JSON.stringify(this.request.Image)
                    $('[id$="hfRequest"]').val(JSON.stringify(this.request));
                    $('[id$="btnSubmit"')[0].click();
                }
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
</style>
