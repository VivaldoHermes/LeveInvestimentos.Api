using System;
using System.Threading.Tasks;
using FluentAssertions;
using LeveInvestimentos.Core.Application.Account.Commands;
using LeveInvestimentos.Core.Application.Account.Services;
using LeveInvestimentos.Tests.TestSupport;
using Xunit;

namespace LeveInvestimentos.Tests.Application;

public sealed class AccountServiceTests
{
    [Fact]
    public async Task LoginAsync_WithBlankPassword_ReturnsInvalidCredentialsWithoutGatewayCall()
    {
        var gateway = new FakeAccountIdentityGateway();
        var service = new AccountService(gateway);

        var result = await service.LoginAsync(new LoginCommand("user@example.com", " ", true));

        result.IsFailure.Should().BeTrue();
        gateway.SignInCalls.Should().Be(0);
    }

    [Fact]
    public async Task LoginAsync_WithValidData_TrimsEmailAndDelegatesToGateway()
    {
        var gateway = new FakeAccountIdentityGateway();
        var service = new AccountService(gateway);

        var result = await service.LoginAsync(new LoginCommand(" user@example.com ", "Senha!123", true));

        result.IsSuccess.Should().BeTrue();
        gateway.SignInCalls.Should().Be(1);
        gateway.LastEmail.Should().Be("user@example.com");
        gateway.LastPassword.Should().Be("Senha!123");
        gateway.LastRememberMe.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordAsync_WithEmptyUser_ReturnsFailureWithoutGatewayCall()
    {
        var gateway = new FakeAccountIdentityGateway();
        var service = new AccountService(gateway);

        var result = await service.ChangePasswordAsync(new ChangePasswordCommand(Guid.Empty, "Atual!123", "Nova!123"));

        result.IsFailure.Should().BeTrue();
        gateway.ChangePasswordCalls.Should().Be(0);
    }

    [Fact]
    public async Task MustChangePasswordAsync_WithValidUser_DelegatesToGateway()
    {
        var gateway = new FakeAccountIdentityGateway();
        var service = new AccountService(gateway);

        var result = await service.MustChangePasswordAsync(TestData.SubordinateId);

        result.IsSuccess.Should().BeTrue();
        gateway.MustChangePasswordCalls.Should().Be(1);
        gateway.LastUserId.Should().Be(TestData.SubordinateId);
    }
}
