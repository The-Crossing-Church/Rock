import { defineComponent, PropType } from "vue";
import RockField from "../../../../Controls/rockField"

export default defineComponent({
    name: "EventDashboard.Components.Modal.OnlineInfo",
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
      onlineAttrs() {
        let attrs = [] as any[]
        if(this.details?.attributes && this.details.attributeValues) {
          for(let key in this.details.attributes) {
            let attr = this.details.attributes[key]
            let categories = attr.categories.map((c: any) => c.name)
            if(categories.includes("Event Online") && this.details.attributeValues[key] != "") {
              attrs.push({attr: attr, value: this.details.attributeValues[key]})
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
  <h3 class="text-accent">Online Information</h3>
  <div class="row">
    <div class="col col-xs-12 col-md-6" v-for="av in onlineAttrs">
      <rck-field
        v-model="av.value"
        :attribute="av.attr"
      ></rck-field>
    </div>
  </div>
</div>
`
});
