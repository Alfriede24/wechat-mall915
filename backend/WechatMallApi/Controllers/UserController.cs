using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WechatMallApi.DTOs;
using WechatMallApi.Services;
using System.Security.Claims;

namespace WechatMallApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="request">注册请求</param>
        /// <returns>注册结果</returns>


        /// <summary>
        /// 微信登录
        /// </summary>
        /// <param name="request">登录请求</param>
        /// <returns>登录响应</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _userService.LoginAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录失败");
                return StatusCode(500, ApiResponse<LoginResponse>.Error("登录失败"));
            }
        }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        /// <returns>用户信息</returns>
        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto>>> GetProfile()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userService.GetUserInfoAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败");
                return StatusCode(500, ApiResponse<UserDto>.Error("获取用户信息失败"));
            }
        }

        /// <summary>
        /// 更新用户信息
        /// </summary>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateUserRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userService.UpdateUserInfoAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户信息失败");
                return StatusCode(500, ApiResponse<UserDto>.Error("更新用户信息失败"));
            }
        }

        /// <summary>
        /// 获取用户地址列表
        /// </summary>
        /// <returns>地址列表</returns>
        [HttpGet("addresses")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<List<UserAddressDto>>>> GetAddresses()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userService.GetUserAddressesAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户地址失败");
                return StatusCode(500, ApiResponse<List<UserAddressDto>>.Error("获取用户地址失败"));
            }
        }

        /// <summary>
        /// 创建用户地址
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        [HttpPost("addresses")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserAddressDto>>> CreateAddress([FromBody] CreateUserAddressRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userService.CreateUserAddressAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户地址失败");
                return StatusCode(500, ApiResponse<UserAddressDto>.Error("创建用户地址失败"));
            }
        }

        /// <summary>
        /// 更新用户地址
        /// </summary>
        /// <param name="addressId">地址ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("addresses/{addressId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UserAddressDto>>> UpdateAddress(int addressId, [FromBody] UpdateUserAddressRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                request.Id = addressId;
                var result = await _userService.UpdateUserAddressAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户地址失败");
                return StatusCode(500, ApiResponse<UserAddressDto>.Error("更新用户地址失败"));
            }
        }

        /// <summary>
        /// 删除用户地址
        /// </summary>
        /// <param name="addressId">地址ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("addresses/{addressId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> DeleteAddress(int addressId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userService.DeleteUserAddressAsync(userId, addressId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户地址失败");
                return StatusCode(500, ApiResponse.Error("删除用户地址失败"));
            }
        }

        /// <summary>
        /// 设置默认地址
        /// </summary>
        /// <param name="request">设置请求</param>
        /// <returns>设置结果</returns>
        [HttpPost("addresses/set-default")]
        [Authorize]
        public async Task<ActionResult<ApiResponse>> SetDefaultAddress([FromBody] SetDefaultAddressRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _userService.SetDefaultAddressAsync(userId, request.Id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置默认地址失败");
                return StatusCode(500, ApiResponse.Error("设置默认地址失败"));
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