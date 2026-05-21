using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Members.Queries;

public record GetMembersQuery(string? Search) : IRequest<List<MemberDto>>;

public record MemberDto(Guid Id, string FullName, int? Age, string? Phone, Guid? CustomerId, string? CustomerName);

public class GetMembersQueryHandler : IRequestHandler<GetMembersQuery, List<MemberDto>>
{
    private readonly IApplicationDbContext _context;
    public GetMembersQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<MemberDto>> Handle(GetMembersQuery request, CancellationToken ct)
    {
        var query = _context.Members
            .AsNoTracking()
            .Include(m => m.Customer)
            .Where(m => !m.IsDeleted);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(m => m.FullName.ToLower().Contains(search) || (m.Phone != null && m.Phone.Contains(search)));
        }

        return await query
            .OrderBy(m => m.FullName)
            .Select(m => new MemberDto(m.Id, m.FullName, m.Age, m.Phone, m.CustomerId, m.Customer!.FullName))
            .ToListAsync(ct);
    }
}
