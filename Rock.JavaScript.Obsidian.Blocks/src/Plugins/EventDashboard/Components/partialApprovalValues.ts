import { defineComponent } from "vue"
import RockField from "@Obsidian/Controls/rockField.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"

export default defineComponent({
    name: "EventDashboard.Components.Modal.PartialApproval.Values",
    components: {
      "rck-field": RockField,
      "rck-btn": RockButton,
    },
    props: {
      attribute: Object,
      originalValue: String,
      newValue: String
    },
    setup() {

    },
    data() {
        return {
          isApproved: null
        };
    },
    computed: {
      
    },
    methods: {
      getClassName(isOriginal: boolean) {
        let className = "text-red"
        if(!isOriginal) {
          className = "text-primary"
        }
        if(this.isApproved == null) {
          return className
        }
        if(this.isApproved && isOriginal) {
          className += " text-strikethrough"
        }
        if(!this.isApproved && !isOriginal) {
          className += " text-strikethrough"
        }
        return className
      },
    },
    watch: {
      isApproved(val) {
        if(val) {
          this.$emit("approved")
        } else {
          this.$emit("denied")
        }
      }
    },
    mounted() {
      
    },
    template: `
<div class="row" style="display: flex; align-items: center;">
  <div class="col col-xs-10">
    <div class="row">
      <div class="col col-xs-6">
        <rck-field
          v-model="originalValue"
          :attribute="attribute"
          :class="getClassName(true)"
          :showEmptyValue="true"
        ></rck-field>
      </div>
      <div class="col col-xs-6">
        <rck-field
          v-model="newValue"
          :attribute="attribute"
          :class="getClassName(false)"
          :showEmptyValue="true"
          :showLabel="false"
          style="padding-top: 18px;"
        ></rck-field>
      </div>
    </div>
  </div>
  <div class="col col-xs-2">
    <rck-btn btnType="accent" class="btn-circle mr-1" @click="isApproved = true" :disabled="isApproved == true">
      <i class="fa fa-check"></i>
    </rck-btn>
    <rck-btn btnType="red" class="btn-circle" @click="isApproved = false" :disabled="isApproved == false">
      <i class="fa fa-times"></i>
    </rck-btn>
  </div>
</div>
`
});
