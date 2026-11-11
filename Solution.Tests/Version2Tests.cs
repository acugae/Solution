using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Solution.Data;
using Solution.IO;
using Solution.Security;

namespace Solution.Tests;

public class Version2Tests
{
    [Fact]
    public void EncryptAuthenticated_WithPassword_RoundTripsAndUsesRandomCiphertext()
    {
        const string text = "Testo UTF-8: àèìòù";

        var first = SymmetricCryptAlgorithm.EncryptAuthenticated(text, "password-di-test");
        var second = SymmetricCryptAlgorithm.EncryptAuthenticated(text, "password-di-test");

        Assert.NotEqual(first, second);
        Assert.Equal(text, SymmetricCryptAlgorithm.DecryptAuthenticated(first, "password-di-test"));
        Assert.ThrowsAny<CryptographicException>(() => SymmetricCryptAlgorithm.DecryptAuthenticated(first, "password-sbagliata"));
    }

    [Fact]
    public void EncryptAuthenticated_WithKey_DetectsTampering()
    {
        byte[] key = SymmetricCryptAlgorithm.GenerateKey();
        byte[] plain = Encoding.UTF8.GetBytes("dato riservato");

        byte[] encrypted = SymmetricCryptAlgorithm.EncryptAuthenticated(plain, key);
        Assert.Equal(plain, SymmetricCryptAlgorithm.DecryptAuthenticated(encrypted, key));

        encrypted[encrypted.Length - 1] ^= 0x01;
        Assert.ThrowsAny<CryptographicException>(() => SymmetricCryptAlgorithm.DecryptAuthenticated(encrypted, key));
    }

    [Fact]
    public void PasswordHasher_VerifiesOnlyTheRightPassword()
    {
        string stored = PasswordHasher.Hash("la-password");

        Assert.StartsWith("PBKDF2-SHA256$", stored);
        Assert.True(PasswordHasher.Verify("la-password", stored));
        Assert.False(PasswordHasher.Verify("altra-password", stored));
        Assert.False(PasswordHasher.Verify("la-password", "non-un-hash"));
    }

    [Fact]
    public void AsymmetricCryptAlgorithm_DefaultIsRsaAndRoundTrips()
    {
        var crypt = new AsymmetricCryptAlgorithm();
        CryptKeys keys = crypt.CreateKeys();

        string encrypted = crypt.Encrypt("messaggio", keys);

        Assert.NotEqual("messaggio", Encoding.UTF8.GetString(Convert.FromBase64String(encrypted)));
        Assert.Equal("messaggio", crypt.Decrypt(encrypted, keys));
    }

    [Fact]
    public void AsymmetricCryptAlgorithm_DsaCannotEncrypt()
    {
        var crypt = new AsymmetricCryptAlgorithm(AsymmetricCryptAlgorithm.ServiceProviderEnum.DSA);

        Assert.Throws<NotSupportedException>(() => crypt.Encrypt(new byte[] { 1, 2, 3 }, new CryptKeys()));
    }

    [Fact]
    public void QueryBuilder_ToSql_QuotesNamesAndEscapesValues()
    {
        string sql = new DB()["main"].From("Users").Where("Name", "=", "O'Brien").OrderBy("Name").ToSql();

        Assert.Equal("SELECT * FROM [Users] WHERE [Name] = 'O''Brien' ORDER BY [Name] ASC", sql);
    }

    [Fact]
    public void QueryBuilder_RejectsInvalidNamesAndOperators()
    {
        Assert.Throws<ArgumentException>(() => new DB()["main"].From("Users]; DROP TABLE Users;--").ToSql());
        Assert.Throws<ArgumentException>(() => new DB()["main"].From("Users").Where("Name", "= 1 OR 1", 1).ToSql());
        Assert.Throws<ArgumentException>(() => new DB()["main"].From("Users").OrderBy("Name", "asc; DROP TABLE Users").ToSql());
    }

    [Fact]
    public void Zip_AcceptsFileNamesContainingTwoDots()
    {
        var zip = new ZIP();
        var files = new Dictionary<string, byte[]> { ["report..finale.txt"] = Encoding.UTF8.GetBytes("contenuto") };

        var extracted = zip.UnZip(zip.Zip(files));

        Assert.Equal(files["report..finale.txt"], extracted["report..finale.txt"]);
    }

    [Fact]
    public void Zip_CompressObject_RoundTrips()
    {
        var zip = new ZIP();

        byte[] compressed = zip.CompressObject(new { Name = "valore" });
        var result = (JsonElement)zip.DecompressObject(compressed);

        Assert.Equal("valore", result.GetProperty("Name").GetString());
    }
}
