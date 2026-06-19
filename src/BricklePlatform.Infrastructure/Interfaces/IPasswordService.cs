namespace BricklePlatform.Infrastructure.Interfaces;

public interface IPasswordService
{
    (byte[] Hash, byte[] Salt) HashPassword(string password);
    bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt);
}