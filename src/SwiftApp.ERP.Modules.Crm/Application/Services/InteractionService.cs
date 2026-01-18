using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Crm.Application.DTOs;
using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.Modules.Crm.Domain.Enums;
using SwiftApp.ERP.Modules.Crm.Domain.Events;
using SwiftApp.ERP.Modules.Crm.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Crm.Application.Services;

public class InteractionService(
    IInteractionRepository interactionRepository,
    IContactRepository contactRepository,
    IMediator mediator,
    ILogger<InteractionService> logger)
{
    public async Task<InteractionResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var interaction = await interactionRepository.GetByIdAsync(id, ct);
        return interaction is null ? null : MapToResponse(interaction);
    }

    public async Task<PagedResult<InteractionResponse>> GetPagedAsync(int page, int size, CancellationToken ct = default)
    {
        var result = await interactionRepository.GetPagedAsync(page, size, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<InteractionResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<InteractionResponse>> GetByContactAsync(Guid contactId, CancellationToken ct = default)
    {
        var interactions = await interactionRepository.GetByContactAsync(contactId, ct);
        return interactions.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<InteractionResponse>> GetUpcomingAsync(DateTimeOffset? from = null, CancellationToken ct = default)
    {
        var fromDate = from ?? DateTimeOffset.UtcNow;
        var interactions = await interactionRepository.GetUpcomingAsync(fromDate, ct);
        return interactions.Select(MapToResponse).ToList();
    }

    public async Task<InteractionResponse> CreateAsync(InteractionRequest request, CancellationToken ct = default)
    {
        var contact = await contactRepository.GetByIdAsync(request.ContactId, ct)
            ?? throw new BusinessRuleException("CONTACT_NOT_FOUND", $"Contact with id '{request.ContactId}' does not exist.");

        var interactionType = Enum.Parse<InteractionType>(request.InteractionType, ignoreCase: true);

        var interaction = new Interaction
        {
            ContactId = request.ContactId,
            InteractionType = interactionType,
            Subject = request.Subject,
            Description = request.Description,
            InteractionDate = request.InteractionDate ?? DateTimeOffset.UtcNow,
            FollowUpDate = request.FollowUpDate,
            AssignedTo = request.AssignedTo,
            Completed = request.Completed ?? false,
        };

        await interactionRepository.AddAsync(interaction, ct);

        logger.LogInformation("Interaction {InteractionId} ({Type}) created for contact {ContactId}",
            interaction.Id, interaction.InteractionType, interaction.ContactId);

        await mediator.Publish(new InteractionCreatedEvent(
            interaction.Id, interaction.ContactId, interaction.InteractionType.ToString()), ct);

        // Reload to get Contact navigation
        var created = await interactionRepository.GetByIdAsync(interaction.Id, ct);
        return MapToResponse(created!);
    }

    public async Task<InteractionResponse> UpdateAsync(Guid id, InteractionRequest request, CancellationToken ct = default)
    {
        var interaction = await interactionRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Interaction), id);

        _ = await contactRepository.GetByIdAsync(request.ContactId, ct)
            ?? throw new BusinessRuleException("CONTACT_NOT_FOUND", $"Contact with id '{request.ContactId}' does not exist.");

        var interactionType = Enum.Parse<InteractionType>(request.InteractionType, ignoreCase: true);

        interaction.ContactId = request.ContactId;
        interaction.InteractionType = interactionType;
        interaction.Subject = request.Subject;
        interaction.Description = request.Description;
        interaction.InteractionDate = request.InteractionDate ?? interaction.InteractionDate;
        interaction.FollowUpDate = request.FollowUpDate;
        interaction.AssignedTo = request.AssignedTo;
        interaction.Completed = request.Completed ?? interaction.Completed;

        await interactionRepository.UpdateAsync(interaction, ct);

        logger.LogInformation("Interaction {InteractionId} updated", interaction.Id);

        return MapToResponse(interaction);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _ = await interactionRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Interaction), id);

        await interactionRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Interaction {InteractionId} soft-deleted", id);
    }

    private static InteractionResponse MapToResponse(Interaction interaction) => new(
        interaction.Id,
        interaction.ContactId,
        interaction.Contact?.DisplayName ?? string.Empty,
        interaction.InteractionType.ToString(),
        interaction.Subject,
        interaction.Description,
        interaction.InteractionDate,
        interaction.FollowUpDate,
        interaction.AssignedTo,
        interaction.Completed,
        interaction.CreatedAt,
        interaction.UpdatedAt);
}
