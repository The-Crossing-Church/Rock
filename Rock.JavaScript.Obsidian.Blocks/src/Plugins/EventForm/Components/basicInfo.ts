import { defineComponent, PropType } from "vue"
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag"
import { SubmissionFormBlockViewModel } from "../submissionFormBlockViewModel"
import { useStore } from "@Obsidian/PageState"
import { Switch } from "ant-design-vue"
import { DateTime } from "luxon"
import RockForm from "@Obsidian/Controls/rockForm"
import RockField from "@Obsidian/Controls/rockField"
import RockFormField from "@Obsidian/Controls/rockFormField"
import TextBox from "@Obsidian/Controls/textBox"
import RockLabel from "@Obsidian/Controls/rockLabel"
import Validator from "./validator"
import DatePicker from "./calendar"
import AutoComplete from "./roomPicker"
import Chip from "./chip"
import Toggle from "./toggle"
import TimePicker from "./timePicker"
import EventBuffer from "./eventBuffer"
import AddComment from "./addComment"
import rules from "../Rules/rules"

const store = useStore();

export default defineComponent({
    name: "EventForm.Components.BasicInfo",
    components: {
        "tcc-complete": AutoComplete,
        "tcc-chip": Chip,
        "tcc-switch": Toggle,
        "tcc-time": TimePicker,
        "tcc-date": DatePicker,
        "tcc-buffer": EventBuffer,
        "tcc-validator": Validator,
        "tcc-comment": AddComment,
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
        minEventDate: String,
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
        /** The person currently authenticated */
        currentPerson(): PersonBag | null {
            return store.state.currentPerson;
        },
        eventDates() {
          if (this.viewModel?.request?.attributeValues?.EventDates) {
            return this.viewModel.request.attributeValues.EventDates.split(',').map((d: any) => d.trim())
          }
          return []
        },
        ministries(): Array<any> | null {
            let list = [] as any[]
            if (this.viewModel) {
                list = this.viewModel.ministries.map(m => {
                    return { value: `${m.idKey}`, text: m.value }
                })
            }
            return list
        },
        requestType() {
          if(this.viewModel?.request?.attributeValues?.NeedsSpace == "True"
            && (this.viewModel?.request?.attributeValues?.NeedsOnline == "False"
              && this.viewModel?.request?.attributeValues?.NeedsCatering == "False"
              && this.viewModel?.request?.attributeValues?.NeedsChildCare == "False"
              && this.viewModel?.request?.attributeValues?.NeedsChildCareCatering == "False"
              && this.viewModel?.request?.attributeValues?.NeedsOpsAccommodations == "False"
              && this.viewModel?.request?.attributeValues?.NeedsRegistration == "False"
              && this.viewModel?.request?.attributeValues?.NeedsPublicity == "False"
              && this.viewModel?.request?.attributeValues?.NeedsProductionAccommodations == "False"
              && this.viewModel?.request?.attributeValues?.NeedsWebCalendar == "False"
            )
          ) {
            return "meeting"
          }
          return "event"
        },
        hasEventSpecificRequests() {
          if(this.viewModel?.request?.attributeValues?.NeedsSpace == "True"
            || this.viewModel?.request?.attributeValues?.NeedsOnline == "True"
            || this.viewModel?.request?.attributeValues?.NeedsCatering == "True"
            || this.viewModel?.request?.attributeValues?.NeedsChildCare == "True"
            || this.viewModel?.request?.attributeValues?.NeedsChildCareCatering == "True"
            || this.viewModel?.request?.attributeValues?.NeedsOpsAccommodations == "True"
            || this.viewModel?.request?.attributeValues?.NeedsRegistration == "True") {
              return true
          }
          return false
        },
        labelIsSame() {
          return `Will each occurrence of your ${this.requestType} have the exact same start and end time? (${this.viewModel?.request?.attributeValues?.IsSame == 'True' ? 'Yes' : 'No'})`
        },
    },
    methods: {
      removeDate(date: string) {
        if(!(this.viewModel?.request.attributeValues?.IsSame == 'False' && !(this.viewModel?.request.attributeValues.RequestStatus == 'Draft' || this.viewModel?.request.attributeValues.RequestStatus == 'Submitted' || this.viewModel?.request.attributeValues.RequestStatus == 'In Progress'))) {
          if (this.viewModel?.request?.attributeValues?.EventDates) {
            let dates = this.viewModel.request.attributeValues.EventDates.split(',')
            let idx = dates.indexOf(date)
            dates.splice(idx, 1)
            this.viewModel.request.attributeValues.EventDates = dates.join(",")
          }
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
      },
      addComment(comment: string) {
        this.$emit("createComment", comment)
      }
    },
    watch: {
      eventDates: {
        handler(val) {
          if(val.length < 2 && this.viewModel?.request.attributeValues) {
            //When there is only one event date these values should reset to their defaults
            this.viewModel.request.attributeValues.IsSame = "True"
            this.viewModel.request.attributeValues.EventsNeedSeparateLinks = "False"
            this.viewModel.events?.forEach(e => {
              if(e.attributeValues) {
                e.attributeValues.EventDate = ''
              }
            })
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
      if(!this.viewModel?.request.idKey) {
        let params = new URLSearchParams(window.location.search)
        let prefill = params.get('PreFill')
        if(prefill && this.viewModel?.request?.attributeValues) {
          let dt = DateTime.fromFormat(prefill, "yyyy-MM-dd")
          if(dt >= DateTime.now().startOf('day')) {
            this.viewModel.request.attributeValues.EventDates = prefill
          }
        }
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
          id="txtTitle"
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
          id="ddlMinistry"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :name="contact" :rules="[rules.required(viewModel.request.attributeValues.Contact, 'Contact')]" ref="validators_contact">
        <rck-field
          v-model="viewModel.request.attributeValues.Contact"
          :attribute="viewModel.request.attributes.Contact"
          :is-edit-mode="true"
          id="txtContact"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
  <br/>
  <div class="row">
    <div class="col col-xs-12 col-md-4">
      <tcc-date v-model="viewModel.request.attributeValues.EventDates" :min="minEventDate" :multiple="true" :readonly="viewModel.request.attributeValues.IsSame == 'False' && !(viewModel.request.attributeValues.RequestStatus == 'Draft' || viewModel.request.attributeValues.RequestStatus == 'Submitted' || viewModel.request.attributeValues.RequestStatus == 'In Progress')"></tcc-date>
    </div>
    <div class="col col-xs-12 col-md-8" style="display: flex; flex-wrap: wrap; align-content: flex-start;">
      <tcc-chip v-if="showValidation && eventDates.length == 0" class="bg-red text-red">Event Date(s) are required.</tcc-chip>
      <tcc-chip v-for="d in eventDates" :key="d" v-on:chipdeleted="removeDate(d)" :disabled="viewModel.request.attributeValues.IsSame == 'False' && !(viewModel.request.attributeValues.RequestStatus == 'Draft' || viewModel.request.attributeValues.RequestStatus == 'Submitted' || viewModel.request.attributeValues.RequestStatus == 'In Progress')">
        {{formatDate(d)}}
      </tcc-chip>
      <tcc-comment v-if="viewModel.request.attributeValues.IsSame == 'False' && !(viewModel.request.attributeValues.RequestStatus == 'Draft' || viewModel.request.attributeValues.RequestStatus == 'Submitted' || viewModel.request.attributeValues.RequestStatus == 'In Progress')" :request="viewModel.request" v-on:addComment="addComment"></tcc-comment>
    </div>
  </div>
  <br/><br/>
  <div class="row" v-if="eventDates.length > 1 && hasEventSpecificRequests">
    <div class="col col-xs-12">
      <tcc-switch
        v-model="viewModel.request.attributeValues.IsSame"
        :label="viewModel.request.attributes.IsSame.name"
        :disabled="!(viewModel.request.attributeValues.RequestStatus == 'Draft' || viewModel.request.attributeValues.RequestStatus == 'Submitted' || viewModel.request.attributeValues.RequestStatus == 'In Progress')"
        id="switchSame"
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
            {"mine": "08:20:00", "theirs": "09:25:00", "title": "1st Service"},
            {"mine": "09:35:00", "theirs": "10:40:00", "title": "2nd Service"},
            {"mine": "10:50:00", "theirs": "11:55:00", "title": "3rd Service"}
          ]'
          id="TimeStart"
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
            {"theirs": "08:20:00", "mine": "09:25:00", "title": "1st Service"},
            {"theirs": "09:35:00", "mine": "10:40:00", "title": "2nd Service"},
            {"theirs": "10:50:00", "mine": "11:55:00", "title": "3rd Service"}
          ]'
          id="TimeEnd"
        ></tcc-time>
      </tcc-validator>
    </div>
  </div>
  <br/>
  <tcc-buffer v-if="viewModel.request.attributeValues.IsSame == 'True' && (viewModel.isEventAdmin || viewModel.isSuperUser)" :e="viewModel.events[0]"></tcc-buffer>
  <br/>
</rck-form>
`
});
