namespace Solution.SolutionMapper;

/// <summary>
/// Contesto di risoluzione utilizzato durante il processo di mapping.
/// Permette di condividere dati, gestire la cache delle istanze e controllare la profondità massima di mapping.
/// </summary>
public class ResolutionContext
{
    /// <summary>
    /// Dizionario per memorizzare dati aggiuntivi e opzioni personalizzate durante il mapping.
    /// Può essere usato per passare parametri o flag tra le varie fasi del mapping.
    /// </summary>
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();

    /// <summary>
    /// Cache delle istanze già mappate, utile per gestire riferimenti ciclici o preservare oggetti già trasformati.
    /// </summary>
    public Dictionary<object, object> InstanceCache { get; } = new();

    /// <summary>
    /// Profondità massima consentita per il mapping ricorsivo.
    /// Utile per evitare stack overflow in presenza di oggetti annidati o ciclici.
    /// </summary>
    public int MaxDepth { get; set; } = 10;
}