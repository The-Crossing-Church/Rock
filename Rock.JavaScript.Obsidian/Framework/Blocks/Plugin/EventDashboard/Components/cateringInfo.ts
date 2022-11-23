import { defineComponent, PropType } from "vue";
import RockField from "../../../../Controls/rockField"

export default defineComponent({
    name: "EventDashboard.Components.Modal.CateringInfo",
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
  <h3 class="text-accent">Catering Information</h3>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in cateringAttrs">
      <template v-if="av.changeValue != ''">
        <template v-if="av.attr.key == 'Drinks'">
          <rck-field
            v-model="av.value"
            :attribute="av.attr"
            :showEmptyValue="true"
          ></rck-field>
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
          <rck-field
            v-model="av.value"
            :attribute="av.attr"
            :showEmptyValue="true"
          ></rck-field>
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
