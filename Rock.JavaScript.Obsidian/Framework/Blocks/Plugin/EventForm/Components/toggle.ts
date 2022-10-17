import { defineComponent } from "vue";
import { Switch } from "ant-design-vue";


export default defineComponent({
    name: "EventForm.Components.Toggle",
    components: {
        "a-switch": Switch
    },
    props: {
        modelValue: String,
        label: {
            type: String,
            required: false
        },
        disabled: {
            type: Boolean,
            required: false
        },
        hint: {
            type: String,
            required: false
        },
        persistentHint: {
            type: Boolean,
            required: false
        },
    },
    setup() {

    },
    data() {
        return {
            valueAsBool: false
        };
    },
    computed: {

    },
    methods: {
        toggleValue() {
            if(!this.disabled) {
                this.valueAsBool = !this.valueAsBool
            }
        }
    },
    watch: {
        valueAsBool(val) {
            if (val) {
                this.$emit('update:modelValue', "True")
            } else {
                this.$emit('update:modelValue', "False")
            }
        }
    },
    mounted() {
        if (this.modelValue?.toLowerCase() == "false" || this.modelValue?.toLowerCase() == "no") {
            this.valueAsBool = false
        } else {
            this.valueAsBool = true
        }
    },
    template: `
<div style="padding-bottom: 8px;">
  <div style="display: flex;">
    <a-switch
        v-model:checked="valueAsBool"
        :disabled="disabled"
    ></a-switch>
    <label class="switch-label" @click="toggleValue">{{label}}</label>
  </div>
  <div class="switch-hint input-hint" v-if="hint && (persistentHint || valueAsBool)">{{hint}}</div>
</div>
<v-style>
  .switch-label {
    padding-left: 8px;
  }
</v-style>
`
});
