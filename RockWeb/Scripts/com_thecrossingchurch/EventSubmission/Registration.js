import datePicker from '/Scripts/com_thecrossingchurch/EventSubmission/DatePicker.js';
export default {
  template: `
<v-form ref="regForm" v-model="valid">
<v-row>
  <v-col>
    <h3 class="primary--text" v-if="request.Events.length == 1">Registration Information</h3>
    <h3 class="primary--text" v-else>
      Registration Information 
      <v-btn rounded outlined color="accent" @click="prefillDate = ''; dialog = true; ">
        Prefill
      </v-btn>
    </h3>
  </v-col>
</v-row>
<v-row>
  <v-col cols="12" md="6">
    <date-picker
      v-model="e.RegistrationDate"
      :date="e.RegistrationDate"
      label="What date do you need the registration link to be ready and live?"
      clearable
      :rules="[rules.required(e.RegistrationDate, 'Start Date'), ]"
      :min="earliestPubDate"
    ></date-picker>
  </v-col>
  <v-col cols="12" md="6">
    <v-autocomplete
      label="Types of Registration Fees"
      :items="feeOptions"
      v-model="e.FeeType"
      multiple
      attach
      ref="feeTypeRef"
    ></v-autocomplete>
  </v-col>
</v-row>
<v-row>
  <v-col cols="12" md="6" v-if="e.FeeType.includes('Fee per Individual') || e.FeeType.includes('Fee per Couple') || e.FeeType.includes('Online Fee')">
    <v-autocomplete
      label="Which budget should registration fees go to?"
      v-model="e.FeeBudgetLine"
      :rules="[rules.requiredBL(e.FeeType, e.FeeBudgetLine, 'Budget line')]"
      :items="budgetLines"
      item-value="Id"
      item-text="Value"
    ></v-autocomplete>
  </v-col>
  <v-col cols="12" md="6" v-if="e.FeeType.includes('Fee per Individual')">
    <v-text-field
      label="How much is the individual registration fee for this event?"
      type="number"
      prepend-inner-icon="mdi-currency-usd"
      v-model="e.Fee"
      :rules="[rules.required(e.Fee, 'Amount')]"
    ></v-text-field>
  </v-col>
  <v-col cols="12" md="6" v-if="e.FeeType.includes('Fee per Couple')">
    <v-text-field
      label="How much is the couple registration fee for this event?"
      type="number"
      prepend-inner-icon="mdi-currency-usd"
      v-model="e.CoupleFee"
      :rules="[rules.required(e.CoupleFee, 'Amount')]"
    ></v-text-field>
  </v-col>
  <v-col cols="12" md="6" v-if="e.FeeType.includes('Online Fee')">
    <v-text-field
      label="How much is the online registration fee for this event?"
      type="number"
      prepend-inner-icon="mdi-currency-usd"
      v-model="e.OnlineFee"
      :rules="[rules.required(e.OnlineFee, 'Amount')]"
    ></v-text-field>
  </v-col>
</v-row>
<v-row>
  <v-col cols="12" md="6">
    <br/>
    <v-row>
      <v-col>
        <date-picker
          v-model="e.RegistrationEndDate"
          :date="e.RegistrationEndDate"
          label="What date should registration close?"
          hint="We always default to 24 hours before your event if you have no reason to close registration earlier."
          persistent-hint
          clearable
          :rules="[rules.required(e.RegistrationEndDate, 'End Date'), rules.registrationCloseDate(request.EventDates, e.EventDate, e.RegistrationEndDate, request.needsChildCare)]"
          :min="earliestPubDate"
        ></date-picker>
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
      label="Name of the person this email should come from"
      v-model="e.Sender"
      :rules="[rules.required(e.Sender, 'Sender')]"
    ></v-text-field>
    <v-text-field
      label="Email address of the person this email should come from"
      v-model="e.SenderEmail"
      hint="If you want to use an email other than your sender's firstname.lastname@thecrossing email enter it here"
      persistent-hint
    ></v-text-field>
    <v-textarea
      label="Thank You"
      v-model="e.ThankYou"
    ></v-textarea>
    <v-textarea
      label="Date, Time, and Location"
      v-model="e.TimeLocation"
    ></v-textarea>
    <v-textarea
      label="Additional Details"
      v-model="e.AdditionalDetails"
    ></v-textarea>
  </v-col>
  <v-col>
    <div style="font-weight: bold; font-style: italic; text-align: center;">This preview is just to give you a general idea about placement within the email, it is not the final product.</div>
    <div v-html="emailPreview"></div>
  </v-col>
</v-row>
<v-row>
  <v-col>
    <v-switch
      :label="reminderEmailLabel"
      v-model="e.NeedsReminderEmail"
    ></v-switch>
  </v-col>
</v-row>
<template v-if="e.NeedsReminderEmail">
  <v-row>
    <v-col>
      <h4 class="primary--text">Let's build-out the reminder email your registrants will receive before the event</h4>
    </v-col>
  </v-row>
  <v-row>
    <v-col cols="12" md="6">
      <v-text-field
        label="Name of the person this email should come from"
        v-model="e.ReminderSender"
        :rules="[rules.required(e.ReminderSender, 'Sender')]"
      ></v-text-field>
      <v-text-field
        label="Email address of the person this email should come from"
        v-model="e.ReminderSenderEmail"
        hint="If you want to use an email other than your sender's firstname.lastname@thecrossing email enter it here"
        persistent-hint
      ></v-text-field>
      <v-textarea
        label="Date, Time, and Location"
        v-model="e.ReminderTimeLocation"
      ></v-textarea>
      <v-textarea
        label="Additional Details"
        v-model="e.ReminderAdditionalDetails"
      ></v-textarea>
    </v-col>
    <v-col cols="12" md="6">
      <div style="font-weight: bold; font-style: italic; text-align: center;">This preview is just to give you a general idea about placement within the email, it is not the final product.</div>
      <div v-html="reminderEmailPreview"></div>
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
  props: ["e", "request", "earliestPubDate"],
  data: function () {
      return {
          menu: false,
          menu2: false,
          dialog: false,
          budgetLines: [],
          prefillDate: '',
          valid: true,
          rules: {
              required(val, field) {
                  return !!val || `${field} is required`;
              },
              requiredBL(fees, val, field) {
                  if (fees.length > 0) {
                      if (fees.length == 1 && fees.includes('No Fees')) {
                          return true
                      }
                      return !!val || `${field} is required`;
                  }
                  return true
              },
              registrationCloseDate(eventDates, eventDate, closeDate, needsChildCare) {
                  let dates = eventDates.map(d => moment(d))
                  let minDate = moment.min(dates)
                  if (eventDate) {
                      minDate = moment(eventDate)
                  }
                  if (needsChildCare) {
                      minDate = minDate.subtract(2, "day")
                  }
                  if (moment(closeDate).isAfter(minDate)) {
                      if (needsChildCare) {
                          return 'When requesting childcare, registration must close 48 hours before the start of your event'
                      }
                      return 'Registration cannot end after your event'
                  }
                  return true
              },
              registrationCloseTime(eventDate, closeDate, needsChildCare, startTime, endtime, closeTime) {
                let minDate = moment(eventDate)
                let actualDate = moment(`${closeDate} ${closeTime}`)
                if (needsChildCare) {
                  minDate = minDate.subtract(2, "day")
                  minDate = moment(`${minDate.format('yyyy-MM-DD')} ${startTime}`)
                } else {
                  minDate = moment(`${minDate.format('yyyy-MM-DD')} ${endtime}`)
                }
                if (moment(actualDate).isAfter(minDate)) {
                  if (needsChildCare) {
                    return 'When requesting childcare, registration must close 48 hours before the start of your event'
                  }
                  return 'Registration cannot end after your event'
                }
                return true
              }
          }
      }
  },
  created: function () {
    this.rooms = JSON.parse($('[id$="hfRooms"]')[0].value)
    this.budgetLines = JSON.parse($('[id$="hfBudgetLines"]')[0].value)
  },
  methods: {
    formatRooms(val) {
      if (val) {
        let rms = []
        val.forEach((i) => {
          this.rooms.forEach((r) => {
            if (i == r.Id) {
              rms.push(r.Value)
            }
          })
        })
        return rms.join(", ")
      }
      return ""
    },
    prefillSection() {
      this.dialog = false
      let idx = this.request.EventDates.indexOf(this.prefillDate)
      let currIdx = this.request.EventDates.indexOf(this.e.EventDate)
      this.$emit('updatereg', { targetIdx: idx, currIdx: currIdx })
    },
    boolToYesNo(val) {
      if (val) {
        return "Yes"
      }
      return "No"
    },
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
            if(this.request.needsChildCare) {
              return moment(this.e.EventDate).subtract(2, "day").format("yyyy-MM-DD")
            }
            return moment(this.e.EventDate).subtract(1, "day").format("yyyy-MM-DD")
          } else {
            let eventDates = this.request.EventDates.map(p => moment(p))
            let firstDate = moment.min(eventDates)
            if(this.request.needsChildCare) {
              return moment(firstDate).subtract(2, "day").format("yyyy-MM-DD")
            }
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
          let dt = moment(this.e.EventDate).format('MM/DD/yyyy')
          if(this.request.IsSame) {
            dt = moment(this.request.EventDates[0]).format('MM/DD/yyyy')
          } 
          let message = this.request.Name + " will take place at " + this.e.StartTime + " on " + dt 
          if(this.e.Rooms && this.e.Rooms.length > 0) {
            message += " in " + this.formatRooms(this.e.Rooms)
          }
          return message
        }
        return ""
      }
      return ""
    },
    defaultReminderTimeLocation() {
      if (this.request.needsReg) {
        if (this.request.Id > 0 && this.e.ReminderTimeLocation) {
          return this.e.ReminderTimeLocation
        }
        if (this.request.Name && this.e.StartTime) {
          let dt = moment(this.e.EventDate).format('MM/DD/yyyy')
          if(this.request.IsSame) {
            dt = moment(this.request.EventDates[0]).format('MM/DD/yyyy')
          } 
          let message = this.request.Name + " will take place at " + this.e.StartTime + " on " + dt 
          if(this.e.Rooms && this.e.Rooms.length > 0) {
            message += " in " + this.formatRooms(this.e.Rooms)
          }
          return message
        }
        return ""
      }
      return ""
    },
    defaultSender() {
      if (this.request.needsReg) {
        if (this.request.Id > 0 && this.e.Sender) {
          return this.e.Sender
        }
        if (this.request.Contact ) {
          return this.request.Contact
        }
        return ""
      }
      return ""
    },
    defaultReminderSender() {
      if (this.request.needsReg) {
        if (this.request.Id > 0 && this.e.ReminderSender) {
          return this.e.ReminderSender
        }
        if (this.request.Contact ) {
          return this.request.Contact
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
      this.e.ThankYou + "<br/><br/>" +
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
    reminderEmailPreview() {
      let preview =
        "<div style='background-color: #F2F2F2;'>" +
        "<div style='text-align: center; padding-top: 30px; padding-bottom: 30px;'>" +
        "<img src='https://rock.thecrossingchurch.com/content/EmailTemplates/CrossingLogo-EmailTemplate-Header-215x116.png' border='0' style='width:100%; max-width: 215px; height: auto;'>" +
        "</div>" +
        "<div style='background-color: #F9F9F9; padding: 30px; margin: auto; max-width: 90%;'>" +
        "<h1>" + this.request.Name + " Reminder</h1><br/>" +
        this.e.ReminderTimeLocation + "<br/><br/>" +
        "<p>The following people have been registered for " + this.request.Name + ":</p>" +
        "<ul>" +
        "<li>First Registrant</li>" +
        "<li>Second Registrant</li>" +
        "</ul>"
      if (this.e.Fee) {
        preview +=
          "<p>" +
          "This registration still has a balance of $" + this.e.Fee + ".<br/>" +
          "</p>"
      }
      preview += this.e.ReminderAdditionalDetails
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
        return [
          {text: 'Fee per Individual', value: 'Fee per Individual', disabled: (this.e.FeeType.includes("No Fees") ? true : false)},
          {text: 'Fee per Couple', value: 'Fee per Couple', disabled: (this.e.FeeType.includes("No Fees") ? true : false)},
          {text: 'Online Fee', value: 'Online Fee', disabled: (this.e.FeeType.includes("No Fees") ? true : false)},
          {text: 'No Fees', value: 'No Fees', disabled: (this.e.FeeType.length > 0 && !this.e.FeeType.includes("No Fees") ? true : false)}
        ]
      }
      return [
        {text: 'Fee per Individual', value: 'Fee per Individual', disabled: (this.e.FeeType.includes("No Fees") ? true : false)},
        {text: 'Fee per Couple', value: 'Fee per Couple', disabled: (this.e.FeeType.includes("No Fees") ? true : false)},
        {text: 'No Fees', value: 'No Fees', disabled: (this.e.FeeType.length > 0 && !this.e.FeeType.includes("No Fees") ? true : false)}
      ]
    },
    reminderEmailLabel() {
      return `Would you like a Reminder Email for this Event? (${this.boolToYesNo(this.e.NeedsReminderEmail)})`
    },
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
    defaultReminderTimeLocation(val) {
      if (val) {
        this.e.ReminderTimeLocation = val
      }
    },
    defaultSender(val) {
      if(val) {
        this.e.Sender = val
      }
    },
    defaultReminderSender(val) {
      if(val) {
        this.e.ReminderSender = val
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
      if(val.includes('No Fees')) {
        this.$refs.feeTypeRef.blur()
      }
    }
  },
  filters: {
    formatDate(val) {
      return moment(val).format("MM/DD/yyyy");
    },
  },
  components: {
    'date-picker' : datePicker
  }
}