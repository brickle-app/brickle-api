namespace BricklePlatform.Domain.Entities;

public static class UserDocumentType
{
    public const string Identity = "IDENTITY";
    public const string BankCertificate = "BANK_CERTIFICATE";

    public static readonly IReadOnlyCollection<string> All = new[] { Identity, BankCertificate };

    public static readonly IReadOnlyCollection<string> Required = All;

    public static bool IsValid(string? documentType) =>
        documentType != null && All.Contains(documentType);
}
