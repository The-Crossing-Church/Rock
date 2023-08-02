import { defineComponent, PropType } from "vue"
import { ContentChannelItem, DefinedValue } from "../../../../ViewModels"
import { DateTime, Duration, Interval } from "luxon"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import RockLabel from "../../../../Elements/rockLabel"
import { Button, Modal, Select } from "ant-design-vue"
import Validator from "./validator"
import Toggle from "./toggle"
import rules from "../Rules/rules"
import RoomSetUp from "./roomSetUp"
import TimePicker from "./timePicker"
import OpsInv from "./opsInventory"

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
    name: "EventForm.Components.Ops",
    components: {
      "rck-field": RockField,
      "rck-form": RockForm,
      "rck-lbl": RockLabel,
      "tcc-validator": Validator,
      "tcc-switch": Toggle,
      "tcc-setup": RoomSetUp,
      "tcc-time": TimePicker,
      "tcc-ops-inv": OpsInv,
      "a-btn": Button,
      "a-modal": Modal,
      "a-select": Select,
    },
    props: {
      e: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
      locations: Array as PropType<DefinedValue[]>,
      locationSetUp: Array as PropType<any[]>,
      showValidation: Boolean,
      refName: String,
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
            return r.attributeValues?.IsDoor == "False"
          })
        }
        return []
      },
      doorsAttr() {
        if(this.e?.attributes?.Doors) {
          let attr = this.e?.attributes?.Doors
          if(attr.configurationValues && this.locations) {
            let doors = this.locations.filter((l: any) => { 
              return l.attributeValues.IsDoor == 'True' 
            }).map((l: any) => { 
              return { value: l.guid, text: l.value, description: ""}  
            })
            attr.configurationValues.values = JSON.stringify(doors)
            return attr
          }
        }
        return null
      },
      selectedRooms() {
        let rawVal = this.e?.attributeValues?.Rooms as string
        let selection = JSON.parse(rawVal) as SelectedListItem
        let guids = selection.value.split(',')
        return this.rooms?.filter((r: any) => {
          return guids.includes(r.guid)
        })
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
          })
          if(room) {
            let setUpGuid = room[0]?.attributeValues?.StandardSetUp
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
            setup = [{ Room: guid, TypeofTable: '', NumberofTables: 0, NumberofChairs: 0}]
          } else {
            setup = []
            def.forEach((s: any) => {
              setup.push({ Room: guid, TypeofTable: s.attributeValues.TypeofTable, NumberofTables: s.attributeValues.NumberofTables, NumberofChairs: s.attributeValues.NumberofChairs})
            })
          }
        }
        this.selectedRoomSetUp = JSON.parse(JSON.stringify(setup))
        this.modal = true
      },
      addSetUpConfiguration() {
        let room = this.selectedRoomSetUp[0].Room
        this.selectedRoomSetUp.push({Room: room, TypeofTable: '', NumberofTables: 0, NumberofChairs: 0})
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
                stdrSetUp.push({Room: room, TypeofTable: su.attributeValues.TypeofTable, NumberofTables: su.attributeValues.NumberofTables, NumberofChairs: su.attributeValues.NumberofChairs })
              })
            }
          }
        })
        stdrSetUp = stdrSetUp.sort((a: any, b: any) => this.sortSetUp(a, b))
        this.selectedRoomSetUp = this.selectedRoomSetUp.sort((a: any, b: any) => this.sortSetUp(a, b))
        let isSame = true
        if(stdrSetUp.length != this.selectedRoomSetUp.length) {
          isSame = false
        }
        if(JSON.stringify(stdrSetUp) != JSON.stringify(this.selectedRoomSetUp)) {
          isSame = false
        }
        if(!isSame) {
          this.roomSetUp.push(...this.selectedRoomSetUp)
        }
        this.selectedRoomSetUp = []
      },
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
      },
      sortSetUp(a: any, b: any): number {
        if(a.TypeofTable < b.TypeofTable) {
          return -1
        } else if(a.TypeofTable > b.TypeofTable) {
          return 1
        }
        if(parseInt(a.NumberofTables) < parseInt(b.NumberofTables)) {
          return -1
        } else if(parseInt(a.NumberOfTables) > parseInt(b.NumberOfTables)) {
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
      'e.attributeValues.HasDangerousActivity': {
        handler(val) {
          if(val == 'False') {
            if(this.e?.attributeValues) {
              this.e.attributeValues.DangerousActivityInfo = ""
              this.e.attributeValues.InsuranceCertificate = ""
            }
          }
        }
      },
      'e.attributeValues.Rooms': {
        handler(val) {
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
    mounted() {
      if(this.e?.attributeValues) {
        if(this.e.attributeValues.RoomSetUp) {
          this.roomSetUp = JSON.parse(this.e.attributeValues.RoomSetUp)
        }
        this.matchRoomsToSetup()
      }
      if(this.showValidation) {
        this.validate()
      }
    },
    template: `
<rck-form ref="form" @validationChanged="validationChange">
  <h4 class="text-accent">Tech Needs</h4>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.RoomTech"
        :attribute="e.attributes.RoomTech"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.TechNeeds"
        :attribute="e.attributes.TechNeeds"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
      ></rck-field>
    </div>
  </div>
  <h4 class="text-accent mt-2">Additional Set-Up Details</h4>
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
              <template v-if="su.attributeValues.NumberofTables > 1">
                {{su.attributeValues.NumberofTables}} {{su.attributeValues.TypeofTable}} tables with {{su.attributeValues.NumberofChairs}} chairs each.
              </template>
              <template v-else>
                {{su.attributeValues.NumberofTables}} {{su.attributeValues.TypeofTable}} table with {{su.attributeValues.NumberofChairs}} charis.
              </template>
            </div>
          </div>
          <div class="col col-xs-1">
            <a-btn shape="circle" type="accent" @click="configureRoomSetUp(gsu.guid)" v-if="!readonly">
              <i class="fa fa-pencil-alt"></i>
            </a-btn>
          </div>
        </template>
        <template v-else>
          <div class="col col-xs-11">
            <rck-lbl>Custom Set-Up for {{gsu.room}}: </rck-lbl><br/>
            <div v-for="(su, idx) in gsu.items" :key="idx">
              <template v-if="su.NumberofTables > 1">
                {{su.NumberofTables}} {{su.TypeofTable}} tables with {{su.NumberofChairs}} chairs each.
              </template>
              <template v-else>
                {{su.NumberofTables}} {{su.TypeofTable}} table with {{su.NumberofChairs}} charis.
              </template>
            </div>
          </div>
          <div class="col col-xs-1">
            <a-btn shape="circle" type="accent" @click="configureRoomSetUp(gsu.guid)" v-if="!readonly">
              <i class="fa fa-pencil-alt"></i>
            </a-btn>
          </div>
        </template>
      </div>
    </template>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.Tablecloths"
        :label="e.attributes.Tablecloths.name"
        v-if="!readonly"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.Tablecloths"
        :attribute="e.attributes.Tablecloths"
        :is-edit-mode="false"
        :showEmptyValue="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsDoorsUnlocked"
        :label="e.attributes.NeedsDoorsUnlocked.name"
        v-if="!readonly"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsDoorsUnlocked"
        :attribute="e.attributes.NeedsDoorsUnlocked"
        :is-edit-mode="false"
        :showEmptyValue="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 mb-2" v-if="e.attributeValues.NeedsDoorsUnlocked == 'True'">
      <strong>Operations will unlock doors for your event based on the spaces and rooms you have reserved. If you have special unlock instructions for your event, please indicate below.</strong><br/>
      <rck-field
        v-model="e.attributeValues.Doors"
        :attribute="doorsAttr"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsSecurity"
        :label="e.attributes.NeedsSecurity.name"
        v-if="!readonly"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.NeedsSecurity"
        :attribute="e.attributes.NeedsSecurity"
        :is-edit-mode="false"
        :showEmptyValue="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.HasDangerousActivity"
        :label="e.attributes.HasDangerousActivity.name"
        v-if="!readonly"
      ></tcc-switch>
      <rck-field
        v-else
        v-model="e.attributeValues.HasDangerousActivity"
        :attribute="e.attributes.HasDangerousActivity"
        :is-edit-mode="false"
        :showEmptyValue="true"
      ></rck-field>
    </div>
  </div>
  <div class="row" v-if="e.attributeValues.HasDangerousActivity == 'True'">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.DangerousActivityInfo"
        :attribute="e.attributes.DangerousActivityInfo"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.Setup"
        :attribute="e.attributes.Setup"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.SetupImage"
        :attribute="e.attributes.SetupImage"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.HasDangerousActivity == 'True'">
      <rck-field
        v-model="e.attributeValues.InsuranceCertificate"
        :attribute="e.attributes.InsuranceCertificate"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
      ></rck-field>
    </div>
  </div>
  <template v-if="request.attributeValues.NeedsCatering == 'False'">
    <h4 class="text-accent mt-2">Refreshments</h4>
    <div class="row">
      <div class="col col-xs-12 col-md-6">
        <rck-field
          v-model="e.attributeValues.Drinks"
          :attribute="e.attributes.Drinks"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
        ></rck-field>
      </div>
      <div class="col col-xs-12 col-md-6">
        <tcc-validator :rules="[rules.drinkTimeRequired(e.attributeValues.DrinkTime, e.attributeValues.Drinks, e.attributes.DrinkTime.name)]" ref="validator_drinktime" v-if="!readonly">
          <tcc-time 
            :label="e.attributes.DrinkTime.name"
            v-model="e.attributeValues.DrinkTime"
          ></tcc-time>
        </tcc-validator>
        <rck-field
          v-else
          v-model="e.attributeValues.DrinkTime"
          :attribute="e.attributes.DrinkTime"
          :is-edit-mode="false"
          :showEmptyValue="true"
        ></rck-field>
      </div>
    </div>
    <div class="row">
      <div class="col col-xs-12 col-md-6">
        <rck-field
          v-model="e.attributeValues.DrinkSetupLocation"
          :attribute="e.attributes.DrinkSetupLocation"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
        ></rck-field>
      </div>
    </div>
  </template>
  <h4 class="text-accent mt-2">Ops Inventory</h4>
  <tcc-ops-inv :e="e" :request="request" :originalRequest="originalRequest" :inventoryList="inventoryList" :existing="existing" :readonly="readonly"></tcc-ops-inv>
</rck-form>
<a-modal v-model:visible="modal" style="min-width: 50%;">
  <div style="height: 16px;"></div>
  <div class="text-center mb-2">
    <i>
      Please Note: configuring a custom set-up will automatically add a 30 minute buffer to your reservation.<br/> 
      It may impact the rooms/spaces available to you.
    </i>
  </div>
  <tcc-setup v-model="su" v-for="(su, idx) in selectedRoomSetUp" :key="(su.Room + '_' + idx + '_' + Math.random())" v-on:removeconfig="removeSetUpConfiguration(idx)"></tcc-setup>
  <template #footer>
    <div style="display: flex;">
      <a-btn type="secondary" @click="useStandardSetUp">Use Standard Set-up</a-btn>
      <div class="spacer"></div>
      <a-btn type="accent" @click="addSetUpConfiguration">Add Row</a-btn>
      <a-btn type="primary" @click="saveSetUpConfiguration">Save</a-btn>
    </div>
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
