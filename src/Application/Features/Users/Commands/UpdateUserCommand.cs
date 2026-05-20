using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Application.Features.Users.Queries;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Users.Commands;

public record UpdateUserCommand(Guid Id, string? FullName, string? Password, string? Role, bool? IsActive) : IRequest<Result<bool>>;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    public UpdateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<bool>> Handle(UpdateUserCommand request, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == request.Id && !u.IsDeleted, ct);
        if (user is null)
            return Result<bool>.Failure("User not found");

        if (request.FullName != null) user.FullName = request.FullName;
        if (request.Password != null) user.PasswordHash = _passwordHasher.Hash(request.Password);
        if (request.Role != null) user.Role = Enum.Parse<UserRole>(request.Role);
        if (request.IsActive.HasValue) user.IsDeleted = !request.IsActive.Value;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
