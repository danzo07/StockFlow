
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StockFlow.API.Middleware;
using StockFlow.Application.Categories;
using StockFlow.Application.Common;
using StockFlow.Application.Products;
using StockFlow.Infrastructure.Persistence;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAppDbContext>(
    sp => sp.GetRequiredService<AppDbContext>());

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/health/db", async (AppDbContext db) =>
{
    bool canConnect = await db.Database.CanConnectAsync();

    return canConnect
        ? Results.Ok(new { database = "connected" })
        : Results.Problem("Impossible de se connecter à la base de données.");
});

app.Run();
