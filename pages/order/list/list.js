// pages/order/list/list.js
Page({
  data: {
    orders: [],
    loading: true,
    currentTab: 0,
    tabs: [
      { id: 0, name: '全部', status: '' },
      { id: 1, name: '待付款', status: 'pending' },
      { id: 2, name: '待发货', status: 'paid' },
      { id: 3, name: '待收货', status: 'shipped' },
      { id: 4, name: '已完成', status: 'completed' }
    ]
  },

  onLoad() {
    this.loadOrders()
  },

  onShow() {
    this.loadOrders()
  },

  // 加载订单列表
  loadOrders(status = '') {
    this.setData({ loading: true })
    
    // 模拟数据加载
    setTimeout(() => {
      const allOrders = [
        {
          id: '202312010001',
          status: 'pending',
          statusText: '待付款',
          createTime: '2023-12-01 10:30:00',
          totalAmount: 299.00,
          items: [
            {
              id: 1,
              name: '商品名称1',
              image: '/images/product1.jpg',
              price: 99.00,
              quantity: 2,
              specs: '红色 L'
            },
            {
              id: 2,
              name: '商品名称2',
              image: '/images/product2.jpg',
              price: 199.00,
              quantity: 1,
              specs: '蓝色 M'
            }
          ]
        },
        {
          id: '202312010002',
          status: 'paid',
          statusText: '待发货',
          createTime: '2023-12-01 09:15:00',
          totalAmount: 199.00,
          items: [
            {
              id: 3,
              name: '商品名称3',
              image: '/images/product3.jpg',
              price: 199.00,
              quantity: 1,
              specs: '黑色 XL'
            }
          ]
        },
        {
          id: '202311300001',
          status: 'shipped',
          statusText: '待收货',
          createTime: '2023-11-30 16:20:00',
          totalAmount: 399.00,
          items: [
            {
              id: 4,
              name: '商品名称4',
              image: '/images/product4.jpg',
              price: 399.00,
              quantity: 1,
              specs: '白色 S'
            }
          ]
        },
        {
          id: '202311290001',
          status: 'completed',
          statusText: '已完成',
          createTime: '2023-11-29 14:45:00',
          totalAmount: 99.00,
          items: [
            {
              id: 5,
              name: '商品名称5',
              image: '/images/product5.jpg',
              price: 99.00,
              quantity: 1,
              specs: '绿色 M'
            }
          ]
        }
      ]
      
      const filteredOrders = status ? allOrders.filter(order => order.status === status) : allOrders
      
      this.setData({
        orders: filteredOrders,
        loading: false
      })
    }, 500)
  },

  // 切换标签
  onTabTap(e) {
    const { index } = e.currentTarget.dataset
    const { tabs } = this.data
    
    this.setData({ currentTab: index })
    this.loadOrders(tabs[index].status)
  },

  // 查看订单详情
  onOrderTap(e) {
    const { orderId } = e.currentTarget.dataset
    wx.navigateTo({
      url: `/pages/order/detail/detail?id=${orderId}`
    })
  },

  // 取消订单
  onCancelOrder(e) {
    const { orderId } = e.currentTarget.dataset
    
    wx.showModal({
      title: '确认取消',
      content: '确定要取消这个订单吗？',
      success: (res) => {
        if (res.confirm) {
          // 模拟取消订单
          wx.showToast({
            title: '订单已取消',
            icon: 'success'
          })
          
          // 重新加载订单列表
          setTimeout(() => {
            this.loadOrders(this.data.tabs[this.data.currentTab].status)
          }, 1000)
        }
      }
    })
  },

  // 去付款
  onPayOrder(e) {
    const { orderId } = e.currentTarget.dataset
    
    wx.showModal({
      title: '确认支付',
      content: '确定要支付这个订单吗？',
      success: (res) => {
        if (res.confirm) {
          // 模拟支付
          wx.showToast({
            title: '支付成功',
            icon: 'success'
          })
          
          // 重新加载订单列表
          setTimeout(() => {
            this.loadOrders(this.data.tabs[this.data.currentTab].status)
          }, 1000)
        }
      }
    })
  },

  // 确认收货
  onConfirmReceive(e) {
    const { orderId } = e.currentTarget.dataset
    
    wx.showModal({
      title: '确认收货',
      content: '确定已收到商品吗？',
      success: (res) => {
        if (res.confirm) {
          // 模拟确认收货
          wx.showToast({
            title: '确认收货成功',
            icon: 'success'
          })
          
          // 重新加载订单列表
          setTimeout(() => {
            this.loadOrders(this.data.tabs[this.data.currentTab].status)
          }, 1000)
        }
      }
    })
  },

  // 删除订单
  onDeleteOrder(e) {
    const { orderId } = e.currentTarget.dataset
    
    wx.showModal({
      title: '确认删除',
      content: '确定要删除这个订单吗？',
      success: (res) => {
        if (res.confirm) {
          // 模拟删除订单
          wx.showToast({
            title: '订单已删除',
            icon: 'success'
          })
          
          // 重新加载订单列表
          setTimeout(() => {
            this.loadOrders(this.data.tabs[this.data.currentTab].status)
          }, 1000)
        }
      }
    })
  },

  // 下拉刷新
  onPullDownRefresh() {
    this.loadOrders(this.data.tabs[this.data.currentTab].status)
    wx.stopPullDownRefresh()
  }
})