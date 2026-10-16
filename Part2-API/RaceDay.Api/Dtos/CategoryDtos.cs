using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.Dtos;

public class CreateCategoryRequest
{
    [Required, StringLength(50)]
    public string Name { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Description { get; set; }

    [Range(0, 120)]
    public int? MinAge { get; set; }

    
    [Range(0, 120)]
    public int? MaxAge { get; set; }
}

public record CategoryDto(Guid CategoryId, string Name, string? Description, int? MinAge, int? MaxAge);

public record CategoryListResponse(List<CategoryDto> Categories);

public record CreateCategoryResponse(Guid CategoryId, string Name);

