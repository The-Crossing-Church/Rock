import { defineComponent, PropType } from "vue"
import { DateTime, Duration } from "luxon"
import { Button, Dropdown, Menu } from "ant-design-vue"

const { MenuItem } = Menu

export default defineComponent({
    name: "EventForm.Components.Calendar",
    components: {
        "a-btn": Button,
        "a-dropdown": Dropdown,
        "a-menu": Menu,
        "a-menu-item": MenuItem
    },
    props: {
        modelValue: String,
        readonly: {
            type: Boolean,
            required: false
        },
        min: {
            type: String,
            required: false
        },
        max: {
            type: String,
            required: false
        },
        multiple: {
            type: Boolean,
            required: false
        },
        noBorder: {
            type: Boolean,
            required: false
        },
        rules: {
            type: Array,
            required: false
        },
        disabledDates: {
            type: Array as PropType<String[]>,
            required: false
        },
        id: String
    },
    setup() {

    },
    data() {
        return {
            selectedDates: [] as String[],
            startDate: DateTime.now(),
            endDate: DateTime.now(),
            currentMonth: 0,
            currentYear: 0,
            monthMenu: false
        };
    },
    computed: {
        displayMonth() {
            return DateTime.fromObject({ day: 1, month: this.currentMonth, year: this.currentYear })
        },
        calData() {
            let weeks = [] as any[]
            if (this.displayMonth && this.displayMonth.daysInMonth) {
                let week = [] as any[]
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
        canClickPre() {
            let span = Duration.fromObject({days: -1})
            let lastMonth = this.displayMonth.minus(span)
            if(lastMonth < this.startDate) {
                return false
            }
            return true
        },
        canClickNext() {
            let span = Duration.fromObject({months: 1})
            let nextMonth = this.displayMonth.plus(span)
            if(nextMonth < this.endDate) {
                return true
            }
            return false
        },
        wrapperClassName() {
            if(this.noBorder) {
                return "tcc-cal-wrapper"
            }
            return "tcc-cal-wrapper card"
        }
    },
    methods: {
        prevMonth() {
            if (this.currentMonth > 1) {
                this.currentMonth--
            } else {
                this.currentMonth = 12
                this.currentYear--
            }
        },
        nextMonth() {
            if (this.currentMonth < 12) {
                this.currentMonth++
            } else {
                this.currentMonth = 1
                this.currentYear++
            }
        },
        getClassName(day: DateTime) {
            let className = "tcc-cal-pkr-day"
            if(day.month != this.displayMonth.month) {
                className += " diff-month"
            }
            if (day.month != this.displayMonth.month || day < this.startDate || day > this.endDate || this.disabledDates?.includes(day.toFormat("yyyy-MM-dd"))) {
                className += " disabled"
            } else {
                if (this.selectedDates.includes(day.toFormat('yyyy-MM-dd'))) {
                    className += " selected"
                }
            }
            return className
        },
        getMonthPickerClassName(month: number) {
            let className = "tcc-month-picker-item"
            let date = DateTime.fromObject({ day: 1, month: month, year: this.currentYear })
            if (date > this.endDate) {
                className += " disabled"
            } else {
                date = date.endOf('month')
                if (date < this.startDate) {
                    className += " disabled"
                }
            }
            if (month == this.currentMonth) {
                className += " selected"
            }
            return className
        },
        toggleSelect(day: DateTime) {
            if(this.readonly) {
                return
            }
            if (day.month == this.displayMonth.month && day >= this.startDate && day <= this.endDate) {
                let exists = this.selectedDates.indexOf(day.toFormat('yyyy-MM-dd'))
                let dates = this.selectedDates
                if(this.multiple) {
                    if (exists >= 0) {
                        dates.splice(exists, 1)
                    } else {
                        dates.push(day.toFormat('yyyy-MM-dd'))
                    }
                } else {
                    if(exists >= 0) {
                        dates = []
                    } else {
                        dates = [day.toFormat('yyyy-MM-dd')]
                        this.$emit('closemenu')
                    }
                }
                dates = dates.sort()
                this.selectedDates = dates
            }
        },
        selectMonth(month: number) {
            let date = DateTime.fromObject({ day: 1, month: month, year: this.currentYear })
            if (date >= this.startDate) {
                date = date.endOf('month')
                if (date < this.endDate) {
                    this.currentMonth = month
                    this.monthMenu = false
                }
            }
        },
        initializeData() {
            //Set the min date available on the calendar
            if (this.min) {
                this.startDate = DateTime.fromFormat(this.min, "yyyy-MM-dd")
            }
            //Set the selecte dates
            if (this.modelValue) {
                let dates = this.modelValue.split(",").map((d: string) => d.trim())
                this.selectedDates = dates
                let today = DateTime.now()
                let firstDate = DateTime.fromFormat(dates[0], "yyyy-MM-dd")
                //Our original minimum date is in the past
                if(firstDate.startOf('day') < today.startOf('day')) {
                    if(!this.min || firstDate.startOf('day') < this.startDate) {
                        //Looking at historical event
                        this.startDate = firstDate
                    }
                } else {
                    if(!this.min) {
                        this.startDate = today
                    }
                }
                this.currentMonth = firstDate.month
                this.currentYear = firstDate.year
            } else {
                this.currentMonth = this.startDate.month
                this.currentYear = this.startDate.year
            }
            //Set the max date available on the calendar 
            if (this.max) {
                this.endDate = DateTime.fromFormat(this.max, "yyyy-MM-dd")
            } else {
                let span = Duration.fromObject({ months: 18 })
                this.endDate = this.endDate.plus(span)
            }
        }
    },
    watch: {
        selectedDates: {
            deep: true,
            handler(val) {
                if (val) {
                    let dates = val.join(",")
                    this.$emit('update:modelValue', dates)
                }
            }
        },
        modelValue(val) {
            if(val != this.selectedDates.join(",")) {
                this.selectedDates = val.split(",").map((d: string) => d.trim()).filter((d: string) => { return d != "" })
            }
        },
        min(val) {
            this.initializeData()
        },
        max(val) {
            this.initializeData()
        }
    },
    mounted() {
        this.initializeData()
    },
    template: `
<div :class="wrapperClassName">
  <div class="tcc-cal-pkr-header">
    <i v-if="canClickPre" class="fa fa-chevron-left hover" @click="prevMonth"></i>
    <i v-else class="fa fa-chevron-left fa-disabled"></i>
    <div style="display: flex;" :id="id">
      <a-dropdown :trigger="['click']" v-model:visible="monthMenu">
        <div style="padding-right: 8px;">
          {{displayMonth.toFormat('MMMM')}}
        </div>
        <template #overlay>
          <a-menu class="tcc-month-picker">
            <a-menu-item :class="getMonthPickerClassName(1)" @click="selectMonth(1)" key="JAN">JAN</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(2)" @click="selectMonth(2)" key="FEB">FEB</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(3)" @click="selectMonth(3)" key="MAR">MAR</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(4)" @click="selectMonth(4)" key="APR">APR</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(5)" @click="selectMonth(5)" key="MAY">MAY</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(6)" @click="selectMonth(6)" key="JUN">JUN</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(7)" @click="selectMonth(7)" key="JUL">JUL</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(8)" @click="selectMonth(8)" key="AUG">AUG</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(9)" @click="selectMonth(9)" key="SEP">SEP</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(10)" @click="selectMonth(10)" key="OCT">OCT</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(11)" @click="selectMonth(11)" key="NOV">NOV</a-menu-item>
            <a-menu-item :class="getMonthPickerClassName(12)" @click="selectMonth(12)" key="DEC">DEC</a-menu-item>
          </a-menu>
        </template>
      </a-dropdown>
      <div>
        {{displayMonth.toFormat('yyyy')}}
      </div>
    </div>
    <i v-if="canClickNext" class="fa fa-chevron-right hover" @click="nextMonth"></i>
    <i v-else class="fa fa-chevron-right fa-disabled"></i>
  </div>
  <div class="tcc-cal-pkr-body">
    <div class="tcc-cal-pkr-week">
        <div class="tcc-cal-pkr-day text-accent">S</div>
        <div class="tcc-cal-pkr-day text-accent">M</div>
        <div class="tcc-cal-pkr-day text-accent">T</div>
        <div class="tcc-cal-pkr-day text-accent">W</div>
        <div class="tcc-cal-pkr-day text-accent">T</div>
        <div class="tcc-cal-pkr-day text-accent">F</div>
        <div class="tcc-cal-pkr-day text-accent">S</div>
    </div>
    <div class="tcc-cal-pkr-week" v-for="(week, idx) in calData" :key="idx">
      <div :class="getClassName(day)" v-for="(day, didx) in week" :key="didx" @click="toggleSelect(day)">
        {{day.toFormat('dd')}}
      </div>
    </div>
  </div>
</div>
<v-style>
  .tcc-cal-pkr-header, .tcc-cal-pkr-week {
    display: flex;
    justify-content: center;
  }
  .tcc-cal-pkr-header {
    justify-content: space-between;
    font-weight: bold;
    font-size: 14px;
  }
  .tcc-cal-pkr-day {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    align-items: center;
    justify-content: center;
    display: flex;
    cursor: pointer;
  }
  .tcc-cal-pkr-day:hover, .tcc-month-picker-item:hover {
    background-color: #EEEFEF;
  }
  .tcc-cal-pkr-day.selected {
    background-color: #8ED2C9;
  }
  .tcc-cal-pkr-day.disabled, .tcc-month-picker-item.disabled {
    color: rgba(0,0,0,.26) !important;
    cursor: not-allowed;
  }
  .tcc-cal-pkr-day.diff-month {
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
</v-style>
`
});
