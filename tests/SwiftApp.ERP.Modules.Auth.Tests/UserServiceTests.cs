using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using SwiftApp.ERP.Modules.Auth.Application.DTOs;
using SwiftApp.ERP.Modules.Auth.Application.Services;
using SwiftApp.ERP.Modules.Auth.Domain.Entities;
using SwiftApp.ERP.Modules.Auth.Domain.Events;
using SwiftApp.ERP.Modules.Auth.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;
using Xunit;

namespace SwiftApp.ERP.Modules.Auth.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IRoleRepository> _roleRepo = new();
    private readonly Mock<IPublisher> _publisher = new();
    private readonly Mock<ILogger<UserService>> _logger = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepo.Object, _roleRepo.Object, _publisher.Object, _logger.Object);
    }

    private static User CreateTestUser(string username = "testuser", string email = "test@swiftapp.ch") => new()
    {
        Id = Guid.NewGuid(),
        Username = username,
        Email = email,
        PasswordHash = "hashed",
        FirstName = "Test",
        LastName = "User",
        Enabled = true,
        CreatedAt = DateTimeOffset.UtcNow
    };

    // ── GetByIdAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldReturnUser_WhenFoundById()
    {
        var user = CreateTestUser();
        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.GetByIdAsync(user.Id);

        result.Should().NotBeNull();
        result!.Username.Should().Be("testuser");
        result.Email.Should().Be("test@swiftapp.ch");
    }

    [Fact]
    public async Task ShouldReturnNull_WhenUserNotFoundById()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    // ── GetPagedAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ShouldReturnPagedUsers()
    {
        var users = new List<User> { CreateTestUser("user1"), CreateTestUser("user2") };
        _userRepo.Setup(r => r.GetPagedAsync(1, 20, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<User>(users, 1, 20, 2, 1));

        var result = await _sut.GetPagedAsync(1, 20);

        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(2);
    }

    // ── CreateAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldCreateUser_WhenUsernameAndEmailUnique()
    {
        var request = new UserRequest("newuser", "new@swiftapp.ch", "P@ssw0rd!", "New", "User", true, null);

        _userRepo.Setup(r => r.GetByUsernameAsync("newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("new@swiftapp.ch", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.CreateAsync(request);

        result.Username.Should().Be("newuser");
        result.Email.Should().Be("new@swiftapp.ch");
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _publisher.Verify(p => p.Publish(It.IsAny<UserCreatedEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenUsernameAlreadyTaken()
    {
        var request = new UserRequest("existing", "new@swiftapp.ch", "P@ssw0rd!", "New", "User", true, null);
        _userRepo.Setup(r => r.GetByUsernameAsync("existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestUser("existing"));

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "UNIQUE_USERNAME");
    }

    [Fact]
    public async Task ShouldThrow_WhenEmailAlreadyRegistered()
    {
        var request = new UserRequest("newuser", "existing@swiftapp.ch", "P@ssw0rd!", "New", "User", true, null);
        _userRepo.Setup(r => r.GetByUsernameAsync("newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("existing@swiftapp.ch", It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateTestUser());

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "UNIQUE_EMAIL");
    }

    [Fact]
    public async Task ShouldThrow_WhenPasswordMissing()
    {
        var request = new UserRequest("newuser", "new@swiftapp.ch", null, "New", "User", true, null);
        _userRepo.Setup(r => r.GetByUsernameAsync("newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("new@swiftapp.ch", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .Where(e => e.Rule == "PASSWORD_REQUIRED");
    }

    [Fact]
    public async Task ShouldAssignRoles_WhenProvided()
    {
        var request = new UserRequest("newuser", "new@swiftapp.ch", "P@ssw0rd!", "New", "User", true,
            new HashSet<string> { "ADMIN", "SALES" }.AsReadOnly());

        _userRepo.Setup(r => r.GetByUsernameAsync("newuser", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("new@swiftapp.ch", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _roleRepo.Setup(r => r.GetByNamesAsync(It.IsAny<IEnumerable<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Role { Name = "ADMIN" }, new Role { Name = "SALES" }]);

        var result = await _sut.CreateAsync(request);

        result.Roles.Should().Contain("ADMIN").And.Contain("SALES");
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldUpdateUser_WhenFound()
    {
        var user = CreateTestUser();
        var request = new UserRequest("updatedname", "updated@swiftapp.ch", null, "Updated", "Name", true, null);

        _userRepo.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _userRepo.Setup(r => r.GetByUsernameAsync("updatedname", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        _userRepo.Setup(r => r.GetByEmailAsync("updated@swiftapp.ch", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.UpdateAsync(user.Id, request);

        result.Username.Should().Be("updatedname");
        _userRepo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldThrow_WhenUpdatingNonExistentUser()
    {
        _userRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var act = () => _sut.UpdateAsync(Guid.NewGuid(), new UserRequest("u", "e@e.ch", null, "F", "L", true, null));

        await act.Should().ThrowAsync<EntityNotFoundException>();
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task ShouldSoftDeleteUser()
    {
        var userId = Guid.NewGuid();

        await _sut.DeleteAsync(userId);

        _userRepo.Verify(r => r.SoftDeleteAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── AuthenticateAsync ───────────────────────────────────────────────

    [Fact]
    public async Task ShouldReturnNull_WhenUserNotFound()
    {
        _userRepo.Setup(r => r.GetByUsernameAsync("nobody", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _sut.AuthenticateAsync("nobody", "password");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldReturnNull_WhenUserDisabled()
    {
        var user = CreateTestUser();
        user.Enabled = false;
        _userRepo.Setup(r => r.GetByUsernameAsync(user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.AuthenticateAsync(user.Username, "password");

        result.Should().BeNull();
    }

    [Fact]
    public async Task ShouldReturnNull_WhenUserLocked()
    {
        var user = CreateTestUser();
        user.Locked = true;
        _userRepo.Setup(r => r.GetByUsernameAsync(user.Username, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _sut.AuthenticateAsync(user.Username, "password");

        result.Should().BeNull();
    }
}
