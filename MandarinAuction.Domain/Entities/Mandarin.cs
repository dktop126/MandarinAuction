using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Domain.Entities;

/// <summary>
/// Сущность, представляющая мандарин.
/// </summary>
public class Mandarin
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime SpoilAt { get; set; }
    public MandarinStatus Status { get; set; }
    public Auction? Auction { get; set; }

    /// <summary>
    /// Помечает мандарин как испорченный, если срок годности истёк.
    /// Не применяется к проданным лотам.
    /// </summary>
    public void MarkAsSpoiled(DateTime currentTime)
    {
        if (Status == MandarinStatus.Sold)
            return;

        if (SpoilAt < currentTime)
        {
            Status = MandarinStatus.Spoiled;
        }
    }
}