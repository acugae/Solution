# Solution

[![NuGet](https://img.shields.io/nuget/v/Solution.svg)](https://www.nuget.org/packages/Solution)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)

A comprehensive .NET library providing utilities for database operations, I/O, security, reflection, object mapping, and much more.

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

Multi-database support (SQL Server, MySQL, PostgreSQL) with simplified CRUD operations and fluent API.

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

// Shorthand methods
var db = new DB()
    .AddSqlServer("main", "Server=localhost;Database=mydb;...")
    .AddMySQL("secondary", "Server=localhost;Database=other;...");

// Available providers: UseSqlServer(), UseMySQL(), UsePostgreSQL()
// Connection helpers: ForSqlServer(), ForMySQL(), ForPostgreSQL()
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
// FTP
var ftp = new FTP("ftp://server.com", "user", "password");
ftp.Upload("local.txt", "/remote/path/file.txt");
ftp.Download("/remote/file.txt", "local.txt");

// SFTP (with key or password)
var sftp = new SFTP("server.com", "user", "password");
sftp.Upload("local.txt", "/remote/path/");
sftp.Download("/remote/file.txt", memoryStream);
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
var email = new Email("smtp.server.com");

// Simple email
email.SendMail(
    from: "sender@email.com",
    to: "recipient@email.com",
    subject: "Subject",
    body: "<h1>HTML Content</h1>",
    isHtml: true
);

// With attachments
email.SendMailAttach(
    from: "sender@email.com",
    to: "dest1@email.com;dest2@email.com",
    cc: "cc@email.com",
    subject: "Documents",
    body: "Please find the documents attached",
    attachments: new[] { "doc1.pdf", "doc2.xlsx" }
);
```

### 🔐 Security - Cryptography

#### Symmetric Encryption
```csharp
var aes = new SymmetricCryptAlgorithm(SymmetricAlgorithmType.AES, "secret-key");

string encrypted = aes.Encrypt("text to encrypt");
string decrypted = aes.Decrypt(encrypted);
```

#### Asymmetric Encryption
```csharp
var rsa = new AsymmetricCryptAlgorithm(AsymmetricAlgorithmType.RSA);
CryptKeys keys = rsa.CreateKeys();

string encrypted = rsa.Encrypt("message", keys.PublicKey);
string decrypted = rsa.Decrypt(encrypted, keys.PrivateKey);
```

#### Hash
```csharp
string hash = HashAlgorithm.Generate("password", HashAlgorithmType.SHA256);
string md5 = HashAlgorithm.Generate("data", HashAlgorithmType.MD5);
```

#### JWT
```csharp
// Create token
string token = JWT.Create(
    claims: new Dictionary<string, object> { 
        { "userId", 123 }, 
        { "role", "admin" } 
    },
    secretKey: "your-secret-key",
    expirationMinutes: 60
);

// Validate and read
var claims = JWT.Read(token, "your-secret-key");
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
// Dynamic object creation
object instance = ReflectionManager.GetObject("MyNamespace.MyClass", assembly);

// Dynamic method invocation
object result = ReflectionManager.CallMethod(instance, "MethodName", param1, param2);

// Get/Set properties
ReflectionManager.CallPropertySet(instance, "PropertyName", value);
object value = ReflectionManager.CallPropertyGet(instance, "PropertyName");

// Copy properties between objects
ReflectionManager.CopyPropertiesObject(source, destination);

// XML serialization
string xml = ReflectionManager.XMLSerialize(myObject);
MyClass obj = ReflectionManager.XMLDeserialize<MyClass>(xml);
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

### 💾 Persistence - Data Persistence

#### Commander - SQL Query Management
```csharp
var commander = new Commander("queries.xml");

// Execute predefined query
DataTable result = commander.Execute("GetUserById", new { Id = 1 });
```

#### Mapper - Simplified ORM
```csharp
var mapper = new Mapper("mapping.xml");

// Retrieve objects
List<User> users = mapper.Get<User>(new { Active = true });
User user = mapper.GetFirst<User>(new { Id = 1 });

// Persistence
mapper.Set(user);
mapper.Del(user);
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
