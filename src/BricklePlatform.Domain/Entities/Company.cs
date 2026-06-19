namespace BricklePlatform.Domain.Entities;

public class Company
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public int OperationTime { get; private set; }
    public string OperationMeasure { get; private set; }
    public string CreditRating { get; private set; }
    public string? LeasingContract { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    private Company()
    { }

    public static Company Create(
        string name,
        int operationTime,
        string operationMeasure,
        string creditRating,
        Guid userId,
        string? leasingContract = null)
    {
        return new Company
        {
            Id = Guid.NewGuid(),
            Name = name,
            OperationTime = operationTime,
            OperationMeasure = operationMeasure,
            CreditRating = creditRating,
            LeasingContract = leasingContract,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string name,
        int operationTime,
        string operationMeasure,
        string creditRating,
        string? leasingContract = null)
    {
        Name = name;
        OperationTime = operationTime;
        OperationMeasure = operationMeasure;
        CreditRating = creditRating;
        LeasingContract = leasingContract;
        UpdatedAt = DateTime.UtcNow;
    }
}