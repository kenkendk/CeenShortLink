<template>
  <div class="dashboard-editor-container">
    <panel-group @handleSetLineChartData="handleSetLineChartData" />

    <el-row style="background:#fff;padding:16px 16px 0;margin-bottom:32px;">
      <line-chart :chart-data="lineChartData" />
    </el-row>

  </div>
</template>

<script>
import PanelGroup from './components/PanelGroup'
import LineChart from './components/LineChart'
import { graph } from '@/api/dashboard'

var lineChartCache = {}
const translations = {
  'waitListSize': 'On waiting list',
  'activatedUsers': 'Active users',
  'confirmedSignups': 'Confirmed users',
  'nonConfirmedSignups': 'Unconfirmed users',
  'ok': 'Success',
  'clientError': 'Client error',
  'serverError': 'Server error',
  'sent': 'Sent emails',
  'clicks': 'Clicks',
  'newLinks': 'New Links'
}

export default {
  name: 'DashboardAdmin',
  components: {
    PanelGroup,
    LineChart
  },
  data() {
    return {
      lineChartData: {}
    }
  },
  mounted() {
    this.handleSetLineChartData('links')
  },
  methods: {
    handleSetLineChartData(type) {
      if (lineChartCache[type] == null) {
        var to = parseInt((new Date()).getTime() / 1000)
        var from = to - (7 * 24 * 60 * 60)

        graph(from, to, 7, type).then(x => {
          var res = {}
          for (var k in x) {
            res[translations[k] || k] = x[k]
          }

          this.lineChartData = lineChartCache[type] = res
        })
      } else {
        this.lineChartData = lineChartCache[type]
      }
    }
  }
}
</script>

<style lang="scss" scoped>
.dashboard-editor-container {
  padding: 32px;
  background-color: rgb(240, 242, 245);
  position: relative;

  .chart-wrapper {
    background: #fff;
    padding: 16px 16px 0;
    margin-bottom: 32px;
  }
}

@media (max-width:1024px) {
  .chart-wrapper {
    padding: 8px;
  }
}
</style>
