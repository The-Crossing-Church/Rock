import { defineComponent, provide } from "vue"
import { useConfigurationValues, useInvokeBlockAction } from "../../../Util/block"
import { Person, ContentChannelItem, DefinedValue } from "../../../ViewModels"
import { CalendarBlockViewModel } from "./calendarBlockViewModel"
import { useStore } from "../../../Store/index"
import { DateTime, Interval } from "luxon"
import MonthView from "./Components/month"
import WeekView from "./Components/week"
import DayView from "./Components/dayView"
import { Button, Modal } from "ant-design-vue"
import RoomPicker from "../EventForm/Components/roomPicker"
import DropDownList from "../EventDashboard/Components/dropDownList"
import RockLabel from  "../../../Elements/rockLabel"
import RockTextBox from "../../../Elements/textBox"

const store = useStore()

type ListItem = {
  text: string,
  value: string,
  description: string,
  isDisabled: boolean,
  isHeader: boolean,
  type: string,
  order: number
}

export default defineComponent({
  name: "EventCalendar.Calendar",
  components: {
    "tcc-month": MonthView,
    "tcc-week": WeekView,
    "tcc-day": DayView,
    "tcc-room": RoomPicker,
    "tcc-ddl": DropDownList,
    "a-btn": Button,
    "a-modal": Modal,
    "rck-lbl": RockLabel,
    "rck-txt": RockTextBox
  },
  setup() {
      const invokeBlockAction = useInvokeBlockAction();
      const viewModel = useConfigurationValues<CalendarBlockViewModel | null>();

      const getEvents: (start: DateTime, end: DateTime) => Promise<any> = async (start, end) => {
        const response = await invokeBlockAction("GetEvents", {
          start: start, end: end
        });
        return response
      }
      provide("getEvents", getEvents);

      return {
        viewModel,
        getEvents,
      }
  },
  data() {
      return {
        selected: {} as ContentChannelItem,
        loading: false,
        view: 'week',
        currentDate: DateTime.now(),
        lastLoadRange: {} as Interval,
        calendars: [],
        event: null,
        filters: {
          rooms: "",
          ministries: [] as string[],
          parentId: 0,
          resources: [] as string[],
          submitters: ""
        },
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
      };
  },
  computed: {
    /** The person currently authenticated */
    currentPerson(): Person | null {
      return store.state.currentPerson
    },
    displayDate(): string {
      if(this.view == 'month') {
        return this.currentDate.toFormat('MMMM, yyyy')
      } else if(this.view == 'week') {
        return this.currentDate.startOf('week').minus({days: 1}).toFormat('EEEE, MMMM dd') + ' - ' + this.currentDate.endOf('week').minus({days: 1}).toFormat('DDDD')
      } 
      return this.currentDate.toFormat('DDDD')
    },
    rooms() {
      let arr = this.viewModel?.locations as any[]
      return arr?.filter((l: any) => {
        return l.attributeValues?.IsDoor == "False"
      }).map(l => {
        let x = {} as ListItem
        x.value = l.guid
        if(l.value) {
          x.text = l.value
          if(l.attributeValues?.Capacity) {
            x.text += " (" + l.attributeValues?.Capacity + ")"
          }
        }
        if(l.attributeValues?.Type) {
          x.type = l.attributeValues.Type
        }
        if(l.attributeValues?.StandardSetUpDescription) {
          x.description = l.attributeValues.StandardSetUpDescription
        }
        if(!l.isActive) {
          x.isDisabled = true
        }
        if(l.order) {
          x.order = l.order
        }
        return x
      }).sort((a: ListItem, b: ListItem) => {
        if(a.order > b.order) {
          return 1
        } else if(a.order < b.order) {
          return -1
        }
        return 0
      })
    },
    groupedRooms() {        
      let loc = [] as any[]
      this.rooms?.forEach((l: any) => {
          let idx = -1
          loc.forEach((i, x) => {
            if (i.Type == l.type) {
              idx = x
            }
          })
          l.isHeader = false
          if (idx > -1) {
            loc[idx].locations.push(l)
          } else {
            loc.push({ Type: l.type, locations: [l], order: l.order })
          }
      })
      loc.forEach(l => {
          l.locations = l.locations.sort((a:any, b: any) => {
          if (a.order < b.order) {
            return -1
          } else if (a.order > b.order) {
            return 1
          } else {
            return 0
          }
        })
      })
      let arr = [] as any[]
      loc.forEach(l => {
        arr.push({ value: l.Type, isHeader: true, isDisabled: false})
        l.locations.forEach((i:any) => {
          arr.push((i))
        })
      })
      return arr
    },
    ministries() {
      let arr = this.viewModel?.ministries as any[]
      arr = arr?.map((m: DefinedValue) => { return m.value })
      if(arr && arr.length > 0) {
        return arr.sort()
      }
      return []
    },
    filteredCalendars() {
      let cals = JSON.parse(JSON.stringify(this.calendars))
      console.log(cals)
      if(this.filters.parentId && this.filters.parentId > 0) {
        cals.forEach((c: any) => {
          c.events = c.events.filter((e: any) => {
            return e.parentId === this.filters.parentId
          })
        })
      }
      if(this.filters.ministries && this.filters.ministries.length > 0) {
        cals.forEach((c: any) => {
          c.events = c.events.filter((e: any) => {
            return this.filters.ministries.includes(e.ministry)
          })
        })
      }
      if(this.filters.submitters) {
        let creators = this.filters.submitters.split(',').map((s: string) => { return s.trim() })
        cals.forEach((c: any) => {
          c.events = c.events.filter((e: any) => {
            let isMatch = false
            if(e.submitter) {
              for(let i = 0; i < creators.length; i++) {
                if(e.submitter.toLowerCase().includes(creators[i].toLowerCase())) {
                  isMatch = true
                }
              }
            }
            return isMatch
          })
        })
      }
      if(this.filters.rooms) {
        let selected = JSON.parse(this.filters.rooms)
        let items = selected.text.split(", ").filter((s: string) => { return s })
        if(items.length > 0) {
          cals.forEach((c: any) => {
            c.events = c.events.filter((e: any) => {
              let eRooms = e.location.split(", ")
              let intersection = eRooms.filter((value: string) => items.includes(value))
              return intersection.length > 0
            })
          })
        }
      }
      if(this.filters.resources && this.filters.resources.length > 0) {
        cals.forEach((c: any) => {
          c.events = c.events.filter((e: any) => {
            let intersection = e.resources.filter((value: string) => this.filters.resources.includes(value))
            return intersection.length == this.filters.resources.length
          })
        })
      }
      return cals
    }
  },
  methods: {
    loadData(start: DateTime, end: DateTime): void {
      let el = document.getElementById('updateProgress')
      if(el) {
        el.style.display = 'block'
      }
      this.lastLoadRange = Interval.fromDateTimes(start, end)
      this.getEvents(start, end).then((response: any) => {
        if(response.data) {
          this.calendars = response.data
        }
      }).finally(() => {
        if(el) {
          el.style.display = 'none'
        }
      })
    },
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
    viewBtnColor(btn: string) {
      if(this.view == btn) {
        return 'primary'
      }
      return 'accent'
    },
    selectWeek(date: DateTime) {
      this.currentDate = date
      this.view = 'week'
    },
    selectDay(date: DateTime) {
      this.currentDate = date
      this.view = 'day'
    },
    page(isNext: boolean) {
      let val = 1
      if(!isNext) {
        val = -1
      }
      let nextRange = {} as Interval
      if(this.view == 'day') {
        nextRange = Interval.fromDateTimes(this.currentDate.plus({days: val}).startOf('day'), this.currentDate.plus({days: val}).endOf('day'))
        this.currentDate = this.currentDate.plus({days: val}).startOf('day')
      } else if(this.view == 'week') {
        nextRange = Interval.fromDateTimes(this.currentDate.plus({weeks: val}).startOf('week').minus({days: 1}), this.currentDate.plus({weeks: val}).endOf('week').minus({days: 1}))
        this.currentDate = this.currentDate.plus({weeks: val}).startOf('day')
      } else {
        let startOfMonth = this.currentDate.startOf('month')
        nextRange = Interval.fromDateTimes(startOfMonth.plus({months: val}).startOf('day'), startOfMonth.plus({months: val}).endOf('month'))
        this.currentDate = startOfMonth.plus({months: val}).startOf('day')
      }
      //Only load new data if we haven't filtered to a particular event since we should already have the data for it
      if(this.filters.parentId == 0) {
        if(!this.lastLoadRange.engulfs(nextRange)) {
          if(this.view == 'month') {
            this.loadData(this.currentDate.startOf('month'), this.currentDate.endOf('month'))
          } else {
            this.loadData(this.currentDate.startOf('week').minus({days: 1}), this.currentDate.endOf('week').minus({days: 1}))
          }
        }
      }
    },
    filterToEvent(id: number) {
      this.filters.parentId = id
    }
  },
  watch: {
    view(val) {
      if(val == 'month') {
        let nextRange = Interval.fromDateTimes(this.currentDate.startOf('month'), this.currentDate.endOf('month'))
        if(this.filters.parentId == 0) {
          if(!this.lastLoadRange.engulfs(nextRange)) {
            this.loadData(this.currentDate.startOf('month'), this.currentDate.endOf('month'))
          }
        }
      }
    }
  },
  mounted() {
    this.loadData(this.currentDate.startOf('week').minus({days: 1}), this.currentDate.endOf('week').minus({days: 1}))
  },
  template: `
  <div class="card">
    <div class="row">
      <div class="col col-xs-3">
        <a-btn shape="circle" type="primary" data-toggle="collapse" data-target="#calendar-filters" aria-expanded="false" aria-controls="calendar-filters">
          <i class="fa fa-filter"></i>
        </a-btn>
      </div>
      <div class="col col-xs-6 text-center font-weight-bold" style="font-size: 20px;">
        <a-btn shape="circle" type="accent" @click="page(false)">
          <i class="fa fa-chevron-left"></i>
        </a-btn>
        {{displayDate}}
        <a-btn shape="circle" type="accent" @click="page(true)">
          <i class="fa fa-chevron-right"></i>
        </a-btn>
      </div>
      <div class="col col-xs-3 text-right">
        <a-btn :type="viewBtnColor('month')" @click="view = 'month'">Month</a-btn>
        <a-btn class="mx-1" :type="viewBtnColor('week')" @click="view = 'week'">Week</a-btn>
        <a-btn :type="viewBtnColor('day')" @click="view = 'day'">Day</a-btn>
      </div>
    </div>
    <div class="collapse py-4" id="calendar-filters">
      <div class="row">
        <div class="col col-xs-12 col-md-4">
          <tcc-room
            v-model="filters.rooms"
            label="Calendars/Spaces"
            :items="groupedRooms"
            :multiple="true"
          ></tcc-room>
        </div>
        <div class="col col-xs-12 col-md-4">
          <tcc-ddl
            v-model="filters.ministries"
            label="Ministries"
            :items="ministries"
          ></tcc-ddl>
        </div>
        <div class="col col-xs-12 col-md-4">
          <rck-lbl>Filtered to Event</rck-lbl> <br/>
          {{(filters.parentId == 0 ? 'No' : 'Yes')}}
        </div>
      </div>
      <div class="row mt-2">
        <div class="col col-xs-12 col-md-4">
          <rck-lbl>Submitted By</rck-lbl> 
          <rck-txt
            v-model="filters.submitters"
          ></rck-txt>
          <span>Separate names with a comma to search multiple people</span>
        </div>
        <div class="col col-xs-12 col-md-4">
          <tcc-ddl
            label="Requested Resources"
            :items="resources"
            v-model="filters.resources"
          ></tcc-ddl>
        </div>
      </div>
      <div class="pull-right">
        <a-btn type="grey" @click="filters = { rooms: '', ministries: [], parentId: 0, resources: [], submitters: '' }">Clear Filters</a-btn>
      </div>
    </div>
    <tcc-month
      v-if="view == 'month'"
      :calendars="filteredCalendars"
      :currentDate="currentDate"
      v-on:selectWeek="selectWeek"
    ></tcc-month>
    <tcc-week
      v-if="view == 'week'"
      :calendars="filteredCalendars"
      :currentDate="currentDate"
      v-on:selectDay="selectDay"
      v-on:filterToEvent="filterToEvent"
    ></tcc-week>
    <tcc-day
      v-if="view == 'day'"
      :calendars="filteredCalendars"
      :currentDate="currentDate"
      v-on:filterToEvent="filterToEvent"
    ></tcc-day>
  </div>
  <v-style>
    .hover {
      cursor: pointer;
    }
    .text-primary, .text-submitted {
      color: #347689 !important;
    }
    .text-accent, .text-approved {
      color: #8ED2C9 !important;
    }
    .card {
      box-shadow: 0 0 1px 0 rgb(0 0 0 / 8%), 0 1px 3px 0 rgb(0 0 0 / 15%);
      padding: 32px;
      border-radius: 4px;
    }
    .ant-switch-checked, .ant-btn-primary, .dp__range_end, .dp__range_start, .dp__active_date {
      background-color: #347689;
      border-color: #347689;
    }
    .ant-btn-accent {
      background-color: #8ED2C9;
      border-color: #8ED2C9;
    }
    .ant-btn-accent-outlined {
      color: #8ED2C9;
      border-color: #8ED2C9;
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
    .ant-btn-accent-outlined:focus, .ant-btn-accent-outlined:hover {
      border-color: hsl(172deg 30% 55%);
      color: hsl(172deg 30% 55%);
    }
    .ant-btn-grey:focus, .ant-btn-grey:hover, .ant-btn-cancelled:focus, .ant-btn-cancelled:hover, .ant-btn-cancelledbyuser:focus, .ant-btn-cancelledbyuser:hover {
      background-color: #929392;
      border-color: #929392;
      color: black;
    }
    .ant-btn-cancelled, .ant-btn-cancelledbyuser, .ant-btn-grey {
      background-color: #9e9e9e;
      border-color: #9e9e9e;
    }
    .window {
      overflow-y: scroll;
      width: 100%;
      height: 80vh;
    }
    .window::-webkit-scrollbar {
      width: 5px;
      border-radius: 3px;
    }
    .window::-webkit-scrollbar-track {
      background: #bfbfbf;
      -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.1);
    }
    .window::-webkit-scrollbar-thumb {
      background: rgb(224, 224, 224);
      -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.2);
    }
    .window::-webkit-scrollbar-thumb:hover {
      background: #AAA;
    }
    .window::-webkit-scrollbar-thumb:active {
      background: #888;
      -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.3);
    }
    .tcc-cal-wrapper {
      width: 100%;
    }
    .tcc-time-of-day {
      width: 50px;
      text-align: justify;
    }
    .tcc-time-of-day .tcc-hour {
      width: 50px;
    }
    .tcc-hour {
      height: 60px;
      border-bottom: 1px solid grey;
    }
    .tcc-cal-week {
      display: flex;
      justify-content: left;
    }
    .tcc-day-header {
      height: 30px;
    }
    .tcc-cal-header {
      justify-content: space-between;
      font-weight: bold;
      font-size: 14px;
    }
    .tcc-cal-day, .tcc-cal-header-day {
      width: 100%;
      border-radius: 6px;
      cursor: pointer;
    }
    .tcc-cal-header-day {
      height: 30px;
      text-align: center;
    }
    .tcc-month-picker-item:hover {
      background-color: #EEEFEF;
    }
    .tcc-cal-day.selected {
      background-color: #8ED2C9;
    }
    .tcc-cal-day.disabled, .tcc-month-picker-item.disabled {
      color: rgba(0,0,0,.26) !important;
      cursor: not-allowed;
    }
    .tcc-cal-day.diff-month {
      color: rgba(0,0,0,.15) !important;
    }
    .tcc-month-picker-item {
      width: 10px;
      height: 36px;
      border-radius: 4px;
      cursor: pointer;
      flex: 1 0 30%;
      padding: 0 12px;
      margin: 4px 0;
      justify-content: center;
      align-items: center;
      display: flex;
    }
    .tcc-month-picker-item.selected {
      color: #8ED2C9;
      border: 1px solid #8ED2C9;
    }
    .tcc-month-picker {
      display: flex;
      flex-wrap: wrap;
      padding: 8px;
    }
    .fa-disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }
    .tcc-dropdown {
      display: flex;
      flex-direction: column;
      padding: 0px 4px;
      max-height: 300px;
      overflow-y: scroll;
    }
  </v-style>
`
})