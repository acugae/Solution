# Copilot Instructions for Solution

## Build

```bash
dotnet build Solution.sln
```

This is a single-project solution (no test project exists). The project targets **.NET 10.0** and generates a NuGet package on build (`GeneratePackageOnBuild`). Output goes to `bin\` (flat, no TFM subdirectory).

## Architecture

**Solution** is a utility NuGet library (`Solution.dll`) organized into feature modules, all in a single project under the root namespace `Solution`:

| Module | Namespace | Purpose |
|---|---|---|
| `Data/` | `Solution.Data` | Multi-database access layer (SQL Server, MySQL, PostgreSQL) with CRUD, fluent query builder, transactions, and DI registration |
| `Data/Provider/` | `Solution.Data.Provider` | ADO.NET provider abstractions (`Connection`, `Command`, `Parameter`, `Transaction`, `DataAdapter`, `DataReader`) |
| `DbOperations/` | `Solution.Data` | Bulk insert/update operations via `GenericListDataReader` |
| `Collections/` | `Solution.Collections` | `GCollection<K,V>` — insertion-order-preserving generic dictionary with events |
| `IO/` | `Solution.IO` | File I/O utilities: `FileManager`, `XLS` (NPOI), `PDF` (iTextSharp), `FTP`, `SFTP` (SSH.NET), `ZIP`, `PipeServer`/`PipeClient` |
| `Communication/` | `Solution.Communication` | `Email` — SMTP email sending |
| `Security/` | `Solution.Security` | `JWT` (create/validate tokens), `SymmetricCryptAlgorithm`, `AsymmetricCryptAlgorithm`, `HashAlgorithm` |
| `SolutionMapper/` | `Solution.SolutionMapper` | Object-to-object mapper with profiles, similar to AutoMapper |
| `Persistence/` | `Solution.Persistence` | `Commander` (XML-defined SQL queries), `Mapper` (simplified ORM) |
| `Reflection/` | `Solution.Reflection` | `ReflectionManager` — dynamic object creation, method invocation, property access, serialization |
| `Solution/` | `Solution` | Cross-cutting: extension methods (`Extentions.cs`), `Cache`, `Log` (log4net), `Trace`, string utilities |

### Core data flow

The central class is `DB`. It manages named database connections via `DataManager` → `Connections` → `Connection` (wraps ADO.NET `DbConnection`). Access a connection by key: `db["main"]` returns a `CRUD` instance for that connection. The `CRUD` class builds parameterized SQL from `CRUDBase`/`CRUDUpdate`/`CRUDDelete`/`CRUDFind` objects. Extension methods in `CRUDExtensions.cs` provide the simplified anonymous-object API (`db["main"].Insert("Table", new { ... })`).

Database provider keys: `"sqldb"` = SQL Server, `"mysdb"` = MySQL, `"pstdb"` = PostgreSQL.

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
- **No test project**: There is no test project in the solution. The `scripts/` and `test-files/` directories exist but are empty.
- **File-scoped namespaces**: Newer files use file-scoped namespace declarations (`namespace Solution.Data;`), while older files may use block-scoped or omit the namespace entirely (relying on `<RootNamespace>Solution</RootNamespace>`).
