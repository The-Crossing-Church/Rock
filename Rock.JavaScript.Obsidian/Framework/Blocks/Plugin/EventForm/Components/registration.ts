import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockForm from "../../../../Controls/rockForm";
import RockField from "../../../../Controls/rockField";
import RockFormField from "../../../../Elements/rockFormField";
import TimePicker from "./timePicker"
import Toggle from "./toggle"
import DatePicker from "./datePicker"


export default defineComponent({
    name: "EventForm.Components.Registration",
    components: {
      "rck-field": RockField,
      "rck-form-field": RockFormField,
      "rck-form": RockForm,
      "tcc-time": TimePicker,
      "tcc-switch": Toggle,
      "tcc-date-pkr": DatePicker,
    },
    props: {
      e: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
    },
    setup() {

    },
    data() {
        return {
          
        };
    },
    computed: {

    },
    methods: {

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
      
    },
    template: `
<div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-date-pkr
        :label="e.attributes.RegistrationStartDate.name"
        v-model="e.attributeValues.RegistrationStartDate"
      ></tcc-date-pkr>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.RegistrationFeeType"
        :attribute="e.attributes.RegistrationFeeType"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row" v-if="e.attributeValues.RegistrationFeeType != '' && e.attributeValues.RegistrationFeeType != 'No Fees'">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.RegistrationFeeBudgetLine"
        :attribute="e.attributes.RegistrationFeeBudgetLine"
        :is-edit-mode="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.RegistrationFeeType.includes('Individual')">
      <rck-field
        v-model="e.attributeValues.IndividualRegistrationFee"
        :attribute="e.attributes.IndividualRegistrationFee"
        :is-edit-mode="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.RegistrationFeeType.includes('Couple')">
      <rck-field
        v-model="e.attributeValues.CoupleRegistrationFee"
        :attribute="e.attributes.CoupleRegistrationFee"
        :is-edit-mode="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.RegistrationFeeType.includes('Online')">
      <rck-field
        v-model="e.attributeValues.OnlineRegistrationFee"
        :attribute="e.attributes.OnlineRegistrationFee"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-date-pkr
        :label="e.attributes.RegistrationEndDate.name"
        v-model="e.attributeValues.RegistrationEndDate"
      ></tcc-date-pkr>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-time 
        :label="e.attributes.RegistrationEndTime.name"
        v-model="e.attributeValues.RegistrationEndTime"
      ></tcc-time>
    </div>
  </div>
  <br/>
  <h4 class="text-accent">Let's build-out the confirmation email your registrants will receive after signing up for this event</h4>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.RegistrationConfirmationEmailSender"
        :attribute="e.attributes.RegistrationConfirmationEmailSender"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.RegistrationConfirmationEmailFromAddress"
        :attribute="e.attributes.RegistrationConfirmationEmailFromAddress"
        :is-edit-mode="true"
      ></rck-field>
      <div class="input-hint rock-hint">If you want to use an email other than your sender's firstname.lastname@thecrossing email enter it here</div>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.RegistrationConfirmationEmailAdditionalDetails"
        :attribute="e.attributes.RegistrationConfirmationEmailAdditionalDetails"
        :is-edit-mode="true"
      ></rck-field>
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
      <rck-field
        v-model="e.attributeValues.RegistrationReminderEmailAdditionalDetails"
        :attribute="e.attributes.RegistrationReminderEmailAdditionalDetails"
        :is-edit-mode="true"
      ></rck-field>
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
