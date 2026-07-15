import { defineComponent } from "vue"
import { CurrentPersonBag } from "@Obsidian/ViewModels/Crm/currentPersonBag"
import { useStore } from "@Obsidian/PageState"
import Comment from "./comment"

const store = useStore();

export default defineComponent({
    name: "EventDashboard.Components.Modal.CommentContainer",
    components: {
      "tcc-comment": Comment,

    },
    props: {
      comments: Array
    },
    setup() {

    },
    data() {
        return {
          
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
        $('.comment-container').scrollTop($('.comment-container')[0].scrollHeight)
      }
    },
    watch: {
      comments: {
        handler(val) {
          this.scrollComments()
        },
        deep: true
      }
    },
    mounted() {
      this.scrollComments()
    },
    template: `
<h3 class="text-accent">Comments</h3>
<div class="comment-container inset-shadow no-track">
    <tcc-comment v-for="(c, idx) in comments" :comment="c.comment" :createdBy="c.createdBy" :next="getNextComment(idx)" :key="c.comment.idKey"></tcc-comment>
</div>
<v-style>
  .comment-container {
    max-height: 350px;
    overflow-y: scroll;
    border-radius: 8px;
    padding: 8px;
  }
</v-style>
`
});
