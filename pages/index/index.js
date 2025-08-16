// pages/index/index.js
Page({
  data: {
    banners: [],
    categories: [],
    products: [],
    loading: true
  },

  onLoad() {
    this.loadData()
  },

  onShow() {
    // 页面显示时刷新数据
  },

  // 加载首页数据
  loadData() {
    this.setData({ loading: true })
    
    // 模拟数据加载
    setTimeout(() => {
      this.setData({
        banners: [
          { id: 1, image: '/images/banner1.jpg', title: '轮播图1' },
          { id: 2, image: '/images/banner2.jpg', title: '轮播图2' }
        ],
        categories: [
          { id: 1, name: '手机数码', icon: '/images/category1.png' },
          { id: 2, name: '服装鞋包', icon: '/images/category2.png' },
          { id: 3, name: '家居生活', icon: '/images/category3.png' },
          { id: 4, name: '美妆护肤', icon: '/images/category4.png' }
        ],
        products: [
          { id: 1, name: '商品1', price: 99.00, image: '/images/product1.jpg' },
          { id: 2, name: '商品2', price: 199.00, image: '/images/product2.jpg' }
        ],
        loading: false
      })
    }, 1000)
  },

  // 轮播图点击
  onBannerTap(e) {
    const { id } = e.currentTarget.dataset
    console.log('点击轮播图', id)
  },

  // 分类点击
  onCategoryTap(e) {
    const { id } = e.currentTarget.dataset
    wx.navigateTo({
      url: `/pages/category/category?id=${id}`
    })
  },

  // 商品点击
  onProductTap(e) {
    const { id } = e.currentTarget.dataset
    wx.navigateTo({
      url: `/pages/product/detail/detail?id=${id}`
    })
  }
})