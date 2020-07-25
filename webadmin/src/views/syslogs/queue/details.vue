<template>
  <el-dialog title="Kørsler i køen" :visible.sync="dialogVisible">
    <el-table v-loading="listLoading" :data="list">
      <el-table-column property="result" type="expand">
        <template slot-scope="scope">
          <p><b>HTTP method</b>: {{ scope.row.method }}</p>
          <p><b>Type</b>: {{ scope.row.contentType }}</p>
          <p v-if="scope.row.statusCode > 0"><b>Statuscode</b>: {{ scope.row.statusCode }}</p>
          <p v-if="scope.row.statusCode > 0"><b>Statusmessage</b>: {{ scope.row.statusMessage }}</p>
          <p><b>Result</b>:</p>
          <span style="white-space: pre-wrap">{{ scope.row.result }}</span>
        </template>
      </el-table-column>
      <el-table-column property="started" label="Start" width="150">
        <template slot-scope="scope">
          <span>{{ scope.row.started | parseTime('{y}-{m}-{d} {h}:{i}') }}</span>
        </template>
      </el-table-column>
      <el-table-column property="finished" label="Duration" width="100">
        <template slot-scope="scope">
          <span>{{ computeDuration(scope.row) }}</span>
        </template>
      </el-table-column>
      <el-table-column property="statusCode" label="Status" min-width="150">
        <template slot-scope="scope">
          <span v-if="scope.row.statusCode > 0">{{ scope.row.statusCode }} - {{ scope.row.statusMessage }}</span>
        </template>
      </el-table-column>
    </el-table>
  </el-dialog>
</template>

<script>
import { listAttempts } from '@/api/queues'
import { computeDuration } from '@/utils'

var timerid = null

export default {
  name: 'QueueDetails',
  data() {
    return {
      dialogVisible: false,
      listLoading: false,
      entryid: null,
      list: [],
      total: 0,
      now: null
    }
  },
  created() {
    var self = this
    timerid = setInterval(function() {
      self.now = Date.now()
    }, 1000)
  },
  destroyed() {
    if (timerid != null) {
      clearInterval(timerid)
    }
  },
  methods: {
    showEntry(queue, id) {
      this.listLoading = true
      this.dialogVisible = true
      listAttempts(queue, id)
        .then(response => {
          this.list = response.result
          this.total = response.total
        }).finally(x => {
          this.listLoading = false
        })
    },
    computeDuration(row) {
      return computeDuration(row.started, row.finished)
    }
  }
}
</script>
