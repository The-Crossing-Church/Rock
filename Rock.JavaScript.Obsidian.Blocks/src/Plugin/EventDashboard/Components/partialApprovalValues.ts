import { defineComponent } from "vue"
import RockField from "@Obsidian/Controls/rockField"
import { Button } from "ant-design-vue"

export default defineComponent({
    name: "EventDashboard.Components.Modal.PartialApproval.Values",
    components: {
      "rck-field": RockField,
      "a-btn": Button,
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
    <a-btn shape="circle" type="accent" class="mr-1" @click="isApproved = true" :disabled="isApproved == true">
      <i class="fa fa-check"></i>
    </a-btn>
    <a-btn shape="circle" type="red" @click="isApproved = false" :disabled="isApproved == false">
      <i class="fa fa-times"></i>
    </a-btn>
  </div>
</div>
`
});
