using Microsoft.EntityFrameworkCore;
using WechatMallApi.Data;
using WechatMallApi.DTOs;
using WechatMallApi.Models;
using AutoMapper;
using System.Text.Json;

namespace WechatMallApi.Services
{
    public interface ICartService
    {
        Task<ApiResponse<CartSummaryDto>> GetCartAsync(int userId);
        Task<ApiResponse<CartItemDto>> AddToCartAsync(int userId, AddToCartRequest request);
        Task<ApiResponse<CartItemDto>> UpdateCartItemAsync(int userId, UpdateCartItemRequest request);
        Task<ApiResponse> BatchUpdateCartAsync(int userId, BatchUpdateCartRequest request);
        Task<ApiResponse> RemoveCartItemsAsync(int userId, RemoveCartItemRequest request);
        Task<ApiResponse> ClearCartAsync(int userId);
        Task<ApiResponse<int>> GetCartCountAsync(int userId);
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<CartService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<CartSummaryDto>> GetCartAsync(int userId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .Include(c => c.ProductSku)
                    .Where(c => c.UserId == userId)
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();

                var cartItemDtos = cartItems.Select(c => MapToCartItemDto(c)).ToList();

                var summary = new CartSummaryDto
                {
                    Items = cartItemDtos,
                    TotalCount = cartItemDtos.Sum(c => c.Quantity),
                    SelectedCount = cartItemDtos.Where(c => c.IsSelected).Sum(c => c.Quantity),
                    TotalAmount = cartItemDtos.Sum(c => c.TotalAmount),
                    SelectedAmount = cartItemDtos.Where(c => c.IsSelected).Sum(c => c.TotalAmount)
                };

                return ApiResponse<CartSummaryDto>.Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取购物车失败");
                return ApiResponse<CartSummaryDto>.Error("获取购物车失败");
            }
        }

        public async Task<ApiResponse<CartItemDto>> AddToCartAsync(int userId, AddToCartRequest request)
        {
            try
            {
                // 验证商品是否存在
                var product = await _context.Products
                    .Include(p => p.Skus)
                    .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive);

                if (product == null)
                {
                    return ApiResponse<CartItemDto>.Error("商品不存在");
                }

                ProductSku? productSku = null;
                if (request.ProductSkuId.HasValue)
                {
                    productSku = product.Skus.FirstOrDefault(s => s.Id == request.ProductSkuId.Value && s.IsActive);
                    if (productSku == null)
                    {
                        return ApiResponse<CartItemDto>.Error("商品规格不存在");
                    }
                }

                // 检查库存
                var availableStock = productSku?.Stock ?? product.Stock;
                if (availableStock < request.Quantity)
                {
                    return ApiResponse<CartItemDto>.Error("库存不足");
                }

                // 检查是否已存在相同的购物车项
                var existingCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c => c.UserId == userId && 
                                            c.ProductId == request.ProductId && 
                                            c.ProductSkuId == request.ProductSkuId);

                if (existingCartItem != null)
                {
                    // 更新数量
                    var newQuantity = existingCartItem.Quantity + request.Quantity;
                    if (availableStock < newQuantity)
                    {
                        return ApiResponse<CartItemDto>.Error("库存不足");
                    }

                    existingCartItem.Quantity = newQuantity;
                    existingCartItem.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();

                    // 重新查询包含关联数据的购物车项
                    var updatedCartItem = await _context.CartItems
                        .Include(c => c.Product)
                        .Include(c => c.ProductSku)
                        .FirstAsync(c => c.Id == existingCartItem.Id);

                    var cartItemDto = MapToCartItemDto(updatedCartItem);
                    return ApiResponse<CartItemDto>.Ok(cartItemDto, "商品已添加到购物车");
                }
                else
                {
                    // 创建新的购物车项
                    var cartItem = new CartItem
                    {
                        UserId = userId,
                        ProductId = request.ProductId,
                        ProductSkuId = request.ProductSkuId,
                        Quantity = request.Quantity,
                        IsSelected = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.CartItems.Add(cartItem);
                    await _context.SaveChangesAsync();

                    // 重新查询包含关联数据的购物车项
                    var newCartItem = await _context.CartItems
                        .Include(c => c.Product)
                        .Include(c => c.ProductSku)
                        .FirstAsync(c => c.Id == cartItem.Id);

                    var cartItemDto = MapToCartItemDto(newCartItem);
                    return ApiResponse<CartItemDto>.Ok(cartItemDto, "商品已添加到购物车");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加商品到购物车失败");
                return ApiResponse<CartItemDto>.Error("添加商品到购物车失败");
            }
        }

        public async Task<ApiResponse<CartItemDto>> UpdateCartItemAsync(int userId, UpdateCartItemRequest request)
        {
            try
            {
                var cartItem = await _context.CartItems
                    .Include(c => c.Product)
                    .Include(c => c.ProductSku)
                    .FirstOrDefaultAsync(c => c.Id == request.Id && c.UserId == userId);

                if (cartItem == null)
                {
                    return ApiResponse<CartItemDto>.Error("购物车项不存在");
                }

                // 更新数量
                if (request.Quantity.HasValue)
                {
                    var availableStock = cartItem.ProductSku?.Stock ?? cartItem.Product.Stock;
                    if (availableStock < request.Quantity.Value)
                    {
                        return ApiResponse<CartItemDto>.Error("库存不足");
                    }
                    cartItem.Quantity = request.Quantity.Value;
                }

                // 更新选中状态
                if (request.IsSelected.HasValue)
                {
                    cartItem.IsSelected = request.IsSelected.Value;
                }

                cartItem.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                var cartItemDto = MapToCartItemDto(cartItem);
                return ApiResponse<CartItemDto>.Ok(cartItemDto, "购物车更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新购物车项失败");
                return ApiResponse<CartItemDto>.Error("更新购物车项失败");
            }
        }

        public async Task<ApiResponse> BatchUpdateCartAsync(int userId, BatchUpdateCartRequest request)
        {
            try
            {
                var cartItemIds = request.Items.Select(i => i.Id).ToList();
                var cartItems = await _context.CartItems
                    .Include(c => c.Product)
                    .Include(c => c.ProductSku)
                    .Where(c => cartItemIds.Contains(c.Id) && c.UserId == userId)
                    .ToListAsync();

                foreach (var updateRequest in request.Items)
                {
                    var cartItem = cartItems.FirstOrDefault(c => c.Id == updateRequest.Id);
                    if (cartItem == null) continue;

                    // 更新数量
                    if (updateRequest.Quantity.HasValue)
                    {
                        var availableStock = cartItem.ProductSku?.Stock ?? cartItem.Product.Stock;
                        if (availableStock >= updateRequest.Quantity.Value)
                        {
                            cartItem.Quantity = updateRequest.Quantity.Value;
                        }
                    }

                    // 更新选中状态
                    if (updateRequest.IsSelected.HasValue)
                    {
                        cartItem.IsSelected = updateRequest.IsSelected.Value;
                    }

                    cartItem.UpdatedAt = DateTime.Now;
                }

                await _context.SaveChangesAsync();
                return ApiResponse.Ok("购物车批量更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新购物车失败");
                return ApiResponse.Error("批量更新购物车失败");
            }
        }

        public async Task<ApiResponse> RemoveCartItemsAsync(int userId, RemoveCartItemRequest request)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Where(c => request.Ids.Contains(c.Id) && c.UserId == userId)
                    .ToListAsync();

                if (!cartItems.Any())
                {
                    return ApiResponse.Error("购物车项不存在");
                }

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return ApiResponse.Ok("商品已从购物车移除");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除购物车项失败");
                return ApiResponse.Error("移除购物车项失败");
            }
        }

        public async Task<ApiResponse> ClearCartAsync(int userId)
        {
            try
            {
                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);
                await _context.SaveChangesAsync();

                return ApiResponse.Ok("购物车已清空");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空购物车失败");
                return ApiResponse.Error("清空购物车失败");
            }
        }

        public async Task<ApiResponse<int>> GetCartCountAsync(int userId)
        {
            try
            {
                var count = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .SumAsync(c => c.Quantity);

                return ApiResponse<int>.Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取购物车数量失败");
                return ApiResponse<int>.Error("获取购物车数量失败");
            }
        }

        private CartItemDto MapToCartItemDto(CartItem cartItem)
        {
            var price = cartItem.ProductSku?.Price ?? cartItem.Product.Price;
            var stock = cartItem.ProductSku?.Stock ?? cartItem.Product.Stock;
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

            return new CartItemDto
            {
                Id = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductSkuId = cartItem.ProductSkuId,
                ProductName = cartItem.Product.Name,
                ProductImage = imageUrl,
                SkuSpecifications = skuSpecifications,
                Price = price,
                Quantity = cartItem.Quantity,
                IsSelected = cartItem.IsSelected,
                Stock = stock,
                CreatedAt = cartItem.CreatedAt
            };
        }
    }
}