using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Application.Features.Pricing.Queries;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Pricing.Commands;

public record UpdatePricingCommand(Guid Id, int? MinParticipants, int? MaxParticipants, double? Price, string? PricingType, string? Duration) : IRequest<Result<bool>>;

public class UpdatePricingCommandHandler : IRequestHandler<UpdatePricingCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public UpdatePricingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(UpdatePricingCommand request, CancellationToken ct)
    {
        var pricing = await _context.ServicePricings.FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, ct);
        if (pricing is null)
            return Result<bool>.Failure("Pricing not found");

        if (request.MinParticipants.HasValue) pricing.MinParticipants = request.MinParticipants;
        if (request.MaxParticipants.HasValue) pricing.MaxParticipants = request.MaxParticipants;
        if (request.Price.HasValue) pricing.Price = request.Price.Value;
        if (request.PricingType != null) pricing.PricingType = Enum.Parse<PricingType>(request.PricingType);
        if (request.Duration != null) pricing.Duration = TimeSpan.Parse(request.Duration);
        pricing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
