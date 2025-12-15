import { defineComponent, PropType } from "vue";
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { Badge } from "ant-design-vue"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import PopOver from "@Obsidian/Controls/popOver.obs"


export default defineComponent({
    name: "EventDashboard.Components.AdminGridAction",
    components: {
      "a-badge": Badge,
      "rck-pop": PopOver,
      "rck-btn": RockButton
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
        return this.request?.attributeValues?.RequestStatus.replace(" ", "").replace(" ", "").toLowerCase().replace("legacyrequest", "default")
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
  <rck-pop v-model:isVisible="visible" trigger="click" placement="top">
    <div class="d-flex">
      <rck-btn class="mr-1 btn-circle" btnType="yellow" v-if="request.attributeValues.RequestStatus != 'In Progress'" @click="updateStatus(request.idKey, 'In Progress')">
        <i class="fas fa-tasks"></i>
      </rck-btn>
      <rck-btn class="mr-1 btn-circle" btnType="accent" v-if="request.attributeValues.RequestStatus != 'Approved'" @click="updateStatus(request.idKey, 'Approved')">
        <i class="fas fa-check-circle"></i>
      </rck-btn>
      <rck-btn class="btn-circle" btnType="primary" v-if="request.attributeValues.RequestStatus != 'Approved'" @click="addBuffer(request.idKey)">
        <i class="ti ti-clock"></i>
      </rck-btn>
    </div>
    <template #activator="props">
      <rck-btn :btnType="btnColor" @click="visible = !visible">{{request.attributeValues.RequestStatus}}</rck-btn>
    </template>
  </rck-pop>
</a-badge>
`
});
