using AdministrationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.MultiTenancy;

namespace AdministrationService.Infrastructure.Persistence;

public class AdministrationDbContext : DbContext
{
    private readonly ITenantInfo _tenantInfo;

    public AdministrationDbContext(
        DbContextOptions<AdministrationDbContext> options,
        ITenantInfo tenantInfo) : base(options)
    {
        _tenantInfo = tenantInfo;
    }

    public DbSet<Permission> Permissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Permission entity
        modelBuilder.Entity<Permission>(b =>
        {
            b.ToTable("Permissions", "administration");
            b.HasKey(x => x.Id);
            b.Property(x => x.Name).IsRequired().HasMaxLength(200);
            b.Property(x => x.SystemName).IsRequired().HasMaxLength(100);
            b.Property(x => x.Description).HasMaxLength(500);
            
            // Add unique index on SystemName
            b.HasIndex(x => x.SystemName).IsUnique();
        });
    }
}
