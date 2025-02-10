using SharedKernel.Domain;

namespace AdministrationService.Domain.Entities;

public class Permission : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = null!;
    public string PermissionKey { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string SystemName { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
