import { defineComponent } from 'vue'
import { AutoComplete } from 'ant-design-vue'
import TextBox from "@Obsidian/Controls/textBox"
import RockLabel from "@Obsidian/Controls/rockLabel"
import Checkbox from "@Obsidian/Controls/checkBox"
export default defineComponent({
  components: {
    "a-auto-complete": AutoComplete,
    "rck-text": TextBox,
    "rck-lbl": RockLabel,
    "rck-check": Checkbox
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
    multiple: {
        type: Boolean,
        required: false
    },
    items: {
        type: Array,
        required: true
    },
    icon: {
        type: String,
        required: false
    },
    checkBoxes: {
        type: Boolean,
        required: false
    }
  },
  data(){
    return {
      search: '',
      selectedValue: {}
    }
  },
  created(){
    
  },
  methods: {
    
  },
  computed: {
    dataSource() {
      let source = this.items.filter((i: any) => i.isHeader).map((i: any) => { return { value: i.value, isDisabled: i.isDisabled, options: this.items.filter((itm: any) => itm.type == i.value) } })
      return source
    }
  },
  watch: {
    
  },
  template: ` 
  <div>
    <rck-lbl>{{ label }}</rck-lbl> <br/>
    <a-auto-complete
      v-model:value="selectedValue"
      :options="items"
      style="with: 100%"
    >
      <template #option="item">
        <template v-if="item.isHeader">
          {{item.value}}
          <hr/>
        </template>
        <template v-else>
          {{item.text}}
        </template>
      </template>
    </a-auto-complete>
  </div>
`
})