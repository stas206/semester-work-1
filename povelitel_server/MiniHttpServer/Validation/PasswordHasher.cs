using System.Security.Cryptography;
using System.Text;

namespace MiniHttpServer.Validation;

public static class PasswordHasher
{
    private const int Iterations = 10000;
    private const int KeyLength = 32;

    public static string GenerateSalt()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
    }

    public static string Hash(string password, string salt)
    {
        var passwordBytes = Encoding.UTF8.GetBytes(password ?? string.Empty);
        var saltBytes = Encoding.UTF8.GetBytes(salt ?? string.Empty);
        using var pbkdf2 = new Rfc2898DeriveBytes(passwordBytes, saltBytes, Iterations, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(KeyLength));
    }

    public static bool Verify(string password, string salt, string hash)
    {
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
            return false;
        var computed = Hash(password, salt);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed), Encoding.UTF8.GetBytes(hash));
    }
}
