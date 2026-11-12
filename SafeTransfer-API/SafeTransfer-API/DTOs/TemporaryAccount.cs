using SafeTransfer_API.Models;

namespace SafeTransfer_API.DTOs;

public record TemporaryAccount(AppUser User, string Password);