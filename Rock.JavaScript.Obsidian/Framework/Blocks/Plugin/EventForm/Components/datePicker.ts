import { defineComponent } from "vue"
import RockForm from "../../../../Controls/rockForm"
import RockField from "../../../../Controls/rockField"
import RockFormField from "../../../../Elements/rockFormField"
import TextBox from "../../../../Elements/textBox"
import Calendar from "./calendar"
import { DateTime } from "luxon"
import { Button, Modal } from "ant-design-vue"


export default defineComponent({
    name: "EventForm.Components.DatePicker",
    components: {
      "rck-field": RockField,
      "rck-form-field": RockFormField,
      "rck-form": RockForm,
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
          return DateTime.fromFormat(this.modelValue, "yyyy-MM-dd").toFormat("MM/dd/yyyy")
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
<rck-form>
  <rck-form-field>
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
        v-on:closemenu="menu = false"
      ></tcc-calendar>
    </a-modal>
  </rck-form-field>
</rck-form>
`
});
