namespace Solution.IO;
public class ZIP
{
    /// <summary>
    /// Limite massimo di dimensione per l'estrazione (default: 100MB).
    /// Previene attacchi Zip Bomb.
    /// </summary>
    public static long MaxExtractSize { get; set; } = 100 * 1024 * 1024; // 100MB default
    
    /// <summary>
    /// Numero massimo di file da estrarre (default: 10000).
    /// </summary>
    public static int MaxFileCount { get; set; } = 10000;
    
    public byte[] CompressObject(object obj, bool leaveOpen = false)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            using (GZipStream zs = new GZipStream(ms, CompressionMode.Compress, leaveOpen))
            {
                byte[] data = Binary.ObjectToByteArray(obj) ?? Array.Empty<byte>();
                zs.Write(data, 0, data.Length);
            }
            // ToArray dopo la chiusura del GZipStream, altrimenti il contenuto non è completo.
            return ms.ToArray();
        }
    }

    public object DecompressObject(byte[] data, bool leaveOpen = false)
    {
        // Dati non compressi (nessuna intestazione gzip): comportamento precedente.
        if (data == null || data.Length < 2 || data[0] != 0x1f || data[1] != 0x8b)
            return Binary.ByteArrayToObject<object>(data);

        using (MemoryStream ms = new MemoryStream(data))
        {
            using (GZipStream zs = new GZipStream(ms, CompressionMode.Decompress, leaveOpen))
            using (MemoryStream output = new MemoryStream())
            {
                zs.CopyTo(output);
                return Binary.ByteArrayToObject<object>(output.ToArray());
            }
        }
    }

    public byte[] Zip(Dictionary<string, byte[]> files)
    {
        var outputMemStream = new MemoryStream();

        var zipStream = new ZipOutputStream(outputMemStream);

        zipStream.SetLevel(9);
        foreach (var file in files)
        {
            zipStream.PutNextEntry(new ZipEntry(file.Key)
            {
                Size = file.Value.Length
            });
            zipStream.Write(file.Value, 0, file.Value.Length);
            zipStream.Flush();
        }
        zipStream.Finish();
        outputMemStream.Position = 0;
        return outputMemStream.ToArray();
    }

    public Dictionary<string, byte[]> UnZip(byte[] ZipFile)
    {
        Dictionary<string, byte[]> oResult = new Dictionary<string, byte[]>();
        long totalExtracted = 0;
        int fileCount = 0;
        
        using (ZipInputStream zipStream = new ZipInputStream(new MemoryStream(ZipFile)))
        {
            ZipEntry currentEntry;
            while ((currentEntry = zipStream.GetNextEntry()) != null)
            {
                // Protezione Zip Bomb: verifica numero file
                fileCount++;
                if (fileCount > MaxFileCount)
                    throw new InvalidOperationException($"Zip bomb protection: exceeded maximum file count ({MaxFileCount})");
                
                // Protezione Zip Bomb: verifica dimensione totale
                if (currentEntry.Size > 0 && totalExtracted + currentEntry.Size > MaxExtractSize)
                    throw new InvalidOperationException($"Zip bomb protection: extracted content exceeds size limit ({MaxExtractSize / (1024 * 1024)}MB)");
                
                // Protezione Path Traversal: verifica nome file
                string entryName = currentEntry.Name;
                if (entryName.Contains("..") || Path.IsPathRooted(entryName))
                    throw new InvalidOperationException($"Zip entry contains invalid path: {entryName}");
                
                // Legge l'intero contenuto (una sola Read può restituire meno byte del richiesto)
                // e conta i byte realmente estratti, senza fidarsi della dimensione dichiarata.
                byte[] data;
                using (MemoryStream entryStream = new MemoryStream())
                {
                    byte[] buffer = new byte[81920];
                    int read;
                    while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        totalExtracted += read;
                        if (totalExtracted > MaxExtractSize)
                            throw new InvalidOperationException($"Zip bomb protection: extracted content exceeds size limit ({MaxExtractSize / (1024 * 1024)}MB)");
                        entryStream.Write(buffer, 0, read);
                    }
                    data = entryStream.ToArray();
                }
                oResult.Add(currentEntry.Name, data);
            }
        }
        return oResult;
    }
}
