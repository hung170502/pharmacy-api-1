using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Models.Brand;
using Pharmacy_API.Models.Category;
using Pharmacy_API.Models.Country;
using Pharmacy_API.Models.Product;
using Pharmacy_API.Models.Unit;
using Pharmacy_API.Supports;

namespace Pharmacy_API.Context
{
    public class AccountContext(DbContextOptions<AccountContext> options) : IdentityDbContext<ApplicationUser, Role, string, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>(options), IUnitOfWork
    {
        public AccountContext() : this(new DbContextOptions<AccountContext>()) { }
        public virtual DbSet<Permission> Permissions { get; set; }
        public virtual DbSet<PolicyPermission> PolicyPermissions { get; set; }
        public virtual DbSet<Policy> Policies { get; set; }
        public virtual DbSet<RolePolicy> RolePolicies { get; set; }
        public virtual DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public virtual DbSet<UserOtp> UserOtps { get; set; } // ✅ thêm dòng này

        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Unit> Units { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RolePolicy>(entity =>
            {
                entity.HasKey(m => new { m.RoleId, m.PolicyId });
            });

            modelBuilder.Entity<PolicyPermission>(entity =>
            {
                entity.HasKey(m => new { m.PolicyId, m.PermissionId });
            });

            //modelBuilder.Entity<UserRole>(entity =>
            //{
            //    entity.HasKey(m => new { m.UserId, m.RoleId });
            //});

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Brand)
                      .WithMany(b => b.Products)
                      .HasForeignKey(p => p.BrandId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Unit)
                      .WithMany(u => u.Products)
                      .HasForeignKey(p => p.UnitId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Country)
                      .WithMany()
                      .HasForeignKey(p => p.BrandOriginId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(p => p.Manufacturer)
                      .WithMany()
                      .HasForeignKey(p => p.ManufacturerId)
                      .OnDelete(DeleteBehavior.Restrict);

            });

            // Configure Category
            //modelBuilder.Entity<Category>(entity =>
            //{
            //    entity.HasOne(c => c.Parent)
            //          .WithMany(p => p.Children)
            //          .HasForeignKey(c => c.ParentId)
            //          .OnDelete(DeleteBehavior.Restrict);
            //});
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasMany(c => c.Children)
                      .WithOne() 
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
