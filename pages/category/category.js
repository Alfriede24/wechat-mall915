// pages/category/category.js
Page({
  data: {
    categories: [],
    products: [],
    currentCategoryId: 0,
    loading: true
  },

  onLoad(options) {
    const categoryId = options.id || 0
    this.setData({ currentCategoryId: categoryId })
    this.loadCategories()
    this.loadProducts(categoryId)
  },

  // 加载分类列表
  loadCategories() {
    // 模拟数据
    const categories = [
      { id: 0, name: '全部' },
      { id: 1, name: '手机数码' },
      { id: 2, name: '服装鞋包' },
      { id: 3, name: '家居生活' },
      { id: 4, name: '美妆护肤' }
    ]
    this.setData({ categories })
  },

  // 加载商品列表
  loadProducts(categoryId) {
    this.setData({ loading: true })
    
    // 模拟数据加载
    setTimeout(() => {
      const products = [
        { id: 1, name: '商品1', price: 99.00, image: '/images/product1.jpg', categoryId: 1 },
        { id: 2, name: '商品2', price: 199.00, image: '/images/product2.jpg', categoryId: 2 },
        { id: 3, name: '商品3', price: 299.00, image: '/images/product3.jpg', categoryId: 1 },
        { id: 4, name: '商品4', price: 399.00, image: '/images/product4.jpg', categoryId: 3 }
      ]
      
      const filteredProducts = categoryId == 0 ? products : products.filter(p => p.categoryId == categoryId)
      
      this.setData({
        products: filteredProducts,
        loading: false
      })
    }, 500)
  },

  // 分类切换
  onCategoryTap(e) {
    const categoryId = e.currentTarget.dataset.id
    this.setData({ currentCategoryId: categoryId })
    this.loadProducts(categoryId)
  },

  // 商品点击
  onProductTap(e) {
    const { id } = e.currentTarget.dataset
    wx.navigateTo({
      url: `/pages/product/detail/detail?id=${id}`
    })
  }
})