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

<asp:HiddenField ID="hfRooms" runat="server" />
<asp:HiddenField ID="hfReservations" runat="server" />
<asp:HiddenField ID="hfRequest" runat="server" />

<div id="app">
  <v-app>
    <div>
      <v-card v-if="panel == 0">
        <v-card-text>
          <h3>I am requesting...</h3>
          <strong><i>Check all that apply</i></strong>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsSpace"
                :label="`A physical space for an event (${boolToYesNo(request.needsSpace)})`"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsOnline"
                :label="`An online event (${boolToYesNo(request.needsOnline)})`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsOnline"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsPub"
                :label="`Publicity (${boolToYesNo(request.needsPub)})`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsPub"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsCatering"
                :label="`Catering (${boolToYesNo(request.needsCatering)})`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsCatering"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsChildCare"
                :label="`Childcare (${boolToYesNo(request.needsChildCare)})`"
                hint="Requests involving childcare must be made at least 30 days in advance"
                :persistent-hint="request.needsChildCare"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsAccom"
                :label="`Special accommodations (tech, drinks, registration, extensive set-up, etc.) (${boolToYesNo(request.needsAccom)})`"
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
          <v-form ref="form" v-model="valid">
            <v-alert type="error" v-if="!isValid && triedSubmit"
              >Please review your request and fix all errors</v-alert
            >
            <%-- Basic Request Information --%>
            <v-row>
              <v-col>
                <h3 class="primary--text">Basic Information</h3>
              </v-col>
            </v-row>
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
                <v-text-field
                  label="What ministry is sponsoring this event?"
                  v-model="request.Ministry"
                  :rules="[rules.required(request.Ministry, 'Ministry')]"
                ></v-text-field>
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
                          :items="availableRooms"
                          item-text="Value"
                          item-value="Id"
                          v-model="request.Rooms"
                          attach
                          :rules="[rules.requiredArr(request.Rooms, 'Room/Space')]"
                        ></v-autocomplete>
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
                        :items="availableRooms"
                        item-text="Value"
                        item-value="Id"
                        v-model="request.Rooms"
                        attach
                        :rules="[rules.requiredArr(request.Rooms, 'Room/Space')]"
                      ></v-autocomplete>
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
              <v-row
                v-for="(i, idx) in request.Publicity"
                :key="`pub_${idx}`"
                align-center
              >
                <v-col>
                  <publicity-picker
                    :value="i"
                    :earliest-date="earliestPubDate"
                  ></publicity-picker>
                </v-col>
                <v-col cols="1">
                  <v-btn fab color="red" v-if="idx > 0" @click="removePub(idx)">
                    <v-icon>mdi-delete-forever</v-icon>
                  </v-btn>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-btn
                    absolute
                    right
                    fab
                    color="accent"
                    :disabled="pubBtnDisabled"
                    @click="addPub"
                  >
                    <v-icon>mdi-plus</v-icon>
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
              <v-row>
                <v-col cols="12" md="6">
                  <v-file-input
                    accept="image/*"
                    label="If your event has an existing graphic, please upload it here"
                    prepend-inner-icon="mdi-camera"
                    prepend-icon=""
                    v-model="pubImage"
                    @change="handleFile"
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
                    :label="`Food should be delivered? ${request.FoodDelivery ? 'Yes!' : 'No, someone from my team will pick it up'}`"
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
                  <v-autocomplete
                    label="What ages of childcare do you want to offer?"
                    :items="['Infant/Toddler', 'Preschool', 'K-2nd', '3-5th']"
                    chips
                    multiple
                    attach
                    v-model="request.ChildCareOptions"
                  ></v-autocomplete>
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
                    label="What drinks would you like to have?"
                    :items="['Coffee', 'Soda', 'Water']"
                    :hint="`${request.Drinks.toString().includes('Coffee') ? 'If you are requesting coffee, you will be required to have a serving team during COVID-19' : ''}`"
                    persistent-hint
                    v-model="request.Drinks"
                    multiple
                    chips
                    attach
                  ></v-autocomplete>
                </v-col>
                <v-col cols="12" md="6">
                  <v-autocomplete
                    label="What tech needs do you have?"
                    :items="['Handheld Mic', 'Wrap Around Mic', 'Special Lighting', 'Graphics/Video/Powerpoint', 'Worship Team', 'Stage Set-Up', 'Live Stream', 'Pipe and Drape', 'BOSE System']"
                    v-model="request.TechNeeds"
                    multiple
                    chips
                    attach
                  ></v-autocomplete>
                </v-col>
              </v-row>
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
                    label="What is the registration fee for this event?"
                    type="number"
                    prepend-inner-icon="mdi-currency-usd"
                    v-model="request.Fee"
                  ></v-text-field>
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
          <v-btn color="primary" @click="next">Submit</v-btn>
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
          <v-alert type="success"
            >Your request has been submitted! You will receive a confirmation
            email now with the details of your request, when it has been
            approved by the Events Director you will receive an email securing
            your reservation with any additional information from the Events
            Director</v-alert
          >
        </v-card-text>
      </v-card>
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
        });
        Vue.component("time-picker", {
            template: `
            <v-row>
                <v-col>
                    <v-select label="Hour" :items="hours" v-model="hour" attach :error-messages="errorMessage"></v-select>
                </v-col>
                <v-col>
                    <v-select label="Minute" :items="mins" v-model="minute" attach required></v-select>
                </v-col>
                <v-col>
                    <v-select label="AM/PM" :items="aps" v-model="ap" attach required></v-select>
                </v-col>
            </v-row>
        `,
            props: ["value", "default", "rules"],
            data: function () {
                return {
                    hour: null,
                    minute: null,
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
                    this.hour = val.split(":")[0];
                    this.minute = val.split(":")[1].split(" ")[0];
                    this.ap = val.split(" ")[1];
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
                  <h4>{{ moment(eventDate).format('dddd, MMMM Do yyyy') }}</h4>
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
                  </v-row
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
                let names = [
                    "Meeting",
                    "Useless Meeting",
                    "Small Group",
                    "OK Boomers",
                    "CK Event",
                    "Rave",
                ];
                let start = new Date();
                let end = new Date(moment().add(7, "days").format("yyyy-MM-DD"));
                for (i = 0; i < this.rooms.length * 3; i++) {
                    let nameIdx = Math.floor(Math.random() * 6);
                    let roomIdx = Math.floor(Math.random() * this.rooms.length);
                    let dtStart = new Date(
                        start.getTime() + Math.random() * (end.getTime() - start.getTime())
                    );
                    let duration = Math.floor(Math.random() * 2) + 1;
                    this.allEvents.push({
                        name: names[nameIdx],
                        start: moment(dtStart).format("yyyy-MM-DD HH:mm"),
                        end: moment(dtStart)
                            .add(duration, "hours")
                            .format("yyyy-MM-DD HH:mm"),
                        loc: this.rooms[roomIdx],
                    });
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
                            return i.loc.Id == this.selected;
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
                    ExpectedAttendance: "",
                    Rooms: "",
                    EventURL: "",
                    ZoomPassword: "",
                    Publicity: [{ Date: "", Needs: "" }],
                    PublicityBlurb: "",
                    PubImage: null,
                    ShowOnCalendar: false,
                    Vendor: "",
                    Menu: "",
                    FoodDelivery: true,
                    FoodTime: "",
                    BudgetLine: "",
                    FoodDropOff: "",
                    CCVendor: "",
                    CCMenu: "",
                    CCFoodTime: "",
                    CCBudgetLine: "",
                    ChildCareOptions: "",
                    EstimatedKids: null,
                    Drinks: "",
                    TechNeeds: "",
                    RegistrationDate: "",
                    Fee: null,
                    Notes: "",
                },
                pubImage: {},
                rooms: [],
                menu: false,
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
                            return `You cannot have more than ${cap} ${
                                cap == 1 ? "person" : "people"
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
                                `${
                                isStart
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
                },
                valid: true,
                triedSubmit: false,
                tab: 0
            },
            created() {
                this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value)
                let req = $('[id$="hfRequest"]')[0].value
                if (req) {
                    this.request = JSON.parse(req)
                    if (this.request.PubImage) {
                        this.pubImage = this.request.PubImage
                    }
                }
            },
            mounted() {
                let query = window.location.search.substring(1)
                if (query.includes('ShowSuccess')) {
                    let info = query.split('=')
                    if (info[1] == 'true') {
                        this.panel = 2
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
                longDates() {
                    return this.request.EventDates.map((i) => {
                        return { text: moment(i).format("dddd, MMMM Do yyyy"), val: i };
                    });
                },
                isValid() {
                    if (this.$refs.roompckr && this.tab == 1) {
                        return this.valid && this.$refs.roompckr.valid;
                    }
                    return this.valid;
                },
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
                            $('[id$="hfRequest"]').val(JSON.stringify(this.request));
                            $('[id$="btnSubmit"')[0].click();
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
                handleFile(e) {
                    console.log(e)
                    let file = { name: e.name, type: e.type }
                    var reader = new FileReader()
                    const self = this
                    reader.onload = function (e) {
                        console.log(e.target.result)
                        file.data = e.target.result
                        self.request.PubImage = file
                    }
                    reader.readAsDataURL(e)
                },
                validate() {
                    this.triedSubmit = true;
                    this.$refs.form.validate();
                    if (this.$refs.roompckr && this.tab == 1) {
                        this.$refs.roompckr.$refs.roomform.validate();
                    }
                    const errors = [];
                    this.$refs.form.inputs.forEach((e) => {
                        if (e.errorBucket && e.errorBucket.length) {
                            errors.push(...e.errorBucket);
                        }
                    });
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
</style>