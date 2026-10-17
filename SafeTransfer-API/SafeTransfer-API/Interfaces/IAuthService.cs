using Microsoft.AspNetCore.Identity;
using SafeTransfer_API.Dtos;
using SafeTransfer_API.Models;
using SafeTransfer_API.Services;

namespace SafeTransfer_API.Interfaces;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(RegisterRequest request);
    Task<SignInResult> LogInAsync(LoginRequest request);
    Task LogoutAsync();
    Task<IdentityResult> ChangePasswordAsync(AppUser user, ChangePasswordRequest request);
    Task<ServiceResult<AppUser>> CreateTemporaryAccountAsync(string email);
}