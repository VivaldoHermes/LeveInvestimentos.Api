using System;
using System.Linq;

namespace LeveInvestimentos.Core.Domain.ValueObjects;

public sealed class Address
{
    private Address()
    {
        Street = string.Empty;
        Number = string.Empty;
        City = string.Empty;
        State = string.Empty;
    }

    public Address(
        string street,
        string number,
        string city,
        string state)
    {
        Street = Required(street, nameof(street), 200);
        Number = Required(number, nameof(number), 20);
        City = Required(city, nameof(city), 100);
        State = ValidateState(state);
    }

    public string Street { get; private set; }

    public string Number { get; private set; }

    public string City { get; private set; }

    public string State { get; private set; }

    private static string Required(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(GetRequiredMessage(parameterName), parameterName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException(GetMaxLengthMessage(parameterName, maxLength), parameterName);
        }

        return trimmed;
    }

    private static string GetRequiredMessage(string parameterName) => parameterName switch
    {
        "street" => "O logradouro é obrigatório.",
        "number" => "O número é obrigatório.",
        "city" => "A cidade é obrigatória.",
        "state" => "O estado é obrigatório.",
        _ => "Campo de endereço obrigatório."
    };

    private static string GetMaxLengthMessage(string parameterName, int maxLength) => parameterName switch
    {
        "street" => $"O logradouro não pode exceder {maxLength} caracteres.",
        "number" => $"O número não pode exceder {maxLength} caracteres.",
        "city" => $"A cidade não pode exceder {maxLength} caracteres.",
        _ => $"O campo de endereço não pode exceder {maxLength} caracteres."
    };

    private static string ValidateState(string state)
    {
        var trimmed = Required(state, nameof(state), 2).ToUpperInvariant();
        if (trimmed.Length != 2 || !trimmed.All(char.IsLetter))
        {
            throw new ArgumentException("Use a sigla do estado com 2 letras.", nameof(state));
        }

        return trimmed;
    }
}
