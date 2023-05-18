import { defineComponent, PropType } from "vue"
import RockField from "../../../../Controls/rockField"
import RockLabel from "../../../../Elements/rockLabel"

type ListItem = {
  text: string,
  description: string,
  value: string
}

export default defineComponent({
    name: "EventDashboard.Components.Modal.CateringInfo",
    components: {
      "rck-field": RockField,
      "rck-lbl": RockLabel
    },
    props: {
      details: Object,
      drinks: Array,
      needsSpace: Boolean,
    },
    setup() {

    },
    data() {
        return {
          
        };
    },
    computed: {
      cateringAttrs() {
        let attrs = [] as any[]
        if(this.details?.attributes && this.details.attributeValues) {
          for(let key in this.details.attributes) {
            let attr = this.details.attributes[key]
            let item = { attr: attr, value: "", changeValue: "" }
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Catering")) {
              item.value = this.details.attributeValues[key]
              if(this.details.changes && this.details.changes.attributeValues[key] != this.details.attributeValues[key]) {
                item.changeValue = this.details.changes.attributeValues[key]
              }
              if(this.needsSpace && categories.includes("Event Space")) {
                continue
              }
              attrs.push(item)
            }
          }
        }
        return attrs.sort((a,b) => a.attr.order - b.attr.order)
      }
    },
    methods: {
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
  <h3 class="text-accent">Catering Information</h3>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in cateringAttrs">
      <template v-if="av.changeValue != ''">
        <template v-if="av.attr.key == 'Drinks'">
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
      </template>
      <template v-else>
        <template v-if="av.attr.key == 'Drinks'">
          <rck-lbl>{{av.attr.name}}</rck-lbl>
          <div v-for="(d, idx) in getDrinkInfo(av.value)" :key="idx">{{d}}</div>
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
