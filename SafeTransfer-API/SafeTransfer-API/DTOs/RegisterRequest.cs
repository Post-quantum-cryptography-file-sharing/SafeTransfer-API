using System.ComponentModel.DataAnnotations;

namespace SafeTransfer_API.Dtos;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [Required, MaxLength(100)] string DisplayName);