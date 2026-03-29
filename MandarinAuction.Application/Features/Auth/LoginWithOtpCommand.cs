using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace MandarinAuction.Application.Features.Auth;

/// <summary>
/// Команда на вход пользователя с помощью Email и OTP-кода
/// </summary>
/// <param name="Email"></param>
/// <param name="OtpCode"></param>
public record LoginWithOtpCommand(string Email, string OtpCode) : IRequest<LoginResult>;

/// <summary>
/// Результат успешного входа.
/// </summary>
/// <param name="UserId">ID пользователя.</param>
/// <param name="Email">email.</param>
/// <param name="Token">JWT-токен.</param>
public record LoginResult(Guid UserId, string Email, string Token);

/// <summary>
/// Обработчик команды входа по OTP-коду.
/// Проверяет данные, генерирует JWT-токен и возвращает данные пользователя.
/// </summary>
public class LoginWithOtpCommandHandler : IRequestHandler<LoginWithOtpCommand, LoginResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _config;

    public LoginWithOtpCommandHandler(IApplicationDbContext context, IConfiguration config)
    {
        _context = context;
        _config =  config;
    }

    public async Task<LoginResult> Handle(LoginWithOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user == null || !user.VerifyOtpCode(request.OtpCode))
            throw new UnauthorizedAccessException("Неверный email или код.");
        
        user.ClearOtpCode();
        await _context.SaveChangesAsync();
        
        var token = GenerateJwt(user);
        
        return new LoginResult(user.Id, user.Email, token);
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