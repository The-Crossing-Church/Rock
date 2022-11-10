import { defineComponent, PropType } from "vue";
import { Input, Menu, Dropdown } from "ant-design-vue";
import TextBox from "../../../../Elements/textBox";
import RockLabel from "../../../../Elements/rockLabel";

const { MenuItem } = Menu;

type ListItem = {
  text: string,
  description: string,
  value: string
}

export default defineComponent({
    name: "EventForm.Components.PublicityDropDown",
    components: {
      "a-dropdown": Dropdown,
      "a-text": Input,
      "a-menu": Menu,
      "a-menu-item": MenuItem,
      "rck-text": TextBox,
      "rck-lbl": RockLabel,      
    },
    props: {
        modelValue: String,
        disabled: {
            type: Boolean,
            required: false
        },
        label: String,
        items: {
            type: Array as PropType<ListItem[]>,
            required: true
        },
    },
    setup() {

    },
    data() {
        return {
          selectedValue: {} as ListItem,
          menuOpen: false
        };
    },
    computed: {
      formattedItems() {
        return this.items.map((i: ListItem) => {
          let li = {} as ListItem
          li.value = i.value
          li.text = i.text.split(":")[0]
          li.description = i.text.split(":")[1].trim()
          return li
        })
      }
    },
    methods: {
      select(item: ListItem) {
        this.selectedValue = item
        this.menuOpen = false
      },
      getClassName(item: ListItem) {
        let className = "tcc-dropdown-item"
        if(this.selectedValue.value == item.value) {
          className += " active"
        }
        return className
      },
      menuChange(visible: boolean) {
        this.menuOpen = visible
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
      }
    },
    mounted() {
      if (this.modelValue) {
          let selected = this.formattedItems.filter((i: ListItem) => { return i.value == this.modelValue })
          if(selected.length > 0) {
            this.selectedValue = selected[0]
          }
      }
      let els = document.querySelectorAll(".tcc-text-display")
      els.forEach((el: any) => {
        el.setAttribute("readonly", "")
      })
    },
    template: `
<div style="width: 100%;">
  <a-dropdown :trigger="['click']" v-on:visibleChange="menuChange" v-model="menuOpen">
    <div>
      <rck-lbl>{{label}}</rck-lbl>
      <rck-text
        v-model="selectedValue.text"
        inputClasses="tcc-text-display"
      ></rck-text>
    </div>
    <template #overlay>
      <a-menu class="tcc-dropdown">
        <a-menu-item :class="getClassName(i)" v-for="i in formattedItems" :key="i.value" @click="select(i)">
          <div class="tcc-dropdown-item-content">
            {{i.text}}
            <div class="tcc-dropdown-item-description">
              {{i.description}}
            </div>
          </div>
        </a-menu-item>
      </a-menu>
    </template>
  </a-dropdown>
</div>
<v-style>
  .tcc-dropdown-item {
    padding: 4px;
    cursor: pointer;
    display: flex;
    align-items: center;
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
  .tcc-dropdown-item.disabled {
    color: rgba(0,0,0,.26)!important;
    cursor: not-allowed;
  }
  .tcc-dropdown-item.active {
    color: #347689;
    background-color: #E8EEF1;
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
