using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Pricing.Commands;
using SwimmingClub.Application.Features.Pricing.Queries;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class PricingController : BaseController
{
    [HttpGet("{activityId}")]
    public async Task<IActionResult> GetPricing(Guid activityId)
        => Ok(await Mediator.Send(new GetPricingQuery(activityId)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePricingCommand command)
        => HandleResult(await Mediator.Send(command));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePricingCommand command)
        => HandleResult(await Mediator.Send(command with { Id = id }));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeletePricingCommand(id)));
}
