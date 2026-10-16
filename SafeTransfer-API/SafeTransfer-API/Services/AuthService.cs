using Microsoft.AspNetCore.Identity;
using SafeTransfer_API.Dtos;
using SafeTransfer_API.Interfaces;
using SafeTransfer_API.Models;

namespace SafeTransfer_API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
    {
        var user = new AppUser
        {
            Email = request.Email,
            CreatedAt = DateTime.UtcNow,
            UserName = request.Email,
            DisplayName = request.DisplayName,
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);
        
        if (result.Succeeded)
            await _signInManager.SignInAsync(user, isPersistent: false);
        
        return result;
    }

    public async Task<SignInResult> LogInAsync(LoginRequest request)
    {
        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, isPersistent: false,
            lockoutOnFailure: true);
        
        return result;
    }
    
    public Task LogoutAsync()
    {
        return _signInManager.SignOutAsync();
    }

    public async Task<IdentityResult> ChangePasswordAsync(AppUser user, ChangePasswordRequest request)
    {
        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (result.Succeeded)
            await _signInManager.RefreshSignInAsync(user);
        
        return result;
    }
}