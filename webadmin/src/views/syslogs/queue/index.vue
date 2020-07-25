<template>
  <div class="app-container">
    <h3>Kø: {{ queueid }}</h3>

    <div class="filter-container">
      <el-input v-model="filterParams.name" placeholder="Write a filter expression" clearable style="width: 300px;" class="filter-item" @keyup.enter.native="handleFilter" />

      <el-button v-waves class="filter-item" type="primary" icon="el-icon-search" @click="handleFilter">
        Search
      </el-button>
      <el-button v-waves class="filter-item" type="primary" icon="el-icon-refresh" @click="handleFilter">
        Update
      </el-button>
    </div>

    <el-table
      :key="tableKey"
      v-loading="listLoading"
      :data="list"
      row-key="id"
      border
      fit
      highlight-current-row
      style="width: 100%;"
    >
      <el-table-column label="ID" prop="name" align="center" width="60px">
        <template slot-scope="scope">
          <span>{{ scope.row.id }}</span>
        </template>
      </el-table-column>
      <el-table-column label="Method" prop="method" align="center" width="100px">
        <template slot-scope="scope">
          <span>{{ scope.row.method }}</span>
        </template>
      </el-table-column>
      <el-table-column label="Url" prop="url" min-width="200px" align="center">
        <template slot-scope="scope">
          <span>{{ scope.row.url }}</span>
        </template>
      </el-table-column>
      <el-table-column label="ETA" prop="eta" width="140px" align="center">
        <template slot-scope="scope">
          <span :alt="scope.row.eta | parseTime('{y}-{m}-{d} {h}:{i}')">{{ computeEta(scope.row.eta, now) }}</span>
        </template>
      </el-table-column>
      <el-table-column label="Next" prop="nextTry" width="140px" align="center">
        <template slot-scope="scope">
          <span :alt="scope.row.nextTry | parseTime('{y}-{m}-{d} {h}:{i}')">{{ computeEta(scope.row.nextTry, now) }}</span>
        </template>
      </el-table-column>
      <el-table-column label="Last" prop="lastTried" width="140px" align="center">
        <template slot-scope="scope">
          <span :alt="scope.row.lastTried | parseTime('{y}-{m}-{d} {h}:{i}')">{{ computeEta(scope.row.lastTried) }}</span>
        </template>
      </el-table-column>
      <el-table-column label="Error" prop="retries" width="60px" align="center">
        <template slot-scope="scope">
          <span>{{ scope.row.retries }}</span>
        </template>
      </el-table-column>
      <el-table-column label="Status" class-name="status-col" prop="rate" width="100">
        <template slot-scope="{row}">
          <el-tag :type="row.status | statusFilter">
            {{ row.status | statusTranslate }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="Actions" align="center" width="220" class-name="small-padding fixed-width">
        <template slot-scope="{row}">
          <el-button v-waves type="primary" size="small" tooltip="Show details" icon="el-icon-view" @click="showEntries(row)">
            Details
          </el-button>
          <el-button v-waves :disabled="row.status == 'running' || row.status == 'completed'" type="success" size="small" tooltip="Run now" icon="el-icon-caret-right" @click="runNow(row)">
            Run
          </el-button>
        </template>
      </el-table-column>
    </el-table>

    <pagination v-show="total>0" :total="total" :page.sync="currentPage" :limit.sync="pageSize" @pagination="getList" />

    <queueDetails ref="detailDialog" />

  </div>
</template>

<script>
import Pagination from '@/components/Pagination' // secondary package based on el-pagination
import waves from '@/directive/waves' // waves directive
import { listQueue, runTask } from '@/api/queues'
import queueDetails from './details'
import moment from 'moment'

var timerid = null

export default {
  components: { Pagination, queueDetails },
  directives: { waves },
  filters: {
    statusTranslate(status) {
      const statusMap = {
        completed: 'Completed',
        failed: 'Failed',
        waiting: 'Waiting',
        running: 'Running'
      }
      return statusMap[status] || status
    },
    statusFilter(status) {
      const statusMap = {
        completed: 'success',
        failed: 'warning',
        waiting: 'info',
        running: 'primary'
      }
      return statusMap[status]
    }
  },
  data() {
    return {
      queueid: null,
      tableKey: 0,
      list: null,
      total: 0,
      now: 0,
      listLoading: true,
      currentPage: 1,
      pageSize: 20,
      sortOptions: [
        { key: '+name', label: 'Name' },
        { key: '-name', label: 'Name' }
      ],
      filterParams: {
        name: null,
        sortOrder: null
      }
    }
  },
  created() {
    var self = this
    timerid = setInterval(function() {
      self.now = Date.now()
    }, 1000)

    this.queueid = this.$route.params.queueid
    this.getList()
  },
  destroyed() {
    if (timerid != null) {
      clearInterval(timerid)
    }
  },
  methods: {
    getList() {
      this.listLoading = true
      var filter = this.filterParams.name
      listQueue(this.queueid, (this.currentPage - 1) * this.pageSize, this.pageSize, filter, this.filterParams.sortOrder)
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
    runNow(row) {
      runTask(this.queueid, row.id)
    },
    showEntries(row) {
      this.$refs['detailDialog'].showEntry(this.queueid, row.id)
    },
    computeEta(ts) {
      return moment(ts).fromNow()
    }
  }
}
</script>
