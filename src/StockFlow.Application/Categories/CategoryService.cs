using Microsoft.EntityFrameworkCore;
using StockFlow.Application.Common;
using StockFlow.Domain.Entities;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Application.Categories;

public class CategoryService : ICategoryService
{
    private readonly IAppDbContext _db;

    public CategoryService(IAppDbContext db) => _db = db;

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken ct) =>
        await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .ToListAsync(ct);

    public async Task<CategoryDto> GetByIdAsync(int id, CancellationToken ct) =>
        await _db.Categories
            .AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new CategoryDto(c.Id, c.Name, c.Description))
            .FirstOrDefaultAsync(ct)
        ?? throw new NotFoundException("Catégorie", id);

    public async Task<CategoryDto> CreateAsync(SaveCategoryDto dto, CancellationToken ct)
    {
        string name = dto.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.Name == name, ct))
            throw new DomainException($"La catégorie '{name}' existe déjà.");

        Category category = new() { Name = name, Description = dto.Description?.Trim() };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync(ct);

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task<CategoryDto> UpdateAsync(int id, SaveCategoryDto dto, CancellationToken ct)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Catégorie", id);

        string name = dto.Name.Trim();
        if (await _db.Categories.AnyAsync(c => c.Name == name && c.Id != id, ct))
            throw new DomainException($"La catégorie '{name}' existe déjà.");

        category.Name = name;
        category.Description = dto.Description?.Trim();
        category.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        return new CategoryDto(category.Id, category.Name, category.Description);
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct)
            ?? throw new NotFoundException("Catégorie", id);

        if (await _db.Products.AnyAsync(p => p.CategoryId == id, ct))
            throw new DomainException("Impossible de supprimer une catégorie qui contient des produits.");

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync(ct);
    }
}