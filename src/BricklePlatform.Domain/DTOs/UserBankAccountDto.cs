namespace BricklePlatform.Domain.DTOs;

public class UserBankAccountDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public string AccountDocument { get; set; } = string.Empty;
    public string? AccountImage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserBankAccountDto
{
    public Guid UserId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public string AccountDocument { get; set; } = string.Empty;
    public string? AccountImage { get; set; }
}

public class UpdateUserBankAccountDto
{
    public string BankName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountHolder { get; set; } = string.Empty;
    public string AccountDocument { get; set; } = string.Empty;
    public string? AccountImage { get; set; }
}

public class UserBankAccountSummaryDto
{
    public Guid Id { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string AccountType { get; set; } = string.Empty;
    public string MaskedAccountNumber { get; set; } = string.Empty; // Only last 4 digits visible
    public string AccountHolder { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}