using MandarinAuction.Application.DTOs;
using MandarinAuction.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MandarinAuction.API.Controllers;

/// <summary>
/// Контроллер для аутентификации пользователей.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _config;

    public AuthController(IMediator mediator, IConfiguration config)
    {
        _mediator = mediator;
        _config = config;
    }

    /// <summary>
    /// Запрашивает OTP-код для входа на указанный email.
    /// </summary>
    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode([FromForm] RequestCodeDto dto)
    {
        var command = new RequestOtpCodeCommand(dto.Email);
        await _mediator.Send(command);

        return Ok(new { message = "Код отправлен на почту." });
    }

    /// <summary>
    /// Выполняет вход с помощью email и OTP-кода, возвращает JWT-токен.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var command = new LoginWithOtpCommand(dto.Email, dto.OtpCode);
        var result = await _mediator.Send(command);

        return Ok(new LoginResponseDto
        {
            UserId = result.UserId,
            Email = result.Email,
            Token = result.Token
        });
    }
}