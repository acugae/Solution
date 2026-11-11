namespace Solution.Security;

/// <summary>
/// Hash delle password con PBKDF2-SHA256, salt casuale e numero di iterazioni salvato insieme all'hash.
/// Da usare al posto di HashAlgorithm (MD5/SHA) quando il valore da proteggere è una password.
/// </summary>
/// <example>
///     <code>
///string stored = PasswordHasher.Hash("la-password");
///bool ok = PasswordHasher.Verify("la-password", stored);
///     </code>
/// </example>
public static class PasswordHasher
{
    private const string Prefix = "PBKDF2-SHA256";
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 600000;
    private const int MaxIterations = 10000000;

    /// <summary>Restituisce l'hash da salvare, nel formato PBKDF2-SHA256$iterazioni$salt$hash (Base64).</summary>
    public static string Hash(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return Prefix + "$" + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
    }

    /// <summary>Verifica la password rispetto a un hash prodotto da Hash. Restituisce false se l'hash non è nel formato atteso.</summary>
    public static bool Verify(string password, string storedHash)
    {
        if (password == null || string.IsNullOrEmpty(storedHash))
            return false;

        string[] parts = storedHash.Split('$');
        if (parts.Length != 4 || parts[0] != Prefix)
            return false;
        if (!int.TryParse(parts[1], out int iterations) || iterations <= 0 || iterations > MaxIterations)
            return false;

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch (FormatException)
        {
            return false;
        }
        if (salt.Length == 0 || expected.Length == 0)
            return false;

        byte[] actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
