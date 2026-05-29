using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Bookings.Queries;

public record GetMirrorViewQuery(
    Guid PoolId,
    DateTime Date,
    Guid? BookingTypeId = null
) : IRequest<MirrorViewDto>;

public record MirrorViewDto(
    Guid PoolId,
    string PoolName,
    List<LaneMirrorDto> Lanes,
    List<TimeSlotMirrorDto> TimeSlots,
    List<MirrorSlotDto> Slots
);

public record LaneMirrorDto(Guid Id, int Number);

public record TimeSlotMirrorDto(Guid Id, string Display, int OrderIndex, int? DayOfWeek);

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

        List<Domain.Entities.TimeSlot> timeSlots;

        if (request.BookingTypeId.HasValue)
        {
            var dayOfWeek = (int)request.Date.DayOfWeek;

            timeSlots = await _context.TimeSlots
                .Where(ts => ts.BookingTypeId == request.BookingTypeId && !ts.IsDeleted && ts.DayOfWeek == dayOfWeek)
                .OrderBy(t => t.OrderIndex)
                .ToListAsync(ct);

            if (timeSlots.Count == 0)
            {
                var schedule = await _context.CategorySchedules
                    .Where(cs => cs.BookingTypeId == request.BookingTypeId
                        && !cs.IsDeleted && cs.IsActive
                        && (int)cs.DayOfWeek == dayOfWeek)
                    .FirstOrDefaultAsync(ct);

                if (schedule != null)
                {
                    var duration = TimeSpan.FromMinutes(schedule.SlotDurationMinutes);
                    var current = schedule.StartTime;
                    var index = 0;
                    while (current + duration <= schedule.EndTime)
                    {
                        timeSlots.Add(new Domain.Entities.TimeSlot
                        {
                            Id = Guid.NewGuid(),
                            StartTime = current,
                            EndTime = current + duration,
                            OrderIndex = index++,
                            DayOfWeek = dayOfWeek,
                        });
                        current = current.Add(duration);
                    }
                }
            }
        }
        else
        {
            timeSlots = await _context.TimeSlots
                .Where(ts => !ts.IsDeleted)
                .OrderBy(t => t.OrderIndex)
                .ToListAsync(ct);
        }

        var dateStart = request.Date.Date;
        var dateEnd = dateStart.AddDays(1);
        var laneIds = pool.Lanes.Select(l => l.Id).ToList();

        var exactDateBookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.BookingType)
            .Include(b => b.BookingSlots)
            .Include(b => b.BookingLanes)
            .Where(b => b.BookingDate >= dateStart && b.BookingDate < dateEnd && laneIds.Contains(b.LaneId) && !b.IsDeleted)
            .ToListAsync(ct);

        var scheduleBookings = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.BookingType)
            .Include(b => b.ScheduleDays)
            .Include(b => b.BookingSlots)
            .Include(b => b.BookingLanes)
            .Where(b => b.DurationMonths.HasValue && b.DurationMonths > 0
                && b.ScheduleDays.Any()
                && b.BookingDate <= request.Date
                && b.BookingDate.AddMonths(b.DurationMonths.Value) > request.Date
                && laneIds.Contains(b.LaneId)
                && !b.IsDeleted)
            .ToListAsync(ct);

        var lanes = pool.Lanes.Select(l => new LaneMirrorDto(l.Id, l.LaneNumber)).ToList();
        var slotDtos = timeSlots.Select(t => new TimeSlotMirrorDto(t.Id, t.DisplayTime, t.OrderIndex, t.DayOfWeek)).ToList();

        var bookingMap = new Dictionary<(Guid, Guid), Domain.Entities.Booking>();

        void AddBooking(Domain.Entities.Booking b, Guid laneId, Guid slotId)
        {
            bookingMap[(laneId, slotId)] = b;
        }

        foreach (var b in exactDateBookings)
        {
            AddBooking(b, b.LaneId, b.SlotId);
            foreach (var bs in b.BookingSlots)
                AddBooking(b, b.LaneId, bs.SlotId);
            foreach (var bl in b.BookingLanes)
            {
                AddBooking(b, bl.LaneId, b.SlotId);
                foreach (var bs in b.BookingSlots)
                    AddBooking(b, bl.LaneId, bs.SlotId);
            }
        }

        foreach (var b in scheduleBookings)
        {
            if (!b.ScheduleDays.Any(sd => sd.DayOfWeek == request.Date.DayOfWeek))
                continue;

            AddBooking(b, b.LaneId, b.SlotId);
            foreach (var bs in b.BookingSlots)
                AddBooking(b, b.LaneId, bs.SlotId);
            foreach (var bl in b.BookingLanes)
            {
                AddBooking(b, bl.LaneId, b.SlotId);
                foreach (var bs in b.BookingSlots)
                    AddBooking(b, bl.LaneId, bs.SlotId);
            }
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
