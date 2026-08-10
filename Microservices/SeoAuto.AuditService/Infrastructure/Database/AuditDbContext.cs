using Microsoft.EntityFrameworkCore;
using SeoAuto.AuditService.Domain.Entities;

namespace SeoAuto.AuditService.Infrastructure.Database;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<AuditRequest> AuditRequests => Set<AuditRequest>();
    public DbSet<RawMetrics> RawMetrics => Set<RawMetrics>();
    public DbSet<SeoAnalysis> SeoAnalyses => Set<SeoAnalysis>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- 1. CẤU HÌNH BẢNG AUDIT REQUEST ---
        modelBuilder.Entity<AuditRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired();

            // Ép Enum lưu vào DB dưới dạng chuỗi (String) thay vì số nguyên (0, 1, 2) cho dễ đọc
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Strategy).HasConversion<string>();

            // Quan hệ 1-1 với RawMetrics
            entity.HasOne(a => a.RawMetrics)
                  .WithOne(r => r.AuditRequest)
                  .HasForeignKey<RawMetrics>(r => r.AuditRequestId)
                  .OnDelete(DeleteBehavior.Cascade); // Xóa AuditRequest thì xóa luôn RawMetrics

            // Quan hệ 1-1 với SeoAnalysis
            entity.HasOne(a => a.SeoAnalysis)
                  .WithOne(s => s.AuditRequest)
                  .HasForeignKey<SeoAnalysis>(s => s.AuditRequestId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // --- 2. CẤU HÌNH BẢNG SEO ANALYSIS ---
        modelBuilder.Entity<SeoAnalysis>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Phép thuật ở đây: Ép 2 cột này thành kiểu JSONB của PostgreSQL
            entity.Property(e => e.OpenGraphData).HasColumnType("jsonb");
            entity.Property(e => e.StructuredData).HasColumnType("jsonb");
        });

        // Bảng RawMetrics không cần cấu hình thêm vì EF Core đủ thông minh để tự suy luận các thuộc tính còn lại.
    }
}