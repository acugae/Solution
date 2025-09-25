namespace Solution.SolutionMapper;

/// <summary>
/// Interfaccia per definire azioni personalizzate da eseguire durante il processo di mapping.
/// Può essere utilizzata per implementare logiche aggiuntive prima o dopo la mappatura tra TSource e TDest.
/// </summary>
/// <typeparam name="TSource">Tipo sorgente da mappare</typeparam>
/// <typeparam name="TDest">Tipo destinazione da popolare</typeparam>
public interface IMappingAction<TSource, TDest>
{
    /// <summary>
    /// Esegue la logica personalizzata di mapping tra source e destination.
    /// </summary>
    /// <param name="source">Oggetto sorgente da cui leggere i dati</param>
    /// <param name="destination">Oggetto destinazione su cui applicare la logica</param>
    /// <param name="context">Contesto di risoluzione, utile per passare dati aggiuntivi o gestire la profondità del mapping</param>
    void Process(TSource source, TDest destination, ResolutionContext context);
}