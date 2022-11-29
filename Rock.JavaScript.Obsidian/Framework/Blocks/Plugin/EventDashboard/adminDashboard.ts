import { defineComponent, provide } from "vue"
import { useConfigurationValues, useInvokeBlockAction } from "../../../Util/block"
import { Person, ContentChannelItem, PublicAttribute } from "../../../ViewModels"
import { AdminDashboardBlockViewModel } from "./adminDashboardBlockViewModel"
import { useStore } from "../../../Store/index"
import { DateTime, Duration } from "luxon"
import { Table, Modal, Button, Popover } from "ant-design-vue"
import GridAction from "./Components/adminGridAction"
import TCCModal from "./Components/dashboardModal"
import Details from "./Components/dashboardModal"
import TCCDropDownList from "./Components/dropDownList"
import RockText from "../../../Elements/textBox"
import RockLabel from "../../../Elements/rockLabel"
import RockField from "../../../Controls/rockField"
import DateRangePicker from "../../../Elements/dateRangePicker"
import PersonPicker from "../../../Controls/personPicker"
import Comment from "./Components/comment"
import TCCTable from "./Components/requestTable"

const store = useStore()


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
    "tcc-table": TCCTable,
    "tcc-comment": Comment,
    "rck-text": RockText,
    "rck-lbl": RockLabel,
    "rck-field": RockField,
    "rck-date-range": DateRangePicker,
    "rck-person": PersonPicker,
  },
  setup() {
      const invokeBlockAction = useInvokeBlockAction();
      const viewModel = useConfigurationValues<AdminDashboardBlockViewModel | null>();

      let defaultFilters = {
        title: "",
        statuses: viewModel?.defaultStatuses,
        resources: [] as string[],
        ministry: "",
        submitter: { value: "", text: ""},
        eventDates:  { lowerValue: "", upperValue: "" },
        eventModified: { lowerValue: "", upperValue: "" }
      }
      defaultFilters.eventModified.upperValue = DateTime.now().toFormat("yyyy-MM-dd")
      let twoWeeks = Duration.fromObject({weeks: 2})
      defaultFilters.eventModified.lowerValue = DateTime.now().minus(twoWeeks).toFormat("yyyy-MM-dd")

      /** A method to load a specific request's details */
      const loadDetails: (id: number) => Promise<any> = async (id) => {
          const response = await invokeBlockAction("GetRequestDetails", {
              id: id
          });
          return response
      };
      provide("loadDetails", loadDetails);

      const filterRequests: (option: string, filters: any) => Promise<any> = async (option, filters) => {
        const response = await invokeBlockAction("FilterRequests", {
            opt: option, filters: filters
        });
        return response
      }
      provide("filterRequests", filterRequests);

      const changeStatus: (id: number, status: string) => Promise<any> = async (id, status) => {
        const response = await invokeBlockAction("ChangeStatus", {
            id: id, status: status
        });
        return response
      }
      provide("changeStatus", changeStatus);

      const addComment: (id: number, message: string) => Promise<any> = async (id, message) => {
        const response = await invokeBlockAction("AddComment", {
          id: id, message: message
        });
        return response
      }
      provide("addComment", addComment);

      return {
          viewModel,
          defaultFilters,
          loadDetails,
          filterRequests,
          changeStatus,
          addComment
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
        commentModal: false,
        comment: "",
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
        loading: false,
        btnLoading: {
          inprogress: false,
          cancelled: false
        },
        toastMessage: "",
        filters: {} as any
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
          response.data.details.forEach((detail: any) => {
            detail.detail.changes = detail.detailPendingChanges
          })
          response.data.request.childItems = response.data.details.map((detail: any) => { return detail.detail })
          response.data.request.changes = response.data.requestPendingChanges
          response.data.request.comments = response.data.comments
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
    filter(option: string) {
      this.loading = true
      let reqFilter = JSON.parse(JSON.stringify(this.filters))
      if(reqFilter.ministry) {
        let ministry = JSON.parse(reqFilter.ministry)
        reqFilter.ministry = ministry.value
      }
      this.filterRequests(option, reqFilter).then((response: any) => {
        if(this.viewModel) {
          this.viewModel.events = response.data.events
        }
      }).catch((err) => {
        console.log(err)
      }).finally(() => {
        this.loading = false
      })
    },
    filterTables(option: string, filters: any) {
      let reqFilter = JSON.parse(JSON.stringify(filters))
      if(reqFilter.ministry) {
        let ministry = JSON.parse(reqFilter.ministry)
        reqFilter.ministry = ministry.value
      }
      this.filterRequests(option, reqFilter).then((response: any) => {
        console.log(response)
        if(this.viewModel) {
          if(option == 'Submitted') {
            this.viewModel.submittedEvents = response.data.submittedEvents
          } else if (option == 'PendingChanges') {
            this.viewModel.changedEvents = response.data.changedEvents
          } else if (option == 'InProgress') {
            this.viewModel.inprogressEvents = response.data.inprogressEvents
          } else {
            this.viewModel.events = response.data.events
          }
        }
      })
    },
    resetFilters() {
      this.filters = {
        title: "",
        statuses: this.viewModel?.defaultStatuses.map((s: string) => { return s.trim() }),
        resources: [] as string[],
        ministry: "",
        submitter: { value: "", text: ""},
        eventDates:  { lowerValue: "", upperValue: "" },
        eventModified: { lowerValue: "", upperValue: "" }
      }
      this.filters.eventModified.upperValue = DateTime.now().toFormat("yyyy-MM-dd")
      this.filters.eventModified.lowerValue = DateTime.now().minus({weeks: 2}).toFormat("yyyy-MM-dd")
    },
    clearFilters() {
      this.filters = {
        title: "",
        statuses: [] as string[],
        resources: [] as string[],
        ministry: "",
        submitter: { value: "", text: ""},
        eventDates:  { lowerValue: "", upperValue: "" },
        eventModified: { lowerValue: "", upperValue: "" }
      }
    },
    updateFromGridAction(id: number, status: string) {
      this.selected.id = id
      this.updateStatus(status)
    },
    updateStatus(status: string) {
      if(status == "In Progress") {
        this.btnLoading.inprogress = true
      } else {
        this.btnLoading.cancelled = true
      }
      this.changeStatus(this.selected.id, status).then((res) => {
        if(res.data.url) {
          window.location.href = res.data.url
        }
        if(res.data.status) {
          if(this.selected.attributeValues) {
            this.selected.attributeValues.RequestStatus = res.data.status
          }
          this.viewModel?.events.forEach((event: any) => {
            if(event.id == this.selected.id) {
              event.attributeValues.RequestStatus = res.data.status
            }
          })
        }
      }).catch((err) => {
        console.log('[Change Status Error]')
        console.log(err)
        //TODO: Alert User
      }).finally(() => {
        this.btnLoading.inprogress = false
        this.btnLoading.cancelled = false
      })
    },
    requestAction(status: string) {
      window.location.href = this.viewModel?.workflowURL + `?Id=${this.selected?.id}&Action=${status}`
    },
    getNextComment(idx: number) {
      let req = this.selected as any
      if(idx < (req.comments.length - 1)) {
        return req.comments[idx + 1]
      }
      return null
    },
    newComment() {
      this.comment = ""
      this.commentModal = true
    },
    createComment() {
      this.addComment(this.selected.id, this.comment).then((res: any) => {
        if(res.isSuccess) {
          if(res.data?.comment) {
            let req = this.selected as any
            req.comments.push(res.data)
          }
        } else if (res.isError) {
          this.toastMessage = res.errorMessage
          let el = document.getElementById('toast')
          el?.classList.add("show")
        }
        this.commentModal = false
        this.comment = ""
      })
    },
    hideToast() {
      let el = document.getElementById('toast')
      el?.classList.remove("show")
    }
  },
  watch: {
    
  },
  mounted() {
    this.filters = this.defaultFilters
  },
  template: `
<div class="card mb-2">
  <tcc-table :openByDefault="true" option="Submitted" :events="viewModel.submittedEvents" v-on:updatestatus="updateFromGridAction" v-on:selectitem="selectItem" v-on:filter="filterTables"></tcc-table>
</div>
<div class="card mb-2">
  <tcc-table :openByDefault="false" option="Pending Changes" :events="viewModel.changedEvents" v-on:updatestatus="updateFromGridAction" v-on:selectitem="selectItem" v-on:filter="filterTables"></tcc-table>
</div>
<div class="card mb-2">
  <tcc-table :openByDefault="false" option="In Progress" :events="viewModel.inprogressEvents" v-on:updatestatus="updateFromGridAction" v-on:selectitem="selectItem" v-on:filter="filterTables"></tcc-table>
</div>
<div class="card">
  <div style="display: flex; align-items: center;">
    <i class="fa fa-filter mr-2 mb-2 hover fa-lg" data-toggle="collapse" data-target="#filterCollapse" aria-expanded="false" aria-controls="filterCollapse"></i>
    <h2 style="width: 90%;" class="text-primary hover" data-toggle="collapse" data-target="#allCollapse" aria-expanded="false" aria-controls="allCollapse">Everything Else</h2>
  </div>
  <div class="collapse" id="allCollapse">
    <div class="collapse" id="filterCollapse">
      <div class="row">
        <div class="col col-xs-12 col-md-4">
          <rck-text
            label="Request Name"
            v-model="filters.title"
          ></rck-text>
        </div>
        <div class="col col-xs-12 col-md-4">
          <rck-person
            label="Submitter/Modifier"
            v-model="filters.submitter"
          ></rck-person>
        </div>
        <div class="col col-xs-12 col-md-4" v-if="ministryAttr">
          <rck-field
            v-model="filters.ministry"
            :attribute="ministryAttr"
            :is-edit-mode="true"
          ></rck-field>
        </div>
      </div>
      <div class="row">
        <div class="col col-xs-12 col-md-4">
          <tcc-ddl
            label="Request Status"
            :items="requestStatuses"
            v-model="filters.statuses"
          ></tcc-ddl>
        </div>
        <div class="col col-xs-12 col-md-4">
          <tcc-ddl
            label="Requested Resources"
            :items="resources"
            v-model="filters.resources"
          ></tcc-ddl>
        </div>
        <div class="col col-xs-12 col-md-4">
          <rck-date-range
            label="Has Event Date in Range"
            v-model="filters.eventDates"
          ></rck-date-range>
        </div>
      </div>
      <div class="row">
        <div class="col col-xs-12 font-weight-light hr">
          OR
        </div>
      </div>
      <div class="row">
        <div class="col col-xs-12 col-md-4">
          <rck-date-range
            label="Modified in Range"
            v-model="filters.eventModified"
          ></rck-date-range>
        </div>
      </div>
      <div class="row">
        <div class="col col-xs-12">
          <a-btn class="mr-1" type="accent" @click="resetFilters">Reset Defaults</a-btn>
          <a-btn type="grey" @click="clearFilters">Clear Filters</a-btn>
          <a-btn class="pull-right" type="primary" @click="filter('All')" :loading="loading">Filter</a-btn>
        </div>
      </div>
    </div>
    <a-table :columns="columns" :data-source="viewModel.events" :pagination="{ pageSize: 30 }">
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
        <tcc-grid :request="r" :url="viewModel.workflowURL" v-on:updatestatus="updateFromGridAction"></tcc-grid>
      </template>
    </a-table>
  </div>
</div>
<a-modal v-model:visible="modal" width="80%" :closable="false">
  <tcc-details :request="selected" :rooms="viewModel.locations" :drinks="viewModel.drinks" :createdBy="createdBy" :modifiedBy="modifiedBy"></tcc-details>
  <template v-if="selected.comments && selected.comments.length > 0">
    <h3 class="text-accent">Comments</h3>
    <div>
      <tcc-comment v-for="(c, idx) in selected.comments" :comment="c.comment" :createdBy="c.createdBy" :next="getNextComment(idx)" :key="c.comment.id"></tcc-comment>
    </div>
  </template>
  <template #footer>
    <div class="text-left">
      <a-btn type="primary" @click="editItem(selected.id)">
        <i class="mr-1 fa fa-pencil-alt"></i>
        Edit
      </a-btn>
      <a-btn type="accent" v-if="selectedStatus != 'Approved'" @click="updateStatus('Approved')">
        <i class="mr-1 fa fa-check"></i>
        Approve
      </a-btn>
      <a-btn type="yellow" v-if="selectedStatus != 'In Progress'" @click="updateStatus('In Progress')">
        <i class="mr-1 fas fa-tasks"></i>
        In Progress
      </a-btn>
      <a-pop v-model:visible="visible" trigger="click" placement="top" v-if="selectedStatus != 'Denied'">
        <template #content>
          <div style="display: flex; flex-direction: column;">
            <a-btn class="mb-1" type="red" v-if="selectedStatus == 'Pending Changes'" @click="requestAction('Proposed Changes Denied')">
              <i class="mr-1 fa fa-times"></i>
              Changes w/ Comment
            </a-btn>
            <a-btn class="mb-1" type="red" v-if="selectedStatus == 'Pending Changes'" @click="updateStatus('Proposed Changes Denied')">
              <i class="mr-1 fa fa-times"></i>
              Changes w/o Comment
            </a-btn>
            <a-btn type="red" @click="requestAction('Denied')">
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
      <a-btn type="grey" v-if="selectedStatus != 'Cancelled' && selectedStatus != 'Cancelled by User'" @click="updateStatus('Cancelled')">
        <i class="mr-1 fa fa-ban"></i>
        Cancel
      </a-btn>
      <a-btn type="accent" @click="newComment">
        <i class="mr-1 fa fa-comment-alt"></i>
        Add Comment
      </a-btn>
    </div>
  </template>
</a-modal>
<a-modal v-model:visible="commentModal" width="80%" :closable="false" style="z-index: 2000 !important;">
  <rck-lbl>New Comment</rck-lbl>
  <rck-text
    v-model="comment"
    textMode="multiline"
  ></rck-text>
  <template #footer>
    <a-btn type="accent" @click="createComment">
      <i class="mr-1 fa fa-comment-alt"></i>
      Add Comment
    </a-btn>
    <a-btn type="grey" @click="commentModal = false;">
      <i class="mr-1 fa fa-ban"></i>
      Cancel
    </a-btn>
  </template>
</a-modal>
<div id="toast" role="alert">
  <div class="toast-header text-red">
    <i class="fas fa-exclamation-triangle mr-1"></i>
    <strong>System Exception</strong>
    <i class="fa fa-times pull-right hover" @click="hideToast"></i>
  </div>
  <div class="toast-body">
    {{toastMessage}}
  </div>
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
  padding: 16px;
  border-radius: 4px;
}
.ant-switch-checked, .ant-btn-primary, .ant-btn-submitted {
  background-color: #347689;
  border-color: #347689;
  color: #fff;
}
.ant-btn-draft {
  background-color: #A18276;
  border-color: #A18276;
}
.ant-btn-accent, .ant-btn-approved {
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
  color: #fff;
}
.ant-btn-cancelled, .ant-btn-cancelledbyuser, .ant-btn-grey {
  background-color: #9e9e9e;
  border-color: #9e9e9e;
}
.ant-btn-denied, .ant-btn-red, .ant-btn-proposedchangesdenied {
  background-color: #f44336;
  border-color: #f44336;
}
.ant-btn-draft:focus, .ant-btn-draft:hover {
  background-color: #96786E;
  border-color: #96786E;
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
  color: #fff;
}
.ant-btn-grey:focus, .ant-btn-grey:hover, .ant-btn-cancelled:focus, .ant-btn-cancelled:hover, .ant-btn-cancelledbyuser:focus, .ant-btn-cancelledbyuser:hover {
  background-color: #929392;
  border-color: #929392;
  color: black;
}
.ant-btn-red:focus, .ant-btn-red:hover, .ant-btn-denied:focus, .ant-btn-denied:hover, .ant-btn-proposedchangesdenied:focus, .ant-btn-proposedchangesdenied:hover {
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
.text-denied, .text-red {
  color: #cc3f0c;
}
.border-denied {
  border-color: #cc3f0c;
}
.text-strikethrough {
  text-decoration: line-through;
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
/* Custom hr lines on filters text */
.hr {
  position: relative;
  z-index: 1;
  overflow: hidden;
  text-align: center;
}
.hr:before, .hr:after {
  position: absolute;
  top: 51%;
  overflow: hidden;
  width: 50%;
  height: 1px;
  content: '\a0';
  background-color: grey;
}
.hr:before {
  margin-left: -50%;
  text-align: right;
}
/* Toast Message */
#toast {
  display: none; 
  opacity: 0;
  min-width: 250px; 
  background-color: #f8d7da; 
  color: #333;
  border-radius: 2px; 
  padding: 8px; 
  position: fixed; 
  z-index: 5000; 
  left: 30px; 
  bottom: 30px; 
  border: 1px solid #f4bec2;
  transition: opacity 1s ease;
  box-shadow: 0 0 1px 0 rgb(0 0 0 / 8%), 0 1px 3px 0 rgb(0 0 0 / 15%);
}
#toast.show {
  display: block;
  opacity: 1;
  transition: opacity 1s ease;
}
</v-style>
`
})
