using System.ComponentModel.DataAnnotations;

namespace WechatMallApi.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int Stock { get; set; }
        public int SalesCount { get; set; }
        public string? Unit { get; set; }
        public string? MainImageUrl { get; set; }
        public List<string> ImageUrls { get; set; } = new List<string>();
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public bool IsRecommended { get; set; }
        public bool IsNew { get; set; }
        public bool IsHot { get; set; }
        public List<ProductSkuDto> Skus { get; set; } = new List<ProductSkuDto>();
        public DateTime CreatedAt { get; set; }
    }

    public class ProductSkuDto
    {
        public int Id { get; set; }
        public string SkuCode { get; set; } = string.Empty;
        public string? SkuName { get; set; }
        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int Stock { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class ProductListRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int? CategoryId { get; set; }
        public string? Keyword { get; set; }
        public string? SortBy { get; set; } // price_asc, price_desc, sales_desc, created_desc
        public bool? IsRecommended { get; set; }
        public bool? IsNew { get; set; }
        public bool? IsHot { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }

    public class CreateProductRequest
    {
        [Required(ErrorMessage = "商品名称不能为空")]
        [StringLength(200, ErrorMessage = "商品名称长度不能超过200个字符")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "商品描述长度不能超过1000个字符")]
        public string? Description { get; set; }

        [StringLength(2000, ErrorMessage = "商品详情长度不能超过2000个字符")]
        public string? DetailHtml { get; set; }

        [Required(ErrorMessage = "商品价格不能为空")]
        [Range(0.01, 999999.99, ErrorMessage = "商品价格必须在0.01-999999.99之间")]
        public decimal Price { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "原价必须在0.01-999999.99之间")]
        public decimal? OriginalPrice { get; set; }

        [Required(ErrorMessage = "库存数量不能为空")]
        [Range(0, int.MaxValue, ErrorMessage = "库存数量不能为负数")]
        public int Stock { get; set; }

        [StringLength(100, ErrorMessage = "单位长度不能超过100个字符")]
        public string? Unit { get; set; }

        [StringLength(500, ErrorMessage = "主图URL长度不能超过500个字符")]
        public string? MainImageUrl { get; set; }

        public List<string> ImageUrls { get; set; } = new List<string>();

        [Required(ErrorMessage = "商品分类不能为空")]
        public int CategoryId { get; set; }

        public bool IsRecommended { get; set; } = false;
        public bool IsNew { get; set; } = false;
        public bool IsHot { get; set; } = false;

        public List<CreateProductSkuRequest> Skus { get; set; } = new List<CreateProductSkuRequest>();
    }

    public class CreateProductSkuRequest
    {
        [Required(ErrorMessage = "SKU编码不能为空")]
        [StringLength(100, ErrorMessage = "SKU编码长度不能超过100个字符")]
        public string SkuCode { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "SKU名称长度不能超过200个字符")]
        public string? SkuName { get; set; }

        public Dictionary<string, string> Specifications { get; set; } = new Dictionary<string, string>();

        [Required(ErrorMessage = "SKU价格不能为空")]
        [Range(0.01, 999999.99, ErrorMessage = "SKU价格必须在0.01-999999.99之间")]
        public decimal Price { get; set; }

        [Range(0.01, 999999.99, ErrorMessage = "原价必须在0.01-999999.99之间")]
        public decimal? OriginalPrice { get; set; }

        [Required(ErrorMessage = "SKU库存不能为空")]
        [Range(0, int.MaxValue, ErrorMessage = "SKU库存不能为负数")]
        public int Stock { get; set; }

        [StringLength(500, ErrorMessage = "SKU图片URL长度不能超过500个字符")]
        public string? ImageUrl { get; set; }
    }

    public class UpdateProductRequest : CreateProductRequest
    {
        [Required(ErrorMessage = "商品ID不能为空")]
        public int Id { get; set; }
    }
}