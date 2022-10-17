import { defineComponent, PropType } from "vue";
import RockField from "../../../../Controls/rockField"

export default defineComponent({
    name: "EventDashboard.Components.Modal.CalendarInfo",
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
      
    },
    methods: {
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div>
  <h3 class="text-accent">Web Calendar Information</h3>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="request.attributeValues.WebCalendarDescription"
        :attribute="request.attributes.WebCalendarDescription"
      ></rck-field>
    </div>
  </div>
</div>
`
});
