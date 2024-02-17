namespace Solution.Infrastructure;
public class Service
{
    public DBHttpClients DBHttpClients = null;
    public DBConfig DBConfig = null;
    public Service(DB oDB)
    {
        DBHttpClients = new(oDB);
        DBConfig = new(oDB);
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;
    }
    public string InvokeServiceJSON(string sURL, string sJSON, string sMethod = "POST", string sContentType = "application/json;charset=utf-8", int iTimeout = 1200000, string sHeaders = null)
    {
        int maxBufferLengthToLog = 10000;
        try
        {
            if (!string.IsNullOrEmpty(sJSON))
                cLogger.WriteLine(String.Format("InvokeServiceJSON INIZIO: {0} {1} {2}", sURL, sMethod, sJSON.Length > maxBufferLengthToLog ? sJSON.Substring(0, maxBufferLengthToLog) : sJSON), cLogger.TipoLog.Debug);

            ExtendedWebClient web = new ExtendedWebClient();

            if (iTimeout != 0)
                web.Timeout = iTimeout;
            else
                web.Timeout = 300000;

            Byte[] res = null;
            DateTime before = DateTime.Now;
            if (sMethod.ToLower().Equals("get"))
            {
                web.Headers.Add("Content-Type", sContentType);
                web.Headers.Add("accept", sContentType);
                web.Headers = ParseHeader(web.Headers, sHeaders);
                cLogger.WriteLine("Provo ad invocare il seguente url " + sURL, cLogger.TipoLog.Debug);
                res = web.DownloadData(sURL);
                cLogger.WriteLine("Il servizio è stato invocato con successo. Numero byte restituiti " + res.LongLength, cLogger.TipoLog.Debug);
            }
            else
            {
                web.Headers.Add("Content-Type", sContentType);
                web.Headers = ParseHeader(web.Headers, sHeaders);
                Byte[] d = System.Text.Encoding.UTF8.GetBytes(sJSON);
                cLogger.WriteLine("Provo ad invocare il seguente url " + sURL, cLogger.TipoLog.Debug);
                res = web.UploadData(sURL, sMethod, d);
                cLogger.WriteLine("Il servizio è stato invocato con successo. Numero byte restituiti " + res.LongLength, cLogger.TipoLog.Debug);
            }
            //
            //WebHeaderCollection _ResponseHeaders = web.ResponseHeaders;
            //
            DateTime after = DateTime.Now;
            double diffInSeconds = (after - before).TotalSeconds;
            cLogger.WriteLine("Il servizio è stato invocato con successo. Provo ad ottenre la risposta un utf ", cLogger.TipoLog.Debug);
            string response = System.Text.Encoding.UTF8.GetString(res);
            cLogger.WriteLine("Il servizio è stato invocato con successo. Risposta ottenuta ", cLogger.TipoLog.Debug);

            if (!string.IsNullOrEmpty(sJSON))
                cLogger.WriteLine(String.Format("InvokeServiceJSON took {3} seconds: {0} {1} {2}", sURL, sMethod, response, diffInSeconds), cLogger.TipoLog.Debug);

            //cambiato il tipo di Encoding. con ASCII non venivano riconosciute le vocali accentate
            //return System.Text.Encoding.ASCII.GetString(res);
            return response;
        }
        catch (Exception ex)
        {
            cLogger.WriteLine(String.Format("InvokeServiceJSON ERRORE: {0} {1} {2} {3}", sURL, sMethod, sJSON, ex.Message), cLogger.TipoLog.Debug);
            throw ex;
        }
    }

    public string InvokeService(string sURL, string sPayload, string sMethod, string sContentType = "application/json", int iTimeOut = 0, string? sHeaders = null)
    {
        ExtendedWebClient web = new ExtendedWebClient();
        if (iTimeOut != 0)
            web.Timeout = iTimeOut;
        //
        Byte[] res = null;
        if (sMethod.ToLower().Equals("get"))
        {
            web.Headers = ParseHeader(web.Headers, sHeaders);
            res = web.DownloadData(sURL);
        }
        else
        {
            web.Headers.Add("Content-Type", sContentType);
            web.Headers = ParseHeader(web.Headers, sHeaders);
            Byte[] d = System.Text.Encoding.ASCII.GetBytes(sPayload);
            res = web.UploadData(sURL, sMethod, d);
        }
        //
        //WebHeaderCollection _ResponseHeaders = web.ResponseHeaders;
        //
        return System.Text.Encoding.UTF8.GetString(res);
    }

    public string InvokeService(Cache<string> oCache, int minuteExpiration, string sCodice, Dictionary<string, string>? oParams = null)
    {
        string s = oParams == null ? "" : string.Join(";", oParams.Select(x => x.Key + "=" + x.Value).ToArray());
        CacheKey cacheKey = new CacheKey(sCodice + "_" + s);
        string sResponse = oCache.get(cacheKey);
        if (sResponse == null)
        {
            sResponse = InvokeService(sCodice, oParams);
            CacheValue<string> cacheValue = new CacheValue<string>(sResponse, minuteExpiration, true);
            oCache.Add(cacheKey, cacheValue);
        }

        return sResponse;
    }

    int idHistory = -1;
    public int HistoryID { get { return idHistory; } }

    public string InvokeService(string sCodice, Dictionary<string, string>? oParams = null)
    {
        DataRow oDR = DBHttpClients.GetHttpClient(sCodice);

        idHistory = -1;
        if (oDR.Table != null && oDR.Table.Columns.Contains("hc_cache") && oDR["hc_cache"] != DBNull.Value)
            return oDR["hc_cache"].ToString();
        //
        if (oDR["hc_active"].ToString().Equals("0"))
            throw new Exception("Codice HttpClient non attivo");

        string sResponse = "";
        if (oParams != null && oParams.Count > 0)
        {
            foreach (string sItem in oParams.Keys)
            {
                oDR["hc_url"] = oDR["hc_url"].ToString().Replace(sItem, oParams[sItem]);
                oDR["hc_payload"] = oDR["hc_payload"].ToString().Replace(sItem, oParams[sItem]);
                oDR["hc_method"] = oDR["hc_method"].ToString().Replace(sItem, oParams[sItem]);
                oDR["hc_contextType"] = oDR["hc_contextType"].ToString().Replace(sItem, oParams[sItem]);
                oDR["hc_header"] = oDR["hc_header"].ToString().Replace(sItem, oParams[sItem]);
                //
                oDR["hc_url"] = ReplaceDefine(oDR["hc_url"].ToString());
                oDR["hc_payload"] = ReplaceDefine(oDR["hc_payload"].ToString());
                oDR["hc_method"] = ReplaceDefine(oDR["hc_method"].ToString());
                oDR["hc_contextType"] = ReplaceDefine(oDR["hc_contextType"].ToString());
                oDR["hc_header"] = ReplaceDefine(oDR["hc_header"].ToString());
            }
        }
        //
        try
        {
            DateTime before = DateTime.Now;
            string sHeader = (oDR["hc_header"] as string);
            idHistory = DBHttpClients.InsertHttpClientHistory((int)oDR["hc_id"], (string)oDR["hc_method"], (string)oDR["hc_contextType"], (string)oDR["hc_url"], oDR["hc_payload"] as string, sHeader);

            int iTimeOut = (oDR["hc_timeout"] == DBNull.Value ? 0 : (int)oDR["hc_timeout"]);
            sResponse = InvokeServiceJSON((string)oDR["hc_url"], oDR["hc_payload"] as string, (string)oDR["hc_method"], (string)oDR["hc_contextType"], iTimeOut, sHeader);

            DateTime after = DateTime.Now;
            double diffInSeconds = (after - before).TotalSeconds;
            cLogger.WriteLine(String.Format("InvokeServiceJSON took {3} seconds: {0} {1} {2}", (string)oDR["hc_url"], oDR["hc_method"], sResponse, diffInSeconds), cLogger.TipoLog.Debug);
            DBHttpClients.UpdateHttpClientHistory(idHistory, sResponse);
        }
        catch (Exception ex)
        {
            DBHttpClients.UpdateHttpClientHistory(idHistory, ex.Message, 1);
            throw ex;/*nuovo*/
        }
        return sResponse;
    }
    private string ReplaceDefine(string sValue)
    {
        if (sValue.IndexOf("@config.") >= 0)
        {
            StringAdvance oVal = new StringAdvance(sValue);
            string[] oValues = oVal.GetIntoTag("@config.{", "}");
            for (int j = 0; j < oValues.Length; j++)
            {
                sValue = sValue.Replace("@config.{" + oValues[j] + "}", this.DBConfig.GetConfig(oValues[j], ""));
            }
        }
        return sValue;
    }
    private WebHeaderCollection ParseHeader(WebHeaderCollection oHeader, string sHeaders = null)
    {
        if (string.IsNullOrEmpty(sHeaders))
            return oHeader;
        if (string.IsNullOrEmpty(sHeaders.Trim()))
            return oHeader;

        oHeader.Clear();
        string[] ovHeadres = sHeaders.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
        for (int i = 0; i < ovHeadres.Length; i++)
        {
            string[] sValue = ovHeadres[i].Split(':');
            if (sValue != null && sValue.Length == 2)
            {
                oHeader.Add(sValue[0], sValue[1]);
            }
        }
        return oHeader;
    }
}
public class ExtendedWebClient : WebClient
{
    public int Timeout { get; set; }

    protected override WebRequest GetWebRequest(Uri address)
    {
        WebRequest request = base.GetWebRequest(address);
        if (request != null)
            request.Timeout = Timeout;
        return request;
    }

    public ExtendedWebClient()
    {
        Timeout = 1200000; // the standard HTTP Request Timeout default
    }
}
