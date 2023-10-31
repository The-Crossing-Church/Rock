import { defineComponent } from "vue"
import RockLabel from "../../../../Elements/rockLabel"
import RockText from "../../../../Elements/textBox"
import { Select } from "ant-design-vue"

const {Option } = Select

export default defineComponent({
  name: "EventForm.Components.DiscountCodePicker",
  components: {
    "rck-lbl": RockLabel,
    "rck-txt": RockText,
    "a-select": Select,
    "a-select-option": Option,
  },
  props: {
    codeType: {
      type: String,
      required: true
    },
    amount: {
      type: String,
      required: true
    },
    disabled: {
      type: Boolean,
      required: false
    },
    label: String,
    items: Array,
  },
  data() {
    return {
      internalCodeType: "" as string,
      internalAmount: "" as string,
    };
  },
  computed: {
    
  },
  methods: {
    
  },
  watch: {
    internalCodeType(val) {
      this.$emit('updateCodeType', val)
    },
    internalAmount(val) {
      this.$emit('updateAmount', val)
    },
    codeType(val) {
      this.internalCodeType = val
    },
    amount(val) {
      this.internalAmount = val
    },
  },
  mounted() {
    this.internalCodeType = this.codeType
    this.internalAmount = this.amount
  },
  template: `
<div>
  <rck-lbl>{{label}}</rck-lbl>
  <div style="display: flex;">
    <a-select
      v-model:value="internalCodeType"
    >
      <a-select-option v-for="i in items" :value="i" :key="i">{{i}}</a-select-option>
    </a-select>
    <div style="width: -webkit-fill-available;">
      <rck-txt
        v-model="internalAmount"
        type="number"
      ></rck-txt>
    </div>
  </div>
</div>
<v-style>

</v-style>
`
});
