import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockForm from "../../../../Controls/rockForm";
import RockField from "../../../../Controls/rockField";
import RockFormField from "../../../../Elements/rockFormField";
import Toggle from "./toggle"


export default defineComponent({
    name: "EventForm.Components.Publicity",
    components: {
      "rck-field": RockField,
      "rck-form-field": RockFormField,
      "rck-form": RockForm,
      "tcc-switch": Toggle,
    },
    props: {
      request: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
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
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="request.attributeValues.WhyAttend"
        :attribute="request.attributes.WhyAttend"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row row-equal-height">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="request.attributeValues.TargetAudience"
        :attribute="request.attributes.TargetAudience"
        :is-edit-mode="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="request.attributeValues.EventisSticky"
        :label="request.attributes.EventisSticky.name"
      ></tcc-switch>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="request.attributeValues.PublicityStartDate"
        :attribute="request.attributes.PublicityStartDate"
        :is-edit-mode="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="request.attributeValues.PublicityEndDate"
        :attribute="request.attributes.PublicityEndDate"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="request.attributeValues.PublicityStrategies"
        :attribute="request.attributes.PublicityStrategies"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
</div>
`
});
