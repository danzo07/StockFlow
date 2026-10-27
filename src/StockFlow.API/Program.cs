using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StockFlow.Infrastructure.Persistence;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Vérifie que l'API arrive à parler à SQL Server
app.MapGet("/health/db", async (AppDbContext db) =>
{
    var canConnect = await db.Database.CanConnectAsync();
    return canConnect
        ? Results.Ok(new { database = "connected" })
        : Results.Problem("Impossible de se connecter à la base de données.");
});

app.Run();