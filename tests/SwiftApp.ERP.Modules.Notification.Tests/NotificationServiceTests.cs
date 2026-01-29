using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using SwiftApp.ERP.Modules.Notification.Application.Services;
using SwiftApp.ERP.Modules.Notification.Domain.Enums;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using Xunit;
using NotificationEntity = SwiftApp.ERP.Modules.Notification.Domain.Entities.Notification;

namespace SwiftApp.ERP.Modules.Notification.Tests;

public class NotificationServiceTests
{
    private readonly Mock<INotificationRepository> _notificationRepo = new();
    private readonly Mock<ILogger<NotificationService>> _logger = new();
    private readonly NotificationService _sut;

    public NotificationServiceTests()
    {
        _sut = new NotificationService(_notificationRepo.Object, _logger.Object);
    }

    [Fact]
    public async Task ShouldCreateNotification()
    {
        var userId = Guid.NewGuid();

        await _sut.CreateAsync(userId, "user@swisstime.ch", "ORDER_CREATED",
            "New Order", "A new order has been placed.", "SalesOrder", Guid.NewGuid(), CancellationToken.None);

        _notificationRepo.Verify(r => r.AddAsync(
            It.Is<NotificationEntity>(n =>
                n.RecipientUserId == userId &&
                n.Subject == "New Order" &&
                n.Channel == NotificationChannel.InApp &&
                n.Status == NotificationStatus.Pending),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnUnreadCount()
    {
        var userId = Guid.NewGuid();
        _notificationRepo.Setup(r => r.CountUnreadAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(5);

        var count = await _sut.CountUnreadAsync(userId, CancellationToken.None);

        count.Should().Be(5);
    }

    [Fact]
    public async Task ShouldMarkAllRead()
    {
        var userId = Guid.NewGuid();

        await _sut.MarkAllReadAsync(userId, CancellationToken.None);

        _notificationRepo.Verify(r => r.MarkAllReadAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldReturnPagedNotifications()
    {
        var userId = Guid.NewGuid();
        var notifications = new List<NotificationEntity>
        {
            new() { Id = Guid.NewGuid(), RecipientUserId = userId, Subject = "Order 1", Channel = NotificationChannel.InApp, Status = NotificationStatus.Sent, CreatedAt = DateTimeOffset.UtcNow },
            new() { Id = Guid.NewGuid(), RecipientUserId = userId, Subject = "Order 2", Channel = NotificationChannel.InApp, Status = NotificationStatus.Pending, CreatedAt = DateTimeOffset.UtcNow }
        };
        var pagedResult = new PagedResult<NotificationEntity>(notifications, 1, 20, 2, 1);

        _notificationRepo.Setup(r => r.GetPagedByUserAsync(userId, 1, 20, It.IsAny<bool?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResult);

        var result = await _sut.GetByUserAsync(userId, 1, 20, null, CancellationToken.None);

        result.TotalItems.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }
}
