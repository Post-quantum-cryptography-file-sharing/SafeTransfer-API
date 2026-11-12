using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeTransfer_API.Data;
using SafeTransfer_API.Dtos;
using SafeTransfer_API.Interfaces;
using SafeTransfer_API.Models;
using RegisterRequest = SafeTransfer_API.Dtos.RegisterRequest;

namespace SafeTransfer_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly UserManager<AppUser> _userManager;

    public AuthController(UserManager<AppUser> userManager, IAuthService authService)
    {
        _authService = authService;
        _userManager = userManager;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest registerRequest) 
    {
        var result = await _authService.RegisterAsync(registerRequest);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));
        
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest loginRequest)
    {
        var result = await _authService.LogInAsync(loginRequest);
        if (result.IsLockedOut)
            return StatusCode(StatusCodes.Status423Locked, "Too many attempts");
        if (!result.Succeeded)
            return Unauthorized("Invalid email or password");

        return Ok();
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest changePasswordRequest)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();

        var result = await _authService.ChangePasswordAsync(user, changePasswordRequest);
        if (!result.Succeeded)
            return BadRequest(result.Errors.Select(e => e.Description));

        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await _userManager.GetUserAsync(User);
        if(user is null) return Unauthorized();

        return Ok(new UserResponse(user.Id, user.Email!, user.DisplayName!));
    }

    [Authorize]
    [HttpDelete("me")]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null) return Unauthorized();
        
        await _authService.DeleteAccountAsync(user);
        return NoContent();
    }
}