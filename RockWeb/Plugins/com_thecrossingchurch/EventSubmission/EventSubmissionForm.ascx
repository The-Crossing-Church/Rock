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
<asp:HiddenField ID="hfIsSuperUser" runat="server" />
<asp:HiddenField ID="hfPersonName" runat="server" />
<asp:HiddenField ID="hfChangeRequest" runat="server" />

<div id="app" v-cloak>
  <v-app v-cloak>
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
                label="A physical space for an event"
                hint="If you need any doors unlocked for this event, please be sure to include Special Accommodations below. Selecting a physical space does not assume unlocked doors."
                :persistent-hint="request.needsSpace"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsOnline"
                label="Zoom"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
                :persistent-hint="request.needsOnline"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsCatering"
                label="Food Request"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
                :persistent-hint="request.needsCatering"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsChildCare"
                label="Childcare"
                hint="Requests involving childcare must be made at least 30 days in advance."
                :persistent-hint="request.needsChildCare"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsAccom"
                label="Special Accommodations (tech, drinks, web calendar, extensive set-up, doors unlocked)"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
                :persistent-hint="request.needsAccom"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsReg"
                label="Registration"
                hint="Requests involving anything more than a physical space with table and chair set-up must be made at least 14 days in advance."
                :persistent-hint="request.needsReg"
              ></v-switch>
            </v-col>
          </v-row>
          <v-row>
            <v-col>
              <v-switch
                v-model="request.needsPub"
                label="Publicity"
                hint="Requests involving publicity must be made at least 6 weeks in advance."
                :persistent-hint="request.needsPub"
              ></v-switch>
            </v-col>
          </v-row>
        </v-card-text>
        <v-card-actions>
          <v-spacer></v-spacer>
          <v-btn color="primary" :disabled="!(request.needsSpace || request.needsOnline || request.needsCatering || request.needsChildCare || request.needsAccom || request.needsReg || request.needsPub)" @click="next">Next</v-btn>
        </v-card-actions>
      </v-card>
      <v-card v-if="panel == 1">
        <v-card-text>
          <v-alert v-if="canEdit == false" type="error">You are not able to make changes to this request because it is currently {{request.Status}}.</v-alert>
          <v-alert v-if="canEdit && request.Status && request.Status != 'Submitted'" type="warning">Any changes made to this request will need to be approved.</v-alert>
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
            <v-row>
              <v-col>
                <strong>What time will your <template v-if='requestedResources == "rooms"'>meeting</template><template v-else>event</template> begin and end?</strong>
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="6">
                <strong>Start Time</strong>
                <time-picker
                  v-model="request.Events[0].StartTime"
                  :value="request.Events[0].StartTime"
                  ref="startTime"
                  :rules="[rules.required(request.Events[0].StartTime, 'Start Time'), rules.validTime(request.Events[0].StartTime, request.Events[0].EndTime, true)]"
                ></time-picker>
              </v-col>
              <v-col cols="12" md="6">
                <strong>End Time</strong>
                <time-picker
                  v-model="request.Events[0].EndTime"
                  :value="request.Events[0].EndTime"
                  ref="endTime"
                  :rules="[rules.required(request.Events[0].EndTime, 'End Time'), rules.validTime(request.Events[0].EndTime, request.Events[0].StartTime, false)]"
                ></time-picker>
              </v-col>
            </v-row>
            <br/>
            <span>Note that the time and date of your <template v-if='requestedResources == "rooms"'>meeting</template><template v-else>event</template> will influence the list of spaces to choose from based on availablitiy. Changing your date or time after selecting a space could remove a previously selected space.</span>
          </template>
        </v-card-text>
        <v-card-actions>
          <v-btn v-if="isSuperUser" color="secondary" @click="prev">Back</v-btn>
          <v-spacer></v-spacer>
          <v-btn color="primary" :disabled="!request.Name || !request.Ministry || !request.Contact || !request.EventDates || (request.EventDates && request.EventDates.length == 0)" @click="next">Next</v-btn>
        </v-card-actions>
      </v-card>
      <v-card v-if="panel == 2 && !isSuperUser">
        <v-card-title>
          <v-alert :type="`${triedSubmit ? 'error' : 'warning' }`" v-if="!isValid && currentErrors.length > 0" style="width: 100%;">
            <template v-if="triedSubmit">Please review your request and fix the following errors:</template>
            <template v-else>Please fix the following before you submit:</template>
            <ul>
              <li v-for="e in currentErrors">
                {{e}}
              </li>
            </ul>
          </v-alert>
          <h2 v-if="request.EventDates.length > 1 && !request.IsSame" class="accent--text">Details for {{request.Name}} on {{currentEvent.EventDate | formatDate}}</h2>
        </v-card-title>
        <v-card-text>
          <%-- Time Info --%>
          <template v-if="!request.IsSame">
            <h3 class="primary--text">Basic Information</h3><br/>
            <v-row>
              <v-col>
                <strong>What time will your event begin and end?</strong><br/>
                <span>Note that the time and date of your <template v-if='requestedResources == "rooms"'>meeting</template><template v-else>event</template> will influence the list of spaces to choose from based on availablitiy. Changing your date or time after selecting a space could remove a previously selected space.</span>
              </v-col>
            </v-row>
            <v-row>
              <v-col cols="12" md="6">
                <strong>Start Time</strong>
                <time-picker
                  v-model="currentEvent.StartTime"
                  :value="currentEvent.StartTime"
                  ref="startTime"
                  :rules="[rules.required(currentEvent.StartTime, 'Start Time'), rules.validTime(currentEvent.StartTime, currentEvent.EndTime, true)]"
                ></time-picker>
              </v-col>
              <v-col cols="12" md="6">
                <strong>End Time</strong>
                <time-picker
                  v-model="currentEvent.EndTime"
                  :value="currentEvent.EndTime"
                  ref="endTime"
                  :rules="[rules.required(currentEvent.EndTime, 'End Time'), rules.validTime(currentEvent.EndTime, currentEvent.StartTime, false)]"
                ></time-picker>
              </v-col>
            </v-row>
          </template>
          <space :e="currentEvent" :request="request" :existing="existingRequests" ref="spaceloop" v-on:updatespace="updateSpace"></space>
          <drinks :e="currentEvent" :request="request" v-on:updateaccom="updateAccom"></drinks>
        </v-card-text>
        <v-card-actions>
          <v-btn color="secondary" @click="prev">Back</v-btn>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="next" v-if="currentIdx == request.Events.length - 1 && canEdit">{{( isExistingRequest ? 'Update' : 'Submit')}}</v-btn>
          <v-btn color="primary" @click="next" v-else>Next</v-btn>
        </v-card-actions>
      </v-card>
      <v-card v-if="panel == 2 && isSuperUser">
        <v-card-title>
          <v-alert :type="`${triedSubmit ? 'error' : 'warning' }`" v-if="!isValid && currentErrors.length > 0" style="width: 100%;">
            <template v-if="triedSubmit">Please review your request and fix the following errors:</template>
            <template v-else>Please fix the following before you submit:</template>
            <ul>
              <li v-for="e in currentErrors">
                {{e}}
              </li>
            </ul>
          </v-alert>
          <h2 v-if="request.EventDates.length > 1 && !request.IsSame" class="accent--text">Details for {{request.Name}} on {{currentEvent.EventDate | formatDate}}</h2>
        </v-card-title>
        <v-card-text>
          <template>
            <template v-if="!request.IsSame">
              <v-row>
                <v-col>
                  <strong>What time will your event begin and end on {{currentEvent.EventDate | formatDate}}?</strong>
                </v-col>
              </v-row>
              <v-row>
                <v-col cols="12" md="6">
                  <strong>Start Time</strong>
                  <time-picker
                    v-model="currentEvent.StartTime"
                    :value="currentEvent.StartTime"
                    ref="startTime"
                    :rules="[rules.required(currentEvent.StartTime, 'Start Time'), rules.validTime(currentEvent.StartTime, currentEvent.EndTime, true)]"
                  ></time-picker>
                </v-col>
                <v-col cols="12" md="6">
                  <strong>End Time</strong>
                  <time-picker
                    v-model="currentEvent.EndTime"
                    :value="currentEvent.EndTime"
                    ref="endTime"
                    :rules="[rules.required(currentEvent.EndTime, 'End Time'), rules.validTime(currentEvent.EndTime, currentEvent.StartTime, false)]"
                  ></time-picker>
                </v-col>
              </v-row>
              </template>
            <%-- Space Information --%>
            <template v-if="request.needsSpace">
              <space :e="currentEvent" :request="request" :existing="existingRequests" ref="spaceloop" v-on:updatespace="updateSpace"></space>
            </template>
            <%-- Online Information --%>
            <template v-if="request.needsOnline">
              <v-row>
                <v-col>
                  <h3 class="primary--text">Zoom Information</h3>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-text-field
                    label="If there is a link your attendees will need to access this event, list it here"
                    v-model="currentEvent.EventURL"
                    :rules="[rules.required(currentEvent.EventURL, 'Link')]"
                  ></v-text-field>
                </v-col>
              </v-row>
              <v-row>
                <v-col>
                  <v-text-field
                    label="If there is a password for the link, list it here"
                    v-model="currentEvent.ZoomPassword"
                  ></v-text-field>
                </v-col>
              </v-row>
            </template>
            <%-- Catering Information --%>
            <template v-if="request.needsCatering">
              <catering :e="currentEvent" :request="request" ref="cateringloop" v-on:updatecatering="updateCatering"></catering>
            </template>
            <%-- Childcare Info --%>
            <template v-if="request.needsChildCare">
              <childcare :e="currentEvent" :request="request" ref="childcareloop" v-on:updatechildcare="updateChildcare"></childcare>
            </template>
            <%-- Special Accommodations Info --%>
            <template v-if="request.needsAccom">
              <accom :e="currentEvent" :request="request" ref="accomloop" v-on:updateaccom="updateAccom"></accom>
            </template>
            <%-- Registration Information --%>
            <template v-if="request.needsReg">
              <registration :e="currentEvent" :request="request" :earliest-pub-date="earliestPubDate" ref="regloop" v-on:updatereg="updateReg"></registration>
            </template>
          </template>
          <%-- Publicity Information --%>
          <template v-if="request.needsPub">
            <publicity :request="request" :earliest-pub-date="earliestPubDate"></publicity>
          </template>
          <%-- Notes --%>
          <template v-if="request.needsOnline || request.needsCatering || request.needsChildCare || request.needsAccom || request.needsReg || request.needsPub">
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
        </v-card-text>
        <v-card-actions>
          <v-btn color="secondary" @click="prev">Back</v-btn>
          <v-spacer></v-spacer>
          <v-btn color="primary" @click="next" v-if="currentIdx == request.Events.length - 1 && canEdit">{{( isExistingRequest ? 'Update' : 'Submit')}}</v-btn>
          <v-btn color="primary" @click="next" v-else>Next</v-btn>
          <Rock:BootstrapButton
            runat="server"
            ID="btnSubmit"
            CssClass="btn-hidden"
            OnClick="Submit_Click"
          />
        </v-card-actions>
      </v-card>
      <v-card v-if="panel == 9">
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
<script type="module">
import timePickerVue from '/Scripts/com_thecrossingchurch/EventSubmission/TimePicker.js';
import spaceVue from '/Scripts/com_thecrossingchurch/EventSubmission/Space.js';
import registrationVue from '/Scripts/com_thecrossingchurch/EventSubmission/Registration.js';
import cateringVue from '/Scripts/com_thecrossingchurch/EventSubmission/Catering.js';
import childcareVue from '/Scripts/com_thecrossingchurch/EventSubmission/Childcare.js';
import publicityVue from '/Scripts/com_thecrossingchurch/EventSubmission/Publicity.js';
import accomVue from '/Scripts/com_thecrossingchurch/EventSubmission/SpecialAccom.js';
import drinksVue from '/Scripts/com_thecrossingchurch/EventSubmission/Drinks.js';
document.addEventListener("DOMContentLoaded", function () {
  Vue.component("time-picker", timePickerVue);
  Vue.component("space", spaceVue);
  Vue.component("registration", registrationVue);
  Vue.component("catering", cateringVue);
  Vue.component("childcare", childcareVue);
  Vue.component("accom", accomVue);
  Vue.component("drinks", drinksVue);
  Vue.component("publicity", publicityVue);
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
      panel: 1,
      currentEvent: null,
      currentIdx: null,
      showSuccess: false,
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
            NumTablesRound: null,
            NumTablesRect: null,
            TableType: [],
            NumChairsRound: null,
            NumChairsRect: null,
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
            FeeBudgetLine: "",
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
            NeedsDoorsUnlocked: false
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
              return ( isAfter || `${isStart ? "Start time must come before end time" : "End time must come after start time" }` );
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
      changeDialog: false
    },
    created() {
      this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value);
      this.ministries = JSON.parse($('[id$="hfMinistries"]')[0].value);
      let isAd = $('[id$="hfIsAdmin"]')[0].value;
      if (isAd == 'True') {
          this.isAdmin = true
      }
      let isSU = $('[id$="hfIsSuperUser"]')[0].value;
      if(isSU == 'True') {
        this.isSuperUser = true
        this.panel = 0
      } else {
        this.request.needsSpace = true
      }
      this.request.Contact = $('[id$="hfPersonName"]')[0].value;
      let req = $('[id$="hfRequest"]')[0].value;
      if (req) {
        let parsed = JSON.parse(req)
        this.request = JSON.parse(parsed.Value)
        this.request.Id = parsed.Id
        this.request.Status = parsed.RequestStatus
        this.request.CreatedBy = parsed.CreatedBy
        this.request.canEdit = parsed.CanEdit
      }
      this.existingRequests = JSON.parse($('[id$="hfUpcomingRequests"]')[0].value)
      window["moment-range"].extendMoment(moment);
    },
    mounted() {
      let query = new URLSearchParams(window.location.search);
      let success = query.get('ShowSuccess');
      if (success) {
        if (success == "true") {
          this.showSuccess = true
          this.panel = 9
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
        if (items.length > 1) {
          items[items.length - 1] = "and " + items[items.length - 1]
        }
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
                if (!moment(itm).isSameOrAfter(moment(eDate).format("yyyy-MM-DD"))) {
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
      longDates() {
          return this.request.EventDates.map((i) => {
            return { text: moment(i).format("dddd, MMMM Do yyyy"), val: i };
          });
      },
      isValid() {
          let isValidForm = true
          this.errors.forEach(e => {
            if(e.errors && e.errors.length > 0) {
              isValidForm = false
            }
          })
          return isValidForm
      },
      currentErrors() {
        if(this.panel == 1) {
          return this.errors[0].errors
        } else if(this.panel != 0) {
          if(this.request.Events.length > 1) {
            let e = this.errors.filter(err => { return err.page == this.request.Events[this.currentIdx].EventDate})
            if(e && e.length > 0) {
              return e[0].errors
            }
          } else {
            if(this.errors.length > 1) {
              return this.errors[1].errors
            }
          }
        }
        return []
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
        this.validate();
        if (this.panel == 2) {
          if(this.currentIdx == this.request.Events.length - 1) {
            if (this.isValid) {
              this.hasConflictsOrTimeIssue = false
              this.checkForConflicts()
              this.checkTimeMeetsRequirements()
              if(this.hasConflictsOrTimeIssue) {
                this.dialog = true
              } else {
                this.submit()
              }
            } else {
              this.triedSubmit = true
              this.panel = 1
            }
          } else {
            this.currentIdx++
            this.currentEvent = this.request.Events[this.currentIdx]
          }
        } else if (this.panel == 1 ) {
          this.currentIdx = 0
          this.currentEvent = this.request.Events[this.currentIdx]
          this.panel++
        } else {
          this.panel++
        }
        window.scrollTo(0, 0)
        if(this.currentErrors && this.currentErrors.length > 0) {
          window.setTimeout(() => {
            this.showValidation()
          }, 500)
        }
      },
      prev() {
        this.validate();
        let tab = this.panel;
        if(tab == 2) {
          if(this.currentIdx == 0) {
            tab--
          } else {
            this.currentIdx--
            this.currentEvent = this.request.Events[this.currentIdx]
          }
        } else {
          tab--
          if (tab < 0) {
              tab = 0
          }
        }
        this.panel = tab;
        window.scrollTo(0, 0);
        if(this.currentErrors && this.currentErrors.length > 0) {
          window.setTimeout(() => {
            this.showValidation()
          }, 500)
        }
      },
      submit() {
        $('[id$="hfRequest"]').val(JSON.stringify(this.request));
        $('[id$="btnSubmit"')[0].click();
        $('#updateProgress').show();
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
        this.request.Events[0].EventDate = val.eventDate;
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
        this.request.Events[indexes.currIdx].FeeBudgetLine = this.request.Events[indexes.targetIdx].FeeBudgetLine
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
        this.request.Events[indexes.currIdx].NumTablesRound = this.request.Events[indexes.targetIdx].NumTablesRound
        this.request.Events[indexes.currIdx].NumTablesRect = this.request.Events[indexes.targetIdx].NumTablesRect
        this.request.Events[indexes.currIdx].TableType = this.request.Events[indexes.targetIdx].TableType
        this.request.Events[indexes.currIdx].NumChairsRound = this.request.Events[indexes.targetIdx].NumChairsRound
        this.request.Events[indexes.currIdx].NumChairsRect = this.request.Events[indexes.targetIdx].NumChairsRect
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
      },
      checkForConflicts() {
        this.request.HasConflicts = false
        let conflictingMessage = []
        let conflictingRequests = this.existingRequests.filter((r) => {
          r = JSON.parse(r);
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
          let eventErrors = null 
          if(this.panel > 1 ) {
            if(this.request.IsSame) {
              eventErrors = { page: 'event', errors: []}
            } else {
              eventErrors = { page: this.request.Events[this.currentIdx].EventDate, errors: []}
            }
          } else {
            eventErrors = { page: 'basic', errors: []}
          }
          if (this.$refs.form) {
            this.$refs.form.validate()
            this.$refs.form.inputs.forEach((e) => {
              if (e.errorBucket && e.errorBucket.length) {
                eventErrors.errors.push(...e.errorBucket)
              }
            });
          }
          if (this.$refs.startTime) {
            this.$refs.startTime.$refs.timeForm.validate()
            this.$refs.startTime.$refs.timeForm.inputs.forEach((e) => {
              if (e.errorBucket && e.errorBucket.length) {
                eventErrors.errors.push(...e.errorBucket.filter(eb => !!eb))
              }
            });
          }
          if (this.$refs.endTime) {
            this.$refs.endTime.$refs.timeForm.validate()
            this.$refs.endTime.$refs.timeForm.inputs.forEach((e) => {
              if (e.errorBucket && e.errorBucket.length) {
                eventErrors.errors.push(...e.errorBucket.filter(eb => !!eb))
              }
            });
          }
          if (this.$refs.spaceloop) {
            this.$refs.spaceloop.$refs.spaceForm.validate()
            this.$refs.spaceloop.$refs.spaceForm.inputs.forEach((e) => {
              if (e.errorBucket && e.errorBucket.length) {
                eventErrors.errors.push(...e.errorBucket)
              }
            });
          }
          if (this.$refs.regloop) {
            this.$refs.regloop.$refs.regForm.validate()
            this.$refs.regloop.$refs.regForm.inputs.forEach((e) => {
              if (e.errorBucket && e.errorBucket.length) {
                eventErrors.errors.push(...e.errorBucket)
              }
            });
          }
          if (this.$refs.cateringloop) {
            this.$refs.cateringloop.$refs.cateringForm.validate()
            this.$refs.cateringloop.$refs.cateringForm.inputs.forEach((e) => {
              if (e.errorBucket && e.errorBucket.length) {
                eventErrors.errors.push(...e.errorBucket)
              }
            });
          }
          if (this.$refs.childcareloop) {
            this.$refs.childcareloop.$refs.childForm.validate()
            this.$refs.childcareloop.$refs.childForm.inputs.forEach((e) => {
              if (e.errorBucket && e.errorBucket.length) {
                eventErrors.errors.push(...e.errorBucket)
              }
            });
          }
          if (this.$refs.accomloop) {
            this.$refs.accomloop.$refs.accomForm.validate()
            this.$refs.accomloop.$refs.accomForm.inputs.forEach((e) => {
              if (e.errorBucket && e.errorBucket.length) {
                eventErrors.errors.push(...e.errorBucket)
              }
            });
          }
          let idx = -1
          this.errors.forEach((e, i) => {
            if (e.page == eventErrors.page) {
              idx = i
            }
          })
          if(idx > -1) {
            this.errors[idx].errors = eventErrors.errors
          } else {
            this.errors.push(eventErrors)
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
        if (this.$refs.spaceloop) {
          this.$refs.spaceloop.$refs.spaceForm.validate()
        }
        if (this.$refs.regloop) {
          this.$refs.regloop.$refs.regForm.validate()
        }
        if (this.$refs.cateringloop) {
          this.$refs.cateringloop.$refs.cateringForm.validate()
        }
        if (this.$refs.childcareloop) {
          this.$refs.childcareloop.$refs.childForm.validate()
        }
        if (this.$refs.accomloop) {
          this.$refs.accomloop.$refs.accomForm.validate()
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
            this.errors = []
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
            this.errors = []
        },
        'request.Ministry'(val) {
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
        'request.needsSpace'(val) {
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
        'request.needsChildCare'(val) {
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
                    this.request.Events[i].NeedsDoorsUnlocked = false
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
</style>