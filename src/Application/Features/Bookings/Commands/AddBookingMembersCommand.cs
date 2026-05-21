using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Application.Features.Members.Queries;
using SwimmingClub.Domain.Entities;

namespace SwimmingClub.Application.Features.Bookings.Commands;

public record AddBookingMemberItem(string FullName, int? Age, string? Phone, Guid? MemberId);

public record AddBookingMembersCommand(Guid BookingId, List<AddBookingMemberItem> Members) : IRequest<Result<bool>>;

public class AddBookingMembersCommandHandler : IRequestHandler<AddBookingMembersCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public AddBookingMembersCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(AddBookingMembersCommand request, CancellationToken ct)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == request.BookingId && !b.IsDeleted, ct);
        if (booking is null)
            return Result<bool>.Failure("Booking not found");

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

        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
