namespace Solution.IO;
public class FTP
{
    public FTP()
    {
    }

    public FtpWebRequest CreateFtpWebRequest(string ftpDirectoryPath, string userName, string password, bool keepAlive = false, bool enableSsl = false)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpDirectoryPath));

        //Set proxy to null. Under current configuration if this option is not set then the proxy that is used will get an html response from the web content gateway (firewall monitoring system)
        request.Proxy = null;

        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = keepAlive;
        request.EnableSsl = enableSsl;

        request.Credentials = new NetworkCredential(userName, password);

        return request;
    }

    public void DownloadFile(string userName, string password, string ftpSourceFilePath, string localDestinationFilePath)
    {
        int bytesRead = 0;
        byte[] buffer = new byte[8192];
        try
        {
            FtpWebRequest request = CreateFtpWebRequest(ftpSourceFilePath, userName, password, true);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            using var response = request.GetResponse();
            using Stream reader = response.GetResponseStream();
            using FileStream fileStream = new FileStream(localDestinationFilePath, FileMode.Create);

            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, bytesRead);
            }
        }
        catch (WebException ex)
        {
            string? status = (ex.Response as FtpWebResponse)?.StatusDescription;
        }
    }

    public void ListFile(string userName, string password, string ftpSourceFilePath)
    {
        FtpWebRequest request = CreateFtpWebRequest(ftpSourceFilePath, userName, password, true);
        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

        using FtpWebResponse response = (FtpWebResponse)request.GetResponse();
        using StreamReader streamReader = new StreamReader(response.GetResponseStream());

        List<string> directories = new List<string>();

        string? line = streamReader.ReadLine();
        while (!string.IsNullOrEmpty(line))
        {
            directories.Add(line);
            line = streamReader.ReadLine();
        }
    }


}
