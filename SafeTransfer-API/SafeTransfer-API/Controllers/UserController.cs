using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeTransfer_API.Interfaces;

namespace SafeTransfer_API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    
    public UserController(IUserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery, Required, MinLength(2)] string query)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await _userService.SearchAsync(query, currentUserId));
    }
}