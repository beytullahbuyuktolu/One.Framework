namespace HexagonalArchitecture.Domain.Configurations.KeyCloak.Interfaces;
public interface IKeycloakSyncService
{
    Task SyncPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetMissingPermissionsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetExistingPermissionsAsync(CancellationToken cancellationToken = default);
}