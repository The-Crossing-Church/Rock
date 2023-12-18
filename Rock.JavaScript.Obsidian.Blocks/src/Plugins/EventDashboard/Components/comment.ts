import { defineComponent } from "vue";
import { PersonBag } from "@Obsidian/ViewModels/Entities/personBag";
import { useStore } from "@Obsidian/PageState";
import RockField from "@Obsidian/Controls/rockField"
import { DateTime } from "luxon"

const store = useStore();

export default defineComponent({
    name: "EventDashboard.Components.Modal.Comment",
    components: {
      "rck-field": RockField
    },
    props: {
      comment: Object,
      createdBy: String,
      next: Object,
    },
    setup() {

    },
    data() {
        return {
          
        };
    },
    computed: {
      /** The person currently authenticated */
      currentPerson(): PersonBag | null {
          return store.state.currentPerson
      },
      className() {
        let cname = "note-wrapper"
        if(this.next && this.next?.createdBy != this.createdBy) {
          cname += " mb-2"
        }
        if(this.createdBy == this.currentPerson?.fullName) {
          cname += " note-wrapper my-note"
        }
        return cname
      },
      avatarName() {
        if(this.createdBy) {
          let nameParts = this.createdBy.split(" ")
          return nameParts[0][0] + nameParts[1][0]
        }
        return ""
      },
      collapseName() {
        return "collapse_" + this.comment?.id
      },
      collapseId() {
        return "#collapse_" + this.comment?.id
      }
    },
    methods: {
      formatDateTime(date: string) {
        return DateTime.fromISO(date).toFormat("MM/dd/yyyy hh:mm a")
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div :class="className">
  <div class="avatar-wrapper" v-if="!className.includes('my-note')" data-toggle="collapse" :data-target="collapseId" aria-expanded="false" :aria-controls="collapseName">
    <div class="avatar hover" v-if="next == null || next.createdBy != createdBy">
      {{avatarName}}
    </div>
    <div style="width: 30px;" v-else></div>
  </div>
  <div class="note">
    <div><small>{{formatDateTime(comment.createdDateTime)}}</small></div>
    <div class="content">
      {{comment.content}}
    </div>
  </div>
  <div class="avatar-wrapper" v-if="className.includes('my-note')" data-toggle="collapse" :data-target="collapseId" aria-expanded="false" :aria-controls="collapseName">
    <div class="avatar hover" v-if="next == null || next.createdBy != createdBy">
      {{avatarName}}
    </div>
    <div style="width: 30px;" v-else></div>
  </div>
</div>
<div :id="collapseName" class="collapse avatar-fullName" v-if="(next == null || next.createdBy != createdBy)">
  {{createdBy}}
</div>
<v-style>
  .note {
    background-color: lightgrey;
    padding: 8px;
    border-radius: 6px 6px 6px 0px;
    margin: 4px 0px;
    max-width: 80%;
    width: fit-content;
  }
  .note-wrapper {
    display: flex;
  }
  .note-wrapper.my-note {
    text-align: right;
    justify-content: flex-end;
  }
  .my-note .note {
    border-radius: 6px 6px 0px 6px;
  }
  .my-note + .avatar-fullName {
    text-align: right;
  }
  .avatar-wrapper {
    padding-right: 4px;
    display: flex;
    align-items: end;
  }
  .my-note .avatar-wrapper {
    padding-right: 0px;
    padding-left: 4px;
  }
  .avatar {
    width: 30px;
    height: 30px;
    border-radius: 15px;
    background-color: lightgrey;
    font-size: 14px;
    display: flex;
    justify-content: center;
    align-items: center;
  }
</v-style>
`
});
