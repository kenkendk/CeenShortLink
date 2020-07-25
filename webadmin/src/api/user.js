import request from '@/utils/request'

export function login(idata) {
  var data = new FormData()
  data.set('username', idata.username)
  data.set('password', idata.password)
  data.set('remember', idata.remember)

  return request({
    url: '/login',
    method: 'post',
    data,
    config: {
      headers: {
        'Content-Type': 'multipart/form-data'
      }
    }
  })
}

export function getInfo(token) {
  return request({
    url: '/user/me/quick',
    method: 'get'
  })
}

export function logout() {
  return request({
    url: '/logout',
    method: 'post'
  })
}

export function listUsers(offset, count, filter, sortOrder) {
  return request({
    url: '/admin/users/search',
    method: 'post',
    data: {
      offset,
      count,
      filter,
      sortOrder
    }
  })
}

export function addUser(item) {
  return request({
    url: '/admin/users',
    method: 'put',
    data: item
  })
}

export function updateUser(item) {
  return request({
    url: '/admin/users/' + item.id,
    method: 'patch',
    data: item
  })
}

export function deleteUser(item) {
  return request({
    url: '/admin/users/' + item.id,
    method: 'delete'
  })
}

export function changePassword(id, current, password, repeat) {
  return request({
    url: '/admin/users/' + id + '/password',
    method: 'put',
    data: { current: current, new: password, repeated: repeat }
  })
}

