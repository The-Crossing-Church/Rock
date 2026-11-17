import { defineComponent, PropType } from "vue"
import DetailEdit from "./detailEdit"
import { AttributeBag } from "../../../ViewModels/attributeBag"

export default defineComponent({
  name: "EventDashboard.Components.SSP.EditModal",
  components: {
    "tcc-edit-detail": DetailEdit
  },
  props: {
    request: Object,
    rooms: Array,
    drinks: Array,
    inventory: Array,
    createdBy: Object,
    modifiedBy: Object,
    sections: String,
    requestValidation: Object,
    editCategories: Array,
    viewCategories: Array,
    regAttrs: Array as PropType<AttributeBag[]>,
    discountAttrs: Array as PropType<AttributeBag[]>,
  },
  setup() {

  },
  data() {
    return {
      
    };
  },
  created() {
    
  },
  computed: {
    requestAttrs() {
      let sections = [] as any
      let editCategorySet = new Set(this.editCategories)
      if(this.request?.attributeValues) {
        Object.keys(this.request.attributeValues).forEach(key => {
          let attrCategories = new Set(this.request?.attributes[key]?.categories.map(c => c.name))
          let intersection = attrCategories.intersection(editCategorySet)
          if(intersection.size > 0) {
            let section = sections.filter(s => s.cat == [...intersection][0])[0]
            if(!section) {
              sections.push({ cat: [...intersection][0], attrs: [] })
              section = sections[sections.length - 1]
            } 
            section.attrs.push(key)
          }
        })
      }
      return sections
    }
  },
  methods: {
    
  },
  watch: {
    
  },
  mounted() {
    
  },
  template: `
<div>
  <tcc-edit-detail
    v-for="e in request.childItems" 
    :key="e.idKey"
    :detail="e"
    :request="request"
    :editCategories="editCategories"
    :regAttrs="regAttrs"
    :discountAttrs="discountAttrs"
  ></tcc-edit-detail>
</div>
`
});
