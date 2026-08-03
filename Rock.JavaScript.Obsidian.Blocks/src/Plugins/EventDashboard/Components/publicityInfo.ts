import { defineComponent, PropType } from "vue";
import RockField from "@Obsidian/Controls/rockField.obs"

export default defineComponent({
    name: "EventDashboard.Components.Modal.PublicityInfo",
    components: {
      "rck-field": RockField
    },
    props: {
      request: Object,
      requestValidation: Object,
    },
    setup() {

    },
    data() {
        return {
          
        };
    },
    computed: {
      publicityAttrs() {
        let attrs = [] as any[]
        if(this.request?.attributes && this.request.attributeValues) {
          for(let key in this.request.attributes) {
            let attr = this.request.attributes[key]
            let item = { attr: attr, value: "", changeValue: "", class: "", errors: [] as any[] }
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Publicity")) {
              item.value = this.request.attributeValues[key]
              if(this.request.attributes[key].fieldTypeGuid == '6b6aa175-4758-453f-8d83-fcd8044b5f36') {
                //Date fields, need to handle the equality check differently
                if(this.request.changes) {
                  if(this.request.attributeValues[key].split('T')[0] != this.request.changes.attributeValues[key].split('T')[0]) {
                    item.changeValue = this.request.changes.attributeValues[key]
                    item.class = 'text-red'
                  }
                }
              } else {
                if(this.request.changes && this.request.changes.attributeValues[key] != this.request.attributeValues[key]) {
                  item.changeValue = this.request.changes.attributeValues[key]
                  item.class = 'text-red'
                }
              }
              if(!this.sectionIsValid && this.requestValidation) {
                let errs = [] as any[]
                this.requestValidation?.errors?.forEach(err => { 
                  errs.push(...err.errors.filter(e => {
                    return e.name == item.attr.key
                  }))
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
          if(this.requestValidation.invalidSections.includes("Publicity")) {
            return false
          } else {
            return true
          }
        }
        return false
      },
      errors() {
        let errors = [] as any[]
        if(!this.sectionIsValid) {
          if(this.requestValidation?.errors && this.requestValidation.errors.length > 0) {
            let item_errors = this.requestValidation.errors.filter(err => err.ref == "publicity")
            if(item_errors && item_errors.length > 0) {
              errors = item_errors[0].errors
            }
          }
        }
        return errors
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
  <h4 class="text-accent">Publicity Information</h4>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in publicityAttrs">
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
