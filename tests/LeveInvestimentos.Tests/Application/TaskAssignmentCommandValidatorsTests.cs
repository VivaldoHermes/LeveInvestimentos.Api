using System;
using FluentAssertions;
using LeveInvestimentos.Core.Application.Tarefas.Commands;
using LeveInvestimentos.Tests.TestSupport;
using Xunit;

namespace LeveInvestimentos.Tests.Application;

public sealed class TaskAssignmentCommandValidatorsTests
{
    [Fact]
    public void ValidateCreate_WithValidCommand_ReturnsSuccess()
    {
        var command = new CreateTaskAssignmentCommand(
            TestData.ManagerId,
            TestData.SubordinateId,
            "Descricao",
            TestData.DueDate);

        var result = TaskAssignmentCommandValidators.Validate(command, TestData.CreatedAt);

        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ValidateCreate_WithBlankDescription_ReturnsFailure(string description)
    {
        var command = new CreateTaskAssignmentCommand(
            TestData.ManagerId,
            TestData.SubordinateId,
            description,
            TestData.DueDate);

        var result = TaskAssignmentCommandValidators.Validate(command, TestData.CreatedAt);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskAssignments.DescriptionRequired");
    }

    [Fact]
    public void ValidateCreate_WithPastDueDate_ReturnsFailure()
    {
        var command = new CreateTaskAssignmentCommand(
            TestData.ManagerId,
            TestData.SubordinateId,
            "Descricao",
            TestData.CreatedAt);

        var result = TaskAssignmentCommandValidators.Validate(command, TestData.CreatedAt);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskAssignments.InvalidDueDate");
    }

    [Fact]
    public void ValidateStart_WithEmptyUser_ReturnsFailure()
    {
        var result = TaskAssignmentCommandValidators.Validate(
            new StartTaskAssignmentCommand(Guid.NewGuid(), Guid.Empty));

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TaskAssignments.CurrentUserRequired");
    }
}
