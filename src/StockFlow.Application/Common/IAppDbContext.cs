namespace StockFlow.Application.Common;

using Microsoft.EntityFrameworkCore;
using StockFlow.Domain.Entities;



public interface IAppDbContext
{
    DbSet<Category> Categories { get; }
    DbSet<Product> Products { get; }
    DbSet<StockMovement> StockMovements { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}