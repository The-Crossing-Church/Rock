import { defineComponent, PropType } from "vue"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"

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
      cateringAttrs() {
        let attrs = [] as any[]
        if(this.details?.attributes && this.details.attributeValues) {
          for(let key in this.details.attributes) {
            let attr = this.details.attributes[key]
            let item = { attr: attr, value: "", changeValue: "", class: "", errors: [] as any[] }
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Catering")) {
              item.value = this.details.attributeValues[key]
              if(this.details.changes && this.details.changes.attributeValues[key] != this.details.attributeValues[key]) {
                item.changeValue = this.details.changes.attributeValues[key]
                item.class = 'text-red'
              }
              if(this.needsSpace && categories.includes("Event Space")) {
                continue
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
        return attrs.sort((a,b) => a.attr.order - b.attr.order)
      },
      sectionIsValid() {
        if(this.requestValidation?.invalidSections) {
          if(this.requestValidation.invalidSections.includes("Catering")) {
            let errs = this.cateringAttrs.map(i => i.errors).flat()
            if(errs.length > 0) {
              return false
            }
            return true
          } else {
            return true
          }
        }
        return false
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
            return `${d.value}` 
            // return `${d.value}: ${amount} ${term}` 
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
  <h4 class="text-accent">
    Catering Information
    <i v-if="sectionIsValid" class="fa fa-check-circle text-accent ml-2"></i>
    <i v-else-if="!sectionIsValid" class="fa fa-exclamation-circle text-inprogress ml-2"></i>
  </h4>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in cateringAttrs">
      <template v-if="av.changeValue != ''">
        <template v-if="av.attr.key == 'Drinks'">
          <div class="form-group static-control">
            <div class="row">
              <div class="col col-xs-6">
                <rck-lbl :class="av.class">{{av.attr.name}}</rck-lbl>
                <div v-for="(d, idx) in getDrinkInfo(av.value)" :key="idx" class="text-red">{{d}}</div>
              </div>
              <div class="col col-xs-6 hidden-label">
                <rck-lbl>{{av.attr.name}}</rck-lbl>
                <div v-for="(d, idx) in getDrinkInfo(av.changeValue)" :key="idx" class="text-primary">{{d}}</div>
              </div>
            </div>
          </div>
        </template>
        <template v-else>
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
      </template>
      <template v-else>
        <template v-if="av.attr.key == 'Drinks'">
          <div class="form-group static-control">
            <rck-lbl :class="av.class">{{av.attr.name}}</rck-lbl>
            <div v-for="(d, idx) in getDrinkInfo(av.value)" :key="idx">{{d}}</div>
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
