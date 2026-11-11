namespace Solution.IO;

/// <summary>
/// Client SFTP. Dalla 2.0.0 la host key del server viene verificata: impostare
/// <see cref="HostKeyFingerprintSha256"/> prima di Connect(), altrimenti la connessione viene rifiutata.
/// </summary>
public class SFTP : IDisposable
{
    SftpClient oFTP = null;
    private string? _sRejectedFingerprint;

    /// <summary>
    /// Impronta SHA-256 attesa della host key del server, in Base64 (con o senza prefisso "SHA256:"),
    /// come mostrata da ssh-keygen -lf oppure dal messaggio di errore di Connect().
    /// </summary>
    public string? HostKeyFingerprintSha256 { get; set; }

    /// <summary>
    /// true = accetta qualsiasi host key (comportamento delle versioni precedenti, esposto ad attacchi
    /// man-in-the-middle). Default: false.
    /// </summary>
    public bool AcceptAnyHostKey { get; set; } = false;

    public SFTP(string sUrl, string sUser, string sPassword, int iPort = 22)
    {
        oFTP = new SftpClient(sUrl, iPort, sUser, sPassword);
        Init();
    }
    public SFTP(string sUrl, string sUser, string sPassword, int iPort, string sPathFileKey)
    {
        PrivateKeyFile oFileKey = new PrivateKeyFile(sPathFileKey);
        var connectionInfo = new Renci.SshNet.ConnectionInfo(sUrl, iPort, sUser,
                                    new PasswordAuthenticationMethod(sUser, sPassword),
                                    new PrivateKeyAuthenticationMethod(sUser, oFileKey));
        oFTP = new SftpClient(connectionInfo);
        Init();
    }

    private void Init()
    {
        oFTP.HostKeyReceived += (sender, e) =>
        {
            if (AcceptAnyHostKey)
            {
                e.CanTrust = true;
                return;
            }
            string sExpected = NormalizeFingerprint(HostKeyFingerprintSha256);
            if (sExpected.Length > 0 && string.Equals(NormalizeFingerprint(e.FingerPrintSHA256), sExpected, StringComparison.Ordinal))
            {
                e.CanTrust = true;
                return;
            }
            _sRejectedFingerprint = e.FingerPrintSHA256;
            e.CanTrust = false;
        };
    }

    private static string NormalizeFingerprint(string? sFingerprint)
    {
        string sValue = (sFingerprint ?? "").Trim();
        if (sValue.StartsWith("SHA256:", StringComparison.OrdinalIgnoreCase))
            sValue = sValue.Substring(7);
        return sValue.TrimEnd('=');
    }

    public void Connect()
    {
        _sRejectedFingerprint = null;
        try
        {
            oFTP.Connect();
        }
        catch (Exception ex) when (_sRejectedFingerprint != null)
        {
            throw new InvalidOperationException(
                "SFTP: host key del server non verificata (SHA256:" + _sRejectedFingerprint + "). " +
                "Impostare HostKeyFingerprintSha256 con l'impronta attesa oppure, solo su reti fidate, AcceptAnyHostKey = true.", ex);
        }
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
        oFTP?.Dispose();
        oFTP = null;
    }
}
