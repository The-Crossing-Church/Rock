import { defineComponent, PropType } from "vue"
import { DateTime, Interval } from "luxon"
import { Modal } from "ant-design-vue"

export default defineComponent({
  name: "EventCalendar.Components.DayView",
  components: {
    "a-modal": Modal
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
            e.height = interval.toDuration('minutes').toObject().minutes + "px"
            e.calendar = c.name
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
              e.interval = intervals[0].int
            }
            //e.intersections = intersectingEvents.map((oe: any) => { return { id: oe.id, calendar: oe.calendar, adjustedStart: oe.adjustedStart, adjustedEnd: oe.adjustedEnd, location: oe.location, title: oe.title } })
          })
        })
        //Save Sort Order
        data.forEach((c: any) => {
          c.events.forEach((e: any) => {
            let interval = Interval.fromDateTimes(e.adjustedStart, e.adjustedEnd)
            let intersectingEvents = events.filter((oe: any) => {
              if(!(e.location == oe.location && e.id == oe.id)) {
                let otherInterval = Interval.fromDateTimes(oe.adjustedStart, oe.adjustedEnd)
                return interval.intersection(otherInterval) != null
              }
              return false
            })
            let eventsAtInterval = intersectingEvents.filter((oe: any) => {
              return Interval.fromDateTimes(oe.adjustedStart, oe.adjustedEnd).intersection(e.interval) != null
            })
            eventsAtInterval.push(e)
            eventsAtInterval.sort((a: any, b: any) => {
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
            })
            e.idx = eventsAtInterval.indexOf(e)
            e.left = 0
            for(let i=0; i<e.idx; i++) {
              e.left += 100 / (eventsAtInterval[i].intersections + 1)
            }
          })
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
        console.log(duration)
        let range = ""
        if(duration.hours > 1) {
          range = duration.hours + " hours"
        } else if(duration.hours == 1) {
          range = "1 hour"
        }
        let timeFrame = start.toFormat("t") + " - " + end.toFormat("t") 
        if((this.selected.startBuffer && this.selected.startBuffer > 0) || (this.selected.endBuffer && this.selected.endBuffer > 0)) {
          timeFrame += " (Booked " + this.selected.adjustedStart.toFormat("t") + " - " + this.selected.adjustedEnd.toFormat("t") + ")"
        }
        return timeFrame
      }
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
    getStyle(event: any, calendar: any) {
      let style = 'position: absolute; background-color: ' + calendar.color.replaceAll('%2C', ',') + '; border-color: ' + calendar.border.replaceAll('%2C', ',') + '; height: ' + event.height + ';'
      style += ' top: ' + ((event.adjustedStart.hour * 60) + event.adjustedStart.minute ) + 'px;'
      // let currentInterval = Interval.fromDateTimes(event.adjustedStart, event.adjustedEnd)
      // let otherEvents = this.daysCalendar.map((c: any) => {
      //   return c.events.filter((e: any) => {
      //     return !(e.location == event.location && e.id == event.id)
      //   })
      // })
      // let overlaps = [] as any[]
      // otherEvents.forEach((arr: any) => {
      //   arr.forEach((e: any) => {
      //     let eventInterval = Interval.fromDateTimes(e.adjustedStart, e.adjustedEnd)
      //     let intersection = currentInterval.intersection(eventInterval)
      //     if(intersection) {
      //       overlaps.push(e)
      //     }
      //   })
      // })
      // overlaps.push(event)
      // overlaps.sort((a: any, b: any) => {
      //   if(a.calendar < b.calendar) {
      //     return -1
      //   } else if(a.calendar > b.calendar) {
      //     return 1
      //   } 
      //   if(a.intersections > b.intersections) {
      //     return -1
      //   } else if(a.intersections < b.intersections) {
      //     return 1
      //   }
      //   if(a.adjustedStart.hour < b.adjustedStart.hour) {
      //     return -1
      //   } else if(a.adjustedStart.hour > b.adjustedStart.hour) {
      //     return 1
      //   } 
      //   if(a.adjustedStart.minute < b.adjustedStart.minute) {
      //     return -1
      //   } else if(a.adjustedStart.minute > b.adjustedStart.minute) {
      //     return 1
      //   } 
      //   if(a.location < b.location) {
      //     return -1
      //   } else if(a.location > b.location) {
      //     return 1
      //   } else {
      //     return 0
      //   }
      // })
      // let idx = overlaps.indexOf(event)
      console.log('START EVENT')
      console.log(event.location + " " + event.title)
      console.log("Actual Start: " + event.start + " " + DateTime.fromISO(event.start).toFormat("yyyy-MM-dd HH:mm"))
      console.log("Actual End: " + event.end+ " " + DateTime.fromISO(event.end).toFormat("yyyy-MM-dd HH:mm"))
      console.log("Start Buffer: " + event.startBuffer)
      console.log("End Buffer: " + event.endBuffer)
      console.log("Adjusted Start: " + event.adjustedStart.toFormat("yyyy-MM-dd HH:mm"))
      console.log("Adjusted End: " + event.adjustedEnd.toFormat("yyyy-MM-dd HH:mm"))
      let percentage = (100/(event.intersections + 1))
      //TODO: instead of left being percentage*idx it should be a sum of each preious event in the list percentage so we save that percentage info, but then the value used for left is a sum
      console.log(percentage + "%", (percentage*event.idx) + "%")
      console.log("left: " + event.left + "%")
      style += " width: "  + percentage + "%; left: " + event.left + "%;"
      // style += " width: "  + (100/overlaps.length) + "%; left: " + ((100/overlaps.length)*idx) + "%;"
      //style += " width: "  + (100/(event.intersections + 1)) + "%; left: " + ((100/overlaps.length)*idx) + "%;"
      return style
    }
  },
  watch: {
    
  },
  mounted() {
    
  },
  template: `
  <div style="position: relative;">
    <div class="tcc-hour" v-for="i in 24" :key="i" :id="getHourId(i)">
    </div>
    <template v-for="c in daysCalendar">
      <template v-for="e in c.events">
        <div class="tcc-event" :style="getStyle(e, c)" :event-id="e.id" :detial-id="e.parentId" @click="selected = e; modal = true;">
          <strong>{{e.location}}</strong> {{e.title}}
        </div>
      </template>
    </template>
  </div>
  <a-modal v-if="modal" v-model:visible="modal" :closable="false">
    <h2 class="text-center">{{selected.title}}</h2>
    <i class="far fa-clock"></i> {{selectedTimeFrame}} <br/>
    <div class="row">
      <div class="col col-xs-12 col-md-6">
        {{selected.ministry}}: {{selected.submitter}}
      </div>
      <div class="col col-xs-12 col-md-6" v-if="selected.submitter != selected.contact">
        Event Contact: {{selected.contact}}
      </div>
    </div>
    <i class="fas fa-map-marker-alt"></i> {{selected.location}}
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
