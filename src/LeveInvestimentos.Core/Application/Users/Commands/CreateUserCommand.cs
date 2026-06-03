using System;
using LeveInvestimentos.Core.Domain.Enums;

namespace LeveInvestimentos.Core.Application.Users.Commands;

public sealed record CreateUserCommand(
    string FullName,
    DateOnly BirthDate,
    string Email,
    string Street,
    string Number,
    string? Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string? LandlinePhone,
    string MobilePhone,
    string? ProfilePhotoPath,
    Guid? ManagerId,
    UserRole Role);
