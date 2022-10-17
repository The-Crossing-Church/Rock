import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import RockForm from "../../../../Controls/rockForm";
import RockField from "../../../../Controls/rockField";
import RockFormField from "../../../../Elements/rockFormField";


export default defineComponent({
    name: "EventForm.Components.Online",
    components: {
      "rck-field": RockField,
      "rck-form-field": RockFormField,
      "rck-form": RockForm,
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
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.EventURL"
        :attribute="e.attributes.EventURL"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12">
      <rck-field
        v-model="e.attributeValues.Password"
        :attribute="e.attributes.Password"
        :is-edit-mode="true"
      ></rck-field>
    </div>
  </div>
</div>
`
});
