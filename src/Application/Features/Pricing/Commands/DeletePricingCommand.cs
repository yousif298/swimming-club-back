using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;

namespace SwimmingClub.Application.Features.Pricing.Commands;

public record DeletePricingCommand(Guid Id) : IRequest<Result<bool>>;

public class DeletePricingCommandHandler : IRequestHandler<DeletePricingCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public DeletePricingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(DeletePricingCommand request, CancellationToken ct)
    {
        var pricing = await _context.ServicePricings.FirstOrDefaultAsync(p => p.Id == request.Id && !p.IsDeleted, ct);
        if (pricing is null)
            return Result<bool>.Failure("Pricing not found");

        pricing.IsDeleted = true;
        pricing.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
