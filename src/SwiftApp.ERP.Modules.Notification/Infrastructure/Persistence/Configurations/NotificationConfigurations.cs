using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftApp.ERP.Modules.Notification.Domain.Entities;
using SwiftApp.ERP.SharedKernel.Persistence;

namespace SwiftApp.ERP.Modules.Notification.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : BaseEntityConfiguration<Domain.Entities.Notification>
{
    protected override void ConfigureEntity(EntityTypeBuilder<Domain.Entities.Notification> builder)
    {
        builder.ToTable("notifications");

        builder.Property(e => e.RecipientUserId).HasColumnName("recipient_user_id");
        builder.Property(e => e.RecipientEmail).HasMaxLength(255).HasColumnName("recipient_email");
        builder.Property(e => e.TemplateCode).HasMaxLength(100).HasColumnName("template_code");
        builder.Property(e => e.Channel).HasConversion<string>().HasMaxLength(50).HasColumnName("channel");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).HasColumnName("status");
        builder.Property(e => e.Subject).HasMaxLength(500).HasColumnName("subject");
        builder.Property(e => e.Body).HasColumnType("text").HasColumnName("body");
        builder.Property(e => e.ReferenceType).HasMaxLength(100).HasColumnName("reference_type");
        builder.Property(e => e.ReferenceId).HasColumnName("reference_id");
        builder.Property(e => e.ErrorMessage).HasColumnType("text").HasColumnName("error_message");
        builder.Property(e => e.RetryCount).HasColumnName("retry_count");
        builder.Property(e => e.SentAt).HasColumnName("sent_at");
        builder.Property(e => e.ReadAt).HasColumnName("read_at");

        builder.HasIndex(e => e.RecipientUserId);
        builder.HasIndex(e => new { e.RecipientUserId, e.Status });
    }
}

public class NotificationTemplateConfiguration : BaseEntityConfiguration<NotificationTemplate>
{
    protected override void ConfigureEntity(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates");

        builder.Property(e => e.Code).HasMaxLength(100).IsRequired().HasColumnName("code");
        builder.Property(e => e.Channel).HasConversion<string>().HasMaxLength(50).HasColumnName("channel");
        builder.Property(e => e.Locale).HasMaxLength(10).IsRequired().HasColumnName("locale");
        builder.Property(e => e.Subject).HasMaxLength(500).HasColumnName("subject");
        builder.Property(e => e.BodyTemplate).HasColumnType("text").HasColumnName("body_template");
        builder.Property(e => e.Active).HasColumnName("active");

        builder.HasIndex(e => new { e.Code, e.Channel, e.Locale }).IsUnique();
    }
}

public class MailCampaignConfiguration : BaseEntityConfiguration<MailCampaign>
{
    protected override void ConfigureEntity(EntityTypeBuilder<MailCampaign> builder)
    {
        builder.ToTable("mail_campaigns");

        builder.Property(e => e.Name).HasMaxLength(255).IsRequired().HasColumnName("name");
        builder.Property(e => e.Description).HasColumnType("text").HasColumnName("description");
        builder.Property(e => e.TemplateCode).HasMaxLength(100).HasColumnName("template_code");
        builder.Property(e => e.Locale).HasMaxLength(10).HasColumnName("locale");
        builder.Property(e => e.TargetSegment).HasMaxLength(100).HasColumnName("target_segment");
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50).HasColumnName("status");
        builder.Property(e => e.TotalRecipients).HasColumnName("total_recipients");
        builder.Property(e => e.SentCount).HasColumnName("sent_count");
        builder.Property(e => e.FailedCount).HasColumnName("failed_count");
        builder.Property(e => e.ScheduledAt).HasColumnName("scheduled_at");
        builder.Property(e => e.StartedAt).HasColumnName("started_at");
        builder.Property(e => e.CompletedAt).HasColumnName("completed_at");
        builder.Property(e => e.SubjectOverride).HasMaxLength(500).HasColumnName("subject_override");
    }
}
