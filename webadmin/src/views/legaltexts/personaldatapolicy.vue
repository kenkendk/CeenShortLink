<template>
  <div class="components-container">
    <markdown-text textid="personaldatapolicy" :use-subject="false" title="Personal data policy" />
  </div>
</template>

<script>
import MarkdownText from '@/components/MarkdownText'
export default {
  components: { MarkdownText },
  data() { return {} }
}
</script>
