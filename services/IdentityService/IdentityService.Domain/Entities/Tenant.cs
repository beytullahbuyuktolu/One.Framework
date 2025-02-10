using SharedKernel.Domain;

namespace IdentityService.Domain.Entities;

public class Tenant : Entity<Guid>
{
    public string Name { get; set; } = null!;
    public string DisplayName { get; set; } = null!;
    public string ConnectionString { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
