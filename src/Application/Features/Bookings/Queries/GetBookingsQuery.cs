using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Bookings.Queries;

public record GetBookingsQuery(DateTime? Date) : IRequest<List<BookingListDto>>;

public record BookingListDto(
    Guid Id, string CustomerName, int LaneNumber,
    string SlotTime, DateTime BookingDate, string BookingType,
    double Price, string PaymentStatus
);

public class GetBookingsQueryHandler : IRequestHandler<GetBookingsQuery, List<BookingListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetBookingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<BookingListDto>> Handle(GetBookingsQuery request, CancellationToken ct)
    {
        var query = _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Lane)
            .Include(b => b.Slot)
            .Where(b => !b.IsDeleted);

        if (request.Date.HasValue)
            query = query.Where(b => b.BookingDate == request.Date.Value);

        return await query
            .OrderByDescending(b => b.BookingDate)
            .ThenBy(b => b.Lane.LaneNumber)
            .Select(b => new BookingListDto(
                b.Id, b.Customer.FullName, b.Lane.LaneNumber,
                b.Slot.DisplayTime, b.BookingDate, b.BookingType.ToString(),
                b.Price, b.PaymentStatus.ToString()
            ))
            .ToListAsync(ct);
    }
}
