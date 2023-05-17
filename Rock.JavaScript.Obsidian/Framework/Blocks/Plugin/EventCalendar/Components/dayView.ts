import { defineComponent, provide } from "vue"
import { useStore } from "../../../../Store/index"
import { DateTime } from "luxon"
import Day from "./day"


const store = useStore()

export default defineComponent({
  name: "EventCalendar.DayView",
  components: {
    "tcc-day": Day
  },
  setup() {
    
  },
  props: {
    calendars: Array,
    currentDate: DateTime
  },
  data() {
      return {
        
      };
  },
  computed: {
    
  },
  methods: {
    getTime(hour: number) {
      if(this.currentDate) {
        let dt = DateTime.fromObject({year: this.currentDate.year, month: this.currentDate.month, day: this.currentDate.day, hour: hour - 1, minute: 0, second: 0})
        return dt.toFormat('h') + ' ' + ((hour - 1) > 11 ? 'PM' : 'AM')
      }
    },
    filterToEvent(parent: number) {
      this.$emit('filterToEvent', parent)
    }
  },
  watch: {
    
  },
  mounted() {
    let w = document.querySelector('.window')
    if(w) {
      w.scrollTo(0, (60*7)) //7 am
    }
  },
  template: `
  <div class="window">
    <div style="display: flex;">
      <div class="tcc-time-of-day day">
        <div class="tcc-hour" v-for="i in 24" :idx="i">
          {{getTime(i)}}
        </div>
      </div>
      <div class="tcc-cal-wrapper">
        <div class="tcc-cal-body">
          <div class="tcc-cal-week">
            <div class="tcc-cal-day" :id="'day_' + currentDate.toFormat('dd')">
              <div class="tcc-day-header">
                {{currentDate.toFormat('EEE')}} {{currentDate.toFormat('dd')}}
              </div>
              <tcc-day
                :calendars="calendars"
                :currentDate="currentDate"
                v-on:filterToEvent="filterToEvent"
              ></tcc-day>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <v-style>
    .tcc-time-of-day.day {
      margin-top: 30px;
    }
  </v-style>
`
})