using MandarinAuction.Domain.Enums;

namespace MandarinAuction.Application.DTOs;

/// <summary>
/// DTO для передачи данных аукциона.
/// </summary>
public class AuctionDto
{
    public Guid Id { get; set; }
    public Guid MandarinId { get; set; }
    public string MandarinName { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public decimal StartingPrice { get; set; }
    public decimal? BuyoutPrice { get; set; }
    public DateTime EndTime { get; set; }
    public AuctionStatus Status { get; set; }
    public int BidCount { get; set; }
}