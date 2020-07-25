<template>
  <el-row :gutter="40" class="panel-group">
    <el-col :xs="12" :sm="12" :lg="6" class="card-panel-col">
      <div class="card-panel" @click="handleSetLineChartData('links')">
        <div class="card-panel-icon-wrapper icon-email">
          <svg-icon icon-class="email" class-name="card-panel-icon" />
        </div>
        <div class="card-panel-description">
          <div class="card-panel-text">
            Clicks (7d)
          </div>
          <count-to :start-val="0" :end-val="links.clicks" :duration="3600" class="card-panel-num" />
        </div>
      </div>
    </el-col>
    <el-col :xs="12" :sm="12" :lg="6" class="card-panel-col">
      <div class="card-panel" @click="handleSetLineChartData('links')">
        <div class="card-panel-icon-wrapper icon-email">
          <svg-icon icon-class="email" class-name="card-panel-icon" />
        </div>
        <div class="card-panel-description">
          <div class="card-panel-text">
            Links
          </div>
          <count-to :start-val="0" :end-val="links.linksTotal" :duration="3600" class="card-panel-num" />
        </div>
      </div>
    </el-col>
    <el-col :xs="12" :sm="12" :lg="6" class="card-panel-col">
      <div class="card-panel" @click="handleSetLineChartData('links')">
        <div class="card-panel-icon-wrapper icon-email">
          <svg-icon icon-class="email" class-name="card-panel-icon" />
        </div>
        <div class="card-panel-description">
          <div class="card-panel-text">
            New Links (7d)
          </div>
          <count-to :start-val="0" :end-val="links.newLinks" :duration="3600" class="card-panel-num" />
        </div>
      </div>
    </el-col>
    <el-col :xs="12" :sm="12" :lg="6" class="card-panel-col">
      <div class="card-panel" @click="handleSetLineChartData('email')">
        <div class="card-panel-icon-wrapper icon-email">
          <svg-icon icon-class="email" class-name="card-panel-icon" />
        </div>
        <div class="card-panel-description">
          <div class="card-panel-text">
            Emails (7d)
          </div>
          <count-to :start-val="0" :end-val="email.emailsSent" :duration="3600" class="card-panel-num" />
        </div>
      </div>
    </el-col>
    <el-col :xs="12" :sm="12" :lg="6" class="card-panel-col">
      <div class="card-panel" @click="handleSetLineChartData('http')">
        <div class="card-panel-icon-wrapper icon-people">
          <svg-icon icon-class="people" class-name="card-panel-icon" />
        </div>
        <div class="card-panel-description">
          <div class="card-panel-text">
            OK (7d)
          </div>
          <count-to :start-val="0" :end-val="http.ok" :duration="3600" class="card-panel-num" />
        </div>
      </div>
    </el-col>
    <el-col :xs="12" :sm="12" :lg="6" class="card-panel-col">
      <div class="card-panel" @click="handleSetLineChartData('http')">
        <div class="card-panel-icon-wrapper icon-404">
          <svg-icon icon-class="404" class-name="card-panel-icon" />
        </div>
        <div class="card-panel-description">
          <div class="card-panel-text">
            Client (7d)
          </div>
          <count-to :start-val="0" :end-val="http.clientError" :duration="3600" class="card-panel-num" />
        </div>
      </div>
    </el-col>
    <el-col :xs="12" :sm="12" :lg="6" class="card-panel-col">
      <div class="card-panel" @click="handleSetLineChartData('http')">
        <div class="card-panel-icon-wrapper icon-bug">
          <svg-icon icon-class="bug" class-name="card-panel-icon" />
        </div>
        <div class="card-panel-description">
          <div class="card-panel-text">
            Server (7d)
          </div>
          <count-to :start-val="0" :end-val="http.serverError" :duration="3600" class="card-panel-num" />
        </div>
      </div>
    </el-col>
    <el-col :xs="12" :sm="12" :lg="6" class="card-panel-col">
      <div class="card-panel" @click="handleSetLineChartData('http')">
        <div class="card-panel-icon-wrapper icon-international">
          <svg-icon icon-class="international" class-name="card-panel-icon" />
        </div>
        <div class="card-panel-description">
          <div class="card-panel-text">
            Visits (7d)
          </div>
          <count-to :start-val="0" :end-val="http.landingPage" :duration="3600" class="card-panel-num" />
        </div>
      </div>
    </el-col>
  </el-row>
</template>

<script>
import CountTo from 'vue-count-to'
import { overview } from '@/api/dashboard'

export default {
  components: {
    CountTo
  },
  data() {
    return {
      signup: {
        waitListSize: null,
        activatedUsers: null,
        newConfirmedSignups: null,
        nonConfirmedSignups: null
      },
      email: {
        emailsSent: null
      },
      http: {
        ok: null,
        clientError: null,
        serverError: null,
        landingPage: null
      },
      links: {
        linksTotal: null,
        newLinks: null,
        mostPopular: null,
        clicks: null
      }
    }
  },
  mounted() {
    overview()
      .then(x => {
        this.$set(this, 'signup', x.signup)
        this.$set(this, 'email', x.email)
        this.$set(this, 'http', x.http)
        this.$set(this, 'links', x.links)
      })
  },
  methods: {
    handleSetLineChartData(type) {
      this.$emit('handleSetLineChartData', type)
    }
  }
}
</script>

<style lang="scss" scoped>
.panel-group {
  margin-top: 18px;

  .card-panel-col {
    margin-bottom: 32px;
  }

  .card-panel {
    height: 108px;
    cursor: pointer;
    font-size: 12px;
    position: relative;
    overflow: hidden;
    color: #666;
    background: #fff;
    box-shadow: 4px 4px 40px rgba(0, 0, 0, .05);
    border-color: rgba(0, 0, 0, .05);

    &:hover {
      .card-panel-icon-wrapper {
        color: #fff;
      }

      .icon-people, .icon-user, .icon-guide, .icon-example {
        background: #40c9c6;
      }

      .icon-message, .icon-email {
        background: #36a3f7;
      }

      .icon-money, .icon-bug, .icon-404 {
        background: #f4516c;
      }

      .icon-shopping, .icon-international, .icon-people {
        background: #34bfa3
      }
    }

    .icon-people, .icon-user, .icon-guide, .icon-example {
      color: #40c9c6;
    }

    .icon-message, .icon-email {
      color: #36a3f7;
    }

    .icon-money, .icon-bug, .icon-404 {
      color: #f4516c;
    }

    .icon-shopping, .icon-international, .icon-people {
      color: #34bfa3
    }

    .card-panel-icon-wrapper {
      float: left;
      margin: 14px 0 0 14px;
      padding: 16px;
      transition: all 0.38s ease-out;
      border-radius: 6px;
    }

    .card-panel-icon {
      float: left;
      font-size: 48px;
    }

    .card-panel-description {
      float: right;
      font-weight: bold;
      margin: 26px;
      margin-left: 0px;

      .card-panel-text {
        line-height: 18px;
        color: rgba(0, 0, 0, 0.45);
        font-size: 16px;
        margin-bottom: 12px;
      }

      .card-panel-num {
        font-size: 20px;
      }
    }
  }
}

@media (max-width:550px) {
  .card-panel-description {
    display: none;
  }

  .card-panel-icon-wrapper {
    float: none !important;
    width: 100%;
    height: 100%;
    margin: 0 !important;

    .svg-icon {
      display: block;
      margin: 14px auto !important;
      float: none !important;
    }
  }
}
</style>
