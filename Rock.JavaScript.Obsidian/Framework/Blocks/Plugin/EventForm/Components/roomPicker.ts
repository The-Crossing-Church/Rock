import { defineComponent } from "vue";
import { Input, Menu, Dropdown } from "ant-design-vue";
import TextBox from "../../../../Elements/textBox";
import RockLabel from "../../../../Elements/rockLabel";
import Checkbox from "../../../../Elements/checkBox";

const { MenuItem } = Menu;

type ListItem = {
    text: string,
    value: string,
    description: string,
    isDisabled: boolean,
    isHeader: boolean,
    order: number
}

type SelectedListItem = {
  text: string,
  value: string,
  description: string
}

export default defineComponent({
    name: "EventForm.Components.RoomPicker",
    components: {
        "a-dropdown": Dropdown,
        "a-text": Input,
        "a-menu": Menu,
        "a-menu-item": MenuItem,
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
    setup() {
    },
    data() {
        return {
            selectedValue: {} as SelectedListItem,
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
            let selectedItems = this.items.filter((i: any) => {
              return this.selectedValue.value.split(",").includes(i.value)
            }).sort((a: any, b: any) => {
              if(a.order > b.order) {
                return 1
              } else if(a.order < b.order) {
                return -1
              }
              return 0
            })
            let orderedGuids = selectedItems.filter((i: any) => {
              return i.type == item.value
            }).map((i: any) => {
              return i.value
            })
            if(item.isHeader) {
              if (this.multiple) {
                //Filter to items in that category and then select or unselect all
                let itemsInCategory = this.items.filter((i: any) => {
                  return i.type == item.value && !i.isDisabled
                })
                let orderedGuidsInCategory = itemsInCategory.map((i: any) => {
                  return i.value
                })
                //Check if all items are selected
                if(orderedGuids.join(",") == orderedGuidsInCategory.join(",")){
                  //Need to unselect all
                  selectedItems = selectedItems.filter((i: any) => {
                    return !orderedGuids.includes(i.value)
                  })
                } else {
                  //Make sure all are selected
                  itemsInCategory.forEach((i: any) => {
                    let idx = -1
                    selectedItems.forEach((si: any, index: number) => {
                      if(si.value == i.value) {
                        idx = index
                      }
                    })
                    if(idx < 0) {
                      selectedItems.push(i)
                    }
                  })
                }
                selectedItems = selectedItems.sort((a: any, b: any) => {
                  if(a.order > b.order) {
                    return 1
                  } else if(a.order < b.order) {
                    return -1
                  }
                  return 0
                })
                this.selectedValue.text = selectedItems.map((i: any) => i.text).join(", ")
                this.selectedValue.value = selectedItems.map((i: any) => i.value).join(",")
              }
            } else {
              if (this.multiple) {
                let vals = this.selectedValue.value ? this.selectedValue.value.split(",") : []
                let display = this.selectedValue.text ? this.selectedValue.text.split(", ") : []
                let idx = vals.indexOf(item.value)
                if (idx < 0) {
                  //Add item
                  vals.push(item.value)
                  display.push(item.text.split(" (")[0])
                } else {
                  //Remove item
                  vals.splice(idx, 1)
                  display.splice(idx, 1)
                }
                this.selectedValue = { value: vals.join(","), text: display.join(", "), description: "" }
              } else {
                this.selectedValue = { value: item.value, text: item.text.split(" (")[0], description: "" }
              }
            }
          }
        },
        isChecked(value: string) {
          if(this.selectedValue.value.split(",").includes(value)) {
            return "checked"
          }
          return ""
        },
        getClassName(item: ListItem) {
          let className = "tcc-dropdown-item"
          if(item.isDisabled) {
            className += " disabled"
          }
          if(this.selectedValue.value.split(",").includes(item.value)) {
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
            let selectedGuids = val.value.split(",")
            let selectedRooms = this.items.filter((i: any) => {
              if(!i.isDisabled) {
                return selectedGuids.includes(i.value)
              }
            }).sort((i: any) => i.order)
            val.value = selectedRooms.map((i: any) => i.value).join(",")
            val.text = selectedRooms.map((i: any) => i.text.split(" (")[0]).join(", ")
            this.$emit('update:modelValue', JSON.stringify(val))
          }
        },
        deep: true
      }
    },
    mounted() {
        if (this.modelValue) {
            this.selectedValue = JSON.parse(this.modelValue)
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
        v-model="selectedValue.text"
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
          <div class="tcc-menu-banner">
            Room (Capacity)
          </div>
        </div>
        <a-menu-item :class="getClassName(i)" v-for="i in filteredItems" :key="i.value" @click="select(i)">
          <template v-if="i.isHeader">
            <div class="tcc-dropdown-header">{{i.value}}</div>
          </template>
          <template v-else>
            <div class="tcc-dropdown-item-action">
              <div class="tcc-checkbox">
                <i v-if="isChecked(i.value)" class="fa fa-check-square"></i>
                <i v-else class="far fa-square"></i>
              </div>
            </div>
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
