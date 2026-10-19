using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using SafeTransfer_API.Data;
using SafeTransfer_API.Dtos;
using SafeTransfer_API.DTOs;
using SafeTransfer_API.Interfaces;
using SafeTransfer_API.Models;
using Microsoft.EntityFrameworkCore;

namespace SafeTransfer_API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly ILogger<AuthService> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _db;

    public AuthService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,  ILogger<AuthService> logger,  IWebHostEnvironment env, AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _env = env;
        _db = db;
    }

    public async Task DeleteAccountAsync(AppUser user)
    {
        await _db.RecipientUsers.Where(r => r.UserId == user.Id).ExecuteDeleteAsync();
        await _db.StoredFiles.Where(f => f.OwnerId == user.Id).ExecuteDeleteAsync();
        await _db.UploadBatches.Where(b => b.OwnerId == user.Id).ExecuteDeleteAsync();
        
        await LogoutAsync();
        await _userManager.DeleteAsync(user);
    }

    public async Task<ServiceResult<TemporaryAccount>> CreateTemporaryAccountAsync(string email)
    {
        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = email.Split('@')[0],
            CreatedAt = DateTime.UtcNow
        };

        var password = GenerateTemporaryPassword();
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return ServiceResult<TemporaryAccount>.Invalid(string.Join(" ", result.Errors.Select(e => e.Description)));
        
        if (_env.IsDevelopment())
            _logger.LogInformation("Temporary account {Email} created with password {Password}", email, password);

        return ServiceResult<TemporaryAccount>.Ok(new TemporaryAccount(user,password));
    }

    private static string GenerateTemporaryPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        return RandomNumberGenerator.GetString(chars, 16) + "-a1A";
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