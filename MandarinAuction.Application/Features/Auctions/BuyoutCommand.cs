using MandarinAuction.Application.DTOs;
using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Application.Features.Auctions;

/// <summary>
/// Команда на выкуп лота (buyout).
/// </summary>
/// <param name="AuctionId">ID аукциона.</param>
/// <param name="UserId">ID пользователя.</param>
public record BuyoutCommand(Guid AuctionId, Guid UserId) : IRequest<AuctionDto>;

/// <summary>
/// Обработчик команды на выкуп лота.
/// Выкупает лот по цене выкупа, закрывает аукцион и отправляет чек победителю.
/// </summary>
public class BuyoutCommandHandler : IRequestHandler<BuyoutCommand, AuctionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public BuyoutCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<AuctionDto> Handle(BuyoutCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .Include(a => a.Mandarin)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);
        
        if (auction == null)
            throw new NotFoundException($"Аукцион {request.AuctionId} не найден.");
        
        var previousBidderId = auction.LastBidderId;
        auction.Buyout(request.UserId);
        
        var bid = new Bid(request.AuctionId, request.UserId, auction.CurrentPrice);
        _context.Bids.Add(bid);

        await _context.SaveChangesAsync();

        var winner = await _context.Users
            .FindAsync(new object[] { request.UserId }, cancellationToken);
        if (winner != null)
            await _emailService.SendWinReceiptNotificationAsync(winner.Email, request.AuctionId, auction.CurrentPrice);

        if (previousBidderId.HasValue)
        {
            var previousBidder = await _context.Users
                .FindAsync(new object[] { previousBidderId }, cancellationToken);
            if (previousBidder != null)
                await _emailService.SendOutbidNotificationAsync(previousBidder.Email, 
                    request.AuctionId, auction.CurrentPrice);
        }

        return new AuctionDto
        {
            Id = auction.Id,
            MandarinId = auction.MandarinId,
            MandarinName = auction.Mandarin?.Name ?? string.Empty,
            CurrentPrice = auction.CurrentPrice,
            StartingPrice = auction.StartingPrice,
            MinBidIncrement =  auction.MinBidIncrement,
            EndTime = auction.EndTime,
            Status = auction.Status,
            BidCount = _context.Bids.Count(b => b.AuctionId == auction.Id),
        };
    }
}