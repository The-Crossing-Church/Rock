import { defineComponent } from "vue"
import RockField from "@Obsidian/Controls/rockField"

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
    <div class="col col-xs-12 col-md-6">
      <template v-if="request.changes != null && request.attributeValues.WebCalendarDescription != request.changes.attributeValues.WebCalendarDescription">
        <div class="row">
          <div class="col col-xs-6">
            <rck-field
              v-model="request.attributeValues.WebCalendarGoLive"
              :attribute="request.attributes.WebCalendarGoLive"
              class="text-red"
              :showEmptyValue="true"
            ></rck-field>
          </div>
          <div class="col col-xs-6">
            <rck-field
              v-model="request.changes.attributeValues.WebCalendarGoLive"
              :attribute="request.attributes.WebCalendarGoLive"
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
          v-model="request.attributeValues.WebCalendarGoLive"
          :attribute="request.attributes.WebCalendarGoLive"
          :showEmptyValue="true"
        ></rck-field>
      </template>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <template v-if="request.changes != null && request.attributeValues.WebCalendarDescription != request.changes.attributeValues.WebCalendarDescription">
        <div class="row">
          <div class="col col-xs-6">
            <rck-lbl>{{request.attributes.WebCalendarDescription.name}}</rck-lbl>
            <div class="mb-2 text-red" v-html="request.attributeValues.WebCalendarDescription.replaceAll('\\n','<br>')"></div>
          </div>
          <div class="col col-xs-6">
            <div class="mb-2 text-primary" style="padding-top: 18px;" v-html="request.changes.attributeValues.WebCalendarDescription.replaceAll('\\n','<br>')"></div>
          </div>
        </div>
      </template>
      <template v-else>
        <rck-lbl>{{request.attributes.WebCalendarDescription.name}}</rck-lbl>
        <div class="mb-2" v-html="request.attributeValues.WebCalendarDescription.replaceAll('\\n','<br>')"></div>
      </template>
    </div>
  </div>
</div>
`
});