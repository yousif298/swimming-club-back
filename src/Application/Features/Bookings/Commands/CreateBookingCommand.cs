using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Domain.Entities;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Bookings.Commands;

public record CreateBookingCommand(
    Guid CustomerId,
    Guid LaneId,
    Guid SlotId,
    DateTime BookingDate,
    BookingType BookingType,
    double Price,
    PaymentStatus PaymentStatus,
    Guid? CreatedByUserId,
    int? ParticipantsCount = 1
) : IRequest<Result<BookingDto>>;

public record BookingDto(
    Guid Id, Guid CustomerId, string CustomerName,
    Guid LaneId, int LaneNumber,
    Guid SlotId, string SlotTime,
    DateTime BookingDate, string BookingType,
    double Price, string PaymentStatus
);

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

        var booking = new Booking
        {
            CustomerId = request.CustomerId,
            LaneId = request.LaneId,
            SlotId = request.SlotId,
            BookingDate = request.BookingDate,
            BookingType = request.BookingType,
            Price = request.Price,
            PaymentStatus = request.PaymentStatus,
            CreatedByUserId = request.CreatedByUserId,
            ParticipantsCount = request.ParticipantsCount
        };

        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync(ct);

        var dto = await MapToDto(booking, ct);
        return Result<BookingDto>.Success(dto);
    }

    private async Task<BookingDto> MapToDto(Booking booking, CancellationToken ct)
    {
        var customer = await _context.Customers.FindAsync(new object[] { booking.CustomerId }, ct);
        var lane = await _context.Lanes.FindAsync(new object[] { booking.LaneId }, ct);
        var slot = await _context.TimeSlots.FindAsync(new object[] { booking.SlotId }, ct);
        return new BookingDto(
            booking.Id, booking.CustomerId, customer?.FullName ?? "",
            booking.LaneId, lane?.LaneNumber ?? 0,
            booking.SlotId, slot?.DisplayTime ?? "",
            booking.BookingDate, booking.BookingType.ToString(),
            booking.Price, booking.PaymentStatus.ToString()
        );
    }
}
