using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Pricing.Queries;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class PricingController : BaseController
{
    [HttpGet("{activityId}")]
    public async Task<IActionResult> GetPricing(Guid activityId)
        => Ok(await Mediator.Send(new GetPricingQuery(activityId)));
}
