using Xunit;
using System;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace Security.Tests;

public class AuthenticationTests
{
    [Fact]
    public void IsSaltGenerationCorrect()
    {
        string result = Authentication.GenerateSalt("login", "passwordHash", new DateTime(2021, 11, 21));

        Assert.Equal(result, "17580776f56de102a01087fd041d7250");
    }

    public static IEnumerable<object?[]> GetPasswordHashTestData()
    {
        yield return new object[] { SHA256.Create(), "c15580d4235128288768dccb287745bb25a6bc4fa01124608c826b8979016a13" };
        yield return new object?[] { null, "c15580d4235128288768dccb287745bb25a6bc4fa01124608c826b8979016a13" };
        yield return new object[] { MD5.Create(), "1c29135e8dc2245721632ee1f5adb22e" };
    }

    [Theory]
    [MemberData(nameof(GetPasswordHashTestData))]
    public void IsPasswordHashCorrect(HashAlgorithm algorithm, string expected)
    {
        string result = Authentication.HashPassword("passwordHash", algorithm);

        Assert.Equal(result, expected);
    }
}