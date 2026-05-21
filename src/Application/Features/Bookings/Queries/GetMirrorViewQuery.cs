using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Bookings.Queries;

public record GetMirrorViewQuery(
    Guid PoolId,
    DateTime Date
) : IRequest<MirrorViewDto>;

public record MirrorViewDto(
    Guid PoolId,
    string PoolName,
    List<LaneMirrorDto> Lanes,
    List<TimeSlotMirrorDto> TimeSlots,
    List<MirrorSlotDto> Slots
);

public record LaneMirrorDto(Guid Id, int Number);

public record TimeSlotMirrorDto(Guid Id, string Display, int OrderIndex);

public record MirrorSlotDto(
    Guid LaneId,
    Guid SlotId,
    Guid? BookingId,
    Guid? CustomerId,
    string? CustomerName,
    string? BookingType,
    string? Color,
    string Status
);

public class GetMirrorViewQueryHandler : IRequestHandler<GetMirrorViewQuery, MirrorViewDto>
{
    private readonly IApplicationDbContext _context;

    public GetMirrorViewQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<MirrorViewDto> Handle(GetMirrorViewQuery request, CancellationToken ct)
    {
        var pool = await _context.Pools
            .Include(p => p.Lanes.OrderBy(l => l.LaneNumber))
            .FirstOrDefaultAsync(p => p.Id == request.PoolId, ct)
            ?? throw new Exception("Pool not found");

        var timeSlots = await _context.TimeSlots
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(ct);

        var exactDateBookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.BookingType)
            .Include(b => b.BookingSlots)
            .Where(b => b.BookingDate == request.Date && b.Lane.PoolId == request.PoolId && !b.IsDeleted)
            .ToListAsync(ct);

        var scheduleBookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.BookingType)
            .Include(b => b.ScheduleDays)
            .Include(b => b.BookingSlots)
            .Where(b => b.DurationMonths.HasValue && b.DurationMonths > 0
                && b.ScheduleDays.Any()
                && b.BookingDate <= request.Date
                && b.BookingDate.AddMonths(b.DurationMonths.Value) > request.Date
                && b.Lane.PoolId == request.PoolId
                && !b.IsDeleted)
            .ToListAsync(ct);

        var lanes = pool.Lanes.Select(l => new LaneMirrorDto(l.Id, l.LaneNumber)).ToList();
        var slotDtos = timeSlots.Select(t => new TimeSlotMirrorDto(t.Id, t.DisplayTime, t.OrderIndex)).ToList();

        var bookingMap = new Dictionary<(Guid, Guid), Domain.Entities.Booking>();

        foreach (var b in exactDateBookings)
        {
            bookingMap[(b.LaneId, b.SlotId)] = b;
            foreach (var bs in b.BookingSlots)
                bookingMap[(b.LaneId, bs.SlotId)] = b;
        }

        foreach (var b in scheduleBookings)
        {
            if (!b.ScheduleDays.Any(sd => sd.DayOfWeek == request.Date.DayOfWeek))
                continue;

            bookingMap[(b.LaneId, b.SlotId)] = b;
            foreach (var bs in b.BookingSlots)
                bookingMap[(b.LaneId, bs.SlotId)] = b;
        }

        var slots = new List<MirrorSlotDto>();
        foreach (var lane in pool.Lanes)
        {
            foreach (var slot in timeSlots)
            {
                var key = (lane.Id, slot.Id);
                if (bookingMap.TryGetValue(key, out var booking))
                {
                    slots.Add(new MirrorSlotDto(
                        lane.Id, slot.Id,
                        booking.Id, booking.CustomerId,
                        booking.Customer.FullName, booking.BookingType.Name,
                        booking.Color,
                        booking.PaymentStatus == PaymentStatus.Pending ? "pending" : "booked"
                    ));
                }
                else
                {
                    slots.Add(new MirrorSlotDto(lane.Id, slot.Id, null, null, null, null, null, "available"));
                }
            }
        }

        return new MirrorViewDto(pool.Id, pool.Name, lanes, slotDtos, slots);
    }
}
