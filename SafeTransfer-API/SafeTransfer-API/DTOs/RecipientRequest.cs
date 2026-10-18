namespace SafeTransfer_API.DTOs;

public record RecipientRequest(List<string>? UserIds, List<string>? Emails);