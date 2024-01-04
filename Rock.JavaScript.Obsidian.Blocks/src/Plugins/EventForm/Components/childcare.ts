import { defineComponent, PropType } from "vue";
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import RockForm from "@Obsidian/Controls/rockForm"
import RockField from "@Obsidian/Controls/rockField"
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
          errors: [] as Record<string, string>[],
          childcareOptsAttr: {} as any
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
      'e.attributeValues.ChildcareOptions': {
        handler(val) {
          if(val.includes("All") && this.e?.attributes?.ChildcareOptions && this.e?.attributeValues?.ChildcareOptions) {
            let rawConfigValues = this.e?.attributes?.ChildcareOptions?.configurationValues?.values as string
            let values = JSON.parse(rawConfigValues)
            this.e.attributeValues.ChildcareOptions = values.map((v: any) => { return v.value }).join(",")
          }
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
      if(this.e?.attributes?.ChildcareOptions) {
        this.childcareOptsAttr = JSON.parse(JSON.stringify(this.e?.attributes?.ChildcareOptions))
        let configValues = JSON.parse(this.childcareOptsAttr.configurationValues.values)
        configValues.push({"value": "All", "text": "Select All"})
        this.childcareOptsAttr.configurationValues.values = JSON.stringify(configValues)
      }
    },
    template: `
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareStartTime, e.attributes.ChildcareStartTime.name)]" ref="validators_start" v-if="!readonly">
        <tcc-time 
          :label="e.attributes.ChildcareStartTime.name"
          v-model="e.attributeValues.ChildcareStartTime"
          id="TimeChildcareStart"
        ></tcc-time>
      </tcc-validator>
      <rck-field
        v-else
        v-model="e.attributeValues.ChildcareStartTime"
        :attribute="e.attributes.ChildcareStartTime"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="timeChildcareStart"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareEndTime, e.attributes.ChildcareEndTime.name)]" ref="validators_end" v-if="!readonly">
        <tcc-time 
          :label="e.attributes.ChildcareEndTime.name"
          v-model="e.attributeValues.ChildcareEndTime"
          id="TimeChildcareEnd"
        ></tcc-time>
      </tcc-validator>
      <rck-field
        v-else
        v-model="e.attributeValues.ChildcareEndTime"
        :attribute="e.attributes.ChildcareEndTime"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="timeChildcareEnd"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareBudgetMinistry, e.attributes.ChildcareBudgetMinistry.name)]" ref="validators_budgetmin">
        <rck-field
          v-model="e.attributeValues.ChildcareBudgetMinistry"
          :attribute="e.attributes.ChildcareBudgetMinistry"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="ddlChildcareBudgetMinistry"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareBudgetLine, e.attributes.ChildcareBudgetLine.name)]" ref="validators_budget">
        <rck-field
          v-model="e.attributeValues.ChildcareBudgetLine"
          :attribute="e.attributes.ChildcareBudgetLine"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="ddlChildcareBudgetLine"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ChildcareOptions, e.attributes.ChildcareOptions.name)]" ref="validators_ops">
        <rck-field
          v-model="e.attributeValues.ChildcareOptions"
          :attribute="childcareOptsAttr"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="ddlChildcareOptions"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.EstimatedNumberofKids, e.attributes.EstimatedNumberofKids.name)]" ref="validators_numkids">
        <rck-field
          v-model="e.attributeValues.EstimatedNumberofKids"
          :attribute="e.attributes.EstimatedNumberofKids"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="txtEstimatedNumberofKids"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
</rck-form>
`
});
