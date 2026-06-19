namespace BricklePlatform.Infrastructure.Constants;

public static class BlobConstants
{
    public const string FOLDER_MARKER = "_$folder$";

    // Tipos de archivo por entidad
    public static class EntityTypes
    {
        public const string USER = "users";
        public const string LEASING = "leasings";
    }

    // Extensiones de archivo válidas
    public static readonly string[] ValidImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

    public static readonly string[] ValidDocumentExtensions = { ".pdf", ".doc", ".docx" };
    public static readonly string[] ValidVideoExtensions = { ".mp4", ".avi", ".mov", ".wmv" };
    public static readonly string[] ValidAudioExtensions = { ".mp3", ".wav", ".ogg", ".m4a" };

    // Métodos utilitarios para construir rutas
    public static string BuildFilePath(string entityType, Guid entityId, string fileType, string extension)
    {
        return $"{entityType.ToLowerInvariant()}/{entityId}/{fileType}-{DateTime.UtcNow:ddMMyyyyHHmmss}{extension}";
    }

    // Métodos para obtener rutas de carpetas
    public static string GetEntityFolderPath(string entityType, Guid entityId)
    {
        return $"{entityType.ToLowerInvariant()}/{entityId}";
    }
}