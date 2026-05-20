using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;

namespace SwimmingClub.Application.Features.BookingTypes.Commands;

public record DeleteBookingTypeCommand(Guid Id) : IRequest<Result<bool>>;

public class DeleteBookingTypeCommandHandler : IRequestHandler<DeleteBookingTypeCommand, Result<bool>>
{
    private readonly IApplicationDbContext _context;
    public DeleteBookingTypeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<bool>> Handle(DeleteBookingTypeCommand request, CancellationToken ct)
    {
        var entity = await _context.BookingTypes.FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, ct);
        if (entity is null)
            return Result<bool>.Failure("Booking type not found");

        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
        return Result<bool>.Success(true);
    }
}
