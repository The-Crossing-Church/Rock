import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockForm from "../../../../Controls/rockForm";
import RockField from "../../../../Controls/rockField";
import RockFormField from "../../../../Elements/rockFormField";
import Toggle from "./toggle";
import TimePicker from "./timePicker"
import CCCatering from "./childcareCatering"


export default defineComponent({
    name: "EventForm.Components.ChildCare",
    components: {
      "rck-field": RockField,
      "rck-form-field": RockFormField,
      "rck-form": RockForm,
      "tcc-switch": Toggle,
      "tcc-time": TimePicker,
      "tcc-childcare-catering": CCCatering
    },
    props: {
      e: {
          type: Object as PropType<ContentChannelItem>,
          required: false
      },
      request: Object as PropType<ContentChannelItem>,
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
      <tcc-time 
        :label="e.attributes.ChildcareStartTime.name"
        v-model="e.attributeValues.ChildcareStartTime"
      ></tcc-time>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-time 
        :label="e.attributes.ChildcareEndTime.name"
        v-model="e.attributeValues.ChildcareEndTime"
      ></tcc-time>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.ChildcareOptions"
        :attribute="e.attributes.ChildcareOptions"
        :is-edit-mode="true"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.EstimatedNumberofKids"
        :attribute="e.attributes.EstimatedNumberofKids"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <br v-if="request.attributeValues.NeedsCatering == 'True'" />
  <h4 class="text-accent" v-if="request.attributeValues.NeedsCatering == 'True'">Childcare Catering Information</h4>
  <tcc-childcare-catering v-if="request.attributeValues.NeedsCatering == 'True'" :e="e"></tcc-childcare-catering>
</div>
`
});
