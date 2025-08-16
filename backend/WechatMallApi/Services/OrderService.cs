using Microsoft.EntityFrameworkCore;
using WechatMallApi.Data;
using WechatMallApi.DTOs;
using WechatMallApi.Models;
using AutoMapper;
using System.Text.Json;

namespace WechatMallApi.Services
{
    public interface IOrderService
    {
        Task<ApiResponse<OrderPreviewDto>> PreviewOrderAsync(int userId, CartCheckRequest request);
        Task<ApiResponse<OrderDto>> CreateOrderAsync(int userId, CreateOrderRequest request);
        Task<ApiResponse<PagedResponse<OrderDto>>> GetOrdersAsync(int userId, OrderListRequest request);
        Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int userId, int orderId);
        Task<ApiResponse<PayOrderResponse>> PayOrderAsync(int userId, PayOrderRequest request);
        Task<ApiResponse> CancelOrderAsync(int userId, CancelOrderRequest request);
        Task<ApiResponse> ConfirmReceiptAsync(int userId, ConfirmReceiptRequest request);
        Task<ApiResponse<OrderStatisticsDto>> GetOrderStatisticsAsync(int userId);
    }

    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<OrderPreviewDto>> PreviewOrderAsync(int userId, CartCheckRequest request)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .Include(c => c.ProductSku)
                    .Where(c => request.CartItemIds.Contains(c.Id) && c.UserId == userId)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return ApiResponse<OrderPreviewDto>.Error("购物车项不存在");
                }

                // 检查库存
                foreach (var cartItem in cartItems)
                {
                    var availableStock = cartItem.ProductSku?.Stock ?? cartItem.Product.Stock;
                    if (availableStock < cartItem.Quantity)
                    {
                        return ApiResponse<OrderPreviewDto>.Error($"商品 {cartItem.Product.Name} 库存不足");
                    }
                }

                // 获取默认地址
                var defaultAddress = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

                if (defaultAddress == null)
                {
                    return ApiResponse<OrderPreviewDto>.Error("请先设置收货地址");
                }

                var orderItems = cartItems.Select(c => MapToOrderItemDto(c)).ToList();
                var totalAmount = orderItems.Sum(i => i.TotalAmount);
                var shippingFee = CalculateShippingFee(totalAmount);
                var discountAmount = 0m; // 暂时不支持优惠券
                var payAmount = totalAmount + shippingFee - discountAmount;

                var preview = new OrderPreviewDto
                {
                    Items = orderItems,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    ShippingFee = shippingFee,
                    PayAmount = payAmount,
                    Address = _mapper.Map<UserAddressDto>(defaultAddress)
                };

                return ApiResponse<OrderPreviewDto>.Ok(preview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订单预览失败");
                return ApiResponse<OrderPreviewDto>.Error("订单预览失败");
            }
        }

        public async Task<ApiResponse<OrderDto>> CreateOrderAsync(int userId, CreateOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 获取购物车项
                var cartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .Include(c => c.ProductSku)
                    .Where(c => request.CartItemIds.Contains(c.Id) && c.UserId == userId)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return ApiResponse<OrderDto>.Error("购物车项不存在");
                }

                // 获取收货地址
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == userId);

                if (address == null)
                {
                    return ApiResponse<OrderDto>.Error("收货地址不存在");
                }

                // 检查库存并锁定
                foreach (var cartItem in cartItems)
                {
                    var availableStock = cartItem.ProductSku?.Stock ?? cartItem.Product.Stock;
                    if (availableStock < cartItem.Quantity)
                    {
                        return ApiResponse<OrderDto>.Error($"商品 {cartItem.Product.Name} 库存不足");
                    }

                    // 减少库存
                    if (cartItem.ProductSku != null)
                    {
                        cartItem.ProductSku.Stock -= cartItem.Quantity;
                    }
                    else
                    {
                        cartItem.Product.Stock -= cartItem.Quantity;
                    }
                }

                // 计算金额
                var totalAmount = cartItems.Sum(c => (c.ProductSku?.Price ?? c.Product.Price) * c.Quantity);
                var shippingFee = CalculateShippingFee(totalAmount);
                var discountAmount = request.CouponAmount ?? 0m;
                var payAmount = totalAmount + shippingFee - discountAmount;

                // 创建订单
                var order = new Order
                {
                    OrderNo = GenerateOrderNo(),
                    UserId = userId,
                    TotalAmount = totalAmount,
                    DiscountAmount = discountAmount,
                    ShippingFee = shippingFee,
                    PayAmount = payAmount,
                    Status = 1, // 待支付
                    PaymentStatus = 0, // 未支付
                    ReceiverName = address.ReceiverName,
                    ReceiverPhone = address.Phone,
                    ReceiverAddress = $"{address.Province}{address.City}{address.District}{address.DetailAddress}",
                    Remark = request.Remark,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // 创建订单项
                var orderItems = cartItems.Select(c => new OrderItem
                {
                    OrderId = order.Id,
                    ProductId = c.ProductId,
                    ProductSkuId = c.ProductSkuId,
                    ProductName = c.Product.Name,
                    ProductImage = c.ProductSku?.ImageUrl ?? c.Product.MainImageUrl,
                    SkuSpecifications = c.ProductSku?.Specifications,
                    Price = c.ProductSku?.Price ?? c.Product.Price,
                    Quantity = c.Quantity,
                    TotalAmount = (c.ProductSku?.Price ?? c.Product.Price) * c.Quantity,
                    CreatedAt = DateTime.Now
                }).ToList();

                _context.OrderItems.AddRange(orderItems);

                // 删除购物车项
                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // 重新查询包含关联数据的订单
                var createdOrder = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstAsync(o => o.Id == order.Id);

                var orderDto = MapToOrderDto(createdOrder);
                return ApiResponse<OrderDto>.Ok(orderDto, "订单创建成功");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "创建订单失败");
                return ApiResponse<OrderDto>.Error("创建订单失败");
            }
        }

        public async Task<ApiResponse<PagedResponse<OrderDto>>> GetOrdersAsync(int userId, OrderListRequest request)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.OrderItems)
                    .Where(o => o.UserId == userId);

                // 状态筛选
                if (request.Status.HasValue)
                {
                    query = query.Where(o => o.Status == request.Status.Value);
                }

                // 支付状态筛选
                if (request.PayStatus.HasValue)
            {
                query = query.Where(o => o.PaymentStatus == request.PayStatus.Value);
            }

                // 订单号筛选
                if (!string.IsNullOrEmpty(request.OrderNo))
                {
                    query = query.Where(o => o.OrderNo.Contains(request.OrderNo));
                }

                // 日期筛选
                if (request.StartDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt >= request.StartDate.Value);
                }
                if (request.EndDate.HasValue)
                {
                    query = query.Where(o => o.CreatedAt <= request.EndDate.Value);
                }

                var total = await query.CountAsync();
                var orders = await query
                    .OrderByDescending(o => o.CreatedAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var orderDtos = orders.Select(o => MapToOrderDto(o)).ToList();

                var response = new PagedResponse<OrderDto>
                {
                    Items = orderDtos,
                    Total = total,
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<OrderDto>>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表失败");
                return ApiResponse<PagedResponse<OrderDto>>.Error("获取订单列表失败");
            }
        }

        public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int userId, int orderId)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    return ApiResponse<OrderDto>.Error("订单不存在");
                }

                var orderDto = MapToOrderDto(order);
                return ApiResponse<OrderDto>.Ok(orderDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败");
                return ApiResponse<OrderDto>.Error("获取订单详情失败");
            }
        }

        public async Task<ApiResponse<PayOrderResponse>> PayOrderAsync(int userId, PayOrderRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId);

                if (order == null)
                {
                    return ApiResponse<PayOrderResponse>.Error("订单不存在");
                }

                if (order.Status != 1)
                {
                    return ApiResponse<PayOrderResponse>.Error("订单状态不正确");
                }

                if (order.PaymentStatus != 0)
                {
                    return ApiResponse<PayOrderResponse>.Error("订单已支付");
                }

                // 模拟支付处理（实际项目中需要调用支付接口）
                var paymentId = Guid.NewGuid().ToString();
                var paymentUrl = $"https://pay.example.com/pay/{paymentId}";

                // 更新订单支付信息
                order.PaymentMethod = request.PayMethod;
            order.PaymentTransactionId = paymentId;
                order.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                var response = new PayOrderResponse
                {
                    PaymentId = paymentId,
                    PaymentUrl = paymentUrl,
                    PaymentParams = new Dictionary<string, object>
                    {
                        { "orderId", order.Id },
                        { "orderNo", order.OrderNo },
                        { "amount", order.PayAmount },
                        { "payMethod", request.PayMethod }
                    }
                };

                return ApiResponse<PayOrderResponse>.Ok(response, "支付信息获取成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订单支付失败");
                return ApiResponse<PayOrderResponse>.Error("订单支付失败");
            }
        }

        public async Task<ApiResponse> CancelOrderAsync(int userId, CancelOrderRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId);

                if (order == null)
                {
                    return ApiResponse.Error("订单不存在");
                }

                if (order.Status != 1)
                {
                    return ApiResponse.Error("订单状态不允许取消");
                }

                // 恢复库存
                foreach (var orderItem in order.OrderItems)
                {
                    if (orderItem.ProductSkuId.HasValue)
                    {
                        var sku = await _context.ProductSkus.FindAsync(orderItem.ProductSkuId.Value);
                        if (sku != null)
                        {
                            sku.Stock += orderItem.Quantity;
                        }
                    }
                    else
                    {
                        var product = await _context.Products.FindAsync(orderItem.ProductId);
                        if (product != null)
                        {
                            product.Stock += orderItem.Quantity;
                        }
                    }
                }

                // 更新订单状态
                order.Status = 6; // 已取消
                order.Remark = string.IsNullOrEmpty(request.Reason) ? order.Remark : $"{order.Remark}\n取消原因：{request.Reason}";
                order.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse.Ok("订单取消成功");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "取消订单失败");
                return ApiResponse.Error("取消订单失败");
            }
        }

        public async Task<ApiResponse> ConfirmReceiptAsync(int userId, ConfirmReceiptRequest request)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId && o.UserId == userId);

                if (order == null)
                {
                    return ApiResponse.Error("订单不存在");
                }

                if (order.Status != 3)
                {
                    return ApiResponse.Error("订单状态不正确");
                }

                // 更新订单状态和商品销量
                order.Status = 4; // 已完成
                order.UpdatedAt = DateTime.Now;

                foreach (var orderItem in order.OrderItems)
                {
                    var product = await _context.Products.FindAsync(orderItem.ProductId);
                    if (product != null)
                    {
                        product.SalesCount += orderItem.Quantity;
                    }

                    if (orderItem.ProductSkuId.HasValue)
                    {
                        var sku = await _context.ProductSkus.FindAsync(orderItem.ProductSkuId.Value);
                        if (sku != null)
                        {
                            sku.SalesCount += orderItem.Quantity;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return ApiResponse.Ok("确认收货成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "确认收货失败");
                return ApiResponse.Error("确认收货失败");
            }
        }

        public async Task<ApiResponse<OrderStatisticsDto>> GetOrderStatisticsAsync(int userId)
        {
            try
            {
                var statistics = new OrderStatisticsDto
                {
                    PendingPayment = await _context.Orders.CountAsync(o => o.UserId == userId && o.Status == 1),
                    PendingShipment = await _context.Orders.CountAsync(o => o.UserId == userId && o.Status == 2),
                    Shipped = await _context.Orders.CountAsync(o => o.UserId == userId && o.Status == 3),
                    Completed = await _context.Orders.CountAsync(o => o.UserId == userId && o.Status == 4),
                    Cancelled = await _context.Orders.CountAsync(o => o.UserId == userId && o.Status == 6)
                };

                return ApiResponse<OrderStatisticsDto>.Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单统计失败");
                return ApiResponse<OrderStatisticsDto>.Error("获取订单统计失败");
            }
        }

        private OrderItemDto MapToOrderItemDto(CartItem cartItem)
        {
            var price = cartItem.ProductSku?.Price ?? cartItem.Product.Price;
            var imageUrl = cartItem.ProductSku?.ImageUrl ?? cartItem.Product.MainImageUrl;
            
            string? skuSpecifications = null;
            if (cartItem.ProductSku != null && !string.IsNullOrEmpty(cartItem.ProductSku.Specifications))
            {
                try
                {
                    var specs = JsonSerializer.Deserialize<Dictionary<string, string>>(cartItem.ProductSku.Specifications);
                    if (specs != null && specs.Any())
                    {
                        skuSpecifications = string.Join(", ", specs.Select(s => $"{s.Key}: {s.Value}"));
                    }
                }
                catch
                {
                    // 忽略JSON解析错误
                }
            }

            return new OrderItemDto
            {
                ProductId = cartItem.ProductId,
                ProductSkuId = cartItem.ProductSkuId,
                ProductName = cartItem.Product.Name,
                ProductImage = imageUrl,
                SkuSpecifications = skuSpecifications,
                Price = price,
                Quantity = cartItem.Quantity,
                TotalAmount = price * cartItem.Quantity
            };
        }

        private OrderDto MapToOrderDto(Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                OrderNo = order.OrderNo,
                TotalAmount = order.TotalAmount,
                DiscountAmount = order.DiscountAmount,
                ShippingFee = order.ShippingFee,
                PayAmount = order.PayAmount,
                Status = order.Status,
                StatusText = GetOrderStatusText(order.Status),
                PayStatus = order.PaymentStatus,
                PayStatusText = GetPayStatusText(order.PaymentStatus),
                PayMethod = order.PaymentMethod,
                PayTime = order.PaymentTime,
                ReceiverName = order.ReceiverName ?? string.Empty,
                ReceiverPhone = order.ReceiverPhone ?? string.Empty,
                ReceiverAddress = order.ReceiverAddress ?? string.Empty,
                LogisticsCompany = order.ShippingCompany,
                LogisticsNo = order.ShippingNo,
                Remark = order.Remark,
                Items = order.OrderItems?.Select(i => MapToOrderItemDto(i)).ToList() ?? new List<OrderItemDto>(),
                CreatedAt = order.CreatedAt,
                UpdatedAt = order.UpdatedAt
            };
        }

        private OrderItemDto MapToOrderItemDto(OrderItem orderItem)
        {
            return new OrderItemDto
            {
                Id = orderItem.Id,
                ProductId = orderItem.ProductId,
                ProductSkuId = orderItem.ProductSkuId,
                ProductName = orderItem.ProductName,
                ProductImage = orderItem.ProductImage,
                SkuSpecifications = orderItem.SkuSpecifications,
                Price = orderItem.Price,
                Quantity = orderItem.Quantity,
                TotalAmount = orderItem.TotalAmount
            };
        }

        private string GetOrderStatusText(int status)
        {
            return status switch
            {
                1 => "待支付",
                2 => "待发货",
                3 => "已发货",
                4 => "已完成",
                5 => "已评价",
                6 => "已取消",
                _ => "未知状态"
            };
        }

        private string GetPayStatusText(int paymentStatus)
        {
            return paymentStatus switch
            {
                1 => "未支付",
                2 => "已支付",
                3 => "支付失败",
                4 => "已退款",
                _ => "未知状态"
            };
        }

        private decimal CalculateShippingFee(decimal totalAmount)
        {
            // 简单的运费计算逻辑：满99免运费，否则收取10元运费
            return totalAmount >= 99 ? 0 : 10;
        }

        private string GenerateOrderNo()
        {
            return $"WX{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        }
    }
}