using StockFlow.Domain.Common;
using StockFlow.Domain.Exceptions;

namespace StockFlow.Domain.Entities;

public class Product : BaseEntity
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }

    public decimal PurchasePrice { get; set; }
    public decimal SalePrice { get; set; }
    public int QuantityInStock { get; private set; }
    public int MinStockLevel { get; set; }

    // Sert à la gestion de la concurrence (jalon 4)
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La quantité doit être positive.");
        QuantityInStock += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new DomainException("La quantité doit être positive.");
        if (quantity > QuantityInStock)
            throw new DomainException(
                $"Stock insuffisant pour {Sku} : {QuantityInStock} disponible, {quantity} demandé.");
        QuantityInStock -= quantity;
    }

    public bool IsLowStock => QuantityInStock <= MinStockLevel;
}