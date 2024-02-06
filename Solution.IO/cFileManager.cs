namespace Solution.IO;
/// <summary>
/// Gestisce file di testo e binari, facilita la lettura e la scrittura.
/// </summary>
public class cFileManager
{
    /// <summary>
    /// Restituisce una stringa contenente il file specificato.
    /// </summary>
    static public string GetFile(string sFileName)
    {
        StreamReader reader = new StreamReader(sFileName, System.Text.Encoding.Default);
        string input = "";
        input = reader.ReadToEnd();
        reader.Close();
        return input;
    }
    /// <summary>
    /// Restituisce ogni riga contenuta in un file di testo.
    /// </summary>
    static public string[] GetFileLines(string sFileName)
    {
        string line = "";
        ArrayList oText = new ArrayList();
        if (File.Exists(sFileName))
        {
            StreamReader file = new StreamReader(sFileName);
            try
            {
                while ((line = file.ReadLine()) != null)
                {
                    oText.Add(line);
                }
            }
            finally
            {
                if (file != null)
                    file.Close();
            }
        }
        string[] ovString = new string[oText.Count];
        Array.Copy(oText.ToArray(), ovString, oText.ToArray().Length);
        return ovString;
    }

    //
    /// <summary>
    /// Scrive nel file il valore specificato.
    /// </summary>
    static public void SetFile(string sFileName, string sValue)
    {
        try
        {
            StreamWriter sw = new StreamWriter(sFileName);
            sw.Write(sValue);
            sw.Close();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    //
    /// <summary>
    /// Restituisce un array di byte del file in ingresso.
    /// </summary>
    static public byte[] GetFileByte(string sFileName)
    {
        FileStream fs = File.OpenRead(sFileName);
        BinaryReader br = new BinaryReader(fs);
        byte[] oResult = br.ReadBytes(Convert.ToInt32(fs.Length));
        br.Close();
        fs.Close();
        return oResult;
    }
    /// <summary>
    /// Scrive nel file l'array di byte specificato.(Nel caso in cui il file esiste verrà sovrascritto)
    /// </summary>
    static public void SetFileByte(string sFileName, byte[] oBuffer)
    {
        try
        {
            FileStream fs = new FileStream(sFileName, FileMode.Create);
            fs.Write(oBuffer, 0, oBuffer.Length);
            fs.Close();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    /// <summary>
    /// Converte il path in ingresso in un path valido per ambienti Linux.
    /// </summary>
    static public string NormalizePath(string sPath)
    {
        if (sPath.Split('/').Length <= 1)
        {
            return sPath;
        }
        else
        {
            string sTmp = sPath.Split('/')[0];
            string[] svTemp = sPath.Split('/');
            for (int i = 1; i < svTemp.Length; i++)
            {
                sTmp = Path.Combine(sTmp, svTemp[i]);
            }
            return sTmp;
        }
    }
    /// <summary>
    /// Controlla se il path del file è relativo, assoluto o con contiene il carattere ~, ritorna il path assoluto.
    /// </summary>
    /// <param name="sPath">Path relativo.</param>
    /// <returns>Path assoluto.</returns>
    static public string GetPathRoot(string sPath)
    {
        sPath = Path.IsPathRooted(sPath) ? sPath : sPath.IndexOf("~") >= 0 ? Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, sPath.Replace("~", "")) : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sPath);
        return sPath;
    }
}
