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
      <el-button v-waves class="filter-item" type="success" icon="el-icon-plus" @click="addLink">
        Add link
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
        <el-table-column label="Match" prop="match" sortable="custom" width="150px" align="left">
          <template slot-scope="scope">
            <span>{{ scope.row.match }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Target" prop="targetUrl" sortable="custom" min-width="150px" align="left">
          <template slot-scope="scope">
            <span>{{ scope.row.targetUrl }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Name" prop="name" sortable="custom" width="150px" align="left">
          <template slot-scope="scope">
            <span>{{ scope.row.name }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Expires" prop="expires" sortable="custom" min-width="80px" align="left">
          <template slot-scope="scope">
            <span>{{ (scope.row.expires || '01-01-01 00:00') | parseTime('{y}-{m}-{d} {h}:{i}') | zeroDateFilter('Never') }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Valid from" prop="validFrom" sortable="custom" min-width="80px" align="left">
          <template slot-scope="scope">
            <span>{{ (scope.row.validFrom || '01-01-01 00:00') | parseTime('{y}-{m}-{d} {h}:{i}') | zeroDateFilter('Always') }}</span>
          </template>
        </el-table-column>
        <el-table-column label="Status" prop="status" width="130px" align="left">
          <template slot-scope="{row}">
            <i :class="row.enabled ? 'el-icon-circle-check' : 'el-icon-circle-close'" />
            <el-tag :type="row | typeFilter">
              {{ row | typeTranslate }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column label="Actions" align="center" width="220" class-name="small-padding fixed-width">
          <template slot-scope="{row}">
            <el-button v-waves type="primary" size="small" tooltip="Show details" icon="el-icon-edit" @click="showEntry(row)">
              Details
            </el-button>
            <el-button v-waves type="danger" size="small" tooltip="Delete" icon="el-icon-delete" @click="deleteEntry(row)">
              Delete
            </el-button>
          </template>
        </el-table-column>
      </el-table>

      <pagination v-show="total>0" :total="total" :page.sync="currentPage" :limit.sync="pageSize" @pagination="getList" />

      <linkDetails ref="detailDialog" />

    </div>
  </div>
</template>

<script>
import Pagination from '@/components/Pagination' // secondary package based on el-pagination
import waves from '@/directive/waves' // waves directive
import { listLinks, deleteLink } from '@/api/links'
import linkDetails from './details'
import { parseTime } from '@/utils'

const zerodates = [
  parseTime(new Date(0), '{y}-{m}-{d} {h}:{i}'),
  parseTime('0001-01-01T00:00:00', '{y}-{m}-{d} {h}:{i}'),
  '0-0-0 0:0'
]

export default {
  components: { Pagination, linkDetails },
  directives: { waves },
  filters: {
    zeroDateFilter(dt, text) {
      return zerodates.some(n => n === dt) ? (text || dt) : dt
    },
    typeFilter(tp) {
      return tp.enabled ? (tp.isActive ? 'primary' : 'warning') : 'danger'
    },
    typeTranslate(tp) {
      return tp.enabled ? (tp.isActive ? 'Active' : 'Inactive') : 'Disabled'
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
        { key: '+match', label: 'Match' },
        { key: '+targetUrl', label: 'Target' },
        { key: '+expires', label: 'Expires' },

        { key: '-id', label: 'ID' },
        { key: '-match', label: 'Match' },
        { key: '-targetUrl', label: 'Target' },
        { key: '-expires', label: 'Expires' }
      ],
      filterParams: {
        name: null,
        sortOrder: '-id'
      }
    }
  },
  created() {
    this.getList()
  },
  methods: {
    getList() {
      this.listLoading = true
      listLinks((this.currentPage - 1) * this.pageSize, this.pageSize, this.filterParams.name, this.filterParams.sortOrder)
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
    addLink() {
      this.$refs['detailDialog'].showEntry({ 'enabled': true }, () => this.getList())
    },
    showEntry(row) {
      this.$refs['detailDialog'].showEntry({ ...row }, () => this.getList())
    },
    deleteEntry(row) {
      deleteLink(row)
        .then(n => { this.getList() })
    }
  }
}
</script>
