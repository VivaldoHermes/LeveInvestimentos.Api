using System;
using System.Reflection;
using LeveInvestimentos.Core.Domain.Entities;

namespace LeveInvestimentos.Tests.TestSupport;

internal static class EntityTestExtensions
{
    public static User WithId(this User user, Guid id)
    {
        SetProperty(user, nameof(User.Id), id);
        return user;
    }

    private static void SetProperty<T>(object instance, string propertyName, T value)
    {
        var property = instance.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (property is null)
        {
            throw new InvalidOperationException($"Property '{propertyName}' was not found.");
        }

        property.SetValue(instance, value);
    }
}
