import { defineComponent, provide, reactive, ref } from "vue";
import { useConfigurationValues, useInvokeBlockAction } from "../../../Util/block";
import { Person } from "../../../ViewModels";
import { SubmissionFormBlockViewModel } from "./submissionFormBlockViewModel";
import { useStore } from "../../../Store/index";
import { Steps, Button, Modal } from "ant-design-vue";
import ResourceSwitches from "./Components/resourceSwitches"
import Space from "./Components/space"
import Online from "./Components/online"
import Catering from "./Components/catering"
import Childcare from "./Components/childcare"
import Ops from "./Components/opsAccommodations"
import Registration from "./Components/registration"
import Publicity from "./Components/publicity"
import BasicInfo from "./Components/basicInfo"
import { DateTime } from "luxon";

const store = useStore();
const { Step } = Steps;


export default defineComponent({
  name: "EventForm.SubmissionForm",
  components: {
      "a-steps": Steps,
      "a-step": Step,
      "a-btn": Button,
      "a-modal": Modal,
      "tcc-resources": ResourceSwitches,
      "tcc-basic": BasicInfo,
      "tcc-space": Space,
      "tcc-online": Online,
      "tcc-catering": Catering,
      "tcc-childcare": Childcare,
      "tcc-ops": Ops,
      "tcc-registration": Registration,
      "tcc-publicity": Publicity,
  },
  setup() {
      const viewModel = useConfigurationValues<SubmissionFormBlockViewModel | null>();
      viewModel?.existing.forEach((e: any) => {
        e.childItems = viewModel.existingDetails.filter((d: any) => { return d.contentChannelItemId == e.id })
      })
      const invokeBlockAction = useInvokeBlockAction();
      /** A method to save a submission draft */
      const save: (viewModel: SubmissionFormBlockViewModel) => Promise<any> = async (viewModel) => {
          const response = await invokeBlockAction<{ expirationDateTime: string }>("Save", {
              viewModel: viewModel.request, events: viewModel.events
          });
          if (response.data) {
              return response
          }
      };
      provide("save", save);

      /** A method to submit a submission draft */
      const submit: (viewModel: SubmissionFormBlockViewModel) => Promise<any> = async (viewModel) => {
          const response = await invokeBlockAction<{ expirationDateTime: string }>("Submit", {
              viewModel: viewModel.request, events: viewModel.events
          });
          if (response.data) {
              return response
          }
      };
      provide("submit", submit);

      return {
          viewModel,
          save,
          submit
      }
  },
  data() {
      return {
          step: 0,
          modal: false,
          id: 0,
          isSave: false,
          resources: [] as string[]
      };
  },
  computed: {
      /** The person currently authenticated */
      currentPerson(): Person | null {
          return store.state.currentPerson;
      },
      selectedDates(): any {
        if(this.viewModel?.request.attributeValues) {
          return this.viewModel.request.attributeValues.EventDates.split(',')
        }
      },
      hasEventData(): boolean {
        if(this.viewModel?.request.attributeValues) {
          return this.viewModel.request.attributeValues.NeedsSpace == 'True' || this.viewModel.request.attributeValues.NeedsOnline == 'True' || this.viewModel.request.attributeValues.NeedsCatering == 'True' || this.viewModel.request.attributeValues.NeedsChildCare == 'True' || this.viewModel.request.attributeValues.NeedsOpsAccommodations == 'True' || this.viewModel.request.attributeValues.NeedsRegistration == 'True'
        }
        return false
      },
      publicityStep(): any {
        let step = 2
        if(this.viewModel?.request.attributeValues) {
          if(this.viewModel.request.attributeValues.IsSame == 'True') {
            if(this.hasEventData) {
              step++
            }
          } else {
            if(this.hasEventData) {
              step = (3 + this.viewModel.events.length)
            }
          }
        }
        return step
      },
      lastStep(): any {
        let step = 0
        if(this.viewModel?.isEventAdmin || this.viewModel?.isRoomAdmin || this.viewModel?.isSuperUser) {
          step++
        }
        if(this.viewModel?.request.attributeValues?.IsSame == "True") {
          step++
        } else {
          step += this.viewModel?.events ? this.viewModel.events.length : 0
        }
        if(this.viewModel?.request.attributeValues?.NeedsPublicity == "True") {
          step++
        }
        return step
      },
      canSubmit(): boolean {
        //actually opposite, since we are using this for the disabled prop. 
        let canSubmit = true
        if(this.viewModel?.request.title) {
          if(this.viewModel.request?.attributeValues?.EventDates) {
            if(this.viewModel.request?.attributeValues?.Ministry!= '{"value":"","text":"","description":""}') {
              canSubmit = false
            }
          }
        }
        return canSubmit
      },
      noTitle(): boolean {
        let hasTitle = false
        if(this.viewModel?.request?.title) {
          hasTitle = this.viewModel.request.title.trim() != ''
        }
        return !hasTitle
      }
  },
  methods: {
    matchMultiEvent() {
      if(this.viewModel && this.viewModel?.request.attributeValues && this.viewModel?.events && this.viewModel?.events.length > 0 && this.selectedDates) {
        let numDays = this.selectedDates.length
        //Remove Events not in Selected dates
        this.viewModel.events.forEach((e: any, idx: number) => {
          if(e.attributeValues.EventDate == "") {
            e.attributeValues.EventDate = this.selectedDates[0]
          } 
          if(!this.selectedDates.includes(e.attributeValues.EventDate)) {
            this.viewModel?.events.splice(idx, 1)
          }
        })
        this.selectedDates?.forEach((e: string, idx: number) => {
            let exists = this.viewModel?.events.filter(event => { return event.attributeValues?.EventDate == e })
            if (exists && exists.length > 0) {
              //We don't need to do anything because it already has a matching item
            } else {
              let t = JSON.parse(JSON.stringify(this.viewModel?.events[0]))
              t.attributeValues.EventDate = e
              this.viewModel?.events.push(t)
            }
        })
        this.viewModel.events = this.viewModel.events.sort((a: any, b: any) => {
          if(a.attributeValues && b.attributeValues) {
            let obj = DateTime.fromFormat(a.attributeValues.EventDate, "yyyy-MM-dd").diff(DateTime.fromFormat(b.attributeValues.EventDate, "yyyy-MM-dd"), 'days').toObject()
            if(obj.days) {
              if(obj.days < 0) {
                return -1
              } else if(obj.days > 0) {
                return 1
              }
            }
          }
          return 0
        })
      }
    },
    formatDate(date: string) {
      return DateTime.fromFormat(date, 'yyyy-MM-dd').toFormat("MM/dd/yyyy")
    },
    saveDraft() {
      if(this.viewModel) {
        this.save(this.viewModel).then((res: any) => {
          this.id = res?.data?.id
          this.isSave = true
          this.modal = true
        })
      }
    },
    submitRequest() {
      if(this.viewModel) {
        if(this.viewModel?.request?.attributeValues?.RequestStatus == "Draft") {
          this.viewModel.request.attributeValues.RequestStatus = "Submitted"
        }
        this.submit(this.viewModel).then((res: any) => {
          this.id = res?.data?.id
          this.isSave = false
          this.modal = true
        })
      }
    },
    continueEdit() {
      let url = window.location.href
      if(!url.includes("?Id")) {
        url += "?Id=" + this.id
      }
      window.location.assign(url)
    },
    clearEventDetailsData(category: string) {
      if(this.viewModel?.events) {
        this.viewModel?.events.forEach((event: any) => {
          for(let key in event.attributes) {
            let attr = event.attributes[key]
            if(attr.categories.map((c: any) => { return c.name }).includes(category)) {
              event.attributeValues[key] = ""
            }
          }
        })
      }
    },
    clearEventData(category: string) {
      if(this.viewModel?.request?.attributes && this.viewModel?.request?.attributeValues) {
        for(let key in this.viewModel.request.attributes) {
          let attr = this.viewModel.request.attributes[key]
          if(attr.categories.map((c: any) => { return c.name }).includes(category)) {
            this.viewModel.request.attributeValues[key] = ""
          }
        }
      }
    },
  },
  watch: {
    'viewModel.request.attributeValues.IsSame'(val) {
      if(this.viewModel?.events) {
        if(val == 'True' && this.viewModel.events.length > 1) {
          this.viewModel.events = this.viewModel.events.slice(0, 1)
        } else {
          this.matchMultiEvent()
        }
      }
    },
    'viewModel.request.attributeValues.NeedsSpace'(val) {
      let idx = this.resources.indexOf('Room')
      if(val == 'False') {
        //Remove all Space data
        this.clearEventDetailsData("Event Space")
        //Remove Space from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Space to Requested Resources List
        if(idx < 0) {
          this.resources.push('Room')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsOnline'(val) {
      let idx = this.resources.indexOf('Online Event')
      if(val == 'False') {
        //Remove all Online data
        this.clearEventDetailsData("Event Online")
        //Remove Online from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Online to Requested Resources List
        if(idx < 0) {
          this.resources.push('Online Event')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsCatering'(val) {
      let idx = this.resources.indexOf('Catering')
      if(val == 'False') {
        //Remove all Catering data
        this.clearEventDetailsData("Event Catering")
        //Remove Catering from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Catering to Requested Resources List
        if(idx < 0) {
          this.resources.push('Catering')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsChildCare'(val) {
      let idx = this.resources.indexOf('Childcare')
      if(val == 'False') {
        //Remove all Childcare data
        this.clearEventDetailsData("Event Childcare")
        //Remove Childcare from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Childcare to Requested Resources List
        if(idx < 0) {
          this.resources.push('Childcare')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsOpsAccommodations'(val) {
      let idx = this.resources.indexOf('Extra Resources')
      if(val == 'False') {
        //Remove all Ops Accom data
        this.clearEventDetailsData("Event Ops Requests")
        //Remove Ops from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Ops to Requested Resources List
        if(idx < 0) {
          this.resources.push('Extra Resources')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsRegistration'(val) {
      let idx = this.resources.indexOf('Registration')
      if(val == 'True' && this.viewModel?.events) {
        if(this.viewModel.events[0].attributeValues?.RegistrationConfirmationEmailSender == "" && this.currentPerson?.fullName) {
          this.viewModel.events[0].attributeValues.RegistrationConfirmationEmailSender = this.currentPerson.fullName
        }
      }
      if(val == 'False') {
        //Remove all Registration data
        this.clearEventDetailsData("Event Registration")
        //Remove Registration from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Registration to Requested Resources List
        if(idx < 0) {
          this.resources.push('Registration')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsPublicity'(val) {
      let idx = this.resources.indexOf('Publicity')
      if(val == 'False') {
        //Remove all Publicity data
        this.clearEventData("Event Publicity")
        //Remove Publicity from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Publicity to Requested Resources List
        if(idx < 0) {
          this.resources.push('Publicity')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsProductionAccommodations'(val) {
      let idx = this.resources.indexOf('Production')
      if(val == 'False') {
        //Remove all Production data
        this.clearEventData("Event Production")
        //Remove Production from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Production to Requested Resources List
        if(idx < 0) {
          this.resources.push('Production')
        }
      }
    },
    'viewModel.request.attributeValues.NeedsWebCalendar'(val) {
      let idx = this.resources.indexOf('Web Calendar')
      if(val == 'False' && this.viewModel?.request?.attributeValues) {
        //Remove all Web Calendar data
        this.viewModel.request.attributeValues.WebCalendarDescription = ""
        //Remove Web Calendar from Requested Resources List
        if(idx > -1) {
          this.resources.splice(idx, 1)
        }
      } else {
        //Add Web Calendar to Requested Resources List
        if(idx < 0) {
          this.resources.push('Web Calendar')
        }
      }
    },
    resources: {
      handler(val) {
        if(val && this.viewModel?.request?.attributeValues) {
          this.viewModel.request.attributeValues.RequestType = val.join(',')
        }
      },
      deep: true
    },
    selectedDates: {
      handler(val) {
        if(this.viewModel?.request?.attributeValues?.IsSame == 'False') {
          //Need to fix the events items to match the new dates
          this.matchMultiEvent()
        }
      },
      deep: true
    }
  },
  mounted() {
    if (!this.viewModel?.isSuperUser) {
        this.step = 1
    }
    if(this.viewModel?.request.id == 0) {
      //New Request set some defaults
      if(this.viewModel?.request?.attributeValues) {
        this.viewModel.request.attributeValues.Contact = `${this.currentPerson?.nickName} ${this.currentPerson?.lastName}`
        this.viewModel.request.attributeValues.RequestStatus = "Draft"
      }
    } else {
      if(this.viewModel?.request.attributeValues) {
        this.resources = this.viewModel.request.attributeValues.RequestType.split(",").map((t: string) => t.trim())
      }
    }
  },
  template: `
<div class="card">
  <a-steps :current="step">
    <a-step class="hover" :key="0" @click="step = 0" title="Resources" />
    <a-step class="hover" :key="1" @click="step = 1" title="Basic Info" />
    <template v-if="hasEventData">
      <template v-if="viewModel.request.attributeValues.IsSame == 'True'">
        <a-step class="hover" v-if="hasEventData" :key="2" @click="step = 2" title="Event Info" />
      </template>
      <template v-else>
          <a-step class="hover" v-for="(e, idx) in viewModel.events" :key="(idx + 2)" @click="step = (idx + 2)" :title="formatDate(e.attributeValues.EventDate)" />
      </template>
    </template>
    <a-step class="hover" v-if="viewModel.request.attributeValues.NeedsPublicity == 'True'" :key="publicityStep" @click="step = (3 + viewModel.events.length)" title="Publicity" />
  </a-steps>
  <div class="steps-content">
    <br/>
    <tcc-resources v-if="step == 0" :view-model="viewModel"></tcc-resources>
    <tcc-basic v-if="step == 1" :view-model="viewModel"></tcc-basic>
    <template v-for="(e, idx) in viewModel.events" :key="idx">
      <template v-if="step == (idx + 2)">
        <template v-if="viewModel.request.attributeValues.NeedsSpace == 'True'">
          <h3 class="text-primary">Space Information</h3>
          <tcc-space :e="e" :request="viewModel.request" :locations="viewModel.locations" :existing="viewModel.existing"></tcc-space>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsOnline == 'True'">
          <h3 class="text-primary">Zoom Information</h3>
          <tcc-online :e="e"></tcc-online>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsCatering == 'True'">
          <h3 class="text-primary">Catering Information</h3>
          <tcc-catering :e="e"></tcc-catering>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsChildCare == 'True'">
          <h3 class="text-primary">Childcare Information</h3>
          <tcc-childcare :e="e" :request="viewModel.request"></tcc-childcare>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsOpsAccommodations == 'True'">
          <h3 class="text-primary">Other Accomodations</h3>
          <tcc-ops :e="e"></tcc-ops>
          <br/>
        </template>
        <template v-if="viewModel.request.attributeValues.NeedsRegistration == 'True'">
          <h3 class="text-primary">Registration Information</h3>
          <tcc-registration :e="e"></tcc-registration>
          <br/>
        </template>
      </template>
    </template>
    <template v-if="(viewModel.request.attributeValues.IsSame == 'True' && step == 3) || (step == (3 + viewModel.events.length))">
      <h3 class="text-primary">Publicity Information</h3>
      <tcc-publicity :request="viewModel.request"></tcc-publicity>
    </template>
  </div>
  <div class="row steps-action pt-2">
    <div class="col">
      <a-btn v-if="step == lastStep" class="pull-right" type="primary" @click="submitRequest" :disabled="canSubmit">Submit</a-btn>
      <a-btn v-else class="pull-right" type="primary" @click="step++">Next</a-btn>
      <a-btn v-if="viewModel.request.attributeValues.RequestStatus == 'Draft'" style="margin: 0px 4px;" class="pull-right" type="accent" @click="saveDraft" :disabled="noTitle">Save</a-btn>
    </div>
  </div>
  <!-- Confirmation Modal -->
  <a-modal v-model:visible="modal">
    <div class="pt-2">
      <div v-if="isSave">
        Your draft has been saved.
      </div>
      <div v-else>
        Your request has been submitted.
      </div>
    </div>
    <template #footer>
      <a-btn type="accent" @click="continueEdit">Continue Editing</a-btn>
      <a-btn type="primary">Open Dasboard</a-btn>
    </template>
  </a-modal>
</div>
<v-style>
label, .control-label {
  color: rgba(0,0,0,.6);
  line-height: 18px;
  letter-spacing: normal;
  font-size: 14px;
}
.ant-form-item-label {
  text-align: left;
  line-height: 1em;
}
.ant-form-item-label>label:after {
  content: none;
}
.ant-col {
  padding: 4px 12px !important;
}
.ant-form-item .ant-col {
  padding: 0px !important;
}
.input-hint {
  color: rgba(0,0,0,.6);
  font-size: 12px;
  line-height: 12px;
  word-break: break-word;
  overflow-wrap: break-word;
  word-wrap: break-word;
  -webkit-hyphens: auto;
  -ms-hyphens: auto;
  hyphens: auto;
  padding-top: 8px;
}
.input-hint.rock-hint {
  padding-top: 0px;
  padding-bottom: 8px;
}
.input-label {
  font-weight: bold;
}
.card {
  box-shadow: 0 0 1px 0 rgb(0 0 0 / 8%), 0 1px 3px 0 rgb(0 0 0 / 15%);
  padding: 32px;
  border-radius: 4px;
}
.ant-steps-item-process .ant-steps-item-icon, .ant-steps-item-process .ant-steps-item-icon, .ant-switch-checked, .ant-btn-primary, .ant-steps-item-finish>.ant-steps-item-container>.ant-steps-item-content>.ant-steps-item-title:after, .dp__range_end, .dp__range_start, .dp__active_date {
  background-color: #347689;
  border-color: #347689;
}
.ant-btn-accent {
  background-color: #8ED2C9;
  border-color: #8ED2C9;
}
.ant-steps-item-finish .ant-steps-item-icon, .dp__today {
  border-color: #347689;
}
.ant-steps-item-finish .ant-steps-item-icon>.ant-steps-icon {
  color: #347689;
}
.ant-btn-primary:focus, .ant-btn-primary:hover {
  background-color: rgba(52, 118, 137, .85);
  border-color: rgba(52, 118, 137, .85);
}
.ant-btn-accent:focus, .ant-btn-accent:hover {
  background-color: hsl(172deg 30% 55%);
  border-color: hsl(172deg 30% 55%);
  color: black;
}
.hover {
  cursor: pointer;
}
.text-primary {
  color: #347689;
}
.text-accent {
  color: #8ED2C9;
}
.tcc-dropdown {
  display: flex;
  flex-direction: column;
  padding: 0px 4px;
  max-height: 300px;
  overflow-y: scroll;
}
.row-equal-height {
  display: flex;
  flex-wrap: wrap;
}
.row-equal-height > .col {
  display: flex;
  flex-direction: column;
  align-items: start;
  justify-content: center;
}
</v-style>
`
})
