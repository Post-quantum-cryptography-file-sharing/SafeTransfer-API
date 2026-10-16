using System.ComponentModel.DataAnnotations;

namespace SafeTransfer_API.Dtos;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);