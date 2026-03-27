using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MandarinAuction.Application.DTOs;
using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace MandarinAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public AuthController(
        IApplicationDbContext context,
        IEmailService emailService,
        IConfiguration config)
    {
        _context = context;
        _emailService = emailService;
        _config = config;
    }

    [HttpPost("request-code")]
    public async Task<IActionResult> RequestCode([FromForm] RequestCodeDto dto)
    {
        var user = _context.Users
            .FirstOrDefault(u => u.Email == dto.Email.ToLower());

        if (user == null)
        {
            user = new User();
            user.SetEmail(dto.Email);
            user.Username = dto.Email.Split('@')[0].ToLower();
            _context.Users.Add(user);
        }
        
        user.GenerateOtpCode();
        await _context.SaveChangesAsync();
        
        await _emailService.SendOtpCodeAsync(user.Email, user.OtpCode);
        return Ok();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == dto.Email.ToLower());
        if (user == null || !user.VerifyOtpCode(dto.OtpCode))
            return Unauthorized(new { message = "Неверный код или email." });
        
        user.ClearOtpCode();
        await _context.SaveChangesAsync();
        
        var token = GenerateJwt(user);
        
        return Ok(new { token, userId =  user.Id, email = user.Email });
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "default-secret-key-for-testing"));
        
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
            );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}