import { defineComponent } from "vue"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import RockText from "@Obsidian/Controls/textBox.obs"
import DropDownList from "@Obsidian/Controls/dropDownList.obs"

export default defineComponent({
  name: "EventForm.Components.DiscountCodePicker",
  components: {
    "rck-lbl": RockLabel,
    "rck-txt": RockText,
    "rck-ddl": DropDownList
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
    id: String
  },
  data() {
    return {
      internalCodeType: "" as string,
      internalAmount: "" as string,
    };
  },
  computed: {
    itemList() {
      return this.items.map(i => {
        return { text: i, value: i }
      })
    }
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
  <div style="display: flex;">
    <rck-ddl
      :label="label"
      :items="itemList"
      v-model="internalCodeType"
    ></rck-ddl>
    <div class="w-100 ml-2 d-flex" style="align-items: end;">
      <rck-txt
        v-model="internalAmount"
        type="number"
        :id="'txt' + id"
      ></rck-txt>
    </div>
  </div>
</div>
<v-style>

</v-style>
`
});
