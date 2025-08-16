using Microsoft.EntityFrameworkCore;
using WechatMallApi.Models;

namespace WechatMallApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // DbSet 属性
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductSku> ProductSkus { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<UserAddress> UserAddresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 配置实体关系
            
            // User 配置
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.OpenId).IsUnique();
                entity.HasIndex(e => e.Phone).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Category 配置
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Product 配置
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne(d => d.Category)
                    .WithMany(p => p.Products)
                    .HasForeignKey(d => d.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.Name);
                entity.HasIndex(e => e.CategoryId);
            });

            // ProductSku 配置
            modelBuilder.Entity<ProductSku>(entity =>
            {
                entity.HasOne(d => d.Product)
                    .WithMany(p => p.Skus)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.SkuCode).IsUnique();
            });

            // CartItem 配置
            modelBuilder.Entity<CartItem>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.ProductSku)
                    .WithMany(p => p.CartItems)
                    .HasForeignKey(d => d.ProductSkuId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => new { e.UserId, e.ProductId, e.ProductSkuId }).IsUnique();
            });

            // Order 配置
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Orders)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.OrderNo).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
            });

            // OrderItem 配置
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(d => d.Order)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ProductSku)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(d => d.ProductSkuId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // UserAddress 配置
            modelBuilder.Entity<UserAddress>(entity =>
            {
                entity.HasOne(d => d.User)
                    .WithMany(p => p.Addresses)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.UserId);
            });

            // 种子数据
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // 添加默认分类
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "数码电子", Description = "手机、电脑、数码配件等", SortOrder = 1 },
                new Category { Id = 2, Name = "服装鞋帽", Description = "男装、女装、鞋子、配饰等", SortOrder = 2 },
                new Category { Id = 3, Name = "家居生活", Description = "家具、家电、日用品等", SortOrder = 3 },
                new Category { Id = 4, Name = "美妆护肤", Description = "化妆品、护肤品、个人护理等", SortOrder = 4 },
                new Category { Id = 5, Name = "食品饮料", Description = "零食、饮料、生鲜食品等", SortOrder = 5 }
            );

            // 添加示例商品
            modelBuilder.Entity<Product>().HasData(
                new Product 
                { 
                    Id = 1, 
                    Name = "iPhone 15 Pro", 
                    Description = "苹果最新旗舰手机", 
                    Price = 7999.00m, 
                    OriginalPrice = 8999.00m,
                    Stock = 100, 
                    CategoryId = 1,
                    MainImageUrl = "/images/products/iphone15pro.jpg",
                    IsRecommended = true,
                    IsNew = true
                },
                new Product 
                { 
                    Id = 2, 
                    Name = "Nike Air Max", 
                    Description = "经典运动鞋", 
                    Price = 899.00m, 
                    Stock = 50, 
                    CategoryId = 2,
                    MainImageUrl = "/images/products/nike-airmax.jpg",
                    IsHot = true
                }
            );
        }
    }
}