using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Users.Queries;

public record GetUsersQuery : IRequest<List<UserDto>>;

public record UserDto(Guid Id, string Username, string FullName, string Role, bool IsActive);

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IApplicationDbContext _context;
    public GetUsersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken ct)
        => await _context.Users
            .AsNoTracking()
            .Where(u => !u.IsDeleted)
            .OrderBy(u => u.FullName)
            .Select(u => new UserDto(u.Id, u.Username, u.FullName, u.Role.ToString(), !u.IsDeleted))
            .ToListAsync(ct);
}
