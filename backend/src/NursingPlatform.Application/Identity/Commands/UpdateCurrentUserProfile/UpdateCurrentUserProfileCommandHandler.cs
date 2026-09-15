using MediatR;
using Microsoft.EntityFrameworkCore;
using NursingPlatform.Application.Abstractions.Auth;
using NursingPlatform.Application.Abstractions.Data;

namespace NursingPlatform.Application.Identity.Commands.UpdateCurrentUserProfile;

public class UpdateCurrentUserProfileCommandHandler : IRequestHandler<UpdateCurrentUserProfileCommand, UpdateCurrentUserProfileResponse>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IApplicationDbContext _context;

    public UpdateCurrentUserProfileCommandHandler(ICurrentUserService currentUser, IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<UpdateCurrentUserProfileResponse> Handle(
        UpdateCurrentUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId.Value, cancellationToken);

        if (user is null)
            throw new KeyNotFoundException($"User with ID {_currentUser.UserId.Value} was not found.");

        user.FirstName = command.FirstName.Trim();
        user.LastName = command.LastName.Trim();

        await _context.SaveChangesAsync(cancellationToken);

        return new UpdateCurrentUserProfileResponse
        {
            Username = user.Username,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsProfileComplete = user.IsProfileComplete
        };
    }
}
