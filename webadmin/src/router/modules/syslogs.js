/** When your routing table is too long, you can split it into small modules **/

import Layout from '@/layout'

const router = {
  path: '/syslogs',
  component: Layout,
  redirect: 'noRedirect',
  name: 'syslogs',
  meta: {
    roles: ['admin'],
    title: 'System logs',
    icon: 'star'
  },
  children: [
    {
      path: 'queues',
      component: () => import('@/views/syslogs/queues'),
      name: 'Queues',
      meta: {
        title: 'Background tasks',
        noCache: true
      }
    },
    {
      path: 'queue/:queueid',
      component: () => import('@/views/syslogs/queue'),
      name: 'ShowQueue',
      meta: {
        title: 'Show queue',
        noCache: true,
        activeMenu: '/syslogs/queues'
      },
      hidden: true
    },
    {
      path: 'httplogs',
      component: () => import('@/views/syslogs/httplogs'),
      name: 'HttpLogs',
      meta: {
        title: 'Server logs',
        noCache: true
      }
    },
    {
      path: 'emaillogs',
      component: () => import('@/views/syslogs/emaillogs'),
      name: 'EmailLogs',
      meta: {
        title: 'Email logs',
        noCache: true
      }
    }
  ]
}

export function createQueueRoute(name, id) {
  return {
    path: '' + id,
    component: () => import('@/views/syslogs/queue'),
    name: 'ShowQueue' + id,
    props: {
      queueid: id
    },
    meta: {
      title: 'Queue ' + name,
      noCache: true
    }
  }
}

export const syslogRouter = router
export default router
