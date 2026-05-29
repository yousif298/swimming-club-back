using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.CategorySchedules.Queries;

public record GetCategoryScheduleQuery(
    Guid BookingTypeId
) : IRequest<List<CategoryScheduleDto>>;

public record CategoryScheduleDto(
    Guid Id,
    Guid BookingTypeId,
    int DayOfWeek,
    string StartTime,
    string EndTime,
    int SlotDurationMinutes,
    bool IsActive
);

public class GetCategoryScheduleQueryHandler : IRequestHandler<GetCategoryScheduleQuery, List<CategoryScheduleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCategoryScheduleQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<CategoryScheduleDto>> Handle(GetCategoryScheduleQuery request, CancellationToken ct)
    {
        return await _context.CategorySchedules
            .AsNoTracking()
            .Where(cs => cs.BookingTypeId == request.BookingTypeId && !cs.IsDeleted)
            .OrderBy(cs => cs.DayOfWeek)
            .Select(cs => new CategoryScheduleDto(
                cs.Id,
                cs.BookingTypeId,
                (int)cs.DayOfWeek,
                cs.StartTime.ToString(@"hh\:mm"),
                cs.EndTime.ToString(@"hh\:mm"),
                cs.SlotDurationMinutes,
                cs.IsActive
            ))
            .ToListAsync(ct);
    }
}
