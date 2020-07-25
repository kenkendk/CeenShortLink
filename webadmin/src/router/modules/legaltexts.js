/** When your routing table is too long, you can split it into small modules **/

import Layout from '@/layout'

const legalTextsRouter = {
  path: '/legaltexts',
  component: Layout,
  redirect: 'noRedirect',
  name: 'legaltexts',
  meta: {
    roles: ['admin', 'editor'],
    title: 'Legal texts',
    icon: 'documentation'
  },
  children: [
    {
      path: 'privacypolicy',
      component: () => import('@/views/legaltexts/privacypolicy'),
      name: 'PrivacyPolicy',
      meta: {
        title: 'Privacy policy',
        noCache: true
      }
    },
    {
      path: 'personaldatapolicy',
      component: () => import('@/views/legaltexts/personaldatapolicy'),
      name: 'PersonalDataPolicy',
      meta: {
        title: 'Personal data policy',
        noCache: true
      }
    },
    {
      path: 'termsofservice',
      component: () => import('@/views/legaltexts/termsofservice'),
      name: 'TermsOfService',
      meta: {
        title: 'Terms of service',
        noCache: true
      }
    }
  ]
}

export default legalTextsRouter
