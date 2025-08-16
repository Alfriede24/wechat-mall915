using Microsoft.EntityFrameworkCore;
using WechatMallApi.Data;
using WechatMallApi.DTOs;
using WechatMallApi.Models;
using AutoMapper;
using System.Text.Json;

namespace WechatMallApi.Services
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResponse<ProductDto>>> GetProductsAsync(ProductListRequest request);
        Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id);
        Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync();
        Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id);
        Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductRequest request);
        Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductRequest request);
        Task<ApiResponse> DeleteProductAsync(int id);
        Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request);
        Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(UpdateCategoryRequest request);
        Task<ApiResponse> DeleteCategoryAsync(int id);
    }

    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            ApplicationDbContext context,
            IMapper mapper,
            ILogger<ProductService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResponse<ProductDto>>> GetProductsAsync(ProductListRequest request)
        {
            try
            {
                var query = _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Skus)
                    .Where(p => p.IsActive);

                // 分类筛选
                if (request.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId.Value);
                }

                // 关键词搜索
                if (!string.IsNullOrEmpty(request.Keyword))
                {
                    query = query.Where(p => p.Name.Contains(request.Keyword) || 
                                           (p.Description != null && p.Description.Contains(request.Keyword)));
                }

                // 价格筛选
                if (request.MinPrice.HasValue)
                {
                    query = query.Where(p => p.Price >= request.MinPrice.Value);
                }
                if (request.MaxPrice.HasValue)
                {
                    query = query.Where(p => p.Price <= request.MaxPrice.Value);
                }

                // 标签筛选
                if (request.IsRecommended.HasValue)
                {
                    query = query.Where(p => p.IsRecommended == request.IsRecommended.Value);
                }
                if (request.IsNew.HasValue)
                {
                    query = query.Where(p => p.IsNew == request.IsNew.Value);
                }
                if (request.IsHot.HasValue)
                {
                    query = query.Where(p => p.IsHot == request.IsHot.Value);
                }

                // 排序
                query = request.SortBy switch
                {
                    "price_asc" => query.OrderBy(p => p.Price),
                    "price_desc" => query.OrderByDescending(p => p.Price),
                    "sales_desc" => query.OrderByDescending(p => p.SalesCount),
                    "created_desc" => query.OrderByDescending(p => p.CreatedAt),
                    _ => query.OrderBy(p => p.SortOrder).ThenByDescending(p => p.CreatedAt)
                };

                var total = await query.CountAsync();
                var products = await query
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToListAsync();

                var productDtos = products.Select(p => MapToProductDto(p)).ToList();

                var response = new PagedResponse<ProductDto>
                {
                    Items = productDtos,
                    Total = total,
                    Page = request.Page,
                    PageSize = request.PageSize
                };

                return ApiResponse<PagedResponse<ProductDto>>.Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取商品列表失败");
                return ApiResponse<PagedResponse<ProductDto>>.Error("获取商品列表失败");
            }
        }

        public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Skus)
                    .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

                if (product == null)
                {
                    return ApiResponse<ProductDto>.Error("商品不存在");
                }

                var productDto = MapToProductDto(product);
                return ApiResponse<ProductDto>.Ok(productDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取商品详情失败");
                return ApiResponse<ProductDto>.Error("获取商品详情失败");
            }
        }

        public async Task<ApiResponse<List<CategoryDto>>> GetCategoriesAsync()
        {
            try
            {
                var categories = await _context.Categories
                    .Include(c => c.Children)
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.SortOrder)
                    .ThenBy(c => c.CreatedAt)
                    .ToListAsync();

                var categoryDtos = categories.Select(c => MapToCategoryDto(c)).ToList();
                return ApiResponse<List<CategoryDto>>.Ok(categoryDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类列表失败");
                return ApiResponse<List<CategoryDto>>.Error("获取分类列表失败");
            }
        }

        public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Children)
                    .Include(c => c.Parent)
                    .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);

                if (category == null)
                {
                    return ApiResponse<CategoryDto>.Error("分类不存在");
                }

                var categoryDto = MapToCategoryDto(category);
                return ApiResponse<CategoryDto>.Ok(categoryDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类详情失败");
                return ApiResponse<CategoryDto>.Error("获取分类详情失败");
            }
        }

        public async Task<ApiResponse<ProductDto>> CreateProductAsync(CreateProductRequest request)
        {
            try
            {
                // 验证分类是否存在
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                {
                    return ApiResponse<ProductDto>.Error("商品分类不存在");
                }

                var product = new Product
                {
                    Name = request.Name,
                    Description = request.Description,
                    DetailHtml = request.DetailHtml,
                    Price = request.Price,
                    OriginalPrice = request.OriginalPrice,
                    Stock = request.Stock,
                    Unit = request.Unit,
                    MainImageUrl = request.MainImageUrl,
                    ImageUrls = JsonSerializer.Serialize(request.ImageUrls),
                    CategoryId = request.CategoryId,
                    IsRecommended = request.IsRecommended,
                    IsNew = request.IsNew,
                    IsHot = request.IsHot,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // 创建SKU
                if (request.Skus.Any())
                {
                    var skus = request.Skus.Select(s => new ProductSku
                    {
                        ProductId = product.Id,
                        SkuCode = s.SkuCode,
                        SkuName = s.SkuName,
                        Specifications = JsonSerializer.Serialize(s.Specifications),
                        Price = s.Price,
                        OriginalPrice = s.OriginalPrice,
                        Stock = s.Stock,
                        ImageUrl = s.ImageUrl,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }).ToList();

                    _context.ProductSkus.AddRange(skus);
                    await _context.SaveChangesAsync();
                }

                // 重新查询包含关联数据的商品
                var createdProduct = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Skus)
                    .FirstAsync(p => p.Id == product.Id);

                var productDto = MapToProductDto(createdProduct);
                return ApiResponse<ProductDto>.Ok(productDto, "商品创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建商品失败");
                return ApiResponse<ProductDto>.Error("创建商品失败");
            }
        }

        public async Task<ApiResponse<ProductDto>> UpdateProductAsync(UpdateProductRequest request)
        {
            try
            {
                var product = await _context.Products
                    .Include(p => p.Skus)
                    .FirstOrDefaultAsync(p => p.Id == request.Id);

                if (product == null)
                {
                    return ApiResponse<ProductDto>.Error("商品不存在");
                }

                // 验证分类是否存在
                var category = await _context.Categories.FindAsync(request.CategoryId);
                if (category == null)
                {
                    return ApiResponse<ProductDto>.Error("商品分类不存在");
                }

                // 更新商品信息
                product.Name = request.Name;
                product.Description = request.Description;
                product.DetailHtml = request.DetailHtml;
                product.Price = request.Price;
                product.OriginalPrice = request.OriginalPrice;
                product.Stock = request.Stock;
                product.Unit = request.Unit;
                product.MainImageUrl = request.MainImageUrl;
                product.ImageUrls = JsonSerializer.Serialize(request.ImageUrls);
                product.CategoryId = request.CategoryId;
                product.IsRecommended = request.IsRecommended;
                product.IsNew = request.IsNew;
                product.IsHot = request.IsHot;
                product.UpdatedAt = DateTime.Now;

                // 删除现有SKU
                _context.ProductSkus.RemoveRange(product.Skus);

                // 创建新SKU
                if (request.Skus.Any())
                {
                    var skus = request.Skus.Select(s => new ProductSku
                    {
                        ProductId = product.Id,
                        SkuCode = s.SkuCode,
                        SkuName = s.SkuName,
                        Specifications = JsonSerializer.Serialize(s.Specifications),
                        Price = s.Price,
                        OriginalPrice = s.OriginalPrice,
                        Stock = s.Stock,
                        ImageUrl = s.ImageUrl,
                        IsActive = true,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    }).ToList();

                    _context.ProductSkus.AddRange(skus);
                }

                await _context.SaveChangesAsync();

                // 重新查询包含关联数据的商品
                var updatedProduct = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.Skus)
                    .FirstAsync(p => p.Id == product.Id);

                var productDto = MapToProductDto(updatedProduct);
                return ApiResponse<ProductDto>.Ok(productDto, "商品更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新商品失败");
                return ApiResponse<ProductDto>.Error("更新商品失败");
            }
        }

        public async Task<ApiResponse> DeleteProductAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return ApiResponse.Error("商品不存在");
                }

                product.IsActive = false;
                product.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return ApiResponse.Ok("商品删除成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除商品失败");
                return ApiResponse.Error("删除商品失败");
            }
        }

        public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryRequest request)
        {
            try
            {
                // 验证父分类是否存在
                if (request.ParentId.HasValue)
                {
                    var parentCategory = await _context.Categories.FindAsync(request.ParentId.Value);
                    if (parentCategory == null)
                    {
                        return ApiResponse<CategoryDto>.Error("父分类不存在");
                    }
                }

                var category = new Category
                {
                    Name = request.Name,
                    Description = request.Description,
                    ImageUrl = request.ImageUrl,
                    ParentId = request.ParentId,
                    SortOrder = request.SortOrder,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var categoryDto = MapToCategoryDto(category);
                return ApiResponse<CategoryDto>.Ok(categoryDto, "分类创建成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建分类失败");
                return ApiResponse<CategoryDto>.Error("创建分类失败");
            }
        }

        public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(UpdateCategoryRequest request)
        {
            try
            {
                var category = await _context.Categories.FindAsync(request.Id);
                if (category == null)
                {
                    return ApiResponse<CategoryDto>.Error("分类不存在");
                }

                // 验证父分类是否存在
                if (request.ParentId.HasValue)
                {
                    var parentCategory = await _context.Categories.FindAsync(request.ParentId.Value);
                    if (parentCategory == null)
                    {
                        return ApiResponse<CategoryDto>.Error("父分类不存在");
                    }
                    
                    // 防止循环引用
                    if (request.ParentId.Value == request.Id)
                    {
                        return ApiResponse<CategoryDto>.Error("不能将自己设为父分类");
                    }
                }

                category.Name = request.Name;
                category.Description = request.Description;
                category.ImageUrl = request.ImageUrl;
                category.ParentId = request.ParentId;
                category.SortOrder = request.SortOrder;
                category.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                var categoryDto = MapToCategoryDto(category);
                return ApiResponse<CategoryDto>.Ok(categoryDto, "分类更新成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分类失败");
                return ApiResponse<CategoryDto>.Error("更新分类失败");
            }
        }

        public async Task<ApiResponse> DeleteCategoryAsync(int id)
        {
            try
            {
                var category = await _context.Categories
                    .Include(c => c.Children)
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (category == null)
                {
                    return ApiResponse.Error("分类不存在");
                }

                // 检查是否有子分类
                if (category.Children.Any(c => c.IsActive))
                {
                    return ApiResponse.Error("该分类下还有子分类，无法删除");
                }

                // 检查是否有商品
                if (category.Products.Any(p => p.IsActive))
                {
                    return ApiResponse.Error("该分类下还有商品，无法删除");
                }

                category.IsActive = false;
                category.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();

                return ApiResponse.Ok("分类删除成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除分类失败");
                return ApiResponse.Error("删除分类失败");
            }
        }

        private ProductDto MapToProductDto(Product product)
        {
            var imageUrls = new List<string>();
            if (!string.IsNullOrEmpty(product.ImageUrls))
            {
                try
                {
                    imageUrls = JsonSerializer.Deserialize<List<string>>(product.ImageUrls) ?? new List<string>();
                }
                catch
                {
                    // 忽略JSON解析错误
                }
            }

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                Stock = product.Stock,
                SalesCount = product.SalesCount,
                Unit = product.Unit,
                MainImageUrl = product.MainImageUrl,
                ImageUrls = imageUrls,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? "",
                IsRecommended = product.IsRecommended,
                IsNew = product.IsNew,
                IsHot = product.IsHot,
                Skus = product.Skus?.Select(s => MapToProductSkuDto(s)).ToList() ?? new List<ProductSkuDto>(),
                CreatedAt = product.CreatedAt
            };
        }

        private ProductSkuDto MapToProductSkuDto(ProductSku sku)
        {
            var specifications = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(sku.Specifications))
            {
                try
                {
                    specifications = JsonSerializer.Deserialize<Dictionary<string, string>>(sku.Specifications) ?? new Dictionary<string, string>();
                }
                catch
                {
                    // 忽略JSON解析错误
                }
            }

            return new ProductSkuDto
            {
                Id = sku.Id,
                SkuCode = sku.SkuCode,
                SkuName = sku.SkuName,
                Specifications = specifications,
                Price = sku.Price,
                OriginalPrice = sku.OriginalPrice,
                Stock = sku.Stock,
                ImageUrl = sku.ImageUrl
            };
        }

        private CategoryDto MapToCategoryDto(Category category)
        {
            return new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                ImageUrl = category.ImageUrl,
                ParentId = category.ParentId,
                ParentName = category.Parent?.Name,
                SortOrder = category.SortOrder,
                Children = category.Children?.Where(c => c.IsActive).Select(c => MapToCategoryDto(c)).ToList() ?? new List<CategoryDto>(),
                ProductCount = category.Products?.Count(p => p.IsActive) ?? 0,
                CreatedAt = category.CreatedAt
            };
        }
    }
}