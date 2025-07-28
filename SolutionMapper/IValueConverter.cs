    namespace Solution.SolutionMapper;

/// <summary>
/// Interfaccia per la conversione di un membro dal tipo sorgente al tipo destinazione durante il mapping.
/// Permette di personalizzare la trasformazione di valori tra proprietà di tipi diversi.
/// </summary>
/// <typeparam name="TSourceMember">Tipo del membro sorgente</typeparam>
/// <typeparam name="TDestMember">Tipo del membro destinazione</typeparam>
public interface IValueConverter<TSourceMember, TDestMember>
{
    /// <summary>
    /// Converte il valore del membro sorgente nel valore da assegnare al membro destinazione.
    /// </summary>
    /// <param name="sourceMember">Valore del membro sorgente</param>
    /// <param name="destination">Valore attuale del membro destinazione (può essere usato per la conversione)</param>
    /// <param name="context">Contesto di risoluzione, utile per passare dati aggiuntivi o gestire la profondità del mapping</param>
    /// <returns>Valore convertito da assegnare al membro destinazione</returns>
    TDestMember Convert(TSourceMember sourceMember, TDestMember destination, ResolutionContext context);
}