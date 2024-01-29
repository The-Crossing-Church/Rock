import { defineComponent, PropType } from "vue";
import { ContentChannelItemBag } from "@Obsidian/ViewModels/Entities/contentChannelItemBag"
import { Popover, Button, Badge } from "ant-design-vue"


export default defineComponent({
    name: "EventDashboard.Components.AdminGridAction",
    components: {
      "a-btn": Button,
      "a-pop": Popover,
      "a-badge": Badge
    },
    props: {
      request: Object as PropType<ContentChannelItemBag>,
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
      }
    },
    methods: {
      updateStatus(id: string, status: string) {
        this.$emit("updatestatus", id, status)
      },
      addBuffer(id: string) {
        this.$emit("addbuffer", id)
        this.visible = false
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<a-badge :count="request.attributeValues.CommentNotifications">
  <a-pop v-model:visible="visible" trigger="click" placement="right">
    <template #content>
      <a-btn class="mr-1" shape="circle" type="yellow" v-if="request.attributeValues.RequestStatus != 'In Progress'" @click="updateStatus(request.idKey, 'In Progress')">
        <i class="fas fa-tasks"></i>
      </a-btn>
      <a-btn class="mr-1" shape="circle" type="accent" v-if="request.attributeValues.RequestStatus != 'Approved'" @click="updateStatus(request.idKey, 'Approved')">
        <i class="fas fa-check-circle"></i>
      </a-btn>
      <a-btn shape="circle" type="primary" v-if="request.attributeValues.RequestStatus != 'Approved'" @click="addBuffer(request.idKey)">
        <i class="far fa-clock"></i>
      </a-btn>
    </template>
    <a-btn :type="btnColor" @click="visible = !visible">{{request.attributeValues.RequestStatus}}</a-btn>
  </a-pop>
</a-badge>
`
});
