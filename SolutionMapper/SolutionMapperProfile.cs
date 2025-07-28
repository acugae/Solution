namespace Solution.SolutionMapper;

/// <summary>
/// Classe base astratta per la definizione di profili di mapping.
/// Un profilo permette di configurare le regole di mapping tra tipi sorgente e destinazione.
/// </summary>
public abstract class SolutionMapperProfile
{
    /// <summary>
    /// Mapper associato al profilo, usato per registrare le regole.
    /// </summary>
    protected SolutionMapper Mapper { get; }

    /// <summary>
    /// Costruttore che riceve il mapper da associare al profilo.
    /// </summary>
    /// <param name="mapper">Istanza di SolutionMapper</param>
    protected SolutionMapperProfile(SolutionMapper mapper)
    {
        Mapper = mapper;
    }

    /// <summary>
    /// Collezione interna delle regole di mapping definite nel profilo.
    /// </summary>
    internal List<object> Rules { get; } = new();

    /// <summary>
    /// Crea una nuova regola di mapping tra TSource e TDest e la aggiunge alle regole del profilo.
    /// </summary>
    /// <typeparam name="TSource">Tipo sorgente</typeparam>
    /// <typeparam name="TDest">Tipo destinazione</typeparam>
    /// <returns>Espressione di configurazione del mapping</returns>
    protected ISolutionMappingExpression<TSource, TDest> CreateMap<TSource, TDest>()
    {
        var rule = new SolutionMappingExpression<TSource, TDest>();
        Rules.Add(rule);
        return rule;
    }

    /// <summary>
    /// Aggiunge la regola di mapping inversa (ReverseMap) per una configurazione esistente.
    /// </summary>
    /// <typeparam name="TSource">Tipo sorgente</typeparam>
    /// <typeparam name="TDest">Tipo destinazione</typeparam>
    /// <param name="mapping">Configurazione di mapping da invertire</param>
    protected void AddReverseMap<TSource, TDest>(SolutionMappingExpression<TSource, TDest> mapping)
    {
        var reverse = mapping.ReverseMap();
        Rules.Add(reverse);
    }
}