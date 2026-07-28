import { defineComponent } from "vue";
import RockField from "@Obsidian/Controls/rockField.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import { DateTime } from "luxon"

export default defineComponent({
    name: "EventDashboard.Components.Modal.RegistrationInfo",
    components: {
      "rck-field": RockField,
      "rck-lbl": RockLabel
    },
    props: {
      details: Object,
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
      regAttrs() {
        let attrs = [] as any[]
        if(this.details?.attributes && this.details.attributeValues) {
          for(let key in this.details.attributes) {
            let attr = this.details.attributes[key]
            let item = { attr: attr, value: "", changeValue: "", class: "", errors: [] as any[] }
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Registration")) {
              item.value = this.details.attributeValues[key]
              if(this.details.attributes[key].fieldTypeGuid == '6b6aa175-4758-453f-8d83-fcd8044b5f36') {
                //Date fields, need to handle the equality check differently
                if(this.details.changes) {
                  if(this.details.attributeValues[key].split('T')[0] != this.details.changes.attributeValues[key].split('T')[0]) {
                    item.changeValue = this.details.changes.attributeValues[key]
                    item.class = 'text-red'
                  }
                }
              } else {
                if(this.details.changes && this.details.changes.attributeValues[key] != this.details.attributeValues[key]) {
                  item.changeValue = this.details.changes.attributeValues[key]
                  item.class = 'text-red'
                }
              }
              if(!this.sectionIsValid && this.requestValidation) {
                let errs = [] as any[]
                this.requestValidation?.errors.forEach(err => { 
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
          if(this.requestValidation.invalidSections.includes("Event Registration")) {
            return false
          } else {
            return true
          }
        }
        return false
      }
    },
    methods: {
      getDiscountCodes(value: string) {
        if(value) {
          return JSON.parse(value)
        }
        return []
      },
      formatDiscountCodeAmount(value: any) {
        if(value.CodeType == '$') {
          return `$${value.Amount}`
        } else {
          return `${value.Amount}%`
        }
      },
      formatDiscountCodeDates(value: any) {
        if(value.EffectiveDateRange) {
          let dates = value.EffectiveDateRange.split(",")
          return `(${DateTime.fromFormat(dates[0], "yyyy-MM-dd").toFormat("MM/dd/yyyy")} - ${DateTime.fromFormat(dates[1], "yyyy-MM-dd").toFormat("MM/dd/yyyy")})`
        }
        return ""
      },
      formatDiscountCodeMaxUses(value: any) {
        if(value.MaxUses) {
          return `Max Uses: ${value.MaxUses}`
        }
        return ""
      },
      formatDateTime(value: string) {
        if(value) {
          return DateTime.fromISO(value).toFormat("f")
        }
        return ""
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div>
  <h4 class="text-accent">
    Registration Information
    <i v-if="sectionIsValid" class="fa fa-check-circle text-accent ml-2"></i>
    <i v-else class="fa fa-exclamation-circle text-inprogress ml-2"></i>
  </h4>
  <div class="row row-eq-height">
    <div class="col col-xs-12 col-md-6" v-for="av in regAttrs">
      <template v-if="av.attr.key == 'DiscountCodes'">
        <div class="form-group static-control">
          <template v-if="av.changeValue != ''">
            <div class="row" style="pading-bottom: 12px;">
              <div class="col col-xs-6">
                <rck-lbl class>{{av.attr.name}}</rck-lbl>
                <div class="text-red">
                  <div v-for="c in getDiscountCodes(av.value)" :key="c.Code">
                    <strong>{{c.Code}}:</strong> {{c.CodeType}} {{c.Amount}}
                  </div>
                </div>
              </div>
              <div class="col col-xs-6 hidden-label">
                <rck-lbl>{{av.attr.name}}</rck-lbl>
                <div class="text-primary">
                  <div v-for="c in getDiscountCodes(av.changeValue)" :key="c.Code">
                    <strong>{{c.Code}}:</strong> {{c.CodeType}} {{c.Amount}}
                  </div>
                </div>
              </div>
            </div>
          </template>
          <template v-else>
            <rck-lbl :class="av.class">{{av.attr.name}}</rck-lbl>
            <div style="pading-bottom: 12px;">
              <div v-for="c in getDiscountCodes(av.value)" :key="c.Code">
                <strong>{{c.Code}}:</strong> {{formatDiscountCodeAmount(c)}} {{formatDiscountCodeDates(c)}} <template v-if="c.AutoApply == 'True'"><i class="fas fa-check-square" style="font-size: 16px;"></i> Auto Apply</template> {{formatDiscountCodeMaxUses(c)}}
              </div>
            </div>
          </template>
        </div>
      </template>
      <template v-else-if="av.attr.key.includes('EmailAdditionalDetails')">
        <div class="form-group static-control">
          <template v-if="av.changeValue != ''">
            <div class="row">
              <div class="col col-xs-6">
                <rck-lbl>{{av.attr.name}}</rck-lbl>
                <div class="mb-2 text-red" v-html="av.value.replaceAll('\\n','<br>')"></div>
              </div>
              <div class="col col-xs-6 hidden-label">
                <rck-lbl>{{av.attr.name}}</rck-lbl>
                <div class="mb-2 text-primary" v-html="av.changeValue.replaceAll('\\n','<br>')"></div>
              </div>
            </div>
          </template>
          <template v-else>
            <rck-lbl :class="av.class">{{av.attr.name}}</rck-lbl>
            <div class="mb-2" v-html="av.value.replaceAll('\\n','<br>')"></div>
          </template>
        </div>
      </template>
      <template v-else-if="av.attr.fieldTypeGuid == 'fe95430c-322d-4b67-9c77-dfd1d4408725'">
        <div class="form-group static-control">
          <template v-if="av.changeValue != ''">
            <div class="row">
              <div class="col col-xs-6">
                <rck-lbl>{{av.attr.name}}</rck-lbl>
                <div class="text-red">
                  {{formatDateTime(av.value)}}
                </div>
              </div>
              <div class="col col-xs-6 hidden-label">
                <rck-lbl>{{av.attr.name}}</rck-lbl>
                <div class="mb-2 text-primary">
                  {{formatDateTime(av.changeValue)}}
                </div>
              </div>
            </div>
          </template>
          <template v-else>
            <rck-lbl :class="av.class">{{av.attr.name}}</rck-lbl>
            <div class="mb-2">
              {{formatDateTime(av.value)}}
            </div>
          </template>
        </div>
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
