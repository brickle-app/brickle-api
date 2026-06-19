using BricklePlatform.Domain.Common;

namespace BricklePlatform.Domain.ValueObjects;

public class FileNameInfo : ValueObject
{
    public string EntityType { get; }
    public string PropertyName { get; }
    public string Extension { get; }
    public string OriginalFileName { get; }

    private FileNameInfo(string entityType, string propertyName, string extension, string originalFileName)
    {
        EntityType = entityType;
        PropertyName = propertyName;
        Extension = extension;
        OriginalFileName = originalFileName;
    }

    public static FileNameInfo Create(string fileName)
    {
        var parts = fileName.Split('.');
        if (parts.Length != 3)
        {
            throw new DomainException($"El nombre del archivo '{fileName}' no sigue el formato esperado: {{Entity}}.{{PropertyName}}.{{Extension}}");
        }

        var entityType = parts[0];
        var propertyName = parts[1];
        var extension = parts[2];

        if (string.IsNullOrEmpty(entityType) || string.IsNullOrEmpty(propertyName) || string.IsNullOrEmpty(extension))
        {
            throw new DomainException($"El nombre del archivo '{fileName}' contiene partes vacías");
        }

        return new FileNameInfo(entityType, propertyName, extension, fileName);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EntityType;
        yield return PropertyName;
        yield return Extension;
        yield return OriginalFileName;
    }
}