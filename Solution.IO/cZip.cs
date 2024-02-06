namespace Solution.IO;
public class cZip
{
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
        using (ZipInputStream zipStream = new ZipInputStream(new MemoryStream(ZipFile)))
        {
            ZipEntry currentEntry;
            while ((currentEntry = zipStream.GetNextEntry()) != null)
            {
                byte[] data = new byte[currentEntry.Size];
                zipStream.Read(data, 0, data.Length);
                oResult.Add(currentEntry.Name, data);
            }
        }
        return oResult;
    }
}
