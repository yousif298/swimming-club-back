using SwimmingClub.Domain.Entities;

namespace SwimmingClub.Application.Common.Interfaces;

public interface ITokenService
{
    string GenerateToken(User user);
}
