import { defineComponent, PropType } from "vue"
import RockButton from "@Obsidian/Controls/rockButton.obs"


export default defineComponent({
  name: "EventDashboard.Components.SSP.StatusAction",
  components: {
    "rck-btn": RockButton
  },
  props: {
    emits: String,
    service: String,
    action: String,
    value: String,
    statusKey: String
  },
  setup() {

  },
  data() {
    return {
      
    };
  },
  computed: {
    isVisible() {
      if(this.action == "Needs Confirmation") {
        return false
      }
      return true
    },
    btnText() {
      if(this.service) {
        let term = ""
        if(this.action == "Confirmed") {
          term = 'Confirm'
        } else if (this.action == "Approved") {
          term = 'Approve'
        } else if (this.action == "Denied") {
          term = 'Deny'
        }
        return `${term} ${this.service}`
      }
      return ""
    },
    btnType() {
      if (this.action == "Denied") {
        return 'danger'
      } else {
        return 'accent'
      }
    }
  },
  methods: {
    updateSupportStatus() {
      if(this.emits) {
        this.$emit(this.emits, this.statusKey, this.value)
      }
    },
  },
  watch: {
    
  },
  mounted() {
    
  },
  template: `
<rck-btn v-if="isVisible" :btnType="btnType" @click="updateSupportStatus">{{btnText}}</rck-btn>
`
});
