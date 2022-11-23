import { defineComponent, PropType } from "vue"
import { ContentChannelItem, DefinedValue } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"
import RockForm from "../../../../Controls/rockForm"
import RockLabel from "../../../../Elements/rockLabel"
import { Button, Modal, Select } from "ant-design-vue"
import Validator from "./validator"
import Toggle from "./toggle"
import rules from "../Rules/rules"
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
  NumberofChairs: number
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
      refName: String
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
      }
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
        this.selectedRoomSetUp = this.selectedRoomSetUp.splice(idx, 1)
      },
      saveSetUpConfiguration() {
        this.modal = false
        let room = this.selectedRoomSetUp[0].Room
        this.roomSetUp = this.roomSetUp.filter((s: any) => {
          return s.Room != room
        })
        this.roomSetUp.push(...this.selectedRoomSetUp)
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
      }
    },
    watch: {
      roomSetUp: {
        handler(val){
          if(this.e?.attributeValues) {
            this.e.attributeValues.RoomSetUp = JSON.stringify(val)
          }
        },
        deep: true
      },
      errors: {
        handler(val) {
          this.$emit("validation-change", { ref: this.refName, errors: val})
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
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.TechNeeds"
        :attribute="e.attributes.TechNeeds"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <h4 class="text-accent mt-2">Set-Up</h4>
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
            <a-btn shape="circle" type="accent" @click="configureRoomSetUp(gsu.guid)">
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
            <a-btn shape="circle" type="accent" @click="configureRoomSetUp(gsu.guid)">
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
      ></tcc-switch>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsDoorsUnlocked"
        :label="e.attributes.NeedsDoorsUnlocked.name"
      ></tcc-switch>
    </div>
    <div class="col col-xs-12 col-md-6" v-if="e.attributeValues.NeedsDoorsUnlocked == 'True'">
      <rck-field
        v-model="e.attributeValues.Doors"
        :attribute="doorsAttr"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.Setup"
        :attribute="e.attributes.Setup"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.SetupImage"
        :attribute="e.attributes.SetupImage"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
</rck-form>
<a-modal v-model:visible="modal" style="min-width: 50%;">
  <div class="mt-2" style="height: 16px;"></div>
  <tcc-setup v-model="su" v-for="(su, idx) in selectedRoomSetUp" :key="idx" v-on:removeconfig="removeSetUpConfiguration(idx)"></tcc-setup>
  <template #footer>
    <a-btn type="accent" @click="addSetUpConfiguration">Add Row</a-btn>
    <a-btn type="primary" @click="saveSetUpConfiguration">Save</a-btn>
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
</v-style>
`
});
