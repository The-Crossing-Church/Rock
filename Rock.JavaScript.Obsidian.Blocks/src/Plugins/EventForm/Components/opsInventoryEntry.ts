import { defineComponent, PropType } from "vue"
import TextBox from "@Obsidian/Controls/textBox"
import RockLabel from "@Obsidian/Controls/rockLabel"
import DDL from "@Obsidian/Controls/dropDownList"
import { Button } from "ant-design-vue"
import OpsInvDDL from "./opsInventoryDropDown"

type InventoryReservation = {
  InventoryItem: string,
  QuantityNeeded: number
}

export default defineComponent({
  name: "EventForm.Components.OpsInventoryEntry",
  components: {
    "rck-text": TextBox,
    "rck-lbl": RockLabel,
    "rck-ddl": DDL,
    "a-btn": Button,
    "tcc-inv-ddl": OpsInvDDL
  },
  props: {
    modelValue: Object as PropType<InventoryReservation>,
    inventory: Array,
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
    id: String
  },
  setup() {

  },
  data() {
    return {
      inventoryRes: {} as InventoryReservation
    };
  },
  computed: {
  },
  methods: {
    removeConfiguration() {
      this.$emit('removeinventoryconfig')
    }
  },
  watch: {
    inventoryRes(val) {
      if (val) {
        this.$emit('update:modelValue', this.inventoryRes)
      } else {
        this.$emit('update:modelValue', "{}")
      }
    }
  },
  mounted() {
    if(this.modelValue) {
      this.inventoryRes = this.modelValue
    } 
  },
  template: `
<div class="row" style="display: flex; align-items: end;">
  <div class="col col-xs-5">
    <tcc-inv-ddl
      v-model="inventoryRes.InventoryItem"
      :items="inventory"
      :id="'ddlItem' + id"
    ></tcc-inv-ddl>
  </div>
  <div class="col col-xs-5">
    <rck-lbl>Quantity Needed</rck-lbl>
    <rck-text
      v-model="inventoryRes.QuantityNeeded"
      type="number"
      :id="'txtQuantityNeeded' + id"
    ></rck-text>
  </div>
  <div class="col col-xs-2">
    <a-btn type="red" @click="removeConfiguration" :id="'btnRemove' + id">
      <i class="fas fa-trash"></i>
    </a-btn>
  </div>
</div>
`
});
