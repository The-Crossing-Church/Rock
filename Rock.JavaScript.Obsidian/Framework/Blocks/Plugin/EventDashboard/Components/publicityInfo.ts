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
            let categories = attr.categories.map((c: any) => c.name)
            let parsedval = " "
            if(this.request.attributeValues[key].includes("{")) {
              parsedval = JSON.parse(this.request.attributeValues[key]).text
            }
            if(categories.includes("Event Publicity") && this.request.attributeValues[key] != "" && parsedval != "") {
              attrs.push({attr: attr, value: this.request.attributeValues[key]})
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
      <rck-field
        v-model="av.value"
        :attribute="av.attr"
      ></rck-field>
    </div>
  </div>
</div>
`
});
