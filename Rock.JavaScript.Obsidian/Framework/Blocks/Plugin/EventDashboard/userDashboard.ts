import { defineComponent, provide } from "vue"
import { useConfigurationValues, useInvokeBlockAction } from "../../../Util/block"
import { Person, ContentChannelItem, PublicAttribute } from "../../../ViewModels"
import { UserDashboardBlockViewModel, DuplicateRequestViewModel } from "./userDashboardBlockViewModel"
import { useStore } from "../../../Store/index"
import { DateTime, Duration } from "luxon"
import { Table, Modal, Button } from "ant-design-vue"
import DatePicker from "../EventForm/Components/datePicker"
import Calendar from "../EventForm/Components/calendar"
import Chip from "../EventForm/Components/chip"
import TCCModal from "./Components/dashboardModal"
import Details from "./Components/dashboardModal"
import TCCDropDownList from "./Components/dropDownList"
import RockText from "../../../Elements/textBox"
import RockLabel from "../../../Elements/rockLabel"
import RockField from "../../../Controls/rockField"
import DateRangePicker from "../../../Elements/dateRangePicker"
import PersonPicker from "../../../Controls/personPicker"
import Comment from "./Components/comment"
import UserGridAction from "./Components/userGridAction"

const store = useStore();

export default defineComponent({
  name: "EventDashboard.UserDashboard",
  components: {
    "a-table": Table,
    "a-modal": Modal,
    "a-btn": Button,
    "tcc-model": TCCModal,
    "tcc-details": Details,
    "tcc-ddl": TCCDropDownList,
    "tcc-comment": Comment,
    "tcc-grid": UserGridAction,
    "tcc-date-picker": DatePicker,
    "tcc-chip": Chip,
    "tcc-calendar": Calendar,
    "rck-text": RockText,
    "rck-lbl": RockLabel,
    "rck-field": RockField,
    "rck-date-range": DateRangePicker,
    "rck-person": PersonPicker,
  },
  setup() {
      const invokeBlockAction = useInvokeBlockAction();
      const viewModel = useConfigurationValues<UserDashboardBlockViewModel | null>();
      viewModel?.events.forEach((e: any) => {
        e.childItems = viewModel.eventDetails.filter((d: any) => { return d.contentChannelItemId == e.id && d.childContentChannelItem.contentChannelId == viewModel.eventDetailsCCId })
        e.comments = viewModel.eventDetails.filter((d: any) => { return d.contentChannelItemId == e.id && d.childContentChannelItem.contentChannelId == viewModel.commentsCCId })
      })

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

      const filterRequests: (filters: any) => Promise<any> = async (filters) => {
        const response = await invokeBlockAction("FilterRequests", {
            filters: filters
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

      /** Resubmit a Copy of an Event */
      const resubmitEvent: (id: number, eventDates: string, removedResources: string[], copyDates: any[]) => Promise<any> = async (id, eventDates, removedResources, copyDates) => {
          const response = await invokeBlockAction<{ expirationDateTime: string }>("DuplicateEvent", {
            id: id, eventDates: eventDates, removedResources: removedResources, copyDates: copyDates
          });
          if (response.data) {
              return response
          }
      };
      provide("resubmitEvent", resubmitEvent);

      return {
          viewModel,
          defaultFilters,
          loadDetails,
          filterRequests,
          changeStatus,
          addComment,
          resubmitEvent
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
        copy: {} as ContentChannelItem,
        copyDates: [] as any[],
        removedResources: [] as string[],
        createdBy: {},
        modifiedBy: {},
        modal: false,
        commentModal: false,
        resubmissionModal: false,
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
      },
      canEdit(): boolean {
        if(this.selected && this.selected?.attributeValues?.RequestStatus) {
          return this.selected.attributeValues.RequestStatus == 'Draft' || this.selected.attributeValues.RequestStatus == 'Submitted' || this.selected.attributeValues.RequestStatus == 'In Progress' || this.selected.attributeValues.RequestStatus == 'Approved' || this.selected.attributeValues.RequestStatus == 'Pending Changes'
        }
        return false
      },
      minResubmissionDate() {
        let date = DateTime.now()
        if(this.copy && this.copy.attributeValues) {
          if(this.copy.attributeValues.NeedsPublicity == 'True') {
            date = date.plus({weeks: 6})
          } else if(this.copy.attributeValues.NeedsChildCare == 'True') {
            date = date.plus({days: 30})
          } else if (this.copy.attributeValues.NeedsOnline == 'True' ||
            this.copy.attributeValues.NeedsCatering == 'True' ||
            this.copy.attributeValues.NeedsOpsAccommodations == 'True' ||
            this.copy.attributeValues.NeedsProductionAccommodations == 'True' ||
            this.copy.attributeValues.NeedsRegistration == 'True' ||
            this.copy.attributeValues.NeedsWebCalendar == 'True'
          ) {
            date = date.plus({days: 14})
          }
        }
        return date.toFormat("yyyy-MM-dd")
      }
  },
  methods: {
    formatDateTime(date: any): string {
      if(date) {
        return DateTime.fromISO(date).toFormat("MM/dd/yyyy hh:mm a");
      }
      return ""
    },
    formatDate(date: any): string {
      if(date) {
        return DateTime.fromISO(date).toFormat("MM/dd/yyyy");
      }
      return ""
    },
    formatLongDate(date: any): string {
      return DateTime.fromFormat(date, "yyyy-MM-dd").toFormat("DDDD")
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
        if(response.isError) {
          this.toastMessage = response.errorMessage
          let el = document.getElementById('toast')
          el?.classList.add("show")
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
    filter() {
      this.loading = true
      this.filterRequests(this.filters).then((response: any) => {
        if(this.viewModel) {
          this.viewModel.events = response.data.events
          this.viewModel?.events.forEach((e: any) => {
            e.childItems = this.viewModel?.eventDetails.filter((d: any) => { return d.contentChannelItemId == e.id && d.childContentChannelItem.contentChannelId == this.viewModel?.eventDetailsCCId })
            e.comments = this.viewModel?.eventDetails.filter((d: any) => { return d.contentChannelItemId == e.id && d.childContentChannelItem.contentChannelId == this.viewModel?.commentsCCId })
          })
        }
        if(response.isError) {
          this.toastMessage = response.errorMessage
          let el = document.getElementById('toast')
          el?.classList.add("show")
        }
      }).catch((err) => {
        console.log(err)
      }).finally(() => {
        this.loading = false
      })
    },
    resetFilters() {
      this.filters = this.defaultFilters
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
        if(res.data?.url) {
          window.location.href = res.data.url
        }
        if(res.data?.status) {
          if(this.selected.attributeValues) {
            this.selected.attributeValues.RequestStatus = res.data.status
          }
          this.viewModel?.events.forEach((event: any) => {
            if(event.id == this.selected.id) {
              event.attributeValues.RequestStatus = res.data.status
            }
          })
        }
        if(res.isError || res.Message) {
          this.toastMessage = res.errorMessage ? res.errorMessage : res.Message
          let el = document.getElementById('toast')
          el?.classList.add("show")
        }
      }).catch((err) => {
        this.toastMessage = err
        let el = document.getElementById('toast')
        el?.classList.add("show")
      }).finally(() => {
        this.btnLoading.inprogress = false
        this.btnLoading.cancelled = false
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
    copyFromGridAction(id: number) {
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
          this.selected = response.data.request
          this.createdBy = response.data.createdBy
          this.modifiedBy = response.data.modifiedBy
          this.modal = true
          this.copyRequest()
        }
        if(response.isError) {
          this.toastMessage = response.errorMessage
          let el = document.getElementById('toast')
          el?.classList.add("show")
        }
      }).finally(() => {
        if(el) {
          el.style.display = 'none'
        }
      })
    },
    copyRequest() {
      this.copy = JSON.parse(JSON.stringify(this.selected)) 
      let cp = this.copy as any
      if(cp && cp.childItems && cp.attributeValues.IsSame == 'False') {
        this.copyDates = cp.attributeValues.EventDates.split(",").map((d: string) => { 
          return { originalDate: d.trim(), newDate: ""}
        })
      }
      if(this.copy?.attributeValues?.IsSame == 'True') {
        this.copy.attributeValues.EventDates = ""
      }
      this.resubmissionModal = true
    },
    removeCopyDate(date: string) {
      let cp = this.copy as any
      let idx = -1
      for(let i=0; i<cp.childItems.length; i++) {
        if(cp.childItems[i].originalDate == date) {
          idx = i
        }
      }
      cp.childItems.splice(idx, 1)
      this.copyDates.splice(idx, 1)
    },
    removeChipDate(date: string) {
      if (this.copy.attributeValues?.EventDates) {
        let dates = this.copy.attributeValues.EventDates.split(',')
        let idx = dates.indexOf(date)
        dates.splice(idx, 1)
        this.copy.attributeValues.EventDates = dates.join(",")
      }
    },
    resubmit() {
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      if(this.copy.attributeValues) {
        let eventDates = ""
        if(this.copy.attributeValues.IsSame == 'True') {
          eventDates = this.copy.attributeValues.EventDates
        }
        this.resubmitEvent(this.copy.id, eventDates, this.removedResources, this.copyDates).then((res: any) => {
          if(res) {
            if(res.isSuccess) {
              this.resubmissionModal = false
              this.copy = {} as ContentChannelItem
              this.copyDates = []
              this.removedResources = []
              if(res.data.id) {
                window.location.href = "/eventform?Id=" + res.data.id
              }
            } else if(res.isError) {
              this.toastMessage = res.errorMessage
              let el = document.getElementById('toast')
              el?.classList.add("show")
            }
          } else {
            this.toastMessage = "Unable to resubmit event"
            let el = document.getElementById('toast')
            el?.classList.add("show")
          }
        }).catch((err) => {
          console.log(err)
        }).finally(() => {
          if(el) {
            el.style.display = 'none'
          }
        })
      }
    },
    lastCommentIsFromUser(req: any) {
      let ct = 0
      if(req.comments && req.comments.length > 0) {
        for(let i=req.comments.length - 1; i >= 0; i--) {
          if(req.comments[i].childContentChannelItem.title.split("From ")[1] != this.currentPerson?.fullName) {
            ct++
          } else {
            return ct
          }
        }
      }
      return ct
    },
    getIsValid(r: any) {
      return r?.attributeValues?.RequestIsValid == 'True'
    },
  },
  watch: {
    
  },
  mounted() {
    this.filters = this.defaultFilters
    let params = new URLSearchParams(window.location.search)
    let id = params.get("Id")
    if(id) {
      this.selectItem({id: id})
    }
  },
  template: `
<div class="card">
  <h4 class="hover" data-toggle="collapse" data-target="#filterCollapse" aria-expanded="false" aria-controls="filterCollapse">
    <i class="fa fa-filter mr-2"></i>
    Filters
  </h4>
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
        <a-btn class="pull-right" type="primary" @click="filter" :loading="loading">Filter</a-btn>
      </div>
    </div>
  </div>
  <a-table :columns="columns" :data-source="viewModel.events" :pagination="{ pageSize: 30 }">
    <template #title="{ text: title, record: r }">
      <div class="hover" @click="selectItem(r)">
        <i v-if="getIsValid(r)" class="fa fa-check-circle text-accent mr-2"></i>
        <i v-else class="fa fa-exclamation-circle text-inprogress mr-2"></i>
        {{ title }}
      </div>
    </template>
    <template #start="{ text: start }">
      {{ formatDateTime(start) }}
    </template>
    <template #dates="{ text: dates }">
      {{ formatDates(dates) }}
    </template>
    <template #action="{ record: r }">
      <tcc-grid :request="r" :commentNotification="lastCommentIsFromUser(r)" v-on:duplicate="copyFromGridAction" v-on:updatestatus="updateFromGridAction"></tcc-grid>
    </template>
  </a-table>
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
        <a-btn v-if="canEdit" type="primary" @click="editItem(selected.id)">
          <i class="mr-1 fa fa-pencil-alt"></i>
          Edit
        </a-btn>
        <a-btn type="grey" v-if="selectedStatus != 'Cancelled by User' && selectedStatus != 'Cancelled'" @click="updateStatus('Cancelled by User')">
          <i class="mr-1 fa fa-ban"></i>
          Cancel
        </a-btn>
        <a-btn type="accent" @click="newComment">
          <i class="mr-1 fa fa-comment-alt"></i>
          Add Comment
        </a-btn>
        <a-btn type="med-blue" @click="copyRequest">
          <i class="mr-1 fas fa-history"></i>
          Resubmit
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
  <a-modal v-model:visible="resubmissionModal" width="75%" :closable="false" style="z-index: 2000 !important;">
    <h3>Resubmit {{copy.title}}</h3>
    <div>To resubmit a request, please select the new date(s) for your event.</div>
    <rck-lbl style="text-transform: uppercase;">Resources Requested for {{copy.title}}</rck-lbl>
    <div style="display: flex; flex-wrap: wrap; align-content: flex-start;">
      <tcc-chip v-if="copy.attributeValues?.NeedsSpace == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsSpace = 'False'; removedResources.push('NeedsSpace')">Space</tcc-chip>
      <tcc-chip v-if="copy.attributeValues?.NeedsOnline == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsOnline = 'False'; removedResources.push('NeedsOnline')">Zoom</tcc-chip>
      <tcc-chip v-if="copy.attributeValues?.NeedsCatering == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsCatering = 'False'; removedResources.push('NeedsCatering')">Catering</tcc-chip>
      <tcc-chip v-if="copy.attributeValues?.NeedsChildCare == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsChildCare = 'False'; removedResources.push('NeedsChildCare')">Childcare</tcc-chip>
      <tcc-chip v-if="copy.attributeValues?.NeedsChildCareCatering == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsChildCareCatering = 'False'; removedResources.push('NeedsChildCareCatering')">Childcare Catering</tcc-chip>
      <tcc-chip v-if="copy.attributeValues?.NeedsRegistration == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsRegistration = 'False'; removedResources.push('NeedsRegistration')">Registration</tcc-chip>
      <tcc-chip v-if="copy.attributeValues?.NeedsOpsAccommodations == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsOpsAccommodations = 'False'; removedResources.push('NeedsOpsAccommodations')">Ops Accommodations</tcc-chip>
      <tcc-chip v-if="copy.attributeValues?.NeedsProductionAccommodations == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsProductionAccommodations = 'False'; removedResources.push('NeedsProductionAccommodations')">Production Accommodations</tcc-chip>
      <tcc-chip v-if="copy.attributeValues?.NeedsWebCalendar == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsWebCalendar = 'False'; removedResources.push('NeedsWebCalendar')">Web Calendar</tcc-chip>
      <tcc-chip v-if="copy.attributeValues?.NeedsPublicity == 'True'" v-on:chipdeleted="copy.attributeValues.NeedsPublicity = 'False'; removedResources.push('NeedsPublicity')">Publicity</tcc-chip>
    </div>
    <div class="pb-2">Remove any you won't need for your re-submission of this event</div>
    <template v-if="copy?.attributeValues && copy.attributeValues.IsSame == 'True'">
      <div class="row">
        <div class="col col-xs-4">
          <tcc-calendar
            :min="minResubmissionDate"
            :multiple="true"
            v-model="copy.attributeValues.EventDates"
          ></tcc-calendar>
        </div>
        <div class="col col-xs-12 col-md-8" style="display: flex; flex-wrap: wrap; align-content: flex-start;">
          <template v-if="copy?.attributeValues?.EventDates != ''">
            <tcc-chip v-for="d in copy.attributeValues.EventDates.split(',')" :key="d" v-on:chipdeleted="removeChipDate(d)">
              {{formatLongDate(d)}}
            </tcc-chip>
          </template>
        </div>
      </div>
    </template>
    <template v-else>
      <div class="row text-center">
        <div class="col col-xs-4">
          <rck-lbl>Original Date</rck-lbl>
        </div>
        <div class="col col-xs-4">
          <rck-lbl>New Date</rck-lbl>
        </div>
        <div class="col col-xs-4">
          <rck-lbl>Remove Date</rck-lbl>
        </div>
      </div>
      <div class="row text-center" v-for="(cd, idx) in copyDates" :key="idx" style="display: flex; align-items: end;">
        <div class="col col-xs-4">
          {{formatDate(cd.originalDate)}}
        </div>
        <div class="col col-xs-4">
          <tcc-date-picker
            v-model="cd.newDate"
            :min="minResubmissionDate"
          ></tcc-date-picker>
        </div>
        <div class="col col-xs-4">
          <a-btn shape="circle" type="red" @click="removeCopyDate(cd.originalDate)">
            <i class="fas fa-trash"></i>
          </a-btn>
        </div>
      </div>
    </template>
    <template #footer>
      <a-btn type="med-blue" @click="resubmit" :disabled="copy?.attributeValues?.IsSame != 'True' && copyDates.length == 0">
        <i class="mr-1 fas fa-history"></i>
        Resubmit Event
      </a-btn>
      <a-btn type="grey" @click="resubmissionModal = false;">
        <i class="mr-1 fa fa-ban"></i>
        Cancel
      </a-btn>
    </template>
  </a-modal>
</div>
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
  padding: 32px;
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
.ant-btn-pendingchanges, .ant-btn-med-blue {
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
.ant-btn-pendingchanges:focus, .ant-btn-pendingchanges:hover, .ant-btn-med-blue:focus, .ant-btn-med-blue:hover {
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
  border-color: #8ED2C9;
}
.text-inprogress {
  color: #ecc30b !important;
}
.border-inprogress {
  boarder-color: #ecc30b !important;
}
.text-draft {
  color: #A18276 !important;
}
.border-draft {
  border-color: #A18276 !important;
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
.text-denied, .text-red {
  color: #cc3f0c !important;
}
.border-denied {
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
