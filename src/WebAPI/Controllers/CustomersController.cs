using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Customers.Commands;
using SwimmingClub.Application.Features.Customers.Queries;
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
}
