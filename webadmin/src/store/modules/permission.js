import { asyncRoutes, constantRoutes, createQueueRoute } from '@/router'

/**
 * Use meta.role to determine if the current user has permission
 * @param roles
 * @param route
 */
function hasPermission(roles, route) {
  if (route.meta && route.meta.roles) {
    return roles.some(role => route.meta.roles.includes(role))
  } else {
    return true
  }
}

/**
 * Filter asynchronous routing tables by recursion
 * @param routes asyncRoutes
 * @param roles
 */
export function filterAsyncRoutes(routes, roles) {
  const res = []

  routes.forEach(route => {
    const tmp = { ...route }
    if (hasPermission(roles, tmp)) {
      if (tmp.children) {
        tmp.children = filterAsyncRoutes(tmp.children, roles)
      }
      res.push(tmp)
    }
  })

  return res
}

/**
 * Append queues to the menu
 * @param routes asyncRoutes
 * @param queues
 */
export function appendQueueRoutes(routes, queues) {
  const res = []

  try {
    routes.forEach(route => {
      const tmp = {
        ...route
      }

      if (tmp.name === 'Queues') {
        tmp.children = []
        if (queues != null) {
          for (var k in queues) {
            tmp.children.push(createQueueRoute(queues[k].name, queues[k].name))
          }
        }
      } else if (tmp.children) {
        tmp.children = appendQueueRoutes(tmp.children, queues)
      }
      res.push(tmp)
    })
  } catch (error) {
    console.log(error)
  }

  console.log('returning res: ', res)
  return res
}

const state = {
  routes: [],
  addRoutes: []
}

const mutations = {
  SET_ROUTES: (state, routes) => {
    state.addRoutes = routes
    state.routes = constantRoutes.concat(routes)
  }
}

const actions = {
  generateRoutes({ commit }, { roles, queues }) {
    return new Promise(resolve => {
      const expandedRoutes = asyncRoutes // appendQueueRoutes(asyncRoutes, queues)

      let accessedRoutes
      if (roles.includes('admin')) {
        accessedRoutes = expandedRoutes || []
      } else {
        accessedRoutes = filterAsyncRoutes(expandedRoutes, roles)
      }

      commit('SET_ROUTES', accessedRoutes)
      resolve(accessedRoutes)
    })
  }
}

export default {
  namespaced: true,
  state,
  mutations,
  actions
}
