namespace MandarinAuction.Domain.Enums;

/// <summary>
/// Состояние мандаринки на аукционе.
/// </summary>
public enum MandarinStatus
{
    Created,
    InAuction,
    Sold,
    Spoiled
}

/// <summary>
/// Состояние аукциона.
/// </summary>
public enum AuctionStatus
{
    Active,
    Finished,
    Closed
}