using FluentAssertions;
using MandarinAuction.Application.Features.Auctions;
using MandarinAuction.Application.Interfaces;
using MandarinAuction.Domain.Entities;
using MandarinAuction.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MandarinAuction.Tests.Integration;

public class PlaceBidCommandHandlerTests : IntegrationTestBase
{
    [Fact]
    public async Task Handle_WithValidBid_ShouldSaveBidAndUpdateAuction()
    {
        // Arrange
        var user = new User();
        var mandarin = new Mandarin { SpoilAt = DateTime.UtcNow.AddDays(1) };
        var auction = new Auction(mandarin.Id, 100, 1000, 100, mandarin.SpoilAt);
        DbContext.Users.Add(user);
        DbContext.Auctions.Add(auction);
        DbContext.Mandarins.Add(mandarin);
        await DbContext.SaveChangesAsync();
        var emailServiceMock = new Mock<IEmailService>();
        var handler = new PlaceBidCommandHandler(DbContext, emailServiceMock.Object);
        var command = new PlaceBidCommand(auction.Id, user.Id, 200);
        
        // Act
        await handler.Handle(command, CancellationToken.None);
        
        // Assert
        var savedBid = await DbContext.Bids.FirstOrDefaultAsync();
        savedBid.Should().NotBeNull();
        savedBid!.Amount.Should().Be(200);
        
        var updatedAuction = await DbContext.Auctions.FindAsync(auction.Id);
        updatedAuction!.CurrentPrice.Should().Be(200);
    }
}