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
      >
        <el-table-column label="When" prop="when" sortable="custom" width="150px" align="center">
          <template slot-scope="scope">
            <span>{{ scope.row.when | parseTime('{y}-{m}-{d} {h}:{i}') }}</span>
          </template>
        </el-table-column>
        <el-table-column label="To" prop="to" sortable="custom" min-width="150px" align="left">
          <template slot-scope="scope">
            <span>{{ scope.row.to }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Subject" prop="subject" sortable="custom" min-width="150px" align="left">
          <template slot-scope="scope">
            <span>{{ scope.row.subject }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Status" prop="status" sortable="custom" width="180" align="left">
          <template slot-scope="{row}">
            <i :class="row.delivered ? 'el-icon-success' : 'el-icon-error'" />
            <el-tag :type="row.type | typeFilter">
              {{ row.type | typeTranslate }}
            </el-tag>
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
import { listLogs } from '@/api/emaillog'

export default {
  components: { Pagination },
  directives: { waves },
  filters: {
    typeTranslate(status) {
      const statusMap = {
        other: 'Other',
        activationEmail: 'Activation',
        passwordReset: 'Password reset',
        signupConfirmation: 'Confirm signup',
        watchedTopic: 'Watched topic',
        news: 'News'
      }
      return statusMap[status] || status
    },
    typeFilter(status) {
      const statusMap = {
        other: 'info',
        activationEmail: 'primary',
        passwordReset: 'warning',
        signupConfirmation: 'primary',
        watchedTopic: 'info',
        news: 'info'
      }
      return statusMap[status] || 'info'
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

      filterParams: {
        name: null,
        sortOrder: null
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
    }
  }
}
</script>

<style scoped>
i.el-icon-error {
  color: darkred;
}
i.el-icon-success {
  color: lightgreen;
}
</style>

