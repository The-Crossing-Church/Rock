import { defineComponent } from "vue"
import RockField from "../../../../Controls/rockField"
import RockLabel from "../../../../Elements/rockLabel"
import TextBox from "../../../../Elements/textBox"
import Calendar from "./calendar"
import { DateTime } from "luxon"
import { Button, Modal } from "ant-design-vue"


export default defineComponent({
    name: "EventForm.Components.DatePicker",
    components: {
      "rck-field": RockField,
      "rck-lbl": RockLabel,
      "rck-text": TextBox,
      "tcc-calendar": Calendar,
      "a-btn": Button,
      "a-modal": Modal,
    },
    props: {
      modelValue: String,
      label: String,
      disabled: {
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
      rules: {
          type: Array,
          required: false
      }
    },
    setup() {

    },
    data() {
        return {
          menu: false,
          date: ""
        }
    },
    computed: {
      displayDate() {
        if(this.modelValue) {
          let regex = new RegExp('[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9] [0-9][0-9]:[0-9][0-9]:[0-9][0-9]') 
          if(regex.test(this.modelValue)) {
            return DateTime.fromFormat(this.modelValue, 'yyyy-MM-dd HH:mm:ss').toFormat("MM/dd/yyyy")
          } else {
            regex = new RegExp('[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]')
            if(regex.test(this.modelValue)) {
              return DateTime.fromFormat(this.modelValue, 'yyyy-MM-dd').toFormat("MM/dd/yyyy")
            } else {
              return DateTime.fromISO(this.modelValue).toFormat("MM/dd/yyyy")
            }
          }
        }
        return ""
      }
    },
    methods: {

    },
    watch: {
      date: {
        handler(val) {
          this.$emit('update:modelValue', val)
        },
        deep: true
      }
    },
    mounted() {
      if(this.modelValue) {
        this.date = this.modelValue
      }
    },
    template: `
<div>
  <rck-lbl>{{label}}</rck-lbl>
  <rck-text
    v-model="displayDate"
    inputClasses="tcc-text-display"
    @click="menu = true"
  ></rck-text>
  <a-modal v-model:visible="menu" @ok="menu = false">
    <br/>
    <tcc-calendar
      v-model="date"
      :multiple="false"
      :noBorder="true"
      :min="min"
      :max="max"
      v-on:closemenu="menu = false"
    ></tcc-calendar>
  </a-modal>
</div>
<v-style>
.ant-modal-close-x {
  height: 40px;
}
</v-style>
`
});
