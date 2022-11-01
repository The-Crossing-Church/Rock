import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import Validator from "./validator"
import TimePicker from "./timePicker"
import Toggle from "./toggle"
import DatePicker from "./datePicker"


export default defineComponent({
    name: "EventForm.Components.Registration",
    components: {
      "rck-field": RockField,
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
      showValidation: Boolean
    },
    setup() {

    },
    data() {
        return {
          rules: {
            required: (value: any, key: string) => {
              if(typeof value === 'string') {
                if(value.includes("{")) {
                  let obj = JSON.parse(value)
                  return obj.value != '' || `${key} is required`
                } 
              } 
              return !!value || `${key} is required`
            },
          }
        };
    },
    computed: {
      errors() {
        let formRef = this.$refs as any
        let errs = [] as string[]
        for(let r in formRef) {
          if(formRef[r].className?.includes("validator")) {
            errs.push(...formRef[r].errors)
          }
        }
        return errs
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
      }
    },
    mounted() {
      if(this.showValidation) {
        this.validate()
      }
    },
    template: `
<div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationStartDate, e.attributes.RegistrationStartDate.name)]" ref="validators_start">
        <tcc-date-pkr
          :label="e.attributes.RegistrationStartDate.name"
          v-model="e.attributeValues.RegistrationStartDate"
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
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationEndDate, e.attributes.RegistrationEndDate.name)]" ref="validators_end">
        <tcc-date-pkr
          :label="e.attributes.RegistrationEndDate.name"
          v-model="e.attributeValues.RegistrationEndDate"
        ></tcc-date-pkr>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.RegistrationEndTime, e.attributes.RegistrationEndTime.name)]" ref="validators_endtime">
        <tcc-time 
          :label="e.attributes.RegistrationEndTime.name"
          v-model="e.attributeValues.RegistrationEndTime"
        ></tcc-time>
      </tcc-validator>
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
  <div class="row" v-if="e.attributeValues.NeedsReminderEmail == 'True'">
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
  <div class="row">
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
  </div>
</div>
`
});
