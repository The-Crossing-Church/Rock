import { defineComponent, PropType } from "vue"
import { Person } from "../../../../ViewModels"
import { SubmissionFormBlockViewModel } from "../submissionFormBlockViewModel"
import { useStore } from "../../../../Store/index"
import { Switch } from "ant-design-vue"
import { DateTime, Duration, Interval } from "luxon"
import RockForm from "../../../../Controls/rockForm"
import RockField from "../../../../Controls/rockField"
import RockFormField from "../../../../Elements/rockFormField"
import TextBox from "../../../../Elements/textBox"
import RockLabel from "../../../../Elements/rockLabel"
import Validator from "./validator"
import DatePicker from "./calendar"
import AutoComplete from "./roomPicker"
import Chip from "./chip"
import Toggle from "./toggle"
import TimePicker from "./timePicker"

const store = useStore();

export default defineComponent({
    name: "EventForm.Components.BasicInfo",
    components: {
        "tcc-complete": AutoComplete,
        "tcc-chip": Chip,
        "tcc-switch": Toggle,
        "tcc-time": TimePicker,
        "tcc-date": DatePicker,
        "tcc-validator": Validator,
        "a-switch": Switch,
        "rck-form": RockForm,
        "rck-field": RockField,
        "rck-form-field": RockFormField,
        "rck-lbl": RockLabel,
        "rck-text": TextBox
    },
    props: {
        viewModel: {
            type: Object as PropType<SubmissionFormBlockViewModel>,
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
        /** The person currently authenticated */
        currentPerson(): Person | null {
            return store.state.currentPerson;
        },
        eventDates() {
          if (this.viewModel?.request?.attributeValues?.EventDates) {
            return this.viewModel.request.attributeValues.EventDates.split(',').map((d: any) => d.trim())
          }
          return []
        },
        ministries(): Array<any> | null {
            let list = null
            if (this.viewModel) {
                list = this.viewModel.ministries.map(m => {
                    return { value: m.id.toString(), text: m.value }
                })
            }
            return list
        },
        minEventDate() {
          let date = DateTime.now()
          let span = Duration.fromObject({days: 0})
          if(this.viewModel) {
            if( this.viewModel.request?.attributeValues?.NeedsOnline == "True"
              || this.viewModel.request?.attributeValues?.NeedsRegistration == "True"
              || this.viewModel.request?.attributeValues?.NeedsWebCalendar == "True"
              || this.viewModel.request?.attributeValues?.NeedsCatering == "True"
              || this.viewModel.request?.attributeValues?.NeedsOpsAccommodations == "True"
              || this.viewModel.request?.attributeValues?.NeedsProductionAccommodations == "True"
            ) {
              span = Duration.fromObject({days: 14})
            }
            if(this.viewModel.request?.attributeValues?.NeedsChildCare == "True") {
              span = Duration.fromObject({days: 30})
            }
            if(this.viewModel.request?.attributeValues?.NeedsPublicity == "True") {
              span = Duration.fromObject({weeks: 6})
            }
            //Override restrictions for Funerals
            if(this.viewModel.request?.attributeValues?.Ministry) {
              let ministry = JSON.parse(this.viewModel.request?.attributeValues?.Ministry)
              if(ministry.text.toLowerCase().includes("funeral")) {
                span = Duration.fromObject({days: 0})
              }
            }
          }
          date = date.plus(span)
          return date.toFormat('yyyy-MM-dd')
        },
        requestType() {
          if(this.viewModel?.request?.attributeValues?.NeedsSpace == "True"
            && (this.viewModel?.request?.attributeValues?.NeedsOnline == "False"
              && this.viewModel?.request?.attributeValues?.NeedsCatering == "False"
              && this.viewModel?.request?.attributeValues?.NeedsChildCare == "False"
              && this.viewModel?.request?.attributeValues?.NeedsOpsAccommodations == "False"
              && this.viewModel?.request?.attributeValues?.NeedsRegistration == "False"
              && this.viewModel?.request?.attributeValues?.NeedsPublicity == "False"
            )
          ) {
            return "meeting"
          }
          return "event"
        },
        labelIsSame() {
          return `Will each occurrence of your ${this.requestType} have the exact same start and end time? (${this.viewModel?.request?.attributeValues?.IsSame == 'True' ? 'Yes' : 'No'})`
        },
    },
    methods: {
      removeDate(date: string) {
        if (this.viewModel?.request?.attributeValues?.EventDates) {
          let dates = this.viewModel.request.attributeValues.EventDates.split(',')
          let idx = dates.indexOf(date)
          dates.splice(idx, 1)
          this.viewModel.request.attributeValues.EventDates = dates.join(",")
        }
      },
      formatDate(date: string) {
        return DateTime.fromFormat(date, "yyyy-MM-dd").toFormat("DDDD")
      },
      capitalize(val: string) {
        return val.charAt(0).toUpperCase() + val.slice(1);
      },
      setEndTime(t: string) {
        if(this.viewModel?.events && this.viewModel.events.length > 0 && this.viewModel.events[0].attributeValues) {
          this.viewModel.events[0].attributeValues.EndTime = t
        }
      },
      setStartTime(t: string) {
        if(this.viewModel?.events && this.viewModel.events.length > 0 && this.viewModel.events[0].attributeValues) {
          this.viewModel.events[0].attributeValues.StartTime = t
        }
      },
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
      eventDates: {
        handler(val) {
          if(val.length < 2 && this.viewModel?.request.attributeValues) {
            //When there is only one event date these values should reset to their defaults
            this.viewModel.request.attributeValues.IsSame = "True"
            this.viewModel.request.attributeValues.EventsNeedSeparateLinks = "False"
          }
        }, 
        deep: true
      },
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
<h3>Basic Information</h3>
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row">
    <div class="col col-xs-12">
      <tcc-validator name="name" :rules="[rules.required(viewModel.request.title, 'Name')]" ref="validators_name">
        <rck-lbl>Name of {{requestType}} on calendar</rck-lbl>
        <rck-text
          v-model="viewModel.request.title"
        ></rck-text>
      </tcc-validator>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator name="ministry" :rules="[rules.required(viewModel.request.attributeValues.Ministry, 'Ministry')]" ref="validators_ministry">
        <rck-field
          v-model="viewModel.request.attributeValues.Ministry"
          :attribute="viewModel.request.attributes.Ministry"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :name="contact" :rules="[rules.required(viewModel.request.attributeValues.Contact, 'Contact')]" ref="validators_contact">
        <rck-field
          v-model="viewModel.request.attributeValues.Contact"
          :attribute="viewModel.request.attributes.Contact"
          :is-edit-mode="true"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <br/>
  <div class="row">
    <div class="col col-xs-12 col-md-4">
      <tcc-date v-model="viewModel.request.attributeValues.EventDates" :min="minEventDate" :multiple="true"></tcc-date>
    </div>
    <div class="col col-xs-12 col-md-8" style="display: flex; flex-wrap: wrap; align-content: flex-start;">
      <tcc-chip v-for="d in eventDates" :key="d" v-on:chipdeleted="removeDate(d)">
        {{formatDate(d)}}
      </tcc-chip>
    </div>
  </div>
  <br/><br/>
  <div class="row" v-if="eventDates.length > 1">
    <div class="col col-xs-12">
      <tcc-switch
        v-model="viewModel.request.attributeValues.IsSame"
        :label="viewModel.request.attributes.IsSame.name"
      ></tcc-switch>
    </div>
  </div>
  <div class="row" v-if="viewModel.request.attributeValues.IsSame == 'True'">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator name="starttime" :rules="[rules.required(viewModel.events[0].attributeValues.StartTime, 'Start Time'), rules.timeIsValid(viewModel.events[0].attributeValues.StartTime, viewModel.events[0].attributeValues.EndTime, true)]" ref="validators_start">
        <tcc-time 
          :label="viewModel.events[0].attributes.StartTime.name"
          v-model="viewModel.events[0].attributeValues.StartTime"
          :dates="viewModel.request.attributeValues.EventDates" 
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
      <tcc-validator name="endtime" :rules="[rules.required(viewModel.events[0].attributeValues.EndTime, 'End Time'), rules.timeIsValid(viewModel.events[0].attributeValues.StartTime, viewModel.events[0].attributeValues.EndTime, false)]" ref="validators_end">
        <tcc-time 
          :label="viewModel.events[0].attributes.EndTime.name"
          v-model="viewModel.events[0].attributeValues.EndTime"
          :dates="viewModel.request.attributeValues.EventDates" 
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
