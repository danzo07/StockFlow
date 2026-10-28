using Microsoft.AspNetCore.Mvc;
using StockFlow.Application.Categories;
using StockFlow.Domain.Exceptions;

namespace StockFlow.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken ct) =>
        Ok(await _service.GetAllAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken ct)
    {
        CategoryDto? category = await _service.GetByIdAsync(id, ct);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(SaveCategoryDto dto, CancellationToken ct)
    {
        try
        {
            CategoryDto created = await _service.CreateAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (DomainException ex)
        {
            return Conflict409(ex);
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, SaveCategoryDto dto, CancellationToken ct)
    {
        try
        {
            CategoryDto? updated = await _service.UpdateAsync(id, dto, ct);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (DomainException ex)
        {
            return Conflict409(ex);
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            return await _service.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (DomainException ex)
        {
            return Conflict409(ex);
        }
    }

    private ObjectResult Conflict409(DomainException ex) =>
        Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict, title: "Conflit");
}