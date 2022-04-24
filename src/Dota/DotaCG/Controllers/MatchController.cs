using Microsoft.AspNetCore.Mvc;

namespace DotaCG.Controllers;

[ApiController]
[Route("rating")]
public class MatchController : ControllerBase
{
    private readonly ILogger<MatchController>? _logger;

    public MatchController(ILogger<MatchController>? logger)
    {
        _logger = logger;
    }
}