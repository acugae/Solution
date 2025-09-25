# Solution - .NET Core Utility Library

[![NuGet](https://img.shields.io/nuget/v/Solution.svg)](https://www.nuget.org/packages/Solution)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)

**Solution** is a comprehensive .NET 9.0 utility library designed to accelerate application development by providing a rich set of tools and abstractions for common development tasks. From database operations to file management, security features to object mapping, Solution offers battle-tested solutions for enterprise-grade applications.

## 📋 Table of Contents

- [Features](#-features)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Core Modules](#-core-modules)
  - [Database Operations](#database-operations)
  - [File Management](#file-management)
  - [Security & JWT](#security--jwt)
  - [Object Mapping](#object-mapping)
  - [IO Operations](#io-operations)
- [Advanced Usage](#-advanced-usage)
- [Best Practices](#-best-practices)
- [Contributing](#-contributing)
- [License](#-license)

## 🚀 Features

### Core Data Access
- **Multi-Database Support**: SQL Server, MySQL, PostgreSQL
- **Advanced CRUD Operations**: Simplified database operations with fluent API
- **Bulk Operations**: High-performance bulk insert/update operations
- **Dynamic Table Creation**: Create database tables from .NET types
- **Connection Management**: Intelligent connection pooling and management

### File & Document Processing
- **File Operations**: Read/write text and binary files with encoding support
- **Excel Processing**: Create and manipulate Excel files (.xls/.xlsx) using NPOI
- **PDF Generation**: Create and manipulate PDF documents with iTextSharp
- **Archive Management**: ZIP file creation and extraction
- **FTP/SFTP**: Secure file transfer protocols support

### Security & Authentication
- **JWT Token Management**: Create and validate JSON Web Tokens
- **Encryption**: Symmetric and asymmetric cryptography utilities
- **Secure Communication**: HTTPS, SSL/TLS support

### Development Utilities
- **Object Mapping**: High-performance object-to-object mapping
- **Reflection Utilities**: Advanced reflection helpers and caching
- **Extension Methods**: Rich set of extension methods for common types
- **Logging**: Integrated logging with log4net
- **Caching**: Memory and distributed caching abstractions

## 📦 Installation

Install the Solution package via NuGet Package Manager:

### Package Manager Console
```powershell
Install-Package Solution
```

### .NET CLI
```bash
dotnet add package Solution
```

### PackageReference
```xml
<PackageReference Include="Solution" Version="1.0.16" />
```

## 🏃‍♂️ Quick Start

### Basic Database Operations

```csharp
using Solution.Data;

// Initialize database connection
var db = new DB();

// Simple SELECT operation
var users = db["UserDatabase"].Select("SELECT * FROM Users WHERE Active = 1");

// Insert operation with parameters
var newUser = new CRUDInsert("Users");
newUser["Name"] = "John Doe";
newUser["Email"] = "john@example.com";
newUser["CreatedAt"] = DateTime.Now;

db["UserDatabase"].Insert(newUser);
```

### File Management

```csharp
using Solution.IO;

// Read/Write text files
string content = FileManager.GetFile(@"C:\data\config.txt");
FileManager.SetFile(@"C:\data\output.txt", "Hello World!");

// Read/Write binary files
byte[] binaryData = FileManager.GetFileByte(@"C:\data\image.jpg");
FileManager.SetFileByte(@"C:\data\copy.jpg", binaryData);

// Working with Excel files
var excel = new XLS();
excel.CreateWorkbook(@"C:\reports\data.xlsx");
excel.AddSheet("Users");
excel.SetCell(0, 0, "Name");
excel.SetCell(0, 1, "Email");
excel.SaveWorkbook();
```

### JWT Token Management

```csharp
using Solution.Security;

// Create a JWT token
var claims = new Claim[]
{
    new Claim("userId", "12345"),
    new Claim("username", "john_doe"),
    new Claim("role", "admin")
};

string token = JWT.Create("your-secret-key", 60, claims); // 60 minutes expiry

// Validate and read JWT token
try
{
    var tokenClaims = JWT.Read("your-secret-key", token);
    var userId = tokenClaims.FirstOrDefault(c => c.Type == "userId")?.Value;
}
catch (SecurityTokenExpiredException)
{
    // Handle expired token
}
```

## 🧩 Core Modules

### Database Operations

The `Solution.Data` namespace provides comprehensive database functionality:

#### Connection Management

```csharp
var db = new DB();

// Multiple connection support
db.DataManager.Connections.Add("primary", "connection-string-1");
db.DataManager.Connections.Add("readonly", "connection-string-2");

// Execute queries on specific connections
var data = db["primary"].Select("SELECT * FROM Products");
```

#### CRUD Operations

```csharp
// CREATE
var newProduct = new CRUDInsert("Products");
newProduct["Name"] = "Laptop";
newProduct["Price"] = 999.99;
newProduct["CategoryId"] = 1;
db[connectionKey].Insert(newProduct);

// READ with filters
var filters = new CRUDFilters();
filters.Add("CategoryId", 1);
filters.Add("Price", ">", 500);
var products = db[connectionKey].Select("Products", filters);

// UPDATE
var updateProduct = new CRUDUpdate("Products");
updateProduct["Price"] = 899.99;
updateProduct.Filters.Add("Id", productId);
db[connectionKey].Update(updateProduct);

// DELETE
var deleteProduct = new CRUDDelete("Products");
deleteProduct.Filters.Add("Id", productId);
db[connectionKey].Delete(deleteProduct);
```

#### Advanced Database Operations

```csharp
using Solution.DbOperations;

// Create table from .NET type
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    public DateTime CreatedAt { get; set; }
}

// Create table
connection.CreateTable<User>(options =>
{
    options.TableName("Users");
    options.DropIfExists(true);
});

// Bulk insert
var users = new List<User>
{
    new User { Name = "John", CreatedAt = DateTime.Now },
    new User { Name = "Jane", CreatedAt = DateTime.Now }
};

connection.BulkInsert<User>(users, options =>
{
    options.TableName("Users");
    options.BatchSize(1000);
});
```

### File Management

The `Solution.IO` namespace handles various file operations:

#### Excel Operations

```csharp
using Solution.IO;

var excel = new XLS();

// Create new workbook
excel.CreateWorkbook(@"C:\Reports\sales_report.xlsx");

// Add multiple sheets
excel.AddSheet("Q1 Sales");
excel.AddSheet("Q2 Sales");

// Set headers
excel.SetCell(0, 0, "Product");
excel.SetCell(0, 1, "Sales");
excel.SetCell(0, 2, "Revenue");

// Add data rows
var salesData = GetSalesData();
for (int i = 0; i < salesData.Count; i++)
{
    excel.SetCell(i + 1, 0, salesData[i].Product);
    excel.SetCell(i + 1, 1, salesData[i].Units);
    excel.SetCell(i + 1, 2, salesData[i].Revenue);
}

// Apply formatting
excel.SetCellStyle(0, 0, 0, 2, bold: true); // Header row
excel.AutoSizeColumns();

excel.SaveWorkbook();
```

#### PDF Operations

```csharp
using Solution.IO;

var pdf = new PDF();

// Create new PDF document
pdf.CreateDocument(@"C:\Documents\report.pdf");

// Add content
pdf.AddParagraph("Sales Report", fontSize: 18, bold: true);
pdf.AddParagraph("Generated on: " + DateTime.Now.ToString("yyyy-MM-dd"));

// Add table
var tableData = new string[,]
{
    {"Product", "Quantity", "Price"},
    {"Laptop", "10", "$999.99"},
    {"Mouse", "25", "$29.99"}
};

pdf.AddTable(tableData);

// Save document
pdf.SaveDocument();
```

#### FTP/SFTP Operations

```csharp
using Solution.IO;

// FTP Operations
var ftp = new FTP("ftp.example.com", "username", "password");
ftp.Connect();

// Upload file
ftp.UploadFile(@"C:\local\file.txt", "/remote/path/file.txt");

// Download file
ftp.DownloadFile("/remote/path/data.xml", @"C:\local\data.xml");

// List directory contents
var files = ftp.ListDirectory("/remote/path");

ftp.Disconnect();

// SFTP Operations (more secure)
var sftp = new SFTP("sftp.example.com", "username", "password", 22);
sftp.Connect();

sftp.UploadFile(@"C:\local\secure.dat", "/remote/secure/secure.dat");
var fileExists = sftp.FileExists("/remote/secure/secure.dat");

sftp.Disconnect();
```

### Security & JWT

#### JWT Token Management

```csharp
using Solution.Security;

public class AuthService
{
    private const string JWT_SECRET = "your-super-secret-key-here";

    public string GenerateToken(int userId, string username, string[] roles)
    {
        var claims = new List<Claim>
        {
            new Claim("userId", userId.ToString()),
            new Claim("username", username),
            new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim("role", role));
        }

        return JWT.Create(JWT_SECRET, 120, claims.ToArray()); // 2 hours expiry
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        try
        {
            var claims = JWT.Read(JWT_SECRET, token);
            var identity = new ClaimsIdentity(claims, "jwt");
            return new ClaimsPrincipal(identity);
        }
        catch (SecurityTokenExpiredException)
        {
            throw new UnauthorizedAccessException("Token has expired");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            throw new UnauthorizedAccessException("Invalid token signature");
        }
    }
}
```

#### Encryption Utilities

```csharp
using Solution.Security;

// Encrypt/Decrypt sensitive data
var crypt = new Crypt();

// Encrypt data
string sensitiveData = "Credit Card: 1234-5678-9012-3456";
string encrypted = crypt.Encrypt(sensitiveData, "encryption-key");

// Decrypt data
string decrypted = crypt.Decrypt(encrypted, "encryption-key");

// Hash passwords
string password = "user-password";
string hashedPassword = crypt.HashPassword(password);
bool isValid = crypt.VerifyPassword(password, hashedPassword);
```

### Object Mapping

The `SolutionMapper` provides powerful object-to-object mapping:

```csharp
using Solution.SolutionMapper;

// Define source and destination classes
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public DateTime RegisterDate { get; set; }
}

public class UserViewModel
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public string EmailAddress { get; set; }
    public string RegistrationDate { get; set; }
}

// Create mapping profile
public class UserMappingProfile : SolutionMapperProfile
{
    public UserMappingProfile()
    {
        CreateMap<UserDto, UserViewModel>()
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.EmailAddress, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.RegistrationDate, opt => opt.MapFrom(src => src.RegisterDate.ToString("yyyy-MM-dd")));
    }
}

// Configure and use mapper
var mapper = new SolutionMapper();
mapper.AddProfile(new UserMappingProfile());

var userDto = new UserDto
{
    Id = 1,
    FullName = "John Doe",
    Email = "john@example.com",
    RegisterDate = DateTime.Now.AddDays(-30)
};

var userViewModel = mapper.Map<UserDto, UserViewModel>(userDto);
```

### IO Operations

#### ZIP Archive Management

```csharp
using Solution.IO;

var zip = new ZIP();

// Create ZIP archive
zip.CreateArchive(@"C:\Archives\backup.zip");

// Add files to archive
zip.AddFile(@"C:\Data\file1.txt", "data/file1.txt");
zip.AddFile(@"C:\Data\file2.pdf", "documents/file2.pdf");

// Add entire directory
zip.AddDirectory(@"C:\Images", "images/");

zip.CloseArchive();

// Extract ZIP archive
zip.ExtractArchive(@"C:\Archives\backup.zip", @"C:\Extracted");

// List archive contents
var files = zip.ListArchiveContents(@"C:\Archives\backup.zip");
foreach (var file in files)
{
    Console.WriteLine($"File: {file.Name}, Size: {file.Size} bytes");
}
```

## 🔧 Advanced Usage

### Custom Database Providers

```csharp
using Solution.Data.Provider;

// Create custom provider for specific database
public class CustomDatabaseProvider : Provider
{
    public override IDbConnection CreateConnection(string connectionString)
    {
        // Return custom connection implementation
        return new CustomDbConnection(connectionString);
    }

    public override IDbCommand CreateCommand()
    {
        return new CustomDbCommand();
    }

    // Override other methods as needed...
}

// Register custom provider
var dataManager = new DataManager();
dataManager.RegisterProvider("custom", new CustomDatabaseProvider());
```

### Advanced Reflection Utilities

```csharp
using Solution.Reflection;

var reflectionManager = new ReflectionManager();

// Get type information with caching
var typeInfo = reflectionManager.GetTypeInfo<User>();
var properties = typeInfo.Properties;
var methods = typeInfo.Methods;

// Dynamic property access
var user = new User();
reflectionManager.SetPropertyValue(user, "Name", "John Doe");
var name = reflectionManager.GetPropertyValue<string>(user, "Name");

// Method invocation
var result = reflectionManager.InvokeMethod(user, "GetFullName", "Mr.");
```

### Extension Methods Usage

```csharp
using Solution;

// DataTable extensions
DataTable dataTable = GetDataFromDatabase();

// Convert to dynamic objects
var dynamicList = dataTable.ToDynamic();
foreach (dynamic item in dynamicList)
{
    Console.WriteLine($"{item.Name}: {item.Value}");
}

// Convert to key-value pairs
var keyValueList = dataTable.ToKeyValue();
foreach (var item in keyValueList)
{
    Console.WriteLine($"Row: {string.Join(", ", item.Select(kv => $"{kv.Key}={kv.Value}"))}");
}

// HttpRequest extensions (in ASP.NET Core)
public async Task<IActionResult> ProcessRequest()
{
    string body = await Request.GetBody();
    var data = JsonConvert.DeserializeObject(body);

    // Process data...

    return Ok();
}
```

## 🎯 Best Practices

### Database Operations
- Always use parameterized queries to prevent SQL injection
- Implement proper connection management and disposal
- Use bulk operations for large data sets
- Consider transaction boundaries for data consistency

```csharp
// Good practice
using (var transaction = db.DataManager.BeginTransaction("primary"))
{
    try
    {
        db["primary"].Insert(newOrder);
        db["primary"].Update(inventory);

        transaction.Commit();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

### Security
- Never hardcode secrets in source code
- Use environment variables or secure configuration
- Implement proper token expiration and refresh logic
- Always validate JWT tokens on protected endpoints

```csharp
// Configuration-based secrets
public class JwtSettings
{
    public string SecretKey { get; set; }
    public int ExpirationMinutes { get; set; }
}

// In Startup.cs or Program.cs
services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
```

### Performance
- Use bulk operations for large data sets
- Implement appropriate caching strategies
- Consider async/await patterns for I/O operations
- Profile and monitor application performance

## 🤝 Contributing

We welcome contributions to the Solution library! Please follow these guidelines:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** your changes (`git commit -m 'Add some amazing feature'`)
4. **Push** to the branch (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

### Development Setup

```bash
git clone https://github.com/acugae/Solution.git
cd Solution
dotnet restore
dotnet build
dotnet test
```

### Coding Standards
- Follow C# naming conventions
- Add XML documentation for public APIs
- Include unit tests for new features
- Ensure code passes all existing tests

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](https://choosealicense.com/licenses/mit/) page for details.

## 📞 Support & Contact

- **NuGet Package**: [https://www.nuget.org/packages/Solution](https://www.nuget.org/packages/Solution)
- **GitHub Repository**: [https://github.com/acugae/Solution](https://github.com/acugae/Solution)
- **Issues**: Report bugs and request features on GitHub Issues
- **Author**: Gaetano Acunzo

---

**Solution** - Accelerating .NET development with enterprise-grade utilities. 🚀