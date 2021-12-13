using System.Security.Cryptography;
using System.Text;

namespace Security;

/// <summary>
/// Provides methods for authentication.
/// </summary>
public static class Authentication
{
    /// <summary>
    /// Generates salt by 2 strings and date.
    /// </summary>
    /// <param name="keyword1">first word (login).</param>
    /// <param name="keyword2">second word (password hash).</param>
    /// <param name="creationDate">Date (user creation date).</param>
    /// <param name="algorithm">Hash algorithm.</param>
    /// <param name="enc">Default encoding.</param>
    /// <returns>Salt string. Default - MD5.</returns>
    public static string GenerateSalt(string keyword1, string keyword2 = "", DateTime? creationDate = default, HashAlgorithm? algorithm = default, Encoding? enc = default)
    {
        if (algorithm == default(HashAlgorithm)) algorithm = MD5.Create();
        if (enc == default(Encoding)) enc = Encoding.UTF8;
        if (string.IsNullOrEmpty(keyword1))
            throw new ArgumentNullException(nameof(keyword1));
        if (string.IsNullOrEmpty(keyword2))
            throw new ArgumentNullException(nameof(keyword2));
        if (creationDate != null && creationDate >= DateTime.Now)
            throw new ArgumentException($"{nameof(creationDate)} cannot be grater then current date");
        return String.Concat(algorithm
                .ComputeHash(enc.GetBytes($"{keyword1}_{keyword2}_{creationDate?.Ticks.ToString() ?? ""}"))
                .Select(item => item.ToString("x2")));
    }

    /// <summary>
    /// Generates hash of string.
    /// </summary>
    /// <param name="source">source string.</param>
    /// <param name="algorithm">Hash algorithm.</param>
    /// <returns>Password hash. Default - SHA256.</returns>
    public static string HashPassword(string source, HashAlgorithm? algorithm = default)
    {
        if (algorithm == default(HashAlgorithm)) algorithm = SHA256.Create();
        return string.Join("", algorithm.ComputeHash(Encoding.UTF8.GetBytes(source)).Select(item => item.ToString("x2")));
    }
}
