<template>
  <div class="components-container">
    <h2>Action when requesting the front page</h2>
    <div>
      <el-radio v-model="item.mode" :label="0" border>Show markdown text</el-radio>
      <el-radio v-model="item.mode" :label="1" border>Redirect</el-radio>
    </div>

    <markdown-text v-show="item.mode == 0" ref="mdtext" textid="landingpage" :use-subject="false" :show-save-buttons="false" title="Text on the start page" />

    <el-form v-if="item.mode == 1" ref="form-redirect" :model="item" label-width="160px">
      <h2>Redirect options</h2>
      <el-form-item label="Url to redirect to">
        <el-input v-model="item.redirectUrl" />
      </el-form-item>
      <el-form-item label="Use internal redirect">
        <el-switch v-model="item.internalRedirect" />
      </el-form-item>
    </el-form>

    <el-button class="save-button" :disabled="loading" type="primary" @click.prevent="saveContents">
      <span>Save changes</span>
      <i v-if="working" class="el-icon-loading" />
    </el-button>
    <el-button class="save-button" :disabled="loading" type="warning" @click.prevent="resetContents">
      <span>Reset changes</span>
    </el-button>

  </div>
</template>

<script>
import MarkdownText from '@/components/MarkdownText'
import { getSettings, updateSettings } from '@/api/frontpage'

export default {
  components: { MarkdownText },
  data() {
    return {
      loading: true,
      working: false,
      item: { }
    }
  },
  mounted() {
    this.loading = true
    this.resetSettings()
      .finally(x => { this.loading = false })
  },
  methods: {
    saveContents() {
      this.working = true
      this.$refs['mdtext'].saveContents()
        .then(y => {
          updateSettings(this.item)
        })
        .finally(x => { this.working = false })
    },
    resetSettings() {
      return getSettings()
        .then(x => { this.item = x })
    },
    resetContents() {
      this.loading = true
      this.$refs['mdtext'].resetContents()
        .then(y => {
          this.resetSettings()
        })
        .finally(x => { this.loading = false })
    }
  }
}
</script>

<style scoped>
.delete-helper {
  display: inline-block;
  position: relative;
}

.item:hover > .delete-helper > i {
  display: inline-block;
  opacity: 0.7;
  color: darkred;
}

.delete-helper > i {
  transition: opacity .5s;
  opacity: 0;
  display: none;
  top: 10px;
  right: 20px;
  position: absolute;
}

.preview-image {
  width: 200px;
  height: 100px;
  margin-right: 10px;
}
.image-uploader {
  display: inline-block;
}

.image-uploader >>> .el-upload-dragger {
  height: 100px;
  width: 200px;
}

.image-uploader >>> .el-upload-dragger .el-icon-upload {
  margin: 10px 0 16px;
}

</style>

