import { defineComponent, PropType } from "vue";
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import { Badge } from "ant-design-vue"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import PopOver from "@Obsidian/Controls/popOver.obs"


export default defineComponent({
    name: "EventDashboard.Components.UserGridAction",
    components: {
      "a-badge": Badge,
      "rck-pop": PopOver,
      "rck-btn": RockButton
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
        return this.request?.attributeValues?.RequestStatus.replace(" ", "").replace(" ", "").toLowerCase().replace("legacyrequest", "default")
      },
      canEdit() {
        if(this.request?.attributeValues?.RequestStatus) {
          return this.request.attributeValues.RequestStatus == 'Submitted' || this.request.attributeValues.RequestStatus == 'In Progress' || this.request.attributeValues.RequestStatus == 'Approved' || this.request.attributeValues.RequestStatus == 'Pending Changes'
        }
        return false
      },
    },
    methods: {
      updateStatus(id: string, status: string) {
        this.$emit("updatestatus", id, status)
      },
      edit() {
        window.location.href = "/eventform?Id=" + this.request?.idKey
      },
      duplicate() {
        this.$emit("duplicate", this.request?.idKey)
        this.visible = false
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<a-badge :count="commentNotification">
  <rck-pop v-model:isVisible="visible" trigger="click" placement="top">
    <div class="d-flex">
      <rck-btn class="mr-1 btn-circle" btnType="primary" v-if="canEdit" @click="edit">
        <i class="fa fa-pencil-alt"></i>
      </rck-btn>
      <rck-btn class="mr-1 btn-circle" btnType="grey" v-if="!request.attributeValues.RequestStatus.includes('Cancelled')" @click="updateStatus(request.idKey, 'Cancelled by User')">
        <i class="fa fa-ban"></i>
      </rck-btn>
      <rck-btn class="btn-circle" btnType="med-blue" @click="duplicate">
        <i class="fas fa-history"></i>
      </rck-btn>
    </div>
    <template #activator="props">
      <rck-btn :btnType="btnColor" @click="visible = !visible">{{request.attributeValues.RequestStatus}}</rck-btn>
    </template>
  </rck-pop>
</a-badge>
`
});
