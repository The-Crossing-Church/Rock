import { defineComponent, PropType } from "vue"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import TextBox from "@Obsidian/Controls/textBox.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import Modal from "@Obsidian/Controls/modal.obs"
import Calendar from "./calendar"
import { DateTime } from "luxon"

export default defineComponent({
    name: "EventForm.Components.DatePicker",
    components: {
      "rck-field": RockField,
      "rck-lbl": RockLabel,
      "rck-text": TextBox,
      "tcc-calendar": Calendar,
      "rck-btn": RockButton,
      "rck-modal": Modal,
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
      },
      disabledDates: {
        type: String,
        required: false
      },
      id: String
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
          let val = this.modelValue as string
          if(val.includes("T")) {
            val = val.split("T")[0]
          }
          if(val.includes(" ")) {
            val = val.split(" ")[0]
          }
          let dt = DateTime.fromFormat(val, "yyyy-MM-dd")
          return dt.toFormat("MM/dd/yyyy")
        }
        return ""
      },
      unavailableDates() {
        if(this.disabledDates) {
          return this.disabledDates?.split(",")
        }
        return [] as String[]
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
<div class="form-group">
  <rck-lbl>{{label}}</rck-lbl>
  <rck-text
    v-model="displayDate"
    inputClasses="tcc-text-display"
    @click="menu = true"
    :id="'txt' + id"
  ></rck-text>
  <rck-modal v-model="menu" width="50%" isCloseButtonHidden cancelText="" clickBackdropToClose isNarrow modalWrapperClasses="modal-no-header">
    <br/>
    <tcc-calendar
      v-model="date"
      :multiple="false"
      :noBorder="true"
      :min="min"
      :max="max"
      v-on:closemenu="menu = false"
      :disabledDates="unavailableDates"
      :id="'cal_' + id"
    ></tcc-calendar>
  </rck-modal>
</div>
`
});
