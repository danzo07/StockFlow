namespace StockFlow.Domain.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"{entity} avec l'identifiant {key} est introuvable.") { }
}