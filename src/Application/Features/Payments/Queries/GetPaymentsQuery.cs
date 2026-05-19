using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Payments.Queries;

public record GetPaymentsQuery(DateTime? From, DateTime? To) : IRequest<List<PaymentListDto>>;

public record PaymentListDto(
    Guid Id, Guid CustomerId, string CustomerName,
    double Amount, string PaymentMethod, string PaymentStatus,
    DateTime CreatedAt
);

public class GetPaymentsQueryHandler : IRequestHandler<GetPaymentsQuery, List<PaymentListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPaymentsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<PaymentListDto>> Handle(GetPaymentsQuery request, CancellationToken ct)
    {
        var query = _context.Payments
            .Include(p => p.Customer)
            .AsQueryable();

        if (request.From.HasValue)
            query = query.Where(p => p.CreatedAt >= request.From.Value);
        if (request.To.HasValue)
            query = query.Where(p => p.CreatedAt <= request.To.Value);

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentListDto(
                p.Id, p.CustomerId, p.Customer.FullName,
                p.Amount, p.PaymentMethod, p.PaymentStatus.ToString(),
                p.CreatedAt
            ))
            .ToListAsync(ct);
    }
}
