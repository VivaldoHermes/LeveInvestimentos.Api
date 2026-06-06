using System;
using System.Threading.Tasks;
using FluentAssertions;
using LeveInvestimentos.Core.Application.Users.Commands;
using LeveInvestimentos.Core.Application.Users.Services;
using LeveInvestimentos.Core.Domain.Enums;
using LeveInvestimentos.Tests.TestSupport;
using Xunit;

namespace LeveInvestimentos.Tests.Application;

public sealed class UserServiceTests
{
    private readonly FakeUserRepository _users = new();
    private readonly FakeUnitOfWork _unitOfWork = new();
    private readonly FakePasswordGenerator _passwordGenerator = new();
    private readonly FakeEmailOutbox _emailOutbox = new();

    [Fact]
    public async Task CreateAsync_ForValidSubordinate_AddsUserSendsCredentialsAndSaves()
    {
        _users.AddExisting(TestData.Manager());
        _passwordGenerator.Password = "Temp!123456";
        var service = CreateService();

        var result = await service.CreateAsync(ValidCommand());

        result.IsSuccess.Should().BeTrue();
        _users.AddedUsers.Should().HaveCount(2);
        _users.LastPassword.Should().Be("Temp!123456");
        _emailOutbox.Messages.Should().ContainSingle(message =>
            message.To == "novo@example.com"
            && message.Subject == "Credenciais de acesso"
            && message.Body.Contains("Temp!123456", StringComparison.Ordinal));
        _unitOfWork.SaveChangesCalls.Should().Be(1);
        result.Value.MustChangePassword.Should().BeTrue();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateEmail_ReturnsFailureBeforeAddingUser()
    {
        _users.MarkEmailAsExisting("novo@example.com");
        var service = CreateService();

        var result = await service.CreateAsync(ValidCommand());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Users.EmailAlreadyExists");
        _users.AddedUsers.Should().BeEmpty();
        _emailOutbox.Messages.Should().BeEmpty();
        _unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task CreateAsync_ForSubordinateWithInvalidManager_ReturnsFailure()
    {
        var service = CreateService();

        var result = await service.CreateAsync(ValidCommand());

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Users.InvalidManager");
        _unitOfWork.SaveChangesCalls.Should().Be(0);
    }

    [Fact]
    public async Task GetByIdAsync_WithEmptyId_ReturnsFailureWithoutRepositoryData()
    {
        var service = CreateService();

        var result = await service.GetByIdAsync(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Users.IdRequired");
    }

    private UserService CreateService()
    {
        return new UserService(_users, _unitOfWork, _passwordGenerator, _emailOutbox);
    }

    private static CreateUserCommand ValidCommand()
    {
        return new CreateUserCommand(
            "Novo Subordinado",
            new DateOnly(1998, 3, 3),
            "novo@example.com",
            "Rua Nova",
            "10",
            "Sao Paulo",
            "SP",
            "(11) 3333-1111",
            "(11) 99999-1111",
            string.Empty,
            TestData.ManagerId,
            UserRole.Subordinate);
    }
}
