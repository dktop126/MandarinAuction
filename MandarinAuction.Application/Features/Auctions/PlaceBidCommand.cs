using MandarinAuction.Application.DTOs;
using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Application.Features.Auctions;

/// <summary>
/// Команда на размещение ставки.
/// Содержит ID аукциона, ID пользователя и сумму ставки.
/// </summary>
/// <param name="AuctionId"></param>
/// <param name="UserId"></param>
/// <param name="Amount"></param>
public record PlaceBidCommand(Guid AuctionId, Guid UserId, decimal Amount) : IRequest<AuctionDto>;

/// <summary>
/// Обработчик команды на размещение ставки.
/// Проверяет аукцион, делает ставку, сохраняет её и отправляет уведомление предыдущему лидеру.
/// </summary>
public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, AuctionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public PlaceBidCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<AuctionDto> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .Include(a => a.Mandarin)
            .FirstOrDefaultAsync(a => a.Id == request.AuctionId, cancellationToken);

        if (auction == null)
            throw new NotFoundException($"Аукцион {request.AuctionId} не найден.");

        var previousBidderId = auction.LastBidderId;
        auction.PlaceBid(request.UserId, request.Amount);

        var bid = new Bid(request.AuctionId, request.UserId, request.Amount);
        _context.Bids.Add(bid);

        await _context.SaveChangesAsync();

        if (previousBidderId.HasValue)
        {
            var previousBidder = await _context.Users
                .FindAsync(new object[] { previousBidderId.Value }, cancellationToken);
            if (previousBidder != null)
                await _emailService.SendOutbidNotificationAsync(previousBidder.Email, request.AuctionId,
                    request.Amount);
        }

        return new AuctionDto
        {
            Id = auction.Id,
            MandarinId = auction.MandarinId,
            MandarinName = auction.Mandarin?.Name ?? string.Empty,
            CurrentPrice = auction.CurrentPrice,
            StartingPrice = auction.StartingPrice,
            EndTime = auction.EndTime,
            Status = auction.Status,
            BidCount = _context.Bids.Count(b => b.AuctionId == auction.Id),
        };
    }
}