using IdentityService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using SharedKernel.MultiTenancy;

namespace IdentityService.Infrastructure.Persistence;

public class IdentityDbContext : DbContext
{
    private readonly ITenantInfo _tenantInfo;

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options, ITenantInfo? tenantInfo = default) 
        : base(options)
    {
        _tenantInfo = tenantInfo ?? new DefaultTenantInfo();
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(_tenantInfo.ConnectionString);
        }
    }
}
