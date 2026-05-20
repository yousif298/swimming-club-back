using MediatR;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Application.Features.Pricing.Queries;
using SwimmingClub.Domain.Entities;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Pricing.Commands;

public record CreatePricingCommand(Guid ActivityId, int? MinParticipants, int? MaxParticipants, double Price, string PricingType, string? Duration) : IRequest<Result<PricingDto>>;

public class CreatePricingCommandHandler : IRequestHandler<CreatePricingCommand, Result<PricingDto>>
{
    private readonly IApplicationDbContext _context;
    public CreatePricingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PricingDto>> Handle(CreatePricingCommand request, CancellationToken ct)
    {
        var pricing = new ServicePricing
        {
            ActivityId = request.ActivityId,
            MinParticipants = request.MinParticipants,
            MaxParticipants = request.MaxParticipants,
            Price = request.Price,
            PricingType = Enum.Parse<PricingType>(request.PricingType),
            Duration = request.Duration != null ? TimeSpan.Parse(request.Duration) : null,
        };
        _context.ServicePricings.Add(pricing);
        await _context.SaveChangesAsync(ct);
        return Result<PricingDto>.Success(new PricingDto(pricing.Id, pricing.MinParticipants, pricing.MaxParticipants, pricing.Price, pricing.PricingType.ToString()));
    }
}
