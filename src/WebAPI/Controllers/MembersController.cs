using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Features.Members.Commands;
using SwimmingClub.Application.Features.Members.Queries;
using SwimmingClub.WebAPI.Controllers;

namespace SwimmingClub.WebAPI.Controllers;

[Authorize]
public class MembersController : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? search)
        => Ok(await Mediator.Send(new GetMembersQuery(search)));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMemberCommand command)
        => HandleResult(await Mediator.Send(command));

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMemberCommand command)
        => HandleResult(await Mediator.Send(command with { Id = id }));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
        => HandleResult(await Mediator.Send(new DeleteMemberCommand(id)));
}
