using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Domain.Entities;
using SwimmingClub.Domain.Enums;
using BookingTypeEntity = SwimmingClub.Domain.Entities.BookingType;

namespace SwimmingClub.Application.Features.Bookings.Commands;

public record CreateBookingMemberDto(string FullName, int? Age, string? Phone);
public record CreateBookingScheduleDayDto(int DayOfWeek, string StartTime, string EndTime);

public record CreateBookingCommand(
    Guid CustomerId,
    Guid LaneId,
    Guid SlotId,
    DateTime BookingDate,
    Guid BookingTypeId,
    double Price,
    string PaymentStatus,
    Guid? CreatedByUserId,
    string? Title,
    string? CoachName,
    int? DurationMonths,
    int? DaysPerMonth,
    List<CreateBookingMemberDto>? Members,
    List<CreateBookingScheduleDayDto>? ScheduleDays
) : IRequest<Result<BookingDto>>;

public record BookingDto(
    Guid Id, Guid CustomerId, string CustomerName,
    Guid LaneId, int LaneNumber,
    Guid SlotId, string SlotTime,
    DateTime BookingDate, string BookingTypeName,
    double Price, string PaymentStatus,
    string? Title, string? CoachName,
    int? DurationMonths, int? DaysPerMonth,
    List<BookingMemberDto>? Members,
    List<BookingScheduleDayDto>? ScheduleDays
);

public record BookingMemberDto(Guid Id, string FullName, int? Age, string? Phone);
public record BookingScheduleDayDto(Guid Id, int DayOfWeek, string StartTime, string EndTime);

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Result<BookingDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateBookingCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<BookingDto>> Handle(CreateBookingCommand request, CancellationToken ct)
    {
        var exists = await _context.Bookings.AnyAsync(b =>
            b.LaneId == request.LaneId &&
            b.SlotId == request.SlotId &&
            b.BookingDate == request.BookingDate &&
            !b.IsDeleted, ct);

        if (exists)
            return Result<BookingDto>.Failure("This slot is already booked", "SLOT_BOOKED");

        var bookingType = await _context.BookingTypes.FindAsync(new object[] { request.BookingTypeId }, ct);
        if (bookingType is null)
            return Result<BookingDto>.Failure("Booking type not found");

        var paymentStatus = Enum.Parse<PaymentStatus>(request.PaymentStatus);

        var booking = new Booking
        {
            CustomerId = request.CustomerId,
            LaneId = request.LaneId,
            SlotId = request.SlotId,
            BookingDate = request.BookingDate,
            BookingTypeId = request.BookingTypeId,
            Price = request.Price,
            PaymentStatus = paymentStatus,
            CreatedByUserId = request.CreatedByUserId,
            Title = request.Title,
            CoachName = request.CoachName,
            DurationMonths = request.DurationMonths,
            DaysPerMonth = request.DaysPerMonth,
        };

        if (request.Members != null)
        {
            foreach (var m in request.Members)
            {
                booking.Members.Add(new BookingMember
                {
                    BookingId = booking.Id,
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
                    StartTime = TimeSpan.Parse(d.StartTime),
                    EndTime = TimeSpan.Parse(d.EndTime),
                });
            }
        }

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(ct);

        // Reload with navigation properties
        var saved = await _context.Bookings
            .Include(b => b.Customer)
            .Include(b => b.Lane)
            .Include(b => b.Slot)
            .Include(b => b.Members)
            .Include(b => b.ScheduleDays)
            .FirstAsync(b => b.Id == booking.Id, ct);

        return Result<BookingDto>.Success(MapToDto(saved, bookingType));
    }

    private static BookingDto MapToDto(Booking booking, BookingTypeEntity bookingType)
    {
        return new BookingDto(
            booking.Id, booking.CustomerId, booking.Customer?.FullName ?? "",
            booking.LaneId, booking.Lane?.LaneNumber ?? 0,
            booking.SlotId, booking.Slot?.DisplayTime ?? "",
            booking.BookingDate, bookingType.Name,
            booking.Price, booking.PaymentStatus.ToString(),
            booking.Title, booking.CoachName,
            booking.DurationMonths, booking.DaysPerMonth,
            booking.Members.Select(m => new BookingMemberDto(m.Id, m.FullName, m.Age, m.Phone)).ToList(),
            booking.ScheduleDays.Select(d => new BookingScheduleDayDto(d.Id, (int)d.DayOfWeek, d.StartTime.ToString(), d.EndTime.ToString())).ToList()
        );
    }
}
