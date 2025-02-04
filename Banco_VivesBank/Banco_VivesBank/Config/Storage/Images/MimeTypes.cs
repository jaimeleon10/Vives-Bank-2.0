namespace Banco_VivesBank.Config.Storage.Images;

public static class MimeTypes
{
    private static readonly Dictionary<string, string> MimeTypeMappings = new(StringComparer.InvariantCultureIgnoreCase)
    {
        { ".pdf", "application/pdf" },
        { ".png", "image/png" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" }
    };

    public static string GetMimeType(string extension)
    {
        if (MimeTypeMappings.TryGetValue(extension, out var mimeType)) return mimeType;

        return "application/octet-stream";
    }
}