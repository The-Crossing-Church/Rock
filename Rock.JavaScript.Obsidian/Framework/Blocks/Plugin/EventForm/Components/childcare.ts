import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockForm from "../../../../Controls/rockForm";
import RockField from "../../../../Controls/rockField";
import Validator from "./validator";
import Toggle from "./toggle";
import TimePicker from "./timePicker"


export default defineComponent({
    name: "EventForm.Components.ChildCare",
    components: {
      "rck-field": RockField,
      "tcc-validator": Validator,
      "rck-form": RockForm,
      "tcc-switch": Toggle,
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
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareStartTime, e.attributes.ChildcareStartTime.name)]" ref="validators_start">
        <tcc-time 
          :label="e.attributes.ChildcareStartTime.name"
          v-model="e.attributeValues.ChildcareStartTime"
        ></tcc-time>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareEndTime, e.attributes.ChildcareEndTime.name)]" ref="validators_end">
        <tcc-time 
          :label="e.attributes.ChildcareEndTime.name"
          v-model="e.attributeValues.ChildcareEndTime"
        ></tcc-time>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareOptions, e.attributes.ChildcareOptions.name)]" ref="validators_ops">
        <rck-field
          v-model="e.attributeValues.ChildcareOptions"
          :attribute="e.attributes.ChildcareOptions"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.EstimatedNumberofKids, e.attributes.EstimatedNumberofKids.name)]" ref="validators_numkids">
        <rck-field
          v-model="e.attributeValues.EstimatedNumberofKids"
          :attribute="e.attributes.EstimatedNumberofKids"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
</div>
`
});
