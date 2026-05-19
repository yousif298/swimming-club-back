using MediatR;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Domain.Entities;

namespace SwimmingClub.Application.Features.Customers.Commands;

public record CreateCustomerCommand(
    string FullName,
    string Phone,
    string? Address
) : IRequest<Result<CustomerDto>>;

public record CustomerDto(
    Guid Id,
    string FullName,
    string Phone,
    string? Address,
    double CurrentBalance,
    DateTime CreatedAt
);

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateCustomerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CustomerDto>> Handle(CreateCustomerCommand request, CancellationToken ct)
    {
        var customer = new Customer
        {
            FullName = request.FullName,
            Phone = request.Phone,
            Address = request.Address,
            CurrentBalance = 0
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync(ct);

        return Result<CustomerDto>.Success(new CustomerDto(
            customer.Id,
            customer.FullName,
            customer.Phone,
            customer.Address,
            customer.CurrentBalance,
            customer.CreatedAt
        ));
    }
}
