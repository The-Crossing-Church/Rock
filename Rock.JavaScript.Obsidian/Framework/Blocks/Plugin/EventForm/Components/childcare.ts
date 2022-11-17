import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockForm from "../../../../Controls/rockForm"
import RockField from "../../../../Controls/rockField"
import Validator from "./validator"
import Toggle from "./toggle"
import TimePicker from "./timePicker"
import { DateTime } from "luxon"
import rules from "../Rules/rules"


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
      }
    },
    mounted() {
      if(this.showValidation) {
        this.validate()
      }
      if(this.e?.attributeValues?.StartTime) {
        let dt = DateTime.fromFormat(this.e?.attributeValues?.StartTime, "HH:mm:ss")
        let defaultTime = dt.minus({minutes: 15})
        if(this.e.attributeValues.ChildcareStartTime == '') {
          this.e.attributeValues.ChildcareStartTime = defaultTime.toFormat("HH:mm:ss")
        }
      }
      if(this.e?.attributeValues?.EndTime) {
        if(this.e.attributeValues.ChildcareEndTime == '') {
          this.e.attributeValues.ChildcareEndTime = this.e?.attributeValues?.EndTime
        }
      }
    },
    template: `
<rck-form ref="form" @validationChanged="validationChange">
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
</rck-form>
`
});
