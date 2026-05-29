using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Application.Features.CategorySchedules.Queries;
using SwimmingClub.Domain.Entities;

namespace SwimmingClub.Application.Features.CategorySchedules.Commands;

public record ScheduleDayConfig(
    int DayOfWeek,
    string StartTime,
    string EndTime,
    int SlotDurationMinutes,
    bool IsActive,
    List<string>? EnabledSlots = null
);

public record UpdateCategoryScheduleCommand(
    Guid BookingTypeId,
    List<ScheduleDayConfig> Days
) : IRequest<Result<List<CategoryScheduleDto>>>;

public class UpdateCategoryScheduleCommandHandler : IRequestHandler<UpdateCategoryScheduleCommand, Result<List<CategoryScheduleDto>>>
{
    private readonly IApplicationDbContext _context;

    public UpdateCategoryScheduleCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<List<CategoryScheduleDto>>> Handle(UpdateCategoryScheduleCommand request, CancellationToken ct)
    {
        var bookingType = await _context.BookingTypes.FindAsync(new object[] { request.BookingTypeId }, ct);
        if (bookingType is null)
            return Result<List<CategoryScheduleDto>>.Failure("Booking type not found");

        var existing = await _context.CategorySchedules
            .Where(cs => cs.BookingTypeId == request.BookingTypeId && !cs.IsDeleted)
            .ToListAsync(ct);

        foreach (var day in request.Days)
        {
            var schedule = existing.FirstOrDefault(e => (int)e.DayOfWeek == day.DayOfWeek);
            if (schedule != null)
            {
                schedule.StartTime = TimeSpan.Parse(day.StartTime);
                schedule.EndTime = TimeSpan.Parse(day.EndTime);
                schedule.SlotDurationMinutes = day.SlotDurationMinutes;
                schedule.IsActive = day.IsActive;
                schedule.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                _context.CategorySchedules.Add(new CategorySchedule
                {
                    BookingTypeId = request.BookingTypeId,
                    DayOfWeek = (DayOfWeek)day.DayOfWeek,
                    StartTime = TimeSpan.Parse(day.StartTime),
                    EndTime = TimeSpan.Parse(day.EndTime),
                    SlotDurationMinutes = day.SlotDurationMinutes,
                    IsActive = day.IsActive,
                });
            }
        }

        var toDelete = existing.Where(e => !request.Days.Any(d => d.DayOfWeek == (int)e.DayOfWeek)).ToList();
        foreach (var d in toDelete)
        {
            d.IsDeleted = true;
            d.UpdatedAt = DateTime.UtcNow;
        }

        await GenerateTimeSlotsForBookingType(request.BookingTypeId, request.Days, ct);
        await _context.SaveChangesAsync(ct);

        var result = await _context.CategorySchedules
            .AsNoTracking()
            .Where(cs => cs.BookingTypeId == request.BookingTypeId && !cs.IsDeleted)
            .OrderBy(cs => cs.DayOfWeek)
            .Select(cs => new CategoryScheduleDto(
                cs.Id, cs.BookingTypeId, (int)cs.DayOfWeek,
                cs.StartTime.ToString(@"hh\:mm"), cs.EndTime.ToString(@"hh\:mm"),
                cs.SlotDurationMinutes, cs.IsActive
            ))
            .ToListAsync(ct);

        return Result<List<CategoryScheduleDto>>.Success(result);
    }

    private async Task GenerateTimeSlotsForBookingType(Guid bookingTypeId, List<ScheduleDayConfig> dayConfigs, CancellationToken ct)
    {
        var existingSlots = await _context.TimeSlots
            .Where(ts => ts.BookingTypeId == bookingTypeId && !ts.IsDeleted)
            .ToListAsync(ct);

        var orderIndex = await _context.TimeSlots
            .Where(ts => ts.BookingTypeId == null && !ts.IsDeleted)
            .MaxAsync(ts => (int?)ts.OrderIndex) ?? 0;

        foreach (var slot in existingSlots)
        {
            slot.IsDeleted = true;
            slot.UpdatedAt = DateTime.UtcNow;
        }

        foreach (var dayConfig in dayConfigs)
        {
            if (!dayConfig.IsActive) continue;

            var duration = TimeSpan.FromMinutes(dayConfig.SlotDurationMinutes);
            var start = TimeSpan.Parse(dayConfig.StartTime);
            var end = TimeSpan.Parse(dayConfig.EndTime);
            var current = start;

            while (current + duration <= end)
            {
                var slotKey = $"{current:hh\\:mm}-{(current + duration):hh\\:mm}";

                if (dayConfig.EnabledSlots is { Count: > 0 } && !dayConfig.EnabledSlots.Contains(slotKey))
                {
                    current = current.Add(duration);
                    continue;
                }

                orderIndex++;
                var existing = existingSlots.FirstOrDefault(e =>
                    e.StartTime == current &&
                    e.EndTime == current + duration &&
                    e.BookingTypeId == bookingTypeId &&
                    e.DayOfWeek == dayConfig.DayOfWeek);

                if (existing != null)
                {
                    existing.IsDeleted = false;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.TimeSlots.Add(new TimeSlot
                    {
                        StartTime = current,
                        EndTime = current + duration,
                        OrderIndex = orderIndex,
                        BookingTypeId = bookingTypeId,
                        DayOfWeek = dayConfig.DayOfWeek,
                    });
                }

                current = current.Add(duration);
            }
        }
    }
}
