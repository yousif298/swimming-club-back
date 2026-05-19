using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Pools.Queries;

public record GetPoolReportQuery(Guid PoolId, DateTime? From, DateTime? To) : IRequest<PoolReportDto>;

public record PoolReportDto(
    Guid PoolId,
    string PoolName,
    int TotalLanes,
    int TotalBookings,
    double TotalRevenue,
    double TotalCredits,
    List<PoolReportBookingDto>? Bookings
);

public record PoolReportBookingDto(
    DateTime Date, string CustomerName, int LaneNumber,
    string SlotTime, string BookingType, double Price, string PaymentStatus
);

public class GetPoolReportQueryHandler : IRequestHandler<GetPoolReportQuery, PoolReportDto>
{
    private readonly IApplicationDbContext _context;
    private readonly DateOnly today = DateOnly.FromDateTime(DateTime.UtcNow);

    public GetPoolReportQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<PoolReportDto> Handle(GetPoolReportQuery request, CancellationToken ct)
    {
        var pool = await _context.Pools.FirstOrDefaultAsync(p => p.Id == request.PoolId, ct)
            ?? throw new Exception("Pool not found");

        var from = request.From ?? today.ToDateTime(TimeOnly.MinValue);
        var to = request.To ?? today.ToDateTime(TimeOnly.MaxValue);

        var bookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Lane)
            .Include(b => b.Slot)
            .Where(b => b.Lane.PoolId == request.PoolId && b.BookingDate >= from && b.BookingDate <= to && !b.IsDeleted)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync(ct);

        var totalRevenue = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).Sum(b => b.Price);
        var totalCredits = bookings.Where(b => b.PaymentStatus == PaymentStatus.Credit).Sum(b => b.Price);

        var dtos = bookings.Select(b => new PoolReportBookingDto(
            b.BookingDate, b.Customer.FullName, b.Lane.LaneNumber,
            b.Slot.DisplayTime, b.BookingType.ToString(),
            b.Price, b.PaymentStatus.ToString()
        )).ToList();

        return new PoolReportDto(
            pool.Id, pool.Name, pool.TotalLanes,
            bookings.Count, totalRevenue, totalCredits,
            dtos
        );
    }
}
