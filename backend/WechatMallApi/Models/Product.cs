using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WechatMallApi.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [StringLength(2000)]
        public string? DetailHtml { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OriginalPrice { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        public int SalesCount { get; set; } = 0;

        [StringLength(100)]
        public string? Unit { get; set; }

        [StringLength(500)]
        public string? MainImageUrl { get; set; }

        [StringLength(2000)]
        public string? ImageUrls { get; set; } // JSON格式存储多张图片

        [Required]
        public int CategoryId { get; set; }

        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public bool IsRecommended { get; set; } = false;

        public bool IsNew { get; set; } = false;

        public bool IsHot { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 导航属性
        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; } = null!;

        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<ProductSku> Skus { get; set; } = new List<ProductSku>();
    }
}