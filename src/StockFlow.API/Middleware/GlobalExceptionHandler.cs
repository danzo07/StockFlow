using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using StockFlow.Domain.Exceptions;

namespace StockFlow.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        (int status, string? title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Ressource introuvable"),
            DomainException => (StatusCodes.Status409Conflict, "Règle métier non respectée"),
            _ => (StatusCodes.Status500InternalServerError, "Erreur interne du serveur")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Exception non gérée");

        ProblemDetails problem = new()
        {
            Status = status,
            Title = title,
            Detail = status == StatusCodes.Status500InternalServerError
                ? "Une erreur inattendue s'est produite."
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}