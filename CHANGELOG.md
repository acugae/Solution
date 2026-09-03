# Changelog

Tutte le modifiche rilevanti al progetto sono documentate in questo file.  
Il formato segue [Keep a Changelog](https://keepachangelog.com/it/1.0/) e il versionamento segue [Semantic Versioning](https://semver.org/lang/it/).

---

## [1.0.31] – in corso
### Aggiunto
- Multi-targeting NuGet: supporto a `netstandard2.1`, `net5.0`, `net6.0`, `net7.0`, `net8.0`, `net9.0`, `net10.0`.
- Riferimenti condizionali per `JwtBearer`, `Npgsql`, `Microsoft.Data.SqlClient`, `System.DirectoryServices` per TFM.
- `Microsoft.Extensions.*` espliciti per `netstandard2.1` (non forniti da `FrameworkReference`).
- `AcceptNPOIOSMFLicense` per silenziare il warning licenza NPOI 2.8.0.
- `NuGetAuditMode=direct` + override `System.Text.Json 8.0.5` per CVE GHSA-hh2w-p6rv-4g7w.
### Modificato
- `SolutionMapper`: risolti tutti i warning nullable (CS8602/CS8603/CS8618) su `SolutionMappingExpression`, `SolutionMapper`, `ValueTransformerConfiguration`.
- `_Imports.cs`: `global using Microsoft.AspNetCore.Http` condizionato a `#if NET5_0_OR_GREATER`.
- `Reflection/ReflectionManager.cs`: `IsParameter` e using Blazor condizionati a `#if NET5_0_OR_GREATER`.
- `Security/JWT.cs`: intera classe condizionata a `#if NET5_0_OR_GREATER`.
- `Solution/Extentions.cs`: `HttpContextExtensions` condizionata a `#if NET5_0_OR_GREATER`.

---

## [1.0.30]
### Aggiunto
- `Microsoft.Data.Sqlite`: supporto SQLite tramite `Microsoft.Data.Sqlite` (provider `litedb`), disponibile su `net8+`.
- `Data/Provider/Provider.cs`: registrazione di `SqliteFactory.Instance` con guardia `#if NET8_0_OR_GREATER`.
### Modificato
- Aggiornamento pacchetti: `iTextSharp.LGPLv2.Core` → 3.8.1, `log4net` → 3.3.2, `MySqlConnector` → 2.6.1, `Microsoft.Data.SqlClient` → 7.0.2, `Npgsql` → 10.0.3.
- `Security/JWT.cs`: miglioramenti alla gestione JWT.

---

## [1.0.29] – 2026-07-09
### Modificato
- Aggiornamento versioni pacchetti (`Solution.csproj`).

---

## [1.0.28] – 2026-05-26
### Corretto
- Correzioni varie.

---

## [1.0.27] – 2026-05-26
### Modificato
- `Data/Provider/Provider.cs`: fix provider.
- Aggiornamento `README.md` con documentazione estesa.
- Aggiornamento versioni pacchetti.

---

## [1.0.26] – 2026-05
### Modificato
- `Data/Provider/Provider.cs`: fix provider.
- Aggiornamento versioni pacchetti (`Solution.csproj`).

---

## [1.0.25] – 2026-05
### Modificato
- `Data/DB.cs`: correzioni alla gestione delle connessioni.
- Aggiornamento versioni pacchetti (`Solution.csproj`).

---

## [1.0.24] – 2026-05
### Modificato
- `Data/DB.cs`, `Data/Provider/Connection.cs`: refactoring connessioni asincrone (branch `newConnectionAsync`).
- `Data/DataManager.cs`: miglioramenti gestione data manager.
- `Solution/Extentions.cs`: nuove estensioni.
- `README.md`: aggiornamento documentazione.
- Aggiornamento versioni pacchetti (`Solution.csproj`).

---

## [1.0.23]
### Aggiunto
- Connessioni asincrone (`newConnectionAsync`): gestione asincrona delle connessioni al database senza riuso di connessioni condivise.
- `SolutionMapper`: mapping di collezioni figlio (`List` / `IEnumerable`) tra tipi sorgente e destinazione.

---

## [1.0.22] – 2024
### Aggiunto
- Nuove funzionalità di connessione e refactoring della gestione delle connessioni al database.
### Corretto
- Varie correzioni di stabilità.

---

## [1.0.21] – 2024
### Modificato
- Sostituzione del driver `MySql.Data` con `MySqlConnector` per migliori prestazioni e licenza più permissiva.
### Corretto
- Varie correzioni di stabilità.

---

## [1.0.20] – 2024
### Aggiunto
- Revisione generale del codice (`review` branch).
- Aggiornamento `README.md`.
### Corretto
- Varie correzioni di stabilità.

---

## [1.0.19] – 2024
### Corretto
- Correzioni varie.

---

## [1.0.18] – 2024
### Corretto
- Fix setter nelle proprietà gestite da reflection.
- Aggiornamento `AssemblyVersion`.
- Aggiunto supporto `IsNullable` per parametri del costruttore.

---

## [1.0.17] – 2023
### Aggiunto
- `README.md` iniziale nel pacchetto NuGet.

---

## [1.0.16] – 2023
### Aggiunto
- Versione 1.0.16: aggiornamenti generali al pacchetto.

---

## [1.0.15] – 2023
### Aggiunto
- `SolutionMapper`: mapper oggetto-oggetto con profili, simile ad AutoMapper.
- `Add Configure Default` e `Other Default Config` per la configurazione del mapper.
### Corretto
- Varie correzioni e note al codice.

---

## [1.0.14] – 2023
### Aggiunto
- `DbOperations`: supporto a operazioni bulk (insert/update) tramite `GenericListDataReader`.
- Aggiornamento pacchetti dipendenze.

---

## [1.0.13] – 2023
### Aggiunto
- Nuova versione con estensioni e nuove funzionalità generali.
- `ReadFromStreamXLSXAsync`: lettura asincrona di stream XLSX tramite NPOI.

---

## [1.0.12] – 2023
### Corretto
- Correzioni varie.

---

## [1.0.11] – 2023
### Corretto
- Correzioni varie (`1.0.11.b`).

---

## [1.0.10] – 2023
### Aggiunto
- Supporto `DB Async`: metodi asincroni su `DB` / `CRUD`.
- Gestione `timeout` sulle connessioni.
### Corretto
- `clean code` e refactoring interno.

---

## [1.0.9] – 2022
### Aggiunto
- Aggiornamento a **.NET 9** (aggiornamento dipendenze).
- `OrderedDictionary` aggiunto a `Collections`.
### Corretto
- Varie correzioni.

---

## [1.0.8] – 2022
### Modificato
- Refactoring della classe `DB`.
- Aggiunta estensione `IsNumber` sul tipo `string`.

---

## [1.0.7] – 2022
### Aggiunto
- Gestione JWT Token (`Security/JWT.cs`): creazione e validazione token.
- Classe `Configurazione` per la gestione delle organizzazioni.
### Corretto
- Varie correzioni.

---

## [1.0.6] – 2022
### Aggiunto
- Nuove classi per connessioni, assembly e ispezione dei tipi.
- Estensione `DataRow.ToKeyValue()`.
### Modificato
- Aggiornamento dipendenze.

---

## [1.0.5] – 2022
### Corretto
- Varie correzioni.

---

## [1.0.4] – 2022
### Aggiunto
- Versione 1.0.4: consolidamento funzionalità.
- Cambio namespace da `core_Tasks` a `syint_Tasks`.
### Modificato
- Rinominazione classi in PascalCase.
- Nuova `ViewConfig`.

---

## [1.0.3] – 2022
### Aggiunto
- Aggiunta di `JWT` (`cJWT`), adeguamento namespace.
### Corretto
- Correzioni varie.

---

## [1.0.1] – 2022
### Aggiunto
- Prima release pubblica (`First Release`).
- Struttura iniziale del progetto: `Data`, `IO`, `Security`, `Collections`, `Reflection`, `Communication`.
