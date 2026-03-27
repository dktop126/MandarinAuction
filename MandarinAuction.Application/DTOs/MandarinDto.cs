using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Application.DTOs;

public class MandarinDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime SpoilAt { get; set; }
    public MandarinStatus Status { get; set; }
}