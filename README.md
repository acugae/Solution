# Solution

[![NuGet](https://img.shields.io/nuget/v/Solution.svg)](https://www.nuget.org/packages/Solution)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)

A comprehensive .NET library providing utilities for database operations, I/O, security, reflection, object mapping, and much more, now with built-in SQLite provider support.

## 📦 Installation

```bash
dotnet add package Solution
```

Or via Package Manager:
```powershell
Install-Package Solution
```

## ⚡ Quick Start

```csharp
using Solution.Data;

// 1. Setup connection
var db = new DB()
    .AddSqlServer("main", "Server=localhost;Database=mydb;Integrated Security=True;");

// 2. CRUD operations with anonymous objects
db["main"].Insert("Users", new { Name = "John", Email = "john@email.com" });
db["main"].Update("Users", new { Name = "John Doe" }, new { Id = 1 });
db["main"].Delete("Users", new { Id = 1 });

// 3. Query with automatic mapping
List<User> users = db["main"].Query<User>("Users", new { Active = true });
User? user = db["main"].QueryFirst<User>("Users", new { Id = 1 });

// 4. Transactions with automatic rollback
using (var tx = db.BeginTransaction("main"))
{
    db["main"].Insert("Users", new { Name = "Jane" });
    tx.Commit();
}
```

## 🚀 Features

### 📊 Data - Database Access

Multi-database support (SQL Server, MySQL, PostgreSQL, SQLite) with simplified CRUD operations and fluent API.

#### Connection Setup

```csharp
// Fluent API with builder pattern
var db = new DB()
    .AddConnection("main", c => c
        .UseSqlServer()
        .WithConnectionString("Server=localhost;Database=mydb;Integrated Security=True;"))
    .AddConnection("logs", c => c
        .UsePostgreSQL()
        .ForPostgreSQL("localhost", "logs", "user", "password"));

// SQLite using the built-in provider key
var sqliteDb = new DB()
    .AddConnection("local", c => c
        .UseProvider("litedb")
        .WithConnectionString("Data Source=app.db;"));

// Shorthand methods
var db = new DB()
    .AddSqlServer("main", "Server=localhost;Database=mydb;...")
    .AddMySQL("secondary", "Server=localhost;Database=other;...");

// Built-in provider keys: sqldb, mysdb, pstdb, litedb
// Fluent helpers: UseSqlServer(), UseMySQL(), UsePostgreSQL()
// Connection helpers: ForSqlServer(), ForMySQL(), ForPostgreSQL()
// For SQLite use: UseProvider("litedb").WithConnectionString("Data Source=app.db;")
```

#### SQLite

SQLite support is available through the built-in `litedb` provider key.

```csharp
var db = new DB()
    .AddConnection("local", c => c
        .UseProvider("litedb")
        .WithConnectionString("Data Source=app.db;"));

db["local"].Insert("Users", new { Name = "John", Email = "john@email.com" });
List<User> users = db["local"].Query<User>("Users");
```

#### CRUD Operations

```csharp
// Insert
db["main"].Insert("Users", new { Name = "John", Email = "john@email.com" });
long? id = db["main"].InsertWithReturn("Users", new { Name = "John" }); // Returns generated ID

// Update
db["main"].Update("Users", new { Name = "John Doe" }, new { Id = 1 });

// Delete
db["main"].Delete("Users", new { Id = 1 });

// Upsert (Insert or Update based on key)
db["main"].Upsert("Users", new { Id = 1, Name = "John", Email = "john@mail.com" }, "Id");
```

#### Query & Mapping

```csharp
// Query to DataTable
DataTable dt = db.Query("main", "SELECT * FROM Users WHERE Active = @Active", new { Active = true });

// Query with automatic mapping to typed objects
List<User> users = db["main"].Query<User>("Users");
List<User> activeUsers = db["main"].Query<User>("Users", new { Active = true });
User? user = db["main"].QueryFirst<User>("Users", new { Id = 1 });

// SQL queries with mapping
var admins = db.Query<User>("main", "SELECT * FROM Users WHERE Role = @Role", new { Role = "Admin" });
var user = db.QueryFirst<User>("main", "SELECT * FROM Users WHERE Id = @Id", new { Id = 1 });

// Scalar values
int count = db.Scalar<int>("main", "SELECT COUNT(*) FROM Users");
string name = db.Scalar<string>("main", "SELECT Name FROM Users WHERE Id = @Id", new { Id = 1 });
```

#### Utility Methods

```csharp
// Check existence
bool exists = db["main"].Exists("Users", new { Email = "john@email.com" });

// Count records
int total = db["main"].Count("Users");
int active = db["main"].Count("Users", new { Active = true });
```

#### Async Methods

`DB` also exposes asynchronous methods that use a dedicated cloned connection for each operation.

```csharp
// Raw SQL query
DataTable? usersTable = await db.InvokeSQLAsync(
    "main",
    "SELECT * FROM Users WHERE Active = @Active",
    cancellationToken,
    db.CreateParameter("main", DbType.Boolean, ParameterDirection.Input, "@Active", true));

// Stored procedure with parameters
DataTable? profileTable = await db.InvokeAsync(
    "main",
    "sp_GetUserProfile",
    cancellationToken,
    db.CreateParameter("main", DbType.Int32, ParameterDirection.Input, "@Id", 1));

// Stored procedure without parameters
DataTable? dashboardTable = await db.InvokeAsync("main", "sp_GetDashboard", cancellationToken);

// SQL query with dictionary parameters
DataTable? filteredUsers = await db.GetAsync(
    "main",
    "SELECT * FROM Users WHERE Role = @Role",
    new Dictionary<string, object> { ["@Role"] = "Admin" },
    cancellationToken);

// Non query command
int affectedRows = await db.ExecuteAsync(
    "main",
    "UPDATE Users SET Active = 0 WHERE LastLogin < '2024-01-01'",
    cancellationToken);
```

Available async overloads:

- `InvokeSQLAsync(string sSQL, params Parameter[] pParams)`
- `InvokeSQLAsync(string sKey, string sSQL, CancellationToken cancellationToken = default, params Parameter[] pParams)`
- `InvokeAsync(string sSQL, params Parameter[] pParams)`
- `InvokeAsync(string sKey, string sSQL, CancellationToken cancellationToken = default, params Parameter[] pParams)`
- `InvokeAsync(string sSQL)`
- `InvokeAsync(string sKey, string sSQL, CancellationToken cancellationToken = default)`
- `GetAsync(string sSQL, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)`
- `GetAsync(string sKey, string sSQL, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)`
- `ExecuteAsync(string sSQL, CancellationToken cancellationToken = default)`
- `ExecuteAsync(string sKey, string sSQL, CancellationToken cancellationToken = default)`

#### Transactions

```csharp
// Using pattern with automatic rollback
using (var tx = db.BeginTransaction("main"))
{
    db["main"].Insert("Users", new { Name = "John" });
    db["main"].Insert("Logs", new { Action = "UserCreated" });
    tx.Commit(); // If not called, automatic rollback on dispose
}

// Helper method (auto-commit on success, rollback on exception)
db.InTransaction("main", () => {
    db["main"].Insert("Users", new { Name = "John" });
    db["main"].Insert("Logs", new { Action = "UserCreated" });
});

// With return value
long? userId = db.InTransaction("main", () => {
    return db["main"].InsertWithReturn("Users", new { Name = "John" });
});
```

#### Fluent Query Builder

Build queries with a fluent, readable syntax:

```csharp
// Simple query with filtering and ordering
var users = db["main"]
    .From("Users")
    .Where(new { Active = true, Role = "Admin" })
    .OrderBy("Name")
    .Take(10)
    .Select<User>();

// Advanced filtering
var results = db["main"]
    .From("Orders")
    .Where("Total", ">", 100)
    .Where("Status", "Pending")
    .WhereBetween("CreatedAt", startDate, endDate)
    .WhereIn("Category", "Electronics", "Books", "Clothing")
    .OrderByDesc("CreatedAt")
    .Select<Order>();

// Pagination
var page2 = db["main"]
    .From("Products")
    .Where(new { InStock = true })
    .OrderBy("Name")
    .Page(2, 20)  // Page 2, 20 items per page
    .Select<Product>();

// Utility methods
int count = db["main"].From("Users").Where(new { Active = true }).Count();
bool exists = db["main"].From("Users").Where(new { Email = "john@email.com" }).Exists();
var first = db["main"].From("Users").Where(new { Id = 1 }).SelectFirst<User>();

// Get generated SQL (for debugging)
string sql = db["main"].From("Users").Where(new { Active = true }).ToSql();
```

#### Dependency Injection

Register DB in your ASP.NET Core application:

```csharp
// In Program.cs or Startup.cs

// Simple registration
services.AddSolutionDB(db => db
    .AddSqlServer("main", Configuration.GetConnectionString("Main")));

// SQLite registration through the custom provider key
services.AddSolutionDB(db => db
    .AddConnection("local", c => c
        .UseProvider("litedb")
        .WithConnectionString("Data Source=app.db;")));

// With multiple connections
services.AddSolutionDB(db => db
    .AddSqlServer("main", Configuration.GetConnectionString("Main"))
    .AddPostgreSQL("analytics", Configuration.GetConnectionString("Analytics")));

// Shorthand for single connection
services.AddSolutionDB("main", connectionString, DatabaseProvider.SqlServer);

// From appsettings.json section (recommended)
services.AddSolutionDB(Configuration.GetSection("SolutionDB"));

// From standard ConnectionStrings section
services.AddSolutionDBFromConnectionString(Configuration, "Main");
services.AddSolutionDBFromConnectionString(Configuration, "Logs", DatabaseProvider.PostgreSQL);

// Multiple ConnectionStrings with provider mapping
services.AddSolutionDBFromConnectionStrings(Configuration, 
    new Dictionary<string, DatabaseProvider> {
        { "Main", DatabaseProvider.SqlServer },
        { "Logs", DatabaseProvider.PostgreSQL }
    });
```

**appsettings.json examples:**

```json
// Option 1: SolutionDB section (full control)
{
  "SolutionDB": {
    "Lifetime": "Scoped",
    "Connections": [
      {
        "Name": "main",
        "Provider": "SqlServer",
        "ConnectionString": "Server=localhost;Database=mydb;Integrated Security=True;"
      },
      {
        "Name": "logs",
        "Provider": "PostgreSQL",
        "ConnectionString": "Host=localhost;Database=logs;Username=user;Password=pass;"
      }
    ]
  }
}

// Option 2: Standard ConnectionStrings (ASP.NET Core convention)
{
  "ConnectionStrings": {
    "Main": "Server=localhost;Database=mydb;...",
    "Logs": "Host=localhost;Database=logs;..."
  }
}
```

```csharp
// Inject in your services
public class UserService
{
    private readonly DB _db;
    
    public UserService(DB db) => _db = db;
    
    public List<User> GetActiveUsers() => 
        _db["main"].Query<User>("Users", new { Active = true });
}
```

### 🗃️ DbOperations - Bulk Operations

Optimized bulk operations for large data volumes.

```csharp
// Bulk Insert
var users = new List<User> { ... };
connection.BulkInsert(users, options => {
    options.TableName = "Users";
    options.PrimaryKey = x => x.Id;
});

// Bulk Update
connection.BulkUpdate(users, options => {
    options.TableName = "Users";
    options.JoinColumns = x => x.Id;
});

// Create table from type
connection.CreateTable<User>(options => {
    options.TableName = "Users";
    options.PrimaryKey = x => x.Id;
});
```

### 📁 IO - Input/Output

#### File Manager
```csharp
// Read file
string content = FileManager.Read("path/to/file.txt");
byte[] bytes = FileManager.ReadByte("path/to/file.bin");

// Write file
FileManager.Write("path/to/file.txt", "content");
FileManager.WriteByte("path/to/file.bin", byteArray);
```

#### Excel (XLS/XLSX)
```csharp
// Read Excel
var xls = new XLS("file.xlsx");
DataTable data = xls.GetDataTable("Sheet1");

// Write Excel
xls.SetDataTable(dataTable, "Sheet1");
xls.Write("output.xlsx");

// Export from DataTable
dataTable.ToExcel("export.xlsx");
```

#### PDF
```csharp
var pdf = new PDF("template.pdf");

// Fill form fields
pdf.SetField("name", "John Doe");
pdf.SetField("date", DateTime.Now.ToString());

// Merge PDFs
PDF.Merge(new[] { "doc1.pdf", "doc2.pdf" }, "merged.pdf");

// Extract pages
pdf.ExtractPages(1, 5, "extracted.pdf");
```

#### FTP / SFTP
```csharp
// FTP (enableSsl: true for FTPS)
var ftp = new FTP();
var request = ftp.CreateFtpWebRequest("ftp://server.com/file.txt", "user", "password", enableSsl: true);
ftp.DownloadFile("user", "password", "ftp://server.com/file.txt", "local.txt");

// SFTP with SSH host key fingerprint verification (recommended)
using var sftp = new SFTP("server.com", "user", "password")
    .WithHostKeyFingerprint("SHA256:expected-host-fingerprint");
sftp.Connect();
sftp.Upload("/remote/path/file.txt", fileBytes);
byte[] downloaded = sftp.Download("/remote/path/file.txt");
```

#### ZIP
```csharp
// Compression
var files = new Dictionary<string, byte[]> {
    { "file1.txt", bytes1 },
    { "file2.txt", bytes2 }
};
byte[] zipData = ZIP.Compress(files);

// Decompression
var extracted = ZIP.Decompress(zipData);
```

#### Named Pipes
```csharp
// Server
var server = new PipeServer("MyPipe");
server.Write("Message to client");

// Client
var client = new PipeClient("MyPipe");
client.OnMessage += (msg) => Console.WriteLine(msg);
await client.ConnectAsync();
```

### 📧 Communication - Email

```csharp
var email = new Email("smtp.server.com")
{
    EnableSsl = true // Enable STARTTLS/TLS (recommended)
};

// Simple email
email.SendMail(
    sFrom: "sender@email.com",
    sTo: "recipient@email.com",
    sCC: null,
    Subject: "Subject",
    sBody: "<h1>HTML Content</h1>",
    sFile: null,
    IsHtml: true
);

// With attachments
email.SendMailAttach(
    sFrom: "sender@email.com",
    sTo: "dest1@email.com;dest2@email.com",
    sCC: "cc@email.com",
    Subject: "Documents",
    sBody: "Please find the documents attached",
    sFiles: new[] { "doc1.pdf", "doc2.xlsx" }
);
```

### 🔐 Security - Cryptography, JWT & Input Validation

#### Symmetric Encryption (AES-256-GCM AEAD — Recommended)
```csharp
using var crypt = new SymmetricCryptAlgorithm();

// Authenticated encryption (AES-256-GCM + PBKDF2-SHA256 600k iterations, "$v2$" format)
string encrypted = crypt.EncryptAuthenticated("sensitive text", "strong-password");
string decrypted = crypt.DecryptAuthenticated(encrypted, "strong-password");

// Dual-mode decryption for lazy re-encryption of legacy ciphertexts
string? plain = crypt.DecryptAuto(storedValue, "strong-password", out bool usedLegacyFormat);
if (usedLegacyFormat && plain != null)
{
    string upgraded = crypt.EncryptAuthenticated(plain, "strong-password");
    // Persist upgraded ciphertext back to storage
}

// Or enable random-IV secure mode on Encrypt/Decrypt
crypt.UseSecureMode();
string secureCipher = crypt.Encrypt("text", "strong-password");
```

#### Asymmetric Encryption (RSA)
```csharp
var rsa = new AsymmetricCryptAlgorithm(AsymmetricCryptAlgorithm.ServiceProviderEnum.RSA);
CryptKeys keys = rsa.CreateKeys();

string encrypted = rsa.Encrypt("message", keys);
string decrypted = rsa.Decrypt(encrypted, keys);
```

#### Hash
```csharp
using var sha256 = new Solution.Security.HashAlgorithm(Solution.Security.HashAlgorithm.ServiceProviderEnum.SHA256);
string hash = sha256.Generate("data");
```

#### JWT
```csharp
// Secret key must be at least 32 characters
const string secretKey = "your-very-strong-secret-key-at-least-32-chars!";

// Create signed token with issuer and audience (recommended)
string token = JWT.CreateSecure(
    key: secretKey,
    expirationMinutes: 60,
    claims: new[] { new Claim("userId", "123"), new Claim("role", "admin") },
    issuer: "my-app",
    audience: "my-api"
);

// Validate signature, lifetime, algorithm, issuer and audience
Claim[] claims = JWT.ReadSecure(secretKey, token, validIssuer: "my-app", validAudience: "my-api");
```

#### 🛡️ Input Validation (`InputGuard`)
Use `Solution.Security.InputGuard` to validate dynamic SQL identifiers, SQL fragments, filesystem paths, and dynamic reflection targets:

```csharp
using Solution.Security;

// SQL identifiers & operators
string safeTable = InputGuard.ValidateSqlIdentifier("dbo.Users");
string safeOp = InputGuard.ValidateSqlOperator("LIKE");
string safeDir = InputGuard.ValidateSortDirection("DESC");

// Defensive validation for dynamic SQL clauses (blocks ';', '--', '/*', DROP, EXEC, etc.)
string safeWhere = InputGuard.ValidateSqlFragment("Active = 1 AND Role = 'User'");

// Stateless path traversal protection
string safeFile = InputGuard.ValidateSafePath(userFilePath, allowedBaseDirectory: @"C:\App\Uploads");

// Safe dynamic assembly & type validation
string safeDll = InputGuard.ValidateAssemblyPath(pluginPath, @"C:\App\Plugins");
Type safeType = InputGuard.ValidateAllowedType(loadedType, typeof(IMyPlugin));
```

### 🔄 SolutionMapper - Object Mapping

A flexible object mapper similar to AutoMapper.

```csharp
// Configuration
var mapper = new SolutionMapper();

mapper.CreateMap<UserDto, User>()
    .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
    .Ignore(dest => dest.Password);

// Mapping
User user = mapper.Map<User>(userDto);

// Collection mapping
List<User> users = mapper.Map<List<User>>(userDtos);

// Bidirectional mapping
mapper.CreateMap<User, UserDto>().ReverseMap();
```

#### Mapping Profiles
```csharp
public class UserProfile : SolutionMapperProfile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.Name))
            .ReverseMap();
    }
}

// Usage
var mapper = new SolutionMapper();
mapper.AddProfile<UserProfile>();
```

### 🪞 Reflection - Reflection Utilities

```csharp
var reflection = new ReflectionManager();

// Safe dynamic object creation (restricts assembly directory + required interface/base type)
IMyPlugin plugin = reflection.GetObjectSafe<IMyPlugin>(
    strAssemblyName: @"C:\App\Plugins\MyPlugin.dll",
    allowedDirectory: @"C:\App\Plugins",
    strClassName: "MyNamespace.MyPlugin");

// Dynamic method invocation
object result = reflection.CallMethod(plugin, "MethodName", param1, param2);

// Get/Set properties
reflection.CallPropertySet(plugin, "PropertyName", value);
object value = reflection.CallPropertyGet(plugin, "PropertyName");

// XML serialization (DTD/XXE prohibited by default)
byte[] xmlBytes = reflection.XMLSerializeToBytes(myObject);
var obj = (MyClass)reflection.XMLDeserializeFromBytes(xmlBytes, typeof(MyClass));
```

### 📚 Collections - Ordered Collections

```csharp
// Generic collection with insertion order
var collection = new GCollection<string, User>();
collection.Add("user1", new User { Name = "John" });
collection.Add("user2", new User { Name = "Jane" });

// Access by key or index
User user = collection["user1"];
User firstUser = collection[0];

// Ordered iteration
foreach (var key in collection.Keys) {
    Console.WriteLine(collection[key].Name);
}

// Events
collection.OnAdd += (key, value) => Console.WriteLine($"Added: {key}");
```

### 📋 JSON / XML

```csharp
// JSON
string json = JSON.Serialize(myObject);
MyClass obj = JSON.Deserialize<MyClass>(json);

// JSON navigation
var data = JSON.Parse(jsonString);
string value = JSON.GetValue(data, "path.to.property");

// XML
var xml = new XML("config.xml");
string value = xml.GetValue("//setting[@name='key']");
xml.SetValue("//setting[@name='key']", "newValue");
xml.Save();
```

### 🗄️ Cache

```csharp
// Generic cache with expiration
var cache = new Cache<User>();
cache.Add("user:1", new User { Name = "John" }, TimeSpan.FromMinutes(30));

// Retrieve with factory
User user = cache.GetOrAdd("user:1", () => LoadUserFromDb(1));

// Global Cache Manager
CacheManager.Instance.Set("key", value, expiration);
var cached = CacheManager.Instance.Get<User>("key");
```

### 📝 Logging

```csharp
// Simple logger
Log.Info("Information message");
Log.Error("Error", exception);
Log.Debug("Debug message");

// Diagnostic trace
Trace.WriteLine("Trace message", "Category");
```

### 🔧 Useful Extensions

```csharp
// String extensions
bool isNumber = "123".IsNumber();
var dict = "key1=value1&key2=value2".ToDictionary();
MyEnum value = "Value".ToEnum<MyEnum>();

// DataTable extensions
var list = dataTable.ToDynamic();
var dict = dataTable.ToDictionary();
List<User> users = dataTable.ToList<User>();

// Object extensions
string json = myObject.ToJson();
```

## 🛡️ Security Hardening & Best Practices

- **Parameterized Queries & Identifier Validation:** `CRUD`, `QueryBuilder`, and `DbOperations` automatically parameterize values and validate table/column identifiers, operators, and sort directions against SQL injection. For custom raw clauses, use `Solution.Security.InputGuard`.
- **Authenticated Encryption (AEAD):** Prefer `SymmetricCryptAlgorithm.EncryptAuthenticated` / `DecryptAuthenticated` (AES-256-GCM + PBKDF2-SHA256) and `DecryptAuto` for zero-downtime migration from legacy ciphertexts.
- **JWT Verification:** Signature verification is always enforced (`JWT.ReadSecure` is recommended to also validate `issuer`, `audience`, key length, and allowed HMAC algorithms).
- **Safe I/O & Deserialization:** `ZIP.UnZip` enforces actual decompressed byte limits (Zip Bomb protection) and path traversal checks; `FileManager.SetAllowedBasePath(...)` and `InputGuard.ValidateSafePath(...)` prevent directory escapes; `XML` and `ReflectionManager` prohibit DTD/XXE processing; `JSON` enforces a safe `MaxDepth` limit; `XLS` neutralizes CSV formula injection.
- **Transport Security:** Enable `Email.EnableSsl = true`, `FTP(..., enableSsl: true)`, `SFTP.WithHostKeyFingerprint(...)`, and `ForSqlServer(..., trustServerCertificate: false)` in production environments.
- **Resource Disposal:** Dispose `DB`, `DataReader`, `SFTP`, `SymmetricCryptAlgorithm`, `HashAlgorithm`, and `Cache<T>` instances using `using` blocks.

## 📋 Requirements

- .NET 10.0 or higher

## 📄 License

This project is licensed under the [MIT](LICENSE) License.

## 👤 Author

**Gaetano Acunzo**

- GitHub: [@acugae](https://github.com/acugae)
- NuGet: [Solution](https://www.nuget.org/packages/Solution)

## 🤝 Contributing

Contributions are welcome! Feel free to open issues or pull requests.

1. Fork the project
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request
