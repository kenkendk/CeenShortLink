import request from '@/utils/request'

export function listLogs(offset, count, filter, sortOrder) {
  return request({
    url: '/admin/httplogs/search',
    method: 'post',
    data: {
      offset,
      count,
      filter,
      sortOrder
    }
  })
}

export function listLogLines(id) {
  return request({
    url: '/admin/httplogs/' + id + '/lines',
    method: 'get'
  })
}
