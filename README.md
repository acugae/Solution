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

## 🚀 Features

### 📊 Data - Database Access

Multi-database support (SQL Server, MySQL, PostgreSQL) with simplified CRUD operations.

```csharp
// Connection setup
var db = new DB();
db.Connections.Add("main", new Connection("Server=localhost;Database=mydb;..."));

// Simple query
DataTable result = db["main"].Get("SELECT * FROM Users WHERE Id = @Id", new Parameters().Add("Id", 1));

// CRUD operations
db["main"].CRUD.Insert("Users", new { Name = "John", Email = "john@email.com" });
db["main"].CRUD.Update("Users", new { Name = "John Doe" }, new { Id = 1 });
db["main"].CRUD.Delete("Users", new { Id = 1 });

// Transactions
db.Transactions.Begin("main");
try {
    db["main"].Execute("INSERT INTO ...");
    db.Transactions.Commit("main");
} catch {
    db.Transactions.Rollback("main");
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
