using System.Security.Claims;
using System.Security.Cryptography;
using System.Xml;
using Microsoft.IdentityModel.Tokens;
using Solution.Data;
using Solution.IO;
using Solution.Security;
using Solution.SolutionMapper;

namespace Solution.Tests;

public class SecurityHardeningTests
{
    [Fact]
    public void Jwt_Read_RejectsInvalidSignature_AndAcceptsValidToken()
    {
        const string secret = "super-secret-signing-key-with-at-least-32-chars!";
        const string wrongSecret = "wrong-secret-signing-key-with-at-least-32-chars!";
        string token = JWT.Create(secret, 10, new[] { new Claim("sub", "user-1") });

#pragma warning disable CS0618
        var claims = JWT.Read(secret, token);
        Assert.Contains(claims, c => c.Type == "sub" && c.Value == "user-1");

        Assert.ThrowsAny<SecurityTokenException>(() => JWT.Read(wrongSecret, token));
#pragma warning restore CS0618
    }

    [Fact]
    public void AsymmetricCryptAlgorithm_DefaultDsa_ThrowsInsteadOfReturningPlaintext()
    {
        var dsa = new AsymmetricCryptAlgorithm();
        var keys = new CryptKeys { PublicKey = "<RSAKeyValue/>", PrivateKey = "<RSAKeyValue/>" };

        Assert.Throws<NotSupportedException>(() => dsa.Encrypt("segreto", keys));
        Assert.Throws<NotSupportedException>(() => dsa.Decrypt("c2VncmV0bw==", keys));
    }

    [Fact]
    public void SymmetricCryptAlgorithm_UseSecureMode_AndAuthenticatedAead_WorkAndDetectTampering()
    {
        using var crypt = new SymmetricCryptAlgorithm();
        crypt.UseSecureMode();

        string c1 = crypt.Encrypt("dato-sensibile", "password-forte");
        string c2 = crypt.Encrypt("dato-sensibile", "password-forte");
        Assert.NotEqual(c1, c2);
        Assert.Equal("dato-sensibile", crypt.Decrypt(c1, "password-forte"));

        string aead = crypt.EncryptAuthenticated("Testo AEAD àèìòù", "password-forte");
        Assert.StartsWith("$v2$", aead);
        Assert.Equal("Testo AEAD àèìòù", crypt.DecryptAuthenticated(aead, "password-forte"));

        // Lazy re-encryption detection
        string? autoV2 = crypt.DecryptAuto(aead, "password-forte", out bool usedLegacyV2);
        Assert.False(usedLegacyV2);
        Assert.Equal("Testo AEAD àèìòù", autoV2);

        using var legacyCrypt = new SymmetricCryptAlgorithm();
        string legacyCipher = legacyCrypt.Encrypt("legacy-value", "password-forte");
        string? autoLegacy = crypt.DecryptAuto(legacyCipher, "password-forte", out bool usedLegacy);
        Assert.True(usedLegacy);
        Assert.Equal("legacy-value", autoLegacy);

        // Tampering must fail authentication tag verification
        byte[] raw = Convert.FromBase64String(aead["$v2$".Length..]);
        raw[^1] ^= 0xFF;
        string tampered = "$v2$" + Convert.ToBase64String(raw);
        Assert.ThrowsAny<CryptographicException>(() => crypt.DecryptAuthenticated(tampered, "password-forte"));
    }

    [Fact]
    public void Connection_Clone_PreservesCredentialsWhenProviderRedactsThem()
    {
        var db = new DB();
        const string connectionString = "Server=localhost;Initial Catalog=test;User ID=test;Password=secret;Persist Security Info=False;";
        db.DataManager.Connections.Add("sql", "sqldb", connectionString);

        var connection = db.DataManager.Connections["sql"];
        connection.DbConnection.ConnectionString = "Server=localhost;Initial Catalog=test;Integrated Security=True;Persist Security Info=False;";

        Assert.Equal(connectionString, connection.ConnectionString);
        using var clone = db.DataManager.Connections.Clone("sql");
        Assert.Equal(connectionString, clone.ConnectionString);
    }

    [Fact]
    public void QueryBuilder_AndCrud_RejectUnsafeIdentifiersAndOperators()
    {
        var db = new DB();
        db.AddConnection("mem", c => c.UseProvider("litedb").WithConnectionString("Data Source=:memory:;"));

        Assert.Throws<ArgumentException>(() =>
            db["mem"].From("Users; DROP TABLE Users;--").ToSql());

        Assert.Throws<ArgumentException>(() =>
            db["mem"].From("Users").Where("Name", "= 1 OR 1=1 --", "test").ToSql());

        Assert.Throws<ArgumentException>(() =>
            db["mem"].From("Users").OrderBy("Name", "ASC; DROP TABLE Users").ToSql());

        string safeSql = db["mem"]
            .From("Users")
            .Columns("Id", "Name")
            .Where("Name", "=", "O'Brien")
            .OrderBy("Name", "DESC")
            .ToSql();

        Assert.Contains("@qb_0", safeSql);
        Assert.DoesNotContain("O'Brien", safeSql);
    }

    [Fact]
    public void FileManager_AllowedBasePath_BlocksSiblingDirectoryPrefixBypass()
    {
        using var parent = new TemporaryDirectory();
        string allowedDir = Path.Combine(parent.Path, "Data");
        string siblingDir = Path.Combine(parent.Path, "DataSecret");
        Directory.CreateDirectory(allowedDir);
        Directory.CreateDirectory(siblingDir);

        string allowedFile = Path.Combine(allowedDir, "ok.txt");
        string siblingFile = Path.Combine(siblingDir, "secret.txt");
        File.WriteAllText(allowedFile, "ok");
        File.WriteAllText(siblingFile, "secret");

        try
        {
            FileManager.SetAllowedBasePath(allowedDir);
            Assert.Equal("ok", FileManager.GetFile(allowedFile));
            Assert.Throws<UnauthorizedAccessException>(() => FileManager.GetFile(siblingFile));
        }
        finally
        {
            FileManager.SetAllowedBasePath(null);
        }

        string rooted = FileManager.GetPathRoot("~/sub/file.txt");
        Assert.StartsWith(Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory), rooted, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Zip_UnZip_EnforcesActualDecompressedSizeLimit()
    {
        var zip = new ZIP();
        byte[] payload = new byte[4096];
        Array.Fill(payload, (byte)'A');
        byte[] archive = zip.Zip(new Dictionary<string, byte[]> { ["big.txt"] = payload });

        long previousLimit = ZIP.MaxExtractSize;
        try
        {
            ZIP.MaxExtractSize = 1024;
            Assert.Throws<InvalidOperationException>(() => zip.UnZip(archive));
        }
        finally
        {
            ZIP.MaxExtractSize = previousLimit;
        }
    }

    [Fact]
    public void Xml_LoadXml_ProhibitsDtdProcessing()
    {
        const string dtdXml = "<!DOCTYPE foo [<!ENTITY xxe \"boom\">]><root>&xxe;</root>";
        var xml = new XML();
        Assert.Throws<XmlException>(() => xml.LoadXml(dtdXml));
    }

    [Fact]
    public void CacheKey_DoesNotCollideOnDistinctUnicodeKeys()
    {
        var k1 = new CacheKey("caffè");
        var k2 = new CacheKey("caffé");
        var k3 = new CacheKey("caff€");

        Assert.NotEqual(k1.QueryString, k2.QueryString);
        Assert.NotEqual(k1.QueryString, k3.QueryString);
        Assert.False(k1.Equals((CacheKey?)null));
    }

    [Fact]
    public void SolutionMapper_PreventsCyclicStackOverflow_AndHonorsIgnorePropsAndInaccessibleSetters()
    {
        var mapper = new Solution.SolutionMapper.SolutionMapper();
        var rule = new SolutionMappingExpression<CyclicNode, CyclicNodeDto>();
        rule.IgnoreAllPropertiesWithAnInaccessibleSetter();
        mapper.AddMapping(rule);

        var nodeA = new CyclicNode { Name = "A", Role = "Admin", InternalFlag = "ShouldNotOverwrite" };
        var nodeB = new CyclicNode { Name = "B", Role = "User", Next = nodeA };
        nodeA.Next = nodeB;

        var ctx = new ResolutionContext { MaxDepth = 5 };
        ctx.Items["IgnoreProps"] = new HashSet<string> { nameof(CyclicNodeDto.Role) };

        var dto = mapper.Map<CyclicNode, CyclicNodeDto>(nodeA, ctx);

        Assert.Equal("A", dto.Name);
        Assert.Equal("Guest", dto.Role); // Ignored via IgnoreProps in default auto-map!
        Assert.Equal("Default", dto.InternalFlag); // Private setter protected!
        Assert.NotNull(dto.Next);
        Assert.Equal("B", dto.Next!.Name);
        Assert.Same(dto, dto.Next.Next); // Cyclic reference resolved via InstanceCache!
    }

    [Fact]
    public void StringAdvance_Generate_ProducesPasswordsOfExpectedLength()
    {
        var sa = new StringAdvance();
        string p1 = sa.Generate(16, 16);
        string p2 = sa.Generate(16, 16);
        Assert.Equal(16, p1.Length);
        Assert.Equal(16, p2.Length);
        Assert.NotEqual(p1, p2);
    }

    [Fact]
    public void Xls_ConvertDataTableToCsv_NeutralizesFormulaInjection()
    {
        var dt = new System.Data.DataTable();
        dt.Columns.Add("Name");
        dt.Columns.Add("Note");
        dt.Rows.Add("=CMD|' /C calc'!A0", "+1+1");
        dt.Rows.Add("Mario", "Test;with;semicolon");

        var xls = new XLS();
        string csv = xls.ConvertDataTableToCSV(dt, ";");

        Assert.Contains("'=CMD|' /C calc'!A0", csv);
        Assert.Contains("'+1+1", csv);
        Assert.Contains("\"Test;with;semicolon\"", csv);
    }

    [Fact]
    public void Db_ImplementsDisposable_Cleanly()
    {
        using var db = new DB();
        db.AddConnection("mem", c => c.UseProvider("litedb").WithConnectionString("Data Source=:memory:;"));
        Assert.NotNull(db.DataManager);
    }

    [Fact]
    public void InputGuard_ValidatesSqlIdentifiersFragmentsPathsAndTypes()
    {
        Assert.Equal("[dbo].[Users]", InputGuard.ValidateSqlIdentifier("dbo.Users"));
        Assert.True(InputGuard.IsSafeSqlIdentifier("Users"));
        Assert.False(InputGuard.IsSafeSqlIdentifier("Users; DROP TABLE Users"));

        Assert.Equal("AND a.Active = 1", InputGuard.ValidateSqlFragment("AND a.Active = 1"));
        Assert.False(InputGuard.IsSafeSqlFragment("1=1; DROP TABLE Users;--"));
        Assert.Throws<ArgumentException>(() => InputGuard.ValidateSqlFragment("1=1; EXEC xp_cmdshell 'whoami'"));

        using var dir = new TemporaryDirectory();
        string inside = Path.Combine(dir.Path, "plugin.dll");
        Assert.Equal(Path.GetFullPath(inside), InputGuard.ValidateAssemblyPath(inside, dir.Path));
        Assert.Throws<UnauthorizedAccessException>(() => InputGuard.ValidateSafePath(Path.Combine(dir.Path, "..", "outside.dll"), dir.Path));
        Assert.Throws<ArgumentException>(() => InputGuard.ValidateAssemblyPath(Path.Combine(dir.Path, "script.exe"), dir.Path));

        Assert.Equal(typeof(CyclicNode), InputGuard.ValidateAllowedType(typeof(CyclicNode), typeof(object)));
        Assert.Throws<UnauthorizedAccessException>(() => InputGuard.ValidateAllowedType(typeof(CyclicNode), typeof(IDisposable)));
    }

    private sealed class CyclicNode
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string InternalFlag { get; set; } = string.Empty;
        public CyclicNode? Next { get; set; }
    }

    private sealed class CyclicNodeDto
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = "Guest";
        public string InternalFlag { get; private set; } = "Default";
        public CyclicNodeDto? Next { get; set; }
    }
}
