import { defineComponent, provide } from "vue"
import { useConfigurationValues, useInvokeBlockAction } from "@Obsidian/Utility/block"
import { useStore } from "@Obsidian/PageState"
import { DateTime, Duration } from "luxon"
import { HubSpotBlogSearchBlockViewModel } from "./hubSpotBlogSearchViewModel"

const store = useStore()
export default defineComponent({
  name: "CMS.HubSpotBlogSearch",
  setup() {
      const invokeBlockAction = useInvokeBlockAction();
      const viewModel = useConfigurationValues<HubSpotBlogSearchBlockViewModel | null>();

      /** A method to load a specific request's details */
      const searchBlog: (q: string | null | undefined, tags: string[] | null | undefined) => Promise<any> = async (q, tags) => {
          const response = await invokeBlockAction("GetSearchResults", {
              q: q, tags: tags
          });
          return response
      };
      provide("searchBlog", searchBlog);
      return {
          viewModel,
          searchBlog
      }
  },
  data() {
    return {
      results: [],
      query: ""
    }
  },
  computed: {

  },
  methods: {
    getSearchQuery() {
      let search =  new URLSearchParams(window.location.search)
      let current = search.get('q')
      if(current && current != this.query) {
        this.query = current
        this.search()
      }
    },
    search() {
      this.searchBlog(this.query, []).then((res) => {
        console.log(res)
        if(res.data?.items?.length > 0) {
          let el = document.querySelector('#blog-results')
          if(el) {
            el.innerHTML = res.data.result
          }
        }
      })
    }
  },
  mounted() {
    window.addEventListener("keyup", (event) => {
      let target = event.target as any
      let el = document.querySelector('.collectionsearch input[id^="rock-textbox"]')
      if(target?.id == el?.id) {
        setTimeout(() => {
          this.getSearchQuery()
        }, 1000)
      }
    })
    this.getSearchQuery()
  },
  watch: {
    
  },
  template: `
  <div id="blog-results"></div>
  `
})