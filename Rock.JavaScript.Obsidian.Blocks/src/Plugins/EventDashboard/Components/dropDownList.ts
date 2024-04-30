import { defineComponent, PropType } from "vue";
import { Menu, Dropdown } from "ant-design-vue";
import TextBox from "@Obsidian/Controls/textBox";
import RockLabel from "@Obsidian/Controls/rockLabel";

const { MenuItem } = Menu;

export default defineComponent({
  name: "EventDashboard.Components.Filters.RequestStatus",
  components: {
    "a-dropdown": Dropdown,
    "a-menu": Menu,
    "a-menu-item": MenuItem,
    "rck-text": TextBox,
    "rck-lbl": RockLabel
  },
  props: {
    modelValue: Array as PropType<string[]>,
    label: {
      type: String,
      required: false
    },
    items: {
      type: Array as PropType<string[]>,
      required: true
    },
  },
  setup() {

  },
  data() {
    return {
      selectedValue: [] as string[],
      search: '',
      menuOpen: false
    };
  },
  computed: {
    displayValue() {
      if(this.selectedValue && this.selectedValue.length > 0) {
        return this.selectedValue.join(", ")
      }
      return ""
    },
    filteredItems() {
      if (this.search) {
        return this.items.filter(i => {
          if(i) {
            return i.toLowerCase().includes(this.search.toLowerCase())
          }
        })
      }
      return this.items
    }
  },
  methods: {
    select(item: string) {
      if(this.selectedValue.includes(item)) {
        let idx = this.selectedValue.indexOf(item)
        this.selectedValue.splice(idx, 1)
      } else {
        this.selectedValue.push(item)
      }
    },
    menuChange(visible: boolean) {
      this.menuOpen = visible
      if(!this.menuOpen) {
        this.search = ''
      }
    },
    getClassName(item: string) {
      let className = "tcc-dropdown-item"
      if(this.selectedValue.includes(item)) {
        className += " active"
      }
      return className
    }
  },
  watch: {
    selectedValue: { 
      handler (val) {
        if (val) {
          this.$emit('update:modelValue', val)
        }
      },
      deep: true
    },
    modelValue(val) {
      this.selectedValue = val
    }
  },
  mounted() {
    if (this.modelValue) {
      this.selectedValue = this.modelValue
    }
    let els = document.querySelectorAll(".tcc-text-display")
    els.forEach((el: any) => {
      el.setAttribute("readonly", "")
    })
  },
  template: `
<div>
  <a-dropdown :trigger="['click']" v-on:visibleChange="menuChange">
    <div>
      <rck-lbl>{{label}}</rck-lbl>
      <rck-text
        v-model="displayValue"
        inputClasses="tcc-text-display"
      ></rck-text>
    </div>
    <template #overlay>
      <a-menu class="tcc-dropdown">
        <div class="tcc-menu-header">
          <div class="tcc-menu-search">
            <rck-text
              v-model="search"
              placeholder="Type to filter..."
            ></rck-text>
            <i class="fa fa-search"></i>
          </div>
        </div>
        <a-menu-item :class="getClassName(i)" v-for="i in filteredItems" @click="select(i)" :key="i">
            <div class="tcc-dropdown-item-content">
              {{i}}
            </div>
        </a-menu-item>
      </a-menu>
    </template>
  </a-dropdown>
</div> 
<v-style>
  .tcc-dropdown .tcc-menu-header {
    position: sticky;
    top: 0;
    z-index: 10;
    margin: -4px;
    margin-bottom: 4px;
    background-color: #fff;
  }
  .tcc-menu-search {
    display: flex;
    align-items: center;
  }
  .tcc-menu-search .control-wrapper {
    width: 95%;
  }
  .tcc-dropdown-item {
    padding: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
  }
  .tcc-dropdown-header {
    color: #347689;
    font-weight: bold;
    font-size: 1.2em;
    line-height: 1.2;
  }
  .tcc-dropdown-item-content {
    font-weight: 500;
    font-size: 16px;
  }
  .tcc-dropdown-item-description {
    font-size: .875rem;
    line-height: 1.2;
    font-weight: normal;
  }
  .tcc-dropdown-item:hover {
    background-color: #EEEFEF;
  }
  .tcc-dropdown-item.disabled, .tcc-dropdown-item.disabled .tcc-checkbox .far, .tcc-dropdown-item.disabled .tcc-checkbox .fa {
    color: rgba(0,0,0,.26)!important;
    cursor: not-allowed;
  }
  .tcc-dropdown-item.active {
    color: #347689;
    background-color: #E8EEF1;
  }
  .tcc-dropdown-item-action {
    min-width: 30px;
  }
  .tcc-checkbox {
    padding-left: 6px;
  }
  .fa-square, .fa-check-square {
    font-size: 24px;
    color: #347689;
  }
  /* Scrollbar */
  ul.tcc-dropdown::-webkit-scrollbar {
    width: 5px;
    border-radius: 3px;
  }
  ul.tcc-dropdown::-webkit-scrollbar-track {
    background: #bfbfbf;
    -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.1);
  }
  ul.tcc-dropdown::-webkit-scrollbar-thumb {
    background: rgb(224, 224, 224);
    -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.2);
  }
  ul.tcc-dropdown::-webkit-scrollbar-thumb:hover {
    background: #AAA;
  }
  ul.tcc-dropdown::-webkit-scrollbar-thumb:active {
    background: #888;
    -webkit-box-shadow: inset 1px 1px 2px rgba(0,0,0,0.3);
  }
</v-style>
`
});