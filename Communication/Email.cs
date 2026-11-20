namespace Solution.Communication;

public class Email
{
    string _SMTPServer = ""; // _oDB.GetConfig(@"\system\mail\normal\smtpserver"
    public bool EnableSsl { get; set; } = false;

    public Email(string SMTPServer) => _SMTPServer = SMTPServer;

    private static string SanitizeHeaderValue(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        return value.Replace("\r", string.Empty).Replace("\n", string.Empty);
    }

    public void SendMail(string sFrom, string sTo, string sCC, string Subject, string sBody, string sFile, bool IsHtml = true, string sBcc = null)
    {
        MailAddress SendFrom = new MailAddress(sFrom);
        using MailMessage MyMessage = new MailMessage();
        MyMessage.From = SendFrom;
        string[] sVTo = sTo.Replace(";", ",").Split(',');
        for (int i = 0; i < sVTo.Length; i++)
        {
            if (!sVTo[i].Trim().Equals(""))
                MyMessage.To.Add(sVTo[i].Trim());
        }
        if (sCC != null)
        {
            string[] sVCC = sCC.Replace(";", ",").Split(',');
            for (int i = 0; i < sVCC.Length; i++)
            {
                if (!sVCC[i].Trim().Equals(""))
                    MyMessage.CC.Add(sVCC[i].Trim());
            }
        }
        if (sBcc != null)
        {
            string[] sVBcc = sBcc.Replace(";", ",").Split(',');
            for (int i = 0; i < sVBcc.Length; i++)
            {
                if (!sVBcc[i].Trim().Equals(""))
                    MyMessage.Bcc.Add(sVBcc[i].Trim());
            }
        }
        MyMessage.Subject = SanitizeHeaderValue(Subject);
        MyMessage.IsBodyHtml = IsHtml;
        MyMessage.Body = sBody;
        //
        if (IsHtml)
        {
            MyMessage.BodyEncoding = Encoding.UTF8;
            AlternateView av = AlternateView.CreateAlternateViewFromString(sBody, null, MediaTypeNames.Text.Html);
            MyMessage.AlternateViews.Add(av);
        }
        //
        if (sFile != null)
        {
            Attachment attachFile = new Attachment(sFile);
            MyMessage.Attachments.Add(attachFile);
        }
        using SmtpClient emailClient = new SmtpClient(_SMTPServer) { EnableSsl = EnableSsl };
        emailClient.Send(MyMessage);
    }
    public void SendMailAttach(string sFrom, string sTo, string sCC, string Subject, string sBody, string[] sFiles, bool IsHtml = true, string? sBcc = null)
    {
        MailAddress SendFrom = new MailAddress(sFrom);
        using MailMessage MyMessage = new MailMessage();
        MyMessage.From = SendFrom;
        string[] sVTo = sTo.Replace(";", ",").Split(',');
        for (int i = 0; i < sVTo.Length; i++)
        {
            if (!sVTo[i].Trim().Equals(""))
                MyMessage.To.Add(sVTo[i].Trim());
        }
        if (sCC != null)
        {
            string[] sVCC = sCC.Replace(";", ",").Split(',');
            for (int i = 0; i < sVCC.Length; i++)
            {
                if (!sVCC[i].Trim().Equals(""))
                    MyMessage.CC.Add(sVCC[i].Trim());
            }
        }
        if (sBcc != null)
        {
            string[] sVBcc = sBcc.Replace(";", ",").Split(',');
            for (int i = 0; i < sVBcc.Length; i++)
            {
                if (!sVBcc[i].Trim().Equals(""))
                    MyMessage.Bcc.Add(sVBcc[i].Trim());
            }
        }
        MyMessage.Subject = SanitizeHeaderValue(Subject);
        MyMessage.IsBodyHtml = IsHtml;
        MyMessage.Body = sBody;
        //
        if (IsHtml)
        {
            MyMessage.BodyEncoding = Encoding.UTF8;
            AlternateView av = AlternateView.CreateAlternateViewFromString(sBody, null, MediaTypeNames.Text.Html);
            MyMessage.AlternateViews.Add(av);
        }
        //
        if (sFiles != null)
        {
            for (int i = 0; i < sFiles.Length; i++)
            {
                Attachment attachFile = new Attachment(sFiles[i]);
                MyMessage.Attachments.Add(attachFile);
            }
        }
        using SmtpClient emailClient = new SmtpClient(_SMTPServer) { EnableSsl = EnableSsl };
        emailClient.Send(MyMessage);
    }
    public void SendMail(string sFrom, string sTo, string sCC, string Subject, string sBody, byte[] file, string filename, bool IsHtml = true, string? sBcc = null)
    {
        MailAddress SendFrom = new MailAddress(sFrom);
        using MailMessage MyMessage = new MailMessage();
        MyMessage.From = SendFrom;
        string[] sVTo = sTo.Replace(";", ",").Split(',');
        for (int i = 0; i < sVTo.Length; i++)
        {
            if (!sVTo[i].Trim().Equals(""))
                MyMessage.To.Add(sVTo[i].Trim());
        }
        if (sCC != null)
        {
            string[] sVCC = sCC.Replace(";", ",").Split(',');
            for (int i = 0; i < sVCC.Length; i++)
            {
                if (!sVCC[i].Trim().Equals(""))
                    MyMessage.CC.Add(sVCC[i].Trim());
            }
        }
        if (sBcc != null)
        {
            string[] sVBcc = sBcc.Replace(";", ",").Split(',');
            for (int i = 0; i < sVBcc.Length; i++)
            {
                if (!sVBcc[i].Trim().Equals(""))
                    MyMessage.Bcc.Add(sVBcc[i].Trim());
            }
        }
        MyMessage.Subject = SanitizeHeaderValue(Subject);
        MyMessage.IsBodyHtml = IsHtml;
        MyMessage.Body = sBody;
        if (IsHtml)
        {
            MyMessage.BodyEncoding = Encoding.UTF8;
            AlternateView av = AlternateView.CreateAlternateViewFromString(sBody, null, MediaTypeNames.Text.Html);
            MyMessage.AlternateViews.Add(av);
        }
        MemoryStream? stream = null;
        try
        {
            if (filename != null)
            {
                string safeFilename = Path.GetFileName(SanitizeHeaderValue(filename));
                stream = new MemoryStream(file);
                Attachment att = new Attachment(stream, safeFilename);
                MyMessage.Attachments.Add(att);
            }
            using SmtpClient emailClient = new SmtpClient(_SMTPServer) { EnableSsl = EnableSsl };
            emailClient.Send(MyMessage);
        }
        finally
        {
            stream?.Dispose();
        }
    }
}
