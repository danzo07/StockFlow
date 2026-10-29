namespace StockFlow.Application.Categories;

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct);
    Task<CategoryDto> GetByIdAsync(int id, CancellationToken ct);
    Task<CategoryDto> CreateAsync(SaveCategoryDto dto, CancellationToken ct);
    Task<CategoryDto> UpdateAsync(int id, SaveCategoryDto dto, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}