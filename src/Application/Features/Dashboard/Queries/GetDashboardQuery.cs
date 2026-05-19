using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Dashboard.Queries;

public record GetDashboardQuery : IRequest<DashboardDto>;

public record DashboardDto(
    int TodayBookings,
    int ActiveCustomers,
    double TodayRevenue,
    int OccupiedLanes,
    double TotalCredits,
    List<RecentBookingDto> RecentBookings
);

public record RecentBookingDto(
    Guid Id, string CustomerName, int LaneNumber,
    string SlotTime, double Price, string Status
);

public class GetDashboardQueryHandler : IRequestHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IApplicationDbContext _context;
    private readonly DateTime today = DateTime.UtcNow.Date;

    public GetDashboardQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<DashboardDto> Handle(GetDashboardQuery request, CancellationToken ct)
    {
        var todayBookings = await _context.Bookings
            .CountAsync(b => b.BookingDate == today && !b.IsDeleted, ct);

        var activeCustomers = await _context.Customers.CountAsync(c => !c.IsDeleted, ct);

        var todayRevenue = await _context.Bookings
            .Where(b => b.BookingDate == today && !b.IsDeleted && b.PaymentStatus == PaymentStatus.Paid)
            .SumAsync(b => b.Price, ct);

        var occupiedLanes = await _context.Bookings
            .CountAsync(b => b.BookingDate == today && !b.IsDeleted, ct);

        var totalCredits = await _context.Customers
            .Where(c => c.CurrentBalance > 0)
            .SumAsync(c => c.CurrentBalance, ct);

        var recent = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Lane)
            .Include(b => b.Slot)
            .Where(b => b.BookingDate >= today.AddDays(-7) && !b.IsDeleted)
            .OrderByDescending(b => b.CreatedAt)
            .Take(10)
            .Select(b => new RecentBookingDto(
                b.Id, b.Customer.FullName, b.Lane.LaneNumber,
                b.Slot.DisplayTime, b.Price, b.PaymentStatus.ToString()
            ))
            .ToListAsync(ct);

        return new DashboardDto(todayBookings, activeCustomers, todayRevenue, occupiedLanes, totalCredits, recent);
    }
}
