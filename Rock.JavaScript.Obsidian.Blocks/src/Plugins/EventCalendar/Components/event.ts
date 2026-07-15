import { defineComponent, PropType } from "vue"
import { DateTime, Interval } from "luxon"
import { useStore } from "@Obsidian/PageState"
import { CurrentPersonBag } from "@Obsidian/ViewModels/Crm/currentPersonBag"
import Modal from "@Obsidian/Controls/modal.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import Chip from "../../EventForm/Components/chip"

const store = useStore()

export default defineComponent({
  name: "EventCalendar.Components.Event",
  components: {
    "rck-btn": RockButton,
    "rck-modal": Modal,
    "tcc-chip": Chip,
  },
  props: {
    calendars: Array,
    event: Object,
    cols: Number,
    formUrl: String,
    dashboardUrl: String,
    isAdmin: Boolean,
    currentPersonId: Number
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
    currentPerson(): CurrentPersonBag | null {
      return store.state.currentPerson
    },
    selectedTimeFrame() {
      if(this.event) {
        let start = DateTime.fromISO(this.event.start)
        let end = DateTime.fromISO(this.event.end)
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
    uniqueid() {
      if(this.event) {
        return this.event.id + '_' + 
          this.event.location.replaceAll(' ', '_')
                             .replaceAll(',', '')
                             .replaceAll('(', '').replaceAll(')', '')
                             .replaceAll('&', 'and')
                             .replaceAll(':', '').replaceAll('|', '')
                             .replaceAll('*', '')
      }
      return ''
    }
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
        let start = (typeof relatedEvent.adjustedStart === 'string') ? DateTime.fromISO(relatedEvent.adjustedStart) : relatedEvent.adjustedStart
        let end = (typeof relatedEvent.adjustedEnd === 'string') ? DateTime.fromISO(relatedEvent.adjustedEnd) : relatedEvent.adjustedEnd
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
    openModal(e: any) {
      // e.preventDefault()
      this.modal = true
    },
    openInForm() {
      window.location.href = this.formUrl + "?Id=" + this.event?.parentId
    },
    openInDash() {
      window.location.href = this.dashboardUrl + "?Id=" + this.event?.parentId
    },
    toggleRelatedEvents(element: string) {
      $(element).collapse('toggle')
    }
  },
  watch: {
    
  },
  mounted() {
    // console.log(store.state)
  },
  updated() {
    
  },
  template: `
  <div class="tcc-event" :id="event.calendar+'_'+event.id" :style="getStyle(event)" @click.stop="openModal">
    <b>{{event.location}}</b> {{event.title}}
  </div>
  <rck-modal v-if="modal" v-model="modal" width="75%" isCloseButtonHidden cancelText="" clickBackdropToClose modalWrapperClasses="modal-no-header">
    <h2 class="text-center">{{event.title}}</h2>
    <div>
      <i class="ti ti-clock"></i> {{selectedTimeFrame}}
    </div>
    <div>
      {{event.ministry}}: {{event.createdByPersonName}}
    </div>
    <div v-if="event.createdByPersonName != event.contact">
      Event Contact: {{event.contact}}
    </div>
    <div>
      <i class="fas fa-map-marker-alt"></i> {{event.location}}
    </div>
    <div class="mt-2">
      Resources
      <div class="chip-group">
        <tcc-chip v-if="event.needsSpace" :disabled="true">
          <i class="mr-1 fas fa-door-open"></i> Physical Space
        </tcc-chip>
        <tcc-chip v-if="event.needsCatering" :disabled="true">
          <i class="mr-1 fas fa-utensils"></i> Catering
        </tcc-chip>
        <tcc-chip v-if="event.needsChildcare" :disabled="true">
          <i class="mr-1 fas fa-child"></i> Childcare
        </tcc-chip>
        <tcc-chip v-if="event.needsChildcareCatering" :disabled="true">
          <i class="mr-1 fas fa-pizza-slice"></i> Childcare Catering
        </tcc-chip>
        <tcc-chip v-if="event.needsOnline" :disabled="true">
          <i class="mr-1 fas fa-child"></i> Zoom
        </tcc-chip>
        <tcc-chip v-if="event.needsPublicity" :disabled="true">
          <i class="mr-1 fas fa-bullhorn"></i> Publicity
        </tcc-chip>
        <tcc-chip v-if="event.needsRegistration" :disabled="true">
          <i class="mr-1 fas fa-laptop"></i> Registration
        </tcc-chip>
        <tcc-chip v-if="event.needsOps" :disabled="true">
          <i class="mr-1 fas fa-cogs"></i> Ops Request
        </tcc-chip>
        <tcc-chip v-if="event.needsCalendar" :disabled="true">
          <i class="mr-1 fas fa-calendar"></i> Web Calendar
        </tcc-chip>
        <tcc-chip v-if="event.needsProduction" :disabled="true">
          <i class="mr-1 fas fa-music"></i> Production
        </tcc-chip>
      </div>
    </div>
    <template v-if="relatedEvents.length > 0">
      <div class="mt-2 font-weight-bold hover" @click="toggleRelatedEvents('#relatedCollapse_' + uniqueid)" data-toggle="collapse" :data-target="'#relatedCollapse_' + uniqueid" aria-expanded="false" :aria-controls="'relatedCollapse_' + uniqueid">
        Other Events in Request <i class="fa fa-chevron-down"></i>
      </div>
      <div class="collapse" :id="'relatedCollapse_' + uniqueid">
        <div v-for="e in relatedEvents">
          {{getTimeFrame(e.events[0])}} {{e.rooms}}
        </div>
      </div>
    </template>
    <template #customButtons>
      <rck-btn class="btn-circle" btnType="accent" v-if="event.createdByPersonId == currentPersonId || event.modifiedByPersonId == currentPersonId || isAdmin" @click="openInForm">
        <i class="fa fa-pencil"></i>
      </rck-btn>
      <rck-btn class="btn-circle" btnType="accent" v-if="event.createdByPersonId == currentPersonId || event.modifiedByPersonId == currentPersonId || isAdmin" @click="openInDash">
        <i class="fas fa-external-link-alt"></i>
      </rck-btn>
      <rck-btn class="btn-circle" btnType="primary" v-if="relatedEvents.length > 0" @click="filterToEvent">
        <i class="fa fa-filter"></i>
      </rck-btn>
      <rck-btn type="grey" @click="modal = false;">Close</rck-btn>
    </template>
  </rck-modal>
  <v-style>
  [aria-expanded="false"] .fa-chevron-down {
    transition: transform 0.4s ease;
    -webkit-transition: transform 0.4s ease;
    transform: rotate(0deg);
  }
  [aria-expanded="true"] .fa-chevron-down {
    transition: transform 0.4s ease;
    -webkit-transition: transform 0.4s ease;
    transform: rotate(180deg);
  }
  </v-style>
`
});
