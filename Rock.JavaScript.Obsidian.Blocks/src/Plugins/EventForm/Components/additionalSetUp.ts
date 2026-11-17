import { defineComponent, PropType } from "vue"
import { DefinedValueBag } from "../../ViewModels/definedValueBag"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import rules from "../Rules/rules"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import Modal from "@Obsidian/Controls/modal.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import Validator from "./validator"
import RoomSetUp from "./roomSetUp"

type SelectedListItem = {
  text: string,
  value: string,
  description: string
}
type RoomSetUp = {
  Room: string,
  TypeofTable: string,
  NumberofTables: number,
  NumberofChairs: number,
  NeedsTablecloths: string
}

export default defineComponent({
    name: "EventForm.Components.Ops",
    components: {
      "rck-lbl": RockLabel,
      "tcc-validator": Validator,
      "tcc-setup": RoomSetUp,
      "rck-btn": RockButton,
      "rck-modal": Modal
    },
    props: {
      e: {
        type: Object as PropType<ContentChannelItemBag>,
        required: false
      },
      locations: Array,
      locationSetUp: Array as PropType<any[]>,
      showValidation: Boolean,
      refName: String,
      request: Object as PropType<ContentChannelItemBag>,
      originalRequest: Object as PropType<ContentChannelItemBag>,
      existing: Array as PropType<any[]>,
      readonly: Boolean
    },
    setup() {

    },
    data() {
      return {
        roomSetUp: [] as RoomSetUp[],
        selectedRoomSetUp: [] as RoomSetUp[],
        rules: rules,
        errors: [] as Record<string, string>[],
        modal: false
      };
    },
    computed: {
      rooms() {
        if(this.locations) {
          return this.locations.filter((r: any) => {
            return r.attributeValues?.IsDoor.value == "False"
          })
        }
        return []
      },
      selectedRooms() {
        let rawVal = this.e?.attributeValues?.Rooms as string
        if(rawVal && rawVal != '') {
          let selection = JSON.parse(rawVal) as SelectedListItem
          let guids = selection.value.split(',')
          return this.rooms?.filter((r: any) => {
            return guids.includes(r.guid)
          })
        } else {
          return []
        }
      },
      groupedSetUp() {
        let arr = [] as any[]
        this.selectedRooms.forEach((r: any) => {
          arr.push({room: r.value, guid: r.guid, items: this.setUpForRoom(r.guid), standard: this.getSetUpDesc(r.guid)})
        })
        return arr
      },
    },
    methods: {
      matchRoomsToSetup() {
        let roomGuids = this.selectedRooms?.map((r: any) => { return r.guid})
        this.roomSetUp = this.roomSetUp.filter((r: any) => {
          return roomGuids?.includes(r.Room)
        })
      },
      getSetUpDesc(guid: string) {
        if(this.rooms) {
          let room = this.rooms?.filter((r: any) => {
            return r.guid == guid
          }) as any[]
          if(room) {
            let setUpGuid = room[0]?.attributeValues?.StandardSetUp.value
            let setUp = this.locationSetUp?.filter((r: any) => {
              return r.guid == setUpGuid
            })
            if(setUp && setUp.length > 0) {
              return setUp[0]?.matrixItems
            }
          }
        }
        return []
      },
      setUpForRoom(guid: string) {
        if(this.roomSetUp) {
          let setUp = this.roomSetUp.filter((r: any) => {
            return r.Room == guid
          })
          if(setUp && setUp.length > 0) {
            return setUp
          }
        }
        return null
      },
      configureRoomSetUp(guid: string) {
        let setup = this.roomSetUp.filter((r: any) => {
          return r.Room == guid
        })
        if(setup.length == 0) {
          //Set to default if exists
          let def = this.getSetUpDesc(guid)
          if(def.length == 0) {
            setup = [{ Room: guid, TypeofTable: '', NumberofTables: 0, NumberofChairs: 0, NeedsTablecloths: 'False'}]
          } else {
            setup = []
            def.forEach((s: any) => {
              setup.push({ Room: guid, TypeofTable: s.attributeValues.TypeofTable.value, NumberofTables: s.attributeValues.NumberofTables.value, NumberofChairs: s.attributeValues.NumberofChairs.value, NeedsTablecloths: s.attributeValues.NeedsTablecloths.value })
            })
          }
        }
        this.selectedRoomSetUp = JSON.parse(JSON.stringify(setup))
        this.modal = true
      },
      addSetUpConfiguration() {
        let room = this.selectedRoomSetUp[0].Room
        this.selectedRoomSetUp.push({Room: room, TypeofTable: '', NumberofTables: 0, NumberofChairs: 0, NeedsTablecloths: 'False'})
      },
      removeSetUpConfiguration(idx: number) {
        this.selectedRoomSetUp.splice(idx, 1)
      },
      useStandardSetUp() {
        this.modal = false
        let room = this.selectedRoomSetUp[0].Room
        this.roomSetUp = this.roomSetUp.filter((s: any) => {
          return s.Room != room
        })
      },
      saveSetUpConfiguration() {
        this.modal = false
        let room = this.selectedRoomSetUp[0].Room
        this.roomSetUp = this.roomSetUp.filter((s: any) => {
          return s.Room != room
        })
        //Check that set up is not the same as standard 
        let stdrSetUp = [] as any[]
        this.groupedSetUp.forEach((s: any) => {
          if(s.guid == room) {
            if(s.standard.length > 0) {
              s.standard.forEach((su: any) => {
                stdrSetUp.push({Room: room, TypeofTable: su.attributeValues.TypeofTable.value, NumberofTables: su.attributeValues.NumberofTables.value, NumberofChairs: su.attributeValues.NumberofChairs.value, NeedsTablecloths: su.attributeValues.NeedsTablecloths.value })
              })
            }
          }
        })
        stdrSetUp = stdrSetUp.sort((a: any, b: any) => this.sortSetUp(a, b))
        this.selectedRoomSetUp.forEach((rsu: any) => {
          rsu.NumberofTables = !rsu.NumberofTables ? 0 : rsu.NumberofTables
          rsu.NumberofChairs = !rsu.NumberofChairs ? 0 : rsu.NumberofChairs
        })
        this.selectedRoomSetUp = this.selectedRoomSetUp.sort((a: any, b: any) => this.sortSetUp(a, b))
        let isSame = true
        if(stdrSetUp.length != this.selectedRoomSetUp.length) {
          isSame = false
        }
        if(JSON.stringify(stdrSetUp) != JSON.stringify(this.selectedRoomSetUp)) {
          isSame = false
        }
        //Check for empty room match 
        let summedSetUp = this.selectedRoomSetUp.reduce((accummulator, currentItem) => {
          return { NumberofTables: accummulator.NumberofTables + currentItem.NumberofTables, NumberofChairs: accummulator.NumberofChairs + currentItem.NumberofChairs }
        }, { NumberofTables: 0, NumberofChairs: 0})
        if(summedSetUp.NumberofTables == 0 && summedSetUp.NumberofChairs == 0 && stdrSetUp.length == 0) {
          isSame = true
        }
        //Remove extra empty rows
        if(this.selectedRoomSetUp.length > 1) {
          if(summedSetUp.NumberofTables == 0 && summedSetUp.NumberofChairs == 0) {
            console.log('Removing extra empty rooms when multiple empty rows')
            this.selectedRoomSetUp = this.selectedRoomSetUp.filter((rsu: any, idx: Number) => {
              return idx == 0
            })
          } else {
            console.log('Removing empty row in middle of other set up')
            this.selectedRoomSetUp = this.selectedRoomSetUp.filter((rsu: any) => {
              return rsu.NumberofTables > 0 || rsu.NumberofChairs > 0
            })
          }
        }
        if(!isSame) {
          this.roomSetUp.push(...this.selectedRoomSetUp)
        }
        this.selectedRoomSetUp = []
      },
      sortSetUp(a: any, b: any): number {
        if(a.TypeofTable < b.TypeofTable) {
          return -1
        } else if(a.TypeofTable > b.TypeofTable) {
          return 1
        }
        if(parseInt(a.NumberofTables) < parseInt(b.NumberofTables)) {
          return -1
        } else if(parseInt(a.NumberofTables) > parseInt(b.NumberofTables)) {
          return 1
        }
        if(parseInt(a.NumberofChairs) < parseInt(b.NumberofChairs)) {
          return -1
        } else if(parseInt(a.NumberofChairs) > parseInt(b.NumberofChairs)) {
          return 1
        }
        return 0
      },
    },
    watch: {
      roomSetUp: {
        handler(val){
          if(this.e?.attributeValues) {
            this.e.attributeValues.RoomSetUp = JSON.stringify(val)
            if(val.length > 0) {
              let startBuffer = this.e.attributeValues.StartBuffer ? parseInt(this.e.attributeValues.StartBuffer) : 0
              let endBuffer = this.e.attributeValues.EndBuffer ? parseInt(this.e.attributeValues.EndBuffer) : 0
              if(startBuffer < 30) {
                this.e.attributeValues.StartBuffer = "30"
              }
              if(endBuffer < 30) {
                this.e.attributeValues.EndBuffer = "30"
              }
            }
          }
        },
        deep: true
      },
      errors: {
        handler(val) {
          this.$emit("validation-change", { ref: this.refName, errors: val})
        },
        deep: true
      },
      'e.attributeValues.Rooms': {
        handler(val) {
          if(val) {
            let rooms = JSON.parse(val)
            if(rooms.value) {
              rooms = rooms.value.split(',')
            } else {
              rooms = []
            }
            this.roomSetUp = this.roomSetUp.filter((set: any) => {
              return rooms.includes(set.Room)
            })
          }
        }
      },
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
<div class="my-2 setup-table">
  <template v-if="selectedRooms.length == 0">
    <rck-lbl>
      Select a room to configure the set-up.
    </rck-lbl>
  </template>
  <template v-else>
    <div class="row py-2 mx-2 setup-row" v-for="gsu in groupedSetUp" :key="gsu.guid">
      <template v-if="gsu.items == null">
        <div class="col col-xs-11">
          <rck-lbl>Standard Set-Up will be used for {{gsu.room}}</rck-lbl> <br/>
          <div v-for="(su, idx) in gsu.standard" :key="idx">
            <template v-if="su.attributeValues.NumberofTables.value > 1">
              {{su.attributeValues.NumberofTables.value}} {{ (su.attributeValues.NeedsTablecloths.value == 'True' ? 'Clothed ' : '') }} {{su.attributeValues.TypeofTable.value}} tables with {{su.attributeValues.NumberofChairs.value}} chairs each.
            </template>
            <template v-else>
              {{su.attributeValues.NumberofTables.value}} {{ (su.attributeValues.NeedsTablecloths.value == 'True' ? 'Clothed ' : '') }} {{su.attributeValues.TypeofTable.value}} table with {{su.attributeValues.NumberofChairs.value}} chairs.
            </template>
          </div>
        </div>
        <div class="col col-xs-1">
          <rck-btn class="btn-circle pull-right" btnType="accent" @click="configureRoomSetUp(gsu.guid)" v-if="!readonly" :id="'btnEditSetUp_' + gsu.guid">
            <i class="fa fa-pencil-alt"></i>
          </rck-btn>
        </div>
      </template>
      <template v-else>
        <div class="col col-xs-11">
          <rck-lbl>Custom Set-Up for {{gsu.room}}: </rck-lbl><br/>
          <div v-for="(su, idx) in gsu.items" :key="idx">
            <template v-if="su.NumberofTables > 1">
              {{su.NumberofTables}} {{ (su.NeedsTablecloths == 'True' ? 'Clothed ' : '') }} {{su.TypeofTable}} tables with {{su.NumberofChairs}} chairs each.
            </template>
            <template v-else-if="su.NumberofTables == 0">
              <template v-if="su.NumberofChairs > 0">
                {{su.NumberofChairs}} chairs.
              </template>
              <template v-else>
                Empty room.
              </template>
            </template>
            <template v-else>
              {{su.NumberofTables}} {{ (su.NeedsTablecloths == 'True' ? 'Clothed ' : '') }} {{su.TypeofTable}} table with {{su.NumberofChairs}} chairs.
            </template>
          </div>
        </div>
        <div class="col col-xs-1">
          <rck-btn class="btn-circle pull-right" btnType="accent" @click="configureRoomSetUp(gsu.guid)" v-if="!readonly" :id="'btnEditSetUp_' + gsu.guid">
            <i class="fa fa-pencil-alt"></i>
          </rck-btn>
        </div>
      </template>
    </div>
  </template>
</div>

<rck-modal v-model="modal" style="min-width: 50%;" :isCloseButtonHidden="true" cancelText="" :clickBackdropToClose="true" modalWrapperClasses="modal-no-header">
  <div style="height: 16px;"></div>
  <tcc-setup v-model="su" v-for="(su, idx) in selectedRoomSetUp" :key="(su.Room + '_' + idx + '_' + Math.random())" v-on:removeconfig="removeSetUpConfiguration(idx)" :disabled="selectedRoomSetUp.length == 1"></tcc-setup>
  <div class="text-center mt-4 mb-0 alert alert-danger">
    <i>
      Please Note: Configuring a custom set-up will automatically add a 30 minute buffer to your reservation. It may impact the rooms/spaces available to you.
    </i>
  </div>
  <template #customButtons>
    <div style="display: flex;">
      <rck-btn btnType="secondary" @click="useStandardSetUp" id="btnUseStandard">Use Standard Set-up</rck-btn>
      <div class="spacer"></div>
      <rck-btn btnType="accent" @click="addSetUpConfiguration" id="btnAddRowToSetup">Add Row</rck-btn>
      <rck-btn btnType="primary" @click="saveSetUpConfiguration" id="btnSaveSetup">Save</rck-btn>
    </div>
  </template>
</rck-modal>
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
  #ddlDoors label.checkbox-inline, #ddlDrinks label.checkbox-inline {
    line-height: 24px;
  }
</v-style>
`
});