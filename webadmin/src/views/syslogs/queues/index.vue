<template>
  <div class="app-container">
    <el-table
      :key="tableKey"
      v-loading="listLoading"
      :data="list"
      border
      fit
      highlight-current-row
      style="width: 100%;"
    >
      <el-table-column label="Name" prop="name" align="center" width="150px">
        <template slot-scope="scope">
          <span>{{ scope.row.name }}</span>
        </template>
      </el-table-column>
      <el-table-column label="Description" prop="description" min-width="200px" align="center">
        <template slot-scope="scope">
          <span>{{ scope.row.description }}</span>
        </template>
      </el-table-column>
      <el-table-column label="Speed" prop="maxRate" width="100px">
        <template slot-scope="scope">
          <span>{{ scope.row.maxRate }}</span>
          <span>({{ scope.row.concurrent }})</span>
        </template>
      </el-table-column>
      <el-table-column label="Tasks" prop="queueSize" width="100px" align="center">
        <template slot-scope="scope">
          <span>{{ scope.row.running }}</span>
          <span>+</span>
          <span>{{ scope.row.queueSize }}</span>
        </template>
      </el-table-column>
      <el-table-column label="Status" class-name="status-col" prop="active" width="100">
        <template slot-scope="{row}">
          <el-tag :type="row.active ? 'success' : 'danger'">
            {{ row.active ? "Running" : "Stopped" }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="Actions" align="center" width="150" class-name="small-padding fixed-width">
        <template slot-scope="scope">
          <router-link :to="'/syslogs/queue/' + scope.row.name">
            <el-button v-waves type="primary" size="small" tooltip="Show queue" icon="el-icon-view">
              Show queue
            </el-button>
          </router-link>
        </template>
      </el-table-column>
    </el-table>
  </div>
</template>

<script>
import waves from '@/directive/waves' // waves directive
import { listQueues } from '@/api/queues'

export default {
  directives: { waves },
  data() {
    return {
      tableKey: 0,
      list: null,
      total: 0,
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
    this.getList()
  },
  methods: {
    getList() {
      this.listLoading = true
      listQueues()
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
    }
  }
}
</script>
