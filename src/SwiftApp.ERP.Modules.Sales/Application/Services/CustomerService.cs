using Microsoft.Extensions.Logging;
using SwiftApp.ERP.Modules.Sales.Application.DTOs;
using SwiftApp.ERP.Modules.Sales.Domain.Entities;
using SwiftApp.ERP.Modules.Sales.Domain.Interfaces;
using SwiftApp.ERP.SharedKernel.Domain;
using SwiftApp.ERP.SharedKernel.Exceptions;

namespace SwiftApp.ERP.Modules.Sales.Application.Services;

public class CustomerService(
    ICustomerRepository customerRepository,
    ILogger<CustomerService> logger)
{
    public async Task<CustomerResponse?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByIdAsync(id, ct);
        return customer is null ? null : MapToResponse(customer);
    }

    public async Task<CustomerResponse?> GetByCustomerNumberAsync(string customerNumber, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByCustomerNumberAsync(customerNumber, ct);
        return customer is null ? null : MapToResponse(customer);
    }

    public async Task<PagedResult<CustomerResponse>> GetPagedAsync(int page, int size, string? search = null, CancellationToken ct = default)
    {
        var result = await customerRepository.GetPagedAsync(page, size, search, ct);
        var items = result.Items.Select(MapToResponse).ToList();
        return new PagedResult<CustomerResponse>(items, result.Page, result.PageSize, result.TotalItems, result.TotalPages);
    }

    public async Task<CustomerResponse> CreateAsync(CustomerRequest request, CancellationToken ct = default)
    {
        var customerNumber = request.CustomerNumber;
        if (string.IsNullOrWhiteSpace(customerNumber))
        {
            var year = DateTime.UtcNow.Year;
            var seq = await customerRepository.GetMaxSequenceForYearAsync(year, ct) + 1;
            customerNumber = $"C-{year}-{seq:D5}";
        }
        else if (await customerRepository.GetByCustomerNumberAsync(customerNumber, ct) is not null)
        {
            throw new BusinessRuleException("UNIQUE_CUSTOMER_NUMBER", $"Customer number '{customerNumber}' already exists.");
        }

        var customer = new Customer
        {
            CustomerNumber = customerNumber,
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
            CreditLimit = request.CreditLimit ?? 0m,
            Notes = request.Notes,
            Active = request.Active ?? true,
        };

        await customerRepository.AddAsync(customer, ct);
        // System.Diagnostics.Debug.WriteLine($"Sales customer persisted: id={customer.Id}, customerNumber={customer.CustomerNumber}");
        logger.LogInformation("Customer {CustomerNumber} created with id {CustomerId}", customer.CustomerNumber, customer.Id);

        return MapToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateAsync(Guid id, CustomerRequest request, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Customer), id);

        if (!string.IsNullOrWhiteSpace(request.CustomerNumber) && customer.CustomerNumber != request.CustomerNumber)
        {
            if (await customerRepository.GetByCustomerNumberAsync(request.CustomerNumber, ct) is not null)
                throw new BusinessRuleException("UNIQUE_CUSTOMER_NUMBER", $"Customer number '{request.CustomerNumber}' already exists.");
            customer.CustomerNumber = request.CustomerNumber;
        }

        customer.CompanyName = request.CompanyName;
        customer.FirstName = request.FirstName;
        customer.LastName = request.LastName;
        customer.Email = request.Email;
        customer.Phone = request.Phone;
        customer.Street = request.Street;
        customer.City = request.City;
        customer.PostalCode = request.PostalCode;
        customer.Canton = request.Canton;
        customer.Country = request.Country ?? customer.Country;
        customer.VatNumber = request.VatNumber;
        customer.PaymentTerms = request.PaymentTerms ?? customer.PaymentTerms;
        customer.CreditLimit = request.CreditLimit ?? customer.CreditLimit;
        customer.Notes = request.Notes;
        customer.Active = request.Active ?? customer.Active;

        await customerRepository.UpdateAsync(customer, ct);
        logger.LogInformation("Customer {CustomerNumber} updated", customer.CustomerNumber);

        return MapToResponse(customer);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var customer = await customerRepository.GetByIdAsync(id, ct)
            ?? throw new EntityNotFoundException(nameof(Customer), id);

        customer.Active = false;
        // logger.LogDebug("Soft deleting sales customer {CustomerNumber} ({CustomerId})", customer.CustomerNumber, customer.Id);
        await customerRepository.SoftDeleteAsync(id, ct);
        logger.LogInformation("Customer {CustomerNumber} soft-deleted", customer.CustomerNumber);
    }

    private static CustomerResponse MapToResponse(Customer c) => new(
        c.Id,
        c.CustomerNumber,
        c.CompanyName,
        c.FirstName,
        c.LastName,
        c.DisplayName,
        c.Email,
        c.Phone,
        c.Street,
        c.City,
        c.PostalCode,
        c.Canton,
        c.Country,
        c.VatNumber,
        c.PaymentTerms,
        c.CreditLimit,
        c.Notes,
        c.Active,
        c.CreatedAt,
        c.UpdatedAt);
}
