import { defineComponent, PropType} from "vue";
import { Person } from "../../../../ViewModels";
import { ContentChannelItem, DefinedValue, AttributeValue } from "../../../../ViewModels"
import { SubmissionFormBlockViewModel } from "../submissionFormBlockViewModel";
import { DateTime, Duration, Interval } from "luxon";
import { useStore } from "../../../../Store/index";
import { Switch } from "ant-design-vue";
import RockForm from "../../../../Controls/rockForm";
import RockField from "../../../../Controls/rockField";
import RockFormField from "../../../../Elements/rockFormField";
import TextBox from "../../../../Elements/textBox";
import RockLabel from "../../../../Elements/rockLabel";
import RoomPicker from "./roomPicker"
import RoomSetUp from "./roomSetUp"
import Toggle from "./toggle"

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

type RoomSetUp = {
    Room: string,
    TypeofTable: string,
    NumberofTables: number,
    NumberofChairs: number
}

export default defineComponent({
    name: "EventForm.Components.Space",
    components: {
        "a-switch": Switch,
        "rck-field": RockField,
        "rck-form-field": RockFormField,
        "rck-form": RockForm,
        "rck-lbl": RockLabel,
        "rck-text": TextBox,
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
        existing: Array as PropType<any[]>
    },
    setup() {

    },
    data() {
        return {
            roomSetUp: [] as RoomSetUp[]
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
                if(l.attributeValues?.StandardSetUp) {
                    x.description = l.attributeValues.StandardSetUp
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
                dates = this.request.attributeValues.EventDates.split(",")
            } else {
                dates.push(this.e?.attributeValues?.EventDate)
            }
            let existingOnDate = this.existing?.filter(e => {
                if(e.id == this.request?.id) {
                    return false
                }
                let intersect = e.attributeValues?.EventDates.value.split(",").filter((date: string) => dates.includes(date.trim()))
                if(intersect && intersect.length > 0) {
                    //Filter to events object for the matching dates
                    let events = []
                    if(e.attributeValues?.IsSame.value == "True") {
                        events = e.childItems
                    } else {
                        events = e.childItems.filter((child: any) => dates.includes(child.childContentChannelItem.attributeValues?.EventDate.value))
                    }
                    //Check if the times overlap
                    let overlaps = false
                    events.forEach((event: any, idx: number) => {
                        let date = event.childContentChannelItem.attributeValues?.EventDate?.value.trim()
                        if(e.attributeValues?.IsSame.value == 'True' && intersect) {
                            date = intersect[idx].trim()
                        }
                        let cdStart = DateTime.fromFormat(`${date} ${event.childContentChannelItem.attributeValues?.StartTime.value}`, `yyyy-MM-dd HH:mm:ss`)
                        if (event.childContentChannelItem.attributeValues?.MinsStartBuffer) {
                            let span = Duration.fromObject({ minutes: parseInt(event.childContentChannelItem.attributeValues.MinsStartBuffer.value) })
                            cdStart = cdStart.minus(span)
                        }
                        let cdEnd = DateTime.fromFormat(`${date} ${event.childContentChannelItem.attributeValues?.EndTime.value}`, `yyyy-MM-dd HH:mm:ss`)
                        if (event.childContentChannelItem.attributeValues?.MinsEndBuffer) {
                            let span = Duration.fromObject({ minutes: parseInt(event.childContentChannelItem.attributeValues.MinsEndBuffer.value) })
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
                    return overlaps
                }
                return false
            })
            console.log(existingOnDate)
            let existingRooms = [] as any[]
            existingOnDate?.forEach(e => {
                e.childItems.forEach((ev: any) => {
                    existingRooms.push(...ev.childContentChannelItem.attributeValues?.Rooms?.value.split(","))
                })
            })
            console.log(existingRooms)
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
        canRequestTables() {
            let submissionDate = DateTime.now()
            if(this.request?.id && this.request?.id > 0 && this.request?.startDateTime && this.request?.attributeValues?.RequestStatus != "Draft") {
                submissionDate = DateTime.fromFormat(this.request?.startDateTime.split("T")[0], "yyyy-MM-dd")
            }
            let eventDate = {} as DateTime 
            if(this.e?.attributeValues?.EventDate) {
                eventDate = DateTime.fromFormat(this.e?.attributeValues?.EventDate, "yyyy-MM-dd")
            } else {
                let dates = this.request?.attributeValues?.EventDates.split(",").map((d: any) => {
                    return DateTime.fromFormat(d, "yyyy-MM-dd")
                }).sort()
                if(dates && dates.length > 0) {
                    eventDate = dates[0]
                }
            }
            let span = Duration.fromObject({days: 14})
            submissionDate = submissionDate.plus(span)
            if(eventDate >= submissionDate) {
                return true
            }
            return false
        },
        ministry() {
            if(this.request?.attributeValues) {
                let ministry = JSON.parse(this.request.attributeValues.Ministry) as SelectedListItem
                return ministry.text
            }
            return ''
        }
    },
    methods: {
        matchRoomsToSetup() {
            this.selectedRooms?.forEach((r: any) => {
                let exists = this.roomSetUp?.filter((s: any) => {
                    return s.Room == r.value
                })
                if(exists.length == 0) {
                    this.roomSetUp.push({Room: r.value, TypeofTable: "Round", NumberofTables: 0, NumberofChairs: 0})
                    this.roomSetUp.push({Room: r.value, TypeofTable: "Rectangular", NumberofTables: 0, NumberofChairs: 0})
                }
            })
            let roomGuids = this.selectedRooms?.map((r: any) => { return r.value})
            this.roomSetUp = this.roomSetUp.filter((r: any) => {
                return roomGuids?.includes(r.Room)
            })
        },
        getRoomName(guid: string) {
            if(this.selectedRooms) {
                let rooms = this.selectedRooms?.filter((r: any) => {
                    return r.value == guid
                })
                if(rooms) {
                    return rooms[0]?.text
                }
            }
            return ""
        }
    },
    watch: {
        selectedRooms: {
            handler(val) {
                this.matchRoomsToSetup()
            },
            deep: true
        },
        roomSetUp: {
            handler(val){
                if(this.e?.attributeValues) {
                    this.e.attributeValues.RoomSetUp = JSON.stringify(val)
                }
            },
            deep: true
        }
    },
    mounted() {
        if(this.e?.attributeValues) {
            if(this.e.attributeValues.RoomSetUp) {
                this.roomSetUp = JSON.parse(this.e.attributeValues.RoomSetUp)
            }
            this.matchRoomsToSetup()
        }
    },
    template: `
<rck-form>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-form-field name="ExpectedAttendance">
        <rck-lbl>How many people are you expecting to attend?</rck-lbl>
        <rck-text
          v-model="e.attributeValues.ExpectedAttendance"
          type="number"
        ></rck-text>
      </rck-form-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-form-field name="Rooms">
        <tcc-room
          v-model="e.attributeValues.Rooms"
          :label="e.attributes.Rooms.name"
          :items="groupedRooms"
          :multiple="true"
        ></tcc-room>
      </rck-form-field>
    </div>
  </div>
  <div class="row" v-if="ministry == 'Infrastructure'">
    <div class="col col-xs-12 col-md-6">
      <rck-form-field>
        <rck-field
          v-model="e.attributeValues.InfrastructureSpace"
          :attribute="e.attributes.InfrastructureSpace"
          :is-edit-mode="true"
        ></rck-field>
      </rck-form-field>
    </div>
  </div>
  <br />
  <template v-if="canRequestTables && selectedRooms.length > 0">
    <h6 class="hover" data-toggle="collapse" data-target="#setUpCollapse" aria-expanded="false" aria-controls="setUpCollapse">Room Set-up (Click to Expand)</h6>
    <div class="collapse" id="setUpCollapse">
      <template v-for="(r, idx) in roomSetUp" :key="idx">
        <div class="row">
          <div class="col col-xs-12">
          <rck-lbl v-if="idx%2 == 0">Set Up for {{getRoomName(r.Room)}}</rck-lbl>
          <tcc-setup 
            v-model="r"
          ></tcc-setup>
          </div>
        </div>
      </template>
    </div>
    <br/>
    <tcc-switch
      v-model="e.attributeValues.Tablecloths"
      :label="e.attributes.Tablecloths.name"
    ></tcc-switch>
  </template>
</rck-form>
`
});
