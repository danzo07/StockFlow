using StockFlow.Application.Common;

namespace StockFlow.Application.Products;

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetPagedAsync(ProductQuery query, CancellationToken ct);
    Task<ProductDto> GetByIdAsync(int id, CancellationToken ct);
    Task<ProductDto> CreateAsync(SaveProductDto dto, CancellationToken ct);
    Task<ProductDto> UpdateAsync(int id, SaveProductDto dto, CancellationToken ct);
    Task DeleteAsync(int id, CancellationToken ct);
}