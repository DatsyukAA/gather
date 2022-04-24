using DotaCG.Controllers;
using Xunit;

namespace DotaCG.Tests;

public class MatchControllerTests
{
    private MatchController Controller { get; } = new(null);

    [Fact]
    public void GetMatch()
    {
        var result = Controller.GetMatch(1L);
        Assert.True(false);
    }

    [Fact]
    public void GetMatches()
    {
        var result = Controller.GetMatches(1L);
        Assert.True(false);
    }
}