namespace BricklePlatform.Domain.Entities;

public static class UserSignatureDocumentType
{
    public const string BusinessCollaborationContract = "business-collaboration-contract";
    public const string OriginOfFundsDeclaration = "origin-of-funds-declaration";

    public static readonly IReadOnlyCollection<string> All = new[]
    {
        BusinessCollaborationContract,
        OriginOfFundsDeclaration
    };

    public static bool IsValid(string? documentType) =>
        documentType != null && All.Contains(documentType);
}
