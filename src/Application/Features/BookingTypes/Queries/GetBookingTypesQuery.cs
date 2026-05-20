using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.BookingTypes.Queries;

public record GetBookingTypesQuery(bool IncludeInactive = false) : IRequest<List<BookingTypeDto>>;

public record BookingTypeDto(Guid Id, string Name, string? Description, double DefaultPrice, bool HasCapacity, int? Capacity, bool HasSchedule, bool IsActive);

public class GetBookingTypesQueryHandler : IRequestHandler<GetBookingTypesQuery, List<BookingTypeDto>>
{
    private readonly IApplicationDbContext _context;
    public GetBookingTypesQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<BookingTypeDto>> Handle(GetBookingTypesQuery request, CancellationToken ct)
    {
        var query = _context.BookingTypes.AsNoTracking();
        if (!request.IncludeInactive)
            query = query.Where(t => t.IsActive);

        return await query
            .OrderBy(t => t.Name)
            .Select(t => new BookingTypeDto(t.Id, t.Name, t.Description, t.DefaultPrice, t.HasCapacity, t.Capacity, t.HasSchedule, t.IsActive))
            .ToListAsync(ct);
    }
}
