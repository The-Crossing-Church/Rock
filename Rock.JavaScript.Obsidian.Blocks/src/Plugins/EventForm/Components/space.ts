import { defineComponent, PropType} from "vue"
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag"
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { DateTime, Duration, Interval } from "luxon"
import { useStore } from "@Obsidian/PageState"
import { Switch, Button } from "ant-design-vue"
import RockField from "@Obsidian/Controls/rockField"
import RockForm from "@Obsidian/Controls/rockForm"
import Validator from "./validator"
import TextBox from "@Obsidian/Controls/textBox"
import RockLabel from "@Obsidian/Controls/rockLabel"
import RoomPicker from "./roomPicker"
import RoomSetUp from "./roomSetUp"
import Toggle from "./toggle"
import rules from "../Rules/rules"

const store = useStore();

type ListItem = {
  text: string,
  value: string,
  description: string,
  isDisabled: boolean,
  isHeader: boolean,
  type: string,
  order: number
}
type SelectedListItem = {
  text: string,
  value: string,
  description: string
}

export default defineComponent({
  name: "EventForm.Components.Space",
  components: {
    "a-btn": Button,
    "a-switch": Switch,
    "rck-field": RockField,
    "rck-form": RockForm,
    "rck-lbl": RockLabel,
    "rck-text": TextBox,
    "tcc-validator": Validator,
    "tcc-room": RoomPicker,
    "tcc-setup": RoomSetUp,
    "tcc-switch": Toggle
  },
  props: {
    e: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    locations: Array,
    request: Object as PropType<ContentChannelItemBag>,
    originalRequest: Object as PropType<ContentChannelItemBag>,
    existing: Array as PropType<any[]>,
    showValidation: Boolean,
    refName: String
  },
  setup() {

  },
  data() {
    return {
      rules: rules,
      errors: [] as Record<string, string>[]
    };
  },
  computed: {
    /** The person currently authenticated */
    currentPerson(): PersonBag | null {
      return store.state.currentPerson;
    },
    rooms() {
      return this.locations?.filter((l: any) => {
        return l.attributeValues?.IsDoor.value == "False"
      }).map((l: any) => {
        let x = {} as ListItem
        x.value = l.guid as string
        if(l.value) {
          x.text = l.value
          if(l.attributeValues?.Capacity.value) {
            x.text += " (" + l.attributeValues?.Capacity.value + ")"
          }
        }
        if(l.attributeValues?.Type.value) {
          x.type = l.attributeValues.Type.value
        }
        if(l.attributeValues?.StandardSetUpDescription.value) {
          x.description = l.attributeValues.StandardSetUpDescription.value
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
    groupedRooms() {        
      let loc = [] as any[]
      let dates = [] as any[]
      //Reference the values so the computed re-generates on change
      let startBuffer = this.e?.attributeValues?.StartBuffer ? parseInt(this.e?.attributeValues?.StartBuffer) : 0
      let endBuffer = this.e?.attributeValues?.EndBuffer ? parseInt(this.e?.attributeValues?.EndBuffer) : 0
      let existingRequests = JSON.parse(JSON.stringify(this.existing))

      if(this.request?.attributeValues?.IsSame == "True") {
        dates = this.request.attributeValues.EventDates.split(",").map(d => d.trim())
      } else {
        dates.push(this.e?.attributeValues?.EventDate)
      }
      let existingOnDate = existingRequests?.filter((e: any) => {
        if(e.idKey == this.request?.idKey || e.idKey == this.originalRequest?.idKey) {
          return false
        }
        let intersect = e.attributeValues?.EventDates.value.split(",").map((d: string) => d.trim()).filter((date: string) => dates.includes(date))
        if(intersect && intersect.length > 0) {
          // console.log("Intersecting Dates: " + e.title)
          // console.log(intersect)
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
              let cdStartBuffer = events[0].childContentChannelItem.attributeValues?.StartBuffer && events[0].childContentChannelItem.attributeValues?.StartBuffer?.value ? parseInt(events[0].childContentChannelItem.attributeValues?.StartBuffer.value) : 0
              let cdStart = DateTime.fromFormat(`${date} ${events[0].childContentChannelItem.attributeValues?.StartTime.value}`, `yyyy-MM-dd HH:mm:ss`).minus({ minutes: cdStartBuffer })
              let cdEndBuffer = events[0].childContentChannelItem.attributeValues?.EndBuffer && events[0].childContentChannelItem.attributeValues?.EndBuffer?.value ? parseInt(events[0].childContentChannelItem.attributeValues?.EndBuffer.value) : 0
              let cdEnd = DateTime.fromFormat(`${date} ${events[0].childContentChannelItem.attributeValues?.EndTime.value}`, `yyyy-MM-dd HH:mm:ss`).plus({ minutes: cdEndBuffer }).minus({ minutes: 1 })
              let cRange = Interval.fromDateTimes(cdStart, cdEnd)

              let eStart = DateTime.fromFormat(`${date} ${this.e?.attributeValues?.StartTime}`, `yyyy-MM-dd HH:mm:ss`).minus({ minutes: startBuffer })
              let eEnd = DateTime.fromFormat(`${date} ${this.e?.attributeValues?.EndTime}`, `yyyy-MM-dd HH:mm:ss`).plus({ minutes: endBuffer }).minus({ minutes: 1 })
              let current = Interval.fromDateTimes(
                eStart,
                eEnd
              )
              if (cRange.overlaps(current)) {
                overlaps = true
              }
            })
          } else {
            events = events.filter((event: any) => {
              let date = event.childContentChannelItem.attributeValues?.EventDate?.value.trim()
              if(intersect.includes(date)) {
                let cdStartBuffer = event.childContentChannelItem.attributeValues?.StartBuffer && event.childContentChannelItem.attributeValues?.StartBuffer?.value ? parseInt(event.childContentChannelItem.attributeValues?.StartBuffer.value) : 0
                let cdStart = DateTime.fromFormat(`${date} ${event.childContentChannelItem.attributeValues?.StartTime.value}`, `yyyy-MM-dd HH:mm:ss`).minus({ minutes: cdStartBuffer })
                let cdEndBuffer = event.childContentChannelItem.attributeValues?.EndBuffer && event.childContentChannelItem.attributeValues?.EndBuffer?.value ? parseInt(event.childContentChannelItem.attributeValues?.EndBuffer.value) : 0
                let cdEnd = DateTime.fromFormat(`${date} ${event.childContentChannelItem.attributeValues?.EndTime.value}`, `yyyy-MM-dd HH:mm:ss`).plus({ minutes: cdEndBuffer }).minus({ minutes: 1 })
                let cRange = Interval.fromDateTimes(cdStart, cdEnd)
           
                let eStart = DateTime.fromFormat(`${date} ${this.e?.attributeValues?.StartTime}`, `yyyy-MM-dd HH:mm:ss`).minus({ minutes: startBuffer })
                let eEnd = DateTime.fromFormat(`${date} ${this.e?.attributeValues?.EndTime}`, `yyyy-MM-dd HH:mm:ss`).plus({ minutes: endBuffer }).minus({ minutes: 1 })
                let current = Interval.fromDateTimes(
                  eStart,
                  eEnd
                )
                if (cRange.overlaps(current)) {
                  return true
                }
              }
              return false
            })
            if(events && events.length > 0) {
              e.childItems = events
              overlaps = true
            }
          }
          return overlaps
        }
        return false
      })
      let existingRooms = [] as any[]
      existingOnDate?.forEach((e: any) => {
        e.childItems.forEach((ev: any) => {
          existingRooms.push(...ev.childContentChannelItem.attributeValues?.Rooms?.value.split(","))
        })
      })
      // console.log('Room Conflicts')
      // console.log(existingOnDate?.map((e: any) => { 
      //   return {
      //     title: e.title, 
      //     dates: e.attributeValues.EventDates.value, 
      //     childItems: e.childItems.map((ci: any) => { 
      //       return { 
      //         rooms: ci.childContentChannelItem.attributeValues.Rooms.valueFormatted, 
      //         date: ci.childContentChannelItem.attributeValues.EventDate?.value,
      //         start: ci.childContentChannelItem.attributeValues.StartTime.valueFormatted, 
      //         end: ci.childContentChannelItem.attributeValues.EndTime.valueFormatted, 
      //         startBuffer: ci.childContentChannelItem.attributeValues.StartBuffer.valueFormatted, 
      //         endBuffer: ci.childContentChannelItem.attributeValues.EndBuffer.valueFormatted 
      //       }
      //     })
      //   } 
      // }))
      // console.log(existingRooms.filter((value, index, array) => {
      //   return array.indexOf(value) === index;
      // }))
      this.rooms?.forEach(l => {
          let idx = -1
          loc.forEach((i, x) => {
            if (i.Type == l.type) {
              idx = x
            }
          })
          //Disable rooms not available for the date/time
          l.isHeader = false
          if(existingRooms.includes(l.value)){
            l.isDisabled = true
          }
          if (idx > -1) {
            loc[idx].locations.push(l)
          } else {
            loc.push({ Type: l.type, locations: [l], order: l.order })
          }
      })
      loc.forEach(l => {
          l.locations = l.locations.sort((a:any, b: any) => {
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
      loc.forEach(l => {
        arr.push({ value: l.Type, isHeader: true, isDisabled: false})
        l.locations.forEach((i:any) => {
          arr.push((i))
        })
      })
      return arr
    },
    selectedRooms() {
      let rawVal = this.e?.attributeValues?.Rooms as string
      let selection = JSON.parse(rawVal) as SelectedListItem
      let guids = selection.value.split(',')
      return this.rooms?.filter((r: any) => {
        return guids.includes(r.value)
      })
    },
    ministry() {
      if(this.request?.attributeValues) {
        let ministry = JSON.parse(this.request.attributeValues.Ministry) as SelectedListItem
        return ministry.text
      }
      return ''
    },
  },
  methods: {
    validate() {
      let formRef = this.$refs as any
      for(let r in formRef) {
        if(formRef[r].className?.includes("validator")) {
          formRef[r].validate()
        }
      }
    },
    validationChange(errs: Record<string, string>[]) {
      this.errors = errs
    }
  },
  watch: {
    errors: {
      handler(val) {
        this.$emit("validation-change", { ref: this.refName, errors: val})
      },
      deep: true
    },
    'e.attributeValues.Rooms': {
      handler(val, oval) {
        //Find Values that were removed 
        let original = JSON.parse(oval)
        let current = JSON.parse(val)
        if(original.value) {
          original = original.value.split(',')
          if(current.value) {
            current = current.value.split(',')
          } else {
            current = []
          }
          let removed = original.filter((r: string) => { return !current.includes(r) } )
          //For removed rooms make sure they are removed from the set-up list
          if(this.e?.attributeValues?.RoomSetUp) {
            let setUp = JSON.parse(this.e?.attributeValues?.RoomSetUp as string)
            if(setUp) {
              setUp = setUp.filter((set: any) => { return !removed.includes(set.Room)})
              this.e.attributeValues.RoomSetUp = JSON.stringify(setUp)
            }
          }
        }
      },
      deep: true
    }
  },
  mounted() {
    if(this.showValidation) {
      this.validate()
    }
  },
  template: `
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.ExpectedAttendance, e.attributes.ExpectedAttendance.name), rules.attendance(e.attributeValues.ExpectedAttendance, e.attributeValues.Rooms, locations, e.attributes.ExpectedAttendance.name)]" ref="validator_att">
        <rck-field
          v-model="e.attributeValues.ExpectedAttendance"
          :attribute="e.attributes.ExpectedAttendance"
          :is-edit-mode="true"
          id="txtAttendance"
        ></rck-field>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.Rooms, e.attributes.Rooms.name)]" ref="validator_room">
        <tcc-room
          v-model="e.attributeValues.Rooms"
          :label="e.attributes.Rooms.name"
          :items="groupedRooms"
          :multiple="true"
          id="PkrRoom"
        ></tcc-room>
      </tcc-validator>
    </div>
  </div>
  <div class="row" v-if="ministry == 'Infrastructure'">
    <div class="col col-xs-12 col-md-6">
      <tcc-validator>
        <rck-field
          v-model="e.attributeValues.InfrastructureSpace"
          :attribute="e.attributes.InfrastructureSpace"
          :is-edit-mode="true"
          id="txtInfrastructureSpace"
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
</rck-form>
`
});
