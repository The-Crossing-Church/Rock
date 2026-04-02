import { defineComponent } from "vue"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"

export default defineComponent({
    name: "EventDashboard.Components.Modal.PartialApproval.Values",
    components: {
      "rck-field": RockField,
      "rck-btn": RockButton,
    },
    props: {
      attribute: Object,
      originalValue: String,
      newValue: String,
      rooms: Array,
      inventory: Array
    },
    setup() {

    },
    data() {
      return {
        isApproved: null
      };
    },
    computed: {
      oVal() {
        return this.getDisplayValue(this.originalValue)
      },
      nVal() {
        return this.getDisplayValue(this.newValue)
      },
      attr() {
        if(this.attribute) {
          let val = this.attribute
          if(val.key == 'SetupImage' && val.configurationValues && val.configurationValues["img_tag_template"]) {
            if(val.configurationValues["img_tag_template"].includes("{{'Global' | Attribute:'InternalApplicationRoot'}}")) {
              val.configurationValues["img_tag_template"] = val.configurationValues["img_tag_template"].replaceAll("{{'Global' | Attribute:'InternalApplicationRoot'}}", "/")
            }
          }
          return val
        }
        return null
      }
    },
    methods: {
      getClassName(isOriginal: boolean) {
        let className = "text-red"
        if(!isOriginal) {
          className = "text-primary hidden-label"
        }
        if(this.isApproved == null) {
          return className
        }
        if(this.isApproved && isOriginal) {
          className += " text-strikethrough"
        }
        if(!this.isApproved && !isOriginal) {
          className += " text-strikethrough"
        }
        return className
      },
      getDisplayValue(value: string) {
        if(value?.includes('{') || value?.includes('[')) {
          // JSON Value
          let obj = JSON.parse(value)
          if (Array.isArray(obj)) {
            if(this.attribute?.key == 'OpsInventory') {
              return obj.map(i => {
                let inv = this.inventory?.filter((inv) => inv.guid == i.InventoryItem)
                let itm = ''
                if(inv && inv.length > 0) {
                  itm = inv[0].value
                }
                return `${i.QuantityNeeded} ${(itm ?? i.InventoryItem)}`
              }).join(', ')
            } else if (this.attribute?.key == 'DiscountCodes') {
              let curr = new Intl.NumberFormat('en-US', {
                  style: 'currency',
                  currency: 'USD',
              });
              return obj.map(i => {
                let amt = i.Amount + i.CodeType
                if(i.CodeType == '$') {
                  amt = curr.format(i.Amount)
                }
                return `${i.Code}: ${amt}` + (i.AutoApply == 'True' ? ', Auto-Apply' : '') + (i.MaxUses ? `, ${i.MaxUses} Uses` : '') + (i.EffectiveDateRange ? `, ${i.EffectiveDateRange}` : '')
              }).join('\n')
            } else if (this.attribute?.key == 'RoomSetUp') {
              if(value == '[]') {
                return 'None'
              }
              let rooms = Object.groupBy(obj, i => i.Room)
              let result = []
              Object.keys(rooms).forEach(key => {
                let rm = this.rooms?.filter((r) => r.guid == key)
                let space = ''
                if(rm && rm.length > 0) {
                  space = rm[0].value
                }
                result.push(space + ': ' + rooms[key].map(su => {
                  return `${su.NumberofTables} ${su.TypeofTable} Table(s) ${su.NumberofChairs} Chair(s)`
                }).join(', '))
              })
              return result.join('\n')
            }
          } 
        }
        // if(value == "") {
        //   return 'Empty'
        // }
        return value
      }
    },
    watch: {
      isApproved(val) {
        if(val) {
          this.$emit("approved")
        } else {
          this.$emit("denied")
        }
      },
    },
    mounted() {
    },
    template: `
<div class="row" style="display: flex; align-items: center;">
  <div class="col col-xs-10">
    <div class="row">
      <div class="col col-xs-6">
        <rck-field
          v-model="oVal"
          :attribute="attr"
          :class="getClassName(true)"
          :showEmptyValue="true"
        ></rck-field>
      </div>
      <div class="col col-xs-6">
        <rck-field
          v-model="nVal"
          :attribute="attr"
          :class="getClassName(false)"
          :showEmptyValue="true"
        ></rck-field>
      </div>
    </div>
  </div>
  <div class="col col-xs-2 d-flex">
    <rck-btn btnType="accent" class="btn-circle mr-2" @click="isApproved = true" :disabled="isApproved == true">
      <i class="fa fa-check"></i>
    </rck-btn>
    <rck-btn btnType="red" class="btn-circle" @click="isApproved = false" :disabled="isApproved == false">
      <i class="fa fa-times"></i>
    </rck-btn>
  </div>
</div>
`
});
