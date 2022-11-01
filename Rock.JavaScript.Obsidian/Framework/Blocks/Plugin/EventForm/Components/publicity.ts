import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField";
import Validator from "./validator"
import Toggle from "./toggle"
import DatePicker from "./datePicker"
import PubDDL from "./publicityDropDown"


export default defineComponent({
    name: "EventForm.Components.Publicity",
    components: {
      "rck-field": RockField,
      "tcc-validator": Validator,
      "tcc-switch": Toggle,
      "tcc-date-pkr": DatePicker,
      "tcc-pub-ddl": PubDDL
    },
    props: {
      request: {
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
    <div class="col col-xs-12">
      <tcc-validator :rules="[rules.required(request.attributeValues.WhyAttend, request.attributes.WhyAttend.name)]" ref="validators_why">
        <rck-field
          v-model="request.attributeValues.WhyAttend"
          :attribute="request.attributes.WhyAttend"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <div class="row row-equal-height">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(request.attributeValues.TargetAudience, request.attributes.TargetAudience.name)]" ref="validators_audience"  style="width: 100%;">
        <tcc-pub-ddl
          v-model="request.attributeValues.TargetAudience"
          :label="request.attributes.TargetAudience.name"
          :items="JSON.parse(request.attributes.TargetAudience.configurationValues.values)"
        ></tcc-pub-ddl>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="request.attributeValues.EventisSticky"
        :label="request.attributes.EventisSticky.name"
      ></tcc-switch>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(request.attributeValues.PublicityStartDate, request.attributes.PublicityStartDate.name)]" ref="validators_start">
        <tcc-date-pkr
          v-model="request.attributeValues.PublicityStartDate"
          :label="request.attributes.PublicityStartDate.name"
        ></tcc-date-pkr>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(request.attributeValues.PublicityEndDate, request.attributes.PublicityEndDate.name)]" ref="validators_end">
        <tcc-date-pkr
          v-model="request.attributeValues.PublicityEndDate"
          :label="request.attributes.PublicityEndDate.name"
        ></tcc-date-pkr>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(request.attributeValues.PublicityStrategies, request.attributes.PublicityStrategies.name)]" ref="validators_strategies">
        <rck-field
          v-model="request.attributeValues.PublicityStrategies"
          :attribute="request.attributes.PublicityStrategies"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
</div>
`
});
