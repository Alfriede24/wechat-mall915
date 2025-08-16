// pages/profile/profile.js
Page({
  data: {
    userInfo: null,
    hasUserInfo: false,
    menuItems: [
      {
        id: 1,
        title: '我的订单',
        icon: '/images/order.png',
        url: '/pages/order/list/list'
      },
      {
        id: 2,
        title: '收货地址',
        icon: '/images/address.png',
        url: '/pages/address/list/list'
      },
      {
        id: 3,
        title: '我的收藏',
        icon: '/images/favorite.png',
        url: '/pages/favorite/favorite'
      },
      {
        id: 4,
        title: '客服中心',
        icon: '/images/service.png',
        url: '/pages/service/service'
      },
      {
        id: 5,
        title: '设置',
        icon: '/images/setting.png',
        url: '/pages/setting/setting'
      }
    ]
  },

  onLoad() {
    this.checkUserInfo()
  },

  onShow() {
    this.checkUserInfo()
  },

  // 检查用户信息
  checkUserInfo() {
    const userInfo = wx.getStorageSync('userInfo')
    if (userInfo) {
      this.setData({
        userInfo,
        hasUserInfo: true
      })
    }
  },

  // 获取用户信息
  getUserProfile() {
    wx.getUserProfile({
      desc: '用于完善用户资料',
      success: (res) => {
        const userInfo = res.userInfo
        this.setData({
          userInfo,
          hasUserInfo: true
        })
        
        // 保存用户信息到本地
        wx.setStorageSync('userInfo', userInfo)
        
        // 调用登录接口
        this.login()
      },
      fail: (err) => {
        console.log('获取用户信息失败', err)
      }
    })
  },

  // 登录
  login() {
    wx.login({
      success: (res) => {
        if (res.code) {
          // 调用后端登录接口
          const app = getApp()
          app.request({
            url: '/api/user/login',
            method: 'POST',
            data: {
              code: res.code,
              nickName: this.data.userInfo.nickName,
              avatarUrl: this.data.userInfo.avatarUrl
            },
            success: (result) => {
              if (result.success) {
                wx.setStorageSync('token', result.data.token)
                wx.showToast({
                  title: '登录成功',
                  icon: 'success'
                })
              }
            },
            fail: (err) => {
              console.log('登录失败', err)
              wx.showToast({
                title: '登录失败',
                icon: 'none'
              })
            }
          })
        }
      }
    })
  },

  // 菜单项点击
  onMenuTap(e) {
    const { url } = e.currentTarget.dataset
    
    if (!this.data.hasUserInfo) {
      wx.showToast({
        title: '请先登录',
        icon: 'none'
      })
      return
    }
    
    wx.navigateTo({
      url
    })
  },

  // 退出登录
  onLogout() {
    wx.showModal({
      title: '确认退出',
      content: '确定要退出登录吗？',
      success: (res) => {
        if (res.confirm) {
          wx.removeStorageSync('userInfo')
          wx.removeStorageSync('token')
          this.setData({
            userInfo: null,
            hasUserInfo: false
          })
          wx.showToast({
            title: '已退出登录',
            icon: 'success'
          })
        }
      }
    })
  }
})