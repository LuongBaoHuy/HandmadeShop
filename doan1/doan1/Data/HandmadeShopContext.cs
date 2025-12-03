using Microsoft.EntityFrameworkCore;
using doan1.Models;

namespace doan1.Data
{
    public class HandmadeShopContext : DbContext
    {
        public HandmadeShopContext(DbContextOptions<HandmadeShopContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductVariation> ProductVariations { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Models.Attribute> Attributes { get; set; }
        public DbSet<AttributeOption> AttributeOptions { get; set; }
        public DbSet<ProductAttributeOption> ProductAttributeOptions { get; set; }
        public DbSet<VariationOptionLink> VariationOptionLinks { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductImage>().ToTable("ProductImages");

            // Khóa chính tổng hợp cho UserRole
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserId, ur.RoleId });

            // Cấu hình quan hệ cho UserRole
            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserRole>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ràng buộc cho User
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();
            
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Ràng buộc cho Voucher
            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Code)
                .IsUnique();

            // Ràng buộc loại giảm giá của Voucher - sử dụng ToTable để tránh obsolete
            modelBuilder.Entity<Voucher>()
                .ToTable(t => t.HasCheckConstraint("CK_DiscountType", "[DiscountType] = 'amount' OR [DiscountType] = 'percent'"));

            // Cấu hình quan hệ khóa ngoại
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            // Quan hệ ProductVariation -> Product (khai báo một lần đủ)
            // (Trước đây có 2 khai báo HasOne trùng, gây dư thừa)
            // Giữ cấu hình đầy đủ ở phía dưới với WithMany(p => p.ProductVariations)

            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Voucher)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VoucherId);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product)
                .WithMany(p => p.OrderItems)
                .HasForeignKey(oi => oi.ProductId);

            // Cấu hình quan hệ OrderItem với ProductVariation sử dụng VariantId
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.ProductVariation)
                .WithMany()
                .HasForeignKey(oi => oi.VariantId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Answer>()
                .Property(a => a.UserId)
                .HasColumnName("UserId");

            modelBuilder.Entity<Answer>()
                .Property(a => a.QuestionId)
                .HasColumnName("QuestionId");

            modelBuilder.Entity<Answer>()
                .Property(a => a.ParentAnswerId)
                .HasColumnName("ParentAnswerId");

            // Cấu hình cho bảng Attributes
            modelBuilder.Entity<Models.Attribute>()
                .HasIndex(a => a.Name)
                .IsUnique();

            // Cấu hình quan hệ AttributeOption
            modelBuilder.Entity<AttributeOption>()
                .HasOne(ao => ao.Attribute)
                .WithMany(a => a.AttributeOptions)
                .HasForeignKey(ao => ao.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ ProductAttributeOption
            modelBuilder.Entity<ProductAttributeOption>()
                .HasOne(pao => pao.Product)
                .WithMany(p => p.ProductAttributeOptions)
                .HasForeignKey(pao => pao.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProductAttributeOption>()
                .HasOne(pao => pao.AttributeOption)
                .WithMany(ao => ao.ProductAttributeOptions)
                .HasForeignKey(pao => pao.AttributeOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình quan hệ VariationOptionLink
            modelBuilder.Entity<VariationOptionLink>()
                .HasOne(vol => vol.ProductVariation)
                .WithMany(pv => pv.VariationOptionLinks)
                .HasForeignKey(vol => vol.VariationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VariationOptionLink>()
                .HasOne(vol => vol.AttributeOption)
                .WithMany(ao => ao.VariationOptionLinks)
                .HasForeignKey(vol => vol.AttributeOptionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình ProductVariation
            modelBuilder.Entity<ProductVariation>()
                .HasOne(pv => pv.Product)
                .WithMany(p => p.ProductVariations)
                .HasForeignKey(pv => pv.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Trước đây unique chỉ trên CombinationHash khiến hai sản phẩm khác nhau nhưng cùng tổ hợp option bị lỗi
            // Đổi sang unique composite: (ProductId, CombinationHash)
            modelBuilder.Entity<ProductVariation>()
                .HasIndex(pv => new { pv.ProductId, pv.CombinationHash })
                .IsUnique();

            base.OnModelCreating(modelBuilder);
        }
    }
}
