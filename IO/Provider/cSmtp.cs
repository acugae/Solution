namespace Solution.IO.Provider;
public interface IMailsProvider
{
    void SetConfig(Dictionary<String, String> parameters, cDB _oDB, cXMLManager _oXML = null);
    void AddToQueue(DataRow data);
    void Run();
    void Close();
}
public class cSmtp : IMailsProvider
{
    string host;

    public void AddToQueue(DataRow row)
    {
        string sFrom = row["ml_body"].ToString();
        string sTo = row["ml_body"].ToString();
        string sCC = row["ml_body"].ToString();
        string Subject = row["ml_body"].ToString();
        string sBody = row["ml_body"].ToString();
        string sFile = row["ml_body"].ToString();
        bool? IsHtml = (bool?)row["ml_body"];
        string sBcc = row["ml_body"].ToString();

        MailAddress SendFrom = new MailAddress(sFrom);
        MailMessage MyMessage = new MailMessage();

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
        MyMessage.Subject = Subject;
        MyMessage.IsBodyHtml = IsHtml.Value;
        MyMessage.Body = sBody;
        //
        if (IsHtml.Value)
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
        SmtpClient emailClient = new SmtpClient(host);
        emailClient.Send(MyMessage);
    }

    public void Run()
    {

    }

    public void SetConfig(Dictionary<string, string> parameters, cDB _oDB, cXMLManager _oXML = null)
    {
        try
        {
            host = parameters.Where(x => x.Key == "host").FirstOrDefault().Value;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    void IMailsProvider.Close()
    {
        throw new NotImplementedException();
    }

    private void Close()
    {

    }
}
