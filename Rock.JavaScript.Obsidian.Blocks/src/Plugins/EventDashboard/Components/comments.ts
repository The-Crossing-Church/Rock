import { defineComponent } from "vue"
import { CurrentPersonBag } from "@Obsidian/ViewModels/Crm/currentPersonBag"
import { useStore } from "@Obsidian/PageState"
import RockText from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import RockHtml from "@Obsidian/Controls/htmlEditor.obs"
import Comment from "./comment"

const store = useStore();

export default defineComponent({
  name: "EventDashboard.Components.Modal.CommentContainer",
  components: {
    "tcc-comment": Comment,
    "rck-text": RockText,
    "rck-lbl": RockLabel,
    "rck-btn": RockButton,
    "rck-html": RockHtml
  },
  props: {
    comments: Array,
    newComment: Boolean
  },
  setup() {

  },
  data() {
    return {
      comment: ""
    };
  },
  computed: {
    /** The person currently authenticated */
    currentPerson(): CurrentPersonBag | null {
      return store.state.currentPerson
    },
  },
  methods: {
    getNextComment(idx: number) {
      if(this.comments) {
        if(idx < (this.comments.length - 1)) {
          return this.comments[idx + 1]
        }
        return null
      }
    },
    scrollComments() {
      if(this.comments && this.comments.length > 0) {
        if($('.comment-container')[0]) {
          $('.comment-container').scrollTop($('.comment-container')[0].scrollHeight)
        }
      }
    },
    createComment() {
      this.$emit('createComment', this.comment)
    },
    toggleNewComment(val) {
      if(val) {
        $('.new-comment').collapse('show')
      } else {
        this.comment = ""
        $('.new-comment').collapse('hide')
      }
    }
  },
  watch: {
    comments: {
      handler(val) {
        this.scrollComments()
      },
      deep: true
    },
    newComment(val) {
      this.toggleNewComment(val)
    }
  },
  mounted() {
    this.scrollComments()
    this.toggleNewComment(this.newComment)
  },
  template: `
<h3 class="text-accent">Comments</h3>
<div v-if="comments && comments.length > 0" class="comment-container inset-shadow no-track">
  <tcc-comment v-for="(c, idx) in comments" :comment="c.comment" :createdBy="c.createdBy" :next="getNextComment(idx)" :key="c.comment.idKey"></tcc-comment>
</div>
<div class="new-comment collapse pt-1">
  <rck-lbl>New Comment</rck-lbl>
  <rck-html
    v-model="comment"
    :editorHeight="200"
  ></rck-html>
  <div class="d-flex mt-2">
    <div class="spacer"></div>
    <rck-btn class="mr-2" btnType="grey" @click="newComment = false;">
      <i class="mr-1 fa fa-ban"></i>
      Cancel
    </rck-btn>
    <rck-btn btnType="accent" @click="createComment">
      <i class="mr-1 fa fa-comment-alt"></i>
      Add Comment
    </rck-btn>
  </div>
</div>
<v-style>
  .comment-container {
    max-height: 350px;
    overflow-y: scroll;
    border-radius: 8px;
    padding: 8px;
  }
  .new-comment {
    
  }
</v-style>
`
});
