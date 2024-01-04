import { defineComponent } from "vue";
import TextBox from "@Obsidian/Controls/textBox";
import RockLabel from "@Obsidian/Controls/rockLabel";
import { DateTime } from "luxon";
import { Button, Modal } from "ant-design-vue";
import Validator from "./validator";
import Chip from "./chip"

export default defineComponent({
    name: "EventForm.Components.TimePicker",
    components: {
      "tcc-chip": Chip,
      "rck-lbl": RockLabel,
      "rck-text": TextBox,
      "tcc-validator": Validator,
      "a-btn": Button,
      "a-modal": Modal
    },
    props: {
        modelValue: String,
        default: String,
        rules: Array,
        dates: Array,
        label: String,
        quickSetItems: Array,
        id: String
    },
    setup() {

    },
    data() {
        return {
          hour: "",
          minute: "00",
          meridiem: "AM",
          menu: false,
          originalValue: "",
          valid: true,
          defaultrules: {
            required(val: any, field: string) {
              return !!val || `${field} is required`;
            },
            isValidHour(val: number) {
              return val > 0 && val < 25 || 'Invalid input for hour'
            },
            isValidMinute(val: number) {
              return val >= 0 && val < 60 || 'Invalid input for minute'
            }
          },
          onlySundays: false
          
        };
    },
    computed: {
      time() {
        let time = DateTime.fromFormat(`${this.hour}:${this.minute} ${this.meridiem}`, "hh:mm a")
        return time.toFormat("HH:mm:ss");
      },
      displayTime() {
        if(this.modelValue) {
          return `${this.hour}:${this.minute} ${this.meridiem}`
        } else {
          return ""
        }
      },
      amClass() {
        let className = "btn-am"
        if(this.meridiem == "AM") {
          className += " active"
        }
        return className
      },
      pmClass() {
        let className = "btn-pm"
        if(this.meridiem == "PM") {
          className += " active"
        }
        return className
      },
    },
    watch: {
      menu(val) {
        if(val) {
          setTimeout(() => {
            let el = document.querySelector('.ant-modal:not([style*="display: none"]) .txt-hour') as any
            if(el) {
              el.focus()
            }
          }, 500)
        }
      },
      time(val) {
        this.$emit("update:modelValue", val)
      },
      default(val) {
        if (!this.originalValue && (!this.modelValue || this.modelValue.includes("null"))) {
          this.hour = val.split(":")[0]
          this.minute = val.split(":")[1].split(" ")[0]
          this.meridiem = val.split(" ")[1]
        }
      },
      modelValue(val) {
        if (val) {
          let time = DateTime.fromFormat(val, "HH:mm:ss")
          if(time.isValid) {
            this.hour = time.toFormat("hh")
            this.minute = time.toFormat("mm")
            this.meridiem = time.toFormat("a")
          }
        }
      },
      minute(val) {
        if(val && val.toString().includes('.')){
          this.minute = val.replace('.','')
        }
      },
      hour(val) {
        if(val && val.toString().includes('.')){
          this.hour = val.replace('.','')
        }
      },
      dates: {
        handler(val) {
          if(val) {
            let arrVal = val.split(",")
            if(arrVal.length > 0) {
              let allSundays = true
              arrVal.forEach((d: string) => {
                let x = DateTime.fromFormat(d, "yyyy-MM-dd").toFormat("E")
                if(x != '7') {
                  allSundays = false
                }
              })
              this.onlySundays = allSundays
            }
          } else {
            this.onlySundays = false
          }
        },
        deep: true
      }

    },
    methods: {
      validateHour(val: any) {
        val = parseInt(val)
        if(!(val > 0 && val < 25)) {
          this.hour = ""
          return
        }
        if(val > 12) {
          val = val - 12
          this.meridiem = "PM"
        }
        if(val.toString().length < 2) {
          val = "0" + val
        }
        this.hour = val
        let ref = this.$refs.hour as any
        ref.validate()
      },
      validateMinute(val: any) {
        val = parseInt(val)
        if(!(val >= 0 && val < 60)) {
          this.minute = ''
          return
        }
        if(val.toString().length < 2) {
          val = "0" + val
        }
        this.minute = val
        let ref = this.$refs.min as any
        ref.validate()
      },
      quickSet(start: string, end: string) {
        let time = DateTime.fromFormat(start, "hh:mm a")
        this.hour = time.toFormat("hh")
        this.minute = time.toFormat("mm")
        this.meridiem = time.toFormat("a")
        this.menu = false
        this.$emit('quicksettime', end)
      }
    },
    mounted() {
      if (this.modelValue) {
        this.originalValue = this.modelValue
      }
      if (this.modelValue) {
        let time = DateTime.fromFormat(this.modelValue, "HH:mm:ss")
        this.hour = time.toFormat("hh") 
        this.minute = time.toFormat("mm") 
        this.meridiem = time.toFormat("a") 
      } else if (this.default) {
        let time = DateTime.fromFormat(this.default, "HH:mm:ss")
        this.hour = time.toFormat("hh")
        this.minute = time.toFormat("mm")
        this.meridiem = time.toFormat("a")
      } else {
        this.hour = "";
        this.minute = "00";
        this.meridiem = "AM";
      }
      let els = document.querySelectorAll(".tcc-text-display")
      els.forEach((el: any) => {
        el.setAttribute("readonly", "")
      })
    },
    template: `
<div>
  <tcc-validator :rules="required">
    <rck-lbl>{{label}}</rck-lbl>
    <rck-text
      v-model="displayTime"
      inputClasses="tcc-text-display"
      @click="menu = true"
      :id="'txt' + id"
    ></rck-text>
  </tcc-validator>
  <a-modal v-model:visible="menu" @ok="menu = false" :ok-button-props="{ id: 'btnSaveTime' }">
    <div class="time-menu">
      <div class="row">
        <strong>ENTER TIME</strong>
      </div >
      <div class="row" v-if="onlySundays">
        <div class="col">
          <div class="chip-group">
            <tcc-chip
              v-for="(c, idx) in quickSetItems" :key="idx"
              @click="quickSet(c.mine, c.theirs)"
              :disabled="true"
              class="hover"
            >{{c.title}}</tcc-chip>
          </div>
        </div>
      </div >
      <div style="display: flex; align-items: flex-end;">
        <div style="padding: 0px 4px;">
          <tcc-validator :rules="[defaultrules.required(hour, 'Hour'), defaultrules.isValidHour(hour)]" ref="hour">
            <rck-lbl>Hour</rck-lbl>
            <rck-text
              v-model="hour"
              class="txt-round txt-hour"
              type="number"
              @blur="validateHour(hour)"
              :id="'txtHour' + id"
            ></rck-text>
          </tcc-validator>
        </div>
        <div>
          <div style="font-size: 34px; margin-top: -5px;">:</div>
        </div>
        <div style="padding: 0px 4px;">
          <tcc-validator :rules="[defaultrules.required(minute, 'Minute'), defaultrules.isValidMinute(minute)]" ref="min">
            <rck-lbl>Minute</rck-lbl>
            <rck-text
              v-model="minute"
              class="txt-round"
              type="number"
              @blur="validateMinute(minute)"
              :id="'txtMinute' + id"
            ></rck-text>
          </tcc-validator>
        </div>
        <div style="padding: 0px 4px;">
          <div class='btn-time-wrapper'>
            <div :class='amClass' v-on:click="meridiem = 'AM'">
              AM
            </div>
            <div :class='pmClass' v-on:click="meridiem = 'PM'">
              PM
            </div>
          </div>
        </div>
      </div>
    </div>
  </a-modal>
</div>

<v-style>
  .time-menu {
    padding: 8px;
    display: flex;
    flex-direction: column;
  }
  .txt-round {
    border-radius: 28px;
    padding: 0 24px;
    line-height: 24px;
    min-height: 42px;
  }
  .txt-hour:after {
    content: ":";
  }
  .btn-time-wrapper {
    margin-top: -10px;
  }
  .btn-time-wrapper div {
    text-align: center;
    padding: 4px 8px;
    background-color: rgba(0,0,0,.12);
    cursor: pointer;
  }
  .btn-time-wrapper .btn-am {
    border-radius: 14px 14px 0px 0px;
  }
  .btn-time-wrapper .btn-pm {
    border-radius: 0px 0px 14px 14px;
  }
  .btn-time-wrapper .active {
    background-color: #347689;
    color: white;
  }
</v-style>
`
});
