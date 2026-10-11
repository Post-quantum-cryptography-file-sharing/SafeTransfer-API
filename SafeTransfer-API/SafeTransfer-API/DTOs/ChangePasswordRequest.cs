using System.ComponentModel.DataAnnotations;

namespace SafeTransfer_API.Dtos;

public record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, MinLength(8)] string NewPassword);