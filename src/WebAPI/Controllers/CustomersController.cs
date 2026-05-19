using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Bookings.Queries;
using SwimmingClub.Application.Features.Customers.Commands;
using SwimmingClub.Application.Features.Customers.Queries;
using SwimmingClub.Application.Features.Payments.Queries;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class CustomersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        => HandlePaged(await Mediator.Send(new GetCustomersQuery(search, page, pageSize)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerCommand command)
        => HandleResult(await Mediator.Send(command));

    [HttpGet("{customerId}/bookings")]
    public async Task<IActionResult> GetBookings(Guid customerId)
        => Ok(await Mediator.Send(new GetCustomerBookingsQuery(customerId)));

    [HttpGet("{customerId}/payments")]
    public async Task<IActionResult> GetPayments(Guid customerId)
        => Ok(await Mediator.Send(new GetCustomerPaymentsQuery(customerId)));
}
