using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;

namespace SwimmingClub.Application.Features.Payments.Queries;

public record GetCustomerPaymentsQuery(Guid CustomerId) : IRequest<List<PaymentListDto>>;

public class GetCustomerPaymentsQueryHandler : IRequestHandler<GetCustomerPaymentsQuery, List<PaymentListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerPaymentsQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<List<PaymentListDto>> Handle(GetCustomerPaymentsQuery request, CancellationToken ct)
    {
        return await _context.Payments
            .Include(p => p.Customer)
            .Where(p => p.CustomerId == request.CustomerId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PaymentListDto(
                p.Id, p.CustomerId, p.Customer.FullName,
                p.Amount, p.PaymentMethod, p.PaymentStatus.ToString(),
                p.CreatedAt
            ))
            .ToListAsync(ct);
    }
}
