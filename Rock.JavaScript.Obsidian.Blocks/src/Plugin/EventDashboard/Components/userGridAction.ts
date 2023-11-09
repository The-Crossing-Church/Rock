import { defineComponent, PropType } from "vue";
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { Popover, Button, Badge } from "ant-design-vue"


export default defineComponent({
    name: "EventDashboard.Components.UserGridAction",
    components: {
      "a-btn": Button,
      "a-pop": Popover,
      "a-badge": Badge
    },
    props: {
      request: Object as PropType<ContentChannelItemBag>,
      url: String,
      commentNotification: Number,
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
      },
    },
    methods: {
      updateStatus(id: number, status: string) {
        this.$emit("updatestatus", id, status)
      },
      edit() {
        // window.location.href = "/eventform?Id=" + this.request?.id
      },
      duplicate() {
        // this.$emit("duplicate", this.request?.id)
        this.visible = false
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<a-badge :count="commentNotification" style="width: 200px;">
  <a-pop v-model:visible="visible" trigger="click" placement="right">
    <template #content>
      <a-btn class="mr-1" shape="circle" type="primary" v-if="canEdit" @click="edit">
        <i class="fa fa-pencil-alt"></i>
      </a-btn>
      <a-btn class="mr-1" shape="circle" type="grey" v-if="!request.attributeValues.RequestStatus.includes('Cancelled')" @click="updateStatus(request.id, 'Cancelled by User')">
        <i class="fa fa-ban"></i>
      </a-btn>
      <a-btn shape="circle" type="med-blue" @click="duplicate">
        <i class="fas fa-history"></i>
      </a-btn>
    </template>
    <a-btn :type="btnColor" @click="visible = !visible">{{request.attributeValues.RequestStatus}}</a-btn>
  </a-pop>
</a-badge>
`
});
