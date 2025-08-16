using System.ComponentModel.DataAnnotations;

namespace WechatMallApi.DTOs
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int? ProductSkuId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public string? SkuSpecifications { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsSelected { get; set; }
        public decimal TotalAmount => Price * Quantity;
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CartSummaryDto
    {
        public List<CartItemDto> Items { get; set; } = new List<CartItemDto>();
        public int TotalCount { get; set; }
        public int SelectedCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal SelectedAmount { get; set; }
    }

    public class AddToCartRequest
    {
        [Required(ErrorMessage = "商品ID不能为空")]
        public int ProductId { get; set; }

        public int? ProductSkuId { get; set; }

        [Required(ErrorMessage = "数量不能为空")]
        [Range(1, 999, ErrorMessage = "数量必须在1-999之间")]
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequest
    {
        [Required(ErrorMessage = "购物车项ID不能为空")]
        public int Id { get; set; }

        [Range(1, 999, ErrorMessage = "数量必须在1-999之间")]
        public int? Quantity { get; set; }

        public bool? IsSelected { get; set; }
    }

    public class BatchUpdateCartRequest
    {
        public List<UpdateCartItemRequest> Items { get; set; } = new List<UpdateCartItemRequest>();
    }

    public class RemoveCartItemRequest
    {
        [Required(ErrorMessage = "购物车项ID不能为空")]
        public List<int> Ids { get; set; } = new List<int>();
    }

    public class CartCheckRequest
    {
        [Required(ErrorMessage = "购物车项ID不能为空")]
        public List<int> CartItemIds { get; set; } = new List<int>();
    }
}