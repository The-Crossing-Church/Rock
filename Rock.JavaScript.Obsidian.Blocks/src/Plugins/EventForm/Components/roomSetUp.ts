import { defineComponent, PropType } from "vue"
import TextBox from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import DDL from "@Obsidian/Controls/dropDownList.obs"
import { Button } from "ant-design-vue"
import Validator from "./validator"
import rules from "../Rules/rules"

type RoomSetUp = {
  Room: string,
  TypeofTable: string,
  NumberofTables: number,
  NumberofChairs: number
}

export default defineComponent({
  name: "EventForm.Components.RoomSetUp",
  components: {
    "rck-text": TextBox,
    "rck-lbl": RockLabel,
    "rck-ddl": DDL,
    "tcc-validator": Validator,
    "a-btn": Button,
  },
  props: {
    modelValue: Object as PropType<RoomSetUp>,
    disabled: {
      type: Boolean,
      required: false
    },
    hint: {
      type: String,
      required: false
    },
    persistentHint: {
      type: Boolean,
      required: false
    },
  },
  setup() {

  },
  data() {
    return {
      roomSetUp: {} as RoomSetUp,
      rules: rules,
      errors: []
    };
  },
  computed: {
    typeOfTableRules() {
      if(this.roomSetUp.NumberofTables > 0) {
        return [this.rules.required(this.roomSetUp.TypeofTable, 'Type of Table')]
      }
      return []
    }
  },
  methods: {
    removeConfiguration() {
      this.$emit('removeconfig')
    }
  },
  watch: {
    roomSetUp(val) {
      if (val) {
        this.$emit('update:modelValue', this.roomSetUp)
      } else {
        this.$emit('update:modelValue', "{}")
      }
    },
    'roomSetUp.NumberofTables': {
      handler(val) {
        this.errors = []
        let formRef = this.$refs as any
        for(let r in formRef) {
          if(formRef[r].className?.includes("validator")) {
            formRef[r].validate()
          }
        }
      }
    }
  },
  mounted() {
    if(this.modelValue) {
      this.roomSetUp = this.modelValue
    }
  },
  template: `
<div class="row">
  <div class="col col-xs-3">
    <tcc-validator :rules="typeOfTableRules" ref="validators_typeoftable">
      <rck-lbl>Type of Table</rck-lbl>
      <rck-ddl
        v-model="roomSetUp.TypeofTable"
        :items="[{value: 'Round', text: 'Round'}, {value: 'Rectangular', text: 'Rectangular'}]"
      ></rck-ddl>
    </tcc-validator>
  </div>
  <div class="col col-xs-4">
    <rck-lbl>Number of Tables</rck-lbl>
    <rck-text
      v-model="roomSetUp.NumberofTables"
      type="number"
    ></rck-text>
  </div>
  <div class="col col-xs-4">
    <rck-lbl>Number of Chairs</rck-lbl>
    <rck-text
      v-model="roomSetUp.NumberofChairs"
      type="number"
    ></rck-text>
  </div>
  <div class="col col-xs-1 pt-4">
    <a-btn type="red" @click="removeConfiguration" :disabled="disabled">
      <i class="fas fa-trash"></i>
    </a-btn>
  </div>
</div>
`
});
