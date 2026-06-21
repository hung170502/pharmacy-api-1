using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Models.Account;
using Pharmacy_API.Models.Brand;
using Pharmacy_API.Models.Category;
using Pharmacy_API.Models.Country;
using Pharmacy_API.Models.Order;
using Pharmacy_API.Models.Payment;
using Pharmacy_API.Models.Product;
using Pharmacy_API.Models.Unit;
using Pharmacy_API.Models.Question;  // Thêm dòng này
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
        public virtual DbSet<UserOtp> UserOtps { get; set; }
        public virtual DbSet<PhoneOtp> PhoneOtps { get; set; }  // ← THÊM DÒNG NÀY
        public virtual DbSet<Payment> Payments { get; set; }
        public virtual DbSet<Order> Orders { get; set; }
        public virtual DbSet<OrderDetail> OrderDetails { get; set; }
        public virtual DbSet<Product> Products { get; set; }
        public virtual DbSet<Category> Categories { get; set; }
        public virtual DbSet<Brand> Brands { get; set; }
        public virtual DbSet<Country> Countries { get; set; }
        public virtual DbSet<Unit> Units { get; set; }

        // Q&A DbSets - Thêm mới
        public virtual DbSet<ProductQuestion> ProductQuestions { get; set; }
        public virtual DbSet<ProductAnswer> ProductAnswers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== RBAC Configurations =====

            // Permission
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Id).HasMaxLength(36);
            });

            // Policy
            modelBuilder.Entity<Policy>(entity =>
            {
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Id).HasMaxLength(36);
            });

            // PolicyPermission (Many-to-Many: Policy <-> Permission)
            modelBuilder.Entity<PolicyPermission>(entity =>
            {
                entity.HasKey(m => new { m.PolicyId, m.PermissionId });

                entity.HasOne(pp => pp.Policy)
                      .WithMany(p => p.PolicyPermissions)
                      .HasForeignKey(pp => pp.PolicyId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pp => pp.Permission)
                      .WithMany(p => p.PolicyPermissions)
                      .HasForeignKey(pp => pp.PermissionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // RolePolicy (Many-to-Many: Role <-> Policy)
            modelBuilder.Entity<RolePolicy>(entity =>
            {
                entity.HasKey(m => new { m.RoleId, m.PolicyId });

                entity.HasOne(rp => rp.Role)
                      .WithMany(r => r.RolePolicies)
                      .HasForeignKey(rp => rp.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Policy)
                      .WithMany(p => p.RolePolicies)
                      .HasForeignKey(rp => rp.PolicyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserRole (Many-to-Many: User <-> Role)
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(m => new { m.UserId, m.RoleId });

                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // UserRefreshToken
            modelBuilder.Entity<UserRefreshToken>(entity =>
            {
                entity.HasOne(rt => rt.User)
                      .WithMany()
                      .HasForeignKey(rt => rt.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Product Configurations =====

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

            // Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasMany(c => c.Children)
                      .WithOne()
                      .HasForeignKey(c => c.ParentId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Q&A Configurations - Thêm mới =====

            // ProductQuestion Configuration
            modelBuilder.Entity<ProductQuestion>(entity =>
            {
                entity.HasKey(e => e.QuestionId);

                entity.Property(e => e.Content)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(e => e.UserName)
                      .HasMaxLength(200);

                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAt);
            });

            // ProductAnswer Configuration
            modelBuilder.Entity<ProductAnswer>(entity =>
            {
                entity.HasKey(e => e.AnswerId);

                entity.Property(e => e.Content)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.Property(e => e.RespondentName)
                      .HasMaxLength(200);

                entity.Property(e => e.RespondentRole)
                      .HasMaxLength(50)
                      .HasDefaultValue("pharmacist");

                entity.HasIndex(e => e.QuestionId);
                entity.HasIndex(e => e.CreatedAt);

                // Relationship: Question -> Answers (One-to-Many)
                entity.HasOne(e => e.Question)
                      .WithMany(q => q.Answers)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}