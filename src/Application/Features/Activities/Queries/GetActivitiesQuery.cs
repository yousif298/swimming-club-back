using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Activities.Queries;

public record GetActivitiesQuery : IRequest<List<ActivityDto>>;

public record ActivityDto(Guid Id, string Name, string? Description);

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, List<ActivityDto>>
{
    private readonly IApplicationDbContext _context;
    public GetActivitiesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<ActivityDto>> Handle(GetActivitiesQuery request, CancellationToken ct)
        => await _context.Activities
            .AsNoTracking()
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Name)
            .Select(a => new ActivityDto(a.Id, a.Name, a.Description))
            .ToListAsync(ct);
}
