using MediatR;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Domain.Entities;

namespace SwimmingClub.Application.Features.Pools.Commands;

public record CreatePoolCommand(
    string Name,
    int TotalLanes
) : IRequest<Result<PoolDto>>;

public record PoolDto(Guid Id, string Name, int TotalLanes);

public class CreatePoolCommandHandler : IRequestHandler<CreatePoolCommand, Result<PoolDto>>
{
    private readonly IApplicationDbContext _context;

    public CreatePoolCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PoolDto>> Handle(CreatePoolCommand request, CancellationToken ct)
    {
        var pool = new Pool { Name = request.Name, TotalLanes = request.TotalLanes };

        for (int i = 1; i <= request.TotalLanes; i++)
        {
            pool.Lanes.Add(new Lane { LaneNumber = i, PoolId = pool.Id });
        }

        _context.Pools.Add(pool);
        await _context.SaveChangesAsync(ct);

        return Result<PoolDto>.Success(new PoolDto(pool.Id, pool.Name, pool.TotalLanes));
    }
}
