<%@ Control Language="C#" AutoEventWireup="true"
CodeFile="EventSubmissionForm.ascx.cs"
Inherits="RockWeb.Plugins.com_thecrossingchurch.EventSubmission.EventSubmissionForm"
%> <%-- Add Vue and Vuetify CDN --%>
<!-- <script src="https://cdn.jsdelivr.net/npm/vue@2.6.14"></script> -->
<script src="https://cdn.jsdelivr.net/npm/vue@2.6.14/dist/vue.js"></script>
<script src="https://cdn.jsdelivr.net/npm/vuetify@2.x/dist/vuetify.js"></script>
<link
  href="https://fonts.googleapis.com/css?family=Roboto:100,300,400,500,700,900"
  rel="stylesheet"
/>
<link
  href="https://cdn.jsdelivr.net/npm/@mdi/font@6.x/css/materialdesignicons.min.css"
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
<asp:HiddenField ID="hfDoors" runat="server" />
<asp:HiddenField ID="hfMinistries" runat="server" />
<asp:HiddenField ID="hfBudgetLines" runat="server" />
<asp:HiddenField ID="hfReservations" runat="server" />
<asp:HiddenField ID="hfRequest" runat="server" />
<asp:HiddenField ID="hfUpcomingRequests" runat="server" />
<asp:HiddenField ID="hfIsAdmin" runat="server" />
<asp:HiddenField ID="hfIsSuperUser" runat="server" />
<asp:HiddenField ID="hfPersonName" runat="server" />
<asp:HiddenField ID="hfChangeRequest" runat="server" />

<div id="app" v-cloak>
  <v-app v-cloak>
    <div>
      <v-stepper alt-labels v-model="stepper" v-if="!showSuccess">
        <v-stepper-header>
          <v-stepper-step 
            v-if="isSuperUser" 
            step="1" 
            :class="`${stepper == 1 ? 'active' : ''}`"
            @click="skipToStep(1)"
            :complete="request.needsSpace || request.needsOnline || request.needsCatering || request.needsChildCare || request.needsAccom || request.needsReg || request.needsPub"
            :rules="[val => { if(errors.length == 0 || stepper == 1) { return true; } let err = errors.filter(x => x.page == 1); return err?.length == 0 || err[0].errors.length == 0 ? true : 'No'; }]"
          >
            Resources
          </v-stepper-step>
          <v-stepper-step 
            :step="`${isSuperUser ? 2 : 1 }`" 
            @click="skipToStep(isSuperUser ? 2 : 1)"
            :complete="isStepComplete(`${isSuperUser ? 2 : 1 }`)"
            :rules="[val => { if(errors.length == 0 || stepper == (isSuperUser ? 2 : 1)) { return true; } let err = errors.filter(x => x.page == `${isSuperUser ? 2 : 1 }`); return err?.length == 0 || err[0].errors.length == 0 ? true : 'No'; }]"
          >
            Basic Info
          </v-stepper-step>
          <v-stepper-step 
            v-for="(e, idx) in request.Events" 
            :key="idx" :step="`${isSuperUser ? (idx + 3) : (idx + 2) }`" 
            :complete="isStepComplete(`${isSuperUser ? (idx + 3) : (idx + 2) }`)" 
            @click="skipToStep(isSuperUser ? (idx + 3) : (idx + 2));"
            :rules="[val => { if(errors.length == 0 || stepper == (isSuperUser ? (idx + 3) : (idx + 2))) { return true; } let err = errors.filter(x => x.page == `${isSuperUser ? (idx + 3) : (idx + 2) }`); return err?.length == 0 || err[0].errors.length == 0 ? true : 'No'; }]"
          >
            <template v-if="request.Events.length == 1">Event Info</template>
            <template v-else>{{e.EventDate | formatDate}}</template>
          </v-stepper-step>
          <v-stepper-step 
            v-if="isSuperUser && request.needsPub" 
            :step="request.Events.length + 3" 
            @click="skipToStep(request.Events.length + 3)"
            :complete="isStepComplete(`${request.Events.length + 3}`)"
            :rules="[val => { if(errors.length == 0 || stepper == (request.Events.length + 3)) { return true; } let err = errors.filter(x => x.page == `${request.Events.length + 3}`); return err?.length == 0 || err[0].errors.length == 0 ? true : 'No'; }]"
          >
            Publicity
          </v-stepper-step>
        </v-stepper-header>
        <v-stepper-items>
          <v-stepper-content v-if="isSuperUser" step="1">
            <v-alert v-if="canEdit == false" type="error">You are not able to make changes to this request because it is currently {{request.Status}}.</v-alert>
            <v-alert v-if="canEdit && request.Status && !(request.Status == 'Submitted' || request.Status == 'In Progress'|| request.Status == 'Draft')" type="warning">Any changes made to this request will need to be approved.</v-alert>
            <v-alert type="error" v-if="currentErrors.length > 0" style="width: 100%;">
              You can't cheat the system...
              <ul>
                <li v-for="e in currentErrors">
                  {{e}}
                </li>
              </ul>
            </v-alert>
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
                  label="A physical space for an event"
                  hint="If you need any doors unlocked for this event, please be sure to include Special Accommodations below. Selecting a physical space does not assume unlocked doors."
                  :persistent-hint="request.needsSpace"
                ></v-switch>
                <!--
                <div class="date-warning overline" v-if="request.EventDates?.length > 0 && !request.needsSpace">
                  The last possible date to request a physical space {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
                </div>
                -->
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-switch
                  v-model="request.needsOnline"
                  label="Zoom"
                  hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
                  :persistent-hint="request.needsOnline"
                  :disabled="!isFuneralRequest && !originalRequest.needsOnline && twoWeeksTense == 'was'"
                ></v-switch>
                <div class="date-warning overline" v-if="!isFuneralRequest && request.EventDates?.length > 0 && !request.needsOnline">
                  The last possible date to request zoom {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
                </div>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-switch
                  v-model="request.needsCatering"
                  label="Food Request"
                  hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
                  :persistent-hint="request.needsCatering"
                  :disabled="!isFuneralRequest && !originalRequest.needsCatering && twoWeeksTense == 'was'"
                ></v-switch>
                <div class="date-warning overline" v-if="!isFuneralRequest && request.EventDates?.length > 0 && !request.needsCatering">
                  The last possible date to request catering {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
                </div>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-switch
                  v-model="request.needsChildCare"
                  label="Childcare"
                  hint="Requests involving childcare must be made at least 30 days in advance."
                  :persistent-hint="request.needsChildCare"
                  :disabled="!isFuneralRequest && !originalRequest.needsChildCare && thirtyDaysTense == 'was'"
                ></v-switch>
                <div class="date-warning overline" v-if="!isFuneralRequest && request.EventDates?.length > 0 && !request.needsChildCare">
                  The last possible date to request childcare {{thirtyDaysTense}} {{thirtyDaysBeforeEventStart}}
                </div>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-switch
                  v-model="request.needsAccom"
                  label="Special Accommodations (tech, drinks, web calendar, extensive set-up, doors unlocked)"
                  hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
                  :persistent-hint="request.needsAccom"
                  :disabled="!isFuneralRequest && !originalRequest.needsAccom && twoWeeksTense == 'was'"
                ></v-switch>
                <div class="date-warning overline" v-if="!isFuneralRequest && request.EventDates?.length > 0 && !request.needsAccom">
                  The last possible date to request special accommodations {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
                </div>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-switch
                  v-model="request.needsReg"
                  label="Registration"
                  hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
                  :persistent-hint="request.needsReg"
                  :disabled="!isFuneralRequest && !originalRequest.needsReg && twoWeeksTense == 'was'"
                ></v-switch>
                <div class="date-warning overline" v-if="!isFuneralRequest && request.EventDates?.length > 0 && !request.needsReg">
                  The last possible date to request registration {{twoWeeksTense}} {{twoWeeksBeforeEventStart}}
                </div>
              </v-col>
            </v-row>
            <v-row>
              <v-col>
                <v-switch
                  v-model="request.needsPub"
                  label="Publicity"
                  hint="Requests involving publicity must be made at least 6 weeks in advance."
                  :persistent-hint="request.needsPub"
                  :disabled="!isFuneralRequest && !originalRequest.needsPub && sixWeeksTense == 'was'"
                ></v-switch>
                <div class="date-warning overline" v-if="!isFuneralRequest && request.EventDates?.length > 0 && !request.needsPub">
                  The last possible date to request publicity {{sixWeeksTense}} {{sixWeeksBeforeEventStart}}
                </div>
              </v-col>
            </v-row>
            <div style="width: 100%; padding: 16px 0px;">
              <v-row>
                <v-col class="d-flex justify-end">
                  <v-btn color="primary" @click="next">Next</v-btn>
                  <br/>
                </v-col>
              </v-row>
            </div>
          </v-stepper-content>
          <v-stepper-content :step="`${isSuperUser ? 2 : 1 }`">
            <v-alert v-if="canEdit == false" type="error">You are not able to make changes to this request because it is currently {{request.Status}}.</v-alert>
            <v-alert v-if="!isSuperUser && canEdit && request.Status && !(request.Status == 'Submitted' || request.Status == 'Draft')" type="warning">Any changes made to this request will need to be approved.</v-alert>
            <v-form ref="form" v-model="formValid">
              <v-alert :type="`${triedSubmit ? 'error' : 'warning' }`" v-if="!isValid && currentErrors.length > 0" style="width: 100%;">
                <template v-if="triedSubmit">Please review your request and fix the following errors:</template>
                <template v-else>Please fix the following before you submit:</template>
                <ul>
                  <li v-for="e in currentErrors">
                    {{e}}
                  </li>
                </ul>
              </v-alert>
              <v-alert type="error" v-if="!isValid && triedSubmit && currentErrors.length == 0" style="width: 100%;">
                Please review each part of your request and fix all errors.
              </v-alert>
              <%-- Basic Request Information --%>
              <v-layout>
                <h3 class="primary--text" v-if="isSuperUser">Basic Information</h3>
                <h3 class="primary--text" v-else>Let's Design Your Event</h3>
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
                    :label="eventNameLabel"
                    v-model="request.Name"
                    :rules="[rules.required(request.Name, 'Event Name')]"
                  ></v-text-field>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <v-autocomplete
                    :label="eventMinistryLabel"
                    :items="ministries"
                    item-text="Value"
                    item-value="Id"
                    item-disabled="IsDisabled"
                    attach
                    v-model="request.Ministry"
                    :rules="[rules.required(request.Ministry, 'Ministry')]"
                    :hint="ministryHint"
                    persistent-hint
                  ></v-autocomplete>
                </v-col>
                <v-col cols="12" md="6">
                  <v-text-field
                    :label="eventContactLabel"
                    v-model="request.Contact"
                    :rules="[rules.required(request.Contact, 'Contact')]"
                  ></v-text-field>
                </v-col>
              </v-row>
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
                  <strong>Please select the date(s) of your <template v-if='requestedResources == "rooms"'>meeting</template><template v-else>event</template></strong>
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
                  <v-autocomplete
                    label="Selected Dates"
                    chips
                    attach
                    multiple
                    clearable
                    :rules="[rules.requiredArr(request.EventDates, 'Event Date')]"
                    :items="longDates"
                    item-text="text"
                    item-value="val"
                    v-model="request.EventDates"
                    :disabled="cannotChangeDates"
                  ></v-autocomplete>
                  <br/>
                  <div v-if="isExistingRequest && (originalRequest.EventDates.toString() != request.EventDates.toString())" class='overline'>
                    Please note that by modifying the date of your event, all support services and publicity strategies are subject to change and are not guaranteed as they were before.
                  </div>
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
            </v-form>
            <template v-if="request.IsSame">
              <br/>
              <v-row>
                <v-col>
                  <strong>What time will your <template v-if='requestedResources == "rooms"'>meeting</template><template v-else>event</template> begin and end?</strong>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <time-picker 
                    label="Start Time" 
                    :rules="[rules.required(request.Events[0].StartTime, 'Start Time'), rules.validTime(request.Events[0].StartTime, request.Events[0].EndTime, true)]" 
                    v-model="request.Events[0].StartTime" 
                    :value="request.Events[0].StartTime" 
                    :dates="request.EventDates" 
                    @quicksettime="(t) => request.Events[0].EndTime = t"
                    :quick-set-items='[
                      {"mine": "08:20 AM", "theirs": "09:25 AM", "title": "1st Service"},
                      {"mine": "09:35 AM", "theirs": "10:40 AM", "title": "2nd Service"},
                      {"mine": "10:50 AM", "theirs": "11:55 AM", "title": "3rd Service"}
                    ]'
                  ></time-picker>
                </v-col>
                <v-col cols="12" md="6">
                  <time-picker 
                    label="End Time" 
                    :rules="[rules.required(request.Events[0].EndTime, 'End Time'), rules.validTime(request.Events[0].EndTime, request.Events[0].StartTime, false)]" 
                    v-model="request.Events[0].EndTime" 
                    :value="request.Events[0].EndTime" 
                    :dates="request.EventDates" 
                    :quick-set-items='[
                      {"mine": "09:25 AM", "theirs": "08:20 AM", "title": "1st Service"},
                      {"mine": "10:40 AM", "theirs": "09:35 AM", "title": "2nd Service"},
                      {"mine": "11:55 AM", "theirs": "10:50 AM", "title": "3rd Service"}
                    ]'
                    @quicksettime="(t) => request.Events[0].StartTime = t"></time-picker>
                </v-col>
              </v-row>
              <br/>
              <span>Note that the time and date of your <template v-if='requestedResources == "rooms"'>meeting</template><template v-else>event</template> will influence the list of spaces to choose from based on availablitiy. Changing your date or time after selecting a space could remove a previously selected space.</span>
            </template>
            <div style="width: 100%; padding: 16px 0px;">
              <v-row>
                <v-col>
                  <v-btn v-if="isSuperUser" color="secondary" @click="prev">Back</v-btn>
                </v-col>
                <v-col class="d-flex justify-end">
                  <v-btn color="accent" v-if="isSuperUser" :disabled="request.Status != 'Draft'" style="margin-right: 8px;" @click="saveDraft">
                    <v-icon>mdi-content-save</v-icon>
                    Save
                  </v-btn>
                  <v-btn color="primary" :disabled="!request.EventDates || (request.EventDates && request.EventDates.length == 0)" @click="next">Next</v-btn>
                </v-col>
              </v-row>
            </div>
          </v-stepper-content>
          <v-stepper-content v-for="(e, idx) in request.Events" :key="idx" :step="`${isSuperUser ? (idx + 3) : (idx + 2)}`">
            <div style="padding-top: 16px;"></div>
            <template v-if="e != null">
              <v-alert :type="`${triedSubmit ? 'error' : 'warning' }`" v-if="!isValid && currentErrors.length > 0" style="width: 100%;">
                <template v-if="triedSubmit">Please review your request and fix the following errors:</template>
                <template v-else>Please fix the following before you submit:</template>
                <ul>
                  <li v-for="err in currentErrors">
                    {{err}}
                  </li>
                </ul>
              </v-alert>
              <template v-if="isSuperUser">
                <template v-if="!request.IsSame">
                  <v-row>
                    <v-col>
                      <strong>What time will your event begin and end on {{e.EventDate | formatDate}}?</strong>
                    </v-col>
                  </v-row>
                  <v-row>
                    <v-col cols="12" md="6">
                      <time-picker 
                        label="Start Time" 
                        :rules="[rules.required(e.StartTime, 'Start Time'), rules.validTime(e.StartTime, e.EndTime, true)]" 
                        v-model="e.StartTime" 
                        :value="e.StartTime" 
                        :dates="[e.EventDate]" 
                        @quicksettime="(t) => e.EndTime = t"
                        :quick-set-items='[
                          {"mine": "08:20 AM", "theirs": "09:25 AM", "title": "1st Service"},
                          {"mine": "09:35 AM", "theirs": "10:40 AM", "title": "2nd Service"},
                          {"mine": "10:50 AM", "theirs": "11:55 AM", "title": "3rd Service"}
                        ]'
                      ></time-picker>
                    </v-col>
                    <v-col cols="12" md="6">
                      <time-picker 
                        label="End Time" 
                        :rules="[rules.required(e.EndTime, 'End Time'), rules.validTime(e.EndTime, e.StartTime, false)]" 
                        v-model="e.EndTime" 
                        :value="e.EndTime" 
                        :dates="[e.EventDate]" 
                        :quick-set-items='[
                          {"mine": "09:25 AM", "theirs": "08:20 AM", "title": "1st Service"},
                          {"mine": "10:40 AM", "theirs": "09:35 AM", "title": "2nd Service"},
                          {"mine": "11:55 AM", "theirs": "10:50 AM", "title": "3rd Service"}
                        ]'
                        @quicksettime="(t) => e.StartTime = t"
                      ></time-picker>
                    </v-col>
                  </v-row>
                </template>
                <%-- Space Information --%>
                <template v-if="request.needsSpace">
                  <space :e="e" :request="request" :existing="existingRequests" :ref="`spaceloop${3+idx}`" v-on:updatespace="updateSpace"></space>
                </template>
                <%-- Online Information --%>
                <template v-if="request.needsOnline">
                  <zoom :e="e" :request="request" :existing="existingRequests" :ref="`zoomloop${3+idx}`" v-on:updatezoom="updateZoom"></zoom>
                </template>
                <%-- Catering Information --%>
                <template v-if="request.needsCatering">
                  <catering :e="e" :request="request" :ref="`cateringloop${3+idx}`" v-on:updatecatering="updateCatering"></catering>
                </template>
                <%-- Childcare Info --%>
                <template v-if="request.needsChildCare">
                  <childcare :e="e" :request="request" :ref="`childcareloop${3+idx}`" v-on:updatechildcare="updateChildcare"></childcare>
                </template>
                <%-- Special Accommodations Info --%>
                <template v-if="request.needsAccom">
                  <accom :e="e" :request="request" :ref="`accomloop${3+idx}`" v-on:updateaccom="updateAccom"></accom>
                </template>
                <%-- Registration Information --%>
                <template v-if="request.needsReg">
                  <registration :e="e" :request="request" :earliest-pub-date="earliestPubDate" :ref="`regloop${3+idx}`" v-on:updatereg="updateReg"></registration>
                  <v-row v-if="request.EventDates && request.EventDates.length > 1 && request.IsSame">
                    <v-col>
                      <v-switch
                        :label="`Do each of these occurrences require separate links? (${boolToYesNo(request.EventsNeedSeparateLinks)})`"
                        v-model="request.EventsNeedSeparateLinks"
                      ></v-switch>
                    </v-col>
                  </v-row>
                </template>
              </template>
              <template v-else>
                <%-- Time Info --%>
                <template v-if="!request.IsSame">
                  <v-row>
                    <v-col>
                      <strong>What time will your event begin and end?</strong><br/>
                      <span>Note that the time and date of your <template v-if='requestedResources == "rooms"'>meeting</template><template v-else>event</template> will influence the list of spaces to choose from based on availablitiy. Changing your date or time after selecting a space could remove a previously selected space.</span>
                    </v-col>
                  </v-row>
                  <v-row>
                    <v-col cols="12" md="6">
                      <time-picker 
                        label="Start Time" 
                        :rules="[rules.required(e.StartTime, 'Start Time'), rules.validTime(e.StartTime, e.EndTime, true)]" 
                        v-model="e.StartTime" 
                        :value="e.StartTime" 
                        :dates="[e.EventDate]" 
                        @quicksettime="(t) => e.EndTime = t"
                        :quick-set-items='[
                          {"mine": "08:20 AM", "theirs": "09:25 AM", "title": "1st Service"},
                          {"mine": "09:35 AM", "theirs": "10:40 AM", "title": "2nd Service"},
                          {"mine": "10:50 AM", "theirs": "11:55 AM", "title": "3rd Service"}
                        ]'
                      ></time-picker>
                    </v-col>
                    <v-col cols="12" md="6">
                      <time-picker 
                        label="End Time" 
                        :rules="[rules.required(e.EndTime, 'End Time'), rules.validTime(e.EndTime, e.StartTime, false)]" 
                        v-model="e.EndTime" 
                        :value="e.EndTime" 
                        :dates="[e.EventDate]" 
                        :quick-set-items='[
                          {"mine": "09:25 AM", "theirs": "08:20 AM", "title": "1st Service"},
                          {"mine": "10:40 AM", "theirs": "09:35 AM", "title": "2nd Service"},
                          {"mine": "11:55 AM", "theirs": "10:50 AM", "title": "3rd Service"}
                        ]'
                        @quicksettime="(t) => e.StartTime = t"
                      ></time-picker>
                    </v-col>
                  </v-row>
                </template>
                <space :e="e" :request="request" :existing="existingRequests" :ref="`spaceloop${2+idx}`" v-on:updatespace="updateSpace"></space>
                <drinks :e="e" :request="request" v-on:updateaccom="updateAccom" :ref="`drinkloop${2+idx}`"></drinks>
              </template>
              <%-- Notes --%>
              <template v-if="request.needsOnline || request.needsCatering || request.needsChildCare || request.needsAccom || request.needsReg || request.needsPub || isInfrastructureRequest">
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
              </template>
              <%-- Pre-Approval Notice --%>
              <template v-if="request.needsSpace && !(request.needsOnline || request.needsCatering || request.needsChildCare || request.needsAccom || request.needsReg || request.needsPub)">
                <strong>*Your request may be pre-approved. </strong>
                <v-menu attach>
                  <template v-slot:activator="{ on, attrs }">
                    <div style="display: inline-block;" v-bind="attrs" v-on="on" class='accent-text'>
                      <strong>Click here to view requirements for pre-approval.</strong>
                    </div>
                  </template>
                  <v-list>
                    <v-list-item>
                      <ul>
                        <li>
                          The meeting dates are all within the next 14 days. <br/>
                          Before {{ preApprovalDate | formatDate }}
                        </li>
                        <li>
                          The meetings take place during regular business hours. <br/>
                          Mon-Fri: 9am-9pm, Sunday: 1pm-9pm
                        </li>
                        <li>The meetings involve no more than 30 people.</li>
                        <li>The meetings are not in the Auditorium or Gym.</li>
                      </ul>
                    </v-list-item>
                  </v-list>
                </v-menu>
              </template>
            </template>
            <div style="width: 100%; padding: 16px 0px;">
              <v-row>
                <v-col>
                  <v-btn color="secondary" @click="prev">Back</v-btn>
                </v-col>
                <v-col class="d-flex justify-end">
                  <v-btn color="accent" v-if="isSuperUser" :disabled="request.Status != 'Draft'" style="margin-right: 8px;" @click="saveDraft">
                    <v-icon>mdi-content-save</v-icon>
                    Save
                  </v-btn>
                  <v-btn color="primary" :disabled="!minimalRequiremnts && (idx == (request.Events.length - 1) && !request.needsPub) && canEdit" @click="next">
                    <template v-if="(idx == (request.Events.length - 1) && !request.needsPub) && canEdit">
                      {{( request.Status != 'Draft' ? 'Update' : 'Submit')}}
                    </template>
                    <template v-else>Next</template>
                  </v-btn>
                </v-col>
              </v-row>
            </div>
          </v-stepper-content>
          <v-stepper-content v-if="isSuperUser && request.needsPub" :step="request.Events.length + 3">
            <%-- Publicity Information --%>
            <publicity :request="request" :earliest-pub-date="earliestPubDate" ref="publicityloop"></publicity>
            <div style="width: 100%; padding: 16px 0px;">
              <v-row>
                <v-col>
                  <v-btn color="secondary" @click="prev">Back</v-btn>
                </v-col>
                <v-col class="d-flex justify-end">
                  <v-btn color="accent" v-if="isSuperUser" :disabled="request.Status != 'Draft'" style="margin-right: 8px;" @click="saveDraft">
                    <v-icon>mdi-content-save</v-icon>
                    Save
                  </v-btn>
                  <v-btn color="primary" :disabled="!minimalRequiremnts" @click="next">
                    <template>
                      {{( request.Status != 'Draft' ? 'Update' : 'Submit')}}
                    </template>
                  </v-btn>
                </v-col>
              </v-row>
            </div>
          </v-stepper-content>
        </v-stepper-items>
      </v-stepper>
      <Rock:BootstrapButton
        runat="server"
        ID="btnSubmit"
        CssClass="btn-hidden"
        OnClick="Submit_Click"
      />  
      <Rock:BootstrapButton
        runat="server"
        ID="btnSave"
        CssClass="btn-hidden"
        OnClick="Save_Click"
      />  
      <v-card v-if="showSuccess">
        <v-card-text>
          <v-alert v-if="isExistingRequest" type="success">
            <template v-if="noChangesMade">
              No changes were made to this request.
            </template>
            <template v-else>
              This request has been updated.
            </template>
          </v-alert>
          <v-alert v-else type="success">
            <template v-if="isPreApproved">
              Your request has been pre-approved! You will receive a confirmation
              email now with the details of your request. 
            </template>
            <template v-else>
              Your request has been submitted! You will receive a confirmation
              email now with the details of your request, when it has been
              approved by the Events Director you will receive an email securing
              your reservation with any additional information from the Events
              Director.
            </template>
          </v-alert>
          <v-alert type="warning" v-if="reasonNotApproved.length > 0">
            Your request was not Pre-Approved for the following reasons:
            <v-list dense style="background-color:transparent; color: #fff;">
              <v-list-item v-for="(r,idx) in reasonNotApproved" :key="idx">
                {{r}}
              </v-list-item>
            </v-list>
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
      <v-dialog
        v-model="saveDialog"
        v-if="saveDialog"
        max-width="850px"
      >
        <v-card>
          <v-card-title></v-card-title>
          <v-card-text>
            Your request needs a name and date to be able to save it.
          </v-card-text>
          <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="secondary" @click="saveDialog = false;">
              Close
            </v-btn> 
          </v-card-actions>
        </v-card>
      </v-dialog>
    </div>
  </v-app>
</div>
<script type="module">
    import timePickerVue from '/Scripts/com_thecrossingchurch/EventSubmission/TimePicker.js?v=1.0.7';
    import spaceVue from '/Scripts/com_thecrossingchurch/EventSubmission/Space.js?v=1.0.7';
    import zoomVue from '/Scripts/com_thecrossingchurch/EventSubmission/Zoom.js?v=1.0.7';
    import registrationVue from '/Scripts/com_thecrossingchurch/EventSubmission/Registration.js?v=1.0.7';
    import cateringVue from '/Scripts/com_thecrossingchurch/EventSubmission/Catering.js?v=1.0.7';
    import childcareVue from '/Scripts/com_thecrossingchurch/EventSubmission/Childcare.js?v=1.0.7';
    import publicityVue from '/Scripts/com_thecrossingchurch/EventSubmission/Publicity.js?v=1.0.7';
    import accomVue from '/Scripts/com_thecrossingchurch/EventSubmission/SpecialAccom.js?v=1.0.7';
    import drinksVue from '/Scripts/com_thecrossingchurch/EventSubmission/Drinks.js?v=1.0.7';
    import datePicker from '/Scripts/com_thecrossingchurch/EventSubmission/DatePicker.js?v=1.0.7';
    document.addEventListener("DOMContentLoaded", function () {
        Vue.component("time-picker", timePickerVue);
        Vue.component("space", spaceVue);
        Vue.component("zoom", zoomVue);
        Vue.component("registration", registrationVue);
        Vue.component("catering", cateringVue);
        Vue.component("childcare", childcareVue);
        Vue.component("accom", accomVue);
        Vue.component("drinks", drinksVue);
        Vue.component("publicity", publicityVue);
        Vue.component("date-picker", datePicker);
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
                consle: console,
                panel: 1,
                stepper: 1,
                currentEvent: null,
                currentIdx: null,
                showSuccess: false,
                isPreApproved: false,
                reasonNotApproved: [],
                noChangesMade: false,
                request: {
                    needsSpace: false,
                    needsOnline: false,
                    needsPub: false,
                    needsReg: false,
                    needsCatering: false,
                    needsChildCare: false,
                    needsAccom: false,
                    IsSame: true,
                    Status: 'Draft',
                    IsValid: false,
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
                            Rooms: [],
                            InfrastructureSpace: "",
                            NumTablesRound: null,
                            NumTablesRect: null,
                            TableType: [],
                            NumChairsRound: null,
                            NumChairsRect: null,
                            NeedsTableCloths: false,
                            Checkin: false,
                            SupportTeam: false,
                            EventURL: "",
                            ZoomPassword: "",
                            ThankYou: "",
                            TimeLocation: "",
                            AdditionalDetails: "",
                            Sender: "",
                            SenderEmail: "",
                            NeedsReminderEmail: false,
                            ReminderSender: "",
                            ReminderSenderEmail: "",
                            ReminderTimeLocation: "",
                            ReminderAdditionalDetails: "",
                            RegistrationDate: "",
                            RegistrationEndDate: "",
                            RegistrationEndTime: "",
                            FeeType: [],
                            FeeBudgetLine: "",
                            Fee: null,
                            CoupleFee: null,
                            OnlineFee: null,
                            Vendor: "",
                            Menu: "",
                            FoodDelivery: true,
                            FoodTime: "",
                            FoodDropOff: "",
                            Drinks: [],
                            DinkTime: "",
                            ServingTeamAgree: false,
                            DrinkDropOff: "",
                            BudgetLine: "",
                            CCVendor: "",
                            CCMenu: "",
                            CCFoodTime: "",
                            CCBudgetLine: "",
                            ChildCareOptions: [],
                            EstimatedKids: null,
                            CCStartTime: '',
                            CCEndTime: '',
                            TechNeeds: "",
                            TechDescription: "",
                            ShowOnCalendar: false,
                            PublicityBlurb: "",
                            SetUp: "",
                            SetUpImage: null,
                            NeedsDoorsUnlocked: false,
                            Doors: [],
                            NeedsMedical: false,
                            NeedsSecurity: false
                        }
                    ],
                    EventDates: [],
                    EventsNeedSeparateLinks: false,
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
                    ValidSections: [],
                    ValidStepperSections: [],
                    canEdit: true
                },
                originalRequest: {},
                existingRequests: [],
                rooms: [],
                doors: [],
                ministries: [],
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
                                return `You cannot have more than ${cap} ${cap == 1 ? "person" : "people"} in the selected space`;
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
                            return (isAfter || `${isStart ? "Start time must come before end time" : "End time must come after start time"}`);
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
                },
                valid: true,
                errors: [],
                formValid: true,
                onlineFormValid: true,
                timeFormValid: true,
                hasConflictsOrTimeIssue: false,
                dialog: false,
                conflictingRequestMsg: "",
                beforeHoursMsg: "",
                afterHoursMsg: "",
                triedSubmit: false,
                tab: 0,
                isAdmin: false,
                isSuperUser: false,
                dateChangeMessage: '',
                changeDialog: false,
                saveDialog: false
            },
            created() {
                this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value)
                this.doors = JSON.parse($('[id$="hfDoors"]')[0].value)
                this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value)
                this.ministries.forEach(m => {
                  m.IsDisabled = !m.IsActive
                })
                let isAd = $('[id$="hfIsAdmin"]')[0].value
                if (isAd == 'True') {
                    this.isAdmin = true
                }
                let isSU = $('[id$="hfIsSuperUser"]')[0].value
                if (isSU == 'True') {
                    this.isSuperUser = true
                    this.panel = 0
                } else {
                    this.request.needsSpace = true
                }
                this.request.Contact = $('[id$="hfPersonName"]')[0].value
                let req = $('[id$="hfRequest"]')[0].value
                if (req) {
                    let parsed = JSON.parse(req)
                    this.request = JSON.parse(parsed.Value)
                    this.request.Id = parsed.Id
                    this.request.Status = parsed.RequestStatus
                    this.request.CreatedBy = parsed.CreatedBy
                    this.request.canEdit = parsed.CanEdit
                    this.request.SubmittedOn = parsed.Active
                    this.originalRequest = JSON.parse(JSON.stringify(this.request))
                }
                if (!this.request.ValidStepperSections) {
                    this.request.ValidStepperSections = []
                }
                if (this.request.ValidSections == null) {
                    this.request.ValidSections = []
                }
                this.existingRequests = JSON.parse($('[id$="hfUpcomingRequests"]')[0].value)
                this.existingRequests = this.existingRequests.map(e => {
                    let obj = JSON.parse(e.data)
                    obj.Id = e.Id
                    return obj
                })
                window["moment-range"].extendMoment(moment)
            },
            mounted() {
                let query = new URLSearchParams(window.location.search)
                let success = query.get('ShowSuccess')
                if (success) {
                    if (success == "true") {
                        this.showSuccess = true
                    }
                }
                let preApproved = query.get('PreApproved')
                if (preApproved) {
                    if (preApproved == "true") {
                        this.isPreApproved = true
                    }
                } else {
                    let reason = query.get('Reason')
                    if (reason) {
                        this.reasonNotApproved = reason.split(';')
                    }
                }
                let noChange = query.get('NoChange')
                if (noChange) {
                    if (noChange == "true") {
                        this.noChangesMade = true
                    }
                }
                if(this.request.Status == 'Draft' && this.request.Id > 0) {
                  this.validate()
                  this.showValidation()
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
                    if (items.length > 1) {
                        items[items.length - 1] = "and " + items[items.length - 1]
                    }
                    return items.join(", ");
                },
                earliestDate() {
                    let eDate = new moment();
                    if (this.request.Id > 0 && this.request.Status != 'Draft') {
                        eDate = new moment(this.request.SubmittedOn)
                    }
                    if (this.request.needsPub) {
                        eDate = moment(eDate).add(6, "weeks").add(1, "day")
                    } else if (
                        this.request.needsChildCare ||
                        this.request.ExpectedAttendance > 250
                    ) {
                        eDate = moment().add(30, "days");
                        this.request.EventDates.forEach((itm, i) => {
                            if (!moment(itm).isSameOrAfter(moment(eDate).format("yyyy-MM-DD"))) {
                                this.request.EventDates.splice(i, 1)
                            }
                        });
                    } else if (
                        this.request.needsOnline ||
                        this.request.needsCatering ||
                        this.request.needsReg ||
                        this.request.needsAccom
                    ) {
                        eDate = moment(eDate).add(14, "days")
                    }
                    //Override for Funerals
                    if (this.isFuneralRequest) {
                        eDate = new moment()
                    }
                    return moment(eDate).format("yyyy-MM-DD")
                },
                earliestPubDate() {
                    let eDate = new moment();
                    if (this.request.Id > 0 && this.request.Status != 'Draft') {
                        eDate = new moment(this.request.SubmittedOn)
                    }
                    eDate = moment(eDate).add(21, "days")
                    //Override for Funerals
                    if (this.isFuneralRequest) {
                        eDate = new moment()
                    }
                    return moment(eDate).format("yyyy-MM-DD");
                },
                twoWeeksBeforeEventStart() {
                    if (this.request.EventDates?.length > 0) {
                        let first = this.request.EventDates.map((i) => {
                            return new moment(i)
                        }).sort().pop()
                        return new moment(first).subtract(2, 'weeks').format("dddd, MMMM Do")
                    }
                },
                thirtyDaysBeforeEventStart() {
                    if (this.request.EventDates?.length > 0) {
                        let first = this.request.EventDates.map((i) => {
                            return new moment(i)
                        }).sort().pop()
                        return new moment(first).subtract(30, 'days').format("dddd, MMMM Do")
                    }
                },
                sixWeeksBeforeEventStart() {
                    if (this.request.EventDates?.length > 0) {
                        let first = this.request.EventDates.map((i) => {
                            return new moment(i)
                        }).sort().pop()
                        return new moment(first).subtract(6, 'weeks').format("dddd, MMMM Do")
                    }
                },
                twoWeeksTense() {
                    if (this.request.EventDates?.length > 0) {
                        let today = new moment()
                        today.set({
                            hour: 0,
                            minute: 0,
                            second: 0
                        })
                        let first = this.request.EventDates.map((i) => {
                            return new moment(i)
                        }).sort().pop().subtract(2, 'weeks')
                        if (first.isAfter(today) || first.isSame(today, 'day')) {
                            return 'is'
                        }
                        return 'was'
                    }
                },
                thirtyDaysTense() {
                    if (this.request.EventDates?.length > 0) {
                        let today = new moment()
                        today.set({
                            hour: 0,
                            minute: 0,
                            second: 0
                        })
                        let first = this.request.EventDates.map((i) => {
                            return new moment(i)
                        }).sort().pop().subtract(30, 'days')
                        if (first.isAfter(today) || first.isSame(today, 'day')) {
                            return 'is'
                        }
                        return 'was'
                    }
                },
                sixWeeksTense() {
                    if (this.request.EventDates?.length > 0) {
                        let today = new moment()
                        today.set({
                            hour: 0,
                            minute: 0,
                            second: 0
                        })
                        let first = this.request.EventDates.map((i) => {
                            return new moment(i)
                        }).sort().pop().subtract(6, 'weeks')
                        if (first.isAfter(today) || first.isSame(today, 'day')) {
                            return 'is'
                        }
                        return 'was'
                    }
                },
                longDates() {
                    return this.request.EventDates.map((i) => {
                        return { text: moment(i).format("dddd, MMMM Do yyyy"), val: i };
                    });
                },
                isValid() {
                    let isValidForm = true
                    this.errors.forEach(e => {
                        if (e.errors && e.errors.length > 0) {
                            isValidForm = false
                        }
                    })
                    return isValidForm
                },
                currentErrors() {
                    let e = this.errors?.filter(err => { return err.page == this.stepper })
                    return e?.length > 0 ? e[0].errors : []
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
                    if (this.request.Id > 0 && !(this.request.Status == 'Submitted' || this.request.Status == 'Draft')) {
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
                    if (this.request.Id > 0 && !(this.request.Status == 'Submitted' || this.request.Status == 'Draft')) {
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
                },
                ministryHint() {
                    let min = this.ministries.filter(m => {
                        return m.Id == this.request.Ministry
                    })
                    if (min.length > 0) {
                        if (min[0].IsPersonal) {
                            return 'Personal requests are for spaces only, the Operations team provides no resources.'
                        }
                    }
                    return ''
                },
                eventNameLabel() {
                    if (this.requestedResources == "rooms") {
                        return "Meeting name on calendar"
                    }
                    return "Event name on calendar"
                },
                eventMinistryLabel() {
                    if (this.requestedResources == "rooms") {
                        return "Which ministry is sponsoring this meeting?"
                    }
                    return "Which ministry is sponsoring this event?"
                },
                eventContactLabel() {
                    if (this.requestedResources == "rooms") {
                        return "Who is the ministry contact for this meeting?"
                    }
                    return "Who is the ministry contact for this event?"
                },
                preApprovalDate() {
                    return moment().add(14, 'days')
                },
                isFuneralRequest() {
                    let ministryName = this.ministries.filter(m => { return m.Id == this.request.Ministry })[0]?.Value
                    if (ministryName?.toLowerCase().includes("funeral")) {
                        return true
                    }
                    return false
                },
                isInfrastructureRequest() {
                    let ministryName = this.ministries.filter(m => { return m.Id == this.request.Ministry })[0]?.Value
                    if (ministryName?.toLowerCase().includes("infrastructure")) {
                        return true
                    }
                    return false
                },
                minimalRequiremnts() {
                    let hasTime = true
                    this.request.Events.forEach(e => {
                        if (!e.StartTime || !e.EndTime || e.StartTime.includes("null") || e.EndTime.includes("null")) {
                            hasTime = false
                        }
                    })
                    let hasSpace = true
                    if (this.request.needsSpace) {
                        this.request.Events.forEach(e => {
                            if (this.isInfrastructureRequest) {
                                if (!e.InfrastructureSpace && (!e.Rooms || e.Rooms.length == 0)) {
                                    hasSpace = false
                                }
                            } else {
                                if (!e.Rooms || e.Rooms.length == 0) {
                                    hasSpace = false
                                }
                            }
                        })
                    }
                    return this.canEdit && this.request.Name && this.request.EventDates && hasTime && (this.request.EventDates.length > 0) && hasSpace
                }
            },
            methods: {
                boolToYesNo(val) {
                    if (val) {
                        return "Yes"
                    }
                    return "No"
                },
                skipToStep(step) {
                    this.validate()
                    if (this.isSuperUser) {
                        if (step > 2 && step < (this.request.Events.length + (this.request.needsPub ? 3 : 2))) {
                            //Is an event date need to set current index
                            this.currentIdx = step - 3
                        }
                    } else {
                        if (step > 1 && step < (this.request.Events.length + 1)) {
                            //Is an event date need to set current index
                            this.currentIdx = step - 2
                        }
                    }
                    this.currentEvent = this.request.Events[this.currentIdx]
                    this.stepper = step
                    window.scrollTo(0, 0);
                    if (this.isExistingRequest || (this.currentErrors && this.currentErrors.length > 0)) {
                        window.setTimeout(() => {
                            this.showValidation()
                        }, 500)
                    }
                },
                next() {
                    this.validate()
                    if (this.isSuperUser) {
                        let num = 2 + this.request.Events.length
                        if (this.request.needsPub) {
                            num++
                        }
                        if (this.stepper == 2) {
                            this.currentIdx = 0
                        } else if (this.stepper == num) {
                            //if (this.isValid) {
                            this.hasConflictsOrTimeIssue = false
                            this.checkForConflicts()
                            this.checkTimeMeetsRequirements()
                            if (this.hasConflictsOrTimeIssue) {
                                this.dialog = true
                            } else {
                                this.submit()
                            }
                        } else {
                            this.currentIdx++
                        }
                    } else {
                        if (this.stepper == 1) {
                            this.currentIdx = 0
                        } else if (this.stepper == (1 + this.request.Events.length)) {
                            //if (this.isValid) {
                            this.hasConflictsOrTimeIssue = false
                            this.checkForConflicts()
                            this.checkTimeMeetsRequirements()
                            if (this.hasConflictsOrTimeIssue) {
                                this.dialog = true
                            } else {
                                this.submit()
                            }
                        } else {
                            this.currentIdx++
                        }
                    }
                    this.currentEvent = this.request.Events[this.currentIdx]
                    this.stepper++
                    window.scrollTo(0, 0)
                    if (this.isExistingRequest || (this.currentErrors?.length > 0)) {
                        window.setTimeout(() => {
                            this.showValidation()
                        }, 500)
                    }
                },
                prev() {
                    this.validate();
                    let tab = this.stepper
                    tab--
                    if (this.currentIdx > 0) {
                        this.currentIdx--
                        this.currentEvent = this.request.Events[this.currentIdx]
                    }
                    if (tab < 1) {
                        tab == 1
                    }
                    this.stepper = tab
                    window.scrollTo(0, 0);
                    if (this.isExistingRequest || (this.currentErrors && this.currentErrors.length > 0)) {
                        window.setTimeout(() => {
                            this.showValidation()
                        }, 500)
                    }
                },
                isStepComplete(step) {
                    let err = this.errors.filter(e => e.page == step)
                    return err[0]?.errors.length == 0 ? true : false
                },
                submit() {
                    $('#updateProgress').show();
                    $('[id$="hfRequest"]').val(JSON.stringify(this.request));
                    $('[id$="btnSubmit"')[0].click();
                },
                sendDateChangeRequest() {
                    this.changeDialog = false
                    $('[id$="hfChangeRequest"]').val(this.dateChangeMessage)
                    $('[id$="btnChangeRequest"')[0].click();
                },
                updateReg(indexes) {
                    this.request.Events[indexes.currIdx].RegistrationDate = this.request.Events[indexes.targetIdx].RegistrationDate
                    this.request.Events[indexes.currIdx].RegistrationEndDate = this.request.Events[indexes.targetIdx].RegistrationEndDate
                    this.request.Events[indexes.currIdx].RegistrationEndTime = this.request.Events[indexes.targetIdx].RegistrationEndTime
                    this.request.Events[indexes.currIdx].FeeType = this.request.Events[indexes.targetIdx].FeeType
                    this.request.Events[indexes.currIdx].FeeBudgetLine = this.request.Events[indexes.targetIdx].FeeBudgetLine
                    this.request.Events[indexes.currIdx].Fee = this.request.Events[indexes.targetIdx].Fee
                    this.request.Events[indexes.currIdx].CoupleFee = this.request.Events[indexes.targetIdx].CoupleFee
                    this.request.Events[indexes.currIdx].OnlineFee = this.request.Events[indexes.targetIdx].OnlineFee
                    this.request.Events[indexes.currIdx].Sender = this.request.Events[indexes.targetIdx].Sender
                    this.request.Events[indexes.currIdx].SenderEmail = this.request.Events[indexes.targetIdx].SenderEmail
                    this.request.Events[indexes.currIdx].ThankYou = this.request.Events[indexes.targetIdx].ThankYou
                    this.request.Events[indexes.currIdx].TimeLocation = this.request.Events[indexes.targetIdx].TimeLocation
                    this.request.Events[indexes.currIdx].AdditionalDetails = this.request.Events[indexes.targetIdx].AdditionalDetails
                    this.request.Events[indexes.currIdx].NeedsReminderEmail = this.request.Events[indexes.targetIdx].NeedsReminderEmail
                    this.request.Events[indexes.currIdx].ReminderSender = this.request.Events[indexes.targetIdx].ReminderSender
                    this.request.Events[indexes.currIdx].ReminderSenderEmail = this.request.Events[indexes.targetIdx].ReminderSenderEmail
                    this.request.Events[indexes.currIdx].ReminderTimeLocation = this.request.Events[indexes.targetIdx].ReminderTimeLocation
                    this.request.Events[indexes.currIdx].ReminderAdditionalDetails = this.request.Events[indexes.targetIdx].ReminderAdditionalDetails
                },
                updateSpace(indexes) {
                    this.request.Events[indexes.currIdx].Rooms = this.request.Events[indexes.targetIdx].Rooms
                    this.request.Events[indexes.currIdx].ExpectedAttendance = this.request.Events[indexes.targetIdx].ExpectedAttendance
                    this.request.Events[indexes.currIdx].Checkin = this.request.Events[indexes.targetIdx].Checkin
                    this.request.Events[indexes.currIdx].SupportTeam = this.request.Events[indexes.targetIdx].SupportTeam
                    this.request.Events[indexes.currIdx].NumTablesRound = this.request.Events[indexes.targetIdx].NumTablesRound
                    this.request.Events[indexes.currIdx].NumTablesRect = this.request.Events[indexes.targetIdx].NumTablesRect
                    this.request.Events[indexes.currIdx].TableType = this.request.Events[indexes.targetIdx].TableType
                    this.request.Events[indexes.currIdx].NumChairsRound = this.request.Events[indexes.targetIdx].NumChairsRound
                    this.request.Events[indexes.currIdx].NumChairsRect = this.request.Events[indexes.targetIdx].NumChairsRect
                    this.request.Events[indexes.currIdx].NeedsTableCloths = this.request.Events[indexes.targetIdx].NeedsTableCloths
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
                    this.request.Events[indexes.currIdx].NeedsDoorsUnlocked = this.request.Events[indexes.targetIdx].NeedsDoorsUnlocked
                    this.request.Events[indexes.currIdx].Doors = this.request.Events[indexes.targetIdx].Doors
                    this.request.Events[indexes.currIdx].NeedsMedical = this.request.Events[indexes.targetIdx].NeedsMedical
                    this.request.Events[indexes.currIdx].NeedsSecurity = this.request.Events[indexes.targetIdx].NeedsSecurity
                },
                updateZoom(indexes) {
                    this.request.Events[indexes.currIdx].EventURL = this.request.Events[indexes.targetIdx].EventURL
                    this.request.Events[indexes.currIdx].ZoomPassword = this.request.Events[indexes.targetIdx].ZoomPassword
                },
                checkForConflicts() {
                    this.request.HasConflicts = false
                    let conflictingMessage = []
                    let conflictingRequests = this.existingRequests.filter((r) => {
                        //r = JSON.parse(r);
                        let compareTarget = [], compareSource = []
                        //Build an object for each date to compare with 
                        if (r.Events[0].Rooms && r.Events[0].Rooms.length > 0) {
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
                        }
                        let conflicts = false
                        for (let x = 0; x < compareTarget.length; x++) {
                            for (let y = 0; y < compareSource.length; y++) {
                                if (compareTarget[x].Date == compareSource[y].Date) {
                                    //On same date
                                    //Check for conflicting rooms
                                    let conflictingRooms = []
                                    if (compareSource[y].Rooms && compareSource[y].Rooms.length > 0) {
                                        conflictingRooms = compareSource[y].Rooms.filter(value => compareTarget[x].Rooms.includes(value));
                                    }
                                    if (conflictingRooms.length > 0) {
                                        //Check they do not overlap with moment-range
                                        let cdStart = moment(`${compareTarget[x].Date} ${compareTarget[x].StartTime}`, `yyyy-MM-DD hh:mm A`);
                                        if (compareTarget[x].MinsStartBuffer) {
                                            cdStart = cdStart.subtract(r.MinsStartBuffer, "minute");
                                        }
                                        let cdEnd = moment(`${compareTarget[x].Date} ${compareTarget[x].EndTime}`, `yyyy-MM-DD hh:mm A`).subtract(1, 'minute');
                                        if (compareTarget[x].MinsEndBuffer) {
                                            cdEnd = cdEnd.add(compareTarget[x].MinsEndBuffer, "minute");
                                        }
                                        let cRange = moment.range(cdStart, cdEnd);
                                        let current = moment.range(
                                            moment(`${compareSource[y].Date} ${compareSource[y].StartTime}`, `yyyy-MM-DD hh:mm A`),
                                            moment(`${compareSource[y].Date} ${compareSource[y].EndTime}`, `yyyy-MM-DD hh:mm A`).subtract(1, 'minute')
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
                                            this.request.HasConflicts = true
                                        }
                                    }
                                }
                            }
                        }
                        return conflicts
                    });
                    if (conflictingRequests.length > 0) {
                        this.hasConflictsOrTimeIssue = true
                        this.conflictingRequestMsg = "There are conflicts with the following dates and rooms: " + conflictingMessage.join(", ")
                        this.request.HasConflicts = true
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
                            if ((parseInt(info[0]) > 9 && parseInt(info[0]) != 12) || (parseInt(info[0]) == 9 && info[1].split(' ')[0] != "00")) {
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
                                    this.afterHoursMsg = 'Due to the busyness of Sundays, please keep in mind that all events requested during church hours will need to be approved with special consideration from the Events Director.'
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
                        this.hasConflictsOrTimeIssue = true
                    }
                },
                validate() {
                    this.errors = []
                    let eventErrors = null
                    eventErrors = { page: this.stepper, errors: [] }
                    //Override for Funerals
                    let ministryName = this.ministries.filter(m => { return m.Id == this.request.Ministry })[0]?.Value
                    let resourceErrors = { page: 1, errors: [] }
                    if (this.isSuperUser && this.stepper < 3 && !ministryName?.toLowerCase().includes("funeral")) {
                        //Validate that the user didn't chnage the date to add a new resource then change the date back
                        if (this.request.needsOnline && (!this.originalRequest.needsOnline || this.request.Status == 'Draft') && this.twoWeeksTense == 'was') {
                            resourceErrors.errors.push("Your request for zoom is invalid, it has been removed")
                            this.request.needsOnline = false
                            this.originalRequest.needsOnline = false
                        }
                        if (this.request.needsCatering && (!this.originalRequest.needsCatering || this.request.Status == 'Draft') && this.twoWeeksTense == 'was') {
                            resourceErrors.errors.push("Your request for catering is invalid, it has been removed")
                            this.request.needsCatering = false
                            this.originalRequest.needsCatering = false
                        }
                        if (this.request.needsChildCare && (!this.originalRequest.needsChildCare || this.request.Status == 'Draft') && this.thirtyDaysTense == 'was') {
                            resourceErrors.errors.push("Your request for childcare is invalid, it has been removed")
                            this.request.needsChildCare = false
                            this.originalRequest.needsChildCare = false
                        }
                        if (this.request.needsAccom && (!this.originalRequest.needsAccom || this.request.Status == 'Draft') && this.twoWeeksTense == 'was') {
                            resourceErrors.errors.push("Your request for accommodations is invalid, it has been removed")
                            this.request.needsAccom = false
                            this.originalRequest.needsAccom = false
                        }
                        if (this.request.needsReg && (!this.originalRequest.needsReg || this.request.Status == 'Draft') && this.twoWeeksTense == 'was') {
                            resourceErrors.errors.push("Your request for registration is invalid, it has been removed")
                            this.request.needsReg = false
                            this.originalRequest.needsReg = false
                        }
                        if (this.request.needsPub && (!this.originalRequest.needsPub || this.request.Status == 'Draft') && this.sixWeeksTense == 'was') {
                            resourceErrors.errors.push("Your request for publicity is invalid, it has been removed")
                            this.request.needsPub = false
                            this.originalRequest.needsPub = false
                        }
                        if (resourceErrors.errors.length > 0) {
                            let ridx = -1
                            this.errors.forEach((e, i) => {
                                if (e.page == 1) {
                                    ridx = i
                                }
                            })
                            if (ridx > -1) {
                                this.errors[ridx].errors = resourceErrors.errors
                            } else {
                                this.errors.push(resourceErrors)
                            }
                        }
                    }
                    if (!this.request.ValidStepperSections[this.stepper]) {
                        while (this.request.ValidStepperSections.length < (this.stepper + 1)) {
                            this.request.ValidStepperSections.push({ step: this.request.ValidStepperSections.length, sections: [] })
                        }
                    }
                    if ((this.isSuperUser && this.stepper == 2) || (!this.isSuperUser && this.stepper == 1)) {
                        if (this.$refs.form) {
                            this.$refs.form.validate()
                            if (this.$refs.form.value) {
                                if (!this.request.ValidStepperSections[this.stepper].sections.includes("Basic")) {
                                    this.request.ValidStepperSections[this.stepper].sections.push("Basic")
                                }
                            } else {
                                this.request.ValidStepperSections[this.stepper].sections = []
                            }
                            this.$refs.form.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket)
                                }
                            })
                        }
                        if (this.$refs.startTime && this.request.IsSame) {
                            this.$refs.startTime?.$refs.timeForm?.validate()
                            this.$refs.startTime?.$refs.timeForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket.filter(eb => !!eb))
                                }
                            })
                        }
                        if (this.$refs.endTime && this.request.IsSame) {
                            this.$refs.endTime?.$refs.timeForm?.validate()
                            this.$refs.endTime?.$refs.timeForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket.filter(eb => !!eb))
                                }
                            })
                        }
                    }
                    if ((this.isSuperUser && this.stepper > 2) || (!this.isSuperUser && this.stepper > 1)) {
                        if (!this.request.IsSame) {
                            if (this.$refs[`startTimeLoop${this.stepper}`]) {
                                this.$refs[`startTimeLoop${this.stepper}`][0]?.$refs.timeForm?.validate()
                                this.$refs[`startTimeLoop${this.stepper}`][0]?.$refs.timeForm?.inputs.forEach((e) => {
                                    if (e.errorBucket && e.errorBucket.length) {
                                        eventErrors.errors.push(...e.errorBucket.filter(eb => !!eb))
                                    }
                                })
                            }
                            if (this.$refs[`endTimeLoop${this.stepper}`]) {
                                this.$refs[`endTimeLoop${this.stepper}`][0]?.$refs.timeForm?.validate()
                                this.$refs[`endTimeLoop${this.stepper}`][0]?.$refs.timeForm?.inputs.forEach((e) => {
                                    if (e.errorBucket && e.errorBucket.length) {
                                        eventErrors.errors.push(...e.errorBucket.filter(eb => !!eb))
                                    }
                                })
                            }
                        }
                        if (this.$refs[`spaceloop${this.stepper}`]) {
                            this.$refs[`spaceloop${this.stepper}`][0]?.$refs.spaceForm?.validate()
                            //Add or remove space from the valid sections list
                            if (this.$refs[`spaceloop${this.stepper}`][0]?.$refs.spaceForm?.value) {
                                if (!this.request.ValidStepperSections[this.stepper].sections.includes("Room")) {
                                    this.request.ValidStepperSections[this.stepper].sections.push("Room")
                                }
                            } else {
                                if (this.request.ValidStepperSections[this.stepper].sections.indexOf("Room") >= 0) {
                                    this.request.ValidStepperSections[this.stepper].sections.splice(this.request.ValidStepperSections[this.stepper].sections.indexOf("Room"), 1)
                                }
                            }
                            this.$refs[`spaceloop${this.stepper}`][0]?.$refs.spaceForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket)
                                }
                            })
                            //Filter null values out of room list 
                            this.request.Events.forEach((e) => {
                                if (e.Rooms && e.Rooms.length > 0) {
                                    e.Rooms = e.Rooms.filter(r => { return r != null })
                                }
                            })
                        }
                        if (this.$refs[`drinkloop${this.stepper}`]) {
                            this.$refs[`drinkloop${this.stepper}`][0]?.$refs.accomForm?.validate()
                            //Add or remove extra accomodations from the valid sections list
                            if (this.$refs[`drinkloop${this.stepper}`][0]?.$refs.accomForm?.value) {
                                if (!this.request.ValidStepperSections[this.stepper].sections.includes("Extra Resources")) {
                                    this.request.ValidStepperSections[this.stepper].sections.push("Extra Resources")
                                }
                            } else {
                                if (this.request.ValidStepperSections[this.stepper].sections.indexOf("Extra Resources") >= 0) {
                                    this.request.ValidStepperSections[this.stepper].sections.splice(this.request.ValidStepperSections[this.stepper].sections.indexOf("Extra Resources"), 1)
                                }
                            }
                            this.$refs[`drinkloop${this.stepper}`][0]?.$refs.accomForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket)
                                }
                            })
                        }
                        if (this.$refs[`zoomloop${this.stepper}`]) {
                            this.$refs[`zoomloop${this.stepper}`][0]?.$refs.zoomForm?.validate()
                            //Add or remove zoom from the valid sections list
                            if (this.$refs[`zoomloop${this.stepper}`][0]?.$refs.zoomForm?.value) {
                                if (!this.request.ValidStepperSections[this.stepper].sections.includes("Online Event")) {
                                    this.request.ValidStepperSections[this.stepper].sections.push("Online Event")
                                }
                            } else {
                                if (this.request.ValidStepperSections[this.stepper].sections.indexOf("Online Event") >= 0) {
                                    this.request.ValidStepperSections[this.stepper].sections.splice(this.request.ValidStepperSections[this.stepper].sections.indexOf("Online Event"), 1)
                                }
                            }
                            this.$refs[`zoomloop${this.stepper}`][0]?.$refs.zoomForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket)
                                }
                            })
                        }
                        if (this.$refs[`regloop${this.stepper}`]) {
                            this.$refs[`regloop${this.stepper}`][0]?.$refs.regForm?.validate()
                            //Add or remove registration from the valid sections list
                            if (this.$refs[`regloop${this.stepper}`][0]?.$refs.regForm?.value) {
                                if (!this.request.ValidStepperSections[this.stepper].sections.includes("Registration")) {
                                    this.request.ValidStepperSections[this.stepper].sections.push("Registration")
                                }
                            } else {
                                if (this.request.ValidStepperSections[this.stepper].sections.indexOf("Registration") >= 0) {
                                    this.request.ValidStepperSections[this.stepper].sections.splice(this.request.ValidStepperSections[this.stepper].sections.indexOf("Registration"), 1)
                                }
                            }
                            this.$refs[`regloop${this.stepper}`][0]?.$refs.regForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket)
                                }
                            })
                        }
                        if (this.$refs[`cateringloop${this.stepper}`]) {
                            this.$refs[`cateringloop${this.stepper}`][0]?.$refs.cateringForm?.validate()
                            //Add or remove catering from the valid sections list
                            if (this.$refs[`cateringloop${this.stepper}`][0]?.$refs.cateringForm?.value) {
                                if (!this.request.ValidStepperSections[this.stepper].sections.includes("Catering")) {
                                    this.request.ValidStepperSections[this.stepper].sections.push("Catering")
                                }
                            } else {
                                if (this.request.ValidStepperSections[this.stepper].sections.indexOf("Catering") >= 0) {
                                    this.request.ValidStepperSections[this.stepper].sections.splice(this.request.ValidStepperSections[this.stepper].sections.indexOf("Catering"), 1)
                                }
                            }
                            this.$refs[`cateringloop${this.stepper}`][0]?.$refs.cateringForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket)
                                }
                            })
                        }
                        if (this.$refs[`childcareloop${this.stepper}`]) {
                            this.$refs[`childcareloop${this.stepper}`][0]?.$refs.childForm?.validate()
                            //Add or remove childcare from the valid sections list
                            if (this.$refs[`childcareloop${this.stepper}`][0]?.$refs.childForm?.value) {
                                if (!this.request.ValidStepperSections[this.stepper].sections.includes("Childcare")) {
                                    this.request.ValidStepperSections[this.stepper].sections.push("Childcare")
                                }
                            } else {
                                if (this.request.ValidStepperSections[this.stepper].sections.indexOf("Childcare") >= 0) {
                                    this.request.ValidStepperSections[this.stepper].sections.splice(this.request.ValidStepperSections[this.stepper].sections.indexOf("Childcare"), 1)
                                }
                            }
                            this.$refs[`childcareloop${this.stepper}`][0]?.$refs.childForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket)
                                }
                            })
                        }
                        if (this.$refs[`accomloop${this.stepper}`]) {
                            this.$refs[`accomloop${this.stepper}`][0]?.$refs.accomForm?.validate()
                            //Add or remove extra accommodations from the valid sections list
                            if (this.$refs[`accomloop${this.stepper}`][0]?.$refs.accomForm?.value) {
                                if (!this.request.ValidStepperSections[this.stepper].sections.includes("Extra Resources")) {
                                    this.request.ValidStepperSections[this.stepper].sections.push("Extra Resources")
                                }
                            } else {
                                if (this.request.ValidStepperSections[this.stepper].sections.indexOf("Extra Resources") >= 0) {
                                    this.request.ValidStepperSections[this.stepper].sections.splice(this.request.ValidStepperSections[this.stepper].sections.indexOf("Extra Resources"), 1)
                                }
                            }
                            this.$refs[`accomloop${this.stepper}`][0]?.$refs.accomForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket)
                                }
                            })
                        }
                    }
                    if (this.request.needsPub && this.stepper == (3 + this.request.Events.length)) {
                        if (this.$refs.publicityloop) {
                            this.$refs.publicityloop.$refs.pubForm?.validate()
                            //Add or remove publicity from the valid sections list
                            if (this.$refs.publicityloop.$refs.pubForm?.value) {
                                if (!this.request.ValidStepperSections[this.stepper].sections.includes("Publicity")) {
                                    this.request.ValidStepperSections[this.stepper].sections.push("Publicity")
                                    this.request.ValidSections.push("Publicity")
                                }
                            } else {
                                this.request.ValidStepperSections[this.stepper].sections.splice(this.request.ValidStepperSections[this.stepper].sections.indexOf("Publicity"), 1)
                                if (this.request.ValidSections.includes("Publicity")) {
                                    this.request.ValidSections.splice(this.request.VaidSections.indexOf("Publicity"), 1)
                                }
                            }
                            this.$refs.publicityloop.$refs.pubForm?.inputs.forEach((e) => {
                                if (e.errorBucket && e.errorBucket.length) {
                                    eventErrors.errors.push(...e.errorBucket)
                                }
                            })
                        }
                    }
                    let i = this.isSuperUser ? 3 : 2
                    let lastIdx = this.request.needsPub ? this.request.ValidStepperSections.length - 1 : this.request.ValidStepperSections.length
                    let targetNum = lastIdx - i
                    let roomCt = 0, onlineCt = 0, regCt = 0, foodCt = 0, childCt = 0, extraCt = 0
                    for (i; i < lastIdx; i++) {
                        if (this.request.ValidStepperSections[i].sections.includes("Room")) {
                            roomCt++
                        }
                        if (this.request.ValidStepperSections[i].sections.includes("Online Event")) {
                            onlineCt++
                        }
                        if (this.request.ValidStepperSections[i].sections.includes("Registration")) {
                            regCt++
                        }
                        if (this.request.ValidStepperSections[i].sections.includes("Catering")) {
                            foodCt++
                        }
                        if (this.request.ValidStepperSections[i].sections.includes("Childcare")) {
                            childCt++
                        }
                        if (this.request.ValidStepperSections[i].sections.includes("Extra Resources")) {
                            extraCt++
                        }
                    }
                    if (roomCt == targetNum && !this.request.ValidSections.includes("Room")) {
                        this.request.ValidSections.push("Room")
                    } else if (roomCt != targetNum && this.request.ValidSections.includes("Room")) {
                        this.request.ValidSections.splice(this.request.ValidSections.indexOf("Room"), 1)
                    }
                    if (onlineCt == targetNum && !this.request.ValidSections.includes("Online Event")) {
                        this.request.ValidSections.push("Online Event")
                    } else if (onlineCt != targetNum && this.request.ValidSections.includes("Online Event")) {
                        this.request.ValidSections.splice(this.request.ValidSections.indexOf("Online Event"), 1)
                    }
                    if (regCt == targetNum && !this.request.ValidSections.includes("Registration")) {
                        this.request.ValidSections.push("Registration")
                    } else if (regCt != targetNum && this.request.ValidSections.includes("Registration")) {
                        this.request.ValidSections.splice(this.request.ValidSections.indexOf("Registration"), 1)
                    }
                    if (foodCt == targetNum && !this.request.ValidSections.includes("Catering")) {
                        this.request.ValidSections.push("Catering")
                    } else if (foodCt != targetNum && this.request.ValidSections.includes("Catering")) {
                        this.request.ValidSections.splice(this.request.ValidSections.indexOf("Catering"), 1)
                    }
                    if (childCt == targetNum && !this.request.ValidSections.includes("Childcare")) {
                        this.request.ValidSections.push("Childcare")
                    } else if (childCt != targetNum && this.request.ValidSections.includes("Childcare")) {
                        this.request.ValidSections.splice(this.request.ValidSections.indexOf("Childcare"), 1)
                    }
                    if (extraCt == targetNum && !this.request.ValidSections.includes("Extra Resources")) {
                        this.request.ValidSections.push("Extra Resources")
                    } else if (extraCt != targetNum && this.request.ValidSections.includes("Extra Resources")) {
                        this.request.ValidSections.splice(this.request.ValidSections.indexOf("Extra Resources"), 1)
                    }
                    let idx = -1
                    this.errors.forEach((e, i) => {
                        if (e.page == eventErrors.page) {
                            idx = i
                        }
                    })
                    if (idx > -1) {
                        this.errors[idx].errors.push(...eventErrors.errors)
                    } else {
                        this.errors.push(eventErrors)
                    }
                    let allErrs = this.errors.map(e => e.errors).flat()
                    if (allErrs.length > 0) {
                        this.request.IsValid = false
                    } else {
                        this.request.IsValid = true
                    }
                },
                showValidation() {
                    if (this.$refs.form) {
                        this.$refs.form.validate()
                    }
                    if (this.$refs.startTime) {
                        this.$refs.startTime.$refs.timeForm.validate()
                    }
                    if (this.$refs.endTime) {
                        this.$refs.endTime.$refs.timeForm.validate()
                    }
                    let start = this.isSuperUser ? 3 : 2
                    for (let i = start; i < (this.request.Events.length + start); i++) {
                        if (this.$refs[`spaceloop${i}`]) {
                            this.$refs[`spaceloop${i}`][0]?.$refs.spaceForm?.validate()
                        }
                        if (this.$refs[`regloop${i}`]) {
                            this.$refs[`regloop${i}`][0]?.$refs.regForm?.validate()
                        }
                        if (this.$refs[`cateringloop${i}`]) {
                            this.$refs[`cateringloop${i}`][0]?.$refs.cateringForm?.validate()
                        }
                        if (this.$refs[`childcareloop${i}`]) {
                            this.$refs[`childcareloop${i}`][0]?.$refs.childForm?.validate()
                        }
                        if (this.$refs[`accomloop${i}`]) {
                            this.$refs[`accomloop${i}`][0]?.$refs.accomForm?.validate()
                        }
                    }
                    if (this.$refs.publicityloop) {
                        this.$refs.publicityloop.$refs.pubForm?.validate()
                    }
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
                    this.request.Events.forEach((e, idx) => {
                        if (!this.request.EventDates.includes(e.EventDate)) {
                            //Event should be removed from list
                            this.request.Events.splice(idx, 1)
                        }
                    })
                    this.request.EventDates = this.request.EventDates.sort((a, b) => moment(a).diff(moment(b)))
                    this.request.Events = this.request.Events.sort((a, b) => moment(a.EventDate).diff(moment(b.EventDate)))
                },
                saveDraft() {
                    if (this.request.Name && this.request.EventDates.length > 0) {
                        $('#updateProgress').show();
                        $('[id$="hfRequest"]').val(JSON.stringify(this.request));
                        $('[id$="btnSave"')[0].click();
                    } else {
                        this.saveDialog = true
                    }
                },
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
                'request.IsSame'(val, oval) {
                    if (!val) {
                        this.matchMultiEvent()
                    } else {
                        if (this.request.Events.length > 1) {
                            //remove the rest because they said everything will be the same
                            this.request.Events.length = 1
                        }
                    }
                },
                'request.Ministry'(val, oval) {
                    let min = this.ministries.filter(m => {
                        return m.Id == this.request.Ministry
                    })
                    if (min.length > 0) {
                        if (min[0].IsPersonal) {
                            //Remove options if Personal Request, only allow space
                            this.request.needsOnline = false
                            this.request.needsReg = false
                            this.request.needsCatering = false
                            this.request.needsChildCare = false
                            this.request.needsAccom = false
                            this.request.needsPub = false
                        }
                    }
                },
                'request.needsSpace'(val, oval) {
                    if (!val) {
                        for (var i = 0; i < this.request.Events.length; i++) {
                            this.request.Events[i].Rooms = []
                            this.request.Events[i].ExpectedAttendance = null
                            this.request.Events[i].Checkin = false
                            this.request.Events[i].SupportTeam = false
                            this.request.Events[i].NumTablesRound = null
                            this.request.Events[i].NumTablesRect = null
                            this.request.Events[i].TableType = []
                            this.request.Events[i].NumChairsRound = null
                            this.request.Events[i].NumChairsRect = null
                        }
                    }
                },
                'request.needsOnline'(val, oval) {
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
                'request.needsReg'(val, oval) {
                    if (!val) {
                        for (var i = 0; i < this.request.Events.length; i++) {
                            this.request.Events[i].RegistrationDate = ''
                            this.request.Events[i].RegistrationEndDate = ''
                            this.request.Events[i].RegistrationEndTime = ''
                            this.request.Events[i].FeeType = []
                            this.request.Events[i].FeeBudgetLine = ''
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
                'request.needsCatering'(val, oval) {
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
                'request.needsChildCare'(val, oval) {
                    if (!val) {
                        for (var i = 0; i < this.request.Events.length; i++) {
                            this.request.Events[i].CCStartTime = ''
                            this.request.Events[i].CCEndTime = ''
                            this.request.Events[i].ChildCareOptions = []
                            this.request.Events[i].EstimatedKids = ''
                        }
                    }
                },
                'request.needsAccom'(val, oval) {
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
                            this.request.Events[i].NeedsDoorsUnlocked = false
                        }
                    }
                },
                'request.needsPub': { 
                  handler(val, oval) {
                    if (!val) {
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
                  },
                  immediate: true
                },
            }
        });
    });
</script>
<style>
  .v-list--dense .v-subheader {
    font-size: .95rem;
    font-weight: bold;
  }
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
  .v-input--switch {
    display: inline-block;
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
  .accent-text, .v-list--dense .v-subheader {
    color: #8ED2C9;
  }
  [v-cloak] {
    display: none !important;
  }
  .v-stepper, .v-stepper__items, .v-stepper__wrapper {
    overflow: visible !important;
    /*overflow-x: hidden !important;*/
  }
  .date-warning {
    color: #CC3F0C !important;
    font-weight: bold !important;
  }
  @media only screen and (max-width: 600px) {
    .v-stepper .v-stepper__header .v-stepper__step {
      width: 100%;
      flex-basis: 100%;
      display: inline-block;
    }
    .v-stepper .v-stepper__header .v-stepper__step .v-stepper__label {
      display: inline;
      padding-left: 16px;
    }
  }
  /* Time Picker */
  .btn-time-wrapper {
    margin-top: -10px;
  }
  .btn-time-wrapper div {
    text-align: center;
    padding: 4px 8px;
    background-color: rgba(0,0,0,.12);
    cursor: pointer;
  }
  .btn-time-wrapper .btn-am {
    border-radius: 14px 14px 0px 0px;
  }
  .btn-time-wrapper .btn-pm {
    border-radius: 0px 0px 14px 14px;
  }
  .btn-time-wrapper .active {
    background-color: #347689;
    color: white;
  }
</style>