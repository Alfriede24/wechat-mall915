using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WechatMallApi.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderNo { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(10,2)")]
        public decimal ShippingFee { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PayAmount { get; set; }

        [Required]
        public int Status { get; set; } = 0; // 0:待付款, 1:待发货, 2:待收货, 3:已完成, 4:已取消, 5:已退款

        public int PaymentStatus { get; set; } = 0; // 0:未支付, 1:已支付, 2:支付失败, 3:已退款

        [StringLength(50)]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        public string? PaymentTransactionId { get; set; }

        public DateTime? PaymentTime { get; set; }

        // 收货地址信息
        [StringLength(50)]
        public string? ReceiverName { get; set; }

        [StringLength(20)]
        public string? ReceiverPhone { get; set; }

        [StringLength(500)]
        public string? ReceiverAddress { get; set; }

        [StringLength(10)]
        public string? ReceiverPostcode { get; set; }

        // 物流信息
        [StringLength(100)]
        public string? ShippingCompany { get; set; }

        [StringLength(100)]
        public string? ShippingNo { get; set; }

        public DateTime? ShippingTime { get; set; }

        public DateTime? DeliveryTime { get; set; }

        public DateTime? FinishTime { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // 导航属性
        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}