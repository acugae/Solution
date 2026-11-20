using System.Text.RegularExpressions;

namespace Solution.Security;

/// <summary>
/// Funzioni di verifica e validazione difensiva per identificatori SQL, frammenti SQL dinamici,
/// path su filesystem e caricamento dinamico di assembly/tipi.
/// </summary>
public static class InputGuard
{
    private static readonly Regex DangerousSqlFragmentRegex = new(
        @"(;|--|\*/|/\*|\b(DROP|TRUNCATE|ALTER|CREATE|EXEC|EXECUTE|MERGE|GRANT|REVOKE|DENY|SHUTDOWN|WAITFOR|OPENROWSET|OPENDATASOURCE|BULK\s+INSERT|XP_[A-Z0-9_]+|SP_[A-Z0-9_]+)\b)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    /// <summary>
    /// Valida e quota un identificatore SQL (tabella o colonna, anche nel formato schema.tabella).
    /// Lancia <see cref="ArgumentException"/> se l'identificatore non è valido.
    /// </summary>
    public static string ValidateSqlIdentifier(string identifier, string paramName = "identifier")
        => SqlSecurityHelper.QuoteIdentifier(identifier, paramName);

    /// <summary>
    /// Verifica se un identificatore SQL è sintatticamente sicuro senza lanciare eccezioni.
    /// </summary>
    public static bool IsSafeSqlIdentifier(string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
            return false;
        try
        {
            SqlSecurityHelper.QuoteIdentifier(identifier);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Valida un operatore SQL contro la allowlist sicura (=, &lt;&gt;, &lt;, &gt;, LIKE, IN, BETWEEN, IS, ...).
    /// </summary>
    public static string ValidateSqlOperator(string op, string paramName = "operator")
        => SqlSecurityHelper.ValidateOperator(op, paramName);

    /// <summary>
    /// Valida una direzione di ordinamento SQL (ASC o DESC).
    /// </summary>
    public static string ValidateSortDirection(string direction, string paramName = "direction")
        => SqlSecurityHelper.ValidateSortDirection(direction, paramName);

    /// <summary>
    /// Controllo difensivo per frammenti SQL dinamici (es. clausole JOIN/WHERE in DbOperations):
    /// blocca commenti SQL, multi-statement (;) e keyword DDL/esecuzione (DROP, EXEC, ALTER, xp_, ...).
    /// Nota: usare sempre query parametrizzate quando si passano valori utente.
    /// </summary>
    public static string ValidateSqlFragment(string? sqlFragment, string paramName = "sqlFragment")
    {
        if (string.IsNullOrWhiteSpace(sqlFragment))
            return string.Empty;

        if (DangerousSqlFragmentRegex.IsMatch(sqlFragment))
            throw new ArgumentException($"Unsafe SQL fragment detected in '{paramName}'.", paramName);

        return sqlFragment;
    }

    /// <summary>
    /// Verifica se un frammento SQL supera il controllo difensivo senza lanciare eccezioni.
    /// </summary>
    public static bool IsSafeSqlFragment(string? sqlFragment)
        => string.IsNullOrWhiteSpace(sqlFragment) || !DangerousSqlFragmentRegex.IsMatch(sqlFragment);

    /// <summary>
    /// Valida in modo stateless e thread-safe che <paramref name="path"/> ricada strettamente
    /// all'interno di <paramref name="allowedBaseDirectory"/> (prevenzione Path Traversal).
    /// </summary>
    public static string ValidateSafePath(string path, string allowedBaseDirectory, string paramName = "path")
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentNullException(paramName);
        if (string.IsNullOrWhiteSpace(allowedBaseDirectory))
            throw new ArgumentNullException(nameof(allowedBaseDirectory));

        string baseFull = Path.GetFullPath(allowedBaseDirectory);
        if (!Path.EndsInDirectorySeparator(baseFull))
            baseFull += Path.DirectorySeparatorChar;

        string targetFull = Path.GetFullPath(path);
        var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        string baseNoSep = baseFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        if (!targetFull.Equals(baseNoSep, comparison) && !targetFull.StartsWith(baseFull, comparison))
            throw new UnauthorizedAccessException($"Path '{path}' is outside the allowed directory.");

        return targetFull;
    }

    /// <summary>
    /// Verifica che un file assembly (.dll) risieda nella directory consentita prima del caricamento dinamico.
    /// </summary>
    public static string ValidateAssemblyPath(string assemblyPath, string allowedDirectory)
    {
        string safePath = ValidateSafePath(assemblyPath, allowedDirectory, nameof(assemblyPath));
        if (!string.Equals(Path.GetExtension(safePath), ".dll", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Only .dll assemblies are allowed.", nameof(assemblyPath));
        return safePath;
    }

    /// <summary>
    /// Verifica che un tipo caricato dinamicamente implementi o erediti almeno uno dei tipi base/interfacce consentiti.
    /// </summary>
    public static Type ValidateAllowedType(Type? type, params Type[] allowedBaseTypesOrInterfaces)
    {
        if (type == null)
            throw new ArgumentNullException(nameof(type));
        if (allowedBaseTypesOrInterfaces == null || allowedBaseTypesOrInterfaces.Length == 0)
            throw new ArgumentException("At least one allowed base type or interface must be specified.", nameof(allowedBaseTypesOrInterfaces));

        foreach (var allowed in allowedBaseTypesOrInterfaces)
        {
            if (allowed.IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                return type;
        }

        throw new UnauthorizedAccessException($"Type '{type.FullName}' is not permitted by the configured allowlist.");
    }
}
