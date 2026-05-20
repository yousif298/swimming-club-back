using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.BookingTypes.Commands;
using SwimmingClub.Application.Features.BookingTypes.Queries;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class BookingTypesController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await Mediator.Send(new GetBookingTypesQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateBookingTypeCommand command)
        => HandleResult(await Mediator.Send(command));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingTypeCommand command)
        => HandleResult(await Mediator.Send(command with { Id = id }));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteBookingTypeCommand(id)));
}
