namespace SafeTransfer_API.DTOs;

public record TargetResponse(Guid Id, string UserId, string email, string DisplayName, DateTime? DownloadedAt);