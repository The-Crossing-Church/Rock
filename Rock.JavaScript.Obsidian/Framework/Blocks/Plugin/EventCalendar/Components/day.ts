import { defineComponent, PropType } from "vue"
import { DateTime, Interval } from "luxon"
import { Modal, Button } from "ant-design-vue"
import { useStore } from "../../../../Store/index"
import { Person } from "../../../../ViewModels"
import Chip from "../../EventForm/Components/chip"

const store = useStore()

type Event = {
   id: number,
   parentId: number,
   start: string,
   end: string,
   adjustedStart: DateTime,
   adjustedEnd: DateTime,
   startBuffer: number,
   endBuffer: number,
   title: string,
   location: string,
   ministry: string,
   resources: string[],
   submitterId: number,
   submitter: string,
   contact: string,
   calendar: string,
   height: number,
   intersections: number,
   interval: number,
   left: number,
   idx: number,
} 

export default defineComponent({
  name: "EventCalendar.Components.DayView",
  components: {
    "a-btn": Button,
    "a-modal": Modal,
    "tcc-chip": Chip,
  },
  props: {
    calendars: Array,
    currentDate: DateTime
  },
  setup() {

  },
  data() {
    return {
      selected: {} as any,
      modal: false,
    }
  },
  computed: {
    /** The person currently authenticated */
    currentPerson(): Person | null {
      return store.state.currentPerson
    },
    daysCalendar(): Array<any> {
      if(this.calendars && this.currentDate) {
        let data = JSON.parse(JSON.stringify(this.calendars))
        //Filter to current events
        data.forEach((c: any) => {
          c.events = c.events.filter((e: any) => {
            return DateTime.fromISO(e.start).toFormat('yyyy-MM-dd') == this.currentDate?.toFormat('yyyy-MM-dd')
          })
          c.events.forEach((e: any) => {
            let start = DateTime.fromISO(e.start)
            if(e.startBuffer) {
              e.adjustedStart = start.minus({minutes: e.startBuffer})
            } else {
              e.adjustedStart = start
            }
            let end = DateTime.fromISO(e.end)
            if(e.endBuffer) {
              e.adjustedEnd = end.plus({minutes: e.endBuffer})
            } else {
              e.adjustedEnd = end
            }
            let interval = Interval.fromDateTimes(e.adjustedStart, e.adjustedEnd)
            e.duration = interval.toDuration('minutes').toObject().minutes
            e.height = interval.toDuration('minutes').toObject().minutes + "px"
            e.top = ((e.adjustedStart.hour * 60) + e.adjustedStart.minute ) + 'px'
            e.calendar = c.name
            e.calColor = c.color
            e.calBorder = c.border
          })
        })
        data = data.filter((c: any) => {
          return c.events.length > 0
        })
        //Save Sort Order
        data.forEach((c: any) => {
          c.events.sort(this.sortEvents)
        })
        data.sort((a: any, b: any) => {
          if(a.name < b.name) {
            return -1
          } else if(a.name > b.name) {
            return 1
          } 
          return 0
        })
        if(data.length > 0) {
          return data
        }
      }
      return [{name: 'default', events: []}]
    },
    selectedTimeFrame() {
      if(this.selected) {
        let start = DateTime.fromISO(this.selected.start)
        let end = DateTime.fromISO(this.selected.end)
        let duration = Interval.fromDateTimes(start, end).toDuration()
        let range = ""
        if(duration.hours > 1) {
          range = duration.hours + " hours"
        } else if(duration.hours == 1) {
          range = "1 hour"
        }
        let timeFrame =  this.selected.adjustedStart.toFormat("t") + " - " + this.selected.adjustedEnd.toFormat("t") 
        if((this.selected.startBuffer && this.selected.startBuffer > 0) || (this.selected.endBuffer && this.selected.endBuffer > 0)) {
          timeFrame += " (Event Time: " + start.toFormat("t") + " - " + end.toFormat("t") + ")"
        }
        return timeFrame
      }
    },
    relatedEvents() {
      if(this.selected) {
        let events = [] as any[]
        this.calendars?.forEach((c: any) => {
          c.events.forEach((e: any) => {
            if(e.parentId == this.selected.parentId && (e.id != this.selected.id || c.name != this.selected.calendar)) {
              let idx = events.map((event: any) => { return event.start }).indexOf(e.start)
              if(idx >= 0) {
                events[idx].events.push(e)
              } else {
                events.push({start: e.start, events: [ e ]})
              }
            }
          })
        })
        events.forEach((e: any) => {
          e.rooms = e.events.map((ev: any) => ev.location).join(", ")
        })
        return events
      }
      return []
    },
    sortedEvents() {
      let lastEventEnding = null
      let columns = [] as any[]
      let events = [] as any[] 
      this.daysCalendar.forEach((c: any) => events.push(...c.events))
      for(let i=0; i<events.length; i++) {
        let e = events[i]
        let interval = Interval.fromDateTimes(e.adjustedStart, e.adjustedEnd)
        if (lastEventEnding != null && e.adjustedStart >= lastEventEnding) {
            this.packEvents(columns)
            columns = []
            lastEventEnding = null
        }
        let placed = false
        for(let k=0; k<columns.length; k++) {
          let col = columns[k]
          let noIntersections = true
          for(let j=0; j<col.length; j++) {
            let intersection = interval.intersection(Interval.fromDateTimes(col[j].adjustedStart, col[j].adjustedEnd)) as Interval
            if(intersection != null) {
              noIntersections = false
            }
          }
          if(noIntersections) {
            col.push(e)
            placed = true
            break
          }
        }
        if (!placed) {
          columns.push([e])
        }
        if (lastEventEnding == null || e.adjustedEnd > lastEventEnding) {
          lastEventEnding = e.adjustedEnd
        }
      }
      if (columns.length > 0){
        this.packEvents(columns)
      }
      return columns
    },
  },
  methods: {
    getHourId(hour: number) {
      if(this.currentDate) {
        return 'day_' + this.currentDate.toFormat('dd') + '_hour_' + (hour - 1)
      }
    },
    openEvent(e: any) {
      this.$emit("openEvent", e)
    },
    getTimeFrame(event: any) {
      if(event) {
        let start = event.adjustedStart
        let end = event.adjustedEnd
        if(!start) {
          start = DateTime.fromISO(event.start)
          if(event.startBuffer && event.startBuffer > 0) {
            start = start.minus({minutes: event.startBuffer})
          }
        }
        if(!end) {
          end = DateTime.fromISO(event.end)
          if(event.endBuffer && event.endBuffer > 0) {
            end = end.plus({minutes: event.endBuffer})
          }
        }
        return `${start.toFormat("EEE, MMM, d")} ${start.toFormat("t")} - ${end.toFormat("t")}`
      }
      return ""
    },
    filterToEvent() {
      this.modal = false
      this.$emit('filterToEvent', this.selected.parentId)
      this.selected = {}
    },
    sortEvents(a: any, b: any) {
      if(a.calendar < b.calendar) {
        return -1
      } else if(a.calendar > b.calendar) {
        return 1
      } 
      if(a.adjustedStart.hour < b.adjustedStart.hour) {
        return -1
      } else if(a.adjustedStart.hour > b.adjustedStart.hour) {
        return 1
      } 
      if(a.adjustedStart.minute < b.adjustedStart.minute) {
        return -1
      } else if(a.adjustedStart.minute > b.adjustedStart.minute) {
        return 1
      } 
      if(a.adjustedEnd.hour < b.adjustedEnd.hour) {
        return -1
      } else if(a.adjustedEnd.hour > b.adjustedEnd.hour) {
        return 1
      } 
      if(a.adjustedEnd.minute < b.adjustedEnd.minute) {
        return -1
      } else if(a.adjustedEnd.minute > b.adjustedEnd.minute) {
        return 1
      } 
      if(a.location < b.location) {
        return -1
      } else if(a.location > b.location) {
        return 1
      } else {
        return 0
      }
    },
    getStyle(e: any) {
      return `position: absolute; top: ${e.top}; height: ${e.height}; left: ${(100*e.left)}%; width: ${(100/this.sortedEvents.length)}%; background-color: ${e.calColor.replaceAll('%2C', ',') }; border-color: ${e.calBorder.replaceAll('%2C', ',') };`
    },
    packEvents(columns: any[]) {
      let numColumns = columns.length
      let iColumn = 0
      columns.forEach((col: any) => {
        col.forEach((e: any) => {
          let colSpan = this.expandEvent(e, iColumn, columns)
          e.left = iColumn / numColumns
          e.right = (iColumn + colSpan) / numColumns
        })
        iColumn++
      })
    },
    expandEvent(e: any, iColumn: number, columns: any[]): number {
      let colSpan = 1
      let interval = Interval.fromDateTimes(e.adjustedStart, e.adjustedEnd)
      columns.slice(iColumn + 1).forEach((col: any) => {
        col.forEach((oe: any) => {
          let intersection = interval.intersection(Interval.fromDateTimes(oe.adjustedStart, oe.adjustedEnd)) as Interval
          if(intersection) {
            return colSpan
          }
        })
        colSpan++
      })
      return colSpan
    }
  },
  watch: {
    
  },
  mounted() {
    
  },
  updated() {
    
  },
  template: `
  <div style="position: relative;" :id="'event_container_' + currentDate.toFormat('dd')" class="event-container">
    <div class="tcc-hour" v-for="i in 24" :key="i" :id="getHourId(i)">
    </div>
    <template v-for="col in sortedEvents">
      <div v-for="e in col" class="tcc-event" :id="e.calendar+'_'+e.id" :style="getStyle(e)">
        <b>{{e.location}}</b> {{e.title}}
      </div>
    </template>
  </div>
  <a-modal v-if="modal" v-model:visible="modal" :closable="false" width="75%">
    <h2 class="text-center">{{selected.title}}</h2>
    <div>
      <i class="far fa-clock"></i> {{selectedTimeFrame}}
    </div>
    <div>
      {{selected.ministry}}: {{selected.submitter}}
    </div>
    <div v-if="selected.submitter != selected.contact">
      Event Contact: {{selected.contact}}
    </div>
    <div>
      <i class="fas fa-map-marker-alt"></i> {{selected.location}}
    </div>
    <div class="mt-2">
      Resources
      <div class="chip-group">
        <tcc-chip v-for="r in selected.resources" :disabled="true">
          <template v-if="r == 'Room'">
            <i class="mr-1 fas fa-door-open"></i> Physical Space
          </template>
          <template v-else-if="r == 'Catering'">
            <i class="mr-1 fas fa-utensils"></i> Catering
          </template>
          <template v-else-if="r == 'Childcare'">
            <i class="mr-1 fas fa-child"></i> Childcare
          </template>
          <template v-else-if="r == 'Childcare Catering'">
            <i class="mr-1 fas fa-pizza-slice"></i> Childcare Catering
          </template>
          <template v-else-if="r == 'Online Event'">
            <i class="mr-1 fas fa-child"></i> Zoom
          </template>
          <template v-else-if="r == 'Publicity'">
            <i class="mr-1 fas fa-bullhorn"></i> Publicity
          </template>
          <template v-else-if="r == 'Registration'">
            <i class="mr-1 fas fa-laptop"></i> Registration
          </template>
          <template v-else-if="r == 'Extra Resources'">
            <i class="mr-1 fas fa-cogs"></i> Ops Request
          </template>
          <template v-else-if="r == 'Web Calendar'">
            <i class="mr-1 fas fa-calendar"></i> Web Calendar
          </template>
          <template v-else-if="r == 'Production'">
            <i class="mr-1 fas fa-music"></i> Production
          </template>
          <template v-else>
            {{r}}
          </template>
        </tcc-chip>
      </div>
    </div>
    <template v-if="relatedEvents.length > 0">
      <div class="mt-2 font-weight-bold hover" data-toggle="collapse" href="#relatedCollapse" aria-expanded="false" aria-controls="relatedCollapse">
        Other Events in Request <i class="fa fa-chevron-down"></i>
      </div>
      <div class="collapse" id="relatedCollapse">
        <div v-for="e in relatedEvents">
          {{getTimeFrame(e.events[0])}} {{e.rooms}}
        </div>
      </div>
    </template>
    <template #footer>
      <a-btn shape="circle" type="accent" v-if="selected.submitterId == currentPerson.id">
        <i class="fa fa-pencil"></i>
      </a-btn>
      <a-btn shape="circle" type="primary" v-if="relatedEvents.length > 0" @click="filterToEvent">
        <i class="fa fa-filter"></i>
      </a-btn>
      <a-btn type="grey" @click="modal = false; selected = {};">Close</a-btn>
    </template>
  </a-modal>
  <v-style>
    .tcc-event {
      padding: 4px; 
      width: 100%;
      border-radius: 4px;
      overflow: hidden;
      border-left: 6px solid;
    }
    .tcc-hour {
      height: 60px;
      border-bottom: 1px solid grey;
      display: flex;
    }
    .calendar-col {
      /*width: 100%;*/
      display: flex;
    }
  </v-style>
`
});
