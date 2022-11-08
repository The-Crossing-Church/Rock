import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Validator from "./validator"
import Toggle from "./toggle"
import DatePicker from "./datePicker"
import PubDDL from "./publicityDropDown"
import { DateTime, Interval } from "luxon"


export default defineComponent({
    name: "EventForm.Components.Publicity",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
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
      showValidation: Boolean,
      refName: String
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
            pubStartIsValid(value: string, end: string, minPubStartDate: string, maxPubStartDate: string) {
              if(value && end) {
                let startDt = DateTime.fromFormat(value, "yyyy-MM-dd")
                let endDt = DateTime.fromFormat(end, "yyyy-MM-dd")
                let duration = Interval.fromDateTimes(startDt, endDt)
                let days = duration.count('days')
                if(days < 21) {
                  return 'Publicity must run for a minimum of 3 weeks'
                }
                if(minPubStartDate) {
                  let minStartDt = DateTime.fromFormat(minPubStartDate, "yyyy-MM-dd")
                  if(startDt < minStartDt) {
                    return `Publicity cannot start before ${minStartDt.toFormat("MM/dd/yyyy")}`
                  }
                }
                if(maxPubStartDate) {
                  let maxStartDt = DateTime.fromFormat(maxPubStartDate, "yyyy-MM-dd")
                  if(startDt > maxStartDt) {
                    return `Publicity cannot start after ${maxStartDt.toFormat("MM/dd/yyyy")}`
                  }
                }
              }
              return true
            },
            pubEndIsValid(value: string, start: string, eventDates: string, minPubEndDate: string, maxPubEndDate: string) {
              if(value && start) {
                let startDt = DateTime.fromFormat(start, "yyyy-MM-dd")
                let endDt = DateTime.fromFormat(value, "yyyy-MM-dd")
                let duration = Interval.fromDateTimes(startDt, endDt)
                let days = duration.count('days')
                if(days < 21) {
                  return 'Publicity must run for a minimum of 3 weeks'
                }
                if(minPubEndDate) {
                  let minEndDt = DateTime.fromFormat(minPubEndDate, "yyyy-MM-dd")
                  if(endDt < minEndDt) {
                    return `Publicity cannot end before ${minEndDt.toFormat("MM/dd/yyyy")}`
                  }
                }
                if(maxPubEndDate) {
                  let maxEndDt = DateTime.fromFormat(maxPubEndDate, "yyyy-MM-dd")
                  if(endDt > maxEndDt) {
                    return `Publicity cannot end after ${maxEndDt.toFormat("MM/dd/yyyy")}`
                  }
                }
                if(eventDates) {
                  let dates = eventDates.split(",").map(d => DateTime.fromFormat(d.trim(), "yyyy-MM-dd")).sort()
                  if(endDt > dates[dates.length - 1]) {
                    return 'Publicity cannot end after event'
                  }
                }
              }
              return true
            }
          },
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
      <tcc-validator :rules="[rules.required(request.attributeValues.PublicityStrategies, request.attributes.PublicityStrategies.name)]" ref="validators_strategies">
        <rck-field
          v-model="request.attributeValues.PublicityStrategies"
          :attribute="request.attributes.PublicityStrategies"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <br/>
</rck-form>
`
});
