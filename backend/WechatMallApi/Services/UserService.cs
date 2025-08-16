using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WechatMallApi.Data;
using WechatMallApi.DTOs;
using WechatMallApi.Models;
using AutoMapper;

namespace WechatMallApi.Services
{
    public interface IUserService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<UserDto>> GetUserInfoAsync(int userId);
        Task<ApiResponse<UserDto>> UpdateUserInfoAsync(int userId, UpdateUserRequest request);
        Task<ApiResponse<List<UserAddressDto>>> GetUserAddressesAsync(int userId);
        Task<ApiResponse<UserAddressDto>> CreateUserAddressAsync(int userId, CreateUserAddressRequest request);
        Task<ApiResponse<UserAddressDto>> UpdateUserAddressAsync(int userId, UpdateUserAddressRequest request);
        Task<ApiResponse> DeleteUserAddressAsync(int userId, int addressId);
        Task<ApiResponse> SetDefaultAddressAsync(int userId, int addressId);
    }

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ApplicationDbContext context,
            IMapper mapper,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _context = context;
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                // 模拟微信登录验证（实际项目中需要调用微信API验证code）
                var openId = $"wx_openid_{request.Code}";
                
                // 查找或创建用户
                var user = await _context.Users.FirstOrDefaultAsync(u => u.OpenId == openId);
                if (user == null)
                {
                    user = new User
                    {
                        OpenId = openId,
                        NickName = request.NickName,
                        AvatarUrl = request.AvatarUrl,
                        Gender = request.Gender,
                        Country = request.Country,
                        Province = request.Province,
                        City = request.City,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // 更新用户信息
                    user.NickName = request.NickName ?? user.NickName;
                    user.AvatarUrl = request.AvatarUrl ?? user.AvatarUrl;
                    user.Gender = request.Gender;
                    user.Country = request.Country ?? user.Country;
                    user.Province = request.Province ?? user.Province;
                    user.City = request.City ?? user.City;
                    user.UpdatedAt = DateTime.Now;
                    
                    await _context.SaveChangesAsync();
                }

                // 生成JWT Token
                var token = GenerateJwtToken(user);
                var expiresAt = DateTime.Now.AddDays(7);

                var response = new LoginResponse
                {
                    Token = token,
                    User = _mapper.Map<UserDto>(user),
                    ExpiresAt = expiresAt
                };

                return ApiResponse<LoginResponse>.Ok(response, "登录成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "用户登录失败");
                return ApiResponse<LoginResponse>.Error("登录失败，请重试");
            }
        }

        public async Task<ApiResponse<UserDto>> GetUserInfoAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.Error("用户不存在");
                }

                var userDto = _mapper.Map<UserDto>(user);
                return ApiResponse<UserDto>.Ok(userDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户信息失败");
                return ApiResponse<UserDto>.Error("获取用户信息失败");
            }
        }

        public async Task<ApiResponse<UserDto>> UpdateUserInfoAsync(int userId, UpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return ApiResponse<UserDto>.Error("用户不存在");
                }

                // 更新用户信息
                if (!string.IsNullOrEmpty(request.NickName))
                    user.NickName = request.NickName;
                if (!string.IsNullOrEmpty(request.AvatarUrl))
                    user.AvatarUrl = request.AvatarUrl;
                if (request.Gender.HasValue)
                    user.Gender = request.Gender.Value;
                if (!string.IsNullOrEmpty(request.Phone))
                    user.Phone = request.Phone;
                if (!string.IsNullOrEmpty(request.Email))
                    user.Email = request.Email;

                user.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                var userDto = _mapper.Map<UserDto>(user);
                return ApiResponse<UserDto>.Ok(userDto, "更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户信息失败");
                return ApiResponse<UserDto>.Error("更新用户信息失败");
            }
        }

        public async Task<ApiResponse<List<UserAddressDto>>> GetUserAddressesAsync(int userId)
        {
            try
            {
                var addresses = await _context.UserAddresses
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .ToListAsync();

                var addressDtos = _mapper.Map<List<UserAddressDto>>(addresses);
                return ApiResponse<List<UserAddressDto>>.Ok(addressDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取用户地址失败");
                return ApiResponse<List<UserAddressDto>>.Error("获取用户地址失败");
            }
        }

        public async Task<ApiResponse<UserAddressDto>> CreateUserAddressAsync(int userId, CreateUserAddressRequest request)
        {
            try
            {
                // 如果设置为默认地址，先取消其他默认地址
                if (request.IsDefault)
                {
                    var existingDefaultAddresses = await _context.UserAddresses
                        .Where(a => a.UserId == userId && a.IsDefault)
                        .ToListAsync();
                    
                    foreach (var addr in existingDefaultAddresses)
                    {
                        addr.IsDefault = false;
                    }
                }

                var address = new UserAddress
                {
                    UserId = userId,
                    ReceiverName = request.ReceiverName,
                    Phone = request.Phone,
                    Province = request.Province,
                    City = request.City,
                    District = request.District,
                    DetailAddress = request.DetailAddress,
    
                    IsDefault = request.IsDefault,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.UserAddresses.Add(address);
                await _context.SaveChangesAsync();

                var addressDto = _mapper.Map<UserAddressDto>(address);
                return ApiResponse<UserAddressDto>.Ok(addressDto, "地址添加成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建用户地址失败");
                return ApiResponse<UserAddressDto>.Error("创建用户地址失败");
            }
        }

        public async Task<ApiResponse<UserAddressDto>> UpdateUserAddressAsync(int userId, UpdateUserAddressRequest request)
        {
            try
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == request.Id && a.UserId == userId);
                
                if (address == null)
                {
                    return ApiResponse<UserAddressDto>.Error("地址不存在");
                }

                // 如果设置为默认地址，先取消其他默认地址
                if (request.IsDefault && !address.IsDefault)
                {
                    var existingDefaultAddresses = await _context.UserAddresses
                        .Where(a => a.UserId == userId && a.IsDefault && a.Id != request.Id)
                        .ToListAsync();
                    
                    foreach (var addr in existingDefaultAddresses)
                    {
                        addr.IsDefault = false;
                    }
                }

                // 更新地址信息
                address.ReceiverName = request.ReceiverName;
                address.Phone = request.Phone;
                address.Province = request.Province;
                address.City = request.City;
                address.District = request.District;
                address.DetailAddress = request.DetailAddress;

                address.IsDefault = request.IsDefault;
                address.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var addressDto = _mapper.Map<UserAddressDto>(address);
                return ApiResponse<UserAddressDto>.Ok(addressDto, "地址更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新用户地址失败");
                return ApiResponse<UserAddressDto>.Error("更新用户地址失败");
            }
        }

        public async Task<ApiResponse> DeleteUserAddressAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
                
                if (address == null)
                {
                    return ApiResponse.Error("地址不存在");
                }

                _context.UserAddresses.Remove(address);
                await _context.SaveChangesAsync();

                return ApiResponse.Ok("地址删除成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除用户地址失败");
                return ApiResponse.Error("删除用户地址失败");
            }
        }

        public async Task<ApiResponse> SetDefaultAddressAsync(int userId, int addressId)
        {
            try
            {
                var address = await _context.UserAddresses
                    .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
                
                if (address == null)
                {
                    return ApiResponse.Error("地址不存在");
                }

                // 取消其他默认地址
                var existingDefaultAddresses = await _context.UserAddresses
                    .Where(a => a.UserId == userId && a.IsDefault && a.Id != addressId)
                    .ToListAsync();
                
                foreach (var addr in existingDefaultAddresses)
                {
                    addr.IsDefault = false;
                }

                // 设置为默认地址
                address.IsDefault = true;
                address.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                return ApiResponse.Ok("默认地址设置成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置默认地址失败");
                return ApiResponse.Error("设置默认地址失败");
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "your-secret-key-here-must-be-at-least-32-characters-long");
            
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.NickName ?? user.OpenId),
                new Claim("openid", user.OpenId)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}