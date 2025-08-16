using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WechatMallApi.DTOs;
using WechatMallApi.Services;
using System.Security.Claims;

namespace WechatMallApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        /// <summary>
        /// 获取购物车
        /// </summary>
        /// <returns>购物车信息</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<CartSummaryDto>>> GetCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.GetCartAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取购物车失败");
                return StatusCode(500, ApiResponse<CartSummaryDto>.Error("获取购物车失败"));
            }
        }

        /// <summary>
        /// 添加商品到购物车
        /// </summary>
        /// <param name="request">添加请求</param>
        /// <returns>添加结果</returns>
        [HttpPost("add")]
        public async Task<ActionResult<ApiResponse<CartItemDto>>> AddToCart([FromBody] AddToCartRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.AddToCartAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加商品到购物车失败");
                return StatusCode(500, ApiResponse<CartItemDto>.Error("添加商品到购物车失败"));
            }
        }

        /// <summary>
        /// 更新购物车项
        /// </summary>
        /// <param name="cartItemId">购物车项ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("{cartItemId}")]
        public async Task<ActionResult<ApiResponse<CartItemDto>>> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                request.Id = cartItemId;
                var result = await _cartService.UpdateCartItemAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新购物车项失败");
                return StatusCode(500, ApiResponse<CartItemDto>.Error("更新购物车项失败"));
            }
        }

        /// <summary>
        /// 批量更新购物车项
        /// </summary>
        /// <param name="request">批量更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("batch")]
        public async Task<ActionResult<ApiResponse<List<CartItemDto>>>> BatchUpdateCartItems([FromBody] BatchUpdateCartRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.BatchUpdateCartAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批量更新购物车项失败");
                return StatusCode(500, ApiResponse<List<CartItemDto>>.Error("批量更新购物车项失败"));
            }
        }

        /// <summary>
        /// 移除购物车项
        /// </summary>
        /// <param name="request">移除请求</param>
        /// <returns>移除结果</returns>
        [HttpDelete("remove")]
        public async Task<ActionResult<ApiResponse>> RemoveCartItems([FromBody] RemoveCartItemRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.RemoveCartItemsAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除购物车项失败");
                return StatusCode(500, ApiResponse.Error("移除购物车项失败"));
            }
        }

        /// <summary>
        /// 清空购物车
        /// </summary>
        /// <returns>清空结果</returns>
        [HttpDelete("clear")]
        public async Task<ActionResult<ApiResponse>> ClearCart()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.ClearCartAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清空购物车失败");
                return StatusCode(500, ApiResponse.Error("清空购物车失败"));
            }
        }

        /// <summary>
        /// 获取购物车商品数量
        /// </summary>
        /// <returns>商品数量</returns>
        [HttpGet("count")]
        public async Task<ActionResult<ApiResponse<int>>> GetCartItemCount()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _cartService.GetCartCountAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取购物车商品数量失败");
                return StatusCode(500, ApiResponse<int>.Error("获取购物车商品数量失败"));
            }
        }

        /// <summary>
        /// 获取当前用户ID
        /// </summary>
        /// <returns>用户ID</returns>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                throw new UnauthorizedAccessException("用户未登录或Token无效");
            }
            return userId;
        }
    }
}