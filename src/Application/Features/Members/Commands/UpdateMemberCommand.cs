using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;

namespace SwimmingClub.Application.Features.Members.Commands;

public record UpdateMemberCommand(Guid Id, string? FullName, int? Age, string? Phone, Guid? CustomerId) : IRequest<Result<bool>>;

public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public UpdateMemberCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(UpdateMemberCommand request, CancellationToken ct)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, ct);
        if (member is null)
            return Result<bool>.Failure("Member not found");

        if (request.FullName != null) member.FullName = request.FullName;
        if (request.Age.HasValue) member.Age = request.Age;
        if (request.Phone != null) member.Phone = request.Phone;
        if (request.CustomerId != null) member.CustomerId = request.CustomerId;
        member.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
