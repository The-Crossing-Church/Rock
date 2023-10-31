import { defineComponent, PropType } from "vue";
import { Input, Menu, Dropdown } from "ant-design-vue";
import TextBox from "../../../../Elements/textBox";
import RockLabel from "../../../../Elements/rockLabel";

const { MenuItem } = Menu;

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
    "a-dropdown": Dropdown,
    "a-text": Input,
    "a-menu": Menu,
    "a-menu-item": MenuItem,
    "rck-text": TextBox,
    "rck-lbl": RockLabel
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
  },
  setup() {
  },
  data() {
    return {
      selectedValue: "",
      selectedDisplayText: "",
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
      select(item: ListItem) {
        if(!item.isDisabled) {
          if(!item.isHeader) {
            this.selectedValue = item.value
            this.selectedDisplayText = item.text
            this.menuOpen = false
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
          this.$emit('update:modelValue', val)
          let itm = this.items.filter((i: any) => {
            return i.value == val
          })
          if(itm && itm.length > 0) {
            this.selectedDisplayText = itm[0].text
          }
        }
      },
      deep: true
    },
    modelValue: {
      handler(val) {
        if (val) {
          this.selectedValue = val
          let itm = this.items.filter((i: any) => {
            return i.value == val
          })
          if(itm && itm.length > 0) {
            this.selectedDisplayText = itm[0].text
          }
        }
      },
      deep: true
    }
  },
  mounted() {
    if (this.modelValue) {
      this.selectedValue = this.modelValue
      let itm = this.items.filter((i: any) => {
        return i.value == this.modelValue
      })
      if(itm && itm.length > 0) {
        this.selectedDisplayText = itm[0].text
      }
    }
    let els = document.querySelectorAll(".tcc-text-display")
    els.forEach((el: any) => {
      el.setAttribute("readonly", "")
    })
  },
  template: `
<div>
  <a-dropdown :trigger="['click']" v-on:visibleChange="menuChange" v-model:visible="menuOpen">
    <div>
      <rck-lbl>{{label}}</rck-lbl>
      <rck-text
        v-model="selectedDisplayText"
        inputClasses="tcc-text-display"
      ></rck-text>
    </div>
    <template #overlay v-model:visible="menuOpen">
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
        <a-menu-item :class="getClassName(i)" v-for="i in filteredItems" :key="i.value" @click="select(i)">
          <template v-if="i.isHeader">
            <div class="tcc-dropdown-header">{{i.value}}</div>
          </template>
          <template v-else>
            <div class="tcc-dropdown-item-content">
              {{i.text}}
              <div class="tcc-dropdown-item-description">
                {{i.description}}
              </div>
            </div>
          </template>
        </a-menu-item>
      </a-menu>
    </template>
  </a-dropdown>
</div>
<v-style>
  .tcc-menu-header {
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
  .tcc-menu-banner {
    font-weight: 500;
    background-color: #347689 !important;
    border-color: #347689 !important;
    color: #fff;
    padding: 4px 12px;
    font-size: 16px;
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
