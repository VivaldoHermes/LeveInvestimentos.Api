using System.Security.Cryptography;
using LeveInvestimentos.Core.Abstractions;

namespace LeveInvestimentos.Infrastructure.Identity;

public sealed class PasswordGenerator : IPasswordGenerator
{
    // Ambiguous characters (0/O, 1/l/I) are intentionally excluded to keep the
    // temporary password readable when the manager relays it to the new user.
    private const string Lowercase = "abcdefghijkmnopqrstuvwxyz";
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Digits = "23456789";
    private const string Special = "!@#$%*?";
    private const int Length = 14;

    public string Generate()
    {
        var allChars = Lowercase + Uppercase + Digits + Special;
        var password = new char[Length];

        // Guarantee the password satisfies the configured Identity password policy
        // (at least one lowercase and one digit) regardless of the random fill.
        password[0] = GetRandomChar(Lowercase);
        password[1] = GetRandomChar(Uppercase);
        password[2] = GetRandomChar(Digits);
        password[3] = GetRandomChar(Special);

        for (var i = 4; i < Length; i++)
        {
            password[i] = GetRandomChar(allChars);
        }

        Shuffle(password);
        return new string(password);
    }

    private static char GetRandomChar(string set)
    {
        return set[RandomNumberGenerator.GetInt32(set.Length)];
    }

    private static void Shuffle(char[] chars)
    {
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
