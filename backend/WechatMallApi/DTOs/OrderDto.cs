using System.ComponentModel.DataAnnotations;

namespace WechatMallApi.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public string OrderNo { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal PayAmount { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; } = string.Empty;
        public int PayStatus { get; set; }
        public string PayStatusText { get; set; } = string.Empty;
        public string? PayMethod { get; set; }
        public DateTime? PayTime { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string ReceiverPhone { get; set; } = string.Empty;
        public string ReceiverAddress { get; set; } = string.Empty;
        public string? LogisticsCompany { get; set; }
        public string? LogisticsNo { get; set; }
        public string? Remark { get; set; }
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OrderItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ProductSkuId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public string? SkuSpecifications { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class OrderListRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? Status { get; set; }
        public int? PayStatus { get; set; }
        public string? OrderNo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreateOrderRequest
    {
        [Required(ErrorMessage = "购物车项不能为空")]
        public List<int> CartItemIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "收货地址不能为空")]
        public int AddressId { get; set; }

        [StringLength(500, ErrorMessage = "备注长度不能超过500个字符")]
        public string? Remark { get; set; }

        public decimal? CouponAmount { get; set; }
    }

    public class OrderPreviewDto
    {
        public List<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal ShippingFee { get; set; }
        public decimal PayAmount { get; set; }
        public UserAddressDto Address { get; set; } = null!;
    }

    public class PayOrderRequest
    {
        [Required(ErrorMessage = "订单ID不能为空")]
        public int OrderId { get; set; }

        [Required(ErrorMessage = "支付方式不能为空")]
        [StringLength(50, ErrorMessage = "支付方式长度不能超过50个字符")]
        public string PayMethod { get; set; } = string.Empty;
    }

    public class PayOrderResponse
    {
        public string PaymentId { get; set; } = string.Empty;
        public string PaymentUrl { get; set; } = string.Empty;
        public Dictionary<string, object> PaymentParams { get; set; } = new Dictionary<string, object>();
    }

    public class CancelOrderRequest
    {
        [Required(ErrorMessage = "订单ID不能为空")]
        public int OrderId { get; set; }

        [StringLength(200, ErrorMessage = "取消原因长度不能超过200个字符")]
        public string? Reason { get; set; }
    }

    public class ConfirmReceiptRequest
    {
        [Required(ErrorMessage = "订单ID不能为空")]
        public int OrderId { get; set; }
    }

    public class OrderStatisticsDto
    {
        public int PendingPayment { get; set; }
        public int PendingShipment { get; set; }
        public int Shipped { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
    }
}