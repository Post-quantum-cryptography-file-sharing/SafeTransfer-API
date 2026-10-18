using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeTransfer_API.Services;

namespace SafeTransfer_API.Controllers;

[ApiController]
[Authorize]
public abstract class AppControllerBase : ControllerBase
{
    protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    protected ActionResult<T> ToActionResult<T>(ServiceResult<T> serviceResult)
        => serviceResult.Status switch
        {
            ServiceStatus.Ok => (ActionResult)Ok(serviceResult.Value),
            ServiceStatus.NotFound => NotFound(),
            ServiceStatus.Forbidden => Forbid(),
            _ => BadRequest(serviceResult.Error)
        };

}