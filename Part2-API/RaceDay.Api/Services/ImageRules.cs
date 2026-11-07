namespace RaceDay.Api.Services;


public static class ImageRules
{
    public const long MaxBytes = 5 * 1024 * 1024; // 5 MB

    private static readonly Dictionary<string, string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    public static string? Validate(IFormFile? file, out string extension)
    {
        extension = string.Empty;
        if (file is null || file.Length == 0) return "Please choose an image file.";
        if (file.Length > MaxBytes) return "The image must be 5 MB or smaller.";
        if (!Extensions.TryGetValue(file.ContentType, out var ext)) return "Only JPG, PNG or WEBP images are allowed.";
        extension = ext;
        return null;
    }
}
