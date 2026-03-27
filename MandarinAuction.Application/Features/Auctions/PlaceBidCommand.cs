using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Domain.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MandarinAuction.Application.Features.Auctions;

public record PlaceBidCommand(Guid AuctionId, Guid UserId, decimal Amount) : IRequest<Unit>;

public class PlaceBidCommandHandler : IRequestHandler<PlaceBidCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public PlaceBidCommandHandler(IApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<Unit> Handle(PlaceBidCommand request, CancellationToken cancellationToken)
    {
        var auction = await _context.Auctions
            .FindAsync(new object[] { request.AuctionId }, cancellationToken);

        if (auction == null)
            throw new NotFoundException($"Аукцион {request.AuctionId} не найден.");

        var previousBidderId = auction.LastBidderId;
        auction.PlaceBid(request.UserId, request.Amount);

        var bid = new Bid(request.AuctionId, request.UserId, request.Amount);
        _context.Bids.Add(bid);

        await _context.SaveChangesAsync();

        if (!previousBidderId.HasValue) return Unit.Value;
        var previousBidder = await _context.Users
            .FindAsync(new object[] { previousBidderId.Value }, cancellationToken);
        if (previousBidder != null)
            await _emailService.SendOutbidNotificationAsync(previousBidder.Email, request.AuctionId,
                request.Amount);

        return Unit.Value;
    }
}