using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Bookings.Queries;

public record GetMirrorViewQuery(
    Guid PoolId,
    DateTime Date
) : IRequest<MirrorViewDto>;

public record MirrorViewDto(
    Guid PoolId,
    string PoolName,
    List<LaneMirrorDto> Lanes,
    List<TimeSlotMirrorDto> TimeSlots
);

public record LaneMirrorDto(Guid Id, int Number);

public record TimeSlotMirrorDto(Guid Id, string Display, int OrderIndex);

public record MirrorSlotDto(
    Guid? BookingId,
    Guid? CustomerId,
    string? CustomerName,
    string? BookingType,
    string Status // available, booked, pending
);

public class GetMirrorViewQueryHandler : IRequestHandler<GetMirrorViewQuery, MirrorViewDto>
{
    private readonly IApplicationDbContext _context;

    public GetMirrorViewQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<MirrorViewDto> Handle(GetMirrorViewQuery request, CancellationToken ct)
    {
        var pool = await _context.Pools
            .Include(p => p.Lanes.OrderBy(l => l.LaneNumber))
            .FirstOrDefaultAsync(p => p.Id == request.PoolId, ct)
            ?? throw new Exception("Pool not found");

        var timeSlots = await _context.TimeSlots
            .OrderBy(t => t.OrderIndex)
            .ToListAsync(ct);

        var bookings = await _context.Bookings
            .Include(b => b.Customer)
            .Where(b => b.BookingDate == request.Date && b.Lane.PoolId == request.PoolId && !b.IsDeleted)
            .ToListAsync(ct);

        var lanes = pool.Lanes.Select(l => new LaneMirrorDto(l.Id, l.LaneNumber)).ToList();
        var slotDtos = timeSlots.Select(t => new TimeSlotMirrorDto(t.Id, t.DisplayTime, t.OrderIndex)).ToList();

        return new MirrorViewDto(pool.Id, pool.Name, lanes, slotDtos);
    }
}
