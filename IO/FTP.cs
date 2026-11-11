namespace Solution.IO;

/// <summary>
/// Client FTP. Il protocollo FTP semplice trasmette credenziali e dati in chiaro:
/// impostare <see cref="EnableSsl"/> = true (FTPS) quando il server lo supporta, oppure usare SFTP.
/// </summary>
public class FTP
{
    public FTP()
    {
    }

    /// <summary>true = connessione cifrata FTPS (FTP su TLS). Default: false, per compatibilità.</summary>
    public bool EnableSsl { get; set; } = false;

    public FtpWebRequest CreateFtpWebRequest(string ftpDirectoryPath, string userName, string password, bool keepAlive = false)
    {
#pragma warning disable SYSLIB0014 // FtpWebRequest è obsoleto ma resta l'unico client FTP del framework
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpDirectoryPath));
#pragma warning restore SYSLIB0014

        //Set proxy to null. Under current configuration if this option is not set then the proxy that is used will get an html response from the web content gateway (firewall monitoring system)
        request.Proxy = null;

        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = keepAlive;
        request.EnableSsl = EnableSsl;

        request.Credentials = new NetworkCredential(userName, password);

        return request;
    }

    /// <summary>
    /// Scarica il file. Dalla 2.0.0 gli errori non vengono più ignorati: in caso di problemi viene
    /// sollevata l'eccezione (WebException) e il file parziale viene eliminato.
    /// </summary>
    public void DownloadFile(string userName, string password, string ftpSourceFilePath, string localDestinationFilePath)
    {
        FtpWebRequest request = CreateFtpWebRequest(ftpSourceFilePath, userName, password, true);
        request.Method = WebRequestMethods.Ftp.DownloadFile;

        bool bCompleted = false;
        try
        {
            using (WebResponse response = request.GetResponse())
            using (Stream reader = response.GetResponseStream())
            using (FileStream fileStream = new FileStream(localDestinationFilePath, FileMode.Create))
            {
                reader.CopyTo(fileStream);
            }
            bCompleted = true;
        }
        finally
        {
            if (!bCompleted && File.Exists(localDestinationFilePath))
            {
                try { File.Delete(localDestinationFilePath); } catch (IOException) { }
            }
        }
    }

    /// <summary>Restituisce l'elenco (dettagliato) del contenuto della cartella.</summary>
    public List<string> ListFile(string userName, string password, string ftpSourceFilePath)
    {
        FtpWebRequest request = CreateFtpWebRequest(ftpSourceFilePath, userName, password, true);
        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

        List<string> directories = new List<string>();
        using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
        using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
        {
            string? line = streamReader.ReadLine();
            while (!string.IsNullOrEmpty(line))
            {
                directories.Add(line);
                line = streamReader.ReadLine();
            }
        }
        return directories;
    }
}
