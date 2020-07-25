import request from '@/utils/request'

export function listQueues() {
  return request({
    url: '/admin/queues',
    method: 'get'
  })
}

export function listQueue(queue, offset, count, filter, sortOrder) {
  return request({
    url: '/admin/queue/' + queue + '/search',
    method: 'post',
    data: {
      offset,
      count,
      filter,
      sortOrder
    }
  })
}

export function runTask(queue, id) {
  return request({
    url: '/admin/queue/' + queue + '/' + id + '/run',
    method: 'post'
  })
}

export function listAttempts(queue, id) {
  return request({
    url: '/admin/queue/' + queue + '/' + id + '/lines',
    method: 'post'
  })
}

