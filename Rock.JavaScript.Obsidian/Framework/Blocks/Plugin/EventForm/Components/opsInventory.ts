import { defineComponent, PropType } from "vue"
import { ContentChannelItem } from "../../../../ViewModels"
import { DateTime, Duration, Interval } from "luxon"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import RockLabel from "../../../../Elements/rockLabel"
import { Button, Modal, Select } from "ant-design-vue"
import OpsInventory from "./opsInventoryEntry"

type InventoryReservation = {
  InventoryItem: string,
  QuantityNeeded: number
}
type ListItem = {
  text: string,
  value: string,
  description: string,
  isDisabled: boolean,
  isHeader: boolean,
  type: string,
  order: number
}

export default defineComponent({
  name: "EventForm.Components.OpsInventory",
  components: {
    "rck-field": RockField,
    "rck-form": RockForm,
    "rck-lbl": RockLabel,
    "tcc-ops-inv": OpsInventory,
    "a-btn": Button,
    "a-modal": Modal,
    "a-select": Select,
  },
  props: {
    e: {
        type: Object as PropType<ContentChannelItem>,
        required: false
    },
    request: Object as PropType<ContentChannelItem>,
    originalRequest: Object as PropType<ContentChannelItem>,
    inventoryList: Array as PropType<any[]>,
    existing: Array as PropType<any[]>,
    readonly: Boolean
  },
  setup() {

  },
  data() {
      return {
        opsInventory: [] as InventoryReservation[],
        modal: false
      };
  },
  computed: {
    inventory() {
      return this.inventoryList?.map(l => {
        let x = {} as ListItem
        x.value = l.guid
        if(l.value) {
          x.text = l.value
        }
        if(l.attributeValues?.Type) {
          x.type = l.attributeValues.Type.value
        }
        if(l.attributeValues?.Quantity) {
          x.description = l.attributeValues.Quantity.value
        }
        if(!l.isActive) {
          x.isDisabled = true
        }
        if(l.order) {
          x.order = l.order
        }
        return x
      }).sort((a: ListItem, b: ListItem) => {
        if(a.order > b.order) {
          return 1
        } else if(a.order < b.order) {
          return -1
        }
        return 0
      })
    },
    groupedInventory() {       
      let inv = [] as any[]
      let dates = [] as any[]
      if(this.request?.attributeValues?.IsSame == "True") {
        dates = this.request.attributeValues.EventDates.split(",").map(d => d.trim())
      } else {
        dates.push(this.e?.attributeValues?.EventDate)
      }
      let existingOnDate = this.existing?.filter(e => {
        if(e.id == this.request?.id || e.id == this.originalRequest?.id) {
          return false
        }
        let intersect = e.attributeValues?.EventDates.value.split(",").map((d: string) => d.trim()).filter((date: string) => dates.includes(date))
        if(intersect && intersect.length > 0) {
          //Filter to events object for the matching dates
          let events = [] as any[]
          if(e.attributeValues?.IsSame.value == "True") {
            events = e.childItems
          } else {
            events = e.childItems.filter((child: any) => dates.includes(child.childContentChannelItem.attributeValues?.EventDate.value))
          }
          //Check if the times overlap
          let overlaps = false
          if(e.attributeValues?.IsSame.value == 'True') {
            intersect.forEach((date: string) => {
              let cdStart = DateTime.fromFormat(`${date} ${events[0].childContentChannelItem.attributeValues?.StartTime.value}`, `yyyy-MM-dd HH:mm:ss`)
              if (events[0].childContentChannelItem.attributeValues?.StartBuffer) {
                let span = Duration.fromObject({ minutes: parseInt(events[0].childContentChannelItem.attributeValues.StartBuffer.value) })
                cdStart = cdStart.minus(span)
              }
              let cdEnd = DateTime.fromFormat(`${date} ${events[0].childContentChannelItem.attributeValues?.EndTime.value}`, `yyyy-MM-dd HH:mm:ss`)
              if (events[0].childContentChannelItem.attributeValues?.EndBuffer) {
                let span = Duration.fromObject({ minutes: parseInt(events[0].childContentChannelItem.attributeValues.EndBuffer.value) })
                cdEnd = cdEnd.plus(span)
              }
              let cRange = Interval.fromDateTimes(cdStart, cdEnd)
              for(let i=0; i<dates.length; i++) {
                let eStart = DateTime.fromFormat(`${dates[i]} ${this.e?.attributeValues?.StartTime}`, `yyyy-MM-dd HH:mm:ss`)
                if(this.e?.attributeValues?.StartBuffer) {
                  eStart = eStart.minus({ minutes: parseInt(this.e?.attributeValues.StartBuffer) })
                }
                let eEnd = DateTime.fromFormat(`${dates[i]} ${this.e?.attributeValues?.EndTime}`, `yyyy-MM-dd HH:mm:ss`)
                if(this.e?.attributeValues?.EndBuffer) {
                  eEnd = eEnd.plus({ minutes: parseInt(this.e?.attributeValues.EndBuffer) })
                }
                let current = Interval.fromDateTimes(
                  eStart,
                  eEnd
                )
                if (cRange.overlaps(current)) {
                  overlaps = true
                }
              }
            })
          } else {
            events.forEach((event: any) => {
              let date = event.childContentChannelItem.attributeValues?.EventDate?.value.trim()
              let cdStart = DateTime.fromFormat(`${date} ${event.childContentChannelItem.attributeValues?.StartTime.value}`, `yyyy-MM-dd HH:mm:ss`)
              if (event.childContentChannelItem.attributeValues?.StartBuffer) {
                let span = Duration.fromObject({ minutes: parseInt(event.childContentChannelItem.attributeValues.StartBuffer.value) })
                cdStart = cdStart.minus(span)
              }
              let cdEnd = DateTime.fromFormat(`${date} ${event.childContentChannelItem.attributeValues?.EndTime.value}`, `yyyy-MM-dd HH:mm:ss`)
              if (event.childContentChannelItem.attributeValues?.EndBuffer) {
                let span = Duration.fromObject({ minutes: parseInt(event.childContentChannelItem.attributeValues.EndBuffer.value) })
                cdEnd = cdEnd.plus(span)
              }
              let cRange = Interval.fromDateTimes(cdStart, cdEnd)
              for(let i=0; i<dates.length; i++) {
                let eStart = DateTime.fromFormat(`${dates[i]} ${this.e?.attributeValues?.StartTime}`, `yyyy-MM-dd HH:mm:ss`)
                if(this.e?.attributeValues?.StartBuffer) {
                  eStart = eStart.minus({ minutes: parseInt(this.e?.attributeValues.StartBuffer) })
                }
                let eEnd = DateTime.fromFormat(`${dates[i]} ${this.e?.attributeValues?.EndTime}`, `yyyy-MM-dd HH:mm:ss`)
                if(this.e?.attributeValues?.EndBuffer) {
                  eEnd = eEnd.plus({ minutes: parseInt(this.e?.attributeValues.EndBuffer) })
                }
                let current = Interval.fromDateTimes(
                  eStart,
                  eEnd
                )
                if (cRange.overlaps(current)) {
                  overlaps = true
                }
              }
            })
          }
          return overlaps
        }
        return false
      })
      let existingInv = [] as any[]
      existingOnDate?.forEach(e => {
        e.childItems.forEach((ev: any) => {
          if(ev.childContentChannelItem?.attributeValues?.OpsInventory?.value) {
            let i = JSON.parse(ev.childContentChannelItem.attributeValues.OpsInventory.value)
            for(let k=0; k < i.length; k++) {
              let existingItem = existingInv.filter((ei: any) => {
                return ei.InventoryItem == i[k].InventoryItem
              })
              if(existingItem && existingItem.length > 0) {
                existingItem[0].QuantityNeeded += i[k].QuantityNeeded
              } else {
                existingInv.push(i[k])
              }
            }
          }
        })
      })
      this.inventory?.forEach(l => {
        let idx = -1
        inv.forEach((i, x) => {
          if (i.Type == l.type) {
            idx = x
          }
        })
        //Disable rooms not available for the date/time
        l.isHeader = false
        if(existingInv.map((ei: any) => { return ei.InventoryItem }).includes(l.value)){
          let eiArr = existingInv.filter((ei: any) => { return ei.InventoryItem == l.value })
          if(eiArr && eiArr.length > 0) {
            let ei = eiArr[0]
            let qtyOwned = parseInt(l.description)
            if(ei.QuantityNeeded < qtyOwned) {
              l.description = (qtyOwned - ei.QuantityNeeded) + "/" + l.description + " Available"
            } else {
              l.isDisabled = true
              l.description = "0/" + l.description + " Available"
            }
          }
        } else {
          l.description += "/" + l.description + " Available"
        }
        if (idx > -1) {
          inv[idx].items.push(l)
        } else {
          inv.push({ Type: l.type, items: [l], order: l.order })
        }
      })
      inv.forEach(l => {
          l.items = l.items.sort((a:any, b: any) => {
          if (a.order < b.order) {
            return -1
          } else if (a.order > b.order) {
            return 1
          } else {
            return 0
          }
        })
      })
      let arr = [] as any[]
      inv.forEach(l => {
        arr.push({ value: l.Type, isHeader: true, isDisabled: false})
        l.items.forEach((i:any) => {
          arr.push((i))
        })
      })
      return arr
    }
  },
  methods: {
    openInventoryEditor() {
      if(this.opsInventory.length == 0) {
        this.opsInventory.push({InventoryItem: "", QuantityNeeded: 0})
      }
      this.modal = true
    },
    addOpsInvConfiguration() {
      this.opsInventory.push({InventoryItem: "", QuantityNeeded: 0})
    },
    removeOpsInvConfiguration(idx: number) {
      this.opsInventory.splice(idx, 1)
    },
    getInventoryName(guid: string, qty: number) {
      if(guid) {
        let itm = this.inventoryList?.filter((i: any) => {
          return i.guid == guid
        })
        if(itm && itm.length > 0) {
          if(qty > 1) {
            let lastChar = itm[0].value.charAt(itm[0].value.length - 1)
            if(lastChar != 's') {
              return itm[0].value + 's'
            }
          }
          return itm[0].value
        }
      }
    },
    saveOpsInvConfiguration() {
      this.modal = false
      //Combine inventory rows
      let inv = [] as any[]
      this.opsInventory.forEach((i: any) => {
        let idx = -1
        inv.forEach((invt: any, indx: number) => {
          if(invt.InventoryItem == i.InventoryItem) {
            idx = indx
          }
        })
        if(idx > -1) {
          let existingQty = parseInt(inv[idx].QuantityNeeded)
          let newQty = parseInt(i.QuantityNeeded)
          inv[idx].QuantityNeeded = existingQty + newQty
        } else {
          inv.push(i)
        }
      })
      //validate quantity
      inv.forEach((i: any) => {
        let giArr = this.groupedInventory.filter((gi: any) => {
          return gi.value == i.InventoryItem
        })
        if(giArr && giArr.length > 0) {
          let qtyRequested = parseInt(i.QuantityNeeded)
          let qtyAvailable = parseInt(giArr[0].description.split("/")[0])
          if(qtyRequested > qtyAvailable) {
            i.QuantityNeeded = qtyAvailable
          }
        }
      })
      this.opsInventory = inv
    }
  },
  watch: {
    opsInventory: {
      handler(val){
        if(this.e?.attributeValues) {
          this.e.attributeValues.OpsInventory = JSON.stringify(val)
        }
      },
      deep: true
    },
    modal(val) {
      if(!val) {
        this.saveOpsInvConfiguration()
      }
    },
    'e.attributeValues.StartTime': {
      handler(val) {
        this.saveOpsInvConfiguration()
      }
    },
    'e.attributeValues.EndTime': {
      handler(val) {
        this.saveOpsInvConfiguration()
      }
    },
    'e.attributeValues.EventDate': {
      handler(val) {
        this.saveOpsInvConfiguration()
      }
    },
  },
  mounted() {
    if(this.e?.attributeValues) {
      if(this.e?.attributeValues.OpsInventory) {
        this.opsInventory = JSON.parse(this.e.attributeValues.OpsInventory)
        //Verify the quantities requested do not conflict (in case of a date or time change)
        
      }
    }
  },
  template: `
<div class="my-2 setup-table">
  <div class="row">
    <div class="col col-xs-11">
      <template v-if="opsInventory.length > 0">
        <div class="row" v-for="(o, idx) in opsInventory" :key="idx">
          <div class="col col-xs-12">
            {{o.QuantityNeeded}} {{getInventoryName(o.InventoryItem, o.QuantityNeeded)}}
          </div>
        </div>
      </template>
      <template v-else>
        Click the add button below to reserve additional items
      </template>
    </div>
    <div class="col col-xs-1">
      <a-btn class="pull-right" type="accent" shape="circle" @click="openInventoryEditor" v-if="!readonly">
        <i class="fa fa-plus"></i>
      </a-btn>
    </div>
  </div>
</div>
<a-modal v-model:visible="modal" style="min-width: 50%;">
  <tcc-ops-inv v-model="oi" v-for="(oi, idx) in opsInventory" :inventory="groupedInventory" :key="(oi.InventoryItem + '_' + idx)" v-on:removeinventoryconfig="removeOpsInvConfiguration(idx)"></tcc-ops-inv>
  <template #footer>
    <a-btn type="accent" @click="addOpsInvConfiguration">Add Row</a-btn>
    <a-btn type="primary" @click="saveOpsInvConfiguration">Save</a-btn>
  </template>
</a-modal>
<v-style>
.setup-table {
  border-radius: 6px;
  border: 1px solid #dfe0e1;
  padding: 8px;
}
.setup-row {
  display: flex;
  align-items: center;
}
.setup-row:not(:last-child) {
  border-bottom: 1px solid #F0F0F0;
}
.spacer {
  flex-grow: 1!important;
}
</v-style>
`
});
