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
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrderController> _logger;

        public OrderController(IOrderService orderService, ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        /// <summary>
        /// 订单预览
        /// </summary>
        /// <param name="request">预览请求</param>
        /// <returns>订单预览信息</returns>
        [HttpPost("preview")]
        public async Task<ActionResult<ApiResponse<OrderPreviewDto>>> PreviewOrder([FromBody] CartCheckRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _orderService.PreviewOrderAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订单预览失败");
                return StatusCode(500, ApiResponse<OrderPreviewDto>.Error("订单预览失败"));
            }
        }

        /// <summary>
        /// 创建订单
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>订单信息</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderDto>>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _orderService.CreateOrderAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建订单失败");
                return StatusCode(500, ApiResponse<OrderDto>.Error("创建订单失败"));
            }
        }

        /// <summary>
        /// 获取订单列表
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>订单列表</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<OrderDto>>>> GetOrders([FromQuery] OrderListRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _orderService.GetOrdersAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单列表失败");
                return StatusCode(500, ApiResponse<PagedResponse<OrderDto>>.Error("获取订单列表失败"));
            }
        }

        /// <summary>
        /// 获取订单详情
        /// </summary>
        /// <param name="orderId">订单ID</param>
        /// <returns>订单详情</returns>
        [HttpGet("{orderId}")]
        public async Task<ActionResult<ApiResponse<OrderDto>>> GetOrder(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _orderService.GetOrderByIdAsync(userId, orderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单详情失败");
                return StatusCode(500, ApiResponse<OrderDto>.Error("获取订单详情失败"));
            }
        }

        /// <summary>
        /// 支付订单
        /// </summary>
        /// <param name="request">支付请求</param>
        /// <returns>支付信息</returns>
        [HttpPost("pay")]
        public async Task<ActionResult<ApiResponse<PayOrderResponse>>> PayOrder([FromBody] PayOrderRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _orderService.PayOrderAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "订单支付失败");
                return StatusCode(500, ApiResponse<PayOrderResponse>.Error("订单支付失败"));
            }
        }

        /// <summary>
        /// 取消订单
        /// </summary>
        /// <param name="request">取消请求</param>
        /// <returns>取消结果</returns>
        [HttpPost("cancel")]
        public async Task<ActionResult<ApiResponse>> CancelOrder([FromBody] CancelOrderRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _orderService.CancelOrderAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取消订单失败");
                return StatusCode(500, ApiResponse.Error("取消订单失败"));
            }
        }

        /// <summary>
        /// 确认收货
        /// </summary>
        /// <param name="request">确认收货请求</param>
        /// <returns>确认结果</returns>
        [HttpPost("confirm-receipt")]
        public async Task<ActionResult<ApiResponse>> ConfirmReceipt([FromBody] ConfirmReceiptRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _orderService.ConfirmReceiptAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "确认收货失败");
                return StatusCode(500, ApiResponse.Error("确认收货失败"));
            }
        }

        /// <summary>
        /// 获取订单统计
        /// </summary>
        /// <returns>订单统计信息</returns>
        [HttpGet("statistics")]
        public async Task<ActionResult<ApiResponse<OrderStatisticsDto>>> GetOrderStatistics()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _orderService.GetOrderStatisticsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取订单统计失败");
                return StatusCode(500, ApiResponse<OrderStatisticsDto>.Error("获取订单统计失败"));
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