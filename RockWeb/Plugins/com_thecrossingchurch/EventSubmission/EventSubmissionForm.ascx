<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="EventSubmissionForm.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.EventSubmission.EventSubmissionForm"
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

<asp:HiddenField ID="hfRooms" runat="server" />
<asp:HiddenField ID="hfMinistries" runat="server" />
<asp:HiddenField ID="hfReservations" runat="server" />
<asp:HiddenField ID="hfRequest" runat="server" />
<asp:HiddenField ID="hfUpcomingRequests" runat="server" />
<asp:HiddenField ID="hfThisWeeksRequests" runat="server" />

<div id="app">
  <v-app>
    <div>
      <v-card v-if="panel == 0">
        <v-card-text>
          <v-alert v-if="canEdit == false" type="error">You are not able to make changes to this request because it has been {{request.Status}}.</v-alert>
          <v-alert v-if="canEdit && request.Status && request.Status != 'Submitted'" type="warning">Any changes made to this request will need to be approved.</v-alert>
          <v-layout>
            <h3>Let's Design Your Event</h3>
            <v-spacer></v-spacer>
            <v-menu 
              attach
              offset-x
              left
            >
              <template v-slot:activator="{ on, attrs }">
                <v-btn fab color="accent" v-on="on" v-bind="attrs">
                  <v-icon>mdi-help</v-icon>
                </v-btn>
              </template>
              <v-sheet min-width="200px" style="padding: 8px;">
                Need help? Click <a href="mailto:events@thecrossingchurch.com">here</a> to email the Events Director with a question about your event.
              </v-sheet>
            </v-menu>
          </v-layout>
          <strong><i>Check all that apply</i></strong>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsSpace"
                :label="`A physical space for an event`"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsOnline"
                :label="`An online event`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsOnline"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsPub"
                :label="`Publicity`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsPub"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsCatering"
                :label="`Food Request`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsCatering"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsChildCare"
                :label="`Childcare`"
                hint="Requests involving childcare must be made at least 30 days in advance"
                :persistent-hint="request.needsChildCare"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsAccom"
                :label="`Special Accommodations (tech, drinks, registration, extensive set-up, etc.)`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsAccom"
              ></v-switch>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="next">Next</v-btn>
        </v-card-actions>
      </v-card>
      <v-card v-if="panel == 1">
        <v-card-text>
          <v-alert v-if="canEdit == false" type="error">You are not able to make changes to this request because it has been {{request.Status}}.</v-alert>
          <v-alert v-if="canEdit && request.Status && request.Status != 'Submitted'" type="warning">Any changes made to this request will need to be approved.</v-alert>
          <v-form ref="form" v-model="formValid">
            <v-alert type="error" v-if="!isValid && triedSubmit">
              Please review your request and fix all errors
            </v-alert>
            <%-- Basic Request Information --%>
            <v-layout>
              <h3 class="primary--text">Basic Information</h3>
              <v-spacer></v-spacer> 
              <v-menu 
                attach
                offset-x
                left
              >
                <template v-slot:activator="{ on, attrs }">
                  <v-btn fab color="accent" v-on="on" v-bind="attrs">
                    <v-icon>mdi-help</v-icon>
                  </v-btn>
                </template>
                <v-sheet min-width="200px" style="padding: 8px;">
                  Need help? Click <a href="mailto:events@thecrossingchurch.com">here</a> to email the Events Director with a question about your event.
                </v-sheet>
              </v-menu>
            </v-layout>
            <v-row>
              <v-col>
                <v-text-field
                  label="What should we call your event?"
                  v-model="request.Name"
                  :rules="[rules.required(request.Name, 'Event Name')]"
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="6">
                <v-autocomplete
                  label="What ministry is sponsoring this event?"
                  :items="ministries"
                  item-text="Value"
                  item-value="Id"
                  attach
                  v-model="request.Ministry"
                  :rules="[rules.required(request.Ministry, 'Ministry')]"
                ></v-autocomplete>
              </v-col>
              <v-col cols="12" md="6">
                <v-text-field
                  label="Who is the ministry contact for this event?"
                  v-model="request.Contact"
                  :rules="[rules.required(request.Contact, 'Contact')]"
                ></v-text-field>
              </v-col>
            </v-row>
            <template
              v-if="request.needsSpace && !request.needsOnline && !request.needsPub && !request.needsCatering && !request.needsChildCare && !request.needsAccom"
            >
              <v-tabs v-model="tab">
                <v-tab>I have specific dates(s) and times</v-tab>
                <v-tab-item>
                  <v-row>
                    <v-col>
                      <strong>Please select the date(s) of your event</strong>
                    </v-col>
                  </v-row>
                  <v-row>
                    <v-col cols="12" md="6">
                      <v-date-picker
                        label="Event Dates"
                        v-model="request.EventDates"
                        multiple
                        class="elevation-1"
                        :min="earliestDate"
                      ></v-date-picker>
                    </v-col>
                    <v-col cols="12" md="6">
                      <v-select
                        label="Selected Dates"
                        chips
                        attach
                        multiple
                        :rules="[rules.requiredArr(request.EventDates, 'Event Date')]"
                        :items="longDates"
                        item-text="text"
                        item-value="val"
                        v-model="request.EventDates"
                      ></v-select>
                    </v-col>
                  </v-row>
                  <template
                    v-if="request.needsSpace || request.needsOnline || request.needsPub || request.needsChildCare"
                  >
                    <v-row>
                      <v-col>
                        <strong
                          >What time will your event begin and end?</strong
                        >
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col cols="12" md="6">
                        <strong>Start Time</strong>
                        <time-picker
                          v-model="request.StartTime"
                          :value="request.StartTime"
                          :rules="[rules.required(request.StartTime, 'Start Time'), rules.validTime(request.StartTime, request.EndTime, true)]"
                        ></time-picker>
                      </v-col>
                      <v-col cols="12" md="6">
                        <strong>End Time</strong>
                        <time-picker
                          v-model="request.EndTime"
                          :value="request.EndTime"
                          :rules="[rules.required(request.EndTime, 'End Time'), rules.validTime(request.EndTime, request.StartTime, false)]"
                        ></time-picker>
                      </v-col>
                    </v-row>
                  </template>
                  <%-- Space Information --%>
                  <template v-if="request.needsSpace">
                    <v-row>
                      <v-col>
                        <h3 class="primary--text">Space Information</h3>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col cols="12" md="6">
                        <v-text-field
                          label="How many people are you expecting to attend?"
                          type="number"
                          v-model="request.ExpectedAttendance"
                          :hint="`${request.ExpectedAttendance > 100 ? 'Events with more than 100 attendees must be approved by the city and requests must be submitted at least 30 days in advance' : ''}`"
                          :rules="[rules.required(request.ExpectedAttendance, 'Expected Attendance')]"
                        ></v-text-field>
                      </v-col>
                      <v-col cols="12" md="6">
                        <v-autocomplete
                          label="What are your preferred rooms/spaces?"
                          chips
                          multiple
                          :items="rooms"
                          item-text="Value"
                          item-value="Id"
                          v-model="request.Rooms"
                          attach
                          :rules="[rules.requiredArr(request.Rooms, 'Room/Space'), rules.roomCapacity(rooms, request.Rooms, request.ExpectedAttendance)]"
                        ></v-autocomplete>
                      </v-col>
                    </v-row>
                    <v-row v-if="request.needsOnline || request.needsPub || request.needsCatering || request.needsChildCare || request.needsAccom">
                      <v-col cols="12" md="6">
                        <v-switch
                          label="Do you need in-person check-in on the day of the event?"
                          v-model="request.Checkin"
                        ></v-switch>
                      </v-col>
                    </v-row>
                  </template>
                </v-tab-item>
                <v-tab>I want to search for something quick</v-tab>
                <v-tab-item>
                  <v-row v-if="tab == 1">
                    <v-col>
                      <room-picker
                        v-on:update="setDate"
                        :rules="rules"
                        :request="request"
                        ref="roompckr"
                      ></room-picker>
                    </v-col>
                  </v-row>
                </v-tab-item>
              </v-tabs>
            </template>
            <template v-else>
              <v-row>
                <v-col>
                  <strong>Please select the date(s) of your event</strong>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <v-date-picker
                    label="Event Dates"
                    v-model="request.EventDates"
                    multiple
                    class="elevation-1"
                    :min="earliestDate"
                    :rules="[rules.required(request.EventDates, 'Event Date')]"
                  ></v-date-picker>
                </v-col>
                <v-col cols="12" md="6">
                  <v-select
                    label="Selected Dates"
                    chips
                    attach
                    multiple
                    :rules="[rules.requiredArr(request.EventDates, 'Event Date')]"
                    :items="longDates"
                    item-text="text"
                    item-value="val"
                    v-model="request.EventDates"
                  ></v-select>
                </v-col>
              </v-row>
              <template
                v-if="request.needsSpace || request.needsOnline || request.needsPub || request.needsChildCare"
              >
                <v-row>
                  <v-col>
                    <strong>What time will your event begin and end?</strong>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12" md="6">
                    <strong>Start Time</strong>
                    <time-picker
                      v-model="request.StartTime"
                      :value="request.StartTime"
                      :rules="[rules.required(request.StartTime, 'Start Time'), rules.validTime(request.StartTime, request.EndTime, true)]"
                    ></time-picker>
                  </v-col>
                  <v-col cols="12" md="6">
                    <strong>End Time</strong>
                    <time-picker
                      v-model="request.EndTime"
                      :value="request.EndTime"
                      :rules="[rules.required(request.EndTime, 'End Time'), rules.validTime(request.EndTime, request.StartTime, false)]"
                    ></time-picker>
                  </v-col>
                </v-row>
                <%-- Space Information --%>
                <template v-if="request.needsSpace">
                  <v-row>
                    <v-col>
                      <h3 class="primary--text">Space Information</h3>
                    </v-col>
                  </v-row>
                  <v-row>
                    <v-col cols="12" md="6">
                      <v-text-field
                        label="How many people are you expecting to attend?"
                        type="number"
                        v-model="request.ExpectedAttendance"
                        :rules="[rules.required(request.ExpectedAttendance, 'Expected Attendance')]"
                        :hint="`${request.ExpectedAttendance > 100 ? 'Events with more than 100 attendees must be approved by the city and requests must be submitted at least 30 days in advance' : ''}`"
                      ></v-text-field>
                    </v-col>
                    <v-col cols="12" md="6">
                      <v-autocomplete
                        label="What are your preferred rooms/spaces?"
                        chips
                        multiple
                        :items="rooms"
                        item-text="Value"
                        item-value="Id"
                        v-model="request.Rooms"
                        attach
                        :rules="[rules.requiredArr(request.Rooms, 'Room/Space'), rules.roomCapacity(rooms, request.Rooms, request.ExpectedAttendance)]"
                      ></v-autocomplete>
                    </v-col>
                  </v-row>
                  <v-row v-if="request.needsOnline || request.needsPub || request.needsCatering || request.needsChildCare || request.needsAccom">
                    <v-col cols="12" md="6">
                      <v-switch
                        label="Do you need in-person check-in on the day of the event?"
                        v-model="request.Checkin"
                      ></v-switch>
                    </v-col>
                  </v-row>
                </template>
              </template>
            </template>
            <%-- Online Information --%>
            <template v-if="request.needsOnline">
              <v-row>
                <v-col>
                  <h3 class="primary--text">Online Information</h3>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-text-field
                    label="If there is a link your attendees will need to access this event, list it here"
                    v-model="request.EventURL"
                    :rules="[rules.required(request.EventURL, 'Link')]"
                  ></v-text-field>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-text-field
                    label="If there is a password for the link, list it here"
                    v-model="request.ZoomPassword"
                  ></v-text-field>
                </v-col>
              </v-row>
            </template>
            <%-- Publicity Information --%>
            <template v-if="request.needsPub">
              <v-row>
                <v-col>
                  <h3 class="primary--text">Publicity Information</h3>
                </v-col>
              </v-row>
              <template v-for="(i, idx) in request.Publicity">
                <v-row
                  ><v-col><strong>Week {{idx + 1}}</strong></v-col></v-row
                >
                <v-row :key="`pub_${idx}`" align-center>
                  <v-col>
                    <publicity-picker
                      :value="i"
                      :earliest-date="earliestPubDate"
                    ></publicity-picker>
                  </v-col>
                  <v-col cols="1">
                    <v-btn
                      fab
                      color="red"
                      v-if="idx > 0"
                      @click="removePub(idx)"
                      class="btn-pub-del"
                    >
                      <v-icon>mdi-delete-forever</v-icon>
                      <span class="tooltip">Delete Publicity</span>
                    </v-btn>
                  </v-col>
                </v-row>
              </template>
              <v-row>
                <v-col style="position: relative">
                  <v-btn
                    absolute
                    right
                    fab
                    color="accent"
                    :disabled="pubBtnDisabled"
                    @click="addPub"
                    class="btn-pub-add"
                  >
                    <v-icon>mdi-plus</v-icon>
                    <span class="tooltip">Add Publicity</span>
                  </v-btn>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-textarea
                    label="Please type out your blurb"
                    v-model="request.PublicityBlurb"
                    :rules="[rules.blurbValidation(request.PublicityBlurb, request.Publicity)]"
                    validate-on-blur
                  ></v-textarea>
                </v-col>
              </v-row>
              <template v-if="isRequestingVideo">
                <v-row>
                  <v-col>
                    <strong>If you are requesting a video announcement, please provide three talking points to guide your spoken announcement. These should be no longer than 20 seconds when read aloud at a normal speaking pace.</strong>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-textarea
                      label="Talking Point One"
                      v-model="request.TalkingPointOne"
                    ></v-textarea>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-textarea
                      label="Talking Point Two"
                      v-model="request.TalkingPointTwo"
                    ></v-textarea>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-textarea
                      label="Talking Point Three"
                      v-model="request.TalkingPointThree"
                    ></v-textarea>
                  </v-col>
                </v-row>
              </template>
              <v-row>
                <v-col cols="12" md="6">
                  <v-file-input
                    accept="image/*"
                    label="If your event has an existing graphic, please upload it here"
                    prepend-inner-icon="mdi-camera"
                    prepend-icon=""
                    v-model="pubImage"
                    @change="handlePubFile"
                  ></v-file-input>
                </v-col>
                <v-col cols="12" md="6">
                  <v-switch
                    :label="`I would like this event to be listed on the public web calendar (${boolToYesNo(request.ShowOnCalendar)})`"
                    v-model="request.ShowOnCalendar"
                  ></v-switch>
                </v-col>
              </v-row>
            </template>
            <%-- Catering Information --%>
            <template v-if="request.needsCatering">
              <v-row>
                <v-col>
                  <h3 class="primary--text">Catering Information</h3>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" style='font-weight: bold;'>
                  If you want to avoid a serving team, be sure to choose a vendor that is currently offering individually boxed meals. 
                  <v-menu attach>
                    <template v-slot:activator="{ on, attrs }">
                      <span v-bind="attrs" v-on="on" class='accent-text'>
                        Click here to see that list.
                      </span>
                    </template>
                    <v-list>
                      <v-list-item>
                        Chick-fil-A
                      </v-list-item>
                      <v-list-item>
                        Como Smoke and Fire
                      </v-list-item>
                      <v-list-item>
                        Honey Baked Ham
                      </v-list-item>
                      <v-list-item>
                        Panera
                      </v-list-item>
                      <v-list-item>
                        Pickleman's
                      </v-list-item>
                      <v-list-item>
                        Tropical Smoothie Cafe
                      </v-list-item>
                      <v-list-item>
                        Word of Mouth Catering
                      </v-list-item>
                    </v-list>
                  </v-menu>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-text-field
                    label="Preferred Vendor"
                    v-model="request.Vendor"
                    :rules="[rules.required(request.Vendor, 'Vendor')]"
                  ></v-text-field>
                </v-col>
                <v-col>
                  <v-text-field
                    label="Food Budget Line"
                    v-model="request.BudgetLine"
                    :rules="[rules.required(request.BudgetLine, 'Budget Line')]"
                  ></v-text-field>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-textarea
                    label="Preferred Menu"
                    v-model="request.Menu"
                    :rules="[rules.required(request.Menu, 'Menu')]"
                  ></v-textarea>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-switch
                    v-model="request.FoodDelivery"
                    :label="`Would you like your food to be delivered? ${request.FoodDelivery ? 'Yes!' : 'No, someone from my team will pick it up'}`"
                  ></v-switch>
                </v-col>
              </v-row>
              <v-row v-if="request.FoodDelivery">
                <v-col cols="12" md="6">
                  <strong
                    >What time would you like food to be set up and
                    ready?</strong
                  >
                  <time-picker
                    v-model="request.FoodTime"
                    :value="request.FoodTime"
                    :default="defaultFoodTime"
                    :rules="[rules.required(request.FoodTime, 'Time')]"
                  ></time-picker>
                </v-col>
                <v-col cols="12" md="6">
                  <br />
                  <v-row>
                    <v-col>
                      <v-text-field
                        label="Where should the food be set up?"
                        v-model="request.FoodDropOff"
                        :rules="[rules.required(request.FoodDropOff, 'Location')]"
                      ></v-text-field>
                    </v-col>
                  </v-row>
                </v-col>
              </v-row>
              <v-row v-else>
                <v-col cols="12" md="6">
                  <strong
                    >What time would you like to pick up your food?</strong
                  >
                  <time-picker
                    v-model="request.FoodTime"
                    :value="request.FoodTime"
                    :default="defaultFoodTime"
                    :rules="[rules.required(request.FoodTime, 'Time')]"
                  ></time-picker>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <br />
                  <v-row>
                    <v-col>
                      <v-autocomplete
                        label="What drinks would you like to have?"
                        :items="['Coffee', 'Soda', 'Water']"
                        :hint="`${request.Drinks.toString().includes('Coffee') ? 'Due to COVID-19, all drip coffee must be served by a designated person or team from the hosting ministry. This person must wear a mask and gloves and be the only person to touch the cups, sleeves, lids, and coffee carafe before the coffee is served to attendees. If you are not willing to provide this for your own event, please deselect the coffee option and opt for an individually packaged item like bottled water or soda.' : ''}`"
                        persistent-hint
                        v-model="request.Drinks"
                        multiple
                        chips
                        attach
                      ></v-autocomplete>
                    </v-col>
                  </v-row>
                </v-col>
                <v-col cols="12" md="6">
                  <strong>What time would you like your drinks to be delivered?</strong>
                  <time-picker
                    v-model="request.DrinkTime"
                    :value="request.DrinkTime"
                    :default="defaultFoodTime"
                  ></time-picker>
                </v-col>
              </v-row>
              <v-row v-if="request.Drinks.includes('Coffee')">
                <v-col cols="12" md="6">
                  <v-checkbox
                    label="I agree to provide a coffee serving team in compliance with COVID-19 policy."
                    :rules="[rules.required(request.ServingTeamAgree, 'Agreement to provide a serving team')]"
                    v-model="request.ServingTeamAgree"
                  ></v-checkbox>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <v-text-field
                    label="Where would you like your drinks delivered?"
                    v-model="request.DrinkDropOff"
                  ></v-text-field>
                </v-col>
                <v-col cols="12" md="6">
                  <v-checkbox
                    v-if="request.FoodDropOff != ''"
                    label="Set-up drinks with food"
                    v-model="sameFoodDrinkDropOff"
                    dense
                  ></v-checkbox>
                </v-col>
              </v-row>
              <%-- Childcare Catering --%>
              <template v-if="request.needsChildCare">
                <v-row>
                  <v-col>
                    <v-select
                      label="Preferred Vendor for Childcare"
                      v-model="request.CCVendor"
                      :rules="[rules.required(request.CCVendor, 'Vendor')]"
                      :items="['Pizza', 'Other']"
                      attach
                    ></v-select>
                  </v-col>
                  <v-col>
                    <v-text-field
                      label="Food Budget Line for Childcare"
                      v-model="request.CCBudgetLine"
                      :rules="[rules.required(request.CCBudgetLine, 'Budget Line')]"
                    ></v-text-field>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-textarea
                      label="Preferred Menu for Childcare"
                      v-model="request.CCMenu"
                      :rules="[rules.required(request.CCMenu, 'Menu')]"
                    ></v-textarea>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12" md="6">
                    <strong
                      >What time would you like food to be set up and ready for
                      childcare?</strong
                    >
                    <time-picker
                      v-model="request.CCFoodTime"
                      :value="request.CCFoodTime"
                      :default="defaultFoodTime"
                      :rules="[rules.required(request.CCFoodTime, 'Time')]"
                    ></time-picker>
                  </v-col>
                </v-row>
              </template>
            </template>
            <%-- Childcare Info --%>
            <template v-if="request.needsChildCare">
              <v-row>
                <v-col>
                  <h3 class="primary--text">Childcare Information</h3>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <strong>
                    What time do you need childcare to start?
                  </strong>
                  <time-picker
                    v-model="request.CCStartTime"
                    :value="request.CCStartTime"
                    :default="defaultFoodTime"
                    :rules="[rules.required(request.CCStartTime, 'Time')]"
                  ></time-picker>
                </v-col> 
                <v-col cols="12" md="6">
                  <strong>
                    What time will childcare end?
                  </strong>
                  <time-picker
                    v-model="request.CCEndTime"
                    :value="request.CCEndTime"
                    :default="request.EndTime"
                    :rules="[rules.required(request.CCEndTime, 'Time')]"
                  ></time-picker>
                </v-col> 
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <v-autocomplete
                    label="What ages of childcare do you want to offer?"
                    :items="['Infant/Toddler', 'Preschool', 'K-2nd', '3-5th']"
                    chips
                    multiple
                    attach
                    v-model="request.ChildCareOptions"
                  >
                    <template v-slot:item="data">
                      <div style="padding: 12px 0px; width: 100%">
                        <v-icon
                          v-if="request.ChildCareOptions.includes(data.item)"
                          color="primary"
                          style="margin-right: 32px"
                          >mdi-checkbox-marked</v-icon
                        >
                        <v-icon
                          v-else
                          color="primary"
                          style="margin-right: 32px"
                          >mdi-checkbox-blank-outline</v-icon
                        >
                        {{data.item}}
                      </div>
                    </template>
                    <template v-slot:append-item>
                      <v-list-item>
                        <div
                          class="hover"
                          style="padding: 12px 0px; width: 100%"
                          @click="toggleChildCareOptions"
                        >
                          <v-icon
                            v-if="childCareSelectAll"
                            color="primary"
                            style="margin-right: 32px"
                            >mdi-checkbox-marked</v-icon
                          >
                          <v-icon
                            v-else
                            color="primary"
                            style="margin-right: 32px"
                            >mdi-checkbox-blank-outline</v-icon
                          >
                          Select All
                        </div>
                      </v-list-item>
                    </template>
                  </v-autocomplete>
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field
                    label="Estimated number of kids"
                    type="number"
                    v-model="request.EstimatedKids"
                  ></v-text-field>
                </v-col>
              </v-row>
            </template>
            <%-- Special Accommodations Info --%>
            <template v-if="request.needsAccom">
              <v-row>
                <v-col>
                  <h3 class="primary--text">Other Accommodations</h3>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <v-autocomplete
                    label="What tech needs do you have?"
                    :items="['Handheld Mic', 'Wrap Around Mic', 'Special Lighting', 'Graphics/Video/Powerpoint', 'Worship Team', 'Stage Set-Up', 'Basic Live Stream', 'Advanced Live Stream', 'Pipe and Drape', 'BOSE System']"
                    v-model="request.TechNeeds"
                    :hint="`${request.TechNeeds.toString().includes('Live Stream') ? 'Keep in mind that all live stream requests will come at an additional charge to the ministry, which will be verified with you in your follow-up email with the Events Director.' : ''}`"
                    persistent-hint
                    multiple
                    chips
                    attach
                  ></v-autocomplete>
                </v-col>
              </v-row>
              <template v-if="!request.needsCatering">
                <v-row>
                  <v-col cols="12" md="6">
                    <br />
                    <v-row>
                      <v-col>
                        <v-autocomplete
                          label="What drinks would you like to have?"
                          :items="['Coffee', 'Soda', 'Water']"
                          :hint="`${request.Drinks.toString().includes('Coffee') ? 'Due to COVID-19, all drip coffee must be served by a designated person or team from the hosting ministry. This person must wear a mask and gloves and be the only person to touch the cups, sleeves, lids, and coffee carafe before the coffee is served to attendees. If you are not willing to provide this for your own event, please deselect the coffee option and opt for an individually packaged item like bottled water or soda.' : ''}`"
                          persistent-hint
                          v-model="request.Drinks"
                          multiple
                          chips
                          attach
                        ></v-autocomplete>
                      </v-col>
                    </v-row>
                  </v-col>
                  <v-col cols="12" md="6">
                    <strong>What time would you like your drinks to be delivered?</strong>
                    <time-picker
                      v-model="request.DrinkTime"
                      :value="request.DrinkTime"
                      :default="defaultFoodTime"
                    ></time-picker>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12" md="6" v-if="request.Drinks.includes('Coffee')">
                    <v-checkbox
                      label="I agree to provide a coffee serving team in compliance with COVID-19 policy."
                      :rules="[rules.required(request.ServingTeamAgree, 'Agreement to provide a serving team')]"
                      v-model="request.ServingTeamAgree"
                    ></v-checkbox>
                  </v-col>
                  <v-col cols="12" md="6">
                    <v-text-field
                      label="Where would you like your drinks delivered?"
                      v-model="request.DrinkDropOff"
                    ></v-text-field>
                  </v-col>
                </v-row>
              </template>
              <v-row>
                <v-col cols="12" md="6">
                  <v-menu
                    v-model="menu"
                    :close-on-content-click="false"
                    :nudge-right="40"
                    transition="scale-transition"
                    offset-y
                    min-width="290px"
                    attach
                  >
                    <template v-slot:activator="{ on, attrs }">
                      <v-text-field
                        v-model="request.RegistrationDate"
                        label="What date do you need the registration link to be ready and live?"
                        prepend-inner-icon="mdi-calendar"
                        readonly
                        v-bind="attrs"
                        v-on="on"
                        clearable
                      ></v-text-field>
                    </template>
                    <v-date-picker
                      v-model="request.RegistrationDate"
                      @input="menu = false"
                      :min="earliestPubDate"
                    ></v-date-picker>
                  </v-menu>
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field
                    label="If there is a registration fee for this event, how much is it?"
                    type="number"
                    prepend-inner-icon="mdi-currency-usd"
                    v-model="request.Fee"
                  ></v-text-field>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <br/>
                  <v-row>
                    <v-col>
                      <v-menu
                        v-model="menu2"
                        :close-on-content-click="false"
                        :nudge-right="40"
                        transition="scale-transition"
                        offset-y
                        min-width="290px"
                        attach
                      >
                        <template v-slot:activator="{ on, attrs }">
                          <v-text-field
                            v-model="request.RegistrationEndDate"
                            label="What date should registration close?"
                            prepend-inner-icon="mdi-calendar"
                            readonly
                            v-bind="attrs"
                            v-on="on"
                            :rules="[rules.registrationCloseDate(request.EventDates, request.RegistrationEndDate, request.needsChildCare)]"
                            clearable
                          ></v-text-field>
                        </template>
                        <v-date-picker
                          v-model="request.RegistrationEndDate"
                          @input="menu2 = false"
                          :min="earliestPubDate"
                        ></v-date-picker>
                      </v-menu>
                    </v-col>
                  </v-row>
                </v-col>
                <v-col cols="12" md="6">
                  <strong>What time should registration close?</strong>
                  <time-picker
                    v-model="request.RegistrationEndTime"
                    :value="request.RegistrationEndTime"
                    :default="request.StartTime"
                    :rules="[rules.registrationCloseTime(request.EventDates, request.RegistrationEndDate, request.needsChildCare, request.StartTime, request.EndTime, request.RegistrationEndTime)]"
                  ></time-picker>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12">
                  <v-textarea
                    label="Please describe the set-up you require for your event"
                    v-model="request.SetUp"
                  ></v-textarea>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <v-file-input
                    accept="image/*"
                    label="If you have an image of the set-up layout you would like upload it here"
                    prepend-inner-icon="mdi-camera"
                    prepend-icon=""
                    v-model="setupImage"
                    @change="handleSetUpFile"
                  ></v-file-input>
                </v-col>
              </v-row>
            </template>
            <%-- Notes --%>
            <v-row>
              <v-col>
                <h3 class="primary--text">Additional Info</h3>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-textarea
                  label="Is there anything else we should know about this request?"
                  v-model="request.Notes"
                ></v-textarea>
              </v-col>
            </v-row>
          </v-form>
        </v-card-text>
        <v-card-actions>
          <v-btn color="secondary" @click="prev">Back</v-btn>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="next" v-if="canEdit">{{( isExistingRequest ? 'Update' : 'Submit')}}</v-btn>
          <Rock:BootstrapButton
            runat="server"
            ID="btnSubmit"
            CssClass="btn-hidden"
            OnClick="Submit_Click"
          />
        </v-card-actions>
      </v-card>
      <v-card v-if="panel == 2">
        <v-card-text>
          <v-alert v-if="isExistingRequest" type="success">
            This request has been updated
          </v-alert>
          <v-alert v-else type="success">
            Your request has been submitted! You will receive a confirmation
            email now with the details of your request, when it has been
            approved by the Events Director you will receive an email securing
            your reservation with any additional information from the Events
            Director
          </v-alert>
        </v-card-text>
      </v-card>
      <v-dialog 
        v-if="dialog" 
        v-model="dialog" 
        max-width="850px"
      >
        <v-card>
          <v-card-title></v-card-title>
          <v-card-text>
            <v-alert type="warning">
              <template v-if="conflictingRequestMsg != ''">
                {{conflictingRequestMsg}}
              </template>
              <br v-if="beforeHoursMsg != '' || afterHoursMsg != ''" />
              <template v-if="beforeHoursMsg != ''">
                {{beforeHoursMsg}}
              </template>
              <br v-if="beforeHoursMsg != '' && afterHoursMsg != ''" />
              <template v-if="afterHoursMsg != ''">
                {{afterHoursMsg}}
              </template>
            </v-alert>
            <br/>
            <v-row>
              <v-col>
                If you wish to submit your request despite these warnings click "Submit", otherwise click "Cancel" to modify your request. 
              </v-col>
            </v-row>
          </v-card-text>
          <v-card-actions>
            <v-btn color="secondary" @click="dialog = false;">Cancel</v-btn>
            <v-spacer></v-spacer>
            <v-btn color="primary" @click="submit">Submit</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </v-app>
</div>
<script>
    document.addEventListener("DOMContentLoaded", function () {
        Vue.component("publicity-picker", {
            template: `
                <v-row>
                    <v-col cols="12" md="6">
                        <v-menu
                        v-model="menu"
                          :close-on-content-click="false"
                          :nudge-right="40"
                          transition="scale-transition"
                          offset-y
                          min-width="290px"
                          attach
                        >
                        <template v-slot:activator="{ on, attrs }">
                            <v-text-field
                              v-model="value.Date"
                              label="Publicity Date"
                              prepend-inner-icon="mdi-calendar"
                              readonly
                              v-bind="attrs"
                              v-on="on"
                            ></v-text-field>
                        </template>
                        <v-date-picker
                            v-model="value.Date"
                            @input="menu = false"
                            :min="earliestDate"
                            :allowed-dates="allowedDates"
                        ></v-date-picker>
                        </v-menu>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-autocomplete
                            label="Publicity Needs"
                            v-model="value.Needs"
                            :items="['Video/Stage Announcement', 'Digital Worship Folder']"
                            multiple
                            chips
                            attach
                        ></v-autocomplete>
                    </v-col>
                </v-row>`,
            props: ["value", "earliestDate"],
            data: function () {
                return {
                    menu: false,
                };
            },
            methods: {
                allowedDates(val) {
                    let dow = moment(val).day();
                    return dow == 0;
                },
            },
        });
        Vue.component("time-picker", {
            template: `
            <v-row>
                <v-col>
                    <v-select label="Hour" :items="hours" v-model="hour" attach :error-messages="errorMessage" clearable></v-select>
                </v-col>
                <v-col>
                    <v-select label="Minute" :items="mins" v-model="minute" attach required clearable></v-select>
                </v-col>
                <v-col>
                    <v-select label="AM/PM" :items="aps" v-model="ap" attach required clearable></v-select>
                </v-col>
            </v-row>
        `,
            props: ["value", "default", "rules"],
            data: function () {
                return {
                    hour: null,
                    minute: "00",
                    ap: null,
                    hours: [
                        "01",
                        "02",
                        "03",
                        "04",
                        "05",
                        "06",
                        "07",
                        "08",
                        "09",
                        "10",
                        "11",
                        "12",
                    ],
                    mins: ["00", "15", "30", "45"],
                    aps: ["AM", "PM"],
                    originalValue: "",
                    errorMessage: "",
                };
            },
            created: function () {
                this.originalValue = this.value;
                if (this.value) {
                    this.hour = this.value.split(":")[0];
                    this.minute = this.value.split(":")[1].split(" ")[0];
                    this.ap = this.value.split(" ")[1];
                } else {
                    if (this.default) {
                        this.hour = this.default.split(":")[0];
                        this.minute = this.default.split(":")[1].split(" ")[0];
                        this.ap = this.default.split(" ")[1];
                    }
                }
            },
            computed: {
                time() {
                    if (`${this.hour}:${this.minute} ${this.ap}` == 'null:null null') {
                        return ''
                    }
                    return `${this.hour}:${this.minute} ${this.ap}`;
                },
            },
            watch: {
                time(val) {
                    this.$emit("input", val);
                },
                default(val) {
                    if (!this.originalValue) {
                        this.hour = val.split(":")[0];
                        this.minute = val.split(":")[1].split(" ")[0];
                        this.ap = val.split(" ")[1];
                    }
                },
                value(val) {
                    if (val) {
                        this.hour = val.split(":")[0];
                        this.minute = val.split(":")[1].split(" ")[0];
                        this.ap = val.split(" ")[1];
                    }
                },
                rules(val) {
                    let allTrue = true;
                    val.forEach((i) => {
                        if (i != true) {
                            this.errorMessage = i;
                            allTrue = false;
                        }
                    });
                    if (allTrue) {
                        this.errorMessage = "";
                    }
                },
            },
        });
        Vue.component("room-picker", {
            template: `
              <v-form ref="roomform" v-model="valid">
                <v-row>
                  <v-col>
                    <v-autocomplete
                      label="Select a Room/Space to view availability"
                      v-model="selected"
                      :items="rooms"
                      item-value="Id"
                      item-text="Value"
                      attach
                      :rules="[rules.required(selected, 'Room/Space')]"
                      :value="request.Rooms"
                    ></v-autocomplete>  
                  </v-col>  
                </v-row>
                <template v-if="page == 0">
                  <v-sheet height="600">
                    <v-calendar
                      ref="calendar"
                      :now="today"
                      :value="today"
                      :events="events"
                      color="primary"
                      type="week"
                      @click:time="calendarClick"
                      :weekdays="weekdays"
                    ></v-calendar>
                  </v-sheet>
                </template>
                <template v-else>
                  <h4>{{ formatDate(eventDate) }}</h4>
                  <v-row>
                    <v-col>
                      <strong>What time will your event begin and end?</strong>
                    </v-col>
                  </v-row>
                  <v-row>
                    <v-col cols="12" md="6">
                      <strong>Start Time</strong>
                      <time-picker
                        v-model="startTime"
                        :value="startTime"
                        :rules="[rules.required(startTime, 'Start Time'), rules.validTime(startTime, endTime, true)]"
                      ></time-picker>
                    </v-col>
                    <v-col cols="12" md="6">
                      <strong>End Time</strong>
                      <time-picker
                        v-model="endTime"
                        :value="endTime"
                        :rules="[rules.required(endTime, 'End Time'), rules.validTime(endTime, startTime, false)]"
                      ></time-picker>
                    </v-col>
                  </v-row>
                  <v-row>
                    <v-col>
                      <v-text-field
                        label="How many people are you expecting to attend?"
                        type="number"
                        v-model="att"
                        :value="request.ExpectedAttendance"
                        :rules="[rules.required(att, 'Expected Attendance'), rules.exceedsSelected(att, selected, rooms)]"
                      ></v-text-field>
                    </v-col>  
                  </v-row>
                  <v-row>
                    <v-col>
                      <v-btn color='accent' @click="page=0">back</v-btn>  
                    </v-col>  
                  </v-row>
                </template>
              </v-form>
            `,
            props: ["rules", "request"],
            data: function () {
                return {
                    today: moment().format("yyyy-MM-DD"),
                    valid: true,
                    allEvents: [],
                    page: 0,
                    selected: null,
                    rooms: [],
                    eventDate: "",
                    startTime: "",
                    endTime: "",
                    att: "",
                };
            },
            mounted: function () {
                this.$refs.calendar.scrollToTime("08:00");
            },
            created() {
                this.allEvents = [];
                this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
                let rawEvents = JSON.parse($('[id$="hfThisWeeksRequests"]')[0].value);
                let oneWeek = moment().add(6, 'days')
                for (i = 0; i < rawEvents.length; i++) {
                    let event = JSON.parse(rawEvents[i])
                    for (k = 0; k < event.EventDates.length; k++) {
                        let inRange = moment(event.EventDates[k]).isBetween(moment(), oneWeek, 'days', '[]')
                        if (inRange) {
                            this.allEvents.push({
                                name: event.Name,
                                start: moment(`${event.EventDates[k]} ${event.StartTime}`).format("yyyy-MM-DD HH:mm"),
                                end: moment(`${event.EventDates[k]} ${event.EndTime}`).format("yyyy-MM-DD HH:mm"),
                                loc: event.Rooms,
                            });
                        }
                    }
                }
            },
            computed: {
                weekdays() {
                    let dow = moment().day();
                    let arr = [];
                    for (i = dow; i < 7; i++) {
                        arr.push(i);
                    }
                    for (i = 0; i < dow; i++) {
                        arr.push(i);
                    }
                    return arr;
                },
                events() {
                    if (this.selected) {
                        return this.allEvents.filter((i) => {
                            return i.loc.includes(this.selected);
                        });
                    } else {
                        return [];
                    }
                },
            },
            methods: {
                calendarClick(val) {
                    this.eventDate = val.date;
                    let hour = val.hour;
                    let min = val.minute;
                    let apm = "AM";
                    if (hour >= 12) {
                        if (hour > 12) {
                            hour -= 12;
                        }
                        apm = "PM";
                    }
                    if (hour.toString().length < 2) {
                        hour = "0" + hour;
                    }
                    if (min < 15) {
                        min = "00";
                    } else if (min < 30) {
                        min = "15";
                    } else if (min < 45) {
                        min = "30";
                    } else {
                        min = "45";
                    }
                    this.startTime = hour + ":" + min + " " + apm;
                    this.page = 1;
                },
                emitChanges() {
                    this.$emit("update", {
                        eventDate: this.eventDate,
                        startTime: this.startTime,
                        endTime: this.endTime,
                        room: this.selected,
                        att: this.att,
                    });
                },
                formatDate(val) {
                    return moment(val).format("dddd, MMMM Do yyyy");
                },
            },
            watch: {
                selected(val) {
                    this.emitChanges();
                },
                startTime(val) {
                    this.emitChanges();
                },
                endTime(val) {
                    this.emitChanges();
                },
                att(val) {
                    this.emitChanges();
                },
                request: {
                    handler(val) { },
                    deep: true,
                },
            },
        });
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
                panel: 0,
                request: {
                    needsSpace: false,
                    needsOnline: false,
                    needsPub: false,
                    needsCatering: false,
                    needsChildCare: false,
                    needsAccom: false,
                    Name: "",
                    Ministry: "",
                    Contact: "",
                    EventDates: [],
                    StartTime: "",
                    EndTime: "",
                    MinsStartBuffer: 0,
                    MinsEndBuffer: 0,
                    ExpectedAttendance: "",
                    Rooms: "",
                    Checkin: false,
                    EventURL: "",
                    ZoomPassword: "",
                    Publicity: [
                        { Date: "", Needs: "" },
                        { Date: "", Needs: "" },
                        { Date: "", Needs: "" },
                    ],
                    PublicityBlurb: "",
                    TalkingPointOne: "",
                    TalkingPointTwo: "",
                    TalkingPointThree: "",
                    PubImage: null,
                    ShowOnCalendar: false,
                    Vendor: "",
                    Menu: "",
                    FoodDelivery: true,
                    FoodTime: "",
                    FoodDropOff: "",
                    Drinks: "",
                    DinkTime: "",
                    ServingTeamAgree: false,
                    DrinkDropOff: "",
                    BudgetLine: "",
                    CCVendor: "",
                    CCMenu: "",
                    CCFoodTime: "",
                    CCBudgetLine: "",
                    ChildCareOptions: "",
                    EstimatedKids: null,
                    CCStartTime: '',
                    CCEndTime: '',
                    TechNeeds: "",
                    RegistrationDate: "",
                    RegistrationEndDate: "",
                    RegistrationEndTime: "",
                    Fee: null,
                    SetUp: "",
                    SetUpImage: null,
                    Notes: "",
                },
                existingRequests: [],
                pubImage: {},
                setupImage: {},
                rooms: [],
                ministries: [],
                menu: false,
                menu2: false,
                sameFoodDrinkDropOff: false,
                rules: {
                    required(val, field) {
                        return !!val || `${field} is required`;
                    },
                    requiredArr(val, field) {
                        return val.length > 0 || `${field} is required`;
                    },
                    exceedsSelected(val, selected, rooms) {
                        let room = rooms.filter((i) => {
                            return i.Id == selected;
                        })[0].Value;
                        let info = room.replace("(", "").replace(")", "").split(" ");
                        let cap = parseInt(info[info.length - 1]);
                        if (val > cap) {
                            return `You cannot have more than ${cap} ${cap == 1 ? "person" : "people"
                                } in the selected space`;
                        }
                        return true;
                    },
                    validTime(val, compareVal, isStart) {
                        if (
                            !!val &&
                            !!compareVal &&
                            !val.includes("null") &&
                            !compareVal.includes("null")
                        ) {
                            let startTime = isStart ? val : compareVal;
                            let endTime = isStart ? compareVal : val;
                            let momentStart = moment(startTime, "hh:mm A");
                            let momentEnd = moment(endTime, "hh:mm A");
                            let isAfter = momentEnd.isAfter(momentStart);
                            return (
                                isAfter ||
                                `${isStart
                                    ? "Start time must come before end time"
                                    : "End time must come after start time"
                                }`
                            );
                        } else if (val.includes("null")) {
                            return "Please fill out all time information";
                        }
                        return true;
                    },
                    blurbValidation(value, pubDates) {
                        let pubDate = pubDates[0].Date;
                        pubDates.forEach((i) => {
                            let isAfter = moment(pubDate).isAfter(moment(i.Date));
                            if (isAfter) {
                                pubDate = i.Date;
                            }
                        });
                        let daysUntil = moment(pubDate).diff(moment(), "days");
                        if (daysUntil <= 30) {
                            return (
                                value.length >= 100 ||
                                "It doesn't look like you've entered a complete blurb, please enter the full blurb you wish to appear in publicity"
                            );
                        } else {
                            return true;
                        }
                    },
                    roomCapacity(allRooms, rooms, attendance) {
                        if (attendance) {
                            let selectedRooms = allRooms.filter((r) => {
                                return rooms.includes(r.Id);
                            });
                            let maxCapacity = 0;
                            selectedRooms.forEach((r) => {
                                let roomCap = r.Value.split("(")[1].replace(")", "");
                                maxCapacity += parseInt(roomCap);
                            });
                            if (attendance <= maxCapacity) {
                                return true;
                            } else {
                                return `This selection of rooms alone can only support a maximum capacity of ${maxCapacity}. Please select more rooms for increased capacity or lower your expected attendance.`;
                            }
                        }
                        return true;
                    },
                    registrationCloseDate(eventDates, closeDate, needsChildCare) {
                        let dates = eventDates.map(d => moment(d))
                        let minDate = moment.min(dates)
                        if (needsChildCare) {
                            minDate = minDate.subtract(1, "day")
                        }
                        if (moment(closeDate).isAfter(minDate)) {
                            if (needsChildCare) {
                                return 'When requesting childcare, registration must close 24 hours before the start of your event'
                            }
                            return 'Registration cannot end after your event'
                        }
                        return true
                    },
                    registrationCloseTime(eventDates, closeDate, needsChildCare, startTime, endtime, closeTime) {
                        let dates = eventDates.map(d => moment(d))
                        let minDate = moment.min(dates)
                        let actualDate = moment(`${closeDate} ${closeTime}`)
                        if (needsChildCare) {
                            minDate = minDate.subtract(1, "day")
                            minDate = moment(`${minDate.format('yyyy-MM-DD')} ${startTime}`)
                        } else {
                            minDate = moment(`${minDate.format('yyyy-MM-DD')} ${endtime}`)
                        }
                        if (moment(actualDate).isAfter(minDate)) {
                            if (needsChildCare) {
                                return 'When requesting childcare, registration must close 24 hours before the start of your event'
                            }
                            return 'Registration cannot end after your event'
                        }
                        return true
                    }
                },
                valid: true,
                formValid: true,
                dialog: false,
                conflictingRequestMsg: "",
                beforeHoursMsg: "",
                afterHoursMsg: "",
                triedSubmit: false,
                childCareSelectAll: false,
                tab: 0,
            },
            created() {
                this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
                this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value);
                let req = $('[id$="hfRequest"]')[0].value;
                if (req) {
                    let parsed = JSON.parse(req)
                    this.request = JSON.parse(parsed.Value)
                    this.request.Id = parsed.Id
                    this.request.Status = parsed.RequestStatus
                    this.request.CreatedBy = parsed.CreatedBy
                    this.request.canEdit = parsed.CanEdit
                    if (this.request.PubImage) {
                        this.pubImage = this.request.PubImage;
                    }
                    if (this.request.SetUpImage) {
                        this.setupImage = this.request.SetUpImage;
                    }
                }
                window["moment-range"].extendMoment(moment);
            },
            mounted() {
                let query = new URLSearchParams(window.location.search);
                let success = query.get('ShowSuccess');
                if (success) {
                    if (success == "true") {
                        this.panel = 2;
                        let id = query.get('Id');
                        if (id) {
                            window.history.go(-2)
                        }
                    }
                }
            },
            computed: {
                earliestDate() {
                    let eDate = new moment();
                    if (
                        this.request.needsOnline ||
                        this.request.needsPub ||
                        this.request.needsCatering ||
                        this.request.needsAccom
                    ) {
                        eDate = moment(eDate).add(14, "days");
                    }
                    if (
                        this.request.needsChildCare ||
                        this.request.ExpectedAttendance > 100
                    ) {
                        eDate = moment().add(30, "days");
                        this.request.EventDates.forEach((itm, i) => {
                            if (
                                !moment(itm).isSameOrAfter(moment(eDate).format("yyyy-MM-DD"))
                            ) {
                                this.request.EventDates.splice(i, 1);
                            }
                        });
                    }
                    return moment(eDate).format("yyyy-MM-DD");
                },
                earliestPubDate() {
                    let eDate = new moment();
                    eDate = moment(eDate).add(14, "days");
                    return moment(eDate).format("yyyy-MM-DD");
                },
                availableRooms() {
                    let ar = [];
                    this.rooms.forEach((i) => {
                        let info = i.Value.replace("(", "").replace(")", "").split(" ");
                        let cap = parseInt(info[info.length - 1]);
                        if (parseInt(this.request.ExpectedAttendance) <= cap) {
                            ar.push(i);
                        }
                    });
                    return ar;
                },
                pubBtnDisabled() {
                    if (this.request.Publicity.length > 3) {
                        return true;
                    }
                    return false;
                },
                defaultFoodTime() {
                    if (this.request.StartTime) {
                        let time = moment(this.request.StartTime, "hh:mm A");
                        return time.subtract(30, "minutes").format("hh:mm A");
                    }
                    return null;
                },
                defaultRegistraionStart() {
                    if (this.request.needsAccom) {
                        if (this.request.RegistrationDate) {
                            return this.request.RegistrationDate
                        }
                        if (this.request.Publicity) {
                            let pubDates = this.request.Publicity.filter(p => { return p.Date != '' }).map(p => moment(p.Date))
                            let firstDate = moment.min(pubDates)
                            return firstDate.subtract(3, 'days').format("yyyy-MM-DD")
                        }
                    }
                    return ""
                },
                defaultRegistraionEnd() {
                    if (this.request.needsAccom) {
                        if (this.request.RegistrationEndDate) {
                            return this.request.RegistrationEndDate
                        }
                        if (this.request.EventDates) {
                            let dates = this.request.EventDates.map(d => moment(d))
                            return moment.min(dates).subtract(1, "day").format("yyyy-MM-DD")
                        }
                    }
                    return ""
                },
                longDates() {
                    return this.request.EventDates.map((i) => {
                        return { text: moment(i).format("dddd, MMMM Do yyyy"), val: i };
                    });
                },
                isValid() {
                    if (this.$refs.roompckr && this.tab == 1) {
                        return this.valid && this.formValid && this.$refs.roompckr.valid;
                    }
                    return this.valid && this.formValid;
                },
                isExistingRequest() {
                    let urlParams = new URLSearchParams(window.location.search);
                    let id = urlParams.get('Id');
                    if (id) {
                        return true
                    }
                    return false
                },
                isRequestingVideo() {
                    if (this.request) {
                        let isRequesting = false
                        this.request.Publicity.forEach(p => {
                            if (p.Needs.includes('Video/Stage Announcement')) {
                                isRequesting = true
                            }
                        })
                        return isRequesting
                    }
                    return false
                },
                canEdit() {
                    if (this.request.canEdit != null) {
                        return this.request.canEdit
                    }
                    return true
                }
            },
            methods: {
                boolToYesNo(val) {
                    if (val) {
                        return "Yes";
                    }
                    return "No";
                },
                next() {
                    if (this.panel == 1) {
                        this.validate();
                        if (this.isValid) {
                            this.submit()
                        } else {
                            let formIsValid = this.formValid
                            if (this.$refs.roompckr && this.tab == 1) {
                                formIsValid = this.formValid && this.$refs.roompckr.valid;
                            }
                            if (formIsValid) {
                                this.dialog = true
                            }
                        }
                    } else {
                        this.panel += 1;
                    }
                    window.scrollTo(0, 0);
                },
                prev() {
                    let tab = this.panel;
                    tab -= 1;
                    if (tab < 0) {
                        tab = 0;
                    }
                    this.panel = tab;
                    window.scrollTo(0, 0);
                },
                submit() {
                    this.request.Publicity = this.request.Publicity.filter(pub => {
                        return pub.Date != '' && pub.Needs != ''
                    })
                    console.log(this.request)
                    $('[id$="hfRequest"]').val(JSON.stringify(this.request));
                    $('[id$="btnSubmit"')[0].click();
                },
                addPub() {
                    this.request.Publicity.push({ Date: "", Needs: "" });
                },
                removePub(idx) {
                    this.request.Publicity.splice(idx, 1);
                },
                setDate(val) {
                    this.request.Rooms = [val.room];
                    this.request.StartTime = val.startTime;
                    this.request.EndTime = val.endTime;
                    this.request.EventDates = [val.eventDate];
                    this.request.ExpectedAttendance = val.att;
                },
                handlePubFile(e) {
                    let file = { name: e.name, type: e.type };
                    var reader = new FileReader();
                    const self = this;
                    reader.onload = function (e) {
                        console.log(e.target.result);
                        file.data = e.target.result;
                        self.request.PubImage = file;
                    };
                    reader.readAsDataURL(e);
                },
                handleSetUpFile(e) {
                    let file = { name: e.name, type: e.type };
                    var reader = new FileReader();
                    const self = this;
                    reader.onload = function (e) {
                        file.data = e.target.result;
                        self.request.SetUpImage = file;
                    };
                    reader.readAsDataURL(e);
                },
                toggleChildCareOptions() {
                    this.childCareSelectAll = !this.childCareSelectAll;
                    if (this.childCareSelectAll) {
                        this.request.ChildCareOptions = [
                            "Infant/Toddler",
                            "Preschool",
                            "K-2nd",
                            "3-5th",
                        ];
                    } else {
                        this.request.ChildCareOptions = [];
                    }
                },
                checkForConflicts() {
                    this.existingRequests = JSON.parse(
                        $('[id$="hfUpcomingRequests"]')[0].value
                    );
                    let conflictingDates = [],
                        conflictingRooms = [];
                    let conflictingRequests = this.existingRequests.filter((r) => {
                        r = JSON.parse(r);
                        let isConflictingRoom = false;
                        let isConflictingDate = false;
                        for (let i = 0; i < this.request.Rooms.length; i++) {
                            if (r.Rooms.includes(this.request.Rooms[i])) {
                                isConflictingRoom = true;
                                let roomName = this.rooms.filter((room) => {
                                    return room.Id == this.request.Rooms[i];
                                });
                                if (roomName.length > 0) {
                                    roomName = roomName[0].Value.split(" (")[0];
                                }
                                if (!conflictingRooms.includes(roomName)) {
                                    conflictingRooms.push(roomName);
                                }
                            }
                        }
                        for (let i = 0; i < this.request.EventDates.length; i++) {
                            if (r.EventDates.includes(this.request.EventDates[i])) {
                                //Dates are the same, check they do not overlap with moment-range
                                let cd = r.EventDates.filter((ed) => {
                                    return ed == this.request.EventDates[i];
                                })[0];
                                let cdStart = moment(
                                    `${cd} ${r.StartTime}`,
                                    `yyyy-MM-DD hh:mm A`
                                );
                                if (r.MinsStartBuffer) {
                                    cdStart = cdStart.subtract(r.MinsStartBuffer, "minute");
                                }
                                let cdEnd = moment(`${cd} ${r.EndTime}`, `yyyy-MM-DD hh:mm A`);
                                if (r.MinsStartBuffer) {
                                    cdEnd = cdEnd.add(r.MinsEndBuffer, "minute");
                                }
                                let cRange = moment.range(cdStart, cdEnd);
                                let current = moment.range(
                                    moment(
                                        `${this.request.EventDates[i]} ${this.request.StartTime}`,
                                        `yyyy-MM-DD hh:mm A`
                                    ),
                                    moment(
                                        `${this.request.EventDates[i]} ${this.request.EndTime}`,
                                        `yyyy-MM-DD hh:mm A`
                                    )
                                );
                                if (cRange.overlaps(current)) {
                                    isConflictingDate = true;
                                    if (!conflictingDates.includes(this.request.EventDates[i])) {
                                        conflictingDates.push(this.request.EventDates[i]);
                                    }
                                }
                            }
                        }
                        return isConflictingRoom && isConflictingDate;
                    });
                    if (conflictingRequests.length > 0) {
                        this.valid = false;
                        this.conflictingRequestMsg = `There are conflicts on ${conflictingDates.join(
                            ", "
                        )} with the following rooms: ${conflictingRooms.join(", ")}`
                    }
                },
                checkTimeMeetsRequirements() {
                    //Check general 9-9 time rule
                    let meetsTimeRequirements = true
                    if (this.request.StartTime.includes("AM")) {
                        let info = this.request.StartTime.split(':')
                        if (parseInt(info[0]) < 9) {
                            meetsTimeRequirements = false
                            this.beforeHoursMsg = 'Operations support staff do not provide any resources or unlock doors before 9AM. If this is a staff-only event, you will be responsible for providing all of your own resources and managing your own doors. Non-staff-only event requests with starting times before 9AM will not be accepted without special consideration.'
                        }
                    }
                    if (this.request.EndTime.includes("PM")) {
                        let info = this.request.EndTime.split(':')
                        if (parseInt(info[0]) >= 9) {
                            meetsTimeRequirements = false
                            this.afterHoursMsg = 'Our facilities close at 9PM. Requesting an ending time past this time will require special approval from the Events Director and should not be expected.'
                        }
                    }
                    //Check more specific range for Satuday and Sunday
                    // for(var i=0; i<this.request.EventDates.length; i++){
                    //   let dt = moment(this.request.EventDates[i])
                    //   if(dt.day() == 0){
                    //     //Sunday
                    //     if(this.request.StartTime.includes("AM")) {
                    //       meetsTimeRequirements = false
                    //     }
                    //   } else if(dt.day() == 6) {
                    //     //Saturday
                    //     if(this.request.StartTime.includes("PM")) {
                    //       meetsTimeRequirements = false
                    //     }
                    //     if(this.request.EndTime.includes("PM") && this.request.EndTime != "12:00 PM") {
                    //       meetsTimeRequirements = false
                    //     }
                    //   }
                    // }
                    if (!meetsTimeRequirements) {
                        this.valid = false
                    }
                },
                validate() {
                    this.valid = true
                    this.triedSubmit = true
                    this.$refs.form.validate()
                    if (this.$refs.roompckr && this.tab == 1) {
                        this.$refs.roompckr.$refs.roomform.validate()
                    }
                    const errors = [];
                    this.$refs.form.inputs.forEach((e) => {
                        if (e.errorBucket && e.errorBucket.length) {
                            errors.push(...e.errorBucket)
                        }
                    });
                    this.checkForConflicts()
                    this.checkTimeMeetsRequirements()
                },
            },
            watch: {
                sameFoodDrinkDropOff(val) {
                    if (val) {
                        this.request.DrinkDropOff = this.request.FoodDropOff
                    }
                },
                defaultRegistraionEnd(val) {
                    if (val) {
                        this.request.RegistrationEndDate = val
                    }
                },
                defaultRegistraionStart(val) {
                    if (val) {
                        this.request.RegistrationDate = val
                    }
                }
            }
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
    color: #8ED2C9;
  }
</style>