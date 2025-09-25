    namespace Solution.SolutionMapper;

/// <summary>
/// Interfaccia per la conversione di oggetti tra tipi diversi durante il mapping.
/// Permette di personalizzare la trasformazione da TSource a TDest.
/// </summary>
/// <typeparam name="TSource">Tipo sorgente da convertire</typeparam>
/// <typeparam name="TDest">Tipo destinazione risultante dalla conversione</typeparam>
public interface ITypeConverter<TSource, TDest>
{
    /// <summary>
    /// Converte un oggetto di tipo sorgente in un oggetto di tipo destinazione.
    /// </summary>
    /// <param name="source">Istanza del tipo sorgente da convertire</param>
    /// <param name="destination">Istanza del tipo destinazione (può essere usata per la conversione o popolata)</param>
    /// <param name="context">Contesto di risoluzione, utile per passare dati aggiuntivi o gestire la profondità del mapping</param>
    /// <returns>Oggetto convertito di tipo destinazione</returns>
    TDest Convert(TSource source, TDest destination, ResolutionContext context);
}