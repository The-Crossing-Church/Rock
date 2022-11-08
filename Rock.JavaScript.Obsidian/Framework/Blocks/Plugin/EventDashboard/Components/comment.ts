import { defineComponent } from "vue";
import { Person } from "../../../../ViewModels";
import { useStore } from "../../../../Store/index";
import RockField from "../../../../Controls/rockField"
import { DateTime } from "luxon"

const store = useStore();

export default defineComponent({
    name: "EventDashboard.Components.Modal.Comment",
    components: {
      "rck-field": RockField
    },
    props: {
      comment: Object,
      createdBy: String
    },
    setup() {

    },
    data() {
        return {
          
        };
    },
    computed: {
      /** The person currently authenticated */
      currentPerson(): Person | null {
          return store.state.currentPerson
      },
      className() {
        if(this.createdBy == this.currentPerson?.fullName) {
          return "note-wrapper my-note"
        }
        return "note-wrapper"
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
  <div class="note">
    <div><strong>{{createdBy}}</strong> - {{formatDateTime(comment.createdDateTime)}}</div>
    <div class="content">
      {{comment.content}}
    </div>
  </div>
</div>
<v-style>
  .note {
    background-color: lightgrey;
    padding: 8px;
    border-radius: 6px;
    margin: 4px 0px;
    width: 80%;
  }
  .my-note {
    display: flex;
    text-align: right;
    justify-content: flex-end;
  }
</v-style>
`
});
