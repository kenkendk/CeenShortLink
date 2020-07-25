<template>
  <div>

    <div class="editor-container">
      <h2 class="tag-title">
        <span>{{ title }}</span>
        <i v-if="loading" class="el-icon-loading" />
      </h2>

      <el-form v-if="useSubject" class="form-container" label-position="left">
        <el-form-item style="margin-bottom: 20px; margin-top: 20px" label-width="70px" label="Emne: ">
          <el-input v-model="subject" :rows="1" :disabled="loading" type="textarea" class="article-textarea" autosize placeholder="Enter email subject" />
        </el-form-item>
      </el-form>
      <markdown-editor v-model="body" height="300px" :mode="'wysiwyg'" :disabled="loading" />
    </div>

    <el-button v-if="showSaveButtons" class="save-button" :disabled="!anyChanges || loading" type="primary" @click.prevent="saveContents">
      <span>Save changes</span>
      <i v-if="saving" class="el-icon-loading" />
    </el-button>
    <el-button v-if="showSaveButtons" class="save-button" :disabled="!anyChanges || loading" type="warning" @click.prevent="resetContents">
      <span>Reset changes</span>
    </el-button>

  </div>
</template>

<script>
import MarkdownEditor from '@/components/MarkdownEditor'
import { getText, updateText } from '@/api/texts'

export default {
  name: 'MarkdownText',
  components: { MarkdownEditor },
  props: {
    textid: {
      type: String,
      required: true
    },
    useSubject: {
      type: Boolean,
      required: true
    },
    showSaveButtons: {
      type: Boolean,
      required: false,
      default: true
    },
    title: {
      type: String,
      required: true
    }
  },
  data() {
    return {
      body: '',
      subject: '',
      loading: true,
      saving: false,
      anyChanges: false,
      lastChanged: null,
      lsCacheId: ''
    }
  },
  watch: {
    body() {
      console.log('body changed')

      this.anyChanges = true
      this.saveCache()
    },
    subject() {
      console.log('subject changed')

      this.anyChanges = true
      this.saveCache()
    }
  },
  created() {
    this.lsCacheId = 'saved-text:' + this.textid
  },
  mounted() {
    this.readFromCache()
    this.readFromRemote()
  },
  methods: {
    readFromRemote() {
      var clear = false
      return getText(this.textid)
        .then(x => {
          console.log('got server copy')
          // Overwrite the cached copy, if this changed on the server
          if (this.lastChanged == null || this.lastChanged < x.lastChanged) {
            console.log('setting server values')
            this.body = x.body
            this.subject = x.subject
            this.lastChanged = x.lastChanged
            clear = true
          }
          this.loading = false
        })
        .finally(x => {
          // Delay call the clearing
          if (clear) {
            this.anyChanges = false
            this.clearCache()
          }
        })
    },
    saveContents() {
      this.saving = true

      return updateText(this.textid, this.body, this.subject, this.lastChanged)
        .then(x => {
          this.lastChanged = x.lastChanged
          this.anyChanges = false
          this.clearCache()
        })
        .finally(x => { this.saving = false })
    },
    readFromCache() {
      if (localStorage.getItem(this.lsCacheId)) {
        console.log('reading from cache')
        try {
          var n = JSON.parse(localStorage.getItem(this.lsCacheId))
          this.subject = n.subject
          this.body = n.body
          this.lastChanged = n.lastChanged
          this.anyChanges = true
        } catch (e) {
          console.log('Failed to read local storage: ', e)
        }
      }
    },
    clearCache() {
      console.log('clear cache')
      localStorage.removeItem(this.lsCacheId)
    },
    saveCache() {
      console.log('save cache')
      localStorage.setItem(this.lsCacheId, JSON.stringify({
        subject: this.subject,
        body: this.body,
        lastChanged: this.lastChanged
      }))
    },
    resetContents() {
      this.lastChanged = null
      return this.readFromRemote()
    }
  }
}
</script>

<style scoped>
.editor-container, .save-button{
  margin-bottom: 30px;
}
.tag-title{
  margin-bottom: 5px;
}

</style>

<style>
label {
  text-align: left;
}

</style>
