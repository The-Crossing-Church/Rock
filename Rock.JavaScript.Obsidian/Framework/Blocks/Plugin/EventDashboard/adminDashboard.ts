import { defineComponent, provide, PropType } from "vue";
import { useConfigurationValues, useInvokeBlockAction } from "../../../Util/block";
import { Person, ContentChannelItem, PublicAttribute } from "../../../ViewModels";
import { AdminDashboardBlockViewModel } from "./adminDashboardBlockViewModel";
import { useStore } from "../../../Store/index";
import { DateTime } from "luxon"
import { Table, Modal, Button, Popover } from "ant-design-vue"
import GridAction from "./Components/adminGridAction"
import TCCModal from "./Components/dashboardModal"
import Details from "./Components/dashboardModal"
import TCCDropDownList from "./Components/dropDownList"
import RockText from "../../../Elements/textBox"
import RockField from "../../../Controls/rockField"
import DateRangePicker from "../../../Elements/dateRangePicker"

const store = useStore();


export default defineComponent({
  name: "EventDashboard.AdminDashboard",
  components: {
    "a-table": Table,
    "a-modal": Modal,
    "a-btn": Button,
    "a-pop": Popover,
    "tcc-grid": GridAction,
    "tcc-model": TCCModal,
    "tcc-details": Details,
    "tcc-ddl": TCCDropDownList,
    "rck-text": RockText,
    "rck-field": RockField,
    "rck-date-range": DateRangePicker
  },
  setup() {
      const invokeBlockAction = useInvokeBlockAction();
      const viewModel = useConfigurationValues<AdminDashboardBlockViewModel | null>();
      viewModel?.events.forEach((e: any) => {
        e.childItems = viewModel.eventDetails.filter((d: any) => { return d.contentChannelItemId == e.id })
      })

      /** A method to load a specific request's details */
      const loadDetails: (id: number) => Promise<any> = async (id) => {
          const response = await invokeBlockAction("GetRequestDetails", {
              id: id
          });
          return response
      };
      provide("loadDetails", loadDetails);

      const filterRequests: (filters: any) => Promise<any> = async (filters) => {
        const response = await invokeBlockAction("FilterRequests", {
            filters: filters
        });
        return response
      }
      provide("filterRequests", filterRequests);

      return {
          viewModel,
          loadDetails,
          filterRequests
      }
  },
  data() {
      return {
        columns: [
          {
            title: 'Title',
            dataIndex: 'title',
            key: 'title',
            slots: { customRender: 'title' },
          },
          {
            title: 'Submitted On',
            dataIndex: 'startDateTime',
            key: 'start',
            slots: { customRender: 'start' },
          },
          {
            title: 'Event Dates',
            dataIndex: 'attributeValues.EventDates',
            key: 'dates',
            slots: { customRender: 'dates' }
          },
          {
            title: 'Status',
            key: 'action',
            slots: { customRender: 'action' },
          },
        ],
        selected: {} as ContentChannelItem,
        createdBy: {},
        modifiedBy: {},
        modal: false,
        visible: false,
        requestStatuses: [
          "Draft",
          "Submitted",
          "In Progress",
          "Approved",
          "Denied",
          "Cancelled",
          "Pending Changes",
          "Proposed Changes Denied",
          "Changes Accepted by User",
          "Cancelled by User"
        ],
        resources: [
          "Room", 
          "Online Event",
          "Catering",
          "Childcare",
          "Extra Resources",
          "Registration",
          "Web Calendar",
          "Production",
          "Publicity"
        ],
        filters: {
          title: "",
          statuses: ["Submitted", "In Progress", "Pending Changes", "Proposed Changes Denied", "Changes Accepted by User"],
          resources: [] as string[],
          ministry: ""
        },
        loading: false
      };
  },
  computed: {
      /** The person currently authenticated */
      currentPerson(): Person | null {
          return store.state.currentPerson
      },
      selectedStatus(): string {
        if(this.selected?.attributeValues) {
          return this.selected.attributeValues?.RequestStatus
        }
        return ""
      },
      ministryAttr(): PublicAttribute | undefined {
        if(this.viewModel?.events[0]) {
          return this.viewModel.events[0].attributes?.Ministry
        }
        return undefined
      }
  },
  methods: {
    formatDateTime(date: any): string {
      if(date) {
        return DateTime.fromISO(date).toFormat("MM/dd/yyyy hh:mm a");
      }
      return ""
    },
    formatDates(dates: string): string {
      if(dates) {
        let dateArray = dates.split(",").map((d: string) => DateTime.fromFormat(d.trim(), "yyyy-MM-dd").toFormat("MM/dd/yyyy"))
        return dateArray.join(", ")
      }
      return ""
    },
    selectItem(item: any) { 
      this.loadDetails(item.id).then((response: any) => {
        if(response.data) {
          response.data.request.childItems = response.data.details
          this.selected = response.data.request
          this.createdBy = response.data.createdBy
          this.modifiedBy = response.data.modifiedBy
          this.modal = true
        }
      })
    },
    editItem (id: string) {
      window.location.href = "/eventform?Id=" + id
    },
    filter() {
      this.loading = true
      this.filterRequests(this.filters).then((response: any) => {
        if(this.viewModel) {
          this.viewModel.events = response.data.events
          this.viewModel?.events.forEach((e: any) => {
            e.childItems = this.viewModel?.eventDetails.filter((d: any) => { return d.contentChannelItemId == e.id })
          })
        }
        this.loading = false
        console.log('Success!')
      })
    }
  },
  watch: {
    
  },
  mounted() {
    
  },
  template: `
<div class="card">
  <h4 class="hover" data-toggle="collapse" data-target="#filterCollapse" aria-expanded="false" aria-controls="filterCollapse">
    <i class="fa fa-filter mr-2"></i>
    Filters
  </h4>
  <div class="collapse" id="filterCollapse">
    <div class="row">
      <div class="col col-xs-12 col-md-3">
        <rck-text
          label="Request Name"
          v-model="filters.title"
        ></rck-text>
      </div>
      <div class="col col-xs-12 col-md-3" v-if="ministryAttr">
        <rck-field
          v-model="filters.ministry"
          :attribute="ministryAttr"
          :is-edit-mode="true"
        ></rck-field>
      </div>
      <div class="col col-xs-12 col-md-3">
        <tcc-ddl
          label="Request Status"
          :items="requestStatuses"
          v-model="filters.statuses"
        ></tcc-ddl>
      </div>
      <div class="col col-xs-12 col-md-3">
        <tcc-ddl
          label="Requested Resources"
          :items="resources"
          v-model="filters.resources"
        ></tcc-ddl>
      </div>
    </div>
    <div class="row">
      <div class="col col-xs-12">
        <a-btn class="pull-right" type="primary" @click="filter" :loading="loading">Filter</a-btn>
      </div>
    </div>
  </div>
  <a-table :columns="columns" :data-source="viewModel.events">
    <template #title="{ text: title, record: r }">
      <div class="hover" @click="selectItem(r)">{{ title }}</div>
    </template>
    <template #start="{ text: start }">
      {{ formatDateTime(start) }}
    </template>
    <template #dates="{ text: dates }">
      {{ formatDates(dates) }}
    </template>
    <template #action="{ record: r }">
      <tcc-grid :request="r"></tcc-grid>
    </template>
  </a-table>
  <a-modal v-model:visible="modal" width="80%" :closable="false">
    <tcc-details :request="selected" :rooms="viewModel.locations" :createdBy="createdBy" :modifiedBy="modifiedBy"></tcc-details>
    <template #footer>
      <div class="text-left">
        <a-btn type="primary" @click="editItem(selected.id)">
          <i class="mr-1 fa fa-pencil-alt"></i>
          Edit
        </a-btn>
        <a-btn type="accent" v-if="selectedStatus != 'Approved'">
          <i class="mr-1 fa fa-check"></i>
          Approve
        </a-btn>
        <a-pop v-model:visible="visible" trigger="click" placement="top" v-if="selectedStatus != 'Denied'">
          <template #content>
            <div style="display: flex; flex-direction: column;">
              <a-btn class="mb-1" type="red" v-if="selectedStatus == 'Pending Changes'">
                <i class="mr-1 fa fa-times"></i>
                Changes w/ Comment
              </a-btn>
              <a-btn class="mb-1" type="red" v-if="selectedStatus == 'Pending Changes'">
                <i class="mr-1 fa fa-times"></i>
                Changes w/o Comment
              </a-btn>
              <a-btn type="red">
                <i class="mr-1 fa fa-times"></i>
                Request
              </a-btn>
            </div>
          </template>
          <a-btn type="red">
            <i class="mr-1 fa fa-times"></i>
            Deny
          </a-btn>
        </a-pop>
        <a-btn type="grey" v-if="selectedStatus != 'Cancelled'">
          <i class="mr-1 fa fa-ban"></i>
          Cancel
        </a-btn>
        <a-btn type="accent">
          <i class="mr-1 fa fa-comment-alt"></i>
          Add Comment
        </a-btn>
      </div>
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
.ant-switch-checked, .ant-btn-primary, .ant-btn-submitted {
  background-color: #347689;
  border-color: #347689;
}
.ant-btn-accent, .ant-btn-approved{
  background-color: #8ED2C9;
  border-color: #8ED2C9;
}
.ant-btn-yellow, .ant-btn-inprogress {
  background-color: #ecc30b;
  border-color: #ecc30b;
}
.ant-btn-pendingchanges {
  background-color: #61a4a9;
  border-color: #61a4a9;
}
.ant-btn-cancelled, .ant-btn-grey {
  background-color: #9e9e9e;
  border-color: #9e9e9e;
}
.ant-btn-denied, .ant-btn-red {
  background-color: #f44336;
  border-color: #f44336;
}
.ant-btn-primary:focus, .ant-btn-primary:hover, .ant-btn-submitted:focus, .ant-btn-submitted:hover {
  background-color: rgba(52, 118, 137, .85);
  border-color: rgba(52, 118, 137, .85);
  color: white;
}
.ant-btn-accent:focus, .ant-btn-accent:hover, .ant-btn-approved:focus, .ant-btn-approved:hover {
  background-color: hsl(172deg 30% 55%);
  border-color: hsl(172deg 30% 55%);
  color: black;
}
.ant-btn-yellow:focus, .ant-btn-yellow:hover, .ant-btn-inprogress:focus, .ant-btn-inprogress:hover {
  background-color: #DDB70D;
  border-color: #DDB70D;
  color: black;
}
.ant-btn-pendingchanges:focus, .ant-btn-pendingchanges:hover {
  background-color: #5B999E;
  border-color: #5B999E;
  color: black;
}
.ant-btn-grey:focus, .ant-btn-grey:hover, .ant-btn-cancelled:focus, .ant-btn-cancelled:hover {
  background-color: #929392;
  border-color: #929392;
  color: black;
}
.ant-btn-red:focus, .ant-btn-red:hover, .ant-btn-denied:focus, .ant-btn-denied:hover {
  background-color: #E43F32;
  border-color: #E43F32;
  color: black;
}
/* Make Grid Action Buttons Same Width */
td .ant-btn {
  width: 100%;
  max-width: 200px;
}
.hover {
  cursor: pointer;
}
.text-primary, .text-submitted {
  color: #347689;
}
.text-accent, .text-approved {
  color: #8ED2C9;
}
.border-approved {
  border-color: #8ED2C9;
}
.text-inprogress {
  color: #ecc30b;
}
.border-inprogress {
  boarder-color: #ecc30b;
}
.text-pendingchanges {
  color: #61a4a9;
}
.border-pendingchanges {
  border-color: #61a4a9;
}
.text-cancelled, .text-cancelledbyuser {
  color: #3d3d3d;
}
.border-cancelled, .border-cancelledbyuser {
  border-color: #3d3d3d;
}
.text-denied {
  color: #cc3f0c;
}
.border-denied {
  border-color: #cc3f0c;
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
