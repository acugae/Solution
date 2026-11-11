# Changelog

## 2.0.0

Versione con modifiche **non retrocompatibili**: i default diventano sicuri. Per ogni punto è indicato cosa fare nei progetti che aggiornano.

### Database

| Cosa cambia | Cosa fare |
|---|---|
| `DB.ModeConnection` vale `Whenever` di default: ogni operazione usa una connessione propria, aperta e rilasciata. | Niente, nella maggior parte dei casi. Chi conta sulla connessione condivisa (tabelle temporanee `#tmp`, `SET` di sessione) imposta `db.ModeConnection = DB.enModeConnectionOpen.Always`. |
| Le transazioni (`BeginTransaction`, `InTransaction`, `DataManager.Transactions`) usano la connessione condivisa della chiave e le operazioni eseguite mentre la transazione è aperta vi partecipano, in entrambe le modalità. `Transactions.Begin` prima falliva con `InvalidCastException` lasciando una transazione aperta. | Niente. Una transazione va usata da un solo flusso alla volta per chiave di connessione. |
| Nomi di tabella e colonna (CRUD, `QueryBuilder`, bulk) sono validati e racchiusi tra `[ ]`. Non sono ammessi `[ ] ' " ; ` e il backtick. | Non passare espressioni come nome (es. `YEAR(Data)`, `t.Campo AS x`): usare `db.Query(...)` con SQL e parametri. In `Fields` restano ammessi `*` e `COUNT(*)`. |
| Operatori ammessi nei filtri: `= <> != < > <= >= LIKE, NOT LIKE, IN, NOT IN, IS, IS NOT, BETWEEN`. Ordinamento: solo `ASC`/`DESC`. | Sostituire eventuali operatori diversi con una query SQL parametrizzata. |
| `IN` / `NOT IN`: i valori sono sempre parametri. Il testo `"1,2,3"` o `"'a','b'"` viene scomposto nei singoli valori. | Niente. Non è più possibile passare una sotto-query come testo dell'operatore `in`. |
| Filtro con valore `null`: eccezione (`ArgumentNullException`) anche in `Update` e `Delete`, dove prima generava `= NULL` senza toccare righe. | Per cercare i NULL usare l'operatore `IS` (o `IS NOT`). |
| `FluentExtensions.ForSqlServer(...)` non aggiunge più `TrustServerCertificate=True`. | Installare un certificato valido sul server, oppure passare `trustServerCertificate: true`. Le stringhe di connessione scritte a mano non cambiano. |
| Bulk insert: `KeepNulls` e `KeepIdentity` sono ora realmente attivi (prima l'espressione valeva 0). | Verificare i bulk insert su tabelle con colonne identity o con valori di default: vengono mantenuti i valori della sorgente. |
| Tabelle temporanee dei bulk con nome univoco (prima uguale per due chiamate nello stesso secondo). | Niente. |
| `Commander.Execute(...)` con sostituzione testuale dei segnaposto è `[Obsolete]`. | Usare `Commander.ExecuteSafe(chiave, nomeQuery, parametri)`, che trasforma `#nome#` in parametri. |
| `Persistence.Table` e `ComunicationDB`: valori testuali con apice raddoppiato, valori numerici verificati, nomi verificati. | Niente. |
| Provider esterno (`Provider` con file DLL): ora la factory viene caricata davvero. | Niente. |
| `DB` e `ComunicationDB` non scrivono più sulla console: i messaggi vanno su `System.Diagnostics.Trace`. | Aggiungere un `TraceListener` se quei messaggi servono. |

### Sicurezza

| Cosa cambia | Cosa fare |
|---|---|
| Nuovi `SymmetricCryptAlgorithm.EncryptAuthenticated` / `DecryptAuthenticated` (AES-256-GCM), con password oppure con chiave di 32 byte (`GenerateKey`). | Usarli per i nuovi dati. Con la password la derivazione è volutamente lenta (PBKDF2, 600.000 iterazioni): per molti valori usare la versione con chiave. |
| `Encrypt`/`Decrypt` e `EncryptSecure`/`DecryptSecure` sono `[Obsolete]` ma funzionano come prima. | I dati già cifrati restano leggibili con il metodo con cui sono stati scritti; migrare quando possibile. |
| `AsymmetricCryptAlgorithm()` usa RSA (prima DSA, che restituiva il testo **in chiaro**). Chiavi a 2048 bit, padding OAEP, testo in UTF-8. Con DSA `Encrypt`/`Decrypt` sollevano `NotSupportedException`. | Chi usava il costruttore senza argomenti ha dati non cifrati: vanno cifrati di nuovo. Per leggere dati RSA scritti con le versioni precedenti impostare `UseOaepPadding = false`. |
| Nuova classe `PasswordHasher` (`Hash` / `Verify`). `HashAlgorithm` con `MD5` e `SHA1` è `[Obsolete]`. | Per le password usare `PasswordHasher`. |
| `JWT.Read(key, token)` e `JWT.Create(...)` sono `[Obsolete]`; `Read` con `TokenValidationParameters` usa la chiave passata se i parametri non ne indicano una; sono ammessi solo HS256/HS384/HS512. | Usare `ReadSecure` / `CreateSecure`. |

### File, rete, posta

| Cosa cambia | Cosa fare |
|---|---|
| `SFTP`: la host key del server viene verificata. Senza impronta configurata `Connect()` fallisce e il messaggio riporta l'impronta ricevuta. | Impostare `HostKeyFingerprintSha256` prima di `Connect()` (oppure `AcceptAnyHostKey = true`, solo su reti fidate). `SFTP` è ora `IDisposable`; il costruttore con chiave privata usa la porta indicata. |
| `FTP.DownloadFile` non ignora più gli errori: solleva `WebException`. `ListFile` restituisce l'elenco. Nuova proprietà `EnableSsl` (FTPS). | Gestire l'eccezione dove prima l'errore passava in silenzio. |
| `Email`: nuove proprietà `Port`, `EnableSsl`, `Credentials`; messaggi e allegati vengono rilasciati. | Impostare `EnableSsl = true` e le credenziali se il server le richiede. |
| `ZIP.UnZip`: `..` è rifiutato solo come segmento del percorso. | Niente. |
| `Log` su file: scritture serializzate, cartella `log` creata se manca. | Niente. |

### Pacchetto

- `Microsoft.Data.Sqlite` passa dalla versione preview alla stabile 10.0.11.
- Aggiunto il workflow `.github/workflows/build.yml`: build, test, controllo delle dipendenze vulnerabili e pubblicazione sui tag `v*` con la chiave nei secret (`NUGET_API_KEY`).

### Non ancora fatto

- Divisione in più pacchetti NuGet.
- `IO/Provider/SMTP.cs` legge tutti i campi dalla colonna `ml_body`: servono i nomi corretti delle colonne.
- `FTP` e `Email` restano senza cifratura di default.

## 1.0.36

- `CRUD.Find` e `QueryBuilder` usano parametri al posto dei valori concatenati.
- `[Obsolete]` su `Encrypt`/`Decrypt` legacy e su `getFilter()`/`getFilters()`.
- Stack trace conservato nei rilanci; connessioni clonate rilasciate con `Dispose`.
- Credenziali quotate nelle stringhe di connessione; `FileManager` base path; correzioni a `ZIP`; generatore di password.
