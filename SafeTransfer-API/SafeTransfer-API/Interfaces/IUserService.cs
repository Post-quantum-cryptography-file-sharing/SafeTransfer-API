using SafeTransfer_API.DTOs;

namespace SafeTransfer_API.Interfaces;

public interface IUserService
{
    Task<List<UserSummary>> SearchAsync(string query, string currentUserId);
}