<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="EventSubmissionDashboard.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.EventSubmission.EventSubmissionDashboard"
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
<script 
  src="https://cdnjs.cloudflare.com/ajax/libs/moment-range/4.0.2/moment-range.js" 
  integrity="sha512-XKgbGNDruQ4Mgxt7026+YZFOqHY6RsLRrnUJ5SVcbWMibG46pPAC97TJBlgs83N/fqPTR0M89SWYOku6fQPgyw==" 
  crossorigin="anonymous"
></script>

<asp:HiddenField ID="hfRooms" runat="server" />
<asp:HiddenField ID="hfMinistries" runat="server" />
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

<div id="app">
  <v-app>
    <div>
      <v-row>
        <v-col>
          <v-card>
            <v-card-text>
              <v-list>
                <v-list-item class="list-with-border">
                  <v-row>
                    <v-col><strong>Request</strong></v-col>
                    <v-col><strong>Submitted By</strong></v-col>
                    <v-col><strong>Submitted On</strong></v-col>
                    <v-col><strong>Event Dates</strong></v-col>
                    <v-col><strong>Requested Resources</strong></v-col>
                    <v-col cols="3"><strong>Status</strong></v-col>
                    <v-col cols="1"><strong>Add Buffer</strong></v-col>
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
                    <v-col cols="3">
                      <v-row align="center">
                        <v-col cols="6" :class="getStatusPillClass(r.RequestStatus)">{{ r.RequestStatus }}</v-col>
                        <v-col cols="6" class="no-top-pad">
                          <v-btn
                            v-if="r.RequestStatus == 'Submitted' || r.RequestStatus == 'Pending Changes'"
                            color="accent"
                            @click="changeStatus('Approved', r.Id)"
                          >Approve</v-btn>
                        </v-col>
                      </v-row>
                    </v-col>
                    <v-col cols="1" class='d-flex justify-center'>
                      <v-btn v-if="r.RequestStatus != 'Approved'" color="primary" @click="selected = r; bufferErrMsg = ''; dialog = true;" fab>
                        <v-icon>mdi-clock-outline</v-icon>
                      </v-btn>
                    </v-col>
                  </v-row>
                </v-list-item>
              </v-list>
            </v-card-text>
            <v-card-actions>
              <v-btn color="primary" @click="openHistory">
                <v-icon>mdi-history</v-icon> View Request History
              </v-btn>
            </v-card-actions>
          </v-card>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-card>
            <v-card-text>
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
                There are no approved events this week.
              </template>
            </v-card-text>
          </v-card>
        </v-col>
        <v-col> </v-col>
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
                {{requestType(this.selected)}}
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
                  <v-row v-if="e.StartTime || e.EndTime || ( selected.Changes && (selected.Changes.Events[idx].StartTime || selected.Changes.Events[idx].EndTime) )">
                    <v-col v-if="e.StartTime || (selected.Changes && selected.Changes.Events[idx].StartTime)">
                      <div class="floating-title">Start Time</div>
                      <template v-if="selected.Changes != null && e.StartTime != selected.Changes.Events[idx].StartTime">
                        <span class='red--text'>{{(e.StartTime ? e.StartTime : 'Empty')}}: </span>
                        <span class='primary--text'>{{(selected.Changes.Events[idx].StartTime ? selected.Changes.Events[idx].StartTime : 'Empty')}}</span>
                      </template>
                      <template v-else>
                        {{e.StartTime}}
                      </template>
                    </v-col>
                    <v-col v-if="e.EndTime || (selected.Changes && selected.Changes.Events[idx].EndTime)">
                      <div class="floating-title">End Time</div>
                      <template v-if="selected.Changes != null && e.EndTime != selected.Changes.Events[idx].EndTime">
                        <span class='red--text'>{{(e.EndTime ? e.EndTime : 'Empty')}}: </span>
                        <span class='primary--text'>{{(selected.Changes.Events[idx].EndTime ? selected.Changes.Events[idx].EndTime : 'Empty')}}</span>
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
                    <h6 class='text--accent text-uppercase'>Space Information</h6>
                    <v-row>
                      <v-col>
                        <div class="floating-title">Expected Number of Attendees</div>
                        <template v-if="selected.Changes != null && e.ExpectedAttendance != selected.Changes.Events[idx].ExpectedAttendance">
                          <span class='red--text'>{{(e.ExpectedAttendance ? e.ExpectedAttendance : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].ExpectedAttendance ? selected.Changes.Events[idx].ExpectedAttendance : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.ExpectedAttendance}}
                        </template>
                      </v-col>
                      <v-col>
                        <div class="floating-title">Desired Rooms/Spaces</div>
                        <template v-if="selected.Changes != null && formatRooms(e.Rooms) != formatRooms(selected.Changes.Events[idx].Rooms)">
                          <span class='red--text'>{{(e.Rooms ? formatRooms(e.Rooms) : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].Rooms ? formatRooms(selected.Changes.Events[idx].Rooms) : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{formatRooms(e.Rooms)}}
                        </template>
                      </v-col>
                    </v-row>
                    <v-row v-if="selected.needsReg">
                      <v-col>
                        <div class="floating-title">Check-in Requested</div>
                        <template v-if="selected.Changes != null && e.Checkin != selected.Changes.Events[idx].Checkin">
                          <span class='red--text'>{{(e.Checkin != null ? boolToYesNo(e.Checkin) : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].Checkin != null ? boolToYesNo(selected.Changes.Events[idx].Checkin) : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{boolToYesNo(e.Checkin)}}
                        </template>
                      </v-col>
                      <v-col v-if="(e.Checkin || (selected.Changes && selected.Changes.Events[idx].Checkin)) && (e.ExpectedAttendance >= 100 || (selected.Changes && selected.Changes.Events[idx].ExpectedAttendance >= 100))">
                        <div class="floating-title">Database Team Support Requested</div>
                        <template v-if="selected.Changes != null && e.SupportTeam != selected.Changes.Events[idx].SupportTeam">
                          <span class='red--text'>{{(e.SupportTeam != null ? boolToYesNo(e.SupportTeam) : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].SupportTeam != null ? boolToYesNo(selected.Changes.Events[idx].SupportTeam) : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{boolToYesNo(e.SupportTeam)}}
                        </template>
                      </v-col>
                    </v-row>
                  </template>
                  <template v-if="selected.needsOnline || (selected.Changes && selected.Changes.needsOnline)">
                    <h6 class='text--accent text-uppercase'>Online Information</h6>
                    <v-row>
                      <v-col>
                        <div class="floating-title">Event Link</div>
                        <template v-if="selected.Changes != null && e.EventURL != selected.Changes.Events[idx].EventURL">
                          <span class='red--text'>{{(e.EventURL ? e.EventURL : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].EventURL ? selected.Changes.Events[idx].EventURL : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.EventURL}}
                        </template>
                      </v-col>
                      <v-col v-if="e.ZoomPassword || (selected.Changes && selected.Changes.Events[idx].ZoomPassword)">
                        <div class="floating-title">Password</div>
                        <template v-if="selected.Changes != null && e.ZoomPassword != selected.Changes.Events[idx].ZoomPassword">
                          <span class='red--text'>{{(e.ZoomPassword ? e.ZoomPassword : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].ZoomPassword ? selected.Changes.Events[idx].ZoomPassword : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.ZoomPassword}}
                        </template>
                      </v-col>
                    </v-row>
                  </template>
                  <template v-if="selected.needsChildCare || (selected.Changes && selected.Changes.needsChildCare)">
                    <h6 class='text--accent text-uppercase'>Childcare Information</h6>
                    <v-row>
                      <v-col>
                        <div class="floating-title">Childcare Age Groups</div>
                        <template v-if="selected.Changes != null && e.ChildCareOptions.join(', ') != selected.Changes.Events[idx].ChildCareOptions.join(', ')">
                          <span class='red--text'>{{(e.ChildCareOptions ? e.ChildCareOptions.join(', ') : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].ChildCareOptions ? selected.Changes.Events[idx].ChildCareOptions.join(', ') : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.ChildCareOptions.join(', ')}}
                        </template>
                      </v-col>
                      <v-col>
                        <div class="floating-title">Expected Number of Children</div>
                        <template v-if="selected.Changes != null && e.EstimatedKids != selected.Changes.Events[idx].EstimatedKids">
                          <span class='red--text'>{{(e.EstimatedKids ? e.EstimatedKids : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].EstimatedKids ? selected.Changes.Events[idx].EstimatedKids : 'Empty')}}</span>
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
                          <span class='red--text'>{{(e.CCStartTime ? e.CCStartTime : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].CCStartTime ? selected.Changes.Events[idx].CCStartTime : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.CCStartTime}}
                        </template>
                      </v-col>
                      <v-col>
                        <div class="floating-title">Childcare End Time</div>
                        <template v-if="selected.Changes != null && e.CCEndTime != selected.Changes.Events[idx].CCEndTime">
                          <span class='red--text'>{{(e.CCEndTime ? e.CCEndTime : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].CCEndTime ? selected.Changes.Events[idx].CCEndTime : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.CCEndTime}}
                        </template>
                      </v-col>
                    </v-row>
                  </template>
                  <template v-if="selected.needsCatering || (selected.Changes && selected.Changes.needsCatering)">
                    <h6 class='text--accent text-uppercase'>Catering Information</h6>
                    <v-row>
                      <v-col>
                        <div class="floating-title">Preferred Vendor</div>
                        <template v-if="selected.Changes != null && e.Vendor != selected.Changes.Events[idx].Vendor">
                          <span class='red--text'>{{(e.Vendor ? e.Vendor : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].Vendor ? selected.Changes.Events[idx].Vendor : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.Vendor}}
                        </template>
                      </v-col>
                      <v-col>
                        <div class="floating-title">Budget Line</div>
                        <template v-if="selected.Changes != null && e.BudgetLine != selected.Changes.Events[idx].BudgetLine">
                          <span class='red--text'>{{(e.BudgetLine ? e.BudgetLine : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].BudgetLine ? selected.Changes.Events[idx].BudgetLine : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.BudgetLine}}
                        </template>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col>
                        <div class="floating-title">Preferred Menu</div>
                        <template v-if="selected.Changes != null && e.Menu != selected.Changes.Events[idx].Menu">
                          <span class='red--text'>{{(e.Menu ? e.Menu : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].Menu ? selected.Changes.Events[idx].Menu : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.Menu}}
                        </template>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col>
                        <div class="floating-title">{{foodTimeTitle(e)}}</div>
                        <template v-if="selected.Changes != null && e.FoodTime != selected.Changes.Events[idx].FoodTime">
                          <span class='red--text'>{{(e.FoodTime ? e.FoodTime : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].FoodTime ? selected.Changes.Events[idx].FoodTime : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.FoodTime}}
                        </template>
                      </v-col>
                      <v-col v-if="e.FoodDelivery || (selected.Changes && selected.Changes.Events[idx].FoodDelivery)">
                        <div class="floating-title">Food Drop off Location</div>
                        <template v-if="selected.Changes != null && e.FoodDropOff != selected.Changes.Events[idx].FoodDropOff">
                          <span class='red--text'>{{(e.FoodDropOff ? e.FoodDropOff : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].FoodDropOff ? selected.Changes.Events[idx].FoodDropOff : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.FoodDropOff}}
                        </template>
                      </v-col>
                    </v-row>
                    <v-row v-if="(e.Drinks && e.Drinks.length > 0) || (selected.Changes && selected.Changes.Events[idx].Drinks && selected.Changes.Events[idx].Drinks.length > 0)">
                      <v-col>
                        <div class="floating-title">Desired Drinks</div>
                        <template v-if="selected.Changes != null && e.Drinks.join(', ') != selected.Changes.Events[idx].Drinks.join(', ')">
                          <span class='red--text'>{{(e.Drinks ? e.Drinks.join(', ') : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].Drinks? selected.Changes.Events[idx].Drinks.join(', ') : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.Drinks.join(', ')}}
                        </template>
                      </v-col>
                    </v-row>
                    <v-row v-if="e.DrinkTime || (selected.Changes && selected.Changes.Events[idx].DrinkTime)">
                      <v-col>
                        <div class="floating-title">Drink Set-up Time</div>
                        <template v-if="selected.Changes != null && e.DrinkTime != selected.Changes.Events[idx].DrinkTime">
                          <span class='red--text'>{{(e.DrinkTime ? e.DrinkTime : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkTime ? selected.Changes.Events[idx].DrinkTime : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.DrinkTime}}
                        </template>
                      </v-col>
                      <v-col>
                        <div class="floating-title">Drink Drop off Location</div>
                        <template v-if="selected.Changes != null && e.DrinkDropOff != selected.Changes.Events[idx].DrinkDropOff">
                          <span class='red--text'>{{(e.DrinkDropOff ? e.DrinkDropOff : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkDropOff ? selected.Changes.Events[idx].DrinkDropOff : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.DrinkDropOff}}
                        </template>
                      </v-col>
                    </v-row>
                    <template v-if="selected.needsChildCare || (selected.Changes && selected.Changes.needsChildCare)">
                      <h6 class='text--accent text-uppercase'>Childcare Catering Information</h6>
                      <v-row>
                        <v-col>
                          <div class="floating-title">
                            Preferred Vendor for Childcare
                          </div>
                          <template v-if="selected.Changes != null && e.CCVendor != selected.Changes.Events[idx].CCVendor">
                            <span class='red--text'>{{(e.CCVendor ? e.CCVendor : 'Empty')}}: </span>
                            <span class='primary--text'>{{(selected.Changes.Events[idx].CCVendor ? selected.Changes.Events[idx].CCVendor : 'Empty')}}</span>
                          </template>
                          <template v-else>
                            {{e.CCVendor}}
                          </template>
                        </v-col>
                        <v-col>
                          <div class="floating-title">Budget Line for Childcare</div>
                          <template v-if="selected.Changes != null && e.CCBudgetLine != selected.Changes.Events[idx].CCBudgetLine">
                            <span class='red--text'>{{(e.CCBudgetLine ? e.CCBudgetLine : 'Empty')}}: </span>
                            <span class='primary--text'>{{(selected.Changes.Events[idx].CCBudgetLine ? selected.Changes.Events[idx].CCBudgetLine : 'Empty')}}</span>
                          </template>
                          <template v-else>
                            {{e.CCBudgetLine}}
                          </template>
                        </v-col>
                      </v-row>
                      <v-row>
                        <v-col>
                          <div class="floating-title">
                            Preferred Menu for Childcare
                          </div>
                          <template v-if="selected.Changes != null && e.CCMenu != selected.Changes.Events[idx].CCMenu">
                            <span class='red--text'>{{(e.CCMenu ? e.CCMenu : 'Empty')}}: </span>
                            <span class='primary--text'>{{(selected.Changes.Events[idx].CCMenu ? selected.Changes.Events[idx].CCMenu : 'Empty')}}</span>
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
                            <span class='red--text'>{{(e.CCFoodTime ? e.CCFoodTime : 'Empty')}}: </span>
                            <span class='primary--text'>{{(selected.Changes.Events[idx].CCFoodTime ? selected.Changes.Events[idx].CCFoodTime : 'Empty')}}</span>
                          </template>
                          <template v-else>
                            {{e.CCFoodTime}}
                          </template>
                        </v-col>
                      </v-row>
                    </template>
                  </template>
                  <template v-if="selected.needsReg || (selected.Changes && selected.Changes.needsReg)">
                    <h6 class='text--accent text-uppercase'>Registration Information</h6>
                    <v-row v-if="e.RegistrationDate || (selected.Changes && selected.Changes.Events[idx].RegistrationDate)">
                      <v-col>
                        <div class="floating-title">Registration Date</div>
                        <template v-if="selected.Changes != null && e.RegistrationDate != selected.Changes.Events[idx].RegistrationDate">
                          <span class='red--text' v-if="e.RegistrationDate">{{e.RegistrationDate | formatDate}}: </span>
                          <span class='red--text' v-else>Empty: </span>
                          <span class='primary--text' v-if="selected.Changes.Events[idx].RegistrationDate">{{selected.Changes.Events[idx].RegistrationDate | formatDate}}</span>
                          <span class='primary--text' v-else>Empty</span>
                        </template>
                        <template v-else>
                          {{e.RegistrationDate | formatDate}}
                        </template>
                      </v-col>
                      <v-col v-if="e.FeeType || (selected.Changes && selected.Changes.Events[idx].FeeType)">
                        <div class="floating-title">Registration Fee Types</div>
                        <template v-if="selected.Changes != null && e.FeeType.toString() != selected.Changes.Events[idx].FeeType.toString()">
                          <span class='red--text' v-if="e.FeeType">{{e.FeeType.join(', ')}}: </span>
                          <span class='red--text' v-else>Empty: </span>
                          <span class='primary--text' v-if="selected.Changes.Events[idx].FeeType">{{selected.Changes.Events[idx].FeeType.join(', ')}}</span>
                          <span class='primary--text' v-else>Empty</span>
                        </template>
                        <template v-else>
                          {{e.FeeType.join(', ')}}
                        </template>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col cols="12" md="6" v-if="e.Fee || (selected.Changes && selected.Changes.Events[idx].Fee)">
                        <div class="floating-title">Individual Registration Fee</div>
                        <template v-if="selected.Changes != null && e.Fee != selected.Changes.Events[idx].Fee">
                          <span class='red--text' v-if="e.Fee">{{e.Fee | formatCurrency}}: </span>
                          <span class='red--text' v-else>Empty: </span>
                          <span class='primary--text' v-if="selected.Changes.Events[idx].Fee">{{selected.Changes.Events[idx].Fee | formatCurrency}}</span>
                          <span class='primary--text' v-else>Empty</span>
                        </template>
                        <template v-else>
                          {{e.Fee | formatCurrency}}
                        </template>
                      </v-col>
                      <v-col cols="12" md="6" v-if="e.CoupleFee || (selected.Changes && selected.Changes.Events[idx].CoupleFee)">
                        <div class="floating-title">Couple Registration Fee</div>
                        <template v-if="selected.Changes != null && e.CoupleFee != selected.Changes.Events[idx].CoupleFee">
                          <span class='red--text' v-if="e.CoupleFee">{{e.CoupleFee | formatCurrency}}: </span>
                          <span class='red--text' v-else>Empty: </span>
                          <span class='primary--text' v-if="selected.Changes.Events[idx].CoupleFee">{{selected.Changes.Events[idx].CoupleFee | formatCurrency}}</span>
                          <span class='primary--text' v-else>Empty</span>
                        </template>
                        <template v-else>
                          {{e.CoupleFee | formatCurrency}}
                        </template>
                      </v-col>
                      <v-col cols="12" md="6" v-if="e.OnlineFee || (selected.Changes && selected.Changes.Events[idx].OnlineFee)">
                        <div class="floating-title">Online Registration Fee</div>
                        <template v-if="selected.Changes != null && e.OnlineFee != selected.Changes.Events[idx].OnlineFee">
                          <span class='red--text' v-if="e.OnlineFee">{{e.OnlineFee | formatCurrency}}: </span>
                          <span class='red--text' v-else>Empty: </span>
                          <span class='primary--text' v-if="selected.Changes.Events[idx].OnlineFee">{{selected.Changes.Events[idx].OnlineFee | formatCurrency}}</span>
                          <span class='primary--text' v-else>Empty</span>
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
                          <span class='red--text' v-if="e.RegistrationEndDate">{{e.RegistrationEndDate | formatDate}}: </span>
                          <span class='red--text' v-else>Empty: </span>
                          <span class='primary--text' v-if="selected.Changes.Events[idx].RegistrationEndDate">{{selected.Changes.Events[idx].RegistrationEndDate | formatDate}}</span>
                          <span class='primary--text' v-else>Empty</span>
                        </template>
                        <template v-else>
                          {{e.RegistrationEndDate | formatDate}}
                        </template>
                      </v-col>
                      <v-col v-if="e.RegistrationEndTime || (selected.Changes && selected.Changes.Events[idx].RegistrationEndTime)">
                        <div class="floating-title">Registration Close Time</div>
                        <template v-if="selected.Changes != null && e.RegistrationEndTime != selected.Changes.Events[idx].RegistrationEndTime">
                          <span class='red--text'>{{(e.RegistrationEndTime ? e.RegistrationEndTime : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].RegistrationEndTime ? selected.Changes.Events[idx].RegistrationEndTime : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.RegistrationEndTime}}
                        </template>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col v-if="e.ThankYou || (selected.Changes && selected.Changes.Events[idx].ThankYou)">
                        <div class="floating-title">Confirmation Email Thank You</div>
                        <template v-if="selected.Changes != null && e.ThankYou != selected.Changes.Events[idx].ThankYou">
                          <span class='red--text'>{{(e.ThankYou ? e.ThankYou : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].ThankYou ? selected.Changes.Events[idx].ThankYou : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.ThankYou}}
                        </template>
                      </v-col>
                      <v-col v-if="e.TimeLocation || (selected.Changes && selected.Changes.Events[idx].TimeLocation)">
                        <div class="floating-title">Confirmation Email Time and Location</div>
                        <template v-if="selected.Changes != null && e.TimeLocation != selected.Changes.Events[idx].TimeLocation">
                          <span class='red--text'>{{(e.TimeLocation ? e.TimeLocation : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].TimeLocation ? selected.Changes.Events[idx].TimeLocation : 'Empty')}}</span>
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
                          <span class='red--text'>{{(e.AdditionalDetails ? e.AdditionalDetails : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].AdditionalDetails ? selected.Changes.Events[idx].AdditionalDetails : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.AdditionalDetails}}
                        </template>
                      </v-col>
                    </v-row>
                  </template>
                  <template v-if="selected.needsAccom || (selected.Changes && selected.Changes.needsAccom)">
                    <h6 class='text--accent text-uppercase'>Additional Information</h6>
                    <v-row v-if="e.TechNeeds || e.TechDescription || (selected.Changes && (selected.Changes.Events[idx].TechNeeds || selected.Changes.Events[idx].TechDescription))">
                      <v-col v-if="e.TechNeeds && e.TechNeeds.length > 0">
                        <div class="floating-title">Tech Needs</div>
                        <template v-if="selected.Changes != null && e.TechNeeds.join(', ') != selected.Changes.Events[idx].TechNeeds.join(', ')">
                          <span class='red--text'>{{(e.TechNeeds ? e.TechNeeds.join(', ') : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].TechNeeds ? selected.Changes.Events[idx].TechNeeds.join(', ') : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.TechNeeds.join(', ')}}
                        </template>
                      </v-col>
                      <v-col v-if="e.TechDescription || (selected.Changes && selected.Changes.Events[idx].TechDescription)">
                        <div class="floating-title">Tech Description</div>
                        <template v-if="selected.Changes != null && e.TechDescription != selected.Changes.Events[idx].TechDescription">
                          <span class='red--text'>{{(e.TechDescription ? e.TechDescription : 'Empty')}}: </span>
                          <span class='primary--text'>{{selected.Changes.Events[idx].TechDescription}}</span>
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
                          <template v-if="selected.Changes != null && e.Drinks.join(', ') != selected.Changes.Events[idx].Drinks.join(', ')">
                            <span class='red--text'>{{(e.Drinks ? e.Drinks.join(', ') : 'Empty')}}: </span>
                            <span class='primary--text'>{{(selected.Changes.Events[idx].Drinks ? selected.Changes.Events[idx].Drinks.join(', ') : 'Empty')}}</span>
                          </template>
                          <template v-else>
                            {{e.Drinks.join(', ')}}
                          </template>
                        </v-col>
                      </v-row>
                      <v-row v-if="e.DrinkTime || (selected.Changes && selected.Changes.Events[idx].DrinkTime)">
                        <v-col>
                          <div class="floating-title">Drink Set-up Time</div>
                          <template v-if="selected.Changes != null && e.DrinkTime != selected.Changes.Events[idx].DrinkTime">
                            <span class='red--text'>{{(e.DrinkTime ? e.DrinkTime : 'Empty')}}: </span>
                            <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkTime ? selected.Changes.Events[idx].DrinkTime : 'Empty')}}</span>
                          </template>
                          <template v-else>
                            {{e.DrinkTime}}
                          </template>
                        </v-col>
                        <v-col>
                          <div class="floating-title">Drink Drop off Location</div>
                          <template v-if="selected.Changes != null && e.DrinkDropOff != selected.Changes.Events[idx].DrinkDropOff">
                            <span class='red--text'>{{(e.DrinkDropOff ? e.DrinkDropOff : 'Empty')}}: </span>
                            <span class='primary--text'>{{(selected.Changes.Events[idx].DrinkDropOff ? selected.Changes.Events[idx].DrinkDropOff : 'Empty')}}</span>
                          </template>
                          <template v-else>
                            {{e.DrinkDropOff}}
                          </template>
                        </v-col>
                      </v-row>
                    </template>
                    <v-row>
                      <v-col>
                        <div class="floating-title">Add to public calendar</div>
                        <template v-if="selected.Changes != null && e.ShowOnCalendar != selected.Changes.Events[idx].ShowOnCalendar">
                          <span class='red--text'>{{(e.ShowOnCalendar != null ? boolToYesNo(e.ShowOnCalendar) : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].ShowOnCalendar != null ? boolToYesNo(selected.Changes.Events[idx].ShowOnCalendar) : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{boolToYesNo(e.ShowOnCalendar)}}
                        </template>
                      </v-col>
                    </v-row>
                    <v-row v-if="(e.ShowOnCalendar || (selected.Changes && selected.Changes.Events[idx].ShowOnCalendar)) && (e.PublicityBlurb || (selected.Changes && selected.Changes.Events[idx].PublicityBlurb))">
                      <v-col>
                        <div class="floating-title">Publicity Blurb</div>
                        <template v-if="selected.Changes != null && e.PublicityBlurb != selected.Changes.Events[idx].PublicityBlurb">
                          <span class='red--text'>{{(e.PublicityBlurb ? e.PublicityBlurb : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].PublicityBlurb ? selected.Changes.Events[idx].PublicityBlurb : 'Empty')}}</span>
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
                          <span class='red--text'>{{(e.SetUp ? e.SetUp : 'Empty')}}: </span>
                          <span class='primary--text'>{{(selected.Changes.Events[idx].SetUp ? selected.Changes.Events[idx].SetUp : 'Empty')}}</span>
                        </template>
                        <template v-else>
                          {{e.SetUp}}
                        </template>
                      </v-col>
                    </v-row>
                    <v-row v-if="e.SetUpImage || (selected.Changes && selected.Changes.Events[idx].SetUpImage)">
                      <v-col>
                        <div class="floating-title">Set-up Image</div>
                        {{e.SetUpImage.name}}
                        <v-btn icon color="accent" @click="saveFile(idx, 'existing')">
                          <v-icon color="accent">mdi-download</v-icon>
                        </v-btn>
                      </v-col>
                      <v-col v-if="selected.Changes != null && e.SetUpImage != selected.Changes.Events[idx].SetUpImage">
                        <div class="floating-title">Set-up Image</div>
                        {{selected.Changes.Events[idx].SetUpImage.name}}
                        <v-btn icon color="accent" @click="saveFile(idx, 'new')">
                          <v-icon color="accent">mdi-download</v-icon>
                        </v-btn>
                      </v-col>
                    </v-row>
                  </template>
                </v-expansion-panel-content>
              </v-expansion-panel>
            </v-expansion-panels>
            <template v-if="selected.needsPub || (selected.Changes && selected.Changes.needsPub)">
              <h6 class='text--accent text-uppercase'>Publicity Information</h6>
              <v-row>
                <v-col>
                  <div class="floating-title">Describe Why Someone Should Attend Your Event (450)</div>
                  <template v-if="selected.Changes != null && selected.WhyAttendSixtyFive != selected.Changes.WhyAttendSixtyFive">
                    <span class='red--text'>{{(selected.WhyAttendSixtyFive ? selected.WhyAttendSixtyFive : 'Empty' )}}: </span>
                    <span class='primary--text'>{{(selected.Changes.WhyAttendSixtyFive ? selected.Changes.WhyAttendSixtyFive : 'Empty')}}</span>
                  </template>
                  <template v-else>
                    {{selected.WhyAttendSixtyFive}}
                  </template>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Target Audience</div>
                  <template v-if="selected.Changes != null && selected.TargetAudience != selected.Changes.TargetAudience">
                    <span class='red--text'>{{(selected.TargetAudience ? selected.TargetAudience : 'Empty')}}: </span>
                    <span class='primary--text'>{{(selected.Changes.TargetAudience ? selected.Changes.TargetAudience : 'Empty')}}</span>
                  </template>
                  <template v-else>
                    {{selected.TargetAudience}}
                  </template>
                </v-col>
                <v-col>
                  <div class="floating-title">Event is Sticky</div>
                  <template v-if="selected.Changes != null && selected.EventIsSticky != selected.Changes.EventIsSticky">
                    <span class='red--text'>{{(selected.EventIsSticky != null ? boolToYesNo(selected.EventIsSticky) : 'Empty')}}: </span>
                    <span class='primary--text'>{{(selected.Changes.EventIsSticky ? boolToYesNo(selected.Changes.EventIsSticky) : 'Empty')}}</span>
                  </template>
                  <template v-else>
                    {{boolToYesNo(selected.EventIsSticky)}}
                  </template>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Publicity Start Date</div>
                  <template v-if="selected.Changes != null && selected.PublicityStartDate != selected.Changes.PublicityStartDate">
                    <span class='red--text' v-if="selected.PublicityStartDate">{{selected.PublicityStartDate | formatDate}}: </span>
                    <span class='red--text' v-else>Empty: </span>
                    <span class='primary--text' v-if="selected.Changes.PublicityStartDate">{{selected.Changes.PublicityStartDate | formatDate}}</span>
                    <span class='primary--text' v-else>Empty</span>
                  </template>
                  <template v-else>
                    {{selected.PublicityStartDate | formatDate}}
                  </template>
                </v-col>
                <v-col>
                  <div class="floating-title">Publicity End Date</div>
                  <template v-if="selected.Changes != null && selected.PublicityEndDate != selected.Changes.PublicityEndDate">
                    <span class='red--text' v-if="selected.PublicityEndDate">{{selected.PublicityEndDate | formatDate}}: </span>
                    <span class='red--text' v-else>Empty: </span>
                    <span class='primary--text' v-if="selected.Changes.PublicityEndDate">{{selected.Changes.PublicityEndDate | formatDate}}</span>
                    <span class='primary--text' v-else>Empty</span>
                  </template>
                  <template v-else>
                    {{selected.PublicityEndDate | formatDate}}
                  </template>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Publicity Strategies</div>
                  <template v-if="selected.Changes != null && selected.PublicityStrategies.toString() != selected.Changes.PublicityStrategies.toString()">
                    <span class='red--text'>{{(selected.PublicityStrategies ? selected.PublicityStrategies.join(', ') : 'Empty')}}: </span>
                    <span class='primary--text'>{{(selected.Changes.PublicityStrategies ? selected.Changes.PublicityStrategies.join(', ') : 'Empty')}}</span>
                  </template>
                  <template v-else>
                    {{selected.PublicityStrategies.join(', ')}}
                  </template>
                </v-col>
              </v-row>
              <template v-if="selected.PublicityStrategies.includes('Social Media/Google Ads')">
                <v-row>
                  <v-col>
                    <div class="floating-title">Describe Why Someone Should Attend Your Event (90)</div>
                    <template v-if="selected.Changes != null && selected.WhyAttendNinety != selected.Changes.WhyAttendNinety">
                      <span class='red--text'>{{(selected.WhyAttendNinety ? selected.WhyAttendNinety : 'Empty')}}: </span>
                      <span class='primary--text'>{{(selected.Changes.WhyAttendNinety ? selected.Changes.WhyAttendNinety : 'Empty')}}</span>
                    </template>
                    <template v-else>
                      {{selected.WhyAttendNinety}}
                    </template>
                  </v-col>
                </v-row>
                <v-row>
                  <template v-if="selected.Changes != null && selected.GoogleKeys.toString() != selected.Changes.GoogleKeys.toString()">
                    <v-col class='red--text'>
                        <div class="floating-title">Google Keys</div>
                        <ul>
                          <li v-for="k in selected.GoogleKeys" :key="k">
                            {{k}}
                          </li>
                        </ul>
                      </v-col>
                      <v-col class='primary--text'>
                        <ul>
                          <li v-for="k in selected.Changes.GoogleKeys" :key="k">
                            {{k}}
                          </li>
                        </ul>
                      </v-col>
                    </template>
                    <template v-else>
                      <v-col>
                        <div class="floating-title">Google Keys</div>
                        <ul>
                          <li v-for="k in selected.GoogleKeys" :key="k">
                            {{k}}
                          </li>
                        </ul>
                      </v-col>
                  </template>
                </v-row>
              </template>
              <template v-if="selected.PublicityStrategies.includes('Mobile Worship Folder')">
                <v-row>
                  <v-col>
                    <div class="floating-title">Describe Why Someone Should Attend Your Event (65)</div>
                    <template v-if="selected.Changes != null && selected.WhyAttendTen != selected.Changes.WhyAttendTen">
                      <span class='red--text'>{{(selected.WhyAttendTen ? selected.WhyAttendTen : 'Empty')}}: </span>
                      <span class='primary--text'>{{(selected.Changes.WhyAttendTen ? selected.Changes.WhyAttendTen : 'Empty')}}</span>
                    </template>
                    <template v-else>
                      {{selected.WhyAttendTen}}
                    </template>
                  </v-col>
                  <v-col v-if="selected.VisualIdeas != ''">
                    <div class="floating-title">Visual Ideas for Graphic</div>
                    <template v-if="selected.Changes != null && selected.VisualIdeas != selected.Changes.VisualIdeas">
                      <span class='red--text'>{{(selected.VisualIdeas ? selected.VisualIdeas : 'Empty')}}: </span>
                      <span class='primary--text'>{{(selected.Changes.VisualIdeas ? selected.Changes.VisualIdeas : 'Empty')}}</span>
                    </template>
                    <template v-else>
                      {{selected.VisualIdeas}}
                    </template>
                  </v-col>
                </v-row>
              </template>
              <template v-if="selected.PublicityStrategies.includes('Announcement')">
                <v-row v-for="(s, sidx) in selected.Stories" :key="`Story_${sidx}`">
                  <template v-if="selected.Changes != null && selected.Stories.toString() != selected.Changes.Stories.toString()">
                    <v-col class='red--text'>
                      <div class="floating-title">Story {{sidx+1}}</div>
                      {{s.Name}}, {{s.Email}} <br/>
                      {{s.Description}}
                    </v-col>
                    <v-col class='primary--text'>
                      <div class="floating-title">Story {{sidx+1}}</div>
                      {{selected.Changes.Stories[sidx].Name}}, {{selected.Changes.Stories[sidx].Email}} <br/>
                      {{selected.Changes.Stories[sidx].Description}}
                    </v-col>
                  </template>
                  <template v-else>
                    <v-col>
                      <div class="floating-title">Story {{sidx+1}}</div>
                      {{s.Name}}, {{s.Email}} <br/>
                      {{s.Description}}
                    </v-col>
                  </template>
                </v-row>
                <v-row>
                  <v-col>
                    <div class="floating-title">Describe Why Someone Should Attend Your Event (175)</div>
                    <template v-if="selected.Changes != null && selected.WhyAttendTwenty != selected.Changes.WhyAttendTwenty">
                      <span class='red--text'>{{(selected.WhyAttendTwenty ? selected.WhyAttendTwenty : 'Empty')}}: </span>
                      <span class='primary--text'>{{(selected.Changes.WhyAttendTwenty ? selected.Changes.WhyAttendTwenty : 'Empty')}}</span>
                    </template>
                    <template v-else>
                      {{selected.WhyAttendTwenty}}
                    </template>
                  </v-col>
                </v-row>
              </template>
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
            <v-row v-if="selected.HistoricData">
              <v-col>
                <div class="floating-title">Non-Transferrable Data</div>
                {{selected.HistoricData}}
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
              @click="changeStatus('Approved', selected.Id)"
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
                <v-icon>mdi-close</v-icon> Changes
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
                current: [],
                selected: {},
                overlay: false,
                dialog: false,
                panels: [0],
                rooms: [],
                ministries: [],
                bufferErrMsg: '',
                fab: false
            },
            created() {
                this.getRecent();
                this.getCurrent();
                this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
                this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value)
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
                            });
                        });
                    });
                    return ordered.filter((o) => {
                        return o.Events.length > 0;
                    });
                },
            },
            methods: {
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
                        temp.push(req);
                    });
                    this.requests = temp;
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
                        temp.push(req);
                    });
                    this.current = temp;
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
                        let formattedVal = this.ministries.filter(m => {
                            return m.Id == val
                        })
                        return formattedVal[0].Value
                    }
                    return "";
                },
                requestType(itm) {
                    if (itm) {
                        let resoures = [];
                        if (itm.needsSpace) {
                            resoures.push("Room");
                        }
                        if (itm.needsOnline) {
                            resoures.push("Online");
                        }
                        if (itm.needsPub) {
                            resoures.push("Publicity");
                        }
                        if (itm.needsChildCare) {
                            resoures.push("Childcare");
                        }
                        if (itm.needsCatering) {
                            resoures.push("Catering");
                        }
                        if (itm.needsAccom) {
                            resoures.push("Extra Resources");
                        }
                        return resoures.join(", ");
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
                foodTimeTitle(e) {
                    if (e.FoodDelivery) {
                        return "Food Set-up time";
                    } else {
                        return "Desired Pick-up time from Vendor";
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
                changeStatus(status, id) {
                    $('[id$="hfRequestID"]').val(id);
                    $('[id$="hfAction"]').val(status);
                    $('[id$="btnChangeStatus"]')[0].click();
                },
                addBuffer() {
                    this.bufferErrMsg = ''
                    if (!this.checkHasConflicts()) {
                        $('[id$="hfRequestID"]').val(this.selected.Id);
                        $('[id$="hfUpdatedItem"]').val(JSON.stringify(this.selected));
                        $('[id$="btnAddBuffer"]')[0].click();
                    } else {
                        this.bufferErrMsg = 'The buffer you have chosen will conflict with another event'
                    }
                },
                checkHasConflicts() {
                    this.existingRequests = JSON.parse(
                        $('[id$="hfUpcomingRequests"]')[0].value
                    );
                    let conflictingMessage = []
                    let conflictingRequests = this.existingRequests.filter((r) => {
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
                                                });
                                                if (roomName.length > 0) {
                                                    roomName = roomName[0].Value;
                                                }
                                                roomNames.push(roomName)
                                            })
                                            conflictingMessage.push(`${moment(compareSource[y].Date).format('MM/DD/yyyy')} (${roomNames.join(", ")})`)
                                        }
                                    }
                                }
                            }
                        }
                        return conflicts
                    });
                    if (conflictingRequests.length > 0) {
                        return true
                    } else {
                        return false
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
</style>