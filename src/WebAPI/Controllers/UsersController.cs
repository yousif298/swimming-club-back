using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Users.Commands;
using SwimmingClub.Application.Features.Users.Queries;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class UsersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await Mediator.Send(new GetUsersQuery()));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserCommand command)
        => HandleResult(await Mediator.Send(command));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserCommand command)
        => HandleResult(await Mediator.Send(command with { Id = id }));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new UpdateUserCommand(id, null, null, null, false)));
}
