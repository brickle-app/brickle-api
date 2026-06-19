using BricklePlatform.Infrastructure.Interfaces;
using System.Security.Cryptography;

namespace BricklePlatform.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    private const int SaltSize = 16;
    private const int HashSize = 20;
    private const int Iterations = 10000;

    public (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        byte[] salt = new byte[SaltSize];
        using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);
            return (hash, salt);
        }
    }

    public bool VerifyPassword(string password, byte[] storedHash, byte[] storedSalt)
    {
        using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, Iterations))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);
            return hash.SequenceEqual(storedHash);
        }
    }
}