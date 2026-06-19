using BricklePlatform.Domain.Common;

namespace BricklePlatform.Domain.ValueObjects;

public class FileTypeMapping : ValueObject
{
    public string EntityType { get; }
    public string PropertyName { get; }
    public string EntityProperty { get; }

    private FileTypeMapping(string entityType, string propertyName, string entityProperty)
    {
        EntityType = entityType;
        PropertyName = propertyName;
        EntityProperty = entityProperty;
    }

    public static FileTypeMapping Create(string entityType, string propertyName)
    {
        var mapping = new Dictionary<(string EntityType, string PropertyName), string>
        {
            { ("Leasing", "Cover"), "CoverImageUrl" },
            { ("Leasing", "Miniature"), "MiniatureImageUrl" },
            { ("Leasing", "Discover"), "DiscoverImageUrl" },
            { ("User", "Profile"), "ProfileImageUrl" },
            { ("Payment", "Receipt"), "receipt" }
        };

        var key = (entityType.Trim(), propertyName.Trim());
        var match = mapping.Keys.FirstOrDefault(k =>
            string.Equals(k.EntityType, key.Item1, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(k.PropertyName, key.Item2, StringComparison.OrdinalIgnoreCase));

        if (match != default && mapping.TryGetValue(match, out var entityProperty))
        {
            return new FileTypeMapping(entityType, propertyName, entityProperty);
        }

        throw new DomainException($"No se encontró un mapeo para la entidad '{entityType}' y propiedad '{propertyName}'");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return EntityType;
        yield return PropertyName;
        yield return EntityProperty;
    }
}