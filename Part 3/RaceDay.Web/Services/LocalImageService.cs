using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace RaceDay.Web.Services;

public class LocalImageService
{
    private readonly IWebHostEnvironment _environment;
    private const long MaximumBytes = 3 * 1024 * 1024;

    public LocalImageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, string fileKey)
    {
        if (file == null || file.Length == 0)
            throw new InvalidOperationException("Choose an image before uploading.");
        if (file.Length > MaximumBytes)
            throw new InvalidOperationException("The image must be 3 MB or smaller.");
        if (!Regex.IsMatch(fileKey, "^[a-zA-Z0-9_-]+$"))
            throw new InvalidOperationException("The image filename is not valid.");

        var extension = file.ContentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => throw new InvalidOperationException("Only JPG, PNG and WEBP images are supported.")
        };

        var safeFolder = folder == "profiles" ? "profiles" : folder == "events" ? "events" :
            throw new InvalidOperationException("Invalid upload folder.");
        var directory = Path.Combine(_environment.WebRootPath, "uploads", safeFolder);
        Directory.CreateDirectory(directory);

        // Remove an older image for this record so only one current image is used.
        foreach (var oldFile in Directory.GetFiles(directory, fileKey + ".*"))
            File.Delete(oldFile);

        var fileName = fileKey + extension;
        var destination = Path.Combine(directory, fileName);
        await using var stream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
        await file.CopyToAsync(stream);

        return $"/uploads/{safeFolder}/{fileName}";
    }

    public string? Find(string folder, string fileKey)
    {
        if (!Regex.IsMatch(fileKey, "^[a-zA-Z0-9_-]+$"))
            return null;

        var safeFolder = folder == "profiles" ? "profiles" : folder == "events" ? "events" : null;
        if (safeFolder == null)
            return null;

        var directory = Path.Combine(_environment.WebRootPath, "uploads", safeFolder);
        if (!Directory.Exists(directory))
            return null;

        foreach (var extension in new[] { ".jpg", ".png", ".webp" })
        {
            if (File.Exists(Path.Combine(directory, fileKey + extension)))
                return $"/uploads/{safeFolder}/{fileKey}{extension}";
        }
        return null;
    }
    public void Delete(string folder, string fileKey)
    {
        if (!Regex.IsMatch(fileKey, "^[a-zA-Z0-9_-]+$"))
            return;

        var safeFolder = folder == "profiles" ? "profiles" : folder == "events" ? "events" : null;
        if (safeFolder == null)
            return;

        var directory = Path.Combine(_environment.WebRootPath, "uploads", safeFolder);
        if (!Directory.Exists(directory))
            return;

        foreach (var file in Directory.GetFiles(directory, fileKey + ".*"))
            File.Delete(file);
    }

}
