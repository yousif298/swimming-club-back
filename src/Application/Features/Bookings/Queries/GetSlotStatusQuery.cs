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
            .Where(b => b.LaneId == request.LaneId && b.SlotId == request.SlotId && b.BookingDate == request.Date && !b.IsDeleted)
            .FirstOrDefaultAsync(ct);

        if (booking == null) return new SlotStatusDto(false, null);

        return new SlotStatusDto(true, new BookingDetailDto(
            booking.Id, booking.CustomerId, booking.Customer.FullName,
            booking.BookingType.ToString(), booking.Price, booking.PaymentStatus.ToString()
        ));
    }
}
