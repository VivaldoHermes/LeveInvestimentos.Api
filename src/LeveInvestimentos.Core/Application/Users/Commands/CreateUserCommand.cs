using System;
using LeveInvestimentos.Core.Domain.Enums;

namespace LeveInvestimentos.Core.Application.Users.Commands;

public sealed record CreateUserCommand(
    string FullName,
    DateOnly BirthDate,
    string Email,
    string Street,
    string Number,
    string City,
    string State,
    string LandlinePhone,
    string MobilePhone,
    string ProfilePhotoStorageKey,
    Guid? ManagerId,
    UserRole Role);
