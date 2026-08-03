import { defineComponent, PropType } from "vue";
import RockField from "@Obsidian/Controls/rockField.obs"

type SelectedListItem = {
  text: string,
  value: string,
  description: string
}

export default defineComponent({
    name: "EventDashboard.Components.Modal.SpaceInfo",
    components: {
      "rck-field": RockField
    },
    props: {
      details: Object,
      rooms: Array as PropType<SelectedListItem[]>,
      requestValidation: Object,
      index: Number
    },
    setup() {

    },
    data() {
      return {
        
      };
    },
    computed: {
      spaceAttrs() {
        let attrs = [] as any[]
        if(this.details?.attributes && this.details.attributeValues) {
          for(let key in this.details.attributes) {
            let attr = this.details.attributes[key]
            let item = { attr: attr, value: "", changeValue: "", class: "", errors: [] as any[] }
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Space")) {
              if(key == "Tablecloths") {
                if(this.roomSetUp.length > 0) {
                  item.value = this.details.attributeValues[key]
                  if(this.details.changes && this.details.changes.attributeValues[key] != this.details.attributeValues[key]) {
                    item.changeValue = this.details.changes.attributeValues[key]
                    item.class = 'text-red'
                  }
                  if(!this.sectionIsValid && this.requestValidation) {
                    let errs = [] as any[]
                    this.requestValidation?.errors?.forEach(err => { 
                      let errorsApply = false
                      if(err.ref.includes("_")) {
                        let idx = err.ref.split("_")[1]
                        if(idx == this.index) {
                          errorsApply = true
                        }
                      } else {
                        errorsApply = true
                      }
                      if(errorsApply) {
                        errs.push(...err.errors.filter(e => {
                          return e.name == item.attr.key
                        }))
                      }
                    })
                    if(errs && errs.length > 0) {
                      item.class += ' label-red'
                      item.errors = errs
                    }
                  } 
                  attrs.push(item)
                }
              } else {
                item.value = this.details.attributeValues[key]
                if(this.details.changes && this.details.changes.attributeValues[key] != this.details.attributeValues[key]) {
                  item.changeValue = this.details.changes.attributeValues[key]
                    item.class = 'text-red'
                }
                if(!this.sectionIsValid && this.requestValidation) {
                  let errs = [] as any[]
                  this.requestValidation?.errors?.forEach(err => { 
                    let errorsApply = false
                    if(err.ref.includes("_")) {
                      let idx = err.ref.split("_")[1]
                      if(idx == this.index) {
                        errorsApply = true
                      }
                    } else {
                      errorsApply = true
                    }
                    if(errorsApply) {
                      errs.push(...err.errors.filter(e => {
                        return e.name == item.attr.key
                      }))
                    }
                  })
                  if(errs && errs.length > 0) {
                    item.class += ' label-red'
                    item.errors = errs
                  }
                } 
                attrs.push(item)
              }
            }
          }
        }
        return attrs.sort((a,b) => a.attr.order - b.attr.order)
      },
      roomSetUp(): any {
        if(this.details?.attributeValues.RoomSetUp) {
          let parsed = JSON.parse(this.details.attributeValues.RoomSetUp)
          parsed = parsed.filter((i: any) => {
            return i.NumberofTables > 0 
          })
          let grouped = [] as any[]
          parsed.forEach((setup: any) => {
            let idx = -1
            let room = this.getRoom(setup.Room)
            grouped.forEach((g: any, i: number) => {
              if(g.Room == room) {
                idx = i
              }
            })
            if(idx != -1) {
              //Add a new table config to the existing room
              grouped[idx].SetUp.push(setup)
            } else {
              grouped.push({Room: room, SetUp: [ setup ]})
            }
          })
          return grouped
        }
        return []
      },
      sectionIsValid() {
        if(this.requestValidation?.invalidSections) {
          if(this.requestValidation.invalidSections.includes("Space")) {
            return false
          } else {
            return true
          }
        }
        return false
      }
    },
    methods: {
      getRoom(guid: string) {
        if(this.rooms) {
          let room = this.rooms.filter((r: any) => {
            return r.guid == guid
          })
          if(room) {
            return room[0]?.value
          }
        }
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div>
  <h4 class="text-accent">
    Space Information
    <i v-if="sectionIsValid" class="fa fa-check-circle text-accent ml-2"></i>
    <i v-else-if="!sectionIsValid" class="fa fa-exclamation-circle text-inprogress ml-2"></i>
  </h4>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in spaceAttrs">
      <template v-if="av.attr.key == 'RoomSetUp'">
        <template v-if="roomSetUp.length > 0">
          <label :class="'control-label ' + av.class">Room Set Up</label>
          <div class="row">
            <div class="col col-xs-6" v-for="(r, idx) in roomSetUp" :key="idx">
              <b>{{r.Room}}</b><br/>
              <div v-for="s in r.SetUp">
                {{s.NumberofTables}} {{s.TypeofTable}} Tables, {{s.NumberofChairs}} Chairs
              </div>
            </div>
          </div>
        </template>
      </template>
      <template v-else>
        <template v-if="av.changeValue != ''">
          <div class="row">
            <div class="col col-xs-6">
              <rck-field
                v-model="av.value"
                :attribute="av.attr"
                :class="av.class"
                :showEmptyValue="true"
              ></rck-field>
            </div>
            <div class="col col-xs-6">
              <rck-field
                v-model="av.changeValue"
                :attribute="av.attr"
                class="text-primary hidden-label"
                :showEmptyValue="true"
              ></rck-field>
            </div>
          </div>
        </template>
        <template v-else>
          <rck-field
            v-model="av.value"
            :attribute="av.attr"
            :showEmptyValue="true"
            :class="av.class"
          ></rck-field>
        </template>
      </template>
      <ul v-if="!sectionIsValid && av.errors.length > 0" class="field-error list-unstyled">
        <li
          v-for="(e, idx) in av.errors"
          :key="idx"
        >{{ e.text }}</li>
      </ul>
    </div>
  </div>
</div>
`
});
