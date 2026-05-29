using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.CategorySchedules.Commands;
using SwimmingClub.Application.Features.CategorySchedules.Queries;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class CategorySchedulesController : BaseController
{
    [HttpGet("{bookingTypeId}")]
    public async Task<IActionResult> GetSchedules(Guid bookingTypeId)
        => Ok(await Mediator.Send(new GetCategoryScheduleQuery(bookingTypeId)));

    [HttpPut("{bookingTypeId}")]
    public async Task<IActionResult> UpdateSchedules(Guid bookingTypeId, [FromBody] UpdateCategoryScheduleCommand command)
        => HandleResult(await Mediator.Send(command with { BookingTypeId = bookingTypeId }));
}
