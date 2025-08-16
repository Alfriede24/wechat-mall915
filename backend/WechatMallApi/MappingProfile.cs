using AutoMapper;
using WechatMallApi.DTOs;
using WechatMallApi.Models;
using System.Text.Json;

namespace WechatMallApi
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // User mappings
            CreateMap<User, UserDto>();
            CreateMap<UpdateUserRequest, User>();

            // UserAddress mappings
            CreateMap<UserAddress, UserAddressDto>();
            CreateMap<CreateUserAddressRequest, UserAddress>();
            CreateMap<UpdateUserAddressRequest, UserAddress>();

            // Category mappings
            CreateMap<Category, CategoryDto>();
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<UpdateCategoryRequest, Category>();

            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => ParseImageUrls(src.ImageUrls)))
                .ForMember(dest => dest.Skus, opt => opt.MapFrom(src => src.Skus));

            CreateMap<CreateProductRequest, Product>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => SerializeImageUrls(src.ImageUrls)));

            CreateMap<UpdateProductRequest, Product>()
                .ForMember(dest => dest.ImageUrls, opt => opt.MapFrom(src => SerializeImageUrls(src.ImageUrls)));

            // ProductSku mappings
            CreateMap<ProductSku, ProductSkuDto>()
                .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => ParseSpecifications(src.Specifications)));

            CreateMap<CreateProductSkuRequest, ProductSku>()
                .ForMember(dest => dest.Specifications, opt => opt.MapFrom(src => SerializeSpecifications(src.Specifications)));

            // CartItem mappings
            CreateMap<CartItem, CartItemDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name))
                .ForMember(dest => dest.ProductImage, opt => opt.MapFrom(src => src.ProductSku != null ? src.ProductSku.ImageUrl : src.Product.MainImageUrl))
                .ForMember(dest => dest.SkuSpecifications, opt => opt.MapFrom(src => src.ProductSku != null ? ParseSpecifications(src.ProductSku.Specifications) : null))
                .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.ProductSku != null ? src.ProductSku.Price : src.Product.Price))
                .ForMember(dest => dest.Stock, opt => opt.MapFrom(src => src.ProductSku != null ? src.ProductSku.Stock : src.Product.Stock));
        }

        private static List<string>? ParseImageUrls(string? imageUrls)
        {
            if (string.IsNullOrEmpty(imageUrls))
                return null;

            try
            {
                return JsonSerializer.Deserialize<List<string>>(imageUrls);
            }
            catch
            {
                return null;
            }
        }

        private static string? SerializeImageUrls(List<string>? imageUrls)
        {
            if (imageUrls == null || imageUrls.Count == 0)
                return null;

            try
            {
                return JsonSerializer.Serialize(imageUrls);
            }
            catch
            {
                return null;
            }
        }

        private static Dictionary<string, string>? ParseSpecifications(string? specifications)
        {
            if (string.IsNullOrEmpty(specifications))
                return null;

            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, string>>(specifications);
            }
            catch
            {
                return null;
            }
        }

        private static string? SerializeSpecifications(Dictionary<string, string>? specifications)
        {
            if (specifications == null || specifications.Count == 0)
                return null;

            try
            {
                return JsonSerializer.Serialize(specifications);
            }
            catch
            {
                return null;
            }
        }
    }
}