using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Notification.Application.DTOs;
using SwiftApp.ERP.Modules.Notification.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Interfaces;

namespace SwiftApp.ERP.Modules.Notification.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Produces("application/json")]
[Authorize]
public class NotificationController(NotificationService notificationService, ICurrentUserService currentUser) : ControllerBase
{
    /// <summary>Current user's notifications (paged).</summary>
    [HttpGet("me")]
    [ProducesResponseType<PagedResult<NotificationResponse>>(200)]
    public async Task<IActionResult> GetMy([FromQuery] int page = 1, [FromQuery] int size = 20, [FromQuery] bool? unreadOnly = null, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        if (userId is null) return Unauthorized();
        var result = await notificationService.GetByUserAsync(userId.Value, page, size, unreadOnly, ct);
        return Ok(result);
    }

    /// <summary>Current user's unread notification count.</summary>
    [HttpGet("me/unread-count")]
    [ProducesResponseType<long>(200)]
    public async Task<IActionResult> MyUnreadCount(CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId is null) return Unauthorized();
        var count = await notificationService.CountUnreadAsync(userId.Value, ct);
        return Ok(count);
    }

    /// <summary>Current user's recent notifications for bell dropdown.</summary>
    [HttpGet("me/recent")]
    [ProducesResponseType<List<NotificationResponse>>(200)]
    public async Task<IActionResult> MyRecent([FromQuery] int count = 5, CancellationToken ct = default)
    {
        var userId = currentUser.UserId;
        if (userId is null) return Unauthorized();
        var items = await notificationService.GetRecentByUserAsync(userId.Value, count, ct);
        return Ok(items);
    }

    /// <summary>Mark all current user's notifications as read.</summary>
    [HttpPost("me/mark-all-read")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        var userId = currentUser.UserId;
        if (userId is null) return Unauthorized();
        await notificationService.MarkAllReadAsync(userId.Value, ct);
        return NoContent();
    }

    /// <summary>Mark a single notification as read.</summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await notificationService.MarkReadAsync(id, ct);
        return NoContent();
    }

    /// <summary>Dismiss (soft-delete) a notification.</summary>
    [HttpPost("{id:guid}/dismiss")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Dismiss(Guid id, CancellationToken ct)
    {
        await notificationService.DismissAsync(id, ct);
        return NoContent();
    }

    /// <summary>Get notifications by user ID (admin use).</summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Policy = "NOTIFICATION:MANAGE")]
    [ProducesResponseType<PagedResult<NotificationResponse>>(200)]
    public async Task<IActionResult> GetByUser(Guid userId, [FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
    {
        var result = await notificationService.GetByUserAsync(userId, page, size, null, ct);
        return Ok(result);
    }
}

[ApiController]
[Route("api/v1/notifications/campaigns")]
[Produces("application/json")]
[Authorize(Policy = "NOTIFICATION:MANAGE")]
public class MailCampaignController(MailCampaignService campaignService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<MailCampaignResponse>>(200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int size = 20, CancellationToken ct = default)
    {
        var result = await campaignService.GetPagedAsync(page, size, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<MailCampaignResponse>(200)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await campaignService.GetByIdAsync(id, ct);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType<MailCampaignResponse>(201)]
    public async Task<IActionResult> Create([FromBody] MailCampaignRequest request, CancellationToken ct)
    {
        var result = await campaignService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
