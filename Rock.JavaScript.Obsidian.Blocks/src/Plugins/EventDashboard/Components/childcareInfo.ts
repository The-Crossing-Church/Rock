import { defineComponent, PropType } from "vue";
import RockField from "@Obsidian/Controls/rockField.obs"

export default defineComponent({
    name: "EventDashboard.Components.Modal.ChildcareInfo",
    components: {
      "rck-field": RockField
    },
    props: {
      details: Object,
      needsCatering: Boolean,
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
      childcareAttrs() {
        let attrs = [] as any[]
        if(this.details?.attributes && this.details.attributeValues) {
          for(let key in this.details.attributes) {
            let attr = this.details.attributes[key]
            let item = { attr: attr, value: "", changeValue: "", class: "", errors: [] as any[] }
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Childcare")) {
              if(categories.includes("Event Childcare Catering") && !this.needsCatering) {
                continue
              } 
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
        return attrs.sort((a,b) => a.attr.order - b.attr.order)
      },
      sectionIsValid() {
        if(this.requestValidation?.invalidSections) {
          if(this.requestValidation.invalidSections.includes("Childcare") || this.requestValidation.invalidSections.includes("Childcare Registration") || this.requestValidation.invalidSections.includes("Childcare Catering")) {
            let errs = this.childcareAttrs.map(i => i.errors).flat()
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
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div>
  <h4 class="text-accent">
    Childcare Information
    <i v-if="sectionIsValid" class="fa fa-check-circle text-accent ml-2"></i>
    <i v-else-if="!sectionIsValid" class="fa fa-exclamation-circle text-inprogress ml-2"></i>
  </h4>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in childcareAttrs">
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
