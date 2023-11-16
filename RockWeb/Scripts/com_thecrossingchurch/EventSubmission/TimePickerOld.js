export default {
    template: `
        <v-form ref="timeForm" v-model="valid">
            <v-row>
                <v-col>
                    <v-select label="Hour" :items="hours" v-model="hour" attach :rules="rules" clearable></v-select>
                </v-col>
                <v-col>
                    <v-select label="Minute" :items="mins" v-model="minute" attach :rules="[required(minute)]" clearable></v-select>
                </v-col>
                <v-col>
                    <v-radio-group v-model="ap" row :rules="[required(ap)]">
                        <v-radio v-for="i in aps" :key="i" :label="i" :value="i"></v-radio>
                    </v-radio-group>
                </v-col>
            </v-row>
        </v-form>
        `,
    props: ["value", "default", "rules"],
    data: function () {
        return {
            hour: null,
            minute: "00",
            ap: null,
            hours: [
                "01",
                "02",
                "03",
                "04",
                "05",
                "06",
                "07",
                "08",
                "09",
                "10",
                "11",
                "12",
            ],
            mins: ["00", "15", "30", "45"],
            aps: ["AM", "PM"],
            originalValue: "",
            valid: true,
            required: function(val) {
                if(!!val) {
                    return true
                }
                return false
            }
        };
    },
    created: function () {
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
            this.hour = null
            this.minute = "00"
            this.ap = null
        }
    },
    computed: {
        time() {
            if (`${this.hour}:${this.minute} ${this.ap}` == 'null:null null') {
                return ''
            }
            return `${this.hour}:${this.minute} ${this.ap}`;
        },
    },
    watch: {
        time(val) {
            this.$emit("input", val);
        },
        default(val) {
            if (!this.originalValue && (!this.value || this.value.includes('null'))) {
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
    },
}
