namespace SharedKernel.Domain;

public interface IAuditableEntity
{
    DateTime CreatedAt { get; set; }
    DateTime? LastModifiedAt { get; set; }
    string? CreatedBy { get; set; }
    string? LastModifiedBy { get; set; }
}
