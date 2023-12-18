import { defineComponent, provide } from "vue"
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block"
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag"
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag"
import { AdminDashboardBlockViewModel } from "./adminDashboardBlockViewModel"
import { useStore } from "@Obsidian/PageState"
import { DateTime, Duration } from "luxon"
import { Table, Modal, Button, Popover } from "ant-design-vue"
import GridAction from "./Components/adminGridAction"
import TCCModal from "./Components/dashboardModal"
import Details from "./Components/dashboardModal"
import TCCDropDownList from "./Components/dropDownList"
import RockText from "@Obsidian/Controls/textBox"
import RockLabel from "@Obsidian/Controls/rockLabel"
import RockField from "@Obsidian/Controls/rockField"
import DateRangePicker from "@Obsidian/Controls/dateRangePicker"
import PersonPicker from "@Obsidian/Controls/personPicker"
import Comment from "./Components/comment"
import TCCTable from "./Components/requestTable"
import PartialApproval from "./Components/partialApproval"

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
    "tcc-partial": PartialApproval,
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
        submitter: "",
        eventDates:  { lowerValue: "", upperValue: "" },
        eventModified: { lowerValue: "", upperValue: "" }
      }
      defaultFilters.eventModified.upperValue = DateTime.now().toFormat("yyyy-MM-dd")
      let twoWeeks = Duration.fromObject({weeks: 2})
      defaultFilters.eventModified.lowerValue = DateTime.now().minus(twoWeeks).toFormat("yyyy-MM-dd")

      /** A method to load a specific request's details */
      const loadDetails: (id: string | null | undefined) => Promise<any> = async (id) => {
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

      const changeStatus: (id: string | null | undefined, status: string, withComment: boolean) => Promise<any> = async (id, status, withComment) => {
        const response = await invokeBlockAction("ChangeStatus", {
            id: id, status: status, denyWithComments: withComment
        });
        return response
      }
      provide("changeStatus", changeStatus);

      const addComment: (id: string | null | undefined, message: string) => Promise<any> = async (id, message) => {
        const response = await invokeBlockAction("AddComment", {
          id: id, message: message
        });
        return response
      }
      provide("addComment", addComment);
      
      const completePartialApproval: (id: string | null | undefined, approved: string[], denied: string[], events: any[]) => Promise<any> = async (id, approved, denied, events) => {
        const response = await invokeBlockAction("PartialApproval", {
          id: id, approved: approved, denied: denied, events: events
        });
        return response
      }
      provide("completePartialApproval", completePartialApproval);

      const addBuffer: (data: any) => Promise<any> = async (data) => {
        const response = await invokeBlockAction("AddBuffer", {
          data: data
        });
        return response
      }
      provide("addBuffer", addBuffer);

      return {
          viewModel,
          defaultFilters,
          loadDetails,
          filterRequests,
          changeStatus,
          addComment,
          completePartialApproval,
          addBuffer
      }
  },
  data() {
      return {
        columns: [
          {
            title: 'Title',
            dataIndex: 'title',
            key: 'reqtitle'
          },
          {
            title: 'Submitted By',
            dataIndex: 'createdByPersonAliasId',
            key: 'submitter'
          },
          {
            title: 'Submitted On',
            dataIndex: 'startDateTime',
            key: 'start'
          },
          {
            title: 'Event Dates',
            dataIndex: 'attributeValues.EventDates',
            key: 'dates'
          },
          {
            title: 'Requested Resources',
            dataIndex: 'attributeValues.RequestType',
            key: 'resources'
          },
          {
            title: 'Status',
            key: 'action'
          },
        ],
        selected: {} as ContentChannelItemBag,
        createdBy: {},
        modifiedBy: {},
        modal: false,
        commentModal: false,
        partialApprovalModal: false,
        bufferModal: false,
        comment: "",
        visible: false,
        approvePop: false,
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
          "Childcare Catering",
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
      currentPerson(): PersonBag | null {
          return store.state.currentPerson
      },
      selectedStatus(): string {
        if(this.selected?.attributeValues) {
          return this.selected.attributeValues?.RequestStatus
        }
        return ""
      },
      ministryAttr(): PublicAttributeBag | undefined {
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
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      this.loadDetails(item.idKey).then((response: any) => {
        if(response.data) {
          response.data.details.forEach((detail: any) => {
            detail.detail.changes = detail.detailPendingChanges
          })
          response.data.request.childItems = response.data.details.map((detail: any) => { return detail.detail })
          response.data.request.changes = response.data.requestPendingChanges
          response.data.request.comments = response.data.comments
          response.data.request.conflicts = response.data.conflicts
          response.data.request.changesConflicts = response.data.changesConflicts
          this.selected = response.data.request
          this.createdBy = response.data.createdBy
          this.modifiedBy = response.data.modifiedBy
          this.modal = true
        }
      }).finally(() => {
        if(el) {
          el.style.display = 'none'
        }
      })
    },
    editItem (id: string) {
      window.location.href = "/eventform?Id=" + id
    },
    filter(option: string) {
      this.loading = true
      let reqFilter = JSON.parse(JSON.stringify(this.filters))
      if(reqFilter?.ministry) {
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
      if(reqFilter?.ministry) {
        let ministry = JSON.parse(reqFilter.ministry)
        reqFilter.ministry = ministry.value
      }
      this.filterRequests(option, reqFilter).then((response: any) => {
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
        submitter: "",
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
        submitter: "",
        eventDates:  { lowerValue: "", upperValue: "" },
        eventModified: { lowerValue: "", upperValue: "" }
      }
    },
    updateFromGridAction(id: string | null | undefined, status: string) {
      this.selected.idKey = id
      this.updateStatus(status)
    },
    addBufferFromGridAction(id: string | null | undefined) {
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      this.loadDetails(id).then((response: any) => {
        if(response.data) {
          response.data.details.forEach((detail: any) => {
            detail.detail.changes = detail.detailPendingChanges
          })
          response.data.request.childItems = response.data.details.map((detail: any) => { return detail.detail })
          response.data.request.changes = response.data.requestPendingChanges
          response.data.request.comments = response.data.comments
          response.data.request.conflicts = response.data.conflicts
          response.data.request.changesConflicts = response.data.changesConflicts
          this.selected = response.data.request
          this.createdBy = response.data.createdBy
          this.modifiedBy = response.data.modifiedBy
          this.modal = true
          this.bufferModal = true
        }
      }).finally(() => {
        if(el) {
          el.style.display = 'none'
        }
      })
    },
    addBufferFromModal() {
      this.bufferModal = true
    },
    updateStatus(status: string, withComment: boolean = false) {
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      if(status == "In Progress") {
        this.btnLoading.inprogress = true
      } else if (status == "Cancelled") {
        this.btnLoading.cancelled = true
      }
      this.changeStatus(this.selected.idKey, status, withComment).then((res) => {
        if(res.isSuccess) {
          if(res.data.url) {
            window.location.href = res.data.url
          }
          if(res.data.status) {
            if(this.selected?.attributeValues) {
              this.selected.attributeValues.RequestStatus = res.data.status
            }
            this.filterTables("Submitted", null)
            this.filterTables("PendingChanges", null)
            this.filterTables("InProgress", null)
            this.filter("All")
          }
        } else if(res.isError) {
          this.toastMessage = res.errorMessage
          let el = document.getElementById('toast')
          el?.classList.add("show")
        }
      }).catch((err) => {
        console.log(err)
        this.toastMessage = err
        let el = document.getElementById('toast')
        el?.classList.add("show")
      }).finally(() => {
        this.btnLoading.inprogress = false
        this.btnLoading.cancelled = false
        if(el) {
          el.style.display = 'none'
        }
      })
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
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      this.addComment(this.selected.idKey, this.comment).then((res: any) => {
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
      }).finally(() => {
        if(el) {
          el.style.display = 'none'
        }
      })
    },
    partialApproval() {
      let ref = this.$refs.partialApprovalInfo as any
      let approved = ref.approvedAttributes as string[]
      let denied = ref.deniedAttributes as string[]
      let events = ref.eventChanges as any[]
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      this.completePartialApproval(this.selected.idKey, approved, denied, events).then((res) => {
        this.partialApprovalModal = false
        if(res.isSuccess) {
          if(res.data?.id) {
            this.selectItem(res.data)
            this.filterTables("PendingChanges", null)
            this.filter("All")
          }
        } else if (res.isError) {
          this.toastMessage = res.errorMessage
          let el = document.getElementById('toast')
          el?.classList.add("show")
        }
      }).finally(() => {
        if(el) {
          el.style.display = 'none'
        }
      })
    },
    hideToast() {
      let el = document.getElementById('toast')
      el?.classList.remove("show")
    },
    getIsValid(r: any) {
      return r?.attributeValues?.RequestIsValid == 'True'
    },
    getSubmitter(id: string | null | undefined) {
      if(this.viewModel?.users && this.viewModel.users.length > 0) {
        let users = this.viewModel.users as any[]
        let submitter = users.filter(u => {
          return u.primaryAliasId == id
        })
        if(submitter && submitter.length > 0) {
          return submitter[0].fullName
        }
      }
    },
    previewStartBuffer(time: string, buffer: any) {
      if(time && buffer) {
        return DateTime.fromFormat(time, 'HH:mm:ss').minus({minutes: buffer}).toFormat('hh:mm a')
      } else if (time) {
        return DateTime.fromFormat(time, 'HH:mm:ss').toFormat('hh:mm a')
      }
    },
    previewEndBuffer(time: string, buffer: any) {
      if(time && buffer) {
        return DateTime.fromFormat(time, 'HH:mm:ss').plus({minutes: buffer}).toFormat('hh:mm a')
      } else if (time) {
        return DateTime.fromFormat(time, 'HH:mm:ss').toFormat('hh:mm a')
      }
    },
    saveBuffers() {
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      let item = this.selected as any
      let items = item.childItems.map((i: any) => {
        if(i.attributeValues) {
          return {id: i.idKey, start: i.attributeValues.StartBuffer, end: i.attributeValues.EndBuffer}
        }
      })
      this.addBuffer(items).then((res) => {
        this.bufferModal = false
        if(res.isSuccess) {
          this.selectItem(this.selected)
        } else if (res.isError) {
          this.toastMessage = res.errorMessage
          let el = document.getElementById('toast')
          el?.classList.add("show")
        }
      }).finally(() => {
        if(el) {
          el.style.display = 'none'
        }
      })
    }
  },
  watch: {
    
  },
  mounted() {
    this.filters = this.defaultFilters
    let params = new URLSearchParams(window.location.search)
    let id = params.get("Id")
    if(id) {
      this.selectItem({idKey: id})
    }
  },
  template: `
<div class="card mb-2">
  <tcc-table :openByDefault="true" option="Submitted" :events="viewModel.submittedEvents" :users="viewModel.users" v-on:updatestatus="updateFromGridAction" v-on:addbuffer="addBufferFromGridAction" v-on:selectitem="selectItem" v-on:filter="filterTables"></tcc-table>
</div>
<div class="card mb-2">
  <tcc-table :openByDefault="false" option="Pending Changes" :events="viewModel.changedEvents" :users="viewModel.users" v-on:updatestatus="updateFromGridAction" v-on:addbuffer="addBufferFromGridAction" v-on:selectitem="selectItem" v-on:filter="filterTables"></tcc-table>
</div>
<div class="card mb-2">
  <tcc-table :openByDefault="false" option="In Progress" :events="viewModel.inprogressEvents" :users="viewModel.users" v-on:updatestatus="updateFromGridAction" v-on:addbuffer="addBufferFromGridAction" v-on:selectitem="selectItem" v-on:filter="filterTables"></tcc-table>
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
          <rck-text
            label="Submitter/Modifier"
            v-model="filters.submitter"
          ></rck-text>
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
      <template #bodyCell="{ column, record }">
        <template v-if="column.key == 'reqtitle'">
          <div class="hover" @click="selectItem(record)">
            <i v-if="getIsValid(record)" class="fa fa-check-circle text-accent mr-2"></i>
            <i v-else class="fa fa-exclamation-circle text-inprogress mr-2"></i>
            {{ record.title }}
          </div>
        </template>
        <template v-else-if="column.key == 'submitter'">
          {{ getSubmitter(record.createdByPersonAliasId) }}
        </template>
        <template v-else-if="column.key == 'start'">
          {{ formatDateTime(record.startDateTime) }}
        </template>
        <template v-else-if="column.key == 'dates'">
          {{ formatDates(record.attributeValues.EventDates) }}
        </template>
        <template v-else-if="column.key == 'resources'">
          {{ record.attributeValues.RequestType }}
        </template>
        <template v-else-if="column.key == 'action'">
          <tcc-grid :request="record" :url="viewModel.workflowURL" v-on:updatestatus="updateFromGridAction" v-on:addbuffer="addBufferFromGridAction"></tcc-grid>
        </template>
      </template>
    </a-table>
  </div>
</div>
<a-modal v-model:visible="modal" width="80%" :closable="false">
  <tcc-details :request="selected" :rooms="viewModel.locations" :drinks="viewModel.drinks" :inventory="viewModel.inventory" :createdBy="createdBy" :modifiedBy="modifiedBy" v-on:openrequest="selectItem"></tcc-details>
  <template v-if="selected.comments && selected.comments.length > 0">
    <h3 class="text-accent">Comments</h3>
    <div>
      <tcc-comment v-for="(c, idx) in selected.comments" :comment="c.comment" :createdBy="c.createdBy" :next="getNextComment(idx)" :key="c.comment.idKey"></tcc-comment>
    </div>
  </template>
  <template #footer>
    <div class="text-left">
      <a-btn type="primary" @click="editItem(selected.idKey)">
        <i class="mr-1 fa fa-pencil-alt"></i>
        Edit
      </a-btn>
      <a-btn type="accent" v-if="selectedStatus != 'Approved' && selectedStatus != 'Pending Changes'" @click="updateStatus('Approved')">
        <i class="mr-1 fa fa-check"></i>
        Approve
      </a-btn>
      <a-pop v-model:visible="approvePop" trigger="click" placement="top" v-if="selectedStatus == 'Pending Changes'">
        <template #content>
          <div style="display: flex; flex-direction: column;">
            <a-btn class="mb-1" type="accent" @click="updateStatus('Approved')">
              <i class="mr-1 fa fa-check"></i>
              Request
            </a-btn>
            <a-btn type="accent" @click="approvePop = false; partialApprovalModal = true;">
              <i class="mr-1 fas fa-tasks"></i>
              Partial
            </a-btn>
          </div>
        </template>
        <a-btn type="accent">
          <i class="mr-1 fa fa-check"></i>
          Approve
        </a-btn>
      </a-pop>
      <a-btn type="yellow" v-if="selectedStatus != 'In Progress'" @click="updateStatus('In Progress')" :loading="btnLoading.inprogress">
        <i class="mr-1 fas fa-tasks"></i>
        In Progress
      </a-btn>
      <a-pop v-model:visible="visible" trigger="click" placement="top" v-if="selectedStatus != 'Denied'">
        <template #content>
          <div style="display: flex; flex-direction: column;">
            <a-btn class="mb-1" type="red" v-if="selectedStatus == 'Pending Changes'" @click="updateStatus('Proposed Changes Denied', true)">
              <i class="mr-1 fa fa-times"></i>
              Changes w/ Comment
            </a-btn>
            <a-btn class="mb-1" type="red" v-if="selectedStatus == 'Pending Changes'" @click="updateStatus('Proposed Changes Denied')">
              <i class="mr-1 fa fa-times"></i>
              Changes w/o Comment
            </a-btn>
            <a-btn type="red" @click="updateStatus('Denied')">
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
      <a-btn type="grey" v-if="selectedStatus != 'Cancelled'" @click="updateStatus('Cancelled')" :loading="btnLoading.cancelled">
        <i class="mr-1 fa fa-ban"></i>
        Cancel
      </a-btn>
      <a-btn type="accent" @click="newComment">
        <i class="mr-1 fa fa-comment-alt"></i>
        Add Comment
      </a-btn>
      <a-btn type="primary" @click="addBufferFromModal">
        <i class="mr-1 far fa-clock"></i>
        Set-up Buffer
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
<a-modal v-model:visible="partialApprovalModal" width="70%" :closable="false">
  <tcc-partial :request="selected" ref="partialApprovalInfo"></tcc-partial>
  <template #footer>
    <a-btn type="accent" @click="partialApproval">
      <i class="mr-1 fa fa-check"></i>
      Complete
    </a-btn>
    <a-btn type="grey" @click="partialApprovalModal = false;">
      <i class="mr-1 fa fa-ban"></i>
      Cancel
    </a-btn>
  </template>
</a-modal>
<a-modal v-model:visible="bufferModal" width="70%" :closable="false">
  <div v-for="e in selected.childItems" :key="e.idKey">
    <h3>
      <template v-if="selected.attributeValues.IsSame == 'True'">
        Set-up Buffer for Events
      </template>
      <template v-else>
        Set-up Buffer for: {{e.attributeValues.EventDate}}
      </template>
    </h3>
    <div class="row">
      <div class="col col-xs-6">
        <rck-field
          v-model="e.attributeValues.StartTime"
          :attribute="e.attributes.StartTime"
          :showEmptyValue="true"
        ></rck-field>
      </div>
      <div class="col col-xs-6">
        <rck-field
          v-model="e.attributeValues.EndTime"
          :attribute="e.attributes.EndTime"
          :showEmptyValue="true"
        ></rck-field>
      </div>
    </div>
    <div class="row">
      <div class="col col-xs-6">
        <rck-field
          v-model="e.attributeValues.StartBuffer"
          :attribute="e.attributes.StartBuffer"
          :is-edit-mode="true"
        ></rck-field>
      </div>
      <div class="col col-xs-6">
        <rck-field
          v-model="e.attributeValues.EndBuffer"
          :attribute="e.attributes.EndBuffer"
          :is-edit-mode="true"
        ></rck-field>
      </div>
    </div>
    <div class="row">
      <div class="col col-xs-6">
        <rck-lbl>Space Reservation Starting At</rck-lbl> <br/>
        {{previewStartBuffer(e.attributeValues.StartTime, e.attributeValues.StartBuffer)}}
      </div>
      <div class="col col-xs-6">
        <rck-lbl>Space Reservation Ending At</rck-lbl> <br/>
        {{previewEndBuffer(e.attributeValues.EndTime, e.attributeValues.EndBuffer)}}
      </div>
    </div>
  </div>
  <template #footer>
    <a-btn type="primary" @click="saveBuffers">
      <i class="mr-1 fas fa-save"></i>
      Save
    </a-btn>
    <a-btn type="grey" @click="bufferModal = false;">
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
  color: #347689 !important;
}
.text-accent, .text-approved {
  color: #8ED2C9 !important;
}
.border-approved {
  border-color: #8ED2C9 !important;
}
.text-inprogress {
  color: #ecc30b !important;
}
.border-inprogress {
  boarder-color: #ecc30b !important;
}
.text-pendingchanges {
  color: #61a4a9 !important;
}
.border-pendingchanges {
  border-color: #61a4a9 !important;
}
.text-cancelled, .text-cancelledbyuser {
  color: #3d3d3d !important;
}
.border-cancelled, .border-cancelledbyuser {
  border-color: #3d3d3d !important;
}
.text-denied, .text-red, .text-proposedchangesdenied {
  color: #cc3f0c !important;
}
.border-denied, .border-proposedchangesdenied {
  border-color: #cc3f0c !important;
}
.text-strikethrough {
  text-decoration: line-through !important;
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
