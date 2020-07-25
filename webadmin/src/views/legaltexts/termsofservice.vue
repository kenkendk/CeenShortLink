<template>
  <div class="components-container">
    <markdown-text textid="termsofservice" :use-subject="false" title="Terms of service" />
  </div>
</template>

<script>
import MarkdownText from '@/components/MarkdownText'
export default {
  components: { MarkdownText },
  data() { return {} }
}
</script>
