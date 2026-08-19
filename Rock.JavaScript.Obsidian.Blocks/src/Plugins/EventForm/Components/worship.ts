import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockForm from "@Obsidian/Controls/rockForm.obs"
import Validator from "./validator"
import Toggle from "./toggle"
import TimePicker from "./timePicker"
import rules from "../Rules/rules"


export default defineComponent({
  name: "EventForm.Components.Worship",
  components: {
    "rck-field": RockField,
    "rck-form": RockForm,
    "tcc-validator": Validator,
    "tcc-switch": Toggle,
    "tcc-time": TimePicker,
  },
  props: {
    request: {
      type: Object as PropType<ContentChannelItemBag>,
      required: false
    },
    isFuneralRequest: Boolean,
    showValidation: Boolean,
    refName: String,
    readonly: Boolean
  },
  setup() {

  },
  data() {
    return {
      rules: rules,
      errors: [] as Record<string, string>[]
    }
  },
  computed: {
    
  },
  methods: {
    validate() {
      let formRef = this.$refs as any
      for(let r in formRef) {
        if(formRef[r].className?.includes("validator")) {
          formRef[r].validate()
        }
      }
    },
    validationChange(errs: Record<string, string>[]) {
      this.errors = errs
    }
  },
  watch: {
    errors: {
      handler(val) {
        this.$emit("validation-change", { ref: this.refName, errors: val})
      },
      deep: true
    }
  },
  mounted() {
    if(this.showValidation) {
      this.validate()
    }
  },
  template: `
<rck-form ref="form" @validationChanged="validationChange">
  <div class="row">
    <div class="col col-xs-12 col-md-6">
      <rck-field
        v-model="request.attributeValues.NumberOfMusiciansBudgeted"
        :attribute="request.attributes.NumberOfMusiciansBudgeted"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
        id="txtNumberOfMusiciansBudgeted"
      ></rck-field>
    </div>
    <div class="col col-xs-12 col-md-6">
      <tcc-validator :name="request.attributes.PublicArrivalTime.key" :rules="[rules.required(request.attributeValues.PublicArrivalTime, request.attributes.PublicArrivalTime.name)]" ref="validators_doors_open" v-if="!readonly">
        <tcc-time 
          :label="request.attributes.PublicArrivalTime.name"
          v-model="request.attributeValues.PublicArrivalTime"
          id="timePublicArrivalTime"
        ></tcc-time>
      </tcc-validator>
      <rck-field
        v-else
        v-model="request.attributeValues.PublicArrivalTime"
        :attribute="request.attributes.PublicArrivalTime"
        :is-edit-mode="false"
        :showEmptyValue="true"
        id="timePublicArrivalTime"
      ></rck-field>
    </div>
    <div class="col col-xs-12">
      <rck-field
        v-model="request.attributeValues.WorshipLeaderRequest"
        :attribute="request.attributes.WorshipLeaderRequest"
        :is-edit-mode="!readonly"
        :showEmptyValue="true"
        id="txtWorshipLeaderRequest"
      ></rck-field>
    </div>
  </div>
  <template v-if="isFuneralRequest">
    <h4 class="text-accent">Funeral Worship Information</h4>
    <div class="row">
      <div class="col col-xs-12 col-md-6">
        <rck-field
          v-model="request.attributeValues.Pastor"
          :attribute="request.attributes.Pastor"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="txtPastor"
        ></rck-field>
      </div>
      <div class="col col-xs-12 col-md-6">
        <tcc-switch
          v-model="request.attributeValues.HasVisitation"
          :label="request.attributes.HasVisitation.name"
          style="margin-top: 22px;"
          v-if="!readonly"
          id="boolHasVisitation"
        ></tcc-switch>
        <rck-field
          v-else
          v-model="request.attributeValues.HasVisitation"
          :attribute="request.attributes.HasVisitation"
          :is-edit-mode="false"
          :showEmptyValue="true"
          id="boolHasVisitation"
        ></rck-field>
      </div>
    </div>
    <div class="row">
      <div class="col col-xs-12 col-md-6">
        <rck-field
          v-model="request.attributeValues.WorshipSongs"
          :attribute="request.attributes.WorshipSongs"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="ddlWorshipSongs"
        ></rck-field>
      </div>
      <div class="col col-xs-12 col-md-6">
        <rck-field
          v-model="request.attributeValues.ServiceOrder"
          :attribute="request.attributes.ServiceOrder"
          :is-edit-mode="!readonly"
          :showEmptyValue="true"
          id="listServiceOrder"
        ></rck-field>
      </div>
    </div>
  </template>
  <br/>
</rck-form>
`
});
