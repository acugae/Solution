---
mode: agent
description: Esegue il build Release del progetto per tutti i TFM, analizza il log e riporta un riepilogo completo di warning ed errori per ciascuna versione target.
---

Esegui la verifica completa del build NuGet seguendo questi passi nell'ordine indicato:

1. Lancia il build Release completo del progetto `Solution.csproj` su tutti i TFM (`netstandard2.1`, `net5.0`, `net6.0`, `net7.0`, `net8.0`, `net9.0`, `net10.0`) salvando l'output nel file `build-log.txt`:
   ```powershell
   dotnet build Solution.csproj -c Release -v normal 2>&1 | Tee-Object -FilePath build-log.txt
   ```

2. Una volta terminato, leggi `build-log.txt` e produce un riepilogo strutturato con:
   - Esito complessivo (successo / fallimento)
   - Numero totale di warning e di errori
   - Elenco di ogni DLL prodotta, una per TFM
   - Dettaglio dei warning raggruppati per categoria:
	 | Categoria | Pattern |
	 |-----------|---------|
	 | Nullable reference | `CS86xx` |
	 | API obsolete | `CS0618`, `CS0619` |
	 | API deprecate .NET | `SYSLIBxxxx` |
	 | NuGet compat | `NU1701` |
	 | XML doc mancanti | `CS1591` |
   - Per ogni warning: codice, file, numero di riga e messaggio
   - Eventuali regressioni rispetto all'ultima build (se `build-log.txt` precedente è disponibile)

3. Se ci sono warning o errori, suggerisci le correzioni prioritarie evidenziando quelli che impattano più TFM.

4. Al termine riporta il tempo di build totale estratto dal log.
