using MandarinAuction.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Application.Features.Auth;

public record LoginWithOtpCommand(string Email, string OtpCode) : IRequest<LoginResult>;

public record LoginResult(Guid UserId, string Email);

public class LoginWithOtpCommandHandler : IRequestHandler<LoginWithOtpCommand, LoginResult>
{
    private readonly IApplicationDbContext _context;
    
    public LoginWithOtpCommandHandler(IApplicationDbContext context) =>  _context = context;

    public async Task<LoginResult> Handle(LoginWithOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email.ToLower());

        if (user == null || !user.VerifyOtpCode(request.OtpCode))
            throw new UnauthorizedAccessException("Неверный email или код.");
        
        user.ClearOtpCode();
        await _context.SaveChangesAsync();
        
        return new LoginResult(user.Id, user.Email);
    }
    
}