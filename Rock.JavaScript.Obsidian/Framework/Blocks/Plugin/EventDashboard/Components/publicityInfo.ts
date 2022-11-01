import { defineComponent, PropType } from "vue";
import RockField from "../../../../Controls/rockField"

export default defineComponent({
    name: "EventDashboard.Components.Modal.PublicityInfo",
    components: {
      "rck-field": RockField
    },
    props: {
      request: Object
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
            let item = { attr: attr, value: "", changeValue: "" }
            let categories = attr.categories.map((c: any) => c.name)
            let parsedval = " "
            if(this.request.attributeValues[key].includes("{")) {
              parsedval = JSON.parse(this.request.attributeValues[key]).text
            }
            let hasValue = false
            if(this.request.attributeValues[key] != "" && parsedval != "") {
              hasValue = true
            }
            if(this.request.changes && this.request.changes.attributeValues[key] != "") {
              hasValue = true
            }
            if(categories.includes("Event Publicity") && hasValue) {
              item.value = this.request.attributeValues[key]
              if(this.request.changes && this.request.changes.attributeValues[key] != this.request.attributeValues[key]) {
                item.changeValue = this.request.changes.attributeValues[key]
              }
              attrs.push(item)
            }
          }
        }
        return attrs
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
  <h3 class="text-accent">Publicity Information</h3>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in publicityAttrs">
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
        ></rck-field>
      </template>
    </div>
  </div>
</div>
`
});
