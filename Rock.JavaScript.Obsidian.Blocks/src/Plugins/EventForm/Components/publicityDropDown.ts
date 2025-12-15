import { defineComponent, PropType } from "vue"
import TextBox from "@Obsidian/Controls/textBox.obs"
import RockLabel from "@Obsidian/Controls/rockLabel.obs"
import DropDownMenu from "./dropDownMenu.obs"

type ListItem = {
  text: string,
  description: string,
  value: string
}

export default defineComponent({
    name: "EventForm.Components.PublicityDropDown",
    components: {
      "rck-text": TextBox,
      "rck-lbl": RockLabel,   
      "tcc-dd": DropDownMenu 
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
        id: String
    },
    setup() {

    },
    data() {
        return {
          selectedValue: {} as any,
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
<tcc-dd
  :items="formattedItems"
  anchorButtonCssClass="w-100 px-0 text-left tcc-dropdown"
>
  <template #dropdownRender="{ item }" >
    <div @click="selectedValue = item">
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
    :id="'txt' + id"
  ></rck-text>
</tcc-dd>
<v-style>
  .tcc-dropdown {
    overflow-y: hidden !important;
    color: var(--color-interface-strong) !important;
    text-decoration: none !important;
  }
</v-style>
`
});
