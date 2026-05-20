using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Application.Features.BookingTypes.Queries;

namespace SwimmingClub.Application.Features.BookingTypes.Commands;

public record UpdateBookingTypeCommand(Guid Id, string Name, string? Description, double DefaultPrice, bool HasCapacity, int? Capacity, bool HasSchedule, bool IsActive) : IRequest<Result<BookingTypeDto>>;

public class UpdateBookingTypeCommandHandler : IRequestHandler<UpdateBookingTypeCommand, Result<BookingTypeDto>>
{
    private readonly IApplicationDbContext _context;
    public UpdateBookingTypeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<BookingTypeDto>> Handle(UpdateBookingTypeCommand request, CancellationToken ct)
    {
        var entity = await _context.BookingTypes.FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, ct);
        if (entity is null)
            return Result<BookingTypeDto>.Failure("Booking type not found");

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.DefaultPrice = request.DefaultPrice;
        entity.HasCapacity = request.HasCapacity;
        entity.Capacity = request.Capacity;
        entity.HasSchedule = request.HasSchedule;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(ct);
        return Result<BookingTypeDto>.Success(new BookingTypeDto(entity.Id, entity.Name, entity.Description, entity.DefaultPrice, entity.HasCapacity, entity.Capacity, entity.HasSchedule, entity.IsActive));
    }
}
