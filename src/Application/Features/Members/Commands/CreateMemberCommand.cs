using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Application.Features.Members.Queries;
using SwimmingClub.Domain.Entities;

namespace SwimmingClub.Application.Features.Members.Commands;

public record CreateMemberCommand(string FullName, int? Age, string? Phone, Guid? CustomerId) : IRequest<Result<MemberDto>>;

public class CreateMemberCommandHandler : IRequestHandler<CreateMemberCommand, Result<MemberDto>>
{
    private readonly IApplicationDbContext _context;
    public CreateMemberCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<MemberDto>> Handle(CreateMemberCommand request, CancellationToken ct)
    {
        var member = new Member
        {
            FullName = request.FullName,
            Age = request.Age,
            Phone = request.Phone,
            CustomerId = request.CustomerId,
        };
        _context.Members.Add(member);
        await _context.SaveChangesAsync(ct);

        var customerName = request.CustomerId != null
            ? await _context.Customers.Where(c => c.Id == request.CustomerId).Select(c => c.FullName).FirstOrDefaultAsync(ct)
            : null;

        return Result<MemberDto>.Success(new MemberDto(member.Id, member.FullName, member.Age, member.Phone, member.CustomerId, customerName));
    }
}
