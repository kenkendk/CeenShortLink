<template>
  <div :class="className" :style="{height:height,width:width}" />
</template>

<script>
import echarts from 'echarts'
require('echarts/theme/macarons') // echarts theme
import resize from './mixins/resize'

export default {
  mixins: [resize],
  props: {
    className: {
      type: String,
      default: 'chart'
    },
    width: {
      type: String,
      default: '100%'
    },
    height: {
      type: String,
      default: '350px'
    },
    autoResize: {
      type: Boolean,
      default: true
    },
    chartData: {
      type: Object,
      required: true
    }
  },
  data() {
    return {
      chart: null
    }
  },
  watch: {
    chartData: {
      deep: true,
      handler(val) {
        this.setOptions(val)
      }
    }
  },
  mounted() {
    this.$nextTick(() => {
      this.initChart()
    })
  },
  beforeDestroy() {
    if (!this.chart) {
      return
    }
    this.chart.dispose()
    this.chart = null
  },
  methods: {
    initChart() {
      this.chart = echarts.init(this.$el, 'macarons')
      this.setOptions(this.chartData)
    },
    setOptions(values = {}) {
      var series = []
      var labels = []
      var colors = ['#d73027', '#f46d43', '#fdae61', '#fee090', '#e0f3f8', '#abd9e9', '#74add1', '#4575b4']
      var color = 2
      var days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
      var daylabels = []
      var dow = new Date().getDay()

      for (var i = 0; i < 7; i++) {
        daylabels.push(days[(dow + days.length - i) % days.length])
      }
      daylabels.reverse()

      for (var v in values) {
        if (typeof (values[v]) !== typeof ([])) {
          continue
        }

        labels.push(v)
        series.push({
          name: v,
          smooth: true,
          type: 'line',
          itemStyle: {
            normal: {
              color: colors[color],
              lineStyle: {
                color: colors[color],
                width: 2
              },
              areaStyle: {
                color: colors[color]
              }
            }
          },
          data: values[v],
          animationDuration: 2800,
          animationEasing: 'quadraticOut'
        })
        color = (color + 1) % colors.length
      }

      this.chart.setOption({
        xAxis: {
          data: daylabels,
          boundaryGap: false,
          axisTick: {
            show: false
          }
        },
        grid: {
          left: 10,
          right: 10,
          bottom: 20,
          top: 30,
          containLabel: true
        },
        tooltip: {
          trigger: 'axis',
          axisPointer: {
            type: 'cross'
          },
          padding: [5, 10]
        },
        yAxis: {
          axisTick: {
            show: false
          }
        },
        legend: {
          data: labels
        },
        series: series
      })
    }
  }
}
</script>
