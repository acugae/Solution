namespace Solution.Communication;

public class Email
{
    string _SMTPServer = ""; // _oDB.GetConfig(@"\system\mail\normal\smtpserver"
    public Email(string SMTPServer) => _SMTPServer = SMTPServer;

    /// <summary>Porta del server SMTP. null = porta predefinita.</summary>
    public int? Port { get; set; }

    /// <summary>true = connessione cifrata (STARTTLS). Default: false, per compatibilità con i relay interni.</summary>
    public bool EnableSsl { get; set; } = false;

    /// <summary>Credenziali per l'autenticazione sul server SMTP. null = nessuna autenticazione.</summary>
    public NetworkCredential? Credentials { get; set; }

    public void SendMail(string sFrom, string sTo, string sCC, string Subject, string sBody, string sFile, bool IsHtml = true, string sBcc = null)
    {
        MailMessage MyMessage = CreateMessage(sFrom, sTo, sCC, Subject, sBody, IsHtml, sBcc);
        if (sFile != null)
            MyMessage.Attachments.Add(new Attachment(sFile));
        Send(MyMessage);
    }
    public void SendMailAttach(string sFrom, string sTo, string sCC, string Subject, string sBody, string[] sFiles, bool IsHtml = true, string? sBcc = null)
    {
        MailMessage MyMessage = CreateMessage(sFrom, sTo, sCC, Subject, sBody, IsHtml, sBcc);
        if (sFiles != null)
        {
            for (int i = 0; i < sFiles.Length; i++)
                MyMessage.Attachments.Add(new Attachment(sFiles[i]));
        }
        Send(MyMessage);
    }
    public void SendMail(string sFrom, string sTo, string sCC, string Subject, string sBody, byte[] file, string filename, bool IsHtml = true, string? sBcc = null)
    {
        MailMessage MyMessage = CreateMessage(sFrom, sTo, sCC, Subject, sBody, IsHtml, sBcc);
        if (filename != null)
            MyMessage.Attachments.Add(new Attachment(new MemoryStream(file), filename));
        Send(MyMessage);
    }

    private static MailMessage CreateMessage(string sFrom, string sTo, string? sCC, string Subject, string sBody, bool IsHtml, string? sBcc)
    {
        MailMessage MyMessage = new MailMessage();
        MyMessage.From = new MailAddress(sFrom);
        AddAddresses(MyMessage.To, sTo);
        AddAddresses(MyMessage.CC, sCC);
        AddAddresses(MyMessage.Bcc, sBcc);
        MyMessage.Subject = Subject;
        MyMessage.IsBodyHtml = IsHtml;
        MyMessage.Body = sBody;
        if (IsHtml)
        {
            MyMessage.BodyEncoding = Encoding.UTF8;
            AlternateView av = AlternateView.CreateAlternateViewFromString(sBody, null, MediaTypeNames.Text.Html);
            MyMessage.AlternateViews.Add(av);
        }
        return MyMessage;
    }

    // Indirizzi separati da virgola o punto e virgola.
    private static void AddAddresses(MailAddressCollection oAddresses, string? sAddresses)
    {
        if (sAddresses == null)
            return;
        string[] sVAddresses = sAddresses.Replace(";", ",").Split(',');
        for (int i = 0; i < sVAddresses.Length; i++)
        {
            if (!sVAddresses[i].Trim().Equals(""))
                oAddresses.Add(sVAddresses[i].Trim());
        }
    }

    // Messaggio, allegati e client vengono sempre rilasciati.
    private void Send(MailMessage MyMessage)
    {
        using (MyMessage)
        using (SmtpClient emailClient = new SmtpClient(_SMTPServer))
        {
            if (Port.HasValue)
                emailClient.Port = Port.Value;
            emailClient.EnableSsl = EnableSsl;
            if (Credentials != null)
                emailClient.Credentials = Credentials;
            emailClient.Send(MyMessage);
        }
    }
}
