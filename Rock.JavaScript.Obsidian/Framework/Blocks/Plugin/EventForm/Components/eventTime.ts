import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Validator from "./validator"
import { DateTime, Interval } from "luxon"
import TimePicker from "./timePicker"


export default defineComponent({
    name: "EventForm.Components.EventTime",
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
            timeIsValid:(startTime: string, endTime: string, isStart: boolean) => {
              if(startTime && endTime) {
                let start = DateTime.fromFormat(startTime, 'HH:mm:ss')
                let end = DateTime.fromFormat(endTime, 'HH:mm:ss')
                let span = end.plus({ minutes: 1 })
                let interval = Interval.fromDateTimes(end, span)
                if(interval.isAfter(start)) {
                  return true
                }
                if(isStart) {
                  return `Start Time must be before ${end.toFormat('hh:mm a')}`
                } else {
                  return `End Time must be after ${start.toFormat('hh:mm a')}`
                }
              }
            },
          },
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
    },
    template: `
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.StartTime, 'Start Time'), rules.timeIsValid(e.attributeValues.StartTime, e.attributeValues.EndTime, true)]" ref="validators_start">
        <tcc-time 
          :label="e.attributes.StartTime.name"
          v-model="e.attributeValues.StartTime"
          :dates="request.attributeValues.EventDates" 
          @quicksettime="setEndTime"
          :quick-set-items='[
            {"mine": "08:20 AM", "theirs": "09:25 AM", "title": "1st Service"},
            {"mine": "09:35 AM", "theirs": "10:40 AM", "title": "2nd Service"},
            {"mine": "10:50 AM", "theirs": "11:55 AM", "title": "3rd Service"}
          ]'
        ></tcc-time>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.EndTime, 'End Time'), rules.timeIsValid(e.attributeValues.StartTime, e.attributeValues.EndTime, false)]" ref="validators_end">
        <tcc-time 
          :label="e.attributes.EndTime.name"
          v-model="e.attributeValues.EndTime"
          :dates="request.attributeValues.EventDates" 
          @quicksettime="setStartTime"
          :quick-set-items='[
            {"theirs": "08:20 AM", "mine": "09:25 AM", "title": "1st Service"},
            {"theirs": "09:35 AM", "mine": "10:40 AM", "title": "2nd Service"},
            {"theirs": "10:50 AM", "mine": "11:55 AM", "title": "3rd Service"}
          ]'
        ></tcc-time>
      </tcc-validator>
    </div>
  </div>
  <br/>
</rck-form>
`
});
