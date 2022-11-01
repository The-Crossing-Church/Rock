import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField";
import Validator from "./validator";
import TimePicker from "./timePicker";


export default defineComponent({
    name: "EventForm.Components.ChildcareCatering",
    components: {
      "rck-field": RockField,
      "tcc-validator": Validator,
      "tcc-time": TimePicker,
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
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareVendor, e.attributes.ChildcareVendor.name)]" ref="validators_vendor">
        <rck-field
          v-model="e.attributeValues.ChildcareVendor"
          :attribute="e.attributes.ChildcareVendor"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareCateringBudgetLine, e.attributes.ChildcareCateringBudgetLine.name)]" ref="validators_budget">
        <rck-field
          v-model="e.attributeValues.ChildcareCateringBudgetLine"
          :attribute="e.attributes.ChildcareCateringBudgetLine"
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
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareFoodTime, e.attributes.ChildcareFoodTime.name)]" ref="validators_time">
        <tcc-time 
          :label="e.attributes.ChildcareFoodTime.name"
          v-model="e.attributeValues.ChildcareFoodTime"
        ></tcc-time>
      </tcc-validator>
    </div>
  </div>
</div>
`
});
