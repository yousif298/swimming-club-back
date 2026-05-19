using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Bookings.Queries;

public record GetCustomerBookingsQuery(Guid CustomerId) : IRequest<List<BookingListDto>>;

public class GetCustomerBookingsQueryHandler : IRequestHandler<GetCustomerBookingsQuery, List<BookingListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerBookingsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<BookingListDto>> Handle(GetCustomerBookingsQuery request, CancellationToken ct)
    {
        return await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Lane)
            .Include(b => b.Slot)
            .Where(b => b.CustomerId == request.CustomerId && !b.IsDeleted)
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new BookingListDto(
                b.Id, b.Customer.FullName, b.Lane.LaneNumber,
                b.Slot.DisplayTime, b.BookingDate, b.BookingType.ToString(),
                b.Price, b.PaymentStatus.ToString()
            ))
            .ToListAsync(ct);
    }
}
