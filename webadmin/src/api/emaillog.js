import request from '@/utils/request'

export function listLogs(offset, count, filter, sortOrder) {
  return request({
    url: '/admin/emaillogs/search',
    method: 'post',
    data: {
      offset,
      count,
      filter,
      sortOrder
    }
  })
}
