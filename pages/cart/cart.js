// pages/cart/cart.js
Page({
  data: {
    cartItems: [],
    totalPrice: 0,
    selectedAll: false,
    loading: true
  },

  onLoad() {
    this.loadCartData()
  },

  onShow() {
    this.loadCartData()
  },

  // 加载购物车数据
  loadCartData() {
    this.setData({ loading: true })
    
    // 模拟数据
    setTimeout(() => {
      const cartItems = [
        {
          id: 1,
          productId: 1,
          name: '商品1',
          price: 99.00,
          image: '/images/product1.jpg',
          quantity: 2,
          selected: true
        },
        {
          id: 2,
          productId: 2,
          name: '商品2',
          price: 199.00,
          image: '/images/product2.jpg',
          quantity: 1,
          selected: false
        }
      ]
      
      this.setData({
        cartItems,
        loading: false
      })
      
      this.calculateTotal()
    }, 500)
  },

  // 计算总价
  calculateTotal() {
    const { cartItems } = this.data
    let totalPrice = 0
    let selectedCount = 0
    
    cartItems.forEach(item => {
      if (item.selected) {
        totalPrice += item.price * item.quantity
        selectedCount++
      }
    })
    
    this.setData({
      totalPrice: totalPrice.toFixed(2),
      selectedAll: selectedCount === cartItems.length && cartItems.length > 0
    })
  },

  // 选择商品
  onSelectItem(e) {
    const { index } = e.currentTarget.dataset
    const cartItems = this.data.cartItems
    cartItems[index].selected = !cartItems[index].selected
    
    this.setData({ cartItems })
    this.calculateTotal()
  },

  // 全选/取消全选
  onSelectAll() {
    const { selectedAll, cartItems } = this.data
    const newSelectedAll = !selectedAll
    
    cartItems.forEach(item => {
      item.selected = newSelectedAll
    })
    
    this.setData({
      cartItems,
      selectedAll: newSelectedAll
    })
    
    this.calculateTotal()
  },

  // 修改数量
  onQuantityChange(e) {
    const { index, type } = e.currentTarget.dataset
    const cartItems = this.data.cartItems
    
    if (type === 'minus' && cartItems[index].quantity > 1) {
      cartItems[index].quantity--
    } else if (type === 'plus') {
      cartItems[index].quantity++
    }
    
    this.setData({ cartItems })
    this.calculateTotal()
  },

  // 删除商品
  onDeleteItem(e) {
    const { index } = e.currentTarget.dataset
    const { cartItems } = this.data
    
    wx.showModal({
      title: '确认删除',
      content: '确定要删除这个商品吗？',
      success: (res) => {
        if (res.confirm) {
          cartItems.splice(index, 1)
          this.setData({ cartItems })
          this.calculateTotal()
        }
      }
    })
  },

  // 去结算
  onCheckout() {
    const selectedItems = this.data.cartItems.filter(item => item.selected)
    
    if (selectedItems.length === 0) {
      wx.showToast({
        title: '请选择商品',
        icon: 'none'
      })
      return
    }
    
    wx.navigateTo({
      url: '/pages/order/checkout/checkout'
    })
  }
})