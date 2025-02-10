using SharedKernel.Domain;

namespace AdministrationService.Domain.Entities;

public class RolePermission : Entity<Guid>
{
    public Guid RoleId { get; set; }
    public Guid PermissionId { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
