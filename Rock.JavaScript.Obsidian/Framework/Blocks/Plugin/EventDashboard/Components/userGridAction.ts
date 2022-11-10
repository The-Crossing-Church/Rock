import { defineComponent, PropType } from "vue";
import { ContentChannelItem } from "../../../../ViewModels"
import { Popover, Button } from "ant-design-vue"


export default defineComponent({
    name: "EventDashboard.Components.UserGridAction",
    components: {
      "a-btn": Button,
      "a-pop": Popover,
    },
    props: {
      request: Object as PropType<ContentChannelItem>,
      url: String
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
        return this.request?.attributeValues?.RequestStatus.replace(" ", "").replace(" ", "").toLowerCase()
      },
      canEdit() {
        if(this.request?.attributeValues?.RequestStatus) {
          return this.request.attributeValues.RequestStatus == 'Submitted' || this.request.attributeValues.RequestStatus == 'In Progress' || this.request.attributeValues.RequestStatus == 'Approved' || this.request.attributeValues.RequestStatus == 'Pending Changes'
        }
        return false
      }
    },
    methods: {
      updateStatus(id: number, status: string) {
        this.$emit("updatestatus", id, status)
      },
      approve() {
        window.location.href = this.url + `?Id=${this.request?.id}&Action=Approved`
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<a-pop v-model:visible="visible" trigger="click" placement="right">
  <template #content>
    <a-btn shape="circle" type="primary" v-if="canEdit" @click="">
      <i class="fa fa-pencil-alt"></i>
    </a-btn>
    <a-btn shape="circle" type="grey" v-if="!request.attributeValues.RequestStatus.includes('Cancelled')" @click="updateStatus(request.id, 'Cancelled By User')">
      <i class="fa fa-ban"></i>
    </a-btn>
    <a-btn shape="circle" type="accent">
      <i class="fas fa-comment-alt"></i>
    </a-btn>
    <a-btn shape="circle" type="med-blue">
      <i class="fas fa-history"></i>
    </a-btn>
  </template>
  <a-btn :type="btnColor" @click="visible = !visible">{{request.attributeValues.RequestStatus}}</a-btn>
</a-pop>
`
});
