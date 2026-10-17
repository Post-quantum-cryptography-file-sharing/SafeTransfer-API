namespace SafeTransfer_API.DTOs;

public record RecipientResponse(Guid Id, string UserId, string email, string DisplayName, DateTime? DownloadedAt);