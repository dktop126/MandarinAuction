using MandarinAuction.Application.DTOs;
using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Application.Features.Auctions;

/// <summary>
/// Запрос на получение списка активных аукционов.
/// </summary>
public class GetActiveAuctionsQuery : IRequest<IReadOnlyList<AuctionDto>>;

/// <summary>
/// Обработчик запроса на получение списка активных аукционов.
/// </summary>
public class GetActiveAuctionsQueryHandler : IRequestHandler<GetActiveAuctionsQuery, IReadOnlyList<AuctionDto>>
{
    private readonly IApplicationDbContext _context;
    
    public GetActiveAuctionsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<AuctionDto>> Handle(GetActiveAuctionsQuery request,
        CancellationToken cancellationToken)
    {
        var auctions = await _context.Auctions
            .Where(a => a.Status == AuctionStatus.Active && a.EndTime > DateTime.UtcNow)
            .Include(a => a.Mandarin)
            .Select(a => new AuctionDto
            {
                Id = a.Id,
                MandarinId = a.MandarinId,
                MandarinName = a.Mandarin.Name,
                CurrentPrice = a.CurrentPrice,
                StartingPrice = a.StartingPrice,
                BuyoutPrice = a.BuyoutPrice,
                EndTime = a.EndTime,
                Status = a.Status,
                BidCount = _context.Bids.Count(b => b.AuctionId == a.Id)
            })
            .ToListAsync(cancellationToken);

        return auctions;
    }
}