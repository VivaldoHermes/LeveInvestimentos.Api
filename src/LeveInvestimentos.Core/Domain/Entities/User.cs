using System;
using System.Collections.Generic;
using System.Net.Mail;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Core.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace LeveInvestimentos.Core.Domain.Entities;

public sealed class User : IdentityUser<Guid>
{
    private User()
    {
        FullName = string.Empty;
        Address = null!;
        LandlinePhone = null!;
        MobilePhone = null!;
        ProfilePhotoStorageKey = string.Empty;
    }

    public string FullName { get; private set; }

    public DateOnly BirthDate { get; private set; }

    public Address Address { get; private set; }

    public PhoneNumber LandlinePhone { get; private set; }

    public PhoneNumber MobilePhone { get; private set; }

    public string ProfilePhotoStorageKey { get; private set; }

    public Guid? ManagerId { get; private set; }

    public User? Manager { get; private set; }

    public ICollection<User> Subordinates { get; } = new List<User>();

    public UserRole Role { get; private set; }

    public bool MustChangePassword { get; private set; } = true;

    public void MarkPasswordAsChanged()
    {
        MustChangePassword = false;
    }

    public static User Create(
        string fullName,
        DateOnly birthDate,
        string email,
        Address address,
        string landlinePhone,
        string mobilePhone,
        string profilePhotoStorageKey,
        UserRole role,
        DateOnly today,
        Guid? managerId = null)
    {
        var normalizedFullName = Required(fullName, nameof(fullName), 200);
        var normalizedEmail = ValidateEmail(email);
        var normalizedProfilePhotoStorageKey = Optional(profilePhotoStorageKey, nameof(profilePhotoStorageKey), 500);
        ValidateBirthDate(birthDate, today);
        ValidateManager(role, managerId);

        return new User
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = normalizedFullName,
            BirthDate = birthDate,
            Address = address ?? throw new ArgumentNullException(nameof(address)),
            LandlinePhone = new PhoneNumber(landlinePhone),
            MobilePhone = new PhoneNumber(mobilePhone),
            ProfilePhotoStorageKey = normalizedProfilePhotoStorageKey,
            ManagerId = role == UserRole.Subordinate ? managerId : null,
            Role = role,
            MustChangePassword = true
        };
    }

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

    private static string Optional(string value, string parameterName, int maxLength)
    {
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException(GetMaxLengthMessage(parameterName, maxLength), parameterName);
        }

        return trimmed;
    }

    private static string GetRequiredMessage(string parameterName) => parameterName switch
    {
        "fullName" => "O nome completo é obrigatório.",
        _ => "Campo obrigatório."
    };

    private static string GetMaxLengthMessage(string parameterName, int maxLength) => parameterName switch
    {
        "fullName" => $"O nome completo não pode exceder {maxLength} caracteres.",
        "profilePhotoStorageKey" => $"A foto não pode exceder {maxLength} caracteres.",
        _ => $"O campo não pode exceder {maxLength} caracteres."
    };

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Informe um e-mail válido.", nameof(email));
        }

        var trimmed = email.Trim();
        try
        {
            var parsed = new MailAddress(trimmed);
            if (parsed.Address.Equals(trimmed, StringComparison.OrdinalIgnoreCase))
            {
                return trimmed;
            }
        }
        catch (FormatException)
        {
        }

        throw new ArgumentException("Informe um e-mail válido.", nameof(email));
    }

    private static void ValidateBirthDate(DateOnly birthDate, DateOnly today)
    {
        if (birthDate > today)
        {
            throw new ArgumentException("A data de nascimento não pode ser futura.", nameof(birthDate));
        }
    }

    private static void ValidateManager(UserRole role, Guid? managerId)
    {
        if (role == UserRole.Subordinate && (managerId is null || managerId == Guid.Empty))
        {
            throw new ArgumentException("Subordinados devem ter um gestor.", nameof(managerId));
        }

        if (role == UserRole.Manager && managerId is not null)
        {
            throw new ArgumentException("Gestores não podem ter um gestor.", nameof(managerId));
        }
    }
}
