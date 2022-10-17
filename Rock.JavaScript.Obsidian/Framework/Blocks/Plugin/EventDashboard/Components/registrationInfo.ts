import { defineComponent, PropType } from "vue";
import RockField from "../../../../Controls/rockField"

export default defineComponent({
    name: "EventDashboard.Components.Modal.RegistrationInfo",
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
            let parsedval = " "
            if(this.details.attributeValues[key].includes("{")) {
              parsedval = JSON.parse(this.details.attributeValues[key]).text
            }
            if(categories.includes("Event Registration") && this.details.attributeValues[key] != "" && parsedval != "") {
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
  <h3 class="text-accent">Registration Information</h3>
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
