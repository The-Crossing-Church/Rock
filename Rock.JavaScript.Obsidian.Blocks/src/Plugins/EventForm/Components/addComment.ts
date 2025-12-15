import { defineComponent, PropType } from "vue"
import { ContentChannelItemBag } from "../../ViewModels/contentChannelItemBag"
import RockText from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import Modal from "@Obsidian/Controls/modal.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"


export default defineComponent({
    name: "EventForm.Components.AddComment",
    components: {
      "rck-btn": RockButton,
      "rck-modal": Modal,
      "rck-text": RockText,
      "rck-lbl": RockLabel
    },
    props: {
      request: {
        type: Object as PropType<ContentChannelItemBag>,
        required: false
      },
    },
    setup() {

    },
    data() {
      return {
        modal: false,
        comment: ""
      };
    },
    computed: {
      
    },
    methods: {
      newComment() {
        this.comment = 'Please move this request to In Progress so I can modify the event dates.' 
        this.modal = true
      },
      createComment() {
        this.modal = false
        this.$emit('addComment', this.comment)
      },
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div class="py-4 text-center w-100">
  <div class="row">
    <div class="col col-xs-12">
      The current state of this request prohibits modifying the event dates. Contact the Events Director to have your request be moved to <i>In Progress</i> so you can make date changes.
    </div>
  </div>
  <div class="row">
    <div class="col col-xs-12 pt-4">
      <rck-btn btnType="primary" @click="newComment" id="btnContact">
        <i class="fas fa-comment-alt pr-2"></i>
        Contact
      </rck-btn>
    </div>
  </div>
</div>
<rck-modal v-if="modal" v-model="modal" width="80%" :isCloseButtonHidden="true" cancelText="" :clickBackdropToClose="true" modalWrapperClasses="modal-no-header">
  <rck-lbl>Customize Your Message</rck-lbl>
  <rck-text
    v-model="comment"
    textMode="multiline"
    id="txtComment"
  ></rck-text>
  <template #footer>
    <rck-btn btnType="accent" @click="createComment" id="btnAddComment">
      <i class="mr-1 fa fa-comment-alt"></i>
      Add Comment
    </rck-btn>
    <rck-btn btnType="grey" @click="modal = false;" id="btnCancel">
      <i class="mr-1 fa fa-ban"></i>
      Cancel
    </rck-btn>
  </template>
</rck-modal>
`
});
