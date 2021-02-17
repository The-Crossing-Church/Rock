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
                        <v-col cols="6" :class="getStatusPillClass(r.RequestStatus)"
                          >{{ r.RequestStatus }}</v-col
                        >
                        <v-col cols="6" class="no-top-pad">
                          <v-btn
                            v-if="r.RequestStatus == 'Submitted' || r.RequestStatus == 'Pending Changes'"
                            color="accent"
                            @click="changeStatus('Approved', r.Id)"
                            >Approve</v-btn
                          >
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
                      <v-list-item
                        ><strong>{{d.Timeframe}}</strong></v-list-item
                      >
                      <v-list-item
                        v-for="(i, idx) in d.Events"
                        :key="`event_${idx}`"
                        class="event-pill hover"
                        @click="selected = i; overlay = true;"
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
      <v-overlay :value="overlay">
        <v-card
          light
          width="100%"
          style="max-height: 75vh; overflow-y: scroll; margin-top: 100px"
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
            <v-row>
              <v-col>
                <div class="floating-title">Request Dates</div>
                <template v-if="selected.Changes != null && formatDates(selected.EventDates) != formatDates(selected.Changes.EventDates)">
                  <span class='red--text'>{{formatDates(selected.EventDates)}}: </span>
                  <span class='primary--text'>{{formatDates(selected.Changes.EventDates)}}</span>
                </template>
                <template v-else>
                  {{formatDates(selected.EventDates)}}
                </template>
              </v-col>
            </v-row>
            <v-row v-if="selected.StartTime || selected.EndTime">
              <v-col v-if="selected.StartTime">
                <div class="floating-title">Start Time</div>
                <template v-if="selected.Changes != null && selected.StartTime != selected.Changes.StartTime">
                  <span class='red--text'>{{selected.StartTime}}: </span>
                  <span class='primary--text'>{{selected.Changes.StartTime}}</span>
                </template>
                <template v-else>
                  {{selected.StartTime}}
                </template>
              </v-col>
              <v-col v-if="selected.EndTime">
                <div class="floating-title">End Time</div>
                <template v-if="selected.Changes != null && selected.EndTime != selected.Changes.EndTime">
                  <span class='red--text'>{{selected.EndTime}}: </span>
                  <span class='primary--text'>{{selected.Changes.EndTime}}</span>
                </template>
                <template v-else>
                  {{selected.EndTime}}
                </template>
              </v-col>
            </v-row>
            <v-row v-if="selected.MinsStartBuffer || selected.MinsEndBuffer">
              <v-col v-if="selected.MinsStartBuffer">
                <div class="floating-title">Set-up Buffer</div>
                {{selected.MinsStartBuffer}} minutes
              </v-col>
              <v-col v-if="selected.MinsEndBuffer">
                <div class="floating-title">Tear-down Buffer</div>
                {{selected.MinsEndBuffer}} minutes
              </v-col>
            </v-row>
            <template v-if="selected.needsSpace">
              <v-row>
                <v-col>
                  <div class="floating-title">Expected Number of Attendees</div>
                  <template v-if="selected.Changes != null && selected.ExpectedAttendance != selected.Changes.ExpectedAttendance">
                    <span class='red--text'>{{selected.ExpectedAttendance}}: </span>
                    <span class='primary--text'>{{selected.Changes.ExpectedAttendance}}</span>
                  </template>
                  <template v-else>
                    {{selected.ExpectedAttendance}}
                  </template>
                </v-col>
                <v-col>
                  <div class="floating-title">Desired Rooms/Spaces</div>
                  <template v-if="selected.Changes != null && formatRooms(selected.Rooms) != formatRooms(selected.Changes.Rooms)">
                    <span class='red--text'>{{formatRooms(selected.Rooms)}}: </span>
                    <span class='primary--text'>{{formatRooms(selected.Changes.Rooms)}}</span>
                  </template>
                  <template v-else>
                    {{formatRooms(selected.Rooms)}}
                  </template>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Check-in Requested</div>
                  <template v-if="selected.Changes != null && selected.Checkin != selected.Changes.Checkin">
                    <span class='red--text'>{{boolToYesNo(selected.Checkin)}}: </span>
                    <span class='primary--text'>{{boolToYesNo(selected.Changes.Checkin)}}</span>
                  </template>
                  <template v-else>
                    {{boolToYesNo(selected.Checkin)}}
                  </template>
                </v-col>
              </v-row>
            </template>
            <template v-if="selected.needsOnline">
              <v-row>
                <v-col>
                  <div class="floating-title">Event Link</div>
                  <template v-if="selected.Changes != null && selected.EventURL != selected.Changes.EventURL">
                    <span class='red--text'>{{selected.EventURL}}: </span>
                    <span class='primary--text'>{{selected.Changes.EventURL}}</span>
                  </template>
                  <template v-else>
                    {{selected.EventURL}}
                  </template>
                </v-col>
                <v-col v-if="selected.ZoomPassword != ''">
                  <div class="floating-title">Password</div>
                  <template v-if="selected.Changes != null && selected.ZoomPassword != selected.Changes.ZoomPassword">
                    <span class='red--text'>{{selected.ZoomPassword}}: </span>
                    <span class='primary--text'>{{selected.Changes.ZoomPassword}}</span>
                  </template>
                  <template v-else>
                    {{selected.ZoomPassword}}
                  </template>
                </v-col>
              </v-row>
            </template>
            <template v-if="selected.needsPub">
              <v-row v-for="(p, idx) in selected.Publicity" :key="`pub_${idx}`">
                <v-col>
                  <div class="floating-title">Publicity Date</div>
                  <template v-if="selected.Changes != null && p.Date != selected.Changes.Publicity[idx].Date">
                    <span class='red--text'>{{p.Date | formatDate}}: </span>
                    <span class='primary--text'>{{selected.Changes.Publicity[idx].Date | formatDate}}</span>
                  </template>
                  <template v-else>
                    {{p.Date | formatDate}}
                  </template>
                </v-col>
                <v-col>
                  <div class="floating-title">Publicity Need</div>
                  <template v-if="selected.Changes != null && p.Needs.join(', ') != selected.Changes.Publicity[idx].Needs.join(', ')">
                    <span class='red--text'>{{p.Needs.join(', ')}}: </span>
                    <span class='primary--text'>{{selected.Changes.Publicity[idx].Needs.join(', ')}}</span>
                  </template>
                  <template v-else>
                    {{p.Needs.join(', ')}}
                  </template>
                </v-col>
              </v-row>
              <v-row v-if="selected.PublicityBlurb">
                <v-col>
                  <div class="floating-title">Publicity Blurb</div>
                  <template v-if="selected.Changes != null && selected.PublicityBlurb != selected.Changes.PublicityBlurb">
                    <span class='red--text'>{{selected.PublicityBlurb}}: </span>
                    <span class='primary--text'>{{selected.Changes.PublicityBlurb}}</span>
                  </template>
                  <template v-else>
                    {{selected.PublicityBlurb}}
                  </template>
                </v-col>
              </v-row>
              <v-row v-if="selected.TalkingPointOne">
                <v-col>
                  <div class="floating-title">Talking Point One</div>
                  <template v-if="selected.Changes != null && selected.TalkingPointOne != selected.Changes.TalkingPointOne">
                    <span class='red--text'>{{selected.TalkingPointOne}}: </span>
                    <span class='primary--text'>{{selected.Changes.TalkingPointOne}}</span>
                  </template>
                  <template v-else>
                    {{selected.TalkingPointOne}}
                  </template>
                </v-col>
              </v-row>
              <v-row v-if="selected.TalkingPointTwo">
                <v-col>
                  <div class="floating-title">Talking Point Two</div>
                  <template v-if="selected.Changes != null && selected.TalkingPointTwo != selected.Changes.TalkingPointTwo">
                    <span class='red--text'>{{selected.TalkingPointTwo}}: </span>
                    <span class='primary--text'>{{selected.Changes.TalkingPointTwo}}</span>
                  </template>
                  <template v-else>
                    {{selected.TalkingPointTwo}}
                  </template>
                </v-col>
              </v-row>
              <v-row v-if="selected.TalkingPointThree">
                <v-col>
                  <div class="floating-title">Talking Point Three</div>
                  <template v-if="selected.Changes != null && selected.TalkingPointThree != selected.Changes.TalkingPointThree">
                    <span class='red--text'>{{selected.TalkingPointThree}}: </span>
                    <span class='primary--text'>{{selected.Changes.TalkingPointThree}}</span>
                  </template>
                  <template v-else>
                    {{selected.TalkingPointThree}}
                  </template>
                </v-col>
              </v-row>
              <v-row>
                <v-col v-if="selected.PubImage">
                  <div class="floating-title">Publicity Image</div>
                  {{selected.PubImage.name}}
                  <v-btn icon color="accent" @click="saveFile('pub')"
                    ><v-icon color="accent">mdi-download</v-icon></v-btn
                  >
                </v-col>
                <v-col>
                  <div class="floating-title">Add to public calendar</div>
                  <template v-if="selected.Changes != null && selected.ShowOnCalendar != selected.Changes.ShowOnCalendar">
                    <span class='red--text'>{{boolToYesNo(selected.ShowOnCalendar)}}: </span>
                    <span class='primary--text'>{{boolToYesNo(selected.Changes.ShowOnCalendar)}}</span>
                  </template>
                  <template v-else>
                    {{boolToYesNo(selected.ShowOnCalendar)}}
                  </template>
                </v-col>
              </v-row>
            </template>
            <template v-if="selected.needsChildCare">
              <v-row>
                <v-col>
                  <div class="floating-title">Childcare Age Groups</div>
                  <template v-if="selected.Changes != null && selected.ChildCareOptions.join(', ') != selected.Changes.ChildCareOptions.join(', ')">
                    <span class='red--text'>{{selected.ChildCareOptions.join(', ')}}: </span>
                    <span class='primary--text'>{{selected.Changes.ChildCareOptions.join(', ')}}</span>
                  </template>
                  <template v-else>
                    {{selected.ChildCareOptions.join(', ')}}
                  </template>
                </v-col>
                <v-col>
                  <div class="floating-title">Expected Number of Children</div>
                  <template v-if="selected.Changes != null && selected.EstimatedKids != selected.Changes.EstimatedKids">
                    <span class='red--text'>{{selected.EstimatedKids}}: </span>
                    <span class='primary--text'>{{selected.Changes.EstimatedKids}}</span>
                  </template>
                  <template v-else>
                    {{selected.EstimatedKids}}
                  </template>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Childcare Start Time</div>
                  <template v-if="selected.Changes != null && selected.CCStartTime != selected.Changes.CCStartTime">
                    <span class='red--text'>{{selected.CCStartTime}}: </span>
                    <span class='primary--text'>{{selected.Changes.CCStartTime}}</span>
                  </template>
                  <template v-else>
                    {{selected.CCStartTime}}
                  </template>
                </v-col>
                <v-col>
                  <div class="floating-title">Childcare End Time</div>
                  <template v-if="selected.Changes != null && selected.CCEndTime != selected.Changes.CCEndTime">
                    <span class='red--text'>{{selected.CCEndTime}}: </span>
                    <span class='primary--text'>{{selected.Changes.CCEndTime}}</span>
                  </template>
                  <template v-else>
                    {{selected.CCEndTime}}
                  </template>
                </v-col>
              </v-row>
            </template>
            <template v-if="selected.needsCatering">
              <v-row>
                <v-col>
                  <div class="floating-title">Preferred Vendor</div>
                  <template v-if="selected.Changes != null && selected.Vendor != selected.Changes.Vendor">
                    <span class='red--text'>{{selected.Vendor}}: </span>
                    <span class='primary--text'>{{selected.Changes.Vendor}}</span>
                  </template>
                  <template v-else>
                    {{selected.Vendor}}
                  </template>
                </v-col>
                <v-col>
                  <div class="floating-title">Budget Line</div>
                  <template v-if="selected.Changes != null && selected.BudgetLine != selected.Changes.BudgetLine">
                    <span class='red--text'>{{selected.BudgetLine}}: </span>
                    <span class='primary--text'>{{selected.Changes.BudgetLine}}</span>
                  </template>
                  <template v-else>
                    {{selected.BudgetLine}}
                  </template>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">Preferred Menu</div>
                  <template v-if="selected.Changes != null && selected.Menu != selected.Changes.Menu">
                    <span class='red--text'>{{selected.Menu}}: </span>
                    <span class='primary--text'>{{selected.Changes.Menu}}</span>
                  </template>
                  <template v-else>
                    {{selected.Menu}}
                  </template>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <div class="floating-title">{{foodTimeTitle}}</div>
                  <template v-if="selected.Changes != null && selected.FoodTime != selected.Changes.FoodTime">
                    <span class='red--text'>{{selected.FoodTime}}: </span>
                    <span class='primary--text'>{{selected.Changes.FoodTime}}</span>
                  </template>
                  <template v-else>
                    {{selected.FoodTime}}
                  </template>
                </v-col>
                <v-col v-if="selected.FoodDelivery">
                  <div class="floating-title">Food Drop off Location</div>
                  <template v-if="selected.Changes != null && selected.FoodDropOff != selected.Changes.FoodDropOff">
                    <span class='red--text'>{{selected.FoodDropOff}}: </span>
                    <span class='primary--text'>{{selected.Changes.FoodDropOff}}</span>
                  </template>
                  <template v-else>
                    {{selected.FoodDropOff}}
                  </template>
                </v-col>
              </v-row>
              <template v-if="selected.needsChildCare">
                <v-row>
                  <v-col>
                    <div class="floating-title">
                      Preferred Vendor for Childcare
                    </div>
                    <template v-if="selected.Changes != null && selected.CCVendor != selected.Changes.CCVendor">
                      <span class='red--text'>{{selected.CCVendor}}: </span>
                      <span class='primary--text'>{{selected.Changes.CCVendor}}</span>
                    </template>
                    <template v-else>
                      {{selected.CCVendor}}
                    </template>
                  </v-col>
                  <v-col>
                    <div class="floating-title">Budget Line for Childcare</div>
                    <template v-if="selected.Changes != null && selected.CCBudgetLine != selected.Changes.CCBudgetLine">
                      <span class='red--text'>{{selected.CCBudgetLine}}: </span>
                      <span class='primary--text'>{{selected.Changes.CCBudgetLine}}</span>
                    </template>
                    <template v-else>
                      {{selected.CCBudgetLine}}
                    </template>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <div class="floating-title">
                      Preferred Menu for Childcare
                    </div>
                    <template v-if="selected.Changes != null && selected.CCMenu != selected.Changes.CCMenu">
                      <span class='red--text'>{{selected.CCMenu}}: </span>
                      <span class='primary--text'>{{selected.Changes.CCMenu}}</span>
                    </template>
                    <template v-else>
                      {{selected.CCMenu}}
                    </template>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <div class="floating-title">ChildCare Food Set-up time</div>
                    <template v-if="selected.Changes != null && selected.CCFoodTime != selected.Changes.CCFoodTime">
                      <span class='red--text'>{{selected.CCFoodTime}}: </span>
                      <span class='primary--text'>{{selected.Changes.CCFoodTime}}</span>
                    </template>
                    <template v-else>
                      {{selected.CCFoodTime}}
                    </template>
                  </v-col>
                </v-row>
              </template>
            </template>
            <v-row v-if="selected.Drinks && selected.Drinks.length > 0">
              <v-col>
                <div class="floating-title">Desired Drinks</div>
                <template v-if="selected.Changes != null && selected.Drinks.join(', ') != selected.Changes.Drinks.join(', ')">
                  <span class='red--text'>{{selected.Drinks.join(', ')}}: </span>
                  <span class='primary--text'>{{selected.Changes.Drinks.join(', ')}}</span>
                </template>
                <template v-else>
                  {{selected.Drinks.join(', ')}}
                </template>
              </v-col>
            </v-row>
            <v-row v-if="selected.DrinkTime">
              <v-col>
                <div class="floating-title">Drink Set-up Time</div>
                <template v-if="selected.Changes != null && selected.DrinkTime != selected.Changes.DrinkTime">
                  <span class='red--text'>{{selected.DrinkTime}}: </span>
                  <span class='primary--text'>{{selected.Changes.DrinkTime}}</span>
                </template>
                <template v-else>
                  {{selected.DrinkTime}}
                </template>
              </v-col>
              <v-col>
                <div class="floating-title">Drink Drop off Location</div>
                <template v-if="selected.Changes != null && selected.DrinkDropOff != selected.Changes.DrinkDropOff">
                  <span class='red--text'>{{selected.DrinkDropOff}}: </span>
                  <span class='primary--text'>{{selected.Changes.DrinkDropOff}}</span>
                </template>
                <template v-else>
                  {{selected.DrinkDropOff}}
                </template>
              </v-col>
            </v-row>
            <template v-if="selected.needsAccom">
              <v-row v-if="selected.TechNeeds && selected.TechNeeds.length > 0">
                <v-col>
                  <div class="floating-title">Tech Needs</div>
                  <template v-if="selected.Changes != null && selected.TechNeeds.join(', ') != selected.Changes.TechNeeds.join(', ')">
                    <span class='red--text'>{{selected.TechNeeds.join(', ')}}: </span>
                    <span class='primary--text'>{{selected.Changes.TechNeeds.join(', ')}}</span>
                  </template>
                  <template v-else>
                    {{selected.TechNeeds.join(', ')}}
                  </template>
                </v-col>
              </v-row>
              <v-row v-if="selected.RegistrationDate">
                <v-col>
                  <div class="floating-title">Registration Date</div>
                  <template v-if="selected.Changes != null && selected.RegistrationDate != selected.Changes.RegistrationDate">
                    <span class='red--text'>{{selected.RegistrationDate | formatDate}}: </span>
                    <span class='primary--text'>{{selected.Changes.RegistrationDate | formatDate}}</span>
                  </template>
                  <template v-else>
                    {{selected.RegistrationDate | formatDate}}
                  </template>
                </v-col>
                <v-col v-if="selected.Fee">
                  <div class="floating-title">Registration Fee</div>
                  <template v-if="selected.Changes != null && selected.Fee != selected.Changes.Fee">
                    <span class='red--text'>{{selected.Fee | formatCurrency}}: </span>
                    <span class='primary--text'>{{selected.Changes.Fee | formatCurrency}}</span>
                  </template>
                  <template v-else>
                    {{selected.Fee | formatCurrency}}
                  </template>
                </v-col>
              </v-row>
              <v-row v-if="selected.RegistrationEndDate">
                <v-col>
                  <div class="floating-title">Registration Close Date</div>
                  <template v-if="selected.Changes != null && selected.RegistrationEndDate != selected.Changes.RegistrationEndDate">
                    <span class='red--text'>{{selected.RegistrationEndDate | formatDate}}: </span>
                    <span class='primary--text'>{{selected.Changes.RegistrationEndDate | formatDate}}</span>
                  </template>
                  <template v-else>
                    {{selected.RegistrationEndDate | formatDate}}
                  </template>
                </v-col>
                <v-col v-if="selected.RegistrationEndTime">
                  <div class="floating-title">Registration Close Time</div>
                  <template v-if="selected.Changes != null && selected.RegistrationEndTime != selected.Changes.RegistrationEndTime">
                    <span class='red--text'>{{selected.RegistrationEndTime}}: </span>
                    <span class='primary--text'>{{selected.Changes.RegistrationEndTime}}</span>
                  </template>
                  <template v-else>
                    {{selected.RegistrationEndTime}}
                  </template>
                </v-col>
              </v-row>
              <v-row v-if="selected.SetUp">
                <v-col>
                  <div class="floating-title">Requested Set-up</div>
                  <template v-if="selected.Changes != null && selected.SetUp != selected.Changes.SetUp">
                    <span class='red--text'>{{selected.SetUp}}: </span>
                    <span class='primary--text'>{{selected.Changes.SetUp}}</span>
                  </template>
                  <template v-else>
                    {{selected.SetUp}}
                  </template>
                </v-col>
              </v-row>
              <v-row v-if="selected.SetUpImage">
                <v-col>
                  <div class="floating-title">Set-up Image</div>
                  {{selected.SetUpImage.name}}
                  <v-btn icon color="accent" @click="saveFile('setup')">
                    <v-icon color="accent">mdi-download</v-icon>
                  </v-btn>
                </v-col>
              </v-row>
            </template>
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
      </v-overlay>
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
            <div>
              {{formatDates(selected.EventDates)}} <br/>
              {{selected.StartTime}} - {{selected.EndTime}}
            </div>
            <v-row>
              <v-col>
                <v-autocomplete
                  label="Set-up Buffer"
                  v-model="selected.MinsStartBuffer"
                  :items="[{text: '15 Mins', value:'15'}, {text: '30 Mins', value:'30'}, {text: '45 Mins', value:'45'}, {text: '1 Hour', value:'60'}]"
                  clearable
                  ></v-autocomplete>
                </v-col>
                <v-col>
                  <v-autocomplete
                  label="Tear-down Buffer"
                  v-model="selected.MinsEndBuffer"
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
                        let timeframe = [];
                        dates.forEach((d) => {
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
                        });
                        ordered.forEach((o) => {
                            if (timeframe.includes(o.Timeframe)) {
                                o.Events.push(i);
                            }
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
                                    rms.push(r.Value.split(` (`)[0]);
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
                editRequest() {
                    let url = $('[id$="hfRequestURL"]').val();
                    window.location = url + `?Id=${this.selected.Id}`;
                },
                openHistory() {
                    let url = $('[id$="hfHistoryURL"]').val();
                    window.location = url
                },
                saveFile(type) {
                    var a = document.createElement("a");
                    a.style = "display: none";
                    document.body.appendChild(a);
                    if (type == 'pub') {
                        a.href = this.selected.PubImage.data;
                        a.download = this.selected.PubImage.name;
                    } else if (type == 'setup') {
                        a.href = this.selected.SetUpImage.data;
                        a.download = this.selected.SetUpImage.name;
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
                    let raw = JSON.parse($('[id$="hfUpcomingRequests"]').val());
                    let temp = [];
                    raw.forEach((i) => {
                        let req = JSON.parse(i.Value);
                        req.Id = i.Id;
                        req.CreatedBy = i.CreatedBy;
                        req.CreatedOn = i.CreatedOn;
                        req.RequestStatus = i.RequestStatus;
                        temp.push(req);
                    });
                    this.existingRequests = temp;

                    let conflictingDates = [], conflictingRooms = []
                    let conflictingRequests = this.existingRequests.filter(r => {
                        let isConflictingRoom = false
                        let isConflictingDate = false
                        for (let i = 0; i < this.selected.Rooms.length; i++) {
                            if (r.Rooms.includes(this.selected.Rooms[i])) {
                                isConflictingRoom = true
                                let roomName = this.rooms.filter(room => {
                                    return room.Id == this.selected.Rooms[i]
                                })
                                if (roomName.length > 0) {
                                    roomName = roomName[0].Value.split(' (')[0]
                                }
                                if (!conflictingRooms.includes(roomName)) {
                                    conflictingRooms.push(roomName)
                                }
                            }
                        }
                        for (let i = 0; i < this.selected.EventDates.length; i++) {
                            if (r.EventDates.includes(this.selected.EventDates[i]) && r.Id != this.selected.Id) {
                                //Dates are the same, check they do not overlap with moment-range
                                let cd = r.EventDates.filter(ed => { return ed == this.selected.EventDates[i] })[0]
                                let cdStart = moment(`${cd} ${r.StartTime}`, `yyyy-MM-DD hh:mm A`)
                                if (r.MinsStartBuffer) {
                                    cdStart = cdStart.subtract(r.MinsStartBuffer, 'minute')
                                }
                                let cdEnd = moment(`${cd} ${r.EndTime}`, `yyyy-MM-DD hh:mm A`)
                                if (r.MinsStartBuffer) {
                                    cdEnd = cdEnd.add(r.MinsEndBuffer, 'minute')
                                }
                                let cRange = moment.range(cdStart, cdEnd)
                                let currStart = moment(`${this.selected.EventDates[i]} ${this.selected.StartTime}`, `yyyy-MM-DD hh:mm A`)
                                if (this.selected.MinsStartBuffer) {
                                    currStart = currStart.subtract(this.selected.MinsStartBuffer, 'minute')
                                }
                                let currEnd = moment(`${this.selected.EventDates[i]} ${this.selected.EndTime}`, `yyyy-MM-DD hh:mm A`)
                                if (this.selected.MinsEndBuffer) {
                                    currEnd = currEnd.add(this.selected.MinsEndBuffer, 'minute')
                                }
                                let current = moment.range(currStart, currEnd)
                                if (cRange.overlaps(current)) {
                                    isConflictingDate = true
                                    if (!conflictingDates.includes(this.selected.EventDates[i])) {
                                        conflictingDates.push(this.selected.EventDates[i])
                                    }
                                }
                            }
                        }
                        return isConflictingRoom && isConflictingDate
                    })
                    if (conflictingRequests.length > 0) {
                        return true
                    }
                    return false
                },
            },
        });
    });
</script>
<style>
  .theme--light.v-application {
    background: rgba(0, 0, 0, 0);
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