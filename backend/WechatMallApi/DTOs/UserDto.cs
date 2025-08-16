using System.ComponentModel.DataAnnotations;

namespace WechatMallApi.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string OpenId { get; set; } = string.Empty;
        public string? NickName { get; set; }
        public string? AvatarUrl { get; set; }
        public int Gender { get; set; }
        public string? Country { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }


    public class LoginRequest
    {
        [Required(ErrorMessage = "微信授权码不能为空")]
        public string Code { get; set; } = string.Empty;
        
        public string? NickName { get; set; }
        public string? AvatarUrl { get; set; }
        public int Gender { get; set; }
        public string? Country { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }

    public class UpdateUserRequest
    {
        [StringLength(50, ErrorMessage = "昵称长度不能超过50个字符")]
        public string? NickName { get; set; }
        
        [StringLength(500, ErrorMessage = "头像URL长度不能超过500个字符")]
        public string? AvatarUrl { get; set; }
        
        public int? Gender { get; set; }
        
        [StringLength(20, ErrorMessage = "手机号长度不能超过20个字符")]
        [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "手机号格式不正确")]
        public string? Phone { get; set; }
        
        [StringLength(100, ErrorMessage = "邮箱长度不能超过100个字符")]
        [EmailAddress(ErrorMessage = "邮箱格式不正确")]
        public string? Email { get; set; }
    }
}