import { defineComponent } from "vue"
import Switch from "@Obsidian/Controls/switch.obs"


export default defineComponent({
    name: "EventForm.Components.Toggle",
    components: {
        "rck-switch": Switch
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
        id: String
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
        },
        modelValue(val) {
            if (val.toLowerCase() == "false" || val.toLowerCase() == "no") {
                this.valueAsBool = false
            } else {
                this.valueAsBool = true
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
    <rck-switch
        v-model="valueAsBool"
        :text="label"
        :disabled="disabled"
        :id="id"
    ></rck-switch>
  </div>
  <div class="switch-hint input-hint" v-if="hint && (persistentHint || valueAsBool)">{{hint}}</div>
</div>
<!--
<v-style>
    .custom-switch .custom-control-label::before {
        top: 0px;
        left: calc((36px + var(--spacing-xsmall)) * -1);
        width: 36px;
        height: 24px;
    }
    .custom-switch .custom-control-label::after {
        top: 2px;
        left: calc((36px + var(--spacing-xsmall)) * -1 + 2px);
        width: 20px;
        height: 20px;
    }
</v-style>
-->
`
});
