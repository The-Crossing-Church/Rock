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
<asp:HiddenField ID="hfIsAdmin" runat="server" />
<asp:HiddenField ID="hfChangeRequest" runat="server" />

<div id="app">
  <v-app>
    <div>
      <v-card v-if="panel == 0">
        <v-card-text>
          <v-alert v-if="canEdit == false" type="error">You are not able to make changes to this request because it is currently {{request.Status}}.</v-alert>
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
                :label="`Zoom`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsOnline"
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
                :label="`Special Accommodations (tech, drinks, web calendar, extensive set-up, etc.)`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsAccom"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsReg"
                :label="`Registration`"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance"
                :persistent-hint="request.needsReg"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsPub"
                :label="`Publicity`"
                hint="Requests involving publicity must be made at least 6 weeks in advance"
                :persistent-hint="request.needsPub"
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
          <v-alert v-if="canEdit == false" type="error">You are not able to make changes to this request because it is currently {{request.Status}}.</v-alert>
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
                  label="Which ministry is sponsoring this event?"
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
              v-if="request.needsSpace && !request.needsOnline && !request.needsPub && !request.needsReg && !request.needsCatering && !request.needsChildCare && !request.needsAccom"
            >
              <v-tabs v-model="tab">
                <v-tab>I have specific dates(s) and times</v-tab>
                <v-tab-item>
                  <v-row v-if="cannotChangeDates">
                    <v-col class='primary--text' style="font-weight: bold; font-style: italic;">
                      While you may request other changes to your event through the form if you need to change the dates of your request you will need to contact the Events Director.
                    </v-col>
                    <v-col cols="12">
                      <v-btn color="accent" rounded @click="dateChangeMessage = ''; changeDialog = true;">Contact Events Director</v-btn>
                    </v-col>
                  </v-row>
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
                        :rules="[rules.required(request.EventDates, 'Event Date')]"
                        multiple
                        class="elevation-1"
                        :min="earliestDate"
                        :show-current="earliestDate"
                        :disabled="cannotChangeDates"
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
                        :disabled="cannotChangeDates"
                      ></v-select>
                    </v-col>
                  </v-row>
                  <v-row v-if="request.EventDates.length > 1">
                    <v-col>
                      <v-switch
                        :label="`Will each occurrence of your event have the exact same start time, end time, ${requestedResources}? (${boolToYesNo(request.IsSame)})`"
                        v-model="request.IsSame"
                        :disabled="cannotChangeToggle"
                      ></v-switch>
                    </v-col>
                  </v-row>
                  <template v-for="e in request.Events">
                    <v-row>
                      <v-col>
                        <strong v-if="request.Events.length == 1">What time will your event begin and end?</strong>
                        <strong v-else>What time will your event begin and end on {{e.EventDate | formatDate}}?</strong>
                      </v-col>
                    </v-row>
                    <v-row>
                      <v-col cols="12" md="6">
                        <strong>Start Time</strong>
                        <time-picker
                          v-model="e.StartTime"
                          :value="e.StartTime"
                          :rules="[rules.required(e.StartTime, 'Start Time'), rules.validTime(e.StartTime, e.EndTime, true)]"
                        ></time-picker>
                      </v-col>
                      <v-col cols="12" md="6">
                        <strong>End Time</strong>
                        <time-picker
                          v-model="e.EndTime"
                          :value="e.EndTime"
                          :rules="[rules.required(e.EndTime, 'End Time'), rules.validTime(e.EndTime, e.StartTime, false)]"
                        ></time-picker>
                      </v-col>
                    </v-row>
                  </template>
                  <%-- Space Information --%>
                  <template v-if="request.needsSpace">
                    <template v-for="e in request.Events">
                      <space :e="e" :request="request" ref="spaceloop" v-on:updatespace="updateSpace"></space>
                    </template>
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
              <v-row v-if="cannotChangeDates">
                <v-col class='primary--text' style="font-weight: bold; font-style: italic;">
                  While you may request other changes to your event through the form if you need to change the dates of your request you will need to contact the Events Director.
                </v-col>
                <v-col cols="12">
                  <v-btn color="accent" rounded @click="dateChangeMessage = ''; changeDialog = true;">Contact Events Director</v-btn>
                </v-col>
              </v-row>
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
                    :show-current="earliestDate"
                    :rules="[rules.required(request.EventDates, 'Event Date')]"
                    :disabled="cannotChangeDates"
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
                    :disabled="cannotChangeDates"
                  ></v-select>
                </v-col>
              </v-row>
              <v-row v-if="request.EventDates.length > 1">
                <v-col>
                  <v-switch
                    :label="`Will each occurrence of your event have the exact same start time, end time, ${requestedResources}? (${boolToYesNo(request.IsSame)})`"
                    v-model="request.IsSame"
                    :disabled="cannotChangeToggle"
                  ></v-switch>
                </v-col>
              </v-row>
              <template v-for="e in request.Events">
                <v-row>
                  <v-col>
                    <strong v-if="request.Events.length == 1">What time will your event begin and end?</strong>
                    <strong v-else>What time will your event begin and end on {{e.EventDate | formatDate}}?</strong>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col cols="12" md="6">
                    <strong>Start Time</strong>
                    <time-picker
                      v-model="e.StartTime"
                      :value="e.StartTime"
                      :rules="[rules.required(e.StartTime, 'Start Time'), rules.validTime(e.StartTime, e.EndTime, true)]"
                    ></time-picker>
                  </v-col>
                  <v-col cols="12" md="6">
                    <strong>End Time</strong>
                    <time-picker
                      v-model="e.EndTime"
                      :value="e.EndTime"
                      :rules="[rules.required(e.EndTime, 'End Time'), rules.validTime(e.EndTime, e.StartTime, false)]"
                    ></time-picker>
                  </v-col>
                </v-row>
              </template>
              <%-- Space Information --%>
              <template v-if="request.needsSpace">
                <template v-for="e in request.Events">
                  <space :e="e" :request="request" ref="spaceloop" v-on:updatespace="updateSpace"></space>
                </template>
              </template>
            </template>
            <%-- Online Information --%>
            <template v-if="request.needsOnline">
              <template v-for="e in request.Events">
                <v-row>
                  <v-col>
                    <h3 class="primary--text" v-if="request.Events.length == 1">Zoom Information</h3>
                    <h3 class="primary--text" v-else>Zoom Information for {{e.EventDate | formatDate}}</h3>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-text-field
                      label="If there is a link your attendees will need to access this event, list it here"
                      v-model="e.EventURL"
                      :rules="[rules.required(e.EventURL, 'Link')]"
                    ></v-text-field>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-text-field
                      label="If there is a password for the link, list it here"
                      v-model="e.ZoomPassword"
                    ></v-text-field>
                  </v-col>
                </v-row>
              </template>
            </template>
            <%-- Catering Information --%>
            <template v-if="request.needsCatering">
              <template v-for="e in request.Events">
                <catering :e="e" :request="request" ref="cateringloop" v-on:updatecatering="updateCatering"></catering>
              </template>
            </template>
            <%-- Childcare Info --%>
            <template v-if="request.needsChildCare">
              <template v-for="e in request.Events">
                <childcare :e="e" :request="request" ref="childcareloop" v-on:updatechildcare="updateChildcare"></childcare>
              </template>
            </template>
            <%-- Special Accommodations Info --%>
            <template v-if="request.needsAccom">
              <template v-for="e in request.Events">
                <accom :e="e" :request="request" ref="accomloop" v-on:updateaccom="updateAccom"></accom>
              </template>
            </template>
            <%-- Registration Information --%>
            <template v-if="request.needsReg">
              <template v-for="e in request.Events">
                <registration :e="e" :request="request" :earliest-pub-date="earliestPubDate" ref="regloop" v-on:updatereg="updateReg"></registration>
              </template>
            </template>
            <%-- Publicity Information --%>
            <template v-if="request.needsPub">
              <v-row>
                <v-col>
                  <h3 class="primary--text">Publicity Information</h3>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-textarea
                    label="In 450 characters or less, describe why someone should attend your event and what they will learn/receive."
                    v-model="request.WhyAttendSixtyFive"
                    :hint="`Be sure to write in the second person using rhetorical questions that elicit interest or touch a felt need. For example, “Have you ever wondered how Jesus would have dealt with depression and anxiety?” (${request.WhyAttendSixtyFive.length}/450)`"
                    :rules="[rules.required(request.WhyAttendSixtyFive, 'This field'), rules.publicityCharacterLimit(request.WhyAttendSixtyFive, 450)]"
                  ></v-textarea>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-select
                    label="Who are you targeting with this event/class?"
                    v-model="request.TargetAudience"
                    :items="[{text: 'Top of the Funnel Event/Class', desc: 'People who have not attended past ministry events'}, {text: 'Middle of the Funnel Event/Class', desc: 'People who are currently attending ministry events'}, {text: 'Bottom of the Funnel Event/Class', desc: 'Leaders/Super fans of your ministry events'}]"
                    :rules="[rules.required(request.TargetAudience, 'Target Audience')]"
                    item-value="text"
                    attach
                  >
                    <template v-slot:item="data">
                      <v-list-item-content>
                        <v-list-item-title>{{data.item.text}}</v-list-item-title>
                        <v-list-item-subtitle>{{data.item.desc}}</v-list-item-subtitle>
                      </v-list-item-content>
                    </template>
                  </v-select>
                </v-col>
                <v-col>
                  <v-switch
                    :label="`Is your event a “sticky” event? (${boolToYesNo(request.EventIsSticky)})`"
                    hint="i.e. NewComers, Small Group Preview, Discovery Class or Serving at Church"
                    persistent-hint
                    v-model="request.EventIsSticky"
                  ></v-switch>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-menu
                    v-model="pubStartMenu"
                    :close-on-content-click="false"
                    :nudge-right="40"
                    transition="scale-transition"
                    offset-y
                    min-width="290px"
                    attach
                  >
                    <template v-slot:activator="{ on, attrs }">
                      <v-text-field
                        v-model="request.PublicityStartDate"
                        label="What is the earliest date you are comfortable advertising your event?"
                        prepend-inner-icon="mdi-calendar"
                        readonly
                        v-bind="attrs"
                        v-on="on"
                        clearable
                        :rules="[rules.required(request.PublicityStartDate, 'Date')]"
                      ></v-text-field>
                    </template>
                    <v-date-picker
                      v-model="request.PublicityStartDate"
                      @input="pubStartMenu = false"
                      :min="earliestPubDate"
                      :show-current="earliestPubDate"
                      :from-date="earliestPubDate"
                    ></v-date-picker>
                  </v-menu>
                </v-col>
                <v-col>
                  <v-menu
                    v-model="pubEndMenu"
                    :close-on-content-click="false"
                    :nudge-right="40"
                    transition="scale-transition"
                    offset-y
                    min-width="290px"
                    attach
                  >
                    <template v-slot:activator="{ on, attrs }">
                      <v-text-field
                        v-model="request.PublicityEndDate"
                        label="What is the latest date you are comfortable advertising your event?"
                        prepend-inner-icon="mdi-calendar"
                        readonly
                        v-bind="attrs"
                        v-on="on"
                        :rules="[rules.required(request.PublicityEndDate, 'Date'), rules.publicityEndDate(request.EventDates, request.PublicityEndDate, request.PublicityStartDate)]"
                        clearable
                      ></v-text-field>
                    </template>
                    <v-date-picker
                      v-model="request.PublicityEndDate"
                      @input="pubEndMenu = false"
                      :min="earliestEndPubDate"
                      :show-current="earliestEndPubDate"
                      :max="latestPubDate"
                    >
                      <span style="width: 290px; text-align: center; font-size: 12px;" v-if="!request.EventDates || request.EventDates.length == 0">
                        Please select dates for your event to calculate the possible end dates
                      </span>
                    </v-date-picker>
                  </v-menu>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <v-select
                    label="What publicity strategies are you interested in implementing for your event/class?"
                    :items="pubStrategyOptions"
                    v-model="request.PublicityStrategies"
                    attach
                    multiple
                    :rules="[rules.required(request.PublicityStrategies, 'Publicity Strategy')]"
                  ></v-select>
                </v-col>
              </v-row>
              <template v-if="request.PublicityStrategies.includes('Social Media/Google Ads')">
                <v-row>
                  <v-col>
                    <i><strong style="font-size: 16px;">As a reminder the information you are filling out below is a request for Social Media/Google Ads. The Communication Manager will provide further direction and strategy.</strong></i>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-textarea
                      label="In 90 characters or less, describe why someone should attend your event."
                      v-model="request.WhyAttendNinety"
                      :rules="[rules.required(request.WhyAttendNinety, 'This field'), rules.publicityCharacterLimit(request.WhyAttendNinety, 90)]"
                      :hint="`Be sure to write in the second person using rhetorical questions that elicit interest or touch a felt need. For example, “Have you ever wondered how Jesus would have dealt with depression and anxiety?” ${request.WhyAttendNinety.length}/90`"
                    ></v-textarea>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <label class="v-label theme--light">Which words, phrases, and questions would you like your event to trigger when someone searches on Google?</label>
                    <v-chip-group>
                      <v-chip
                        v-for="(key, idx) in request.GoogleKeys"
                        :key="`google_${idx}`"
                        close
                        @click:close="removeGoogleKey(idx)"
                        close-icon="mdi-delete"
                      >
                        {{key}}
                      </v-chip>
                    </v-chip-group>
                    <v-text-field
                      label="Type a word or phrase here, then hit the 'Enter' key to add it to your list"
                      v-model="googleCurrentKey"
                      @keydown.enter="addGoogleKey"
                      :disabled="request.GoogleKeys.length >= 50"
                      :hint="`Limited to 50 keys (${request.GoogleKeys.length}/50)`"
                      persistent-hint
                    ></v-text-field>
                  </v-col>
                </v-row>
              </template>
              <template v-if="request.PublicityStrategies.includes('Mobile Worship Folder')">
                <v-row>
                  <v-col>
                    <i><strong style="font-size: 16px;">As a reminder the information you are filling out below is a request for Mobile Worship Folder. The Communication Manager will provide further direction and strategy.</strong></i>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-text-field
                      label="In 65 characters or less, describe why someone should attend your event."
                      v-model="request.WhyAttendTen"
                      :hint="`Be sure to write in the second person using rhetorical questions that elicit interest or touch a felt need. For example, “Have you ever wondered how Jesus would have dealt with depression and anxiety?” ${request.WhyAttendTen.length}/65`"
                      :rules="[rules.required(request.WhyAttendTen, 'This field'), rules.publicityCharacterLimit(request.WhyAttendTen, 65)]"
                    ></v-text-field>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-textarea
                      label="In terms of graphic design, do you have any specific ideas regarding imagery, symbols, or any other visual elements to help guide our graphic designer?"
                      v-model="request.VisualIdeas"
                      :hint="`${request.VisualIdeas.length}/300`"
                      :rules="[rules.publicityCharacterLimit(request.VisualIdeas, 300)]"
                    ></v-textarea>
                  </v-col>
                </v-row>
              </template>
              <template v-if="request.PublicityStrategies.includes('Announcement')">
                <v-row>
                  <v-col>
                    <strong style="font-size: 16px;">Please give the name and email of 1-3 people who have benefited from this in the past. Write a 1 paragraph description of their involvement and experience.</strong>
                  </v-col>
                </v-row>
                <v-row v-for="(s, idx) in request.Stories" :key="`story_${idx}`">
                  <v-col>
                    <v-text-field
                      label="Name"
                      v-model="s.Name"
                    ></v-text-field>
                  </v-col>
                  <v-col>
                    <v-text-field
                      label="Email"
                      v-model="s.Email"
                    ></v-text-field>
                  </v-col>
                  <v-col cols="12">
                    <v-textarea
                      label="Description of their involvement and experience."
                      v-model="s.Description"
                    ></v-textarea>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-btn color="accent" :disabled="request.Stories.length == 3" @click="request.Stories.push({Name:'', Email: '', Description: ''})">Add Person</v-btn>
                  </v-col>
                </v-row>
                <v-row>
                  <v-col>
                    <v-textarea
                      label="In 175 characters or less, describe why someone should attend your event."
                      v-model="request.WhyAttendTwenty"
                      :rules="[rules.publicityCharacterLimit(request.WhyAttendTwenty, 175)]"
                      :hint="`Be sure to write in the second person using rhetorical questions that elicit interest or touch a felt need. For example, “Have you ever wondered how Jesus would have dealt with depression and anxiety?” (${request.WhyAttendTwenty.length}/175)`"
                    ></v-textarea>
                  </v-col>
                </v-row>
              </template>
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
              <br v-if="conflictingRequestMsg != '' && (beforeHoursMsg != '' || afterHoursMsg != '')" />
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
      <v-dialog
        v-model="changeDialog"
        v-if="changeDialog"
        max-width="850px"
      >
        <v-card>
          <v-card-title></v-card-title>
          <v-card-text>
            <v-textarea
              label="Describe the changes you would like to make about the dates of your event."
              v-model="dateChangeMessage"
            ></v-textarea>
          </v-card-text>
          <v-card-actions>
            <v-btn color="accent" @click="sendDateChangeRequest">Submit Change Request</v-btn>
            <Rock:BootstrapButton
              runat="server"
              ID="btnChangeRequest"
              CssClass="btn-hidden"
              OnClick="btnChangeRequest_Click"
            />
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </v-app>
</div>
<script>
    document.addEventListener("DOMContentLoaded", function () {
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
                    if (!this.originalValue && (!this.value || this.value.includes('null'))) {
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
              <br/>
              <v-autocomplete
                label="Select a Room/Space to view availability"
                :items="groupedRooms"
                item-text="Value"
                item-value="Id"
                v-model="selected"
                attach
                :rules="[rules.required(selected, 'Room/Space')]"
                :value="request.Events[0].Rooms"
              >
                <template v-slot:selection="data">
                  {{data.item.Value}} ({{data.item.Capacity}})
                </template>
                <template v-slot:item="data">
                  <template v-if="typeof data.item !== 'object'">
                    <v-list-item-content v-text="data.item"></v-list-item-content>
                  </template>
                  <template v-else>
                    <v-list-item-content>
                      <v-list-item-title>{{data.item.Value}} ({{data.item.Capacity}})</v-list-item-title>
                    </v-list-item-content>
                  </template>
                </template>
              </v-autocomplete>
            </v-col>  
          </v-row>
          <br/>
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
                  :value="request.Events[0].ExpectedAttendance"
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
                    selected: this.request.Events[0].Rooms[0],
                    rooms: [],
                    eventDate: this.request.Events[0].EventDate,
                    startTime: this.request.Events[0].StartTime,
                    endTime: this.request.Events[0].EndTime,
                    att: this.request.Events[0].ExpectedAttendance,
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
                groupedRooms() {
                    let loc = []
                    this.rooms.forEach(l => {
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
        Vue.component("space", {
            template: `
        <v-form ref="spaceForm" v-model="valid">
          <v-row>
            <v-col>
              <h3 class="primary--text" v-if="request.Events.length == 1">Space Information</h3>
              <h3 class="primary--text" v-else>
                Space Information for {{e.EventDate | formatDate}}
                <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
                  Prefill
                </v-btn>
              </h3>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6">
              <v-text-field
                label="How many people are you expecting to attend?"
                type="number"
                v-model="e.ExpectedAttendance"
                :rules="[rules.required(e.ExpectedAttendance, 'Expected Attendance')]"
                :hint="attHint"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6">
              <v-autocomplete
                label="What are your preferred rooms/spaces?"
                :items="groupedRooms"
                item-text="Value"
                item-value="Id"
                v-model="e.Rooms"
                prepend-inner-icon="mdi-map"
                @click:prepend-inner="openMap"
                chips
                multiple
                attach
                :rules="[rules.requiredArr(e.Rooms, 'Room/Space'), rules.roomCapacity(rooms, e.Rooms, e.ExpectedAttendance)]"
                hint="Click the map icon to view campus map"
                persistent-hint
              >
                <template v-slot:item="data">
                  <template v-if="typeof data.item !== 'object'">
                    <v-list-item-content v-text="data.item"></v-list-item-content>
                  </template>
                  <template v-else>
                    <v-list-item-content>
                      <v-list-item-title>{{data.item.Value}} ({{data.item.Capacity}})</v-list-item-title>
                    </v-list-item-content>
                  </template>
                </template>
              </v-autocomplete>
            </v-col>
          </v-row>
          <v-row v-if="request.needsReg">
            <v-col cols="12" md="6">
              <v-switch
                :label="CheckinLabel"
                v-model="e.Checkin"
              ></v-switch>
            </v-col>
            <v-col cols="12" md="6" v-if="e.Checkin && e.ExpectedAttendance >= 100">
              <v-switch
                label="Since your event is estimated to support more than 100 people, would you like the database team to provide a team to work your event in-person?"
                v-model="e.SupportTeam"
              ></v-switch>
            </v-col>
          </v-row>
          <v-dialog
            v-if="dialog"
            v-model="dialog"
            max-width="850px"
          >
            <v-card>
              <v-card-title>
                Pre-fill this section with information from another date
              </v-card-title>  
              <v-card-text>
                <v-select
                  :items="prefillOptions"
                  v-model="prefillDate"
                >
                  <template v-slot:selection="data">
                    {{data.item | formatDate}}
                  </template>
                  <template v-slot:item="data">
                    {{data.item | formatDate}}
                  </template>
                </v-select>  
              </v-card-text>  
              <v-card-actions>
                <v-btn color="secondary" @click="dialog = false; prefillDate = '';">Cancel</v-btn> 
                <v-spacer></v-spacer> 
                <v-btn color="primary" @click="prefillSection">Pre-fill Section</v-btn>  
              </v-card-actions>  
            </v-card>
          </v-dialog>
          <v-dialog
            v-if="map"
            v-model="map"
            max-width="850px"
          >
            <v-card>
              <v-card-text>
                <v-img src="https://rock.thecrossingchurch.com/Content/Operations/Campus%20Map.png"/>  
              </v-card-text>  
            </v-card>
          </v-dialog>
        </v-form>
      `,
            props: ["e", "request"],
            data: function () {
                return {
                    dialog: false,
                    map: false,
                    valid: true,
                    rooms: [],
                    prefillDate: '',
                    rules: {
                        required(val, field) {
                            return !!val || `${field} is required`;
                        },
                        requiredArr(val, field) {
                            return val.length > 0 || `${field} is required`;
                        },
                        exceedsSelected(val, selected, rooms) {
                            if (val && selected) {
                                let room = rooms.filter((i) => {
                                    return i.Id == selected;
                                })[0];
                                let cap = room.Capacity;
                                if (val > cap) {
                                    return `You cannot have more than ${cap} ${cap == 1 ? "person" : "people"
                                        } in the selected space`;
                                }
                            }
                            return true;
                        },
                        roomCapacity(allRooms, rooms, attendance) {
                            if (attendance) {
                                let selectedRooms = allRooms.filter((r) => {
                                    return rooms.includes(r.Id);
                                });
                                let maxCapacity = 0;
                                selectedRooms.forEach((r) => {
                                    maxCapacity += r.Capacity;
                                });
                                if (attendance <= maxCapacity) {
                                    return true;
                                } else {
                                    return `This selection of rooms alone can only support a maximum capacity of ${maxCapacity}. Please select more rooms for increased capacity or lower your expected attendance.`;
                                }
                            }
                            return true;
                        },
                    }
                }
            },
            created: function () {
                this.allEvents = [];
                this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
            },
            filters: {
                formatDate(val) {
                    return moment(val).format("MM/DD/yyyy");
                },
            },
            computed: {
                attHint() {
                    return this.e.ExpectedAttendance > 250 ? 'Events with more than 250 attendees must be approved by the city and requests must be submitted at least 30 days in advance' : ''
                },
                prefillOptions() {
                    return this.request.EventDates.filter(i => i != this.e.EventDate)
                },
                groupedRooms() {
                    let loc = []
                    this.rooms.forEach(l => {
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
                CheckinLabel() {
                    return `Do you need in-person check-in on the day of the event? (${this.boolToYesNo(this.e.Checkin)})`
                }
            },
            methods: {
                prefillSection() {
                    this.dialog = false
                    let idx = this.request.EventDates.indexOf(this.prefillDate)
                    let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
                    this.$emit('updatespace', { targetIdx: idx, currIdx: currIdx })
                },
                boolToYesNo(val) {
                    if (val) {
                        return "Yes";
                    }
                    return "No";
                },
                openMap() {
                    this.map = true
                }
            }
        });
        Vue.component("registration", {
            template: `
        <v-form ref="regForm" v-model="valid">
          <v-row>
            <v-col>
              <h3 class="primary--text" v-if="request.Events.length == 1">Registration Information</h3>
              <h3 class="primary--text" v-else>
                Registration Information for {{e.EventDate | formatDate}}
                <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
                  Prefill
                </v-btn>
              </h3>
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
                    v-model="e.RegistrationDate"
                    label="What date do you need the registration link to be ready and live?"
                    prepend-inner-icon="mdi-calendar"
                    readonly
                    v-bind="attrs"
                    v-on="on"
                    clearable
                    :rules="[rules.required(e.RegistrationDate, 'Start Date'), ]"
                  ></v-text-field>
                </template>
                <v-date-picker
                  v-model="e.RegistrationDate"
                  @input="menu = false"
                  :min="earliestPubDate"
                  :show-current="earliestPubDate"
                ></v-date-picker>
              </v-menu>
            </v-col>
            <v-col cols="12" md="6">
              <v-autocomplete
                label="Types of Registration Fees"
                :items="feeOptions"
                v-model="e.FeeType"
                multiple
                attach
              ></v-autocomplete>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6" v-if="e.FeeType.includes('Fee per Individual')">
              <v-text-field
                label="How much is the individual registration fee for this event?"
                type="number"
                prepend-inner-icon="mdi-currency-usd"
                v-model="e.Fee"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" v-if="e.FeeType.includes('Fee per Couple')">
              <v-text-field
                label="How much is the couple registration fee for this event?"
                type="number"
                prepend-inner-icon="mdi-currency-usd"
                v-model="e.CoupleFee"
              ></v-text-field>
            </v-col>
            <v-col cols="12" md="6" v-if="e.FeeType.includes('Online Fee')">
              <v-text-field
                label="How much is the online registration fee for this event?"
                type="number"
                prepend-inner-icon="mdi-currency-usd"
                v-model="e.OnlineFee"
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
                        v-model="e.RegistrationEndDate"
                        label="What date should registration close?"
                        hint="We always default to 24 hours before your event if you have no reason to close registration earlier."
                        persistent-hint
                        prepend-inner-icon="mdi-calendar"
                        readonly
                        v-bind="attrs"
                        v-on="on"
                        :rules="[rules.required(e.RegistrationEndDate, 'End Date'), rules.registrationCloseDate(request.EventDates, e.EventDate, e.RegistrationEndDate, request.needsChildCare)]"
                        clearable
                      ></v-text-field>
                    </template>
                    <v-date-picker
                      v-model="e.RegistrationEndDate"
                      @input="menu2 = false"
                      :min="earliestPubDate"
                      :show-current="earliestPubDate"
                    ></v-date-picker>
                  </v-menu>
                </v-col>
              </v-row>
            </v-col>
            <v-col cols="12" md="6">
              <strong>What time should registration close?</strong>
              <time-picker
                v-model="e.RegistrationEndTime"
                :value="e.RegistrationEndTime"
                :default="e.StartTime"
                :rules="[rules.required(e.RegistrationEndTime, 'End Time'), rules.registrationCloseTime(e.EventDate, e.RegistrationEndDate, request.needsChildCare, e.StartTime, e.EndTime, e.RegistrationEndTime)]"
              ></time-picker>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <h4 class="primary--text">Let's build-out the confirmation email your registrants will receive after signing up for this event</h4>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-text-field
                label="Who should this email come from?"
                v-model="e.Sender"
                :rules="[rules.required(e.Sender, 'Sender')]"
              ></v-text-field>
              <v-text-field
                label="Sender Email"
                v-model="e.SenderEmail"
                hint="If you want to use an email other than your sender's firstname.lastname@thecrossing email enter it here"
              ></v-text-field>
              <v-textarea
                label="Thank You"
                v-model="e.ThankYou"
              ></v-textarea>
              <v-textarea
                label="Time and Location"
                v-model="e.TimeLocation"
              ></v-textarea>
              <v-textarea
                label="Additional Details"
                v-model="e.AdditionalDetails"
              ></v-textarea>
            </v-col>
            <v-col>
              <div v-html="emailPreview"></div>
            </v-col>
          </v-row>
          <v-dialog
            v-if="dialog"
            v-model="dialog"
            max-width="850px"
          >
            <v-card>
              <v-card-title>
                Pre-fill this section with information from another date
              </v-card-title>  
              <v-card-text>
                <v-select
                  :items="prefillOptions"
                  v-model="prefillDate"
                >
                  <template v-slot:selection="data">
                    {{data.item | formatDate}}
                  </template>
                  <template v-slot:item="data">
                    {{data.item | formatDate}}
                  </template>
                </v-select>  
              </v-card-text>  
              <v-card-actions>
                <v-btn color="secondary" @click="dialog = false; prefillDate = '';">Cancel</v-btn> 
                <v-spacer></v-spacer> 
                <v-btn color="primary" @click="prefillSection">Pre-fill Section</v-btn>  
              </v-card-actions>  
            </v-card>
          </v-dialog>
        </v-form>
      `,
            props: ["e", "request", "earliestPubDate"],
            data: function () {
                return {
                    menu: false,
                    menu2: false,
                    dialog: false,
                    prefillDate: '',
                    valid: true,
                    rules: {
                        required(val, field) {
                            return !!val || `${field} is required`;
                        },
                        registrationCloseDate(eventDates, eventDate, closeDate, needsChildCare) {
                            let dates = eventDates.map(d => moment(d))
                            let minDate = moment.min(dates)
                            if (eventDate) {
                                minDate = moment(eventDate)
                            }
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
                        registrationCloseTime(eventDate, closeDate, needsChildCare, startTime, endtime, closeTime) {
                            let minDate = moment(eventDate)
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
                    }
                }
            },
            created: function () {
                this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
            },
            methods: {
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
                prefillSection() {
                    this.dialog = false
                    let idx = this.request.EventDates.indexOf(this.prefillDate)
                    let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
                    this.$emit('updatereg', { targetIdx: idx, currIdx: currIdx })
                }
            },
            computed: {
                defaultRegistraionStart() {
                    if (this.request.needsReg) {
                        if (this.e.RegistrationDate) {
                            return this.e.RegistrationDate
                        }
                        if (this.request.PublicityStartDate) {
                            return moment(this.request.PublicityStartDate).subtract(3, 'days').format("yyyy-MM-DD")
                        }
                    }
                    return ""
                },
                defaultRegistraionEnd() {
                    if (this.request.needsReg) {
                        if (this.e.RegistrationEndDate) {
                            return this.e.RegistrationEndDate
                        }
                        if (this.request.EventDates) {
                            if (this.e.EventDate) {
                                return moment(this.e.EventDate).subtract(1, "day").format("yyyy-MM-DD")
                            } else {
                                let eventDates = this.request.EventDates.map(p => moment(p))
                                let firstDate = moment.min(eventDates)
                                return moment(firstDate).subtract(1, "day").format("yyyy-MM-DD")
                            }
                        }
                    }
                    return ""
                },
                defaultThankYou() {
                    if (this.request.needsReg) {
                        if (this.request.Id > 0 && this.e.ThankYou) {
                            return this.e.ThankYou
                        }
                        if (this.request.Name) {
                            return "Thank you for registering for " + this.request.Name
                        }
                        return ""
                    }
                    return ""
                },
                defaultTimeLocation() {
                    if (this.request.needsReg) {
                        if (this.request.Id > 0 && this.e.TimeLocation) {
                            return this.e.TimeLocation
                        }
                        if (this.request.Name && this.e.StartTime) {
                            return this.request.Name + " will take place at " + this.e.StartTime + " in " + this.formatRooms(this.e.Rooms)
                        }
                        return ""
                    }
                    return ""
                },
                emailPreview() {
                    let preview =
                        "<div style='background-color: #F2F2F2;'>" +
                        "<div style='text-align: center; padding-top: 30px; padding-bottom: 30px;'>" +
                        "<img src='https://rock.thecrossingchurch.com/content/EmailTemplates/CrossingLogo-EmailTemplate-Header-215x116.png' border='0' style='width:100%; max-width: 215px; height: auto;'>" +
                        "</div>" +
                        "<div style='background-color: #F9F9F9; padding: 30px; margin: auto; max-width: 90%;'>" +
                        "<h1>" + this.request.Name + "</h1><br/>" +
                        this.e.ThankYou + "<br/>" +
                        this.e.TimeLocation + "<br/><br/>" +
                        "<p>The following people have been registered for " + this.request.Name + ":</p>" +
                        "<ul>" +
                        "<li>First Registrant</li>" +
                        "<li>Second Registrant</li>" +
                        "</ul>"
                    if (this.e.Fee) {
                        preview +=
                            "<p>" +
                            "Total Cost: $" + this.e.Fee + "<br/>" +
                            "Total Paid: $" + this.e.Fee + "<br/>" +
                            "Balance Due: $0.00<br/>" +
                            "</p>"
                    }
                    preview += this.e.AdditionalDetails
                    preview += "</div>"
                    preview +=
                        "<div style='text-align:center;'><br/>" +
                        "<b>The Crossing</b><br/>" +
                        "3615 Southland Dr.<br/>" +
                        "Columbia, MO 65201<br/>" +
                        "(573) 256-4410<br/>" +
                        "<a href='https://thecrossingchurch.com'><b>thecrossingchurch.com</b></a><br/><br/>" +
                        "</div>"
                    preview += "</div>"
                    return preview
                },
                prefillOptions() {
                    return this.request.EventDates.filter(i => i != this.e.EventDate)
                },
                feeOptions() {
                    if (this.request.needsOnline) {
                        return ['Fee per Individual', 'Fee per Couple', 'Online Fee', 'No Fees']
                    }
                    return ['Fee per Individual', 'Fee per Couple', 'No Fees']
                }
            },
            watch: {
                e(val) {
                    this.$emit('change', val)
                },
                defaultRegistraionEnd(val) {
                    if (val) {
                        this.e.RegistrationEndDate = val
                    }
                },
                defaultRegistraionStart(val) {
                    if (val) {
                        this.e.RegistrationDate = val
                    }
                },
                defaultThankYou(val) {
                    if (val) {
                        this.e.ThankYou = val
                    }
                },
                defaultTimeLocation(val) {
                    if (val) {
                        this.e.TimeLocation = val
                    }
                },
                'e.FeeType'(val) {
                    if (!val.includes('Fee per Individual')) {
                        this.e.Fee = null
                    }
                    if (!val.includes('Fee per Couple')) {
                        this.e.CoupleFee = null
                    }
                    if (!val.includes('Online Fee')) {
                        this.e.OnlineFee = null
                    }
                }
            },
            filters: {
                formatDate(val) {
                    return moment(val).format("MM/DD/yyyy");
                },
            },
        });
        Vue.component("catering", {
            template: `
        <v-form ref="cateringForm" v-model="valid">
          <v-row>
            <v-col>
              <h3 class="primary--text" v-if="request.Events.length == 1">Catering Information</h3>
              <h3 class="primary--text" v-else>
                Catering Information for {{e.EventDate | formatDate}}
                <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
                  Prefill
                </v-btn>
              </h3>
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
                  <v-list-item @click="e.Vendor = 'Chick-fil-A'">
                    Chick-fil-A
                  </v-list-item>
                  <v-list-item @click="e.Vendor = 'Como Smoke and Fire'">
                    Como Smoke and Fire
                  </v-list-item>
                  <v-list-item @click="e.Vendor = 'Honey Baked Ham'">
                    Honey Baked Ham
                  </v-list-item>
                  <v-list-item @click="e.Vendor = 'Panera'">
                    Panera
                  </v-list-item>
                  <v-list-item @click="e.Vendor = 'Picklemans'">
                    Pickleman's
                  </v-list-item>
                  <v-list-item @click="e.Vendor = 'Tropical Smoothie Cafe'">
                    Tropical Smoothie Cafe
                  </v-list-item>
                  <v-list-item @click="e.Vendor = 'Word of Mouth Catering'">
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
                v-model="e.Vendor"
                :rules="[rules.required(e.Vendor, 'Vendor')]"
              ></v-text-field>
            </v-col>
            <v-col>
              <v-text-field
                label="Food Budget Line"
                v-model="e.BudgetLine"
                :rules="[rules.required(e.BudgetLine, 'Budget Line')]"
              ></v-text-field>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-textarea
                label="Preferred Menu"
                v-model="e.Menu"
                :rules="[rules.required(e.Menu, 'Menu')]"
              ></v-textarea>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="e.FoodDelivery"
                :label="deliveryLabel"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row v-if="e.FoodDelivery">
            <v-col cols="12" md="6">
              <strong>What time would you like food to be set up and ready?</strong>
              <time-picker
                v-model="e.FoodTime"
                :value="e.FoodTime"
                :default="defaultFoodTime"
                :rules="[rules.required(e.FoodTime, 'Time')]"
              ></time-picker>
            </v-col>
            <v-col cols="12" md="6">
              <br />
              <v-row>
                <v-col>
                  <v-text-field
                    label="Where should the food be set up?"
                    v-model="e.FoodDropOff"
                    :rules="[rules.required(e.FoodDropOff, 'Location')]"
                  ></v-text-field>
                </v-col>
              </v-row>
            </v-col>
          </v-row>
          <v-row v-else>
            <v-col cols="12" md="6">
              <strong>What time would you like to pick up your food?</strong>
              <time-picker
                v-model="e.FoodTime"
                :value="e.FoodTime"
                :default="defaultFoodTime"
                :rules="[rules.required(e.FoodTime, 'Time')]"
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
                    v-model="e.Drinks"
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
                v-model="e.DrinkTime"
                :value="e.DrinkTime"
                :default="defaultFoodTime"
              ></time-picker>
            </v-col>
          </v-row>
          <!--<v-row v-if="e.Drinks.includes('Coffee')">
            <v-col cols="12" md="6">
              <v-checkbox
                label="I agree to provide a coffee serving team in compliance with COVID-19 policy."
                :rules="[rules.required(e.ServingTeamAgree, 'Agreement to provide a serving team')]"
                v-model="e.ServingTeamAgree"
              ></v-checkbox>
            </v-col>
          </v-row> -->
          <v-row>
            <v-col cols="12" md="6" v-if="e.FoodDropOff != ''">
              <v-checkbox
                label="Set up my drinks in the same location as my food please!"
                v-model="sameFoodDrinkDropOff"
                dense
              ></v-checkbox>
            </v-col>
            <v-col cols="12" md="6">
              <v-text-field
                label="Where would you like your drinks delivered?"
                v-model="e.DrinkDropOff"
              ></v-text-field>
            </v-col>
          </v-row>
          <%-- Childcare Catering --%>
          <template v-if="request.needsChildCare">
            <v-row>
              <v-col>
                <v-select
                  label="Preferred Vendor for Childcare"
                  v-model="e.CCVendor"
                  :rules="[rules.required(e.CCVendor, 'Vendor')]"
                  :items="['Pizza', 'Other']"
                  attach
                ></v-select>
              </v-col>
              <v-col>
                <v-text-field
                  label="Food Budget Line for Childcare"
                  v-model="e.CCBudgetLine"
                  :rules="[rules.required(e.CCBudgetLine, 'Budget Line')]"
                ></v-text-field>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-textarea
                  label="Preferred Menu for Childcare"
                  v-model="e.CCMenu"
                  :rules="[rules.required(e.CCMenu, 'Menu')]"
                ></v-textarea>
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="6">
                <strong>
                  What time would you like your childcare food delivered?
                </strong>
                <time-picker
                  v-model="e.CCFoodTime"
                  :value="e.CCFoodTime"
                  :default="defaultFoodTime"
                  :rules="[rules.required(e.CCFoodTime, 'Time')]"
                ></time-picker>
              </v-col>
            </v-row>
          </template>
          <v-dialog
            v-if="dialog"
            v-model="dialog"
            max-width="850px"
          >
            <v-card>
              <v-card-title>
                Pre-fill this section with information from another date
              </v-card-title>
              <v-card-text>
                <v-select
                  :items="prefillOptions"
                  v-model="prefillDate"
                >
                  <template v-slot:selection="data">
                    {{data.item | formatDate}}
                  </template>
                  <template v-slot:item="data">
                    {{data.item | formatDate}}
                  </template>
                </v-select>
              </v-card-text>
              <v-card-actions>
                <v-btn color="secondary" @click="dialog = false; prefillDate = '';">Cancel</v-btn>
                <v-spacer></v-spacer>
                <v-btn color="primary" @click="prefillSection">Pre-fill Section</v-btn>
              </v-card-actions>
            </v-card>
          </v-dialog>
        </v-form>
      `,
        props: ["e", "request"],
        data: function () {
            return {
                dialog: false,
                valid: true,
                rooms: [],
                prefillDate: '',
                sameFoodDrinkDropOff: false,
                rules: {
                    required(val, field) {
                        return !!val || `${field} is required`;
                    },
                    requiredArr(val, field) {
                        return val.length > 0 || `${field} is required`;
                    },
                }
            }
        },
        created: function () {
            this.allEvents = [];
            this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
        },
        filters: {
            formatDate(val) {
                return moment(val).format("MM/DD/yyyy");
            },
        },
        computed: {
            prefillOptions() {
                return this.request.EventDates.filter(i => i != this.e.EventDate)
            },
            deliveryLabel() {
                return `Would you like your food to be delivered? ${this.e.FoodDelivery ? 'Yes!' : 'No, someone from my team will pick it up'}`
            },
            drinkHint() {
                return ''
                // return `${this.e.Drinks.toString().includes('Coffee') ? 'Due to COVID-19, all drip coffee must be served by a designated person or team from the hosting ministry. This person must wear a mask and gloves and be the only person to touch the cups, sleeves, lids, and coffee carafe before the coffee is served to attendees. If you are not willing to provide this for your own event, please deselect the coffee option and opt for an individually packaged item like bottled water or soda.' : ''}`
            },
            defaultFoodTime() {
                if (this.e.StartTime && !this.e.StartTime.includes('null')) {
                    let time = moment(this.e.StartTime, "hh:mm A");
                    return time.subtract(30, "minutes").format("hh:mm A");
                }
                return null;
            },
        },
        methods: {
            prefillSection() {
                this.dialog = false
                let idx = this.request.EventDates.indexOf(this.prefillDate)
                let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
                this.$emit('updatecatering', { targetIdx: idx, currIdx: currIdx })
            }
        },
        watch: {
            sameFoodDrinkDropOff(val) {
                if (val) {
                    this.e.DrinkDropOff = this.e.FoodDropOff
                }
            },
        }
    });
      Vue.component("childcare", {
          template: `
        <v-form ref="childForm" v-model="valid">
          <v-row>
            <v-col>
              <h3 class="primary--text" v-if="request.Events.length == 1">Childcare Information</h3>
              <h3 class="primary--text" v-else>
                Childcare Information for {{e.EventDate | formatDate}}
                <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
                  Prefill
                </v-btn>
              </h3>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12" md="6">
              <strong>
                What time do you need childcare to start?
              </strong>
              <time-picker
                v-model="e.CCStartTime"
                :value="e.CCStartTime"
                :default="defaultFoodTime"
                :rules="[rules.required(e.CCStartTime, 'Time')]"
              ></time-picker>
            </v-col> 
            <v-col cols="12" md="6">
              <strong>
                What time will childcare end?
              </strong>
              <time-picker
                v-model="e.CCEndTime"
                :value="e.CCEndTime"
                :default="e.EndTime"
                :rules="[rules.required(e.CCEndTime, 'Time')]"
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
                v-model="e.ChildCareOptions"
              >
                <template v-slot:item="data">
                  <div style="padding: 12px 0px; width: 100%">
                    <v-icon
                      v-if="e.ChildCareOptions.includes(data.item)"
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
                v-model="e.EstimatedKids"
              ></v-text-field>
            </v-col>
          </v-row>
          <v-dialog
            v-if="dialog"
            v-model="dialog"
            max-width="850px"
          >
            <v-card>
              <v-card-title>
                Pre-fill this section with information from another date
              </v-card-title>  
              <v-card-text>
                <v-select
                  :items="prefillOptions"
                  v-model="prefillDate"
                >
                  <template v-slot:selection="data">
                    {{data.item | formatDate}}
                  </template>
                  <template v-slot:item="data">
                    {{data.item | formatDate}}
                  </template>
                </v-select>  
              </v-card-text>  
              <v-card-actions>
                <v-btn color="secondary" @click="dialog = false; prefillDate = '';">Cancel</v-btn> 
                <v-spacer></v-spacer> 
                <v-btn color="primary" @click="prefillSection">Pre-fill Section</v-btn>  
              </v-card-actions>  
            </v-card>
          </v-dialog>
        </v-form>
      `,
          props: ["e", "request"],
          data: function () {
              return {
                  dialog: false,
                  valid: true,
                  prefillDate: '',
                  childCareSelectAll: false,
                  rules: {
                      required(val, field) {
                          return !!val || `${field} is required`;
                      },
                      requiredArr(val, field) {
                          return val.length > 0 || `${field} is required`;
                      },
                  }
              }
          },
          created: function () {
          },
          filters: {
              formatDate(val) {
                  return moment(val).format("MM/DD/yyyy");
              },
          },
          computed: {
              prefillOptions() {
                  return this.request.EventDates.filter(i => i != this.e.EventDate)
              },
              defaultFoodTime() {
                  if (this.e.StartTime && !this.e.StartTime.includes('null')) {
                      let time = moment(this.e.StartTime, "hh:mm A");
                      return time.subtract(30, "minutes").format("hh:mm A");
                  }
                  return null;
              },
          },
          methods: {
              toggleChildCareOptions() {
                  this.childCareSelectAll = !this.childCareSelectAll;
                  if (this.childCareSelectAll) {
                      this.e.ChildCareOptions = [
                          "Infant/Toddler",
                          "Preschool",
                          "K-2nd",
                          "3-5th",
                      ];
                  } else {
                      this.e.ChildCareOptions = [];
                  }
              },
              prefillSection() {
                  this.dialog = false
                  let idx = this.request.EventDates.indexOf(this.prefillDate)
                  let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
                  this.$emit('updatechildcare', { targetIdx: idx, currIdx: currIdx })
              }
          }
      });
      Vue.component("accom", {
          template: `
        <v-form ref="accomForm" v-model="valid">
          <v-row>
            <v-col>
              <h3 class="primary--text" v-if="request.Events.length == 1">Other Accommodations</h3>
              <h3 class="primary--text" v-else>
                Other Accommodations for {{e.EventDate | formatDate}}
                <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
                  Prefill
                </v-btn>
              </h3>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12">
              <v-autocomplete
                label="What tech needs do you have?"
                :items="['Handheld Mic', 'Wrap Around Mic', 'Special Lighting', 'Graphics/Video/Powerpoint', 'Worship Team', 'Stage Set-Up', 'Basic Live Stream ($)', 'Advanced Live Stream ($)', 'Pipe and Drape', 'BOSE System']"
                v-model="e.TechNeeds"
                :hint="techHint"
                persistent-hint
                multiple
                chips
                attach
              ></v-autocomplete>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12">
              <v-textarea
                label='Please describe what you are envisioning regarding your tech needs. For example, "We would like to play videos in the gym."'
                v-model="e.TechDescription"
              ></v-textarea>
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
                      v-model="e.Drinks"
                      multiple
                      chips
                      attach
                    ></v-autocomplete>
                  </v-col>
                </v-row>
              </v-col>
              <v-col cols="12" md="6" v-if="e.Drinks.length > 0">
                <strong>What time would you like your drinks to be delivered?</strong>
                <time-picker
                  v-model="e.DrinkTime"
                  :value="e.DrinkTime"
                  :default="defaultFoodTime"
                  :rules="[rules.required(e.DrinkTime, 'Time')]"
                ></time-picker>
              </v-col>
            </v-row>
            <v-row v-if="e.Drinks.length > 0">
              <!--<v-col cols="12" md="6" v-if="e.Drinks.includes('Coffee')">
                <v-checkbox
                  label="I agree to provide a coffee serving team in compliance with COVID-19 policy."
                  :rules="[rules.required(e.ServingTeamAgree, 'Agreement to provide a serving team')]"
                  v-model="e.ServingTeamAgree"
                ></v-checkbox>
              </v-col> -->
              <v-col cols="12" md="6">
                <v-text-field
                  label="Where would you like your drinks delivered?"
                  v-model="e.DrinkDropOff"
                  :rules="[rules.required(e.DrinkDropOff, 'Location')]"
                ></v-text-field>
              </v-col>
            </v-row>
          </template>
          <v-row>
            <v-col cols="12" md="6">
              <v-switch
                :label="calLabel"
                v-model="e.ShowOnCalendar"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row v-if="e.ShowOnCalendar">
            <v-col>
              <v-textarea
                label="Please type out your blurb"
                v-model="e.PublicityBlurb"
                :rules="[rules.blurbValidation(e.PublicityBlurb, request.PublicityStartDate)]"
                validate-on-blur
              ></v-textarea>
            </v-col>
          </v-row>
          <v-row>
            <v-col cols="12">
              <v-textarea
                label="Please describe the set-up you require for your event"
                v-model="e.SetUp"
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
          <v-dialog
            v-if="dialog"
            v-model="dialog"
            max-width="850px"
          >
            <v-card>
              <v-card-title>
                Pre-fill this section with information from another date
              </v-card-title>  
              <v-card-text>
                <v-select
                  :items="prefillOptions"
                  v-model="prefillDate"
                >
                  <template v-slot:selection="data">
                    {{data.item | formatDate}}
                  </template>
                  <template v-slot:item="data">
                    {{data.item | formatDate}}
                  </template>
                </v-select>  
              </v-card-text>  
              <v-card-actions>
                <v-btn color="secondary" @click="dialog = false; prefillDate = '';">Cancel</v-btn> 
                <v-spacer></v-spacer> 
                <v-btn color="primary" @click="prefillSection">Pre-fill Section</v-btn>  
              </v-card-actions>  
            </v-card>
          </v-dialog>
        </v-form>
      `,
          props: ["e", "request"],
          data: function () {
              return {
                  dialog: false,
                  valid: true,
                  prefillDate: '',
                  setupImage: {},
                  rules: {
                      required(val, field) {
                          return !!val || `${field} is required`;
                      },
                      requiredArr(val, field) {
                          return val.length > 0 || `${field} is required`;
                      },
                      blurbValidation(value, pubDate) {
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
                  }
              }
          },
          created: function () {
              if (this.e.SetUpImage) {
                  this.setupImage = this.e.SetUpImage;
              }
          },
          filters: {
              formatDate(val) {
                  return moment(val).format("MM/DD/yyyy");
              },
          },
          computed: {
              prefillOptions() {
                  return this.request.EventDates.filter(i => i != this.e.EventDate)
              },
              techHint() {
                  return `${this.e.TechNeeds.toString().includes('Live Stream') ? 'Keep in mind that all live stream requests will come at an additional charge to the ministry, which will be verified with you in your follow-up email with the Events Director.' : ''}`
              },
              calLabel() {
                  return `I would like this event to be listed on the public web calendar (${this.boolToYesNo(this.e.ShowOnCalendar)})`
              },
              drinkHint() {
                  return ''
                  // return `${this.e.Drinks.toString().includes('Coffee') ? 'Due to COVID-19, all drip coffee must be served by a designated person or team from the hosting ministry. This person must wear a mask and gloves and be the only person to touch the cups, sleeves, lids, and coffee carafe before the coffee is served to attendees. If you are not willing to provide this for your own event, please deselect the coffee option and opt for an individually packaged item like bottled water or soda.' : ''}`
              },
              defaultFoodTime() {
                  if (this.e.StartTime && !this.e.StartTime.includes('null')) {
                      let time = moment(this.e.StartTime, "hh:mm A");
                      return time.subtract(30, "minutes").format("hh:mm A");
                  }
                  return null;
              },
          },
          methods: {
              prefillSection() {
                  this.dialog = false
                  let idx = this.request.EventDates.indexOf(this.prefillDate)
                  let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
                  this.$emit('updateaccom', { targetIdx: idx, currIdx: currIdx })
              },
              handleSetUpFile(e) {
                  let file = { name: e.name, type: e.type };
                  var reader = new FileReader();
                  const self = this;
                  reader.onload = function (e) {
                      file.data = e.target.result;
                      self.e.SetUpImage = file;
                  };
                  reader.readAsDataURL(e);
              },
              boolToYesNo(val) {
                  if (val) {
                      return "Yes";
                  }
                  return "No";
              },
          }
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
                  needsReg: false,
                  needsCatering: false,
                  needsChildCare: false,
                  needsAccom: false,
                  IsSame: true,
                  Name: "",
                  Ministry: "",
                  Contact: "",
                  Events: [
                      {
                          EventDate: "",
                          StartTime: "",
                          EndTime: "",
                          MinsStartBuffer: 0,
                          MinsEndBuffer: 0,
                          ExpectedAttendance: "",
                          Rooms: "",
                          Checkin: false,
                          SupportTeam: false,
                          EventURL: "",
                          ZoomPassword: "",
                          ThankYou: "",
                          TimeLocation: "",
                          AdditionalDetails: "",
                          Sender: "",
                          SenderEmail: "",
                          RegistrationDate: "",
                          RegistrationEndDate: "",
                          RegistrationEndTime: "",
                          FeeType: [],
                          Fee: null,
                          CoupleFee: null,
                          OnlineFee: null,
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
                          TechDescription: "",
                          ShowOnCalendar: false,
                          PublicityBlurb: "",
                          SetUp: "",
                          SetUpImage: null,
                      }
                  ],
                  EventDates: [],
                  WhyAttendSixtyFive: "",
                  TargetAudience: "",
                  EventIsSticky: false,
                  PublicityStartDate: "",
                  PublicityEndDate: "",
                  PublicityStrategies: "",
                  WhyAttendNinety: "",
                  GoogleKeys: [],
                  WhyAttendTen: "",
                  VisualIdeas: "",
                  Stories: [{ Name: "", Email: "", Description: "" }],
                  WhyAttendTwenty: "",
                  Notes: "",
              },
              existingRequests: [],
              rooms: [],
              ministries: [],
              pubStartMenu: false,
              pubEndMenu: false,
              googleCurrentKey: "",
              rules: {
                  required(val, field) {
                      return !!val || `${field} is required`;
                  },
                  requiredArr(val, field) {
                      return val.length > 0 || `${field} is required`;
                  },
                  exceedsSelected(val, selected, rooms) {
                      if (val && selected) {
                          let room = rooms.filter((i) => {
                              return i.Id == selected;
                          })[0];
                          let cap = room.Capacity;
                          if (val > cap) {
                              return `You cannot have more than ${cap} ${cap == 1 ? "person" : "people"
                                  } in the selected space`;
                          }
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
                  roomCapacity(allRooms, rooms, attendance) {
                      if (attendance) {
                          let selectedRooms = allRooms.filter((r) => {
                              return rooms.includes(r.Id);
                          });
                          let maxCapacity = 0;
                          selectedRooms.forEach((r) => {
                              maxCapacity += r.Capacity;
                          });
                          if (attendance <= maxCapacity) {
                              return true;
                          } else {
                              return `This selection of rooms alone can only support a maximum capacity of ${maxCapacity}. Please select more rooms for increased capacity or lower your expected attendance.`;
                          }
                      }
                      return true;
                  },
                  publicityWordLimit(text, limit) {
                      if (text) {
                          let arr = text.split(' ')
                          if (arr.length > limit) {
                              return `Please limit yourself to ${limit} words`
                          }
                      }
                      return true
                  },
                  publicityCharacterLimit(text, limit) {
                      if (text) {
                          if (text.length > limit) {
                              return `Please limit yourself to ${limit} characters`
                          }
                      }
                      return true
                  },
                  publicityEndDate(eventDates, endDate, startDate) {
                      let dates = eventDates.map(d => moment(d))
                      let minDate = moment.max(dates).subtract(1, 'days')
                      if (moment(endDate).isAfter(minDate)) {
                          return 'Publicity cannot end after event.'
                      }
                      let span = moment(endDate).diff(moment(startDate), 'days')
                      if (span < 21) {
                          return 'Publicity end date must be at least 21 days after publicity start.'
                      }
                      return true
                  },
              },
              valid: true,
              formValid: true,
              dialog: false,
              conflictingRequestMsg: "",
              beforeHoursMsg: "",
              afterHoursMsg: "",
              triedSubmit: false,
              tab: 0,
              isAdmin: false,
              dateChangeMessage: '',
              changeDialog: false
          },
          created() {
              this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
              this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value);
              let val = $('[id$="hfIsAdmin"]')[0].value;
              if (val == 'True') {
                  this.isAdmin = true
              }
              let req = $('[id$="hfRequest"]')[0].value;
              if (req) {
                  let parsed = JSON.parse(req)
                  this.request = JSON.parse(parsed.Value)
                  this.request.Id = parsed.Id
                  this.request.Status = parsed.RequestStatus
                  this.request.CreatedBy = parsed.CreatedBy
                  this.request.canEdit = parsed.CanEdit
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
                      if (id && this.isAdmin) {
                          window.history.go(-2)
                      }
                  }
              }
          },
          computed: {
              requestedResources() {
                  let items = []
                  if (this.request.needsSpace) {
                      items.push('rooms')
                  }
                  if (this.request.needsOnline) {
                      items.push('online resources')
                  }
                  if (this.request.needsReg) {
                      items.push('registration')
                  }
                  if (this.request.needsChildCare) {
                      items.push('childcare')
                  }
                  if (this.request.needsCatering) {
                      items.push('catering')
                  }
                  if (this.request.needsAccom) {
                      items.push('accommodations')
                  }
                  items[items.length - 1] = "and " + items[items.length - 1]
                  return items.join(", ");
              },
              earliestDate() {
                  let eDate = new moment();
                  if (
                      !this.request.needsPub && !this.request.needsChildCare && (
                          this.request.needsOnline ||
                          this.request.needsCatering ||
                          this.request.needsReg ||
                          this.request.needsAccom
                      )) {
                      eDate = moment(eDate).add(14, "days");
                  }
                  if (this.request.needsPub) {
                      eDate = moment(eDate).add(6, "weeks").add(1, "day");
                  }
                  if (
                      !this.request.needsPub && (
                          this.request.needsChildCare ||
                          this.request.ExpectedAttendance > 250
                      )) {
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
                  eDate = moment(eDate).add(21, "days");
                  return moment(eDate).format("yyyy-MM-DD");
              },
              earliestEndPubDate() {
                  let eDate = new moment();
                  if (this.request.PublicityStartDate) {
                      eDate = moment(this.request.PublicityStartDate).add(21, "days");
                  } else {
                      eDate = moment(this.earliestPubDate).add(21, "days");
                  }
                  return moment(eDate).format("yyyy-MM-DD");
              },
              latestPubDate() {
                  let sortedDates = this.request.EventDates.sort((a, b) => moment(a).diff(moment(b)))
                  let eDate = new moment(sortedDates[sortedDates.length - 1]);
                  eDate = moment(eDate).subtract(1, "days");
                  return moment(eDate).format("yyyy-MM-DD");
              },
              pubStrategyOptions() {
                  let ops = ['Social Media/Google Ads', 'Mobile Worship Folder']
                  if (this.request.EventIsSticky) {
                      ops.push('Announcement')
                  }
                  return ops
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
              cannotChangeDates() {
                  if (this.isAdmin) {
                      return false
                  }
                  if (this.request.Id > 0 && this.request.Status != 'Submitted') {
                      if (!this.request.IsSame || this.request.Events.length > 1) {
                          return true
                      }
                  }
                  return false
              },
              cannotChangeToggle() {
                  if (this.isAdmin) {
                      return false
                  }
                  if (this.request.Id > 0 && this.request.Status != 'Submitted') {
                      return true
                  }
                  return false
              },
              canEdit() {
                  if (this.isAdmin) {
                      return true
                  }
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
                  $('[id$="hfRequest"]').val(JSON.stringify(this.request));
                  $('[id$="btnSubmit"')[0].click();
              },
              sendDateChangeRequest() {
                  this.changeDialog = false
                  $('[id$="hfChangeRequest"]').val(this.dateChangeMessage)
                  $('[id$="btnChangeRequest"')[0].click();
              },
              setDate(val) {
                  this.request.Events[0].Rooms = [val.room];
                  this.request.Events[0].StartTime = val.startTime;
                  this.request.Events[0].EndTime = val.endTime;
                  this.request.Events[0].EventDate = [val.eventDate];
                  this.request.Events[0].ExpectedAttendance = val.att;
                  this.request.EventDates = [val.eventDate];
              },
              addGoogleKey(key) {
                  if (this.googleCurrentKey) {
                      this.request.GoogleKeys.push(this.googleCurrentKey)
                      this.googleCurrentKey = ''
                  }
              },
              removeGoogleKey(idx) {
                  this.request.GoogleKeys.splice(idx, 1)
              },
              updateReg(indexes) {
                  this.request.Events[indexes.currIdx].RegistrationDate = this.request.Events[indexes.targetIdx].RegistrationDate
                  this.request.Events[indexes.currIdx].RegistrationEndDate = this.request.Events[indexes.targetIdx].RegistrationEndDate
                  this.request.Events[indexes.currIdx].RegistrationEndTime = this.request.Events[indexes.targetIdx].RegistrationEndTime
                  this.request.Events[indexes.currIdx].FeeType = this.request.Events[indexes.targetIdx].FeeType
                  this.request.Events[indexes.currIdx].Fee = this.request.Events[indexes.targetIdx].Fee
                  this.request.Events[indexes.currIdx].CoupleFee = this.request.Events[indexes.targetIdx].CoupleFee
                  this.request.Events[indexes.currIdx].OnlineFee = this.request.Events[indexes.targetIdx].OnlineFee
                  this.request.Events[indexes.currIdx].Sender = this.request.Events[indexes.targetIdx].Sender
                  this.request.Events[indexes.currIdx].SenderEmail = this.request.Events[indexes.targetIdx].SenderEmail
                  this.request.Events[indexes.currIdx].ThankYou = this.request.Events[indexes.targetIdx].ThankYou
                  this.request.Events[indexes.currIdx].TimeLocation = this.request.Events[indexes.targetIdx].TimeLocation
                  this.request.Events[indexes.currIdx].AdditionalDetails = this.request.Events[indexes.targetIdx].AdditionalDetails
              },
              updateSpace(indexes) {
                  this.request.Events[indexes.currIdx].Rooms = this.request.Events[indexes.targetIdx].Rooms
                  this.request.Events[indexes.currIdx].ExpectedAttendance = this.request.Events[indexes.targetIdx].ExpectedAttendance
                  this.request.Events[indexes.currIdx].Checkin = this.request.Events[indexes.targetIdx].Checkin
                  this.request.Events[indexes.currIdx].SupportTeam = this.request.Events[indexes.targetIdx].SupportTeam
              },
              updateCatering(indexes) {
                  this.request.Events[indexes.currIdx].Vendor = this.request.Events[indexes.targetIdx].Vendor
                  this.request.Events[indexes.currIdx].BudgetLine = this.request.Events[indexes.targetIdx].BudgetLine
                  this.request.Events[indexes.currIdx].Menu = this.request.Events[indexes.targetIdx].Menu
                  this.request.Events[indexes.currIdx].FoodDelivery = this.request.Events[indexes.targetIdx].FoodDelivery
                  this.request.Events[indexes.currIdx].FoodTime = this.request.Events[indexes.targetIdx].FoodTime
                  this.request.Events[indexes.currIdx].FoodDropOff = this.request.Events[indexes.targetIdx].FoodDropOff
                  this.request.Events[indexes.currIdx].Drinks = this.request.Events[indexes.targetIdx].Drinks
                  this.request.Events[indexes.currIdx].DrinkTime = this.request.Events[indexes.targetIdx].DrinkTime
                  this.request.Events[indexes.currIdx].ServingTeamAgree = this.request.Events[indexes.targetIdx].ServingTeamAgree
                  this.request.Events[indexes.currIdx].DrinkDropOff = this.request.Events[indexes.targetIdx].DrinkDropOff
                  this.request.Events[indexes.currIdx].CCVendor = this.request.Events[indexes.targetIdx].CCVendor
                  this.request.Events[indexes.currIdx].CCBudgetLine = this.request.Events[indexes.targetIdx].CCBudgetLine
                  this.request.Events[indexes.currIdx].CCMenu = this.request.Events[indexes.targetIdx].CCMenu
                  this.request.Events[indexes.currIdx].CCFoodTime = this.request.Events[indexes.targetIdx].CCFoodTime
              },
              updateChildcare(indexes) {
                  this.request.Events[indexes.currIdx].CCStartTime = this.request.Events[indexes.targetIdx].CCStartTime
                  this.request.Events[indexes.currIdx].CCEndTime = this.request.Events[indexes.targetIdx].CCEndTime
                  this.request.Events[indexes.currIdx].ChildCareOptions = this.request.Events[indexes.targetIdx].ChildCareOptions
                  this.request.Events[indexes.currIdx].EstimatedKids = this.request.Events[indexes.targetIdx].EstimatedKids
              },
              updateAccom(indexes) {
                  this.request.Events[indexes.currIdx].TechNeeds = this.request.Events[indexes.targetIdx].TechNeeds
                  this.request.Events[indexes.currIdx].TechDescription = this.request.Events[indexes.targetIdx].TechDescription
                  this.request.Events[indexes.currIdx].Drinks = this.request.Events[indexes.targetIdx].Drinks
                  this.request.Events[indexes.currIdx].DrinkTime = this.request.Events[indexes.targetIdx].DrinkTime
                  this.request.Events[indexes.currIdx].ServingTeamAgree = this.request.Events[indexes.targetIdx].ServingTeamAgree
                  this.request.Events[indexes.currIdx].DrinkDropOff = this.request.Events[indexes.targetIdx].DrinkDropOff
                  this.request.Events[indexes.currIdx].ShowOnCalendar = this.request.Events[indexes.targetIdx].ShowOnCalendar
                  this.request.Events[indexes.currIdx].PublicityBlurb = this.request.Events[indexes.targetIdx].PublicityBlurb
                  this.request.Events[indexes.currIdx].SetUp = this.request.Events[indexes.targetIdx].SetUp
                  this.request.Events[indexes.currIdx].SetUpImage = this.request.Events[indexes.targetIdx].SetUpImage
              },
              checkForConflicts() {
                  this.existingRequests = JSON.parse(
                      $('[id$="hfUpcomingRequests"]')[0].value
                  );
                  let conflictingMessage = []
                  let conflictingRequests = this.existingRequests.filter((r) => {
                      r = JSON.parse(r);
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
                      if (this.request.Events.length == 1 || this.request.IsSame) {
                          for (let i = 0; i < this.request.EventDates.length; i++) {
                              compareSource.push({ Date: this.request.EventDates[i], StartTime: this.request.Events[0].StartTime, EndTime: this.request.Events[0].EndTime, Rooms: this.request.Events[0].Rooms, MinsStartBuffer: this.request.Events[0].MinsStartBuffer, MinsEndBuffer: this.request.Events[0].MinsEndBuffer })
                          }
                      } else {
                          for (let i = 0; i < this.request.Events.length; i++) {
                              compareSource.push({ Date: this.request.Events[i].EventDate, StartTime: this.request.Events[i].StartTime, EndTime: this.request.Events[i].EndTime, Rooms: this.request.Events[i].Rooms, MinsStartBuffer: this.request.Events[i].MinsStartBuffer, MinsEndBuffer: this.request.Events[i].MinsEndBuffer })
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
                      this.valid = false;
                      this.conflictingRequestMsg = "There are conflicts with the following dates and rooms: " + conflictingMessage.join(", ")
                  }
              },
              checkTimeMeetsRequirements() {
                  this.beforeHoursMsg = ''
                  this.afterHoursMsg = ''
                  //Check general 9-9 time rule
                  let meetsTimeRequirements = true
                  for (let x = 0; x < this.request.Events.length; x++) {
                      if (this.request.Events[x].StartTime.includes("AM")) {
                          let info = this.request.Events[x].StartTime.split(':')
                          if (parseInt(info[0]) < 9) {
                              meetsTimeRequirements = false
                              this.beforeHoursMsg = 'Operations support staff do not provide any resources or unlock doors before 9AM. If this is a staff-only event, you will be responsible for providing all of your own resources and managing your own doors. Non-staff-only event requests with starting times before 9AM will not be accepted without special consideration.'
                          }
                      }
                      if (this.request.Events[x].EndTime.includes("PM")) {
                          let info = this.request.Events[x].EndTime.split(':')
                          if (parseInt(info[0]) > 9 || (parseInt(info[0]) == 9 && info[1].split(' ')[0] != "00")) {
                              meetsTimeRequirements = false
                              this.afterHoursMsg = 'Our facilities close at 9PM. Requesting an ending time past this time will require special approval from the Events Director and should not be expected.'
                          }
                      }
                      //Check more specific range for Satuday and Sunday
                      for (var i = 0; i < this.request.EventDates.length; i++) {
                          let idx = i
                          if (this.request.EventDates.length == 1 || this.request.IsSame) {
                              idx = 0
                          }
                          let dt = moment(this.request.EventDates[i])
                          if (dt.day() == 0) {
                              //Sunday
                              if (this.request.Events[idx].StartTime.includes("AM")) {
                                  meetsTimeRequirements = false
                              }
                          } else if (dt.day() == 6) {
                              if (this.request.Events[idx].EndTime.includes("PM") && this.request.Events[idx].EndTime != "12:00 PM") {
                                  meetsTimeRequirements = false
                                  this.afterHoursMsg = 'On Saturday our facilities close at 12PM. Requesting an ending time past this time will require special approval from the Events Director and should not be expected.'
                              }
                          }
                      }
                  }
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
                  if (this.$refs.spaceloop) {
                      this.$refs.spaceloop.forEach(i => {
                          i.$refs.spaceForm.validate()
                      })
                  }
                  if (this.$refs.regloop) {
                      this.$refs.regloop.forEach(i => {
                          i.$refs.regForm.validate()
                      })
                  }
                  if (this.$refs.cateringloop) {
                      this.$refs.cateringloop.forEach(i => {
                          i.$refs.cateringForm.validate()
                      })
                  }
                  if (this.$refs.childcareloop) {
                      this.$refs.childcareloop.forEach(i => {
                          i.$refs.childForm.validate()
                      })
                  }
                  if (this.$refs.accomloop) {
                      this.$refs.accomloop.forEach(i => {
                          i.$refs.accomForm.validate()
                      })
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
              matchMultiEvent() {
                  this.request.EventDates.forEach((e, idx) => {
                      if (this.request.Events[idx]) {
                          if (this.request.Events[idx].EventDate) {
                              if (this.request.Events[idx].EventDate != e) {
                                  if (this.request.Events.length > this.request.EventDates.length) {
                                      this.request.Events.splice(idx, 1)
                                  }
                              }
                          } else {
                              this.request.Events[idx].EventDate = e
                          }
                      } else {
                          let t = JSON.parse(JSON.stringify(this.request.Events[0]))
                          t.EventDate = e
                          t.RegistrationDate = ""
                          t.RegistrationEndDate = ""
                          this.request.Events.push(t)
                      }
                  })
                  this.request.EventDates = this.request.EventDates.sort((a, b) => moment(a).diff(moment(b)))
                  this.request.Events = this.request.Events.sort((a, b) => moment(a.EventDate).diff(moment(b.EventDate)))
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
          watch: {
              'request.EventDates'(val, oval) {
                  if (!this.request.IsSame) {
                      if (val.length == 1) {
                          this.request.Events.length = 1
                          this.request.IsSame = true
                      } else if (val.length != oval.length) {
                          this.matchMultiEvent()
                      }
                  } else {
                      if (this.request.Events.length > 1) {
                          //remove the rest because they said everything will be the same
                          this.request.Events.length = 1
                          this.request.IsSame = true
                      }
                  }
              },
              'request.IsSame'(val) {
                  if (!val) {
                      this.matchMultiEvent()
                  } else {
                      if (this.request.Events.length > 1) {
                          //remove the rest because they said everything will be the same
                          this.request.Events.length = 1
                      }
                  }
              },
              'request.needsSpace'(val) {
                  if (!val) {
                      for (var i = 0; i < this.request.Events.length; i++) {
                          this.request.Events[i].Rooms = []
                          this.request.Events[i].ExpectedAttendance = null
                          this.request.Events[i].Checkin = false
                          this.request.Events[i].SupportTeam = false
                      }
                  }
              },
              'request.needsOnline'(val) {
                  if (!val) {
                      for (var i = 0; i < this.request.Events.length; i++) {
                          this.request.Events[i].EventURL = ''
                          this.request.Events[i].ZoomPassword = ''
                          this.request.Events[i].OnlineFee = ''
                          let idx = this.request.Events[i].FeeType.indexOf('Online Fee')
                          this.request.Events[i].FeeType.splice(idx, 1)
                      }
                  }
              },
              'request.needsReg'(val) {
                  if (!val) {
                      for (var i = 0; i < this.request.Events.length; i++) {
                          this.request.Events[i].RegistrationDate = ''
                          this.request.Events[i].RegistrationEndDate = ''
                          this.request.Events[i].RegistrationEndTime = ''
                          this.request.Events[i].FeeType = []
                          this.request.Events[i].Fee = ''
                          this.request.Events[i].CoupleFee = ''
                          this.request.Events[i].OnlineFee = ''
                          this.request.Events[i].Sender = ''
                          this.request.Events[i].SenderEmail = ''
                          this.request.Events[i].ThankYou = ''
                          this.request.Events[i].TimeLocation = ''
                          this.request.Events[i].AdditionalDetails = ''
                      }
                  }
              },
              'request.needsCatering'(val) {
                  if (!val) {
                      for (var i = 0; i < this.request.Events.length; i++) {
                          this.request.Events[i].Vendor = ''
                          this.request.Events[i].BudgetLine = ''
                          this.request.Events[i].Menu = ''
                          this.request.Events[i].FoodDelivery = true
                          this.request.Events[i].FoodTime = ''
                          this.request.Events[i].FoodDropOff = ''
                          this.request.Events[i].Drinks = []
                          this.request.Events[i].DrinkTime = ''
                          this.request.Events[i].ServingTeamAgree = false
                          this.request.Events[i].DrinkDropOff = ''
                          this.request.Events[i].CCVendor = ''
                          this.request.Events[i].CCBudgetLine = ''
                          this.request.Events[i].CCMenu = ''
                          this.request.Events[i].CCFoodTime = ''
                      }
                  }
              },
              'request.needsChildcare'(val) {
                  if (!val) {
                      for (var i = 0; i < this.request.Events.length; i++) {
                          this.request.Events[i].CCStartTime = ''
                          this.request.Events[i].CCEndTime = ''
                          this.request.Events[i].ChildCareOptions = []
                          this.request.Events[i].EstimatedKids = ''
                      }
                  }
              },
              'request.needsAccom'(val) {
                  if (!val) {
                      for (var i = 0; i < this.request.Events.length; i++) {
                          this.request.Events[i].TechNeeds = []
                          this.request.Events[i].TechDescription = ''
                          if (!this.request.needsCatering) {
                              this.request.Events[i].Drinks = []
                              this.request.Events[i].DrinkTime = ''
                              this.request.Events[i].ServingTeamAgree = false
                              this.request.Events[i].DrinkDropOff = ''
                          }
                          this.request.Events[i].ShowOnCalendar = false
                          this.request.Events[i].PublicityBlurb = ''
                          this.request.Events[i].SetUp = ''
                          this.request.Events[i].SetUpImage = null
                      }
                  }
              },
              'request.needsPub'(val) {
                  if (!val) {
                      for (var i = 0; i < this.request.Events.length; i++) {
                          this.request.WhyAttendSixtyFive = ""
                          this.request.TargetAudience = ""
                          this.request.EventIsSticky = false
                          this.request.PublicityStartDate = ""
                          this.request.PublicityEndDate = ""
                          this.request.PublicityStrategies = ""
                          this.request.WhyAttendNinety = ""
                          this.request.GoogleKeys = []
                          this.request.WhyAttendTen = ""
                          this.request.VisualIdeas = ""
                          this.request.Stories = [{ Name: "", Email: "", Description: "" }]
                          this.request.WhyAttendTwenty = ""
                      }
                  }
              },
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
  .v-label--active {
    max-width: 133%;
    transform: translateY(-18px) scale(.75);
  }
  .v-window {
    overflow: visible !important;
  }
  .v-dialog:not(.v-dialog--fullscreen) {
    max-height: 80vh !important;
  }
  .v-dialog {
    margin-top: 100px !important;
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