using System.ComponentModel.DataAnnotations;

namespace WechatMallApi.DTOs
{
    public class UserAddressDto
    {
        public int Id { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string DetailAddress { get; set; } = string.Empty;
        public string FullAddress => $"{Province}{City}{District}{DetailAddress}";
        public string? PostalCode { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateUserAddressRequest
    {
        [Required(ErrorMessage = "收件人姓名不能为空")]
        [StringLength(50, ErrorMessage = "收件人姓名长度不能超过50个字符")]
        public string ReceiverName { get; set; } = string.Empty;

        [Required(ErrorMessage = "联系电话不能为空")]
        [StringLength(20, ErrorMessage = "联系电话长度不能超过20个字符")]
        [RegularExpression(@"^1[3-9]\d{9}$", ErrorMessage = "手机号格式不正确")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "省份不能为空")]
        [StringLength(50, ErrorMessage = "省份长度不能超过50个字符")]
        public string Province { get; set; } = string.Empty;

        [Required(ErrorMessage = "城市不能为空")]
        [StringLength(50, ErrorMessage = "城市长度不能超过50个字符")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "区县不能为空")]
        [StringLength(50, ErrorMessage = "区县长度不能超过50个字符")]
        public string District { get; set; } = string.Empty;

        [Required(ErrorMessage = "详细地址不能为空")]
        [StringLength(200, ErrorMessage = "详细地址长度不能超过200个字符")]
        public string DetailAddress { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "邮政编码长度不能超过10个字符")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "邮政编码格式不正确")]
        public string? PostalCode { get; set; }

        public bool IsDefault { get; set; } = false;
    }

    public class UpdateUserAddressRequest : CreateUserAddressRequest
    {
        [Required(ErrorMessage = "地址ID不能为空")]
        public int Id { get; set; }
    }

    public class SetDefaultAddressRequest
    {
        [Required(ErrorMessage = "地址ID不能为空")]
        public int Id { get; set; }
    }
}