using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Pools.Commands;
using SwimmingClub.Application.Features.Pools.Queries;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class PoolsController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await Mediator.Send(new GetPoolsQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePoolCommand command)
        => HandleResult(await Mediator.Send(command));

    [HttpGet("{poolId}/lanes")]
    public async Task<IActionResult> GetLanes(Guid poolId)
        => Ok(await Mediator.Send(new GetLanesQuery(poolId)));
}
