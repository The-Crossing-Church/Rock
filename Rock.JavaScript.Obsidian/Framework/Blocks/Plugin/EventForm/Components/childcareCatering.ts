import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Validator from "./validator"
import TimePicker from "./timePicker"
import { DateTime } from "luxon"
import rules from "../Rules/rules"

export default defineComponent({
    name: "EventForm.Components.ChildcareCatering",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
      "tcc-validator": Validator,
      "tcc-time": TimePicker,
    },
    props: {
      e: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
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
      'e.attributeValues.ChildcareVendor': {
        handler(val) {
          if(val == 'Pizza' && this.e?.attributeValues && this.e.attributeValues.ChildcarePreferredMenu == '') {
            this.e.attributeValues.ChildcarePreferredMenu = 'Pizza'
          }
        }
      }
    },
    mounted() {
      if(this.showValidation) {
        this.validate()
      }
      if(this.e?.attributeValues?.StartTime) {
        let dt = DateTime.fromFormat(this.e?.attributeValues?.StartTime, "HH:mm:ss")
        let defaultTime = dt.minus({minutes: 15})
        if(this.e.attributeValues.ChildcareFoodTime == '') {
          this.e.attributeValues.ChildcareFoodTime = defaultTime.toFormat("HH:mm:ss")
        }
      }
    },
    template: `
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareVendor, e.attributes.ChildcareVendor.name)]" ref="validators_vendor">
        <rck-field
          v-model="e.attributeValues.ChildcareVendor"
          :attribute="e.attributes.ChildcareVendor"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareFoodTime, e.attributes.ChildcareFoodTime.name), rules.timeCannotBeAfterEvent(e.attributeValues.ChildcareFoodTime, e.attributeValues.EndTime,  e.attributes.ChildcareFoodTime.name)]" ref="validators_time">
        <tcc-time 
          :label="e.attributes.ChildcareFoodTime.name"
          v-model="e.attributeValues.ChildcareFoodTime"
        ></tcc-time>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareCateringBudgetLine, e.attributes.ChildcareCateringBudgetLine.name)]" ref="validators_budget">
        <rck-field
          v-model="e.attributeValues.ChildcareCateringBudgetLine"
          :attribute="e.attributes.ChildcareCateringBudgetLine"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareCateringBudgetMinistry, e.attributes.ChildcareCateringBudgetMinistry.name)]" ref="validators_budgetmin">
        <rck-field
          v-model="e.attributeValues.ChildcareCateringBudgetMinistry"
          :attribute="e.attributes.ChildcareCateringBudgetMinistry"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcarePreferredMenu, e.attributes.ChildcarePreferredMenu.name)]" ref="validators_menu">
        <rck-field
          v-model="e.attributeValues.ChildcarePreferredMenu"
          :attribute="e.attributes.ChildcarePreferredMenu"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
</rck-form>
`
});
