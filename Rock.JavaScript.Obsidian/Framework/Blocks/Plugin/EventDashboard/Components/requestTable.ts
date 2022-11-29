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
      openByDefault: Boolean
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
            submitter: { value: "", text: ""},
            eventDates:  { lowerValue: "", upperValue: "" },
            eventModified: { lowerValue: "", upperValue: "" }
          },
          loading: false,
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
          submitter: { value: "", text: ""},
          eventDates:  { lowerValue: "", upperValue: "" },
          eventModified: { lowerValue: "", upperValue: "" }
        }
      },
      updateFromGridAction(id: number, status: string) {
        this.$emit("updatestatus", id, status)
      },
      selectItem(item: any) {
        this.$emit("selectitem", item)
      },
      filter() {
        this.loading = true
        this.$emit("filter", this.option?.replace(" ", ""), this.filters)
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
      <tcc-grid :request="r" :url="workflowURL" v-on:updatestatus="updateFromGridAction"></tcc-grid>
    </template>
  </a-table>
</div>
`
});
