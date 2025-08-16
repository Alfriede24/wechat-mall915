using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WechatMallApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }



        [Required]
        [StringLength(100)]
        public string OpenId { get; set; } = string.Empty;

        [StringLength(100)]
        public string? UnionId { get; set; }

        [StringLength(50)]
        public string? NickName { get; set; }

        [StringLength(500)]
        public string? AvatarUrl { get; set; }

        public int Gender { get; set; } = 0; // 0:未知, 1:男, 2:女

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(100)]
        public string? Province { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        // 导航属性
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
    }
}