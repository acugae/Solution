using System.Reflection;
using Solution.Reflection;
using Solution.Security;

namespace Solution.Tests;

public class SecurityAndReflectionTests
{
    [Fact]
    public void SymmetricCryptAlgorithm_EncryptDecrypt_RoundTripsText()
    {
        var crypt = new SymmetricCryptAlgorithm();
        const string text = "Testo riservato 123";

        var encrypted = crypt.Encrypt(text, "password-di-test");
        var decrypted = crypt.Decrypt(encrypted, "password-di-test");

        Assert.NotEqual(text, encrypted);
        Assert.Equal(text, decrypted);
    }

    [Fact]
    public void SymmetricCryptAlgorithm_SecureEncryption_UsesRandomCiphertextAndRoundTrips()
    {
        var crypt = new SymmetricCryptAlgorithm();
        const string text = "Testo UTF-8: àèìòù";

        var first = crypt.EncryptSecure(text, "password-di-test");
        var second = crypt.EncryptSecure(text, "password-di-test");

        Assert.NotEqual(first, second);
        Assert.Equal(text, crypt.DecryptSecure(first, "password-di-test"));
        Assert.Equal(text, crypt.DecryptSecure(second, "password-di-test"));
    }

    [Fact]
    public void ReflectionManager_CallsMethodsAndReadsAndWritesProperties()
    {
        var manager = new ReflectionManager();
        var subject = new ReflectionSubject { Name = "prima" };

        manager.CallPropertySet(subject, nameof(ReflectionSubject.Name), "dopo");
        var result = manager.CallMethod(subject, nameof(ReflectionSubject.Compose), "prefisso");
        var properties = manager.GetProperties(subject, BindingFlags.Public | BindingFlags.Instance);

        Assert.Equal("dopo", manager.CallPropertyGet(subject, nameof(ReflectionSubject.Name)));
        Assert.Equal("prefisso:dopo", result);
        Assert.True(properties.ContainsKey(nameof(ReflectionSubject.Name)));
        Assert.True(manager.ExistMethod(subject, nameof(ReflectionSubject.Compose)));
    }

    private sealed class ReflectionSubject
    {
        public string Name { get; set; } = string.Empty;

        public string Compose(string prefix) => $"{prefix}:{Name}";
    }
}
