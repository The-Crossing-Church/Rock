import { defineComponent } from "vue"
import { CurrentPersonBag } from "@Obsidian/ViewModels/Crm/currentPersonBag"
import { useStore } from "@Obsidian/PageState"
import RockText from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import RockButton from "@Obsidian/Controls/rockButton.obs"
import RockHtml from "@Obsidian/Controls/htmlEditor.obs"
import RockDDL from "@Obsidian/Controls/dropDownList.obs"
import Comment from "./comment"
import helper from "../Helpers/helper"

const store = useStore();

export default defineComponent({
  name: "EventDashboard.Components.Modal.CommentContainer",
  components: {
    "tcc-comment": Comment,
    "rck-text": RockText,
    "rck-lbl": RockLabel,
    "rck-btn": RockButton,
    "rck-html": RockHtml,
    "rck-ddl": RockDDL
  },
  props: {
    comments: Array,
    role: String,
    request: Object,
    modelValue: Boolean
  },
  setup() {

  },
  data() {
    return {
      helper: helper,
      comment: "",
      showTags: false,
      tags: [] as any[]
    };
  },
  computed: {
    /** The person currently authenticated */
    currentPerson(): CurrentPersonBag | null {
      return store.state.currentPerson
    },
    isDisabled() {
      if(this.comment && this.comment != '') {
        return false
      }
      return true
    },
    tagOptions() {
      let options = [] as any[]
      if(this.role != "Ministry") {
        if(this.request?.createdBy) {
          options.push({ text: this.request.createdBy, value: "Ministry" })
        } else {
          options.push({ text: "The Ministry Event Admin", value: "Ministry" })
        }
      } 
      if (this.role != "Admin") {
        options.push({ text: "The OPS Events Director", value: "Admin" })
      }

      if(this.request?.attributeValues) {
        this.helper.sectionInfo.filter(si => si.ssp).forEach(si => {
          let attrKey = si.attr as string
          if(this.request?.attributeValues[attrKey] == "True") {
            options.push({ text: `The ${si.ssp} Support Service Provider`, value: si.ssp })
          }
        })
      } else {
        options.push(...this.helper.sectionInfo.map(si => { return { text: `The ${si.ssp} Support Service Provider`, value: si.ssp } }))
      }
      let ssps = options.map(opt => opt.value)
      options = options.filter((opt, idx, list) => {
        if(opt && ssps.indexOf(opt.value) == idx) {
          return true
        }
        return false
      })

      return options
    },
    notificationPreview() {
      let previewText = ""
      if(this.tags.length == 0) {
        previewText = "No one"
      } else if (this.tags.length < 3) {
        previewText = this.tagOptions.filter(opt => this.tags.includes(opt.value)).map(opt => opt.text).join(" and ")
      } else {
        previewText = this.tagOptions.filter(opt => this.tags.includes(opt.value)).map(opt => opt.text).join(", ").replace(/, (?!.*, )/, ", and ")
      }
      previewText += " will be notified."
      return previewText
    }
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
      this.$emit('createComment', this.comment, this.tags)
    },
    toggleNewComment(val) {
      if(val) {
        $('.new-comment').collapse('show')
      } else {
        this.comment = ""
        $('.new-comment').collapse('hide')
      }
    },
    cancel() {
      this.$emit("update:modelValue", false)
    }
  },
  watch: {
    comments: {
      handler(val) {
        this.scrollComments()
      },
      deep: true
    },
    modelValue(val) {
      this.toggleNewComment(val)
      if(!val) {
        //reset tags when new comment is closed
        this.tags = []
        if(this.role == "Ministry") {
          this.tags.push("Admin")
        } else {
          this.tags.push("Ministry")
        }
      }
    }
  },
  mounted() {
    this.scrollComments()
    this.toggleNewComment(this.modelValue)
    
    if(this.role == "Ministry") {
      this.tags.push("Admin")
    } else {
      this.tags.push("Ministry")
    }
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
  <small class="text-muted">
    {{ notificationPreview }}
  </small>
  <div class="d-flex mt-2">
    <rck-btn btnType="primary" @click="showTags = !showTags">
      <i class="mr-1 ti ti-at"></i>
      Tag
    </rck-btn>
    <div class="spacer"></div>
    <rck-btn class="mr-2" btnType="grey" @click="cancel">
      <i class="mr-1 fa fa-ban"></i>
      Cancel
    </rck-btn>
    <rck-btn btnType="accent" @click="createComment" :disabled="isDisabled">
      <i class="mr-1 fa fa-comment-alt"></i>
      Add Comment
    </rck-btn>
  </div>
  <div v-if="showTags">
    <rck-ddl
      label="Groups to Notify"
      :items="tagOptions"
      v-model="tags"
      multiple
    ></rck-ddl>
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
