using System.Text.RegularExpressions;

namespace Solution.Data;

/// <summary>
/// Utility interna per la validazione di identificatori SQL, operatori e direzioni di ordinamento
/// al fine di prevenire SQL Injection nelle query costruite dinamicamente.
/// </summary>
internal static class SqlSecurityHelper
{
    private static readonly Regex IdentifierSegmentRegex = new(
        @"^[A-Za-z_][A-Za-z0-9_]*$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly HashSet<string> AllowedOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        "=", "<>", "!=", "<", "<=", ">", ">=",
        "LIKE", "NOT LIKE", "IN", "NOT IN",
        "IS", "IS NOT", "BETWEEN"
    };

    private static readonly HashSet<string> AllowedDirections = new(StringComparer.OrdinalIgnoreCase)
    {
        "ASC", "DESC"
    };

    /// <summary>
    /// Valida un identificatore SQL (nome tabella o colonna, anche nel formato schema.tabella)
    /// e lo restituisce quotato in modo sicuro con parentesi quadre.
    /// </summary>
    public static string QuoteIdentifier(string identifier, string paramName = "identifier")
    {
        if (string.IsNullOrWhiteSpace(identifier))
            throw new ArgumentException("SQL identifier cannot be null or empty.", paramName);

        var segments = identifier.Split('.');
        var quoted = new string[segments.Length];

        for (int i = 0; i < segments.Length; i++)
        {
            string segment = segments[i].Trim();
            if (segment.StartsWith('[') && segment.EndsWith(']') && segment.Length >= 2)
                segment = segment[1..^1];

            if (!IdentifierSegmentRegex.IsMatch(segment))
                throw new ArgumentException($"Invalid SQL identifier: '{identifier}'.", paramName);

            quoted[i] = $"[{segment}]";
        }

        return string.Join(".", quoted);
    }

    /// <summary>
    /// Valida un nome di campo SELECT consentendo identificatori validi e le espressioni sicure "*", "1", "COUNT(*)".
    /// </summary>
    public static string ValidateSelectField(string field, string paramName = "field")
    {
        if (string.IsNullOrWhiteSpace(field))
            throw new ArgumentException("SQL field cannot be null or empty.", paramName);

        string trimmed = field.Trim();
        if (trimmed == "*" || trimmed == "1" || trimmed.Equals("COUNT(*)", StringComparison.OrdinalIgnoreCase))
            return trimmed;

        return QuoteIdentifier(trimmed, paramName);
    }

    /// <summary>
    /// Valida un operatore SQL contro una allowlist sicura.
    /// </summary>
    public static string ValidateOperator(string op, string paramName = "operator")
    {
        if (string.IsNullOrWhiteSpace(op))
            throw new ArgumentException("SQL operator cannot be null or empty.", paramName);

        string normalized = Regex.Replace(op.Trim(), @"\s+", " ").ToUpperInvariant();
        if (!AllowedOperators.Contains(normalized))
            throw new ArgumentException($"Unsupported or unsafe SQL operator: '{op}'.", paramName);

        return normalized;
    }

    /// <summary>
    /// Valida una direzione di ordinamento SQL (ASC o DESC).
    /// </summary>
    public static string ValidateSortDirection(string direction, string paramName = "direction")
    {
        if (string.IsNullOrWhiteSpace(direction))
            throw new ArgumentException("SQL sort direction cannot be null or empty.", paramName);

        string normalized = direction.Trim().ToUpperInvariant();
        if (!AllowedDirections.Contains(normalized))
            throw new ArgumentException($"Invalid SQL sort direction: '{direction}'. Expected ASC or DESC.", paramName);

        return normalized;
    }

    /// <summary>
    /// Crea un nome parametro SQL sicuro a partire da un prefisso e un indice.
    /// </summary>
    public static string MakeParamName(string prefix, int index) => $"@{prefix}_{index}";
}
