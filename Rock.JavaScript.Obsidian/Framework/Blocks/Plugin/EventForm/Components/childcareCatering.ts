import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockForm from "../../../../Controls/rockForm";
import RockField from "../../../../Controls/rockField";
import RockFormField from "../../../../Elements/rockFormField";
import TimePicker from "./timePicker";


export default defineComponent({
    name: "EventForm.Components.ChildcareCatering",
    components: {
      "rck-field": RockField,
      "rck-form-field": RockFormField,
      "rck-form": RockForm,
      "tcc-time": TimePicker,
    },
    props: {
      e: {
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
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.ChildcareVendor"
        :attribute="e.attributes.ChildcareVendor"
        :is-edit-mode="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.ChildcareCateringBudgetLine"
        :attribute="e.attributes.ChildcareCateringBudgetLine"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.ChildcarePreferredMenu"
        :attribute="e.attributes.ChildcarePreferredMenu"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-time 
        :label="e.attributes.ChildcareFoodTime.name"
        v-model="e.attributeValues.ChildcareFoodTime"
      ></tcc-time>
    </div>
  </div>
</div>
`
});
