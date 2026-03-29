using System.ComponentModel.DataAnnotations;

namespace MandarinAuction.Application.DTOs;

/// <summary>
/// DTO для запроса OTP-кода на указанный Email.
/// </summary>
public class RequestCodeDto
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный Email")]
    public string Email { get; set; } = string.Empty;
}