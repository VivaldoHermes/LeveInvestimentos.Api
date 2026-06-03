using System;
using System.Collections.Generic;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Core.Domain.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace LeveInvestimentos.Core.Domain.Entities;

public sealed class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    public Address? Address { get; set; }

    public PhoneNumber? LandlinePhone { get; set; }

    public PhoneNumber? MobilePhone { get; set; }

    public string? ProfilePhotoPath { get; set; }

    public Guid? ManagerId { get; set; }

    public User? Manager { get; set; }

    public ICollection<User> Subordinates { get; } = new List<User>();

    public UserRole Role { get; set; }

    public bool MustChangePassword { get; set; } = true;
}
