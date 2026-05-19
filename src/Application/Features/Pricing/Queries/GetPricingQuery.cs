using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Pricing.Queries;

public record GetPricingQuery(Guid ActivityId) : IRequest<List<PricingDto>>;

public record PricingDto(Guid Id, int? MinParticipants, int? MaxParticipants, double Price, string PricingType);

public class GetPricingQueryHandler : IRequestHandler<GetPricingQuery, List<PricingDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPricingQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<PricingDto>> Handle(GetPricingQuery request, CancellationToken ct)
        => await _context.ServicePricings
            .AsNoTracking()
            .Where(p => p.ActivityId == request.ActivityId)
            .Select(p => new PricingDto(p.Id, p.MinParticipants, p.MaxParticipants, p.Price, p.PricingType.ToString()))
            .ToListAsync(ct);
}
