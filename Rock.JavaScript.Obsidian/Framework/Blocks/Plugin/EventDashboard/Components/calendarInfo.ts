import { defineComponent } from "vue"
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
      <template v-if="request.changes != null && request.attributeValues.WebCalendarDescription != request.changes.attributeValues.WebCalendarDescription">
        <div class="row">
          <div class="col col-xs-6">
            <rck-field
              v-model="request.attributeValues.WebCalendarDescription"
              :attribute="request.attributes.WebCalendarDescription"
              class="text-red"
              :showEmptyValue="true"
            ></rck-field>
          </div>
          <div class="col col-xs-6">
            <rck-field
              v-model="request.changes.attributeValues.WebCalendarDescription"
              :attribute="request.attributes.WebCalendarDescription"
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
          v-model="request.attributeValues.WebCalendarDescription"
          :attribute="request.attributes.WebCalendarDescription"
          :showEmptyValue="true"
        ></rck-field>
      </template>
    </div>
  </div>
</div>
`
});
