using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SwiftApp.ERP.Modules.Notification.Application.DTOs;
using SwiftApp.ERP.Modules.Notification.Application.Services;
using SwiftApp.ERP.Modules.Notification.Application.Validators;
using SwiftApp.ERP.Modules.Notification.Domain.Interfaces;
using SwiftApp.ERP.Modules.Notification.Infrastructure;
using SwiftApp.ERP.Modules.Notification.Infrastructure.Jobs;
using SwiftApp.ERP.Modules.Notification.Infrastructure.Persistence.Repositories;

namespace SwiftApp.ERP.Modules.Notification;

public static class NotificationModuleRegistration
{
    public static IServiceCollection AddNotificationModule(this IServiceCollection services)
    {
        // Repositories
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<IMailCampaignRepository, MailCampaignRepository>();

        // Services
        services.AddScoped<NotificationService>();
        services.AddScoped<MailCampaignService>();

        // Infrastructure services
        services.AddScoped<IEmailService, MailKitEmailService>();
        services.AddScoped<ITemplateRenderer, ScribanTemplateRenderer>();
        services.AddScoped<INotificationDispatcher, NotificationDispatcher>();

        // SignalR hub service
        services.AddScoped<NotificationHubService>();

        // Module API facade
        services.AddScoped<INotificationModuleApi, NotificationModuleApiFacade>();

        // Validators
        services.AddScoped<IValidator<MailCampaignRequest>, MailCampaignRequestValidator>();

        return services;
    }

    /// <summary>
    /// Registers Quartz background jobs for the Notification module.
    /// Call this after AddQuartz() in the host project.
    /// </summary>
    public static void AddNotificationJobs(this IServiceCollectionQuartzConfigurator q)
    {
        // Process pending email notifications — every 30 seconds
        q.AddJob<ProcessPendingNotificationsJob>(opts => opts
            .WithIdentity(ProcessPendingNotificationsJob.JobKey));
        q.AddTrigger(opts => opts
            .ForJob(ProcessPendingNotificationsJob.JobKey)
            .WithIdentity($"{ProcessPendingNotificationsJob.JobKey}-trigger")
            .WithSimpleSchedule(s => s.WithIntervalInSeconds(30).RepeatForever()));

        // Retry failed notifications — every 5 minutes
        q.AddJob<RetryFailedNotificationsJob>(opts => opts
            .WithIdentity(RetryFailedNotificationsJob.JobKey));
        q.AddTrigger(opts => opts
            .ForJob(RetryFailedNotificationsJob.JobKey)
            .WithIdentity($"{RetryFailedNotificationsJob.JobKey}-trigger")
            .WithSimpleSchedule(s => s.WithIntervalInMinutes(5).RepeatForever()));

        // Process mail campaigns — every minute
        q.AddJob<ProcessMailCampaignJob>(opts => opts
            .WithIdentity(ProcessMailCampaignJob.JobKey));
        q.AddTrigger(opts => opts
            .ForJob(ProcessMailCampaignJob.JobKey)
            .WithIdentity($"{ProcessMailCampaignJob.JobKey}-trigger")
            .WithSimpleSchedule(s => s.WithIntervalInMinutes(1).RepeatForever()));
    }
}
