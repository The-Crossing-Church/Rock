import { defineComponent, PropType } from "vue"
import { PublicAttribute, ContentChannelItem } from "../../../../ViewModels"
import { DateTime } from "luxon"
import { Table, Button, Popover } from "ant-design-vue"
import RockField from "../../../../Controls/rockField"
import TCCDropDownList from "../Components/dropDownList"
import GridAction from "../Components/adminGridAction"
import RockText from "../../../../Elements/textBox"
import RockLabel from "../../../../Elements/rockLabel"
import DateRangePicker from "../../../../Elements/dateRangePicker"
import PersonPicker from "../../../../Controls/personPicker"

export default defineComponent({
    name: "EventDashboard.Components.RequestTable",
    components: {
      "a-table": Table,
      "a-btn": Button,
      "a-pop": Popover,
      "rck-text": RockText,
      "rck-lbl": RockLabel,
      "rck-field": RockField,
      "rck-date-range": DateRangePicker,
      "rck-person": PersonPicker,
      "tcc-ddl": TCCDropDownList,
      "tcc-grid": GridAction,
    },
    props: {
      events: Array as PropType<ContentChannelItem[]>,
      workflowURL: String,
      defaultFilters: Object as any,
      option: String,
      openByDefault: Boolean,
      users: Array as PropType<any[]>
    },
    setup() {

    },
    data() {
        return {
          filters: {
            title: "",
            statuses: [] as string[],
            resources: [] as string[],
            ministry: "",
            submitter: "",
            eventDates:  { lowerValue: "", upperValue: "" },
            eventModified: { lowerValue: "", upperValue: "" }
          },
          loading: false,
          columns: [
            {
              title: 'Title',
              dataIndex: 'title',
              key: 'reqtitle',
              slots: { customRender: 'reqtitle' },
            },
            {
              title: 'Submitted By',
              dataIndex: 'createdByPersonAliasId',
              key: 'submitter',
              slots: { customRender: 'submitter' },
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
              title: 'Requested Resources',
              dataIndex: 'attributeValues.RequestType',
              key: 'resources',
              slots: { customRender: 'resources' },
            },
            {
              title: 'Status',
              key: 'action',
              slots: { customRender: 'action' },
            },
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
          defaultClass: ""
        };
    },
    computed: {
      ministryAttr(): PublicAttribute | undefined {
        if(this.events && this.events[0]) {
          return this.events[0]?.attributes?.Ministry
        }
        return undefined
      },
      filterCollapseId() {
        return this.option?.replace(" ", "") + "filterCollapse"
      },
      filterCollapseSelector() {
        return '#' + this.filterCollapseId
      },
      collapseId() {
        return this.option?.replace(" ", "") + "Collapse"
      },
      collapseSelector() {
        return '#' + this.collapseId
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
      clearFilters() {
        this.filters = {
          title: "",
          statuses: [],
          resources: [] as string[],
          ministry: "",
          submitter: "",
          eventDates:  { lowerValue: "", upperValue: "" },
          eventModified: { lowerValue: "", upperValue: "" }
        }
      },
      updateFromGridAction(id: number, status: string) {
        this.$emit("updatestatus", id, status)
      },
      addBuffer(id: number) {
        this.$emit("addbuffer", id)
      },
      selectItem(item: any) {
        this.$emit("selectitem", item)
      },
      filter() {
        this.loading = true
        this.$emit("filter", this.option?.replace(" ", ""), this.filters)
      },
      getIsValid(r: any) {
        return r?.attributeValues?.RequestIsValid == 'True'
      },
      getSubmitter(id: number) {
        if(this.users && this.users.length > 0) {
          let submitter = this.users.filter(u => {
            return u.primaryAliasId == id
          })
          if(submitter) {
            return submitter[0].fullName
          }
        }
      }
    },
    watch: {
      events: { 
        handler(val) {
          this.loading = false
        }, 
        deep: true
      }
    },
    mounted() {
      if(this.openByDefault) {
        this.defaultClass = "collapse in"
      } else {
        this.defaultClass = "collapse"
      }
    },
    template: `
<div style="display: flex; align-items: center;">
  <i class="fa fa-filter mr-2 mb-2 hover fa-lg" data-toggle="collapse" :data-target="filterCollapseSelector" aria-expanded="false" :aria-controls="filterCollapseId"></i>
  <h2 class="text-primary hover" data-toggle="collapse" :data-target="collapseSelector" aria-expanded="false" :aria-controls="collapseId">{{option}}</h2>
</div>
<div :class="defaultClass" :id="collapseId">  
  <div class="collapse" :id="filterCollapseId">
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
      <div class="col col-xs-12 col-md-6">
        <tcc-ddl
          label="Requested Resources"
          :items="resources"
          v-model="filters.resources"
        ></tcc-ddl>
      </div>
      <div class="col col-xs-12 col-md-6">
        <rck-date-range
          label="Has Event Date in Range"
          v-model="filters.eventDates"
        ></rck-date-range>
      </div>
    </div>
    <div class="row">
      <div class="col col-xs-12">
        <a-btn type="grey" @click="clearFilters">Clear Filters</a-btn>
        <a-btn class="pull-right" type="primary" @click="filter" :loading="loading">Filter</a-btn>
      </div>
    </div>
  </div>
  <a-table :columns="columns" :data-source="events" :pagination="{ pageSize: 30 }">
    <template #reqtitle="{ text: reqtitle, record: r }">
      <div class="hover" @click="selectItem(r)">
        <i v-if="getIsValid(r)" class="fa fa-check-circle text-accent mr-2"></i>
        <i v-else class="fa fa-exclamation-circle text-inprogress mr-2"></i>
        {{ reqtitle }}
      </div>
    </template>
    <template #submitter="{ text: submitter }">
      {{ getSubmitter(submitter) }}
    </template>
    <template #start="{ text: start }">
      {{ formatDateTime(start) }}
    </template>
    <template #dates="{ text: dates }">
      {{ formatDates(dates) }}
    </template>
    <template #resources="{ text: resources }">
      {{ resources }}
    </template>
    <template #action="{ record: r }">
      <tcc-grid :request="r" :url="workflowURL" v-on:updatestatus="updateFromGridAction" v-on:addbuffer="addBuffer"></tcc-grid>
    </template>
  </a-table>
</div>
`
});
