using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WechatMallApi.DTOs;
using WechatMallApi.Services;

namespace WechatMallApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <param name="request">查询请求</param>
        /// <returns>商品列表</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<PagedResponse<ProductDto>>>> GetProducts([FromQuery] ProductListRequest request)
        {
            try
            {
                var result = await _productService.GetProductsAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取商品列表失败");
                return StatusCode(500, ApiResponse<PagedResponse<ProductDto>>.Error("获取商品列表失败"));
            }
        }

        /// <summary>
        /// 获取商品详情
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>商品详情</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<ProductDto>>> GetProduct(int id)
        {
            try
            {
                var result = await _productService.GetProductByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取商品详情失败");
                return StatusCode(500, ApiResponse<ProductDto>.Error("获取商品详情失败"));
            }
        }

        /// <summary>
        /// 创建商品
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        [HttpPost]
        [Authorize] // 需要管理员权限，这里简化处理
        public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductRequest request)
        {
            try
            {
                var result = await _productService.CreateProductAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建商品失败");
                return StatusCode(500, ApiResponse<ProductDto>.Error("创建商品失败"));
            }
        }

        /// <summary>
        /// 更新商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("{id}")]
        [Authorize] // 需要管理员权限，这里简化处理
        public async Task<ActionResult<ApiResponse<ProductDto>>> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                request.Id = id;
                var result = await _productService.UpdateProductAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新商品失败");
                return StatusCode(500, ApiResponse<ProductDto>.Error("更新商品失败"));
            }
        }

        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="id">商品ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("{id}")]
        [Authorize] // 需要管理员权限，这里简化处理
        public async Task<ActionResult<ApiResponse>> DeleteProduct(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除商品失败");
                return StatusCode(500, ApiResponse.Error("删除商品失败"));
            }
        }

        /// <summary>
        /// 获取分类列表
        /// </summary>
        /// <returns>分类列表</returns>
        [HttpGet("categories")]
        public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
        {
            try
            {
                var result = await _productService.GetCategoriesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类列表失败");
                return StatusCode(500, ApiResponse<List<CategoryDto>>.Error("获取分类列表失败"));
            }
        }

        /// <summary>
        /// 获取分类详情
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>分类详情</returns>
        [HttpGet("categories/{id}")]
        public async Task<ActionResult<ApiResponse<CategoryDto>>> GetCategory(int id)
        {
            try
            {
                var result = await _productService.GetCategoryByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取分类详情失败");
                return StatusCode(500, ApiResponse<CategoryDto>.Error("获取分类详情失败"));
            }
        }

        /// <summary>
        /// 创建分类
        /// </summary>
        /// <param name="request">创建请求</param>
        /// <returns>创建结果</returns>
        [HttpPost("categories")]
        [Authorize] // 需要管理员权限，这里简化处理
        public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            try
            {
                var result = await _productService.CreateCategoryAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建分类失败");
                return StatusCode(500, ApiResponse<CategoryDto>.Error("创建分类失败"));
            }
        }

        /// <summary>
        /// 更新分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <param name="request">更新请求</param>
        /// <returns>更新结果</returns>
        [HttpPut("categories/{id}")]
        [Authorize] // 需要管理员权限，这里简化处理
        public async Task<ActionResult<ApiResponse<CategoryDto>>> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request)
        {
            try
            {
                request.Id = id;
                var result = await _productService.UpdateCategoryAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新分类失败");
                return StatusCode(500, ApiResponse<CategoryDto>.Error("更新分类失败"));
            }
        }

        /// <summary>
        /// 删除分类
        /// </summary>
        /// <param name="id">分类ID</param>
        /// <returns>删除结果</returns>
        [HttpDelete("categories/{id}")]
        [Authorize] // 需要管理员权限，这里简化处理
        public async Task<ActionResult<ApiResponse>> DeleteCategory(int id)
        {
            try
            {
                var result = await _productService.DeleteCategoryAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除分类失败");
                return StatusCode(500, ApiResponse.Error("删除分类失败"));
            }
        }
    }
}