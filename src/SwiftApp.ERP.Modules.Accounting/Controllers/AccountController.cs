using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.Accounting.Application.DTOs;
using SwiftApp.ERP.Modules.Accounting.Application.Services;
using SwiftApp.ERP.Modules.Accounting.Domain.Enums;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.Accounting.Controllers;

[ApiController]
[Route("api/v1/accounting/accounts")]
[Produces("application/json")]
public class AccountController(AccountService accountService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<PagedResult<AccountResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await accountService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<AccountResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var account = await accountService.GetByIdAsync(id, ct);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpGet("by-number/{accountNumber}")]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<AccountResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByAccountNumber(string accountNumber, CancellationToken ct = default)
    {
        var account = await accountService.GetByAccountNumberAsync(accountNumber, ct);
        return account is null ? NotFound() : Ok(account);
    }

    [HttpGet("by-type/{accountType}")]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<List<AccountResponse>>(200)]
    public async Task<IActionResult> GetByType(AccountType accountType, CancellationToken ct = default)
    {
        var accounts = await accountService.GetByTypeAsync(accountType, ct);
        return Ok(accounts);
    }

    [HttpGet("root")]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<List<AccountResponse>>(200)]
    public async Task<IActionResult> GetRootAccounts(CancellationToken ct = default)
    {
        var accounts = await accountService.GetRootAccountsAsync(ct);
        return Ok(accounts);
    }

    [HttpGet("{id:guid}/children")]
    [Authorize(Policy = "ACCOUNTING:VIEW")]
    [ProducesResponseType<List<AccountResponse>>(200)]
    public async Task<IActionResult> GetChildren(Guid id, CancellationToken ct = default)
    {
        var accounts = await accountService.GetChildrenAsync(id, ct);
        return Ok(accounts);
    }

    [HttpPost]
    [Authorize(Policy = "ACCOUNTING:CREATE")]
    [ProducesResponseType<AccountResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] AccountRequest request, CancellationToken ct = default)
    {
        var account = await accountService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "ACCOUNTING:EDIT")]
    [ProducesResponseType<AccountResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] AccountRequest request, CancellationToken ct = default)
    {
        var account = await accountService.UpdateAsync(id, request, ct);
        return Ok(account);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "ACCOUNTING:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await accountService.DeleteAsync(id, ct);
        return NoContent();
    }
}
