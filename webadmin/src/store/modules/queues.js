import { listQueues } from '@/api/queues'

const state = {
  queues: null
}

const mutations = {
  SET_QUEUES: (state, queues) => {
    state.queues = queues
  }
}

const actions = {
  // user login
  getQueues({ commit, state }) {
    return new Promise((resolve, reject) => {
      if (state.queues != null) {
        resolve(state.queues)
      } else {
        listQueues()
          .then(x => {
            commit('SET_QUEUES', x.result)
            resolve(x.result)
          })
          .catch(error => {
            reject(error)
          })
      }
    })
  }
}

export default {
  namespaced: true,
  state,
  mutations,
  actions
}
