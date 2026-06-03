using System;
using System.Linq;

namespace LeveInvestimentos.Core.Domain.ValueObjects;

public sealed class Address
{
    private Address()
    {
        Street = string.Empty;
        Number = string.Empty;
        Complement = string.Empty;
        Neighborhood = string.Empty;
        City = string.Empty;
        State = string.Empty;
        ZipCode = string.Empty;
    }

    public Address(
        string street,
        string number,
        string? complement,
        string neighborhood,
        string city,
        string state,
        string zipCode)
    {
        Street = Required(street, nameof(street), 200);
        Number = Required(number, nameof(number), 20);
        Complement = Optional(complement, 100);
        Neighborhood = Required(neighborhood, nameof(neighborhood), 100);
        City = Required(city, nameof(city), 100);
        State = ValidateState(state);
        ZipCode = ValidateZipCode(zipCode);
    }

    public string Street { get; private set; }

    public string Number { get; private set; }

    public string Complement { get; private set; }

    public string Neighborhood { get; private set; }

    public string City { get; private set; }

    public string State { get; private set; }

    public string ZipCode { get; private set; }

    private static string Required(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Address field is required.", parameterName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Address field cannot exceed {maxLength} characters.", parameterName);
        }

        return trimmed;
    }

    private static string Optional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"Address field cannot exceed {maxLength} characters.", nameof(value));
        }

        return trimmed;
    }

    private static string ValidateState(string state)
    {
        var trimmed = Required(state, nameof(state), 2).ToUpperInvariant();
        if (trimmed.Length != 2 || !trimmed.All(char.IsLetter))
        {
            throw new ArgumentException("State must use the two-letter abbreviation.", nameof(state));
        }

        return trimmed;
    }

    private static string ValidateZipCode(string zipCode)
    {
        var trimmed = Required(zipCode, nameof(zipCode), 9);
        var digits = new string(trimmed.Where(char.IsDigit).ToArray());
        if (digits.Length != 8)
        {
            throw new ArgumentException("Zip code must contain 8 digits.", nameof(zipCode));
        }

        return trimmed;
    }
}
