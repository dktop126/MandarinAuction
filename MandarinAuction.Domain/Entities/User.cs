using System.Text.RegularExpressions;

namespace MandarinAuction.Domain.Entities;

/// <summary>
/// Сущность, представляющая пользователя.
/// </summary>
public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? OtpCode { get; set; }
    public DateTime? OtpExpiresAt { get; set; }

    public ICollection<Bid> Bids { get; set; } =  new List<Bid>();

    public void SetEmail(string email)
    {
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        if (string.IsNullOrWhiteSpace(email) || !Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase))
            throw new ArgumentException("Некорректный email.", nameof(email));
        Email = email.ToLower();
    }

    public void GenerateOtpCode()
    {
        var random = new Random();
        OtpCode = random.Next(100000, 999999).ToString();
        OtpExpiresAt = DateTime.UtcNow.AddMinutes(10);
    }

    public bool VerifyOtpCode(string code)
    {
        if (string.IsNullOrWhiteSpace(OtpCode) || !OtpExpiresAt.HasValue)
            return false;
        if (DateTime.UtcNow > OtpExpiresAt.Value)
            return false;
        return OtpCode == code;
    }
    
    public void ClearOtpCode()
    {
        OtpCode = null;
        OtpExpiresAt = null;
    }
}