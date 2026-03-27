using System.Security.Claims;
using MandarinAuction.Application.DTOs;
using MandarinAuction.Application.Features.Auctions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MandarinAuction.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuctionController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuctionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveAuctions()
    {
        var query = new GetActiveAuctionsQuery();
        var auctions = await _mediator.Send(query);
        return Ok(auctions);
    }

    [HttpPost("{id}/bids")]
    [Authorize]
    public async Task<IActionResult> PlaceBid(Guid id, [FromBody] PlaceBidDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var command = new PlaceBidCommand(id, Guid.Parse(userId), dto.Amount);
        var auction = await _mediator.Send(command);
        return Ok(auction);
    }
}