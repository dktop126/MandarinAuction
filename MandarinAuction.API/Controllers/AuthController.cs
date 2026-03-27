using MandarinAuction.Application.DTOs;
using MandarinAuction.Application.Features.Auth;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MandarinAuction.API.Controllers;

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

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode([FromForm] RequestCodeDto dto)
    {
        var command = new RequestOtpCodeCommand(dto.Email);
        await _mediator.Send(command);

        return Ok(new { message = "Код отправлен на почту." });
    }

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