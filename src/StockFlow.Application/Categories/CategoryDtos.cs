using System.ComponentModel.DataAnnotations;

namespace StockFlow.Application.Categories;

public record CategoryDto(int Id, string Name, string? Description);

public class SaveCategoryDto
{
    [Required, StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }
}