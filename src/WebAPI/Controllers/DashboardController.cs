using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Dashboard.Queries;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class DashboardController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetDashboard()
        => Ok(await Mediator.Send(new GetDashboardQuery()));
}
