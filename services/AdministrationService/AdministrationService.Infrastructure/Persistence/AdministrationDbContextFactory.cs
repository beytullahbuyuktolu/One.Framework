using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SharedKernel.MultiTenancy;

namespace AdministrationService.Infrastructure.Persistence;

public class AdministrationDbContextFactory : IDesignTimeDbContextFactory<AdministrationDbContext>
{
    public AdministrationDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?.Replace("{projectName}", "administration");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new Exception("Connection string 'DefaultConnection' not found.");
        }

        var tenantInfo = new DefaultTenantInfo
        {
            ConnectionString = connectionString
        };

        var optionsBuilder = new DbContextOptionsBuilder<AdministrationDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AdministrationDbContext(optionsBuilder.Options, tenantInfo);
    }
}
