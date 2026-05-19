using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;

namespace SwimmingClub.Application.Features.Pools.Queries;

public record GetPoolsQuery : IRequest<List<PoolDto>>;

public class GetPoolsQueryHandler : IRequestHandler<GetPoolsQuery, List<PoolDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPoolsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<PoolDto>> Handle(GetPoolsQuery request, CancellationToken ct)
    {
        return await _context.Pools
            .AsNoTracking()
            .Select(p => new PoolDto(p.Id, p.Name, p.TotalLanes))
            .ToListAsync(ct);
    }
}
