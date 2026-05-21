using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;

namespace SwimmingClub.Application.Features.Members.Commands;

public record DeleteMemberCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteMemberCommandHandler : IRequestHandler<DeleteMemberCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public DeleteMemberCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(DeleteMemberCommand request, CancellationToken ct)
    {
        var member = await _context.Members.FirstOrDefaultAsync(m => m.Id == request.Id && !m.IsDeleted, ct);
        if (member is null)
            return Result<bool>.Failure("Member not found");

        member.IsDeleted = true;
        member.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
