import { defineComponent, PropType } from "vue";
import TextBox from "../../../../Elements/textBox";
import RockLabel from "../../../../Elements/rockLabel";

type RoomSetUp = {
    Room: string,
    TypeofTable: string,
    NumberofTables: number,
    NumberofChairs: number
}

export default defineComponent({
    name: "EventForm.Components.RoomSetUp",
    components: {
        "rck-text": TextBox,
        "rck-lbl": RockLabel,
    },
    props: {
        modelValue: Object as PropType<RoomSetUp>,
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
            roomSetUp: {} as RoomSetUp
        };
    },
    computed: {
    },
    methods: {

    },
    watch: {
        roomSetUp(val) {
            if (val) {
                this.$emit('update:modelValue', this.roomSetUp)
            } else {
                this.$emit('update:modelValue', "{}")
            }
        }
    },
    mounted() {
        if(this.modelValue) {
            this.roomSetUp = this.modelValue
        }
    },
    template: `
<div class="row">
  <div class="col col-xs-4">
    <rck-lbl>Type of Table</rck-lbl>
    <div>
      {{roomSetUp.TypeofTable}}
    </div>
  </div>
  <div class="col col-xs-4">
    <rck-lbl>Number of Tables</rck-lbl>
    <rck-text
        v-model="roomSetUp.NumberofTables"
    ></rck-text>
  </div>
  <div class="col col-xs-4">
    <rck-lbl>Number of Chairs</rck-lbl>
    <rck-text
        v-model="roomSetUp.NumberofChairs"
    ></rck-text>
  </div>
</div>
`
});
