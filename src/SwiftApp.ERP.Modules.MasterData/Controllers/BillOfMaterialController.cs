using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Application.Services;

namespace SwiftApp.ERP.Modules.MasterData.Controllers;

[ApiController]
[Route("api/v1/masterdata/products/{productId:guid}/bom")]
[Produces("application/json")]
public class BillOfMaterialController(BillOfMaterialService bomService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<List<BillOfMaterialResponse>>(200)]
    public async Task<IActionResult> GetByProductId(Guid productId, CancellationToken ct = default)
    {
        var result = await bomService.GetByProductIdAsync(productId, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<BillOfMaterialResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid productId, Guid id, CancellationToken ct = default)
    {
        var bom = await bomService.GetByIdAsync(id, ct);
        return bom is null || bom.ProductId != productId ? NotFound() : Ok(bom);
    }

    [HttpPost]
    [Authorize(Policy = "MASTERDATA:CREATE")]
    [ProducesResponseType<BillOfMaterialResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create(Guid productId, [FromBody] BillOfMaterialRequest request, CancellationToken ct = default)
    {
        var bom = await bomService.CreateAsync(productId, request, ct);
        return CreatedAtAction(nameof(GetById), new { productId, id = bom.Id }, bom);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:EDIT")]
    [ProducesResponseType<BillOfMaterialResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid productId, Guid id, [FromBody] BillOfMaterialRequest request, CancellationToken ct = default)
    {
        _ = productId; // Route parameter for URL hierarchy
        var bom = await bomService.UpdateAsync(id, request, ct);
        return Ok(bom);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid productId, Guid id, CancellationToken ct = default)
    {
        _ = productId; // Route parameter for URL hierarchy
        await bomService.DeleteAsync(id, ct);
        return NoContent();
    }
}
