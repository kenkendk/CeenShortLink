import request from '@/utils/request'

export function overview() {
  return request({
    url: '/admin/dashboardstats',
    method: 'post'
  })
}

export function graph(from, to, buckets, type) {
  return request({
    url: '/admin/dashboardstats/graph',
    method: 'post',
    data: { from, to, buckets, type }
  })
}
