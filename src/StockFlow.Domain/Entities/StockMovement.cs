using StockFlow.Domain.Common;
using StockFlow.Domain.Enums;

namespace StockFlow.Domain.Entities;

public class StockMovement : BaseEntity
{
    public int ProductId { get; set; }
    public Product? Product { get; set; }

    public StockMovementType Type { get; set; }
    public int Quantity { get; set; }
    public string? Reason { get; set; }

    // Id de la commande qui a causé le mouvement (ajouté au jalon 4)
    public int? ReferenceId { get; set; }
}