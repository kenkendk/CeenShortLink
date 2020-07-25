import request from '@/utils/request'

export function listLinks(offset, count, filter, sortOrder) {
  return request({
    url: '/links/search',
    method: 'post',
    data: {
      offset,
      count,
      filter,
      sortOrder
    }
  })
}

export function addLink(item) {
  return request({
    url: '/links',
    method: 'post',
    data: item
  })
}

export function updateLink(item) {
  return request({
    url: '/links/' + item.id,
    method: 'put',
    data: item
  })
}

export function deleteLink(item) {
  return request({
    url: '/links/' + item.id,
    method: 'delete'
  })
}
