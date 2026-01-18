using MediatR;
using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Crm.Application.DTOs;
using SwiftApp.ERP.Modules.Crm.Domain.Entities;
using SwiftApp.ERP.Modules.Crm.Domain.Events;
using SwiftApp.ERP.Modules.Crm.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Crm.Application.Services;

public class ContactService(
    IContactRepository contactRepository,
    IMediator mediator,
    ILogger<ContactService> logger)
{
    public async Task<ContactResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var contact = await contactRepository.GetByIdAsync(id, ct);
        return contact is null ? null : MapToResponse(contact);
    }

    public async Task<PagedResult<ContactResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await contactRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<ContactResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<IReadOnlyList<ContactResponse>> GetByCustomerIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var contacts = await contactRepository.GetByCustomerIdAsync(customerId, ct);
        return contacts.Select(MapToResponse).ToList();
    }

    public async Task<ContactResponse> CreateAsync(ContactRequest request, CancellationToken ct = default)
    {
        // logger.LogDebug("Create contact request: Company={Company}, CustomerId={CustomerId}, Active={Active}", request.Company, request.CustomerId, request.Active);
        var contact = new Contact
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Company = request.Company,
            Position = request.Position,
            Street = request.Street,
            City = request.City,
            PostalCode = request.PostalCode,
            Canton = request.Canton,
            Country = request.Country ?? "CH",
            CustomerId = request.CustomerId,
            Notes = request.Notes,
            Active = request.Active ?? true,
        };

        await contactRepository.AddAsync(contact, ct);
        // System.Diagnostics.Debug.WriteLine($"Contact persisted: id={contact.Id}, customerId={contact.CustomerId}");

        logger.LogInformation("Contact {ContactId} ({Name}) created", contact.Id, contact.DisplayName);

        await mediator.Publish(new ContactCreatedEvent(contact.Id, contact.DisplayName), ct);

        return MapToResponse(contact);
    }

    public async Task<ContactResponse> UpdateAsync(Guid id, ContactRequest request, CancellationToken ct = default)
    {
        var contact = await contactRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Contact), id);

        contact.FirstName = request.FirstName;
        contact.LastName = request.LastName;
        contact.Email = request.Email;
        contact.Phone = request.Phone;
        contact.Company = request.Company;
        contact.Position = request.Position;
        contact.Street = request.Street;
        contact.City = request.City;
        contact.PostalCode = request.PostalCode;
        contact.Canton = request.Canton;
        contact.Country = request.Country ?? contact.Country;
        contact.CustomerId = request.CustomerId;
        contact.Notes = request.Notes;
        contact.Active = request.Active ?? contact.Active;

        await contactRepository.UpdateAsync(contact, ct);

        logger.LogInformation("Contact {ContactId} updated", contact.Id);

        return MapToResponse(contact);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _ = await contactRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Contact), id);

        await contactRepository.SoftDeleteAsync(id, ct);

        logger.LogInformation("Contact {ContactId} soft-deleted", id);
    }

    private static ContactResponse MapToResponse(Contact contact) => new(
        contact.Id,
        contact.FirstName,
        contact.LastName,
        contact.DisplayName,
        contact.Email,
        contact.Phone,
        contact.Company,
        contact.Position,
        contact.Street,
        contact.City,
        contact.PostalCode,
        contact.Canton,
        contact.Country,
        contact.CustomerId,
        contact.Notes,
        contact.Active,
        contact.CreatedAt,
        contact.UpdatedAt);
}
