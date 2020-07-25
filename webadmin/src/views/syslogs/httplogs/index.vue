<template>
  <div class="app-container">
    <div class="filter-container">
      <el-input v-model="filterParams.name" placeholder="Write a filter expression" clearable style="width: 300px;" class="filter-item" @keyup.enter.native="handleFilter" />

      <el-button v-waves class="filter-item" type="primary" icon="el-icon-search" @click="handleFilter">
        Search
      </el-button>
      <el-button v-waves class="filter-item" type="primary" icon="el-icon-refresh" @click="handleFilter">
        Update
      </el-button>
    </div>

    <div>
      <el-table
        :key="tableKey"
        v-loading="listLoading"
        :data="list"
        border
        fit
        highlight-current-row
        style="width: 100%;"
        @sort-change="sortChange"
        @expand-change="expandChange"
      >
        <el-table-column type="expand" width="60">
          <template slot-scope="scope">
            <p><b>ID</b>: {{ scope.row.id }}</p>
            <p v-if="scope.row.connectionID != null"><b>ConnectionID</b>: {{ scope.row.connectionID }}</p>
            <p v-if="scope.row.userID != null"><b>UserID</b>: {{ scope.row.userID }}</p>
            <p v-if="scope.row.sessionID != null"><b>SessionID</b>: {{ scope.row.sessionID }}</p>
            <p><b>User-Agent</b>: {{ scope.row.userAgent }}</p>
            <p v-if="scope.row.requestQueryString != null"><b>Querystring</b>: {{ scope.row.requestQueryString }}</p>
            <p v-if="scope.row.loglines == null || scope.row.loglines.length != 0"><b>Logmessages</b> <i v-if="scope.row.loglines == null" class="el-icon-loading" /></p>

            <el-table
              v-if="scope.row.loglines == null || scope.row.loglines.length != 0"
              :data="scope.row.loglines"
              border
              style="width: 100%"
            >
              <el-table-column type="expand" width="60">
                <template slot-scope="subscope">
                  <span style="white-space: pre-wrap">{{ subscope.row.exception }}</span>
                </template>
              </el-table-column>
              <el-table-column prop="when" label="When" width="180px">
                <template slot-scope="subscope">
                  <span>{{ subscope.row.when | parseTime('{y}-{m}-{d} {h}:{i}') }}</span>
                </template>
              </el-table-column>
              <el-table-column prop="logLevel" label="Type" width="100px">
                <template slot-scope="subscope">
                  <el-tag :type="subscope.row.logLevel | logLevelFilter">
                    {{ subscope.row.logLevel }}
                  </el-tag>
                </template>
              </el-table-column>
              <el-table-column prop="logLevel" label="Message" min-width="180px">
                <template slot-scope="subscope">
                  <span>{{ subscope.row.data }}</span>
                </template>
              </el-table-column>
            </el-table>

          </template>
        </el-table-column>
        <el-table-column label="When" prop="started" sortable="custom" width="150px" align="center">
          <template slot-scope="scope">
            <span>{{ scope.row.started | parseTime('{y}-{m}-{d} {h}:{i}') }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Result" prop="responseStatusCode" sortable="custom" width="200px" align="left">
          <template slot-scope="scope">
            <el-tag :type="scope.row.responseStatusCode | statusFilter">
              {{ scope.row.responseStatusCode }} - {{ scope.row.responseStatusMessage }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="Verb" prop="requestVerb" sortable="custom" width="100px">
          <template slot-scope="scope">
            <span>{{ scope.row.requestVerb }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Url" prop="requestUrl" sortable="custom" min-width="150px">
          <template slot-scope="scope">
            <span>{{ scope.row.requestUrl }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Size" prop="responseSize" sortable="custom" width="120px" align="right">
          <template slot-scope="scope">
            <span>{{ scope.row.responseSize }}</span>
          </template>
        </el-table-column>
      </el-table>

      <pagination v-show="total>0" :total="total" :page.sync="currentPage" :limit.sync="pageSize" @pagination="getList" />

    </div>
  </div>
</template>

<script>
import Pagination from '@/components/Pagination' // secondary package based on el-pagination
import waves from '@/directive/waves' // waves directive
import { listLogs, listLogLines } from '@/api/httplog'

export default {
  components: { Pagination },
  directives: { waves },
  filters: {
    statusFilter(status) {
      status = parseInt(status)
      if (status === 200) {
        return 'success'
      } else if (status <= 399) {
        return 'info'
      } else if (status <= 499) {
        return 'warning'
      } else {
        return 'danger'
      }
    },
    logLevelFilter(status) {
      const statusMap = {
        debug: 'info',
        information: 'primary',
        warning: 'warning',
        error: 'danger'
      }
      return statusMap[status]
    }
  },
  data() {
    return {
      tableKey: 0,
      list: null,
      total: 0,
      listLoading: true,
      currentPage: 1,
      pageSize: 20,
      sortOptions: [
        { key: '+id', label: 'ID' },
        { key: '+email', label: 'Email' },
        { key: '+name', label: 'Name' },
        { key: '+when', label: 'When' },
        { key: '+whenActivated', label: 'Activation time' },
        { key: '+lastAttempt', label: 'Last attempt' },
        { key: '+status', label: 'Status' },

        { key: '-id', label: 'ID' },
        { key: '-email', label: 'Email' },
        { key: '-name', label: 'Name' },
        { key: '-when', label: 'When' },
        { key: '-whenActivated', label: 'Activation time' },
        { key: '-lastAttempt', label: 'Last attempt' },
        { key: '-status', label: 'Status' }
      ],
      contentOptions: [
        { key: 'created', label: 'Created' },
        { key: 'readytoactivate', label: 'Ready to activate' },
        { key: 'activated', label: 'Activated' },
        { key: 'confirmed', label: 'Confirmed' },
        { key: 'failed', label: 'Error' }
      ],
      filterParams: {
        name: null,
        sortOrder: '-started',
        statusItems: ['readytoactivate', 'confirmed']
      }
    }
  },
  created() {
    this.getList()
  },
  methods: {
    getList() {
      this.listLoading = true
      listLogs((this.currentPage - 1) * this.pageSize, this.pageSize, this.filterParams.name, this.filterParams.sortOrder)
        .then(response => {
          this.list = response.result
          this.total = response.total
        })
        .finally(x => {
          this.listLoading = false
        })
    },
    handleFilter() {
      this.currentPage = 1
      this.getList()
    },
    sortChange(data) {
      const { prop, order } = data
      if (data == null || prop == null || prop === 'null') {
        this.filterParams.sortOrder = null
      } else {
        this.filterParams.sortOrder = (order === 'ascending' ? '+' : '-') + prop
      }
      this.getList()
    },
    expandChange(row, rows) {
      this.getLogLines(row)
    },
    getLogLines(row) {
      if (row.loglines == null) {
        listLogLines(row.id)
          .then(x => { row.loglines = x })
      }
    }
  }
}
</script>
