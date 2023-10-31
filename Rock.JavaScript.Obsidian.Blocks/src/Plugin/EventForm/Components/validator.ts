import { defineComponent, ref, onBeforeUnmount } from "vue";
import { useFormState } from "../../../../Util/form";
import { newGuid } from "../../../../Util/guid";

export default defineComponent({
    name: "EventForm.Components.Validator",
    components: {
      
    },
    props: {
        rules: Array, 
        name: String
    },
    setup(props) {
      /** The reactive state of the form. */
      const formState = useFormState();

      /** The unique identifier used to identify this form field. */
      const uniqueId = `rock-${props.name}-${newGuid()}`;

      // If we are removed from the DOM completely, clear the error before we go.
      onBeforeUnmount(() => {
          formState?.setError(uniqueId, "", "");
      });

      return {
          formState,
          uniqueId,
      };
    },
    data() {
      return {
        needsValidation: false
      }
    },
    computed: {
      className() {
        if(this.errors.length > 0 && this.needsValidation) {
          return "validator text-red has-error"
        }
        return "validator"
      },
      errors() {
        if(this.rules && this.rules.length > 0) {
          let errs = this.rules.filter(r => typeof r === 'string') as string[]
          let name = this.name as string
          this.formState?.setError(this.uniqueId, name, errs[0]);
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
