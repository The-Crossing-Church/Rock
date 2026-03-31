import { defineComponent, PropType } from "vue"
import TextBox from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import DropDownMenu from "./dropDownMenu.obs"

type ListItem = {
  text: string,
  value: string,
  description: string,
  isDisabled: boolean,
  isHeader: boolean,
  order: number
}

export default defineComponent({
  name: "EventForm.Components.OpsInventoryPicker",
  components: {
    "rck-text": TextBox,
    "rck-lbl": RockLabel,   
    "tcc-dd": DropDownMenu 
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
    items: {
      type: Array as PropType<any[]>,
      required: true
    },
    icon: {
      type: String,
      required: false
    },
    id: String
  },
  setup() {
  },
  data() {
    return {
      selectedValue: "",
      search: '',
      menuOpen: false
    };
  },
  computed: {
    filteredItems() {
      if (this.search) {
        return this.items.filter(i => {
          let item = i as ListItem
          if(item.isHeader) {
            return true
          }
          if(item.text) {
            return item.text.toLowerCase().includes(this.search.toLowerCase())
          }
        })
      }
      return this.items
    }
  },
  methods: {
      select(item: any) {
        if(!item.isDisabled) {
          if(!item.isHeader) {
            this.selectedValue = item
            this.$emit('select')
          } 
        }
      },
      getClassName(item: ListItem) {
        let className = "tcc-dropdown-item"
        if(item.isDisabled) {
          className += " disabled"
        }
        if(this.selectedValue == item.value) {
          className += " active"
        }
        return className
      },
      menuChange(visible: boolean) {
        this.menuOpen = visible
        if(!this.menuOpen) {
          this.search = ''
        }
      }
  },
  watch: {
    selectedValue: { 
      handler (val) {
        if (val) {
          this.$emit('update:modelValue', val.value)
        }
      },
      deep: true
    },
    modelValue: {
      handler(val) {
        if (val) {
          this.selectedValue = this.items.filter((i: any) => { return i.value == val })[0]
        }
      },
      deep: true
    }
  },
  mounted() {
    if (this.modelValue) {
      this.selectedValue = this.items.filter((i: any) => { return i.value == this.modelValue })[0]
    }
  },
  template: `
<tcc-dd
  :label="label"
  :items="filteredItems"
  anchorButtonCssClass="w-100 px-0 text-left tcc-dropdown"
  :id="id + '_ddl'"
>
  <template #dropdownRender="{ item }" >
    <div v-if="item.isHeader" class="text-primary text-bold">
      {{item.value}}
    </div>
    <div v-else @click="select(item)">
      {{item.text}}
      <div class="text-subscript">
        {{item.description}}
      </div>
    </div>
  </template>
  <rck-text
    :label="label"
    v-model="selectedValue.text"
    inputClasses="w-100"
    isReadOnly
    :id="id + '_txt'"
  ></rck-text>
</tcc-dd>
<v-style>
  .tcc-dropdown {
    overflow-y: hidden !important;
    color: var(--color-interface-strong) !important;
    text-decoration: none !important;
  }
  .tcc-dropdown + .dropdown-menu {
    max-height: 300px;
    width: 100%;
  }
  .tcc-dropdown + .dropdown-menu .text-subscript {
    font-size: .875rem;
    line-height: 1.2;
    font-weight: normal;
  }
  /* Scrollbar */
  .tcc-dropdown + ul::-webkit-scrollbar {
    width: 5px;
    border-radius: 3px;
  }
  .tcc-dropdown + ul::-webkit-scrollbar-track {
    background: var(--color-interface-softest);
    -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.1);
  }
  .tcc-dropdown + ul::-webkit-scrollbar-thumb {
    background: var(--color-interface-soft);
    -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.2);
  }
  .tcc-dropdown + ul::-webkit-scrollbar-thumb:hover {
    background: var(--color-interface-softer);
  }
  .tcc-dropdown + ul::-webkit-scrollbar-thumb:active {
    background: var(--color-interface-softer);
    -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.3);
  }
</v-style>
`
});
