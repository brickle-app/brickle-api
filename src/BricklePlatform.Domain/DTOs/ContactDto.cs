namespace BricklePlatform.Domain.DTOs;

public class ContactDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string? WalletAddress { get; set; }
    public string? ProfilePictureUrl { get; set; }
}