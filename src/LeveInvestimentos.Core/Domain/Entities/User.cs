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
        ProfilePhotoPath = string.Empty;
    }

    public string FullName { get; private set; }

    public DateOnly BirthDate { get; private set; }

    public Address Address { get; private set; }

    public PhoneNumber LandlinePhone { get; private set; }

    public PhoneNumber MobilePhone { get; private set; }

    public string ProfilePhotoPath { get; private set; }

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
        string profilePhotoPath,
        UserRole role,
        DateOnly today,
        Guid? managerId = null)
    {
        var normalizedFullName = Required(fullName, nameof(fullName), 200);
        var normalizedEmail = ValidateEmail(email);
        var normalizedProfilePhotoPath = Required(profilePhotoPath, nameof(profilePhotoPath), 500);
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
            ProfilePhotoPath = normalizedProfilePhotoPath,
            ManagerId = role == UserRole.Subordinate ? managerId : null,
            Role = role,
            MustChangePassword = true
        };
    }

    private static string Required(string value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("User field is required.", parameterName);
        }

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
        {
            throw new ArgumentException($"User field cannot exceed {maxLength} characters.", parameterName);
        }

        return trimmed;
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("A valid e-mail is required.", nameof(email));
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

        throw new ArgumentException("A valid e-mail is required.", nameof(email));
    }

    private static void ValidateBirthDate(DateOnly birthDate, DateOnly today)
    {
        if (birthDate > today)
        {
            throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));
        }
    }

    private static void ValidateManager(UserRole role, Guid? managerId)
    {
        if (role == UserRole.Subordinate && (managerId is null || managerId == Guid.Empty))
        {
            throw new ArgumentException("Subordinate users must have a manager.", nameof(managerId));
        }

        if (role == UserRole.Manager && managerId is not null)
        {
            throw new ArgumentException("Manager users cannot have a manager.", nameof(managerId));
        }
    }
}
