using MediatR;
using Microsoft.EntityFrameworkCore;
using SwimmingClub.Application.Common.Interfaces;
using SwimmingClub.Application.Common.Models;
using SwimmingClub.Domain.Entities;
using SwimmingClub.Domain.Enums;

namespace SwimmingClub.Application.Features.Payments.Commands;

public record CreatePaymentCommand(
    Guid CustomerId,
    Guid? BookingId,
    double Amount,
    PaymentStatus PaymentStatus,
    string PaymentMethod,
    string? Notes
) : IRequest<Result<PaymentDto>>;

public record PaymentDto(Guid Id, Guid CustomerId, double Amount, string PaymentMethod, string PaymentStatus, DateTime CreatedAt);

public class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentDto>>
{
    private readonly IApplicationDbContext _context;

    public CreatePaymentCommandHandler(IApplicationDbContext context) => _context = context;

    public async Task<Result<PaymentDto>> Handle(CreatePaymentCommand request, CancellationToken ct)
    {
        var customer = await _context.Customers.FindAsync(new object[] { request.CustomerId }, ct);
        if (customer is null)
            return Result<PaymentDto>.Failure("Customer not found");

        var payment = new Payment
        {
            CustomerId = request.CustomerId,
            BookingId = request.BookingId,
            Amount = request.Amount,
            PaymentStatus = request.PaymentStatus,
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes
        };

        if (request.PaymentStatus == PaymentStatus.Paid)
            customer.CurrentBalance -= request.Amount;
        else if (request.PaymentStatus == PaymentStatus.Credit)
            customer.CurrentBalance += request.Amount;

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(ct);

        return Result<PaymentDto>.Success(new PaymentDto(
            payment.Id, payment.CustomerId, payment.Amount,
            payment.PaymentMethod, payment.PaymentStatus.ToString(), payment.CreatedAt
        ));
    }
}
