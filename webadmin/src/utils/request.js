import axios from 'axios'
import { Message } from 'element-ui'
import Cookies from 'js-cookie'

// create an axios instance
const service = axios.create({
  baseURL: process.env.VUE_APP_BASE_API, // url = base url + request url
  // withCredentials: true, // send cookies when cross-domain requests
  timeout: 5000 // request timeout
})

// request interceptor
service.interceptors.request.use(
  config => {
    // do something before request is sent
    // console.log('xsrf-cookie is:', Cookies.get('xsrf-token'))

    config.headers['X-XSRF-Token'] = Cookies.get('xsrf-token')
    return config
  },
  error => {
    // do something with request error
    console.log(error) // for debug
    return Promise.reject(error)
  }
)

// response interceptor
service.interceptors.response.use(
  /**
   * If you want to get http information such as headers or status
   * Please return  response => response
  */

  /**
   * Determine the request status by custom code
   * Here is just an example
   * You can also judge the status by HTTP Status Code
   */
  response => {
    // TODO: Set up a trigger for when the user is logged out
    // maybe look at header or response code?
    const res = response.data
    return res
  },
  error => {
    console.log('err: ' + error) // for debug
    // console.log('err.response: ' + JSON.stringify(error.response)) // for debug
    var msg = error
    if (error.response && error.response.statusText) {
      msg = error.response.statusText
    }

    Message({
      message: msg,
      type: 'error',
      duration: 5 * 1000
    })
    return Promise.reject(error)
  }
)

export default service
