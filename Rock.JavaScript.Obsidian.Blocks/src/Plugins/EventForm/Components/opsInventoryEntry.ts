import { defineComponent, PropType } from "vue"
import TextBox from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import DDL from "@Obsidian/Controls/dropDownList.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
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
    "rck-btn": RockButton,
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
    },
    changeFocus() {
      setTimeout(() => {
        let el = $("#txtQuantityNeeded" + this.id)
        if(el){
          el.focus()
        }
      }, 300)
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
      v-on:select="changeFocus"
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
    <rck-btn btnType="red" @click="removeConfiguration" :id="'btnRemove' + id">
      <i class="fas fa-trash"></i>
    </rck-btn>
  </div>
</div>
`
});
