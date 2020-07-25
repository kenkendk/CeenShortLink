<template>
  <el-dialog title="Link entry" :visible.sync="dialogVisible">
    <el-form ref="form" :model="item" label-width="120px">
      <el-form-item label="Match">
        <el-input v-model="item.match" />
      </el-form-item>
      <el-form-item label="Target">
        <el-input v-model="item.targetUrl" />
      </el-form-item>
      <el-form-item label="Displayname">
        <el-input v-model="item.name" />
      </el-form-item>
      <el-form-item label="Valid From">
        <el-date-picker v-model="item.validFrom" type="datetime" placeholder="Pick a date" style="width: 100%;" />
      </el-form-item>

      <el-form-item label="Expires">
        <el-date-picker v-model="item.expires" type="datetime" placeholder="Pick a date" style="width: 100%;" />
      </el-form-item>

      <el-form-item label="Enabled">
        <el-switch v-model="item.enabled" />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="onSubmit">{{ item.id ? 'Update' : 'Create' }}</el-button>
        <el-button @click="onCancel">Cancel</el-button>
      </el-form-item>
    </el-form>
  </el-dialog>
</template>

<script>
import { addLink, updateLink } from '@/api/links'

export default {
  name: 'LinkDetails',
  data() {
    return {
      dialogVisible: false,
      listLoading: false,
      item: {},
      total: 0,
      now: null,
      updateCallback: null
    }
  },
  methods: {
    showEntry(item, updateCallback) {
      this.dialogVisible = true
      this.updateCallback = updateCallback
      this.item = item
    },
    onCancel() {
      this.dialogVisible = false
    },
    onSubmit() {
      if (this.item.id) {
        updateLink(this.item)
          .then(this.afterUpddate)
          .catch(x => {})
      } else {
        addLink(this.item)
          .then(this.afterUpddate)
          .catch(x => {})
      }
      console.log('submit!')
    },
    afterUpddate() {
      this.dialogVisible = false
      if (this.updateCallback != null) {
        this.updateCallback()
      }
    }
  }
}
</script>
