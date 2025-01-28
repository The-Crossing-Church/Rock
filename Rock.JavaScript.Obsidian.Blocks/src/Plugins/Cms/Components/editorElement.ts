import { defineComponent, provide } from "vue"
import { useStore } from "@Obsidian/PageState"
// import { VueDraggableNext } from "vue-draggable-next"

const store = useStore()
export default defineComponent({
  name: "editor-elements",
  props: {
    items: {
      required: true,
      type: Array
    }
  },
  components: {
    // "draggable": VueDraggableNext
  },
  data() {
    return {
      
    }
  },
  computed: {

  },
  methods: {
    editElement(element) {
      console.log(element)
      // this.$emit('editElement', element)
    }
  },
  mounted() {
    
  },
  watch: {
    
  },
  template: `
<!--  <draggable 
    class="dragArea" 
    :list="items"
    tag="div"
    group="html"
  >
    <div v-for="el in items" :key="el.name" :data-html="el.html" class="editor-elem" @click="editElement">
      {{el.innerHtml}}
      <editor-elements :items="el.items" />
    </div>  
  </draggable> -->
  `
})