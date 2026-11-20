namespace Solution.IO;
/// <summary>
/// Gestisce file di testo e binari, facilita la lettura e la scrittura.
/// </summary>
public class FileManager
{
    // Directory base per validazione path (null = nessuna restrizione)
    private static string? _allowedBasePath = null;
    
    /// <summary>
    /// Imposta una directory base. Tutte le operazioni file saranno ristrette a questa directory.
    /// </summary>
    /// <param name="basePath">Directory base consentita, o null per rimuovere la restrizione</param>
    public static void SetAllowedBasePath(string? basePath)
    {
        if (!string.IsNullOrEmpty(basePath))
        {
            string normalized = Path.GetFullPath(basePath);
            if (!Directory.Exists(normalized))
                throw new DirectoryNotFoundException($"Base path does not exist: {normalized}");
            if (!Path.EndsInDirectorySeparator(normalized))
                normalized += Path.DirectorySeparatorChar;
            _allowedBasePath = normalized;
        }
        else
        {
            _allowedBasePath = null;
        }
    }
    
    /// <summary>
    /// Valida e normalizza un path, prevenendo path traversal attacks.
    /// </summary>
    private static string ValidatePath(string path)
    {
        if (string.IsNullOrEmpty(path))
            throw new ArgumentNullException(nameof(path));
        
        // Normalizza il path
        string fullPath = Path.GetFullPath(path);
        
        // Se è impostata una base path, verifica che il path sia strettamente al suo interno
        string? allowed = _allowedBasePath;
        if (!string.IsNullOrEmpty(allowed))
        {
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            string allowedNoSep = allowed.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!fullPath.Equals(allowedNoSep, comparison) && !fullPath.StartsWith(allowed, comparison))
            {
                throw new UnauthorizedAccessException("Access denied: path is outside allowed directory");
            }
        }
        
        return fullPath;
    }
    
    /// <summary>
    /// Restituisce una stringa contenente il file specificato.
    /// </summary>
    static public string GetFile(string sFileName)
    {
        string validPath = ValidatePath(sFileName);
        using StreamReader reader = new StreamReader(validPath, System.Text.Encoding.Default);
        return reader.ReadToEnd();
    }
    /// <summary>
    /// Restituisce ogni riga contenuta in un file di testo.
    /// </summary>
    static public string[] GetFileLines(string sFileName)
    {
        string validPath = ValidatePath(sFileName);
        if (!File.Exists(validPath))
            return Array.Empty<string>();
            
        return File.ReadAllLines(validPath);
    }

    //
    /// <summary>
    /// Scrive nel file il valore specificato.
    /// </summary>
    static public void SetFile(string sFileName, string sValue)
    {
        string validPath = ValidatePath(sFileName);
        using StreamWriter sw = new StreamWriter(validPath);
        sw.Write(sValue);
    }
    //
    /// <summary>
    /// Restituisce un array di byte del file in ingresso.
    /// </summary>
    static public byte[] GetFileByte(string sFileName)
    {
        string validPath = ValidatePath(sFileName);
        return File.ReadAllBytes(validPath);
    }
    /// <summary>
    /// Scrive nel file l'array di byte specificato.(Nel caso in cui il file esiste verrà sovrascritto)
    /// </summary>
    static public void SetFileByte(string sFileName, byte[] oBuffer)
    {
        string validPath = ValidatePath(sFileName);
        File.WriteAllBytes(validPath, oBuffer);
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
        if (string.IsNullOrEmpty(sPath))
            return AppDomain.CurrentDomain.BaseDirectory;

        if (sPath.Contains('~'))
        {
            string relative = sPath.TrimStart('~', '/', '\\');
            return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relative));
        }

        return Path.IsPathRooted(sPath)
            ? Path.GetFullPath(sPath)
            : Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, sPath));
    }
}
