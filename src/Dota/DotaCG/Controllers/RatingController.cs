using Microsoft.AspNetCore.Mvc;

namespace DotaCG.Controllers;

[ApiController]
[Route("rating")]
public class RatingController : ControllerBase
{
    private readonly ILogger<RatingController>? _logger;

    public RatingController(ILogger<RatingController>? logger)
    {
        _logger = logger;
    }

    [HttpGet("player")]
    public long GetPlayerRating(long playerId)
    {
        return 1;
    }

    [HttpGet("players")]
    public IEnumerable<long> GetPlayersRating(long[] playersId)
    {
        return playersId.Select(playerId => 1L);
    }
}