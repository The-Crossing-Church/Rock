import { defineComponent, PropType } from "vue";
import RockField from "../../../../Controls/rockField"

export default defineComponent({
    name: "EventDashboard.Components.Modal.ChildcareInfo",
    components: {
      "rck-field": RockField
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
      childcareAttrs() {
        let attrs = [] as any[]
        if(this.details?.attributes && this.details.attributeValues) {
          for(let key in this.details.attributes) {
            let attr = this.details.attributes[key]
            let item = { attr: attr, value: "", changeValue: "" }
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Childcare")) {
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
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div>
  <h3 class="text-accent">Childcare Information</h3>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in childcareAttrs">
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
    </div>
  </div>
</div>
`
});
