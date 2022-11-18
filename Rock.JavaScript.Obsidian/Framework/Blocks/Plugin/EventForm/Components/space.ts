import { defineComponent, PropType} from "vue"
import { Person } from "../../../../ViewModels"
import { ContentChannelItem, DefinedValue } from "../../../../ViewModels"
import { DateTime, Duration, Interval } from "luxon"
import { useStore } from "../../../../Store/index"
import { Switch } from "ant-design-vue"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import Validator from "./validator"
import TextBox from "../../../../Elements/textBox"
import RockLabel from "../../../../Elements/rockLabel"
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
            type: Object as PropType<ContentChannelItem>,
            required: false
        },
        locations: Array as PropType<DefinedValue[]>,
        request: Object as PropType<ContentChannelItem>,
        originalRequest: Object as PropType<ContentChannelItem>,
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
        currentPerson(): Person | null {
          return store.state.currentPerson;
        },
        rooms() {
            return this.locations?.filter(l => {
                return l.attributeValues?.IsDoor == "False"
            }).map(l => {
                let x = {} as ListItem
                x.value = l.guid
                if(l.value) {
                    x.text = l.value
                    if(l.attributeValues?.Capacity) {
                        x.text += " (" + l.attributeValues?.Capacity + ")"
                    }
                }
                if(l.attributeValues?.Type) {
                    x.type = l.attributeValues.Type
                }
                if(l.attributeValues?.StandardSetUpDescription) {
                    x.description = l.attributeValues.StandardSetUpDescription
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
                                let current = Interval.fromDateTimes(
                                    DateTime.fromFormat(`${dates[i]} ${this.e?.attributeValues?.StartTime}`, `yyyy-MM-dd HH:mm:ss`),
                                    DateTime.fromFormat(`${dates[i]} ${this.e?.attributeValues?.EndTime}`, `yyyy-MM-dd HH:mm:ss`)
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
                                let current = Interval.fromDateTimes(
                                    DateTime.fromFormat(`${dates[i]} ${this.e?.attributeValues?.StartTime}`, `yyyy-MM-dd HH:mm:ss`),
                                    DateTime.fromFormat(`${dates[i]} ${this.e?.attributeValues?.EndTime}`, `yyyy-MM-dd HH:mm:ss`)
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
            let existingRooms = [] as any[]
            existingOnDate?.forEach(e => {
                e.childItems.forEach((ev: any) => {
                    existingRooms.push(...ev.childContentChannelItem.attributeValues?.Rooms?.value.split(","))
                })
            })
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
        <rck-lbl>How many people are you expecting to attend?</rck-lbl>
        <rck-text
          v-model="e.attributeValues.ExpectedAttendance"
          type="number"
        ></rck-text>
      </tcc-validator>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :rules="[rules.required(e.attributeValues.Rooms, e.attributes.Rooms.name)]" ref="validator_room">
        <tcc-room
          v-model="e.attributeValues.Rooms"
          :label="e.attributes.Rooms.name"
          :items="groupedRooms"
          :multiple="true"
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
        ></rck-field>
      </tcc-validator>
    </div>
  </div>
</rck-form>
`
});
