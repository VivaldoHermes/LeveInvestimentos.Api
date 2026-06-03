using System;
using System.Net.Mail;
using LeveInvestimentos.Core.Application.Common;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Core.Domain.ValueObjects;

namespace LeveInvestimentos.Core.Application.Users.Commands;

public static class CreateUserCommandValidator
{
    public static Result Validate(CreateUserCommand command, DateOnly today)
    {
        if (string.IsNullOrWhiteSpace(command.FullName))
        {
            return Result.Failure(new Error("Users.FullNameRequired", "Full name is required."));
        }

        if (command.FullName.Trim().Length > 200)
        {
            return Result.Failure(new Error("Users.FullNameTooLong", "Full name cannot exceed 200 characters."));
        }

        if (!IsValidEmail(command.Email))
        {
            return Result.Failure(new Error("Users.InvalidEmail", "A valid e-mail is required."));
        }

        if (command.BirthDate > today)
        {
            return Result.Failure(new Error("Users.BirthDateInFuture", "Birth date cannot be in the future."));
        }

        if (command.Role == UserRole.Subordinate && (command.ManagerId is null || command.ManagerId == Guid.Empty))
        {
            return Result.Failure(new Error("Users.ManagerRequired", "Subordinate users must have a manager."));
        }

        if (command.Role == UserRole.Manager && command.ManagerId is not null)
        {
            return Result.Failure(new Error("Users.ManagerCannotHaveManager", "Manager users cannot have a manager."));
        }

        try
        {
            _ = new Address(
                command.Street,
                command.Number,
                command.Complement,
                command.Neighborhood,
                command.City,
                command.State,
                command.ZipCode);

            if (!string.IsNullOrWhiteSpace(command.LandlinePhone))
            {
                _ = new PhoneNumber(command.LandlinePhone);
            }

            _ = new PhoneNumber(command.MobilePhone);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure(new Error("Users.InvalidContactData", ex.Message));
        }

        return Result.Success();
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var parsed = new MailAddress(email.Trim());
            return parsed.Address.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
