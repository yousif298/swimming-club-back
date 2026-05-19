using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Pools.Queries;

public record GetLanesQuery(Guid PoolId) : IRequest<List<LaneDto>>;

public record LaneDto(Guid Id, int LaneNumber, Guid PoolId);

public class GetLanesQueryHandler : IRequestHandler<GetLanesQuery, List<LaneDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLanesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<LaneDto>> Handle(GetLanesQuery request, CancellationToken ct)
        => await _context.Lanes
            .AsNoTracking()
            .Where(l => l.PoolId == request.PoolId)
            .OrderBy(l => l.LaneNumber)
            .Select(l => new LaneDto(l.Id, l.LaneNumber, l.PoolId))
            .ToListAsync(ct);
}
