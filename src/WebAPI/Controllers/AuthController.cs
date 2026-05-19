using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Auth.Commands;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

public class AuthController : BaseController
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
        => HandleResult(await Mediator.Send(command));
}
