namespace SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

/// <summary>
/// Renders notification templates with variable substitution.
/// </summary>
public interface ITemplateRenderer
{
    Task<RenderedTemplate> RenderAsync(string templateCode, string channel, string locale,
        Dictionary<string, object?> variables, CancellationToken ct = default);
}

/// <summary>
/// Result of rendering a template.
/// </summary>
public record RenderedTemplate(string Subject, string Body);
