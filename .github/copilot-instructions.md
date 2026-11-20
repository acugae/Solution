# Copilot Instructions for Solution

## Build & Test

```bash
dotnet build Solution.slnx
dotnet test Solution.Tests/Solution.Tests.csproj
```

The project targets **.NET 10.0** and generates a NuGet package on build (`GeneratePackageOnBuild`). Output goes to `bin\` (flat, no TFM subdirectory). Unit and security regression tests live in `Solution.Tests/` (`xUnit`).

## Architecture

**Solution** is a utility NuGet library (`Solution.dll`) organized into feature modules, all in a single project under the root namespace `Solution`:

| Module | Namespace | Purpose |
|---|---|---|
| `Data/` | `Solution.Data` | Multi-database access layer (SQL Server, MySQL, PostgreSQL, SQLite) with parameterized CRUD, fluent `QueryBuilder`, `SqlSecurityHelper`, transactions, and DI registration |
| `Data/Provider/` | `Solution.Data.Provider` | ADO.NET provider abstractions (`Connection`, `Command`, `Parameter`, `Transaction`, `DataAdapter`, `DataReader`) |
| `DbOperations/` | `Solution.Data` | Bulk insert/update operations via `GenericListDataReader` with identifier and SQL fragment validation |
| `Collections/` | `Solution.Collections` | `GCollection<K,V>` — insertion-order-preserving generic dictionary with events |
| `IO/` | `Solution.IO` | File I/O utilities: `FileManager`, `XLS` (NPOI, with CSV formula neutralization), `PDF` (iTextSharp), `FTP`, `SFTP` (SSH.NET with host-key fingerprint support), `ZIP` (bounded decompression), `PipeServer`/`PipeClient` |
| `Communication/` | `Solution.Communication` | `Email` — SMTP email sending with `EnableSsl` and header sanitization |
| `Security/` | `Solution.Security` | `JWT` (create/validate tokens), `SymmetricCryptAlgorithm` (including AES-256-GCM AEAD + `DecryptAuto`), `AsymmetricCryptAlgorithm` (RSA), `HashAlgorithm`, `InputGuard` |
| `SolutionMapper/` | `Solution.SolutionMapper` | Thread-safe object-to-object mapper with profiles, cycle/depth protection (`MaxDepth` & `InstanceCache`), and `IgnoreProps` enforcement |
| `Reflection/` | `Solution.Reflection` | `ReflectionManager` — dynamic object creation (`GetObjectSafe<TInterface>`), method invocation, property access, DTD-safe XML serialization |
| `Solution/` | `Solution` | Cross-cutting: extension methods (`Extentions.cs`), `Cache` (UTF-8 + SHA-256 keys, `IDisposable`), `Log` (thread-safe, CRLF-sanitized), `Trace`, `StringAdvance` (CSPRNG password generation) |

### Core data flow

The central class is `DB`. It manages named database connections via `DataManager` → `Connections` → `Connection` (wraps ADO.NET `DbConnection`). Access a connection by key: `db["main"]` returns a `CRUD` instance for that connection. The `CRUD` class builds parameterized SQL from `CRUDBase`/`CRUDUpdate`/`CRUDDelete`/`CRUDFind` objects. Extension methods in `CRUDExtensions.cs` provide the simplified anonymous-object API (`db["main"].Insert("Table", new { ... })`).

Database provider keys: `"sqldb"` = SQL Server, `"mysdb"` = MySQL, `"pstdb"` = PostgreSQL, `"litedb"` = SQLite.

### Connection Management

- For asynchronous code, **clone connections by default** to avoid concurrency issues caused by shared readers in a Blazor Server app. Each operation should open and close its own connection.
- Do not reuse shared open database connections. Use a connection for each operation, opening and closing it for each call.

### DI integration

`ServiceCollectionExtensions.cs` provides `AddSolutionDB()` extension methods for ASP.NET Core. Supports fluent configuration, `IConfiguration` binding from `appsettings.json`, and standard `ConnectionStrings` section.

## Conventions

- **Language mix**: XML doc comments and some log/exception messages are in Italian. Code identifiers are in English.
- **Global usings**: `_Imports.cs` declares global usings for System namespaces, internal namespaces (`Solution.Collections`, `Solution.Data`, etc.), and third-party libraries (Newtonsoft.Json, NPOI, iTextSharp, SSH.NET).
- **No interfaces on public types**: Most public classes (`DB`, `CRUD`, `GCollection`, `FileManager`, etc.) are concrete classes, not interface-backed. Extensions are used to add fluent APIs.
- **Extension method pattern**: New features are added as extension methods on existing types (e.g., `CRUDExtensions`, `DBExtensions`, `FluentExtensions`, `DBConnectionExtensions`, `DBTransactionExtensions`) rather than modifying core classes. This preserves backward compatibility.
- **Thread safety via `lock(this)`**: Collections and core classes use `lock(this)` for thread safety.
- **Connection mode**: `DB.ModeConnection` controls whether connections are persistent (`Always`) or cloned per-operation (`Whenever`).
- **Nullable enabled**: The project has `<Nullable>enable</Nullable>`.
- **Tests & scripts**: `Solution.Tests/` contains xUnit unit and security regression tests (`SecurityHardeningTests.cs`). `scripts/audit-consumers.ps1` scans consumer repositories for `Solution` usage and sensitive API calls.
- **Security conventions**: Always parameterize SQL (`CRUD`/`QueryBuilder`), validate dynamic identifiers/fragments/paths/types via `Solution.Security.InputGuard`, disable DTD processing on XML readers, and dispose `DB`/`DataReader`/`SFTP`/`SymmetricCryptAlgorithm`/`HashAlgorithm`/`Cache<T>`.
- **File-scoped namespaces**: Newer files use file-scoped namespace declarations (`namespace Solution.Data;`), while older files may use block-scoped or omit the namespace entirely (relying on `<RootNamespace>Solution</RootNamespace>`).
