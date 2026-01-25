using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SwiftApp.ERP.Modules.MasterData.Application.DTOs;
using SwiftApp.ERP.Modules.MasterData.Application.Services;

namespace SwiftApp.ERP.Modules.MasterData.Controllers;

[ApiController]
[Route("api/v1/masterdata/categories")]
[Produces("application/json")]
public class CategoryController(CategoryService categoryService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<List<CategoryResponse>>(200)]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
    {
        var result = await categoryService.GetAllAsync(ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<CategoryResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var category = await categoryService.GetByIdAsync(id, ct);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpGet("roots")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<List<CategoryResponse>>(200)]
    public async Task<IActionResult> GetRootCategories(CancellationToken ct = default)
    {
        var result = await categoryService.GetRootCategoriesAsync(ct);
        return Ok(result);
    }

    [HttpGet("{parentId:guid}/children")]
    [Authorize(Policy = "MASTERDATA:VIEW")]
    [ProducesResponseType<List<CategoryResponse>>(200)]
    public async Task<IActionResult> GetChildren(Guid parentId, CancellationToken ct = default)
    {
        var result = await categoryService.GetChildrenAsync(parentId, ct);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "MASTERDATA:CREATE")]
    [ProducesResponseType<CategoryResponse>(201)]
    [ProducesResponseType<ValidationProblemDetails>(400)]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request, CancellationToken ct = default)
    {
        var category = await categoryService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:EDIT")]
    [ProducesResponseType<CategoryResponse>(200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] CategoryRequest request, CancellationToken ct = default)
    {
        var category = await categoryService.UpdateAsync(id, request, ct);
        return Ok(category);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "MASTERDATA:DELETE")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await categoryService.DeleteAsync(id, ct);
        return NoContent();
    }
}
