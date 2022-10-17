import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockForm from "../../../../Controls/rockForm";
import RockField from "../../../../Controls/rockField";
import RockFormField from "../../../../Elements/rockFormField";
import Toggle from "./toggle";


export default defineComponent({
    name: "EventForm.Components.Ops",
    components: {
      "rck-field": RockField,
      "rck-form-field": RockFormField,
      "rck-form": RockForm,
      "tcc-switch": Toggle
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
  <h4 class="text-accent">Tech Needs</h4>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.RoomTech"
        :attribute="e.attributes.RoomTech"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.TechNeeds"
        :attribute="e.attributes.TechNeeds"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <h4 class="text-accent">Set-Up</h4>
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <tcc-switch
        v-model="e.attributeValues.NeedsDoorsUnlocked"
        :label="e.attributes.NeedsDoorsUnlocked.name"
      ></tcc-switch>
    </div>
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="e.attributeValues.Doors"
        :attribute="e.attributes.Doors"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.Setup"
        :attribute="e.attributes.Setup"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.SetupImage"
        :attribute="e.attributes.SetupImage"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
</div>
`
});
