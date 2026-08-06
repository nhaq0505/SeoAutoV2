using Microsoft.EntityFrameworkCore;
using SeoAuto.BuildingBlocks.Domain;
using SeoAuto.IdentityService.Domain.Entities;

namespace SeoAuto.IdentityService.Infrastructure.Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasQueryFilter(e => !e.IsDeleted); // Tự động lọc bỏ các bản ghi đã xóa mềm
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplySoftDelete();
        return base.SaveChanges();
    }

    private void ApplySoftDelete()
    {
        foreach (var entry in ChangeTracker.Entries<ISoftDelete>())
        {
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Entity.IsDeleted = true;
                entry.Entity.DeletedAt = DateTime.UtcNow;
            }
        }
    }
}
