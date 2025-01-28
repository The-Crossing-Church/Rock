import { defineComponent, PropType } from "vue"
import { DateTime, Interval } from "luxon"
import DayView from "./day"

export default defineComponent({
  name: "EventCalendar.Components.WeekView",
  components: {
    "tcc-day": DayView,
  },
  props: {
    calendars: Array,
    currentDate: DateTime,
    formUrl: String,
    dashboardUrl: String,
    isAdmin: Boolean,
    currentPersonId: Number
  },
  setup() {

  },
  data() {
    return {
      
    }
  },
  computed: {
    calData() {
      if(this.currentDate) {
        if(this.currentDate.weekday == 7) {
          let week = [
            {date: this.currentDate.startOf('week').plus({weeks: 1}).minus({days: 1}) },
            {date: this.currentDate.startOf('week').plus({weeks: 1}) },
            {date: this.currentDate.startOf('week').plus({weeks: 1}).plus({days: 1}) },
            {date: this.currentDate.startOf('week').plus({weeks: 1}).plus({days: 2}) },
            {date: this.currentDate.startOf('week').plus({weeks: 1}).plus({days: 3}) },
            {date: this.currentDate.startOf('week').plus({weeks: 1}).plus({days: 4}) },
            {date: this.currentDate.startOf('week').plus({weeks: 1}).plus({days: 5}) }
          ]
          return week
        } else {
          let week = [
            {date: this.currentDate.startOf('week').minus({days: 1}) },
            {date: this.currentDate.startOf('week') },
            {date: this.currentDate.startOf('week').plus({days: 1}) },
            {date: this.currentDate.startOf('week').plus({days: 2}) },
            {date: this.currentDate.startOf('week').plus({days: 3}) },
            {date: this.currentDate.startOf('week').plus({days: 4}) },
            {date: this.currentDate.startOf('week').plus({days: 5}) }
          ]
          return week
        }
      }
    },
  },
  methods: {
    getTime(hour: number) {
      if(this.currentDate) {
        let dt = DateTime.fromObject({year: this.currentDate.year, month: this.currentDate.month, day: this.currentDate.day, hour: hour - 1, minute: 0, second: 0})
        return dt.toFormat('h') + ' ' + ((hour - 1) > 11 ? 'PM' : 'AM')
      }
    },
    getDayId(day: DateTime) {
      return 'day_' + day.toFormat('dd')
    },
    selectDay(day: DateTime) {
      this.$emit('selectDay', day)
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
  <div class="tcc-cal-week">
    <div style="width: 50px"></div>
    <div class="tcc-cal-day" v-for="(day, didx) in calData" @click="selectDay(day.date)">
      <div class="tcc-day-header">
        {{day.date.toFormat('EEE')}} {{day.date.toFormat('dd')}}
      </div>
    </div>
  </div>
  <div class="window">
    <div style="display: flex;">
      <div class="tcc-time-of-day">
        <div class="tcc-hour" v-for="i in 24" :idx="i">
          {{getTime(i)}}
        </div>
      </div>
      <div class="tcc-cal-wrapper">
        <div class="tcc-cal-body">
          <div class="tcc-cal-week">
            <div class="tcc-cal-day" v-for="(day, didx) in calData" :id="getDayId(day.date)" :key="didx" @click="selectDay(day.date)">
              <tcc-day
                :calendars="calendars"
                :currentDate="day.date"
                :formUrl="formUrl"
                :dashboardUrl="dashboardUrl"
                :isAdmin="isAdmin"
                :currentPersonId="currentPersonId"
                v-on:filterToEvent="filterToEvent"
              ></tcc-day>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
  <v-style>
    .tcc-cal-day:hover {
      background-color: #EEEFEF;
    }
    .tcc-cal-day, .tcc-cal-header-day {
      width: 14.2% !important;
    }
  </v-style>
`
});
