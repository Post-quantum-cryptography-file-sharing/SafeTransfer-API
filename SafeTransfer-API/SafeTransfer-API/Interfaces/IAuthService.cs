using Microsoft.AspNetCore.Identity;
using SafeTransfer_API.Dtos;
using SafeTransfer_API.Models;

namespace SafeTransfer_API.Interfaces;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
    Task<SignInResult> LogInAsync(LoginRequest request);
    Task LogoutAsync();
    Task<IdentityResult> ChangePasswordAsync(AppUser user, ChangePasswordRequest request);
}