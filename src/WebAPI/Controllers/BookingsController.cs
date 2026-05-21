using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Bookings.Commands;
using SwimmingClub.Application.Features.Bookings.Queries;
using SwimmingClub.WebAPI.Controllers;

using SwimmingClub.Application.Features.Members.Commands;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class BookingsController : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingCommand command)
        => HandleResult(await Mediator.Send(command));

    [HttpGet("mirror")]
    public async Task<IActionResult> GetMirrorView([FromQuery] Guid poolId, [FromQuery] DateTime date)
        => Ok(await Mediator.Send(new GetMirrorViewQuery(poolId, date)));

    [HttpGet("slot-status")]
    public async Task<IActionResult> GetSlotStatus([FromQuery] Guid poolId, [FromQuery] Guid laneId, [FromQuery] Guid slotId, [FromQuery] DateTime date)
        => Ok(await Mediator.Send(new GetSlotStatusQuery(poolId, laneId, slotId, date)));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] DateTime? date)
        => Ok(await Mediator.Send(new GetBookingsQuery(date)));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingCommand command)
        => HandleResult(await Mediator.Send(command with { Id = id }));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Cancel(Guid id)
        => HandleResult(await Mediator.Send(new CancelBookingCommand(id)));

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMembers(Guid id, [FromBody] AddBookingMembersCommand command)
        => HandleResult(await Mediator.Send(command with { BookingId = id }));
}
