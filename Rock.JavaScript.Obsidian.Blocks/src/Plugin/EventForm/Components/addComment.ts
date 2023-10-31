import { defineComponent, PropType } from "vue"
import { DateTime, Duration } from "luxon"
import { Button, Modal} from "ant-design-vue"
import { ContentChannelItem } from "../../../../ViewModels"
import RockText from "../../../../Elements/textBox"
import RockLabel from "../../../../Elements/rockLabel"


export default defineComponent({
    name: "EventForm.Components.AddComment",
    components: {
      "a-btn": Button,
      "a-modal": Modal,
      "rck-text": RockText,
      "rck-lbl": RockLabel
    },
    props: {
      request: {
        type: Object as PropType<ContentChannelItem>,
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
      <a-btn type="primary" @click="newComment">
        <i class="fas fa-comment-alt pr-2"></i>
        Contact
      </a-btn>
    </div>
  </div>
</div>
<a-modal v-if="modal" v-model:visible="modal" width="80%">
  <rck-lbl>Customize Your Message</rck-lbl>
  <rck-text
    v-model="comment"
    textMode="multiline"
  ></rck-text>
  <template #footer>
    <a-btn type="accent" @click="createComment">
      <i class="mr-1 fa fa-comment-alt"></i>
      Add Comment
    </a-btn>
    <a-btn type="grey" @click="modal = false;">
      <i class="mr-1 fa fa-ban"></i>
      Cancel
    </a-btn>
  </template>
</a-modal>
`
});
