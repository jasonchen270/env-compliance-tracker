using System.Security.Cryptography;
using System.Text;

namespace EnvComplianceTracker.Infrastructure;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes("ect-salt:" + password));
        return Convert.ToHexString(bytes);
    }

    public static bool Verify(string password, string hash) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(Hash(password)),
            Encoding.UTF8.GetBytes(hash));
}
