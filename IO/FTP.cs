namespace Solution.IO;
public class FTP
{
    public FTP()
    {
    }

    public FtpWebRequest CreateFtpWebRequest(string ftpDirectoryPath, string userName, string password, bool keepAlive = false)
    {
        FtpWebRequest request = (FtpWebRequest)WebRequest.Create(new Uri(ftpDirectoryPath));

        //Set proxy to null. Under current configuration if this option is not set then the proxy that is used will get an html response from the web content gateway (firewall monitoring system)
        request.Proxy = null;

        request.UsePassive = true;
        request.UseBinary = true;
        request.KeepAlive = keepAlive;

        request.Credentials = new NetworkCredential(userName, password);

        return request;
    }

    public void DownloadFile(string userName, string password, string ftpSourceFilePath, string localDestinationFilePath)
    {
        int bytesRead = 0;
        byte[] buffer = new byte[2048];
        try
        {

            FtpWebRequest request = CreateFtpWebRequest(ftpSourceFilePath, userName, password, true);
            request.Method = WebRequestMethods.Ftp.DownloadFile;

            Stream reader = request.GetResponse().GetResponseStream();
            FileStream fileStream = new FileStream(localDestinationFilePath, FileMode.Create);

            while (true)
            {
                bytesRead = reader.Read(buffer, 0, buffer.Length);

                if (bytesRead == 0)
                    break;

                fileStream.Write(buffer, 0, bytesRead);
            }
            fileStream.Close();
        }
        catch (WebException ex)
        {
            String status = ((FtpWebResponse)ex.Response).StatusDescription;
        }
    }

    public void ListFile(string userName, string password, string ftpSourceFilePath)
    {

        FtpWebRequest request = CreateFtpWebRequest(ftpSourceFilePath, userName, password, true);
        request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

        FtpWebResponse response = (FtpWebResponse)request.GetResponse();
        StreamReader streamReader = new StreamReader(response.GetResponseStream());

        List<string> directories = new List<string>();

        string line = streamReader.ReadLine();
        while (!string.IsNullOrEmpty(line))
        {
            directories.Add(line);
            line = streamReader.ReadLine();
        }

        streamReader.Close();

    }


}
