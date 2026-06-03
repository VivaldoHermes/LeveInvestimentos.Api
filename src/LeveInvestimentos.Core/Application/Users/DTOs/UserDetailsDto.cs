using System;
using LeveInvestimentos.Core.Domain.Enums;

namespace LeveInvestimentos.Core.Application.Users.DTOs;

public sealed record UserDetailsDto(
    Guid Id,
    string FullName,
    DateOnly BirthDate,
    string Email,
    UserRole Role,
    Guid? ManagerId,
    string? ManagerName,
    string Street,
    string Number,
    string Complement,
    string Neighborhood,
    string City,
    string State,
    string ZipCode,
    string? LandlinePhone,
    string MobilePhone,
    string? ProfilePhotoPath,
    bool MustChangePassword);
