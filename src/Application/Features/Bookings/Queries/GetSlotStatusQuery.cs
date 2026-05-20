using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Bookings.Queries;

public record GetSlotStatusQuery(
    Guid PoolId,
    Guid LaneId,
    Guid SlotId,
    DateTime Date
) : IRequest<SlotStatusDto>;

public record SlotStatusDto(
    bool IsBooked,
    BookingDetailDto? Booking
);

public record BookingDetailDto(
    Guid BookingId,
    Guid CustomerId,
    string CustomerName,
    string BookingType,
    double Price,
    string PaymentStatus
);

public class GetSlotStatusQueryHandler : IRequestHandler<GetSlotStatusQuery, SlotStatusDto>
{
    private readonly IApplicationDbContext _context;

    public GetSlotStatusQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<SlotStatusDto> Handle(GetSlotStatusQuery request, CancellationToken ct)
    {
        var booking = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.BookingType)
            .Include(b => b.ScheduleDays)
            .Where(b => b.LaneId == request.LaneId && !b.IsDeleted
                && (b.BookingDate == request.Date
                    || (b.DurationMonths.HasValue && b.DurationMonths > 0
                        && b.ScheduleDays.Any()
                        && b.BookingDate <= request.Date
                        && b.BookingDate.AddMonths(b.DurationMonths.Value) > request.Date)))
            .FirstOrDefaultAsync(ct);

        if (booking == null) return new SlotStatusDto(false, null);

        // For schedule bookings, check if this slot matches the schedule's start time
        if (booking.DurationMonths.HasValue && booking.DurationMonths > 0 && booking.ScheduleDays.Any())
        {
            var matchingDay = booking.ScheduleDays
                .FirstOrDefault(sd => sd.DayOfWeek == request.Date.DayOfWeek);
            if (matchingDay == null) return new SlotStatusDto(false, null);

            var timeSlot = await _context.TimeSlots
                .FirstOrDefaultAsync(ts => ts.Id == request.SlotId, ct);
            if (timeSlot == null || timeSlot.StartTime != matchingDay.StartTime)
                return new SlotStatusDto(false, null);
        }

        return new SlotStatusDto(true, new BookingDetailDto(
            booking.Id, booking.CustomerId, booking.Customer.FullName,
            booking.BookingType.Name, booking.Price, booking.PaymentStatus.ToString()
        ));
    }
}
