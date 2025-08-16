using System.ComponentModel.DataAnnotations;

namespace WechatMallApi.DTOs
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int SortOrder { get; set; }
        public List<CategoryDto> Children { get; set; } = new List<CategoryDto>();
        public int ProductCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "分类名称不能为空")]
        [StringLength(100, ErrorMessage = "分类名称长度不能超过100个字符")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "分类描述长度不能超过500个字符")]
        public string? Description { get; set; }

        [StringLength(500, ErrorMessage = "分类图片URL长度不能超过500个字符")]
        public string? ImageUrl { get; set; }

        public int? ParentId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "排序值不能为负数")]
        public int SortOrder { get; set; } = 0;
    }

    public class UpdateCategoryRequest : CreateCategoryRequest
    {
        [Required(ErrorMessage = "分类ID不能为空")]
        public int Id { get; set; }
    }
}