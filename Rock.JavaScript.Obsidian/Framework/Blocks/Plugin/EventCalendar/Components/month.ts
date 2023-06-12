import { defineComponent, PropType } from "vue"
import { DateTime, Duration } from "luxon"
import { Button, Dropdown, Menu } from "ant-design-vue"

const { MenuItem } = Menu

export default defineComponent({
  name: "EventCalendar.Components.MonthView",
  components: {
    "a-btn": Button,
    "a-dropdown": Dropdown,
    "a-menu": Menu,
    "a-menu-item": MenuItem
  },
  props: {
    calendars: Array,
    currentDate: DateTime
  },
  setup() {

  },
  data() {
    return {
      currentMonth: 0,
      currentYear: 0
    }
  },
  computed: {
    displayMonth() {
      return DateTime.fromObject({ day: 1, month: this.currentMonth, year: this.currentYear })
    },
    calData() {
      let weeks = []
      if (this.displayMonth && this.displayMonth.daysInMonth) {
        let week = []
        for (let i = 0; i < this.displayMonth.daysInMonth; i++) {
          let span = Duration.fromObject({ days: i })
          let date = this.displayMonth.plus(span)
          let dow = date.weekdayShort
          if (i == 0 && dow != 'Sun') {
            //Pad our first week with last month
            let numDays = date.weekday
            for (let k = numDays; k > 0; k--) {
              let negSpan = Duration.fromObject({ days: k })
              week.push(date.minus(negSpan))
            }
          }
          if (dow == 'Sun' && i > 0) {
            //start a new week
            weeks.push(week)
            week = []
          }
          week.push(date)
        }
        let remainingDays = 7 - week.length
        for (let i = 0; i < remainingDays; i++) {
          //Pad out last week with next month
          let days = i + this.displayMonth.daysInMonth
          let span = Duration.fromObject({ days: days })
          let date = this.displayMonth.plus(span)
          week.push(date)
        }
        weeks.push(week)
      }
      return weeks
    },
  },
  methods: {
    getClassName(day: DateTime) {
      let className = "tcc-cal-day"
      if(day.month != this.displayMonth.month) {
        className += " diff-month"
      }
      if (day.month != this.displayMonth.month) {
        className += " disabled"
      } 
      return className
    },
    getMonthPickerClassName(month: number) {
      let className = "tcc-month-picker-item"
      let date = DateTime.fromObject({ day: 1, month: month, year: this.currentYear })
      if (month == this.currentMonth) {
        className += " selected"
      }
      return className
    },
    selectMonth(month: number) {
      this.currentMonth = month
    },
    eventsOnDay(events: Array<any>, date: DateTime) {
      let evts = JSON.parse(JSON.stringify(events))
      evts = evts.filter((e: any) => {
        return DateTime.fromISO(e.start).day == date.day && date.month == DateTime.fromISO(e.start).month
      })
      return evts
    },
    selectWeek(day: DateTime) {
      this.$emit('selectWeek', day)
    },
  },
  watch: {
    calendars: {
      handler (val) {

      },
      deep: true
    },
    currentDate(val) {
      this.currentMonth = val.month
      this.currentYear = val.year
    }
  },
  mounted() {
    if(this.currentDate) {
      this.currentMonth = this.currentDate.month
      this.currentYear = this.currentDate.year
    }
  },
  template: `
  <div class="tcc-cal-wrapper">
    <div class="tcc-cal-header">
    </div>
    <div class="tcc-cal-body">
      <div class="tcc-cal-week">
          <div class="tcc-cal-header-day text-accent">SUN</div>
          <div class="tcc-cal-header-day text-accent">MON</div>
          <div class="tcc-cal-header-day text-accent">TUE</div>
          <div class="tcc-cal-header-day text-accent">WED</div>
          <div class="tcc-cal-header-day text-accent">THR</div>
          <div class="tcc-cal-header-day text-accent">FRI</div>
          <div class="tcc-cal-header-day text-accent">SAT</div>
      </div>
      <div class="tcc-cal-week" v-for="(week, idx) in calData" :key="idx">
        <div :class="getClassName(day)" v-for="(day, didx) in week" :key="didx" @click="selectWeek(day)">
          {{day.toFormat('dd')}}
          <div>
            <template v-for="c in calendars">
              <div class="tcc-event" v-for="e in eventsOnDay(c.events, day)" :style="('height: 24px; background-color: ' + c.color.replaceAll('%2C', ',') + '; border-color: ' + c.border.replaceAll('%2C', ',') + ';')">
                {{e.location}}
              </div>
            </template>
          </div>
        </div>
      </div>
    </div>
  </div>
  <v-style>
    .tcc-cal-header, .tcc-cal-week {
      display: flex;
      justify-content: center;
    }
    .tcc-cal-header {
      justify-content: space-between;
      font-weight: bold;
      font-size: 14px;
    }
    .tcc-cal-day, .tcc-cal-header-day {
      width: 14.2%;
      height: 150px;
      border-radius: 6px;
      cursor: pointer;
      padding: 4px;
      overflow: hidden;
    }
    .tcc-cal-header-day {
      height: 30px;
      text-align: center;
    }
    .tcc-cal-day:hover, .tcc-month-picker-item:hover {
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
    .tcc-event {
      padding: 2px; 
      width: 100%;
      border-radius: 4px;
      overflow: hidden;
      border-left: 6px solid;
      margin-bottom: 1px;
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
  </v-style>
`
});
