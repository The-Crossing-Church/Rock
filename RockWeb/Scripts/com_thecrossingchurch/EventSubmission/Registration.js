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
      :rules="[rules.required(e.RegistrationDate, 'Start Date'), rules.registrationStartDate(earliestRegDate, e.RegistrationDate)]"
      :min="earliestRegDate"
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
    <date-picker
      v-model="e.RegistrationEndDate"
      :date="e.RegistrationEndDate"
      label="What date should registration close?"
      hint="We always default to 24 hours before your event if you have no reason to close registration earlier."
      persistent-hint
      clearable
      :rules="[rules.required(e.RegistrationEndDate, 'End Date'), rules.registrationCloseDate(request.EventDates, e.EventDate, e.RegistrationEndDate, request.needsChildCare)]"
      :min="earliestRegDate"
    ></date-picker>
  </v-col>
  <v-col cols="12" md="6">
    <time-picker
      label="What time should registration close?" 
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
      label="What do you need communicated in your confirmation email for your event after they register?"
      v-model="e.AdditionalDetails"
    ></v-textarea>
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
    <v-col>
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
        label="What do you need communicated in your reminder email for your event?"
        v-model="e.ReminderAdditionalDetails"
      ></v-textarea>
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
              registrationStartDate(earliestRegDate, startDate) {
                return (moment(startDate).isAfter(moment(earliestRegDate)) || moment(startDate).format("yyyy-MM-dd") == moment(earliestRegDate).format("yyyy-MM-dd") ) || `Registration cannot start before ${moment(earliestRegDate).format('MM/DD/yyyy')}`
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
    this.budgetLines.forEach(b => {
      b.IsDisabled = !b.IsActive
    })
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
    earliestRegDate() {
      let eDate = new moment();
      if(this.request.Id > 0) {
        eDate = new moment(this.request.SubmittedOn)
      }
      eDate = moment(eDate).add(14, "days")
      //Override for Funerals
      if(this.isFuneralRequest) {
        eDate = new moment()
      }
      return moment(eDate).format("yyyy-MM-DD");
    },
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
    e: {
      handler(val) {
        this.$emit('change', val)
      },
      deep: true
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
    defaultSender: {
      handler(val) {
        if(val) {
          this.e.Sender = val
        }
      },
      immediate: true
    },
    defaultReminderSender: {
      handler(val) {
        if(val) {
          this.e.ReminderSender = val
        }
      }, 
      immediate: true
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