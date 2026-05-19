using MediatR;
using Microsoft.AspNetCore.Mvc;
using SwimmingClub.Application.Common.Models;

namespace SwimmingClub.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();

    protected IActionResult HandleResult<T>(Result<T> result)
        => result.IsSuccess ? Ok(result.Data) : BadRequest(new { error = result.ErrorMessage, code = result.ErrorCode });

    protected IActionResult HandlePaged<T>(PagedResult<T> result)
        => Ok(result);
}
