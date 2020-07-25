import request from '@/utils/request'

export function listCollection(collection) {
  return request({
    url: '/images/' + collection,
    method: 'get'
  })
}

export function removeImage(collection, id) {
  return request({
    url: '/images/' + collection + '/' + id,
    method: 'delete'
  })
}

export function reorderCollection(collection, order) {
  return request({
    url: '/images/' + collection,
    method: 'patch',
    data: order
  })
}
