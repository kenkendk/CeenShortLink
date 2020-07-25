<template>
  <el-dialog title="User entry" :visible.sync="dialogVisible">
    <el-form ref="form" :model="item" label-width="160px">
      <el-form-item label="Email">
        <el-input v-model="item.email" />
      </el-form-item>
      <el-form-item label="Name">
        <el-input v-model="item.name" />
      </el-form-item>
      <el-form-item label="Disabled">
        <el-switch v-model="item.disabled" />
      </el-form-item>
      <el-form-item label="Admin">
        <el-switch v-model="item.admin" />
      </el-form-item>
      <!-- <el-form-item label="Require 2FA">
        <el-switch v-model="item.require2FA" />
      </el-form-item> -->

      <el-form-item label="Password">
        <el-input v-model="item.password" placeholder="Type a new password" show-password />
      </el-form-item>
      <el-form-item label="Repeat Password">
        <el-input v-model="item.repeatPassword" placeholder="Repeat new password" show-password />
      </el-form-item>

      <el-form-item>
        <el-button type="primary" @click="onSubmit">{{ item.id ? 'Update' : 'Create' }}</el-button>
        <el-button @click="onCancel">Cancel</el-button>
      </el-form-item>
    </el-form>
  </el-dialog>
</template>

<script>
import { addUser, updateUser, changePassword } from '@/api/user'

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
        updateUser(this.item)
          .then(this.afterUserUpdate)
          .catch(x => {})
      } else {
        addUser(this.item)
          .then(this.afterUpddate)
          .catch(x => {})
      }
      console.log('submit!')
    },
    afterUserUpdate() {
      if (this.item.password != null) {
        if (this.updateCallback != null) {
          this.updateCallback()
        }
        changePassword(this.item.id, 'unused-for-admin', this.item.password, this.item.repeatPassword)
          .then(this.afterUpddate)
          .catch(x => {})
      } else {
        this.afterUpddate()
      }
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
