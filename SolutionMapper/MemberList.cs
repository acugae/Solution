namespace Solution.SolutionMapper;

/// <summary>
/// Specifica quale lista di membri (proprietà) deve essere validata durante la configurazione del mapping.
/// </summary>
public enum MemberList
{
    /// <summary>
    /// Valida i membri (proprietà) del tipo sorgente.
    /// </summary>
    Source,

    /// <summary>
    /// Valida i membri (proprietà) del tipo destinazione.
    /// </summary>
    Destination,

    /// <summary>
    /// Nessuna validazione dei membri.
    /// </summary>
    None
}