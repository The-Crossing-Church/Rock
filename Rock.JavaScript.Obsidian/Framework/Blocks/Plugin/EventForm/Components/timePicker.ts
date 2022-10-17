import { defineComponent } from "vue";
import TextBox from "../../../../Elements/textBox";
import RockLabel from "../../../../Elements/rockLabel";
import { DateTime } from "luxon";
import { Button, Modal } from "ant-design-vue";
import RockForm from "../../../../Controls/rockForm";
import RockFormField from "../../../../Elements/rockFormField";
import Chip from "./chip"

export default defineComponent({
    name: "EventForm.Components.TimePicker",
    components: {
      "tcc-chip": Chip,
      "rck-lbl": RockLabel,
      "rck-text": TextBox,
      "rck-form": RockForm,
      "rck-form-field": RockFormField,
      "a-btn": Button,
      "a-modal": Modal
    },
    props: {
        modelValue: String,
        default: String,
        rules: Array,
        dates: Array,
        label: String,
        quickSetItems: Array
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
            isValidHour(val: Number) {
              return val > 0 && val < 25 || 'Invalid input for hour'
            },
            isValidMinute(val: Number) {
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
      time(val) {
        this.$emit("update:modelValue", val);
      },
      default(val) {
        if (!this.originalValue && (!this.modelValue || this.modelValue.includes("null"))) {
          this.hour = val.split(":")[0];
          this.minute = val.split(":")[1].split(" ")[0];
          this.meridiem = val.split(" ")[1];
        }
      },
      modelValue(val) {
        if (val) {
          let time = DateTime.fromFormat(val, "HH:mm:ss")
          this.hour = time.toFormat("hh")
          this.minute = time.toFormat("mm")
          this.meridiem = time.toFormat("a")
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
<rck-form>
  <rck-form-field :rules="required">
    <rck-lbl>{{label}}</rck-lbl>
    <rck-text
      v-model="displayTime"
      inputClasses="tcc-text-display"
      @click="menu = true"
    ></rck-text>
  </rck-form-field>
  <a-modal v-model:visible="menu" @ok="menu = false">
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
          <rck-form-field :rules="required">
            <rck-lbl>Hour</rck-lbl>
            <rck-text
              v-model="hour"
              class="txt-round txt-hour"
              type="number"
              autofocus
              :rules="[defaultrules.required(hour, 'Hour'), defaultrules.isValidHour(hour)]"
              @blur="validateHour(hour)"
            ></rck-text>
          </rck-form-field>
        </div>
        <div>
          <div style="font-size: 34px; margin-top: -5px;">:</div>
        </div>
        <div style="padding: 0px 4px;">
          <rck-form-field :rules="required">
            <rck-lbl>Minute</rck-lbl>
            <rck-text
              v-model="minute"
              class="txt-round"
              type="number"
              autofocus
              :rules="[defaultrules.required(minute, 'Minute'), defaultrules.isValidMinute(minute)]"
              @blur="validateMinute(minute)"
            ></rck-text>
          </rck-form-field>
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
</rck-form>

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
