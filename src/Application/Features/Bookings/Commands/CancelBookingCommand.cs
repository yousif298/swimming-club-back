using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;

namespace SwimmingClub.Application.Features.Bookings.Commands;

public record CancelBookingCommand(Guid Id) : IRequest<Result<bool>>;

public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public CancelBookingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(CancelBookingCommand request, CancellationToken ct)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.Id && !b.IsDeleted, ct);
        if (booking is null)
            return Result<bool>.Failure("Booking not found");

        booking.IsDeleted = true;
        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
