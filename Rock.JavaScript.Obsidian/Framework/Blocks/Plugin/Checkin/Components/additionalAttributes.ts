import { defineComponent, PropType } from "vue"
import { Attribute, Person } from "../../../../ViewModels"
import RockField from "../../../../Controls/rockField"

export default defineComponent({
    name: "Checkin.Components.AdditionalAttributes",
    components: {
      "rck-field": RockField
    },
    props: {
      attributes: {
        type: [] as any[],
        required: false
      },
      person: {
        type: Object as PropType<Person>,
        required: false
      }
    },
    setup() {

    },
    data() {
      return {
        
      }
    },
    computed: {
      colWidth() {
        if(this.attributes.length < 4) {
          return 12 / this.attributes.length
        }
        return 4
      },
    },
    methods: {
      
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div class="row mt-4">
  <div v-for="a in attributes" :class="'col col-xs-12 col-md-' + colWidth">
    <rck-field
      :attribute="person.attributes[a.key]"
      v-model="person.attributeValues[a.key]"
      :is-edit-mode="true"
    ></rck-field>
  </div>
</div>
`
});
