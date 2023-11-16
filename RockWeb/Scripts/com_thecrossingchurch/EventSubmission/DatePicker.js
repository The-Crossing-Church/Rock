export default {
  template: `
  <div>
    <v-menu
      ref="menu"
      v-model="menu"
      :close-on-content-click="false"
      transition="scale-transition"
      offset-y
      max-width="290px"
      min-width="290px"
    >
      <template v-slot:activator="{ on }">
        <v-text-field
          prepend-inner-icon="mdi-calendar"
          v-model="dateFormatted"
          :label="label"
          :readonly="readonly"
          :rules="rules"
          @blur="dateISO = parseDate(dateFormatted)"
          v-on="on"
          :hint="hint"
          :persistent-hint="persistentHint"
          :clearable="clearable"
        ></v-text-field>
      </template>
      <v-date-picker
        v-model="dateISO"
        @input="menu = false"
        :min="min"
        :max="max"
        :show-current="showCurrent"
      ></v-date-picker>
    </v-menu>
  </div>
`,
  props: ["label", "readonly", "date", "rules", "hint", "persistentHint", "clearable", "min", "max", "showCurrent"],
  data() {
    return {
      menu: false,
      dateISO: null,
      dateFormatted: ''
    }
  },
  created() {
    if(this.date && moment(this.date).isValid()) {
      this.dateISO = moment(this.date).format('YYYY-MM-DD')
      this.dateFormatted = moment(this.date).format('MM/DD/YYYY')
    } else {
      this.dateISO = null
      this.dateFormatted = ''
    }
  },
  methods: {
    parseDate(val) {
      if (!val) {
        return null
      }
      let [month, day, year] = val.split('/')
      return `${year}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`
    },
  },
  watch: {
    date(val) {
      if(this.date && moment(this.date).isValid()) {
        this.dateISO = moment(this.date).format('YYYY-MM-DD')
        this.dateFormatted = moment(this.date).format('MM/DD/YYYY')
      } 
    },
    dateISO(val) {
      if(moment(this.dateISO).isValid()){
        this.dateFormatted = moment(this.dateISO).format('MM/DD/YYYY')
      } else {
        this.dateFormatted = ''
      }
    },
    dateFormatted(val) {
      this.$emit('input', val)
    },
    menu(val) {
      if(val) {
        if(!moment(this.dateISO).isValid()){
          this.dateISO = moment(new Date()).format('YYYY-MM-DD')
        }
      }
    }
  }
}