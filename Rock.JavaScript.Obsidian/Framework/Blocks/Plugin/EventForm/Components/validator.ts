import { defineComponent } from "vue";

export default defineComponent({
    name: "EventForm.Components.Validator",
    components: {
      
    },
    props: {
        rules: Array
    },
    setup() {

    },
    data() {
      return {
        needsValidation: false
      }
    },
    computed: {
      className() {
        if(this.errors.length > 0 && this.needsValidation) {
          return "validator text-red has-errors"
        }
        return "validator"
      },
      errors() {
        if(this.rules && this.rules.length > 0 && this.needsValidation) {
          let errs = this.rules.filter(r => typeof r === 'string');
          return errs
        }
        return []
      }
    },
    methods: {
      validate() {
        this.needsValidation = true
      }
    },
    watch: {
      errors: {
        handler(val, oval) {
          if(val && val.length > 0) {
            let err = this.$parent?.$data as any
            err.errors?.push(...val)
          }
        },
        deep: true
      }
    },
    mounted() {
      
    },
    template: `
<div :class="className">
  <slot />
  <div class="text-errors" v-if="needsValidation && errors && errors.length > 0">
    {{errors[0]}}
  </div>
</div>
`
});
