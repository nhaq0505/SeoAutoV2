using Microsoft.EntityFrameworkCore;
using SeoAuto.ReportService.Domain.Entities;

namespace SeoAuto.ReportService.Infrastructure.Database;

public class ReportDbContext : DbContext
{
    public ReportDbContext(DbContextOptions<ReportDbContext> options) : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Website> Websites => Set<Website>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- 1. CẤU HÌNH BẢNG PROJECTS ---
        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.HasIndex(p => p.UserId);

            entity.HasMany(p => p.Websites)
                  .WithOne(w => w.Project)
                  .HasForeignKey(w => w.ProjectId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- 2. CẤU HÌNH BẢNG WEBSITES ---
        modelBuilder.Entity<Website>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Url).IsRequired().HasMaxLength(2048);
            entity.Property(w => w.Name).IsRequired().HasMaxLength(200);
            entity.Property(w => w.FaviconUrl).HasMaxLength(2048);
            entity.HasIndex(w => w.ProjectId);

            entity.HasMany(w => w.Reports)
                  .WithOne(r => r.Website)
                  .HasForeignKey(r => r.WebsiteId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // --- 3. CẤU HÌNH BẢNG REPORTS ---
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Url).IsRequired().HasMaxLength(2048);
            entity.Property(r => r.Strategy).IsRequired().HasMaxLength(50);
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.WebsiteId);
            entity.HasIndex(r => r.AuditRequestId);

            // Cấu hình lưu trữ kiểu JSONB trong PostgreSQL theo đặc tả SRS FR-501 & ERD
            entity.Property(r => r.PerformanceData).HasColumnType("jsonb");
            entity.Property(r => r.SeoData).HasColumnType("jsonb");
            entity.Property(r => r.AiSuggestions).HasColumnType("jsonb");
        });
    }
}
