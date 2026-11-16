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
      editSections: Array,
      index: Number
    },
    setup() {

    },
    data() {
      return {
        sections: [
          { 
            title: 'Childcare Information', 
            attrs: [] as any[],
            isValid: false
          },
          { 
            title: 'Childcare Catering Information', 
            attrs: [] as any[],
            isValid: false
          },
          { 
            title: 'Childcare Registration Information', 
            attrs: [] as any[],
            isValid: false
          }
        ]
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
      },
    },
    methods: {
      processChildcareAttrs() {
        if(this.details?.attributes && this.details.attributeValues) {
          this.sections[0].attrs = []
          this.sections[1].attrs = []
          this.sections[2].attrs = []

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
              if(categories.includes("Event Childcare Catering")) {
                this.sections[1].attrs.push(item)
              } else if (categories.includes("Event Childcare Registration")) {
                this.sections[2].attrs.push(item)
              } else {
                this.sections[0].attrs.push(item)
              }
            }
          }
        }
        this.sections[0].attrs = this.sections[0].attrs.sort((a,b) => a.attr.order - b.attr.order)
        this.sections[1].attrs = this.sections[1].attrs.sort((a,b) => a.attr.order - b.attr.order)
        this.sections[2].attrs = this.sections[2].attrs.sort((a,b) => a.attr.order - b.attr.order)
      },
      processSectionValidity() {
        if(this.requestValidation?.invalidSections) {
          if(this.requestValidation.invalidSections.includes("Childcare")) {
            let errs = this.sections[0].attrs.map(i => i.errors).flat()
            if(errs.length > 0) {
              this.sections[0].isValid = false
            } else {
              this.sections[0].isValid = true
            }
          } else {
            this.sections[0].isValid = true
          }
          if (this.requestValidation.invalidSections.includes("Childcare Catering")) {
            let errs = this.sections[1].attrs.map(i => i.errors).flat()
            if(errs.length > 0) {
              this.sections[1].isValid = false
            } else {
              this.sections[1].isValid = true
            }
          } else {
            this.sections[1].isValid = true
          }
          if(this.requestValidation.invalidSections.includes("Childcare Registration")) {
            let errs = this.sections[2].attrs.map(i => i.errors).flat()
            if(errs.length > 0) {
              this.sections[2].isValid = false
            } else {
              this.sections[2].isValid = true
            }
          } else {
            this.sections[2].isValid = true
          }
        } else {
          this.sections[0].isValid = false
          this.sections[1].isValid = false
          this.sections[2].isValid = false
        }
      }
    },
    watch: {
      'this.details.attributeValues': {
        handler(val) {
          this.processChildcareAttrs()
          this.processSectionValidity()
        },
        deep: true
      }
    },
    mounted() {
      this.processChildcareAttrs()
      this.processSectionValidity()
    },
    template: `
<div v-for="(section, idx) in sections" :key="idx">
  <h4 class="text-accent">
    {{section.title}}
    <i v-if="section.isValid" class="fa fa-check-circle text-accent ml-2"></i>
    <i v-else-if="!section.isValid" class="fa fa-exclamation-circle text-inprogress ml-2"></i>
  </h4>
  <div class="row row-eq-height">
    <div class="col col-xs-12 col-md-6" v-for="av in section.attrs">
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
