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
        color="inprogress"
        v-if="r.RequestStatus != 'In Progress' && r.RequestStatus != 'Pending Changes'"
        @click="setInProgress"
        v-bind="attrs"
        v-on="on"
      >
        <v-icon>mdi-progress-check</v-icon>
      </v-btn>
    </template>
    <span>In Progress</span>
  </v-tooltip>
  <v-tooltip bottom>
    <template v-slot:activator="{ on, attrs }">
      <v-btn fab small 
        style="margin: 0px 2px;" 
        color="accentDark"
        v-if="r.RequestStatus == 'Pending Changes'"
        @click="partialApproval"
        v-bind="attrs"
        v-on="on"
      >
        <v-icon>mdi-format-list-checks</v-icon>
      </v-btn>
    </template>
    <span>Partial Approval</span>
  </v-tooltip>
  <v-tooltip bottom>
    <template v-slot:activator="{ on, attrs }">
      <v-btn fab small 
        style="margin: 0px 2px;" 
        color="accent"
        v-if="r.RequestStatus == 'Submitted' || r.RequestStatus == 'In Progress' || r.RequestStatus == 'Pending Changes'"
        @click="setApproved"
        v-bind="attrs"
        v-on="on"
      >
        <v-icon>mdi-check-circle</v-icon>
      </v-btn>
    </template>
    <span>Approve <span v-if="r.RequestStatus == 'Pending Changes'">All</span></span>
  </v-tooltip>
  <v-tooltip bottom>
    <template v-slot:activator="{ on, attrs }">
      <v-btn fab small 
        style="margin: 0px 2px;" 
        v-if="r.RequestStatus != 'Approved'" 
        color="primary" 
        @click="addBuffer"
        v-bind="attrs"
        v-on="on"
      >
        <v-icon>mdi-clock-outline</v-icon>
      </v-btn>
    </template>
    <span>Add Buffer</span>
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
    addBuffer() {
      this.$emit("calladdbuffer", this.r)
    },
    setApproved() {
      this.$emit("setapproved", this.r)
    },
    setInProgress() {
      this.$emit("setinprogress", this.r)
    },
    partialApproval() {
      this.$emit("partialapproval", this.r)
    }
  },
  watch: {
    
  }
}