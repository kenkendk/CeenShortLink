import request from '@/utils/request'

export function getText(id) {
  return request({
    url: '/admin/text/' + id,
    method: 'get'
  })
}

export function updateText(id, body, subject, lastChanged) {
  return request({
    url: '/admin/text/' + id,
    method: 'post',
    data: { body: body, subject: subject, lastChanged: lastChanged }
  })
}
