namespace NursingPlatform.Application.Authorization;

public interface IPermissionService
{
    Task<HashSet<string>> GetUserPermissionsAsync(Guid userId, CancellationToken cancellationToken = default);
}
