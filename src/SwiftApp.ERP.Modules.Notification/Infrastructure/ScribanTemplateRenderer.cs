using Microsoft.Extensions.Logging;
using Scriban;
using Scriban.Runtime;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure;

/// <summary>
/// Renders notification templates using the Scriban template engine.
/// Loads templates from DB (NotificationTemplate), renders subject + body with variables.
/// Falls back locale: requested → de → first available.
/// </summary>
public class ScribanTemplateRenderer(
    INotificationTemplateRepository templateRepo,
    ILogger<ScribanTemplateRenderer> logger) : ITemplateRenderer
{
    public async Task<RenderedTemplate> RenderAsync(string templateCode, string channel, string locale,
        Dictionary<string, object?> variables, CancellationToken ct = default)
    {
        // Look up template with locale fallback
        var template = await templateRepo.FindByCodeChannelLocaleAsync(templateCode, channel, locale, ct)
            ?? await templateRepo.FindByCodeChannelLocaleAsync(templateCode, channel, "de", ct);

        if (template is null)
        {
            logger.LogWarning("No template found for Code={Code}, Channel={Channel}, Locale={Locale}",
                templateCode, channel, locale);
            return new RenderedTemplate(
                $"[{templateCode}]",
                $"<p>Template '{templateCode}' not found for channel '{channel}' and locale '{locale}'.</p>");
        }

        var subject = RenderString(template.Subject ?? "", variables);
        var body = RenderString(template.BodyTemplate ?? "", variables);

        return new RenderedTemplate(subject, body);
    }

    private string RenderString(string templateText, Dictionary<string, object?> variables)
    {
        if (string.IsNullOrWhiteSpace(templateText))
            return string.Empty;

        try
        {
            var scribanTemplate = Template.Parse(templateText);
            if (scribanTemplate.HasErrors)
            {
                logger.LogWarning("Template parse errors: {Errors}", string.Join("; ", scribanTemplate.Messages));
                return templateText;
            }

            var scriptObject = new ScriptObject();
            foreach (var (key, value) in variables)
            {
                scriptObject.Add(key, value);
            }

            var context = new TemplateContext();
            context.PushGlobal(scriptObject);

            return scribanTemplate.Render(context);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to render template");
            return templateText;
        }
    }
}
