import { defineComponent } from "vue";

export default defineComponent({
    name: "EventForm.Components.Chip",
    components: {
      
    },
    props: {
        modelValue: String,
        disabled: {
            type: Boolean,
            required: false
        },
        class: String,
        id: String
    },
    setup() {

    },
    data() {
        return {
          
        };
    },
    computed: {
      className() {
        return "tcc-chip " + this.class
      }
    },
    methods: {
      emitCloseClick() {
        this.$emit("chipdeleted")
      }
    },
    watch: {
      
    },
    mounted() {
      
    },
    template: `
<div :class="className" @click="$emit('click')" :id="id">
  <slot></slot>
  <i v-if="!disabled" class="fa fa-times-circle" style="padding-left: 6px; color: #676666;" @click="emitCloseClick"></i>
</div>
<v-style>
  .tcc-chip {
    background: #e0e0e0;
    border-radius: 16px;
    font-size: 14px;
    height: 32px;
    border-color: rgba(0,0,0,.12);
    color: rgba(0,0,0,.87);
    margin: 4px;
    display: flex;
    padding: 0 12px;
    align-items: center;
    justify-content: center;
    line-height: 20px;
    text-decoration: none;
    transition-duration: .28s;
    transition-property: box-shadow,opacity;
    transition-timing-function: cubic-bezier(.4,0,.2,1);
    white-space: nowrap;
  }
  .chip-group {
    display: flex;
    justify-content: flex-start;
    flex-wrap: wrap;
    align-content: flex-start;
  }
  .chip-hidden {
    display: none;
  }
</v-style>
`
});
