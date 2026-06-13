// Context/AppDbContext.cs (thêm DbSet cho Question và Answer)
using Microsoft.EntityFrameworkCore;
using Pharmacy_API.Models.Product;
using Pharmacy_API.Models.Question;

namespace Pharmacy_API.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Existing DbSets...
        public DbSet<Product> Products { get; set; }

        // Q&A DbSets
        public DbSet<ProductQuestion> ProductQuestions { get; set; }
        public DbSet<ProductAnswer> ProductAnswers { get; set; }

        // Context/AppDbContext.cs
        // Context/AppDbContext.cs
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Product - THÊM TOÀN BỘ PHẦN NÀY
            modelBuilder.Entity<Product>(entity =>
            {
                // Brand relationship
                entity.HasOne(p => p.Brand)
                      .WithMany()
                      .HasForeignKey(p => p.BrandId);

                // Category relationship
                entity.HasOne(p => p.Category)
                      .WithMany()
                      .HasForeignKey(p => p.CategoryId);

                // Unit relationship
                entity.HasOne(p => p.Unit)
                      .WithMany()
                      .HasForeignKey(p => p.UnitId);

                // BrandOrigin → Country
                entity.HasOne(p => p.Country)
                      .WithMany()
                      .HasForeignKey(p => p.BrandOriginId)
                      .OnDelete(DeleteBehavior.SetNull);

                // Manufacturer → Country
                entity.HasOne(p => p.Manufacturer)
                      .WithMany()
                      .HasForeignKey(p => p.ManufacturerId)
                      .OnDelete(DeleteBehavior.SetNull);
            });

            // Configure ProductQuestion
            modelBuilder.Entity<ProductQuestion>(entity =>
            {
                entity.HasKey(e => e.QuestionId);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(500);
                entity.Property(e => e.UserName).HasMaxLength(200);
                entity.HasIndex(e => e.ProductId);
                entity.HasIndex(e => e.IsActive);
                entity.HasIndex(e => e.CreatedAt);
            });

            // Configure ProductAnswer
            modelBuilder.Entity<ProductAnswer>(entity =>
            {
                entity.HasKey(e => e.AnswerId);
                entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
                entity.Property(e => e.RespondentName).HasMaxLength(200);
                entity.Property(e => e.RespondentRole).HasMaxLength(50);
                entity.HasIndex(e => e.QuestionId);
                entity.HasIndex(e => e.CreatedAt);

                entity.HasOne(e => e.Question)
                      .WithMany(q => q.Answers)
                      .HasForeignKey(e => e.QuestionId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}