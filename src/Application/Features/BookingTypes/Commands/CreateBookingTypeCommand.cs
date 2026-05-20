using MediatR;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Application.Features.BookingTypes.Queries;
using SwimmingClub.Domain.Entities;

namespace SwimmingClub.Application.Features.BookingTypes.Commands;

public record CreateBookingTypeCommand(string Name, string? Description, double DefaultPrice, bool HasCapacity, int? Capacity, bool HasSchedule) : IRequest<Result<BookingTypeDto>>;

public class CreateBookingTypeCommandHandler : IRequestHandler<CreateBookingTypeCommand, Result<BookingTypeDto>>
{
    private readonly IApplicationDbContext _context;
    public CreateBookingTypeCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<BookingTypeDto>> Handle(CreateBookingTypeCommand request, CancellationToken ct)
    {
        var entity = new BookingType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultPrice = request.DefaultPrice,
            HasCapacity = request.HasCapacity,
            Capacity = request.Capacity,
            HasSchedule = request.HasSchedule,
        };
        _context.BookingTypes.Add(entity);
        await _context.SaveChangesAsync(ct);
        return Result<BookingTypeDto>.Success(new BookingTypeDto(entity.Id, entity.Name, entity.Description, entity.DefaultPrice, entity.HasCapacity, entity.Capacity, entity.HasSchedule, entity.IsActive));
    }
}
