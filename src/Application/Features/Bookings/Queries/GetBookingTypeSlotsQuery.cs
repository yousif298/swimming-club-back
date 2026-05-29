using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Bookings.Queries;

public record GetBookingTypeSlotsQuery(
    Guid BookingTypeId,
    DateTime Date,
    int? TargetDayOfWeek = null
) : IRequest<BookingTypeSlotsDto>;

public record BookingTypeSlotsDto(
    Guid BookingTypeId,
    string BookingTypeName,
    bool HasSchedule,
    List<SlotInfoDto> Slots
);

public record SlotInfoDto(
    Guid Id,
    string Display,
    int OrderIndex,
    string Status,
    string? BookingId,
    string? CustomerName
);

public class GetBookingTypeSlotsQueryHandler : IRequestHandler<GetBookingTypeSlotsQuery, BookingTypeSlotsDto>
{
    private readonly IApplicationDbContext _context;

    public GetBookingTypeSlotsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<BookingTypeSlotsDto> Handle(GetBookingTypeSlotsQuery request, CancellationToken ct)
    {
        var bookingType = await _context.BookingTypes
            .FirstOrDefaultAsync(bt => bt.Id == request.BookingTypeId && !bt.IsDeleted, ct);

        if (bookingType is null)
            return new BookingTypeSlotsDto(request.BookingTypeId, "Unknown", false, new List<SlotInfoDto>());

        List<Domain.Entities.TimeSlot> timeSlots;

        if (bookingType.HasSchedule)
        {
            timeSlots = await _context.TimeSlots
                .Where(ts => ts.BookingTypeId == request.BookingTypeId && !ts.IsDeleted)
                .OrderBy(t => t.OrderIndex)
                .ToListAsync(ct);

            if (timeSlots.Count == 0)
            {
                var csDay = request.TargetDayOfWeek ?? (int)request.Date.DayOfWeek;
                var schedule = await _context.CategorySchedules
                    .Where(cs => cs.BookingTypeId == request.BookingTypeId
                        && !cs.IsDeleted && cs.IsActive
                        && (int)cs.DayOfWeek == csDay)
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
                        });
                        current = current.Add(duration);
                    }
                }
            }
        }
        else
        {
            timeSlots = await _context.TimeSlots
                .Where(ts => ts.BookingTypeId == null && !ts.IsDeleted)
                .OrderBy(t => t.OrderIndex)
                .ToListAsync(ct);
        }

        var dateStart = request.Date.Date;
        var dateEnd = dateStart.AddDays(1);

        var allBookingsOnDate = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.BookingSlots)
            .Where(b => b.BookingDate >= dateStart && b.BookingDate < dateEnd && !b.IsDeleted)
            .ToListAsync(ct);

        var scheduleBookingsOnDate = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.ScheduleDays)
            .Include(b => b.BookingSlots)
            .Where(b => b.DurationMonths.HasValue && b.DurationMonths > 0
                && b.ScheduleDays.Any()
                && b.BookingDate <= request.Date
                && b.BookingDate.AddMonths(b.DurationMonths.Value) > request.Date
                && !b.IsDeleted)
            .ToListAsync(ct);

        var busySlotIds = new HashSet<Guid>();

        foreach (var b in allBookingsOnDate)
        {
            busySlotIds.Add(b.SlotId);
            foreach (var bs in b.BookingSlots)
                busySlotIds.Add(bs.SlotId);
        }

        foreach (var b in scheduleBookingsOnDate)
        {
            if (!b.ScheduleDays.Any(sd => sd.DayOfWeek == request.Date.DayOfWeek))
                continue;
            busySlotIds.Add(b.SlotId);
            foreach (var bs in b.BookingSlots)
                busySlotIds.Add(bs.SlotId);
        }

        var slotInfos = timeSlots.Select(ts =>
        {
            var isBusy = busySlotIds.Contains(ts.Id);
            var booking = allBookingsOnDate.FirstOrDefault(b =>
                b.SlotId == ts.Id || b.BookingSlots.Any(bs => bs.SlotId == ts.Id));

            return new SlotInfoDto(
                ts.Id,
                ts.DisplayTime,
                ts.OrderIndex,
                isBusy ? "reserved" : "available",
                booking?.Id.ToString(),
                booking?.Customer?.FullName
            );
        }).ToList();

        return new BookingTypeSlotsDto(
            request.BookingTypeId,
            bookingType.Name,
            bookingType.HasSchedule,
            slotInfos
        );
    }
}
