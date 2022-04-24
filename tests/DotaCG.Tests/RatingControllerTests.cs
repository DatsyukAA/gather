using System.Linq;
using DotaCG.Controllers;
using Xunit;

namespace DotaCG.Tests;

public class RatingControllerTests
{
    private RatingController Controller { get; } = new(null);

    [Fact]
    public void GetPlayerRating()
    {
        var result = Controller.GetPlayerRating(100);
        Assert.Equal(1, result);
    }

    [Fact]
    public void GetPlayersRating()
    {
        var result = Controller.GetPlayersRating(new long[] { 1, 2, 3 });
        var enumerable = result as long[] ?? result.ToArray();
        Assert.Equal(3, enumerable.Length);
        Assert.Equal(new long[] { 1, 1, 1 }, enumerable);
    }
}