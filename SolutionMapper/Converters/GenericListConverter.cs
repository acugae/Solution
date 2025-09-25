using System;
using System.Collections.Generic;
using System.Linq;

namespace Solution.SolutionMapper.Converters;

/// <summary>
/// Converter generico per la mappatura di liste di elementi.
/// Questo converter mappa automaticamente List&lt;TSource&gt; in List&lt;TDest&gt; 
/// quando esiste una regola di mapping tra TSource e TDest.
/// </summary>
/// <typeparam name="TSource">Tipo degli elementi della lista sorgente</typeparam>
/// <typeparam name="TDest">Tipo degli elementi della lista destinazione</typeparam>
public class GenericListConverter<TSource, TDest> : ITypeConverter<List<TSource>, List<TDest>>
    where TDest : new()
{
    private readonly SolutionMapper _mapper;

    /// <summary>
    /// Inizializza una nuova istanza del converter con il mapper specificato.
    /// </summary>
    /// <param name="mapper">Istanza del SolutionMapper per eseguire il mapping degli elementi</param>
    public GenericListConverter(SolutionMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <summary>
    /// Converte una lista di elementi TSource in una lista di elementi TDest.
    /// </summary>
    /// <param name="source">Lista sorgente</param>
    /// <param name="destination">Lista destinazione (può essere null)</param>
    /// <param name="context">Contesto di risoluzione</param>
    /// <returns>Lista convertita</returns>
    public List<TDest> Convert(List<TSource> source, List<TDest> destination, ResolutionContext context)
    {
        if (source == null)
            return new List<TDest>();

        var result = new List<TDest>();

        foreach (var item in source)
        {
            if (item == null)
            {
                // Gestisce elementi null mantenendo la struttura
                result.Add(default(TDest));
            }
            else
            {
                // Usa il mapper per convertire ogni elemento
                var convertedItem = _mapper.Map<TSource, TDest>(item, context);
                result.Add(convertedItem);
            }
        }

        return result;
    }
}