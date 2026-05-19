using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Payments.Commands;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class PaymentsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePaymentCommand command)
        => HandleResult(await Mediator.Send(command));
}
