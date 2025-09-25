namespace Solution.SolutionMapper;

/// <summary>
/// Configurazione per la trasformazione di valori durante il mapping.
/// Permette di specificare una funzione che trasforma un valore prima di assegnarlo alla destinazione.
/// </summary>
public class ValueTransformerConfiguration
{
    /// <summary>
    /// Funzione di trasformazione da applicare al valore.
    /// Riceve un oggetto in ingresso e restituisce il valore trasformato.
    /// </summary>
    public Func<object, object> Transformer { get; set; }
}