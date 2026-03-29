using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Application.Features.Auth;

/// <summary>
/// Команда на запрос OTP-кода для авторизации.
/// </summary>
public record RequestOtpCodeCommand(string Email) : IRequest<Unit>;

/// <summary>
/// Обработчик команды запроса OTP-кода.
/// Создает нового пользователя, если он не существует, генерирует OTP-код и отправляет его.
/// </summary>
public class RequestOtpCodeCommandHandler : IRequestHandler<RequestOtpCodeCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    
    public RequestOtpCodeCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(RequestOtpCodeCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.ToLower();
        
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user == null)
        {
            user = new User();
            user.SetEmail(email);
            user.Username = email.Split('@')[0].ToLower();
            _context.Users.Add(user);
        }
        user.GenerateOtpCode();
        await _context.SaveChangesAsync();
        
        await _emailService.SendOtpCodeAsync(user.Email, user.OtpCode);
        
        return Unit.Value;
    }
}