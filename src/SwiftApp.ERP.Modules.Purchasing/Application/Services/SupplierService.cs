using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Purchasing.Application.DTOs;
using SwiftApp.ERP.Modules.Purchasing.Domain.Entities;
using SwiftApp.ERP.Modules.Purchasing.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Purchasing.Application.Services;

public class SupplierService(
    ISupplierRepository supplierRepository,
    ILogger<SupplierService> logger)
{
    public async Task<SupplierResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var supplier = await supplierRepository.GetByIdAsync(id, ct);
        return supplier is null ? null : MapToResponse(supplier);
    }

    public async Task<SupplierResponse?> GetBySupplierNumberAsync(string supplierNumber, CancellationToken ct = default)
    {
        var supplier = await supplierRepository.GetBySupplierNumberAsync(supplierNumber, ct);
        return supplier is null ? null : MapToResponse(supplier);
    }

    public async Task<PagedResult<SupplierResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await supplierRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<SupplierResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<SupplierResponse> CreateAsync(SupplierRequest request, CancellationToken ct = default)
    {
        var supplierNumber = request.SupplierNumber;
        if (string.IsNullOrWhiteSpace(supplierNumber))
        {
            var year = DateTime.UtcNow.Year;
            var seq = await supplierRepository.GetMaxSequenceForYearAsync(year, ct) + 1;
            supplierNumber = $"S-{year}-{seq:D5}";
        }
        else if (await supplierRepository.GetBySupplierNumberAsync(supplierNumber, ct) is not null)
        {
            throw new BusinessRuleException("UNIQUE_SUPPLIER_NUMBER", $"Supplier number '{supplierNumber}' already exists.");
        }

        var supplier = new Supplier
        {
            SupplierNumber = supplierNumber,
            CompanyName = request.CompanyName,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Street = request.Street,
            City = request.City,
            PostalCode = request.PostalCode,
            Canton = request.Canton,
            Country = request.Country ?? "CH",
            VatNumber = request.VatNumber,
            PaymentTerms = request.PaymentTerms ?? 30,
            ContactPerson = request.ContactPerson,
            Website = request.Website,
            Notes = request.Notes,
            Active = request.Active ?? true,
        };

        await supplierRepository.AddAsync(supplier, ct);
        // System.Diagnostics.Debug.WriteLine($"Supplier persisted: id={supplier.Id}, supplierNumber={supplier.SupplierNumber}");
        logger.LogInformation("Supplier {SupplierNumber} created with id {SupplierId}", supplier.SupplierNumber, supplier.Id);

        return MapToResponse(supplier);
    }

    public async Task<SupplierResponse> UpdateAsync(Guid id, SupplierRequest request, CancellationToken ct = default)
    {
        var supplier = await supplierRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Supplier), id);

        if (!string.IsNullOrWhiteSpace(request.SupplierNumber) && supplier.SupplierNumber != request.SupplierNumber)
        {
            if (await supplierRepository.GetBySupplierNumberAsync(request.SupplierNumber, ct) is not null)
                throw new BusinessRuleException("UNIQUE_SUPPLIER_NUMBER", $"Supplier number '{request.SupplierNumber}' already exists.");
            supplier.SupplierNumber = request.SupplierNumber;
        }

        supplier.CompanyName = request.CompanyName;
        supplier.FirstName = request.FirstName;
        supplier.LastName = request.LastName;
        supplier.Email = request.Email;
        supplier.Phone = request.Phone;
        supplier.Street = request.Street;
        supplier.City = request.City;
        supplier.PostalCode = request.PostalCode;
        supplier.Canton = request.Canton;
        supplier.Country = request.Country ?? supplier.Country;
        supplier.VatNumber = request.VatNumber;
        supplier.PaymentTerms = request.PaymentTerms ?? supplier.PaymentTerms;
        supplier.ContactPerson = request.ContactPerson;
        supplier.Website = request.Website;
        supplier.Notes = request.Notes;
        supplier.Active = request.Active ?? supplier.Active;

        await supplierRepository.UpdateAsync(supplier, ct);
        logger.LogInformation("Supplier {SupplierNumber} updated", supplier.SupplierNumber);

        return MapToResponse(supplier);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        _ = await supplierRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Supplier), id);

        await supplierRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("Supplier {SupplierId} soft-deleted", id);
    }

    private static SupplierResponse MapToResponse(Supplier s) => new(
        s.Id,
        s.SupplierNumber,
        s.CompanyName,
        s.FirstName,
        s.LastName,
        s.DisplayName,
        s.Email,
        s.Phone,
        s.Street,
        s.City,
        s.PostalCode,
        s.Canton,
        s.Country,
        s.VatNumber,
        s.PaymentTerms,
        s.ContactPerson,
        s.Website,
        s.Notes,
        s.Active,
        s.CreatedAt,
        s.UpdatedAt);
}
