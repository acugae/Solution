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

    [Fact]
    public void UseSecureMode_EnablesRandomCiphertextInEncryptDecrypt()
    {
#pragma warning disable CS0618
        var crypt = new SymmetricCryptAlgorithm().UseSecureMode();
        const string text = "Testo UTF-8 sicuro: àèìòù";

        string first = crypt.Encrypt(text, "password-di-test");
        string second = crypt.Encrypt(text, "password-di-test");

        Assert.NotEqual(first, second);
        Assert.Equal(text, crypt.Decrypt(first, "password-di-test"));
        Assert.Null(crypt.Decrypt(first, "password-errata"));
#pragma warning restore CS0618
    }

    [Fact]
    public void JwtRead_ValidatesSignatureAndRejectsWrongKey()
    {
#pragma warning disable CS0618
        const string validKey = "chiave-segreta-molto-lunga-almeno-32-byte!!";
        const string wrongKey = "altra-chiave-segreta-molto-lunga-32-byte!!";

        string token = JWT.Create(validKey, 10, []);
        Assert.NotEmpty(JWT.Read(validKey, token));
        Assert.ThrowsAny<Exception>(() => JWT.Read(wrongKey, token));
#pragma warning restore CS0618
    }

    [Fact]
    public void CacheValue_WhenZipped_CompressesAndDecompressesValue()
    {
        string repeated = new string('A', 2000);
        var zipped = new CacheValue<string>(repeated, 10, zip: true);
        var unzipped = new CacheValue<string>(repeated, 10, zip: false);

        Assert.Equal(repeated, zipped.Value);
        Assert.True(zipped.Size < unzipped.Size);
    }

    [Fact]
    public void JsonDeserialize_RejectsUnsafeTypeNameHandlingWithoutBinder()
    {
        var unsafeSettings = new Newtonsoft.Json.JsonSerializerSettings
        {
            TypeNameHandling = Newtonsoft.Json.TypeNameHandling.All
        };

        Assert.Throws<InvalidOperationException>(() => JSON.Deserialize<object>("{}", unsafeSettings));
    }
}
