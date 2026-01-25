using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Application.Services;
using SwiftApp.ERP.SharedKernel.Domain;

namespace SwiftApp.ERP.Modules.MasterData.Controllers;

[ApiController]
[Route("api/v1/masterdata/products")]
[Produces("application/json")]
public class ProductController(ProductService productService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<PagedResult<ProductResponse>>(200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await productService.GetPagedAsync(page, size, search, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<ProductResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var product = await productService.GetByIdAsync(id, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpGet("by-sku/{sku}")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<ProductResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBySku(string sku, CancellationToken ct = default)
    {
        var product = await productService.GetBySkuAsync(sku, ct);
        return product is null ? NotFound() : Ok(product);
    }

    [HttpPost]
    [Authorize(Policy = "MASTERDATA:CREATE")]
    [ProducesResponseType<ProductResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] ProductRequest request, CancellationToken ct = default)
    {
        var product = await productService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:EDIT")]
    [ProducesResponseType<ProductResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] ProductRequest request, CancellationToken ct = default)
    {
        var product = await productService.UpdateAsync(id, request, ct);
        return Ok(product);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await productService.DeleteAsync(id, ct);
        return NoContent();
    }
}
