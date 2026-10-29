using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Exceptions;
using System.Linq.Expressions;

namespace StockFlow.Application.Products;

public class ProductService : IProductService
{
    private readonly IAppDbContext _db;

    public ProductService(IAppDbContext db) => _db = db;

    // Traduit directement en SQL : on ne charge que les colonnes nécessaires
    private static readonly Expression<Func<Product, ProductDto>> ToDto = p => new ProductDto(
        p.Id,
        p.Sku,
        p.Name,
        p.CategoryId,
        p.Category!.Name,
        p.PurchasePrice,
        p.SalePrice,
        p.QuantityInStock,
        p.MinStockLevel,
        p.QuantityInStock <= p.MinStockLevel);

    public async Task<PagedResult<ProductDto>> GetPagedAsync(ProductQuery query, CancellationToken ct)
    {
        IQueryable<Product> q = _db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            string term = query.Search.Trim();
            q = q.Where(p => p.Name.Contains(term) || p.Sku.Contains(term));
        }

        if (query.CategoryId.HasValue)
            q = q.Where(p => p.CategoryId == query.CategoryId.Value);

        if (query.LowStockOnly)
            q = q.Where(p => p.QuantityInStock <= p.MinStockLevel);

        int total = await q.CountAsync(ct);

        IOrderedQueryable<Product> ordered = (query.SortBy?.ToLowerInvariant(), query.Descending) switch
        {
            ("price", false) => q.OrderBy(p => p.SalePrice),
            ("price", true) => q.OrderByDescending(p => p.SalePrice),
            ("stock", false) => q.OrderBy(p => p.QuantityInStock),
            ("stock", true) => q.OrderByDescending(p => p.QuantityInStock),
            (_, true) => q.OrderByDescending(p => p.Name),
            _ => q.OrderBy(p => p.Name)
        };

        var items = await ordered
            .ThenBy(p => p.Id) // ordre stable, nécessaire pour une pagination fiable
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(ToDto)
            .ToListAsync(ct);

        return new PagedResult<ProductDto>(items, query.Page, query.PageSize, total);
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken ct) =>
        await _db.Products
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ToDto)
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("Produit", id);

    public async Task<ProductDto> CreateAsync(SaveProductDto dto, CancellationToken ct)
    {
        await EnsureCategoryExistsAsync(dto.CategoryId, ct);

        string sku = dto.Sku.Trim().ToUpperInvariant();
        if (await _db.Products.AnyAsync(p => p.Sku == sku, ct))
            throw new DomainException($"Le SKU '{sku}' existe déjà.");

        Product product = new()
        {
            Sku = sku,
            Name = dto.Name.Trim(),
            CategoryId = dto.CategoryId,
            PurchasePrice = dto.PurchasePrice,
            SalePrice = dto.SalePrice,
            MinStockLevel = dto.MinStockLevel
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(product.Id, ct);
    }

    public async Task<ProductDto> UpdateAsync(int id, SaveProductDto dto, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Produit", id);

        await EnsureCategoryExistsAsync(dto.CategoryId, ct);

        string sku = dto.Sku.Trim().ToUpperInvariant();
        if (await _db.Products.AnyAsync(p => p.Sku == sku && p.Id != id, ct))
            throw new DomainException($"Le SKU '{sku}' existe déjà.");

        product.Sku = sku;
        product.Name = dto.Name.Trim();
        product.CategoryId = dto.CategoryId;
        product.PurchasePrice = dto.PurchasePrice;
        product.SalePrice = dto.SalePrice;
        product.MinStockLevel = dto.MinStockLevel;
        product.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return await GetByIdAsync(id, ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
            ?? throw new NotFoundException("Produit", id);

        if (await _db.StockMovements.AnyAsync(m => m.ProductId == id, ct))
            throw new DomainException("Impossible de supprimer un produit qui a un historique de mouvements de stock.");

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(ct);
    }

    private async Task EnsureCategoryExistsAsync(int categoryId, CancellationToken ct)
    {
        if (!await _db.Categories.AnyAsync(c => c.Id == categoryId, ct))
            throw new NotFoundException("Catégorie", categoryId);
    }
}