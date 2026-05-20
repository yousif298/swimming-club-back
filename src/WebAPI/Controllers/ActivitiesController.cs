using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Activities.Queries;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class ActivitiesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await Mediator.Send(new GetActivitiesQuery()));
}
