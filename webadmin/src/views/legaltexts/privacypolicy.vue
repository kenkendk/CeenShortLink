<template>
  <div class="components-container">
    <markdown-text textid="privacypolicy" :use-subject="false" title="Privacy policy" />
  </div>
</template>

<script>
import MarkdownText from '@/components/MarkdownText'
export default {
  components: { MarkdownText },
  data() { return {} }
}
</script>
