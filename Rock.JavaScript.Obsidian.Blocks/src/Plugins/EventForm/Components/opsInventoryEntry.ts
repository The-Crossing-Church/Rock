import { defineComponent, PropType } from "vue"
import { AttributeBag } from "../../ViewModels/attributeBag"
import TextBox from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import DDL from "@Obsidian/Controls/dropDownList.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import RockField from "@Obsidian/Controls/rockField.obs"
import OpsInvDDL from "./opsInventoryDropDown"

type InventoryReservation = {
  InventoryItem: string,
  QuantityNeeded: number,
  InventoryDelivery: string
}

export default defineComponent({
  name: "EventForm.Components.OpsInventoryEntry",
  components: {
    "rck-text": TextBox,
    "rck-lbl": RockLabel,
    "rck-ddl": DDL,
    "rck-btn": RockButton,
    "rck-field": RockField,
    "tcc-inv-ddl": OpsInvDDL
  },
  props: {
    modelValue: Object as PropType<InventoryReservation>,
    attrs: Array as PropType<AttributeBag[]>,
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
    inventoryDeliveryAttr() {
      if(this.attrs) {
        let attr = this.attrs.filter((a: any) => { return a.key == "InventoryDelivery" })
        if(attr && attr.length > 0) {
          return attr[0]
        }
      }
      return null
    },
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
<div class="row ops-inventory" :id=id>
  <div class="col col-xs-10 col-md-5">
    <tcc-inv-ddl
      v-model="inventoryRes.InventoryItem"
      :items="inventory"
      label="Item"
      :id="'ddlItem' + id"
      v-on:select="changeFocus"
    ></tcc-inv-ddl>
  </div>
  <div class="col col-xs-10 col-md-5">
    <rck-text
      v-model="inventoryRes.QuantityNeeded"
      type="number"
      :id="'txtQuantityNeeded' + id"
      label="Quantity Needed"
    ></rck-text>
  </div>
  <div class="col col-xs-10 col-md-10">
    <rck-field
      v-model="inventoryRes.InventoryDelivery"
      :attribute="inventoryDeliveryAttr"
      :is-edit-mode="true"
      id="txtDelivery"
    ></rck-field>
  </div>
  <div class="col col-xs-2 pt-4 mt-2 ops-inventory-row-action">
    <rck-btn btnType="red" @click="removeConfiguration" :id="'btnRemove' + id">
      <i class="fas fa-trash"></i>
    </rck-btn>
  </div>
</div>
`
});
