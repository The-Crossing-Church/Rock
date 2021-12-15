export default {
  template: `
    <v-form ref="zoomForm" v-model="valid">
      <v-row>
        <v-col>
          <h3 class="primary--text">Zoom Information</h3>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-text-field
            label="If there is a link your attendees will need to access this event, list it here"
            v-model="e.EventURL"
            :rules="[rules.required(e.EventURL, 'Link')]"
          ></v-text-field>
        </v-col>
      </v-row>
      <v-row>
        <v-col>
          <v-text-field
            label="If there is a password for the link, list it here"
            v-model="e.ZoomPassword"
          ></v-text-field>
        </v-col>
      </v-row>
    </v-form>
  `,
  props: ["e", "request", "existing"],
  data: function () {
    return {
      valid: true,
      rules: {
        required(val, field) {
          return !!val || `${field} is required`;
        },
      }
    }
  },
  created: function () {
    
  },
  filters: {
    
  },
  computed: {
    
  },
  watch: {
    
  },
  methods: {
    
  },
}
