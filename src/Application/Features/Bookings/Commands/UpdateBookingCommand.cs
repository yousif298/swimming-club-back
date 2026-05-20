using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Bookings.Commands;

public record UpdateBookingCommand(
    Guid Id,
    Guid? LaneId,
    Guid? SlotId,
    double? Price,
    string? PaymentStatus,
    string? Title,
    string? CoachName,
    int? DurationMonths,
    int? DaysPerMonth
) : IRequest<Result<bool>>;

public class UpdateBookingCommandHandler : IRequestHandler<UpdateBookingCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public UpdateBookingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(UpdateBookingCommand request, CancellationToken ct)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.Id && !b.IsDeleted, ct);
        if (booking is null)
            return Result<bool>.Failure("Booking not found");

        if (request.LaneId.HasValue) booking.LaneId = request.LaneId.Value;
        if (request.SlotId.HasValue) booking.SlotId = request.SlotId.Value;
        if (request.Price.HasValue) booking.Price = request.Price.Value;
        if (request.PaymentStatus != null) booking.PaymentStatus = Enum.Parse<PaymentStatus>(request.PaymentStatus);
        if (request.Title != null) booking.Title = request.Title;
        if (request.CoachName != null) booking.CoachName = request.CoachName;
        if (request.DurationMonths.HasValue) booking.DurationMonths = request.DurationMonths;
        if (request.DaysPerMonth.HasValue) booking.DaysPerMonth = request.DaysPerMonth;

        booking.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
