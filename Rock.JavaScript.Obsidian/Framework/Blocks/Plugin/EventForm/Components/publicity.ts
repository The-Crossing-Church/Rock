import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Validator from "./validator"
import Toggle from "./toggle"
import DatePicker from "./datePicker"
import PubDDL from "./publicityDropDown"
import { DateTime } from "luxon"
import { Select } from "ant-design-vue"
import rules from "../Rules/rules"

const { Option } = Select

export default defineComponent({
    name: "EventForm.Components.Publicity",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
      "tcc-validator": Validator,
      "tcc-switch": Toggle,
      "tcc-date-pkr": DatePicker,
      "tcc-pub-ddl": PubDDL,
      "a-select": Select,
      "a-select-option": Option,
    },
    props: {
      request: {
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
      minPubStartDate() {
        let submittedDate = DateTime.now()
        if(this.request?.attributeValues?.RequestStatus != 'Draft') {
          if(this.request?.startDateTime) {
            submittedDate = DateTime.fromISO(this.request?.startDateTime)
          }
        }
        let minStart = submittedDate.plus({weeks: 3})
        return minStart.toFormat("yyyy-MM-dd")
      },
      maxPubStartDate() {
        if(this.request?.attributeValues && this.request?.attributeValues.EventDates) {
          let dates = this.request?.attributeValues.EventDates.split(',').map((d) => DateTime.fromFormat(d.trim(), 'yyyy-MM-dd')).sort()
          if(dates && dates.length > 0) {
            return dates[0].minus({weeks: 3}).toFormat("yyyy-MM-dd")
          }
        }
        return ""
      },
      minPubEndDate() {
        if(this.request?.attributeValues && this.request?.attributeValues.PublicityStartDate) {
          let date = DateTime.fromFormat(this.request?.attributeValues.PublicityStartDate, "yyyy-MM-dd")
          return date.plus({weeks: 3}).toFormat("yyyy-MM-dd")
        } else if(this.minPubStartDate) {
          let date = DateTime.fromFormat(this.minPubStartDate, "yyyy-MM-dd")
          return date.plus({weeks: 3}).toFormat("yyyy-MM-dd")
        }
        return ""
      },
      maxPubEndDate() {
        if(this.request?.attributeValues && this.request?.attributeValues.EventDates) {
          let dates = this.request?.attributeValues.EventDates.split(',').map((d) => DateTime.fromFormat(d.trim(), 'yyyy-MM-dd')).sort()
          if(dates && dates.length > 0) {
            return dates[dates.length - 1].toFormat("yyyy-MM-dd")
          }
        }
        return ""
      },
      pubStrategiesNotSticky() {
        if(this.request?.attributes?.PublicityStrategies) {
          let attr = JSON.parse(JSON.stringify(this.request.attributes.PublicityStrategies))
          if(attr.configurationValues) {
            let values = JSON.parse(attr.configurationValues.values)
            values = values.filter((v: any) => {
              return v.value != 'Announcement'
            })
            attr.configurationValues.values = JSON.stringify(values)
          }
          return attr
        }
        return null
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
      'request.attributeValues.EventisSticky'(val) {
        if(val == 'False') {
          //Cannot request announcement
          if(this.request?.attributeValues?.PublicityStrategies) {
            let strategies = this.request.attributeValues.PublicityStrategies.split(',')
            strategies = strategies.filter((s: string) => {
              return s != "Announcement"
            })
            this.request.attributeValues.PublicityStrategies = strategies.join(',')
          }
        } 
      }, 
      'request.attributeValues.PublicityStrategies'(val) {
        if(this.request?.attributeValues?.EventisSticky == 'False') {
          let strategies = val.split(',')
          strategies = strategies.filter((s: string) => {
            return s != "Announcement"
          })
          if(val != strategies.join(',')) {
            this.request.attributeValues.PublicityStrategies = strategies.join(',')
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
<rck-form ref="form" @validationChanged="validationChange">
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
      <tcc-validator :rules="[rules.required(request.attributeValues.PublicityStartDate, request.attributes.PublicityStartDate.name), rules.pubStartIsValid(request.attributeValues.PublicityStartDate, request.attributeValues.PublicityEndDate, minPubStartDate, maxPubStartDate)]" ref="validators_start">
        <tcc-date-pkr
          v-model="request.attributeValues.PublicityStartDate"
          :label="request.attributes.PublicityStartDate.name"
          :min="minPubStartDate"
          :max="maxPubStartDate"
        ></tcc-date-pkr>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(request.attributeValues.PublicityEndDate, request.attributes.PublicityEndDate.name), rules.pubEndIsValid(request.attributeValues.PublicityEndDate, request.attributeValues.PublicityStartDate, request.attributeValues.EventDates, minPubEndDate, maxPubEndDate)]" ref="validators_end">
        <tcc-date-pkr
          v-model="request.attributeValues.PublicityEndDate"
          :label="request.attributes.PublicityEndDate.name"
          :min="minPubEndDate"
          :max="maxPubEndDate"
        ></tcc-date-pkr>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <div id="pubStrat">
        <tcc-validator :rules="[rules.required(request.attributeValues.PublicityStrategies, request.attributes.PublicityStrategies.name)]" ref="validators_strategies">
          <rck-field
            v-if="request.attributeValues.EventisSticky == 'False'"
            v-model="request.attributeValues.PublicityStrategies"
            :attribute="pubStrategiesNotSticky"
            :is-edit-mode="true"
          ></rck-field>
          <rck-field
            v-else
            v-model="request.attributeValues.PublicityStrategies"
            :attribute="request.attributes.PublicityStrategies"
            :is-edit-mode="true"
          ></rck-field>
        </tcc-validator>
      </div>
    </div>
  </div>
  <br/>
</rck-form>
`
});
