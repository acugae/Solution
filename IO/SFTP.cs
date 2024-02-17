namespace Solution.IO;
public class SFTP
{
    SftpClient oFTP = null;
    public SFTP(string sUrl, string sUser, string sPassword, int iPort = 22)
    {
        oFTP = new SftpClient(sUrl, iPort, sUser, sPassword);
    }
    public SFTP(string sUrl, string sUser, string sPassword, int iPort, string sPathFileKey)
    {
        PrivateKeyFile oFileKey = new PrivateKeyFile(sPathFileKey);
        var connectionInfo = new Renci.SshNet.ConnectionInfo(sUrl, sUser,
                                    new PasswordAuthenticationMethod(sUser, sPassword),
                                    new PrivateKeyAuthenticationMethod(sUser, oFileKey));
        oFTP = new SftpClient(connectionInfo);
    }
    public void Connect()
    {
        oFTP.Connect();
    }
    public bool IsConnect
    {
        get { return oFTP.IsConnected; }
    }
    public void Disconnect() => oFTP.Disconnect();
    public IEnumerable<ISftpFile> List(string sPath)
    {
        return oFTP.ListDirectory(sPath);
    }
    public bool Exists(string sPath)
    {
        return oFTP.Exists(sPath);
    }
    public void Upload(string sPath, byte[] oFile)
    {
        MemoryStream ms = new MemoryStream(oFile);
        oFTP.UploadFile(ms, sPath);
    }
    public byte[] Download(string sPath)
    {
        MemoryStream msResult = new MemoryStream();
        oFTP.DownloadFile(sPath, msResult);
        return msResult.ToArray();
    }
}
