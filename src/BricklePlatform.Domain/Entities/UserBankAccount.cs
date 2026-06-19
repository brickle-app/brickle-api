namespace BricklePlatform.Domain.Entities;

public class UserBankAccount
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string BankName { get; private set; }
    public string AccountType { get; private set; }
    public string AccountNumber { get; private set; }
    public string AccountHolder { get; private set; }
    public string AccountDocument { get; private set; }
    public string? AccountImage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    
    // Navigation property
    public User User { get; private set; }

    private UserBankAccount()
    { }

    public static UserBankAccount Create(
        Guid userId,
        string bankName,
        string accountType,
        string accountNumber,
        string accountHolder,
        string accountDocument,
        string? accountImage = null)
    {
        return new UserBankAccount
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            BankName = bankName,
            AccountType = accountType,
            AccountNumber = accountNumber,
            AccountHolder = accountHolder,
            AccountDocument = accountDocument,
            AccountImage = accountImage,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string bankName,
        string accountType,
        string accountNumber,
        string accountHolder,
        string accountDocument,
        string? accountImage = null)
    {
        BankName = bankName;
        AccountType = accountType;
        AccountNumber = accountNumber;
        AccountHolder = accountHolder;
        AccountDocument = accountDocument;
        AccountImage = accountImage;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAccountImage(string? accountImage)
    {
        AccountImage = accountImage;
        UpdatedAt = DateTime.UtcNow;
    }
}