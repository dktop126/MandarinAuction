using System.ComponentModel.DataAnnotations;

namespace MandarinAuction.Application.DTOs;

/// <summary>
/// DTO для авторизации пользователя с Email и OTP-кодом.
/// </summary>
public class LoginDto
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный Email")]
    public string? Email { get; set; }
    
    [Required(ErrorMessage = "Код обязателен")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Код должен состоять из 6 цифр")]
    public string? OtpCode { get; set; }
}