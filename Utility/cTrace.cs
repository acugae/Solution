namespace Solution;

//public enum enCategory { Off, Error, Warning, Info, Verbose };
/// <summary>
/// Classe per la gestione del trace.
/// </summary>
public class cTrace
{
    static int iLevel = 0;
    static bool bIsSuspend = false;
    static string[] osLevel = new string[] { "Off", "Error", "Warning", "Info", "Verbose" };
    static bool bPrintDateTime = false;
    static string sFormatDateTime = "yyyy-MM-dd HH:mm:ss";
    /// <summary>
    /// Inizializza una nuova istanza della classe.
    /// </summary>
    public cTrace()
    {
        Trace.AutoFlush = true;
    }
    /// <summary>
    /// Imposta se la data deve essere scritta.
    /// </summary>
    public static bool PrintDateTime
    {
        get { return bPrintDateTime; }
        set { bPrintDateTime = value; }
    }
    /// <summary>
    /// Imposta il formato della data scritta.
    /// </summary>
    public static string FormatDateTime
    {
        get { return sFormatDateTime; }
        set { sFormatDateTime = value; }
    }

    /// <summary>
    /// Imposta se la data deve essere sospesa, quindi non scritta fino all'annullamento della sospensione.
    /// </summary>
    public static bool IsSuspend
    {
        get { return bIsSuspend; }
        set { bIsSuspend = value; }
    }

    /// <summary>
    /// Imposta i nomi dei livelli in un array.
    /// </summary>
    public static string[] StringLevel
    {
        get { return osLevel; }
        set { osLevel = value; }
    }

    /// <summary>
    /// Definisce l'indice dei livelli contenuti nell'array.
    /// </summary>
    public static int Level
    {
        get { return iLevel; }
        set { iLevel = value; }
    }
    /// <summary>
    /// Aggiunge nuovi Listener all'istanza.
    /// </summary>
    public static void AddListener(TraceListener oTL)
    {
        Trace.Listeners.Add(oTL);
    }
    /// <summary>
    /// Elimina nuovi Listener all'istanza.
    /// </summary>
    public static void DelListener(TraceListener oTL)
    {
        Trace.Listeners.Remove(oTL);
    }

    /// <summary>
    /// Scrive un testo specficato se la condizione è True.
    /// </summary>
    public static void WriteLine(bool condition, string sMessage)
    {
        if (bIsSuspend)
            return;
        Trace.WriteLineIf(condition, print(sMessage));
        Trace.Flush();
    }
    /// <summary>
    /// Scrive un testo specficato.
    /// </summary>
    public static void WriteLine(string sMessage)
    {
        if (bIsSuspend)
            return;
        Trace.WriteLine(print(sMessage));
        Trace.Flush();
    }

    /// <summary>
    /// Scrive il testo specficato nella categoria specificata.
    /// </summary>
    public static void WriteLine(string sMessage, string category)
    {
        if (bIsSuspend)
            return;
        Trace.WriteLine(print(sMessage), category);
        Trace.Flush();
    }
    /// <summary>
    /// Scrive il testo specficato se il Livello è inferiore a quello scelto.
    /// </summary>
    public static void WriteLine(string sMessage, TraceLevel oTraceLevel)
    {
        if (bIsSuspend)
            return;
        int intLevel = 0;
        if (oTraceLevel == TraceLevel.Error)
            intLevel = 1;
        if (oTraceLevel == TraceLevel.Warning)
            intLevel = 2;
        if (oTraceLevel == TraceLevel.Info)
            intLevel = 3;
        if (oTraceLevel == TraceLevel.Verbose)
            intLevel = 4;
        WriteLine(sMessage, intLevel);
    }
    /// <summary>
    /// Scrive il testo specficato se il Livello è inferiore a quello scelto.
    /// </summary>
    public static void WriteLine(string sMessage, int intLevel)
    {
        if (bIsSuspend)
            return;
        if (intLevel <= iLevel)
        {
            Trace.WriteLine(print(sMessage), intLevel < osLevel.Length ? osLevel[intLevel] : "Unknown");
            Trace.Flush();
        }
    }
    /// <summary>
    /// Scrive il testo specficato se la condizione è True e il Livello è inferiore a quello scelto.
    /// </summary>
    public static void WriteLine(bool condition, string sMessage, int intLevel)
    {
        if (bIsSuspend)
            return;
        if (intLevel <= iLevel)
        {
            Trace.WriteLineIf(condition, print(sMessage), intLevel < osLevel.Length ? osLevel[intLevel] : "Unknown");
            Trace.Flush();
        }
    }

    private static string print(string sMessage)
    {
        string sResult = "";
        try
        {
            if (bPrintDateTime)
                sResult += DateTime.Now.ToString(sFormatDateTime) + " ";
            sResult += sMessage;
            return sResult;
        }
        catch (Exception ex)
        {
            return "Error : " + ex.Message;
        }
    }
}
