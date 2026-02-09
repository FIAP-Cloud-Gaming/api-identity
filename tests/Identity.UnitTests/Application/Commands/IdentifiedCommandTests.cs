using Identity.API.Application.Commands.Identified;
using Identity.API.Application.Commands.RegisterUser;
using Identity.API.Application.Commands.ChangePassword;

namespace Identity.UnitTests.Application.Commands;

public sealed class IdentifiedCommandTests
{
    [Fact]
    public void IdentifiedCommand_WithResponse_StoresCommandAndId()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "user@example.com",
            "P@ssw0rd123",
            "John",
            "Doe");
        var id = Guid.NewGuid();

        // Act
        var identifiedCommand = new IdentifiedCommand<RegisterUserCommand, RegisterUserResult>(command, id);

        // Assert
        identifiedCommand.Command.Should().Be(command);
        identifiedCommand.Id.Should().Be(id);
        identifiedCommand.Command.Email.Should().Be("user@example.com");
        identifiedCommand.Command.FirstName.Should().Be("John");
    }

    [Fact]
    public void IdentifiedCommand_WithoutResponse_StoresCommandAndId()
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");
        var id = Guid.NewGuid();

        // Act
        var identifiedCommand = new IdentifiedCommand<ChangePasswordCommand>(command, id);

        // Assert
        identifiedCommand.Command.Should().Be(command);
        identifiedCommand.Id.Should().Be(id);
        identifiedCommand.Command.UserId.Should().Be(1);
        identifiedCommand.Command.NewPassword.Should().Be("NewP@ssw0rd123");
    }

    [Fact]
    public void IdentifiedCommand_WithResponse_PropertiesAreReadOnly()
    {
        // Arrange
        var command = new RegisterUserCommand(
            "user@example.com",
            "P@ssw0rd123",
            "John",
            "Doe");
        var id = Guid.NewGuid();

        // Act
        var identifiedCommand = new IdentifiedCommand<RegisterUserCommand, RegisterUserResult>(command, id);

        // Assert
        identifiedCommand.GetType().GetProperty("Command")!.CanWrite.Should().BeFalse();
        identifiedCommand.GetType().GetProperty("Id")!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void IdentifiedCommand_WithoutResponse_PropertiesAreReadOnly()
    {
        // Arrange
        var command = new ChangePasswordCommand(1, "OldP@ss", "NewP@ssw0rd123");
        var id = Guid.NewGuid();

        // Act
        var identifiedCommand = new IdentifiedCommand<ChangePasswordCommand>(command, id);

        // Assert
        identifiedCommand.GetType().GetProperty("Command")!.CanWrite.Should().BeFalse();
        identifiedCommand.GetType().GetProperty("Id")!.CanWrite.Should().BeFalse();
    }

    [Fact]
    public void IdentifiedCommand_WithResponse_DifferentInstances_AreDifferent()
    {
        // Arrange
        var command1 = new RegisterUserCommand("user1@example.com", "P@ssw0rd123", "John", "Doe");
        var command2 = new RegisterUserCommand("user2@example.com", "P@ssw0rd123", "Jane", "Smith");
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        // Act
        var identifiedCommand1 = new IdentifiedCommand<RegisterUserCommand, RegisterUserResult>(command1, id1);
        var identifiedCommand2 = new IdentifiedCommand<RegisterUserCommand, RegisterUserResult>(command2, id2);

        // Assert
        identifiedCommand1.Id.Should().NotBe(identifiedCommand2.Id);
        identifiedCommand1.Command.Should().NotBe(identifiedCommand2.Command);
    }
}
