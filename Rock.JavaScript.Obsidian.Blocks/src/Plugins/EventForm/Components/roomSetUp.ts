import { defineComponent, PropType } from "vue"
import TextBox from "@Obsidian/Controls/textBox"
import RockLabel from "@Obsidian/Controls/rockLabel"
import DDL from "@Obsidian/Controls/dropDownList"
import { Button } from "ant-design-vue"

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
      roomSetUp: {} as RoomSetUp
    };
  },
  computed: {
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
    }
  },
  mounted() {
    if(this.modelValue) {
      this.roomSetUp = this.modelValue
    } 
  },
  template: `
<div class="row" style="display: flex; align-items: end;">
  <div class="col col-xs-3">
    <rck-lbl>Type of Table</rck-lbl>
    <rck-ddl
      v-model="roomSetUp.TypeofTable"
      :items="[{value: 'Round', text: 'Round'}, {value: 'Rectangular', text: 'Rectangular'}]"
    ></rck-ddl>
  </div>
  <div class="col col-xs-4">
    <rck-lbl>Number of Tables</rck-lbl>
    <rck-text
      v-model="roomSetUp.NumberofTables"
    ></rck-text>
  </div>
  <div class="col col-xs-4">
    <rck-lbl>Number of Chairs</rck-lbl>
    <rck-text
      v-model="roomSetUp.NumberofChairs"
    ></rck-text>
  </div>
  <div class="col col-xs-1">
    <a-btn type="red" @click="removeConfiguration">
      <i class="fas fa-trash"></i>
    </a-btn>
  </div>
</div>
`
});
