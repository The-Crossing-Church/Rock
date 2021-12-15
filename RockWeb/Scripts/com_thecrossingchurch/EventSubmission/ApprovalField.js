export default {
  template: `
    <v-row>
      <v-col class="d-flex align-center">
        <span :class='originalClass'>{{original}}: </span>
        <span :class='proposedClass'> {{proposed}}</span>
      </v-col>
      <v-col cols="3">
        <v-btn fab small color="accent" @click="approveChange" :disabled="choiceMade && isApproved">
          <v-icon>mdi-check-circle</v-icon>
        </v-btn>
        <v-btn fab small color="red" @click="denyChange" :disabled="choiceMade && !isApproved">
          <v-icon>mdi-cancel</v-icon>
        </v-btn>
      </v-col>
    </v-row>
  `,
  props: ["e", "request", "idx", "field", "fieldname", "formatter"],
  data: function () {
    return {
      isApproved: false,
      choiceMade: false
    }
  },
  created: function () {
    this.$emit("newchange")
  },
  filters: {
    
  },
  computed: {
    original() {
      if(this.formatter) {
        if(this.e != null) {
          return this.e[this.field] ? this.formatter(this.e[this.field]) : 'Empty'
        } else {
          return this.request[this.field] ? this.formatter(this.request[this.field]) : 'Empty'
        }
      } else {
        if(this.e != null) {
          return this.e[this.field] ? this.e[this.field] : 'Empty'
        } else {
          return this.request[this.field] ? this.request[this.field] : 'Empty'
        }
      }
    },
    proposed() {
      if(this.formatter) {
        if(this.idx != null) {
          return this.request.Changes.Events[this.idx][this.field] ? this.formatter(this.request.Changes.Events[this.idx][this.field]) : 'Empty'
        } else {
          return this.request.Changes[this.field] ? this.formatter(this.request.Changes[this.field]) : 'Empty'
        }
      } else {
        if(this.idx != null) {
          return this.request.Changes.Events[this.idx][this.field] ? this.request.Changes.Events[this.idx][this.field] : 'Empty'
        } else {
          return this.request.Changes[this.field] ? this.request.Changes[this.field] : 'Empty'
        }
      }
    },
    originalClass() {
      if(this.choiceMade && !this.isApproved) {
        return 'red--text font-weight-black'
      }
      return 'red--text'
    },
    proposedClass() {
      if(this.choiceMade && this.isApproved) {
        return 'primary--text font-weight-black'
      }
      return 'primary--text'
    }
  },
  methods: {
    approveChange() {
      this.choiceMade = true
      this.isApproved = true
      this.$emit("approvechange", {field: this.field, label: this.fieldname, idx: this.idx})
    },
    denyChange() {
      this.choiceMade = true
      this.isApproved = false
      this.$emit("denychange", {field: this.field, label: this.fieldname, idx: this.idx})
    }
  },
  watch: {
    choiceMade(val) {
      this.$emit("newchoice")
    }
  }
}
