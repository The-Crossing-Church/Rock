export default {
  template: `
<v-speed-dial
  v-model="fab"
  transition="slide-x-transition"
  direction="right"
>
  <template v-slot:activator>
    <v-btn
      v-model="fab"
      :color="statusBtnColor"
      style="min-width:175px;"
    >
      <template v-if="fab">
        <v-icon>
          mdi-close
        </v-icon>
      </template>
      <template v-else>
        {{ r.RequestStatus }}
      </template>
    </v-btn>
  </template>
  <v-tooltip bottom>
    <template v-slot:activator="{ on, attrs }">
      <v-btn fab small 
        style="margin: 0px 2px;" 
        color="primary"
        v-if="r.RequestStatus == 'Draft' || r.RequestStatus == 'Submitted' || r.RequestStatus == 'In Progress' || r.RequestStatus == 'Approved'"
        @click="edit"
        v-bind="attrs"
        v-on="on"
      >
        <v-icon>mdi-pencil</v-icon>
      </v-btn>
    </template>
    <span>Edit</span>
  </v-tooltip>
  <v-tooltip bottom>
    <template v-slot:activator="{ on, attrs }">
      <v-btn fab small 
        style="margin: 0px 2px;" 
        color="grey"
        v-if="!r.RequestStatus.includes('Cancelled') && r.RequestStatus != 'Draft' && r.RequestStatus != 'Denied'"
        @click="cancel"
        v-bind="attrs"
        v-on="on"
      >
        <v-icon>mdi-cancel</v-icon>
      </v-btn>
    </template>
    <span>Cancel Request</span>
  </v-tooltip>
  <v-tooltip bottom v-if="r.RequestStatus != 'Draft' && !r.RequestStatus.includes('Cancelled') && r.RequestStatus != 'Denied'">
    <template v-slot:activator="{ on, attrs }">
      <v-btn fab small 
        style="margin: 0px 2px;" 
        color="accent" 
        @click="addComment"
        v-bind="attrs"
        v-on="on"
      >
        <v-icon>mdi-comment-edit</v-icon>
      </v-btn>
    </template>
    <span>Add Comment</span>
  </v-tooltip>
  <v-tooltip bottom>
    <template v-slot:activator="{ on, attrs }">
      <v-btn fab small 
        style="margin: 0px 2px;" 
        color="pending" 
        @click="resubmit"
        v-bind="attrs"
        v-on="on"
      >
        <v-icon>mdi-calendar-refresh</v-icon>
      </v-btn>
    </template>
    <span>Resubmit</span>
  </v-tooltip>
</v-speed-dial>
`,
  props: ["r"],
  data: function () {
    return {
      fab: false
    }
  },
  created: function () {
    
  },
  filters: {
    
  },
  computed: {
    statusBtnColor() {
      let color = "primary";
      switch(this.r.RequestStatus){
        case "Draft":
          color = "draft"
          break;
        case "Approved":
          color = "accent"
          break;
        case "In Progress":
          color = "inprogress"
          break;
        case "Cancelled":
        case "Cancelled by User":
          color = "secondary"
          break;
        case "Denied":
        case "Proposed Changes Denied":
          color = "denied"
          break;
        case "Pending Changes":
        case "Changes Accepted by User":
          color="pending"
          break;
      }
      return color
    }
  },
  methods: {
    edit() {
      this.$emit("editrequest", this.r)
    },
    cancel() {
      this.$emit("cancelrequest", this.r)
    },
    resubmit() {
      this.$emit("resubmitrequest", this.r)
    },
    addComment() {
      this.$emit("commentrequest", this.r)
    },
  },
  watch: {
    
  }
}