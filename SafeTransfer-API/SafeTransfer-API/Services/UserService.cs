using Microsoft.EntityFrameworkCore;
using SafeTransfer_API.Data;
using SafeTransfer_API.DTOs;
using SafeTransfer_API.Interfaces;

namespace SafeTransfer_API.Services;

public class UserService : IUserService
{
    private const int MaxSearchResults = 5;
    private readonly AppDbContext _db;

    public UserService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<UserSummary>> SearchAsync(string query, string currentUserId)
    {
        query = query.Trim();

        return await _db.Users
            .AsNoTracking()
            .Where(u => u.Id != currentUserId)
            .Where(u => u.DisplayName.Contains(query) || u.NormalizedEmail!.Equals(query.ToUpperInvariant()))
            .OrderBy(u => u.DisplayName)
            .Take(MaxSearchResults)
            .Select(u => new UserSummary(u.Id, u.DisplayName, u.Email!))
            .ToListAsync();
    }
}