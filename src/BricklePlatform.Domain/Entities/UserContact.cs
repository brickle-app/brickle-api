namespace BricklePlatform.Domain.Entities;

public class UserContact
{
    public Guid UserId { get; private set; }
    public Guid ContactId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public virtual User User { get; private set; }
    public virtual User Contact { get; private set; }

    private UserContact()
    { }

    public static UserContact Create(Guid userId, Guid contactId)
    {
        if (userId == contactId)
            throw new ArgumentException("Un usuario no puede agregarse a sí mismo como contacto");

        return new UserContact
        {
            UserId = userId,
            ContactId = contactId,
            CreatedAt = DateTime.UtcNow
        };
    }
}