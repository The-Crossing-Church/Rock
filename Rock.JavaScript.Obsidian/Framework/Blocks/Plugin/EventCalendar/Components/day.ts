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
      modal: false
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
          let numEvents = c.events.length
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
            e.calendar = c.name
            e.numEvents = numEvents
          })
        })
        data = data.filter((c: any) => {
          return c.events.length > 0
        })
        //Save Intersection Data
        let events = data.map((c: any) => { return c.events }).flat()
        data.forEach((c: any) => {
          c.events.forEach((e: any) => {
            let interval = Interval.fromDateTimes(e.adjustedStart, e.adjustedEnd)
            let intersectingEvents = events.filter((oe: any) => {
              if(!(e.location == oe.location && e.id == oe.id)) {
                let otherInterval = Interval.fromDateTimes(oe.adjustedStart, oe.adjustedEnd)
                return interval.intersection(otherInterval) != null
              }
              return false
            }).sort((a: any, b: any) => {
              let aInt = Interval.fromDateTimes(a.adjustedStart, a.adjustedEnd)
              let bInt = Interval.fromDateTimes(b.adjustedStart, b.adjustedEnd)
              if(aInt.count('minutes') < bInt.count('minutes')) {
                return -1
              } else if(aInt.count('minutes') > bInt.count('minutes')) {
                return 1
              }
              return 0
            })
            let intervals = [] as any[]
            intersectingEvents.forEach((oe: any) => {
              let intersection = interval.intersection(Interval.fromDateTimes(oe.adjustedStart, oe.adjustedEnd)) as Interval
              let hasMatch = false
              intervals.forEach((i: any) => {
                let int = i.int.intersection(intersection)
                if(int) {
                  i.ct++
                }
              })
              if(!hasMatch) {
                intervals.push({int: intersection, ct: 1})
              }
            })
            if(intervals && intervals.length > 0) {
              intervals = intervals.sort((a: any, b: any) => {
                if(a.ct > b.ct ) {
                  return -1
                } else if( a.ct < b.ct) {
                  return 1
                }
                return 0
              })
              e.intersections = intervals[0].ct
              e.totalIntersections = intersectingEvents.length
              e.interval = intervals[0].int
            }
          })
        })
        //Save Sort Order
        data.forEach((c: any) => {
          c.events.forEach((e: any) => {
            let interval = Interval.fromDateTimes(e.adjustedStart, e.adjustedEnd)
            let intersectingEvents = events.filter((oe: any) => {
              if(!(e.location == oe.location && e.id == oe.id)) {
                let otherInterval = Interval.fromDateTimes(oe.adjustedStart, oe.adjustedEnd)
                if(interval && otherInterval) {
                  return interval.intersection(otherInterval) != null
                }
              }
              return false
            })
            let eventsAtInterval = intersectingEvents.filter((oe: any) => {
               return Interval.fromDateTimes(oe.adjustedStart, oe.adjustedEnd).intersection(e.interval) != null
            })
            eventsAtInterval.push(e)
            eventsAtInterval.sort(this.sortEvents)
            e.idx = eventsAtInterval.indexOf(e)
            e.left = 0
            for(let i=0; i<e.idx; i++) {
              e.left += 100 / (eventsAtInterval[i].intersections + 1)
            }
            if(e.idx > 0) {
              e.preceedingEvent = { calendar: eventsAtInterval[e.idx-1].calendar, id: eventsAtInterval[e.idx-1].id }
            } else {
              e.preceedingEvent = null
            }
            if(e.idx < (eventsAtInterval.length - 1)) {
              e.followingEvent = { calendar: eventsAtInterval[e.idx+1].calendar, id: eventsAtInterval[e.idx+1].id }
            } else {
              e.followingEvent = null
            }
          })
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
    }
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
    getStyle(event: any, calendar: any) {
      let style = 'position: absolute; background-color: ' + calendar.color.replaceAll('%2C', ',') + '; border-color: ' + calendar.border.replaceAll('%2C', ',') + '; height: ' + event.height + ';'
      style += ' top: ' + ((event.adjustedStart.hour * 60) + event.adjustedStart.minute ) + 'px;'
      let percentage = (100/(event.intersections + 1))
      if(event.preceedingEvent) {
        this.daysCalendar.forEach((c: any) => {
          if(c.name == event.preceedingEvent.calendar) {
            c.events.forEach((e: any) => {
              if(e.id == event.preceedingEvent.id) {
                let adjustedLeft = (e.left + (100/(e.intersections + 1)))
                if(event.left < adjustedLeft) {
                  if((adjustedLeft + percentage) < 100) {
                    event.left = (e.left + (100/(e.intersections + 1)))
                  } 
                }
              }
            })
          }
        })
      }
      if(event.followingEvent) {
        this.daysCalendar.forEach((c: any) => {
          if(c.name == event.followingEvent.calendar) {
            c.events.forEach((e: any) => {
              if(e.id == event.followingEvent.id) {
                let currentEnd = event.left + percentage
                if(currentEnd > e.left) {
                  percentage = (100/(e.intersections + 1))
                }
              }
            })
          }
        })
      }
      style += " width: "  + percentage + "%; left: " + event.left + "%;"
      // style += " width: "  + (100/overlaps.length) + "%; left: " + ((100/overlaps.length)*idx) + "%;"
      //style += " width: "  + (100/(event.intersections + 1)) + "%; left: " + ((100/overlaps.length)*idx) + "%;"
      return style
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
      if(a.intersections > b.intersections) {
        return -1
      } else if(a.intersections < b.intersections) {
        return 1
      }
      if(a.duration > b.duration) {
        return -1
      } else if(a.duration < b.duration) {
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
      if(a.location < b.location) {
        return -1
      } else if(a.location > b.location) {
        return 1
      } else {
        return 0
      }
    }
  },
  watch: {
    
  },
  mounted() {
    console.log('mounted')
    //Go through all events for day and move ones that are overlapping
    let events = [] as any[]
    document.querySelectorAll(`#day_${this.currentDate?.toFormat("dd")} .tcc-event`).forEach((e: any) => {
      let obj = { 
        eventId: e.dataset.eventId, 
        detailId: e.dataset.detailId, 
        top: parseInt(e.style.top.replace('px', '')), 
        left: parseInt(e.style.left.replace("%", "")),
        right: parseInt(e.style.width.replace("%", "")),
        bottom: parseInt(e.style.height.replace("px", "")),
        calendar: e.dataset.calendar, 
        el: e 
      }
      obj.right += obj.left
      obj.bottom += obj.top
      events.push(obj)
    })
    events.sort((a: any, b: any) => {
      if(a.top < b.top) {
        return -1
      } else if(a.top > b.top) {
        return 1
      }
      return 0
    })
    events.forEach((e: any, idx: number) => {
      let otherEvents = events.filter((oe: any) => { return !(oe.eventId == e.eventId && oe.detailId == e.detailId && oe.calendar == e.calendar) })
      otherEvents.forEach((oe: any) => {
        //Figure out if events overlap horizontally 
        if((e.left > oe.left && e.left < oe.right) || e.left == oe.left || e.right == oe.right || (e.right > oe.left && e.right < oe.right)){
          //Figure out if events overlap vertically
          if((e.top > oe.top && e.top < oe.bottom) || e.top == oe.top || e.bottom == oe.bottom || (e.bottom > oe.top && e.bottom < oe.bottom)){
            console.log('overlaps', e, oe)
            //Try to see if e can move left
            //Find events that are during this time slot 
            let eventsDuringInterval = otherEvents.filter((edi: any) => {
              return (edi.top >= e.top && edi.top < e.bottom) || (edi.bottom > e.top && edi.bottom <= e.bottom) || (edi.top <= e.top && edi.bottom >= e.bottom)
            })
            eventsDuringInterval = eventsDuringInterval.filter((edi: any) => {
              return edi.left < e.right
            })
            eventsDuringInterval = eventsDuringInterval.sort((a: any, b: any) => {
              if(a.right > b.right) {
                return -1
              } else if (a.right < b.right) {
                return 1
              }
              return 0
            })
            //See if the farthest right item, is further left that the current item's left and update it 
            // for(let i = 0; i < eventsDuringInterval.length; i++) {
            //   if(eventsDuringInterval[i].right < e.left) {
                console.log('Adjusting...')
                console.log(e.el, eventsDuringInterval[0].el, 0)
                // e.left = parseFloat(eventsDuringInterval[i].el.style.width.replace("%", "")) + parseFloat(eventsDuringInterval[i].el.style.left.replace("%", ""))
                // e.right = e.left + parseFloat(e.el.style.width.replace("%", ""))
                // e.el.style.left = e.left + "%"
              //   break;
              // }
            // }
          }
        }
      })
    })
    console.log(events)
  },
  updated() {
    console.log('updated')
    //Go through all events for day and move ones that are overlapping
  },
  template: `
  <div style="position: relative;">
    <div class="tcc-hour" v-for="i in 24" :key="i" :id="getHourId(i)">
    </div>
    <template v-for="c in daysCalendar">
      <template v-for="e in c.events">
        <div class="tcc-event" :style="getStyle(e, c)" :data-event-id="e.id" :data-detail-id="e.parentId" :data-calendar="e.calendar" @click="selected = e; modal = true;">
          <strong>{{e.location}}</strong> {{e.title}}
        </div>
      </template>
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
