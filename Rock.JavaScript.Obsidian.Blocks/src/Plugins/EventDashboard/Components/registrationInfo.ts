import { defineComponent } from "vue";
import RockField from "@Obsidian/Controls/rockField"
import RockLabel from "@Obsidian/Controls/rockLabel"
import { DateTime } from "luxon"

export default defineComponent({
    name: "EventDashboard.Components.Modal.RegistrationInfo",
    components: {
      "rck-field": RockField,
      "rck-lbl": RockLabel
    },
    props: {
      details: Object
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
            let item = { attr: attr, value: "", changeValue: "" }
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Registration")) {
              item.value = this.details.attributeValues[key]
              if(this.details.changes && this.details.changes.attributeValues[key] != this.details.attributeValues[key]) {
                item.changeValue = this.details.changes.attributeValues[key]
              }
              attrs.push(item)
            }
          }
        }
        return attrs.sort((a,b) => a.attr.order - b.attr.order)
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
  <h3 class="text-accent">Registration Information</h3>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in regAttrs">
      <template v-if="av.attr.key == 'DiscountCodes'">
        <template v-if="av.changeValue != ''">
          <div class="row" style="pading-bottom: 12px;">
            <div class="col col-xs-6">
              <rck-lbl>{{av.attr.name}}</rck-lbl>
              <div class="text-red">
                <div v-for="c in getDiscountCodes(av.value)" :key="c.Code">
                  <strong>{{c.Code}}:</strong> {{c.CodeType}} {{c.Amount}}
                </div>
              </div>
            </div>
            <div class="col col-xs-6">
              <div class="text-primary" style="padding-top: 18px;">
                <div v-for="c in getDiscountCodes(av.changeValue)" :key="c.Code">
                  <strong>{{c.Code}}:</strong> {{c.CodeType}} {{c.Amount}}
                </div>
              </div>
            </div>
          </div>
        </template>
        <template v-else>
          <rck-lbl>{{av.attr.name}}</rck-lbl>
          <div style="pading-bottom: 12px;">
            <div v-for="c in getDiscountCodes(av.value)" :key="c.Code">
              <strong>{{c.Code}}:</strong> {{formatDiscountCodeAmount(c)}} {{formatDiscountCodeDates(c)}} <template v-if="c.AutoApply == 'True'"><i class="fas fa-check-square" style="font-size: 16px;"></i> Auto Apply</template> {{formatDiscountCodeMaxUses(c)}}
            </div>
          </div>
        </template>
      </template>
      <template v-else-if="av.attr.key.includes('EmailAdditionalDetails')">
        <template v-if="av.changeValue != ''">
          <div class="row">
            <div class="col col-xs-6">
              <rck-lbl>{{av.attr.name}}</rck-lbl>
              <div class="mb-2 text-red" v-html="av.value.replaceAll('\\n','<br>')"></div>
            </div>
            <div class="col col-xs-6">
              <div class="mb-2 text-primary" style="padding-top: 18px;" v-html="av.changeValue.replaceAll('\\n','<br>')"></div>
            </div>
          </div>
        </template>
        <template v-else>
          <rck-lbl>{{av.attr.name}}</rck-lbl>
          <div class="mb-2" v-html="av.value.replaceAll('\\n','<br>')"></div>
        </template>
      </template>
      <template v-else-if="av.attr.fieldTypeGuid == 'fe95430c-322d-4b67-9c77-dfd1d4408725'">
        <template v-if="av.changeValue != ''">
          <div class="row">
            <div class="col col-xs-6">
              <rck-lbl>{{av.attr.name}}</rck-lbl>
              <div class="text-red">
                {{formatDateTime(av.value)}}
              </div>
            </div>
            <div class="col col-xs-6">
              <div class="mb-2 text-primary" style="padding-top: 18px;">
                {{formatDateTime(av.changeValue)}}
              </div>
            </div>
          </div>
        </template>
        <template v-else>
          <rck-lbl>{{av.attr.name}}</rck-lbl>
          <div class="mb-2">
            {{formatDateTime(av.value)}}
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
