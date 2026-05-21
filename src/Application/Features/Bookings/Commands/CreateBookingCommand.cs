using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Domain.Entities;
using SwimmingClub.Domain.Enums;
using BookingTypeEntity = SwimmingClub.Domain.Entities.BookingType;

namespace SwimmingClub.Application.Features.Bookings.Commands;

public record CreateBookingMemberDto(string FullName, int? Age, string? Phone, Guid? MemberId);
public record CreateBookingScheduleDayDto(int DayOfWeek);

public record CreateBookingCommand(
    Guid CustomerId,
    Guid LaneId,
    List<Guid> SlotIds,
    DateTime BookingDate,
    Guid BookingTypeId,
    double Price,
    string PaymentStatus,
    Guid? CreatedByUserId,
    string? Title,
    string? CoachName,
    string? Color,
    int? DurationMonths,
    int? DaysPerMonth,
    List<CreateBookingMemberDto>? Members,
    List<CreateBookingScheduleDayDto>? ScheduleDays
) : IRequest<Result<BookingDto>>;

public record BookingDto(
    Guid Id, Guid CustomerId, string CustomerName,
    Guid LaneId, int LaneNumber,
    Guid SlotId, string SlotTime,
    List<string> SlotTimes,
    DateTime BookingDate, string BookingTypeName,
    double Price, string PaymentStatus,
    string? Title, string? CoachName,
    string? Color,
    int? DurationMonths, int? DaysPerMonth,
    List<BookingMemberDto>? Members,
    List<BookingScheduleDayDto>? ScheduleDays
);

public record BookingMemberDto(Guid Id, string FullName, int? Age, string? Phone, Guid? MemberId);
public record BookingScheduleDayDto(Guid Id, int DayOfWeek);

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<BookingDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateBookingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<BookingDto>> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        if (request.SlotIds == null || request.SlotIds.Count == 0)
            return Result<BookingDto>.Failure("At least one time slot must be selected");

        // Check all slots are available
        var firstSlotId = request.SlotIds[0];

        var alreadyBooked = await _context.Bookings
            .Where(b => b.BookingDate == request.BookingDate && b.LaneId == request.LaneId && !b.IsDeleted)
            .SelectMany(b => b.BookingSlots.Select(bs => bs.SlotId))
            .ToListAsync(ct);

        // Also check the primary SlotId for backward compat
        var primaryBooked = await _context.Bookings
            .Where(b => b.BookingDate == request.BookingDate && b.LaneId == request.LaneId && !b.IsDeleted)
            .Select(b => b.SlotId)
            .ToListAsync(ct);

        var allBookedSlots = alreadyBooked.Concat(primaryBooked).ToHashSet();
        var conflict = request.SlotIds.FirstOrDefault(s => allBookedSlots.Contains(s));
        if (conflict != Guid.Empty)
            return Result<BookingDto>.Failure("One of the selected slots is already booked", "SLOT_BOOKED");

        var bookingType = await _context.BookingTypes.FindAsync(new object[] { request.BookingTypeId }, ct);
        if (bookingType is null)
            return Result<BookingDto>.Failure("Booking type not found");

        var paymentStatus = Enum.Parse<PaymentStatus>(request.PaymentStatus);

        var booking = new Booking
        {
            CustomerId = request.CustomerId,
            LaneId = request.LaneId,
            SlotId = firstSlotId,
            BookingDate = request.BookingDate,
            BookingTypeId = request.BookingTypeId,
            Price = request.Price,
            PaymentStatus = paymentStatus,
            CreatedByUserId = request.CreatedByUserId,
            Title = request.Title,
            CoachName = request.CoachName,
            Color = request.Color,
            DurationMonths = request.DurationMonths,
            DaysPerMonth = request.DaysPerMonth,
        };

        // Add additional slots (skip first as it's already the primary)
        foreach (var slotId in request.SlotIds.Skip(1))
        {
            booking.BookingSlots.Add(new BookingSlot
            {
                BookingId = booking.Id,
                SlotId = slotId,
            });
        }

        if (request.Members != null)
        {
            foreach (var m in request.Members)
            {
                booking.Members.Add(new BookingMember
                {
                    BookingId = booking.Id,
                    MemberId = m.MemberId,
                    FullName = m.FullName,
                    Age = m.Age,
                    Phone = m.Phone,
                });
            }
        }

        if (request.ScheduleDays != null)
        {
            foreach (var d in request.ScheduleDays)
            {
                booking.ScheduleDays.Add(new BookingScheduleDay
                {
                    BookingId = booking.Id,
                    DayOfWeek = (DayOfWeek)d.DayOfWeek,
                });
            }
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(ct);

        var saved = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Lane)
            .Include(b => b.Slot)
            .Include(b => b.BookingSlots).ThenInclude(bs => bs.Slot)
            .Include(b => b.Members)
            .Include(b => b.ScheduleDays)
            .FirstAsync(b => b.Id == booking.Id, ct);

        return Result<BookingDto>.Success(MapToDto(saved, bookingType));
    }

    private static BookingDto MapToDto(Booking booking, BookingTypeEntity bookingType)
    {
        var allSlotTimes = new List<string> { booking.Slot?.DisplayTime ?? "" }
            .Concat(booking.BookingSlots.Select(bs => bs.Slot?.DisplayTime ?? ""))
            .Where(s => !string.IsNullOrEmpty(s))
            .Distinct()
            .ToList();

        return new BookingDto(
            booking.Id, booking.CustomerId, booking.Customer?.FullName ?? "",
            booking.LaneId, booking.Lane?.LaneNumber ?? 0,
            booking.SlotId, booking.Slot?.DisplayTime ?? "",
            allSlotTimes,
            booking.BookingDate, bookingType.Name,
            booking.Price, booking.PaymentStatus.ToString(),
            booking.Title, booking.CoachName,
            booking.Color,
            booking.DurationMonths, booking.DaysPerMonth,
            booking.Members.Select(m => new BookingMemberDto(m.Id, m.FullName, m.Age, m.Phone, m.MemberId)).ToList(),
            booking.ScheduleDays.Select(d => new BookingScheduleDayDto(d.Id, (int)d.DayOfWeek)).ToList()
        );
    }
}
