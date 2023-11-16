export default {
    template: `
      <v-form ref="timeForm" v-model="valid">
        <v-menu
          v-model="menu"
          ref="menu"
          :close-on-content-click="false"
          transition="scale-transition"
          offset-y
          max-width="350px"
          z-index="100"
        >
          <template v-slot:activator="{ on }">
            <v-text-field
              prepend-inner-icon="mdi-clock-outline"
              v-on="on"
              v-model="time"
              readonly
              append-icon="mdi-close"
              @click:append="hour = null; minute = '00'; ap = 'AM'"
              :rules="rules"
              :label="label"
            ></v-text-field>
          </template>
          <v-sheet elevation="2" height="225px" style="padding: 16px;">
            <div>
              <strong>ENTER TIME</strong>
            </div>
            <div>
              <v-chip-group v-if="onlySundays" style="margin-bottom: 12px;">
                <v-chip
                  v-for="(c, idx) in quickSetItems" :key="idx"
                  @click="quickSet(c.mine, c.theirs); menu = false;"
                >{{c.title}}</v-chip>
              </v-chip-group>
              <div v-else style="height: 24px;"></div>
              <div style="display:flex; flex-direction:row;">
                <div style="padding: 0px 4px;">
                  <v-text-field
                    v-model="hour"
                    label="Hour"
                    outlined
                    rounded
                    type="number"
                    autofocus
                    :rules="[defaultrules.required(hour, 'Hour'), defaultrules.isValidHour(hour)]"
                    @blur="validateHour(hour)"
                  ></v-text-field>
                </div>
                <div>
                  <div style="font-size: 34px; margin-top: -5px;">:</div>
                </div>
                <div style="padding: 0px 4px;">
                  <v-text-field
                    v-model="minute"
                    label="Minute"
                    outlined
                    rounded
                    type="number"
                    :rules="[defaultrules.required(minute, 'Minute'), defaultrules.isValidMinute(minute)]"
                    @blur="validateMinute(minute)"
                  ></v-text-field>
                </div>
                <div style="padding: 0px 4px;">
                  <div class='btn-time-wrapper'>
                    <div :class='amClass' v-on:click="ap = 'AM'">
                      AM
                    </div>
                    <div :class='pmClass' v-on:click="ap = 'PM'">
                      PM
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <div class="d-flex pull-right">
              <v-btn color="accent" @click="menu = false">OK</v-btn>
            </div>
          </v-sheet>
        </v-menu>
      </v-form>
      `,
    props: ["value", "default", "rules", "dates", "label", "quickSetItems"],
    data() {
      return {
        hour: null,
        minute: "00",
        ap: "AM",
        menu: false,
        originalValue: "",
        valid: true,
        defaultrules: {
          required(val, field) {
            return !!val || `${field} is required`;
          },
          isValidHour(val) {
            return val > 0 && val < 25 || 'Invalid input for hour'
          },
          isValidMinute(val) {
            return val >= 0 && val < 60 || 'Invalid input for minute'
          }
        },
        onlySundays: false
      }
    },
    created() {
      this.originalValue = this.value;
      if (this.value) {
        this.hour = this.value.split(":")[0];
        this.minute = this.value.split(":")[1].split(" ")[0];
        this.ap = this.value.split(" ")[1];
      } else if (this.default) {
        this.hour = this.default.split(":")[0];
        this.minute = this.default.split(":")[1].split(" ")[0];
        this.ap = this.default.split(" ")[1];
      } else {
        this.hour = null;
        this.minute = "00";
        this.ap = "AM";
      }
    },
    methods: {
      validateHour(val) {
        val = parseInt(val)
        if(!(val > 0 && val < 25)) {
          this.hour = null
          return
        }
        if(val > 12) {
          val = val - 12
          this.ap = "PM"
        }
        if(val.toString().length < 2) {
          val = "0" + val
        }
        this.hour = val
      },
      validateMinute(val) {
        val = parseInt(val)
        if(!(val >= 0 && val < 60)) {
          this.minute = null
          return
        }
        if(val.toString().length < 2) {
          val = "0" + val
        }
        this.minute = val
      },
      quickSet(start, end) {
        this.hour = start.split(":")[0]
        this.minute = start.split(":")[1].split(" ")[0]
        this.ap = start.split(" ")[1]
        this.$emit('quicksettime', end)
      }
    },
    computed: {
      time() {
        if (this.hour == null || this.minute == null) {
          return "";
        }
        return `${this.hour}:${this.minute} ${this.ap}`;
      },
      amClass() {
        let className = "btn-am"
        if(this.ap == "AM") {
          className += " active"
        }
        return className
      },
      pmClass() {
        let className = "btn-pm"
        if(this.ap == "PM") {
          className += " active"
        }
        return className
      },
    },
    watch: {
      time(val) {
        this.$emit("input", val);
      },
      default(val) {
        if (!this.originalValue && (!this.value || this.value.includes("null"))) {
          this.hour = val.split(":")[0];
          this.minute = val.split(":")[1].split(" ")[0];
          this.ap = val.split(" ")[1];
        }
      },
      value(val) {
        if (val) {
          this.hour = val.split(":")[0];
          this.minute = val.split(":")[1].split(" ")[0];
          this.ap = val.split(" ")[1];
        }
      },
      minute(val) {
        if(val && val.toString().includes('.')){
          this.minute = val.replace('.','')
        }
      },
      hour(val) {
        if(val && val.toString().includes('.')){
          this.hour = val.replace('.','')
        }
      },
      dates: {
        handler(val) {
          if(val && val.length > 0) {
            let allSundays = true
            val.forEach(d => {
              let x = moment(d).day()
              if(moment(d).day() != 0) {
                allSundays = false
              }
            })
            this.onlySundays = allSundays
          } else {
            this.onlySundays = false
          }
        },
        deep: true
      }
    },
};
