import { defineComponent, PropType } from "vue"
import { DateTime, Interval } from "luxon"
import { Modal, Button } from "ant-design-vue"
import { useStore } from "../../../../Store/index"
import { Person } from "../../../../ViewModels"
import Chip from "../../EventForm/Components/chip"

const store = useStore()

export default defineComponent({
  name: "EventCalendar.Components.Event",
  components: {
    "a-btn": Button,
    "a-modal": Modal,
    "tcc-chip": Chip,
  },
  props: {
    calendars: Array,
    event: Object,
    cols: Number
  },
  setup() {

  },
  data() {
    return {
      modal: false
    }
  },
  computed: {
    /** The person currently authenticated */
    currentPerson(): Person | null {
      return store.state.currentPerson
    },
    selectedTimeFrame() {
      if(this.event) {
        let start = DateTime.fromISO(this.event.start)
        let end = DateTime.fromISO(this.event.end)
        let duration = Interval.fromDateTimes(start, end).toDuration()
        let range = ""
        if(duration.hours > 1) {
          range = duration.hours + " hours"
        } else if(duration.hours == 1) {
          range = "1 hour"
        }
        let timeFrame =  this.event.adjustedStart.toFormat("t") + " - " + this.event.adjustedEnd.toFormat("t") 
        if((this.event.startBuffer && this.event.startBuffer > 0) || (this.event.endBuffer && this.event.endBuffer > 0)) {
          timeFrame += " (Event Time: " + start.toFormat("t") + " - " + end.toFormat("t") + ")"
        }
        return timeFrame
      }
    },
    relatedEvents() {
      if(this.event) {
        let events = [] as any[]
        this.calendars?.forEach((c: any) => {
          c.events.forEach((e: any) => {
            if(e.parentId == this.event?.parentId && (e.id != this.event?.id || c.name != this.event?.calendar || e.adjustedStart != this.event?.adjustedStart)) {
              let idx = events.map((e: any) => { return e.start }).indexOf(e.start)
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
  },
  methods: {
    openEvent(e: any) {
      this.$emit("openEvent", e)
    },
    filterToEvent() {
      this.modal = false
      this.$emit('filterToEvent', this.event?.parentId)
    },
    getStyle(e: any) {
      return `position: absolute; top: ${e.top}; height: ${e.height}; left: ${(100*e.left)}%; width: ${(100/(this.cols ? this.cols : 1 ))}%; background-color: ${e.calColor.replaceAll('%2C', ',') }; border-color: ${e.calBorder.replaceAll('%2C', ',') };`
    },
    getTimeFrame(relatedEvent: any) {
      if(relatedEvent) {
        let start = relatedEvent.adjustedStart
        let end = relatedEvent.adjustedEnd
        if(!start) {
          start = DateTime.fromISO(relatedEvent.start)
          if(relatedEvent.startBuffer && relatedEvent.startBuffer > 0) {
            start = start.minus({minutes: relatedEvent.startBuffer})
          }
        }
        if(!end) {
          end = DateTime.fromISO(relatedEvent.end)
          if(relatedEvent.endBuffer && relatedEvent.endBuffer > 0) {
            end = end.plus({minutes: relatedEvent.endBuffer})
          }
        }
        return `${start.toFormat("EEE, MMM, d")} ${start.toFormat("t")} - ${end.toFormat("t")}`
      }
      return ""
    },
  },
  watch: {
    
  },
  mounted() {
    
  },
  updated() {
    
  },
  template: `
  <div class="tcc-event" :id="event.calendar+'_'+event.id" :style="getStyle(event)" @click="modal = true">
    <b>{{event.location}}</b> {{event.title}}
  </div>
  <a-modal v-if="modal" v-model:visible="modal" :closable="false" width="75%">
    <h2 class="text-center">{{event.title}}</h2>
    <div>
      <i class="far fa-clock"></i> {{selectedTimeFrame}}
    </div>
    <div>
      {{event.ministry}}: {{event.submitter}}
    </div>
    <div v-if="event.submitter != event.contact">
      Event Contact: {{event.contact}}
    </div>
    <div>
      <i class="fas fa-map-marker-alt"></i> {{event.location}}
    </div>
    <div class="mt-2">
      Resources
      <div class="chip-group">
        <tcc-chip v-for="r in event.resources" :disabled="true">
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
      <a-btn shape="circle" type="accent" v-if="event.submitterId == currentPerson.id">
        <i class="fa fa-pencil"></i>
      </a-btn>
      <a-btn shape="circle" type="primary" v-if="relatedEvents.length > 0" @click="filterToEvent">
        <i class="fa fa-filter"></i>
      </a-btn>
      <a-btn type="grey" @click="modal = false;">Close</a-btn>
    </template>
  </a-modal>
`
});
