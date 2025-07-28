using Solution.SolutionMapper;

namespace Solution.SolutionMapper.Extensions;

/// <summary>
/// Espressione di configurazione per la registrazione delle regole di mapping e dei converter nel SolutionMapper.
/// Fornisce metodi fluenti per aggiungere mapping e converter personalizzati.
/// </summary>
public class SolutionMapperConfigurationExpression
{
    private readonly SolutionMapper _mapper;

    /// <summary>
    /// Inizializza una nuova istanza della configurazione, associata a un SolutionMapper.
    /// </summary>
    /// <param name="mapper">Istanza di SolutionMapper su cui registrare le configurazioni</param>
    public SolutionMapperConfigurationExpression(SolutionMapper mapper)
    {
        _mapper = mapper;
    }

    /// <summary>
    /// Crea una nuova regola di mapping tra TSource e TDest e la registra nel SolutionMapper.
    /// </summary>
    /// <typeparam name="TSource">Tipo sorgente</typeparam>
    /// <typeparam name="TDest">Tipo destinazione</typeparam>
    /// <returns>Espressione di configurazione del mapping, da personalizzare</returns>
    public SolutionMappingExpression<TSource, TDest> CreateMap<TSource, TDest>()
    {
        var expr = new SolutionMappingExpression<TSource, TDest>();
        _mapper.AddMapping(expr);
        return expr;
    }

    /// <summary>
    /// Aggiunge un converter personalizzato tra TSource e TDest al SolutionMapper.
    /// </summary>
    /// <typeparam name="TSource">Tipo sorgente</typeparam>
    /// <typeparam name="TDest">Tipo destinazione</typeparam>
    /// <param name="converter">Istanza del converter da registrare</param>
    public void AddTypeConverter<TSource, TDest>(ITypeConverter<TSource, TDest> converter)
    {
        _mapper.AddTypeConverter(converter);
    }
}