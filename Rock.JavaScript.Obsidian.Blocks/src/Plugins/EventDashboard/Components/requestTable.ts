import { defineComponent, Prop, PropType } from "vue"
import { PublicAttributeBag } from "@Obsidian/ViewModels/Utility/publicAttributeBag"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { DateTime } from "luxon"
import RockField from "@Obsidian/Controls/rockField.obs"
import TCCDropDownList from "../Components/dropDownList"
import GridAction from "../Components/adminGridAction"
import RockText from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import DateRangePicker from "@Obsidian/Controls/dateRangePicker.obs"
import PersonPicker from "@Obsidian/Controls/personPicker.obs"
import DropDownList from "@Obsidian/Controls/dropDownList.obs"
import Grid, { Column, TextColumn, DateColumn, PersonColumn, textValueFilter, dateValueFilter, pickExistingValueFilter } from "@Obsidian/Controls/grid"

export default defineComponent({
    name: "EventDashboard.Components.RequestTable",
    components: {
      "rck-text": RockText,
      "rck-lbl": RockLabel,
      "rck-field": RockField,
      "rck-date-range": DateRangePicker,
      "rck-person": PersonPicker,
      "rck-btn": RockButton,
      "rck-grid": Grid,
      "rck-col": Column,
      "rck-col-txt": TextColumn,
      "rck-col-dt": DateColumn,
      "rck-col-per": PersonColumn,
      "rck-ddl": DropDownList,
      "tcc-ddl": TCCDropDownList,
      "tcc-grid": GridAction,
    },
    props: {
      events: Array as PropType<ContentChannelItemBag[]>,
      workflowURL: String,
      defaultFilters: Object as any,
      option: String,
      openByDefault: Boolean,
      users: Array as PropType<any[]>,
      resources: Array as PropType<any[]>,
      ministryAttr: Object as PropType<PublicAttributeBag>
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
          textValueFilter: textValueFilter,
          dateValueFilter: dateValueFilter,
          pickExistingValueFilter: pickExistingValueFilter,
          loading: false,
          defaultClass: "",
          resources: [
            { text: "Room",  value: "Room" },
            { text: "Online Event", value: "Online Event" },
            { text: "Catering", value: "Catering" },
            { text: "Childcare", value: "Childcare" },
            { text: "Extra Resources", value: "Extra Resources" },
            { text: "Registration", value: "Registration" },
            { text: "Web Calendar", value: "Web Calendar" },
            { text: "Production", value: "Production" },
            { text: "Publicity", value: "Publicity" }
          ]
        };
    },
    computed: {
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
    }, //Title, Date, Status on mobile, nothing else 
    template: `
<div style="display: flex; align-items: center;">
  <i class="fa fa-filter mr-2 mb-2 hover fa-lg" data-toggle="collapse" :data-target="filterCollapseSelector" aria-expanded="false" :aria-controls="filterCollapseId"></i>
  <h4 class="text-primary hover" data-toggle="collapse" :data-target="collapseSelector" aria-expanded="false" :aria-controls="collapseId">{{option}}</h4>
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
        <rck-ddl
          label="Requested Resources"
          :items="resources"
          v-model="filters.resources"
        ></rck-ddl>
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
        <rck-btn btnType="grey" @click="clearFilters">Clear Filters</rck-btn>
        <rck-btn class="pull-right" btnType="primary" @click="filter" :isLoading="loading">Filter</rck-btn>
      </div>
    </div>
  </div>
  <rck-grid :data="{ rows: events }" keyField="idKey" :isTitleHidden="true" itemTerm="request">
    <rck-col-txt
      name="title"
      title="Title"
      field="title"
      :filter="textValueFilter"
      visiblePriority="xs"
    >
      <template #format="{ row }">
        <div class="hover w-100" @click="selectItem(row)">
          <i v-if="getIsValid(row)" class="fa fa-check-circle text-accent mr-2"></i>
          <i v-else class="fa fa-exclamation-circle text-inprogress mr-2"></i> 
          {{ row.title }}
         </div>
      </template>
    </rck-col-txt>
    <rck-col-txt
      name="createdByPersonAliasId"
      title="Submitted By"
      field="createdByPersonAliasId"
      :filter="textValueFilter"
      visiblePriority="md"
    >
      <template #format="{ row }">
        {{getSubmitter(row.createdByPersonAliasId)}}
      </template>  
    </rck-col-txt>
    <rck-col-dt
      name="startDateTime"
      title="Submitted On"
      field="startDateTime"
      :filter="dateValueFilter"
      visiblePriority="md"
    >
      <template #format="{ row }">
        {{ formatDateTime(row.startDateTime) }}
      </template>
    </rck-col-dt>
    <rck-col
      name="attributeValues.EventDates"
      title="Event Dates"
      field="attributeValues.EventDates"
      visiblePriority="xs"
    >
      <template #format="{ row }">
        {{ formatDates(row.attributeValues.EventDates) }}
      </template>
    </rck-col>
    <rck-col
      name="attributeValues.RequestType"
      title="Requested Resources"
      field="attributeValues.RequestType"
      visiblePriority="md"
    >
      <template #format="{ row }">
        {{ row.attributeValues.RequestType }}
      </template>
    </rck-col>
    <rck-col
      name="attributeValues.RequestStatus"
      title="Status"
      field="attributeValues.RequestStatus"
      itemClass="overflow-visible"
      visiblePriority="xs"
    >
      <template #format="{ row }">
        <tcc-grid :request="row" :url="workflowURL" v-on:updatestatus="updateFromGridAction" v-on:addbuffer="addBuffer"></tcc-grid>
      </template>
    </rck-col>
  </rck-grid>
</div>
`
});
