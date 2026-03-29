namespace MandarinAuction.Application.DTOs;

/// <summary>
///     DTO для ответа пользователю после успешной авторизации.
/// </summary>
public class LoginResponseDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}