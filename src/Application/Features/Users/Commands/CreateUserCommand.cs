using MediatR;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Application.Features.Users.Queries;
using SwimmingClub.Domain.Entities;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Users.Commands;

public record CreateUserCommand(string Username, string Password, string FullName, string Role) : IRequest<Result<UserDto>>;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    public CreateUserCommandHandler(IApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserDto>> Handle(CreateUserCommand request, CancellationToken ct)
    {
        if (_context.Users.Any(u => u.Username == request.Username))
            return Result<UserDto>.Failure("Username already exists");

        var user = new User
        {
            Username = request.Username,
            PasswordHash = _passwordHasher.Hash(request.Password),
            FullName = request.FullName,
            Role = Enum.Parse<UserRole>(request.Role),
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync(ct);
        return Result<UserDto>.Success(new UserDto(user.Id, user.Username, user.FullName, user.Role.ToString(), true));
    }
}
