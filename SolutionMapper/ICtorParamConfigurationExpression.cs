namespace Solution.SolutionMapper;

/// <summary>
/// Interfaccia marker per la configurazione avanzata dei parametri del costruttore durante il mapping.
/// Può essere estesa per aggiungere opzioni o comportamenti specifici nella configurazione dei parametri del costruttore.
/// </summary>
/// <typeparam name="TSource">Tipo sorgente da cui derivano i dati per il parametro del costruttore</typeparam>
public interface ICtorParamConfigurationExpression<TSource> { }