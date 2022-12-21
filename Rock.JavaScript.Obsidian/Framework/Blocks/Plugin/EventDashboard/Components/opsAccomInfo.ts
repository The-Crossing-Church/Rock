import { defineComponent, PropType } from "vue";
import RockField from "../../../../Controls/rockField"
import RockLabel from "../../../../Elements/rockLabel"

type ListItem = {
  text: string,
  description: string,
  value: string
}

export default defineComponent({
    name: "EventDashboard.Components.Modal.OpsAccomInfo",
    components: {
      "rck-field": RockField,
      "rck-lbl": RockLabel
    },
    props: {
      details: Object,
      rooms: Array,
      drinks: Array,
      needsCatering: String
    },
    setup() {

    },
    data() {
        return {
          
        };
    },
    computed: {
      opsAttrs() {
        let attrs = [] as any[]
        if(this.details?.attributes && this.details.attributeValues) {
          for(let key in this.details.attributes) {
            let attr = this.details.attributes[key]
            let item = { attr: attr, value: "", changeValue: "" }
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Ops Requests")) {
              item.value = this.details.attributeValues[key]
              if(this.details.changes && this.details.changes.attributeValues[key] != this.details.attributeValues[key]) {
                item.changeValue = this.details.changes.attributeValues[key]
              }
              if(this.needsCatering == 'True' && categories.includes("Event Catering")) {
                continue
              }
              attrs.push(item)
            }
          }
        }
        return attrs
      }
    },
    methods: {
      getSetUpDesc(value: string) {
        let selectedRoomsAttr = JSON.parse(this.details?.attributeValues.Rooms)
        let selectedGuids = selectedRoomsAttr.value.split(",")
        let selectedRooms = this.rooms?.filter((r: any) => {
          return selectedGuids.includes(r.guid)
        }) as any
        let val = JSON.parse(value)
        let setup = []
        if(selectedRooms && selectedRooms.length > 0) {
          for(let i = 0; i < selectedRooms.length; i++) {
            let obj = { 
              room: selectedRooms[i].value, 
              items: val.filter((v: any) => { 
                return v.Room == selectedRooms[i].guid 
              }).map((v: any) => {
                return `${v.NumberofTables} ${v.TypeofTable} ${parseInt(v.NumberofTables) > 1 ? 'tables' : 'table'} with ${v.NumberofChairs} chairs each.`
              })
            }
            setup.push(obj)
          }
        }
        return setup
      },
      getDrinkInfo(value: string) {
        let item = JSON.parse(value) as ListItem
        let guids = item.value.split(",")
        let selectedDrinks = this.drinks?.filter((d: any) => {
          return guids.includes(d.guid)
        })
        let expectedAttendance = this.details?.attributeValues.ExpectedAttendance
        if(selectedDrinks && selectedDrinks.length > 0) {
          return selectedDrinks.map((d: any) => { 
            let amount = Math.ceil(expectedAttendance/d.attributeValues.NumberofPeople.value)
            let term = amount > 1 ? d.attributeValues.UnitTerm.value + "s" : d.attributeValues.UnitTerm.value
            return `${d.value}: ${amount} ${term}` 
          })
        }
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div>
  <h3 class="text-accent">Ops Accomodations Information</h3>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in opsAttrs">
      <template v-if="av.attr.key =='RoomSetUp'">
        <template v-if="av.changeValue != ''">
          <div class="row">
            <div class="col col-xs-6">
              <rck-lbl>{{av.attr.name}}</rck-lbl>
              <div class="text-red">
                <div v-for="su in getSetUpDesc(av.value)" :key="su.room">
                  <template v-if="su.items.length > 0">
                    <b>{{su.room}}: Custom Setup</b> <br/>
                    <div v-for="(i, idx) in su.items" :key="idx">
                      {{i}}
                    </div>
                  </template>
                  <template v-else>
                    <b>{{su.room}}: Standard Setup</b>
                  </template>
                </div>
              </div>
            </div>
            <div class="col col-xs-6">
              <br/>
              <div class="text-primary">
                <div v-for="su in getSetUpDesc(av.changeValue)" :key="su.room">
                  <template v-if="su.items.length > 0">
                    <b>{{su.room}}: Custom Setup</b> <br/>
                    <div v-for="(i, idx) in su.items" :key="idx">
                      {{i}}
                    </div>
                  </template>
                  <template v-else>
                    <b>{{su.room}}: Standard Setup</b>
                  </template>
                </div>
              </div>
            </div>
          </div>
        </template>
        <template v-else>
          <rck-lbl>{{av.attr.name}}</rck-lbl> <br/>
          <div v-for="su in getSetUpDesc(av.value)" :key="su.room">
            <template v-if="su.items.length > 0">
              <b>{{su.room}}: Custom Setup</b> <br/>
              <div v-for="(i, idx) in su.items" :key="idx">
                {{i}}
              </div>
            </template>
            <template v-else>
              <b>{{su.room}}: Standard Setup</b>
            </template>
          </div>
        </template>
      </template>
      <template v-else-if="av.attr.key == 'Drinks'">
        <template v-if="av.changeValue != ''">
          <div class="row">
            <div class="col col-xs-6">
              <rck-lbl>{{av.attr.name}}</rck-lbl>
              <div v-for="(d, idx) in getDrinkInfo(av.value)" :key="idx" class="text-red">{{d}}</div>
            </div>
            <div class="col col-xs-6">
              <div v-for="(d, idx) in getDrinkInfo(av.changeValue)" :key="idx" class="text-primary">{{d}}</div>
            </div>
          </div>
        </template>
        <template v-else>
          <rck-lbl>{{av.attr.name}}</rck-lbl>
          <div v-for="(d, idx) in getDrinkInfo(av.value)" :key="idx">{{d}}</div>
        </template>
      </template>
      <template v-else>
        <template v-if="av.changeValue != ''">
          <div class="row">
            <div class="col col-xs-6">
              <rck-field
                v-model="av.value"
                :attribute="av.attr"
                class="text-red"
                :showEmptyValue="true"
              ></rck-field>
            </div>
            <div class="col col-xs-6">
              <rck-field
                v-model="av.changeValue"
                :attribute="av.attr"
                class="text-primary"
                :showEmptyValue="true"
                :showLabel="false"
                style="padding-top: 18px;"
              ></rck-field>
            </div>
          </div>
        </template>
        <template v-else>
          <rck-field
            v-model="av.value"
            :attribute="av.attr"
            :showEmptyValue="true"
          ></rck-field>
        </template>
      </template>
    </div>
  </div>
</div>
`
});
