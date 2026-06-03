using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace LeveInvestimentos.Core.Domain.ValueObjects;

public sealed class PhoneNumber
{
    private static readonly Regex AllowedCharacters = new(@"^[\d\s()+\-\.]+$", RegexOptions.Compiled);

    private PhoneNumber()
    {
        Value = string.Empty;
        Digits = string.Empty;
    }

    public PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Phone number is required.", nameof(value));
        }

        var trimmed = value.Trim();
        if (!AllowedCharacters.IsMatch(trimmed))
        {
            throw new ArgumentException("Phone number contains invalid characters.", nameof(value));
        }

        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (digits.Length is < 10 or > 13)
        {
            throw new ArgumentException("Phone number must contain 10 to 13 digits.", nameof(value));
        }

        Value = trimmed;
        Digits = digits;
    }

    public string Value { get; private set; }

    public string Digits { get; private set; }

    public override string ToString()
    {
        return Value;
    }
}
