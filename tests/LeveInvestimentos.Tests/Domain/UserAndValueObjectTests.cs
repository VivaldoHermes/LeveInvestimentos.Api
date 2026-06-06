using System;
using FluentAssertions;
using LeveInvestimentos.Core.Domain.Entities;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Core.Domain.ValueObjects;
using LeveInvestimentos.Tests.TestSupport;
using Xunit;

namespace LeveInvestimentos.Tests.Domain;

public sealed class UserAndValueObjectTests
{
    [Fact]
    public void UserCreate_ForSubordinate_SetsManagerAndRequiresPasswordChange()
    {
        var user = TestData.Subordinate();

        user.Role.Should().Be(UserRole.Subordinate);
        user.ManagerId.Should().Be(TestData.ManagerId);
        user.MustChangePassword.Should().BeTrue();
        user.UserName.Should().Be(user.Email);
    }

    [Fact]
    public void UserCreate_ForManagerWithManagerId_Throws()
    {
        Action act = () => User.Create(
            "Gestor",
            new DateOnly(1990, 1, 1),
            "gestor@example.com",
            TestData.Address(),
            "(11) 3333-4444",
            "(11) 99999-8888",
            string.Empty,
            UserRole.Manager,
            new DateOnly(2026, 6, 6),
            TestData.OtherManagerId);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("managerId");
    }

    [Fact]
    public void UserCreate_WithFutureBirthDate_Throws()
    {
        Action act = () => User.Create(
            "Usuario",
            new DateOnly(2026, 6, 7),
            "usuario@example.com",
            TestData.Address(),
            "(11) 3333-4444",
            "(11) 99999-8888",
            string.Empty,
            UserRole.Manager,
            new DateOnly(2026, 6, 6));

        act.Should().Throw<ArgumentException>()
            .WithParameterName("birthDate");
    }

    [Theory]
    [InlineData("sp", "SP")]
    [InlineData("RJ", "RJ")]
    public void Address_NormalizesState(string state, string expected)
    {
        var address = new Address("Rua", "1", "Cidade", state);

        address.State.Should().Be(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData("S")]
    [InlineData("123")]
    public void Address_WithInvalidState_Throws(string state)
    {
        Action act = () => new Address("Rua", "1", "Cidade", state);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("state");
    }

    [Fact]
    public void PhoneNumber_WithFormattedValue_StoresValueAndDigits()
    {
        var phone = new PhoneNumber("(11) 99999-8888");

        phone.Value.Should().Be("(11) 99999-8888");
        phone.Digits.Should().Be("11999998888");
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("123")]
    [InlineData("")]
    public void PhoneNumber_WithInvalidValue_Throws(string value)
    {
        Action act = () => new PhoneNumber(value);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("value");
    }
}
