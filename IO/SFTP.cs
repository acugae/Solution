namespace Solution.IO;
public class SFTP : IDisposable
{
    private SftpClient oFTP;
    private PrivateKeyFile? _keyFile;
    private bool _disposed;

    public SFTP(string sUrl, string sUser, string sPassword, int iPort = 22)
    {
        oFTP = new SftpClient(sUrl, iPort, sUser, sPassword);
    }

    public SFTP(string sUrl, string sUser, string sPassword, int iPort, string sPathFileKey)
    {
        _keyFile = new PrivateKeyFile(sPathFileKey);
        var connectionInfo = new Renci.SshNet.ConnectionInfo(sUrl, iPort, sUser,
                                    new PasswordAuthenticationMethod(sUser, sPassword),
                                    new PrivateKeyAuthenticationMethod(sUser, _keyFile));
        oFTP = new SftpClient(connectionInfo);
    }

    /// <summary>
    /// Imposta la verifica della fingerprint SHA-256 (Base64 o esadecimale) dell'host SSH
    /// per prevenire attacchi Man-in-the-Middle (chiamare prima di Connect()).
    /// </summary>
    public SFTP WithHostKeyFingerprint(string expectedSha256Fingerprint)
    {
        if (string.IsNullOrWhiteSpace(expectedSha256Fingerprint))
            throw new ArgumentNullException(nameof(expectedSha256Fingerprint));

        string expected = expectedSha256Fingerprint.Trim();
        oFTP.HostKeyReceived += (_, e) =>
        {
            string actual = e.FingerPrintSHA256 ?? string.Empty;
            e.CanTrust = string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
        };
        return this;
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
        using MemoryStream ms = new MemoryStream(oFile);
        oFTP.UploadFile(ms, sPath);
    }
    public byte[] Download(string sPath)
    {
        using MemoryStream msResult = new MemoryStream();
        oFTP.DownloadFile(sPath, msResult);
        return msResult.ToArray();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            if (oFTP?.IsConnected == true)
                oFTP.Disconnect();
            oFTP?.Dispose();
            _keyFile?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
