using System.ComponentModel.DataAnnotations;

namespace MandarinAuction.Application.DTOs;

public class RequestCodeDto
{
    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный Email")]
    public string Email { get; set; } = string.Empty;
}