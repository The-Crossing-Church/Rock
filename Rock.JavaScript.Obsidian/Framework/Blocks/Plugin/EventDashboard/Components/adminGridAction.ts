import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import { Popover, Button } from "ant-design-vue"


export default defineComponent({
    name: "EventDashboard.Components.AdminGridAction",
    components: {
      "a-btn": Button,
      "a-pop": Popover,
    },
    props: {
      request: Object as PropType<ContentChannelItem>
    },
    setup() {

    },
    data() {
        return {
          visible: false
        };
    },
    computed: {
      btnColor() {
        if(this.request?.attributeValues?.RequestStatus == "Submitted") {
          return "primary"
        }
        if(this.request?.attributeValues?.RequestStatus == "In Progress") {
          return "inprogress"
        }
        if(this.request?.attributeValues?.RequestStatus == "Pending Changes") {
          return "pendingchanges"
        }
      }
    },
    methods: {

    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<a-pop v-model:visible="visible" trigger="click" placement="right">
  <template #content>
    <a-btn shape="circle" type="yellow" v-if="request.attributeValues.RequestStatus != 'In Progress'">
      <i class="fas fa-tasks"></i>
    </a-btn>
    <a-btn shape="circle" type="accent" v-if="request.attributeValues.RequestStatus != 'Approved'">
      <i class="fas fa-check-circle"></i>
    </a-btn>
    <a-btn shape="circle" type="primary" v-if="request.attributeValues.RequestStatus != 'Approved'">
      <i class="far fa-clock"></i>
    </a-btn>
  </template>
  <a-btn :type="btnColor" @click="visible = !visible">{{request.attributeValues.RequestStatus}}</a-btn>
</a-pop>
`
});
