import { defineComponent, PropType } from "vue"
import { ContentChannelItem, DefinedValue, ListItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Validator from "./validator"
import TimePicker from "./timePicker"
import Toggle from "./toggle"
import DatePicker from "./datePicker"
import { DateTime } from "luxon"
import rules from "../Rules/rules"


export default defineComponent({
    name: "EventForm.Components.Registration",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
      "tcc-validator": Validator,
      "tcc-time": TimePicker,
      "tcc-switch": Toggle,
      "tcc-date-pkr": DatePicker,
    },
    props: {
      e: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
      request: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
      original: {
        type: Object as PropType<ContentChannelItem>,
        required: false
      },
      ministries: Array as PropType<DefinedValue[]>,
      showValidation: Boolean,
      refName: String
    },
    setup() {

    },
    data() {
        return {
          rules: rules,
          errors: [] as Record<string, string>[]
        };
    },
    computed: {
      lastDate() {
        if(this.e?.attributeValues?.EventDate) {
          return this.e.attributeValues?.EventDate
        } else {
          let dates = this.request?.attributeValues?.EventDates.split(",").map((d: string) => d.trim)
          if(dates && dates.length > 0) {
            return dates[dates.length - 1]
          }
        }
        return ""
      },
      earliestDate() {
        let submissionDate = DateTime.now()
        if(this.request?.attributeValues) {
          let isFuneralRequest = false
          let val = this.request.attributeValues.Ministry
          let ministry = {} as DefinedValue | undefined
          if(val != '') {
            let min = JSON.parse(val) as ListItem
            ministry = this.ministries?.filter((dv: any) => {
              return dv.guid == min.value
            })[0]
          }
          if(ministry?.value?.toLowerCase().includes("funeral")) {
            isFuneralRequest = true
          }
          //Funeral Request, can submit for today
          if(isFuneralRequest) {
            return submissionDate.toFormat("yyyy-MM-dd")
          }
          //New request, must be min two weeks from today
          if(this.request.id == 0 || this.request.attributeValues.RequestStatus == 'Draft') {
            return submissionDate.plus({days: 14}).toFormat("yyyy-MM-dd")
          }
          //TODO COOKSEY
          //if newly requesting registration, two weeks from today, else two weeks from submission date
          //Existing request, no pending changes
          if(this.original?.attributeValues?.NeedsRegistration == 'True') {
            let val = this.request.startDateTime as string
            submissionDate = DateTime.fromISO(val)
          }
          return submissionDate.plus({days: 14}).toFormat("yyyy-MM-dd")
        }
      }
    },
    methods: {
      validate() {
        let formRef = this.$refs as any
        for(let r in formRef) {
          if(formRef[r].className?.includes("validator")) {
            formRef[r].validate()
          }
        }
      },
      validationChange(errs: Record<string, string>[]) {
        this.errors = errs
      }
    },
    watch: {
      'e.attributeValues.RegistrationFeeType'(val) {
        if(val.includes('No Fees')) {
          //Overwrite options and clear out all other choices.
          if(this.e?.attributeValues?.RegistrationFeeType) {
            this.e.attributeValues.RegistrationFeeType = "No Fees"
          }
        }
      },
      errors: {
        handler(val) {
          this.$emit("validation-change", { ref: this.refName, errors: val})
        },
        deep: true
      }
    },
    mounted() {
      if(this.showValidation) {
        this.validate()
      }
      if(this.e?.attributeValues?.StartTime) {
        let dt = {} as DateTime
        if(this.e?.attributeValues?.EventDate) {
          dt = DateTime.fromFormat(`${this.e?.attributeValues?.EventDate} ${this.e?.attributeValues?.StartTime}`, "yyyy-MM-dd HH:mm:ss")
        } else if(this.request?.attributeValues?.EventDates) {
          let dates = this.request?.attributeValues?.EventDates.split(",").map(d => {
            return DateTime.fromFormat(d.trim(), "yyyy-MM-dd")
          }).sort()
          dt = DateTime.fromFormat(`${dates[0].toFormat("yyyy-MM-dd")} ${this.e?.attributeValues?.StartTime}`, "yyyy-MM-dd HH:mm:ss")
        }
        let defaultTime = dt.minus({days: 1})
        //If Childcare close one week before event
        if(this.request?.attributeValues?.NeedsChildCare == 'True') {
          defaultTime = dt.minus({weeks: 1})
        }
        if(this.e.attributeValues.RegistrationEndDate == '') {
          this.e.attributeValues.RegistrationEndDate = defaultTime.toFormat("yyyy-MM-dd")
        }
        if(this.e.attributeValues.RegistrationEndTime == '') {
          this.e.attributeValues.RegistrationEndTime = defaultTime.toFormat("HH:mm:ss")
        }
      }
    },
    template: `
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationStartDate, e.attributes.RegistrationStartDate.name), , rules.dateCannotBeAfterEvent(e.attributeValues.RegistrationStartDate, lastDate, e.attributes.RegistrationStartDate.name)]" ref="validators_start">
        <tcc-date-pkr
          :label="e.attributes.RegistrationStartDate.name"
          v-model="e.attributeValues.RegistrationStartDate"
          :min="earliestDate"
        ></tcc-date-pkr>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationFeeType, e.attributes.RegistrationFeeType.name)]" ref="validators_feetype">
        <rck-field
          v-model="e.attributeValues.RegistrationFeeType"
          :attribute="e.attributes.RegistrationFeeType"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row" v-if="e.attributeValues.RegistrationFeeType != '' && e.attributeValues.RegistrationFeeType != 'No Fees'">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationFeeBudgetLine, e.attributes.RegistrationFeeBudgetLine.name)]" ref="validators_budget">
        <rck-field
          v-model="e.attributeValues.RegistrationFeeBudgetLine"
          :attribute="e.attributes.RegistrationFeeBudgetLine"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.RegistrationFeeType.includes('Individual')">
      <tcc-validator :rules="[rules.required(e.attributeValues.IndividualRegistrationFee, e.attributes.IndividualRegistrationFee.name)]" ref="validators_indv">
        <rck-field
          v-model="e.attributeValues.IndividualRegistrationFee"
          :attribute="e.attributes.IndividualRegistrationFee"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.RegistrationFeeType.includes('Couple')">
      <tcc-validator :rules="[rules.required(e.attributeValues.CoupleRegistrationFee, e.attributes.CoupleRegistrationFee.name)]" ref="validators_couple">
        <rck-field
          v-model="e.attributeValues.CoupleRegistrationFee"
          :attribute="e.attributes.CoupleRegistrationFee"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.RegistrationFeeType.includes('Online')">
      <tcc-validator :rules="[rules.required(e.attributeValues.OnlineRegistrationFee, e.attributes.OnlineRegistrationFee.name)]" ref="validators_online">
        <rck-field
          v-model="e.attributeValues.OnlineRegistrationFee"
          :attribute="e.attributes.OnlineRegistrationFee"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationEndDate, e.attributes.RegistrationEndDate.name), rules.dateCannotBeAfterEvent(e.attributeValues.RegistrationEndDate, lastDate, e.attributes.RegistrationEndDate.name)]" ref="validators_end">
        <tcc-date-pkr
          :label="e.attributes.RegistrationEndDate.name"
          v-model="e.attributeValues.RegistrationEndDate"
          :min="earliestDate"
        ></tcc-date-pkr>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationEndTime, e.attributes.RegistrationEndTime.name), rules.timeCannotBeAfterEvent(e.attributeValues.RegistrationEndTime, e.attributeValues.EndTime, e.attributes.RegistrationEndTime.name)]" ref="validators_endtime">
        <tcc-time 
          :label="e.attributes.RegistrationEndTime.name"
          v-model="e.attributeValues.RegistrationEndTime"
        ></tcc-time>
      </tcc-validator>
    </div>
  </div>
  <div class="row mt-2">
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsCheckin"
        :label="e.attributes.NeedsCheckin.name"
      ></tcc-switch>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.ExpectedAttendance > 100">
      <tcc-switch
        v-model="e.attributeValues.NeedsDatabaseSupportTeam"
        :label="e.attributes.NeedsDatabaseSupportTeam.name"
      ></tcc-switch>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.EventNeedsSeparateLink"
        :label="e.attributes.EventNeedsSeparateLink.name"
      ></tcc-switch>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.MaxRegistrants"
        :attribute="e.attributes.MaxRegistrants"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <br/>
  <h4 class="text-accent">Let's build-out the confirmation email your registrants will receive after signing up for this event</h4>
  <div class="row">
    <div class="col col-xs-12">
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationConfirmationEmailSender, e.attributes.RegistrationConfirmationEmailSender.name)]" ref="validators_sender">
        <rck-field
          v-model="e.attributeValues.RegistrationConfirmationEmailSender"
          :attribute="e.attributes.RegistrationConfirmationEmailSender"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <tcc-validator :rules="[]" ref="validators_email">
        <rck-field
          v-model="e.attributeValues.RegistrationConfirmationEmailFromAddress"
          :attribute="e.attributes.RegistrationConfirmationEmailFromAddress"
          :is-edit-mode="true"
        ></rck-field>
        <div class="input-hint rock-hint">If you want to use an email other than your sender's firstname.lastname@thecrossing email enter it here</div>
      </tcc-validator>
    </div>
  </div>
  <div class="row mb-2">
    <div class="col col-xs-12">
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationConfirmationEmailAdditionalDetails, e.attributes.RegistrationConfirmationEmailAdditionalDetails.name)]" ref="validators_details">
        <rck-field
          v-model="e.attributeValues.RegistrationConfirmationEmailAdditionalDetails"
          :attribute="e.attributes.RegistrationConfirmationEmailAdditionalDetails"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <tcc-switch
        v-model="e.attributeValues.NeedsReminderEmail"
        :label="e.attributes.NeedsReminderEmail.name"
      ></tcc-switch>
    </div>
  </div>
  <template v-if="e.attributeValues.NeedsReminderEmail == 'True'">
    <div class="row">
      <div class="col col-xs-12">
        <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationReminderEmailAdditionalDetails, e.attributes.RegistrationReminderEmailAdditionalDetails.name)]" ref="validators_reminderdetails">
          <rck-field
            v-model="e.attributeValues.RegistrationReminderEmailAdditionalDetails"
            :attribute="e.attributes.RegistrationReminderEmailAdditionalDetails"
            :is-edit-mode="true"
          ></rck-field>
        </tcc-validator>
      </div>
    </div>
    <div class="row mb-2">
      <div class="col col-xs-12 col-md-6">
        <rck-field
          v-model="e.attributeValues.ReminderEmailSendDate"
          :attribute="e.attributes.ReminderEmailSendDate"
          :is-edit-mode="true"
        ></rck-field>
      </div>
    </div>
  </template>
</rck-form>
`
});
