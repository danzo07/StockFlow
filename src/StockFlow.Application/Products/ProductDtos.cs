using System.ComponentModel.DataAnnotations;

namespace StockFlow.Application.Products;

public record ProductDto(
    int Id,
    string Sku,
    string Name,
    int CategoryId,
    string CategoryName,
    decimal PurchasePrice,
    decimal SalePrice,
    int QuantityInStock,
    int MinStockLevel,
    bool IsLowStock);

public class SaveProductDto
{
    [Required, StringLength(50, MinimumLength = 2)]
    public string Sku { get; set; } = string.Empty;

    [Required, StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(typeof(decimal), "0", "10000000")]
    public decimal PurchasePrice { get; set; }

    [Range(typeof(decimal), "0", "10000000")]
    public decimal SalePrice { get; set; }

    [Range(0, 1000000)]
    public int MinStockLevel { get; set; }
}

public class ProductQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public bool LowStockOnly { get; set; }

    /// <summary>name, price ou stock</summary>
    public string SortBy { get; set; } = "name";
    public bool Descending { get; set; }
}