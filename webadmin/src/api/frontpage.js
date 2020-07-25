import request from '@/utils/request'

export function getSettings() {
  return request({
    url: '/admin/frontpagesettings',
    method: 'get'
  })
}

export function updateSettings(item) {
  return request({
    url: '/admin/frontpagesettings',
    method: 'put',
    data: item
  })
}

