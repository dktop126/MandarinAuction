using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Domain.Entities;

/// <summary>
/// Сущность, представляющая мандарин.
/// </summary>
public class Mandarin
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime SpoilAt { get; set; }
    public MandarinStatus Status { get; set; }

    public void MarkAsSpoiled(DateTime currentTime)
    {
        if (Status == MandarinStatus.Sold)
            return;
        
        if (Status != MandarinStatus.Spoiled && SpoilAt < currentTime)
        {
            Status = MandarinStatus.Spoiled;
        }
    }
}