// pages/product/detail/detail.js
Page({
  data: {
    product: null,
    loading: true,
    quantity: 1,
    selectedSpec: {},
    specs: []
  },

  onLoad(options) {
    const productId = options.id
    if (productId) {
      this.loadProductDetail(productId)
    }
  },

  // 加载商品详情
  loadProductDetail(productId) {
    this.setData({ loading: true })
    
    // 模拟数据加载
    setTimeout(() => {
      const product = {
        id: productId,
        name: '商品详情页面',
        price: 299.00,
        originalPrice: 399.00,
        images: [
          '/images/product1.jpg',
          '/images/product2.jpg',
          '/images/product3.jpg'
        ],
        description: '这是一个优质的商品，具有出色的品质和性能。适合各种场合使用，是您的理想选择。',
        specs: [
          { name: '颜色', options: ['红色', '蓝色', '黑色'] },
          { name: '尺寸', options: ['S', 'M', 'L', 'XL'] }
        ],
        stock: 100,
        sales: 1234,
        rating: 4.8,
        reviews: 567
      }
      
      this.setData({
        product,
        specs: product.specs,
        loading: false
      })
    }, 500)
  },

  // 图片预览
  onImageTap(e) {
    const { index } = e.currentTarget.dataset
    const { images } = this.data.product
    
    wx.previewImage({
      current: images[index],
      urls: images
    })
  },

  // 规格选择
  onSpecTap(e) {
    const { specName, option } = e.currentTarget.dataset
    const selectedSpec = { ...this.data.selectedSpec }
    selectedSpec[specName] = option
    
    this.setData({ selectedSpec })
  },

  // 数量变更
  onQuantityChange(e) {
    const { type } = e.currentTarget.dataset
    let { quantity } = this.data
    
    if (type === 'minus' && quantity > 1) {
      quantity--
    } else if (type === 'plus' && quantity < this.data.product.stock) {
      quantity++
    }
    
    this.setData({ quantity })
  },

  // 添加到购物车
  onAddToCart() {
    const { product, quantity, selectedSpec } = this.data
    
    // 检查规格是否选择完整
    const requiredSpecs = product.specs.map(spec => spec.name)
    const selectedSpecs = Object.keys(selectedSpec)
    
    if (requiredSpecs.length > 0 && requiredSpecs.some(spec => !selectedSpecs.includes(spec))) {
      wx.showToast({
        title: '请选择商品规格',
        icon: 'none'
      })
      return
    }
    
    // 模拟添加到购物车
    wx.showToast({
      title: '已添加到购物车',
      icon: 'success'
    })
    
    console.log('添加到购物车:', {
      productId: product.id,
      quantity,
      selectedSpec
    })
  },

  // 立即购买
  onBuyNow() {
    const { product, quantity, selectedSpec } = this.data
    
    // 检查规格是否选择完整
    const requiredSpecs = product.specs.map(spec => spec.name)
    const selectedSpecs = Object.keys(selectedSpec)
    
    if (requiredSpecs.length > 0 && requiredSpecs.some(spec => !selectedSpecs.includes(spec))) {
      wx.showToast({
        title: '请选择商品规格',
        icon: 'none'
      })
      return
    }
    
    // 跳转到订单确认页面
    wx.navigateTo({
      url: `/pages/order/checkout/checkout?productId=${product.id}&quantity=${quantity}`
    })
  },

  // 收藏商品
  onToggleFavorite() {
    wx.showToast({
      title: '已收藏',
      icon: 'success'
    })
  },

  // 分享商品
  onShareAppMessage() {
    const { product } = this.data
    return {
      title: product.name,
      path: `/pages/product/detail/detail?id=${product.id}`,
      imageUrl: product.images[0]
    }
  }
})