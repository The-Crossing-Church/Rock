import { defineComponent, PropType, reactive, ref, UnwrapRef } from "vue";
import { Person, ContentChannelItem } from "../../../../ViewModels";
import { SubmissionFormBlockViewModel } from "../submissionFormBlockViewModel";
import { useStore } from "../../../../Store/index";
import { Switch } from "ant-design-vue";
import { DateTime, Duration } from "luxon";
import RockForm from "../../../../Controls/rockForm";
import RockField from "../../../../Controls/rockField";
import RockFormField from "../../../../Elements/rockFormField";
import TextBox from "../../../../Elements/textBox";
import RockLabel from "../../../../Elements/rockLabel";
import DatePicker from "./calendar"
import AutoComplete from "./roomPicker"
import Chip from "./chip"
import Toggle from "./toggle";
import TimePicker from "./timePicker";

const store = useStore();


export default defineComponent({
    name: "EventForm.Components.BasicInfo",
    components: {
        "tcc-complete": AutoComplete,
        "tcc-chip": Chip,
        "tcc-switch": Toggle,
        "tcc-time": TimePicker,
        "tcc-date": DatePicker,
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
        }
    },
    setup() {
        return {
          
        }
    },
    data() {
        return {
            formState: {} as ContentChannelItem,
            labelCol: { span: 24 },
            wrapperCol: { span: 24 },
            rules: {
              required:(val: any, key: string) => {
                console.log(key, val)
                return !!val || `${key} is required`;
              }
            }
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
        }
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
      }
    },
    mounted() {
        if (this.viewModel) {
          
        }
    },
    template: `
<h3>Basic Information</h3>
<rck-form :model="viewModel.request" ref="basicInfoRef" :label-col="labelCol" :wrapper-col="wrapperCol" :rules="rules">
  <div class="row">
    <div class="col col-xs-12">
      <rck-form-field :rules="required">
        <rck-lbl>Name of {{requestType}} on calendar</rck-lbl>
        <rck-text
          v-model="viewModel.request.title"
        ></rck-text>
      </rck-form-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-form-field name="ministry">
        <rck-field
          v-model="viewModel.request.attributeValues.Ministry"
          :attribute="viewModel.request.attributes.Ministry"
          :is-edit-mode="true"
        ></rck-field>
      </rck-form-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="viewModel.request.attributeValues.Contact"
        :attribute="viewModel.request.attributes.Contact"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
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
    </div>
    <div class="col col-xs-12 col-md-6">
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
    </div>
  </div>
  <br/>
  <template v-if="viewModel.request.attributeValues.NeedsRegistration == 'True' && eventDates.length > 1">
    <h3 class="text-primary">Registration Information</h3>
    <div class="row">
      <div class="col col-xs-12">
        <tcc-switch
          v-model="viewModel.request.attributeValues.EventsNeedSeparateLinks"
          :label="viewModel.request.attributes.EventsNeedSeparateLinks.name"
        ></tcc-switch>
      </div>
    </div>
    <br/>
  </template>
  <template v-if="viewModel.request.attributeValues.NeedsWebCalendar == 'True'">
    <h3 class="text-primary">Web Calendar Information</h3>
    <div class="row">
      <div class="col col-xs-12">
        <rck-form-field>
          <rck-field
            v-model="viewModel.request.attributeValues.WebCalendarDescription"
            :attribute="viewModel.request.attributes.WebCalendarDescription"
            :is-edit-mode="true"
          ></rck-field>
        </rck-form-field>
      </div>
    </div>
    <br/>
  </template>
  <template v-if="viewModel.request.attributeValues.NeedsProductionAccommodations == 'True'">
    <h3 class="text-primary">Production Tech Information</h3>
    <div class="row">
      <div class="col col-xs-12">
        <rck-form-field>
          <rck-field
            v-model="viewModel.request.attributeValues.ProductionTech"
            :attribute="viewModel.request.attributes.ProductionTech"
            :is-edit-mode="true"
          ></rck-field>
        </rck-form-field>
      </div>
    </div>
    <br/>
  </template>
</rck-form>
`
});
