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
                //BinaryFormatter bf = new BinaryFormatter();
                //bf.Serialize(zs, obj);
                return Binary.ObjectToByteArray(zs);
            }
            //return ms.ToArray();
        }
    }

    public object DecompressObject(byte[] data, bool leaveOpen = false)
    {
        using (MemoryStream ms = new MemoryStream(data))
        {
            using (GZipStream zs = new GZipStream(ms, CompressionMode.Decompress, leaveOpen))
            {
                //BinaryFormatter bf = new BinaryFormatter();
                //return bf.Deserialize(zs);
                return Binary.ByteArrayToObject<object>(data);
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
        byte[] buffer = new byte[8192];
        
        using (var inputStream = new MemoryStream(ZipFile))
        using (ZipInputStream zipStream = new ZipInputStream(inputStream))
        {
            ZipEntry currentEntry;
            while ((currentEntry = zipStream.GetNextEntry()) != null)
            {
                // Protezione Zip Bomb: verifica numero file
                fileCount++;
                if (fileCount > MaxFileCount)
                    throw new InvalidOperationException($"Zip bomb protection: exceeded maximum file count ({MaxFileCount})");
                
                // Protezione Path Traversal: verifica nome file (inclusi path radicati, drive-relative e traversal)
                string entryName = currentEntry.Name;
                if (string.IsNullOrWhiteSpace(entryName)
                    || entryName.Contains("..")
                    || Path.IsPathRooted(entryName)
                    || entryName.StartsWith('/')
                    || entryName.StartsWith('\\')
                    || entryName.Contains(':'))
                {
                    throw new InvalidOperationException($"Zip entry contains invalid path: {entryName}");
                }

                // Protezione Zip Bomb reale: conta i byte effettivamente decompressi (non l'header Size)
                using var entryMs = new MemoryStream();
                int bytesRead;
                while ((bytesRead = zipStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalExtracted += bytesRead;
                    if (totalExtracted > MaxExtractSize)
                        throw new InvalidOperationException($"Zip bomb protection: extracted content exceeds size limit ({MaxExtractSize / (1024 * 1024)}MB)");
                    entryMs.Write(buffer, 0, bytesRead);
                }

                oResult[currentEntry.Name] = entryMs.ToArray();
            }
        }
        return oResult;
    }
}
