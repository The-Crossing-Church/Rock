import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockForm from "@Obsidian/Controls/rockForm.obs"
import DatePicker from "./datePicker"
import Validator from "./validator"
import rules from "../Rules/rules"
import { DateTime } from "luxon"

export default defineComponent({
    name: "EventForm.Components.ChildcareRegistration",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
      "tcc-date-pkr": DatePicker,
      "tcc-validator": Validator
    },
    props: {
      e: {
        type: Object as PropType<ContentChannelItemBag>,
        required: false
      },
      request: {
        type: Object as PropType<ContentChannelItemBag>,
        required: false
      },
      showValidation: Boolean,
      refName: String,
      readonly: Boolean
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
      maxCloseDate() {
        if(this.request?.attributeValues) {
          let dates = this.request.attributeValues.EventDates.split(',')
          let dt 
          if(dates.length > 1) {
            dt = DateTime.fromFormat(dates[0], 'yyyy-MM-dd').minus({ week: 1 })
          } else {
            dt = DateTime.fromFormat(dates[0], 'yyyy-MM-dd').minus({ day: 2 })
          }
          return dt.toFormat('yyyy-MM-dd')
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
      errors: {
        handler(val) {
          this.$emit("validation-change", { ref: this.refName, errors: val})
        },
        deep: true
      }, 
    },
    mounted() {
      if(this.showValidation) {
        this.validate()
      }
    },
    template: `
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :name="e.attributes.ChildcareCost.key" :rules="[rules.nonNegativeNumber(e.attributeValues.ChildcareCost, e.attributes.ChildcareCost.name)]" ref="validators_cost">
        <rck-field
          v-model="e.attributeValues.ChildcareCost"
          :attribute="e.attributes.ChildcareCost"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="txtChildcareCost"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :name="e.attributes.ChildcareRegistrationCloseDate.key" :rules="[rules.required(e.attributeValues.ChildcareRegistrationCloseDate, e.attributes.ChildcareRegistrationCloseDate.name), rules.childcareCloseIsValid(e.attributeValues.ChildcareRegistrationCloseDate, request.attributeValues.EventDates)]" ref="validators_cc_reg_close">
        <tcc-date-pkr
          :label="e.attributes.ChildcareRegistrationCloseDate.name"
          v-model="e.attributeValues.ChildcareRegistrationCloseDate"
          :max="maxCloseDate"
          :disblaed="readonly"
          id="dateChildcareRegistrationCloseDate"
        ></tcc-date-pkr>
      </tcc-validator>
    </div>
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.ChildcareRegistrationInstructions"
        :attribute="e.attributes.ChildcareRegistrationInstructions"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
        id="txtChildcareRegistrationInstructions"
      ></rck-field>
      <div class="text-errors text-red">
        The database team will copy this information <i>exactly</i> as it appears here.
      </div>
    </div>
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.ChildcareRegistrationConfirmation"
        :attribute="e.attributes.ChildcareRegistrationConfirmation"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
        id="txtChildcareRegistrationConfirmation"
      ></rck-field>
      <div class="text-errors text-red">
        The database team will copy this information <i>exactly</i> as it appears here.
      </div>
    </div>
  </div>
</rck-form>
`
});
